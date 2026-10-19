// The radio wheel's HUD: vi_radio's html/ (index.html, style.css, app.js),
// drawn with the pictures tools/render/radio.py renders from that page.
// Credits and licence: see Radio.cs.
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ViExtras
{
    /// <summary>
    /// What vi_radio's page shows, and how it moves. It is told the same things
    /// client.lua tells the page ('state': list, index, muted, open; 'flash')
    /// and animates the way style.css transitions do, on real time.
    ///
    /// Layout, at vi_radio's Config.Hud values (CSS px at 1080p, which are
    /// canvas units here; --s is 1):
    ///   #radio      top 32, centred; #carousel 5 x spacing 112 = 560 wide, 103 tall
    ///   .tile       103 square, 112 apart, centred on the track; the highlighted one
    ///               on the screen's centre line. Inactive: opacity .55, scale(.9).
    ///   #viewport   mask: transparent at each edge, solid from 18% in (100.8px)
    ///   #icon-left  right edge 37px left of the carousel, centred on it, opacity .9
    ///   #icon-right left edge 37px right of the carousel, 24 tall, opacity .9
    ///   #info       from y 135: line 189 x 6.5 at +24, #station at +34,
    ///               #title at +68 (never used here), #artist at +95
    /// Timings: track and tile slide --slide 110ms, track/tile transform
    /// cubic-bezier(0.22, 0.61, 0.36, 1), tile colours and opacity `ease`;
    /// #radio opacity 160ms ease; mute indicator colour 140ms ease; a flash
    /// stays up 1400ms.
    /// </summary>
    internal sealed class RadioHud
    {
        internal sealed class Item
        {
            public string Name, Label, Genre;   // game name, Config.Stations label / genre
            public string Key, GenreKey;        // art keys from stations.txt, or null
            public bool Off;

            internal Art.Entry Tile, TileOn, EdgeL, EdgeR, NameArt, GenreArt;
            internal bool Resolved;
        }

        private sealed class TileAnim
        {
            public bool Active;
            public float FromOp, FromScale, FromMix;
            public long Start = long.MinValue;
        }

        // style.css at Config.Hud's values
        private const float Top = 32f, Icon = 103f, Spacing = 112f, CarouselW = Spacing * 5f, SideGap = 37f;
        private const float InfoTop = Top + Icon;
        private const float LineY = 24f, LineW = 189f, LineH = 6.5f, StationY = 34f, ArtistY = 95f;
        private const float StationPx = 94f * 0.38f, ArtistPx = 94f * 0.30f;   // fontPx x stationScale / artistScale
        private const float InactiveOpacity = 0.55f, InactiveScale = 0.9f, SideOpacity = 0.9f;
        private const int FadeMs = 160, FlashMs = 1400, IndicatorMs = 140;
        private static readonly Color MuteColor = Color.FromArgb(255, 255, 255, 255);   // muteColor 255,255,255
        private static readonly Color MutedColor = Color.FromArgb(255, 255, 59, 48);    // mutedColor 255,59,48
        private static readonly Color Dimmed = Color.FromArgb(255, 158, 158, 158);      // brightness(.62) of white

        private readonly int _slideMs;
        private readonly bool _muteDim, _sideIcons, _genreLine;

        private List<Item> _list = new List<Item>();
        private readonly List<TileAnim> _tiles = new List<TileAnim>();
        private int _index;
        private bool _muted, _isOpen, _visible;
        private long _flashUntil = -1;

        private float _opFrom, _opTo;
        private long _opStart = long.MinValue;
        private float _dispFrom, _dispTo;
        private long _dispStart = long.MinValue;
        private bool _placed;
        private float _redFrom, _redTo;
        private long _redStart = long.MinValue;

        public RadioHud(int slideMs, bool muteDim, bool sideIcons, bool genreLine)
        {
            _slideMs = Math.Max(1, slideMs);
            _muteDim = muteDim;
            _sideIcons = sideIcons;
            _genreLine = genreLine;
        }

        // ---- messages (app.js 'state' and 'flash') ------------------------------

        /// <summary>action 'state'. `list` is null when client.lua sends no list.</summary>
        public void State(List<Item> list, int index, bool muted, bool open)
        {
            long now = RealTime.Ms;
            if (list != null)
            {
                // buildTrack: new tiles, so they start in their end state.
                _list = list;
                _tiles.Clear();
                for (int i = 0; i < list.Count; i++)
                {
                    bool active = i == index;
                    _tiles.Add(new TileAnim { Active = active, FromOp = OpOf(active), FromScale = ScaleOf(active), FromMix = active ? 1f : 0f });
                }
            }

            // render(): the track keeps its element, so its transform slides.
            if (!_placed)
            {
                _dispFrom = _dispTo = index;
                _placed = true;
            }
            else if (Math.Abs(_dispTo - index) > 0.0001f)
            {
                _dispFrom = Disp(now);
                _dispTo = index;
                _dispStart = now;
            }
            _index = index;

            for (int i = 0; i < _tiles.Count; i++)
            {
                bool active = i == index;
                TileAnim t = _tiles[i];
                if (t.Active == active) continue;
                Value(t, now, out float op, out float sc, out float mix);
                t.FromOp = op; t.FromScale = sc; t.FromMix = mix;
                t.Active = active;
                t.Start = now;
            }

            SetMuted(muted, now);
            _isOpen = open;
            _flashUntil = -1;
            SetVisible(open, now);
        }

        /// <summary>action 'flash': the HUD up for 1400ms to show the mute state.</summary>
        public void Flash(bool muted)
        {
            long now = RealTime.Ms;
            SetMuted(muted, now);
            SetVisible(true, now);
            _flashUntil = now + FlashMs;
        }

        private void SetMuted(bool muted, long now)
        {
            if (muted == _muted) return;
            _redFrom = Red(now);
            _redTo = muted ? 1f : 0f;
            _redStart = now;
            _muted = muted;
        }

        private void SetVisible(bool visible, long now)
        {
            if (visible == _visible) return;
            _opFrom = Opacity(now);
            _opTo = visible ? 1f : 0f;
            _opStart = now;
            _visible = visible;
        }

        // ---- the transitions -------------------------------------------------------

        private static float OpOf(bool active) => active ? 1f : InactiveOpacity;
        private static float ScaleOf(bool active) => active ? 1f : InactiveScale;

        private static float Progress(long now, long start, int ms) =>
            start == long.MinValue ? 1f : Num.Clamp01((now - start) / (float)ms);

        private static float Slide(float p) => Num.Bezier(0.22f, 0.61f, 0.36f, 1f, p);

        private float Disp(long now) => Num.Lerp(_dispFrom, _dispTo, Slide(Progress(now, _dispStart, _slideMs)));

        private float Opacity(long now) => Num.Lerp(_opFrom, _opTo, Num.Ease(Progress(now, _opStart, FadeMs)));

        private float Red(long now) => Num.Lerp(_redFrom, _redTo, Num.Ease(Progress(now, _redStart, IndicatorMs)));

        private void Value(TileAnim t, long now, out float op, out float scale, out float mix)
        {
            float p = Progress(now, t.Start, _slideMs);
            float e = Num.Ease(p);
            op = Num.Lerp(t.FromOp, OpOf(t.Active), e);
            mix = Num.Lerp(t.FromMix, t.Active ? 1f : 0f, e);
            scale = Num.Lerp(t.FromScale, ScaleOf(t.Active), Slide(p));
        }

        // ---- drawing -----------------------------------------------------------------

        public void Draw()
        {
            long now = RealTime.Ms;
            if (_flashUntil > 0 && now >= _flashUntil)
            {
                _flashUntil = -1;
                if (!_isOpen) SetVisible(false, now);
            }
            float alpha = Opacity(now);
            if (alpha <= 0.001f) return;

            bool dim = _muted && _muteDim;
            float cx = Canvas.W * 0.5f;
            float left = cx - CarouselW * 0.5f, right = cx + CarouselW * 0.5f;
            float mid = Top + Icon * 0.5f;

            // #icon-left: the static artwork, as the page shows it with On Demand off.
            if (_sideIcons)
            {
                Art.Entry icon = Art.Get(dim ? "radio_icon_left_muted" : "radio_icon_left");
                if (icon != null) Art.Draw(icon, left - SideGap - icon.BoxW, mid - icon.BoxH * 0.5f, Canvas.Alpha(Color.White, SideOpacity * alpha));
            }

            // the carousel
            float disp = Disp(now);
            for (int i = 0; i < _list.Count && i < _tiles.Count; i++)
            {
                float rel = i - disp;
                float far = Math.Abs(rel);
                if (far >= 3f) continue;   // beyond #viewport's overflow
                Item item = Resolve(_list[i]);
                float center = cx + rel * Spacing;

                // Up to one slot out the tile is clear of the edge mask; at two it
                // is the rendered edge picture; in between, the two blended.
                float clear = far <= 1f ? 1f : far < 2f ? 2f - far : 0f;
                float edge = far <= 1f ? 0f : far <= 2f ? far - 1f : 3f - far;

                if (clear > 0f)
                {
                    Value(_tiles[i], now, out float op, out float scale, out float mix);
                    float a = op * clear * alpha;
                    Art.Entry off = Pic(item.Tile, dim), on = Pic(item.TileOn, dim);
                    if (off != null && mix < 1f) DrawScaled(off, center, mid, scale, a * (1f - mix));
                    if (on != null && mix > 0f) DrawScaled(on, center, mid, scale, a * mix);
                }
                if (edge > 0f)
                {
                    Art.Entry e = Pic(rel < 0f ? item.EdgeL : item.EdgeR, dim);
                    if (e != null) Art.Draw(e, center - Icon * 0.5f, Top, Canvas.Alpha(Color.White, edge * alpha));
                }
            }

            // #icon-right: the mute indicator, a white glyph used as a mask and filled.
            if (_sideIcons)
            {
                Art.Entry glyph = Art.Get(_muted ? "radio_icon_unmute" : "radio_icon_mute");
                if (glyph != null)
                {
                    float red = Red(now);
                    Color c = Color.FromArgb(255,
                        (int)Math.Round(Num.Lerp(MuteColor.R, MutedColor.R, red)),
                        (int)Math.Round(Num.Lerp(MuteColor.G, MutedColor.G, red)),
                        (int)Math.Round(Num.Lerp(MuteColor.B, MutedColor.B, red)));
                    Art.Draw(glyph, right + SideGap, mid - glyph.BoxH * 0.5f, Canvas.Alpha(c, (_muted ? 1f : SideOpacity) * alpha));
                }
            }

            // #info
            Color text = Canvas.Alpha(dim ? Dimmed : Color.White, alpha);
            Canvas.Rect(cx - LineW * 0.5f, InfoTop + LineY, LineW, LineH, text);
            Item station = _index >= 0 && _index < _list.Count ? Resolve(_list[_index]) : null;
            if (station != null)
            {
                if (station.NameArt != null) Art.Draw(station.NameArt, cx - station.NameArt.BoxW * 0.5f, InfoTop + StationY, text);
                // renderNowPlaying: nothing feeds a title in single player, so the
                // second line is the genre; a highlighted RADIO OFF tile blanks both.
                if (!station.Off && _genreLine && station.GenreArt != null)
                    Art.Draw(station.GenreArt, cx - station.GenreArt.BoxW * 0.5f, InfoTop + ArtistY, text);
            }
        }

        private static Art.Entry Pic(Art.Entry e, bool dim) => dim ? RadioImages.Muted(e) : e;

        // Scaled about the tile's centre, as transform: scale() does.
        private static void DrawScaled(Art.Entry e, float centerX, float centerY, float scale, float alpha)
        {
            if (alpha <= 0.001f) return;
            Art.Draw(e, centerX - e.BoxW * scale * 0.5f, centerY - e.BoxH * scale * 0.5f, Canvas.Alpha(Color.White, alpha), scale);
        }

        // The pictures for an entry: the rendered ones, or made here for a
        // station vi_radio has no art for.
        private static Item Resolve(Item it)
        {
            if (it.Resolved) return it;
            it.Resolved = true;
            if (it.Key != null)
            {
                it.Tile = Art.Get("radio_tile_" + it.Key);
                it.TileOn = Art.Get("radio_tile_" + it.Key + "_on");
                it.EdgeL = Art.Get("radio_tile_" + it.Key + "_l");
                it.EdgeR = Art.Get("radio_tile_" + it.Key + "_r");
                it.NameArt = Art.Get("radio_name_" + it.Key);
            }
            if (it.Tile == null || it.TileOn == null)
            {
                // app.js fallbackNode: the label as text on the tile.
                it.Tile = RadioImages.FallbackTile(it.Label ?? "", false);
                it.TileOn = RadioImages.FallbackTile(it.Label ?? "", true);
                it.EdgeL = it.EdgeR = null;
            }
            if (it.EdgeL == null) it.EdgeL = RadioImages.EdgeTile(it.Tile, true);
            if (it.EdgeR == null) it.EdgeR = RadioImages.EdgeTile(it.Tile, false);
            if (it.NameArt == null) it.NameArt = RadioImages.TextLine(it.Label, StationPx, 1.05f, 0.01f, 1f);
            if (!string.IsNullOrEmpty(it.Genre))
            {
                if (it.GenreKey != null) it.GenreArt = Art.Get("radio_genre_" + it.GenreKey);
                if (it.GenreArt == null) it.GenreArt = RadioImages.TextLine(it.Genre, ArtistPx, 1.05f, 0.01f, 0.62f);   // #artist opacity .62
            }
            return it;
        }
    }
}
