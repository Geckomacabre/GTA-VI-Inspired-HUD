using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>What the other extras (the gym) can ask of the skills script.</summary>
    internal static class SkillsApi
    {
        /// <summary>True once the Skills script is running and has a character loaded.</summary>
        public static bool Ready { get; internal set; }

        /// <summary>The panel is open: nothing else should draw over its scrim.</summary>
        public static bool PanelOpen { get; internal set; }

        /// <summary>Add XP to a skill (client_skills.lua ViceSkillAward). False when that skill earns no XP here.</summary>
        public static bool Award(string id, double amount) => Handler != null && Handler(id, amount);

        internal static Func<string, double, bool> Handler;
    }

    /// <summary>
    /// The FiveM version's skills (vice_hud client_skills.lua, skills.lua,
    /// server_skills.lua), for single player.
    ///
    ///  * Michael, Franklin and Trevor already have the game's own skill stats
    ///    (SPn_STAMINA, SHOOTING_ABILITY, ...), raised by the game as they play.
    ///    Those ARE their levels: the panel shows them, and nothing here
    ///    writes them. The one
    ///    skill the game does not have, Health, runs on FiveM's own XP (jogging,
    ///    the gym) and lengthens max health as in FiveM.
    ///  * Lucia (and a freemode ped, which is what FiveM made it for) gets FiveM's system
    ///    whole: every skill earned from what she does, on FiveM's curve
    ///    (100 + 18 x level), started at level 50 except Health, saved per
    ///    character under scripts\ViExtras\Save, each level written into the
    ///    stat FiveM writes ([Skills] LuciaStatPrefix).
    ///
    /// The panel opens on [Skills] Key (FiveM's /skills). FiveM's level-up card
    /// (#skillup) is left out: it is not part of GTA VI.
    /// </summary>
    public sealed class Skills : Script
    {
        private const int StockMaxHealth = 200;
        private const int SettleMs = 3000;

        // controls
        private const int CursorX = 239, CursorY = 240, CursorAccept = 237, ScrollUp = 241, ScrollDown = 242;
        private const int FrontendUp = 188, FrontendDown = 187, FrontendCancel = 202, PauseKey = 199, PauseAlt = 200;

        // ---- settings, [Skills] in ViExtras.ini ------------------------------------
        private readonly bool _enabled, _drivingAffectsHandling, _cursor;
        private readonly Keys _key, _testKey;
        private readonly int _sampleMs, _saveMs, _statPollMs, _startingLevel;
        private readonly float _maxStep, _healthMaxBonus, _jogStaminaFactor, _drivingGripBonus;
        private readonly string _luciaPrefix;
        private readonly double _testXp;
        private readonly HashSet<int> _luciaModels = new HashSet<int>();
        private readonly string _luciaKey;

        // ---- the character being played --------------------------------------------
        private sealed class Character
        {
            public string Key;
            public int Slot = -1;                  // 0..2 for Michael, Franklin, Trevor
            public bool Story => Slot >= 0;
            public readonly Dictionary<string, double> Xp = new Dictionary<string, double>(StringComparer.Ordinal);
            public bool Dirty;
            public int[] Seen;                     // story stats as last read, -1 = not yet
        }

        private Character _ch;
        private bool _loaded;
        private int _model;
        private long _settleFrom = -1;
        private long _nextSample, _nextSave, _nextStatPoll, _nextHealth;

        // activity
        private Vector3 _lastPos;
        private bool _hasLastPos;
        private long _lastShotAt = -10000, _lastMeleeAt = -10000;
        private Dictionary<int, int> _pedHealth = new Dictionary<int, int>();

        // driving handling (characters without story stats)
        private struct Traction { public float CurveMax, CurveMin, LossMult; }
        private readonly Dictionary<int, Traction> _tractionBase = new Dictionary<int, Traction>();
        private Vehicle _drivingVeh;

        // max health
        private int _healthPed;
        private int _prevRaw = -1, _prevMax = -1;
        private long _overpowerCheckedAt = -10000;
        private bool _overpowered;

        // the panel
        private bool _toggleRequested, _testRequested;
        private bool _open;
        private List<SkillRow> _rows = new List<SkillRow>();
        private SkillsUi.Layout _layout;
        private float _scroll;
        private bool _minimapHeld;

        public Skills()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Skills", "Enabled", true);
            _key = Config.Get("Skills", "Key", Keys.K);
            _cursor = Config.Get("Skills", "MouseCursor", true);
            _startingLevel = Config.Get("Skills", "StartingLevel", 50);
            _sampleMs = Config.Get("Skills", "TickMs", 200);
            _saveMs = Config.Get("Skills", "SaveMs", 30000);
            _statPollMs = Config.Get("Skills", "StatPollMs", 1000);
            _maxStep = Config.Get("Skills", "MaxStep", 40f);
            _healthMaxBonus = Config.Get("Skills", "HealthMaxBonus", 60f);
            _jogStaminaFactor = Config.Get("Skills", "JogStaminaFactor", 0.4f);
            _drivingAffectsHandling = Config.Get("Skills", "DrivingAffectsHandling", true);
            _drivingGripBonus = Config.Get("Skills", "DrivingGripBonus", 0.20f);
            _luciaPrefix = (Config.Get("Skills", "LuciaStatPrefix", "None") ?? "").Trim();
            if (_luciaPrefix.Equals("None", StringComparison.OrdinalIgnoreCase)) _luciaPrefix = "";
            _testKey = Config.Get("Skills", "TestKey", Keys.None);
            _testXp = Config.Get("Skills", "TestXp", 500.0);

            string lucia = "lucia_s";
            try { lucia = ScriptSettings.Load(Path.Combine(Paths.Scripts, "GTA6Hud.ini")).GetValue("Lucia", "Model", lucia); } catch { }
            _luciaKey = string.IsNullOrWhiteSpace(lucia) ? "lucia_s" : lucia.Trim().ToLowerInvariant();
            foreach (string m in new[] { "lucia", "lucia_nophys", "lucia_s", _luciaKey }) _luciaModels.Add(Game.GenerateHash(m));

            if (!_enabled) return;
            SkillsApi.Handler = (id, amount) => Award(id, amount);
            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += (s, e) => Shutdown();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_key != Keys.None && e.KeyCode == _key) _toggleRequested = true;
            if (_testKey != Keys.None && e.KeyCode == _testKey) _testRequested = true;
        }

        private void Shutdown()
        {
            Save(_ch);
            if (_drivingVeh != null) RestoreHandling(_drivingVeh);
            _drivingVeh = null;
            if (_open) Close();
            SkillsApi.Ready = false;
            SkillsApi.PanelOpen = false;
        }

        // =============================================================================
        // Tick
        // =============================================================================

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            try { Frame(); }
            catch (Exception ex)
            {
                Log.Write("Skills", ex.ToString());
                if (_open) Close();
            }
        }

        private void Frame()
        {
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return;
            long now = RealTime.Ms;

            // Which character: a switch (or GTA6Hud's Lucia lending her host's
            // model at a mission) is a different set of skills.
            int model = ped.Model.Hash;
            if (!_loaded || model != _model)
            {
                if (_drivingVeh != null) { RestoreHandling(_drivingVeh); _drivingVeh = null; }
                Save(_ch);
                _loaded = true;
                _model = model;
                _ch = Load(model);
                _hasLastPos = false;
                _settleFrom = -1;
                _pedHealth.Clear();
                SkillsApi.Ready = _ch != null;
                if (_open && _ch != null) Refresh();
            }
            // Someone with no skills of either kind (an animal on peyote, a
            // mission's stand-in): nothing to track until the player is back.
            if (_ch == null)
            {
                if (_open) Close();
                _toggleRequested = _testRequested = false;
                return;
            }

            bool busy = HudBridge.Busy || HudBridge.Paused;
            bool calm = !busy && Game.Player.CanControlCharacter && !ped.IsDead;
            if (!calm) _settleFrom = -1;
            else if (_settleFrom < 0) _settleFrom = now;
            bool settled = _settleFrom >= 0 && now - _settleFrom >= SettleMs;

            // Hits are events; IS_PED_SHOOTING is true for the frame of the shot only.
            if (!_ch.Story && !ped.IsDead)
            {
                if (Function.Call<bool>(Hash.IS_PED_SHOOTING, ped)) _lastShotAt = now;
                if (Function.Call<bool>(Hash.IS_PED_IN_MELEE_COMBAT, ped) || Function.Call<bool>(Hash.IS_PED_PERFORMING_MELEE_ACTION, ped)) _lastMeleeAt = now;
            }

            if (now >= _nextSample)
            {
                _nextSample = now + Math.Max(50, _sampleMs);
                if (!busy) Sample(ped);
            }
            if (now >= _nextHealth)
            {
                _nextHealth = now + 500;
                SyncMaxHealth(ped);
            }
            if (_ch.Story && now >= _nextStatPoll)
            {
                _nextStatPoll = now + Math.Max(100, _statPollMs);
                if (settled) PollStoryStats();
            }
            if (now >= _nextSave)
            {
                _nextSave = now + Math.Max(1000, _saveMs);
                if (_ch.Dirty) Save(_ch);
            }

            if (_testRequested)
            {
                _testRequested = false;
                Award("health", _testXp);
                Log.Write("Skills", Info());
            }

            if (_toggleRequested)
            {
                _toggleRequested = false;
                if (_open) Close();
                else if (!busy && !HudBridge.HudMenuOpen && !Binoculars.IsActive && !CrashCam.IsPlaying) Open();
            }
            if (_open) PanelFrame(busy);
        }

        // =============================================================================
        // Characters and saving (server_skills.lua: here, a file per character)
        // =============================================================================

        private Character Load(int model)
        {
            var ch = new Character();
            if (model == Game.GenerateHash("player_zero")) { ch.Slot = 0; ch.Key = "player_zero"; }
            else if (model == Game.GenerateHash("player_one")) { ch.Slot = 1; ch.Key = "player_one"; }
            else if (model == Game.GenerateHash("player_two")) { ch.Slot = 2; ch.Key = "player_two"; }
            else if (_luciaModels.Contains(model)) ch.Key = _luciaKey;
            // FiveM's system was made for these; a freemode ped played here gets it too.
            else if (model == Game.GenerateHash("mp_f_freemode_01")) ch.Key = "mp_f_freemode_01";
            else if (model == Game.GenerateHash("mp_m_freemode_01")) ch.Key = "mp_m_freemode_01";
            else return null;

            if (ch.Story)
            {
                ch.Seen = new int[SkillDefs.List.Length];
                for (int i = 0; i < ch.Seen.Length; i++) ch.Seen[i] = -1;
            }

            string file = Paths.Save("skills_" + ch.Key + ".txt");
            bool stored = File.Exists(file);
            if (stored)
            {
                try
                {
                    foreach (string line in File.ReadAllLines(file))
                    {
                        if (line.StartsWith("#")) continue;
                        int eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        string id = line.Substring(0, eq).Trim();
                        if (!SkillDefs.ById.ContainsKey(id) || !EarnsXp(ch, SkillDefs.ById[id])) continue;
                        // Skills.normalise: a number, not negative, not NaN.
                        if (double.TryParse(line.Substring(eq + 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v >= 0 && !double.IsNaN(v) && !double.IsInfinity(v))
                            ch.Xp[id] = Math.Floor(v);
                    }
                }
                catch (Exception ex)
                {
                    Log.Write("Skills", "load " + file + ": " + ex.Message);
                    SaveFile.SetAside(file, "Skills");   // the next save mustn't overwrite what couldn't be read
                }
            }

            foreach (SkillDef def in SkillDefs.List)
            {
                if (!EarnsXp(ch, def) || ch.Xp.ContainsKey(def.Id)) continue;
                // server_skills.lua seedFor: a character never seen before starts
                // at Config.Skills.startingLevel, except noSeed skills (Health).
                // Story characters' other skills are the game's, so only Health is here.
                int start = !stored && !def.NoSeed ? Math.Max(0, Math.Min(SkillDefs.MaxLevel, _startingLevel)) : 0;
                ch.Xp[def.Id] = SkillDefs.XpForLevel(start);
            }
            if (!stored)
            {
                ch.Dirty = true;
                Save(ch);
            }

            if (!ch.Story)
                foreach (SkillDef def in SkillDefs.List) ApplyStat(ch, def);
            Log.Write("Skills", "playing " + ch.Key + Environment.NewLine + Info(ch));
            return ch;
        }

        /// <summary>Whether this skill runs on FiveM's XP for this character.</summary>
        private static bool EarnsXp(Character ch, SkillDef def) => !ch.Story || def.SpStat == null && def.Id == "health";

        private void Save(Character ch)
        {
            if (ch == null || !ch.Dirty) return;
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("# Skills XP for " + ch.Key + " (vice_hud's curve: level n costs 100 + 18n).");
                foreach (SkillDef def in SkillDefs.List)
                    if (ch.Xp.TryGetValue(def.Id, out double v))
                        sb.AppendLine(def.Id + "=" + Math.Floor(v).ToString("0", CultureInfo.InvariantCulture));
                SaveFile.Write(Paths.Save("skills_" + ch.Key + ".txt"), sb.ToString());
                ch.Dirty = false;
            }
            catch (Exception ex) { Log.Write("Skills", "save: " + ex.Message); }
        }

        // =============================================================================
        // Stats
        // =============================================================================

        private static int StatGet(string name)
        {
            var o = new OutputArgument();
            Function.Call(Hash.STAT_GET_INT, Game.GenerateHash(name), o, -1);
            return o.GetResult<int>();
        }

        private int Level(Character ch, SkillDef def)
        {
            if (EarnsXp(ch, def)) return SkillDefs.LevelOf((long)Math.Floor(ch.Xp[def.Id]));
            if (def.SpStat == null) return 0;
            return Math.Max(0, Math.Min(SkillDefs.MaxLevel, StatGet("SP" + ch.Slot + "_" + def.SpStat)));
        }

        /// <summary>
        /// client_skills.lua applyStat, for characters on FiveM's XP: the level
        /// written into the stat FiveM writes. Never lowers what is there.
        /// </summary>
        private void ApplyStat(Character ch, SkillDef def)
        {
            if (ch.Story || def.FiveMStat == null || string.IsNullOrEmpty(_luciaPrefix)) return;
            try
            {
                string name = _luciaPrefix + def.FiveMStat;
                int level = Level(ch, def);
                if (StatGet(name) < level) Function.Call(Hash.STAT_SET_INT, Game.GenerateHash(name), level, true);
            }
            catch { }
        }

        /// <summary>
        /// The game raised one of a story character's stats: that is a level-up,
        /// and the open panel shows it.
        /// </summary>
        private void PollStoryStats()
        {
            bool changed = false;
            for (int i = 0; i < SkillDefs.List.Length; i++)
            {
                SkillDef def = SkillDefs.List[i];
                if (def.SpStat == null) continue;
                int v = Math.Max(0, Math.Min(SkillDefs.MaxLevel, StatGet("SP" + _ch.Slot + "_" + def.SpStat)));
                if (_ch.Seen[i] >= 0 && v > _ch.Seen[i])
                {
                    changed = true;
                }
                _ch.Seen[i] = v;
            }
            if (changed && _open) Refresh();
        }

        // =============================================================================
        // Awarding (client_skills.lua award)
        // =============================================================================

        private bool Award(string id, double amount)
        {
            if (_ch == null || !SkillDefs.ById.TryGetValue(id ?? "", out SkillDef def)) return false;
            if (!EarnsXp(_ch, def) || amount <= 0 || double.IsNaN(amount)) return false;

            int before = Level(_ch, def);
            _ch.Xp[id] = _ch.Xp[id] + amount;
            _ch.Dirty = true;
            int after = Level(_ch, def);
            if (after == before) return true;

            ApplyStat(_ch, def);
            if (_open) Refresh();          // pushSkills(false)
            // A level is worth writing down immediately.
            Save(_ch);
            return true;
        }

        private void AwardUnits(string id, double units)
        {
            if (units <= 0) return;
            Award(id, units * SkillDefs.ById[id].Rate);
        }

        // =============================================================================
        // Activity: the sampling loop
        // =============================================================================

        private void Sample(Ped ped)
        {
            if (ped.IsDead)
            {
                _hasLastPos = false;
                return;
            }
            Vector3 pos = ped.Position;
            float step = _hasLastPos ? pos.DistanceTo(_lastPos) : 0f;
            _lastPos = pos;
            _hasLastPos = true;
            // A teleport, a respawn or a lift: only a distance a person could cover.
            if (step > _maxStep) step = 0f;

            float dt = Math.Max(50, _sampleMs) / 1000f;
            Vehicle veh = ped.CurrentVehicle;
            bool inVehicle = veh != null && veh.Exists();

            if (_ch.Story)
            {
                // Everything but Health is the game's own stat for these three.
                if (!inVehicle && Running(ped) && step > 0f) AwardUnits("health", step);
                return;
            }

            if (inVehicle)
            {
                if (veh.Driver == ped)
                {
                    // "metres driven without a crash"
                    if (!Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, veh)) AwardUnits("driving", step);
                    if (veh != _drivingVeh)
                    {
                        if (_drivingVeh != null) RestoreHandling(_drivingVeh);
                        _drivingVeh = veh;
                    }
                    ApplyHandling(veh);
                }
                else if (_drivingVeh != null)
                {
                    RestoreHandling(_drivingVeh);
                    _drivingVeh = null;
                }

                int cls = Function.Call<int>(Hash.GET_VEHICLE_CLASS, veh);
                bool air = Function.Call<bool>(Hash.IS_ENTITY_IN_AIR, veh);
                if ((cls == 15 || cls == 16) && air) AwardUnits("flying", dt);
                if (cls == 8 && veh.Speed > 4f && Function.Call<float>(Hash.GET_ENTITY_PITCH, veh) > 18f && !air) AwardUnits("wheelie", dt);
            }
            else
            {
                if (_drivingVeh != null)
                {
                    RestoreHandling(_drivingVeh);
                    _drivingVeh = null;
                }
                if (Function.Call<bool>(Hash.IS_PED_SWIMMING_UNDER_WATER, ped)) AwardUnits("lung", dt);
                bool sprinting = Function.Call<bool>(Hash.IS_PED_SPRINTING, ped);
                if (Running(ped) && step > 0f)
                {
                    AwardUnits("health", step);
                    AwardUnits("stamina", step * _jogStaminaFactor);
                }
                if (sprinting && step > 0f) AwardUnits("stamina", step);
                // GET_PED_STEALTH_MOVEMENT is a BOOL (FiveM compared it to 1).
                if (Function.Call<bool>(Hash.GET_PED_STEALTH_MOVEMENT, ped) && step > 0f) AwardUnits("stealth", step);
            }

            Hits(ped);
        }

        /// <summary>Running pace, not a sprint: sprinting is the stamina skill's.</summary>
        private static bool Running(Ped ped) =>
            Function.Call<bool>(Hash.IS_PED_RUNNING, ped) && !Function.Call<bool>(Hash.IS_PED_SPRINTING, ped);

        /// <summary>
        /// Melee hits and shots on target. FiveM had the damage event; single
        /// player has no such event, so it watches the peds around for a drop in
        /// health they took from the player while the player was shooting or
        /// fighting, and classifies it, as FiveM does, by what the player holds.
        /// </summary>
        private void Hits(Ped ped)
        {
            long now = RealTime.Ms;
            bool shooting = now - _lastShotAt < 1000, fighting = now - _lastMeleeAt < 1000;
            var seen = new Dictionary<int, int>();
            foreach (Ped other in World.GetNearbyPeds(ped, 60f))
            {
                if (other == null || !other.Exists() || other == ped) continue;
                int h = other.Health;
                seen[other.Handle] = h;
                if (!_pedHealth.TryGetValue(other.Handle, out int was) || h >= was) continue;
                if (!other.HasBeenDamagedBy(ped)) continue;
                // Melee, or bare hands (armed with nothing at all).
                bool melee = Function.Call<bool>(Hash.IS_PED_ARMED, ped, 1) || !Function.Call<bool>(Hash.IS_PED_ARMED, ped, 7);
                // FiveM's IsPedArmed(ped, 2) is explosives only (1 melee, 2
                // explosive, 4 other); guns are 4, so they are counted here too.
                bool armed = Function.Call<bool>(Hash.IS_PED_ARMED, ped, 6);
                if (melee && fighting && !ped.IsInVehicle()) AwardUnits("strength", 1);
                else if (armed && shooting) AwardUnits("shooting", 1);
            }
            _pedHealth = seen;
        }

        // =============================================================================
        // Driving skill -> real handling (FiveM's, for peds the game gives no grip assist)
        // =============================================================================
        // Single player's handling data is shared by every car of a model, so the
        // change shows on any copy of the car being driven while it is driven.
        // It is put back the moment the player gets out.

        private float DrivingFactor()
        {
            float mid = SkillDefs.MaxLevel / 2f;
            return 1f + ((Level(_ch, SkillDefs.ById["driving"]) - mid) / mid) * _drivingGripBonus;
        }

        private void ApplyHandling(Vehicle veh)
        {
            if (!_drivingAffectsHandling) return;
            // Handling is written straight into game memory. On GTA V Enhanced that
            // goes through ScriptHookVDotNet Enhanced's offsets, which are not all
            // right there; leave the car's grip alone rather than risk the game.
            if (Canvas.Enhanced) return;
            try
            {
                HandlingData hd = veh.HandlingData;
                if (hd == null || !hd.IsValid) return;
                int model = veh.Model.Hash;
                if (!_tractionBase.TryGetValue(model, out Traction b))
                {
                    b = new Traction { CurveMax = hd.TractionCurveMax, CurveMin = hd.TractionCurveMin, LossMult = hd.TractionLossMultiplier };
                    _tractionBase[model] = b;
                }
                float f = DrivingFactor();
                _handledModel = model;
                hd.TractionCurveMax = b.CurveMax * f;
                hd.TractionCurveMin = b.CurveMin * f;
                hd.TractionLossMultiplier = b.LossMult / f;
            }
            catch { }
        }

        private int _handledModel;

        private void RestoreHandling(Vehicle veh)
        {
            try
            {
                // Single player shares handling between every car of a model, so it goes back
                // through the model even when that car has been wrecked or despawned.
                int model = veh != null && veh.Exists() ? veh.Model.Hash : _handledModel;
                if (model == 0 || !_tractionBase.TryGetValue(model, out Traction b)) return;
                HandlingData hd = veh != null && veh.Exists() ? veh.HandlingData : HandlingData.GetByVehicleModel(new Model(model));
                if (hd == null || !hd.IsValid) return;
                hd.TractionCurveMax = b.CurveMax;
                hd.TractionCurveMin = b.CurveMin;
                hd.TractionLossMultiplier = b.LossMult;
            }
            catch { }
        }

        // =============================================================================
        // Health skill -> maximum health (client_skills.lua syncMaxHealth)
        // =============================================================================

        private int WantMaxHealth()
        {
            if (_healthMaxBonus <= 0f) return StockMaxHealth;
            int level = Level(_ch, SkillDefs.ById["health"]);
            return StockMaxHealth + (int)Math.Floor(level / (float)SkillDefs.MaxLevel * _healthMaxBonus + 0.5f);
        }

        private void SyncMaxHealth(Ped ped)
        {
            if (_healthMaxBonus <= 0f) return;
            if (ped.Handle != _healthPed)
            {
                _healthPed = ped.Handle;
                _prevRaw = _prevMax = -1;
            }
            if (ped.IsDead) { _prevRaw = _prevMax = -1; return; }

            int want = WantMaxHealth();
            int have = ped.MaxHealth;
            int raw = ped.Health;

            if (have != want)
            {
                // Raised by someone else: GTA6Hud's "Health overpowered" holds a
                // higher max for two minutes and puts it back itself.
                if (have > want && have != _prevMax && Overpowered()) { _prevRaw = raw; _prevMax = have; return; }

                // Full stays full and hurt stays hurt (see client_skills.lua).
                bool reset = _prevMax >= 0 && have != _prevMax && _prevRaw >= 0;
                bool wasFull = reset ? _prevRaw >= _prevMax : raw >= have;
                Function.Call(Hash.SET_PED_MAX_HEALTH, ped, want);
                Function.Call(Hash.SET_ENTITY_MAX_HEALTH, ped, want);
                int target = wasFull ? want : reset ? Math.Min(want, Math.Max(raw, _prevRaw)) : Math.Min(raw, want);
                if (target != raw) ped.Health = target;
                raw = target;
            }
            _prevRaw = raw;
            _prevMax = want;
        }

        /// <summary>GTA6Hud writes its pre-overpower max health to its ini while the boost lasts.</summary>
        private bool Overpowered()
        {
            long now = RealTime.Ms;
            if (now - _overpowerCheckedAt < 2000) return _overpowered;
            _overpowerCheckedAt = now;
            try { _overpowered = ScriptSettings.Load(Path.Combine(Paths.Scripts, "GTA6Hud.ini")).GetValue("Overpower", "HealthBase", 0) > 0; }
            catch { _overpowered = false; }
            return _overpowered;
        }

        // =============================================================================
        // The panel (/skills)
        // =============================================================================

        /// <summary>client_skills.lua skillPayload, for this character.</summary>
        private void Refresh()
        {
            _rows = new List<SkillRow>();
            foreach (SkillDef def in SkillDefs.List)
            {
                if (_ch.Story && def.SpStat == null && def.Id != "health") continue;   // Wheelie: the game's Driving
                var r = new SkillRow { Id = def.Id };
                if (EarnsXp(_ch, def))
                {
                    SkillDefs.Resolve((long)Math.Floor(_ch.Xp[def.Id]), out r.Level, out r.Into, out r.Need, out r.Frac);
                    r.ShowXp = true;
                }
                else
                {
                    // The game's own stat: the level is the stat, and the bar how
                    // far along 0..100 it is, as the game's own stats page draws it.
                    r.Level = Level(_ch, def);
                    r.Frac = r.Level / (float)SkillDefs.MaxLevel;
                }
                r.Max = r.Level >= SkillDefs.MaxLevel;
                if (r.Max) r.Frac = 1f;
                _rows.Add(r);
            }
            _layout = SkillsUi.Measure(_rows);
            _scroll = Math.Min(_scroll, _layout.MaxScroll);
        }

        private void Open()
        {
            if (_ch == null) return;
            _open = true;
            SkillsApi.PanelOpen = true;
            _scroll = 0f;
            Refresh();
            if (!_minimapHeld)
            {
                _minimapHeld = true;
                HudBridge.HoldMinimapHidden(true);
            }
        }

        private void Close()
        {
            _open = false;
            SkillsApi.PanelOpen = false;
            if (_minimapHeld)
            {
                _minimapHeld = false;
                HudBridge.HoldMinimapHidden(false);
            }
        }

        private void PanelFrame(bool busy)
        {
            if (busy || HudBridge.ViPauseOpen) { Close(); return; }

            // NUI focus: the game gets no input while the panel is up.
            Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
            bool closeHover = false;
            if (_cursor && !Glyphs.UsingPad)
            {
                Function.Call(Hash.SET_MOUSE_CURSOR_THIS_FRAME);
                float mx = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, CursorX) * Canvas.W;
                float my = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, CursorY) * Canvas.H;
                closeHover = _layout.Close.Contains(mx, my);
                Function.Call(Hash.SET_MOUSE_CURSOR_STYLE, closeHover ? 5 : 1);   // cursor: pointer on #skills-close
                if (closeHover && Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, CursorAccept)) { Close(); return; }
            }
            // Escape closes it in app.js; Backspace / B too, as every screen here does.
            if (Pressed(FrontendCancel) || Pressed(PauseKey) || Pressed(PauseAlt)) { Close(); return; }

            // The list scrolls when it overflows (a wheel notch is more than the
            // whole overflow, so it rests at one end or the other).
            if (Pressed(ScrollDown) || Pressed(FrontendDown)) _scroll = _layout.MaxScroll;
            if (Pressed(ScrollUp) || Pressed(FrontendUp)) _scroll = 0f;

            SkillsUi.DrawPanel(_rows, _layout, _scroll, closeHover);
        }

        private static bool Pressed(int control) => Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, control);

        // =============================================================================
        // /skillinfo, to the log
        // =============================================================================

        private string Info() => Info(_ch);

        private string Info(Character ch)
        {
            if (ch == null) return "(no character)";
            var sb = new StringBuilder();
            foreach (SkillDef def in SkillDefs.List)
            {
                if (EarnsXp(ch, def))
                {
                    SkillDefs.Resolve((long)Math.Floor(ch.Xp[def.Id]), out int level, out long into, out long need, out _);
                    sb.Append("  ").Append(def.Id.PadRight(9)).Append(" lvl ").Append(level.ToString().PadLeft(3))
                      .Append("  xp ").Append(Math.Floor(ch.Xp[def.Id]).ToString("0", CultureInfo.InvariantCulture))
                      .Append(" (").Append(into).Append('/').Append(need).Append(" to next)");
                    if (def.FiveMStat != null && !ch.Story && !string.IsNullOrEmpty(_luciaPrefix))
                        sb.Append("   ").Append(_luciaPrefix + def.FiveMStat).Append(" = ").Append(StatGet(_luciaPrefix + def.FiveMStat));
                    if (def.Id == "health")
                        sb.Append("   max health ").Append(WantMaxHealthFor(ch, level)).Append(" (live ").Append(Game.Player.Character.MaxHealth).Append(')');
                }
                else if (def.SpStat != null)
                    sb.Append("  ").Append(def.Id.PadRight(9)).Append(" SP").Append(ch.Slot).Append('_').Append(def.SpStat)
                      .Append(" = ").Append(StatGet("SP" + ch.Slot + "_" + def.SpStat));
                else continue;
                sb.AppendLine();
            }
            return sb.ToString().TrimEnd();
        }

        private int WantMaxHealthFor(Character ch, int level) =>
            _healthMaxBonus <= 0f ? StockMaxHealth : StockMaxHealth + (int)Math.Floor(level / (float)SkillDefs.MaxLevel * _healthMaxBonus + 0.5f);
    }
}
