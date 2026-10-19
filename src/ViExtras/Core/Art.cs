using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The art rendered from vice_hud's own page (tools/render_fivem_art.py):
    /// Art\*.png plus Art\manifest_*.txt (one per group), which give each
    /// picture's size and where its layout box sits inside it, in CSS pixels
    /// at 1080p.
    /// </summary>
    internal static class Art
    {
        internal sealed class Entry
        {
            public string Path;
            public float W, H;       // the picture
            public float BoxX, BoxY; // the layout box's corner inside it
            public float BoxW, BoxH; // the layout box
            public string Txd;       // the game texture dictionary holding it (vix_<group>)
            public string Tex;       // its texture there (the picture's name)
        }

        private static Dictionary<string, Entry> _entries;
        private static Dictionary<string, Entry> _byPath;

        private static void Load()
        {
            if (_entries != null) return;
            _entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            _byPath = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            try
            {
                // Sorted, first listing wins: the same order tools/build_txd.py packs
                // them in, so a picture's dictionary here is the one it went into.
                string[] files = Directory.GetFiles(System.IO.Path.GetDirectoryName(Paths.Art("x")), "manifest_*.txt");
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (string file in files)
                {
                    string group = System.IO.Path.GetFileNameWithoutExtension(file).Substring("manifest_".Length);
                    foreach (string line in File.ReadAllLines(file))
                    {
                        if (line.StartsWith("#")) continue;
                        string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (p.Length != 7 || _entries.ContainsKey(p[0])) continue;
                        float F(int i) => float.Parse(p[i], CultureInfo.InvariantCulture);
                        var e = new Entry
                        {
                            Path = Paths.Art(p[0] + ".png"),
                            W = F(1), H = F(2), BoxX = F(3), BoxY = F(4), BoxW = F(5), BoxH = F(6),
                            Txd = "vix_" + group, Tex = p[0],
                        };
                        _entries[p[0]] = e;
                        _byPath[e.Path] = e;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Art", "manifest: " + ex.Message);
            }
        }

        /// <summary>The texture dictionary and texture a picture file was packed into, if any.</summary>
        public static bool Native(string path, out string txd, out string tex)
        {
            Load();
            txd = tex = null;
            if (path == null || !_byPath.TryGetValue(path, out Entry e)) return false;
            txd = e.Txd;
            tex = e.Tex;
            return true;
        }

        public static Entry Get(string name)
        {
            Load();
            return name != null && _entries.TryGetValue(name, out Entry e) ? e : null;
        }

        /// <summary>Draws `name` with its layout box's top-left at (x, y), scaled by `scale`.</summary>
        public static void Draw(Entry e, float x, float y, Color color, float scale = 1f)
        {
            if (e == null) return;
            Canvas.Image(e.Path, x - e.BoxX * scale, y - e.BoxY * scale, e.W * scale, e.H * scale, color);
        }

        /// <summary>The variant of a full-screen picture drawn for the screen shape closest to this one.</summary>
        public static Entry ForAspect(string baseName)
        {
            float aspect = GTA.UI.Screen.AspectRatio;
            string[] names = { "16x10", "16x9", "21x9", "32x9" };
            float[] ratios = { 1.6f, 16f / 9f, 2520f / 1080f, 32f / 9f };
            int best = 1;
            for (int i = 0; i < ratios.Length; i++)
                if (Math.Abs(ratios[i] - aspect) < Math.Abs(ratios[best] - aspect)) best = i;
            return Get(baseName + "_" + names[best]);
        }
    }

    /// <summary>
    /// Which button a control is on the device in hand, as vice_hud's
    /// client_glyphs.lua works it out, and the art to draw it with. Pads are
    /// drawn the way GTA6Hud.ini [Controller] Style says (PlayStation or Xbox).
    /// </summary>
    internal static class Glyphs
    {
        private static readonly Dictionary<int, string> PadIcons = new Dictionary<int, string>
        {
            [4] = "DPAD_UP", [5] = "DPAD_DOWN", [6] = "DPAD_LEFT", [7] = "DPAD_RIGHT",
            [8] = "DPAD", [9] = "DPAD", [10] = "DPAD_Y", [11] = "DPAD_X",
            [12] = "LS_UP", [13] = "LS_DOWN", [14] = "LS_LEFT", [15] = "LS_RIGHT",
            [16] = "LS", [17] = "LS", [18] = "LS_Y", [19] = "LS_X", [20] = "LS",
            [21] = "RS_UP", [22] = "RS_DOWN", [23] = "RS_LEFT", [24] = "RS_RIGHT",
            [25] = "RS", [26] = "RS", [27] = "RS_Y", [28] = "RS_X", [29] = "RS",
            [30] = "A", [31] = "B", [32] = "X", [33] = "Y",
            [34] = "LB", [35] = "LT", [36] = "RB", [37] = "RT",
            [38] = "START", [39] = "BACK",
        };

        private static readonly Dictionary<int, string> KbmIcons = new Dictionary<int, string>
        {
            [0] = "UP", [1] = "DOWN", [2] = "LEFT", [3] = "RIGHT",
            [100] = "LMB", [101] = "RMB", [102] = "MMB",
            [103] = "MOUSE4", [104] = "MOUSE5", [105] = "MOUSE6", [106] = "MOUSE7", [107] = "MOUSE8",
            [115] = "WHEEL_UP", [116] = "WHEEL_DOWN",
            [130] = "NUM -", [131] = "NUM +",
            [136] = "NUM 0", [137] = "NUM 1", [138] = "NUM 2", [139] = "NUM 3", [140] = "NUM 4",
            [141] = "NUM 5", [142] = "NUM 6", [143] = "NUM 7", [144] = "NUM 8", [145] = "NUM 9",
            [170] = "F1", [171] = "F2", [172] = "F3", [173] = "F4", [174] = "F5", [175] = "F6",
            [176] = "F7", [177] = "F8", [178] = "F9", [179] = "F10", [180] = "F11", [181] = "F12",
            [194] = "UP", [195] = "DOWN", [196] = "LEFT", [197] = "RIGHT",
            [198] = "DEL", [199] = "ESC", [200] = "INS", [201] = "END",
            [210] = "DEL", [211] = "INS", [212] = "END",
            [1000] = "SHIFT", [1001] = "SHIFT", [1002] = "TAB", [1003] = "ENTER", [1004] = "BKSP",
            [1008] = "HOME", [1009] = "PGUP", [1010] = "PGDN", [1012] = "CAPS",
            [1013] = "CTRL", [1014] = "CTRL", [1015] = "ALT", [1016] = "ALT",
            [1055] = "HOME", [1056] = "PGUP",
            [2000] = "SPACE",
        };

        private static string _padStyle;

        public static string PadStyle
        {
            get
            {
                if (_padStyle == null)
                {
                    string style = "";
                    try { style = ScriptSettings.Load(System.IO.Path.Combine(Paths.Scripts, "GTA6Hud.ini")).GetValue("Controller", "Style", ""); } catch { }
                    style = Config.Get("Prompts", "PadStyle", style).ToLowerInvariant();
                    _padStyle = style.StartsWith("ps") || style.Contains("playstation") || style.Contains("dual") ? "ps" : "xbox";
                }
                return _padStyle;
            }
        }

        public static bool UsingPad => !Function.Call<bool>(Hash.IS_USING_KEYBOARD_AND_MOUSE, 2);

        /// <summary>"b_30" -> A on a pad, "t_E" -> E on a keyboard (see client_glyphs.lua).</summary>
        private static bool Parse(string raw, out string token, out string device)
        {
            token = null;
            device = null;
            if (string.IsNullOrEmpty(raw) || raw.Length < 3) return false;
            int pct = raw.IndexOf('%');
            string first = pct > 0 ? raw.Substring(0, pct) : raw;
            string kind = first.Substring(0, 2), body = first.Substring(2);
            if (kind == "b_")
            {
                if (!int.TryParse(body, out int n)) return false;
                if (PadIcons.TryGetValue(n, out token)) { device = "pad"; return true; }
                if (KbmIcons.TryGetValue(n, out token)) { device = "kbm"; return true; }
                return false;
            }
            if (body.Length > 1 && int.TryParse(body, out _)) return false;
            foreach (char c in body) if (c < 0x20 || c > 0x7E) return false;
            token = body.ToUpperInvariant();
            device = kind == "t_" || kind == "T_" ? "kbm" : (UsingPad ? "pad" : "kbm");
            return true;
        }

        /// <summary>The art for `control` on the device in hand, or null when it has no button.</summary>
        public static Art.Entry ForControl(int control)
        {
            string raw = Function.Call<string>(Hash.GET_CONTROL_INSTRUCTIONAL_BUTTONS_STRING, 0, control, true);
            return Parse(raw, out string token, out string device) ? ForToken(token, device) : null;
        }

        /// <summary>The art for a token ("WHEEL_UP", "E", "A"), on `device` ("kbm" / "pad").</summary>
        public static Art.Entry ForToken(string token, string device)
        {
            string file = FileName(token);
            Art.Entry e = device == "pad" ? Art.Get("glyph_pad_" + PadStyle + "_" + file) : null;
            return e ?? Art.Get("glyph_kbm_" + file);
        }

        private static string FileName(string token)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in token.Replace(' ', '_'))
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c.ToString() : "x" + ((int)c).ToString("X2"));
            return sb.ToString();
        }
    }

    /// <summary>
    /// vice_hud's action prompts (#prompts): a column in the bottom-right corner,
    /// 1.31 --w in from the right and 3.75cqh up, rows 1cqh apart, each row the
    /// word and then its button, centred on one line.
    /// </summary>
    internal static class Prompts
    {
        internal struct Row
        {
            public string Label;     // "zoom" -> Art label_zoom
            public Art.Entry Glyph;  // null: the word alone
            public bool Off;         // greyed: cannot be done right now
        }

        private const float WrapW = 4.067f * 10.8f;  // .prompt-glyph-wrap
        private const float RowH = 3.89f * 10.8f;
        private const float Gap = 0.5f * 10.8f;      // between word and button
        private const float RowGap = 1f * 10.8f;
        private static readonly float Right = 1.31f * 1.7778f * 10.8f;
        private const float Bottom = 3.75f * 10.8f;

        public static void Draw(IList<Row> rows)
        {
            float y = Canvas.H - Bottom;
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                Row r = rows[i];
                Art.Entry label = Art.Get("label_" + r.Label);
                float x = Canvas.W - Right;
                float top = y - RowH;
                Color tint = r.Off ? Color.FromArgb(255, 138, 135, 148) : Color.White;
                if (r.Glyph != null)
                {
                    float w = Math.Max(WrapW, r.Glyph.BoxW);
                    x -= w;
                    Art.Draw(r.Glyph, x + (w - r.Glyph.BoxW) * 0.5f, top + (RowH - r.Glyph.BoxH) * 0.5f, r.Off ? Color.FromArgb(255, 160, 160, 160) : Color.White);
                    x -= Gap;
                }
                if (label != null)
                {
                    x -= label.BoxW;
                    Art.Draw(label, x, top + (RowH - label.BoxH) * 0.5f, tint);
                }
                y = top - RowGap;
            }
        }
    }
}
