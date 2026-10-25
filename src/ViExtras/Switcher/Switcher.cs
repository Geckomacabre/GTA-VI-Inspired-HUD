using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// GTA VI's character switcher, for ViSwitch's wheel (hold the wheel key): a row of cards, Michael, Franklin and
    /// Trevor, then Jason and Lucia side by side with a heart between them, as GTA VI shows the pair. Each card has
    /// that character's honor face on its bottom edge (Save\Honor.ini: good, bad, broken, or neutral in between) and
    /// the name under it; the one picked has a white border. Lucia's and Jason's cards are there while ViSwitch can
    /// switch to them (their models are in the game).
    ///
    /// ViSwitch runs the wheel and says what to draw (ViSwitch.SwitchState, read here each frame by reflection);
    /// while this script runs, ViSwitch doesn't draw GTA6Hud's wheel.
    ///
    /// Michael's, Franklin's and Trevor's portraits are the game's own, from its switch wheel (player_switch.ytd,
    /// allcharacters), cut from that sheet as it is drawn; Lucia's is FBICat's and Jason's the player's own picture
    /// (tools/render/switcher.py).
    /// </summary>
    public sealed class Switcher : Script
    {
        // Canvas pixels (1080 high). Where FBICat's wheel was (his player_switch movie, viLayout: 106-pixel tiles in
        // a row in the bottom right corner, 30 px in from the right edge, their tops at 1016 - 14 - 106), at GTA VI's
        // own card size (106, as his are), the pair 2 px apart and the heart on their seam (tools/render/switcher.py).
        private const float Card = 106f, Gap = 12f, PairGap = 2f, FaceSize = 30f, HeartSize = 31f;
        private const float NameH = 15f, NameGap = 5f, Border = 3f;
        private const float Top = 1016f - 14f - Card, Right = 30f;
        private static readonly Color SelectedBorder = Color.FromArgb(255, 240, 240, 240);
        private static readonly Color Edge = Color.FromArgb(255, 52, 56, 70);

        // the row, left to right, by ViSwitch's numbers: 0 Michael, 1 Franklin, 2 Trevor, 3 Lucia, 4 Jason
        private static readonly int[] Row = { 0, 1, 2, 4, 3 };
        private static readonly string[] Names = { "michael", "franklin", "trevor", "lucia", "jason" };
        private static readonly string[] HonorKeys = { "Michael", "Franklin", "Trevor", "Lucia", "Jason" };

        // The game's switch wheel portraits: allcharacters (512 x 256) in player_switch.ytd holds Franklin, Chop
        // (upside down), Michael and Trevor. A 140-pixel square of each round the head. Chop is an island of his own
        // (x 140-238, y 0-136); that rectangle, 4 pixels wider all round for the texture filtering and DXT blocks,
        // is never drawn, and the card's backdrop shows there instead.
        private const string WheelTxd = "player_switch", WheelTex = "allcharacters";
        private const float SheetW = 512f, SheetH = 256f;
        private static readonly RectangleF[] Crops =
        {
            new RectangleF(210f, 34f, 140f, 140f),   // Michael
            new RectangleF(30f, 26f, 140f, 140f),    // Franklin
            new RectangleF(357f, 29f, 140f, 140f),   // Trevor
        };
        private static readonly RectangleF Chop = RectangleF.FromLTRB(136f, 0f, 243f, 141f);

        /// <summary>True while this draws the wheel ([Switcher] Enabled): ViSwitch reads it to leave GTA6Hud's wheel shut.</summary>
        public static bool Drawing;

        private readonly bool _enabled, _honorOn;
        private readonly int _honorDefault, _angelAt, _devilAt;
        private FieldInfo _blend, _selected, _available;
        private int _lookAt;
        // the honor faces, read as the wheel opens
        private readonly string[] _faces = new string[5];
        private bool _wasOpen;

        public Switcher()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Switcher", "Enabled", true);
            _honorOn = Config.Get("Honor", "Enabled", true);
            _honorDefault = Config.Get("Honor", "DefaultHonor", 0);
            _angelAt = Config.Get("Honor", "AngelThreshold", 40);
            _devilAt = Config.Get("Honor", "DevilThreshold", -40);
            if (!_enabled) return;
            Drawing = true;
            Tick += OnTick;
        }

        private bool Find()
        {
            if (_blend != null) return true;
            int now = Environment.TickCount;
            if (now - _lookAt < 2000 && _lookAt != 0) return false;
            _lookAt = now;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("ViSwitch.SwitchState", false);
                if (t == null) continue;
                const BindingFlags S = BindingFlags.Static | BindingFlags.Public;
                _selected = t.GetField("Selected", S);
                _available = t.GetField("Available", S);
                _blend = _selected != null && _available != null ? t.GetField("Blend", S) : null;
                if (_blend != null) Log.Write("Switcher", "found ViSwitch: GTA VI's row of character cards");
                return _blend != null;
            }
            return false;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            try
            {
                if (!Find()) return;
                float blend = (float)_blend.GetValue(null);
                if (blend <= 0.003f) { _wasOpen = false; return; }
                if (!_wasOpen) { _wasOpen = true; ReadFaces(); }
                int sel = (int)_selected.GetValue(null);
                bool[] avail = (bool[])_available.GetValue(null);
                Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 5);   // over the HUD (GTA6Hud draws at 4)
                try { Draw(Num.Ease(blend), sel, avail); }
                finally { Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 4); }
            }
            catch (Exception ex) { Log.Write("Switcher", ex.Message); }
        }

        private void ReadFaces()
        {
            for (int c = 0; c < 5; c++)
            {
                if (!_honorOn) { _faces[c] = null; continue; }
                HonorStore.Load(HonorKeys[c], _honorDefault, out int value, out bool broken);
                _faces[c] = broken ? "broken" : value >= _angelAt ? "good" : value <= _devilAt ? "bad" : "neutral";
            }
        }

        private void Draw(float a, int sel, bool[] avail)
        {
            var cards = new List<int>();
            foreach (int c in Row) if (c < avail.Length && avail[c]) cards.Add(c);
            if (cards.Count == 0) return;
            float width = 0f;
            for (int i = 0; i < cards.Count; i++) width += Card + (i + 1 < cards.Count ? Spacing(cards[i], cards[i + 1]) : 0f);
            float x = Canvas.W - Right - width, y = Top;
            Color white = Canvas.Alpha(Color.White, a);
            float heartX = -1f;
            for (int i = 0; i < cards.Count; i++)
            {
                int c = cards[i];
                DrawCard(c, x, y, white, a, c == sel);
                if (i + 1 < cards.Count && c == 4 && cards[i + 1] == 3) heartX = x + Card + PairGap * 0.5f;
                x += Card + (i + 1 < cards.Count ? Spacing(c, cards[i + 1]) : 0f);
            }
            // the heart on the Jason / Lucia seam, a little above the middle (GTA VI: 54 of 106)
            if (heartX >= 0f) Image("switch_heart", heartX, y + Card * 54f / 106f, HeartSize, HeartSize, white);
        }

        private static float Spacing(int left, int right) => left == 4 && right == 3 ? PairGap : Gap;

        // `part` of the sheet (inside the card's crop `crop`), where it falls on the card at x, y.
        private static void Portrait(RectangleF crop, RectangleF part, float x, float y, Color white)
        {
            if (part.Width <= 0f || part.Height <= 0f) return;
            float k = Card / crop.Width;
            Canvas.GamePart(WheelTxd, WheelTex, x + (part.Left - crop.Left) * k, y + (part.Top - crop.Top) * k, part.Width * k, part.Height * k,
                part.Left / SheetW, part.Top / SheetH, part.Right / SheetW, part.Bottom / SheetH, white);
        }

        private void DrawCard(int c, float x, float y, Color white, float a, bool selected)
        {
            Image("switch_card", x + Card * 0.5f, y + Card * 0.5f, Card, Card, white);
            if (c < 3)
            {
                RectangleF r = Crops[c];
                RectangleF chop = RectangleF.Intersect(r, Chop);
                if (chop.IsEmpty) Portrait(r, r, x, y, white);
                else
                {
                    // around Chop: the columns left and right of him, then above and below him in his columns
                    Portrait(r, RectangleF.FromLTRB(r.Left, r.Top, chop.Left, r.Bottom), x, y, white);
                    Portrait(r, RectangleF.FromLTRB(chop.Right, r.Top, r.Right, r.Bottom), x, y, white);
                    Portrait(r, RectangleF.FromLTRB(chop.Left, r.Top, chop.Right, chop.Top), x, y, white);
                    Portrait(r, RectangleF.FromLTRB(chop.Left, chop.Bottom, chop.Right, r.Bottom), x, y, white);
                }
            }
            else Image(c == 3 ? "switch_lucia" : "switch_jason", x + Card * 0.5f, y + Card * 0.5f, Card, Card, white);

            // the border: white on the card picked, a faint dark edge on the rest
            float t = selected ? Border : 1f;
            Color bc = Canvas.Alpha(selected ? SelectedBorder : Edge, a);
            Canvas.Rect(x - t, y - t, Card + 2f * t, t, bc);
            Canvas.Rect(x - t, y + Card, Card + 2f * t, t, bc);
            Canvas.Rect(x - t, y, t, Card, bc);
            Canvas.Rect(x + Card, y, t, Card, bc);

            // the honor face, centred on the card's bottom edge, and the name under it
            if (_faces[c] != null) Image("switch_face_" + _faces[c], x + Card * 0.5f, y + Card, FaceSize, FaceSize, white);
            Art.Entry name = Art.Get("switch_name_" + Names[c]);
            if (name != null)
            {
                float w = name.W * NameH / name.H;
                Canvas.Image(name.Path, x + Card * 0.5f, y + Card + FaceSize * 0.5f + NameGap + NameH * 0.5f, w, NameH, white, true);
            }
        }

        private static void Image(string art, float cx, float cy, float w, float h, Color c)
        {
            Art.Entry e = Art.Get(art);
            if (e != null) Canvas.Image(e.Path, cx, cy, w, h, c, true);
        }
    }
}
