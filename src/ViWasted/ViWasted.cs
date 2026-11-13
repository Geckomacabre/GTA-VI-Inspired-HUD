// ViWasted: the WASTED and BUSTED screens from the FiveM version's vi_wasted
// resource, which lifted them out of the fenix-police fork
// (github.com/Geckomacabre/fenix-police, client/client.lua). GPL-3.0, like the
// code it comes from; see LICENSE next to this file.
//
// The look and the numbers are vi_wasted's: the stock DeathFailOut effect, slow
// motion, a camera that starts close behind and pulls back and up while
// drifting round, a red wash over the picture, and the bare word in Pricedown.
//
// What happens around it is single player's. The game's own respawn_controller
// still runs the death or arrest: the hospital or police station, the bill, the
// fade back in. While this screen is up it is held off (its fade and its own
// WASTED text), and when this screen fades out the game carries on from there.
using System;
using System.Diagnostics;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViWasted
{
    public sealed class ViWasted : Script
    {
        private static readonly Stopwatch Clock = Stopwatch.StartNew();
        private static long Now => Clock.ElapsedMilliseconds;

        private readonly bool _wastedOn, _bustedOn, _skipInMissions, _bustedSkippable;
        private readonly int _wastedMs, _bustedMs;
        private readonly string _wastedWord, _bustedWord;
        private readonly int[] _wastedColour, _bustedColour, _wash;
        private readonly float _timeScale;
        private readonly float _wordWidth;   // the word's width on a 16:9 screen, as a share of it (0: vi_wasted's fixed scale)
        private readonly bool _outline;      // vi_wasted's drop shadow and dark edge round the word (GTA VI's has none)
        private readonly float _vignette;    // rply_vignette's strength (0: none)
        private readonly bool _lens, _dof, _gtaviCam, _fadeWord;
        private bool _tcSet, _extraTcSet;
        private string _scaledWord;          // the word _wordScale was worked out for
        private float _wordScale = BaseScale, _wordAspect;
        private readonly Keys _testWasted, _testBusted;

        private bool _playing;
        private bool _handled;            // this death / arrest already had its screen
        private int _test;                // 1 wasted, 2 busted, requested by a test key
        private int _bigMessage;          // MP_BIG_MESSAGE_FREEMODE, the game's WASTED text

        public static bool IsPlaying { get; private set; }

        public ViWasted()
        {
            var ini = ScriptSettings.Load(System.IO.Path.Combine(BaseDirectory, "ViWasted.ini"));
            _wastedOn = ini.GetValue("Wasted", "Enabled", true);
            _wastedWord = ini.GetValue("Wasted", "Word", "wasted");
            _wastedColour = Rgb(ini.GetValue("Wasted", "Colour", "154,152,153"), 154, 152, 153);
            _wastedMs = ini.GetValue("Wasted", "DurationMs", 4000);
            _bustedOn = ini.GetValue("Busted", "Enabled", true);
            _bustedWord = ini.GetValue("Busted", "Word", "busted");
            _bustedColour = Rgb(ini.GetValue("Busted", "Colour", "70,140,230"), 70, 140, 230);
            _bustedMs = ini.GetValue("Busted", "DurationMs", 6000);
            _bustedSkippable = ini.GetValue("Busted", "Skippable", true);
            _wash = Rgb(ini.GetValue("Screen", "Wash", "60,8,0,140"), 60, 8, 0, 140);
            _timeScale = ini.GetValue("Screen", "TimeScale", 0.15f);
            _wordWidth = ini.GetValue("Screen", "WordWidth", 0.344f);
            _outline = ini.GetValue("Screen", "Outline", false);
            _vignette = Math.Max(0f, Math.Min(1f, ini.GetValue("Screen", "Vignette", 0.85f)));
            _lens = ini.GetValue("Screen", "LensDistortion", true);
            _dof = ini.GetValue("Screen", "DepthOfField", true);
            _fadeWord = ini.GetValue("Wasted", "RedFadeOut", true);
            _gtaviCam = !string.Equals(ini.GetValue("Screen", "Camera", "gtavi"), "fivem", StringComparison.OrdinalIgnoreCase);
            _skipInMissions = ini.GetValue("Screen", "SkipInMissions", true);
            _testWasted = ini.GetValue("Screen", "TestWastedKey", Keys.None);
            _testBusted = ini.GetValue("Screen", "TestBustedKey", Keys.None);

            Tick += OnTick;
            KeyDown += (s, e) =>
            {
                if (_testWasted != Keys.None && e.KeyCode == _testWasted) _test = 1;
                if (_testBusted != Keys.None && e.KeyCode == _testBusted) _test = 2;
            };
            Aborted += (s, e) => { if (_playing) Cleanup(); };
        }

        private static int[] Rgb(string s, params int[] fallback)
        {
            try
            {
                string[] p = s.Split(',');
                var v = new int[fallback.Length];
                for (int i = 0; i < v.Length; i++) v[i] = i < p.Length ? int.Parse(p[i].Trim()) : fallback[i];
                return v;
            }
            catch { return fallback; }
        }

        private void Trace(string line)
        {
            try { System.IO.File.AppendAllText(System.IO.Path.Combine(BaseDirectory, "ViWasted.log"), DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + Environment.NewLine); } catch { }
        }

        private static bool RespawnControllerRunning =>
            Function.Call<int>(Hash.GET_NUMBER_OF_THREADS_RUNNING_THE_SCRIPT_WITH_THIS_HASH, Game.GenerateHash("respawn_controller")) > 0;

        // Not while a loading screen is up, there is no player ped or a character
        // switch is under way, nor for three seconds after: on Enhanced a ped native on
        // a ped the game is still building or swapping takes the game down.
        private long _okSince = -1;
        private bool Ready()
        {
            bool ok = !Function.Call<bool>(Hash.GET_IS_LOADING_SCREEN_ACTIVE)
                && Function.Call<bool>(Hash.DOES_ENTITY_EXIST, Function.Call<int>(Hash.PLAYER_PED_ID))
                && !Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS);
            long now = Environment.TickCount;
            if (!ok) { _okSince = -1; return false; }
            if (_okSince < 0) _okSince = now;
            return now - _okSince >= 3000;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!Ready()) return;
            // An exception out of a tick stops the script for the session.
            try { Watch(); }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText(System.IO.Path.Combine(BaseDirectory, "ViWasted.log"), DateTime.Now + " " + ex + Environment.NewLine); } catch { }
            }
        }

        private void Watch()
        {
            if (_playing) return;
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return;

            if (_test != 0)
            {
                int which = _test;
                _test = 0;
                if (which == 1) Cinematic(ped.Position, _wastedWord, _wastedColour, _wastedMs, false, test: true);
                else Cinematic(ped.Position, _bustedWord, _bustedColour, _bustedMs, _bustedSkippable, test: true);
                return;
            }

            bool dead = Function.Call<bool>(Hash.IS_PED_INJURED, ped);
            bool arrested = Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, false);
            if (!dead && !arrested)
            {
                _handled = false;
                return;
            }
            if (_handled) return;
            _handled = true;
            Trace((dead ? "dead" : "arrested") + ": model " + ped.Model.Hash + ", mission " + Game.IsMissionActive
                + ", respawn_controller " + RespawnControllerRunning + ", faded " + GTA.UI.Screen.IsFadedOut);
            if (_skipInMissions && Game.IsMissionActive) return;
            if (Function.Call<bool>(Hash.IS_CUTSCENE_PLAYING) || GTA.UI.Screen.IsFadedOut) return;

            if (dead && _wastedOn) Cinematic(ped.Position, _wastedWord, _wastedColour, _wastedMs, false, test: false);
            else if (arrested && !dead && _bustedOn) Cinematic(ped.Position, _bustedWord, _bustedColour, _bustedMs, _bustedSkippable, test: false);
        }

        // FONT_STYLE_PRICEDOWN, no background band, as vi_wasted draws it. vi_wasted's size was a fixed scale (1.35,
        // its top at 0.44); GTA VI's word is bigger. Measured on the user's GTA VI clip (2026-11-13, 1666x936): "wasted"
        // is 0.344 of the screen's width and 0.140 of its height, centred, in a flat light grey the red doesn't touch.
        // So the scale is worked out from the word's own width ([Screen] WordWidth on a 16:9 screen, the same size on
        // any other shape), and the word keeps vi_wasted's centre as it grows.
        // Checked against the clip on Enhanced at 2.39:1 (in game, 13 Nov): drawn to fill 0.344 of the width GTA
        // reports, the letters themselves came out 0.588 of the screen's height wide against GTA VI's 0.612 (GTA counts
        // some spacing round them: GlyphShare), and their middle at 0.460 down against GTA VI's 0.483 (CentreShift).
        private const int Font = 7;
        private const float BaseScale = 1.35f, BaseTop = 0.44f;
        private const float GlyphShare = 0.961f, CentreShift = 0.023f;
        private const ulong RenderedCharacterHeight = 0xDB88A37483346780; // GET_RENDERED_CHARACTER_HEIGHT
        private const ulong BeginWidth = 0x54CE8AC98E120CAB;              // BEGIN_TEXT_COMMAND_GET_SCREEN_WIDTH_OF_DISPLAY_TEXT
        private const ulong EndWidth = 0x85F061DA64ED2F67;                // END_TEXT_COMMAND_GET_SCREEN_WIDTH_OF_DISPLAY_TEXT

        // GTA VI's ending, measured frame by frame on the user's clip (15 fps), from the end of the 4 s screen:
        //   0-450 ms    the picture darkens by about 37%, then cuts to black; the word goes from its grey to (138,116,119)
        //   450-800     black; the word red, (125,84,87)
        //   800-930     the word dims, (109,68,68) then (77,49,51)
        //   930-1000    the word gone
        //   1000-1300   black; then the game fades back in on the respawn
        // Drawn with our own black so the word stays on top of it; the game's own fade only comes once all is black.
        private static readonly int[] RedWord = { 125, 84, 87 }, RedWordDim = { 77, 49, 51 }, TurningWord = { 138, 116, 119 };

        private void Ending(string word, int[] grey, bool test)
        {
            long t0 = Now;
            while (true)
            {
                long ms = Now - t0;
                if (ms >= 1300) break;
                HoldGameScreen(test);
                Game.TimeScale = _timeScale;
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
                if (_dof) Function.Call((Hash)UseHiDof);
                Function.Call(Hash.DRAW_RECT, 0.5f, 0.5f, 1f, 1f, _wash[0], _wash[1], _wash[2], _wash[3], false);
                int black = ms < 450 ? (int)(95f * ms / 450f) : 255;   // 37% darker, then the cut to black
                Function.Call(Hash.DRAW_RECT, 0.5f, 0.5f, 1f, 1f, 0, 0, 0, black, false);
                int[] c;
                int alpha = 255;
                if (ms < 450) c = Lerp(grey, TurningWord, ms / 450f);
                else if (ms < 800) c = RedWord;
                else if (ms < 930) c = Lerp(RedWord, RedWordDim, (ms - 800) / 130f);
                else { c = RedWordDim; alpha = ms < 1000 ? (int)(255f * (1000 - ms) / 70f) : 0; }
                if (alpha > 0) DrawBigWord(word, c, alpha);
                Wait(0);
            }
        }

        private static int[] Lerp(int[] a, int[] b, float t)
        {
            t = Math.Max(0f, Math.Min(1f, t));
            return new[] { (int)(a[0] + (b[0] - a[0]) * t), (int)(a[1] + (b[1] - a[1]) * t), (int)(a[2] + (b[2] - a[2]) * t) };
        }

        private void DrawBigWord(string word, int[] c) => DrawBigWord(word, c, 255);

        private void DrawBigWord(string word, int[] c, int alpha)
        {
            float scale = WordScale(word);
            float top = BaseTop;
            if (scale != BaseScale)
            {
                float hBase = Function.Call<float>((Hash)RenderedCharacterHeight, BaseScale, Font);
                float h = Function.Call<float>((Hash)RenderedCharacterHeight, scale, Font);
                if (hBase > 0f && h > 0f) top = BaseTop + (hBase - h) * 0.5f + CentreShift;
            }
            Function.Call(Hash.SET_TEXT_FONT, Font);
            Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, c[0], c[1], c[2], alpha);
            Function.Call(Hash.SET_TEXT_CENTRE, true);
            // GTA VI's word is flat: the clip's pixels go straight from the scene to the word, no dark edge or shadow
            if (_outline)
            {
                Function.Call(Hash.SET_TEXT_DROP_SHADOW);
                Function.Call(Hash.SET_TEXT_EDGE, 2, 0, 0, 0, 160);
            }
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, word);
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, top, 0);
        }

        // The scale that makes the word [Screen] WordWidth of a 16:9 screen wide: measured once at scale 1 (and again
        // if the screen's shape changes). Kept between 1 and 4; vi_wasted's 1.35 if it can't be measured.
        private float WordScale(string word)
        {
            if (_wordWidth <= 0f) return BaseScale;
            float aspect = GTA.UI.Screen.AspectRatio;
            if (word == _scaledWord && aspect == _wordAspect) return _wordScale;
            _scaledWord = word;
            _wordAspect = aspect;
            Function.Call(Hash.SET_TEXT_FONT, Font);
            Function.Call(Hash.SET_TEXT_SCALE, 1f, 1f);
            Function.Call((Hash)BeginWidth, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, word);
            float at1 = Function.Call<float>((Hash)EndWidth, true);
            float want = _wordWidth / GlyphShare * (16f / 9f) / (aspect > 0.5f ? aspect : 16f / 9f);
            _wordScale = at1 > 0.001f ? Math.Max(1f, Math.Min(4f, want / at1)) : BaseScale;
            Trace("\"" + word + "\" at scale 1 is " + at1.ToString("0.000") + " of the screen wide; drawn at " + _wordScale.ToString("0.00")
                  + " for " + _wordWidth.ToString("0.000") + " of a 16:9 screen (aspect " + aspect.ToString("0.00") + ")");
            return _wordScale;
        }

        private static void DrawSkipHint()
        {
            Function.Call(Hash.SET_TEXT_FONT, 4);
            Function.Call(Hash.SET_TEXT_SCALE, 0.42f, 0.42f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 180);
            Function.Call(Hash.SET_TEXT_CENTRE, true);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Press ~b~SPACE~w~ to skip");
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.88f, 0);
        }

        // Keeps the game's own death / arrest screen out of the way for a frame:
        // no restart yet, no fade, and its WASTED / BUSTED text cleared.
        private void HoldGameScreen(bool test)
        {
            if (test) return;
            Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, false);
            Function.Call(Hash.SET_FADE_OUT_AFTER_ARREST, false);
            Function.Call(Hash.PAUSE_DEATH_ARREST_RESTART, true);
            if (GTA.UI.Screen.IsFadingOut || GTA.UI.Screen.IsFadedOut) GTA.UI.Screen.FadeIn(0);
            if (_bigMessage == 0 || !Function.Call<bool>(Hash.HAS_SCALEFORM_MOVIE_LOADED, _bigMessage))
                _bigMessage = Function.Call<int>(Hash.REQUEST_SCALEFORM_MOVIE, "MP_BIG_MESSAGE_FREEMODE");
            else
            {
                Function.Call(Hash.BEGIN_SCALEFORM_MOVIE_METHOD, _bigMessage, "RESET_MOVIE");
                Function.Call(Hash.END_SCALEFORM_MOVIE_METHOD);
            }
        }

        private void Cinematic(Vector3 at, string word, int[] colour, int duration, bool skippable, bool test)
        {
            _playing = true;
            IsPlaying = true;
            int cam = 0;
            try
            {
                Ped ped = Game.Player.Character;
                HoldGameScreen(test);
                Function.Call(Hash.ANIMPOSTFX_PLAY, "DeathFailOut", 0, false);
                LookOn();
                Function.Call(Hash.PLAY_SOUND_FRONTEND, -1, "ScreenFlash", "MissionFailedSounds", true);
                Game.TimeScale = _timeScale;

                // vi_wasted's shot starts close behind and to one side, and ends further back and higher.
                // GTA VI's ([Screen] Camera = gtavi) is a low shot near the ground, a few metres from where the
                // body lands, drifting a little. In a vehicle the shot frames the vehicle instead, from far
                // enough out to clear its body, and follows it if it's still rolling.
                Vehicle veh = ped.IsInVehicle() ? ped.CurrentVehicle : null;
                Entity focus = veh != null && veh.Exists() ? (Entity)veh : ped;
                if (focus != ped) at = focus.Position;
                double rad = (focus.Heading + 160.0) * Math.PI / 180.0;
                float startDist = 2f, endDist = 6f, startZ = 0.8f, endZ = 3f, lookZ = 0.3f, drift = 15f;
                // WASTED only: BUSTED leaves him standing, and a shot of the ground would frame his legs
                if (focus == ped && _gtaviCam && word == _wastedWord)
                {
                    // from the ground under him: the body ends up there
                    float ground = GroundUnder(at);
                    if (ground > at.Z - 3f && ground < at.Z + 0.5f) at = new Vector3(at.X, at.Y, ground);
                    startDist = 2.8f; endDist = 3.5f; startZ = 0.55f; endZ = 0.8f; lookZ = 0.25f; drift = 8f;
                }
                if (focus != ped)
                {
                    var (rearBottomLeft, frontTopRight) = veh.Model.Dimensions;
                    Vector3 size = frontTopRight - rearBottomLeft;
                    float length = Math.Max(size.X, size.Y), height = size.Z;
                    startDist = Math.Max(startDist, length * 0.9f);
                    endDist = Math.Max(endDist, length * 1.9f);
                    startZ = Math.Max(startZ, height * 0.8f);
                    endZ = Math.Max(endZ, height * 1.8f);
                }

                cam = Function.Call<int>(Hash.CREATE_CAM, "DEFAULT_SCRIPTED_CAMERA", true);
                Vector3 startPos = at + new Vector3((float)Math.Sin(rad) * startDist, (float)Math.Cos(rad) * startDist, startZ);
                Function.Call(Hash.SET_CAM_COORD, cam, startPos.X, startPos.Y, startPos.Z);
                Function.Call(Hash.POINT_CAM_AT_COORD, cam, at.X, at.Y, at.Z + lookZ + 0.1003f);
                if (_dof)
                {
                    // the body sharp, what's nearer or further soft, as in GTA VI's shot
                    Function.Call((Hash)SetCamShallowDof, cam, true);
                    Function.Call((Hash)SetCamNearDof, cam, Math.Max(0.3f, startDist - 1.2f));
                    Function.Call((Hash)SetCamFarDof, cam, endDist + 1.5f);
                    Function.Call((Hash)SetCamDofStrength, cam, 1f);
                }
                Function.Call(Hash.SET_CAM_ACTIVE, cam, true);
                Function.Call(Hash.RENDER_SCRIPT_CAMS, true, true, 800, true, true, 0);

                long t0 = Now;
                bool skipped = false;
                while (Now - t0 < duration && !skipped)
                {
                    float progress = (Now - t0) / (float)duration;
                    float ease = 1f - (1f - progress) * (1f - progress);

                    HoldGameScreen(test);
                    Game.TimeScale = _timeScale;
                    Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);

                    if (skippable)
                    {
                        if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, 201) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, 22))
                            skipped = true;
                        DrawSkipHint();
                    }

                    if (focus != ped && focus.Exists()) at = focus.Position;
                    // another script's camera hand-back (a crash cam ending on the death) turns ours off
                    if (!Function.Call<bool>(Hash.IS_CAM_RENDERING, cam))
                    {
                        Function.Call(Hash.SET_CAM_ACTIVE, cam, true);
                        Function.Call(Hash.RENDER_SCRIPT_CAMS, true, false, 0, true, true, 0);
                    }
                    float dist = startDist + (endDist - startDist) * ease;
                    float z = startZ + (endZ - startZ) * ease;
                    double angle = rad + drift * ease * Math.PI / 180.0;
                    Vector3 pos = at + new Vector3((float)Math.Sin(angle) * dist, (float)Math.Cos(angle) * dist, z);
                    Function.Call(Hash.SET_CAM_COORD, cam, pos.X, pos.Y, pos.Z);
                    Function.Call(Hash.POINT_CAM_AT_COORD, cam, at.X, at.Y, at.Z + lookZ);
                    if (_dof) Function.Call((Hash)UseHiDof);

                    Function.Call(Hash.DRAW_RECT, 0.5f, 0.5f, 1f, 1f, _wash[0], _wash[1], _wash[2], _wash[3], false);
                    DrawBigWord(word, colour);
                    if (skippable) Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
                    Wait(0);
                }

                // GTA VI's ending: the picture goes black under the word, the word turns red on the black, then goes
                bool ending = _fadeWord && word == _wastedWord && !skipped;
                if (ending) Ending(word, colour, test);

                Game.TimeScale = 1f;
                Function.Call(Hash.ANIMPOSTFX_STOP, "DeathFailOut");

                // To black, then the game takes it from here (hospital, police
                // station, the bill) and fades back in once the player is up.
                Trace("screen done (" + word + "), fading out; respawn_controller " + RespawnControllerRunning);
                GTA.UI.Screen.FadeOut(ending ? 0 : word == _bustedWord ? 1500 : 1000);
                long fadeStart = Now;
                while (!GTA.UI.Screen.IsFadedOut && Now - fadeStart < 4000)
                {
                    if (!test)
                    {
                        Function.Call(Hash.PAUSE_DEATH_ARREST_RESTART, true);
                        Function.Call(Hash.BEGIN_SCALEFORM_MOVIE_METHOD, _bigMessage, "RESET_MOVIE");
                        Function.Call(Hash.END_SCALEFORM_MOVIE_METHOD);
                    }
                    Wait(0);
                }
                DropCam(cam);
                cam = 0;
                LookOff();

                Trace("faded out " + GTA.UI.Screen.IsFadedOut + "; handing over, respawn_controller " + RespawnControllerRunning);
                if (test || !RespawnControllerRunning)
                {
                    // Nothing else is going to put the picture back.
                    if (!test)
                    {
                        Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, true);
                        Function.Call(Hash.SET_FADE_OUT_AFTER_ARREST, true);
                        Function.Call(Hash.PAUSE_DEATH_ARREST_RESTART, false);
                        long wait = Now;
                        while (Game.Player.Character.IsDead && Now - wait < 15000) Wait(100);
                    }
                    GTA.UI.Screen.FadeIn(1000);
                }
                else
                {
                    // Safety net: if the game has not faded back in a while after
                    // the player is up again, do it here.
                    long wait = Now;
                    while (Now - wait < 20000)
                    {
                        Ped p = Game.Player.Character;
                        if (!GTA.UI.Screen.IsFadedOut) { Trace("the game faded back in after " + (Now - wait) + " ms"); break; }
                        if (p.Exists() && !p.IsDead && !RespawnControllerRunning && Now - wait > 3000)
                        {
                            Trace("player up, game had not faded in: fading in");
                            GTA.UI.Screen.FadeIn(1000);
                            break;
                        }
                        Wait(250);
                    }
                    if (Now - wait >= 20000)
                        Trace("still black after 20 s: dead " + Game.Player.Character.IsDead + ", respawn_controller " + RespawnControllerRunning + ", model " + Game.Player.Character.Model.Hash);
                }
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText(System.IO.Path.Combine(BaseDirectory, "ViWasted.log"), DateTime.Now + " " + ex + Environment.NewLine); } catch { }
                Cleanup();
                if (GTA.UI.Screen.IsFadedOut) GTA.UI.Screen.FadeIn(500);
            }
            finally
            {
                _playing = false;
                IsPlaying = false;
                if (cam != 0) DropCam(cam);
                LookOff();
                ReleaseBigMessage();
            }
        }

        // GTA VI's picture round the word, from the game's own timecycle modifiers: rply_vignette (the Rockstar
        // Editor's vignette: the edges dark, as in GTA VI's shot) and hud_def_lensdistortion_subtle (a slight wide-lens
        // bend with a touch of colour fringe) as the extra one. Both on from the start of the screen until it has
        // faded out, then cleared; only ours are cleared.
        private const ulong SetTc = 0x2C933ABF17A1DF41, SetTcStrength = 0x82E7FFCD5B2326B3, ClearTc = 0x0F07E7745A236711;
        private const ulong SetExtraTc = 0x5096FD9CCB49056D, ClearExtraTc = 0x92CCC17A7A2285DA;
        private const ulong SetCamShallowDof = 0x16A96863A17552BB, SetCamNearDof = 0x3FA4BF0A7AB7DE2C, SetCamFarDof = 0xEDD91296CD01AEE0;
        private const ulong SetCamDofStrength = 0x5EE29B4D7D5DF897, UseHiDof = 0xA13B0222F3D94A94, GroundZ = 0xC906A7DAB05C8D2B;

        private void LookOn()
        {
            if (_vignette > 0f)
            {
                Function.Call((Hash)SetTc, "rply_vignette");
                Function.Call((Hash)SetTcStrength, _vignette);
                _tcSet = true;
            }
            if (_lens)
            {
                Function.Call((Hash)SetExtraTc, "hud_def_lensdistortion_subtle");
                _extraTcSet = true;
            }
        }

        private void LookOff()
        {
            if (_tcSet) Function.Call((Hash)ClearTc);
            if (_extraTcSet) Function.Call((Hash)ClearExtraTc);
            _tcSet = _extraTcSet = false;
        }

        private static float GroundUnder(Vector3 at)
        {
            var z = new OutputArgument();
            return Function.Call<bool>((Hash)GroundZ, at.X, at.Y, at.Z + 1f, z, false, false) ? z.GetResult<float>() : float.MinValue;
        }

        // SET_SCALEFORM_MOVIE_AS_NO_LONGER_NEEDED takes a pointer to the handle.
        private void ReleaseBigMessage()
        {
            if (_bigMessage == 0) return;
            IntPtr p = System.Runtime.InteropServices.Marshal.AllocHGlobal(8);
            try
            {
                System.Runtime.InteropServices.Marshal.WriteInt64(p, _bigMessage);
                Function.Call(Hash.SET_SCALEFORM_MOVIE_AS_NO_LONGER_NEEDED, p);
            }
            catch { }
            finally { System.Runtime.InteropServices.Marshal.FreeHGlobal(p); }
            _bigMessage = 0;
        }

        private static void DropCam(int cam)
        {
            Function.Call(Hash.SET_CAM_ACTIVE, cam, false);
            Function.Call(Hash.RENDER_SCRIPT_CAMS, false, false, 0, true, true, 0);
            Function.Call(Hash.DESTROY_CAM, cam, false);
        }

        private void Cleanup()
        {
            Game.TimeScale = 1f;
            Function.Call(Hash.ANIMPOSTFX_STOP, "DeathFailOut");
            LookOff();
            Function.Call(Hash.RENDER_SCRIPT_CAMS, false, false, 0, true, true, 0);
            Function.Call(Hash.PAUSE_DEATH_ARREST_RESTART, false);
            Function.Call(Hash.SET_FADE_OUT_AFTER_DEATH, true);
            Function.Call(Hash.SET_FADE_OUT_AFTER_ARREST, true);
        }
    }
}
