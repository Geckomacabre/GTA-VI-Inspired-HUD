using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

namespace ViExtras
{
    /// <summary>
    /// The FiveM version's training (vice_hud client_training.lua and
    /// server_gym.lua), for single player: ways to earn the Health skill.
    ///
    /// The ped plays the game's own exercise scenario and every `secs` on it a
    /// rep is counted and paid in Health XP (only Health, by design, as in
    /// FiveM). A set is 25 reps, then a minute's rest. Push-ups and sit-ups work
    /// anywhere, on their own keys (FiveM's /pushups and /situps); bench press
    /// and chin-ups are offered next to the real gym props (FiveM's ox_target
    /// options on those models), and any exercise at a placed station
    /// ([Gym] Stations, FiveM's data/gym.json).
    /// </summary>
    public sealed class Gym : Script
    {
        private sealed class Exercise
        {
            public string Key, Label, Scenario;
            public float Secs;
            public double Xp;
            public bool AtProp, Station;
        }

        private sealed class Station
        {
            public string Exercise;
            public Vector3 At;
            public float Heading, Radius;
        }

        /// <summary>Somewhere to train: a prop, a map object of a prop model, or a placed station.</summary>
        private sealed class Spot
        {
            public string Exercise;
            public Vector3 At;
            public float Heading;
            public bool AtPosition;
        }

        // config.lua Config.Training.exercises
        private static readonly Exercise[] Exercises =
        {
            new Exercise { Key = "pushups", Label = "Push-ups",     Scenario = "WORLD_HUMAN_PUSH_UPS",               Secs = 3.0f, Xp = 20 },
            new Exercise { Key = "situps",  Label = "Sit-ups",      Scenario = "WORLD_HUMAN_SIT_UPS",                Secs = 3.0f, Xp = 18 },
            new Exercise { Key = "weights", Label = "Free weights", Scenario = "WORLD_HUMAN_MUSCLE_FREE_WEIGHTS",    Secs = 4.0f, Xp = 28, Station = true },
            new Exercise { Key = "bench",   Label = "Bench press",  Scenario = "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS", Secs = 4.5f, Xp = 34, AtProp = true },
            new Exercise { Key = "chinups", Label = "Chin-ups",     Scenario = "PROP_HUMAN_MUSCLE_CHIN_UPS",         Secs = 4.0f, Xp = 30, AtProp = true },
        };

        private static Exercise Find(string key)
        {
            foreach (Exercise ex in Exercises) if (ex.Key == key) return ex;
            return null;
        }

        private const int StopControl = 73;                        // X, "the same cancel key the emote menus use"
        private static readonly int[] MoveControls = { 32, 33, 34, 35 };

        // ---- settings, [Gym] in ViExtras.ini ------------------------------------
        private readonly bool _enabled;
        private readonly Keys _pushUpsKey, _sitUpsKey;
        private readonly int _useControl, _setReps, _restSeconds;
        private readonly float _targetDistance, _stationRadius;
        private readonly HashSet<int> _benchModels = new HashSet<int>(), _chinModels = new HashSet<int>();
        private readonly List<Station> _stations = new List<Station>();

        private bool _exercising;
        private int _restUntil;
        private string _requested;      // an anywhere exercise asked for by key
        private Spot _spot;             // where the player is standing, if somewhere to train
        private long _nextLook;

        public Gym()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Gym", "Enabled", true);
            _pushUpsKey = Config.Get("Gym", "PushUpsKey", Keys.N);
            _sitUpsKey = Config.Get("Gym", "SitUpsKey", Keys.N | Keys.Shift);
            _useControl = Config.Get("Gym", "UseControl", 51);    // E / d-pad right, INPUT_CONTEXT
            _targetDistance = Config.Get("Gym", "TargetDistance", 2.0f);
            _stationRadius = Config.Get("Gym", "StationRadius", 1.2f);
            _setReps = Math.Max(1, Config.Get("Gym", "SetReps", 25));
            _restSeconds = Math.Max(0, Config.Get("Gym", "RestSeconds", 60));
            Models(_benchModels, Config.Get("Gym", "BenchModels",
                "prop_muscle_bench_01, prop_muscle_bench_02, prop_muscle_bench_03, prop_muscle_bench_04, prop_muscle_bench_05, prop_muscle_bench_06, prop_weight_bench_02, m25_2_prop_m52_gymbench_01a"));
            Models(_chinModels, Config.Get("Gym", "ChinUpModels", "prop_beach_bars_01, prop_beach_bars_02, m25_2_prop_m52_gympullup_01a"));
            ParseStations(Config.Get("Gym", "Stations", ""));

            if (!_enabled) return;
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += (s, e) => { if (_exercising) Game.Player.Character?.Task.ClearAll(); };
        }

