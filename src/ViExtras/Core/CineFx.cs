using System.Drawing;

namespace ViExtras
{
    /// <summary>
    /// vice_hud's cinematic overlay (#cine-fx, SetCinematicFx): the Focus
    /// effect's vignette in black with thin bars top and bottom, breathing on a
    /// 2600 ms ease-in-out between scale 1 and 1.04. The vignette is vice_hud's
    /// own CSS rendered to Art\cine_vignette.png (tools/render_fivem_art.py);
    /// the bars are drawn here so they breathe with it, as they do in CSS where
    /// they are children of the scaled element.
    /// </summary>
    internal sealed class CineFx
    {
        private const float PulseMs = 2600f;
        private long _since = -1;

        public float Strength = 1f;   // 0..2
        public float Bars = 0.032f;   // fraction of the screen's height
        public bool Pulse = true;

        public bool Active => _since >= 0;

        public void Show(bool on)
        {
            if (on && _since < 0) _since = RealTime.Ms;
            if (!on) _since = -1;
        }

        /// <summary>Call every frame while active.</summary>
        public void Draw()
        {
            if (_since < 0) return;
            float s = 1f;
            if (Pulse)
            {
                float t = (RealTime.Ms - _since) % PulseMs / PulseMs;
                // keyframes 0%, 100%: scale 1; 50%: scale 1.04; ease-in-out per segment
                float half = t < 0.5f ? t * 2f : (t - 0.5f) * 2f;
                float e = Num.EaseInOut(half);
                s = t < 0.5f ? Num.Lerp(1f, 1.04f, e) : Num.Lerp(1.04f, 1f, e);
            }

            float w = Canvas.W, h = Canvas.H;
            float strength = Num.Clamp(Strength, 0f, 2f);
            string art = Paths.Art("cine_vignette.png");
            // The art is strength 1. Above that CSS clamps each stop's alpha at 1;
            // a second, lighter pass is close enough.
            Canvas.Image(art, w * 0.5f, h * 0.5f, w * s, h * s, Canvas.Alpha(Color.White, Num.Clamp01(strength)), centered: true);
            if (strength > 1f)
                Canvas.Image(art, w * 0.5f, h * 0.5f, w * s, h * s, Canvas.Alpha(Color.White, strength - 1f), centered: true);

            float bar = Num.Clamp(Bars, 0f, 0.2f) * h;
            if (bar <= 0f) return;
            // .cine-bar: left/right -5%, top 0 / bottom 0, scaled about the centre.
            float x0 = w * 0.5f + (-0.05f * w - w * 0.5f) * s;
            float bw = w * 1.1f * s;
            float topY = h * 0.5f + (0f - h * 0.5f) * s;
            float botY = h * 0.5f + (h - bar - h * 0.5f) * s;
            Canvas.Rect(x0, topY, bw, bar * s, Color.Black);
            Canvas.Rect(x0, botY, bw, bar * s, Color.Black);
        }
    }
}
