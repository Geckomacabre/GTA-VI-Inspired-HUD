using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GTA;

namespace ViExtras
{
    /// <summary>Where the extras live: scripts\ViExtras.ini and scripts\ViExtras\.</summary>
    internal static class Paths
    {
        public static string Scripts { get; private set; }
        public static string Root => Path.Combine(Scripts, "ViExtras");
        public static string Art(string name) => Path.Combine(Root, "Art", name);
        private static string _saveDir;
        private static long _saveRetryAt;

        public static string Save(string name)
        {
            // made once a session (SaveFile.Write makes it again if it goes meanwhile).
            // A game folder Windows won't let us write to (Program Files, until GameFolder
            // fixes it) can't have it yet: the path is the same either way, and it's asked
            // for again every few seconds rather than failing whoever wanted the path.
            string dir = _saveDir ?? Path.Combine(Root, "Save");
            if (_saveDir == null && RealTime.Ms >= _saveRetryAt)
            {
                try
                {
                    Directory.CreateDirectory(dir);
                    _saveDir = dir;
                }
                catch (Exception ex)
                {
                    _saveRetryAt = RealTime.Ms + 5000;
                    Log.Write(Name, "can't make " + dir + " yet: " + ex.Message);
                }
            }
            return Path.Combine(dir, name);
        }
#if VICUSTOMS
        // ViCustoms.dll (the mod shop) and ViCrouch.dll (crouch and crawl), GPL-3,
        // keep their own settings and logs.
        public const string Name = "ViCustoms";
#elif VICROUCH
        public const string Name = "ViCrouch";
#else
        public const string Name = "ViExtras";
#endif
        public static string Ini => Path.Combine(Scripts, Name + ".ini");

        public static void Init(string scriptsDir)
        {
            if (Scripts == null)
            {
                Scripts = scriptsDir;
                Log.Write(Name, Banner());
            }
            EnhancedPlayerInfo.TurnOff();
        }

        // The first line of every session in the log: which build this is (version and the
        // time it was built), on which game, under which Script Hook V .NET. What to ask a
        // player for when something goes wrong.
        private static string Banner()
        {
            string me = "?", game = "?", hook = "?";
            try
            {
                var asm = typeof(Paths).Assembly;
                var info = (System.Reflection.AssemblyInformationalVersionAttribute)Attribute.GetCustomAttribute(asm, typeof(System.Reflection.AssemblyInformationalVersionAttribute));
                me = info?.InformationalVersion ?? asm.GetName().Version.ToString();
                // "2.5.4+<build time>.<commit>": the commit cut to its short form
                int plus = me.IndexOf('+'), dot = plus < 0 ? -1 : me.IndexOf('.', plus);
                if (dot > 0 && me.Length > dot + 8) me = me.Substring(0, dot + 8);
            }
            catch { }
            try
            {
                Process p = Process.GetCurrentProcess();
                game = (string.Equals(p.ProcessName, "GTA5_Enhanced", StringComparison.OrdinalIgnoreCase) ? "GTA V Enhanced " : "GTA V Legacy ")
                       + (p.MainModule?.FileVersionInfo.FileVersion ?? "");
            }
            catch { }
            try { hook = typeof(Script).Assembly.GetName().Name + " " + typeof(Script).Assembly.GetName().Version; } catch { }
            return "==== " + Name + " " + me + " on " + game.Trim() + " (" + hook + ") ====";
        }
    }

    /// <summary>
    /// On GTA V Enhanced (1.0.1158.16), Script Hook V .NET Enhanced 1.1.0.6 finds the
    /// player's CPlayerInfo at an offset into the player ped that is wrong there
    /// (0x10A0D7, far past the end of a ped), so every read built on it follows a
    /// junk pointer: Player.Wanted's timers and flags, and Player.IgnoredByPolice,
    /// which GTA6Hud's Heat, PoliceBrain and Detention read every frame. The game
    /// dies whenever the junk isn't mapped. The offset is public and every reader
    /// checks it for 0 first, so 0 makes them return their defaults instead.
    /// Shared by the whole script domain (ViSwitch does the same); set from the
    /// first script constructor, before anything ticks.
    /// </summary>
    internal static class EnhancedPlayerInfo
    {
        private static bool _done;