        private static void Models(HashSet<int> into, string list)
        {
            foreach (string m in list.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                into.Add(Game.GenerateHash(m.Trim()));
        }

        /// <summary>
        /// server_gym.lua clean(): "exercise x y z heading [radius]" stations,
        /// separated by ';'. Only station and prop exercises; radius 0.5..4.
        /// </summary>
        private void ParseStations(string list)
        {
            foreach (string raw in list.Split(new[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] p = raw.Trim().Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 4) continue;
                Exercise ex = Find(p[0].ToLowerInvariant());
                if (ex == null || !(ex.Station || ex.AtProp)) { Log.Write("Gym", "station: unknown exercise " + p[0]); continue; }
                float F(int i, float d) => i < p.Length && float.TryParse(p[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float v) && !float.IsNaN(v) ? v : d;
                float x = F(1, float.NaN), y = F(2, float.NaN), z = F(3, float.NaN);
                if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z) || Math.Abs(x) > 10000 || Math.Abs(y) > 10000 || z < -500 || z > 3000)
                {
                    Log.Write("Gym", "station: bad position in " + raw.Trim());
                    continue;
                }
                float h = ((F(4, 0f) % 360f) + 360f) % 360f;
                _stations.Add(new Station { Exercise = ex.Key, At = new Vector3(x, y, z), Heading = h, Radius = Num.Clamp(F(5, _stationRadius), 0.5f, 4f) });
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_exercising) return;
            if (_pushUpsKey != Keys.None && e.KeyData == _pushUpsKey) _requested = "pushups";
            else if (_sitUpsKey != Keys.None && e.KeyData == _sitUpsKey) _requested = "situps";
        }

        private static void Notify(string text) => Feed.Post(text);

