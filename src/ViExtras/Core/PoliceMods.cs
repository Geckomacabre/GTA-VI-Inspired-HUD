using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Other mods that run the police, and what the HUD leaves to them ([Compat] in ViExtras.ini).
    ///
    /// Rebalanced Dispatch Enhanced: its SixStarResponse script (Script Hook V .NET, like ours) sends and runs the
    /// response, up to ten stars. GTA6Hud's police brain (a role for each cop, re-tasked) and PoliceForce (stun guns,
    /// arrests) would task its units too, and GTA6Hud's six stars would call in a military of its own, so those stay
    /// out while it runs. The stars, the wanted box and the crimes stay: RDE answers the game's own wanted level. Past
    /// five stars its level is SixStarResponse.GetActualWantedLevel(), and GTA6Hud's sixth star is lit from 6 up.
    ///
    /// Los Santos RED: a RAGE Plugin Hook plugin, in an AppDomain of its own, so it's known by its file and by RAGE
    /// Plugin Hook's log saying it loaded since the game started. It runs the police, the crimes and the wanted level
    /// (the game's own stays at 0, and it draws stars of its own), so the HUD adds none: no crimes or witnesses
    /// (Memory, Law), no police brain, police memory, heat or six stars, no PoliceForce.
    ///
    /// GTA6Hud's switches (its Ref) are set while one of them runs and put back as they were when it doesn't.
    /// </summary>
    public sealed class PoliceMods : Script
    {
        private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const int LevelMs = 250, RdeLookMs = 5000, LsrLookMs = 3000;
        private const string LsrLoaded = "Loaded plugin \"Los Santos RED\"", LsrName = "\"Los Santos RED\"";
        private const string NewLog = "Started new log on";

        /// <summary>Rebalanced Dispatch Enhanced's SixStarResponse script is running.</summary>
        public static bool Rde { get; private set; }

        /// <summary>RDE's wanted level, 0 to 10 (0 without RDE).</summary>
        public static int RdeLevel { get; private set; }

        /// <summary>Los Santos RED is loaded in RAGE Plugin Hook this session.</summary>
        public static bool Lsr { get; private set; }

        /// <summary>Another mod runs the police: the HUD's own police AI stays out.</summary>
        public static bool TakesPolice => Rde || Lsr;

        private readonly bool _rdeOn, _lsrOn;
        private readonly string _game;
        private readonly bool _lsrInstalled;
        private readonly DateTime _started;

        private MethodInfo _rdeLevel;
        private int _rdeLookAt, _levelAt, _lsrLookAt;
        private long _logPos;
        private bool _logLsr;

        // GTA6Hud's switches, and what they were before they were changed here
        private bool _looked;
        private object _hud;
        private FieldInfo _sixStar;
        private readonly Dictionary<FieldInfo, object> _saved = new Dictionary<FieldInfo, object>();
        private Type _refs;
        private string _applied = "";

        public PoliceMods()
        {
            Paths.Init(BaseDirectory);
            _rdeOn = Config.Get("Compat", "Rde", true);
            _lsrOn = Config.Get("Compat", "Lsr", true);
            try
            {
                Process me = Process.GetCurrentProcess();
                _game = Path.GetDirectoryName(me.MainModule.FileName);
                _started = me.StartTime;
                _lsrInstalled = File.Exists(Path.Combine(_game, "plugins", "Los Santos RED.dll"));
            }
            catch (Exception ex) { Log.Write("PoliceMods", "game folder: " + ex.Message); }
            if (!_rdeOn && !_lsrOn) return;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                int now = Environment.TickCount;
                if (_rdeOn) WatchRde(now);
                if (_lsrOn && _lsrInstalled && now - _lsrLookAt >= LsrLookMs) { _lsrLookAt = now; WatchLsr(); }
                Apply();
            }
            catch (Exception ex)
            {
                Log.Write("PoliceMods", ex.Message);
            }
        }

        // ---- Rebalanced Dispatch Enhanced ----

        private void WatchRde(int now)
        {
            if (!Rde)
            {
                if (now - _rdeLookAt < RdeLookMs && _rdeLookAt != 0) return;
                _rdeLookAt = now;
                object script = RunningScript("SixStarResponse");
                if (script == null) return;
                Rde = true;
                _rdeLevel = script.GetType().GetMethod("GetActualWantedLevel", Static, null, Type.EmptyTypes, null);
                if (_rdeLevel != null && _rdeLevel.ReturnType != typeof(int)) _rdeLevel = null;
                Log.Write("PoliceMods", _rdeLevel != null
                    ? "Rebalanced Dispatch Enhanced is running: its police take over, and the stars go to six"
                    : "Rebalanced Dispatch Enhanced is running: its police take over (its wanted level was not found, so the stars stop at five)");
            }
            if (now - _levelAt < LevelMs) return;
            _levelAt = now;
            int level = Game.Player.WantedLevel;
            if (_rdeLevel != null) level = Math.Max(level, (int)_rdeLevel.Invoke(null, null));
            RdeLevel = Math.Max(level, Function.Call<int>(Hash.GET_FAKE_WANTED_LEVEL));
        }

        // SHVDN's running script of this type, or null
        private static object RunningScript(string fullName)
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
                    if (inst != null && inst.GetType().FullName == fullName) return inst;
                }
            }
            return null;
        }

        // ---- Los Santos RED ----

        // RagePluginHook.log, read on from where it was left: a new log starts afresh, LSR loading and unloading turn it
        // on and off. A log last written before the game started is another session's (RAGE Plugin Hook loads its
        // plugins once the game has loaded, so this session's says so after the start).
        private void WatchLsr()
        {
            string path = Path.Combine(_game, "RagePluginHook.log");
            var info = new FileInfo(path);
            bool fresh = info.Exists && info.LastWriteTime >= _started;
            if (!fresh)
            {
                SetLsr(false);
                _logPos = 0;
                return;
            }
            if (info.Length < _logPos) { _logPos = 0; _logLsr = false; }
            if (info.Length == _logPos) { SetLsr(_logLsr); return; }
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                fs.Seek(_logPos, SeekOrigin.Begin);
                var bytes = new byte[fs.Length - _logPos];
                int read = fs.Read(bytes, 0, bytes.Length);
                // up to the last whole line; the rest is read next time
                int end = Array.LastIndexOf(bytes, (byte)'\n', Math.Max(0, read - 1));
                if (end < 0) return;
                _logPos += end + 1;
                foreach (string line in Encoding.UTF8.GetString(bytes, 0, end + 1).Split('\n'))
                {
                    if (line.Contains(NewLog)) _logLsr = false;
                    else if (line.Contains(LsrLoaded)) _logLsr = true;
                    else if (line.Contains(LsrName) && line.IndexOf("nload", StringComparison.OrdinalIgnoreCase) >= 0) _logLsr = false;
                }
            }
            SetLsr(_logLsr);
        }

        private static void SetLsr(bool on)
        {
            if (on == Lsr) return;
            Lsr = on;
            Log.Write("PoliceMods", on ? "Los Santos RED is running: it has the police, crimes and wanted level, and the HUD adds none"
                                       : "Los Santos RED is not running");
        }

        // ---- GTA6Hud's switches ----

        private void Apply()
        {
            string want = Lsr ? "lsr" : Rde ? "rde" : "";
            if (want == _applied && want != "rde") return;
            if (!Look()) return;
            if (want != _applied)
            {
                Restore();
                if (want == "lsr")
                {
                    Set("PoliceBrainEnabled", false);
                    Set("PoliceMemoryEnabled", false);
                    Set("HeatEnabled", false);
                    Set("SixStarEnabled", false);
                }
                else if (want == "rde")
                {
                    // GTA6Hud's six stars stay on, for the sixth star, but never start themselves or call in a military
                    Set("PoliceBrainEnabled", false);
                    Set("SixStarAfterSeconds", 1000000);
                    Set("SixStarHelis", 0);
                    Set("SixStarGroundUnits", 0);
                    Set("SixStarTank", false);
                }
                _applied = want;
            }
            if (want == "rde" && _sixStar != null)
            {
                bool six = RdeLevel >= 6 && Game.Player.WantedLevel >= 5;
                if ((bool)_sixStar.GetValue(_hud) != six) _sixStar.SetValue(_hud, six);
            }
        }

        private bool Look()
        {
            if (_hud != null) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            _refs = hud.GetType().Assembly.GetType("GTA6Hud.Ref");
            _sixStar = hud.GetType().GetField("_sixStar", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (_sixStar != null && _sixStar.FieldType != typeof(bool)) _sixStar = null;
            if (_refs == null) Log.Write("PoliceMods", "GTA6Hud's settings were not found; its police stay as they are");
            if (_sixStar == null) Log.Write("PoliceMods", "GTA6Hud's sixth star was not found; RDE's stars stop at five");
            _hud = hud;
            return true;
        }

        private void Set(string name, object value)
        {
            FieldInfo f = _refs?.GetField(name, Static);
            if (f == null || f.FieldType != value.GetType()) { Log.Write("PoliceMods", "GTA6Hud's " + name + " was not found"); return; }
            if (!_saved.ContainsKey(f)) _saved[f] = f.GetValue(null);
            f.SetValue(null, value);
        }

        private void Restore()
        {
            foreach (KeyValuePair<FieldInfo, object> s in _saved) s.Key.SetValue(null, s.Value);
            _saved.Clear();
            if (_applied == "rde" && _sixStar != null && _hud != null) _sixStar.SetValue(_hud, false);
        }
    }
}
