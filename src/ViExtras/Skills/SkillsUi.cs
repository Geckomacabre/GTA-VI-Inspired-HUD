using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ViExtras
{
    /// <summary>One row of the skills panel, as app.js onSkills builds it.</summary>
    internal struct SkillRow
    {
        public string Id;
        public int Level;
        public bool Max;
        /// <summary>The bar, 0..1 (.sk-fill scaleX).</summary>
        public float Frac;
        /// <summary>The sub line reads "into / need · unit"; otherwise it is the blurb.</summary>
        public bool ShowXp;
        public long Into, Need;
    }

    /// <summary>
    /// The skills panel (#skills), drawn from
    /// the pieces tools/render/skills.py cuts out of vice_hud's page and laid
    /// out again the way style.css lays them out. Every number below is CSS px
    /// at 1080p, measured off the page in Edge at 1920x1080 (the CSS source of
    /// each is in its comment).
    /// </summary>
    internal static class SkillsUi
    {
        // ---- #skills-panel ------------------------------------------------------
        private const float PanelW = 844.8f;       // width: calc(44 * var(--w))
        private const float PanelMaxH = 842.39f;   // max-height: 78cqh
        private const float HeadH = 64.16f;        // #skills-head: padding 1.5cqh 1.4w 1.1cqh + the title's line box + 1px line
        private const float FootH = 64.59f;        // #skills-foot: padding 1.0cqh, two lines of 0.8w text, 1px line
        private const float ListPadY = 8.64f;      // #skills-list padding: 0.8cqh 1.4w
        private const float ListPadX = 26.88f;
        private const float PlatePad = 14f;        // skills_plate is cut with this much room for its shadow
        private const float CloseX = 777.99f, CloseY = 21.33f;  // #skills-close (margin-left: auto) inside the panel

        // ---- .sk rows -----------------------------------------------------------
        // .sk { padding: 0.85cqh 0 } + .sk-head (flex, baseline) + .sk-track
        // (0.62cqh, margin 0.55cqh 0 0.4cqh) + .sk-sub. The head line is the
        // level's 30px line box; a maxed row's MAX is 0.86w, so the name's 26px
        // line box sets it instead and the row is 4px shorter.
        private const float RowH = 86.28f, RowHMax = 82.28f;    // + 1 for .sk + .sk's border-top
        private const float NameY = 12.17f, NameYMax = 9.17f;
        private const float LevelY = 9.17f, LevelYMax = 12.17f;
        private const float TrackY = 45.11f, TrackYMax = 41.11f;
        private const float SubY = 56.11f, SubYMax = 52.11f;
        private const float TrackH = 6.69f;
        private const float FillCapW = 8f;                      // skills_fill_cap
        private const float RenderedRowW = 791.0531f;             // the row width the pieces were cut at
        private static readonly Color Line = Color.FromArgb(26, 255, 255, 255);    // --g-line rgba(255,255,255,0.10)
        private static readonly Color Thumb = Color.FromArgb(41, 255, 255, 255);   // ::-webkit-scrollbar-thumb rgba(255,255,255,0.16)

        /// <summary>Where the panel sits and how its list scrolls, for one set of rows.</summary>
        internal sealed class Layout
        {
            public float X, Y, H;
            public float ListTop, ClientH, ContentH;
            public bool Scrollbar;
            public float RowX, RowW;
            public float MaxScroll => Math.Max(0f, ContentH - ClientH);
            public RectangleF Close;
        }

        private static float DevicePx => 1080f / Math.Max(1, GTA.UI.Screen.Resolution.Height);

        public static Layout Measure(IList<SkillRow> rows)
        {
            float content = ListPadY * 2f;
            for (int i = 0; i < rows.Count; i++) content += (rows[i].Max ? RowHMax : RowH) + (i > 0 ? 1f : 0f);
            float h = Math.Min(PanelMaxH, HeadH + content + FootH);
            var l = new Layout
            {
                H = h,
                X = (Canvas.W - PanelW) * 0.5f,
                Y = (Canvas.H - h) * 0.5f,      // #skills: flex, centred
                ContentH = content,
            };
            l.ListTop = l.Y + HeadH;
            l.ClientH = h - HeadH - FootH;
            // CEF draws #skills-list::-webkit-scrollbar (0.34w) whenever the list
            // overflows, and it takes its width out of the rows.
            l.Scrollbar = content > l.ClientH + 0.01f;
            Art.Entry thumbEnd = Art.Get("skills_thumb_top");
            float bar = l.Scrollbar && thumbEnd != null ? thumbEnd.BoxW : 0f;
            l.RowX = l.X + ListPadX;
            l.RowW = PanelW - ListPadX * 2f - bar;
            Art.Entry close = Art.Get("skills_close");
            l.Close = new RectangleF(l.X + CloseX, l.Y + CloseY, close != null ? close.BoxW : 40f, close != null ? close.BoxH : 30f);
            return l;
        }

        public static void DrawPanel(IList<SkillRow> rows, Layout l, float scroll, bool closeHover)
        {
            Art.Entry scrim = Art.Get("skills_scrim");
            // radial-gradient(120% 100% at 50% 45%, ...) is sized off the box, so
            // the 16:9 picture stretched to any screen is the same gradient.
            if (scrim != null) Canvas.Image(scrim.Path, 0f, 0f, Canvas.W, Canvas.H, Color.White);

            // The surface, stretched to this height (it was cut at the full 78cqh).
            Art.Entry plate = Art.Get("skills_plate");
            if (plate != null)
            {
                float sy = l.H / plate.BoxH;
                Canvas.Image(plate.Path, l.X - plate.BoxX, l.Y - plate.BoxY * sy, plate.W, plate.H * sy, Color.White);
            }
            Art.Draw(Art.Get("skills_head"), l.X, l.Y, Color.White);
            Art.Draw(Art.Get(closeHover ? "skills_close_hover" : "skills_close"), l.Close.X, l.Close.Y, Color.White);
            Art.Draw(Art.Get("skills_foot"), l.X, l.Y + l.H - FootH, Color.White);

            // #skills-list is overflow-y: auto: rows are cut at its edges.
            _clipTop = l.ListTop;
            _clipBottom = l.ListTop + l.ClientH;
            float y = l.ListTop + ListPadY - scroll;
            float px = DevicePx;
            for (int i = 0; i < rows.Count; i++)
            {
                SkillRow r = rows[i];
                if (i > 0)
                {
                    // .sk + .sk { border-top: 1px solid var(--g-line) }: one screen pixel.
                    float a = Math.Max(y, _clipTop), b = Math.Min(y + px, _clipBottom);
                    if (b > a) Canvas.Rect(l.RowX, a, l.RowW, b - a, Line);
                    y += 1f;
                }
                DrawRow(r, l, y);
                y += r.Max ? RowHMax : RowH;
            }

            if (l.Scrollbar) DrawThumb(l, scroll);
        }

        // ---- clipping ------------------------------------------------------------
        // A piece that crosses the list's edge is drawn only as far as the edge,
        // by its texture coordinates (Canvas.ImagePart).

        private static float _clipTop, _clipBottom;

        /// <summary>Canvas.Image for a piece inside the list: cut at the list's edges.</summary>
        private static void Piece(string path, float x, float y, float w, float h)
        {
            if (path == null || w <= 0f || h <= 0f) return;
            if (y >= _clipTop && y + h <= _clipBottom) { Canvas.Image(path, x, y, w, h, Color.White); return; }
            if (y + h <= _clipTop || y >= _clipBottom) return;
            // The part of the picture inside the list, drawn by texture coordinates.
            float v0 = Num.Clamp01((_clipTop - y) / h), v1 = Num.Clamp01((_clipBottom - y) / h);
            if (v1 <= v0) return;
            Canvas.ImagePart(path, x, y + h * v0, w, h * (v1 - v0), 0f, v0, 1f, v1, Color.White);
        }

        /// <summary>Art.Draw for a piece inside the list.</summary>
        private static void Piece(Art.Entry e, float boxX, float boxY)
        {
            if (e != null) Piece(e.Path, boxX - e.BoxX, boxY - e.BoxY, e.W, e.H);
        }

        private static void DrawRow(SkillRow r, Layout l, float y)
        {
            float x = l.RowX, w = l.RowW;
            Piece(Art.Get("skills_name_" + r.Id), x, y + (r.Max ? NameYMax : NameY));

            // Level, right-aligned (.sk-name is flex: 1 1 auto). Tabular figures,
            // so every digit is one advance.
            float ly = y + (r.Max ? LevelYMax : LevelY);
            if (r.Max)
            {
                Art.Entry max = Art.Get("skills_lvl_max");
                if (max != null) Piece(max, x + w - max.BoxW, ly);
            }
            else
            {
                string digits = Math.Max(0, r.Level).ToString();
                Art.Entry d0 = Art.Get("skills_lvl_0");
                if (d0 != null)
                {
                    float lx = x + w - digits.Length * d0.BoxW;
                    foreach (char c in digits)
                    {
                        Piece(Art.Get("skills_lvl_" + c), lx, ly);
                        lx += d0.BoxW;
                    }
                }
            }

            float ty = y + (r.Max ? TrackYMax : TrackY);
            float sx = w / RenderedRowW;
            Art.Entry track = Art.Get("skills_track");
            if (track != null) Piece(track.Path, x - track.BoxX * sx, ty - track.BoxY, track.W * sx, track.H);
            DrawFill(r, x, ty, w);

            float sy = y + (r.Max ? SubYMax : SubY);
            if (!r.Max && r.ShowXp)
            {
                Art.Entry d0 = Art.Get("skills_sub_0");
                Art.Entry slash = Art.Get("skills_sub_slash");
                if (d0 == null || slash == null) return;
                // app.js: s.into + ' / ' + s.need + '  ·  ' + s.unit (the spaces
                // collapse to one); each piece is cut as wide as its own run.
                float at = x;
                at = Digits(r.Into, at, sy, d0.BoxW);
                Piece(slash, at, sy);
                at += slash.BoxW;
                at = Digits(r.Need, at, sy, d0.BoxW);
                Piece(Art.Get("skills_sub_tail_" + r.Id), at, sy);
            }
            else Piece(Art.Get("skills_blurb_" + r.Id), x, sy);
        }

        private static float Digits(long value, float x, float y, float advance)
        {
            foreach (char c in Math.Max(0, value).ToString())
            {
                Piece(Art.Get("skills_sub_" + c), x, y);
                x += advance;
            }
            return x;
        }

        // .sk-fill: the full-width gradient scaled from the left (scaleX), cut by
        // the track's rounded ends (overflow: hidden; border-radius: 999px).
        private static void DrawFill(SkillRow r, float x, float y, float w)
        {
            float frac = Num.Clamp01(r.Frac);
            if (frac <= 0f) return;
            float sx = w / RenderedRowW;
            float fw = frac * w;
            if (r.Max || fw >= w - TrackH * 0.5f)
            {
                Art.Entry full = Art.Get(r.Max ? "skills_fill_max" : "skills_fill");
                if (full != null) Piece(full.Path, x - full.BoxX * sx, y - full.BoxY, full.W * sx, full.H);
                return;
            }
            Art.Entry cap = Art.Get("skills_fill_cap"), body = Art.Get("skills_fill_body");
            if (cap == null || body == null) return;
            if (fw <= FillCapW)
            {
                Piece(cap.Path, x, y, fw, TrackH);
                return;
            }
            Piece(cap.Path, x, y, FillCapW, TrackH);
            Piece(body.Path, x + FillCapW, y, fw - FillCapW, TrackH);
        }

        private static void DrawThumb(Layout l, float scroll)
        {
            Art.Entry topEnd = Art.Get("skills_thumb_top"), bottomEnd = Art.Get("skills_thumb_bottom");
            if (topEnd == null || bottomEnd == null) return;
            float track = l.ClientH;
            float len = Math.Max(topEnd.BoxH * 2f, track * l.ClientH / l.ContentH);
            float at = l.MaxScroll > 0f ? scroll / l.MaxScroll : 0f;
            float tx = l.X + PanelW - topEnd.BoxW;
            float ty = l.ListTop + at * (track - len);
            Canvas.Image(topEnd.Path, tx, ty, topEnd.BoxW, topEnd.BoxH, Color.White);
            Canvas.Rect(tx, ty + topEnd.BoxH, topEnd.BoxW, len - topEnd.BoxH - bottomEnd.BoxH, Thumb);
            Canvas.Image(bottomEnd.Path, tx, ty + len - bottomEnd.BoxH, bottomEnd.BoxW, bottomEnd.BoxH, Color.White);
        }
    }
}
