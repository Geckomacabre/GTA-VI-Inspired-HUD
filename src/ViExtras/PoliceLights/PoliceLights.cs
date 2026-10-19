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
    /// screen edges facing it: two lamps per edge, red and blue, with the white
    /// takedown between them. They fire in short hard bursts the way a real
    /// lightbar does (a red double tap, a white pop, a blue double tap, a white
    /// pop, every FlashMs) and drift a little, red and blue in opposite phase.
    /// The glow is spread over the two edges nearest the siren by the cosine of
    /// the angle to each, so a car circling you sweeps round the screen, and it
    /// is full strength inside FullDistance, fading out to MaxDistance.
    ///
    /// Two of vice_hud's modes ([PoliceLights] Mode): Radius, the default here,
    /// puts one red / white / blue group on the screen's edge at the bearing of
    /// the strongest siren alone (a car behind and to the left lights the bottom
    /// left corner), so the light only ever comes from where the police are;
    /// Edges lights each of the four edges for every siren facing it.
    /// It replaces GTA6Hud's police arrows (GTA6Hud.ini [PoliceArrow] Enabled =
    /// False). vice_hud's in-game tuning panel (/hudpolice edit) is not ported;
    /// the same numbers are in [PoliceLights] in ViExtras.ini.
    /// </summary>
    public sealed class PoliceLights : Script
    {
        private const float W1 = 19.2f;            // --w at 1080p
        private const float Vh = 10.8f;            // 1vh at 1080p
        private const int FadeMs = 420;            // .pol transition: opacity 420ms cubic-bezier(0.4, 0, 0.2, 1)

        // .pol-r / .pol-b / .pol-w --polc
        private static readonly Color Red = Color.FromArgb(255, 255, 42, 58);
        private static readonly Color Blue = Color.FromArgb(255, 64, 118, 255);
        private static readonly Color White = Color.FromArgb(255, 255, 250, 240);
        private const float LampAlpha = 0.9203f;

        // client.lua EDGE_ANGLE: 0 is straight ahead, positive is to the left.
        private enum Edge { Top, Right, Bottom, Left }
        private static readonly float[] EdgeAngle = { 0f, -90f, 180f, 90f };

        private readonly bool _enabled, _white, _requireSiren;
        private readonly int _scanMs, _flashMs, _sweepMs;
        private readonly float _maxDistance, _fullDistance, _maxOpacity, _focus, _whiteA;
        private readonly float _lampA, _lampB, _whiteLampA, _whiteLampB, _spread, _softness, _sweepAmt;
        private readonly bool _radius;
        private readonly float _radiusSpread;
        private float _rel, _relTarget;
        private bool _relSet;

        private long _nextScan;
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
            _whiteLampA = Config.Get(S, "WhiteLampWidth", 19f);
            _whiteLampB = Config.Get(S, "WhiteLampHeight", 30f);
            _spread = Config.Get(S, "Spread", 10f);   // vice_hud has 24; closer together here
            _radius = !string.Equals(Config.Get(S, "Mode", "Radius"), "Edges", StringComparison.OrdinalIgnoreCase);
            _radiusSpread = Config.Get(S, "RadiusSpread", 9f);   // degrees round the edge (vice_hud 24)
            _softness = Config.Get(S, "Softness", 68f);
            _sweepAmt = Config.Get(S, "Sweep", 1.6f);
            _sweepMs = Math.Max(100, Config.Get(S, "SweepMs", 2600));
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            if (!_enabled) return;
            try
            {
                long now = RealTime.Ms;
                if (now >= _nextScan)
                {
                    _nextScan = now + Math.Max(50, _scanMs);
                    Scan(now);
                }
                if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen || CrashCam.IsPlaying || Binoculars.IsActive) return;
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
            float best = 0f, bestRel = 0f;
            Ped ped = Game.Player.Character;
            Vector3 p = ped.Position;
            float camHeading = Function.Call<Vector3>(Hash.GET_GAMEPLAY_CAM_ROT, 2).Z;
            float maxDist = _maxDistance;
            float fullDist = Math.Min(_fullDistance, maxDist - 1f);

            foreach (Vehicle v in World.GetNearbyVehicles(p, maxDist))
            {
                if (v == null || !v.Exists() || v.ClassType != VehicleClass.Emergency) continue;
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
                // Radius: only the strongest siren places the lamps (scanPoliceLightsRadius).
                if (strength > best) { best = strength; bestRel = rel; }
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
            for (int i = 0; i < 4; i++)
            {
                // Rounded as client.lua's change key is, so distance jitter does not restart the fade.
                float target = (float)Math.Round(edges[i], 2);
                if (Math.Abs(target - _to[i]) < 0.005f) continue;
                _from[i] = Opacity(i, now);
                _to[i] = target;
                _since[i] = now;
            }
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

        // ---- the lamps (style.css .pol-*) ------------------------------------------------------

        private void Draw(long now)
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
