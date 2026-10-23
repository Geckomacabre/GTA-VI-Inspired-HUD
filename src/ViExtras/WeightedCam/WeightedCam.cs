using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// A driving camera with weight, as in GTA VI: land a jump, take a hard bump
    /// or crash, and the camera dips with the vehicle, as hard as the hit, then
    /// eases back to the view you had over half a second to a second.
    ///
    /// The dip is shown through a scripted camera that, for as long as the dip
    /// lasts, copies the game's own third-person vehicle camera every frame (where
    /// it is, where it looks, its field of view), nodded down by the dip. The
    /// game's camera itself is never touched: it goes on following the vehicle
    /// and the mouse or stick underneath, so the view handed back at the end is
    /// exactly the one the player would have had. Its pitch can't be nudged in
    /// place instead: in a vehicle, SET_GAMEPLAY_CAM_RELATIVE_PITCH lands a couple
    /// of degrees off what GET_GAMEPLAY_CAM_RELATIVE_PITCH reads (2 on a Sultan,
    /// 3.5 on a Sanchez, measured), so reading and writing it every frame walks the
    /// camera down to the ground. Not a shake (SHAKE_GAMEPLAY_CAM) either: a shake
    /// rattles both ways rather than dipping one way and coming back, and the
    /// camera has one at a time, which GTA6Hud's crash jolt and Nitrous use.
    ///
    /// Each hit dips like a critically damped spring kicked by it:
    /// peak * (t / T) * e^(1 - t / T), at the bottom at T, nearly back by 6T.
    /// Hits that come close together add up.
    ///
    /// Left alone: first person, aiming, looking behind, the cinematic camera, any
    /// other scripted camera (missions, CrashCam, GTA6Hud's shops), player control
    /// off, boats and aircraft. Any of those mid-dip hands the view straight back.
    /// </summary>
    public sealed class WeightedCam : Script
    {
        private const float Kmh = 3.6f;
        // How far back a hit is looked for: the game settles a hit in a frame or
        // three, while braking and cornering never change speed this fast.
        private const float WindowMs = 200f;
        // A hit is over once nothing has registered for this long.
        private const float QuietMs = 250f;

        private struct Sample { public float At; public Vector3 Velocity; }
        private struct Dip { public float Start, Peak, Tau; }

        private sealed class Meter
        {
            public float Taken;      // how much of the hit going on has been dipped for, 0..1
            public float Loudest;    // its biggest reading, km/h, for the log
            public float FirstLoud, LastLoud = float.MinValue;
        }

        // ---- settings, [WeightedCam] in ViExtras.ini ------------------------------
        private readonly bool _enabled, _debug;
        private readonly float _strength;
        private readonly float _landMin, _landFull, _landDip, _landMs;
        private readonly float _crashMin, _crashFull, _crashDip, _crashMs;

        private readonly List<Sample> _samples = new List<Sample>();
        private readonly List<Dip> _dips = new List<Dip>();
        private readonly Meter _land = new Meter(), _crash = new Meter();
        private float _clock;        // game milliseconds: slow motion slows the dip too
        private Vehicle _veh;
        private Vector3 _lastPos;
        private float _airMs, _lastAirMs, _landedAt = float.MinValue;
        private string _notOurs = "";   // why the camera was left alone last frame, for the debug log

        // The game camera's turn last frame. The script runs before the game moves
        // its camera, so what it reads is a frame old; the turn carries it on by one.
        private Vector3 _lastRot;
        private bool _hasLastRot;
        private int _cam = -1;              // the camera a dip shows through, while it does
        private static int _showing = -1;   // the same, for CrashCam to take over from

        /// <summary>The dip's camera is the one on screen: CrashCam may cut away from it.</summary>
        public static bool Owns(Camera cam) => cam != null && _showing != -1 && cam.Handle == _showing;

        public WeightedCam()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("WeightedCam", "Enabled", true);
            _strength = Config.Get("WeightedCam", "Strength", 1f);
            _landMin = Config.Get("WeightedCam", "LandMin", 9f);
            _landFull = Config.Get("WeightedCam", "LandFull", 45f);
            _landDip = Config.Get("WeightedCam", "LandDip", 4.5f);
            _landMs = Config.Get("WeightedCam", "LandMs", 110f);
            _crashMin = Config.Get("WeightedCam", "CrashMin", 20f);
            _crashFull = Config.Get("WeightedCam", "CrashFull", 80f);
            _crashDip = Config.Get("WeightedCam", "CrashDip", 3.5f);
            _crashMs = Config.Get("WeightedCam", "CrashMs", 75f);
            _debug = Config.Get("WeightedCam", "Debug", false);

            Tick += OnTick;
            // Never leave the view on a camera nobody is moving any more.
            Aborted += (s, e) => Drop();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled || _strength <= 0f) return;
            if (!GameReady.Now)
            {
                Drop();
                Forget();
                return;
            }
            // An exception out of a tick stops the script for the session.
            try { Run(); }
            catch (Exception ex)
            {
                Log.Write("WeightedCam", ex.ToString());
                Drop();
                Forget();
            }
        }

        private void Run()
        {
            // A pause menu: the view back as it was (the VI one may not stop the game).
            if (HudBridge.Paused)
            {
                Drop();
                return;
            }
            float dt = Game.LastFrameTime * 1000f;
            if (dt <= 0f) return;
            _clock += dt;
            // Whether the game camera's rotation, as read, has its shake in it (a
            // dip's camera copies that rotation): on foot, firing a gun shakes it.
            if (_debug && _cam == -1 && Function.Call<bool>(Hash.IS_GAMEPLAY_CAM_SHAKING))
            {
                Vector3 r = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 2);
                Log.Write("WeightedCam", "shaking: game pitch " + r.X.ToString("0.000") + " heading " + r.Z.ToString("0.000") + " roll " + r.Y.ToString("0.000"));
            }

            Player player = Game.Player;
            Ped ped = player.Character;
            Vehicle veh = ped.CurrentVehicle;
            if (veh == null || !veh.Exists() || ped.IsDead || !Weighted(veh))
            {
                Drop();
                Forget();
                return;
            }
            if (veh != _veh)
            {
                Forget();
                _veh = veh;
            }

            Vector3 pos = veh.Position, vel = veh.Velocity;
            // Further than it could have gone in a frame: a teleport, or a script
            // putting it somewhere. Its old speeds would read as the hit of a lifetime.
            if (_samples.Count > 0 && pos.DistanceTo(_lastPos) > 3f + vel.Length() * dt / 1000f * 2f) _samples.Clear();
            _lastPos = pos;

            bool inAir = Function.Call<bool>(Hash.IS_ENTITY_IN_AIR, veh);
            if (inAir) _airMs += dt;
            else if (_airMs > 0f)
            {
                // a bounce straight after a big drop keeps the drop's air time
                if (_clock - _landedAt > 1000f || _airMs > _lastAirMs) _lastAirMs = _airMs;
                _airMs = 0f;
                _landedAt = _clock;
            }

            _samples.Add(new Sample { At = _clock, Velocity = vel });
            while (_samples.Count > 0 && _clock - _samples[0].At > WindowMs) _samples.RemoveAt(0);

            // Falling stopped: how much of the speed it was coming down at is gone
            // (a ramp throwing it up stops no fall, and doesn't count). Crashing:
            // how much its speed along the ground has changed, any way round.
            float lowest = 0f, jolt = 0f;
            foreach (Sample s in _samples)
            {
                if (s.Velocity.Z < lowest) lowest = s.Velocity.Z;
                float dx = vel.X - s.Velocity.X, dy = vel.Y - s.Velocity.Y;
                jolt = Math.Max(jolt, dx * dx + dy * dy);
            }
            float fall = (Math.Min(vel.Z, 0f) - lowest) * Kmh;
            jolt = (float)Math.Sqrt(jolt) * Kmh;

            // the game camera, a frame old, and how far it turned in its last frame
            Vector3 rot = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 2);
            Vector3 turn = Vector3.Zero;
            if (_hasLastRot)
                turn = new Vector3(Num.Clamp(rot.X - _lastRot.X, -5f, 5f), 0f, Num.Clamp(Wrap(rot.Z - _lastRot.Z), -5f, 5f));
            _lastRot = rot;
            _hasLastRot = true;

            string notOurs = NotOurs(player);
            if (_debug && notOurs != _notOurs) Log.Write("WeightedCam", notOurs == null ? "camera ours" : "camera left alone: " + notOurs);
            _notOurs = notOurs;
            if (notOurs != null)
            {
                Drop();
                return;
            }
            Register(_land, fall, _landMin, _landFull, _landDip, _landMs);
            Register(_crash, jolt, _crashMin, _crashFull, _crashDip, _crashMs);
            Show(rot + turn, vel * (dt / 1000f), inAir, vel.Z);
        }

        private static float Wrap(float deg)
        {
            while (deg > 180f) deg -= 360f;
            while (deg < -180f) deg += 360f;
            return deg;
        }

        // Cars, bikes, quads and bicycles: not boats (every wave a landing),
        // helicopters, planes or trains.
        private static bool Weighted(Vehicle v)
        {
            int c = (int)v.ClassType;
            return c != 14 && c != 15 && c != 16 && c != 21;
        }

        // Null when the game's own third-person vehicle camera is the one on screen
        // (or under the dip's camera) and nobody else wants it, the player least of
        // all; otherwise who has it.
        private string NotOurs(Player player)
        {
            if (CrashCam.IsPlaying) return "the crash camera";
            if (Binoculars.IsActive) return "the binoculars";
            if (HudBridge.Busy) return "a cutscene, character switch or black screen";
            if (HudBridge.HudMenuOpen) return "a GTA6Hud shop, camera or menu";
            if (!Function.Call<bool>(Hash.IS_PLAYER_CONTROL_ON, player)) return "player control off";
            int cam = Function.Call<int>(Hash.GET_RENDERING_CAM);
            if (cam != -1 && cam != _cam && Function.Call<bool>(Hash.DOES_CAM_EXIST, cam)) return "a scripted camera";
            if (Function.Call<bool>(Hash.IS_AIM_CAM_ACTIVE) || Function.Call<bool>(Hash.IS_FIRST_PERSON_AIM_CAM_ACTIVE)
                || Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, player)) return "aiming";
            if (Function.Call<bool>(Hash.IS_GAMEPLAY_CAM_LOOKING_BEHIND)) return "looking behind";
            // first person, in whichever context this vehicle uses (car, bike...)
            int context = Function.Call<int>(Hash.GET_CAM_ACTIVE_VIEW_MODE_CONTEXT);
            if (Function.Call<int>(Hash.GET_CAM_VIEW_MODE_FOR_CONTEXT, context) == 4) return "first person";
            // While the dip's own camera is on screen these read it, not the game's.
            if (_cam != -1) return null;
            if (!Function.Call<bool>(Hash.IS_GAMEPLAY_CAM_RENDERING)) return "not the gameplay camera";
            if (Function.Call<bool>(Hash.IS_CINEMATIC_CAM_RENDERING)) return "the cinematic camera";
            if (!Function.Call<bool>(Hash.IS_FOLLOW_VEHICLE_CAM_ACTIVE)) return "not the vehicle camera";
            return null;
        }

        // A reading of `value` km/h. Over `min` it dips the camera, all of `dip`
        // degrees by `full`. A hit takes a few frames to read in full, so only the
        // part of it not dipped for yet is added each frame; the parts, a frame or
        // two apart, move as one.
        private void Register(Meter m, float value, float min, float full, float dip, float tau)
        {
            if (value < min)
            {
                if (m.Loudest > 0f && _clock - m.LastLoud > QuietMs) Finish(m, dip);
                return;
            }
            if (m.Taken == 0f && m.Loudest == 0f) m.FirstLoud = _clock;
            m.LastLoud = _clock;
            m.Loudest = Math.Max(m.Loudest, value);
            float s = Num.Clamp01((value - min) / Math.Max(full - min, 0.01f));
            if (s < m.Taken + 0.02f) return;
            _dips.Add(new Dip { Start = _clock, Peak = (s - m.Taken) * dip * _strength, Tau = Math.Max(tau, 10f) });
            m.Taken = s;
        }

        private void Finish(Meter m, float dip)
        {
            if (m.Taken > 0f && (_debug || m.Taken >= 0.25f))
            {
                string what = m == _land && _lastAirMs >= 250f && Math.Abs(m.FirstLoud - _landedAt) < 1000f
                    ? "landing after " + (_lastAirMs / 1000f).ToString("0.0") + " s in the air, falling at " + m.Loudest.ToString("0") + " km/h"
                    : m == _land ? "bump, falling at " + m.Loudest.ToString("0") + " km/h"
                    : "crash, " + m.Loudest.ToString("0") + " km/h of speed lost";
                Log.Write("WeightedCam", what + ": dipped " + (m.Taken * dip * _strength).ToString("0.0") + " deg");
            }
            m.Taken = 0f;
            m.Loudest = 0f;
        }

        // The view for this frame: the game camera carried on by a frame (turned
        // by `rot`, moved by `step`, the vehicle's way), nodded down by the dips.
        private void Show(Vector3 rot, Vector3 step, bool inAir, float vz)
        {
            float dip = 0f;
            for (int i = _dips.Count - 1; i >= 0; i--)
            {
                Dip d = _dips[i];
                float x = (_clock - d.Start) / d.Tau;
                if (x >= 8f)
                {
                    _dips.RemoveAt(i);
                    continue;
                }
                dip += d.Peak * x * (float)Math.Exp(1f - x);
            }
            if (_dips.Count == 0)
            {
                EndCam();
                return;
            }
            // several hard hits at once still only dip so far
            dip = Math.Min(dip, Math.Max(_landDip, _crashDip) * _strength * 1.5f);

            Vector3 at = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_COORD) + step;
            float fov = Function.Call<float>(Hash.GET_GAMEPLAY_CAM_FOV);
            if (_cam == -1)
            {
                _cam = Function.Call<int>(Hash.CREATE_CAM, "DEFAULT_SCRIPTED_CAMERA", false);
                if (!Function.Call<bool>(Hash.DOES_CAM_EXIST, _cam))
                {
                    _cam = -1;
                    _dips.Clear();
                    return;
                }
                Function.Call(Hash.SET_CAM_COORD, _cam, at.X, at.Y, at.Z);
                Function.Call(Hash.SET_CAM_ROT, _cam, rot.X - dip, rot.Y, rot.Z, 2);
                Function.Call(Hash.SET_CAM_FOV, _cam, fov);
                Function.Call(Hash.SET_CAM_ACTIVE, _cam, true);
                Function.Call(Hash.RENDER_SCRIPT_CAMS, true, false, 0, true, false, 0);
                _showing = _cam;
            }
            Function.Call(Hash.SET_CAM_COORD, _cam, at.X, at.Y, at.Z);
            Function.Call(Hash.SET_CAM_ROT, _cam, rot.X - dip, rot.Y, rot.Z, 2);
            Function.Call(Hash.SET_CAM_FOV, _cam, fov);
            if (_debug)
                Log.Write("WeightedCam", "t " + _clock.ToString("0") + " dip " + dip.ToString("0.00") + " game pitch " + rot.X.ToString("0.00")
                    + " heading " + rot.Z.ToString("0.0") + " fov " + fov.ToString("0.0")
                    + (Function.Call<bool>(Hash.IS_GAMEPLAY_CAM_SHAKING) ? " shaking" : "") + (inAir ? " in the air" : "") + " vz " + vz.ToString("0.0"));
        }

        // The game's camera back on screen, if the dip's is still the one there
        // (another script's camera that took over is left alone).
        private void EndCam()
        {
            if (_cam == -1) return;
            try
            {
                if (Function.Call<int>(Hash.GET_RENDERING_CAM) == _cam) Function.Call(Hash.RENDER_SCRIPT_CAMS, false, false, 0, true, false, 0);
                if (Function.Call<bool>(Hash.DOES_CAM_EXIST, _cam)) Function.Call(Hash.DESTROY_CAM, _cam, false);
            }
            catch { }
            _cam = -1;
            _showing = -1;
        }

        // Ends any dip, handing the view straight back.
        private void Drop()
        {
            if (_debug && _cam != -1) Log.Write("WeightedCam", "dip ended early");
            EndCam();
            _dips.Clear();
            _land.Taken = _crash.Taken = 0f;
            _land.Loudest = _crash.Loudest = 0f;
        }

        // A new vehicle, or none: what the last one was doing says nothing now.
        private void Forget()
        {
            _veh = null;
            _samples.Clear();
            _airMs = 0f;
            _lastAirMs = 0f;
            _hasLastRot = false;
        }
    }
}