        // ---- the loop ------------------------------------------------------------

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            try { Frame(); }
            catch (Exception ex)
            {
                Log.Write("Gym", ex.ToString());
                _exercising = false;
            }
        }

        private bool Quiet => HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen || SkillsApi.PanelOpen || Binoculars.IsActive || CrashCam.IsPlaying;

        private void Frame()
        {
            // client_training.lua: without the skills system there is nothing to pay into.
            if (!SkillsApi.Ready) { _requested = null; return; }
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return;

            if (_requested != null)
            {
                string key = _requested;
                _requested = null;
                if (!Quiet) Train(Find(key), null);
                return;
            }

            long now = RealTime.Ms;
            if (now >= _nextLook)
            {
                _nextLook = now + 300;
                _spot = ped.IsInVehicle() || ped.IsDead ? null : Look(ped);
            }
            if (_spot == null || Quiet) return;

            Exercise at = Find(_spot.Exercise);
            bool resting = Game.GameTime < _restUntil;
            Prompts.Draw(new List<Prompts.Row>
            {
                new Prompts.Row { Label = Token(at.Label), Glyph = Glyphs.ForControl(_useControl), Off = resting },
            });
            if (Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 0, _useControl)) Train(at, _spot);
        }

        /// <summary>The nearest thing to train on within reach: a station, or a bench or bar.</summary>
        private Spot Look(Ped ped)
        {
            Vector3 me = ped.Position;
            Spot best = null;
            float bestD = float.MaxValue;

            foreach (Station st in _stations)
            {
                // FiveM: a sphere of `radius` round (x, y, z + 1), in ox_target reach.
                float d = me.DistanceTo(st.At + new Vector3(0f, 0f, 1f));
                if (d <= Math.Max(st.Radius, _targetDistance) && d < bestD)
                {
                    bestD = d;
                    best = new Spot { Exercise = st.Exercise, At = st.At, Heading = st.Heading, AtPosition = true };
                }
            }

            // Every bench and bar within ScanRadius, gathered now and then: the
            // map's own are buildings, not props, and the building pool is big.
            long now = RealTime.Ms;
            if (now >= _nextScan || me.DistanceTo(_scanAt) > ScanRadius * 0.5f)
            {
                _nextScan = now + 3000;
                _scanAt = me;
                _equipment.Clear();
                foreach (Prop p in World.GetNearbyProps(me, ScanRadius))
                    if (p != null && p.Exists()) Gather(p.Model.Hash, p.Position, p.Heading);
                foreach (Building b in World.GetNearbyBuildings(me, ScanRadius))
                    if (b != null && b.Exists()) Gather(b.Model.Hash, b.Position, b.Rotation.Z);
            }

            foreach (Spot s in _equipment)
            {
                float d = new Vector2(s.At.X - me.X, s.At.Y - me.Y).Length();
                if (d > _targetDistance || Math.Abs(s.At.Z - me.Z) > 2.5f || d >= bestD) continue;
                bestD = d;
                best = s;
            }
            return best;
        }

        private const float ScanRadius = 60f;
        private readonly List<Spot> _equipment = new List<Spot>();
        private long _nextScan;
        private Vector3 _scanAt;

        private void Gather(int model, Vector3 pos, float heading)
        {
            string ex = _benchModels.Contains(model) ? "bench" : _chinModels.Contains(model) ? "chinups" : null;
            if (ex != null) _equipment.Add(new Spot { Exercise = ex, At = pos, Heading = heading, AtPosition = true });
        }

        // ---- one set (client_training.lua train) --------------------------------------

        private static string Blocked(Ped ped)
        {
            if (ped.IsDead) return "You are in no state to train.";
            if (ped.IsInVehicle()) return "Get out of the vehicle first.";
            if (ped.IsSwimming || ped.IsFalling || ped.IsRagdoll) return "Not here.";
            if (Function.Call<bool>(Hash.IS_PED_CUFFED, ped)) return "Not while restrained.";
            if (Function.Call<bool>(Hash.IS_PED_IN_MELEE_COMBAT, ped) || Function.Call<bool>(Hash.IS_PED_SHOOTING, ped)) return "Not in the middle of a fight.";
            return null;
        }

        private void Train(Exercise ex, Spot spot)
        {
            if (ex == null || _exercising) return;
            Ped ped = Game.Player.Character;
            string why = Blocked(ped);
            if (why != null) { Notify(why); return; }

            int now = Game.GameTime;
            if (now < _restUntil)
            {
                Notify(string.Format(CultureInfo.InvariantCulture, "Catch your breath: {0} s.", (int)Math.Ceiling((_restUntil - now) / 1000.0)));
                return;
            }

            _exercising = true;
            try { Run(ex, spot, ped); }
            finally { _exercising = false; }
        }

        private void Run(Exercise ex, Spot spot, Ped ped)
        {
            if (spot != null && spot.AtPosition)
                Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, ped, ex.Scenario, spot.At.X, spot.At.Y, spot.At.Z, spot.Heading, 0, true, true);
            else
                Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, ped, ex.Scenario, 0, true);

            // The scenario can refuse (occupied, no room, a model it has no clip for).
            bool started = false;
            for (int i = 0; i < 30; i++)
            {
                Wait(100);
                if (Function.Call<bool>(Hash.IS_PED_ACTIVE_IN_SCENARIO, ped)) { started = true; break; }
            }
            if (!started)
            {
                Function.Call(Hash.CLEAR_PED_TASKS, ped);
                Notify("Cannot do that here.");
                return;
            }

            int reps = 0;
            float elapsed = 0f, lostFor = 0f;
            int last = Game.GameTime;
            while (true)
            {
                Wait(0);
                int t = Game.GameTime;
                float dt = (t - last) / 1000f;
                last = t;

                if (ped.IsDead || Stopped()) break;

                if (Function.Call<bool>(Hash.IS_PED_ACTIVE_IN_SCENARIO, ped))
                {
                    lostFor = 0f;
                    elapsed += dt;
                    if (elapsed >= ex.Secs)
                    {
                        elapsed -= ex.Secs;
                        reps++;
                        SkillsApi.Award("health", ex.Xp);
                        if (reps >= _setReps)
                        {
                            _restUntil = Game.GameTime + _restSeconds * 1000;
                            Notify(string.Format(CultureInfo.InvariantCulture, "{0} reps. You are winded, rest a minute.", reps));
                            break;
                        }
                    }
                }
                else
                {
                    // Knocked out of it (hit, bumped, another script's task).
                    lostFor += dt;
                    if (lostFor > 1f) break;
                }

                DrawSet(ex, reps);
            }

            if (Function.Call<bool>(Hash.IS_PED_ACTIVE_IN_SCENARIO, ped)) Function.Call(Hash.CLEAR_PED_TASKS, ped);
        }

        private static bool Stopped()
        {
            if (Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 0, StopControl)) return true;
            foreach (int c in MoveControls)
                if (Math.Abs(Function.Call<float>(Hash.GET_CONTROL_NORMAL, 0, c)) > 0.5f) return true;
            return false;
        }

        /// <summary>FiveM's text UI "Push-ups  3/25  ·  [X] stop", as vice_hud prompt rows.</summary>
        private void DrawSet(Exercise ex, int reps)
        {
            if (Quiet) return;
            string count = Token(ex.Label + " " + reps + "/" + _setReps);
            var rows = new List<Prompts.Row>
            {
                // The counts are rendered for config.lua's 25-rep set; another
                // SetReps shows the exercise alone.
                new Prompts.Row { Label = Art.Get("label_" + count) != null ? count : Token(ex.Label) },
                new Prompts.Row { Label = "stop", Glyph = Glyphs.ForControl(StopControl) },
            };
            Prompts.Draw(rows);
        }

        /// <summary>A prompt word as render_labels names it ("Push-ups 3/25" -> pushx2Dups_3x2F25).</summary>
        private static string Token(string label)
        {
            var sb = new StringBuilder();
            foreach (char c in label.ToLowerInvariant().Replace(' ', '_'))
                sb.Append(c < 128 && (char.IsLetterOrDigit(c) || c == '_') ? c.ToString() : "x" + ((int)c).ToString("X2"));
            return sb.ToString();
        }
    }
}
