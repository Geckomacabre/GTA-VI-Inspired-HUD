// Pictures the radio wheel makes at run time: the muted look of a picture
// that has no muted copy packed beside it. Everything else is drawn from the
// texture dictionaries (tools/render/radio.py) or live: a station vi_radio
// has no logo for, and your own music's stations, get a text tile drawn by
// RadioHud with TextKit's Anton, so on Enhanced too.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ViExtras
{
    internal static class RadioImages
    {
        private const string Version = "r4";

        private static readonly Dictionary<string, Art.Entry> Made = new Dictionary<string, Art.Entry>();

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

        // ---- helpers ------------------------------------------------------------

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
