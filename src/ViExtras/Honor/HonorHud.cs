using System;
using System.Drawing;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// vice_hud's two honor displays, for single player:
    ///
    ///   * the centre pop (#honor-pop): a sign and a face for one DEED, shown
    ///     2200 ms (onHonorPop in html/app.js);
    ///   * the corner panel (#honor): the mugshot and the badge for where honor
    ///     STANDS, shown Config.Honor.holdMs (onHonor).
    ///
    /// The calls mirror vice_hud's client.lua: Seed = SetHonorStanding,
    /// ShowToast = ShowHonorToast, ShowDeed = ShowHonorDeed, including the
    /// "last value seen" the toast measures its change against. Neither display
    /// has an animation in vice_hud (both toggle display: none), so neither
    /// has one here.
    ///
    /// The pictures come from vice_hud's own page (tools/render/honor.py). The
    /// mugshot is the one thing that cannot be a picture: it is a ped headshot,
    /// a game texture, drawn natively, with the mug's gradient drawn natively
    /// under it. ScriptHookV sprites always draw above native drawing, so the
    /// panel picture has a hole cut where the mug is, and the badge goes on top.
    /// </summary>
    internal sealed class HonorHud
    {
        private const int PopMs = 2200;           // onHonorPop: setTimeout(..., 2200)
        private const int ShotTimeoutMs = 2000;   // give up on the headshot, show without it

        // #honor: right 0.25cqw, bottom 5.24cqh (style.css ~1637).
        private const float RightCqw = 0.25f;
        private const float Bottom = 5.24f * 10.8f;
        // Measured off the rendered #honor (.bare) by tools/render/honor.py, in
        // CSS px from the panel's top-left corner:
        //   #honor-mug border box +9.594,+9.719, 81.594 x 83.578, 1px border
        //     (padding 0.9cqh 0.5--w; width 4.25--w, height 7.74cqh)
        //   #honor-badge-wrap +40.797,+84.984, 19.188 square
        //     (left 50%, bottom -1.1cqh, translateX(-50%), 1.0--w square)
        private const float MugX = 9.594f, MugY = 9.719f, MugW = 81.594f, MugH = 83.578f, MugBorder = 1f;
        private const float BadgeX = 40.7973f, BadgeY = 84.984f;

        // #honor-mug background: linear-gradient(155deg, #8d7f74, #4a423c),
        // sized to its padding box. Drawn as a grid of native rects; at this
        // cell size neighbouring cells differ by under two levels of 8 bits.
        private static readonly double GradAngle = 155.0 * Math.PI / 180.0;
        private static readonly Color GradFrom = Color.FromArgb(0x8d, 0x7f, 0x74);
        private static readonly Color GradTo = Color.FromArgb(0x4a, 0x42, 0x3c);
        private const float CellW = 6.4f, CellH = 3f;
        private const float Bleed = 0.5f;
        private static float[] _colX, _colW, _colMx;   // DrawGradient's columns, this frame

        private readonly int _holdMs;
        private readonly Func<int, string> _tierOf;

        // client.lua: lastHonor (nil until known) and lastHonorBroken
        private int? _lastHonor;
        private bool _lastBroken;

        // what is on screen
        private string _popArt;
        private long _popUntil;
        private bool _panelOn;
        private long _panelUntil;          // 0: until something hides it
        private string _badge;             // "angel", "devil", "broken" or null
        private Headshot _shot;            // the panel's mugshot, or null

        // a toast waiting for its mugshot (client/main.lua's getMugshot runs first)
        private Message _pending;
        private Headshot _pendingShot;
        private long _pendingSince;

        private sealed class Message
        {
            public bool PopOnly;
            public int Delta;
            public string Severity;
            public bool Broken;
            public string Emoji;   // the tier face: "angel", "devil" or null
        }

        private sealed class Headshot
        {
            public int Handle;
            public string Txd;
            public bool Ready;
        }

        public HonorHud(int holdMs, Func<int, string> tierOf)
        {
            _holdMs = holdMs;
            _tierOf = tierOf;
        }

        /// <summary>
        /// SetHonorStanding: where honor stands, drawing nothing. `character`
        /// starts a new character's record: in FiveM one session is one
        /// character, so lastHonorBroken only ever latches; here the player
        /// switches between characters with their own standings, so it is reset.
        /// </summary>
        public void Seed(int honor, bool broken, bool character)
        {
            _lastHonor = honor;
            if (character)
            {
                _lastBroken = broken;
                HidePanel();
                _popArt = null;
                DropPending();
            }
            else if (broken) _lastBroken = true;
        }

        /// <summary>
        /// ShowHonorToast: the panel for the current standing, and the pop.
        /// Waits for a fresh mugshot of `ped` first, as client/main.lua's
        /// pushToHud does. A preview leaves the remembered standing alone.
        /// </summary>
        public void ShowToast(Ped ped, int honor, bool broken, string severity, bool preview = false)
        {
            int delta = 0;
            if (_lastHonor.HasValue && honor != _lastHonor.Value) delta = honor - _lastHonor.Value;
            bool sendBroken = _lastBroken || broken;
            if (!preview)
            {
                _lastHonor = honor;
                if (broken) _lastBroken = true;
            }

            _pending = new Message
            {
                Delta = delta,
                Severity = severity,
                Broken = sendBroken,
                Emoji = _tierOf(honor),
            };
            if (_pendingShot == null)
            {
                _pendingShot = Register(ped);
                _pendingSince = RealTime.Ms;
            }
        }

        /// <summary>ShowHonorDeed: the centre pop and nothing else.</summary>
        public void ShowDeed(int delta, string severity, bool broken)
        {
            if (broken) _lastBroken = true;
            Apply(new Message { PopOnly = true, Delta = delta, Severity = severity, Broken = _lastBroken }, null);
        }

        // onHonor (app.js), minus the copy column: Config.Honor.showValue is
        // false, the reference treatment, so the panel is the .bare one.
        private void Apply(Message d, Headshot shot)
        {
            if (d.Delta != 0 || d.Severity != null || d.Broken)
            {
                bool up = d.Delta != 0 ? d.Delta > 0 : d.Severity == "good";
                string face = d.Broken || d.Severity == "terrible" ? "terrible" : up ? "good" : "bad";
                // onHonorPop: broken has no sign (and the crack); otherwise + / -.
                _popArt = d.Broken ? "honor_pop_broken" : "honor_pop_" + (up ? "up_" : "down_") + face;
                _popUntil = RealTime.Ms + PopMs;
            }

            if (d.PopOnly)
            {
                Release(shot);
                return;
            }

            if (_shot != shot) Release(_shot);
            _shot = shot;
            // Once broken the badge is the terrible face, greyed, with the crack.
            _badge = d.Broken ? "broken" : d.Emoji;
            _panelOn = true;
            _panelUntil = _holdMs > 0 ? RealTime.Ms + _holdMs : 0;
        }

        private void HidePanel()
        {
            _panelOn = false;
            Release(_shot);
            _shot = null;
        }

        private void DropPending()
        {
            _pending = null;
            Release(_pendingShot);
            _pendingShot = null;
        }

        /// <summary>Lets go of every headshot (the script is stopping).</summary>
        public void Shutdown()
        {
            DropPending();
            HidePanel();
            _popArt = null;
        }

        // ---- headshots -------------------------------------------------------------

        private static Headshot Register(Ped ped)
        {
            if (ped == null || !ped.Exists()) return null;
            int handle = Function.Call<int>(Hash.REGISTER_PEDHEADSHOT_TRANSPARENT, ped);
            return new Headshot { Handle = handle };
        }

        private static bool Poll(Headshot shot)
        {
            if (shot == null) return false;
            if (!shot.Ready
                && Function.Call<bool>(Hash.IS_PEDHEADSHOT_VALID, shot.Handle)
                && Function.Call<bool>(Hash.IS_PEDHEADSHOT_READY, shot.Handle))
            {
                shot.Txd = Function.Call<string>(Hash.GET_PEDHEADSHOT_TXD_STRING, shot.Handle);
                shot.Ready = !string.IsNullOrEmpty(shot.Txd);
            }
            return shot.Ready;
        }

        private static void Release(Headshot shot)
        {
            if (shot == null) return;
            Function.Call(Hash.UNREGISTER_PEDHEADSHOT, shot.Handle);
            shot.Ready = false;
            shot.Txd = null;
        }

        // ---- every frame -----------------------------------------------------------

        // Where the player has moved them to (canvas px, 1080 tall), from the
        // layout editor: vice_hud's /movehud offsets for "Honor +/- popup" and
        // the honor panel. 0, 0 is vice_hud's own place. The popup's X is in
        // 1920-wide px from the centre and stretches with the screen's width
        // (PopX), so a popup moved out to one side stays at that side on a
        // wider or narrower screen.
        public float PopOffsetX, PopOffsetY, PanelOffsetX, PanelOffsetY;

        /// <summary>The layout editor is open: both are shown, and the chosen one outlined.</summary>
        public bool Editing;
        public bool EditingPanel;

        public void Tick(bool hidden)
        {
            long now = RealTime.Ms;

            if (_pending != null)
            {
                bool ready = Poll(_pendingShot);
                if (ready || _pendingShot == null || now - _pendingSince > ShotTimeoutMs)
                {
                    Headshot shot = _pendingShot;
                    if (!ready)
                    {
                        // No mugshot: vice_hud hides the <img> and the mug's
                        // gradient shows on its own.
                        Release(shot);
                        shot = null;
                    }
                    Message msg = _pending;
                    _pending = null;
                    _pendingShot = null;
                    Apply(msg, shot);
                }
            }

            if (Editing)
            {
                // Both stay up, whatever their timers say, so they can be placed.
                if (!_panelOn) { _panelOn = true; _panelUntil = 0; }
                if (_badge == null) _badge = "devil";
                _popUntil = now + 1000;
                if (_popArt == null) _popArt = "honor_pop_down_bad";
            }
            else
            {
                if (_popArt != null && now >= _popUntil) _popArt = null;
                if (_panelOn && _panelUntil > 0 && now >= _panelUntil) HidePanel();
            }

            if (hidden && !Editing) return;
            if (_panelOn) DrawPanel();
            if (_popArt != null) DrawPop();
        }

        /// <summary>Puts both away after the editor closes.</summary>
        public void EndEditing()
        {
            Editing = false;
            _popArt = null;
            HidePanel();
        }

        // A thin outline round what the editor is moving.
        private static void Outline(float x, float y, float w, float h)
        {
            Color c = Color.FromArgb(220, 255, 255, 255);
            const float t = 2f;
            Canvas.Rect(x - t, y - t, w + 2 * t, t, c);
            Canvas.Rect(x - t, y + h, w + 2 * t, t, c);
            Canvas.Rect(x - t, y, t, h, c);
            Canvas.Rect(x + w, y, t, h, c);
        }

        private void DrawPop()
        {
            // #honor-pop: left 50%, top 50%, translate(-50%, -50%), plus the
            // editor's offset (applied after the centring, as vice_hud does).
            Art.Entry e = Art.Get(_popArt);
            if (e == null) return;
            float x = (Canvas.W - e.BoxW) * 0.5f + PopX, y = (Canvas.H - e.BoxH) * 0.5f + PopOffsetY;
            Art.Draw(e, x, y, Color.White);
            if (Editing && !EditingPanel) Outline(x, y, e.BoxW, e.BoxH);
        }

        /// <summary>PopOffsetX on this screen: 960 is the right edge of a 1920 x 1080 one.</summary>
        private float PopX => PopOffsetX * Canvas.W / 1920f;

        private void DrawPanel()
        {
            Art.Entry panel = Art.Get("honor_panel");
            if (panel == null) return;
            float x = Canvas.W - Canvas.W * RightCqw / 100f - panel.BoxW + PanelOffsetX;
            float y = Canvas.H - Bottom - panel.BoxH + PanelOffsetY;
            if (Editing && EditingPanel) Outline(x, y, panel.BoxW, panel.BoxH);

            // The mug's padding box: under the hole in the panel picture.
            float px = x + MugX + MugBorder, py = y + MugY + MugBorder;
            float pw = MugW - MugBorder * 2f, ph = MugH - MugBorder * 2f;
            Function.Call(Hash.RESET_SCRIPT_GFX_ALIGN);
            DrawGradient(px, py, pw, ph);
            if (_shot != null && _shot.Ready)
            {
                // <img> object-fit: cover. The headshot is square, so it is the
                // taller side square, centred; the ~1px it overhangs either side
                // lies under the mug's border ring, which the picture keeps opaque.
                float s = Math.Max(pw, ph);
                Function.Call(Hash.DRAW_SPRITE, _shot.Txd, _shot.Txd,
                    (px + pw * 0.5f) / Canvas.W, (py + ph * 0.5f) / Canvas.H, s / Canvas.W, s / Canvas.H,
                    0f, 255, 255, 255, 255, false, 0);
            }

            Art.Draw(panel, x, y, Color.White);
            if (_badge != null)
            {
                Art.Entry badge = Art.Get("honor_badge_" + _badge);
                if (badge != null) Art.Draw(badge, x + BadgeX, y + BadgeY, Color.White);
            }
        }

        private static void DrawGradient(float x, float y, float w, float h)
        {
            // CSS: the gradient line runs through the box centre at 155deg
            // (0deg = up, clockwise), long enough that its ends touch the corners.
            float dx = (float)Math.Sin(GradAngle), dy = -(float)Math.Cos(GradAngle);
            float len = Math.Abs(w * dx) + Math.Abs(h * dy);
            float cx = x + w * 0.5f, cy = y + h * 0.5f;
            int cols = Math.Max(1, (int)Math.Ceiling(w / CellW));
            int rows = Math.Max(1, (int)Math.Ceiling(h / CellH));
            // A few hundred rects: what is the same for a whole row or column is
            // worked out once, by the same sums as before, so not a pixel moves.
            float sw = Canvas.W;
            if (_colX == null || _colX.Length < cols) { _colX = new float[cols]; _colW = new float[cols]; _colMx = new float[cols]; }
            for (int c = 0; c < cols; c++)
            {
                float x0 = x + w * c / cols, x1 = x + w * (c + 1) / cols;
                _colMx[c] = (x0 + x1) * 0.5f;
                if (c == 0) x0 -= Bleed;
                if (c == cols - 1) x1 += Bleed;
                _colX[c] = (x0 + x1) * 0.5f / sw;
                _colW[c] = (x1 - x0) / sw;
            }
            float fromR = GradFrom.R, fromG = GradFrom.G, fromB = GradFrom.B, toR = GradTo.R, toG = GradTo.G, toB = GradTo.B;
            for (int r = 0; r < rows; r++)
            {
                float y0 = y + h * r / rows, y1 = y + h * (r + 1) / rows;
                float my = (y0 + y1) * 0.5f;
                // The outer cells reach half a pixel under the mug's (opaque)
                // border ring, so the anti-aliased rim of the hole never shows
                // the world through it.
                if (r == 0) y0 -= Bleed;
                if (r == rows - 1) y1 += Bleed;
                float rowY = (y0 + y1) * 0.5f / Canvas.H, rowH = (y1 - y0) / Canvas.H, rowT = (my - cy) * dy;
                for (int c = 0; c < cols; c++)
                {
                    float t = Num.Clamp01(((_colMx[c] - cx) * dx + rowT) / len + 0.5f);
                    Function.Call(Hash.DRAW_RECT, _colX[c], rowY, _colW[c], rowH,
                        (int)Math.Round(Num.Lerp(fromR, toR, t)),
                        (int)Math.Round(Num.Lerp(fromG, toG, t)),
                        (int)Math.Round(Num.Lerp(fromB, toB, t)),
                        255, false);
                }
            }
        }
    }
}
