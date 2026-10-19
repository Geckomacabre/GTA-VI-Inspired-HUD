using System;
using System.Drawing;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// The 3D minimap's routes in vice_hud's colours (config.lua waypointColour,
    /// with its light and dark companions): your own waypoint teal #47aba7, a
    /// mission or race pink #fc74a4. ViMinimap draws the waypoint route
    /// (Route.Kind 0) in RouteFillC / RouteRimC and a mission or race route
    /// (Kind 1) in RouteDestFillC / RouteDestRimC; the plain waypoint disc in
    /// WaypointFill / WaypointRimC and a mission's destination disc in DestFill /
    /// DestRimC. All eight are set once, from here (ViMinimap.dll is not changed).
    /// [Minimap] RouteColours = False leaves ViMinimap's own colours (a light
    /// blue waypoint, a pink mission).
    /// </summary>
    public sealed class MinimapColours : Script
    {
        // #47aba7 and #fc74a4, each with its light (the route ribbon's rim) and dark (a disc's rim)
        private static readonly Color Teal = Color.FromArgb(255, 71, 171, 167), TealLight = Color.FromArgb(255, 172, 217, 215), TealDark = Color.FromArgb(255, 36, 86, 84);
        private static readonly Color Pink = Color.FromArgb(255, 252, 116, 164), PinkLight = Color.FromArgb(255, 254, 192, 214), PinkDark = Color.FromArgb(255, 126, 58, 82);

        private readonly bool _enabled;
        private bool _done;
        private int _tries;

        public MinimapColours()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Minimap", "RouteColours", true);
            Interval = 1000;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            if (!_enabled || _done) return;
            if (++_tries > 30) { _done = true; Log.Write("Minimap", "ViMinimap's MiniMap was not found; route colours left as they are"); return; }
            Type map = null;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                if ((map = asm.GetType("MiniMap", false)) != null) break;
            if (map == null) return;
            _done = true;
            try
            {
                const BindingFlags S = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                FieldInfo fill = map.GetField("RouteFillC", S), rim = map.GetField("RouteRimC", S);
                FieldInfo destFill = map.GetField("RouteDestFillC", S), destRim = map.GetField("RouteDestRimC", S);
                if (fill == null || rim == null || destFill == null || destRim == null)
                {
                    Log.Write("Minimap", "ViMinimap’s route colours were not found; left as they are");
                    return;
                }
                fill.SetValue(null, Teal);
                rim.SetValue(null, TealLight);
                destFill.SetValue(null, Pink);
                destRim.SetValue(null, PinkLight);
                map.GetField("WaypointFill", S)?.SetValue(null, Teal);
                map.GetField("WaypointRimC", S)?.SetValue(null, TealDark);
                map.GetField("DestFill", S)?.SetValue(null, Pink);
                map.GetField("DestRimC", S)?.SetValue(null, PinkDark);
                Log.Write("Minimap", "route colours: waypoint teal, mission and race pink");
            }
            catch (Exception ex) { Log.Write("Minimap", "route colours: " + ex.Message); }
        }
    }
}
