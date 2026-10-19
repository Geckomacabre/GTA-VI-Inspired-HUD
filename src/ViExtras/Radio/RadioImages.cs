// Pictures the radio wheel makes at run time. The station tiles, names and
// genres vi_radio ships are rendered from its own page (tools/render/radio.py);
// this covers what cannot be known in advance (a station the game has that
// vi_radio's table does not, a changed OffLabel) and the muted look.
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
        private const string Version = "r2";

        // style.css, at vi_radio's Config.Hud defaults
        private const float Icon = 103f, Border = 3.5f, Pad = 4f;
        private static readonly Color Box = Color.FromArgb(170, 0, 0, 0);              // boxColor 0,0,0,170
        private static readonly Color BoxActive = Color.FromArgb(220, 45, 45, 45);     // boxActiveColor
        private static readonly Color Outline = Color.FromArgb(90, 255, 255, 255);     // borderColor
        private static readonly Color OutlineActive = Color.White;                     // borderActiveColor

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
        /// letter-spacing .02em, upper case, centred, padding 0 8%. Box, border
        /// and the two colour sets as style.css. Drawn with GDI+, so close to the
        /// page rather than identical: kerning and hinting differ slightly.
        /// </summary>
        public static Art.Entry FallbackTile(string label, bool active)
        {
            string key = "t|" + (active ? "1|" : "0|") + label;
            if (Made.TryGetValue(key, out Art.Entry done)) return done;
            Art.Entry result = null;
            try
            {
                string path = CachePath(active ? "ta" : "t", key);
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
                            float o = Pad * Px, w = Icon * Px, t = Border * Px;
                            // background-clip is border-box: the box colour runs under the border.
                            using (var fill = new SolidBrush(active ? BoxActive : Box))
                                g.FillRectangle(fill, o, o, w, w);
                            using (var ring = new GraphicsPath(FillMode.Alternate))
                            using (var brush = new SolidBrush(active ? OutlineActive : Outline))
                            {
                                ring.AddRectangle(new RectangleF(o, o, w, w));
                                ring.AddRectangle(new RectangleF(o + t, o + t, w - t * 2, w - t * 2));
                                g.FillPath(brush, ring);
                            }
                            float content = (Icon - Border * 2) * Px;
                            float padX = content * 0.08f;
                            DrawWrapped(g, label.ToUpperInvariant(), o + t + padX, o + t, content - padX * 2, content, 15f * Px, 1.1f, 0.02f);
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

        /// <summary>
        /// The inactive tile as it sits two slots from the centre: .tile's
        /// opacity .55 and scale(.9), and #viewport's mask (transparent at the
        /// edge, solid 18% in, i.e. 100.8px of the 560px viewport). Same box as
        /// the tile, so it draws like the rendered radio_tile_*_l / _r.
        /// </summary>
        public static Art.Entry EdgeTile(Art.Entry tile, bool left)
        {
            if (tile == null) return null;
            string key = (left ? "l|" : "r|") + tile.Path;
            if (Made.TryGetValue(key, out Art.Entry done)) return done;
            Art.Entry result = null;
            try
            {
                string path = CachePath(left ? "l" : "r", tile.Path);
                if (!File.Exists(path))
                {
                    using (Bitmap src = Load(tile.Path))
                    using (var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb))
                    {
                        float k = src.Width / tile.W;   // raster px per CSS px
                        using (Graphics g = Graphics.FromImage(bmp))
                        using (var attrs = new ImageAttributes())
                        {
                            g.Clear(Color.Transparent);
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            attrs.SetColorMatrix(new ColorMatrix { Matrix33 = 0.55f });
                            // scale(.9) about the layout box's centre
                            float cx = (tile.BoxX + tile.BoxW / 2f) * k, cy = (tile.BoxY + tile.BoxH / 2f) * k;
                            float x0 = cx * 0.1f, y0 = cy * 0.1f, w = src.Width * 0.9f, h = src.Height * 0.9f;
                            g.DrawImage(src, new[] { new PointF(x0, y0), new PointF(x0 + w, y0), new PointF(x0, y0 + h) },
                                new RectangleF(0, 0, src.Width, src.Height), GraphicsUnit.Pixel, attrs);
                        }
                        // The layout box's left edge, in the 560px viewport: centre 56
                        // two slots left, 504 two slots right.
                        float boxLeft = (left ? 56f : 504f) - Icon / 2f;
                        Pixels(bmp, (int x, int y, ref byte b, ref byte g, ref byte r, ref byte a) =>
                        {
                            float local = boxLeft - tile.BoxX + (x + 0.5f) / k;
                            float m = left ? local / 100.8f : (560f - local) / 100.8f;
                            if (local < 0f || local > 560f) m = 0f;   // overflow: hidden
                            a = (byte)Math.Round(a * Math.Max(0f, Math.Min(1f, m)));
                        });
                        Save(bmp, path);
                    }
                }
                result = Copy(tile, path);
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "edge tile: " + ex.Message);
            }
            Made[key] = result;
            return result;
        }

        // ---- text for names vi_radio has no art for ------------------------------

        /// <summary>
        /// One line of #station / #artist: Anton, upper case, centred, white,
        /// text-shadow 0 2px 6px rgba(0,0,0,.75), clipped to the line box
        /// (overflow: hidden). The box is the text's advance width by the line
        /// box, as in the rendered radio_name_* pictures. GDI+, so close to the
        /// page rather than identical.
        /// </summary>
        public static Art.Entry TextLine(string text, float fontPx, float lineHeight, float spacingEm, float opacity)
        {
            if (string.IsNullOrEmpty(text)) return null;
            string key = "x|" + fontPx.ToString("0.###") + "|" + lineHeight + "|" + spacingEm + "|" + opacity + "|" + text;
            if (Made.TryGetValue(key, out Art.Entry done)) return done;
            Art.Entry result = null;
            try
            {
                string upper = text.ToUpperInvariant();
                FontFamily fam = Anton();
                float em = fontPx * Px;
                float[] pens = Advances(upper, fam, em, spacingEm, out float advance);
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
                            // the shadow: the glyphs' coverage, blurred (6px is a
                            // Gaussian of sigma 3) and moved 2px down
                            float[] cover = Coverage(glyphs, w, h, 0f, 2f * Px);
                            Blur(cover, w, h, 3f * Px);
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.Clear(Color.Transparent);
                                g.SmoothingMode = SmoothingMode.AntiAlias;
                                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            }
                            int top = (int)Math.Floor(pad), bottom = (int)Math.Ceiling(pad + lineH);
                            Paint(bmp, cover, Color.FromArgb((int)(0.75f * 255), 0, 0, 0), top, bottom);
                            float[] fill = Coverage(glyphs, w, h, 0f, 0f);
                            Paint(bmp, fill, Color.White, top, bottom);
                        }
                        if (opacity < 1f)
                            Pixels(bmp, (int x, int y, ref byte b, ref byte g, ref byte r, ref byte a) => { a = (byte)Math.Round(a * opacity); });
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

        private static float[] Coverage(GraphicsPath path, int w, int h, float dx, float dy)
        {
            var cover = new float[w * h];
            using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                using (var moved = (GraphicsPath)path.Clone())
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    using (var m = new Matrix()) { m.Translate(dx, dy); moved.Transform(m); }
                    g.FillPath(Brushes.White, moved);
                }
                Pixels(bmp, (int x, int y, ref byte b, ref byte gg, ref byte r, ref byte a) => { cover[y * w + x] = a / 255f; });
            }
            return cover;
        }

        // Three box blurs, which add up to a Gaussian of the given sigma.
        private static void Blur(float[] v, int w, int h, float sigma)
        {
            int r = Math.Max(1, (int)Math.Round((Math.Sqrt(4 * sigma * sigma + 1) - 1) / 2));
            var tmp = new float[v.Length];
            for (int pass = 0; pass < 3; pass++)
            {
                BoxPass(v, tmp, w, h, r, true);
                BoxPass(tmp, v, w, h, r, false);
            }
        }

        private static void BoxPass(float[] src, float[] dst, int w, int h, int r, bool horizontal)
        {
            int n = horizontal ? w : h, lines = horizontal ? h : w;
            float inv = 1f / (r * 2 + 1);
            for (int l = 0; l < lines; l++)
            {
                float sum = 0f;
                for (int k = -r; k <= r; k++) sum += At(src, w, h, l, Math.Max(0, Math.Min(n - 1, k)), horizontal, n);
                for (int i = 0; i < n; i++)
                {
                    Set(dst, w, l, i, horizontal, sum * inv);
                    int add = Math.Min(n - 1, i + r + 1), sub = Math.Max(0, i - r);
                    sum += At(src, w, h, l, add, horizontal, n) - At(src, w, h, l, sub, horizontal, n);
                }
            }
        }

        private static float At(float[] a, int w, int h, int line, int i, bool horizontal, int n) =>
            horizontal ? a[line * w + i] : a[i * w + line];

        private static void Set(float[] a, int w, int line, int i, bool horizontal, float value)
        {
            if (horizontal) a[line * w + i] = value; else a[i * w + line] = value;
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
