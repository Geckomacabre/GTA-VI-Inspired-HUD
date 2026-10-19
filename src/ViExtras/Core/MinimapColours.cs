using System;
using System.Drawing;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// The 3D minimap's route colours the way the player wants them, in vice_hud's
    /// own waypoint colours (config.lua waypointColour, with its light and dark
    /// companions): their own waypoint pink, a mission's teal. ViMinimap draws the
    /// waypoint route (Route.Kind 0) in RouteFillC / RouteRimC and a mission or race
    /// route (Kind 1) in RouteDestFillC / RouteDestRimC; the plain waypoint disc in
    /// WaypointFill / WaypointRimC and a mission's destination disc in DestFill /
    /// DestRimC. All eight are set once, from here (ViMinimap.dll is not changed).
    /// [Minimap] WaypointRoutePink = False leaves ViMinimap's own colours.
    /// </summary>
    public sealed class MinimapColours : Script
    {
        private readonly bool _enabled;
        private bool _done;
        private int _tries;

        public MinimapColours()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Minimap", "WaypointRoutePink", false);
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
                FieldInfo wpFill = map.GetField("WaypointFill", S), wpRim = map.GetField("WaypointRimC", S);
                if (fill == null || rim == null || destFill == null || destRim == null)
                {
                    Log.Write("Minimap", "ViMinimap’s route colours were not found; left as they are");
                    return;
                }
                // #fc74a4 and #47aba7, light (rims of the route ribbon) and dark (rims of the discs)
                Color pink = Color.FromArgb(255, 252, 116, 164), pinkLight = Color.FromArgb(255, 254, 192, 214), pinkDark = Color.FromArgb(255, 126, 58, 82);
                Color teal = Color.FromArgb(255, 71, 171, 167), tealLight = Color.FromArgb(255, 172, 217, 215), tealDark = Color.FromArgb(255, 36, 86, 84);
                fill.SetValue(null, pink);
                rim.SetValue(null, pinkLight);
                destFill.SetValue(null, teal);
                destRim.SetValue(null, tealLight);
                wpFill?.SetValue(null, pink);
                wpRim?.SetValue(null, pinkDark);
                map.GetField("DestFill", S)?.SetValue(null, teal);
                map.GetField("DestRimC", S)?.SetValue(null, tealDark);
                Log.Write("Minimap", "route colours: waypoint pink, mission teal");
            }
            catch (Exception ex) { Log.Write("Minimap", "route colours: " + ex.Message); }
        }
    }
}
