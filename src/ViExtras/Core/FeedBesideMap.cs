using System;
using System.IO;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// GTA's notification feed to the right of the 3D minimap. ViMinimap switches GTA's own radar off (it draws its
    /// map in a window of its own, in the bottom-left corner), and without a radar GTA puts the feed down in that same
    /// corner, so the map covered it whenever it showed (a player: "this notifications and stuff like that appear
    /// behind the map when we are inside of cars"). While the map is on screen the feed moves right by the map's width
    /// (HUD component 18, the game stream); when the map goes, the feed goes back. Wider than 16:9, GTA's HUD corner
    /// is already clear of the map, so the feed stays ([Minimap] FeedBesideMapWide = True moves it there too).
    /// [Minimap] FeedBesideMap = False leaves it where GTA puts it.
    /// </summary>
    public sealed class FeedBesideMap : Script
    {
        private const int GameStream = 18;     // HUD_GAME_STREAM: GTA's notification feed
        // ViMinimap's map (its Renderer.Layout, in 720ths of the screen height): 208 wide; and the gap after it
        private const float MapW = 208f, Gap = 8f;
        private const float WideAspect = 1.9f;

        /// <summary>Also wider than 16:9 ([Minimap] FeedBesideMapWide).</summary>
        internal static bool Wide;

        private readonly bool _enabled;
        private readonly string _settings;
        private readonly bool _miniMap;
        private bool _mapEnabled = true, _onlyInVehicle = true, _whenWanted = true;
        private int _readAt;
        private bool _moved, _logged;

        public FeedBesideMap()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Minimap", "FeedBesideMap", true);
            Wide = Config.Get("Minimap", "FeedBesideMapWide", false);
            _settings = Path.Combine(Paths.Scripts ?? ".", "MiniMap.settings");
            _miniMap = File.Exists(Path.Combine(Paths.Scripts ?? ".", "ViMinimap.dll"));
            if (!_enabled || !_miniMap) return;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            try
            {
                ReadSettings();
                float aspect = Function.Call<float>(Hash.GET_ASPECT_RATIO, false);
                if (MapShowing() && aspect > 0.5f && (Wide || aspect < WideAspect))
                {
                    float x = (MapW + Gap) / 720f / aspect;
                    if (!_logged)
                    {
                        Vector3 before = Function.Call<Vector3>(Hash.GET_HUD_COMPONENT_POSITION, GameStream);
                        Log.Write("Feed", "moving GTA's feed beside the 3D map: by " + x.ToString("0.0000") + " of the screen width (it was at "
                            + before.X.ToString("0.0000") + ", " + before.Y.ToString("0.0000") + ")");
                    }
                    // every frame: GTA may put it back while the radar is off
                    Function.Call(Hash.SET_HUD_COMPONENT_POSITION, GameStream, x, 0f);
                    if (!_logged)
                    {
                        _logged = true;
                        Vector3 after = Function.Call<Vector3>(Hash.GET_HUD_COMPONENT_POSITION, GameStream);
                        Log.Write("Feed", "GTA's feed is now at " + after.X.ToString("0.0000") + ", " + after.Y.ToString("0.0000"));
                    }
                    _moved = true;
                }
                else if (_moved)
                {
                    Function.Call(Hash.RESET_HUD_COMPONENT_VALUES, GameStream);
                    _moved = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Feed", "beside the map: " + ex.Message);
            }
        }

        // ViMinimap's own rule: on, and on foot too, or wanted, or in anything but a helicopter or a plane
        private bool MapShowing()
        {
            if (!_mapEnabled) return false;
            if (Function.Call<bool>(Hash.IS_PAUSE_MENU_ACTIVE)) return false;
            if (!_onlyInVehicle) return true;
            if (_whenWanted && Game.Player.Wanted.WantedLevel > 0) return true;
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists() || !ped.IsInVehicle()) return false;
            Vehicle v = ped.CurrentVehicle;
            if (v == null || !v.Exists()) return false;
            VehicleClass c = v.ClassType;
            return c != VehicleClass.Helicopters && c != VehicleClass.Planes;
        }

        // MiniMap.settings, as ViMinimap writes it (its menu can change it while the game runs)
        private void ReadSettings()
        {
            int now = Environment.TickCount;
            if (_readAt != 0 && now - _readAt < 2000) return;
            _readAt = now;
            try
            {
                if (!File.Exists(_settings)) return;
                foreach (string raw in File.ReadAllLines(_settings))
                {
                    int eq = raw.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = raw.Substring(0, eq).Trim();
                    bool on = raw.Substring(eq + 1).Trim() != "0";
                    if (key == "Enabled") _mapEnabled = on;
                    else if (key == "OnlyInVehicle") _onlyInVehicle = on;
                    else if (key == "ShowWhenWanted") _whenWanted = on;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Feed", "MiniMap.settings: " + ex.Message);
            }
        }
    }
}
