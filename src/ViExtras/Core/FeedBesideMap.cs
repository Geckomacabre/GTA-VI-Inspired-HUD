using System;
using System.IO;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// GTA's notification feed clear of the 3D minimap. ViMinimap switches GTA's own radar off (it draws its map in a
    /// window of its own, in the bottom-left corner), and without a radar GTA puts the feed down in that same corner,
    /// so the map covered it whenever it showed (a player: "this notifications and stuff like that appear behind the
    /// map when we are inside of cars"). Lifting the feed over the map depended on the screen's shape and the vehicle
    /// card, so the feed hangs from a fixed place instead, newest on top: where GTA VI's wanted box goes (WantedBox,
    /// vice_hud's #wanted), and while the box is up, under it.
    ///
    /// GTA won't move its feed for a script (SET_HUD_COMPONENT_POSITION on it is ignored), and only draws it in a
    /// narrow column at the left, so the HUD's own feed movie (vihud's game_stream.gfx) does it: it takes numbers no
    /// colour uses from THEFEED_SET_BACKGROUND_COLOR_FOR_NEXT_POST, 7000 + t for the feed's top t (of 720) down, and
    /// keeps it 190 clear of the screen's bottom (the map and the vehicle card). GTA hands that number over with the
    /// next post only, so it's sent all the time, and the move under the wanted box, which can't wait for a post, goes
    /// through THEFEED_SET_SCRIPTED_MENU_HEIGHT: GTA keeps the feed under a script's menu at once, and the movie keeps
    /// its top under that line. [Minimap] FeedBesideMap = False leaves it where GTA puts it.
    ///
    /// Not on Legacy: there GTA's own feed movie is the one on screen (it's loaded for the loading screen, before the
    /// vihud pack is), so nothing reads these numbers as a place, and GTA's movie takes each one as the next post's
    /// background colour, which no colour has, and washes the post out to a pale card the white text can't be read
    /// on (tested 2026-10-25: pale cards in GTA's own place with this on, GTA's dark cards with it off).
    /// </summary>
    public sealed class FeedBesideMap : Script
    {
        // From the 1080 tall canvas to the feed movie's 720; how far over the movie's line a post's box starts, and the
        // gap between the wanted box and the feed under it, on the canvas (measured in game, 2026-10-22)
        private const float ToMovie = 720f / Canvas.H, PostEdge = 17f, Gap = 12f;
        private const int TopSignal = 7000;
        private const int ResendMs = 500;

        private int _sentAt;
        private bool _logged, _under;

        public FeedBesideMap()
        {
            Paths.Init(BaseDirectory);
            bool enabled = Config.Get("Minimap", "FeedBesideMap", true);
            bool miniMap = File.Exists(Path.Combine(Paths.Scripts ?? ".", "ViMinimap.dll"));
            if (!enabled || !miniMap) return;
            if (!Canvas.Enhanced)
            {
                Log.Write("Feed", "Legacy: GTA's feed stays where GTA puts it (the HUD's feed movie can't replace GTA's here)");
                return;
            }
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            try
            {
                // where the feed hangs from, for the next post to carry (and again after a loading screen)
                int now = Environment.TickCount;
                if (now - _sentAt > ResendMs)
                {
                    if (!_logged) Log.Write("Feed", "GTA's feed hangs where the wanted box goes");
                    _logged = true;
                    Function.Call(Hash.THEFEED_SET_BACKGROUND_COLOR_FOR_NEXT_POST, TopSignal + (int)Math.Round((WantedBox.Top + PostEdge) * ToMovie));
                    _sentAt = now;
                }
                // under the wanted box while it's up: the menu's line, from the top of the safe zone
                if (WantedBox.Up)
                {
                    float safeTop = (1f - Function.Call<float>(Hash.GET_SAFE_ZONE_SIZE)) * Canvas.H;
                    float line = (WantedBox.Bottom + Gap + PostEdge - safeTop) / Canvas.H;
                    Function.Call(Hash.THEFEED_SET_SCRIPTED_MENU_HEIGHT, line);
                    _under = true;
                }
                else if (_under)
                {
                    Function.Call(Hash.THEFEED_SET_SCRIPTED_MENU_HEIGHT, 0f);
                    _under = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Feed", "where the wanted box goes: " + ex.Message);
            }
        }
    }
}
