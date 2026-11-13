using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The pump's nozzle, as the FiveM version fills up (Discord, ZennoxVI, 2026-11-01: "any1 got the VI fuel system?").
    /// GTA6Hud already burns fuel and sells it from its SERVICES menu at the pump, all at once, without leaving the car.
    /// This is the on-foot way, on the same tank:
    ///
    ///  * at a pump on foot, E takes its nozzle: it's in his left hand, on a hose back to the pump;
    ///  * at a car's fuel cap, E puts it in. The tank fills at [Nozzle] SecondsPerTank for a whole tank, and costs what
    ///    GTA6Hud's menu charges (its [FuelStation] FullTankCost for a whole tank), paid as it goes in;
    ///  * E at the cap takes it back out (it stops by itself when the tank is full, or the money runs out), and E at the
    ///    pump hangs it up;
    ///  * the hose is HoseLength long: walk or drive further than that and it comes off, and the nozzle drops.
    ///
    /// Its prompts are GTA6Hud's own world markers (as its SERVICES and FILL CAN), and the tank is GTA6Hud's (its Fuel,
    /// which the vehicle card's gauge reads). Without GTA6Hud, or with its fuel stations off ([FuelStation] Enabled),
    /// this stays off. [Switches] FuelNozzle in ViExtras.ini turns it off. GTA6Hud.dll itself is not changed.
    /// </summary>
    public sealed class Nozzle : Script
    {
        private const string NozzleModel = "prop_cs_fuel_nozle";
        private const int LeftHand = 60309;                  // SKEL_L_Hand
        private const int RopeType = 3;                      // ADD_ROPE's thick black one: reads as a hose (1 and 2 are braided rope)
        private const float PumpReach = 1.6f, CapReach = 1.4f, PromptLift = 0.8f;
        private const float ScanEvery = 0.35f;

        // the pumps GTA6Hud sells fuel at, and where on each the hose comes out, above its base
        private static readonly string[] PumpModels = { "prop_gas_pump_1a", "prop_gas_pump_1b", "prop_gas_pump_1c", "prop_gas_pump_1d", "prop_gas_pump_old2", "prop_gas_pump_old3", "prop_vintage_pump" };
        private static readonly float[] HoseHeight = { 1.3f, 1.3f, 1.3f, 1.3f, 1.2f, 1.2f, 1.2f };

        // in his left hand, as Toxic Scripts' Refueling Nozzle holds it in FiveM
        private static readonly Vector3 Grip = new Vector3(0.055f, 0.05f, 0f), GripTurn = new Vector3(-50f, -90f, -50f);
        // where the hose goes in: the metal collar at the bottom of the handle, in the nozzle's own frame
        private static readonly Vector3 Collar = new Vector3(0f, 0f, -0.18f);

        private enum Stage { Off, InHand, InCar }

        private readonly bool _enabled;
        private readonly float _secondsPerTank, _hoseLength;
        private Stage _stage;
        private Prop _pump, _nozzle, _dropped;
        private Vector3 _hoseTop;
        private int _rope;
        private Vehicle _car;
        private Vector3 _capLocal;
        private bool _filling, _wasDown, _warned, _hoseDue;
        private float _owed, _scan;
        private int _paid;
        private long _droppedUntil;
        private Prop _nearPump;
        private int _nearModel = -1;
        private Model[] _models;

        // GTA6Hud: its fuel, its settings, its prompt pieces
        private bool _looked;
        private object _fuel;
        private Func<Vehicle, float> _fraction;
        private Action<Vehicle, float> _set;
        private Action<Vehicle> _fill;
        private Func<Vehicle, bool> _applies;
        private PropertyInfo _hasCan, _canLevel;
        private FieldInfo _stationsOn, _fullTankCost;
        private Func<Vector3, bool> _focused;
        private Func<bool> _contextDown;
        private Action<float, float, string, string, string, bool, bool, bool> _drawMarker;
        private Action<float, float, float, float> _drawRim;
        private Action<Vector3> _passiveDot;
        private Action<string> _notify;

        public Nozzle()
        {
            Paths.Init(this);
            _enabled = Config.Get(Config.Switches, "FuelNozzle", true);
            _secondsPerTank = Math.Max(2f, Config.Get("Nozzle", "SecondsPerTank", 20f));
            _hoseLength = Num.Clamp(Config.Get("Nozzle", "HoseLength", 6f), 2f, 12f);
            Tick += OnTick;
            Aborted += (s, e) => PutAway(false);
        }

        private bool Look()
        {
            if (_fuel != null) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            const BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = hud.GetType();
            object fuel = t.GetField("_fuel", Own)?.GetValue(hud);
            Type f = fuel?.GetType();
            Type refs = t.Assembly.GetType("GTA6Hud.Ref", false);
            if (f != null)
            {
                _fraction = HudBridge.Bind<Func<Vehicle, float>>(fuel, f.GetMethod("Fraction", Own, null, new[] { typeof(Vehicle) }, null));
                _set = HudBridge.Bind<Action<Vehicle, float>>(fuel, f.GetMethod("Set", Own, null, new[] { typeof(Vehicle), typeof(float) }, null));
                _fill = HudBridge.Bind<Action<Vehicle>>(fuel, f.GetMethod("Fill", Own, null, new[] { typeof(Vehicle) }, null));
                _applies = HudBridge.Bind<Func<Vehicle, bool>>(null, f.GetMethod("Applies", Stat, null, new[] { typeof(Vehicle) }, null));
                _hasCan = f.GetProperty("HasCan", Own);
                _canLevel = f.GetProperty("CanLevel", Own);
            }
            _stationsOn = refs?.GetField("FuelStationEnabled", Stat);
            _fullTankCost = refs?.GetField("FuelFullTankCost", Stat);
            _focused = HudBridge.Bind<Func<Vector3, bool>>(hud, t.GetMethod("Focused", Stat, null, new[] { typeof(Vector3) }, null));
            _contextDown = HudBridge.Bind<Func<bool>>(hud, t.GetMethod("ContextDown", Stat, null, Type.EmptyTypes, null));
            _drawMarker = HudBridge.Bind<Action<float, float, string, string, string, bool, bool, bool>>(hud, t.GetMethod("DrawWorldMarker", Own, null,
                new[] { typeof(float), typeof(float), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool) }, null));
            _drawRim = HudBridge.Bind<Action<float, float, float, float>>(hud, t.GetMethod("DrawProgressRim", Own, null, new[] { typeof(float), typeof(float), typeof(float), typeof(float) }, null));
            _passiveDot = HudBridge.Bind<Action<Vector3>>(hud, t.GetMethod("PassiveDot", Own, null, new[] { typeof(Vector3) }, null));
            _notify = HudBridge.Bind<Action<string>>(null, t.GetMethod("Notify", Stat, null, new[] { typeof(string) }, null));
            if (_fraction == null || _set == null || _fill == null || _applies == null || _drawMarker == null || _focused == null)
            {
                Log.Write("Nozzle", "GTA6Hud's fuel or prompts weren't all found; the nozzle stays off");
                return false;
            }
            _fuel = fuel;
            Log.Write("Nozzle", "the pump nozzle is on: " + _secondsPerTank.ToString("0.#") + " s a tank, a " + _hoseLength.ToString("0.#") + " m hose");
            return true;
        }

        private bool StationsOn
        {
            get
            {
                try { return _stationsOn == null || (bool)_stationsOn.GetValue(null); }
                catch { return true; }
            }
        }

        private int FullTankCost
        {
            get
            {
                try { if (_fullTankCost != null) return Math.Max(0, (int)_fullTankCost.GetValue(null)); }
                catch { }
                return 90;
            }
        }

        // GTA6Hud's own E at a pump on foot: FILL CAN, while he carries an empty gas can
        private bool CanWantsPump
        {
            get
            {
                try { return _hasCan != null && (bool)_hasCan.GetValue(_fuel) && (float)_canLevel.GetValue(_fuel) <= 0f; }
                catch { return false; }
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled) return;
            if (_dropped != null && RealTime.Ms >= _droppedUntil)
            {
                if (_dropped.Exists()) _dropped.Delete();
                _dropped = null;
            }
            if (!Look() || HudBridge.Paused) return;
            Ped player = Game.Player.Character;
            bool ok = player != null && player.Exists() && !player.IsDead && !player.IsInVehicle() && StationsOn && !HudBridge.Busy;
            if (_stage != Stage.Off && (!ok || _pump == null || !_pump.Exists()))
            {
                PutAway(false);
                return;
            }
            if (!ok) return;
            if (_hoseDue && _nozzle != null && _nozzle.Exists()) _hoseDue = !Hose(_nozzle);
            // GTA6Hud's ContextDown takes the game's own E (and Enter) away for the frame it's asked in, so it's only asked
            // while a prompt of the nozzle's can be up: with the nozzle out, or at a pump. Asked every frame on foot, as in
            // 2.7.2, it left every E prompt of the game's dead: wardrobes, barbers, the gym, missions.
            bool ours = _stage != Stage.Off || AtPumpPrompt(player);
            if (ours) RopeTexturesIn();
            bool down = Down(ours);
            bool pressed = down && !_wasDown;
            _wasDown = down;

            switch (_stage)
            {
                case Stage.Off: AtPump(player, pressed); break;
                case Stage.InHand: InHand(player, pressed); break;
                case Stage.InCar: InCar(player, pressed); break;
            }
        }

        // ours: GTA6Hud's ContextDown, which takes the key from the game this frame; otherwise only looked at
        private bool Down(bool ours)
        {
            if (ours && _contextDown != null) return _contextDown();
            return Game.IsKeyPressed(Keys.E) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, 51);
        }

        // AtPump's prompt is up (or about to be): the pump found last frame is within reach, and GTA6Hud's FILL CAN
        // doesn't have it
        private bool AtPumpPrompt(Ped player)
        {
            return _nearPump != null && _nearPump.Exists() && !CanWantsPump
                && player.Position.DistanceTo2D(_nearPump.Position) <= PumpReach + 0.6f;
        }

        // ---- a pump --------------------------------------------------------------------------------

        private void AtPump(Ped player, bool pressed)
        {
            Prop pump = NearestPump(player.Position, out int model);
            if (pump == null || CanWantsPump || HudBridge.UiInTheWay) return;
            if (player.Position.DistanceTo2D(pump.Position) > PumpReach + 0.6f) return;
            if (!Prompt(pump.Position + new Vector3(0f, 0f, PromptLift), "TAKE NOZZLE", true, -1f)) return;
            if (pressed) Take(player, pump, model);
        }

        private Prop NearestPump(Vector3 at, out int model)
        {
            if (_models == null)
            {
                _models = new Model[PumpModels.Length];
                for (int i = 0; i < PumpModels.Length; i++) _models[i] = new Model(PumpModels[i]);
            }
            _scan -= Game.LastFrameTime;
            if (_scan <= 0f)
            {
                _scan = ScanEvery;
                _nearPump = null;
                _nearModel = -1;
                float best = float.MaxValue;
                for (int i = 0; i < _models.Length; i++)
                {
                    int h = Function.Call<int>(Hash.GET_CLOSEST_OBJECT_OF_TYPE, at.X, at.Y, at.Z, 3f, _models[i].Hash, false, false, false);
                    if (h == 0) continue;
                    var p = Entity.FromHandle(h) as Prop;
                    if (p == null || !p.Exists()) continue;
                    float d = p.Position.DistanceToSquared(at);
                    if (d < best) { best = d; _nearPump = p; _nearModel = i; }
                }
            }
            model = _nearModel;
            return _nearPump != null && _nearPump.Exists() ? _nearPump : null;
        }

        private void Take(Ped player, Prop pump, int model)
        {
            var m = new Model(NozzleModel);
            if (!m.IsInCdImage)
            {
                if (!_warned) { _warned = true; Log.Write("Nozzle", NozzleModel + " isn't in the game"); }
                return;
            }
            if (!m.Request(1000)) return;
            Prop nozzle = World.CreateProp(m, player.Position + new Vector3(0f, 0f, 0.5f), false, false);
            m.MarkAsNoLongerNeeded();
            if (nozzle == null) return;
            nozzle.IsCollisionEnabled = false;
            _nozzle = nozzle;
            _pump = pump;
            _hoseTop = pump.Position + new Vector3(0f, 0f, HoseHeight[Math.Max(0, model)]);
            player.Weapons.Select(WeaponHash.Unarmed, true);
            ToHand(player);
            _hoseDue = true;   // next frame, once the game has moved the nozzle to where it was attached
            _stage = Stage.InHand;
            Log.Write("Nozzle", "took the nozzle at " + PumpModels[Math.Max(0, model)]);
        }

        private void ToHand(Ped player)
        {
            Function.Call(Hash.DETACH_ENTITY, _nozzle, true, false);
            int bone = Function.Call<int>(Hash.GET_PED_BONE_INDEX, player, LeftHand);
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, _nozzle, player, bone, Grip.X, Grip.Y, Grip.Z, GripTurn.X, GripTurn.Y, GripTurn.Z,
                false, false, false, false, 2, true);
        }

        // The rope's textures, asked for at a pump, so they're in before the nozzle is taken.
        private static bool RopeTexturesIn()
        {
            if (Function.Call<bool>(Hash.ROPE_ARE_TEXTURES_LOADED)) return true;
            Function.Call(Hash.ROPE_LOAD_TEXTURES);
            return false;
        }

        // The hose from the pump to the nozzle (made again each time the nozzle moves to another thing): false until it
        // can be, so it's tried again next frame. Only once the rope's textures are in: made the frame they were asked
        // for, the first hose of a session crashed GTA V Enhanced in ATTACH_ENTITIES_TO_ROPE (2026-11-02).
        private bool Hose(Prop to)
        {
            DropHose();
            if (!RopeTexturesIn()) return false;
            Vector3 a = _hoseTop;
            float length = Math.Max(1f, Math.Min(_hoseLength, a.DistanceTo(to.Position) + 0.6f));
            _rope = Function.Call<int>(Hash.ADD_ROPE, a.X, a.Y, a.Z, 0f, 0f, 0f, length, RopeType, _hoseLength + 1f, 0.5f, 0.5f,
                false, false, true, 1f, false, 0);
            if (_rope == 0 || !new Rope(_rope).Exists())
            {
                _rope = 0;
                Log.Write("Nozzle", "the hose couldn't be made: no rope");
                return true;
            }
            Vector3 b = to.GetOffsetPosition(Collar);
            new Rope(_rope).Connect(_pump, a, to, b, _hoseLength + 1f);
            new Rope(_rope).ActivatePhysics();
            return true;
        }

        private void DropHose()
        {
            if (_rope == 0) return;
            var r = new Rope(_rope);
            if (r.Exists()) r.Delete();
            _rope = 0;
        }

        // ---- in his hand ---------------------------------------------------------------------------

        private void InHand(Ped player, bool pressed)
        {
            KeepHandsFree();
            if (TooFar(_nozzle.Position)) { Snap(player, "The hose came off the pump"); return; }
            if (HudBridge.UiInTheWay) return;

            // the pump to hang it up, or a car's cap to fill: whichever is nearer
            float toPump = player.Position.DistanceTo2D(_pump.Position);
            Vehicle car = NearestCar(player, out Vector3 cap);
            float toCap = car != null ? player.Position.DistanceTo2D(cap) : float.MaxValue;

            if (car != null && toCap <= CapReach && toCap < toPump)
            {
                float level = _fraction(car);
                bool room = level < 0.995f;
                if (!Prompt(cap + new Vector3(0f, 0f, 0.35f), room ? "REFUEL" : "TANK FULL", room, level)) return;
                if (pressed && room) IntoCar(car, cap);
                return;
            }
            if (toPump <= PumpReach + 0.6f && Prompt(_pump.Position + new Vector3(0f, 0f, PromptLift), "HANG UP NOZZLE", true, -1f) && pressed)
                PutAway(false);
        }

        // no gun, no punches, no jumping or getting into a car with it in his hand
        private static void KeepHandsFree()
        {
            foreach (int c in new[] { 22, 23, 24, 25, 37, 44, 45, 140, 141, 142, 143, 257, 263, 264 })
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, c, true);
        }

        private bool TooFar(Vector3 at) => at.DistanceTo(_hoseTop) > _hoseLength + 0.5f;

        // The nearest car he could fill, and its fuel cap on the side he's standing.
        private Vehicle NearestCar(Ped player, out Vector3 cap)
        {
            cap = Vector3.Zero;
            Vehicle best = null;
            float bestD = float.MaxValue;
            foreach (Vehicle v in World.GetNearbyVehicles(player.Position, 6f))
            {
                if (v == null || !v.Exists() || !_applies(v) || v.Speed > 0.5f) continue;
                if (v.Driver != null && v.Driver.Exists()) continue;
                Vector3 c = CapOf(v, player.Position);
                float d = player.Position.DistanceTo2D(c);
                if (d < bestD) { bestD = d; best = v; cap = c; }
            }
            return best;
        }

        // A fuel cap: the car's petrolcap bone when it has one, otherwise above the rear wheel on his side.
        private static Vector3 CapOf(Vehicle car, Vector3 from)
        {
            Vector3 right = car.RightVector;
            foreach (string name in new[] { "petrolcap", "petroltank_l", "petroltank_r" })
            {
                EntityBone bone = car.Bones[name];
                if (bone == null || !bone.IsValid) continue;
                Vector3 p = bone.Position;
                float side = Vector3.Dot(p - car.Position, right);
                if (Math.Abs(side) < 0.2f) continue;          // in the middle: not where a nozzle goes
                return p + (side < 0f ? -right : right) * 0.08f;
            }
            bool onRight = Vector3.Dot(from - car.Position, right) > 0f;
            EntityBone wheel = car.Bones[onRight ? "wheel_rr" : "wheel_lr"];
            Vector3 w = wheel != null && wheel.IsValid ? wheel.Position : car.GetOffsetPosition(new Vector3(onRight ? 0.9f : -0.9f, -1.3f, 0f));
            if (car.Model.IsBike)
            {
                EntityBone tank = car.Bones["petroltank"];
                Vector3 t = tank != null && tank.IsValid ? tank.Position : car.Position + Vector3.WorldUp * 0.5f;
                return t + (onRight ? right : -right) * 0.2f + Vector3.WorldUp * 0.1f;
            }
            return w + (onRight ? right : -right) * 0.2f - car.ForwardVector * 0.25f + Vector3.WorldUp * 0.55f;
        }

        // ---- in the car ----------------------------------------------------------------------------

        private void IntoCar(Vehicle car, Vector3 cap)
        {
            int need = (int)Math.Ceiling((1f - _fraction(car)) * FullTankCost);
            if (need > 0 && Cash.Spendable < Math.Min(need, 5))
            {
                Say("Not enough cash to refuel - $" + need.ToString("N0", CultureInfo.InvariantCulture) + " needed");
                return;
            }
            _car = car;
            Vector3 right = car.RightVector;
            bool onRight = Vector3.Dot(cap - car.Position, right) > 0f;
            _capLocal = car.GetPositionOffset(cap);
            Function.Call(Hash.DETACH_ENTITY, _nozzle, true, false);
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, _nozzle, car, 0, _capLocal.X, _capLocal.Y, _capLocal.Z, -45f, 0f, onRight ? 90f : -90f,
                false, false, false, false, 2, true);
            _hoseDue = true;   // next frame, once the game has moved the nozzle to where it was attached
            _stage = Stage.InCar;
            _filling = true;
            _owed = 0f;
            _paid = 0;
            Log.Write("Nozzle", "filling a " + car.DisplayName + " from " + (_fraction(car) * 100f).ToString("0") + "%");
        }

        private void InCar(Ped player, bool pressed)
        {
            if (_car == null || !_car.Exists() || _car.IsDead) { Snap(player, null); return; }
            Vector3 cap = _car.GetOffsetPosition(_capLocal);
            if (TooFar(cap) || _car.Speed > 1.5f) { Snap(player, "The nozzle came out of the car"); return; }

            float level = _fraction(_car);
            if (_filling) level = Flow(level);

            bool near = player.Position.DistanceTo2D(cap) <= CapReach + 0.4f;
            if (_filling && near) HudBridge.ShowMoney();
            if (!near || HudBridge.UiInTheWay) return;
            if (Prompt(cap + new Vector3(0f, 0f, 0.35f), "TAKE NOZZLE", true, level) && pressed)
            {
                if (_filling) Stop("Refuelled");
                ToHand(player);
                _hoseDue = true;
                _car = null;
                _stage = Stage.InHand;
            }
        }

        // The tank this frame, filled for the time it was in and paid as it goes, $5 at a time.
        private float Flow(float level)
        {
            int cost = FullTankCost;
            float add = Math.Min(1f - level, Game.LastFrameTime / _secondsPerTank);
            if (add > 0f)
            {
                level += add;
                _set(_car, level);
                _owed += add * cost;
            }
            if (_owed >= 5f || (level >= 0.999f && _owed >= 1f))
            {
                int due = (int)_owed;
                if (Cash.Spend(due)) { _owed -= due; _paid += due; }
                else { Stop("Out of cash"); return level; }
            }
            if (level >= 0.999f)
            {
                _fill(_car);
                Stop("Tank full");
            }
            return level;
        }

        private void Stop(string why)
        {
            if (!_filling) return;
            _filling = false;
            if (_owed >= 1f)
            {
                int due = (int)_owed;
                if (Cash.Spend(due)) _paid += due;
            }
            _owed = 0f;
            if (_paid > 0) Say("Refuelled for $" + _paid.ToString("N0", CultureInfo.InvariantCulture));
            Log.Write("Nozzle", why.ToLowerInvariant() + ": $" + _paid + ", the tank at " + (_car != null && _car.Exists() ? (_fraction(_car) * 100f).ToString("0") + "%" : "?"));
        }

        // ---- putting it down -----------------------------------------------------------------------

        // The hose came off: the nozzle falls where it is, and goes a while later.
        private void Snap(Ped player, string why)
        {
            if (_filling) Stop("Refuelled");
            DropHose();
            if (_nozzle != null && _nozzle.Exists())
            {
                Function.Call(Hash.DETACH_ENTITY, _nozzle, true, true);
                _nozzle.IsCollisionEnabled = true;
                Function.Call(Hash.ACTIVATE_PHYSICS, _nozzle);
                if (_dropped != null && _dropped.Exists()) _dropped.Delete();
                _dropped = _nozzle;
                _droppedUntil = RealTime.Ms + 15000;
            }
            _nozzle = null;
            if (why != null) Say(why);
            Log.Write("Nozzle", why ?? "the car went");
            Clear();
        }

        // Back on the pump (or gone, when he's in a car, dead, or it was all stopped).
        private void PutAway(bool quietly)
        {
            if (_filling) Stop("Refuelled");
            DropHose();
            if (_nozzle != null && _nozzle.Exists()) _nozzle.Delete();
            _nozzle = null;
            if (_stage != Stage.Off && !quietly) Log.Write("Nozzle", "hung the nozzle up");
            Clear();
        }

        private void Clear()
        {
            _stage = Stage.Off;
            _car = null;
            _pump = null;
            _filling = false;
            _hoseDue = false;
        }

        // ---- GTA6Hud's prompt ----------------------------------------------------------------------

        // Its world marker at a spot, as its SERVICES: the word and E, a dot while it's off to the side. rim: the tank's
        // level around it (below 0: none). True while it's the one in focus.
        private bool Prompt(Vector3 at, string label, bool enabled, float rim)
        {
            if (!GTA.UI.Screen.IsSphereVisible(at, 0.4f)) return false;
            if (!_focused(at))
            {
                _passiveDot?.Invoke(at);
                return false;
            }
            var p = GTA.UI.Screen.WorldToScreen(at, true);
            if (p.X <= 0f && p.Y <= 0f) return false;
            float x = p.X * 1.5f, y = p.Y * 1.5f;   // GTA6Hud's FromSprite
            _drawMarker(x, y, label, "square", "E", false, false, enabled);
            if (rim >= 0f) _drawRim?.Invoke(x, y, 48.07f, Num.Clamp01(rim));
            return true;
        }

        private void Say(string text)
        {
            if (_notify != null) _notify(text);
            else GTA.UI.Notification.Show(text);
        }
    }
}
