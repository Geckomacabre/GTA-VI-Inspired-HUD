using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace ViExtras
{
    internal enum TextFace { Display, Body, Radio }
    internal enum TextAlign { Left, Center, Right }

    /// <summary>
    /// Leonida Customs' text: any string in the shop's two faces, set the way its
    /// FiveM page sets it. Every character was rendered from that page's CSS by
    /// tools/render/customs_fonts.py into glyph atlases (customsfont_* in
    /// vix_customsfont) with their advances, cells and kerning in
    /// Art\customs_glyphs.txt; this lays a line out from those numbers and draws
    /// each letter from its atlas with DRAW_SPRITE_ARX_WITH_UV.
    ///
    /// Display is 'CustomsDisplay' (Bahnschrift 700 at font-stretch 75%): the
    /// headings, tabs, prices, stat labels, pager. The shop uppercases it
    /// (text-transform), so its atlas has capitals only: uppercase the text
    /// first; lower case left in is drawn with the capitals anyway. Its rule also
    /// has letter-spacing .01em, which is the caller's to pass. Body is GTA Art
    /// Deco 400, mixed case.
    ///
    /// Sizes are CSS font sizes in canvas pixels (1080 tall): 2.4vh = 25.92.
    /// </summary>
    internal static class TextKit
    {
        private struct Glyph
        {
            public float Advance;        // pen advance, base pixels
            public float X, Y, W, H;     // cell in the atlas, atlas pixels; W = 0: no ink
            public float Ox, Oy;         // cell's left less the pen x; baseline below the cell's top
        }

        private sealed class FaceData
        {
            public float Size, Ascent, Descent, LineGap, CapHeight, XHeight;
            public string Atlas, OutlineAtlas;
            public float AtlasW, AtlasH;
            public readonly Dictionary<char, Glyph> Glyphs = new Dictionary<char, Glyph>();
            public readonly Dictionary<int, float> Kern = new Dictionary<int, float>();
        }

        private const char Ellipsis = '\u2026';
        private static FaceData[] _faces;
        private static bool _warned;

        private static FaceData Get(TextFace face)
        {
            if (_faces == null) Load();
            return _faces[(int)face];
        }

        private static void Load()
        {
            _faces = new FaceData[3];
            string path = Paths.Art("customs_glyphs.txt");
            try
            {
                foreach (string line in File.ReadAllLines(path))
                {
                    if (line.Length == 0 || line[0] == '#') continue;
                    string[] p = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (p.Length < 2) continue;
                    int fi = p[1] == "display" ? (int)TextFace.Display : p[1] == "body" ? (int)TextFace.Body
                        : p[1] == "radio" ? (int)TextFace.Radio : -1;
                    if (fi < 0) continue;
                    FaceData f = _faces[fi];
                    if (p[0] == "face" && p.Length >= 12)
                    {
                        _faces[fi] = new FaceData
                        {
                            Size = F(p[2]), Ascent = F(p[3]), Descent = F(p[4]), LineGap = F(p[5]),
                            CapHeight = F(p[6]), XHeight = F(p[7]),
                            Atlas = p[8], OutlineAtlas = p[9], AtlasW = F(p[10]), AtlasH = F(p[11]),
                        };
                    }
                    else if (p[0] == "glyph" && p.Length >= 10 && f != null)
                    {
                        f.Glyphs[(char)Hex(p[2])] = new Glyph
                        {
                            Advance = F(p[3]), X = F(p[4]), Y = F(p[5]), W = F(p[6]), H = F(p[7]), Ox = F(p[8]), Oy = F(p[9]),
                        };
                    }
                    else if (p[0] == "kern" && p.Length >= 5 && f != null)
                    {
                        f.Kern[(Hex(p[2]) << 16) | Hex(p[3])] = F(p[4]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("TextKit", "could not read " + path + ": " + ex.Message);
            }
            for (int i = 0; i < _faces.Length; i++)
                if (_faces[i] == null || _faces[i].Size <= 0f || _faces[i].Glyphs.Count == 0)
                {
                    Log.Write("TextKit", "no glyphs for the " + (TextFace)i + " face in " + path);
                    _faces[i] = null;
                }
        }

        private static float F(string s) => float.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
        private static int Hex(string s) => int.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        // The glyph drawn for c. Lower case the face lacks (the display face has
        // none) is drawn with its capital, an accented letter it lacks with the
        // plain letter, other white space as a space, anything else as '?'.
        private static bool Find(FaceData f, char c, out char key, out Glyph g)
        {
            key = c;
            if (f.Glyphs.TryGetValue(c, out g)) return true;
            char up = char.ToUpperInvariant(c);
            if (up != c && f.Glyphs.TryGetValue(up, out g)) { key = up; return true; }
            if (c > 0x7F)
            {
                string bare = c.ToString().Normalize(NormalizationForm.FormD);
                if (bare.Length > 0 && bare[0] != c && bare[0] <= 0x7F)
                    return Find(f, bare[0], out key, out g);
            }
            key = char.IsWhiteSpace(c) ? ' ' : '?';
            return f.Glyphs.TryGetValue(key, out g);
        }

        // One character's move of the pen, as the browser sets a line: the
        // kerning between it and the glyph before, its advance, then
        // letter-spacing (CSS adds that after every letter, the last one too).
        // `left` is the pen its cell is placed from. Base pixels throughout.
        private static bool Step(FaceData f, char c, float spacing, ref int prev, ref float pen, out Glyph g, out float left)
        {
            if (!Find(f, c, out char key, out g))
            {
                left = pen;
                prev = -1;
                return false;
            }
            if (prev >= 0 && f.Kern.TryGetValue((prev << 16) | key, out float kern)) pen += kern;
            left = pen;
            pen += g.Advance + spacing;
            prev = key;
            return true;
        }

        // Lays out the first `count` characters of s, plus an ellipsis when asked,
        // and returns the width in base pixels. With an atlas, also draws them:
        // pen origin x, baseline y, scale k (canvas pixels per base pixel).
        private static float Lay(FaceData f, string s, int count, bool ellipsis, float spacing,
            string atlas = null, float x = 0f, float baseline = 0f, float k = 1f, Color color = default(Color))
        {
            float pen = 0f;
            int prev = -1;
            int n = count + (ellipsis ? 1 : 0);
            for (int i = 0; i < n; i++)
            {
                // The browser shapes the ellipsis as a run of its own: no kerning into it.
                if (i == count) prev = -1;
                char c = i < count ? s[i] : Ellipsis;
                if (!Step(f, c, spacing, ref prev, ref pen, out Glyph g, out float left) || atlas == null || g.W <= 0f || g.H <= 0f)
                    continue;
                Canvas.ImagePart(atlas, x + (left + g.Ox) * k, baseline - g.Oy * k, g.W * k, g.H * k,
                    g.X / f.AtlasW, g.Y / f.AtlasH, (g.X + g.W) / f.AtlasW, (g.Y + g.H) / f.AtlasH, color);
            }
            return pen;
        }

        // How many characters of s fit in `room` base pixels with the ellipsis
        // after them: where text-overflow: ellipsis cuts the line.
        private static int Fit(FaceData f, string s, float spacing, float room)
        {
            float tail = Find(f, Ellipsis, out _, out Glyph e) ? e.Advance + spacing : 0f;
            float pen = 0f;
            int prev = -1;
            for (int i = 0; i < s.Length; i++)
            {
                Step(f, s[i], spacing, ref prev, ref pen, out _, out _);
                if (pen + tail > room) return i;
            }
            return s.Length;
        }

        private static float Round(float v) => (float)Math.Floor(v + 0.5f);

        private static float SnapY(float canvasY)
        {
            float rows = GTA.UI.Screen.Resolution.Height;
            if (rows <= 0f) return canvasY;
            float s = rows / Canvas.H;
            return Round(canvasY * s) / s;
        }

        private static float BaselineOf(FaceData f, float size)
        {
            // The browser rounds ascent and descent to whole pixels at the size it
            // draws, then splits what is left of the line box, the odd pixel going
            // below (measured: Edge, 18 to 64 px, both faces).
            float k = size / f.Size;
            float a = Round(f.Ascent * k), d = Round(f.Descent * k);
            return a + (float)Math.Floor((size - a - d) * 0.5f + 1e-4f);
        }

        /// <summary>
        /// How far below Draw's y the baseline sits at this size: y is the top of a
        /// CSS line box with line-height: 1 (as tall as the font size), so the
        /// text's line box is centred on y + size / 2. Text whose CSS line box is L
        /// tall with its top at T is drawn at y = T + (L - size) / 2: the browser
        /// centres the letters on the line box whatever its height.
        /// </summary>
        public static float Baseline(TextFace face, float sizePx)
        {
            FaceData f = Get(face);
            return f != null ? BaselineOf(f, sizePx) : sizePx * 0.8f;
        }

        /// <summary>Height of a line box at line-height: normal, as the browser sizes it.</summary>
        public static float LineHeightNormal(TextFace face, float sizePx)
        {
            FaceData f = Get(face);
            if (f == null) return sizePx * 1.25f;
            float k = sizePx / f.Size;
            return Round(f.Ascent * k) + Round(f.Descent * k) + Round(f.LineGap * k);
        }

        /// <summary>Ink height of a capital (H) above the baseline: for centring capitals by eye.</summary>
        public static float CapHeight(TextFace face, float sizePx)
        {
            FaceData f = Get(face);
            return f != null ? f.CapHeight * sizePx / f.Size : sizePx * 0.7f;
        }

        /// <summary>
        /// Width of text in canvas pixels at CSS font-size sizePx, as the browser
        /// measures it: advances, kerning and letter-spacing (after every
        /// character, the last too).
        /// </summary>
        public static float Measure(string text, TextFace face, float sizePx, float letterSpacingEm = 0f)
        {
            if (string.IsNullOrEmpty(text) || sizePx <= 0f) return 0f;
            FaceData f = Get(face);
            // Without metrics nothing is drawn; a rough width keeps layouts apart.
            if (f == null) return text.Length * sizePx * 0.5f;
            return Lay(f, text, text.Length, false, letterSpacingEm * f.Size) * sizePx / f.Size;
        }

        /// <summary>
        /// Draws one line of text at CSS font-size sizePx (canvas pixels).
        /// x is the left edge, centre or right edge (align); y is the top of the
        /// line box at line-height: 1 (see Baseline). outline adds the shop's
        /// --outline-mid text-shadow under the letters, drawn at the colour's
        /// alpha; its own colour is fixed, as in the CSS. maxWidth &gt; 0 cuts the
        /// line like text-overflow: ellipsis in a box that wide: when it has to
        /// cut, the line starts at the box's left edge (x - maxWidth / 2 centred,
        /// x - maxWidth right-aligned), as an overflowing line does in CSS.
        /// </summary>
        public static void Draw(string text, TextFace face, float sizePx, float x, float y, Color color,
            TextAlign align = TextAlign.Left, bool outline = false, float letterSpacingEm = 0f, float maxWidth = 0f)
        {
            if (string.IsNullOrEmpty(text) || sizePx <= 0f || color.A == 0) return;
            try
            {
                FaceData f = Get(face);
                Art.Entry letters = f != null ? Art.Get(f.Atlas) : null;
                if (letters == null)
                {
                    if (!_warned) { _warned = true; Log.Write("TextKit", "no glyph atlas for the " + face + " face; text not drawn"); }
                    return;
                }
                if (f.AtlasW <= 0f || f.AtlasH <= 0f) { f.AtlasW = letters.W; f.AtlasH = letters.H; }

                float k = sizePx / f.Size;
                float spacing = letterSpacingEm * f.Size;
                int count = text.Length;
                bool cut = false;
                float width = Lay(f, text, count, false, spacing) * k;
                if (maxWidth > 0f && width > maxWidth + 0.01f)
                {
                    cut = true;
                    count = Fit(f, text, spacing, maxWidth / k);
                }

                float left;
                if (cut) left = align == TextAlign.Center ? x - maxWidth * 0.5f : align == TextAlign.Right ? x - maxWidth : x;
                else left = align == TextAlign.Center ? x - width * 0.5f : align == TextAlign.Right ? x - width : x;
                // The browser puts the baseline on a whole device pixel (and places
                // letters along it to fractions of one); this puts it on a whole
                // screen pixel.
                float baseline = SnapY(y + BaselineOf(f, sizePx));

                // The browser paints a text-shadow under the whole line, then the
                // letters: every outline cell first, so none covers a letter.
                if (outline)
                {
                    Art.Entry edge = Art.Get(f.OutlineAtlas);
                    if (edge != null)
                        Lay(f, text, count, cut, spacing, edge.Path, left, baseline, k, Color.FromArgb(color.A, 255, 255, 255));
                }
                Lay(f, text, count, cut, spacing, letters.Path, left, baseline, k, color);
            }
            catch (Exception ex)
            {
                if (!_warned) { _warned = true; Log.Write("TextKit", "draw failed: " + ex.Message); }
            }
        }
    }
}
