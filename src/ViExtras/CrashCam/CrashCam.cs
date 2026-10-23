using System;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The FiveM version's crash cam (resources/vi_crashcam), for single player.
    ///
    /// Crash hard enough and the game cuts away. The hit itself is seen, already
    /// slowed, from wherever you were looking; then a hard cut to a camera
    /// standing well back from the car, the edges of the screen closed in and
    /// the HUD gone; then a hard cut back to exactly the view you had, with time
    /// running back up to normal.
    ///
    /// GTA6Hud has a crash camera of its own ([Cinematics] CrashCamera in
    /// GTA6Hud.ini). It is a different shot; leave it off while this one is on.
    /// </summary>
    public sealed class CrashCam : Script
    {
        private const float Kmh = 3.6f;

        private sealed class Shot
        {
            public float Ahead, Side, Height, Fov;
            public Shot(float ahead, float side, float height, float fov) { Ahead = ahead; Side = side; Height = height; Fov = fov; }
        }

        // ---- settings, [CrashCam] in ViExtras.ini --------------------------------
        private readonly bool _enabled;
        private readonly float _minSpeed, _minSpeedLoss;
        private readonly int _sampleMs, _cooldownMs;
        private readonly bool _driverOnly, _skipInMissions;
        private readonly float _timeScale;
        private readonly int _impactHoldMs, _durationMs, _easeOutMs, _hudReturnMs, _blackFlashMs, _fadeInMs;
        private readonly bool _overlay;
        private readonly float _handShake;
        private readonly Keys _testKey;
        private readonly Shot[] _shots;

        private readonly CineFx _fx = new CineFx();
        private readonly Random _rng = new Random();

        private bool _playing;
        private long _readyAt;
        private long _nextSample;
        private Camera _cam;
        private bool _minimapHeld;
        private bool _testRequested;

        // the last sample
        private Vehicle _lastVeh;
        private float _lastSpeed;
        private Vector3 _lastPos;
        private bool _hasLastPos;
        private Vector3? _lastDir;

        public CrashCam()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("CrashCam", "Enabled", true);
            _minSpeed = Config.Get("CrashCam", "MinSpeed", 80f);
            _minSpeedLoss = Config.Get("CrashCam", "MinSpeedLoss", 45f);
            _sampleMs = Config.Get("CrashCam", "SampleMs", 100);
            _driverOnly = Config.Get("CrashCam", "DriverOnly", true);
            _cooldownMs = Config.Get("CrashCam", "CooldownMs", 20000);
            _skipInMissions = Config.Get("CrashCam", "SkipInMissions", true);
            _timeScale = Config.Get("CrashCam", "TimeScale", 0.2f);
            _impactHoldMs = Config.Get("CrashCam", "ImpactHoldMs", 350);
            _durationMs = Config.Get("CrashCam", "DurationMs", 3400);
            _easeOutMs = Config.Get("CrashCam", "EaseOutMs", 450);
            _hudReturnMs = Config.Get("CrashCam", "HudReturnMs", 800);
            _blackFlashMs = Config.Get("CrashCam", "BlackFlashMs", 0);
            _fadeInMs = Config.Get("CrashCam", "FadeInMs", 0);
            _overlay = Config.Get("CrashCam", "Overlay", true);
            _fx.Strength = Config.Get("CrashCam", "OverlayStrength", 1f);
            _fx.Bars = Config.Get("CrashCam", "OverlayBars", 0.032f);
            _fx.Pulse = Config.Get("CrashCam", "OverlayPulse", true);
            _handShake = Config.Get("CrashCam", "HandShake", 0.25f);
            _testKey = Config.Get("CrashCam", "TestKey", Keys.None);
            _shots = new[]
            {
                // low at the roadside, side on
                new Shot(Config.Get("CrashCam", "Shot1Ahead", 6f), Config.Get("CrashCam", "Shot1Side", 15f), Config.Get("CrashCam", "Shot1Height", 0.6f), Config.Get("CrashCam", "Shot1Fov", 40f)),
                // low, further up the road, the car coming to it
                new Shot(Config.Get("CrashCam", "Shot2Ahead", 16f), Config.Get("CrashCam", "Shot2Side", 7f), Config.Get("CrashCam", "Shot2Height", 0.9f), Config.Get("CrashCam", "Shot2Fov", 36f)),
                // up and well back, looking down on it
                new Shot(Config.Get("CrashCam", "Shot3Ahead", 4f), Config.Get("CrashCam", "Shot3Side", 19f), Config.Get("CrashCam", "Shot3Height", 7.5f), Config.Get("CrashCam", "Shot3Fov", 42f)),
            };

            Tick += OnTick;
            KeyDown += (s, e) => { if (_testKey != Keys.None && e.KeyCode == _testKey) _testRequested = true; };
            Aborted += (s, e) => Restore();
        }

        public static bool IsPlaying { get; private set; }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            // An exception out of a tick stops the script for the session.
            try
            {
                Gta6HudCrashToggle.Tick();
                Detect();
            }
            catch (Exception ex) { Log.Write("CrashCam", ex.ToString()); }
        }

        private void Detect()
        {
            if (_testRequested)
            {
                _testRequested = false;
                Vehicle v = Game.Player.Character.CurrentVehicle;
                if (v != null && v.Exists()) Play(v, Flat(v.Velocity));
                return;
            }

            if (!_enabled || !Gta6HudCrashToggle.On || _playing || RealTime.Ms < _nextSample) return;
            _nextSample = RealTime.Ms + _sampleMs;

            Ped ped = Game.Player.Character;
            Vehicle veh = ped.CurrentVehicle;
            bool driving = veh != null && veh.Exists() && (!_driverOnly || veh.Driver == ped);
            if (!driving || ped.IsDead)
            {
                _lastVeh = null;
                _hasLastPos = false;
                _lastDir = null;
                _nextSample = RealTime.Ms + 500;
                return;
            }

            float speed = veh.Speed * Kmh;
            Vector3 pos = veh.Position;

            // The distance test keeps a teleport from reading as the biggest
            // crash of all time.
            if (veh == _lastVeh && _hasLastPos
                && _lastSpeed >= _minSpeed
                && _lastSpeed - speed >= _minSpeedLoss
                && pos.DistanceTo(_lastPos) < 40f
                && RealTime.Ms >= _readyAt
                && !(_skipInMissions && Game.IsMissionActive))
            {
                Vector3? dir = _lastDir;
                _lastVeh = null;
                _hasLastPos = false;
                _lastDir = null;
                Play(veh, dir);
            }
            else
            {
                _lastVeh = veh;
                _lastSpeed = speed;
                _lastPos = pos;
                _hasLastPos = true;
                // The way it was GOING, kept from before the hit: afterwards the
                // car may be spinning or stopped and has no useful heading.
                _lastDir = Flat(veh.Velocity) ?? _lastDir;
            }
        }

        private static Vector3? Flat(Vector3 v)
        {
            float len = (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
            if (len < 0.001f) return null;
            return new Vector3(v.X / len, v.Y / len, 0f);
        }

        // Can the camera, standing at `from`, see the car? Anything solid in
        // between (a wall, a truck, the barrier the car just went through) rules
        // the spot out. Map and objects, as vi_crashcam's probe (flags 1 | 16).
        private static bool ClearView(Vector3 from, Vehicle veh)
        {
            Vector3 to = veh.Position + new Vector3(0f, 0f, 0.5f);
            int ray = Function.Call<int>(Hash.START_EXPENSIVE_SYNCHRONOUS_SHAPE_TEST_LOS_PROBE,
                from.X, from.Y, from.Z, to.X, to.Y, to.Z, 1 | 16, veh, 4);
            var hit = new OutputArgument();
            var end = new OutputArgument();
            var normal = new OutputArgument();
            var entity = new OutputArgument();
            Function.Call<int>(Hash.GET_SHAPE_TEST_RESULT, ray, hit, end, normal, entity);
            return !hit.GetResult<bool>();
        }

        // One of the shots, placed. Walked from a random start so two crashes
        // in a row are not filmed the same way; each shot is tried on both sides
        // of the line of travel, then closer in.
        private void FindSpot(Vehicle veh, Vector3? travel, out Vector3 spot, out float fov)
        {
            Vector3 pos = veh.Position;
            Vector3 dir = travel ?? Flat(veh.ForwardVector) ?? new Vector3(0f, 1f, 0f);
            var side = new Vector3(-dir.Y, dir.X, 0f);
            int first = _rng.Next(_shots.Length);
            float firstSign = _rng.Next(2) == 0 ? 1f : -1f;
            float[] scales = { 1f, 0.65f, 0.4f };

            for (int step = 0; step < _shots.Length; step++)
            {
                Shot shot = _shots[(first + step) % _shots.Length];
                foreach (float scale in scales)
                {
                    foreach (float sign in new[] { firstSign, -firstSign })
                    {
                        Vector3 at = pos + dir * (shot.Ahead * scale) + side * (shot.Side * scale * sign);
                        var groundZ = new OutputArgument();
                        bool found = Function.Call<bool>(Hash.GET_GROUND_Z_FOR_3D_COORD, at.X, at.Y, pos.Z + 3f, groundZ, false, false);
                        float gz = groundZ.GetResult<float>();
                        if (found && Math.Abs(gz - pos.Z) < 4f)
                        {
                            var candidate = new Vector3(at.X, at.Y, gz + shot.Height);
                            if (ClearView(candidate, veh))
                            {
                                spot = candidate;
                                fov = shot.Fov;
                                return;
                            }
                        }
                    }
                }
            }

            // Nothing had a clear view (a tunnel, a multi-storey): stand just
            // behind and above the car instead of not filming it at all.
            spot = pos - dir * 7f + side * 3.5f + new Vector3(0f, 0f, 2.8f);
            fov = 45f;
        }

        private void HideHud(bool hidden)
        {
            if (hidden == _minimapHeld) return;
            _minimapHeld = hidden;
            HudBridge.HoldMinimapHidden(hidden);
            // GTA6Hud's vehicle card (engine, fuel and waypoint gauges), area plate
            // and police arrows: vice_hud's whole HUD is hidden for the shot.
            HudBridge.HoldGta6HudPanelsHidden(hidden);
        }

        private void DrawOverlay()
        {
            if (_overlay) _fx.Draw();
        }

        // Hold for `ms` of real time, running `each` every frame. `each` gets the
        // progress 0..1 and returns false to stop early.
        private void Hold(int ms, Func<float, bool> each)
        {
            long start = RealTime.Ms;
            int frames = 0;
            while (RealTime.Ms - start < ms && frames < 3000)
            {
                if (each != null && !each((RealTime.Ms - start) / (float)Math.Max(ms, 1))) break;
                frames++;
                Wait(0);
            }
        }

        private void DropCam()
        {
            if (_cam == null) return;
            if (World.RenderingCamera == _cam) World.RenderingCamera = null;
            if (_cam.Exists()) _cam.Delete();
            _cam = null;
        }

        private void Play(Vehicle veh, Vector3? dir)
        {
            if (_playing || veh == null || !veh.Exists()) return;
            if (GTA.UI.Screen.IsFadedOut || GTA.UI.Screen.IsFadingOut || HudBridge.Paused || HudBridge.Busy) return;
            // Another script is already filming (a GTA6Hud shop or kill camera).
            // WeightedCam's dip is the player's own view, nodded: cut away from that.
            Camera filming = World.RenderingCamera;
            if (filming != null && filming.Exists() && !WeightedCam.Owns(filming)) return;
            _playing = true;
            IsPlaying = true;

            try
            {
                Ped ped = Game.Player.Character;
                // Where the player's own camera is, measured against the car, so
                // the view can be handed back exactly as it was.
                float heading = GameplayCamera.RelativeHeading;
                float pitch = GameplayCamera.RelativePitch;
                int mode = Function.Call<int>(Hash.GET_FOLLOW_VEHICLE_CAM_VIEW_MODE);

                // 1. The hit, slowed, from the seat.
                Game.TimeScale = _timeScale;
                _fx.Show(true);
                Hold(_impactHoldMs, t => { DrawOverlay(); return !ped.IsDead; });

                // 2. The cut. The spot is chosen now rather than at the moment of
                // the hit, so it is placed against where the car has ended up.
                if (!veh.Exists() || ped.IsDead) return;
                FindSpot(veh, dir, out Vector3 spot, out float fov);
                if (_blackFlashMs > 0)
                {
                    GTA.UI.Screen.FadeOut(0);
                    Wait(_blackFlashMs);
                }
                _cam = World.CreateCamera(spot, Vector3.Zero, fov);
                _cam.PointAt(veh, new Vector3(0f, 0f, 0.3f));
                if (_handShake > 0f) _cam.Shake(CameraShake.Hand, _handShake);
                World.RenderingCamera = _cam;
                HideHud(true);
                if (_blackFlashMs > 0) GTA.UI.Screen.FadeIn(0);

                Hold(_durationMs, t =>
                {
                    Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
                    DrawOverlay();
                    return veh.Exists() && !ped.IsDead;
                });

                // 3. Back to the view the player had.
                if (_fadeInMs > 0) GTA.UI.Screen.FadeOut(0);
                DropCam();
                _fx.Show(false);
                if (ped.IsDead) return;
                if (ped.IsInVehicle()) Function.Call(Hash.SET_FOLLOW_VEHICLE_CAM_VIEW_MODE, mode);
                GameplayCamera.RelativeHeading = heading;
                Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_PITCH, pitch, 1f);
                if (_fadeInMs > 0) GTA.UI.Screen.FadeIn(_fadeInMs);

                // Time runs back up rather than snapping, and the camera is put
                // back once more on the next frame: the first frame after a
                // scripted camera is the one the game is most likely to override.
                // (Hold's first pass is still this frame, so that's its second.)
                // In a vehicle the pitch also lands a couple of degrees under
                // what was set (2 on a Sultan, 3.5 on a Sanchez, measured): what
                // it reads by then says how far, and it's set that much higher.
                // More than a few degrees off is the game moving it, not that.
                int frame = 0;
                float off = 0f;
                Hold(Math.Max(_easeOutMs, 1), t =>
                {
                    Game.TimeScale = _timeScale + (1f - _timeScale) * Math.Min(t, 1f);
                    Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
                    frame++;
                    if (frame == 2)
                    {
                        off = pitch - GameplayCamera.RelativePitch;
                        if (Math.Abs(off) > 6f) off = 0f;
                        GameplayCamera.RelativeHeading = heading;
                        Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_PITCH, pitch + off, 1f);
                    }
                    else if (frame == 3)
                        Log.Write("CrashCam", "view handed back: pitch " + GameplayCamera.RelativePitch.ToString("0.0") + " (was " + pitch.ToString("0.0")
                            + (Math.Abs(off) >= 0.1f ? ", landed " + off.ToString("0.0") + " under it and was set again" : "") + ")");
                    return true;
                });
                Game.TimeScale = 1f;

                Hold(Math.Max(_hudReturnMs - _easeOutMs, 0), t => { Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME); return true; });
            }
            catch (Exception ex)
            {
                Log.Write("CrashCam", ex.ToString());
            }
            finally
            {
                // However it ended, the game is left as it was found.
                Restore();
                _readyAt = RealTime.Ms + _cooldownMs;
                _playing = false;
                IsPlaying = false;
            }
        }

        private void Restore()
        {
            if (_playing || _cam != null) Game.TimeScale = 1f;
            DropCam();
            _fx.Show(false);
            if (GTA.UI.Screen.IsFadedOut && _playing && !Game.Player.Character.IsDead) GTA.UI.Screen.FadeIn(0);
            HideHud(false);
        }
    }
}
