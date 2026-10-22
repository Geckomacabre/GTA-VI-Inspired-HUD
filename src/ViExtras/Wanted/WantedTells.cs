using System.Collections.Generic;
using System.Drawing;

namespace ViExtras
{
    /// <summary>
    /// The wanted tells lit now, worked out by WantedBox every 250 ms (vice_hud's main loop,
    /// Config.Tick) and shown twice, as on FiveM: in the wanted box, and in the row under
    /// the stars (#tells).
    ///
    /// The row is GTA6Hud's own police icon row, which makes room for itself in the
    /// top-right stack: tells_row_patch points its "any icon on" at Count and its drawing at
    /// DrawRow (both found by name; keep them public and static).
    /// </summary>
    public static class WantedTells
    {
        // style.css .tell (1.3 --w round) and #tells (gap 0.4725 --w), 1080p canvas pixels
        private const float Disc = 24.95f, Pitch = 24.95f + 9.072f;

        private static readonly string[] None = new string[0];
        private static string[] _now = None;

        internal static string[] Now => _now;

        internal static void Set(List<string> tells) => _now = tells == null || tells.Count == 0 ? None : tells.ToArray();

        /// <summary>How many tells are lit (GTA6Hud's AnyPoliceIconOn, patched).</summary>
        public static int Count() => _now.Length;

        /// <summary>
        /// #tells, called from GTA6Hud's DrawPoliceRow (patched): the discs in vice_hud's order,
        /// the last one's right edge on `right`, their tops on `top` (canvas pixels, after
        /// GTA6Hud's own scale and fade for the cluster, which come as `scale` and `alpha`).
        /// </summary>
        public static void DrawRow(float top, float right, float scale, float alpha)
        {
            string[] tells = _now;
            if (tells.Length == 0 || alpha <= 0f) return;
            Color c = Color.FromArgb((int)(255 * Num.Clamp01(alpha)), 255, 255, 255);
            for (int i = 0; i < tells.Length; i++)
            {
                Art.Entry e = Art.Get("wanted_tell_" + tells[i]);
                if (e == null) continue;
                float x = right - (Disc + (tells.Length - 1 - i) * Pitch) * scale;
                Art.Draw(e, x, top, c, scale);
            }
        }
    }
}
