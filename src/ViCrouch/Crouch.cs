using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Crouch and crawl from the FiveM server's emotes (um_emotes client/Crouch.lua,
    /// rpemotes' crouch and crawl by Mads), for single player. The animations it
    /// plays are Rockstar's own, from GTA V.
    ///
    /// Crouch (Left Ctrl): the crouched walk and strafe, slowed right down while
    /// aiming, no first person, no idle anims. Left Ctrl is also GTA's stealth
    /// key, so as on the server the first tap is stealth and a second tap inside a
    /// second crouches ([Crouch] OverrideStealthMode crouches on the first).
    ///
    /// Crawl (Right Ctrl): drop prone (a dive when running), W / S crawl forward
    /// and back, A / D turn, Space rolls onto the back and over again; the key
    /// again gets up (to a crouch when crouch is on). Getting into a car, falling,
    /// ragdolling or water ends either.
    /// </summary>
    public sealed class Crouch : Script
    {
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);

        // Turning, crawling or lying still: degrees a second whatever the frame rate (the
        // FiveM loop turned a degree a frame, and lying still a 10-degree step a second),
        // eased in and out, about the shoulders, where a crawler plants the elbows, so the
        // hips swing round rather than the head sweeping sideways.
        private const float CrawlTurnPerSec = 75f;
        private const float TurnEase = 8f;
        // The shoulders' distance ahead of the ped's origin in the lying poses (measured
        // from the clips; on the back they're behind it).
        private const float ShoulderAheadFront = 0.45f, ShoulderAheadBack = -0.5f;
        // AF_EXPAND_PED_CAPSULE_FROM_SKELETON: the ped's collision follows the lying body,
        // so it is pushed out of slopes, steps and walls instead of sinking into them.
        private const int BodyCapsuleFlag = 134217728;
        // Blend into and out of a crawl cycle, and into the resting pose.
        private const float CrawlBlend = 4f;

        private readonly bool _crouchOn, _crawlOn, _overrideStealth, _fpsMode;
        private readonly Keys _crouchKey, _crawlKey;

        private bool _crouched, _prone, _crawling, _inAction, _forceEndProne;
        private string _proneType = "onfront";
        // The crawl's clips: our own (Rockstar's move_crawl with the legs drawing up and
        // pushing, in the vihudanims pack), or Rockstar's when that pack isn't there or
        // [Crawl] OwnClips is off. Same clip names in both.
        private const string OwnCrawl = "vihud_crawl", GameCrawl = "move_crawl";
        private readonly bool _ownCrawl;
        private string _crawlDict = GameCrawl;
        private bool _crawlDictLogged;
        private long _lastKeyPress;
        // The crawl cycle looping while W / S is held ("fwd" / "bwd"), and whether a
        // release is waiting for the cycle to come round to its end before resting.
        private string _loopDir;
        private bool _stopPending;
        private float _lastPhase;
        // Turning: the eased rate, whether the cycle is playing only to turn on the spot,
        // and the shoulder point held still while it does.
        private float _turnRate;
        private bool _turnOnly, _pivotSet;
        private GTA.Math.Vector3 _pivot;
        private readonly bool _bodyCapsule;
        private bool _crouchWas, _crawlWas;

        public static bool IsCrouched { get; private set; }
        public static bool IsProne { get; private set; }

        public Crouch()
        {
            Paths.Init(BaseDirectory);
            _crouchOn = Config.Get("Crouch", "Enabled", true);
            _crouchKey = Config.Get("Crouch", "Key", Keys.LControlKey);
            _overrideStealth = Config.Get("Crouch", "OverrideStealthMode", false);
            _fpsMode = Config.Get("Crouch", "FpsMode", false);
            _crawlOn = Config.Get("Crawl", "Enabled", true);
            _crawlKey = Config.Get("Crawl", "Key", Keys.RControlKey);
            _ownCrawl = Config.Get("Crawl", "OwnClips", true);
            _bodyCapsule = Config.Get("Crawl", "BodyCapsule", true);
            Tick += OnTick;
            Aborted += (s, e) => DropStances();
        }

        // Windows' own key state: Script Hook V .NET sees one Ctrl, not left and right.
        private static bool IsKeyHeld(Keys k) => k != Keys.None && (GetAsyncKeyState((int)k) & 0x8000) != 0;

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now)
            {
                DropStances();
                return;
            }
            try
            {
                Ped ped = Game.Player.Character;
                // a switch or a model swap: the old ped keeps nothing, the new one starts standing
                if ((_crouched || _prone) && (_stancePed == null || _stancePed.Handle != ped.Handle)) DropStances();
                bool crouchKey = _crouchOn && IsKeyHeld(_crouchKey), crawlKey = _crawlOn && IsKeyHeld(_crawlKey);
                bool crouchPressed = crouchKey && !_crouchWas, crawlPressed = crawlKey && !_crawlWas;
                _crouchWas = crouchKey;
                _crawlWas = crawlKey;

                // the keys are read from Windows directly, so: only with the game in front, and not
                // while a menu, the phone or the binoculars have the keys (asked only on a press)
                if (crouchPressed || crawlPressed)
                {
                    bool keysFree = GameInFront() && !HudBridge.UiInTheWay && !BinocularsUp() && !ped.IsInVehicle();
                    if (crouchPressed && keysFree) CrouchKeyPressed(ped);
                    if (crawlPressed && keysFree) CrawlKeyPressed(ped);
                }

                if (_crouched) CrouchFrame(Game.Player.Character);
                if (_prone) CrawlFrame(Game.Player.Character);
                IsCrouched = _crouched;
                IsProne = _prone;
            }
            catch (Exception ex)
            {
                Log.Write("Crouch", ex.Message);
                _inAction = false;   // a move cut short by it would otherwise leave the keys ignored
            }
        }

        /// <summary>CanPlayerCrouchCrawl: on foot, not jumping, falling, hurt, fighting or ragdolled.</summary>
        private static bool CanCrouchCrawl(Ped p) =>
            Function.Call<bool>(Hash.IS_PED_ON_FOOT, p) && !Function.Call<bool>(Hash.IS_PED_JUMPING, p) && !Function.Call<bool>(Hash.IS_PED_FALLING, p)
            && !Function.Call<bool>(Hash.IS_PED_INJURED, p) && !Function.Call<bool>(Hash.IS_PED_IN_MELEE_COMBAT, p) && !Function.Call<bool>(Hash.IS_PED_RAGDOLL, p);

        private void LoadSet(string set)
        {
            long until = RealTime.Ms + 5000;
            while (!Function.Call<bool>(Hash.HAS_ANIM_SET_LOADED, set) && RealTime.Ms < until)
            {
                Function.Call(Hash.REQUEST_ANIM_SET, set);
                Wait(5);
            }
        }

        private bool LoadDict(string dict)
        {
            if (!Function.Call<bool>(Hash.DOES_ANIM_DICT_EXIST, dict)) return false;
            long until = RealTime.Ms + 2000;
            while (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict) && RealTime.Ms < until)
            {
                Function.Call(Hash.REQUEST_ANIM_DICT, dict);
                Wait(5);
            }
            return Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict);
        }

        // PlayAnimOnce
        private void Once(Ped p, string dict, string name, float blendIn = 2f, float blendOut = 2f, int duration = -1, float start = 0f)
        {
            LoadDict(dict);
            Function.Call(Hash.TASK_PLAY_ANIM, p, dict, name, blendIn, blendOut, duration, 0, start, false, false, false);
            Function.Call(Hash.REMOVE_ANIM_DICT, dict);
        }

        // ChangeHeadingSmooth: turns by `amount` degrees over `ms`, eased in and out
        // and stepped every frame (the FiveM one stepped a degree at a time).
        private void TurnSmooth(Ped p, float amount, int ms)
        {
            if (amount == 0f || ms <= 0) return;
            float start = p.Heading;
            long t0 = RealTime.Ms;
            while (true)
            {
                float t = Math.Min(1f, (RealTime.Ms - t0) / (float)ms);
                p.Heading = start + amount * t * t * (3f - 2f * t);
                if (t >= 1f || !p.Exists()) break;
                Wait(0);
            }
        }

        // =============================================================================
        // Crouching
        // =============================================================================

        private Ped _stancePed;

        private void DropStances()
        {
            Ped p = _stancePed != null && _stancePed.Exists() ? _stancePed : Game.Player.Character;
            if (_crouched) ResetCrouch(p);
            if (_prone && p != null && p.Exists())
            {
                Function.Call(Hash.CLEAR_PED_TASKS, p);
                Function.Call(Hash.SET_PED_CONFIG_FLAG, p, 48, false);   // CPED_CONFIG_FLAG_BlockWeaponSwitching
            }
            _crouched = _prone = _crawling = _inAction = _forceEndProne = false;
            _loopDir = null;
            _stopPending = false;
            ResetTurn();
            _stancePed = null;
            IsCrouched = false;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int pid);
        private static readonly int OwnPid = System.Diagnostics.Process.GetCurrentProcess().Id;

        private static bool GameInFront()
        {
            GetWindowThreadProcessId(GetForegroundWindow(), out int pid);
            return pid == OwnPid && !Game.IsPaused;
        }

        // ViExtras' binoculars, by reflection (a separate script file)
        private static System.Reflection.PropertyInfo _binoculars;
        private static bool _binocularsLooked;

        private static bool BinocularsUp()
        {
            if (!_binocularsLooked)
            {
                _binocularsLooked = true;
                foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = asm.GetType("ViExtras.Binoculars", false);
                    if (t == null || t.Assembly == typeof(Crouch).Assembly) continue;
                    _binoculars = t.GetProperty("IsActive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                    break;
                }
            }
            try { return _binoculars != null && _binoculars.GetValue(null) is bool b && b; }
            catch { return false; }
        }

        private void ResetCrouch(Ped p = null)
        {
            if (p == null) p = Game.Player.Character;
            Function.Call(Hash.RESET_PED_STRAFE_CLIPSET, p);
            Function.Call(Hash.RESET_PED_WEAPON_MOVEMENT_CLIPSET, p);
            Function.Call(Hash.SET_PED_MAX_MOVE_BLEND_RATIO, p, 1f);
            Function.Call(Hash.SET_PED_CAN_PLAY_AMBIENT_ANIMS, p, true);
            Function.Call(Hash.RESET_PED_MOVEMENT_CLIPSET, p, 0.5f);
            Function.Call(Hash.REMOVE_ANIM_SET, "move_ped_crouched");
        }

        // CrouchLoop, one frame
        private void CrouchFrame(Ped p)
        {
            if (!CanCrouchCrawl(p))
            {
                _crouched = false;
                ResetCrouch();
                return;
            }
            // walk slowly when aiming
            if (Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player) || Function.Call<bool>(Hash.IS_AIM_CAM_ACTIVE))
                Function.Call(Hash.SET_PED_MAX_MOVE_BLEND_RATIO, p, 0.15f);
            // no standing up for idle anims (needs doing every frame)
            Function.Call(Hash.SET_PED_CAN_PLAY_AMBIENT_ANIMS, p, false);
            // no INPUT_DUCK, no action mode
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 36, true);
            if (Function.Call<bool>(Hash.IS_PED_USING_ACTION_MODE, p)) Function.Call(Hash.SET_PED_USING_ACTION_MODE, p, false, -1, "DEFAULT_ACTION");
            Function.Call(Hash.DISABLE_ON_FOOT_FIRST_PERSON_VIEW_THIS_UPDATE);
            if (_fpsMode) Function.Call(Hash.DISABLE_PLAYER_FIRING, Game.Player, true);
        }

        private void StartCrouch(Ped p)
        {
            _crouched = true;
            _stancePed = p;
            LoadSet("move_ped_crouched");
            if (Function.Call<bool>(Hash.GET_PED_STEALTH_MOVEMENT, p))
            {
                Function.Call(Hash.SET_PED_STEALTH_MOVEMENT, p, false, "DEFAULT_ACTION");
                Wait(100);
            }
            if (Function.Call<int>(Hash.GET_FOLLOW_PED_CAM_VIEW_MODE) == 4) Function.Call(Hash.SET_FOLLOW_PED_CAM_VIEW_MODE, 0);
            Function.Call(Hash.SET_PED_MOVEMENT_CLIPSET, p, "move_ped_crouched", 0.6013f);
            Function.Call(Hash.SET_PED_STRAFE_CLIPSET, p, "move_ped_crouched_strafing");
        }

        private void CrouchKeyPressed(Ped p)
        {
            if (_inAction || HudBridge.Busy) return;
            if (_crouched)
            {
                _crouched = false;
                ResetCrouch();
                return;
            }
            if (!CanCrouchCrawl(p) || !Function.Call<bool>(Hash.IS_PED_HUMAN, p)) return;

            // Left Ctrl is GTA's duck (stealth) key too: the first tap is stealth,
            // a second inside a second (while in stealth) is the crouch.
            if (_crouchKey == Keys.LControlKey)
            {
                if (_overrideStealth) Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 36, true);
                else if (!_prone)
                {
                    long now = RealTime.Ms;
                    if (Function.Call<bool>(Hash.GET_PED_STEALTH_MOVEMENT, p) && now - _lastKeyPress < 1000)
                    {
                        Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 36, true);
                        _lastKeyPress = 0;
                    }
                    else
                    {
                        _lastKeyPress = now;
                        return;
                    }
                }
            }

            StartCrouch(p);
            // from prone, get up onto the knees
            if (_prone)
            {
                _inAction = true;
                _prone = false;
                _forceEndProne = true;   // this anim is the way out; the crawl's own exit is not played
                EndProne();
                Once(p, "get_up@directional@transition@prone_to_knees@crawl", "front", 2f, 2f, 780);
                Wait(780);
                _inAction = false;
            }
        }

        // =============================================================================
        // Crawling
        // =============================================================================

        private void IdleCrawl(Ped p, float? heading = null, float blendIn = 2f)
        {
            GTA.Math.Vector3 c = p.Position;
            Function.Call(Hash.TASK_PLAY_ANIM_ADVANCED, p, _crawlDict, _proneType + "_fwd", c.X, c.Y, c.Z, 0f, 0f, heading ?? p.Heading, blendIn, 2f, -1, 2 | CapsuleFlag, 1f, false, false);
        }

        // A crawl cycle on loop while the key is held: the clip's own length, so one
        // cycle runs into the next with no hold or cut between them.
        private void StartCrawlLoop(Ped p, string dir)
        {
            Function.Call(Hash.TASK_PLAY_ANIM, p, _crawlDict, _proneType + "_" + dir, CrawlBlend, -CrawlBlend, -1, 1 | CapsuleFlag, 0f, false, false, false);
            _loopDir = dir;
            _stopPending = false;
            _lastPhase = 0f;
            _crawling = true;
        }

        // Rest: the end pose of the forward cycle, as the FiveM resting pose is.
        private void StopCrawlLoop(Ped p)
        {
            IdleCrawl(p, null, CrawlBlend);
            _loopDir = null;
            _stopPending = false;
            _crawling = false;
        }

        private void CrawlFlip(Ped p)
        {
            _inAction = true;
            float heading = p.Heading;
            if (_proneType == "onfront")
            {
                _proneType = "onback";
                Once(p, "get_up@directional_sweep@combat@pistol@front", "front_to_prone", 2f);
                TurnSmooth(p, -18f, 3600);
            }
            else
            {
                _proneType = "onfront";
                Once(p, "move_crawlprone2crawlfront", "back", 2f, 2f, -1);
                TurnSmooth(p, 12f, 1700);
            }
            IdleCrawl(p, heading + 180f);
            Wait(400);
            _inAction = false;
        }

        // CrawlLoop, one frame
        private void CrawlFrame(Ped p)
        {
            if (!CanCrouchCrawl(p) || Function.Call<bool>(Hash.IS_ENTITY_IN_WATER, p))
            {
                Function.Call(Hash.CLEAR_PED_TASKS, p);
                _forceEndProne = true;
                _prone = false;
                _loopDir = null;
                ResetTurn();
                EndProne();
                return;
            }
            if (_inAction) return;
            bool forward = Game.IsControlPressed(GTA.Control.MoveUpOnly), back = Game.IsControlPressed(GTA.Control.MoveDownOnly);
            // A / D or the stick, + turning left (INPUT_MOVE_LR is -1 left .. +1 right)
            float steer = -Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, 30);
            if (Math.Abs(steer) < 0.25f) steer = 0f;
            bool wasTurnOnly = _turnOnly;
            _turnOnly = !forward && !back && steer != 0f;
            // turning on the spot plays the forward cycle, its shoulders held where they are
            string want = forward ? "fwd" : back ? "bwd" : _turnOnly ? "fwd" : null;
            if (_loopDir != null && want == null && wasTurnOnly) StopCrawlLoop(p);   // done turning: rest now
            if (_loopDir != null)
            {
                string clip = _proneType + "_" + _loopDir;
                float phase = Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, p, _crawlDict, clip);
                bool playing = Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, p, _crawlDict, clip, 3);
                if (want == _loopDir) _stopPending = false;
                else if (want != null) StartCrawlLoop(p, want);           // turned round: straight into the other cycle
                else _stopPending = true;
                // let go: rest once the cycle comes round to its end (the resting pose), not mid-stride
                if (_stopPending && (!playing || phase >= 0.94f || phase < _lastPhase)) StopCrawlLoop(p);
                else if (_loopDir != null && !playing) StartCrawlLoop(p, _loopDir);   // something else stopped it
                _lastPhase = phase;
            }
            else if (want != null) StartCrawlLoop(p, want);
            // eased turn rate; crawling backwards turns the other way, as the FiveM one did
            float dt = Math.Min(Game.LastFrameTime, 0.1f);
            float target = steer * CrawlTurnPerSec * (back && !forward ? -1f : 1f);
            _turnRate += (target - _turnRate) * Math.Min(1f, dt * TurnEase);
            if (_turnOnly || Math.Abs(_turnRate) > 0.5f) TurnAboutShoulders(p, _turnRate * dt, _turnOnly);
            else
            {
                _turnRate = 0f;
                _pivotSet = false;
            }
            if (!_crawling && Game.IsControlPressed(GTA.Control.Jump)) CrawlFlip(p);
        }

        private int CapsuleFlag => _bodyCapsule ? BodyCapsuleFlag : 0;

        private void ResetTurn()
        {
            _turnRate = 0f;
            _turnOnly = _pivotSet = false;
        }

        // Turns the ped by `deg` about its shoulders. On the spot (hold) the shoulders stay
        // where they were when the turn began, which also stops the cycle crawling forward.
        private void TurnAboutShoulders(Ped p, float deg, bool hold)
        {
            float ahead = _proneType == "onfront" ? ShoulderAheadFront : ShoulderAheadBack;
            GTA.Math.Vector3 pos = p.Position;
            float h = p.Heading;
            if (!hold) _pivotSet = false;
            else if (!_pivotSet)
            {
                _pivot = pos + Ahead(h, ahead);
                _pivotSet = true;
            }
            GTA.Math.Vector3 pivot = _pivotSet ? _pivot : pos + Ahead(h, ahead);
            float nh = h + deg;
            GTA.Math.Vector3 np = pivot - Ahead(nh, ahead);
            // keepTasks, keepIK, no warp: a nudge, not a teleport (dropping the tasks dropped the
            // crawl animation itself, and the ped stood in its bind pose)
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, p, np.X, np.Y, pos.Z, true, true, false);
            p.Heading = nh;
        }

        // `d` metres ahead along a heading (GTA's: 0 is north, counter-clockwise)
        private static GTA.Math.Vector3 Ahead(float heading, float d)
        {
            double r = heading * Math.PI / 180.0;
            return new GTA.Math.Vector3((float)(-Math.Sin(r) * d), (float)(Math.Cos(r) * d), 0f);
        }

        /// <summary>The end of CrawlLoop: the get-up anims (unless forced), then back to normal.</summary>
        private void EndProne()
        {
            Ped p = Game.Player.Character;
            if (!_forceEndProne)
            {
                _inAction = true;
                if (_proneType == "onfront")
                {
                    Once(p, "get_up@directional@transition@prone_to_knees@crawl", "front", 2f, 2f, 780);
                    if (!_crouched)
                    {
                        Wait(780);
                        Once(p, "get_up@directional@movement@from_knees@standard", "getup_l_0", 2f, 2f, 1300);
                    }
                }
                else
                {
                    Once(p, "get_up@directional@transition@prone_to_seated@crawl", "back", 16f, 2f, 950);
                    if (!_crouched)
                    {
                        Wait(950);
                        Once(p, "get_up@directional@movement@from_seated@standard", "get_up_l_0", 2f, 2f, 1300);
                    }
                }
            }
            _crawling = false;
            _loopDir = null;
            _stopPending = false;
            ResetTurn();
            _inAction = false;
            _forceEndProne = false;
            _proneType = "onfront";
            Function.Call(Hash.SET_PED_CONFIG_FLAG, p, 48, false);   // CPED_CONFIG_FLAG_BlockWeaponSwitching
            Function.Call(Hash.REMOVE_ANIM_DICT, _crawlDict);
            Function.Call(Hash.REMOVE_ANIM_DICT, "move_crawlprone2crawlfront");
        }

        private void CrawlKeyPressed(Ped p)
        {
            if (_inAction || HudBridge.Busy) return;
            if (_prone)
            {
                _prone = false;
                EndProne();
                return;
            }
            // crouching ends first
            bool wasCrouched = false;
            if (_crouched)
            {
                _crouched = false;
                ResetCrouch();
                wasCrouched = true;
            }
            if (!CanCrouchCrawl(p) || Function.Call<bool>(Hash.IS_ENTITY_IN_WATER, p) || !Function.Call<bool>(Hash.IS_PED_HUMAN, p)) return;
            _inAction = true;
            _prone = true;
            _stancePed = p;
            Function.Call(Hash.SET_PED_CONFIG_FLAG, p, 48, true);   // CPED_CONFIG_FLAG_BlockWeaponSwitching
            if (Function.Call<bool>(Hash.GET_PED_STEALTH_MOVEMENT, p))
            {
                Function.Call(Hash.SET_PED_STEALTH_MOVEMENT, p, false, "DEFAULT_ACTION");
                Wait(100);
            }
            _crawlDict = _ownCrawl && LoadDict(OwnCrawl) ? OwnCrawl : GameCrawl;
            if (_crawlDict == GameCrawl) LoadDict(GameCrawl);
            if (!_crawlDictLogged)
            {
                _crawlDictLogged = true;
                Log.Write("Crawl", "crawling with " + _crawlDict + (_ownCrawl && _crawlDict == GameCrawl ? " (vihud_crawl isn't installed)" : ""));
            }
            LoadDict("move_crawlprone2crawlfront");
            // a dive when running, otherwise lie down
            if (Function.Call<bool>(Hash.IS_PED_RUNNING, p) || Function.Call<bool>(Hash.IS_PED_SPRINTING, p))
            {
                Once(p, "explosions", "react_blown_forwards", 2f, 3f);
                Wait(1100);
            }
            else if (wasCrouched)
            {
                Once(p, "amb@world_human_sunbathe@male@front@enter", "enter", 2f, 2f, -1, 0.3f);
                Wait(1500);
            }
            else
            {
                Once(p, "amb@world_human_sunbathe@male@front@enter", "enter");
                Wait(3000);
            }
            if (CanCrouchCrawl(p) && !Function.Call<bool>(Hash.IS_ENTITY_IN_WATER, p)) IdleCrawl(p, null, 3f);
            _inAction = false;
            // CrawlLoop starts after Wait(400)
            Wait(400);
        }
    }
}
