using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using GTA.Native;
using GTA.UI;

namespace ViExtras
{
    /// <summary>
    /// Everything the extras draw goes through here.
    ///
    /// Units are "canvas pixels": the screen is 1080 tall and 1080 x aspect wide,
    /// so 1vh in vice_hud's CSS is 10.8 here.
    ///
    /// The art (scripts\ViExtras\Art) is packed into game texture dictionaries
    /// (vix_<group>.ytd, in the vihudfonts pack; tools/build_txd.py) and drawn
    /// with the game's own DRAW_SPRITE, the way GTA6Hud draws its main HUD.
    /// Script Hook V's file textures (CustomSprite) are only a fallback for
    /// pictures made at run time, and never on GTA V Enhanced: there Script
    /// Hook V stops the game once about 32 of them exist, and GTA6Hud uses
    /// those up by itself.
    /// </summary>
    internal static class Canvas
    {
        public const float H = 1080f;
        public static float W => 1080f * Screen.AspectRatio;
        private const float ToSprite = 720f / 1080f;

        private sealed class Tex
        {
            public CustomSprite Sprite;
            public int W, H;                       // the picture as asked for
            public int PadL, PadT, PadR, PadB;     // transparent margin added round it
        }

        private static readonly Dictionary<string, Tex> Sprites = new Dictionary<string, Tex>(StringComparer.OrdinalIgnoreCase);

        // Script Hook V on GTA V Enhanced (DirectX 12) cannot make a texture whose
        // width or height is not a multiple of 4, and stops the game when asked to
        // ("directx texture ... creation failed"). Legacy does not mind. Any such
        // picture is drawn from a copy with a transparent margin that brings both
        // sides up to a multiple of 4, placed so the picture itself lands exactly
        // where it would have.
        private static Tex Get(string path)
        {
            if (Sprites.TryGetValue(path, out Tex t)) return t;
            t = new Tex();
            string use = path;
            if (PngSize(path, out int w, out int h))
            {
                t.W = w;
                t.H = h;
                int w4 = (w + 3) & ~3, h4 = (h + 3) & ~3;
                if (w4 != w || h4 != h)
                {
                    t.PadL = (w4 - w) / 2; t.PadR = w4 - w - t.PadL;
                    t.PadT = (h4 - h) / 2; t.PadB = h4 - h - t.PadT;
                    try { use = Padded(path, t); }
                    catch (Exception ex)
                    {
                        Log.Write("Canvas", "could not pad " + path + ": " + ex.Message);
                        t.PadL = t.PadR = t.PadT = t.PadB = 0;
                    }
                }
            }
            t.Sprite = new CustomSprite(use, SizeF.Empty, PointF.Empty);
            Sprites[path] = t;
            return t;
        }

