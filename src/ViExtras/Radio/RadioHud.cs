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
    /// client.lua tells the page ('state': list, index, muted, open; 'flash';
    /// the song playing) and animates the way style.css transitions do, on real
    /// time.
    ///
    /// The look is the single-player one (tools/render/radio.py, SP_STYLE),
    /// measured off the user's reference shots: logo-filled tiles, all at full
    /// opacity and size, the highlighted one a little larger with a thin light
    /// outline; the line and the text in a light grey, without a shadow; under
    /// the line the station, the song and the artist.
    ///
    /// Layout (CSS px at 1080p, which are canvas units here; --s is 1):
    ///   #radio      top 32, centred; #carousel 5 x spacing 118 = 590 wide, 103 tall
    ///   .tile       103 square, 118 apart, centred on the track; the highlighted one
    ///               on the screen's centre line, scale 1.03, a 2.5px outline round it.
    ///               Two slots out the tiles are whole; they fade out on the way to three.
    ///   #icon-left  right edge 37px left of the carousel, centred on it, opacity .9
    ///   #icon-right left edge 37px right of the carousel, 24 tall, opacity .9
    ///   #info       from y 135: line 189 x 4.8 at +20, #station (25.3px) at +34.7,
    ///               #title (the song, 20.6px) at +70.6, #artist (20.6px) at +97.2;
    ///               642.6 wide (line x 3.4), longer text ends in an ellipsis
    /// Timings: track and tile slide --slide 110ms, track/tile transform
    /// cubic-bezier(0.22, 0.61, 0.36, 1), the outline `ease`; #radio opacity
    /// 160ms ease; mute indicator colour 140ms ease; a flash stays up 1400ms.
    /// </summary>
    internal sealed class RadioHud
    {
        internal sealed class Item
        {
            public string Name, Label, Genre;   // game name, Config.Stations label / genre
            public string Key, GenreKey;        // art keys from stations.txt, or null
            public bool Off;
            public string Custom;               // your own music's station: its folder ("" the loose tracks), else null

            internal Art.Entry Tile, NameArt, GenreArt;   // rendered art; null: the label / genre set in TextKit's Anton
            internal bool Resolved;
        }

        private sealed class TileAnim
        {
            public bool Active;
            public float FromScale, FromMix;
            public long Start = long.MinValue;
        }

        // style.css at Config.Hud's values, with the single-player look's sizes
        private const float Top = 32f, Icon = 103f, Spacing = 118f, CarouselW = Spacing * 5f, SideGap = 37f;
        private const float InfoTop = Top + Icon, InfoW = LineW * 3.4f;
        private const float LineY = 20f, LineW = 189f, LineH = 4.8f;
        private const float StationY = 34.7f, TitleY = 70.6f, ArtistY = 97.2f;
        private const float StationPx = 25.3f, TitlePx = 20.6f, ArtistPx = 20.6f;
        private const float LineHeight = 1.05f, LetterSpacing = 0.01f;
        private const float ActiveScale = 1.03f, OutlineW = 2.5f, SideOpacity = 0.9f;
        private const int FadeMs = 160, FlashMs = 1400, IndicatorMs = 140;
        private static readonly Color Ink = Color.FromArgb(255, 192, 194, 200);         // the reference's text, line and outline
        private static readonly Color MuteColor = Color.FromArgb(255, 255, 255, 255);   // muteColor 255,255,255
        private static readonly Color MutedColor = Color.FromArgb(255, 255, 59, 48);    // mutedColor 255,59,48
        private static readonly Color Dimmed = Color.FromArgb(255, 120, 120, 120);      // grayscale(1) brightness(.62) of Ink

        private readonly int _slideMs;
        private readonly bool _muteDim, _sideIcons, _genreLine;

        private List<Item> _list = new List<Item>();
        private readonly List<TileAnim> _tiles = new List<TileAnim>();
        private int _index;
        private bool _muted, _isOpen, _visible;
        private long _flashUntil = -1;
        private string _title, _artist;   // upper case, as #title / #artist set them

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
                    _tiles.Add(new TileAnim { Active = active, FromScale = ScaleOf(active), FromMix = active ? 1f : 0f });
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
                Value(t, now, out float sc, out float mix);
                t.FromScale = sc; t.FromMix = mix;
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

        /// <summary>
        /// The song on the highlighted station (#title / #artist), or nulls when
        /// none is known: then the genre stands in on the artist line.
        /// </summary>
        public void NowPlaying(string title, string artist)
        {
            _title = string.IsNullOrEmpty(title) ? null : title.ToUpperInvariant();
            _artist = _title == null || string.IsNullOrEmpty(artist) ? null : artist.ToUpperInvariant();
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

        private static float ScaleOf(bool active) => active ? ActiveScale : 1f;

        private static float Progress(long now, long start, int ms) =>
            start == long.MinValue ? 1f : Num.Clamp01((now - start) / (float)ms);

        private static float Slide(float p) => Num.Bezier(0.22f, 0.61f, 0.36f, 1f, p);

        private float Disp(long now) => Num.Lerp(_dispFrom, _dispTo, Slide(Progress(now, _dispStart, _slideMs)));

        private float Opacity(long now) => Num.Lerp(_opFrom, _opTo, Num.Ease(Progress(now, _opStart, FadeMs)));

        private float Red(long now) => Num.Lerp(_redFrom, _redTo, Num.Ease(Progress(now, _redStart, IndicatorMs)));

        private void Value(TileAnim t, long now, out float scale, out float mix)
        {
            float p = Progress(now, t.Start, _slideMs);
            mix = Num.Lerp(t.FromMix, t.Active ? 1f : 0f, Num.Ease(p));
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
            Color ink = dim ? Dimmed : Ink;
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
                if (far >= 3f) continue;
                float a = (far <= 2f ? 1f : 3f - far) * alpha;
                Item item = Resolve(_list[i]);
                float center = cx + rel * Spacing;
                Value(_tiles[i], now, out float scale, out float mix);
                Art.Entry tile = Pic(item.Tile, dim);
                if (tile != null) DrawScaled(tile, center, mid, scale, a);
                else TextTile(item.Label, center, mid, Icon * scale, a, dim);
                if (mix > 0.001f) Outline(center, mid, Icon * scale, Canvas.Alpha(ink, a * mix));
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

            // #info: the line, then the station, the song and the artist. The
            // pictures are white; drawn in the ink colour they come out grey.
            Color text = Canvas.Alpha(ink, alpha);
            Canvas.Rect(cx - LineW * 0.5f, InfoTop + LineY, LineW, LineH, text);
            Item station = _index >= 0 && _index < _list.Count ? Resolve(_list[_index]) : null;
            if (station == null) return;
            if (station.NameArt != null) Centred(station.NameArt, cx, InfoTop + StationY, text);
            else Line(station.Label, StationPx, cx, InfoTop + StationY, text);
            if (station.Off) return;   // a highlighted RADIO OFF tile blanks the rest
            if (_title != null)
            {
                Line(_title, TitlePx, cx, InfoTop + TitleY, text);
                Line(_artist, ArtistPx, cx, InfoTop + ArtistY, text);
            }
            else if (_genreLine)
            {
                if (station.GenreArt != null) Centred(station.GenreArt, cx, InfoTop + ArtistY, text);
                else Line(station.Genre, ArtistPx, cx, InfoTop + ArtistY, text);
            }
        }

        // One #station / #title / #artist line in TextKit's Anton (drawn from a
        // texture dictionary, so on Enhanced too), centred, cut with an ellipsis at
        // #info's width. top is the CSS line box's (line-height 1.05); TextKit
        // takes the top of a line-height 1 box, centred in it.
        private static void Line(string s, float px, float cx, float top, Color color)
        {
            if (string.IsNullOrEmpty(s)) return;
            TextKit.Draw(s.ToUpperInvariant(), TextFace.Radio, px, cx, top + (LineHeight - 1f) * px * 0.5f, color,
                TextAlign.Center, false, LetterSpacing, InfoW);
        }

        private static Art.Entry Pic(Art.Entry e, bool dim) => dim ? RadioImages.Muted(e) : e;

        private static void Centred(Art.Entry e, float cx, float top, Color color)
        {
            if (e != null) Art.Draw(e, cx - e.BoxW * 0.5f, top, color);
        }

        // Scaled about the tile's centre, as transform: scale() does.
        private static void DrawScaled(Art.Entry e, float centerX, float centerY, float scale, float alpha)
        {
            if (alpha <= 0.001f) return;
            Art.Draw(e, centerX - e.BoxW * scale * 0.5f, centerY - e.BoxH * scale * 0.5f, Canvas.Alpha(Color.White, alpha), scale);
        }

        // A tile with the station's name on it, for one with no logo (your own
        // music's, or one vi_radio's table lacks), as app.js's fallbackNode sets
        // it: the box colour, Anton 15px upper case, line-height 1.1,
        // letter-spacing .02em, centred, 8% padding either side, words wrapped.
        // Drawn live (TextKit's Anton), so on Enhanced too.
        private static void TextTile(string label, float cx, float cy, float size, float alpha, bool dim)
        {
            if (alpha <= 0.001f) return;
            Canvas.Rect(cx - size * 0.5f, cy - size * 0.5f, size, size, Color.FromArgb((int)(170f * alpha), 0, 0, 0));
            if (string.IsNullOrEmpty(label)) return;
            float k = size / Icon, px = 15f * k, width = size * 0.84f, lh = px * 1.1f;
            var lines = new List<string>();
            string line = "";
            foreach (string word in label.ToUpperInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string attempt = line.Length == 0 ? word : line + " " + word;
                if (line.Length == 0 || TextKit.Measure(attempt, TextFace.Radio, px, 0.02f) <= width) line = attempt;
                else { lines.Add(line); line = word; }
            }
            if (line.Length > 0) lines.Add(line);
            if (lines.Count > 4) lines.RemoveRange(4, lines.Count - 4);
            Color c = Color.FromArgb((int)(255f * alpha), dim ? Color.FromArgb(158, 158, 158) : Color.White);
            float top = cy - lines.Count * lh * 0.5f;
            for (int i = 0; i < lines.Count; i++)
                TextKit.Draw(lines[i], TextFace.Radio, px, cx, top + i * lh + (lh - px) * 0.5f, c, TextAlign.Center, false, 0.02f, width);
        }

        // The highlighted tile's outline, just outside it.
        private static void Outline(float centerX, float centerY, float size, Color color)
        {
            float x = centerX - size * 0.5f, y = centerY - size * 0.5f, t = OutlineW;
            Canvas.Rect(x - t, y - t, size + t * 2f, t, color);
            Canvas.Rect(x - t, y + size, size + t * 2f, t, color);
            Canvas.Rect(x - t, y, t, size, color);
            Canvas.Rect(x + size, y, t, size, color);
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
                it.NameArt = Art.Get("radio_name_" + it.Key);
            }
            // no logo: Draw sets the label on a tile (TextTile), as app.js's fallbackNode
            // no rendered name or genre: Draw sets the label in TextKit's Anton
            if (!string.IsNullOrEmpty(it.Genre) && it.GenreKey != null) it.GenreArt = Art.Get("radio_genre_" + it.GenreKey);
            return it;
        }
    }
}
