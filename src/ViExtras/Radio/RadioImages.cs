// Pictures the radio wheel makes at run time. The station tiles, names and
// genres vi_radio ships are rendered from its own page (tools/render/radio.py);
// this covers what cannot be known in advance (the song playing, a station the
// game has that vi_radio's table does not, a changed OffLabel) and the muted
// look. In the single-player look: no frame on the tiles, no shadow on the text.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace ViExtras
{
    internal static class RadioImages
    {
        private const float Px = 2f;        // raster pixels per CSS px, as the rendered art
        private const string Version = "r3";
        private const string Ellipsis = "\u2026";   // text-overflow: ellipsis (Anton has the glyph)

        // style.css, at vi_radio's Config.Hud defaults
        private const float Icon = 103f, Pad = 4f;
        private static readonly Color Box = Color.FromArgb(170, 0, 0, 0);              // boxColor 0,0,0,170

        private static readonly Dictionary<string, Art.Entry> Made = new Dictionary<string, Art.Entry>();
        private static PrivateFontCollection _fonts;
        private static FontFamily _anton;

        // ---- the muted look ------------------------------------------------------

        /// <summary>
        /// #radio.muted #viewport / #icon-left / #info: filter: grayscale(1)
        /// brightness(0.62). Both are colour matrices on unpremultiplied sRGB,
        /// alpha untouched: luma 0.2126 / 0.7152 / 0.0722, then times 0.62.
        /// (Checked against the page's own muted render: no difference inside
        /// the shapes.)
        /// </summary>
        public static Art.Entry Muted(Art.Entry e)
        {
            if (e == null) return null;
            // Made ahead of time for the rendered art (tools/radio_muted.py), and
            // packed with it; the run-time version below is for pictures made here.
            if (e.Tex != null)
            {
                Art.Entry packed = Art.Get(e.Tex + "_muted");
                if (packed != null) return packed;
            }
            if (Made.TryGetValue("m|" + e.Path, out Art.Entry done)) return done;
            Art.Entry result = e;
            try
            {
                string path = CachePath("m", e.Path);
                if (!File.Exists(path))
                {
                    using (Bitmap bmp = Load(e.Path))
                    {
                        Pixels(bmp, (int x, int y, ref byte b, ref byte g, ref byte r, ref byte a) =>
                        {
                            double luma = (0.2126 * r + 0.7152 * g + 0.0722 * b) * 0.62;
                            byte v = (byte)Math.Max(0, Math.Min(255, Math.Round(luma)));
                            r = g = b = v;
                        });
                        Save(bmp, path);
                    }
                }
                result = Copy(e, path);
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "muted " + Path.GetFileName(e.Path) + ": " + ex.Message);
            }
            Made["m|" + e.Path] = result;
            return result;
        }

        // ---- tiles for stations vi_radio has no art for --------------------------

        /// <summary>
        /// A .tile holding .fallback text (the station's name), as app.js builds
        /// it for a station with no logo: Anton 15px, line-height 1.1,
        /// letter-spacing .02em, upper case, centred, padding 0 8%. The box
        /// colour as style.css; no border in the single-player look. Drawn with
        /// GDI+, so close to the page rather than identical: kerning and hinting
        /// differ slightly.
        /// </summary>
        public static Art.Entry FallbackTile(string label)
        {
            string key = "t|" + label;
            if (Made.TryGetValue(key, out Art.Entry done)) return done;
            Art.Entry result = null;
            try
            {
                string path = CachePath("t", key);
                if (!File.Exists(path))
                {
                    int size = (int)Math.Round((Icon + Pad * 2) * Px);
                    using (var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.Transparent);
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            float o = Pad * Px, w = Icon * Px;
                            using (var fill = new SolidBrush(Box))
                                g.FillRectangle(fill, o, o, w, w);
                            float padX = w * 0.08f;
                            DrawWrapped(g, label.ToUpperInvariant(), o + padX, o, w - padX * 2, w, 15f * Px, 1.1f, 0.02f);
                        }
                        Save(bmp, path);
                    }
                }
                result = new Art.Entry { Path = path, W = Icon + Pad * 2, H = Icon + Pad * 2, BoxX = Pad, BoxY = Pad, BoxW = Icon, BoxH = Icon };
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "fallback tile: " + ex.Message);
            }
            Made[key] = result;
            return result;
        }

        // ---- text: the song, and names vi_radio has no art for --------------------

        /// <summary>
        /// One line of #station / #title / #artist: Anton, upper case, centred,
        /// white (the HUD tints it), cut with an ellipsis past maxWidth as
        /// text-overflow does. The box is the text's advance width by the line
        /// box, as in the rendered radio_name_* pictures. GDI+, so close to the
        /// page rather than identical.
        /// </summary>
        public static Art.Entry TextLine(string text, float fontPx, float lineHeight, float spacingEm, float maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string key = "x|" + fontPx.ToString("0.###") + "|" + lineHeight + "|" + spacingEm + "|" + maxWidth + "|" + text;
            if (Made.TryGetValue(key, out Art.Entry done)) return done;
            Art.Entry result = null;
            try
            {
                string upper = text.ToUpperInvariant().Trim();
                FontFamily fam = Anton();
                float em = fontPx * Px;
                float[] pens = Advances(upper, fam, em, spacingEm, out float advance);
                if (advance > maxWidth * Px)
                {
                    // Chrome keeps whole characters and puts the ellipsis after them.
                    int keep = upper.Length;
                    string cutText;
                    do
                    {
                        keep--;
                        cutText = upper.Substring(0, keep).TrimEnd() + Ellipsis;
                        pens = Advances(cutText, fam, em, spacingEm, out advance);
                    }
                    while (keep > 1 && advance > maxWidth * Px);
                    upper = cutText;
                }
                const float padCss = 12f;
                float pad = padCss * Px;
                float lineH = fontPx * lineHeight * Px;
                int w = (int)Math.Ceiling(advance + pad * 2), h = (int)Math.Ceiling(lineH + pad * 2);
                string path = CachePath("x", key);
                if (!File.Exists(path))
                {
                    using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                    {
                        float baseline = pad + Baseline(fam, em, lineH);
                        using (var glyphs = new GraphicsPath())
                        {
                            AddText(glyphs, upper, pens, fam, em, pad, baseline);
                            // clipped to the line box (overflow: hidden)
                            int top = (int)Math.Floor(pad), bottom = (int)Math.Ceiling(pad + lineH);
                            Paint(bmp, Coverage(glyphs, w, h), Color.White, top, bottom);
                        }
                        Save(bmp, path);
                    }
                }
                result = new Art.Entry { Path = path, W = w / Px, H = h / Px, BoxX = padCss, BoxY = padCss, BoxW = advance / Px, BoxH = lineH / Px };
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "text: " + ex.Message);
            }
            Made[key] = result;
            return result;
        }

        // ---- helpers ------------------------------------------------------------

        private static FontFamily Anton()
        {
            if (_fonts == null)
            {
                _fonts = new PrivateFontCollection();
                string path = Path.Combine(Paths.Root, "Radio", "Anton.ttf");
                if (File.Exists(path)) _fonts.AddFontFile(path);
                _anton = _fonts.Families.Length > 0 ? _fonts.Families[0] : new FontFamily("Impact");
            }
            return _anton;
        }

        private static FontStyle StyleOf(FontFamily fam) => fam.IsStyleAvailable(FontStyle.Regular) ? FontStyle.Regular : FontStyle.Bold;

        // Where the baseline sits in a CSS line box of height lineH: half-leading,
        // then the ascent. Chrome works this in whole CSS pixels: ascent and
        // descent rounded, the half-leading floored (measured off the page for
        // Anton at 15, 28.2, 35.72 and 100px).
        private static float Baseline(FontFamily fam, float em, float lineH)
        {
            FontStyle st = StyleOf(fam);
            float emH = fam.GetEmHeight(st);
            float emCss = em / Px, lineCss = lineH / Px;
            float asc = (float)Math.Round(fam.GetCellAscent(st) * emCss / emH);
            float desc = (float)Math.Round(fam.GetCellDescent(st) * emCss / emH);
            return ((float)Math.Floor((lineCss - (asc + desc)) / 2f) + asc) * Px;
        }

        private static float[] Advances(string s, FontFamily fam, float em, float spacingEm, out float total)
        {
            var pens = new float[s.Length];
            float pen = 0f;
            var fmt = (StringFormat)StringFormat.GenericTypographic.Clone();
            fmt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip;
            using (var probe = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(probe))
            using (var font = new Font(fam, em, StyleOf(fam), GraphicsUnit.Pixel))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                for (int i = 0; i < s.Length; i++)
                {
                    pens[i] = pen;
                    // CSS letter-spacing goes after every letter, the last one too.
                    pen += g.MeasureString(s[i].ToString(), font, PointF.Empty, fmt).Width + spacingEm * em;
                }
            }
            total = pen;
            return pens;
        }

        private static void AddText(GraphicsPath path, string s, float[] pens, FontFamily fam, float em, float x, float baseline)
        {
            FontStyle st = StyleOf(fam);
            float asc = fam.GetCellAscent(st) * em / fam.GetEmHeight(st);
            var fmt = (StringFormat)StringFormat.GenericTypographic.Clone();
            for (int i = 0; i < s.Length; i++)
                if (s[i] != ' ') path.AddString(s[i].ToString(), fam, (int)st, em, new PointF(x + pens[i], baseline - asc), fmt);
        }

        // Lines of the fallback tile, greedy word wrap (word-break: break-word
        // splits a word that cannot fit on a line of its own).
        private static void DrawWrapped(Graphics g, string text, float left, float top, float width, float height, float em, float lineHeight, float spacingEm)
        {
            FontFamily fam = Anton();
            var lines = new List<string>();
            string line = "";
            foreach (string word in text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string attempt = line.Length == 0 ? word : line + " " + word;
                Advances(attempt, fam, em, spacingEm, out float aw);
                if (aw <= width || line.Length == 0)
                {
                    line = attempt;
                    Advances(line, fam, em, spacingEm, out float lw);
                    while (lw > width && line.Length > 1)
                    {
                        // break inside the word
                        int cut = line.Length - 1;
                        while (cut > 1) { Advances(line.Substring(0, cut), fam, em, spacingEm, out float cw); if (cw <= width) break; cut--; }
                        lines.Add(line.Substring(0, cut));
                        line = line.Substring(cut);
                        Advances(line, fam, em, spacingEm, out lw);
                    }
                }
                else
                {
                    lines.Add(line);
                    line = word;
                }
            }
            if (line.Length > 0) lines.Add(line);

            float lineH = em * lineHeight;
            float y = top + (height - lineH * lines.Count) / 2f;
            using (var path = new GraphicsPath())
            {
                foreach (string l in lines)
                {
                    float[] pens = Advances(l, fam, em, spacingEm, out float lw);
                    AddText(path, l, pens, fam, em, left + (width - lw) / 2f, y + Baseline(fam, em, lineH));
                    y += lineH;
                }
                g.FillPath(Brushes.White, path);
            }
        }

        private static float[] Coverage(GraphicsPath path, int w, int h)
        {
            var cover = new float[w * h];
            using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.FillPath(Brushes.White, path);
                }
                Pixels(bmp, (int x, int y, ref byte b, ref byte gg, ref byte r, ref byte a) => { cover[y * w + x] = a / 255f; });
            }
            return cover;
        }

        // Lays `color` at `cover` over what is in the bitmap, rows top..bottom only.
        private static void Paint(Bitmap bmp, float[] cover, Color color, int top, int bottom)
        {
            int w = bmp.Width;
            Pixels(bmp, (int x, int y, ref byte b, ref byte g, ref byte r, ref byte a) =>
            {
                float s = y >= top && y < bottom ? cover[y * w + x] * color.A / 255f : 0f;
                if (s <= 0f) return;
                float da = a / 255f;
                float oa = s + da * (1f - s);
                if (oa <= 0f) return;
                r = (byte)Math.Round((color.R * s + r * da * (1f - s)) / oa);
                g = (byte)Math.Round((color.G * s + g * da * (1f - s)) / oa);
                b = (byte)Math.Round((color.B * s + b * da * (1f - s)) / oa);
                a = (byte)Math.Round(oa * 255f);
            });
        }

        private delegate void PixelFn(int x, int y, ref byte b, ref byte g, ref byte r, ref byte a);

        // Every pixel, as unpremultiplied BGRA.
        private static void Pixels(Bitmap bmp, PixelFn fn)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            try
            {
                var row = new byte[bmp.Width * 4];
                for (int y = 0; y < bmp.Height; y++)
                {
                    IntPtr line = data.Scan0 + y * data.Stride;
                    System.Runtime.InteropServices.Marshal.Copy(line, row, 0, row.Length);
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int o = x * 4;
                        fn(x, y, ref row[o], ref row[o + 1], ref row[o + 2], ref row[o + 3]);
                    }
                    System.Runtime.InteropServices.Marshal.Copy(row, 0, line, row.Length);
                }
            }
            finally { bmp.UnlockBits(data); }
        }

        private static Bitmap Load(string path)
        {
            using (var file = new Bitmap(path))
            {
                var bmp = new Bitmap(file.Width, file.Height, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(file, new Rectangle(0, 0, file.Width, file.Height));
                }
                return bmp;
            }
        }

        private static void Save(Bitmap bmp, string path)
        {
            string tmp = path + ".tmp";
            bmp.Save(tmp, ImageFormat.Png);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
        }

        private static Art.Entry Copy(Art.Entry e, string path) =>
            new Art.Entry { Path = path, W = e.W, H = e.H, BoxX = e.BoxX, BoxY = e.BoxY, BoxW = e.BoxW, BoxH = e.BoxH };

        // A cache file name that changes whenever the source picture does.
        private static string CachePath(string kind, string source)
        {
            string stamp = source;
            try { if (File.Exists(source)) stamp += "|" + File.GetLastWriteTimeUtc(source).Ticks + "|" + new FileInfo(source).Length; } catch { }
            uint hash = 2166136261;
            foreach (char c in Version + "|" + stamp) { hash ^= c; hash *= 16777619; }
            Directory.CreateDirectory(Cache.Dir);
            return Path.Combine(Cache.Dir, "radio_" + kind + "_" + hash.ToString("x8") + ".png");
        }
    }
}
