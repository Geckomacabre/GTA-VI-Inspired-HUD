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
// Franklin and Trevor, including the one GTA6Hud asks for after switching from Lucia back to her host. It also
// picks where Lucia is when you switch to her: at one of the haunts below, mid-activity, as the game does for the
// other three, unless you left her under a minute ago.
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
    public partial class ViSwitch : Script
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
        const ulong REL_GROUP = 0x7DBDD04862D95F04;           // GET_PED_RELATIONSHIP_GROUP_HASH
        const ulong MAX_WANTED = 0x462E0DB9B137DC5F;          // GET_MAX_WANTED_LEVEL
        const ulong FREEZE = 0x428CA6DBD1094446;              // FREEZE_ENTITY_POSITION
        const ulong REQUEST_COLLISION = 0x07503F7948F491A7;   // REQUEST_COLLISION_AT_COORD
        const ulong COLLISION_LOADED = 0xE9676F61BC0B3321;    // HAS_COLLISION_LOADED_AROUND_ENTITY
        const ulong IS_PRESSED = 0xF3A21BCD95725A4A;          // IS_CONTROL_PRESSED
        const ulong IS_ENABLED = 0x1CEA6BFDF248E5D9;          // IS_CONTROL_ENABLED
        const ulong ENABLE_CONTROL = 0x351220255D64C155;      // ENABLE_CONTROL_ACTION
        const ulong BUTTON_STRING = 0x0499D7B09FC9B407;       // GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING
        const ulong USING_KEYBOARD = 0xA571D46727E2B718;      // IS_USING_KEYBOARD_AND_MOUSE
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
        const ulong ENTITY_COORDS = 0x3FEF770D40960D5A;       // GET_ENTITY_COORDS
        const ulong CHANGE_PLAYER_PED = 0x048189FAC643DEEE;
        const ulong BLOCK_EVENTS = 0x90D2156198831D69;        // SET_BLOCKING_OF_NON_TEMPORARY_EVENTS
        const ulong OFFSET_COORDS = 0x1899F328B0E12848;       // GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS
        const ulong SET_VISIBLE = 0xEA1C610A04DB6BBB;         // SET_ENTITY_VISIBLE
        const ulong ON_SCREEN = 0xE659E47AF827484B;           // IS_ENTITY_ON_SCREEN
        const ulong ENTITY_HEADING = 0xE83D4F9BA2A38914;      // GET_ENTITY_HEADING
        const ulong MISSION_ENTITY = 0x0A7B270912999B3C;      // IS_ENTITY_A_MISSION_ENTITY
        const ulong OURS = 0xDDE6DF5AE89981D2;                // DOES_ENTITY_BELONG_TO_THIS_SCRIPT

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
        // The button string names the key on the device used last: on a pad it is the pad's (these keys have none
        // there), not F5 / F6 / F7, so a pad player's switch read as "rebound", was only simulated and never happened
        // (a player: "tf it dosent", the cards up and Franklin picked, no switch). The keyboard's own, last seen.
        readonly string[] _keyboardBinding = new string[3];

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
        // On duty in LSPDFR: the game's character wheel and its Michael / Franklin / Trevor / Online picks.
        static readonly int[] SwitchControls = { 19, 166, 167, 168, 169 };
        // LSPDFR's shift, as the game shows it: the player in SECURITY_GUARD, the max wanted level 0
        // (seen 2026-10-21: 0xF50B51B7 / 0 on duty, PLAYER 0x6F0783F5 / 5 off).
        const int SecurityGuard = unchecked((int)0xF50B51B7);

        struct Scene
        {
            public string Name; public Vector3 At; public float Heading; public string Scenario; public int Owner;
            public Scene(int owner, string name, float x, float y, float z, float h, string scenario)
            { Owner = owner; Name = name; At = new Vector3(x, y, z); Heading = h; Scenario = scenario; }
        }

        // Where each story character hangs out, and Lucia draws from all of them. Every spot is one of the game's
        // own scenario points (the regions in update.rpf's sp_manifest, read with CodeWalker), with the activity
        // the game itself uses there: on the street (within 3 m of the points around it), outside every interior.
        // Hand-picked spots were guesses, and one put Lucia inside a building shell on Portola Drive.
        static readonly Scene[] Scenes =
        {
            new Scene(0, "Portola Drive shops", -721.328f, -150.729f, 36.036f, 299.4f, "WORLD_HUMAN_WINDOW_SHOP_BROWSE"),
            new Scene(0, "Del Perro Pier", -1811.844f, -1193.747f, 12.018f, 283.8f, "WORLD_HUMAN_DRINKING"),
            new Scene(0, "Galileo Observatory", -412.435f, 1168.113f, 324.854f, 166.5f, "WORLD_HUMAN_TOURIST_MAP"),
            new Scene(0, "Vinewood Boulevard", 300.156f, 179.224f, 103.145f, 22.1f, "WORLD_HUMAN_TOURIST_MAP"),
            new Scene(1, "Grove Street", 118.308f, -1940.438f, 19.624f, 295.5f, "WORLD_HUMAN_HANG_OUT_STREET"),
            new Scene(1, "Strawberry", -20.742f, -1395.066f, 28.374f, 267.2f, "WORLD_HUMAN_LEANING"),
            new Scene(1, "Vespucci Beach", -1291.941f, -1468.763f, 3.315f, 5.6f, "WORLD_HUMAN_SMOKING"),
            new Scene(1, "Legion Square", 195.236f, -943.592f, 29.692f, 129.9f, "WORLD_HUMAN_SMOKING"),
            new Scene(2, "Vespucci Canals", -1125.183f, -1542.142f, 3.382f, 305.6f, "WORLD_HUMAN_LEANING"),
            new Scene(2, "Sandy Shores", 1873.822f, 3753.720f, 31.983f, 240.9f, "WORLD_HUMAN_SMOKING"),
            new Scene(2, "Paleto Bay", -263.911f, 6289.305f, 30.485f, 178.7f, "WORLD_HUMAN_SMOKING"),
            new Scene(2, "Mirror Park", 1123.022f, -642.135f, 55.702f, 242.6f, "WORLD_HUMAN_SMOKING"),
            new Scene(2, "Del Perro Beach", -1481.294f, -1128.822f, 1.275f, 312.3f, "WORLD_HUMAN_HANG_OUT_STREET"),
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
        MethodInfo _mDraw, _mWantBlur, _mInteract, _mSwapToLucia, _mSwapFromLucia, _mEnsureLucia, _mNotify;
        // GTA6Hud's host ped (the story character Lucia was switched to from), its saved outfits, and how it
        // notes an outfit and leaves a ped in the world: the instant way back to the host (BackToHostAt)
        FieldInfo _fHostPed, _fOutfits, _fLuciaActive;
        MethodInfo _mOutfitOf, _mLeaveInWorld, _mOutfitApply, _mLuciaSave;

        // Lucia's haunts (GTA6Hud 2.1). GTA6Hud leaves her ped standing where you last left her, and clears her
        // tasks as it hands her over in the sky (UpdateLuciaSwitch, switch state 8), so she was always found idle,
        // and at Vespucci Beach until moved. Picked on the wheel a minute or more after you left her, she is put at
        // a random haunt before the sky switch starts, held there until its ground has loaded (a ped put on
        // unloaded ground drops through the map), and starts the haunt's activity once GTA6Hud has handed her
        // over, so the camera comes down onto her mid-scene. Moving ends the scene, as for the others.
        FieldInfo _fLuciaPed, _fLuciaPos, _fLuciaHeading;
        const int LuciaKeepMs = 60000;      // real ms: switch back to her within a minute and she's where you left her
        const int LuciaSettleMs = 30000;    // real ms the sky switch gets to hand her over
        int _luciaLeftAt = -1;              // real ms she was last switched away from; -1: not this session
        bool _wasLucia;
        Scene _luciaScene;                  // the haunt she's held at, while _luciaHeld, until she lands there
        bool _luciaHeld;
        int _luciaScenePed, _luciaSceneUntil, _luciaSettleAt;

        // wheel state
        bool _open, _busy, _sceneActive;
        int _sel, _current, _pulseInput, _lastScene = -1;
        int _pulseUntil = -1, _pulseCheckUntil = -1, _pulseTicks, _pulseSeen, _pulseSeenDisabled, _pulseWasDisabled, _lastModel;
        int _pulseReleaseUntil = -1, _pulseWheelAt = -1;
        bool _pulseStarted;
        // GTA6Hud 2.1 goes from Lucia to the story character who isn't her host in two steps: its own sky switch
        // back to the host, then the game's switch on. It asks for the second one the moment the first ends,
        // when the game isn't ready yet: its wheel opens, but the switch starts seconds later or not at all,
        // and a second pick in between cancels it (21 Oct 17:58: Lucia -> Michael ended on Franklin). So the
        // second step waits until the game is ready, is tried again if it doesn't start, and keeps the wheel
        // shut until it's done.
        int _chainTarget = -1, _chainTries, _chainQuietSince = -1, _chainGiveUpAt;
        bool _chainHolding, _chainUnseen;   // unseen: from Lucia, her host unseen in her place (BackToHostAt)
        // the host while he stands unseen where Lucia was, and where he was before her; home once he's been put
        // back there from the sky (UpdateHiddenHostFlight)
        Ped _hiddenHost;
        Vector3 _hiddenHostWas;
        float _hiddenHostHeading;
        bool _hiddenHostHome;
        static readonly int[] UnseenHeld = { 21, 22, 24, 25, 30, 31, 37, 44 };   // sprint, jump, attack, aim, move, wheel, cover
        const int ChainQuietMs = 1200, ChainTries = 3, ChainMaxMs = 25000;
        bool _pulseWheelBefore;
        string _pulseWho;
        float _blend;

        // test hook: a line "michael", "franklin", "trevor" or "lucia" in scripts\ViSwitch.cmd switches to them
        readonly string _cmd;
        int _cmdLookAt, _watchAt;

        // LSPDFR: installed (plugins\LSPD First Response.dll beside the game), and on duty, asked twice a second
        readonly bool _lspdfr;
        bool _onDuty;
        int _dutyAt;

        public ViSwitch()
        {
            _log = Path.Combine(BaseDirectory, "ViSwitch.log");
            _cmd = Path.Combine(BaseDirectory, "ViSwitch.cmd");
            // the last session's log stays beside the new one: a script reload mid-game starts a new log
            try { if (File.Exists(_log)) File.Copy(_log, Path.Combine(BaseDirectory, "ViSwitch.prev.log"), true); } catch { }
            try { File.WriteAllText(_log, ""); } catch { }
            _enabled = ReadIniBool(Path.Combine(BaseDirectory, "GTA6Hud.ini"), "Swap", "Enabled", true);
            try
            {
                string game = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                _lspdfr = File.Exists(Path.Combine(game, "plugins", "LSPD First Response.dll"));
            }
            catch { }
            InitJason();
            Log("loaded; GTA6Hud [Swap] Enabled=" + _enabled + (_lspdfr ? "; LSPDFR installed: no switching while on duty" : ""));
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
                _fLuciaPed = t.GetField("_luciaPed", I);
                _fLuciaPos = t.GetField("_luciaPos", I);
                _fLuciaHeading = t.GetField("_luciaHeading", I);
                _mDraw = t.GetMethod("DrawCharSwap", I);
                _mWantBlur = t.GetMethod("WantBlur", I);
                _mInteract = t.GetMethod("InteractionPressed", I);
                _mSwapToLucia = t.GetMethod("SwapToLucia", I);
                _mEnsureLucia = t.GetMethod("EnsureLuciaPed", I);
                _fHostPed = t.GetField("_hostPed", I);
                _fOutfits = t.GetField("_outfits", I);
                _mOutfitOf = t.GetMethod("OutfitOf", I);
                _mLeaveInWorld = t.GetMethod("LeaveInWorld", S);
                _mOutfitApply = t.GetMethod("OutfitApply", S);
                _fLuciaActive = t.GetField("_luciaActive", I);
                _mLuciaSave = t.GetMethod("LuciaSave", I);
                _mNotify = t.GetMethod("Notify", S, null, new[] { typeof(string) }, null);
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

        bool OnDuty()
        {
            if (!_lspdfr) return false;
            int now = Now();
            if (now < _dutyAt) return _onDuty;
            _dutyAt = now + 500;
            bool duty = Call<int>(REL_GROUP, Me()) == SecurityGuard && Call<int>(MAX_WANTED) == 0;
            if (duty != _onDuty) Log(duty ? "LSPDFR: on duty, character switching off" : "LSPDFR: off duty, character switching back");
            _onDuty = duty;
            return duty;
        }

        int CurrentIndex(int ped)
        {
            int model = Call<int>(ENTITY_MODEL, ped);
            if (model == _lucia) return 3;
            if (_jason != 0 && model == _jason) return 4;
            return Array.IndexOf(StoryModels, (uint)model);
        }

        // ---------------- tick ----------------
        void OnTick(object sender, EventArgs e)
        {
            // On duty in LSPDFR there's no character switching of any kind (the user's call): switching away
            // from the cop mid-shift pulls LSPDFR's character out from under it. Our wheel and GTA6Hud's stay
            // shut and the game's own switch keys are held, whatever [Swap] says.
            if (OnDuty())
            {
                if (_open) { _open = false; WantBlur(false); }
                _blend = 0f;
                if (_enabled && FindHud())
                {
                    try { _fRefSwapEnabled.SetValue(null, false); } catch { }
                    try { if (_hudSwitches) { _fSwapOpen.SetValue(_hud, false); _fSwapBlend.SetValue(_hud, 0f); } } catch { }
                }
                foreach (int c in SwitchControls) Call(DISABLE_CONTROL, 0, c, true);
                return;
            }
            if (!_enabled || !FindHud()) return;
            bool luciaOn;
            try { luciaOn = (bool)_fRefLuciaEnabled.GetValue(null); } catch { return; }
            CheckJason();
            if (!luciaOn && !_jasonOn)
            {
                // Without Lucia or Jason, GTA6Hud's own 3-character wheel is exactly the game's switch: leave it alone.
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
            UpdateChain(Me());
            UpdateGameSwitchPulse();
            UpdateHiddenHostFlight();
            UpdateHiddenHost();
            UpdateFlight();
            LookForCardsDrawer();

            int ped = Me();
            int now = Now();
            if (now >= _watchAt)
            {
                _watchAt = now + 500;   // a log line only: twice a second will do
                WatchOutsideSwaps(ped);
                bool lucia = Call<int>(ENTITY_MODEL, ped) == _lucia;
                if (_wasLucia && !lucia) _luciaLeftAt = RealNow();   // her minute starts
                _wasLucia = lucia;
            }
            if (_luciaHeld) SettleLucia(ped);
            SettleJason(ped);
            JasonMission(ped);
            JasonStoreDoors(ped);
            if (_sceneActive) EndSceneOnInput(ped);
            CheckTestCommand(ped);

            // The game's own wheel must not open on Left Alt, but INPUT_CHARACTER_WHEEL is only disabled while Alt
            // is down: disabled every frame (as GTA6Hud's wheel did) it also disables a physically held F5-F8
            // (11:28 test: the real F5 read disabled on all 19 frames), so the game never switched to Michael or
            // Trevor, not even from the player's own F keys. The game's wheel needs 300 ms of hold to open, so the
            // first frame of an Alt press getting through does nothing.
            bool altDown = Call<bool>(DIS_PRESSED, 0, 19);
            // what can keep the wheel shut, asked only with Alt down or the wheel open (nothing else uses it)
            bool blocked = (altDown || _open)
                           && (_busy || !Call<bool>(ENTITY_EXISTS, ped) || Call<bool>(ENTITY_DEAD, ped, false)
                               || HudBool(_fWheelOpen) || HudBool(_fSettingsOpen) || Call<bool>(SWITCH_IN_PROGRESS) || Game.IsPaused
                               || NitrousHoldsKey() || _chainTarget >= 0 || _flyTo != null);
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
                // along GTA VI's row: Michael, Franklin, Trevor, Jason, Lucia (whoever's in the game)
                if (next) _sel = RowStep(_sel, 1, luciaOn);
                if (prev) _sel = RowStep(_sel, -1, luciaOn);
            }
            _blend = Math.Max(0f, Math.Min(1f, _blend + (_open ? 1f : -1f) * Game.LastFrameTime / 0.18f));
            if (_cardsDrawer)
            {
                // ViExtras' Switcher draws GTA VI's row of cards from SwitchState; GTA6Hud's wheel stays shut
                PublishWheel(luciaOn);
                if (_hudSwitches) { try { _fSwapOpen.SetValue(_hud, false); } catch { } }
            }
            else if (_hudSwitches)
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
            _pulseCheckUntil = _pulseUntil + (_chainTarget >= 0 ? 6000 : 8000);   // the game can take a while after a switch
            _pulseStarted = false;
            _pulseTicks = _pulseSeen = _pulseSeenDisabled = _pulseWasDisabled = 0;
            _pulseWheelAt = -1;
            _pulseWheelBefore = GameWheelOpen(); // an effect left running can't signal this hold
            string key = "?";
            try { key = Call<string>(BUTTON_STRING, 0, _pulseInput, true); } catch { }
            bool keyboard = true;
            try { keyboard = Call<bool>(USING_KEYBOARD, 2); } catch { }
            if (keyboard) _keyboardBinding[target] = key;
            string binding = _keyboardBinding[target] ?? DefaultBindings[target];
            if (!keyboard) key = binding + " on the keyboard, a pad in use";
            ReleaseRealKey();
            string how;
            if (binding != DefaultBindings[target]) how = "simulated only: the key is rebound";
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
                _pulseStarted = true;
                _pulseCheckUntil = -1;
            }
            else if (now > _pulseCheckUntil)
            {
                Log("  game switch to " + _pulseWho + " did NOT start; blockers now: " + Blockers()
                    + " (the game also refuses while on a mission or before that character is unlocked)");
                _pulseCheckUntil = -1;
                // the game's own wheel says nothing; a switch on from Lucia's host tries again first (UpdateChain)
                if (_chainTarget < 0) Tell("The game won't switch to " + _pulseWho + " right now. It refuses during missions, and before the story unlocks a character.");
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
            int target = text.StartsWith("mic") ? 0 : text.StartsWith("fra") ? 1 : text.StartsWith("tre") ? 2 : text.StartsWith("luc") ? 3 : text.StartsWith("jas") ? 4 : -1;
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
                if (target == 4 || cur == 4)
                {
                    JasonSwitch(ped, cur, target);
                    return;
                }
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
                    PlaceLucia(ped);   // before the switch: the sky camera goes to wherever her ped is when it starts
                    _mSwapToLucia.Invoke(_hud, new object[] { p });
                    return;
                }
                if (target != HostIndex() && BackToHostAt(p, target, HudHostPed(), HostIndex())) return;
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
                Log("GTA6Hud asked for the game's switch to " + StoryNames[target] + "; waiting until the game is ready");
                _chainTarget = target;
                _chainTries = 0;
                _chainHolding = false;
                _chainQuietSince = -1;
                _chainGiveUpAt = RealNow() + ChainMaxMs;
            }
            catch (Exception ex) { Log("switch request error: " + ex.Message); _hudSwitches = false; }
        }

        // The second step of a switch from Lucia: once the game is ready (no switch running, safe to start one,
        // the player in control, no switch scene still playing) and has stayed so for ChainQuietMs, the
        // select-key hold; if the switch doesn't start, again, up to ChainTries times.
        // From Lucia (BackToHostAt) there's no waiting for a quiet game: the switch is asked for at once, as from
        // the wheel, and the unseen host can't be walked about meanwhile.
        void UpdateChain(int ped)
        {
            if (_chainTarget < 0) return;
            int now = RealNow();
            if (_chainUnseen && !Call<bool>(SWITCH_IN_PROGRESS))
                foreach (int c in UnseenHeld) Call(DISABLE_CONTROL, 0, c, true);
            if (_chainHolding)
            {
                if (_pulseUntil >= 0 || _pulseCheckUntil >= 0) return;   // holding, or waiting to see it start
                _chainHolding = false;
                if (_pulseStarted)
                {
                    _chainTarget = -1;
                    _chainUnseen = false;
                    return;
                }
                _chainQuietSince = -1;
                if (_chainTries >= ChainTries) { GiveUpChain(); return; }
            }
            if (now > _chainGiveUpAt) { GiveUpChain(); return; }
            bool ready = !Call<bool>(SWITCH_IN_PROGRESS) && !Game.IsPaused && !Call<bool>(ENTITY_DEAD, ped, false)
                         && (_chainUnseen || (Call<bool>(SAFE_TO_SWITCH) && Call<bool>(CONTROL_ON, Call<int>(PLAYER_ID))
                                              && Threads("player_timetable_scene") == 0));
            if (!ready) { _chainQuietSince = -1; return; }
            if (!_chainUnseen)
            {
                if (_chainQuietSince < 0) { _chainQuietSince = now; return; }
                if (now - _chainQuietSince < ChainQuietMs) return;
            }
            _chainTries++;
            Log("  the game is ready: switching on to " + StoryNames[_chainTarget] + (_chainTries > 1 ? " (try " + _chainTries + ")" : ""));
            _chainHolding = true;
            StartGameSwitch(_chainTarget);
        }

        // From Lucia to the story character who isn't her host, in one flight, the game's own. GTA6Hud's way is two
        // (its sky switch back to the host, then the game's switch on: "switched to michael and i still got switched
        // to franklin but like 2 seconds later it did another switch"), and a fade with the host brought in under it
        // left the game refusing its switch in the dark for 25 s, then the host where Lucia stood ("I switched to
        // trevor and got fucking franklin"). The game's switch (its sky camera, the character's own scene, outfit
        // and weapons) only starts from a story character, so: the host goes unseen just behind Lucia and the player
        // is handed to him (what GTA6Hud does at the top of its switch: her outfit noted, the host's events back on,
        // Lucia left in the world; it notices the player isn't Lucia any more and saves her spot itself), and the
        // game's switch is asked for at once. Its camera rises from her spot with her standing in it. The host comes
        // back where he was before her once he's off screen (UpdateHiddenHost). Jason leaves the same way, from his
        // host. A host who isn't in the world any more (GTA6Hud keeps one only for the session it made him in: after a
        // restart as Lucia it swapped you into him first, then switched) is a stand-in in his saved clothes, sent to
        // one of his own spots from the sky and given back to the game after. False when it can't: GTA6Hud's way then.
        bool BackToHostAt(Ped from, int target, Ped host, int hostIndex)
        {
            if (_mLeaveInWorld == null || from == null || !from.Exists()) return false;
            bool standIn = false;
            Vector3 hostWas = Vector3.Zero;
            float hostHeading = 0f;
            if (host == null || !host.Exists() || host.IsDead)
            {
                if (hostIndex < 0 || hostIndex > 2) return false;
                Scene home = PickScene(hostIndex);
                Vector3 behind = Call<Vector3>(OFFSET_COORDS, from.Handle, 0f, -0.8f, 0f);
                host = StandIn(hostIndex, behind, Call<float>(ENTITY_HEADING, from.Handle));
                if (host == null) return false;
                standIn = true;
                hostWas = home.At;
                hostHeading = home.Heading;
            }
            Log("  " + Who(hostIndex) + (standIn ? " (a stand-in)" : "") + " unseen behind " + ModelName(Call<int>(ENTITY_MODEL, from.Handle))
                + ", then the game's own switch to " + StoryNames[target] + " from there");
            Ped lucia = from;
            _busy = true;
            try
            {
                try
                {
                    if (_mOutfitOf != null && _fOutfits != null && _fOutfits.GetValue(_hud) is System.Collections.IDictionary outfits)
                        outfits[Call<int>(ENTITY_MODEL, lucia.Handle)] = _mOutfitOf.Invoke(_hud, new object[] { lucia });
                }
                catch (Exception ex) { Log("  her outfit wasn't noted: " + (ex.InnerException ?? ex).Message); }
                _hiddenHostWas = standIn ? hostWas : Call<Vector3>(ENTITY_COORDS, host.Handle, false);
                _hiddenHostHeading = standIn ? hostHeading : Call<float>(ENTITY_HEADING, host.Handle);
                Vector3 at = Call<Vector3>(OFFSET_COORDS, lucia.Handle, 0f, -0.8f, 0f);
                Call(SET_VISIBLE, host.Handle, false, false);
                _hiddenHost = host;
                Call(SET_COORDS, host.Handle, at.X, at.Y, at.Z, false, false, false, false);
                Call(SET_HEADING, host.Handle, Call<float>(ENTITY_HEADING, lucia.Handle));
                Call(CHANGE_PLAYER_PED, Call<int>(PLAYER_ID), host.Handle, false, false);
                Call(BLOCK_EVENTS, host.Handle, false);
                Call(CLEAR_TASKS, host.Handle);
                _mLeaveInWorld.Invoke(null, new object[] { lucia });
                _chainTarget = target;
                _chainTries = 0;
                _chainHolding = false;
                _chainQuietSince = -1;
                _chainGiveUpAt = RealNow() + ChainMaxMs;
                _chainUnseen = true;
                _hiddenHostHome = false;
                _lastModel = Call<int>(ENTITY_MODEL, host.Handle);   // ViSwitch's own swap, not an outside one
                return true;
            }
            catch (Exception ex)
            {
                Log("  the way from " + ModelName(Call<int>(ENTITY_MODEL, lucia.Handle)) + " failed (" + (ex.InnerException ?? ex).Message + ")");
                ShowHiddenHost(false);
                _chainTarget = -1;
                _chainUnseen = false;
                return true;
            }
            finally { _busy = false; }
        }

        // The unseen host back where he stood before Lucia while the game's switch is up in the sky and he's still
        // the player: the game takes where the player stands as where that character is, and puts him there when
        // you switch back to him. Moved after the switch, he came back in front of Lucia instead of at Michael's
        // house, and the game held that switch for 16 s first (22 Oct 13:20).
        void UpdateHiddenHostFlight()
        {
            if (_hiddenHost == null || _hiddenHostHome || !Call<bool>(SWITCH_IN_PROGRESS)) return;
            if (!_hiddenHost.Exists()) return;
            int h = _hiddenHost.Handle, state = Call<int>(SWITCH_STATE);
            if (Me() != h || state < 5) return;
            Call(SET_COORDS, h, _hiddenHostWas.X, _hiddenHostWas.Y, _hiddenHostWas.Z, false, false, false, false);
            Call(SET_HEADING, h, _hiddenHostHeading);
            _hiddenHostHome = true;
            Log("  " + Who(Array.IndexOf(StoryModels, (uint)Call<int>(ENTITY_MODEL, h))) + " back where he was before Lucia, from the sky (switch state " + state + ")");
        }

        // The unseen host, once the player is someone else and he's off screen (or 40 m off): back where he stood
        // before Lucia, and seen again. While he's still the player (the game's switch hasn't taken), he stays.
        void UpdateHiddenHost()
        {
            if (_hiddenHost == null || _chainTarget >= 0) return;
            if (!_hiddenHost.Exists()) { _hiddenHost = null; return; }
            int h = _hiddenHost.Handle;
            if (Call<bool>(SWITCH_IN_PROGRESS)) return;          // he's the player until the camera comes down
            if (Me() == h) { ShowHiddenHost(false); return; }   // no switch after all: he's who you are
            Vector3 me = Call<Vector3>(ENTITY_COORDS, Me(), false), him = Call<Vector3>(ENTITY_COORDS, h, false);
            if (Call<bool>(ON_SCREEN, h) && me.DistanceTo(him) < 40f) return;
            ShowHiddenHost(true);
        }

        void ShowHiddenHost(bool sendBack)
        {
            if (_hiddenHost == null) return;
            try
            {
                if (_hiddenHost.Exists())
                {
                    int h = _hiddenHost.Handle;
                    if (sendBack && !_hiddenHostHome)
                    {
                        Call(SET_COORDS, h, _hiddenHostWas.X, _hiddenHostWas.Y, _hiddenHostWas.Z, false, false, false, false);
                        Call(SET_HEADING, h, _hiddenHostHeading);
                    }
                    Call(SET_VISIBLE, h, true, false);
                    // He's no longer the player: given back to the game, which looks after the story characters
                    // you've left. GTA6Hud kept him as its own while you were Lucia, and the game can't use a
                    // character a script still holds.
                    bool given = false;
                    if (sendBack)
                    {
                        Call(BLOCK_EVENTS, h, false);
                        if (Call<bool>(MISSION_ENTITY, h) && Call<bool>(OURS, h, false)) { _hiddenHost.MarkAsNoLongerNeeded(); given = true; }
                    }
                    Log("  " + Who(Array.IndexOf(StoryModels, (uint)Call<int>(ENTITY_MODEL, h)))
                        + (sendBack ? (_hiddenHostHome ? " seen again at home" : " back where he was before Lucia") + (given ? ", given back to the game" : "")
                                    : " seen again where he stands"));
                }
            }
            catch (Exception ex) { Log("  the host couldn't be shown: " + ex.Message); }
            _hiddenHost = null;
        }

        void GiveUpChain()
        {
            Log("  gave up switching on to " + StoryNames[_chainTarget] + " after " + _chainTries + " tries; blockers now: " + Blockers());
            _chainUnseen = false;
            if (_hiddenHost != null && _hiddenHost.Exists() && Me() == _hiddenHost.Handle) ShowHiddenHost(false);
            Tell("The game won't switch to " + StoryNames[_chainTarget] + " right now. It refuses during missions, and before the story unlocks a character.");
            _chainTarget = -1;
            _chainHolding = false;
        }

        Ped HudHostPed()
        {
            try { return _fHostPed?.GetValue(_hud) as Ped; } catch { return null; }
        }

        int HostIndex()
        {
            try { return (int)_fLuciaHost.GetValue(_hud); } catch { return -1; }
        }

        void Tell(string text)
        {
            try { if (_mNotify != null) { _mNotify.Invoke(null, new object[] { text }); return; } } catch { }
#pragma warning disable CS0618   // PostTicker isn't in Script Hook V .NET Enhanced
            GTA.UI.Notification.Show(text);
#pragma warning restore CS0618
        }

        static string Who(int i) { return i == 3 ? "Lucia" : i == 4 ? "Jason" : (i >= 0 && i < 3 ? StoryNames[i] : "?"); }

        // A random scene of the owner's (all of them for -1), not the last one, nor one within minDistance of
        // awayFrom while there are others.
        Scene PickScene(int owner, Vector3 awayFrom = default(Vector3), float minDistance = 0f)
        {
            var pool = new List<int>();
            for (int i = 0; i < Scenes.Length; i++)
                if ((owner < 0 || Scenes[i].Owner == owner) && i != _lastScene
                    && (minDistance <= 0f || Scenes[i].At.DistanceTo(awayFrom) >= minDistance)) pool.Add(i);
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

        // Before GTA6Hud's switch to Lucia: unless she was left under a minute ago, her ped goes to a random haunt
        // away from the player, so the sky camera flies straight there. Her ped is made first if there isn't one
        // (GTA6Hud's EnsureLuciaPed, the one SwapToLucia uses), because a new one starts wherever GTA6Hud picks
        // (Vespucci Beach for a spot it thinks is indoors) and the camera would fly there instead.
        void PlaceLucia(int player)
        {
            _luciaHeld = false;
            if (_fLuciaPed == null || _fLuciaPos == null || _fLuciaHeading == null || _mEnsureLucia == null) return;
            if (_luciaLeftAt >= 0 && RealNow() - _luciaLeftAt < LuciaKeepMs)
            {
                Log("  Lucia was left " + (RealNow() - _luciaLeftAt) / 1000 + " s ago: she's where you left her");
                return;
            }
            Scene scene = PickScene(-1, Call<Vector3>(ENTITY_COORDS, player, true), 200f);
            try
            {
                _fLuciaPos.SetValue(_hud, scene.At);
                _fLuciaHeading.SetValue(_hud, scene.Heading);
            }
            catch (Exception ex) { Log("  couldn't place Lucia: " + ex.Message); return; }
            int lucia = 0;
            try { if (_mEnsureLucia.Invoke(_hud, null) is Ped p && p.Exists() && !p.IsDead) lucia = p.Handle; }
            catch (Exception ex) { Log("  couldn't make Lucia's ped: " + (ex.InnerException ?? ex).Message); }
            if (lucia == 0) { Log("  no Lucia ped: GTA6Hud's own spot"); return; }
            Log("  Lucia at " + scene.Name + " (" + scene.Scenario + ")");
            _luciaScene = scene;
            _luciaHeld = true;
            Vector3 at = scene.At;
            Call(CLEAR_TASKS, lucia);
            Call(FREEZE, lucia, true);   // until her ground has loaded: a ped on unloaded ground drops through the map
            Call(SET_COORDS, lucia, at.X, at.Y, at.Z, false, false, false, false);
            Call(SET_HEADING, lucia, scene.Heading);
            _luciaScenePed = lucia;
            _luciaSceneUntil = RealNow() + LuciaSettleMs;
        }

        // Every frame while she's held: once GTA6Hud has handed her over (she is the player) and the haunt's
        // collision is in, she's let go on the ground right where she stands and starts the activity. If the
        // switch never gets that far (it didn't start, or went elsewhere), she stays frozen until the player
        // comes near enough for her ground to load, so she can't fall through the map meanwhile.
        void SettleLucia(int player)
        {
            int lucia = _luciaScenePed;
            if (lucia == 0 || !Call<bool>(ENTITY_EXISTS, lucia) || Call<bool>(ENTITY_DEAD, lucia, false))
            {
                if (lucia != 0) _luciaHeld = false;   // gone: GTA6Hud makes a new one at _luciaPos next time
                return;
            }
            Scene s = _luciaScene;
            bool handedOver = player == lucia;
            bool waiting = RealNow() < _luciaSceneUntil;
            if (waiting) Call(REQUEST_COLLISION, s.At.X, s.At.Y, s.At.Z);
            else
            {
                if (RealNow() < _luciaSettleAt) return;
                _luciaSettleAt = RealNow() + 500;   // given up on the switch: a look twice a second
            }
            if (!Call<bool>(COLLISION_LOADED, lucia)) return;
            if (waiting && !handedOver && Call<bool>(SWITCH_IN_PROGRESS)) return;   // GTA6Hud would clear her tasks
            Vector3 at = Land(s.At);
            Call(SET_COORDS, lucia, at.X, at.Y, at.Z, false, false, false, false);
            Call(SET_HEADING, lucia, s.Heading);
            Call(FREEZE, lucia, false);
            Call(SCENARIO_IN_PLACE, lucia, s.Scenario, 0, true);
            if (handedOver) _sceneActive = true;
            Log("Lucia landed at " + s.Name + (handedOver ? "" : " (not the player: the switch didn't get to her)"));
            _luciaHeld = false;
        }

        // The ground right under the scene's spot, once the area has streamed in. The spots are the game's own
        // scenario points, already at ground height, so the probe starts only half a metre up (from higher it
        // finds awnings and roofs first) and only a small correction is taken; anything else keeps the point.
        static Vector3 Land(Vector3 at)
        {
            var z = new OutputArgument();
            if (Call<bool>(GROUND_Z, at.X, at.Y, at.Z + 0.5f, z, false, false))
            {
                float g = z.GetResult<float>();
                if (Math.Abs(g - at.Z) < 1.5f) return new Vector3(at.X, at.Y, g);
            }
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
            if (_jason != 0 && model == _jason) return "Jason";
            int i = Array.IndexOf(StoryModels, (uint)model);
            return i >= 0 ? StoryNames[i] : "model 0x" + ((uint)model).ToString("X8");
        }
    }
}