        public static void TurnOff()
        {
            if (_done) return;
            _done = true;
            try
            {
                if (!string.Equals(Process.GetCurrentProcess().ProcessName, "GTA5_Enhanced", StringComparison.OrdinalIgnoreCase)) return;
                foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Type t = asm.GetType("SHVDN.NativeMemory", false);
                    System.Reflection.PropertyInfo p = t?.GetProperty("PedPlayerInfoOffset",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (p == null || !p.CanWrite) continue;
                    int was = (int)p.GetValue(null);
                    if (was == 0) return;   // already off (another script got here first)
                    p.SetValue(null, 0);
                    Log.Write("Enhanced", "player info memory reads switched off (PedPlayerInfoOffset 0x" + was.ToString("X") + " -> 0)");
                    return;
                }
            }
            catch (Exception ex) { Log.Write("Enhanced", "player info reads: " + ex.Message); }
        }
    }

    /// <summary>
    /// Real time. Slow motion stretches the game timer, so anything that has to
    /// last a set number of real milliseconds (every cinematic here) uses this.
    /// </summary>
    internal static class RealTime
    {
        private static readonly Stopwatch Clock = Stopwatch.StartNew();
        public static long Ms => Clock.ElapsedMilliseconds;
    }

    /// <summary>
    /// scripts\ViExtras.ini, read once and shared by every script.
    ///
    /// The file opens with [Switches]: one plain True / False line per feature, for
    /// players who only want to turn things on and off. Each switch stands in for
    /// the setting it names in the sections below (Crosshair for [Crosshair]
    /// Enabled), which is only read when the switch isn't there, so an older file
    /// without [Switches] reads as it always did. Writing one of those settings
    /// (HUD OPTIONS) writes its switch.
    /// </summary>
    internal static class Config
    {
        public const string Switches = "Switches";

        // [section/key] -> its switch in [Switches]
        private static readonly System.Collections.Generic.Dictionary<string, string> SwitchFor =
            new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Crosshair/Enabled"] = "Crosshair",
            ["Crosshair/KillMarks"] = "KillMarks",
            ["WantedStars/Enabled"] = "WantedStars",
            ["WantedBox/Enabled"] = "WantedBox",
            ["Minimap/FeedBesideMap"] = "NotificationsBesideMap",
            ["Switcher/Enabled"] = "CharacterCards",
            ["CrashCam/Enabled"] = "CrashCamera",
            ["WeightedCam/Enabled"] = "CameraWeight",
            ["PoliceLights/Enabled"] = "PoliceLights",
            ["PoliceLights/Arrow"] = "PoliceArrow",
            ["PoliceForce/Enabled"] = "PoliceArrests",
            ["PoliceForce/StunGuns"] = "StunGuns",
            ["CarWeapon/Enabled"] = "KeepYourGunInCars",
            ["Memory/Witnesses"] = "Witnesses",
            ["WantedBox/IndoorCameras"] = "IndoorCameras",
            ["Memory/Enabled"] = "PeopleRemember",
            ["Memory/Interact"] = "GreetAndAntagonize",
            ["Memory/NpcSpeech"] = "PeopleTalk",
            ["Memory/PlayerSpeech"] = "YouTalk",
            ["Honor/Enabled"] = "Honor",
            ["Honor/NpcReactions"] = "PeopleFearPsychos",
            ["Honor/PetDogs"] = "PetDogs",
            ["Skills/Enabled"] = "Skills",
            ["Gym/Enabled"] = "Gym",
            ["Focus/Enabled"] = "Focus",
            ["Nitrous/Enabled"] = "Nitrous",
            ["Binoculars/Enabled"] = "Binoculars",
            ["Radio/Enabled"] = "RadioWheel",
            ["Radio/CustomMusic"] = "YourOwnMusic",
            ["Vangelico/Enabled"] = "VangelicoOpen",
        };

        private static ScriptSettings _ini;

        private static ScriptSettings Ini
        {
            get
            {
                if (_ini == null) _ini = ScriptSettings.Load(Paths.Ini);
                return _ini;
            }
        }

        private static bool Has(string section, string key)
        {
            try { return Ini.ContainsKey(section, key); }
            catch { return false; }
        }

        // The [Switches] line standing in for section/key, when the file has it.
        private static string SwitchOf(string section, string key) =>
            SwitchFor.TryGetValue(section + "/" + key, out string name) && Has(Switches, name) ? name : null;

        public static T Get<T>(string section, string key, T fallback)
        {
            try
            {
                string sw = SwitchOf(section, key);
                return sw != null ? Ini.GetValue(Switches, sw, fallback) : Ini.GetValue(section, key, fallback);
            }
            catch { return fallback; }
        }

        public static void Set<T>(string section, string key, T value)
        {
            try { Ini.SetValue(section, key, value); Ini.Save(); }
            catch { }
        }

