using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The FiveM version's nitrous (sk_streetkings modules/nitrous/nitrous_c.lua,
    /// which the server runs, drawn by vice_hud), for single player.
    ///
    /// Hold Left Shift (X / Square on a pad) in a car with a bottle: GTA's own
    /// vehicle boost plus an engine torque multiplier, backfire flames from every
    /// exhaust, the RaceTurbo screen effect and a light camera shake. The bottle
    /// holds a few seconds of boost (its tier) and refills while it is not in
    /// use, from drifting, near-missing traffic and driving into oncoming traffic.
    /// Each car starts with a full bottle.
    ///
    /// Which cars have a bottle: FiveM sells nitrous per car in its customs shop,
    /// and so does Leonida Customs here (ViCustoms.dll): the tier bought for a
    /// car is kept by its plate in scripts\ViExtras\Save\nitrous.txt. [Nitrous]
    /// FittedBy = Turbo gives the [Nitrous] Tier to every car with LS Customs'
    /// Turbo instead, All to every car.
    ///
    /// What vice_hud draws for it: the nitrous pip, last in the vehicle card's
    /// gauge row (here GTA6Hud's row, after its engine, fuel and route gauges,
    /// drawn the way that row draws a gauge), and the "Hold [key] to activate
    /// nitrous" tip, top left, when a bottle has just been fitted.
    ///
    /// Left out: the race checkpoint refill (StreetKings' races are not in single
    /// player) and the per-player tier the server could hand out.
    /// </summary>
    public sealed class Nitrous : Script
    {
        // nitrous_c.lua / shared/config.lua
        private struct Tier { public float Capacity, Torque; }
        private static readonly Dictionary<string, Tier> Tiers = new Dictionary<string, Tier>(StringComparer.OrdinalIgnoreCase)
        {
            ["Street"] = new Tier { Capacity = 3.0f, Torque = 1.25f },
            ["Sport"] = new Tier { Capacity = 4.0f, Torque = 1.35f },
            ["Race"] = new Tier { Capacity = 5.0f, Torque = 1.45f },
        };
        private const int NearMissCooldownMs = 2200;
        private const int ScanMs = 120;
        private const int BackfireMs = 85;
        private const float BackfireScale = 1.2503f;
        private const int FitGraceMs = 1500;          // client.lua: a kit there this soon after getting in was there before

        private readonly bool _enabled, _screenFx, _flames;
        private readonly string _fittedBy, _defaultTier;
        private Tier _tier;
        private static readonly PlateStore Bottles = new PlateStore("nitrous.txt");
        private readonly int _kbControl, _padControl, _hintMs;
        private readonly Keys _kbKey;   // the key behind KeyboardControl, read itself (see Pressed)
        private readonly float _nearMissRefill, _driftRefill, _oncomingRefill;

        private Vehicle _veh;
        private int _vehHandle;
        private long _enteredAt;
        private bool _active, _boosting, _seen, _cardTaken;
        private float _fuel, _shown;
        private long _lastScanAt, _oncomingUntil, _nextBackfireAt;
        private readonly Dictionary<int, long> _nearMissCooldowns = new Dictionary<int, long>();
        private List<Vector3> _exhausts;
        private bool _ptfxRequested;

        // the tip
        private readonly HashSet<int> _hinted = new HashSet<int>();
        private bool _hintPending;
        private long _hintUntil = -1;

        /// <summary>Left Alt is the boost right now ([Nitrous] KeyboardControl = 19, a bottle in the car, keyboard): other scripts leave it alone.</summary>
        public static bool HoldsCharacterKey { get; private set; }

        public Nitrous()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Nitrous", "Enabled", true);
            _fittedBy = Config.Get("Nitrous", "FittedBy", "Shop");
            _defaultTier = Config.Get("Nitrous", "Tier", "Street");
            _kbControl = Config.Get("Nitrous", "KeyboardControl", 21);   // INPUT_SPRINT: Left Shift (FiveM: 19, Left Alt)
            _padControl = Config.Get("Nitrous", "PadControl", 179);      // INPUT_CELLPHONE_EXTRA_OPTION: X / Square
            _kbKey = _kbControl == 21 ? Keys.ShiftKey : _kbControl == 19 ? Keys.Menu : Keys.None;
            _screenFx = Config.Get("Nitrous", "ScreenEffect", true);
            _flames = Config.Get("Nitrous", "Flames", true);
            _hintMs = Config.Get("Nitrous", "HintMs", 7000);
            _nearMissRefill = Config.Get("Nitrous", "NearMissRefill", 0.45f);
            _driftRefill = Config.Get("Nitrous", "DriftRefillPerSec", 0.35f);
            _oncomingRefill = Config.Get("Nitrous", "OncomingRefillPerSec", 0.22f);

            Tick += OnTick;
            Aborted += (s, e) => { SetBoosting(false); Gta6HudCard.End(); HoldsCharacterKey = false; HudBridge.HoldSwapWheelOff("nitrous", false); };
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now)
            {
                SetBoosting(false);
                HoldsCharacterKey = false;
                HudBridge.HoldSwapWheelOff("nitrous", false);
                return;
            }
            if (!_enabled) return;
            try { Run(); }
            catch (Exception ex)
            {
                Log.Write("Nitrous", ex.ToString());
                SetBoosting(false);
                Gta6HudCard.End();
                _cardTaken = false;
                HoldsCharacterKey = false;
                HudBridge.HoldSwapWheelOff("nitrous", false);
            }
        }

        // =============================================================================
        // The bottle (nitrous_c.lua shouldBeActive / enableNitrous / startLoop)
        // =============================================================================

        private void Run()
        {
            long now = RealTime.Ms;
            Ped ped = Game.Player.Character;
            Vehicle veh = ped.IsInVehicle() ? ped.CurrentVehicle : null;
            int handle = veh != null && veh.Exists() ? veh.Handle : 0;
            if (handle != _vehHandle)
            {
                SetBoosting(false);
                if (_active)
                {
                    Gta6HudCard.End();
                    _cardTaken = false;
                    _active = false;
                }
                _hintPending = false;
                _hintUntil = -1;
                _veh = handle != 0 ? veh : null;
                _vehHandle = handle;
                _enteredAt = now;
                _exhausts = null;
                _nearMissCooldowns.Clear();
                _oncomingUntil = 0;
            }

            Tier tier = default(Tier);
            bool fitted = _veh != null && _veh.GetPedOnSeat(VehicleSeat.Driver) == ped && AllowedClass(_veh)
                && BottleOf(_veh, out tier);
            if (fitted && !_active)
            {
                _active = true;
                _tier = tier;
                _fuel = _shown = _tier.Capacity;
                // GTA6Hud's card, drawn from here so the row stays centred with the pip in it.
                _cardTaken = Gta6HudCard.Begin(_veh);
                Log.Write("Nitrous", "bottle fitted: " + _tier.Capacity.ToString("0.#") + " s, gauge " + (_cardTaken ? "on GTA6Hud's card" : "after its gauge row"));
                // client.lua: the reading going from "no kit" to "kit" while the
                // player already sits in the car is a kit being fitted. Never the
                // first reading, never the moment they get in, once a car a session.
                if (_seen && _hintMs > 0 && now - _enteredAt > FitGraceMs && _hinted.Add(_vehHandle)) _hintPending = true;
            }
            else if (!fitted && _active)
            {
                SetBoosting(false);
                Gta6HudCard.End();
                _cardTaken = false;
                _active = false;
                _hintPending = false;
                _hintUntil = -1;
            }
            _seen = true;

            // With FiveM's Left Alt (KeyboardControl = 19), GTA6Hud's swap wheel on the
            // same key stays shut meanwhile. A pad boosts on X / Square instead.
            HoldsCharacterKey = _active && !Glyphs.UsingPad && _kbControl == 19;
            HudBridge.HoldSwapWheelOff("nitrous", HoldsCharacterKey);
            if (_active) Drive(ped, now);
            DrawHint(now);
        }

        private void Drive(Ped ped, long now)
        {
            float dt = Math.Max(Game.LastFrameTime, 0.001f);
            bool blocked = HudBridge.UiInTheWay || CrashCam.IsPlaying || Binoculars.IsActive;
            if (blocked || !_veh.IsEngineRunning)
            {
                SetBoosting(false);
            }
            else
            {
                bool pressed = Pressed();
                if (pressed && _fuel > 0f && !Function.Call<bool>(Hash.IS_VEHICLE_STOPPED, _veh))
                {
                    SetBoosting(true);
                    _fuel = Math.Max(0f, _fuel - dt);
                    Function.Call(Hash.SET_VEHICLE_CHEAT_POWER_INCREASE, _veh, _tier.Torque);
                    if (_flames && now >= _nextBackfireAt)
                    {
                        _nextBackfireAt = now + BackfireMs;
                        Backfire();
                    }
                }
                else
                {
                    SetBoosting(false);
                    Recharge(now, dt);
                }
                if (_fuel <= 0f) SetBoosting(false);
            }

            // The ring follows the tank over 140 ms (style.css #pip-nitrous .pip-fill).
            _shown += (_fuel - _shown) * Math.Min(1f, dt / 0.14f);
            if (_cardTaken)
            {
                // The card goes up and down with GTA6Hud's own, whatever is in the way.
                if (Gta6HudCard.Draw(_veh, 1, out float sx, out float sy)) DrawPip(sx, sy, true);
            }
            else if (!blocked && HudBridge.VehicleGaugeRow(out float cx, out float cy, out _))
            {
                // GTA6Hud's card could not be drawn from here: after its row, off centre.
                bool route = World.WaypointBlip != null && World.WaypointBlip.Exists();
                DrawPip(cx + Pitch * 0.5f * ((route ? 3 : 2) + 1), cy, false);
            }
        }

        /// <summary>The bottle this car carries, if any (see FittedBy).</summary>
        private bool BottleOf(Vehicle v, out Tier tier)
        {
            string name;
            if (string.Equals(_fittedBy, "All", StringComparison.OrdinalIgnoreCase)) name = _defaultTier;
            else if (string.Equals(_fittedBy, "Turbo", StringComparison.OrdinalIgnoreCase))
                name = Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, v, 18) ? _defaultTier : null;
            else name = Bottles.Get(Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, v));
            tier = default(Tier);
            return name != null && Tiers.TryGetValue(name, out tier);
        }

        private static bool AllowedClass(Vehicle v)
        {
            // Not cycles, boats, helicopters, planes or trains.
            int c = (int)v.ClassType;
            return c != 13 && c != 14 && c != 15 && c != 16 && c != 21;
        }

        private bool Pressed()
        {
            if (!Glyphs.UsingPad)
            {
                // The key itself, not its control: 21 and 19 are pad buttons too (A, d-pad
                // down), and a pad read as a keyboard for a moment (a nudged mouse) would
                // boost on them. On a pad only PadControl boosts.
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, _kbControl, true);
                if (_kbKey == Keys.ShiftKey) return Game.IsKeyPressed(Keys.ShiftKey) || Game.IsKeyPressed(Keys.LShiftKey);
                if (_kbKey == Keys.Menu) return Game.IsKeyPressed(Keys.Menu) || Game.IsKeyPressed(Keys.LMenu);
                return Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, _kbControl);
            }
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, _padControl, true);
            return Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, _padControl)
                || Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, _padControl);
        }

        private void SetBoosting(bool on)
        {
            if (_boosting == on) return;
            _boosting = on;
            if (!on && _active) Log.Write("Nitrous", "boost over: tank " + _fuel.ToString("0.0") + " of " + _tier.Capacity.ToString("0.#") + " s");
            if (_veh != null && _veh.Exists())
            {
                Function.Call(Hash.SET_VEHICLE_BOOST_ACTIVE, _veh, on);
                if (!on) Function.Call(Hash.SET_VEHICLE_CHEAT_POWER_INCREASE, _veh, 1.0f);
            }
            if (!_screenFx) return;
            if (on)
            {
                Function.Call(Hash.ANIMPOSTFX_STOP, "RaceTurbo");
                Function.Call(Hash.ANIMPOSTFX_PLAY, "RaceTurbo", 0, false);
                Function.Call(Hash.SHAKE_GAMEPLAY_CAM, "SKY_DIVING_SHAKE", 0.12f);
            }
            else
            {
                Function.Call(Hash.STOP_GAMEPLAY_CAM_SHAKING, true);
                Function.Call(Hash.ANIMPOSTFX_STOP, "RaceTurbo");
            }
        }

        // ---- backfire flames ----------------------------------------------------------

        private void Backfire()
        {
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core"))
            {
                if (!_ptfxRequested) { _ptfxRequested = true; Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core"); }
                return;
            }
            if (_exhausts == null)
            {
                _exhausts = new List<Vector3>();
                for (int i = 1; i <= 16; i++)
                {
                    int bone = Function.Call<int>(Hash.GET_ENTITY_BONE_INDEX_BY_NAME, _veh, i == 1 ? "exhaust" : "exhaust_" + i);
                    if (bone == -1) continue;
                    Vector3 at = Function.Call<Vector3>(Hash.GET_WORLD_POSITION_OF_ENTITY_BONE, _veh, bone);
                    _exhausts.Add(_veh.GetPositionOffset(at));
                }
            }
            foreach (Vector3 o in _exhausts)
            {
                Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
                Function.Call(Hash.START_PARTICLE_FX_NON_LOOPED_ON_ENTITY, "veh_backfire", _veh, o.X, o.Y, o.Z, 0f, 0f, 0f, BackfireScale, false, false, false);
            }
        }

        // ---- refilling (nitrous_c.lua updateRecharge) ----------------------------------

        private void Recharge(long now, float dt)
        {
            if (_fuel >= _tier.Capacity) return;
            float amount = DriftRecharge(dt);
            if (now <= _oncomingUntil) amount += _oncomingRefill * dt;
            // The traffic scans walk the vehicles around, so they run on an interval.
            if (now - _lastScanAt >= ScanMs)
            {
                _lastScanAt = now;
                Vehicle[] traffic = World.GetNearbyVehicles(_veh.Position, 60f);
                if (InOncomingTraffic(traffic)) _oncomingUntil = now + ScanMs + 50;
                amount += NearMissRecharge(traffic, now);
            }
            if (amount > 0f) _fuel = Math.Min(_tier.Capacity, _fuel + amount);
        }

        private float DriftRecharge(float dt)
        {
            if (_veh.IsInAir || _veh.Speed < 12f) return 0f;
            Vector3 v = Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, _veh, true);
            float sideways = Math.Abs(v.X), forward = Math.Abs(v.Y);
            if (sideways < 3f || sideways / Math.Max(forward, 0.1f) < 0.32f) return 0f;
            return _driftRefill * dt;
        }

        private bool IsTraffic(Vehicle other)
        {
            if (other == null || other.Handle == _veh.Handle || !other.Exists()) return false;
            Ped driver = other.GetPedOnSeat(VehicleSeat.Driver);
            return driver != null && driver.Exists() && !driver.IsPlayer;
        }

        private bool Ahead(Vehicle other)
        {
            Vector3 local = _veh.GetPositionOffset(other.Position);
            return local.Y > -2f && local.Y < 22f;
        }

        private bool InOncomingTraffic(Vehicle[] traffic)
        {
            if (_veh.Speed < 16f) return false;
            Vector3 fwd = _veh.ForwardVector;
            foreach (Vehicle other in traffic)
            {
                if (!IsTraffic(other) || !Ahead(other)) continue;
                if (Vector3.Dot(fwd, other.ForwardVector) < -0.55f) return true;
            }
            return false;
        }

        private float NearMissRecharge(Vehicle[] traffic, long now)
        {
            float speed = _veh.Speed;
            if (speed < 18f) return 0f;
            Vector3 pos = _veh.Position;
            foreach (Vehicle other in traffic)
            {
                if (!IsTraffic(other) || !Ahead(other)) continue;
                if (pos.DistanceTo(other.Position) > 4.2f || Math.Abs(speed - other.Speed) < 8f) continue;
                if (_nearMissCooldowns.TryGetValue(other.Handle, out long until) && now < until) continue;
                _nearMissCooldowns[other.Handle] = now + NearMissCooldownMs;
                return _nearMissRefill;
            }
            return 0f;
        }

        // =============================================================================
        // The pip: last in GTA6Hud's gauge row
        // =============================================================================

        private const float Pitch = 38f;     // GTA6Hud DrawVehicleGauges: gauge centres 38 px apart
        private static readonly Color Nitro = Color.FromArgb(255, 196, 224, 240);    // .pip.nitro --tone #c4e0f0
        private static readonly Color Unknown = Color.FromArgb(209, 206, 206, 212);  // .pip.unknown rgba(206,206,212,0.82): no boost left
        private static readonly Color Idle = Color.FromArgb(255, 132, 134, 140);     // GTA6Hud VehGaugeIdle: engine off

        private void DrawPip(float x, float cy, bool viaCard)
        {
            Color tone = !_veh.IsEngineRunning ? Idle : _fuel <= 0f ? Unknown : Nitro;
            // GTA6Hud DrawKitGauge: its disc at 32, the fill ring at 28, then the mark.
            if (viaCard) Gta6HudCard.Tex("hud_gauge_bg", x - 16f, cy - 16f, 32f, 32f, Color.White);
            else Function.Call(Hash.DRAW_SPRITE, "vipause", "hud_gauge_bg", x / Canvas.W, cy / Canvas.H, 32f / Canvas.W, 32f / Canvas.H, 0f, 255, 255, 255, 255, false);
            float level = Num.Clamp01(_shown / _tier.Capacity);
            if (level > 0.001f)
            {
                int step = Math.Max(1, Math.Min(48, (int)Math.Round(level * 48f)));
                Art.Entry fill = Art.Get("nitrous_fill_" + step.ToString("00"));
                if (fill != null) Art.Draw(fill, x - 14f, cy - 14f, tone);
            }
            Art.Entry mark = Art.Get("nitrous_mark");
            if (mark != null) Art.Draw(mark, x - mark.BoxW * 0.5f, cy - mark.BoxH * 0.5f, tone);
        }

        // =============================================================================
        // The "Hold [key] to activate nitrous" tip (vice_hud #nitro-hint)
        // =============================================================================

        private const float HintTop = 10.5f * 10.8f;     // top: 10.5cqh
        private const float HintKeyH = 1.52f * 19.2f;    // #nitro-hint .glyph --g-h: 1.52 --w

        private void DrawHint(long now)
        {
            bool inTheWay = HudBridge.UiInTheWay || CrashCam.IsPlaying || Binoculars.IsActive;
            // Nobody can read a tip behind a menu or a cutaway, and its clock would
            // run out before they could: it waits, then gets its whole time.
            if (_hintPending && !inTheWay)
            {
                _hintPending = false;
                _hintUntil = now + _hintMs;
            }
            if (_hintUntil < 0) return;
            if (now >= _hintUntil) { _hintUntil = -1; return; }
            if (inTheWay) return;

            Art.Entry head = Art.Get("nitrous_hint_head"), mid = Art.Get("nitrous_hint_mid"), tail = Art.Get("nitrous_hint_tail");
            if (head == null || mid == null || tail == null) return;
            Art.Entry key = Glyphs.ForControl(Glyphs.UsingPad ? _padControl : _kbControl);
            float scale = key != null ? HintKeyH / key.BoxH : 0f;
            float keyW = key != null ? key.BoxW * scale : 0f;

            // left: var(--map-left): the minimap's left edge, GTA6Hud's map here.
            HudBridge.VehicleGaugeRow(out _, out _, out float mapLeft);
            float x = mapLeft > 0f ? mapLeft : 0.0175f * Canvas.W;
            float y = HintTop;
            Art.Draw(head, x, y, Color.White);
            x += head.BoxW;
            if (key != null)
            {
                Canvas.Image(mid.Path, x, y - mid.BoxY, keyW, mid.H, Color.White);
                Art.Draw(key, x, y + (head.BoxH - key.BoxH * scale) * 0.5f, Color.White, scale);
                x += keyW;
            }
            Art.Draw(tail, x, y, Color.White);
        }
    }
}
