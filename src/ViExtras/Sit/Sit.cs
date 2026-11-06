using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Sit anywhere: stand still facing a bench, a low wall, a ledge or a drop and SIT comes up on our world prompt
    /// (E / d-pad right); a wall taller than you gives LEAN. E again (STAND UP) or walking off gets you up.
    ///
    /// A port of invalid0190's dynamic-sit for FiveM (MIT; scripts\ViExtras\Sit\LICENSE.md): its ray sweep, edge
    /// scan, fall check, style rules, offsets and the game's own seat / lean scenarios. What's single player's own:
    ///  * Its /sit and /stand commands are our walk-up prompt, as the dog's PET and the bag's PICK UP are.
    ///  * The prompt only comes up once you've stood still for StillMs facing something, so walking past walls
    ///    doesn't flash it; and never at the ground (its picnic fallback), which is everywhere.
    ///  * The rays skip cars and people, and SIT doesn't come up on an ATM, a vending machine, a pump or a meter,
    ///    or with someone right in front of you (GREET's spot), so it doesn't share E with another prompt.
    ///  * No multiplayer state bag; getting hurt, a new star or a mission stands you up.
    ///
    /// [Switches] Sit in ViExtras.ini; the rest in [Sit].
    /// </summary>
    public sealed class Sit : Script
    {
        private const string Section = "Sit";
        private const int Context = 51;                  // INPUT_CONTEXT: E / d-pad right
        private const float ButtonToWord = 0.8f * 10.8f * 0.7f;   // as MemoryInteract's world prompt
        private const IntersectFlags Rays = IntersectFlags.Map | IntersectFlags.Objects;

        // config.lua's Config.Offsets (forward along the wall's normal, z down from the edge) and Config.Scenarios
        private static readonly Dictionary<string, (float forward, float z)> Offsets = new Dictionary<string, (float, float)>
        {
            ["edge_fall"] = (-0.03f, 2.0f),
            ["ledge"] = (-0.3f, 1.05f),
            ["bench"] = (-0.2f, 1.0f),
            ["lean"] = (0.1f, 0.0f),
            ["ground"] = (0.1f, -0.95f),
        };
        // A wall, a ledge or a drop: the two poses made for sitting on an edge. (config.lua picked from seven, chairs
        // and a bus stop's among them, which sat oddly on a wall.)
        private static readonly string[] SeatScenarios = { "WORLD_HUMAN_SEAT_WALL", "WORLD_HUMAN_SEAT_LEDGE" };

        // The game's own seats: benches, chairs and the like carry scenario points, where its people sit. Within
        // SeatReach of what you face, SIT uses one of those (the seat, its height and its pose are the game's), so a
        // bench isn't taken for a ledge: that seated you on whatever was highest, its backrest.
        private static readonly string[] SeatPoints =
        {
            "PROP_HUMAN_SEAT_BENCH", "PROP_HUMAN_SEAT_CHAIR", "PROP_HUMAN_SEAT_BUS_STOP_WAIT", "PROP_HUMAN_SEAT_ARMCHAIR",
            "PROP_HUMAN_SEAT_CHAIR_UPRIGHT", "PROP_HUMAN_SEAT_DECKCHAIR", "PROP_HUMAN_SEAT_BENCH_FOOD",
            "PROP_HUMAN_SEAT_BENCH_DRINK", "PROP_HUMAN_SEAT_CHAIR_FOOD", "PROP_HUMAN_SEAT_CHAIR_DRINK",
            "PROP_HUMAN_SEAT_SUNLOUNGER", "PROP_HUMAN_SEAT_STRIP_WATCH", "PROP_HUMAN_SEAT_BAR", "PROP_HUMAN_SEAT_COMPUTER",
        };
        private const float SeatReach = 1.3f;
        // client.lua's vertical sweep: heights from the player's root, and the three side offsets
        private static readonly float[] SweepHeights = { -0.5f, -0.4f, -0.3f, -0.2f, -0.1f, 0.0f, 0.1f, 0.2f, 0.3f, 0.5f, 0.7f, 1.0f, 1.5f };
        private static readonly float[] SweepSides = { 0.0f, -0.15f, 0.15f };

        // props with their own E prompt: no SIT on them
        private static readonly HashSet<int> NoSitModels = new HashSet<int>();
        private static readonly string[] NoSitNames =
        {
            "prop_atm_01", "prop_atm_02", "prop_atm_03", "prop_fleeca_atm",
            "prop_vend_soda_01", "prop_vend_soda_02", "prop_vend_snak_01", "prop_vend_coffe_01", "prop_vend_water_01",
            "prop_vend_fridge01", "prop_vend_condom_01", "prop_vend_fags_01",
            "prop_gas_pump_1a", "prop_gas_pump_1b", "prop_gas_pump_1c", "prop_gas_pump_1d", "prop_gas_pump_old2",
            "prop_gas_pump_old3", "prop_vintage_pump",
            "prop_parknmeter_01", "prop_parknmeter_02", "prop_parkingpay",
            "prop_phonebox_01a", "prop_phonebox_01b", "prop_phonebox_01c", "prop_phonebox_02", "prop_phonebox_04",
        };

        private readonly bool _enabled;
        private readonly int _stillMs;
        private readonly bool _lean, _edges, _skipInMissions, _debug;
        private readonly Random _rng = new Random();

        // standing still, and what's in front
        private Vector3 _stillAt;
        private float _stillHeading;
        private int _stillSince, _scanAt;
        private Spot _spot;

        // sitting
        private bool _seated;
        private string _style;
        private Vector3 _from;
        private float _fromHeading;
        private int _seatedAt, _wantedAt, _healthAt;
        private bool _contextWasDown;

        // the debug lines (client.lua's AddDebugPoint), for 15 s
        private readonly List<(Vector3 a, Vector3 b, Color c, int until)> _lines = new List<(Vector3, Vector3, Color, int)>();

        private sealed class Spot
        {
            public Vector3 Hit, Edge;
            public int Entity;
            public float HighestHitZOff;
            public bool HasEdge, LeanFallback, FallLedge, FromSweep, FromStanding;
            public string Style;
        }

        public Sit()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get(Config.Switches, "Sit", true) && Config.Get(Section, "Enabled", true);
            if (!_enabled) return;
            _stillMs = Config.Get(Section, "StillMs", 700);
            _lean = Config.Get(Section, "Lean", true);
            _edges = Config.Get(Section, "Edges", true);
            _skipInMissions = Config.Get(Section, "SkipInMissions", true);
            _debug = Config.Get(Section, "Debug", false);
            foreach (string n in NoSitNames) NoSitModels.Add(Game.GenerateHash(n));
            Tick += OnTick;
            Aborted += (s, e) => { if (_seated) StandUp(Game.Player.Character, "the script stopped"); };
        }

        private void Trace(string s)
        {
            if (_debug) Log.Write("Sit", s);
        }

        // ---- the tick ------------------------------------------------------------------------------------------------

        private void OnTick(object sender, EventArgs e)
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;
            int now = Game.GameTime;
            bool pressed = ContextJustPressed();
            if (_debug) DrawLines(now);

            if (_seated)
            {
                Seated(player, now, pressed);
                return;
            }

            bool hidden = HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen;
            if (hidden || !Free(player) || (_skipInMissions && Function.Call<bool>(Hash.GET_MISSION_FLAG)))
            {
                _spot = null;
                _stillSince = 0;
                return;
            }

            // standing still, facing the same way
            Vector3 at = player.Position;
            float heading = player.Heading;
            if (_stillSince == 0 || at.DistanceTo(_stillAt) > 0.08f || Math.Abs(Angle(heading - _stillHeading)) > 6f
                || player.Velocity.Length() > 0.25f)
            {
                _stillAt = at;
                _stillHeading = heading;
                _stillSince = now;
                _spot = null;
                _scanAt = 0;
                return;
            }
            if (now - _stillSince < _stillMs) return;
            if (_scanAt == 0)
            {
                _scanAt = now;
                _spot = Detect(player);
                if (_spot != null)
                    Trace("found " + _spot.Style + " at " + _spot.Edge + " (entity " + _spot.Entity + ", highest hit +" + _spot.HighestHitZOff + ")");
            }
            if (_spot == null || SomeoneInFront(player)) return;

            DrawPrompt(_spot.Edge + new Vector3(0f, 0f, 0.35f), Context, _spot.Style == "lean" ? "wa_label_lean" : "wa_label_sit");
            if (pressed) SitDown(player, _spot, now);
        }

        private static float Angle(float a)
        {
            while (a > 180f) a -= 360f;
            while (a < -180f) a += 360f;
            return a;
        }

        private static bool Free(Ped player)
        {
            if (player.IsDead || player.IsInVehicle() || Function.Call<bool>(Hash.IS_PED_GETTING_INTO_A_VEHICLE, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_RAGDOLL, player) || Function.Call<bool>(Hash.IS_PED_SWIMMING, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_CLIMBING, player) || Function.Call<bool>(Hash.IS_PED_CUFFED, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_FALLING, player) || Function.Call<bool>(Hash.IS_PED_JUMPING, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_IN_COMBAT, player, 0) || Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, player)) return false;
            return true;
        }

        // GREET / ANTAGONIZE come up on a person you face: no SIT on top of them
        private static bool SomeoneInFront(Ped player)
        {
            Vector3 me = player.Position;
            Vector3 fwd = player.ForwardVector;
            foreach (Ped p in World.GetNearbyPeds(player, 2f))
            {
                if (p == null || !p.Exists() || p.IsDead || p.Handle == player.Handle) continue;
                Vector3 d = p.Position - me;
                d.Z = 0f;
                if (d.Length() < 2f && Vector3.Dot(Vector3.Normalize(d), fwd) > 0.5f) return true;
            }
            return false;
        }

        // E / d-pad right, this frame. GTA6Hud turns INPUT_CONTEXT off on foot for its own prompts, so it's read the
        // way the dog walkers and the fuel nozzle read it: the disabled control's state, or the E key itself.
        private bool ContextJustPressed()
        {
            bool down = Game.IsKeyPressed(Keys.E) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, Context);
            bool just = down && !_contextWasDown;
            _contextWasDown = down;
            return just;
        }

        // ---- DetectSurface (client.lua), with SHVDN's rays ------------------------------------------------------------

        private RaycastResult Ray(Vector3 a, Vector3 b, Ped ignore, Color debug)
        {
            if (_debug) _lines.Add((a, b, debug, Game.GameTime + 15000));
            return World.Raycast(a, b, Rays, ignore);
        }

        private Spot Detect(Ped ped)
        {
            Vector3 coords = ped.Position;
            Vector3 forward = ped.ForwardVector;
            Spot best = null;

            // 1. The vertical sweep: walls, benches and props right in front
            float highest = 0f, shortest = 999f;
            var right = new Vector3(-forward.Y, forward.X, 0f);
            foreach (float zOff in SweepHeights)
            {
                foreach (float hOff in SweepSides)
                {
                    Vector3 start = new Vector3(coords.X + right.X * hOff, coords.Y + right.Y * hOff, coords.Z + zOff);
                    RaycastResult r = Ray(start, start + forward * 0.8f, ped, Color.Red);
                    if (!r.DidHit) continue;
                    if (zOff > highest) highest = zOff;
                    float dist = coords.DistanceTo(r.HitPosition);
                    if (dist < shortest)
                    {
                        shortest = dist;
                        best = new Spot { Hit = r.HitPosition, Entity = r.HitEntity != null ? r.HitEntity.Handle : 0 };
                    }
                }
            }
            if (best != null)
            {
                best.HighestHitZOff = highest;
                best.FromSweep = true;
            }

            // 2. Edge-standing: up on a low wall, sit on its edge from where you stand
            if (best == null && _edges)
            {
                Vector3 edgeStart = coords + new Vector3(0f, 0f, 0.5f);
                RaycastResult r = Ray(edgeStart, edgeStart + forward * 0.7f + new Vector3(0f, 0f, -1.0f), ped, Color.Orange);
                if (r.DidHit)
                {
                    Vector3 groundStart = edgeStart + forward * 1.0f;
                    RaycastResult g = Ray(groundStart, groundStart + new Vector3(0f, 0f, -2.0f), ped, Color.Lime);
                    if (!g.DidHit)
                        best = new Spot { Hit = r.HitPosition, Edge = r.HitPosition, Entity = r.HitEntity != null ? r.HitEntity.Handle : 0, FromStanding = true, HasEdge = true };
                }
            }

            // 3. Edge-lookout: a drop in front (a roof's or a pier's edge)
            if (best == null && _edges)
            {
                bool floor = false;
                for (int i = -10; i <= 15 && best == null; i++)
                {
                    float dist = i * 0.1f;
                    Vector3 scanStart = coords + forward * dist + new Vector3(0f, 0f, 0.45f);
                    RaycastResult r = Ray(scanStart, scanStart + new Vector3(0f, 0f, -2.2f), ped, Color.Red);
                    if (r.DidHit) { floor = true; continue; }
                    if (!floor || dist < 0.05f) continue;
                    Vector3 edge = coords + forward * (dist - 0.1f);
                    for (int back = 1; back <= 10; back++)
                    {
                        float sub = dist - back * 0.01f;
                        Vector3 subStart = coords + forward * sub + new Vector3(0f, 0f, 0.45f);
                        if (Ray(subStart, subStart + new Vector3(0f, 0f, -1.2f), ped, Color.Red).DidHit)
                        {
                            edge = coords + forward * sub;
                            break;
                        }
                    }
                    best = new Spot { Hit = edge, Edge = edge, HasEdge = true };
                }
            }

            // 4. The ground fallback (a picnic sit) is left out: it's everywhere, so its prompt would be too.
            if (best == null) return null;
            if (best.Entity != 0 && !SeatEntity(best.Entity)) return null;

            // 5. The edge scan: the top of what was hit
            if (!best.HasEdge)
            {
                float highestZ = -99f;
                bool found = false;
                for (int i = 0; i <= 18; i++)
                {
                    float depth = (i - 3) * 0.04f;
                    Vector3 p = best.Hit + forward * depth;
                    RaycastResult r = Ray(new Vector3(p.X, p.Y, best.Hit.Z + 1.5f), new Vector3(p.X, p.Y, best.Hit.Z - 1.0f), ped, Color.Yellow);
                    if (!r.DidHit) continue;
                    float hDiff = r.HitPosition.Z - best.Hit.Z;
                    if (r.HitPosition.Z > highestZ && hDiff < 0.8f && hDiff > -0.4f)
                    {
                        highestZ = r.HitPosition.Z;
                        best.Edge = r.HitPosition;
                        found = true;
                    }
                }
                if (!found)
                {
                    if (coords.DistanceTo(best.Hit) >= 1.0f) return null;
                    best.Edge = best.Hit;
                    best.LeanFallback = true;
                }
                best.HasEdge = true;
            }

            // 6. The fall check: straight down from the seat
            Vector3 down = best.Edge + new Vector3(0f, 0f, 0.1f);
            RaycastResult d = Ray(down, best.Edge - new Vector3(0f, 0f, 2.5f), ped, Color.Cyan);
            float fall = d.DidHit ? down.DistanceTo(d.HitPosition) : 3f;
            best.FallLedge = !d.DidHit || fall > 0.9f;

            // the sit command's style rules
            if (!best.FallLedge && best.HighestHitZOff >= 1.0f) best.Style = "lean";
            else if (best.FromSweep || best.FromStanding) best.Style = "ledge";
            else if (best.FallLedge) best.Style = "edge_fall";
            else best.Style = "ledge";
            if (best.Style == "lean" && !_lean) return null;
            return best;
        }

        private static bool SeatEntity(int handle)
        {
            if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, handle)) return true;
            if (Function.Call<bool>(Hash.IS_ENTITY_A_PED, handle) || Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, handle)) return false;
            return !NoSitModels.Contains(Function.Call<int>(Hash.GET_ENTITY_MODEL, handle));
        }

        // ---- the sit command (client.lua RegisterCommand "sit") -----------------------------------------------------

        private void SitDown(Ped ped, Spot s, int now)
        {
            if (s.Style != "lean" && s.Style != "edge_fall" && GameSeat(ped, s)) return;
            Vector3 me = ped.Position;
            var diff = new Vector3(me.X - s.Hit.X, me.Y - s.Hit.Y, 0f);
            Vector3 normal = diff.Length() < 0.01f ? -ped.ForwardVector : Vector3.Normalize(diff);
            (float forward, float z) off = Offsets.TryGetValue(s.Style, out var o) ? o : Offsets["ledge"];
            float sitOffset = s.Style == "edge_fall" ? 0.25f : off.forward;
            float forwardOffset = s.Style == "lean" ? 0.12f : sitOffset;
            var spawn = new Vector3(s.Hit.X + normal.X * forwardOffset, s.Hit.Y + normal.Y * forwardOffset, s.Edge.Z - off.z);
            if (s.Style == "lean") spawn.Z = me.Z;
            float heading = Function.Call<float>(Hash.GET_HEADING_FROM_VECTOR_2D, normal.X, normal.Y);
            if (s.Style == "edge_fall") heading = (heading + 180f) % 360f;

            _from = me;
            _fromHeading = ped.Heading;
            string scenario;
            if (s.Style == "lean")
            {
                scenario = "WORLD_HUMAN_LEANING";
                Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, ped, scenario, spawn.X, spawn.Y, spawn.Z, heading, 0, true, true);
            }
            else
            {
                scenario = SeatScenarios[_rng.Next(SeatScenarios.Length)];
                Function.Call(Hash.FREEZE_ENTITY_POSITION, ped, true);
                Function.Call(Hash.SET_ENTITY_COLLISION, ped, false, false);
                Function.Call(Hash.SET_ENTITY_HEADING, ped, heading);
                Function.Call(Hash.SET_ENTITY_COORDS, ped, spawn.X, spawn.Y, spawn.Z, false, false, false, false);
                Wait(50);
                Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, ped, scenario, 0, true);
                Function.Call(Hash.SET_ENTITY_COLLISION, ped, true, true);
            }
            _healthAt = ped.Health + ped.Armor;
            _seated = true;
            _style = s.Style;
            _seatedAt = Game.GameTime;
            _wantedAt = Game.Player.WantedLevel;
            _spot = null;
            Trace(s.Style + ": " + scenario + " at " + spawn + " heading " + heading.ToString("0.0") + " (edge " + s.Edge + ", hit " + s.Hit + ")");
        }

        // A free seat of the game's in reach of what you face: the game sits you on it (its own walk-up warped, its
        // pose). Checked a moment later that it's a seat you're on; if not, the edge placement takes over.
        private bool GameSeat(Ped ped, Spot s)
        {
            Vector3 at = s.Hit;
            string seat = null;
            foreach (string t in SeatPoints)
                if (Function.Call<bool>(Hash.DOES_SCENARIO_OF_TYPE_EXIST_IN_AREA, at.X, at.Y, at.Z, t, SeatReach, true)) { seat = t; break; }
            if (seat == null) return false;
            Vector3 me = ped.Position;
            float meHeading = ped.Heading;
            Function.Call(Hash.TASK_USE_NEAREST_SCENARIO_TO_COORD_WARP, ped, at.X, at.Y, at.Z, SeatReach, 0);
            Wait(400);
            bool onSeat = false;
            foreach (string t in SeatPoints)
                if (Function.Call<bool>(Hash.IS_PED_USING_SCENARIO, ped, t)) { onSeat = true; break; }
            if (!onSeat)
            {
                // the nearest point wasn't a seat (or it was taken meanwhile): back where you stood
                Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped);
                ped.Position = me;
                ped.Heading = meHeading;
                Trace("the nearest scenario by " + seat + " wasn't a seat: the edge placement instead");
                return false;
            }
            _from = me;
            _fromHeading = meHeading;
            _healthAt = ped.Health + ped.Armor;
            _seated = true;
            _style = "seat";
            _seatedAt = Game.GameTime;
            _wantedAt = Game.Player.WantedLevel;
            _spot = null;
            Trace("seat: the game's " + seat + " by " + at);
            return true;
        }

        // ---- sitting, and getting up (client.lua ResetPlayer) --------------------------------------------------------

        private void Seated(Ped ped, int now, bool pressed)
        {
            if (!ped.Exists() || ped.IsDead) { StandUp(ped, "dead"); return; }
            if (HudBridge.Busy) { StandUp(ped, "a cutscene or switch"); return; }
            if (_skipInMissions && Function.Call<bool>(Hash.GET_MISSION_FLAG)) { StandUp(ped, "a mission started"); return; }
            if (Game.Player.WantedLevel > _wantedAt) { StandUp(ped, "wanted"); return; }
            if (ped.Health + ped.Armor < _healthAt) { StandUp(ped, "hurt"); return; }
            if (now - _seatedAt > 1500 && !Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, ped)) { StandUp(ped, "the scenario ended"); return; }

            bool moving = false;
            for (int c = 32; c <= 35; c++)    // INPUT_MOVE_UP_ONLY .. INPUT_MOVE_RIGHT_ONLY, as client.lua
                if (Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, c) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, c)) moving = true;
            if (now - _seatedAt > 600 && (moving || pressed)) { StandUp(ped, moving ? "moved" : "E"); return; }

            if (!(HudBridge.Paused || HudBridge.HudMenuOpen))
                DrawPrompt(ped.Position + new Vector3(0f, 0f, 0.6f), Context, "wa_label_stand_up");
        }

        private void StandUp(Ped ped, string why)
        {
            if (!_seated) return;
            _seated = false;
            _stillSince = 0;
            if (ped == null || !ped.Exists()) return;
            Function.Call(Hash.FREEZE_ENTITY_POSITION, ped, false);
            Function.Call(Hash.SET_ENTITY_COLLISION, ped, true, true);
            if (_style == "edge_fall")
            {
                // back to safety from a drop's edge
                Function.Call(Hash.SET_ENTITY_COORDS, ped, _from.X, _from.Y, _from.Z, false, false, false, false);
                Function.Call(Hash.SET_ENTITY_HEADING, ped, _fromHeading);
            }
            Function.Call(Hash.CLEAR_PED_TASKS, ped);
            Trace("stood up (" + why + ")");
        }

        // ---- the world prompt: button, then the word (DogWalkers.DrawPrompt, MemoryInteract.Draw) -------------------

        private static void DrawPrompt(Vector3 at, int control, string label)
        {
            var sx = new OutputArgument();
            var sy = new OutputArgument();
            if (!Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, at.X, at.Y, at.Z, sx, sy)) return;
            float x = sx.GetResult<float>() * Canvas.W;
            float y = sy.GetResult<float>() * Canvas.H;
            Art.Entry shared = Glyphs.ForControl(control);
            Art.Entry button = shared != null ? Art.Get("wa_" + System.IO.Path.GetFileNameWithoutExtension(shared.Path)) : null;
            Art.Entry word = Art.Get(label);
            float bh = button != null ? button.BoxH : 0f;
            float wh = word != null ? word.BoxH : 0f;
            float rowH = Math.Max(bh, wh);
            if (button != null)
            {
                Art.Draw(button, x, y + (rowH - bh) * 0.5f, Color.White);
                x += button.BoxW + ButtonToWord;
            }
            if (word != null) Art.Draw(word, x, y + (rowH - wh) * 0.5f, Color.White);
        }

        private void DrawLines(int now)
        {
            for (int i = _lines.Count - 1; i >= 0; i--)
            {
                var l = _lines[i];
                if (now > l.until) { _lines.RemoveAt(i); continue; }
                Function.Call(Hash.DRAW_LINE, l.a.X, l.a.Y, l.a.Z, l.b.X, l.b.Y, l.b.Z, (int)l.c.R, (int)l.c.G, (int)l.c.B, 255);
            }
        }
    }
}
