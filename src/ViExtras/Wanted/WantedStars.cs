using System;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's wanted stars in vice_hud's four states (client.lua wantedState,
    /// style.css .star.contact / .searching / .hollow / .red), the reference's:
    ///   white     the police can see you right now
    ///   flashing  they lost sight of you but are still chasing: hollow and white
    ///             in turn, 900 ms a cycle (@keyframes starHunt)
    ///   hollow    a crime was reported, nobody has seen you for it yet
    ///   red       you have shaken them off, but are still in the search zone
    ///
    /// GTA6Hud draws white, hollow and red already, but turns red the moment no
    /// cop can see you. vice_hud reads fenix-police for "still chasing"; single
    /// player has the game's own: until the game starts its hidden evasion (the
    /// search, its stars greyed in plain GTA) the chase is on and the stars
    /// flash, and only then go red. GTA6Hud's own line-of-sight check
    /// (PoliceCanSee) and "seen this chase" decide white and hollow; its
    /// _starState is set from here every frame, after its own update.
    /// </summary>
    public sealed class WantedStars : Script
    {
        // HUD OPTIONS (HudMenu) changes this at once; read from ViExtras.ini at start
        internal static bool On;
        private readonly int _flashMs;

        private bool _looked;
        private object _hud;
        private FieldInfo _state, _seen;
        private MethodInfo _canSee;
        private object _white, _red, _hollow;

        private long _nextScan;
        private bool _contact;

        // [WantedStars] Debug: what GTA6Hud's "cops are searching for you" box
        // (DrawPoliceSearchTip) hangs on, logged whenever it changes.
        private readonly bool _debug;
        private FieldInfo _policeMem, _tipActive, _tipTime;
        private PropertyInfo _hasAnything;
        private bool _wasLost, _wasHas, _wasTip;
        private long _lastFlip;

        /// <summary>vice_hud's wanted state now: contact, searching, hollow or red; null when not wanted.</summary>
        public static string State { get; private set; }

        public WantedStars()
        {
            Paths.Init(BaseDirectory);
            On = Config.Get("WantedStars", "Enabled", true);
            _flashMs = Math.Max(100, Config.Get("WantedStars", "FlashMs", 900));
            _debug = Config.Get("WantedStars", "Debug", false);
            Tick += OnTick;
        }

        private bool Look()
        {
            if (_hud != null) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            try
            {
                const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                Type t = hud.GetType();
                _state = t.GetField("_starState", Own);
                _seen = t.GetField("_seenThisChase", Own);
                _canSee = t.GetMethod("PoliceCanSee", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (_state == null || _seen == null || _canSee == null || !_state.FieldType.IsEnum)
                {
                    Log.Write("WantedStars", "GTA6Hud’s wanted stars were not found; left as they are");
                    return false;
                }
                _white = Enum.Parse(_state.FieldType, "White");
                _red = Enum.Parse(_state.FieldType, "Red");
                _hollow = Enum.Parse(_state.FieldType, "Hollow");
                _hud = hud;
                _policeMem = t.GetField("_police", Own);
                _hasAnything = _policeMem?.FieldType.GetProperty("HasAnything", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                _tipActive = t.GetField("_searchTipActive", Own);
                _tipTime = t.GetField("_searchTipTime", Own);
            }
            catch (Exception ex) { Log.Write("WantedStars", ex.Message); }
            return _hud != null;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) { State = null; return; }
            if (!On || _broken || Game.Player.WantedLevel <= 0 || !Look()) { State = null; return; }
            try
            {
                long now = RealTime.Ms;
                Ped ped = Game.Player.Character;
                // as often as GTA6Hud looks (UpdateStarState, every 300 ms)
                if (now >= _nextScan)
                {
                    _nextScan = now + 300;
                    _contact = (bool)_canSee.Invoke(null, new object[] { ped });
                }
                object state;
                if (_contact) { state = _white; State = "contact"; }
                else if (!(bool)_seen.GetValue(_hud)) { state = _hollow; State = "hollow"; }
                else if (Evading()) { state = _red; State = "red"; }
                else { state = now % _flashMs < _flashMs / 2 ? _hollow : _white; State = "searching"; }   // starHunt: hollow, then white
                _state.SetValue(_hud, state);
                if (_debug) Watch(now);
            }
            catch (Exception ex)
            {
                // GTA6Hud's stars are left to it from here on
                Log.Write("WantedStars", ex.Message);
                _broken = true;
            }
        }

        private bool _broken;

        private void Watch(long now)
        {
            bool lost = Game.Player.Wanted.TimeHiddenEvasionStarted > 0;
            bool has = _hasAnything != null && (bool)_hasAnything.GetValue(_policeMem.GetValue(_hud));
            bool tip = _tipTime != null && (float)_tipTime.GetValue(_hud) > 0f && (bool)_tipActive.GetValue(_hud);
            if (lost == _wasLost && has == _wasHas && tip == _wasTip) return;
            Log.Write("WantedStars", "+" + (now - _lastFlip) + " ms: lostSight=" + lost + " (evasion " + Game.Player.Wanted.TimeHiddenEvasionStarted
                + ") hasDescription=" + has + " boxShowing=" + tip + " contact=" + _contact + " greyed=" + Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player));
            _wasLost = lost; _wasHas = has; _wasTip = tip; _lastFlip = now;
        }

        /// <summary>
        /// The game has started searching instead of chasing: its hidden evasion
        /// has run unbroken for a moment. In a close chase it restarts every frame
        /// or two as a cop catches a glimpse of you; that is still the chase.
        /// </summary>
        private static bool Evading()
        {
            try
            {
                int started = Game.Player.Wanted.TimeHiddenEvasionStarted;
                if (started > 0 && Game.GameTime - started >= 1500) return true;
            }
            catch { }
            return Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player);
        }
    }
}
