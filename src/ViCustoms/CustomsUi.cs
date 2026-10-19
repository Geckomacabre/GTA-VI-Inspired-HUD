using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Draws Leonida Customs' menu the way html/style.css lays out the page that
    /// html/app.js renders: the shade, the header with its slanted tabs, the
    /// list, the option being looked at (bottom left), the vehicle card (bottom
    /// right), the hint, the prompts, the "Purchased" toast and the dialogs.
    /// Every length below is the CSS one in vh (V() turns it into canvas px, the
    /// canvas being 1080 tall); text comes from TextKit, the fixed pieces from
    /// tools/render/customs.py, flat panels from DRAW_RECT.
    /// </summary>
    internal static class CustomsUi
    {
        private static float V(float vh) => vh * 10.8f;
        private static float W => Canvas.W;
        private static float H => Canvas.H;

        private static Color Hex(int rgb, float a = 1f) => Color.FromArgb((int)(a * 255f + 0.5f), (rgb >> 16) & 255, (rgb >> 8) & 255, rgb & 255);
        private static Color Rgba(int r, int g, int b, float a) => Color.FromArgb((int)(a * 255f + 0.5f), r, g, b);

        // :root
        private static readonly Color Ink = Hex(0x17171c), Paper = Hex(0xe9e8ea), Dim = Hex(0xa9a7b0), Pink = Hex(0xf1a2c9);
        private static readonly Color Green = Hex(0x93d9a5), Red = Hex(0xdc4450);
        private const float Track = 0.01f;   // letter-spacing .01em on the display face

        private static string Cash(int n) => "$" + Math.Max(0, n).ToString("N0", CultureInfo.InvariantCulture);
        private static string One(float f) => f.ToString("0.0", CultureInfo.InvariantCulture);

        // ---- primitives ------------------------------------------------------------------

        private static void Rect(float x, float y, float w, float h, Color c) => Canvas.Rect(x, y, w, h, c);

        private static void Art(string name, float x, float y, Color c, float scale = 1f)
        {
            ViExtras.Art.Entry e = ViExtras.Art.Get(name);
            if (e != null) ViExtras.Art.Draw(e, x, y, c, scale);
        }

        /// <summary>A picture drawn with its layout box at x, y, w, h.</summary>
        private static void ArtBox(string name, float x, float y, float w, float h, Color c, float rotation = 0f)
        {
            ViExtras.Art.Entry e = ViExtras.Art.Get(name);
            if (e == null || e.BoxW <= 0f || e.BoxH <= 0f) return;
            float sx = w / e.BoxW, sy = h / e.BoxH;
            Canvas.Image(e.Path, x - e.BoxX * sx, y - e.BoxY * sy, e.W * sx, e.H * sy, c, false, rotation);
        }

        // the slanted ends of tabs and pills: a triangle filling the box under its
        // diagonal from bottom left to top right, or (flip) above it
        private static void Slant(float x, float y, float w, float h, Color c, bool flip) =>
            ArtBox("customs_tri", x, y, w, h, c, flip ? 180f : 0f);

        private static void Text(string s, TextFace face, float size, float x, float y, Color c, TextAlign align = TextAlign.Left, bool outline = false, float maxWidth = 0f) =>
            TextKit.Draw(s, face, size, x, y, c, align, outline, face == TextFace.Display ? Track : 0f, maxWidth);

        private static float Measure(string s, TextFace face, float size) =>
            TextKit.Measure(s, face, size, face == TextFace.Display ? Track : 0f);

        private static string Up(string s) => (s ?? "").ToUpperInvariant();

        /// <summary>A horizontal gradient as strips: stops are (position 0..1, colour).</summary>
        private static void Gradient(float x, float y, float w, float h, (float At, Color C)[] stops, int strips)
        {
            float step = w / strips;
            for (int i = 0; i < strips; i++)
            {
                float t = (i + 0.5f) / strips;
                Rect(x + i * step, y, step + 0.5f, h, Sample(stops, t));
            }
        }

        private static Color Sample((float At, Color C)[] stops, float t)
        {
            if (t <= stops[0].At) return stops[0].C;
            for (int i = 1; i < stops.Length; i++)
            {
                if (t > stops[i].At) continue;
                float k = (t - stops[i - 1].At) / Math.Max(0.0001f, stops[i].At - stops[i - 1].At);
                Color a = stops[i - 1].C, b = stops[i].C;
                return Color.FromArgb((int)Num.Lerp(a.A, b.A, k), (int)Num.Lerp(a.R, b.R, k), (int)Num.Lerp(a.G, b.G, k), (int)Num.Lerp(a.B, b.B, k));
            }
            return stops[stops.Length - 1].C;
        }

        /// <summary>The key cap of an action on the device in hand (app.js glyph()).</summary>
        public static string KeyArt(CustomsMenu m, string action, string variant = "")
        {
            string id;
            if (m.PadMode)
            {
                bool ps = Glyphs.PadStyle == "ps";
                switch (action)
                {
                    case "ok": id = ps ? "ps_cross" : "x_a"; break;
                    case "back": id = ps ? "ps_circle" : "x_b"; break;
                    case "rev": id = ps ? "ps_square" : "x_x"; break;
                    case "card": id = ps ? "ps_triangle" : "x_y"; break;
                    case "prev": id = ps ? "ps_l1" : "x_lb"; break;
                    default: id = ps ? "ps_r1" : "x_rb"; break;
                }
            }
            else
            {
                switch (action)
                {
                    case "ok": id = "enter"; break;
                    case "back": id = "esc"; break;
                    case "rev": id = "r"; break;
                    case "card": id = "tab"; break;
                    case "prev": id = "q"; break;
                    default: id = "e"; break;
                }
            }
            return "customs_key_" + id + variant;
        }

        private static float KeyWidth(string art) => ViExtras.Art.Get(art)?.BoxW ?? V(2.95f);

        /// <summary>A key cap centred on (left, centre y); returns its width.</summary>
        private static float Key(string art, float x, float cy)
        {
            ViExtras.Art.Entry e = ViExtras.Art.Get(art);
            if (e == null) return 0f;
            ViExtras.Art.Draw(e, x, cy - e.BoxH * 0.5f, Color.White);
            return e.BoxW;
        }

        // =============================================================================
        // The whole menu (render)
        // =============================================================================

        public static void Draw(CustomsMenu m)
        {
            Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
            if (m.Dialog != null) { DrawDialog(m); return; }
            Shade();
            MenuView view = m.Top;
            DrawHead(m, view);
            DrawList(m, view);
            DrawDetail(m, view);
            DrawCard(m, view);
            DrawHint(m, view);
            DrawPrompts(m, m.PromptList(), H - V(3.1f), W - V(3f), V(2.75f), true);
            DrawToast(m);
        }

        // .shade: darkens the left side and the bottom edge
        private static readonly (float, Color)[] ShadeLeft =
        {
            (0f, Rgba(30, 31, 44, .9f)), (.16f, Rgba(30, 31, 44, .74f)), (.32f, Rgba(30, 31, 44, .3f)), (.46f, Rgba(30, 31, 44, 0f)),
        };

        private static void Shade()
        {
            Gradient(0f, 0f, W * 0.46f, H, Rescale(ShadeLeft, 0.46f), 96);
            // linear-gradient(0deg, rgba(22,22,32,.5) 0%, transparent 20%): bottom up
            int strips = 48;
            float top = H * 0.8f, step = (H - top) / strips;
            for (int i = 0; i < strips; i++)
            {
                float t = 1f - (i + 0.5f) / strips;    // 1 at the top of the band, 0 at the bottom
                Rect(0f, top + i * step, W, step + 0.5f, Rgba(22, 22, 32, 0.5f * (1f - t)));
            }
        }

        private static (float, Color)[] Rescale((float At, Color C)[] stops, float span)
        {
            var o = new (float, Color)[stops.Length];
            for (int i = 0; i < stops.Length; i++) o[i] = (stops[i].At / span, stops[i].C);
            return o;
        }

        // =============================================================================
        // Header (renderHead) and the slanted tabs (tabsHtml)
        // =============================================================================

        private static void DrawHead(CustomsMenu m, MenuView view)
        {
            if (view.Type == "root")
            {
                float x = V(3.3f), y = V(1.75f);
                Text(Up(m.ShopLabel), TextFace.Display, V(3.46f), x, y, Hex(0xe9e7ea));
                var labels = new List<string>();
                foreach (CustomsTab t in m.Logic.Tabs) labels.Add(t.Label);
                Tabs(m, labels, view.Tab, x, y + V(3.46f) + V(1.15f), V(3.3f), V(4.4f), V(.75f));
            }
            else
            {
                float x = V(2.6f), y = V(2.15f);
                string crumb = Up(view.Crumb) + " ";
                Text(crumb, TextFace.Display, V(3.05f), x, y, Dim);
                Text("/", TextFace.Display, V(3.05f), x + Measure(crumb, TextFace.Display, V(3.05f)), y, Hex(0xd2d0d6));
                Text(Up(view.Title), TextFace.Display, V(3.9f), x, y + V(3.05f) + V(.65f), Hex(0xecebee));
            }
        }

        /// <summary>
        /// A strip of slanted tabs (.tabs / .tab) with its bumper keys at the ends
        /// when there are several. Returns its width.
        /// </summary>
        private static float Tabs(CustomsMenu m, List<string> labels, int active, float x, float y, float h, float marginSingle, float marginSeveral)
        {
            bool several = labels.Count > 1;
            float slant = V(.95f), size = V(2.6f);
            float cy = y + h * 0.5f;
            float pen = x + (several ? marginSeveral : marginSingle);
            float start = pen;
            if (several) pen += Key(KeyArt(m, "prev", "_bump"), pen, cy) + V(.95f);
            for (int i = 0; i < labels.Count; i++)
            {
                bool first = i == 0, last = i == labels.Count - 1;
                string text = Up(labels[i]);
                float padL = first ? V(1.5f) : V(1.45f) + slant / 2f;
                float padR = last && several ? V(1.5f) : V(1.45f) + slant / 2f;
                if (!several) { padL = V(1.5f); padR = V(1.5f); }
                if (!first) pen += V(.45f) - slant;
                float w = padL + Measure(text, TextFace.Display, size) + padR;
                bool on = i == active;
                // clip-path: the first is straight on the left, the last on the right, a single tab not slanted at all
                bool slantL = several && !first, slantR = several && !last;
                Color bg = on ? Paper : Rgba(58, 60, 74, .8f);
                float coreL = pen + (slantL ? slant : 0f), coreR = pen + w - (slantR ? slant : 0f);
                Rect(coreL, y, coreR - coreL, h, bg);
                if (slantL) Slant(pen, y, slant, h, bg, false);
                if (slantR) Slant(pen + w - slant, y, slant, h, bg, true);
                if (!on)
                {
                    // inset 0 0 0 1px rgba(255,255,255,.13), cut by the clip-path along the slants
                    Color edge = Rgba(255, 255, 255, .13f);
                    Rect(pen + (slantL ? slant : 0f), y, w - (slantL ? slant : 0f), 1f, edge);
                    Rect(pen, y + h - 1f, w - (slantR ? slant : 0f), 1f, edge);
                    if (!slantL) Rect(pen, y, 1f, h, edge);
                    if (!slantR) Rect(pen + w - 1f, y, 1f, h, edge);
                }
                Text(text, TextFace.Display, size, pen + padL, y + (h - size) * 0.5f + V(.075f), on ? Ink : Hex(0xe3e3e8));
                pen += w;
            }
            if (several) pen += V(.95f) + Key(KeyArt(m, "next", "_bump"), pen + V(.95f), cy);
            return pen - start;
        }

        // =============================================================================
        // The list (renderMenu)
        // =============================================================================

        private const int ListRows = 8;

        private static void DrawList(CustomsMenu m, MenuView view)
        {
            var rows = m.Rows(view, out int sel);
            if (rows == null) return;
            int offset = view.Offset;
            if (sel < offset) offset = sel;
            if (sel >= offset + ListRows) offset = sel - ListRows + 1;
            view.Offset = offset;

            float x = V(3.2f), y = V(14f), w = V(45.6003f), rh = V(5.85f), size = V(2.4f);
            for (int n = 0; n < ListRows && offset + n < rows.Count; n++)
            {
                int i = offset + n;
                var row = rows[i];
                float top = y + n * rh;
                bool on = i == sel;
                if (on)
                    Gradient(x, top, w, rh, new[] { (0f, Rgba(233, 233, 236, .95f)), (.45f, Rgba(233, 233, 236, .93f)), (1f, Rgba(240, 240, 243, .38f)) }, 32);
                else
                    Rect(x, top + rh - 1f, w, 1f, Rgba(255, 255, 255, .13f));
                Color ink = on ? Hex(0x1d1d22) : Hex(0xdcdce0);
                // the right-hand side: the check, or what is fitted now
                float right = x + w - V(1.5f), rightW = 0f;
                if (row.Have)
                {
                    rightW = V(2.7f);
                    ArtBox("customs_have", right - rightW, top + (rh - rightW) * 0.5f, rightW, rightW, Color.White);
                }
                else if (!string.IsNullOrEmpty(row.Right))
                {
                    float s = V(1.8f);
                    rightW = Math.Min(V(20f), Measure(row.Right, TextFace.Body, s));
                    Text(row.Right, TextFace.Body, s, right, top + (rh - s) * 0.5f, Color.FromArgb((int)(.62f * 255), ink), TextAlign.Right, false, V(20f));
                }
                float labelX = x + V(1.5f);
                float labelMax = right - (rightW > 0f ? rightW + V(1.2f) : 0f) - labelX;
                Text(row.Label, TextFace.Body, size, labelX, top + (rh - size) * 0.5f, ink, TextAlign.Left, false, labelMax);
            }
            if (rows.Count > ListRows)
                Text((sel + 1) + " / " + rows.Count, TextFace.Body, V(1.7f), V(4.7f), V(61.9f), Dim, TextAlign.Left, true);
        }

        // =============================================================================
        // Bottom left: the option looked at (renderDetail)
        // =============================================================================

        private static void DrawDetail(CustomsMenu m, MenuView view)
        {
            float x = V(2.6f), bottom = H - V(1.95f);
            if (view.Type == "item" || view.Type == "kits")
            {
                CustomsKit kit = view.Type == "kits" ? m.Logic.Kits[view.Sel] : null;
                string label = kit != null ? kit.Label : view.Options[view.Sel].Label;
                int count = kit != null ? m.Logic.Kits.Count : view.Options.Count;
                string status = kit != null ? m.KitStatus(kit.Id) : m.StatusOf(view.Slot.Id, view.Options[view.Sel]);
                int price = kit != null ? kit.Price : view.Options[view.Sel].Price;

                float h = V(3.45f) + V(3.9f);
                if (count > 1) h += V(2.6f) + V(1.7f);
                if (status != "stock") h += 1f + V(1.55f) + V(2.9f);
                float y = bottom - h;
                Text(Up(label), TextFace.Display, V(3.45f), x, y, Hex(0xecebee), TextAlign.Left, true);
                y += V(3.45f) + V(3.9f);
                if (count > 1)
                {
                    Counter(x + V(4.4f), y, view.Sel + 1, count);
                    y += V(2.6f) + V(1.7f);
                }
                if (status != "stock")
                {
                    Line(x, y);
                    y += 1f + V(1.55f);
                    Status(x, y, status, price);
                }
            }
            else if (view.Type == "swatch")
            {
                SwatchGroup group = view.Groups[view.G];
                CustomsOption option = group.Options[view.Sel];
                float h = V(3.45f) + V(.8f) + V(2.85f) + V(2.8f) + V(10.6f) + V(3.4f) + V(3.6f) + V(2.8f) + V(2.9f);
                float y = bottom - h;
                Text(Up(option.Label), TextFace.Display, V(3.45f), x, y, Hex(0xecebee), TextAlign.Left, true);
                y += V(3.45f) + V(.8f);
                string finish = option.Finish != null && CustomsData.FinishLabels.TryGetValue(option.Finish, out string f) ? f : view.Slot.Label;
                Text(Up(finish), TextFace.Display, V(2.85f), x, y, Hex(0xe4e3e7), TextAlign.Left, true);
                y += V(2.85f) + V(2.8f);
                Swatches(m, view, group, x + V(.3f), y);
                y += V(10.6f) + V(3.4f);
                if (view.Groups.Count > 1)
                {
                    var names = new List<string>();
                    foreach (SwatchGroup g in view.Groups) names.Add(g.Name);
                    Tabs(m, names, view.G, x, y, V(3.6f), V(4.8f), V(1.15f));
                }
                y += V(3.6f) + V(2.8f);
                string status = m.StatusOf(view.Slot.Id, option);
                if (status != "stock") Status(x, y, status, option.Price);
            }
        }

        private static void Counter(float x, float y, int n, int count)
        {
            float s = V(2.3f), ty = y + (V(2.6f) - s) * 0.5f;
            Color c = Hex(0xd6d5da);
            string a = n.ToString(CultureInfo.InvariantCulture);
            Text(a, TextFace.Body, s, x, ty, c, TextAlign.Left, true);
            x += Measure(a, TextFace.Body, s) + V(1.25f);
            Text("/", TextFace.Body, s, x, ty, Color.FromArgb((int)(.75f * 255), c), TextAlign.Left, true);
            x += Measure("/", TextFace.Body, s) + V(1.25f);
            Text(count.ToString(CultureInfo.InvariantCulture), TextFace.Body, s, x, ty, c, TextAlign.Left, true);
        }

        private static void Line(float x, float y) =>
            Gradient(x, y, V(60f), 1f, new[] { (0f, Rgba(255, 255, 255, .34f)), (.65f, Rgba(255, 255, 255, .22f)), (1f, Rgba(255, 255, 255, 0f)) }, 40);

        // statusHtml: "Applied" with its check, or "Not owned" and the price
        private static void Status(float x, float y, string status, int price)
        {
            float h = V(2.9f), s = V(2.45f), cy = y + h * 0.5f;
            Color c = Hex(0xdedde2);
            if (status == "applied")
            {
                x += V(.15f);
                ArtBox("customs_have", x, cy - V(1.35f), V(2.7f), V(2.7f), Color.White);
                x += V(2.7f) + V(1.1f);
                Text("Applied", TextFace.Body, s, x, cy - s * 0.5f, c, TextAlign.Left, true);
            }
            else
            {
                Text("Not owned", TextFace.Body, s, x, cy - s * 0.5f, c, TextAlign.Left, true);
                x += Measure("Not owned", TextFace.Body, s) + V(2.2f);
                float ps = V(2.3f);
                Text(price > 0 ? Cash(price) : "FREE", TextFace.Display, ps, x, cy - ps * 0.5f + V(.05f), Color.White, TextAlign.Left, true);
            }
        }

        private const int SwatchRow = 7;

        private static void Swatches(CustomsMenu m, MenuView view, SwatchGroup group, float x, float y)
        {
            int offset = view.Offset;
            if (view.Sel < offset) offset = view.Sel;
            if (view.Sel >= offset + SwatchRow) offset = view.Sel - SwatchRow + 1;
            if (offset + SwatchRow > group.Options.Count) offset = Math.Max(0, group.Options.Count - SwatchRow);
            view.Offset = offset;
            float size = V(10.6f), gap = V(.75f);
            float pen = x;
            for (int n = 0; n < SwatchRow && offset + n < group.Options.Count; n++)
            {
                int i = offset + n;
                CustomsOption o = group.Options[i];
                Swatch(m, view, o, pen, y, size, i == view.Sel);
                pen += size + gap;
            }
            if (group.Options.Count > SwatchRow)
            {
                float s = V(2f);
                Text((view.Sel + 1) + " / " + group.Options.Count, TextFace.Display, s, pen + V(1.2f), y + (size - s) * 0.5f, Dim, TextAlign.Left, true);
            }
        }

        private static void Swatch(CustomsMenu m, MenuView view, CustomsOption o, float x, float y, float size, bool sel)
        {
            // background (colors.js swatchOf)
            if (o.Hex.HasValue) Rect(x, y, size, size, o.Hex.Value);
            else if (o.Paint >= 0)
            {
                Color hex = CustomsLogic.PaintColour(o.Paint);
                switch (o.Finish)
                {
                    case "chameleon":
                        if (ViExtras.Art.Get("customs_cham_" + o.Paint) != null) ArtBox("customs_cham_" + o.Paint, x, y, size, size, Color.White);
                        else Rect(x, y, size, size, hex);
                        break;
                    case "chrome": ArtBox("customs_sw_chrome", x, y, size, size, Color.White); break;
                    case "metal": Rect(x, y, size, size, hex); ArtBox("customs_sw_metal", x, y, size, size, Color.White); break;
                    case "matte": Rect(x, y, size, size, hex); break;
                    default: Rect(x, y, size, size, hex); ArtBox("customs_sw_sheen", x, y, size, size, Color.White); break;
                }
            }
            else ArtBox("customs_sw_none", x, y, size, size, Color.White);
            // border: 1px solid rgba(255,255,255,.2)
            Color edge = Rgba(255, 255, 255, .2f);
            Rect(x, y, size, 1f, edge); Rect(x, y + size - 1f, size, 1f, edge);
            Rect(x, y, 1f, size, edge); Rect(x + size - 1f, y, 1f, size, edge);
            string status = m.StatusOf(view.Slot.Id, o);
            if (status == "applied") ArtBox("customs_have_sm", x + size - V(.8f) - V(3f), y + size - V(.8f) - V(3f), V(3f), V(3f), Color.White);
            else if (status == "none") ArtBox("customs_sw_mark", x + V(.9f), y + size - V(.9f) - V(3.8f), V(3.8f), V(3.8f), Color.White);
            if (sel)
            {
                // box-shadow: 0 0 0 .3vh #f1f1f3
                float r = V(.3f);
                Color ring = Hex(0xf1f1f3);
                Rect(x - r, y - r, size + 2f * r, r, ring); Rect(x - r, y + size, size + 2f * r, r, ring);
                Rect(x - r, y, r, size, ring); Rect(x + size, y, r, size, ring);
            }
        }

        // =============================================================================
        // The vehicle card (renderCard)
        // =============================================================================

        private static void DrawCard(CustomsMenu m, MenuView view)
        {
            string page = m.CardPage();
            float cw = V(39f), x = W - V(2.5f) - cw;
            float headH = V(6.6f), footH = V(4.5f);
            List<(string Label, string Option)> parts = page == "parts" ? m.Fitted() : null;
            float bodyH = page == "stats" ? V(1.1f) + 5 * V(5.6f) + V(.35f)
                : page == "parts" ? V(.9f) + Math.Min(parts.Count, 9) * V(2.9f) + (parts.Count > 9 ? V(2.9f) : 0f) + (parts.Count == 0 ? V(2.9f) : 0f) + V(1f) : 0f;
            float total = headH + (page == "name" ? 0f : bodyH + footH);
            float top = H - V(8.1f) - total;

            // .dots: the page key and a dot per page, above the card's right end
            {
                string key = KeyArt(m, "card", "_mini");
                float rowH = Math.Max(V(1.5f), ViExtras.Art.Get(key)?.BoxH ?? V(2.3f));
                float cy = top - V(2.75f) + rowH * 0.5f;
                float dotsW = KeyWidth(key) + V(.35f) + V(.7f) + 2f * V(1.05f) + V(1.5f) + 2f * V(.7f);
                float px = x + cw - V(4.6f) - dotsW;
                px += Key(key, px, cy) + V(.35f) + V(.7f);
                foreach (string p in new[] { "name", "stats", "parts" })
                {
                    if (p == page)
                    {
                        ArtBox("customs_disc", px, cy - V(.75f), V(1.5f), V(1.5f), Hex(0xf1f1f3));
                        px += V(1.5f);
                    }
                    else
                    {
                        ArtBox("customs_ring", px, cy - V(.525f), V(1.05f), V(1.05f), Color.White);
                        px += V(1.05f);
                    }
                    px += V(.7f);
                }
            }

            // .card-head with the maker's mark behind it
            Rect(x, top, cw, headH, Rgba(45, 38, 56, .86f));
            Text(Up(m.Brand), TextFace.Display, V(2.2f), x + V(1.2f), top + V(.95f) + (V(2.3f) - V(2.2f)) * 0.5f, Hex(0xf1f0f3));
            Text(Up(m.Model), TextFace.Display, V(2.45f), x + V(1.2f), top + V(.95f) + V(2.3f) + (V(2.6f) - V(2.45f)) * 0.5f, Hex(0xb9b6be));
            Emblem(m, x, top, cw, headH);
            if (page == "name") return;

            float y = top + headH;
            Rect(x, y, cw, bodyH, Rgba(214, 214, 220, .2f));
            Rect(x, y, 1f, bodyH, Rgba(255, 255, 255, .26f));
            Rect(x + cw - 1f, y, 1f, bodyH, Rgba(255, 255, 255, .26f));
            float footY = y + bodyH;
            Rect(x, footY, cw, footH, Rgba(40, 38, 52, .88f));
            float fs = V(2.5f);
            if (page == "stats")
            {
                CustomsStats s = m.Shown;
                float sx = x + 1f + V(1.15f), sw = cw - 2f - 2f * V(1.15f);
                for (int i = 0; i < 5; i++) Stat(CustomsStats.Labels[i], s.Bars[i], sx, y + V(1.1f) + i * V(5.6f), sw);
                Text("RATING", TextFace.Display, fs, x + V(1.2f), footY + (footH - fs) * 0.5f, Hex(0xd9d8dc));
                Ratings(s, x + cw - V(1.2f) + V(.45f), footY + footH * 0.5f);
            }
            else
            {
                float px = x + V(1.2f), pw = cw - 2f * V(1.2f), ps = V(1.75f);
                float py = y + V(.9f);
                if (parts.Count == 0) Text("This vehicle is factory standard.", TextFace.Body, ps, px, py + (V(2.9f) - ps) * 0.5f, Hex(0xe6e5e8), TextAlign.Left, true);
                for (int i = 0; i < Math.Min(parts.Count, 9); i++)
                {
                    float ty = py + i * V(2.9f) + (V(2.9f) - ps) * 0.5f;
                    float lw = Measure(parts[i].Label, TextFace.Body, ps);
                    Text(parts[i].Label, TextFace.Body, ps, px, ty, Hex(0xe6e5e8), TextAlign.Left, true);
                    Text(parts[i].Option, TextFace.Body, ps, px + pw, ty, Color.FromArgb((int)(.78f * 255), Hex(0xe6e5e8)), TextAlign.Right, true, pw - lw - V(1.2f));
                }
                if (parts.Count > 9)
                    Text("and " + (parts.Count - 9) + " more", TextFace.Body, ps, px, py + 9 * V(2.9f) + (V(2.9f) - ps) * 0.5f, Hex(0xe6e5e8), TextAlign.Left, true);
                Text("FITTED", TextFace.Display, fs, x + V(1.2f), footY + (footH - fs) * 0.5f, Hex(0xd9d8dc));
                Pill(new List<(string, Color, string)> { (parts.Count.ToString(CultureInfo.InvariantCulture), Color.White, null) }, x + cw - V(1.2f) + V(.45f), footY + footH * 0.5f);
            }
        }

        // .emblem: 15.5vh, right -1.75vh, vertically centred, opacity .2, cropped by the head
        private static void Emblem(CustomsMenu m, float x, float top, float cw, float headH)
        {
            if (m.EmblemTex == null) return;
            float size = V(15.5f);
            float ex = x + cw + V(1.75f) - size, ey = top + headH * 0.5f - size * 0.5f;
            float clipR = x + cw;
            float u1 = Math.Min(1f, (clipR - ex) / size);
            float v0 = (top - ey) / size, v1 = (top + headH - ey) / size;
            float dw = size * u1, dy = top, dh = headH;
            Function.Call(Hash.DRAW_SPRITE_ARX_WITH_UV, "vipause", m.EmblemTex, (ex + dw * 0.5f) / W, (dy + dh * 0.5f) / H, dw / W, dh / H,
                0f, v0, u1, v1, 0f, 255, 255, 255, (int)(.2f * 255), false);
        }

        // .stat: label, the value pill, the bar
        private static void Stat(string label, CustomsStat s, float x, float y, float w)
        {
            float ls = V(2.5f);
            Text(Up(label), TextFace.Display, ls, x, y + (V(2.9f) - ls) * 0.5f, Hex(0xf3f3f5), TextAlign.Left, true);
            float delta = (float)Math.Round((s.Value - s.Owned) * 10f) / 10f;
            // .stat-val
            float vs = V(2.35f), vh = V(2.8f);
            string num = One(s.Value);
            float numPadL = delta != 0f ? V(.55f) : V(.75f);
            float numW = Math.Max(V(3.5f), numPadL + Measure(num, TextFace.Display, vs) + V(.55f));
            float right = x + w, cut = V(.75f);
            float numX = right - numW;
            // clip-path: polygon(.75vh 0, 100% 0, 100% 100%, 0 100%): the left end of the pill is cut on the slant
            if (delta != 0f) Rect(numX, y, numW, vh, Hex(0x1a1920));
            else { Rect(numX + cut, y, numW - cut, vh, Hex(0x1a1920)); Slant(numX, y, cut, vh, Hex(0x1a1920), false); }
            Text(num, TextFace.Display, vs, numX + numPadL + (numW - numPadL - V(.55f)) * 0.5f, y + (V(2.9f) - vs) * 0.5f, Color.White, TextAlign.Center);
            if (delta != 0f)
            {
                string d = One(Math.Abs(delta));
                string arrow = delta > 0f ? "↑" : "↓";
                float aw = Measure(arrow, TextFace.Display, V(1.75f));
                float dw = V(1.35f) + aw + V(.35f) + Measure(d, TextFace.Display, vs) + V(.75f);
                float left = numX - dw;
                Rect(left + cut, y, dw - cut, vh, Hex(0x262a33));
                Slant(left, y, cut, vh, Hex(0x262a33), false);
                Color dc = delta > 0f ? Green : Red;
                Text(arrow, TextFace.Display, V(1.75f), left + V(1.35f), y + (V(2.9f) - V(1.75f)) * 0.5f - V(.15f), dc);
                Text(d, TextFace.Display, vs, left + V(1.35f) + aw + V(.35f), y + (V(2.9f) - vs) * 0.5f, dc);
            }

            // .bar
            float by = y + V(3.2f), bh = V(1.2f);
            Color edge = Hex(0x22222b);
            Rect(x, by, w, 1f, edge);
            Rect(x, by + bh - 1f, w, 1f, edge);
            Rect(x, by + 1f, 1f, bh - 2f, edge);
            Rect(x + w - 1f, by + 1f, 1f, bh - 2f, edge);
            float ix = x + 1f, iw = w - 2f, iy = by + 1f, ih = bh - 2f;
            Rect(ix, iy, iw, ih, Rgba(176, 176, 183, .42f));
            float low = Math.Min(s.Value, s.Owned) / 10f, high = Math.Max(s.Value, s.Owned) / 10f;
            Rect(ix, iy, iw * low, ih, Hex(0xe4e4e7));
            Rect(ix + iw * low - 1f, iy, 1f, ih, Hex(0x22222b));
            if (delta != 0f)
            {
                Rect(ix + iw * low, iy, iw * (high - low), ih, delta > 0f ? Hex(0x8fd6a0) : Hex(0xd9434d));
                Rect(ix + iw * high - 1f, iy, 1f, ih, Hex(0x22222b));
            }
            if (Math.Abs(s.Stock - s.Value) >= 0.05f)
            {
                float tx = ix + iw * s.Stock / 10f - V(.28f), tw = V(.55f);
                Rect(tx, iy - V(.5f), tw, ih + 2f * V(.5f), Hex(0x1d1d25));
                Rect(tx + 1f, iy - V(.5f) + 1f, tw - 2f, ih + 2f * V(.5f) - 2f, Hex(0xf5f5f6));
            }
        }

        // .ratings: the off-road and road ratings, with their changes
        private static void Ratings(CustomsStats s, float right, float cy)
        {
            var items = new List<(string, Color, string)>();
            if (s.Dirt != s.DirtOwned) items.Add(((s.Dirt > s.DirtOwned ? "↑" : "↓") + Math.Abs(s.Dirt - s.DirtOwned), s.Dirt > s.DirtOwned ? Green : Red, "delta"));
            items.Add((s.Dirt.ToString(CultureInfo.InvariantCulture), Color.White, null));
            items.Add((null, Color.White, "customs_rate_dirt"));
            if (s.Road != s.RoadOwned) items.Add(((s.Road > s.RoadOwned ? "↑" : "↓") + Math.Abs(s.Road - s.RoadOwned), s.Road > s.RoadOwned ? Green : Red, "delta"));
            items.Add((s.Road.ToString(CultureInfo.InvariantCulture), Color.White, null));
            items.Add((null, Color.White, "customs_rate_road"));
            Pill(items, right, cy);
        }

        /// <summary>The dark slanted pill at the right of the card's foot, right-aligned on `right`.</summary>
        private static void Pill(List<(string Text, Color C, string Kind)> items, float right, float cy)
        {
            float h = V(3.4f), gap = V(.7f), fs = V(2.5f), ds = V(2.2f), icon = V(2.7f);
            float inner = 0f;
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                inner += it.Text == null ? icon : Measure(it.Text, TextFace.Display, it.Kind == "delta" ? ds : fs);
                if (it.Text == null && i < items.Count - 1) inner += V(.4f);
                if (i > 0) inner += gap;
            }
            float w = V(1.5f) + inner + V(.55f);
            float x = right - w, y = cy - h * 0.5f;
            float slant = V(.9f);
            Color bg = Hex(0x1c1b24);
            Rect(x + slant, y, w - slant, h, bg);
            Slant(x, y, slant, h, bg, false);
            float pen = x + V(1.5f);
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (i > 0) pen += gap;
                if (it.Text == null)
                {
                    ArtBox(it.Kind, pen, cy - icon * 0.5f, icon, icon, Color.White);
                    pen += icon;
                    if (i < items.Count - 1) pen += V(.4f);
                }
                else
                {
                    float s = it.Kind == "delta" ? ds : fs;
                    Text(it.Text, TextFace.Display, s, pen, cy - s * 0.5f, it.C);
                    pen += Measure(it.Text, TextFace.Display, s);
                }
            }
        }

        // =============================================================================
        // Hint, prompts, toast, dialog
        // =============================================================================

        private static void DrawHint(CustomsMenu m, MenuView view)
        {
            string hint = m.Hint(view);
            if (string.IsNullOrEmpty(hint)) return;
            float s = V(2.35f);
            float lh = TextKit.LineHeightNormal(TextFace.Body, s);
            Text(hint, TextFace.Body, s, V(3.3f), H - V(3f) - lh + (lh - s) * 0.5f, Hex(0xd9d8dd), TextAlign.Left, true);
        }

        /// <summary>#prompts / .prompts: label then key, right to left from `right`, bottom at `bottom`.</summary>
        private static void DrawPrompts(CustomsMenu m, List<(string Label, string Act, bool Hold)> list, float bottom, float right, float size, bool outline)
        {
            float h = V(2.95f), cy = bottom - h * 0.5f, pen = right;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                var p = list[i];
                string key = KeyArt(m, p.Act, p.Hold ? "_hold" : "");
                ViExtras.Art.Entry e = ViExtras.Art.Get(key);
                float kw = e?.BoxW ?? V(2.95f);
                pen -= kw;
                Key(key, pen, cy);
                pen -= V(1.25f);
                string label = Up(p.Label);
                Text(label, TextFace.Display, size, pen, cy - size * 0.5f + V(.1f), Hex(0xf4f4f6), TextAlign.Right, outline);
                pen -= Measure(label, TextFace.Display, size) + V(2.3f);
            }
        }

        private static void DrawToast(CustomsMenu m)
        {
            long age = RealTime.Ms - m.ToastAt;
            if (m.ToastAt < 0 || age > 2600) return;
            float k = Num.Clamp01(age / 250f);
            float p = 1f - (1f - k) * (1f - k);   // ease-out
            float s = V(2.45f);
            float right = W - V(3.6f) + V(1.5f) * (1f - p), cy = V(38.6f) + V(1.45f);
            Color c = Color.FromArgb((int)(p * 255), 0xf4, 0xf4, 0xf6);
            ArtBox("customs_have", right - V(2.7f), cy - V(1.35f), V(2.7f), V(2.7f), Color.FromArgb((int)(p * 255), 255, 255, 255));
            Text("Purchased", TextFace.Body, s, right - V(2.7f) - V(1f), cy - s * 0.5f, c, TextAlign.Right, true);
        }

        private static void DrawDialog(CustomsMenu m)
        {
            CustomsMenu.DialogState d = m.Dialog;
            float top = H - V(29.4f);
            Rect(0f, top, W, V(29.4f), Hex(0x272831));
            Text(Up(d.Title), TextFace.Display, V(4.75f), V(5.7f), top + V(7.2f), Hex(0xe3e1e4));
            Gradient(V(5.7f), top + V(14.4f), V(58f), V(.2f), new[] { (0f, Rgba(196, 190, 200, .78f)), (.55f, Rgba(205, 160, 215, .7f)), (1f, Rgba(120, 110, 150, .08f)) }, 48);
            float s = V(2.25f), lh = V(3.75f);
            for (int i = 0; i < d.Lines.Count; i++)
                Text(d.Lines[i], TextFace.Body, s, V(5.7f), top + V(17.1f) + i * lh + (lh - s) * 0.5f, Hex(0xd6d6db));
            var list = new List<(string, string, bool)> { ("OK", "ok", false) };
            if (d.Back != null) list.Add(("Back", "back", false));
            DrawPrompts(m, list, H - V(3.2f), W - V(3.4f), V(2.2f), false);
        }
    }
}
