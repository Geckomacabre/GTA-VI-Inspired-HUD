using System;
using System.Collections.Generic;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// What the extras can see of FBICat's HUD and minimap, read the same way
    /// GTA6Hud itself talks to ViMinimap: public statics, found by reflection,
    /// so nothing here needs either DLL to build or to be installed.
    /// </summary>
    internal static class HudBridge
    {
        private static bool _looked;
        private static FieldInfo _menuOpen;      // ViMinimapBridge.HudMenuOpen
        private static FieldInfo _viPauseOpen;   // GTA6Hud.GTA6Hud.ViPauseOpen
        private static FieldInfo _wheelControl;  // GTA6Hud.Ref.WheelVehicleOpenControl ([Wheel] VehicleOpenControl)
        private static int _wheelHolds;
        private static int _wheelControlWas;
        private static int _minimapHolds;
        private static bool _minimapWasOpen;

        private static void Look()
        {
            if (_looked) return;
            _looked = true;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type bridge = asm.GetType("ViMinimapBridge", false);
                    if (bridge != null) _menuOpen = bridge.GetField("HudMenuOpen", BindingFlags.Public | BindingFlags.Static);
                    Type hud = asm.GetType("GTA6Hud.GTA6Hud", false);
                    if (hud != null) _viPauseOpen = hud.GetField("ViPauseOpen", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                    Type refs = asm.GetType("GTA6Hud.Ref", false);
                    if (refs != null) _wheelControl = refs.GetField("WheelVehicleOpenControl", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                }
                catch { }
            }
        }

        /// <summary>
        /// Hides the 3D minimap while held. ViMinimap hides itself whenever this
        /// flag is up (GTA6Hud raises it for its own shops and cameras), and
        /// GTA6Hud only writes it when its own menus open or close.
        /// </summary>
        public static void HoldMinimapHidden(bool hold)
        {
            Look();
            if (_menuOpen == null) return;
            try
            {
                if (hold)
                {
                    if (_minimapHolds++ == 0)
                    {
                        _minimapWasOpen = (bool)_menuOpen.GetValue(null);
                        _menuOpen.SetValue(null, true);
                    }
                }
                else if (_minimapHolds > 0 && --_minimapHolds == 0 && !_minimapWasOpen)
                {
                    _menuOpen.SetValue(null, false);
                }
            }
            catch { }
        }

        /// <summary>
        /// Keeps GTA6Hud's in-vehicle weapon wheel off its pad / arrow button
        /// ([Wheel] VehicleOpenControl, 190: Right arrow and D-pad Right) while
        /// held. GTA6Hud reads that button even when it is disabled, and only stood
        /// down for its own radio wheel; while the radio wheel here is open the same
        /// button steps the station. The setting is pointed at F1 (Rockstar
        /// Editor recording, 288) for as long as the hold lasts, and put back after.
        /// Tab still opens GTA6Hud's wheel on a keyboard. Reference-counted.
        /// </summary>
        public static void HoldVehicleWheelButton(bool hold)
        {
            Look();
            if (_wheelControl == null) return;
            try
            {
                if (hold)
                {
                    if (_wheelHolds++ == 0)
                    {
                        _wheelControlWas = (int)_wheelControl.GetValue(null);
                        _wheelControl.SetValue(null, 288);
                    }
                }
                else if (_wheelHolds > 0 && --_wheelHolds == 0)
                {
                    _wheelControl.SetValue(null, _wheelControlWas);
                }
            }
            catch { }
        }

        // GTA6Hud's own switches (Ref, read every frame) for the panels that stay up
        // during a cutaway: the vehicle card with its engine / fuel / waypoint
        // gauges, the area name plate, and the arrows to nearby police.
        private static readonly string[] PanelSwitches = { "VehicleCardEnabled", "LocationEnabled", "PoliceArrowEnabled" };
        private static FieldInfo[] _panelFields;
        private static bool[] _panelWas;
        private static int[] _switchHolds;
        private static int _panelHolds;

        private static void LookPanels()
        {
            if (_panelFields != null) return;
            Type refs = null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                if ((refs = asm.GetType("GTA6Hud.Ref", false)) != null) break;
            _panelFields = new FieldInfo[PanelSwitches.Length];
            _panelWas = new bool[PanelSwitches.Length];
            _switchHolds = new int[PanelSwitches.Length];
            for (int i = 0; i < PanelSwitches.Length; i++)
                _panelFields[i] = refs?.GetField(PanelSwitches[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        }

        // One switch off while held, put back to what it was after the last hold.
        private static void HoldSwitchOff(int i, bool hold)
        {
            FieldInfo f = _panelFields[i];
            if (f == null) return;
            if (hold)
            {
                if (_switchHolds[i]++ == 0)
                {
                    _panelWas[i] = (bool)f.GetValue(null);
                    f.SetValue(null, false);
                }
            }
            else if (_switchHolds[i] > 0 && --_switchHolds[i] == 0)
            {
                // Only if it still reads the false written here: each DLL has its own copy of
                // this bridge, and another one's release (or GTA6Hud's options) may have set it
                // since. Writing back the value seen at the start would switch it off for good.
                if (!(bool)f.GetValue(null)) f.SetValue(null, _panelWas[i]);
            }
        }

        /// <summary>
        /// Takes GTA6Hud's vehicle card (engine, fuel and waypoint gauges), area
        /// plate and police arrows off the screen while held, the way vice_hud's
        /// SetHudVisible(false) clears its HUD for the crash cam and binoculars.
        /// It switches GTA6Hud's own settings off for that long and puts back what
        /// they were; GTA6Hud only sets them at load and from its options menu.
        /// Reference-counted.
        /// </summary>
        public static void HoldGta6HudPanelsHidden(bool hold)
        {
            try
            {
                LookPanels();
                if (hold) { if (_panelHolds++ != 0) return; }
                else if (_panelHolds == 0 || --_panelHolds != 0) return;
                for (int i = 0; i < PanelSwitches.Length; i++) HoldSwitchOff(i, hold);
            }
            catch { }
        }

        // GTA6Hud's character swap wheel opens on Left Alt / d-pad down
        // (INPUT_CHARACTER_WHEEL) wherever you are, read even while the control is
        // disabled: the key the FiveM nitrous holds for a boost, and the binoculars'
        // zoom out.
        private static FieldInfo _swapField;
        private static bool _swapLooked, _swapWas, _swapHeld;
        private static readonly HashSet<string> _swapHolders = new HashSet<string>();

        /// <summary>
        /// Keeps GTA6Hud's character swap wheel shut while anyone holds it: its
        /// Ref.SwapEnabled off, set again every frame (its settings reload turns it
        /// back on), and put back to what it was when the last holder lets go.
        /// </summary>
        public static void HoldSwapWheelOff(string who, bool hold)
        {
            try
            {
                if (!_swapLooked)
                {
                    _swapLooked = true;
                    Type refs = null;
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                        if ((refs = asm.GetType("GTA6Hud.Ref", false)) != null) break;
                    _swapField = refs?.GetField("SwapEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                }
                if (_swapField == null) return;
                if (hold) _swapHolders.Add(who);
                else _swapHolders.Remove(who);
                if (_swapHolders.Count > 0)
                {
                    if (!_swapHeld) { _swapWas = (bool)_swapField.GetValue(null); _swapHeld = true; }
                    if ((bool)_swapField.GetValue(null)) _swapField.SetValue(null, false);
                }
                else if (_swapHeld)
                {
                    _swapField.SetValue(null, _swapWas);
                    _swapHeld = false;
                }
            }
            catch { }
        }

        /// <summary>A cutaway (crash cam, binoculars) has GTA6Hud's panels hidden.</summary>
        public static bool Gta6HudPanelsHeld => _panelHolds > 0;

        /// <summary>
        /// Switches GTA6Hud's own vehicle card off while held, for a script that
        /// draws the card itself (Gta6HudCard). Reference-counted with the above.
        /// </summary>
        public static void HoldVehicleCardOff(bool hold)
        {
            try { LookPanels(); HoldSwitchOff(0, hold); }
            catch { }
        }

        /// <summary>
        /// GTA6Hud switches its card back on itself when it (re)loads its assets
        /// or settings (EnsureAssets, Ref.Load). While held off, put it back off:
        /// that load is its setting being on. True if it had to.
        /// </summary>
        public static bool ReassertVehicleCardOff()
        {
            try
            {
                LookPanels();
                FieldInfo f = _panelFields[0];
                if (f == null || _switchHolds[0] == 0 || !(bool)f.GetValue(null)) return false;
                _panelWas[0] = true;
                f.SetValue(null, false);
                return true;
            }
            catch { return false; }
        }

        /// <summary>Whether the player has GTA6Hud's vehicle card switched on, under any hold.</summary>
        public static bool VehicleCardWanted
        {
            get
            {
                try
                {
                    LookPanels();
                    if (_switchHolds[0] > 0) return _panelWas[0];
                    return _panelFields[0] == null || (bool)_panelFields[0].GetValue(null);
                }
                catch { return true; }
            }
        }

        // GTA6Hud itself, for where it lays out its vehicle card. SHVDN keeps
        // every running script in SHVDN.ScriptDomain.CurrentDomain.RunningScripts.
        private static object _hud;
        private static long _hudLookAt;
        private static MethodInfo _stackLeft, _stackWidth, _cardBottom;
        private static FieldInfo _cardTop;
        private static bool _hudLogged;

        /// <summary>GTA6Hud's running script, or null when it is not (yet) found.</summary>
        public static object Gta6Hud()
        {
            if (_hud != null || RealTime.Ms < _hudLookAt) return _hud;
            _hudLookAt = RealTime.Ms + 5000;
            try
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type domain = asm.GetType("SHVDN.ScriptDomain", false);
                    if (domain == null) continue;
                    object current = domain.GetProperty("CurrentDomain", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
                    if (!(domain.GetProperty("RunningScripts")?.GetValue(current) is System.Collections.IEnumerable scripts)) continue;
                    foreach (object s in scripts)
                    {
                        object inst = s?.GetType().GetProperty("ScriptInstance")?.GetValue(s);
                        if (inst == null || inst.GetType().FullName != "GTA6Hud.GTA6Hud") continue;
                        const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                        Type t = inst.GetType();
                        _stackLeft = t.GetMethod("KitStackLeft", Own, null, Type.EmptyTypes, null);
                        _stackWidth = t.GetMethod("KitStackWidth", Own, null, Type.EmptyTypes, null);
                        _cardBottom = t.GetMethod("KitCardBottom", Own, null, Type.EmptyTypes, null);
                        _cardTop = t.GetField("_vehCardTop", Own);
                        if (_stackLeft != null && _stackWidth != null && _cardBottom != null && _cardTop != null) _hud = inst;
                    }
                }
            }
            catch (Exception ex) { if (!_hudLogged) { _hudLogged = true; Log.Write("HudBridge", "GTA6Hud's card: " + ex.Message); } }
            if (_hud == null && !_hudLogged) { _hudLogged = true; Log.Write("HudBridge", "GTA6Hud’s vehicle card layout was not found; the nitrous pip stays off"); }
            return _hud;
        }

        /// <summary>
        /// Where GTA6Hud put its vehicle card's gauge row (DrawVehicleCard /
        /// DrawVehicleGauges): the row's centre across, its gauges' centre line,
        /// and the left edge of its map and card column. False while the card is
        /// not on screen; `left` is still filled in when GTA6Hud was found.
        /// </summary>
        public static bool VehicleGaugeRow(out float centreX, out float centreY, out float left)
        {
            centreX = centreY = left = 0f;
            object hud = Gta6Hud();
            if (hud == null) return false;
            try
            {
                left = (float)_stackLeft.Invoke(hud, null);
                centreX = left + (float)_stackWidth.Invoke(hud, null) * 0.5f;
                centreY = (float)_cardBottom.Invoke(hud, null) - 18.35f;
                // 0 while the card is off: DrawVehicleCard clears it first thing.
                return (float)_cardTop.GetValue(hud) > 0f;
            }
            catch { return false; }
        }

        // GTA6Hud's own menus that take the keys while they are up: its weapon /
        // item wheel, settings menu (held camera button), trunk and shop.
        private static readonly string[] UiFlags = { "_wheelOpen", "_settingsOpen", "_trunkOpen", "_shopOpen" };
        private static FieldInfo[] _uiFields;

        /// <summary>One of GTA6Hud's wheel, settings, trunk or shop menus is open.</summary>
        public static bool Gta6HudUiOpen
        {
            get
            {
                object hud = Gta6Hud();
                if (hud == null) return false;
                try
                {
                    if (_uiFields == null)
                    {
                        _uiFields = new FieldInfo[UiFlags.Length];
                        for (int i = 0; i < UiFlags.Length; i++)
                            _uiFields[i] = hud.GetType().GetField(UiFlags[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    }
                    foreach (FieldInfo f in _uiFields)
                        if (f != null && (bool)f.GetValue(hud)) return true;
                }
                catch { }
                return false;
            }
        }

        /// <summary>
        /// Something else has the keys: a pause menu, a cutscene / switch / black
        /// screen, a GTA6Hud shop, camera or menu, the phone, or one of the 2.5
        /// panels (skills, the honor layout editor). A key of ours pressed now
        /// belongs to that, not to us.
        /// </summary>
        public static bool UiInTheWay => InTheWay() != null;

        /// <summary>What has the keys (for the logs), or null when nothing does.</summary>
        public static string InTheWay()
        {
            if (Paused) return "a pause menu";
            if (Busy) return "a cutscene, character switch or black screen";
            if (HudMenuOpen) return "a GTA6Hud shop or camera";
            if (Gta6HudUiOpen) return "a GTA6Hud wheel or menu";
#if !VICUSTOMS && !VICROUCH
            if (SkillsApi.PanelOpen) return "the skills panel";
            if (Honor.LayoutEditorOpen) return "the honor layout editor";
#endif
            // The phone in the player's hand. (CAN_PHONE_BE_SEEN_ON_SCREEN reads true
            // with it put away, so it is not asked.)
            if (Function.Call<bool>(Hash.IS_PED_RUNNING_MOBILE_PHONE_TASK, Game.Player.Character)) return "the phone";
            return null;
        }

        /// <summary>True while the VI pause menu, or a GTA6Hud shop or camera, is up.</summary>
        public static bool HudMenuOpen
        {
            get
            {
                if (ViPauseOpen) return true;
                try { return _menuOpen != null && _minimapHolds == 0 && (bool)_menuOpen.GetValue(null); }
                catch { return false; }
            }
        }

        /// <summary>The VI pause menu is open (or has handed off to one of the game's screens).</summary>
        public static bool ViPauseOpen
        {
            get
            {
                Look();
                try { return _viPauseOpen != null && (bool)_viPauseOpen.GetValue(null); }
                catch { return false; }
            }
        }

        /// <summary>The game's pause menu, or the VI one, is open.</summary>
        public static bool Paused => Game.IsPaused || Function.Call<bool>(Hash.IS_PAUSE_MENU_ACTIVE) || ViPauseOpen;

        /// <summary>A cutscene, a character switch or a black screen: nothing of ours should show.</summary>
        public static bool Busy =>
            Function.Call<bool>(Hash.IS_CUTSCENE_PLAYING) ||
            Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS) ||
            Function.Call<bool>(Hash.IS_SCREEN_FADED_OUT);
    }
}
