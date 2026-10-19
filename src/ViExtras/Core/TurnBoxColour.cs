using System;
using System.Drawing;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// FBICat's turn-by-turn box in the route's colour. His box (vipause
    /// hud_gps_bg) has a pink square for the arrow, drawn as it is. It follows
    /// your own waypoint's GPS route, which the minimap draws teal
    /// (MinimapColours), so the square is painted teal over his, from a mask of
    /// it (tools/gps_turn_accent.py), with his arrow drawn again on top. It shows
    /// only on frames GTA6Hud drew its box (Gta6HudCard.TurnBox). Off with
    /// [Minimap] RouteColours = False.
    /// </summary>
    public sealed class TurnBoxColour : Script
    {
        private static readonly Color Teal = Color.FromArgb(255, 71, 171, 167);   // vice_hud's waypoint teal, #47aba7
        private const float BoxH = 60f, DesignW = 303f, ArrowX = 249.5f, ArrowY = 10f, ArrowSize = 40f;   // UpdateTurnNavigation's layout

        private readonly bool _enabled;

        public TurnBoxColour()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Minimap", "RouteColours", true);
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled || !GameReady.Now) return;
            if (!Gta6HudCard.TurnBox(out float left, out float top, out float width, out float blend, out bool leftTurn)) return;
            Art.Entry mask = Art.Get("gps_turn_accent");
            if (mask == null) return;
            int alpha = (int)(255f * blend);
            Canvas.Image(mask.Path, left, top, width, BoxH, Color.FromArgb(alpha, Teal));
            Gta6HudCard.Tex(leftTurn ? "hud_gps_left" : "hud_gps_right", left + width - DesignW + ArrowX, top + ArrowY, ArrowSize, ArrowSize, Color.FromArgb(alpha, 255, 255, 255));
        }
    }
}
