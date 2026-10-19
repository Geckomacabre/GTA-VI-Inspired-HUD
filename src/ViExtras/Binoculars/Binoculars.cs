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
    /// The FiveM version's binoculars (resources/vi_binoculars), for single player.
    ///
    /// Raise, look around, zoom smoothly, lower. The view is a scripted camera
    /// at the ped's eyes that the player turns with the mouse or right stick;
    /// zoom is the mouse wheel or d-pad up / down, eased so it glides. The
    /// further in, the slower the view turns and the more the hands show. The
    /// ped can walk with them up, forward being wherever they point. The lens
    /// mask and the EXIT / ZOOM / VISION prompts are vice_hud's.
    ///
    /// They come up from their tile on GTA6Hud's weapon wheel (BinocularsWheel),
    /// or on a key if [Binoculars] Key names one. GTA6Hud's own binoculars, the
    /// equipment item on its Items tab, are separate and stay as they are.
    ///
    /// D-pad down zooms out, and it's also GTA6Hud's character swap key, which it
    /// reads even with the control disabled: its swap wheel is held shut while
    /// they're up, and until d-pad down is let go after.
    /// </summary>
    public sealed class Binoculars : Script
    {
        private const int LookLR = 1, LookUD = 2;
        private const int PadZoomIn = 172, PadZoomOut = 173;      // d-pad up / down
        private const int WheelUp = 241, WheelDown = 242;
        private static readonly int[] MoveControls = { 30, 31, 32, 33, 34, 35 };
        private const int ExitControl = 194;                     // Backspace / B
        private const int VisionPrev = 174, VisionNext = 175;    // Left / Right
        private const int CharacterWheel = 19;                   // d-pad down / Left Alt
        private const string SwapHold = "binoculars";

        private readonly bool _enabled;
        private readonly Keys _key;
        private readonly bool _wheel;
        private readonly int _wheelRing;
        private readonly float _fovWide, _fovNarrow, _zoomSpeed, _wheelStep, _smoothing;
        private readonly float _lookPad, _lookMouse, _pitchMin, _pitchMax, _sway, _moveSpeed;
        private readonly bool _move, _allowInVehicle;
        private readonly string _animDict, _animName, _propModel, _scenario;
        private readonly int _bone;
        private readonly float _eyeForward, _eyeUp;
        private readonly List<string> _modes = new List<string> { "normal" };

        private bool _keyWasDown;
        private bool _active;
        private Camera _cam;
        private Prop _prop;
        private string _vision = "normal";
        private bool _minimapHeld;
        private bool _swapHeld;   // GTA6Hud's swap wheel held shut

        public static bool IsActive { get; private set; }

        public Binoculars()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Binoculars", "Enabled", true);
            _key = Config.Get("Binoculars", "Key", Keys.None);
            _wheel = Config.Get("Binoculars", "WheelSlot", true);
            _wheelRing = Config.Get("Binoculars", "WheelRing", 3);
            _fovWide = Config.Get("Binoculars", "FovWide", 42f);
            _fovNarrow = Config.Get("Binoculars", "FovNarrow", 4f);
            _zoomSpeed = Config.Get("Binoculars", "ZoomSpeed", 38f);
            _wheelStep = Config.Get("Binoculars", "WheelStep", 0.12f);
            _smoothing = Config.Get("Binoculars", "Smoothing", 9f);
            _lookPad = Config.Get("Binoculars", "LookSpeedPad", 85f);
            _lookMouse = Config.Get("Binoculars", "LookSpeedMouse", 7f);
            _pitchMin = Config.Get("Binoculars", "PitchMin", -55f);
            _pitchMax = Config.Get("Binoculars", "PitchMax", 55f);
            _sway = Config.Get("Binoculars", "Sway", 0.35f);
            _move = Config.Get("Binoculars", "Move", true);
            _moveSpeed = Config.Get("Binoculars", "MoveSpeed", 1f);
            _animDict = Config.Get("Binoculars", "AnimDict", "amb@world_human_binoculars@male@idle_a");
            _animName = Config.Get("Binoculars", "AnimName", "idle_c");
            _propModel = Config.Get("Binoculars", "Prop", "prop_binoc_01");
            _bone = Config.Get("Binoculars", "Bone", 28422);
            _scenario = Config.Get("Binoculars", "Scenario", "WORLD_HUMAN_BINOCULARS");
            _eyeForward = Config.Get("Binoculars", "EyeForward", 0.35f);
            _eyeUp = Config.Get("Binoculars", "EyeUp", 0.72f);
            _allowInVehicle = Config.Get("Binoculars", "AllowInVehicle", false);
            if (Config.Get("Binoculars", "NightVision", true)) _modes.Add("night");
            if (Config.Get("Binoculars", "ThermalVision", true)) _modes.Add("thermal");

            Tick += OnTick;
            Aborted += (s, e) => { if (_active) Unwind(); ReleaseSwap(); };
        }

        private void OnTick(object sender, EventArgs e)
        {
            // Every frame, ready or not, so the wheel shutting is seen when it happens.
            bool picked = _enabled && _wheel && BinocularsWheel.Picked(_wheelRing);
            if (_swapHeld && !_active)
            {
                // Lowered with d-pad down still held: the swap wheel would open on it.
                if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, CharacterWheel))
                {
                    Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, CharacterWheel, true);
                    HudBridge.HoldSwapWheelOff(SwapHold, true);
                }
                else ReleaseSwap();
            }
            if (!GameReady.Now) return;
            // Polled, not heard through KeyDown: SHVDN queues key events until a
            // tick ends, and the loop in Run() is one long tick, so a press made
            // to lower them would otherwise raise them again straight after.
            bool keyDown = _enabled && _key != Keys.None && Game.IsKeyPressed(_key);
            bool pressed = keyDown && !_keyWasDown;
            _keyWasDown = keyDown;
            if (!(pressed || picked) || _active) return;
            if (BlockedReason(Game.Player.Character) != null) return;
            try { Run(); }
            catch (Exception ex)
            {
                Log.Write("Binoculars", ex.ToString());
                Unwind();
            }
        }

        // Zoom is spread evenly by RATIO, not by degrees: half way along is the
        // geometric middle of the two fields of view.
        private float FovAt(float z) => _fovWide * (float)Math.Pow(_fovNarrow / _fovWide, z);

        private string BlockedReason(Ped ped)
        {
            if (ped == null || !ped.Exists() || ped.IsDead) return "dead";
            if (ped.IsInVehicle() && !_allowInVehicle) return "in a vehicle";
            if (ped.IsSwimming || ped.IsSwimmingUnderWater) return "in water";
            if (ped.IsFalling || ped.IsRagdoll || Function.Call<bool>(Hash.IS_PED_IN_PARACHUTE_FREE_FALL, ped)) return "falling";
            if (Function.Call<bool>(Hash.IS_PED_CUFFED, ped)) return "cuffed";
            if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen) return "paused";
            if (CrashCam.IsPlaying) return "busy";
            return null;
        }

        private bool CanMove => _move && !string.IsNullOrEmpty(_animDict);

        private bool PlayingAnim(Ped ped) => Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, ped, _animDict, _animName, 3);

        private void PlayAnim(Ped ped) =>
            // 51: looped, upper body only, and the player keeps control of the legs.
            Function.Call(Hash.TASK_PLAY_ANIM, ped, _animDict, _animName, 5f, 5f, -1, 51, 0f, false, false, false);

        // Up to the face. Walking: an upper-body animation and the prop, so the
        // legs are free. Standing: the game's own stance, which brings its own prop.
        private void Raise(Ped ped)
        {
            if (!CanMove)
            {
                if (!string.IsNullOrEmpty(_scenario)) Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, ped, _scenario, 0, true);
                return;
            }
            Function.Call(Hash.REQUEST_ANIM_DICT, _animDict);
            long deadline = RealTime.Ms + 1500;
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, _animDict) && RealTime.Ms < deadline) Wait(0);
            PlayAnim(ped);

            if (!string.IsNullOrEmpty(_propModel))
            {
                var model = new Model(_propModel);
                if (model.Request(1500))
                {
                    _prop = World.CreateProp(model, ped.Position, false, false);
                    if (_prop != null)
                    {
                        int bone = Function.Call<int>(Hash.GET_PED_BONE_INDEX, ped, _bone);
                        Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, _prop, ped, bone, 0f, 0f, 0f, 0f, 0f, 0f, true, true, false, true, 1, true);
                    }
                    model.MarkAsNoLongerNeeded();
                }
            }
        }

        private void Lower(Ped ped)
        {
            if (_prop != null)
            {
                if (_prop.Exists()) _prop.Delete();
                _prop = null;
            }
            if (ped == null || !ped.Exists() || ped.IsDead) return;
            if (CanMove) Function.Call(Hash.STOP_ANIM_TASK, ped, _animDict, _animName, 2f);
            else if (!string.IsNullOrEmpty(_scenario) && !ped.IsInVehicle()) ped.Task.ClearAll();
        }

        private void SetVision(string mode)
        {
            _vision = mode;
            Function.Call(Hash.SET_NIGHTVISION, mode == "night");
            Function.Call(Hash.SET_SEETHROUGH, mode == "thermal");
        }

        private void DrawOverlay()
        {
            Art.Entry lens = Art.ForAspect("bino_lens");
            if (lens != null) Canvas.Image(lens.Path, 0f, 0f, Canvas.W, Canvas.H, Color.White);

            bool pad = Glyphs.UsingPad;
            var rows = new List<Prompts.Row>
            {
                new Prompts.Row { Label = "exit", Glyph = Glyphs.ForControl(ExitControl) },
                // ZOOM names what zooms on the device in hand: the wheel, or d-pad up.
                new Prompts.Row { Label = "zoom", Glyph = pad ? Glyphs.ForControl(PadZoomIn) : Glyphs.ForToken("WHEEL_UP", "kbm") },
            };
            if (_modes.Count > 1) rows.Add(new Prompts.Row { Label = "vision", Glyph = Glyphs.ForControl(VisionNext) });
            Prompts.Draw(rows);
        }

        private void Run()
        {
            Ped ped = Game.Player.Character;
            _active = true;
            IsActive = true;
            float zoom = 0f, zoomTarget = 0f;
            float heading = ped.Heading;
            float pitch = Num.Clamp(GameplayCamera.RelativePitch, _pitchMin, _pitchMax);
            int modeAt = 0;

            if (!ped.IsInVehicle()) Raise(ped);

            // The plain scripted camera, which goes only where it is told.
            double turn0 = heading * Math.PI / 180.0;
            Vector3 at0 = ped.Position;
            _cam = World.CreateCamera(new Vector3(at0.X - (float)Math.Sin(turn0) * _eyeForward, at0.Y + (float)Math.Cos(turn0) * _eyeForward, at0.Z + _eyeUp),
                new Vector3(pitch, 0f, heading), FovAt(0f));
            if (_sway > 0f) _cam.Shake(CameraShake.Hand, _sway * 0.4f);
            World.RenderingCamera = _cam;
            _minimapHeld = true;
            HudBridge.HoldMinimapHidden(true);
            HudBridge.HoldGta6HudPanelsHidden(true);   // #stage.bino: the rest of the HUD waits
            _swapHeld = true;
            HudBridge.HoldSwapWheelOff(SwapHold, true);

            // Per z-unit, the field of view changes by Wide * ln(Wide / Narrow)
            // degrees at the wide end; ZoomSpeed is degrees per second there.
            float zoomRate = _zoomSpeed / (_fovWide * (float)Math.Log(_fovWide / _fovNarrow));
            bool keyWasDown = true;
            Wait(0);

            while (_active)
            {
                ped = Game.Player.Character;
                bool keyDown = _key != Keys.None && Game.IsKeyPressed(_key);
                bool keyPressed = keyDown && !keyWasDown;
                keyWasDown = keyDown;
                if (BlockedReason(ped) != null || keyPressed) break;

                bool onFoot = !ped.IsInVehicle();
                bool walking = onFoot && CanMove;

                Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
                Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, 249, true);
                HudBridge.HoldSwapWheelOff(SwapHold, true);   // again: its settings reload turns it back on
                if (walking)
                {
                    foreach (int c in MoveControls) Function.Call(Hash.ENABLE_CONTROL_ACTION, 0, c, true);
                    Function.Call(Hash.SET_PED_MAX_MOVE_BLEND_RATIO, ped, _moveSpeed);
                    // Something else may have taken the arms (a stumble, another script).
                    if (!PlayingAnim(ped)) PlayAnim(ped);
                }
                if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, ExitControl)) break;

                float dt = Game.LastFrameTime / Math.Max(Game.TimeScale, 0.01f);
                float fov = FovAt(zoom);
                float fine = fov / _fovWide;     // 1 at the wide end, small when zoomed in

                // look
                float lx = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, LookLR);
                float ly = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, LookUD);
                bool pad = Glyphs.UsingPad;
                if (!pad)
                {
                    heading -= lx * _lookMouse * fine;
                    pitch -= ly * _lookMouse * fine;
                }
                else
                {
                    heading -= lx * _lookPad * dt * fine;
                    pitch -= ly * _lookPad * dt * fine;
                }
                heading = ((heading % 360f) + 360f) % 360f;
                pitch = Num.Clamp(pitch, _pitchMin, _pitchMax);

                // zoom: stepped on the mouse wheel, held on d-pad up / down.
                if (!pad)
                {
                    if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, WheelUp)) zoomTarget += _wheelStep;
                    if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, WheelDown)) zoomTarget -= _wheelStep;
                }
                else
                {
                    float held = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, PadZoomIn) - Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, PadZoomOut);
                    zoomTarget += held * zoomRate * dt;
                }
                zoomTarget = Num.Clamp01(zoomTarget);
                zoom += (zoomTarget - zoom) * Math.Min(1f, _smoothing * dt);

                // vision: Right steps on, Left steps back, round the list
                if (_modes.Count > 1)
                {
                    int step = (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, VisionNext) ? 1 : 0)
                             - (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, VisionPrev) ? 1 : 0);
                    if (step != 0)
                    {
                        modeAt = ((modeAt + step) % _modes.Count + _modes.Count) % _modes.Count;
                        SetVision(_modes[modeAt]);
                    }
                }

                // Standing still, the ped turns with the view. Walking, "forward"
                // is kept meaning where the binoculars point: movement is steered
                // off the game's own camera, held on the view's heading.
                bool moving = walking && (Math.Abs(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, 30)) > 0.15f || Math.Abs(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, 31)) > 0.15f);
                if (walking)
                {
                    GameplayCamera.RelativeHeading = ((heading - ped.Heading + 540f) % 360f) - 180f;
                    Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_PITCH, pitch, 1f);
                }
                if (onFoot && !moving) ped.Heading = heading;

                // The view sits at the eyes, looking where it is turned.
                Vector3 at = ped.Position;
                double turn = heading * Math.PI / 180.0;
                _cam.Position = new Vector3(at.X - (float)Math.Sin(turn) * _eyeForward, at.Y + (float)Math.Cos(turn) * _eyeForward, at.Z + _eyeUp);
                _cam.Rotation = new Vector3(pitch, 0f, heading);
                _cam.FieldOfView = FovAt(zoom);
                if (_sway > 0f) Function.Call(Hash.SET_CAM_SHAKE_AMPLITUDE, _cam, _sway * (0.4f + 0.6f * zoom));

                // Its own body and the prop in its hands would fill the frame.
                Function.Call(Hash.SET_ENTITY_LOCALLY_INVISIBLE, ped);
                if (_prop != null && _prop.Exists()) Function.Call(Hash.SET_ENTITY_LOCALLY_INVISIBLE, _prop);
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
                DrawOverlay();

                Wait(0);
            }
            _keyWasDown = true;
            Unwind();
        }

        private void Unwind()
        {
            _active = false;
            IsActive = false;
            if (_vision != "normal") SetVision("normal");
            if (_cam != null)
            {
                if (World.RenderingCamera == _cam) World.RenderingCamera = null;
                if (_cam.Exists()) _cam.Delete();
                _cam = null;
            }
            if (_minimapHeld)
            {
                _minimapHeld = false;
                HudBridge.HoldMinimapHidden(false);
                HudBridge.HoldGta6HudPanelsHidden(false);
            }
            Lower(Game.Player.Character);
        }

        private void ReleaseSwap()
        {
            if (!_swapHeld) return;
            _swapHeld = false;
            HudBridge.HoldSwapWheelOff(SwapHold, false);
        }
    }
}
