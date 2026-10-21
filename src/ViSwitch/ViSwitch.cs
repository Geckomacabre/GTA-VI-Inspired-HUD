// ViSwitch: GTA-style sky switches for FBICat's GTA6Hud character wheel (Michael, Franklin, Trevor, Lucia).
//
// GTA6Hud's wheel swaps Lucia in and out instantly with SET_PLAYER_MODEL. ViSwitch takes the wheel over
// (it keeps GTA6Hud's own drawing, portraits and blur, and stands GTA6Hud's wheel down by clearing
// Ref.SwapEnabled) and routes every switch:
//   * between Michael, Franklin and Trevor: the game's own switch, as GTA6Hud did (select-character input),
//     so the vanilla sky camera and the vanilla "what they were up to" scenes play;
//   * to Lucia: the camera rises into the sky, Lucia is swapped in up there through GTA6Hud's own
//     SwapToLucia (weapons, outfits and cash carry over as before), placed at a random scene drawn from all
//     three characters' haunts, and the camera comes back down onto her mid-scene;
//   * from Lucia back to her host character: the same sky switch, landing the host at one of their own spots;
//   * from Lucia to one of the other two: back to the host behind a fade, and the game's own switch started
//     before the screen comes back, so its sky camera rises from where Lucia stood.
// A scene's idle animation ends as soon as the player moves.
//
// GTA6Hud 2.1 and later switch to and from Lucia themselves: a Lucia ped stays in the world and the game's sky
// switch goes between her and the host. ViSwitch then leaves both Lucia directions to GTA6Hud, still runs the
// wheel (2.1 draws it with the game's player_switch movie) and still does the select-key hold for Michael,
// Franklin and Trevor, including the one GTA6Hud asks for after switching from Lucia back to her host.
//
// Talks to GTA6Hud only by reflection, so it builds without it and stands aside if it is missing,
// if [Swap] Enabled = False, or if [Lucia] Enabled = False. Natives are called by raw hash so the same
// build runs on ScriptHookVDotNet 3.7 (Legacy) and ScriptHookVDotNet Enhanced.
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViSwitch
{
    public class ViSwitch : Script
    {
        // ---- natives (raw hashes) ----
        const ulong SWITCH_OUT = 0xAAB3200ED59016BC;          // SWITCH_TO_MULTI_FIRSTPART(ped, flags, type)
        const ulong SWITCH_IN = 0xD8295AF639FD9CB8;           // SWITCH_TO_MULTI_SECONDPART(ped)
        const ulong SWITCH_IN_PROGRESS = 0xD9D2CFFF49FAB35F;
        const ulong SWITCH_STATE = 0x470555300D10B2A5;
        const ulong STOP_SWITCH = 0x95C0A5BBDC189AA1;
        const ulong LOAD_SCENE_SPHERE = 0xACCFB4ACF53551B0;
        const ulong LOAD_SCENE_LOADED = 0x01B8247A7A8B9AD1;
        const ulong LOAD_SCENE_STOP = 0xC197616D221FF4A4;
        const ulong SCENARIO_IN_PLACE = 0x142A02425FF02BD9;
        const ulong CLEAR_TASKS = 0xE1EF3C1216AFF2CD;
        const ulong SET_COORDS = 0x06843DA7060A026B;
        const ulong SET_HEADING = 0x8E2530AA8ADA980E;
        const ulong SAFE_COORD = 0xB61C8E878A4199CA;
        const ulong GROUND_Z = 0xC906A7DAB05C8D2B;
        const ulong FADE_OUT = 0x891B5B39AC6302AF;
        const ulong FADE_IN = 0xD4E8E24955024033;
        const ulong FADED_OUT = 0xB16FCE9DDC7BA182;
        const ulong STREAMED = 0x7350823473013C02;            // HAVE_ALL_STREAMING_REQUESTS_COMPLETED(ped)
        const ulong SET_CONTROL_NEXT = 0xE8A25867FBA3B05E;    // SET_CONTROL_VALUE_NEXT_FRAME
        const ulong DISABLE_CONTROL = 0xFE99B66D079CF6BC;
        const ulong DIS_PRESSED = 0xE2587F8CBBD87B1D;
        const ulong DIS_NORMAL = 0x11E65974A982637C;
        const ulong PLAYER_PED_ID = 0xD80958FC74E988A6;
        const ulong PLAYER_ID = 0x4F8644AF03D0E0D6;
        const ulong ENTITY_MODEL = 0x9F47B058362C84B5;
        const ulong ENTITY_EXISTS = 0x7239B21A38F536BA;
        const ulong ENTITY_DEAD = 0x5F9532F3B5CC2551;
        const ulong HASH_KEY = 0xD24D37CC275948CC;
        const ulong SET_PLAYER_MODEL = 0x00A1CADD00108836;
        const ulong MODEL_REQUEST = 0x963D27A58DF860AC;
        const ulong MODEL_LOADED = 0x98A4EB5D89A0C952;
        const ulong GAME_TIMER = 0x9CD27B0045628463;
        const ulong FREEZE = 0x428CA6DBD1094446;              // FREEZE_ENTITY_POSITION
        const ulong REQUEST_COLLISION = 0x07503F7948F491A7;   // REQUEST_COLLISION_AT_COORD
        const ulong COLLISION_LOADED = 0xE9676F61BC0B3321;    // HAS_COLLISION_LOADED_AROUND_ENTITY
        const ulong IS_PRESSED = 0xF3A21BCD95725A4A;          // IS_CONTROL_PRESSED
        const ulong IS_ENABLED = 0x1CEA6BFDF248E5D9;          // IS_CONTROL_ENABLED
        const ulong ENABLE_CONTROL = 0x351220255D64C155;      // ENABLE_CONTROL_ACTION
        const ulong BUTTON_STRING = 0x0499D7B09FC9B407;       // GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING
        const ulong SAFE_TO_SWITCH = 0x71E7B2E657449AAD;      // IS_SAFE_TO_START_PLAYER_SWITCH
        const ulong WANTED_LEVEL = 0xE28E54788CE8F12D;
        const ulong MISSION_FLAG = 0xA33CDCCDA663159E;
        const ulong CUTSCENE_PLAYING = 0xD3C2E180A40F031E;
        const ulong PAUSE_MENU = 0xB0034A223497FFCB;
        const ulong SYSTEM_UI = 0x5D511E3867C87139;
        const ulong HUD_ACTIVE = 0xBC4C9EA5391ECC0D;          // IS_HUD_COMPONENT_ACTIVE
        const ulong THREADS = 0x2C83A9DA6BFFC4F9;             // GET_NUMBER_OF_THREADS_RUNNING_THE_SCRIPT_WITH_THIS_HASH
        const ulong RAGDOLL = 0x47E4E977581C5B55;
        const ulong ARRESTED = 0x388A47C51ABDAC8E;
        const ulong CONTROL_ON = 0x49C32D60007AFA47;          // IS_PLAYER_CONTROL_ON

        // The game's selector only switches on a character-select key that is held until its wheel is open
        // and has drawn the pick, then released (selector.c: func_134 held, func_133 released). The wheel
        // opens 300 ms into the hold and only once its scaleform has loaded, and the pick is recorded on the
        // first frame it draws, so GTA6Hud's 4-frame tap never switched and a 450 ms hold had no margin.
        // ViSwitch holds the key only until the wheel has opened and lets go the next frame, so the game's own
        // wheel, its slow motion and its colour filter show for a frame or two (the user does not want to see
        // it; at 13:21 a fixed 900 ms on the game clock left it up for 4 s of slow motion). SelectHoldMs is the
        // longest it waits for the wheel, in real milliseconds.
        const int SelectHoldMs = 1200; // real ms; normally the key goes up the frame after the wheel opens
        const ulong POSTFX_RUNNING = 0x36AD3E690DA5ACEB;  // ANIMPOSTFX_IS_RUNNING
        const ulong POSTFX_STOP = 0x068E835A1D0DC0E3;     // ANIMPOSTFX_STOP
        const ulong SF_REQUEST = 0x11FE353CF9733E6F;      // REQUEST_SCALEFORM_MOVIE
        const ulong SF_LOADED = 0x85F01B8D5B90570E;       // HAS_SCALEFORM_MOVIE_LOADED
        const ulong SF_BEGIN = 0xF6E48914C7A8694E;        // BEGIN_SCALEFORM_MOVIE_METHOD
        const ulong SF_PARAM_INT = 0xC3D0841A0CC546A6;    // SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT
        const ulong SF_END = 0xC6796A8FFA375E53;          // END_SCALEFORM_MOVIE_METHOD
        // The wheel the game's selector draws (selector.c: REQUEST_SCALEFORM_MOVIE("PLAYER_SWITCH"); it shows it
        // with SET_SWITCH_VISIBLE(1) as it opens and hides it with SET_SWITCH_VISIBLE(0)). A request for a movie that
        // is already loaded gets the same movie, so ViSwitch can keep it hidden during its own key press. The
        // handle is kept for the session: releasing it could unload the selector's copy.
        int _switchMovie;
        int _hideWheelUntil = -1;
        // The game's wheel starts one of these the frame it opens (selector.c), the same frame it records the pick.
        static readonly string[] WheelOpenEffects = { "SwitchHUDMichaelIn", "SwitchHUDFranklinIn", "SwitchHUDTrevorIn", "SwitchHUDIn" };
        static readonly System.Diagnostics.Stopwatch RealClock = System.Diagnostics.Stopwatch.StartNew();
        static int RealNow() { return (int)RealClock.ElapsedMilliseconds; }

        static readonly uint[] StoryModels = { 225514697u, 2602752943u, 2608926626u }; // player_zero/one/two
        static readonly int[] SelectInputs = { 166, 167, 168 };                       // INPUT_SELECT_CHARACTER_*

        // The game only counts a select key that really comes from the keyboard: SET_CONTROL_VALUE_NEXT_FRAME's
        // value shows up in IS_DISABLED_CONTROL_PRESSED alone (11:02 test: 0 of 43 frames pressed, the control
        // enabled throughout), and the selector reads IS_CONTROL_PRESSED. So while GTA has focus, and the key
        // is still the default F5 / F6 / F7, ViSwitch presses it for real (scan codes) for the hold.
        static readonly string[] DefaultBindings = { "b_174", "b_175", "b_176" };
        static readonly ushort[] FKeyScans = { 0x3F, 0x40, 0x41 };                  // F5, F6, F7
        ushort _realKeyDown;

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion { [FieldOffset(0)] public MOUSEINPUT mi; [FieldOffset(0)] public KEYBDINPUT ki; }
        [StructLayout(LayoutKind.Sequential)]
        struct INPUT { public uint type; public InputUnion u; }
        [DllImport("user32.dll")] static extern uint SendInput(uint count, INPUT[] inputs, int size);
        [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window, out uint pid);

        static bool GameHasFocus()
        {
            uint pid;
            GetWindowThreadProcessId(GetForegroundWindow(), out pid);
            return pid == (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        static void SendScan(ushort scan, bool up)
        {
            var inp = new INPUT[1];
            inp[0].type = 1; // INPUT_KEYBOARD
            inp[0].u.ki.wScan = scan;
            inp[0].u.ki.dwFlags = 0x0008u | (up ? 0x0002u : 0u); // KEYEVENTF_SCANCODE (| KEYEVENTF_KEYUP)
            SendInput(1, inp, Marshal.SizeOf(typeof(INPUT)));
        }

        void ReleaseRealKey()
        {
            if (_realKeyDown == 0) return;
            try { SendScan(_realKeyDown, true); } catch { }
            _realKeyDown = 0;
        }
        static readonly string[] StoryNames = { "Michael", "Franklin", "Trevor" };
        // In a vehicle Q / LB, which step the wheel, are also the radio's (radio wheel 85, next / previous station
        // 81 / 82), so those are held off too while the wheel is open.
        static readonly int[] WheelBlocked = { 37, 44, 51, 205, 206, 174, 175, 189, 190, 85, 81, 82 };

        struct Scene
        {
            public string Name; public Vector3 At; public float Heading; public string Scenario; public int Owner;
            public Scene(int owner, string name, float x, float y, float z, float h, string scenario)
            { Owner = owner; Name = name; At = new Vector3(x, y, z); Heading = h; Scenario = scenario; }
        }

        // Where each story character hangs out (safehouse driveways and their usual haunts).
        // Lucia draws from all of them. Coordinates are approximate on purpose: the landing spot is
        // corrected to the nearest safe ground once the area has streamed in.
        static readonly Scene[] Scenes =
        {
            new Scene(0, "Michael's driveway, Rockford Hills", -814.4f, 183.3f, 72.2f, 115f, "WORLD_HUMAN_SMOKING"),
            new Scene(0, "Portola Drive shops", -711.6f, -150.5f, 37.4f, 120f, "WORLD_HUMAN_WINDOW_SHOP_BROWSE"),
            new Scene(0, "Del Perro Pier", -1813.0f, -1194.0f, 13.0f, 145f, "WORLD_HUMAN_TOURIST_MAP"),
            new Scene(0, "Galileo Observatory", -412.0f, 1170.0f, 325.8f, 165f, "WORLD_HUMAN_TOURIST_MOBILE"),
            new Scene(1, "Forum Drive, Strawberry", -27.8f, -1430.7f, 30.6f, 0f, "WORLD_HUMAN_HANG_OUT_STREET"),
            new Scene(1, "Vinewood Hills house", 24.2f, 539.2f, 176.0f, 330f, "WORLD_HUMAN_STAND_MOBILE"),
            new Scene(1, "Grove Street", 106.0f, -1940.0f, 20.8f, 45f, "WORLD_HUMAN_DRINKING"),
            new Scene(1, "Vespucci Beach", -1258.0f, -1515.0f, 4.4f, 35f, "WORLD_HUMAN_SMOKING"),
            new Scene(2, "Sandy Shores trailer", 1964.2f, 3815.9f, 32.3f, 120f, "WORLD_HUMAN_DRINKING"),
            new Scene(2, "Vespucci Canals", -1146.0f, -1542.4f, 4.4f, 35f, "WORLD_HUMAN_SMOKING_POT"),
            new Scene(2, "Vanilla Unicorn", 142.6f, -1290.5f, 29.3f, 300f, "WORLD_HUMAN_PARTYING"),
            new Scene(2, "Sandy Shores airfield", 1716.0f, 3287.0f, 41.2f, 200f, "WORLD_HUMAN_STAND_IMPATIENT"),
            new Scene(2, "Paleto Bay beach", -276.0f, 6634.0f, 7.4f, 45f, "WORLD_HUMAN_SUNBATHE_BACK"),
        };

        readonly Random _rng = new Random();
        readonly string _log;
        bool _enabled = true;
        int _lucia;

        // GTA6Hud bridge
        object _hud;
        int _hudLookAt;
        FieldInfo _fSwapBlend, _fSwapSel, _fWheelOpen, _fSettingsOpen, _fLuciaHost, _fRefSwapEnabled, _fRefLuciaEnabled, _fRefLuciaModel;
        FieldInfo _fRefHeat, _fRefBrain;
        // GTA6Hud 2.1: the wheel's movie state and the select pulse it asks for after a switch back from Lucia
        FieldInfo _fSwapOpen, _fSwapMovieShown, _fSwapHideAt, _fSwapPulse, _fSwapPulseInput;
        bool _hudSwitches; // GTA6Hud switches Lucia itself (2.1 and later)

        // GTA6Hud's Heat and PoliceBrain read the player's police memory (Game.Player.IgnoredByPolice) every frame.
        // On Enhanced that read hit junk during a swap in the sky (10:43 crash: access violation in a SHVDNE memory
        // getter, GTA6Hud's watchdog "STUCK 2.5 s in Heat"). Both stop before that read while switched off, so
        // they are held off during every switch and swap, and for 3 s after, then put back as they were.
        // PoliceMemory reads it too, but switching it off wipes what the police remember, so it is left alone.
        bool _policeHeld, _heatWas, _brainWas;
        int _policeHoldUntil;
        MethodInfo _mDraw, _mWantBlur, _mInteract, _mSwapToLucia, _mSwapFromLucia;

        // wheel state
        bool _open, _busy, _sceneActive;
        int _sel, _current, _pulseInput, _lastScene = -1;
        int _pulseUntil = -1, _pulseCheckUntil = -1, _pulseTicks, _pulseSeen, _pulseSeenDisabled, _pulseWasDisabled, _lastModel;
        int _pulseReleaseUntil = -1, _pulseWheelAt = -1;
        bool _pulseWheelBefore;
        string _pulseWho;
        float _blend;

        // test hook: a line "michael", "franklin", "trevor" or "lucia" in scripts\ViSwitch.cmd switches to them
        readonly string _cmd;
        int _cmdLookAt;

        public ViSwitch()
        {
            _log = Path.Combine(BaseDirectory, "ViSwitch.log");
            _cmd = Path.Combine(BaseDirectory, "ViSwitch.cmd");
            try { File.WriteAllText(_log, ""); } catch { }
            _enabled = ReadIniBool(Path.Combine(BaseDirectory, "GTA6Hud.ini"), "Swap", "Enabled", true);
            Log("loaded; GTA6Hud [Swap] Enabled=" + _enabled);
            Interval = 0;
            Tick += OnTick;
            Aborted += (s, e) => ReleaseRealKey(); // never leave a pressed F key behind
            DisarmIgnoredByPoliceRead();
        }

        // Enhanced only. ScriptHookVDotNet Enhanced reads Player.IgnoredByPolice from memory: the player ped's
        // CPlayerInfo pointer at NativeMemory.PedPlayerInfoOffset (GetCPlayerInfoAddress uses it with no null or
        // sanity check), then a bit in its CWanted. GTA6Hud's Heat reads it every frame, and on 2026-10-19 the game
        // crashed twice in Heat right after a Lucia swap (10:43:10, 11:05:22: access violations through the CLR,
        // pointers made of float data). With PedPlayerInfoOffset at 0 every CPlayerInfo read returns its default
        // without touching memory. Of all the installed scripts (GTA6Hud, ViExtras, ViCustoms, ViCrouch, ViWasted,
        // ViMinimap) only GTA6Hud's three IgnoredByPolice reads go through it, so those read false: the "police
        // ignore you" state another script may set is lost to GTA6Hud's police systems, and nothing else changes.
        // (CWantedIgnorePlayerFlagOffset itself is a get-only static, which the JIT folds into a constant.)
        void DisarmIgnoredByPoliceRead()
        {
            try
            {
                if (!System.Diagnostics.Process.GetCurrentProcess().ProcessName.StartsWith("GTA5_Enhanced", StringComparison.OrdinalIgnoreCase)) return;
                Type nm = null;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    if ((nm = asm.GetType("SHVDN.NativeMemory", false)) != null) break;
                PropertyInfo info = nm == null ? null : nm.GetProperty("PedPlayerInfoOffset", BindingFlags.Static | BindingFlags.Public);
                if (info == null || !info.CanWrite) { Log("SHVDN.NativeMemory.PedPlayerInfoOffset not found; IgnoredByPolice left as is"); return; }
                int was = (int)info.GetValue(null, null);
                info.SetValue(null, 0, null);
                Log("IgnoredByPolice memory read switched off (PedPlayerInfoOffset 0x" + was.ToString("X") + " -> "
                    + (int)info.GetValue(null, null) + ")");
            }
            catch (Exception ex) { Log("could not switch off the IgnoredByPolice read: " + ex.Message); }
        }

        // ---------------- helpers ----------------
        void Log(string s)
        {
            try { File.AppendAllText(_log, DateTime.Now.ToString("HH:mm:ss.fff") + " " + s + Environment.NewLine); } catch { }
        }

        static T Call<T>(ulong h, params InputArgument[] a) { return Function.Call<T>((Hash)h, a); }
        static void Call(ulong h, params InputArgument[] a) { Function.Call((Hash)h, a); }
        static int Me() { return Call<int>(PLAYER_PED_ID); }
        static int Now() { return Call<int>(GAME_TIMER); }

        static bool ReadIniBool(string path, string section, string key, bool fallback)
        {
            try
            {
                string cur = null;
                foreach (string raw in File.ReadAllLines(path))
                {
                    string line = raw.Trim();
                    if (line.StartsWith("[") && line.EndsWith("]")) { cur = line.Substring(1, line.Length - 2); continue; }
                    if (cur == null || !cur.Equals(section, StringComparison.OrdinalIgnoreCase)) continue;
                    int eq = line.IndexOf('=');
                    if (eq > 0 && line.Substring(0, eq).Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                        return !line.Substring(eq + 1).Trim().Equals("False", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { }
            return fallback;
        }

        void WaitUntil(Func<bool> done, int timeoutMs)
        {
            int end = Now() + timeoutMs;
            while (!done() && Now() < end) Frame();
        }

        // One frame of waiting inside a switch: keeps GTA6Hud's police systems held off meanwhile.
        void Frame()
        {
            UpdatePoliceHold();
            Wait(0);
        }

        void UpdatePoliceHold()
        {
            if (_hud == null || _fRefHeat == null || _fRefBrain == null) return;
            int now = Now();
            try
            {
                if (_busy || Call<bool>(SWITCH_IN_PROGRESS))
                {
                    if (!_policeHeld)
                    {
                        _heatWas = (bool)_fRefHeat.GetValue(null);
                        _brainWas = (bool)_fRefBrain.GetValue(null);
                        _policeHeld = true;
                        Log("  GTA6Hud Heat and PoliceBrain held off for the switch");
                    }
                    _policeHoldUntil = now + 3000;
                }
                if (!_policeHeld) return;
                if (now < _policeHoldUntil)
                {
                    _fRefHeat.SetValue(null, false);
                    _fRefBrain.SetValue(null, false);
                }
                else
                {
                    _fRefHeat.SetValue(null, _heatWas);
                    _fRefBrain.SetValue(null, _brainWas);
                    _policeHeld = false;
                    Log("  GTA6Hud Heat and PoliceBrain back on");
                }
            }
            catch (Exception ex) { Log("police hold error: " + ex.Message); _fRefHeat = null; }
        }

        // ---------------- GTA6Hud bridge ----------------
        bool FindHud()
        {
            if (_hud != null) return true;
            if (Now() < _hudLookAt) return false;
            _hudLookAt = Now() + 2000;
            try
            {
                Type domainType = null, refType = null;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (domainType == null) domainType = asm.GetType("SHVDN.ScriptDomain", false);
                    if (refType == null) refType = asm.GetType("GTA6Hud.Ref", false);
                }
                if (domainType == null || refType == null) return false;
                object domain = domainType.GetProperty("CurrentDomain", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                Array scripts = (Array)domainType.GetProperty("RunningScripts").GetValue(domain, null);
                foreach (object s in scripts)
                {
                    object inst = s.GetType().GetProperty("ScriptInstance").GetValue(s, null);
                    if (inst != null && inst.GetType().FullName == "GTA6Hud.GTA6Hud") { _hud = inst; break; }
                }
                if (_hud == null) return false;
                const BindingFlags I = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                const BindingFlags S = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
                Type t = _hud.GetType();
                _fSwapBlend = t.GetField("_swapBlend", I);
                _fSwapSel = t.GetField("_swapSel", I);
                _fWheelOpen = t.GetField("_wheelOpen", I);
                _fSettingsOpen = t.GetField("_settingsOpen", I);
                _fLuciaHost = t.GetField("_luciaHost", I);
                _mDraw = t.GetMethod("DrawCharSwap", I);
                _mWantBlur = t.GetMethod("WantBlur", I);
                _mInteract = t.GetMethod("InteractionPressed", I);
                _mSwapToLucia = t.GetMethod("SwapToLucia", I);
                _mSwapFromLucia = t.GetMethod("SwapFromLucia", I);
                _fRefSwapEnabled = refType.GetField("SwapEnabled", S);
                _fRefLuciaEnabled = refType.GetField("LuciaEnabled", S);
                _fRefLuciaModel = refType.GetField("LuciaModelName", S);
                _fRefHeat = refType.GetField("HeatEnabled", S);
                _fRefBrain = refType.GetField("PoliceBrainEnabled", S);
                _fSwapOpen = t.GetField("_swapOpen", I);
                _fSwapMovieShown = t.GetField("_swapMovieShown", I);
                _fSwapHideAt = t.GetField("_swapHideAt", I);
                _fSwapPulse = t.GetField("_swapPulse", I);
                _fSwapPulseInput = t.GetField("_swapPulseInput", I);
                _hudSwitches = _fSwapOpen != null && _fSwapMovieShown != null && _fSwapHideAt != null
                               && _fSwapPulse != null && _fSwapPulseInput != null && t.GetMethod("StartLuciaSwitch", I) != null;
                bool ok = _fSwapBlend != null && _fSwapSel != null && _mDraw != null && _mSwapToLucia != null
                          && _mSwapFromLucia != null && _fRefSwapEnabled != null && _fRefLuciaEnabled != null;
                Log("found GTA6Hud; bridge " + (ok ? "complete" : "INCOMPLETE - standing aside")
                    + (ok && _hudSwitches ? "; GTA6Hud switches Lucia itself" : ""));
                if (!ok) { _hud = null; _enabled = false; return false; }
                string model = _fRefLuciaModel != null ? (string)_fRefLuciaModel.GetValue(null) : "lucia_s";
                _lucia = Call<int>(HASH_KEY, string.IsNullOrEmpty(model) ? "lucia_s" : model);
                return true;
            }
            catch (Exception ex) { Log("bridge error: " + ex.Message); return false; }
        }

        bool HudBool(FieldInfo f) { try { return f != null && (bool)f.GetValue(_hud); } catch { return false; } }

        // ViExtras' nitrous boosts on Left Alt (control 19) in a car with a bottle, the same key as the wheel,
        // and its binoculars zoom out on D-pad down (control 19 too). While ViExtras.Nitrous.HoldsCharacterKey or
        // ViExtras.Binoculars.HoldsCharacterKey is true the key is theirs, so the wheel stays shut.
        // Without ViExtras (or an older build without the flags) this is always false.
        MemberInfo _nitrousHolds, _binocularsHolds;
        int _nitrousLookAt;

        bool NitrousHoldsKey()
        {
            if (_nitrousHolds == null)
            {
                if (Now() < _nitrousLookAt) return false;
                _nitrousLookAt = Now() + 5000;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.GetName().Name != "ViExtras") continue;
                    Type t = asm.GetType("ViExtras.Nitrous", false);
                    if (t == null) continue;
                    const BindingFlags S = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                    _nitrousHolds = (MemberInfo)t.GetProperty("HoldsCharacterKey", S) ?? t.GetField("HoldsCharacterKey", S);
                    Type b = asm.GetType("ViExtras.Binoculars", false);
                    _binocularsHolds = b != null ? (MemberInfo)b.GetProperty("HoldsCharacterKey", S) ?? b.GetField("HoldsCharacterKey", S) : null;
                    if (_binocularsHolds != null) Log("found ViExtras binoculars; the wheel stays shut while they hold D-pad down");
                    if (_nitrousHolds != null) { Log("found ViExtras nitrous; the wheel stays shut while it holds Left Alt"); break; }
                }
                if (_nitrousHolds == null) return false;
            }
            return Holds(_nitrousHolds) || Holds(_binocularsHolds);
        }

        static bool Holds(MemberInfo m)
        {
            if (m == null) return false;
            try
            {
                PropertyInfo p = m as PropertyInfo;
                object v = p != null ? p.GetValue(null, null) : ((FieldInfo)m).GetValue(null);
                return v is bool && (bool)v;
            }
            catch { return false; }
        }
        void WantBlur(bool on) { try { if (_mWantBlur != null) _mWantBlur.Invoke(_hud, new object[] { "swap", on }); } catch { } }

        bool Pressed(int control, Keys key)
        {
            try { if (_mInteract != null) return (bool)_mInteract.Invoke(_hud, new object[] { (GTA.Control)control, key }); }
            catch { }
            return false;
        }

        int CurrentIndex(int ped)
        {
            int model = Call<int>(ENTITY_MODEL, ped);
            if (model == _lucia) return 3;
            return Array.IndexOf(StoryModels, (uint)model);
        }

        // ---------------- tick ----------------
        void OnTick(object sender, EventArgs e)
        {
            if (!_enabled || !FindHud()) return;
            bool luciaOn;
            try { luciaOn = (bool)_fRefLuciaEnabled.GetValue(null); } catch { return; }
            if (!luciaOn)
            {
                // Without Lucia, GTA6Hud's own 3-character wheel is exactly the game's switch: leave it alone.
                try { _fRefSwapEnabled.SetValue(null, true); } catch { }
                if (_policeHeld)
                {
                    try { _fRefHeat.SetValue(null, _heatWas); _fRefBrain.SetValue(null, _brainWas); } catch { }
                    _policeHeld = false;
                }
                return;
            }
            try { _fRefSwapEnabled.SetValue(null, false); } catch { } // stand GTA6Hud's wheel down every tick

            UpdatePoliceHold();
            if (_hudSwitches) TakeHudSwitchPulse();
            UpdateGameSwitchPulse();

            int ped = Me();
            WatchOutsideSwaps(ped);
            if (_sceneActive) EndSceneOnInput(ped);
            CheckTestCommand(ped);

            bool blocked = _busy || !Call<bool>(ENTITY_EXISTS, ped) || Call<bool>(ENTITY_DEAD, ped, false)
                           || HudBool(_fWheelOpen) || HudBool(_fSettingsOpen) || Call<bool>(SWITCH_IN_PROGRESS) || Game.IsPaused
                           || NitrousHoldsKey();
            // The game's own wheel must not open on Left Alt, but INPUT_CHARACTER_WHEEL is only disabled while Alt
            // is down: disabled every frame (as GTA6Hud's wheel did) it also disables a physically held F5-F8
            // (11:28 test: the real F5 read disabled on all 19 frames), so the game never switched to Michael or
            // Trevor, not even from the player's own F keys. The game's wheel needs 300 ms of hold to open, so the
            // first frame of an Alt press getting through does nothing.
            bool altDown = Call<bool>(DIS_PRESSED, 0, 19);
            if (altDown || _open) Call(DISABLE_CONTROL, 0, 19, true);
            bool held = !blocked && altDown;
            if (held && !_open)
            {
                _open = true;
                _current = CurrentIndex(ped);
                _sel = _current >= 0 ? _current : 0;
                WantBlur(true);
            }
            else if (!held && _open)
            {
                _open = false;
                WantBlur(false);
                if (!blocked && _sel != _current) SwitchTo(_sel);
            }
            if (_open && blocked) { _open = false; WantBlur(false); }
            if (_open)
            {
                foreach (int c in WheelBlocked) Call(DISABLE_CONTROL, 0, c, true);
                bool next = Pressed(206, Keys.E) | Pressed(190, Keys.Right);
                bool prev = Pressed(205, Keys.Q) | Pressed(189, Keys.Left);
                if (next) _sel = (_sel + 1) % 4;
                if (prev) _sel = (_sel + 3) % 4;
            }
            _blend = Math.Max(0f, Math.Min(1f, _blend + (_open ? 1f : -1f) * Game.LastFrameTime / 0.18f));
            if (_hudSwitches)
            {
                // 2.1 shows and hides its movie from _swapOpen and keeps drawing it for the close (_swapHideAt)
                try
                {
                    _fSwapOpen.SetValue(_hud, _open);
                    _fSwapSel.SetValue(_hud, _sel);
                    _fSwapBlend.SetValue(_hud, _blend);
                    if (_open || HudBool(_fSwapMovieShown) || Now() < (int)_fSwapHideAt.GetValue(_hud)) _mDraw.Invoke(_hud, null);
                }
                catch { }
            }
            else if (_blend > 0.01f)
            {
                try
                {
                    _fSwapBlend.SetValue(_hud, _blend);
                    _fSwapSel.SetValue(_hud, _sel);
                    _mDraw.Invoke(_hud, null);
                }
                catch { }
            }
        }

        // Holds the character-select key for SelectHoldMs, then lets go (the game switches on release), and
        // logs whether the game's switch actually started, with what may have stopped it.
        void StartGameSwitch(int target)
        {
            _pulseInput = SelectInputs[target];
            _pulseWho = StoryNames[target];
            _pulseUntil = RealNow() + SelectHoldMs;
            _pulseCheckUntil = _pulseUntil + 4000;
            _pulseTicks = _pulseSeen = _pulseSeenDisabled = _pulseWasDisabled = 0;
            _pulseWheelAt = -1;
            _pulseWheelBefore = GameWheelOpen(); // an effect left running can't signal this hold
            string key = "?";
            try { key = Call<string>(BUTTON_STRING, 0, _pulseInput, true); } catch { }
            ReleaseRealKey();
            string how;
            if (key != DefaultBindings[target]) how = "simulated only: the key is rebound";
            else if (!GameHasFocus()) how = "simulated only: GTA does not have focus";
            else
            {
                try { SendScan(FKeyScans[target], false); _realKeyDown = FKeyScans[target]; how = "real F" + (5 + target) + " key"; }
                catch (Exception ex) { how = "simulated only: SendInput failed (" + ex.Message + ")"; }
            }
            Log("  holding input " + _pulseInput + " (" + key + ", " + how + ") until the game's wheel opens, at most " + SelectHoldMs + " ms; blockers: " + Blockers());
        }

        void UpdateGameSwitchPulse()
        {
            if (_pulseUntil < 0 && _pulseCheckUntil < 0) return;
            int now = RealNow();
            if (_pulseUntil >= 0)
            {
                // On Enhanced the select key reads as pressed only through IS_DISABLED_CONTROL_PRESSED (10:43 test:
                // 0 of 31 frames held, 32 disabled), so something disables it each frame and the game's selector,
                // which reads the enabled state, never sees it. Re-enable it every frame of the hold.
                if (_pulseTicks > 0 && !Call<bool>(IS_ENABLED, 0, _pulseInput)) _pulseWasDisabled++;
                Call(ENABLE_CONTROL, 0, _pulseInput, true);
                if (_pulseTicks > 0)
                {
                    // the value set last tick is what the game read this frame
                    if (Call<bool>(IS_PRESSED, 0, _pulseInput)) _pulseSeen++;
                    if (Call<bool>(DIS_PRESSED, 0, _pulseInput)) _pulseSeenDisabled++;
                }
                if (_pulseWheelBefore && _pulseTicks > 0 && !GameWheelOpen()) _pulseWheelBefore = false; // leftover stopped
                if (_pulseWheelAt < 0 && _pulseTicks > 0 && !_pulseWheelBefore && GameWheelOpen()) _pulseWheelAt = now;
                HideGameWheel(); // after the check above: it stops the colour filter that check looks for
                bool wheelDone = _pulseWheelAt >= 0 && _pulseTicks > 0 && now > _pulseWheelAt; // a frame after it opened
                if (now < _pulseUntil && !wheelDone) { Call(SET_CONTROL_NEXT, 0, _pulseInput, 1f); _pulseTicks++; return; }
                _pulseUntil = -1;
                ReleaseRealKey();
                _pulseReleaseUntil = now + 300; // keep it enabled so the game also sees the release
                _hideWheelUntil = now + 500;    // the wheel draws until the game has seen the release
                Log("  released " + (wheelDone ? "as the game's wheel opened" : "after " + SelectHoldMs + " ms without seeing the game's wheel open")
                    + "; the game read the key as held on " + _pulseSeen + " of " + (_pulseTicks - 1)
                    + " frames (" + _pulseSeenDisabled + " counting disabled reads; it was disabled on "
                    + _pulseWasDisabled + " before ViSwitch re-enabled it)");
            }
            if (_pulseUntil < 0 && now < _hideWheelUntil) HideGameWheel();
            if (_pulseReleaseUntil >= 0)
            {
                if (now < _pulseReleaseUntil) Call(ENABLE_CONTROL, 0, _pulseInput, true);
                else _pulseReleaseUntil = -1;
            }
            if (Call<bool>(SWITCH_IN_PROGRESS))
            {
                Log("  game switch to " + _pulseWho + " started");
                _pulseCheckUntil = -1;
            }
            else if (now > _pulseCheckUntil)
            {
                Log("  game switch to " + _pulseWho + " did NOT start; blockers now: " + Blockers()
                    + " (the game also refuses while on a mission or before that character is unlocked)");
                _pulseCheckUntil = -1;
            }
        }

        // What the game's selector checks before it opens (selector.c func_120), as far as natives can see it.
        string Blockers()
        {
            var b = new List<string>();
            try
            {
                int ped = Me(), player = Call<int>(PLAYER_ID);
                int wanted = Call<int>(WANTED_LEVEL, player);
                if (wanted > 0) b.Add("wanted level " + wanted);
                if (!Call<bool>(SAFE_TO_SWITCH)) b.Add("not safe to start a switch");
                if (Call<bool>(RAGDOLL, ped)) b.Add("ragdolling");
                if (Call<bool>(ARRESTED, player, true)) b.Add("being arrested");
                if (Call<bool>(CUTSCENE_PLAYING)) b.Add("cutscene");
                if (Call<bool>(PAUSE_MENU)) b.Add("pause menu");
                if (Call<bool>(SYSTEM_UI)) b.Add("system UI");
                if (Call<bool>(HUD_ACTIVE, 16)) b.Add("radio wheel up");
                if (Call<bool>(HUD_ACTIVE, 19)) b.Add("weapon wheel up");
                if (Call<bool>(MISSION_FLAG)) b.Add("mission flag");
                if (!Call<bool>(CONTROL_ON, player)) b.Add("player control off");
                if (Threads("selector") == 0) b.Add("the game's selector script is not running");
                if (Threads("player_timetable_scene") > 0) b.Add("the last switch scene is still running");
                if (Threads("appcamera") > 0) b.Add("phone camera");
            }
            catch (Exception ex) { b.Add("check failed: " + ex.Message); }
            return b.Count == 0 ? "none" : string.Join(", ", b);
        }

        // Keeps the game's own character wheel (and its colour filter) off screen while ViSwitch presses the key.
        void HideGameWheel()
        {
            try
            {
                if (_switchMovie == 0) _switchMovie = Call<int>(SF_REQUEST, "PLAYER_SWITCH");
                if (_switchMovie != 0 && Call<bool>(SF_LOADED, _switchMovie))
                {
                    Call(SF_BEGIN, _switchMovie, "SET_SWITCH_VISIBLE");
                    Call(SF_PARAM_INT, 0);
                    Call(SF_END);
                }
                foreach (string fx in WheelOpenEffects) if (Call<bool>(POSTFX_RUNNING, fx)) Call(POSTFX_STOP, fx);
            }
            catch { }
        }

        static bool GameWheelOpen()
        {
            foreach (string fx in WheelOpenEffects) if (Call<bool>(POSTFX_RUNNING, fx)) return true;
            return false;
        }

        static int Threads(string script) { return Call<int>(THREADS, Call<int>(HASH_KEY, script)); }

        void CheckTestCommand(int ped)
        {
            if (Now() < _cmdLookAt) return;
            _cmdLookAt = Now() + 500;
            if (!File.Exists(_cmd)) return;
            string text;
            try { text = File.ReadAllText(_cmd).Trim().ToLowerInvariant(); File.Delete(_cmd); } catch { return; }
            int target = text.StartsWith("mic") ? 0 : text.StartsWith("fra") ? 1 : text.StartsWith("tre") ? 2 : text.StartsWith("luc") ? 3 : -1;
            Log("test command \"" + text + "\"");
            if (target < 0 || _busy || _open || Call<bool>(SWITCH_IN_PROGRESS) || _pulseUntil >= 0) { Log("  ignored"); return; }
            if (target == CurrentIndex(ped)) { Log("  already " + Who(target)); return; }
            SwitchTo(target);
        }

        void EndSceneOnInput(int ped)
        {
            bool moving = Math.Abs(Call<float>(DIS_NORMAL, 0, 30)) > 0.3f || Math.Abs(Call<float>(DIS_NORMAL, 0, 31)) > 0.3f;
            int[] buttons = { 21, 22, 23, 24, 25, 44 };
            foreach (int b in buttons) if (Call<bool>(DIS_PRESSED, 0, b)) moving = true;
            if (moving) { Call(CLEAR_TASKS, ped); _sceneActive = false; }
        }

        // ---------------- switching ----------------
        void SwitchTo(int target)
        {
            int ped = Me();
            int cur = CurrentIndex(ped);
            Log("switch " + Who(cur) + " -> " + Who(target));
            _sceneActive = false;
            try
            {
                if (_hudSwitches && (target == 3 || cur == 3))
                {
                    HudLuciaSwitch(ped, cur, target);
                    return;
                }
                if (target == 3)
                {
                    if (cur < 0 || cur > 2) { Log("  not on a story character, nothing to do"); return; }
                    SkySwitch(ped, -1, true);
                    return;
                }
                if (cur == 3)
                {
                    int host = HostIndex();
                    if (host == target || host < 0) SkySwitch(ped, target, false);
                    else FadeBackThenGameSwitch(ped, target);
                    return;
                }
                // between the story characters: the game's own switch (sky camera and its own scenes)
                StartGameSwitch(target);
            }
            finally { _lastModel = Call<int>(ENTITY_MODEL, Me()); } // ViSwitch's own swaps are not "outside" ones
        }

        // GTA6Hud 2.1: to Lucia, SwapToLucia starts the game's sky switch to her ped. From Lucia, SwapFromLucia
        // switches back to her host and, for the other story character, asks for the game's switch after it
        // (TakeHudSwitchPulse); with no host ped left it swaps the model back and returns true for that switch.
        void HudLuciaSwitch(int ped, int cur, int target)
        {
            Ped p = (Ped)Entity.FromHandle(ped);
            try
            {
                if (target == 3)
                {
                    if (cur < 0 || cur > 2) { Log("  not on a story character, nothing to do"); return; }
                    Log("  GTA6Hud's switch to Lucia");
                    _mSwapToLucia.Invoke(_hud, new object[] { p });
                    return;
                }
                Log("  GTA6Hud's switch back from Lucia");
                if ((bool)_mSwapFromLucia.Invoke(_hud, new object[] { p, target })) StartGameSwitch(target);
            }
            catch (Exception ex) { Log("  GTA6Hud switch failed: " + (ex.InnerException ?? ex).Message); }
        }

        // GTA6Hud 2.1 asks for the game's switch with a 4-frame select pulse in its own wheel, which ViSwitch
        // keeps stood down, so ViSwitch takes the request and holds the key the way the game's selector needs.
        void TakeHudSwitchPulse()
        {
            try
            {
                if ((int)_fSwapPulse.GetValue(_hud) <= 0) return;
                int target = Array.IndexOf(SelectInputs, (int)_fSwapPulseInput.GetValue(_hud));
                _fSwapPulse.SetValue(_hud, 0);
                if (target < 0 || _pulseUntil >= 0) return;
                Log("GTA6Hud asked for the game's switch to " + StoryNames[target]);
                StartGameSwitch(target);
            }
            catch (Exception ex) { Log("switch request error: " + ex.Message); _hudSwitches = false; }
        }

        int HostIndex()
        {
            try { return (int)_fLuciaHost.GetValue(_hud); } catch { return -1; }
        }

        static string Who(int i) { return i == 3 ? "Lucia" : (i >= 0 && i < 3 ? StoryNames[i] : "?"); }

        Scene PickScene(int owner)
        {
            var pool = new List<int>();
            for (int i = 0; i < Scenes.Length; i++)
                if ((owner < 0 || Scenes[i].Owner == owner) && i != _lastScene) pool.Add(i);
            if (pool.Count == 0) for (int i = 0; i < Scenes.Length; i++) if (owner < 0 || Scenes[i].Owner == owner) pool.Add(i);
            int pick = pool[_rng.Next(pool.Count)];
            _lastScene = pick;
            return Scenes[pick];
        }

        // Rises into the sky, swaps (toLucia: story character -> Lucia; else Lucia -> her host), drops the
        // new character at a random scene and brings the camera back down onto them.
        void SkySwitch(int ped, int storyTarget, bool toLucia)
        {
            _busy = true;
            int frozen = 0;
            try
            {
                Scene scene = PickScene(toLucia ? -1 : storyTarget);
                Log("  sky switch to " + (toLucia ? "Lucia" : StoryNames[storyTarget]) + " at " + scene.Name + " (" + scene.Scenario + ")");
                Call(SWITCH_OUT, ped, 0, 1);
                WaitUntil(() => Call<bool>(SWITCH_IN_PROGRESS), 1500);
                if (!Call<bool>(SWITCH_IN_PROGRESS))
                {
                    Log("  the sky camera did not start - swapping in place");
                    SwapModel(ped, storyTarget, toLucia);
                    return;
                }
                WaitUntil(() => Call<int>(SWITCH_STATE) >= 5, 12000);
                Log("  in the sky (switch state " + Call<int>(SWITCH_STATE) + ")");

                // 1. Swap while the ped still stands where it was: that ground is loaded, so nothing falls.
                SwapModel(ped, storyTarget, toLucia);
                frozen = Me();
                Call(FREEZE, frozen, true);
                int t0 = Now();
                WaitUntil(() => Call<bool>(STREAMED, frozen), 10000);
                Log("  swapped in place; streaming " + (Call<bool>(STREAMED, frozen) ? "done" : "NOT done") + " after " + (Now() - t0) + " ms");

                // 2. Move the (frozen) ped to the scene and wait for the area and its collision. Unfrozen, a ped
                //    teleported onto unloaded ground drops through the map (Enhanced 02:31 crash: z 5 under 32 m).
                Call(SET_COORDS, frozen, scene.At.X, scene.At.Y, scene.At.Z, false, false, false, false);
                Call(LOAD_SCENE_SPHERE, scene.At.X, scene.At.Y, scene.At.Z, 60f, 0);
                t0 = Now();
                int f = frozen, collisionAt = -1;
                Vector3 sc = scene.At;
                // Collision is what keeps the ped up; the rest of the scene gets 2.5 s more to fill in
                // (Galileo never reported loaded on Enhanced and held the camera in the sky for 12 s).
                WaitUntil(() =>
                {
                    Call(REQUEST_COLLISION, sc.X, sc.Y, sc.Z);
                    if (!Call<bool>(COLLISION_LOADED, f)) return false;
                    if (collisionAt < 0) collisionAt = Now();
                    return Call<bool>(LOAD_SCENE_LOADED) || Now() - collisionAt > 2500;
                }, 12000);
                Log("  scene " + (Call<bool>(LOAD_SCENE_LOADED) ? "loaded" : "NOT loaded") + ", collision "
                    + (Call<bool>(COLLISION_LOADED, frozen) ? "loaded" : "NOT loaded") + " after " + (Now() - t0) + " ms");
                Call(LOAD_SCENE_STOP);

                // 3. Land on safe ground, start the scene and bring the camera down.
                Vector3 at = Land(scene.At);
                Call(SET_COORDS, frozen, at.X, at.Y, at.Z, false, false, false, false);
                Call(SET_HEADING, frozen, scene.Heading);
                Call(FREEZE, frozen, false);
                Call(SCENARIO_IN_PLACE, frozen, scene.Scenario, 0, true);
                int landed = frozen;
                frozen = 0;

                Call(SWITCH_IN, landed);
                WaitUntil(() => !Call<bool>(SWITCH_IN_PROGRESS), 20000);
                _sceneActive = true;
                Log("  landed at " + (int)at.X + "," + (int)at.Y + "," + (int)at.Z);
            }
            catch (Exception ex)
            {
                Log("  sky switch failed: " + ex.Message);
                if (Call<bool>(SWITCH_IN_PROGRESS)) { try { Call(SWITCH_IN, Me()); } catch { } }
            }
            finally
            {
                if (frozen != 0) { try { Call(FREEZE, frozen, false); } catch { } }
                _busy = false;
            }
        }

        // Nearest safe spot on the ground near the scene, once the area has streamed in.
        static Vector3 Land(Vector3 at)
        {
            var safe = new OutputArgument();
            if (Call<bool>(SAFE_COORD, at.X, at.Y, at.Z, true, safe, 16))
            {
                Vector3 s = safe.GetResult<Vector3>();
                if (s.DistanceTo(at) < 25f) return s;
            }
            var z = new OutputArgument();
            if (Call<bool>(GROUND_Z, at.X, at.Y, at.Z + 50f, z, false, false))
                return new Vector3(at.X, at.Y, z.GetResult<float>());
            return at;
        }

        void SwapModel(int ped, int storyTarget, bool toLucia)
        {
            Ped p = (Ped)Entity.FromHandle(ped);
            try
            {
                if (toLucia) _mSwapToLucia.Invoke(_hud, new object[] { p });
                else _mSwapFromLucia.Invoke(_hud, new object[] { p, storyTarget });
            }
            catch (Exception ex)
            {
                Log("  GTA6Hud swap failed (" + ex.InnerException?.Message + "), swapping the model directly");
                int model = toLucia ? _lucia : (int)StoryModels[storyTarget];
                Call(MODEL_REQUEST, model);
                WaitUntil(() => Call<bool>(MODEL_LOADED, model), 5000);
                Call(SET_PLAYER_MODEL, Call<int>(PLAYER_ID), model);
            }
        }

        // Lucia -> a story character other than her host: behind a fade, swap back to the host and start the
        // game's own switch to the target from there. The screen stays black until the game's sky camera is
        // rising, so the host is never seen; if the game refuses the switch, it fades back in on the host.
        void FadeBackThenGameSwitch(int ped, int target)
        {
            _busy = true;
            bool faded = false;
            try
            {
                Log("  behind a fade: back to " + Who(HostIndex()) + ", then the game's switch to " + StoryNames[target]);
                Call(FADE_OUT, 300);
                faded = true;
                WaitUntil(() => Call<bool>(FADED_OUT), 1500);
                Ped p = (Ped)Entity.FromHandle(ped);
                bool needSwitch = true;
                try { needSwitch = (bool)_mSwapFromLucia.Invoke(_hud, new object[] { p, target }); }
                catch (Exception ex) { Log("  GTA6Hud swap back failed: " + (ex.InnerException ?? ex).Message); }
                int now = Me();
                WaitUntil(() => Call<bool>(STREAMED, now), 8000);
                if (needSwitch)
                {
                    // OnTick is not re-entered while this runs, so drive the select-key hold from here
                    StartGameSwitch(target);
                    while (_pulseUntil >= 0 || _pulseCheckUntil >= 0) { UpdateGameSwitchPulse(); Frame(); }
                }
            }
            finally
            {
                if (faded) { try { Call(FADE_IN, 500); } catch { } }
                _busy = false;
            }
        }

        // GTA6Hud also swaps on its own on Enhanced (UpdateLuciaMode): it lends Lucia's host as soon as she is
        // near a mission marker ([Lucia] MissionRange) or a mission runs, and swaps back after. Those instant
        // swaps look like a switch without the sky, so they are logged as not ViSwitch's.
        void WatchOutsideSwaps(int ped)
        {
            int model = Call<int>(ENTITY_MODEL, ped);
            bool quiet = !_busy && !Call<bool>(SWITCH_IN_PROGRESS) && _pulseUntil < 0 && _pulseCheckUntil < 0;
            if (quiet && _lastModel != 0 && model != _lastModel)
                Log("player became " + ModelName(model) + " (was " + ModelName(_lastModel) + ") without ViSwitch"
                    + (_lastModel == _lucia && !_hudSwitches ? ": GTA6Hud lends Lucia's host near a mission marker or on a mission ([Lucia] MissionRange)" : ""));
            _lastModel = model;
        }

        string ModelName(int model)
        {
            if (model == _lucia) return "Lucia";
            int i = Array.IndexOf(StoryModels, (uint)model);
            return i >= 0 ? StoryNames[i] : "model 0x" + ((uint)model).ToString("X8");
        }
    }
}
