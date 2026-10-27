using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// The HUD's own words in the game's language. scripts\Hud\Lang\&lt;code&gt;.txt holds "English = translation"
    /// lines, the same files GTA6Hud reads (its ViTranslate patch): a number in a line (a count, a price, a time) is
    /// matched as {0}, {1}..., and a key with {n} also matches any text there ("BUY {0} (${1})"), whose pieces get their
    /// own translation when they have one. GTA6Hud.ini [Language] Use = Auto follows the game's language (English, or a
    /// file name such as ko, picks one); Collect = True writes every line drawn without a translation to
    /// Lang\missing_&lt;this mod&gt;.txt as "line = ", for a translator to fill in.
    ///
    /// Lines are translated where they're drawn and measured (TextKit.Draw / Measure, Feed.Post, the help boxes), so a
    /// label the code compares stays English there and both see the same words. Self-contained (no Paths, no Config):
    /// ViCustoms and ViSwitch compile this file too.
    /// </summary>
    internal static class Tr
    {
        private static bool _loaded, _active, _collect;
        private static string _missing;
        private static readonly Dictionary<string, string> Exact = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly List<KeyValuePair<string, string>> Patterns = new List<KeyValuePair<string, string>>();
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly HashSet<string> Out = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> Seen = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>The line in the player's language, or the line as it was.</summary>
        public static string T(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (!_loaded) Load();
            if (!_active && !_collect) return s;
            if (Cache.TryGetValue(s, out string r)) return r;
            if (Out.Contains(s)) return s;   // already a translation
            r = Find(s);
            if (r == null)
            {
                r = s;
                if (_collect) Note(s);
            }
            else if (!ReferenceEquals(r, s))
            {
                if (Out.Count > 4096) Out.Clear();
                Out.Add(r);
            }
            if (Cache.Count > 2048) Cache.Clear();
            Cache[s] = r;
            return r;
        }

        private static void Load()
        {
            _loaded = true;
            try
            {
                string scripts = ScriptsDir();
                string dir = Path.Combine(scripts, "Hud", "Lang");
                _missing = Path.Combine(dir, "missing_" + typeof(Tr).Assembly.GetName().Name + ".txt");
                string use = "Auto";
                string ini = Path.Combine(scripts, "GTA6Hud.ini");
                if (File.Exists(ini))
                {
                    ScriptSettings s = ScriptSettings.Load(ini);
                    use = s.GetValue("Language", "Use", "Auto");
                    _collect = s.GetValue("Language", "Collect", false);
                }
                string code = Code(use);
                if (code == null) return;
                string path = Path.Combine(dir, code + ".txt");
                if (!File.Exists(path) && code == "es-MX") path = Path.Combine(dir, "es.txt");
                if (!File.Exists(path)) return;
                foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
                {
                    if (line.Length == 0 || line[0] == '#' || line[0] == ';') continue;
                    int eq = line.IndexOf(" = ", StringComparison.Ordinal);
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim(), val = line.Substring(eq + 3).Trim();
                    if (key.Length == 0 || val.Length == 0) continue;
                    Exact[key] = val;
                    if (key.IndexOf('{') >= 0) Patterns.Add(new KeyValuePair<string, string>(key, val));
                }
                // the most specific key first (the most fixed text), so "BUY {0} (${1})" wins over "{0} (${1})"
                Patterns.Sort((a, b) => FixedLength(b.Key).CompareTo(FixedLength(a.Key)));
                _active = Exact.Count > 0;
            }
            catch
            {
                // no language file then: English, as before
            }
        }

        private static int FixedLength(string key)
        {
            int n = 0;
            bool inside = false;
            foreach (char c in key)
            {
                if (c == '{') inside = true;
                else if (c == '}') inside = false;
                else if (!inside) n++;
            }
            return n;
        }

        // the scripts folder this mod was loaded from (Script Hook V .NET's own folder when that can't be told)
        private static string ScriptsDir()
        {
            try
            {
                string dir = Path.GetDirectoryName(new Uri(typeof(Tr).Assembly.CodeBase).LocalPath);
                if (dir != null && File.Exists(Path.Combine(dir, "GTA6Hud.ini"))) return dir;
            }
            catch { }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        // Auto is the game's language (GET_CURRENT_LANGUAGE), English (or en) is none, anything else is a file name
        private static string Code(string use)
        {
            use = (use ?? "Auto").Trim();
            if (use.Length == 0 || use.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                switch ((int)Game.Language)
                {
                    case 1: return "fr";
                    case 2: return "de";
                    case 3: return "it";
                    case 4: return "es";
                    case 5: return "pt-BR";
                    case 6: return "pl";
                    case 7: return "ru";
                    case 8: return "ko";
                    case 9: return "zh-Hant";
                    case 10: return "ja";
                    case 11: return "es-MX";
                    case 12: return "zh-Hans";
                    default: return null;
                }
            }
            if (use.Equals("English", StringComparison.OrdinalIgnoreCase) || use.Equals("en", StringComparison.OrdinalIgnoreCase)) return null;
            return use;
        }

        // the translation of s, or null; the spaces around s stay as they were
        private static string Find(string s)
        {
            int a = 0, b = s.Length;
            while (a < b && char.IsWhiteSpace(s[a])) a++;
            while (b > a && char.IsWhiteSpace(s[b - 1])) b--;
            if (a == b) return s;
            string core = a == 0 && b == s.Length ? s : s.Substring(a, b - a);
            if (Exact.TryGetValue(core, out string t)) return Wrap(s, a, b, t);
            var nums = new List<string>();
            string shape = Shape(core, nums);
            if (nums.Count > 0 && Exact.TryGetValue(shape, out t)) return Wrap(s, a, b, Fill(t, nums, false));
            foreach (KeyValuePair<string, string> p in Patterns)
            {
                List<string> parts = Match(p.Key, core);
                if (parts == null) continue;
                string filled = Fill(p.Value, parts, true);
                if (filled != core) return Wrap(s, a, b, filled);   // the same line back: let Collect list it
            }
            return null;
        }

        private static string Wrap(string s, int a, int b, string t) => a == 0 && b == s.Length ? t : s.Substring(0, a) + t + s.Substring(b);

        // each number (digits, with , . : between digits) becomes {0}, {1}... in order; the numbers go in nums
        private static string Shape(string s, List<string> nums)
        {
            StringBuilder sb = null;
            int i = 0, last = 0;
            while (i < s.Length)
            {
                if (!char.IsDigit(s[i])) { i++; continue; }
                int j = i + 1;
                while (j < s.Length && (char.IsDigit(s[j]) || ((s[j] == ',' || s[j] == '.' || s[j] == ':') && j + 1 < s.Length && char.IsDigit(s[j + 1])))) j++;
                if (sb == null) sb = new StringBuilder(s.Length + 8);
                sb.Append(s, last, i - last).Append('{').Append(nums.Count).Append('}');
                nums.Add(s.Substring(i, j - i));
                last = j;
                i = j;
            }
            if (sb == null) return s;
            sb.Append(s, last, s.Length - last);
            return sb.ToString();
        }

        // the pieces of s a key's {n} stand for (indexed by n), or null when s doesn't have the key's shape
        private static List<string> Match(string key, string s)
        {
            int open = key.IndexOf('{');
            if (open < 0 || !s.StartsWith(key.Substring(0, open), StringComparison.Ordinal)) return null;
            var parts = new List<string>();
            int pos = open, k = open;
            while (k < key.Length)
            {
                int close = key.IndexOf('}', k);
                if (close < 0 || !int.TryParse(key.Substring(k + 1, close - k - 1), out int n) || n < 0 || n > 9) return null;
                int next = key.IndexOf('{', close + 1);
                string lit = next < 0 ? key.Substring(close + 1) : key.Substring(close + 1, next - close - 1);
                int at;
                if (next < 0)
                {
                    at = s.Length - lit.Length;
                    if (at <= pos || !s.EndsWith(lit, StringComparison.Ordinal)) return null;
                }
                else
                {
                    if (lit.Length == 0) return null;
                    at = s.IndexOf(lit, pos + 1, StringComparison.Ordinal);
                    if (at < 0) return null;
                }
                while (parts.Count <= n) parts.Add("");
                parts[n] = s.Substring(pos, at - pos);
                pos = at + lit.Length;
                if (next < 0) return parts;
                k = next;
            }
            return null;
        }

        // the template with each {n} filled; a piece with its own translation gets it (names in "BUY {0}")
        private static string Fill(string t, List<string> vals, bool translatePieces)
        {
            var sb = new StringBuilder(t.Length + 16);
            for (int i = 0; i < t.Length; i++)
            {
                if (t[i] == '{')
                {
                    int close = t.IndexOf('}', i + 1);
                    if (close > i + 1 && int.TryParse(t.Substring(i + 1, close - i - 1), out int n) && n >= 0 && n < vals.Count)
                    {
                        string v = vals[n];
                        if (translatePieces && Exact.TryGetValue(v, out string tv)) v = tv;
                        sb.Append(v);
                        i = close;
                        continue;
                    }
                }
                sb.Append(t[i]);
            }
            return sb.ToString();
        }

        // Collect: each line with a word in it, once, as "shape = "
        private static void Note(string s)
        {
            string core = s.Trim();
            int letters = 0;
            foreach (char c in core) if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) letters++;
            if (letters < 2 || _missing == null) return;
            string shape = Shape(core, new List<string>());
            if (!Seen.Add(shape)) return;
            try { File.AppendAllText(_missing, shape + " = \r\n", Encoding.UTF8); } catch { }
        }
    }
}
