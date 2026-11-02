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
    /// The ped plays the game's own exercise scenario, and each rep he does in it
    /// (a hand or his head going down and back up) is counted and paid in
    /// Health XP (only Health, by design, as in FiveM). A set is 25 reps, then a minute's rest. Push-ups and sit-ups work
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
            public int Bone;      // what goes down and up once a rep
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
            public bool AtPosition, FromStation;
            public int Model;
        }

        // The exercise a piece of equipment has, where the ped's root goes on it (from its origin, in its own frame: x
        // right, y forward, z up) and which way he faces from its heading: the game's own scenario point on that
        // model (its CExtensionDefSpawnPoint in v_sports.ytyp), which Muscle Beach's gym-goers use. Measured in game
        // too, the same to the millimetre. Started at the origin, or a guessed height, the set played inside the
        // bench, under the sand, on top of the bench or facing away from the bar (iliuha, 2026-11-01); and only one of
        // the bench models has a bench press at all (bench_05 is a pull-up station). Equipment with no placement here
        // isn't offered.
        private readonly Dictionary<int, (string Exercise, Vector3 Offset, float Turn)> _placements = new Dictionary<int, (string, Vector3, float)>();

        private void Placements()
        {
            _placements[Game.GenerateHash("prop_muscle_bench_03")] = ("bench", new Vector3(0.020f, -0.150f, -0.260f), 180f);
            _placements[Game.GenerateHash("prop_muscle_bench_05")] = ("chinups", new Vector3(-0.014f, 0.800f, -0.032f), 180f);
            _placements[Game.GenerateHash("prop_beach_bars_01")] = ("chinups", new Vector3(-1.396f, 0.139f, 0.965f), 180f);
            _placements[Game.GenerateHash("prop_beach_bars_02")] = ("chinups", new Vector3(0f, 0.140f, 0.965f), 180f);
        }

        // config.lua Config.Training.exercises
        private static readonly Exercise[] Exercises =
        {
            new Exercise { Key = "pushups", Label = "Push-ups",     Scenario = "WORLD_HUMAN_PUSH_UPS",               Bone = Head,      Xp = 20 },
            new Exercise { Key = "situps",  Label = "Sit-ups",      Scenario = "WORLD_HUMAN_SIT_UPS",                Bone = Head,      Xp = 18 },
            new Exercise { Key = "weights", Label = "Free weights", Scenario = "WORLD_HUMAN_MUSCLE_FREE_WEIGHTS",    Bone = RightHand, Xp = 28, Station = true },
            new Exercise { Key = "bench",   Label = "Bench press",  Scenario = "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS", Bone = RightHand, Xp = 34, AtProp = true },
            new Exercise { Key = "chinups", Label = "Chin-ups",     Scenario = "PROP_HUMAN_MUSCLE_CHIN_UPS",         Bone = Head,      Xp = 30, AtProp = true },
        };

        private const int Head = 31086, RightHand = 57005;   // SKEL_Head, SKEL_R_Hand

        /// <summary>
        /// One rep: the exercise's bone goes down to near the bottom of its range and back up to near the top, the
        /// range being what it covered in the last few seconds (longer than half the slowest rep, a sit-up's ~5 s; the
        /// whole set would take in the get-down from standing, whose top no push-up or sit-up comes back to). A pause,
        /// or the clip holding the bar up, counts nothing. They were counted on a timer (FiveM's `secs`), out of step
        /// with the presses: he'd come down and up and it wouldn't count (iliuha, 2026-11-02).
        /// </summary>
        private sealed class RepCounter
        {
            private const float MinRange = 0.08f, Margin = 0.3f;
            private const int WindowMs = 4500;
            private readonly Queue<(int At, float Z)> _recent = new Queue<(int, float)>();
            private bool _down;

            public bool Update(float z)
            {
                int now = Game.GameTime;
                _recent.Enqueue((now, z));
                while (_recent.Count > 0 && now - _recent.Peek().At > WindowMs) _recent.Dequeue();
                float lo = float.MaxValue, hi = float.MinValue;
                foreach (var s in _recent)
                {
                    lo = Math.Min(lo, s.Z);
                    hi = Math.Max(hi, s.Z);
                }
                float range = hi - lo;
                if (range < MinRange) return false;
                if (!_down && z <= lo + Margin * range) _down = true;
                else if (_down && z >= hi - Margin * range)
                {
                    _down = false;
                    return true;
                }
                return false;
            }
        }

        private static float BoneZ(Ped ped, int bone) => Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, ped, bone, 0f, 0f, 0f).Z;

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
        private int _reps;              // this set's reps so far
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
            Placements();

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
                    best = new Spot { Exercise = st.Exercise, At = st.At, Heading = st.Heading, AtPosition = true, FromStation = true };
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
                // Only the bench and bar models come back (the pool is filtered in memory),
                // so the reads below are a handful of props, not every one around.
                if (_equipmentModels == null)
                {
                    var models = new List<Model>();
                    foreach (int m in _benchModels) models.Add(new Model(m));
                    foreach (int m in _chinModels) models.Add(new Model(m));
                    _equipmentModels = models.ToArray();
                }
                if (_equipmentModels.Length > 0)
                    foreach (Prop p in World.GetNearbyProps(me, ScanRadius, _equipmentModels))
                        if (p != null) Gather(p.Model.Hash, p.Position, p.Heading);
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
        private Model[] _equipmentModels;
        private long _nextScan;
        private Vector3 _scanAt;

        private void Gather(int model, Vector3 pos, float heading)
        {
            // one of [Gym]'s models, with a placement: the placement says which exercise it is
            if ((_benchModels.Contains(model) || _chinModels.Contains(model)) && _placements.TryGetValue(model, out var place))
                _equipment.Add(new Spot { Exercise = place.Exercise, At = pos, Heading = heading, AtPosition = true, Model = model });
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
            // The scenario starts where the ped's root goes: a placed station's own position, or the measured placement
            // on a bench or bar (see _placements), turned with it.
            if (spot != null && spot.AtPosition)
            {
                Vector3 at = spot.At;
                float heading = spot.Heading;
                if (!spot.FromStation && _placements.TryGetValue(spot.Model, out var place))
                {
                    double h = spot.Heading * Math.PI / 180.0;
                    float c = (float)Math.Cos(h), s = (float)Math.Sin(h);
                    at += new Vector3(place.Offset.X * c - place.Offset.Y * s, place.Offset.X * s + place.Offset.Y * c, place.Offset.Z);
                    heading = (spot.Heading + place.Turn) % 360f;
                }
                if (ex.Key == "bench" && !spot.FromStation)
                {
                    BenchPress(ex, ped, at, heading);
                    return;
                }
                Function.Call(Hash.TASK_START_SCENARIO_AT_POSITION, ped, ex.Scenario, at.X, at.Y, at.Z, heading, 0, true, true);
            }
            else
                Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, ped, ex.Scenario, 0, true);

            // The scenario can refuse (occupied, no room, a model it has no clip for). Using one counts from its first
            // step (getting down for push-ups): IS_PED_ACTIVE_IN_SCENARIO stayed false through that, and after 3 s the
            // set was cleared mid-way down with "Cannot do that here." (iliuha).
            bool started = false;
            for (int i = 0; i < 50; i++)
            {
                Wait(100);
                if (InScenario(ped)) { started = true; break; }
            }
            if (!started)
            {
                Function.Call(Hash.CLEAR_PED_TASKS, ped);
                Notify("Cannot do that here.");
                return;
            }

            int reps = _reps = 0;
            float lostFor = 0f;
            var counter = new RepCounter();
            int last = Game.GameTime;
            while (true)
            {
                Wait(0);
                int t = Game.GameTime;
                float dt = (t - last) / 1000f;
                last = t;

                if (ped.IsDead || Stopped()) break;

                if (InScenario(ped))
                {
                    lostFor = 0f;
                    if (counter.Update(BoneZ(ped, ex.Bone)))
                    {
                        _reps = ++reps;
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

            if (InScenario(ped)) Function.Call(Hash.CLEAR_PED_TASKS, ped);
        }

        private static bool InScenario(Ped ped) => Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, ped);

        // ---- the bench press (its scenario's own rep clip, played here) -----------------------------------
        //
        // PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS plays only for a ped holding its BARBELL prop set (every one of its clips
        // has CScenarioConditionHasProp BARBELL, scenarios.meta) and only for Muscle Beach's own models: the gym-goers
        // spawn with the bar, lying. The player has none, so the scenario put him at the bench's point and played
        // nothing, standing in the bench. Its rep clip, from the bench's point, with the same bar (propsets.meta
        // BARBELL: Prop_Barbell_02) in his right hand, behind a short fade either way: its enter and exit clips need the
        // walk-up and collision handling of the scenario itself, and played by hand they put him in the bench or under
        // the floor (2026-11-02).
        // Its base clip only holds the bar up (a scenario's resting pose); the presses are its idle, which the scenario
        // plays a second into each base (NextIdleTime 1). Looped on its own, base held the bar in the air with no reps.
        private const string BenchReps = "amb@prop_human_seat_muscle_bench_press@idle_a", BenchRepsClip = "idle_a", Barbell = "prop_barbell_02";
        private const int RightHandProp = 28422;   // PH_R_Hand, where scenario props are held
        private const int FadeMs = 300;
        private const float BenchTakenRadius = 1.2f;

        private static bool Load(string dict)
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, dict);
            for (int i = 0; i < 50 && !Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict); i++) Wait(100);
            return Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict);
        }

        private static void FadeOut()
        {
            Function.Call(Hash.DO_SCREEN_FADE_OUT, FadeMs);
            for (int i = 0; i < 20 && !Function.Call<bool>(Hash.IS_SCREEN_FADED_OUT); i++) Wait(50);
        }

        private static bool Playing(Ped ped, string dict, string clip) => Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, ped, dict, clip, 3);

        private static bool BenchTaken(Ped ped, Vector3 at)
        {
            foreach (Ped p in World.GetNearbyPeds(at, BenchTakenRadius))
                if (p != null && p.Exists() && p != ped && !p.IsDead) return true;
            return false;
        }

        private void BenchPress(Exercise ex, Ped ped, Vector3 at, float heading)
        {
            if (BenchTaken(ped, at))
            {
                Notify("Someone is using that.");
                return;
            }
            var bar = new Model(Barbell);
            if (!Load(BenchReps) || !bar.Request(5000))
            {
                Notify("Cannot do that here.");
                return;
            }
            Vector3 stood = ped.Position;
            float stoodHeading = ped.Heading;
            Prop barbell = null;
            try
            {
                FadeOut();
                Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped);
                ped.IsPositionFrozen = true;
                Function.Call(Hash.TASK_PLAY_ANIM_ADVANCED, ped, BenchReps, BenchRepsClip, at.X, at.Y, at.Z, 0f, 0f, heading, 1000f, -4f, -1, 1, 0f, 2, 0);
                barbell = World.CreateProp(bar, ped.Position, false, false);
                if (barbell != null)
                {
                    barbell.IsCollisionEnabled = false;
                    Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, barbell, ped, Function.Call<int>(Hash.GET_PED_BONE_INDEX, ped, RightHandProp),
                        0f, 0f, 0f, 0f, 0f, 0f, false, false, false, false, 2, true);
                }
                Wait(150);
                Function.Call(Hash.DO_SCREEN_FADE_IN, FadeMs);

                float lostFor = 0f;
                var counter = new RepCounter();
                int reps = _reps = 0, last = Game.GameTime;
                while (true)
                {
                    Wait(0);
                    int t = Game.GameTime;
                    float dt = (t - last) / 1000f;
                    last = t;
                    if (ped.IsDead || Stopped()) break;
                    if (Playing(ped, BenchReps, BenchRepsClip))
                    {
                        lostFor = 0f;
                        if (counter.Update(BoneZ(ped, ex.Bone)))
                        {
                            _reps = ++reps;
                            SkillsApi.Award("health", ex.Xp);
                            if (reps >= _setReps)
                            {
                                _restUntil = Game.GameTime + _restSeconds * 1000;
                                Notify(string.Format(CultureInfo.InvariantCulture, "{0} reps. You are winded, rest a minute.", reps));
                                break;
                            }
                        }
                    }
                    else if ((lostFor += dt) > 1f) break;   // knocked out of it
                    DrawSet(ex, reps);
                }
                if (!ped.IsDead) FadeOut();
            }
            finally
            {
                if (barbell != null && barbell.Exists())
                {
                    barbell.Detach();
                    barbell.Delete();
                }
                bar.MarkAsNoLongerNeeded();
                ped.IsPositionFrozen = false;
                Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, ped);
                if (!ped.IsDead)
                {
                    ped.Position = stood;
                    ped.Heading = stoodHeading;
                }
                Function.Call(Hash.REMOVE_ANIM_DICT, BenchReps);
                if (!Function.Call<bool>(Hash.IS_SCREEN_FADED_IN)) Function.Call(Hash.DO_SCREEN_FADE_IN, FadeMs);
            }
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