        // A PNG's size from its header, without decoding it.
        private static bool PngSize(string path, out int w, out int h)
        {
            w = h = 0;
            try
            {
                using (var f = File.OpenRead(path))
                {
                    var b = new byte[24];
                    if (f.Read(b, 0, 24) < 24 || b[0] != 0x89 || b[1] != (byte)'P') return false;
                    w = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19];
                    h = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23];
                    return w > 0 && h > 0;
                }
            }
            catch { return false; }
        }

        private static string Padded(string path, Tex t)
        {
            var info = new FileInfo(path);
            string key = path.ToLowerInvariant() + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks;
            string name = "pad_" + ((uint)key.GetHashCode()).ToString("x8") + "_" + Path.GetFileName(path);
            return Cache.MakeOnce(name, bmp =>
            {
                using (var src = new Bitmap(path))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(src, new Rectangle(t.PadL, t.PadT, t.W, t.H), 0, 0, t.W, t.H, GraphicsUnit.Pixel);
                }
            }, t.W + t.PadL + t.PadR, t.H + t.PadT + t.PadB);
        }

        public static bool Exists(string path) => Sprites.ContainsKey(path) || File.Exists(path);

        public static readonly bool Enhanced = IsEnhanced();

        private static bool IsEnhanced()
        {
            try { return System.Diagnostics.Process.GetCurrentProcess().ProcessName.IndexOf("Enhanced", StringComparison.OrdinalIgnoreCase) >= 0; }
            catch { return false; }
        }

        private static readonly Dictionary<string, bool> Dicts = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        // Asks for a texture dictionary; true once it is in memory. Kept loaded.
        private static bool DictReady(string txd)
        {
            if (Dicts.TryGetValue(txd, out bool ready) && ready) return true;
            Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, txd, false);
            ready = Function.Call<bool>(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, txd);
            Dicts[txd] = ready;
            return ready;
        }

        private static bool _warnedFallback;

        /// <summary>Draws an image file. x/y is the top-left corner unless centred.</summary>
        public static void Image(string path, float x, float y, float w, float h, Color color, bool centered = false, float rotation = 0f)
        {
            if (w <= 0f || h <= 0f || color.A == 0) return;
            if (Art.Native(path, out string txd, out string tex))
            {
                if (!DictReady(txd)) return;
                float cx = centered ? x : x + w * 0.5f, cy = centered ? y : y + h * 0.5f;
                Function.Call(Hash.DRAW_SPRITE, txd, tex, cx / W, cy / H, w / W, h / H, rotation,
                    (int)color.R, (int)color.G, (int)color.B, (int)color.A, false);
                return;
            }
            if (Enhanced)
            {
                if (!_warnedFallback) { _warnedFallback = true; Log.Write("Canvas", "not drawn on Enhanced (no texture dictionary): " + path); }
                return;
            }
            Tex t = Get(path);
            if (t.PadL + t.PadR + t.PadT + t.PadB > 0)
            {
                // The margin is drawn at the picture's own scale, round it.
                float sx = w / t.W, sy = h / t.H;
                if (centered)
                {
                    x += (t.PadR - t.PadL) * 0.5f * sx;
                    y += (t.PadB - t.PadT) * 0.5f * sy;
                }
                else
                {
                    x -= t.PadL * sx;
                    y -= t.PadT * sy;
                }
                w += (t.PadL + t.PadR) * sx;
                h += (t.PadT + t.PadB) * sy;
            }
            CustomSprite s = t.Sprite;
            s.Centered = centered;
            s.Position = new PointF(x * ToSprite, y * ToSprite);
            s.Size = new SizeF(w * ToSprite, h * ToSprite);
            s.Color = color;
            s.Rotation = rotation;
            s.ScaledDraw();
        }

        public static void Rect(float x, float y, float w, float h, Color color)
        {
            if (w <= 0f || h <= 0f || color.A == 0) return;
            Function.Call(Hash.DRAW_RECT, (x + w * 0.5f) / W, (y + h * 0.5f) / H, w / W, h / H,
                (int)color.R, (int)color.G, (int)color.B, (int)color.A, false);
        }

        /// <summary>
        /// Draws part of a packed picture: the rectangle u0..u1, v0..v1 of it (0..1)
        /// into x, y, w, h. How a picture is clipped at a list's edge.
        /// </summary>
        public static void ImagePart(string path, float x, float y, float w, float h, float u0, float v0, float u1, float v1, Color color)
        {
            if (w <= 0f || h <= 0f || color.A == 0 || u1 <= u0 || v1 <= v0) return;
            if (!Art.Native(path, out string txd, out string tex) || !DictReady(txd)) return;
            Function.Call(Hash.DRAW_SPRITE_ARX_WITH_UV, txd, tex, (x + w * 0.5f) / W, (y + h * 0.5f) / H, w / W, h / H,
                u0, v0, u1, v1, 0f, (int)color.R, (int)color.G, (int)color.B, (int)color.A, false);
        }

        public static Color Alpha(Color c, float a) => Color.FromArgb((int)Num.Clamp(c.A * a, 0f, 255f), c.R, c.G, c.B);
    }

    /// <summary>Files the extras make at run time, kept in %TEMP%\ViExtras.</summary>
    internal static class Cache
    {
        public static readonly string Dir = Path.Combine(Path.GetTempPath(), "ViExtras");

        public static string MakeOnce(string name, Action<Bitmap> paint, int w, int h)
        {
            Directory.CreateDirectory(Dir);
            string path = Path.Combine(Dir, name);
            if (!File.Exists(path))
            {
                using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                {
                    paint(bmp);
                    bmp.Save(path, ImageFormat.Png);
                }
            }
            return path;
        }
    }

    internal enum Face { Regular, Medium, Bold }
    internal enum Align { Left, Center, Right }

    /// <summary>
    /// Text in GTA Art Deco, the face vice_hud uses. Each string is drawn once
    /// with GDI+ into a white PNG, then tinted and scaled like any other image.
    /// </summary>
    internal static class Text
    {
        private const float Em = 96f;      // raster size of the cached strings
        private const int Pad = 6;

        private sealed class Img
        {
            public string Path;
            public float W, H;         // bitmap size, raster pixels
            public float Advance;      // pen advance of the whole string, raster pixels
            public float Ascent, Descent;
        }

        private static readonly Dictionary<string, Img> Strings = new Dictionary<string, Img>();
        private static PrivateFontCollection _fonts;
        private static readonly FontFamily[] Families = new FontFamily[3];

        private static FontFamily Family(Face face)
        {
            if (_fonts == null)
            {
                _fonts = new PrivateFontCollection();
                string[] files = { "GTAArtDecoRegular.ttf", "GTAArtDecoMedium.ttf", "GTAArtDecoBold.ttf" };
                for (int i = 0; i < files.Length; i++)
                {
                    string path = System.IO.Path.Combine(Paths.Root, "Fonts", files[i]);
                    int before = _fonts.Families.Length;
                    if (File.Exists(path)) _fonts.AddFontFile(path);
                    Families[i] = _fonts.Families.Length > before ? _fonts.Families[_fonts.Families.Length - 1] : null;
                }
                // All three files can share one family name; then match by style below.
                for (int i = 0; i < 3; i++)
                    if (Families[i] == null) Families[i] = _fonts.Families.Length > 0 ? _fonts.Families[0] : FontFamily.GenericSansSerif;
            }
            return Families[(int)face];
        }

        private static FontStyle Style(Face face, FontFamily fam)
        {
            // When the three files register as separate families, Regular is right
            // for each; when they share one, ask for Bold the GDI+ way.
            if (face == Face.Bold && fam.IsStyleAvailable(FontStyle.Bold) && Families[0] == Families[2]) return FontStyle.Bold;
            return fam.IsStyleAvailable(FontStyle.Regular) ? FontStyle.Regular : FontStyle.Bold;
        }

        private static readonly Dictionary<string, FontFamily> SystemFamilies = new Dictionary<string, FontFamily>(StringComparer.OrdinalIgnoreCase);

        // An installed font by name ("Bahnschrift", "Arial"), or GTA Art Deco when it is missing.
        private static FontFamily SystemFamily(string name)
        {
            if (!SystemFamilies.TryGetValue(name, out FontFamily fam))
            {
                try { fam = new FontFamily(name); } catch { fam = null; }
                SystemFamilies[name] = fam;
            }
            return fam;
        }

        private static Img Get(string s, Face face, float spacingEm, string system = null, bool bold = false)
        {
            string key = "v2|" + (int)face + "|" + (system ?? "") + "|" + (bold ? "b" : "") + "|" + spacingEm.ToString("0.###") + "|" + s;
            if (Strings.TryGetValue(key, out Img img)) return img;

            FontFamily fam = system != null ? SystemFamily(system) : null;
            FontStyle style;
            if (fam != null) style = bold && fam.IsStyleAvailable(FontStyle.Bold) ? FontStyle.Bold : FontStyle.Regular;
            else
            {
                fam = Family(face);
                style = Style(face, fam);
            }
            float emH = fam.GetEmHeight(style);
            float ascent = fam.GetCellAscent(style) * Em / emH;
            float descent = fam.GetCellDescent(style) * Em / emH;

            var fmt = (StringFormat)StringFormat.GenericTypographic.Clone();
            fmt.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.NoClip;
            var pens = new float[s.Length];
            float pen = 0f;
            using (var probe = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(probe))
            using (var font = new System.Drawing.Font(fam, Em, style, GraphicsUnit.Pixel))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                for (int i = 0; i < s.Length; i++)
                {
                    pens[i] = pen;
                    pen += g.MeasureString(s[i].ToString(), font, PointF.Empty, fmt).Width + spacingEm * Em;
                }
            }

            img = new Img { Advance = pen, Ascent = ascent, Descent = descent };
            // Both sides a multiple of 4: Enhanced cannot make a texture otherwise
            // (see Canvas.Get). The extra is transparent, right and bottom.
            int w = (Math.Max(1, (int)Math.Ceiling(pen) + Pad * 2) + 3) & ~3;
            int h = ((int)Math.Ceiling(ascent + descent) + Pad * 2 + 3) & ~3;
            img.W = w;
            img.H = h;
            string name = "t_" + ((uint)key.GetHashCode()).ToString("x8") + "_" + key.Length + ".png";
            img.Path = Cache.MakeOnce(name, bmp =>
            {
                using (Graphics g = Graphics.FromImage(bmp))
                using (var path = new GraphicsPath())
                {
                    g.Clear(Color.Transparent);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    for (int i = 0; i < s.Length; i++)
                        if (s[i] != ' ') path.AddString(s[i].ToString(), fam, (int)style, Em, new PointF(Pad + pens[i], Pad), fmt);
                    g.FillPath(Brushes.White, path);
                }
            }, w, h);
            Strings[key] = img;
            return img;
        }

        /// <summary>Width of the string at CSS font-size `size` (canvas pixels).</summary>
        public static float Measure(string s, float size, Face face = Face.Regular, float spacingEm = 0f, string system = null, bool bold = false)
        {
            if (string.IsNullOrEmpty(s)) return 0f;
            return Get(s, face, spacingEm, system, bold).Advance * size / Em;
        }

        /// <summary>Height of a CSS line box at line-height: normal.</summary>
        public static float LineHeight(float size, Face face = Face.Regular, string system = null, bool bold = false)
        {
            Img img = Get("A", face, 0f, system, bold);
            return (img.Ascent + img.Descent) * size / Em;
        }

        /// <summary>
        /// Draws `s` with its line box's top at y, like a CSS inline box at
        /// line-height: normal. x is the left edge, centre or right edge.
        /// `system` draws in an installed font instead of GTA Art Deco.
        /// </summary>
        public static void Draw(string s, float x, float y, float size, Color color, Align align = Align.Left, Face face = Face.Regular, float spacingEm = 0f, string system = null, bool bold = false)
        {
            if (string.IsNullOrEmpty(s) || color.A == 0) return;
            Img img = Get(s, face, spacingEm, system, bold);
            float k = size / Em;
            // CSS letter-spacing adds space after every letter, the last one included,
            // so a centred string sits half a spacing to the left; this matches it.
            float width = img.Advance * k;
            float left = align == Align.Center ? x - width * 0.5f : align == Align.Right ? x - width : x;
            Canvas.Image(img.Path, left - Pad * k, y - Pad * k, img.W * k, img.H * k, color);
        }

        /// <summary>Like Draw, with a soft drop shadow underneath.</summary>
        public static void DrawShadowed(string s, float x, float y, float size, Color color, Color shadow, float dx, float dy,
            Align align = Align.Left, Face face = Face.Regular, float spacingEm = 0f, string system = null, bool bold = false)
        {
            Draw(s, x + dx, y + dy, size, shadow, align, face, spacingEm, system, bold);
            Draw(s, x, y, size, color, align, face, spacingEm, system, bold);
        }
    }
}
