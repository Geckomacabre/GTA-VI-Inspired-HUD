using System;
using System.Reflection;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The slim jim, done properly. GTA6Hud's SLIM JIM (a locked car's door, and its Waink parked cars) plays the
    /// game's veh@break_in@0h@p_m_one@ low_force_entry_ds on the upper body, with nothing in his hands. While it runs,
    /// this plays the vihudanims pack's own clips instead (vi_slimjim: slimjim_in, slimjim_loop, slimjim_out, made in
    /// vi hud\slimjim\anim\make_slimjim.py on top of that same GTA clip) with the pack's 50 cm strip,
    /// vi_prop_slimjim, in his right fist:
    ///
    ///  * in (1.0 s): from GTA's idle he bends to the window, both fists on the strip, and works it into the gap;
    ///  * the loop (0.8 s) for as long as GTA6Hud's slim jim ring runs (its ViSlimCar; older builds: the lock trace /
    ///    progress);
    ///  * out (0.8 s): he pulls it out, straightens, and is back in GTA's idle.
    ///
    /// He's brought to where the clips were made for: 30 cm out from the front door's handle, facing it, the strip
    /// 5 cm to his left, slid there over the first 0.3 s. [Switches] SlimJim in ViExtras.ini turns it off (GTA6Hud's
    /// own clip then plays); GTA6Hud's [BreakIn] SlimJimAnim off leaves both off.
    /// Without the pack (no vi_slimjim), GTA6Hud's own clip is left to play. GTA6Hud.dll itself is not changed.
    /// </summary>
    public sealed class SlimJim : Script
    {
        private const string Dict = "vi_slimjim", InClip = "slimjim_in", LoopClip = "slimjim_loop", OutClip = "slimjim_out";
        private const string GtaDict = "veh@break_in@0h@p_m_one@", GtaClip = "low_force_entry_ds";
        private const string PropModel = "vi_prop_slimjim";
        private const int RightHand = 57005;            // SKEL_R_Hand
        private const int InMs = 1000, OutMs = 800, SlideMs = 300;
        // the middle of his right fist from the wrist, in the hand bone's frame (make_slimjim.py PH_OFF + LOOP), where
        // the strip's handle goes. The prop's axes are the hand bone's, so it needs no turn.
        private static readonly Vector3 Grip = new Vector3(0.0934f, 0f, -0.003f);
        private const float StandOut = 0.30f, StripLeft = 0.05f, SkinIn = 0.03f, MaxSlide = 1.2f;

        private enum Stage { Off, In, Loop, Out }

        private readonly bool _enabled;
        private Stage _stage;
        private long _stageAt, _requestAt;
        private Prop _prop;
        private Vehicle _kept, _keepFor;
        private long _keepUntil;
        private const int KeepMs = 20000;
        private const float KeepNear = 15f;
        private Vector3 _slideFrom, _slideTo;
        private float _headFrom, _headTo;
        private bool _warned;

        // GTA6Hud's slim jim state
        private bool _looked;
        private object _hud;
        private FieldInfo _entryVehicle, _entryCompleted, _wainkCar, _wainkOn, _animSetting;
        // GTA6Hud's own slim jim ring (its ViSlim patch): the car being worked until the result is in, door or Waink
        private FieldInfo _ringCar;

        public SlimJim()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get(Config.Switches, "SlimJim", true);
            Tick += OnTick;
            Aborted += (s, e) => { Release(); Finish(Game.Player.Character, stopClip: true); };
        }

        private bool Look()
        {
            if (_hud != null) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = hud.GetType();
            _entryVehicle = t.GetField("_entryVehicle", Own);
            _entryCompleted = t.GetField("_entryCompleted", Own);
            _wainkCar = t.GetField("_slimJimCar", Own);
            _wainkOn = t.GetField("_wainkAnimOn", Own);
            _animSetting = t.Assembly.GetType("GTA6Hud.Ref", false)?.GetField("SlimJimAnim", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            _ringCar = t.GetField("ViSlimCar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (_entryVehicle == null || _entryCompleted == null || _wainkCar == null || _wainkOn == null)
            {
                Log.Write("SlimJim", "GTA6Hud's slim jim was not found; its own break-in clip stays");
                return false;
            }
            _hud = hud;
            return true;
        }

        // The car GTA6Hud is slim-jimming right now, or null.
        private Vehicle Working(Ped player)
        {
            try
            {
                if (_animSetting != null && !(bool)_animSetting.GetValue(null)) return null;
                if (_ringCar != null) return _ringCar.GetValue(null) is Vehicle ring && ring.Exists() ? ring : null;
                if (_entryVehicle.GetValue(_hud) is Vehicle door && door.Exists() && !(bool)_entryCompleted.GetValue(_hud)) return door;
                if ((bool)_wainkOn.GetValue(_hud) && _wainkCar.GetValue(_hud) is Vehicle parked && parked.Exists()) return parked;
            }
            catch { }
            return null;
        }

        private static bool Playing(Ped ped, string clip) => Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, ped, Dict, clip, 3);

        private static void Play(Ped ped, string clip, float blendIn, int flags) =>
            Function.Call(Hash.TASK_PLAY_ANIM, ped, Dict, clip, blendIn, -4f, -1, flags, 0f, false, false, false);

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled) return;
            Ped player = Game.Player.Character;
            if (RealTime.Ms >= _requestAt)
            {
                // kept loaded (14 KB), so the clip is there the moment GTA6Hud starts its own
                _requestAt = RealTime.Ms + 5000;
                if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, Dict)) Function.Call(Hash.REQUEST_ANIM_DICT, Dict);
            }
            if (!Look()) return;
            bool ok = player != null && player.Exists() && !player.IsDead && !player.IsInVehicle() && !player.IsRagdoll;
            Vehicle car = ok ? Working(player) : null;
            long now = RealTime.Ms;

            if (car != null)
            {
                Keep(car, player, now);
                if (_stage == Stage.Off || _stage == Stage.Out)
                {
                    if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, Dict))
                    {
                        if (!_warned) { _warned = true; Log.Write("SlimJim", "vi_slimjim isn't loaded (is the vihudanims pack registered?); GTA6Hud's own clip plays"); }
                        return;
                    }
                    Begin(player, car);
                }
                // GTA6Hud starts its own clip once, when the slim jim starts
                if (Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, player, GtaDict, GtaClip, 3))
                    Function.Call(Hash.STOP_ANIM_TASK, player, GtaDict, GtaClip, 1000f);
                if (_stage == Stage.In && now - _stageAt >= InMs - 50)
                {
                    Play(player, LoopClip, 8f, 1);
                    _stage = Stage.Loop;
                    _stageAt = now;
                }
                Slide(player, now);
                HoldProp(player);
                return;
            }
            Keep(null, player, now);

            if (_stage == Stage.In || _stage == Stage.Loop)
            {
                if (ok)
                {
                    Play(player, OutClip, 8f, 0);
                    _stage = Stage.Out;
                    _stageAt = now;
                }
                else Finish(player, stopClip: false);
            }
            else if (_stage == Stage.Out)
            {
                // done, or something else took him over (he pressed enter, a cop grabbed him...)
                if (now - _stageAt >= OutMs || !ok || (now - _stageAt > 150 && !Playing(player, OutClip))) Finish(player, stopClip: false);
                else HoldProp(player);
            }
        }

        // The car being worked can't be cleaned up under him (street cars and released script cars vanished mid-ring,
        // 2026-10-27): a car the game owns is held as this script's while the ring runs, and after it until he's in it,
        // has walked off or KeepMs has passed. One a script already holds is left alone.
        private void Keep(Vehicle car, Ped player, long now)
        {
            if (car != null)
            {
                _keepUntil = now + KeepMs;
                if (car == _keepFor) return;
                _keepFor = car;
                if (car == _kept) return;              // back on the car still held from the last go
                Release();
                if (!car.Exists() || Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, car)) return;
                Function.Call(Hash.SET_ENTITY_AS_MISSION_ENTITY, car, true, true);
                _kept = car;
                return;
            }
            _keepFor = null;
            if (_kept != null && (!_kept.Exists() || now >= _keepUntil || player == null || !player.Exists()
                || player.IsInVehicle(_kept) || player.Position.DistanceTo(_kept.Position) > KeepNear)) Release();
        }

        private void Release()
        {
            if (_kept != null && _kept.Exists()) _kept.MarkAsNoLongerNeeded();
            _kept = null;
        }

        private void Begin(Ped player, Vehicle car)
        {
            long now = RealTime.Ms;
            bool again = _stage == Stage.Out;   // straight back in from a pull-out
            _stage = Stage.In;
            _stageAt = now;
            player.Weapons.Select(WeaponHash.Unarmed, true);
            Play(player, InClip, again ? 8f : 4f, 2);

            // where the clips were made for: facing the front door, 30 cm out from its handle, the strip 5 cm to his left
            _slideFrom = player.Position;
            _headFrom = player.Heading;
            _slideTo = _slideFrom;
            _headTo = _headFrom;
            if (DoorHandle(car, player.Position, out Vector3 handle, out Vector3 outward))
            {
                Vector3 facing = -outward;
                Vector3 left = Vector3.Cross(Vector3.WorldUp, facing);
                Vector3 strip = handle - outward * SkinIn;
                Vector3 to = strip + outward * StandOut - left * StripLeft;
                // a step, not a glide across the car (a Waink car can be worked from further off)
                if (Vector3.Distance2D(_slideFrom, to) <= MaxSlide)
                {
                    _slideTo = new Vector3(to.X, to.Y, _slideFrom.Z);
                    _headTo = (float)(Math.Atan2(-facing.X, facing.Y) * 180.0 / Math.PI);
                }
            }
            if (again) _slideFrom = _slideTo;
        }

        // The nearer front door's handle (its door when the car has no handle bones), and which way is out from it.
        private static bool DoorHandle(Vehicle car, Vector3 from, out Vector3 handle, out Vector3 outward)
        {
            handle = outward = Vector3.Zero;
            float best = float.MaxValue;
            foreach (string[] names in new[] { new[] { "handle_dside_f", "door_dside_f" }, new[] { "handle_pside_f", "door_pside_f" } })
            {
                foreach (string name in names)
                {
                    EntityBone bone = car.Bones[name];
                    if (bone == null || !bone.IsValid) continue;
                    Vector3 p = bone.Position;
                    float d = Vector3.DistanceSquared(p, from);
                    if (d < best)
                    {
                        best = d;
                        handle = p;
                        Vector3 right = car.RightVector;
                        right.Z = 0f;
                        right.Normalize();
                        outward = Vector3.Dot(p - car.Position, right) < 0f ? -right : right;
                    }
                    break;
                }
            }
            return best < float.MaxValue;
        }

        private void Slide(Ped player, long now)
        {
            float t = Num.Clamp01((now - _stageAt) / (float)SlideMs);
            if (_stage != Stage.In || (t >= 1f && player.Position.DistanceTo2D(_slideTo) < 0.02f)) return;
            float e = Num.EaseInOut(t);
            Vector3 p = Vector3.Lerp(_slideFrom, _slideTo, e);
            // keepTasks and keepIK on: with keepTasks off every step dropped slimjim_in, so he T-posed until the loop
            // started. doWarp off: a 30 cm step, not a teleport the game's population has to catch up with.
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, player, p.X, p.Y, _slideFrom.Z, true, true, false);
            float turn = ((_headTo - _headFrom) % 360f + 540f) % 360f - 180f;
            player.Heading = _headFrom + turn * e;
        }

        // The strip in his right fist, made as soon as its model is in.
        private void HoldProp(Ped player)
        {
            if (_prop != null && _prop.Exists()) return;
            var model = new Model(PropModel);
            if (!model.IsInCdImage)
            {
                if (!_warned) { _warned = true; Log.Write("SlimJim", PropModel + " isn't in the game (is the vihudanims pack registered?)"); }
                return;
            }
            if (!model.IsLoaded) { model.Request(); return; }
            _prop = World.CreateProp(model, player.Position, false, false);
            model.MarkAsNoLongerNeeded();
            if (_prop == null) return;
            _prop.IsCollisionEnabled = false;
            int bone = Function.Call<int>(Hash.GET_PED_BONE_INDEX, player, RightHand);
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, _prop, player, bone, Grip.X, Grip.Y, Grip.Z, 0f, 0f, 0f, false, false, false, false, 2, true);
        }

        private void Finish(Ped player, bool stopClip)
        {
            if (_prop != null)
            {
                if (_prop.Exists()) _prop.Delete();
                _prop = null;
            }
            if (stopClip && _stage != Stage.Off && player != null && player.Exists() && !player.IsDead)
                foreach (string clip in new[] { InClip, LoopClip, OutClip })
                    if (Playing(player, clip)) Function.Call(Hash.STOP_ANIM_TASK, player, Dict, clip, 4f);
            _stage = Stage.Off;
        }
    }
}