        /// <summary>
        /// Writes "key = value" into [section] of ViExtras.ini, changing that one
        /// line (or adding it under the section) and leaving the rest of the file,
        /// comments included, as it was: its line breaks too (Unix or Windows, and
        /// whether it ends with one). ScriptSettings.Save would rewrite the whole
        /// file without its comments.
        /// </summary>
        public static void SetLine(string section, string key, string value)
        {
            string sw = SwitchOf(section, key);
            if (sw != null) { section = Switches; key = sw; }
            try
            {
                string text = File.Exists(Paths.Ini) ? File.ReadAllText(Paths.Ini) : "";
                string nl = text.Contains("\r\n") || text.Length == 0 ? "\r\n" : "\n";
                bool endsWithNl = text.Length == 0 || text.EndsWith("\n");
                var lines = new System.Collections.Generic.List<string>(text.Length == 0 ? new string[0] : text.Replace("\r\n", "\n").Split('\n'));
                if (endsWithNl && lines.Count > 0 && lines[lines.Count - 1].Length == 0) lines.RemoveAt(lines.Count - 1);
                int start = -1, end = lines.Count;
                for (int i = 0; i < lines.Count; i++)
                {
                    string t = lines[i].Trim();
                    if (!t.StartsWith("[")) continue;
                    if (start >= 0) { end = i; break; }
                    if (string.Equals(t, "[" + section + "]", StringComparison.OrdinalIgnoreCase)) start = i;
                }
                string line = key + " = " + value;
                if (start < 0)
                {
                    lines.Add("");
                    lines.Add("[" + section + "]");
                    lines.Add(line);
                }
                else
                {
                    int found = -1;
                    for (int i = start + 1; i < end; i++)
                    {
                        string t = lines[i].TrimStart();
                        int eq = t.IndexOf('=');
                        if (!t.StartsWith(";") && eq > 0 && string.Equals(t.Substring(0, eq).Trim(), key, StringComparison.OrdinalIgnoreCase)) { found = i; break; }
                    }
                    if (found >= 0) lines[found] = line;
                    else
                    {
                        int at = end;
                        while (at > start + 1 && lines[at - 1].Trim().Length == 0) at--;
                        lines.Insert(at, line);
                    }
                }
                File.WriteAllText(Paths.Ini, string.Join(nl, lines) + (endsWithNl ? nl : ""));
                _ini = null;   // read afresh next time
            }
            catch (Exception ex) { Log.Write("Config", "could not write " + section + "/" + key + ": " + ex.Message); }
        }
    }

    /// <summary>
    /// Whether the game is in a state where the scripts can touch the player. Not
    /// while a loading screen is up, there is no player ped, or a character switch
    /// is under way (the game swaps the ped's model then); and not for the first
    /// three seconds after any of that ends, nor after the scripts start or are
    /// reloaded. On Enhanced, a ped native called on a ped the game is still
    /// building or swapping reads freed memory and takes the game down.
    /// </summary>
    internal static class GameReady
    {
        private const int SettleMs = 3000;
        private static int _frame = -1;
        private static bool _now;
        private static long _okSince = -1;

        public static bool Now
        {
            get
            {
                int frame = Game.FrameCount;
                if (frame == _frame) return _now;
                _frame = frame;
                bool ok = !GTA.Native.Function.Call<bool>(GTA.Native.Hash.GET_IS_LOADING_SCREEN_ACTIVE)
                    && GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, GTA.Native.Function.Call<int>(GTA.Native.Hash.PLAYER_PED_ID))
                    && !GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS);
                long now = RealTime.Ms;
                if (!ok) _okSince = -1;
                else if (_okSince < 0) _okSince = now;
                _now = ok && now - _okSince >= SettleMs;
                return _now;
            }
        }
    }

    /// <summary>
    /// A ticker in GTA's feed, through the natives: SHVDN's Notification.PostTicker
    /// (string, bool, bool) is not in Script Hook V .NET Enhanced, and a script
    /// calling it stops there.
    /// </summary>
    internal static class Feed
    {
        public static void Post(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "CELL_EMAIL_BCON");
            for (int i = 0; i < text.Length; i += 99)
                GTA.Native.Function.Call(GTA.Native.Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, text.Substring(i, Math.Min(99, text.Length - i)));
            GTA.Native.Function.Call(GTA.Native.Hash.END_TEXT_COMMAND_THEFEED_POST_TICKER, false, false);
        }
    }

    /// <summary>
    /// Player saves (skills XP, NPC memory): written to a temporary file and then
    /// swapped in, so a crash mid-write can't leave half a file; and a save that
    /// can't be read is set aside under a new name instead of being overwritten.
    /// </summary>
    internal static class SaveFile
    {
        public static void Write(string path, string text)
        {
            string tmp = path + ".tmp";
            try { File.WriteAllText(tmp, text); }
            catch (DirectoryNotFoundException)
            {
                // the Save folder, made once a session (Paths.Save), has gone since
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(tmp, text);
            }
            try
            {
                if (File.Exists(path)) File.Replace(tmp, path, null);
                else File.Move(tmp, path);
            }
            catch (IOException)
            {
                File.Copy(tmp, path, true);
                File.Delete(tmp);
            }
        }

        // Saves made during play (skills XP, honor) are written off the script thread, so
        // no frame waits on the disk: the text is taken on the script thread, and one
        // writer at a time writes it (Write, above), in order. A newer save of a file
        // replaces one of it still waiting; one that fails waits for the next save or Flush.
        private static readonly object Queue = new object(), Writer = new object();
        private static readonly List<string> Order = new List<string>();
        private static readonly Dictionary<string, string[]> Waiting = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        private static bool _draining;

        /// <summary>Writes `text` to `path` soon, on a pool thread. `who` names it in the log if it fails.</summary>
        public static void WriteLater(string path, string text, string who)
        {
            lock (Queue)
            {
                if (!Waiting.ContainsKey(path)) Order.Add(path);
                Waiting[path] = new[] { text, who };
                if (_draining) return;
                _draining = true;
            }
            System.Threading.ThreadPool.QueueUserWorkItem(_ => { try { Drain(); } catch { } });
        }

        /// <summary>Writes whatever is still waiting, here and now: a script stopping, or a save about to be read.</summary>
        public static void Flush() => Drain();

        private static void Drain()
        {
            lock (Writer)
            {
                List<KeyValuePair<string, string[]>> failed = null;
                while (true)
                {
                    string path;
                    string[] job;
                    lock (Queue)
                    {
                        if (Order.Count == 0)
                        {
                            if (failed != null)
                                foreach (KeyValuePair<string, string[]> f in failed)
                                    if (!Waiting.ContainsKey(f.Key)) { Order.Add(f.Key); Waiting[f.Key] = f.Value; }
                            _draining = false;
                            return;
                        }
                        path = Order[0];
                        Order.RemoveAt(0);
                        job = Waiting[path];
                        Waiting.Remove(path);
                    }
                    try { Write(path, job[0]); }
                    catch (Exception ex)
                    {
                        Log.Write(job[1], "save " + Path.GetFileName(path) + ": " + ex.Message);
                        (failed ?? (failed = new List<KeyValuePair<string, string[]>>())).Add(new KeyValuePair<string, string[]>(path, job));
                    }
                }
            }
        }

        public static void SetAside(string path, string who)
        {
            try
            {
                string to = path + ".unreadable-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                File.Move(path, to);
                Log.Write(who, "could not read " + Path.GetFileName(path) + "; kept it as " + Path.GetFileName(to) + " and started a new one");
            }
            catch (Exception ex) { Log.Write(who, "could not set aside " + path + ": " + ex.Message); }
        }
    }

    internal static class Log
    {
        private static readonly object Gate = new object();

        public static void Write(string who, string text)
        {
            try
            {
                lock (Gate)
                    File.AppendAllText(Path.Combine(Paths.Scripts ?? ".", Paths.Name + ".log"),
                        DateTime.Now.ToString("HH:mm:ss") + " [" + who + "] " + text + Environment.NewLine);
            }
            catch { }
        }
    }

    internal static class Num
    {
        public static float Clamp(float v, float lo, float hi) => v < lo ? lo : v > hi ? hi : v;
        public static float Clamp01(float v) => Clamp(v, 0f, 1f);
        public static float Lerp(float a, float b, float t) => a + (b - a) * t;

        /// <summary>CSS cubic-bezier(x1, y1, x2, y2) at progress x.</summary>
        public static float Bezier(float x1, float y1, float x2, float y2, float x)
        {
            if (x <= 0f) return 0f;
            if (x >= 1f) return 1f;
            float lo = 0f, hi = 1f, t = x;
            for (int i = 0; i < 24; i++)
            {
                t = (lo + hi) * 0.5f;
                float bx = Cubic(x1, x2, t);
                if (bx < x) lo = t; else hi = t;
            }
            return Cubic(y1, y2, t);
        }

        private static float Cubic(float p1, float p2, float t)
        {
            float u = 1f - t;
            return 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t;
        }

        /// <summary>CSS ease-in-out.</summary>
        public static float EaseInOut(float x) => Bezier(0.42f, 0f, 0.58f, 1f, x);
        /// <summary>CSS ease.</summary>
        public static float Ease(float x) => Bezier(0.25f, 0.1f, 0.25f, 1f, x);
        /// <summary>CSS ease-out.</summary>
        public static float EaseOut(float x) => Bezier(0f, 0f, 0.58f, 1f, x);
    }
}
