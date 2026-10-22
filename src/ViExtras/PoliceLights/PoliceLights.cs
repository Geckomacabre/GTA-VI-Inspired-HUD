using System;
using System.Drawing;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// vice_hud's directional police lights (client.lua "Directional police
    /// lights", style.css #police), for single player.
    ///
    /// An emergency vehicle with its siren on within MaxDistance lights the
    /// screen edges facing it. The glow is spread over the two edges nearest the
    /// siren by the cosine of the angle to each, so a car circling you sweeps
    /// round the screen, and it is full strength inside FullDistance, fading out
    /// to MaxDistance.
    ///
    /// Shape = Round (the default) is the GTA VI trailer's police chase, frame
    /// by frame and looped: a row of blue and red lamps glowing up from the
    /// edge, one to four lit at a time, each flashing on its own beat, their
    /// cores burning cyan-white and pink-white and their light adding up to
    /// violet and white where they meet. Each frame is one picture of the whole
    /// row (police_row_*, drawn by tools/render/police.py from the lamps fitted
    /// to the trailer's frames; PoliceTrailer says which frame shows which).
    /// They grow as the nearest siren closes in: the trailer's own size
    /// (NearSize 19) inside TallDistance, down to FarSize from ShortDistance out.
    /// With Arrow on (the default; GTA6Hud's own police arrows are off in
    /// GTA6Hud.ini, [PoliceArrow] Enabled = False), the second clip's direction
    /// arrow sits on a ring round the screen's middle at the siren's bearing:
    /// white while the car is far, filling blue from its point as it closes in
    /// (all blue by ArrowFull metres), and gone once it is beside you (inside
    /// ArrowGone).
    /// Shape = Wide keeps vice_hud's lamps: LampWidth x LampHeight ellipses, red
    /// and blue with the white takedown between them, firing in short hard
    /// bursts the way a real lightbar does (a red double tap, a white pop, a blue
    /// double tap, a white pop, every FlashMs) and drifting a little.
    ///
    /// Two of vice_hud's modes ([PoliceLights] Mode): Radius, the default here,
    /// puts the lights on the screen's edge at the bearing of the strongest siren
    /// alone (a car behind and to the left lights the bottom left corner), so the
    /// light only ever comes from where the police are; Edges lights each of the
    /// four edges for every siren facing it.
    /// It replaces GTA6Hud's police arrows (GTA6Hud.ini [PoliceArrow] Enabled =
    /// False). vice_hud's in-game tuning panel (/hudpolice edit) is not ported;
    /// the same numbers are in [PoliceLights] in ViExtras.ini.
    /// </summary>
    public sealed class PoliceLights : Script
    {
        private const float W1 = 19.2f;            // --w at 1080p
        private const float Vh = 10.8f;            // 1vh at 1080p
        private const int FadeMs = 420;            // .pol transition: opacity 420ms cubic-bezier(0.4, 0, 0.2, 1)

        // .pol-r / .pol-b / .pol-w --polc (Wide)
        private static readonly Color Red = Color.FromArgb(255, 255, 42, 58);
        private static readonly Color Blue = Color.FromArgb(255, 64, 118, 255);
        private static readonly Color White = Color.FromArgb(255, 255, 250, 240);
        private const float LampAlpha = 0.9203f;

        // Round: the row pictures are the trailer's size at this NearSize (its police car right behind)
        private const float TrailerSize = 19f;
        // how strong each frame of the loop is drawn, a hair either side of the trailer's (under half a percent)
        private const uint Grain = 0x6842F317;
        private static readonly string[] RowBottom = RowNames("police_row_b_"), RowLeft = RowNames("police_row_l_");

        // The direction arrow, in canvas pixels: on a ring 0.396 of the height round the middle, a sliver curved round a
        // point ArrowBend further in, ArrowHalf of its length each way, ArrowThick at its thickest and tapered over the
        // last eighth at each end, with the point (police_tip_*, 15 x 11) on its outer side. The game turns a picture
        // in its 0..1 screen units, so a turned one is sheared: the sliver is a row of police_dot, which look the same
        // at any angle, and the point is one of 64 pictures already turned.
        private const float ArrowRing = 0.396f * 1080f, ArrowBend = 190f, ArrowHalf = 52f, ArrowThick = 3.2f, ArrowStep = 1.2f;
        private const int Tips = 64;
        private static readonly Color ArrowWhite = Color.FromArgb(230, 240, 240, 240);
        private static readonly Color ArrowBlue = Color.FromArgb(242, 62, 98, 222);

        // client.lua EDGE_ANGLE: 0 is straight ahead, positive is to the left.
        private enum Edge { Top, Right, Bottom, Left }
        private static readonly float[] EdgeAngle = { 0f, -90f, 180f, 90f };

        private readonly bool _enabled, _white, _requireSiren;
        private readonly int _scanMs, _flashMs, _sweepMs;
        private readonly float _maxDistance, _fullDistance, _maxOpacity, _focus, _whiteA;
        private readonly float _lampA, _lampB, _whiteLampA, _whiteLampB, _spread, _softness, _sweepAmt;
        private readonly bool _radius, _round;
        private readonly float _radiusSpread, _nearSize, _farSize, _tallDistance, _shortDistance, _roundOpacity;
        // [PoliceLights] Size, the Round lights' size against NearSize / FarSize (1 = as set); HUD OPTIONS' POLICE
        // LIGHTS row steps it (HudMenu), and it takes at once
        public static float SizeScale = 1f;
        private float _rel, _relTarget;
        private bool _relSet;
        // how close the nearest siren is: 1 inside TallDistance, 0 from ShortDistance out (the round lights' size)
        private float _near, _nearTarget;
        private bool _nearSet;
        // the arrow: on, the siren it points at (bearing, how full), and whether that car is beside you
        private readonly bool _arrow;
        private readonly float _arrowFull, _arrowGone;
        private float _arrowRel, _arrowRelTarget, _arrowFill, _arrowFillTarget, _arrowA;
        private bool _arrowSeen, _arrowBeside;

        private long _nextScan, _blockedSince = -1;
        private readonly float[] _from = new float[4], _to = new float[4];
        private readonly long[] _since = new long[4];
        private readonly long _clock0 = RealTime.Ms;

        public PoliceLights()
        {
            Paths.Init(BaseDirectory);
            const string S = "PoliceLights";
            _enabled = Config.Get(S, "Enabled", true);
            _scanMs = Config.Get(S, "ScanMs", 250);
            _maxDistance = Config.Get(S, "MaxDistance", 140f);
            _fullDistance = Config.Get(S, "FullDistance", 25f);
            _maxOpacity = Config.Get(S, "MaxOpacity", 0.55f);
            _flashMs = Math.Max(100, Config.Get(S, "FlashMs", 900));
            _white = Config.Get(S, "White", true);
            _whiteA = Config.Get(S, "WhiteAlpha", 0.70f);
            _focus = Config.Get(S, "Focus", 1.6f);
            _requireSiren = Config.Get(S, "RequireSiren", true);
            _lampA = Config.Get(S, "LampWidth", 16f);
            _lampB = Config.Get(S, "LampHeight", 24f);
            _round = !string.Equals(Config.Get(S, "Shape", "Round"), "Wide", StringComparison.OrdinalIgnoreCase);
            _nearSize = Math.Max(1f, Config.Get(S, "NearSize", 19f));
            _farSize = Math.Max(1f, Config.Get(S, "FarSize", 8f));
            _roundOpacity = Num.Clamp01(Config.Get(S, "RoundOpacity", 1f));
            SizeScale = Num.Clamp(Config.Get(S, "Size", 100f), 40f, 200f) / 100f;
            _tallDistance = Math.Max(0f, Config.Get(S, "TallDistance", 5f));
            _shortDistance = Math.Max(_tallDistance + 1f, Config.Get(S, "ShortDistance", 60f));
            _whiteLampA = Config.Get(S, "WhiteLampWidth", 19f);
            _whiteLampB = Config.Get(S, "WhiteLampHeight", 30f);
            _spread = Config.Get(S, "Spread", 10f);   // vice_hud has 24; closer together here
            _radius = !string.Equals(Config.Get(S, "Mode", "Radius"), "Edges", StringComparison.OrdinalIgnoreCase);
            _radiusSpread = Config.Get(S, "RadiusSpread", 9f);   // degrees round the edge (vice_hud 24)
            _softness = Config.Get(S, "Softness", 68f);
            _sweepAmt = Config.Get(S, "Sweep", 1.6f);
            _sweepMs = Math.Max(100, Config.Get(S, "SweepMs", 2600));
            _arrow = Config.Get(S, "Arrow", true);
            _arrowGone = Math.Max(0f, Config.Get(S, "ArrowGone", 8f));
            _arrowFull = Math.Max(_arrowGone, Config.Get(S, "ArrowFull", 15f));
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            if (!_enabled) return;
            try
            {
                long now = RealTime.Ms;
                // Nothing is drawn meanwhile, so nothing is scanned either: the first scan
                // after fades as if it had been made when the block began (Scan), so what
                // changed meanwhile shows as it would have, not fading in late.
                if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen || CrashCam.IsPlaying || Binoculars.IsActive)
                {
                    if (_blockedSince < 0) _blockedSince = now;
                    return;
                }
                if (now >= _nextScan)
                {
                    _nextScan = now + Math.Max(50, _scanMs);
                    Scan(now);
                }
                _blockedSince = -1;
                // Under the HUD's panels, as vice_hud's #police (z-index 1) is: GTA6Hud
                // draws at the default order (4, after the game's HUD), these at 3.
                Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 3);
                try { Draw(now); }
                finally { Function.Call(Hash.SET_SCRIPT_GFX_DRAW_ORDER, 4); }
            }
            catch (Exception ex) { Log.Write("PoliceLights", ex.Message); }
        }

        // ---- the scan (forEachSiren / scanPoliceLightsEdges) -------------------------------

        private static readonly int Ambulance = Game.GenerateHash("ambulance"), FireTruck = Game.GenerateHash("firetruk"),
            Lifeguard = Game.GenerateHash("lguard");

        private void Scan(long now)
        {
            var edges = new float[4];
            float best = 0f, bestRel = 0f, bestDist = 0f, nearest = float.MaxValue;
            Ped ped = Game.Player.Character;
            Vector3 p = ped.Position;
            float camHeading = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 2).Z;
            float maxDist = _maxDistance;
            float fullDist = Math.Min(_fullDistance, maxDist - 1f);

            foreach (Vehicle v in World.GetNearbyVehicles(p, maxDist))
            {
                if (v == null || v.ClassType != VehicleClass.Emergency) continue;   // (it exists: the pool handed it back)
                // Config.PoliceLights.ignoreModels: an ambulance or fire truck on a call isn't the police
                int model = v.Model.Hash;
                if (model == Ambulance || model == FireTruck || model == Lifeguard) continue;
                if (_requireSiren && !Function.Call<bool>(Hash.IS_VEHICLE_SIREN_ON, v)) continue;
                Vector3 q = v.Position;
                float dx = q.X - p.X, dy = q.Y - p.Y;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                if (dist > maxDist) continue;
                // GTA headings: degrees counter-clockwise from north, atan2(-dx, dy).
                float rel = Normalise((float)(Math.Atan2(-dx, dy) * 180.0 / Math.PI) - camHeading);
                float t = dist > fullDist ? 1f - (dist - fullDist) / (maxDist - fullDist) : 1f;
                float strength = Num.Clamp01(t) * _maxOpacity;
                if (strength <= 0f) continue;
                // Radius: only the strongest siren places the lamps (scanPoliceLightsRadius); every one inside
                // FullDistance is as strong as the next, so of those the nearest
                if (strength > best || (strength == best && dist < bestDist)) { best = strength; bestRel = rel; bestDist = dist; }
                // the nearest one sets how big the round lights are, in both modes
                if (dist < nearest) nearest = dist;
                // Edges: the strongest siren per edge wins.
                for (int i = 0; i < 4; i++)
                {
                    double w = Math.Cos(Normalise(rel - EdgeAngle[i]) * Math.PI / 180.0);
                    float lit = w > 0 ? strength * (float)Math.Pow(w, _focus) : 0f;
                    if (lit > edges[i]) edges[i] = lit;
                }
            }

            if (_radius)
            {
                // one group, faded as a whole (#police-radius opacity), at the bearing
                edges = new float[4];
                edges[0] = best;
                if (best > 0f)
                {
                    _relTarget = bestRel;
                    if (!_relSet || Opacity(0, now) < 0.01f) _rel = bestRel;   // appears where the car is
                    _relSet = true;
                }
            }
            // The arrow follows the siren the lights come from: fuller the closer it is, gone once the police are
            // beside you, the car or a cop on foot (its car can stop a little way off while they walk up), and back
            // 2 m further out, so it doesn't blink at the line.
            _arrowSeen = best > 0f;
            if (_arrowSeen)
            {
                _arrowRelTarget = bestRel;
                _arrowFillTarget = Num.Clamp01((maxDist - bestDist) / Math.Max(1f, maxDist - _arrowFull));
                float beside = bestDist;
                if (_arrow)
                    foreach (Ped c in World.GetNearbyPeds(ped, _arrowGone + 2f))
                        if (c != null && !c.IsDead && IsCop(c)) beside = Math.Min(beside, c.Position.DistanceTo(p));
                if (beside < _arrowGone) _arrowBeside = true;
                else if (beside > _arrowGone + 2f) _arrowBeside = false;
            }
            // With no siren left the size stays put, so the lights fade out at the size they were.
            if (best > 0f)
            {
                float close = Num.Clamp01((_shortDistance - nearest) / (_shortDistance - _tallDistance));
                bool dark = true;
                for (int i = 0; i < 4; i++) if (Opacity(i, now) >= 0.01f) dark = false;
                if (!_nearSet || dark) _near = close;   // appears at the car's distance
                _nearTarget = close;
                _nearSet = true;
            }
            long at = _blockedSince >= 0 ? _blockedSince : now;   // see OnTick
            for (int i = 0; i < 4; i++)
            {
                // Rounded as client.lua's change key is, so distance jitter does not restart the fade.
                float target = (float)Math.Round(edges[i], 2);
                if (Math.Abs(target - _to[i]) < 0.005f) continue;
                _from[i] = Opacity(i, at);
                _to[i] = target;
                _since[i] = at;
            }
        }

        // the game's own police on foot: cops and SWAT (GET_PED_TYPE 6 and 27)
        private static bool IsCop(Ped c)
        {
            int type = Function.Call<int>(Hash.GET_PED_TYPE, c);
            return type == 6 || type == 27;
        }

        private static float Normalise(float a)
        {
            a = (a + 180f) % 360f;
            if (a < 0f) a += 360f;
            return a - 180f;
        }

        private float Opacity(int i, long now)
        {
            float t = Num.Clamp01((now - _since[i]) / (float)FadeMs);
            return Num.Lerp(_from[i], _to[i], Num.Bezier(0.4f, 0f, 0.2f, 1f, t));
        }

        // ---- drawing ----------------------------------------------------------------------------

        private void Draw(long now)
        {
            // the round lights' size follows the siren's distance smoothly between scans
            _near += (_nearTarget - _near) * Math.Min(1f, Game.LastFrameTime * 4f);
            if (_round) DrawRound(now);
            else DrawWide(now);
            if (_arrow) DrawArrow();
        }

        // ---- the direction arrow (the clip's: driveUPSguy 0:44-0:51) --------------------------------------

        private void DrawArrow()
        {
            float dt = Game.LastFrameTime;
            bool show = _arrowSeen && !_arrowBeside;
            if (show && _arrowA <= 0f)
            {
                // appears where the car is, as full as it is
                _arrowRel = _arrowRelTarget;
                _arrowFill = _arrowFillTarget;
            }
            _arrowA = Num.Clamp01(_arrowA + (show ? dt * 1000f / FadeMs : -dt / 0.3f));
            if (_arrowA <= 0.003f) return;
            _arrowRel += Normalise(_arrowRelTarget - _arrowRel) * Math.Min(1f, dt * 8f);
            _arrowFill += (_arrowFillTarget - _arrowFill) * Math.Min(1f, dt * 4f);

            // with one group of lights, exactly where they are; with Edges, the siren they'd come from
            float rel = _radius && _relSet ? _rel : _arrowRel;
            double rad = rel * Math.PI / 180.0;
            float ux = (float)-Math.Sin(rad), uy = (float)-Math.Cos(rad);
            // the sliver's centre of curve, ArrowBend in from the ring
            float qx = Canvas.W * 0.5f + (ArrowRing - ArrowBend) * ux, qy = Canvas.H * 0.5f + (ArrowRing - ArrowBend) * uy;
            float a = Num.Ease(_arrowA), fill = Num.Clamp01(_arrowFill);
            float half = ArrowHalf / ArrowBend, a0 = (float)Math.Atan2(uy, ux);
            int n = (int)(2f * ArrowHalf / ArrowStep);
            float ds = 1f / n;   // one dot along, as a share of the whole length
            for (int i = 0; i <= n; i++)
            {
                float s = i * ds, off = Math.Abs(2f * s - 1f);
                // tapered over the last eighth at each end
                float th = ArrowThick * (float)Math.Pow(Num.Clamp01((1f - off) / 0.25f), 0.7);
                if (th < 0.6f) continue;
                double ang = a0 + (s - 0.5f) * 2f * half;
                float x = qx + ArrowBend * (float)Math.Cos(ang), y = qy + ArrowBend * (float)Math.Sin(ang);
                // blue out from the middle as far as the fill has got, the dot on the line part way
                float blue = Num.Clamp01((fill - off) / (2f * ds) + 0.5f);
                Sprite("police_dot", x, y, th, th, Lerp(ArrowWhite, ArrowBlue, blue), a);
            }
            // the point: its base's middle on the sliver's outer edge
            int k = ((int)Math.Round(Math.Atan2(ux, -uy) / (2.0 * Math.PI) * Tips) % Tips + Tips) % Tips;
            float bx = qx + (ArrowBend + ArrowThick * 0.5f - 0.6f) * ux, by = qy + (ArrowBend + ArrowThick * 0.5f - 0.6f) * uy;
            Art.Entry tip = Art.Get("police_tip_" + k);
            if (tip != null) Sprite("police_tip_" + k, bx, by, tip.W, tip.H, Lerp(ArrowWhite, ArrowBlue, Num.Clamp01(fill / 0.05f)), a);
        }

        private static Color Lerp(Color a, Color b, float t) => Color.FromArgb((int)(a.A + (b.A - a.A) * t + 0.5f),
            (int)(a.R + (b.R - a.R) * t + 0.5f), (int)(a.G + (b.G - a.G) * t + 0.5f), (int)(a.B + (b.B - a.B) * t + 0.5f));

        // ---- Round: the GTA VI trailer's lights -------------------------------------------------

        private void DrawRound(long now)
        {
            // the trailer's size close in, smaller further out, and the menu's Size on top
            float scale = Num.Lerp(_farSize, _nearSize, _near) / TrailerSize * SizeScale;
            // the trailer's frame for now, looped: shown as it is, frame for frame, its flashes as sharp as its own
            int n = PoliceTrailer.Picture.Length;
            int f = (int)((now - _clock0) * PoliceTrailer.Fps / 1000L % n);
            if (_radius)
            {
                float o = Opacity(0, now);
                if (o < 0.003f) return;
                // the group turns with the siren rather than jumping a scan at a time
                _rel += Normalise(_relTarget - _rel) * Math.Min(1f, Game.LastFrameTime * 8f);
                Perimeter(_rel, out float px, out float py);
                EdgeFrame(_rel, out float tx, out float ty, out float nx, out float ny);
                Row(px, py, nx, ny, scale, o, f);
                return;
            }
            for (int i = 0; i < 4; i++)
            {
                float o = Opacity(i, now);
                if (o < 0.003f) continue;
                Perimeter(EdgeAngle[i], out float px, out float py);
                EdgeFrame(EdgeAngle[i], out float tx, out float ty, out float nx, out float ny);
                // each edge a quarter of the loop on from the last, so on its own beat
                Row(px, py, nx, ny, scale, o, (f + i * n / 4) % n);
            }
        }

        // Frame f of the trailer's row of lights, its middle at (px, py) on the edge and shining in along n, as strong
        // as the edge is lit (o). The bottom and left edges have their own pictures; the top's and right's are them
        // half turned (the one turn the game draws true).
        private void Row(float px, float py, float nx, float ny, float scale, float o, int f)
        {
            int k = PoliceTrailer.Picture[f];
            if (k < 0) return;
            float alpha = o / Math.Max(0.01f, _maxOpacity) * _roundOpacity * GrainAt(f);
            // the picture reaches In into the screen and Below past the edge, off screen
            float along = PoliceTrailer.Along * scale, deep = (PoliceTrailer.In + PoliceTrailer.Below) * scale;
            float mid = (PoliceTrailer.In - PoliceTrailer.Below) * scale * 0.5f;
            bool side = nx != 0f;
            Art.Entry e = Art.Get(side ? RowLeft[k] : RowBottom[k]);
            if (e == null || alpha <= 0.003f) return;
            float turn = side ? (nx > 0f ? 0f : 180f) : (ny < 0f ? 0f : 180f);
            Canvas.Image(e.Path, px + nx * mid, py + ny * mid, side ? deep : along, side ? along : deep,
                Canvas.Alpha(Color.White, Num.Clamp01(alpha)), true, turn);
        }

        private static float GrainAt(int f) => ((Grain >> (f & 31)) & 1u) != 0u ? 1.004f : 0.996f;

        private static string[] RowNames(string prefix)
        {
            var names = new string[PoliceTrailer.Pictures];
            for (int k = 0; k < names.Length; k++) names[k] = prefix + k.ToString("D3");
            return names;
        }

        // a picture centred on (cx, cy), never turned (see ArrowRing)
        private static void Sprite(string name, float cx, float cy, float w, float h, Color c, float alpha)
        {
            Art.Entry e = Art.Get(name);
            if (e == null || alpha <= 0.003f) return;
            Canvas.Image(e.Path, cx, cy, w, h, Canvas.Alpha(c, Num.Clamp01(alpha)), true);
        }

        // the edge a bearing's perimeter point is on: its direction along the edge (t) and into the screen (n)
        private static void EdgeFrame(float relDeg, out float tx, out float ty, out float nx, out float ny)
        {
            double rad = relDeg * Math.PI / 180.0;
            double dx = -Math.Sin(rad), dy = -Math.Cos(rad);
            if (Math.Abs(dx) >= Math.Abs(dy)) { tx = 0f; ty = 1f; nx = dx < 0 ? 1f : -1f; ny = 0f; }
            else { tx = 1f; ty = 0f; nx = 0f; ny = dy < 0 ? 1f : -1f; }
        }

        // ---- Wide: vice_hud's lamps (style.css .pol-*) -------------------------------------------

        private void DrawWide(long now)
        {
            long t = now - _clock0;
            float flash = (t % _flashMs) / (float)_flashMs;
            // @keyframes polRed / polBlue / polWhite
            bool red = flash < 0.07f || (flash >= 0.14f && flash < 0.21f);
            bool blue = (flash >= 0.5f && flash < 0.57f) || (flash >= 0.64f && flash < 0.71f);
            bool white = _white && ((flash >= 0.23f && flash < 0.26f) || (flash >= 0.73f && flash < 0.76f));
            if (!red && !blue && !white) return;
            if (_radius) { DrawRadius(now, red, blue, white); return; }

            // @keyframes polSweepX / polSweepY, ease-in-out; blue runs in reverse.
            float sweep = (t % _sweepMs) / (float)_sweepMs;
            float driftRed = Drift(sweep), driftBlue = Drift(1f - sweep);

            float lampW = _lampA * W1, lampH = _lampB / 100f * 1080f, stop = _softness / 100f;
            float whiteW = _whiteLampA * W1, whiteH = _whiteLampB / 100f * 1080f;
            float cw = Canvas.W, ch = Canvas.H;
            for (int i = 0; i < 4; i++)
            {
                float o = Opacity(i, now);
                if (o < 0.003f) continue;
                Edge edge = (Edge)i;
                bool side = edge == Edge.Left || edge == Edge.Right;
                float x, y, along;
                switch (edge)
                {
                    case Edge.Left: x = 0f; y = ch * 0.5f; along = _spread / 100f * ch; break;
                    case Edge.Right: x = cw; y = ch * 0.5f; along = _spread / 100f * ch; break;
                    case Edge.Top: x = cw * 0.5f; y = 0f; along = _spread * W1; break;
                    default: x = cw * 0.5f; y = ch; along = _spread * W1; break;
                }
                // red before the midpoint, blue after, each drifting along the edge
                float rx = side ? x : x - along + driftRed * W1, ry = side ? y - along + driftRed * Vh : y;
                float bx = side ? x : x + along + driftBlue * W1, by = side ? y + along + driftBlue * Vh : y;
                if (red) Lamp(rx, ry, lampW * stop, lampH * stop, Red, o * LampAlpha);
                if (blue) Lamp(bx, by, lampW * stop, lampH * stop, Blue, o * LampAlpha);
                if (white) Lamp(x, y, whiteW * 0.58f, whiteH * 0.58f, White, o * _whiteA);
            }
        }

        // #police-radius: red and blue either side of the bearing, the white on it
        private void DrawRadius(long now, bool red, bool blue, bool white)
        {
            float o = Opacity(0, now);
            if (o < 0.003f) return;
            // the group turns with the siren rather than jumping a scan at a time
            _rel += Normalise(_relTarget - _rel) * Math.Min(1f, Game.LastFrameTime * 8f);
            float lampW = _lampA * W1 * _softness / 100f, lampH = _lampB / 100f * 1080f * _softness / 100f;
            float whiteW = _whiteLampA * W1 * 0.58f, whiteH = _whiteLampB / 100f * 1080f * 0.58f;
            if (red) { Perimeter(_rel - _radiusSpread, out float x, out float y); Lamp(x, y, lampW, lampH, Red, o * LampAlpha); }
            if (blue) { Perimeter(_rel + _radiusSpread, out float x, out float y); Lamp(x, y, lampW, lampH, Blue, o * LampAlpha); }
            if (white) { Perimeter(_rel, out float x, out float y); Lamp(x, y, whiteW, whiteH, White, o * _whiteA); }
        }

        /// <summary>
        /// perimeterPoint: a bearing (0 ahead, positive to the left) to the point on
        /// the screen's rectangle in that direction, so straight ahead is the top's
        /// middle and diagonals land in the corners.
        /// </summary>
        private static void Perimeter(float relDeg, out float x, out float y)
        {
            double rad = relDeg * Math.PI / 180.0;
            double dx = -Math.Sin(rad), dy = -Math.Cos(rad);
            double t = 1.0 / Math.Max(Math.Max(Math.Abs(dx), Math.Abs(dy)), 0.0001);
            x = (float)(50.0 + dx * t * 50.0) / 100f * Canvas.W;
            y = (float)(50.0 + dy * t * 50.0) / 100f * Canvas.H;
        }

        /// <summary>polSweep at progress p: -Sweep at 0 and 1, +Sweep at the half, eased.</summary>
        private float Drift(float p)
        {
            float a = _sweepAmt;
            return p < 0.5f ? -a + 2f * a * Num.EaseInOut(p / 0.5f) : a - 2f * a * Num.EaseInOut((p - 0.5f) / 0.5f);
        }

        // One radial-gradient lamp: the gradient picture stretched to the ellipse's
        // stop (rx, ry from the centre), tinted.
        private static void Lamp(float cx, float cy, float rx, float ry, Color c, float alpha)
        {
            Art.Entry lamp = Art.Get("police_lamp");
            if (lamp == null || alpha <= 0f) return;
            Canvas.Image(lamp.Path, cx - rx, cy - ry, rx * 2f, ry * 2f, Canvas.Alpha(c, Num.Clamp01(alpha)));
        }
    }
}
