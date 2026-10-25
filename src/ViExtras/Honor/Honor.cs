using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

namespace ViExtras
{
    /// <summary>
    /// The FiveM version's honor (vice_hud/honor, formerly qbx_honor), for
    /// single player.
    ///
    /// Every character has an honor value, -100 to 100, saved between sessions.
    /// Deeds move it through the named hooks of Config.Hooks (HonorHooks.cs),
    /// each with its own amount, cooldown and session cap; robberies escalate
    /// when the police are already on you or you killed someone a few minutes
    /// before. Hitting the floor breaks honor for good.
    ///
    /// What fires hooks here: the conduct watcher (kills, carjacking, aiming;
    /// HonorConduct.cs), the wanted watcher (each new star), and anything else
    /// through HonorApi.Report (um_memory's Greet / Antagonize prompt).
    ///
    /// What shows: every deed pops a sign and a face in the middle of the
    /// screen; the corner panel (mugshot and badge) only when the standing
    /// changes tier, or honor has just broken (HonorHud.cs).
    ///
    /// Sources: honor/server/main.lua (ApplyHook, AdjustHonor, the wanted hook),
    /// honor/client/main.lua (honorUpdated, the wanted watcher),
    /// honor/client/conduct.lua and client.lua's ShowHonorToast / ShowHonorDeed.
    ///
    /// Single player's own, on top of the FiveM version: a bad deed a cop sees
    /// costs WitnessedExtra more; losing the police with no innocent or cop
    /// killed pays (clean_escape), and so does LayLowMinutes with no stars and
    /// no bad deed (lay_low); killing someone attacking a bystander pays
    /// (HonorConduct); dogs can be petted (HonorDogs); and five named ranks
    /// (not drawn), people backing off from a Psycho or Shattered player
    /// (HonorCrowd).
    /// </summary>
    public sealed class Honor : Script
    {
        private const string Section = "Honor";

        /// <summary>The ranks, best to worst (RankOf).</summary>
        public const string Professional = "PROFESSIONAL", Aggressive = "AGGRESSIVE", Violent = "VIOLENT",
            Psycho = "PSYCHO", Shattered = "SHATTERED";

        // The bad deeds that cost WitnessedExtra more when a cop sees them.
        private static readonly HashSet<string> Witnessable = new HashSet<string>(StringComparer.Ordinal)
        {
            "kill_civilian", "kill_cop", "kill_player", "kill_animal", "carjack", "aim_at_civilian", "antagonize_npc",
        };
        // The police see a crime this far off when facing it; anywhere round them this close.
        private const float WitnessNearby = 8f;
        // A clean escape is paid this long after the stars go, if nothing ended it badly.
        private const int EscapeSettleMs = 3000;

        private readonly int _witnessExtra;
        private readonly float _witnessRange;
        private readonly bool _cleanEscape;
        private readonly int _minChaseMs, _layLowMs;
        private readonly int _rankProfessional, _rankAggressive, _rankPsycho;
        private readonly HonorCrowd _crowd;
        private readonly HonorDogs _dogs;
        private int _chaseFrom, _escapeAt, _layLowFrom;
        private bool _chaseKilled;
        private string _rank;

        private readonly bool _enabled;
        private readonly int _min, _max, _default, _angelAt, _devilAt;
        private readonly bool _whenWanted, _whenRecentKill;
        private readonly int _recentKillWindowMs;
        private readonly bool _notify, _debug, _skipInMissions;
        private readonly Keys _testKey;
        private readonly string _testHook;
        private readonly bool _testBroken;

        private readonly HonorHud _hud;
        private readonly HonorConduct _conduct;

        // The layout editor ([Honor] EditKey): vice_hud's /movehud, for the two
        // honor elements only. Polled keys, not KeyDown: it runs inside the tick.
        private readonly Keys _editKey;
        private bool _editKeyWas, _editing;

        /// <summary>The layout editor is open (its arrow keys, Tab, Enter and Backspace are taken).</summary>
        public static bool LayoutEditorOpen { get; private set; }
        private float[] _editWas;
        private readonly Dictionary<Keys, long> _keyNext = new Dictionary<Keys, long>();

        // server/main.lua's per-player memory, for this session
        private sealed class HookState
        {
            public int? Last;
            public int Moved;
        }
        private readonly Dictionary<string, HookState> _hookState = new Dictionary<string, HookState>(StringComparer.Ordinal);
        private int? _lastViolence;

        private int _model;
        private string _key;
        private bool _clientBroken;       // client/main.lua's currentHonorBroken
        private int _lastWanted;
        private int _wantedAt;
        private bool _testRequested;

        public Honor()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get(Section, "Enabled", true);
            _min = Config.Get(Section, "MinHonor", -100);
            _max = Config.Get(Section, "MaxHonor", 100);
            _default = Config.Get(Section, "DefaultHonor", 0);
            _angelAt = Config.Get(Section, "AngelThreshold", 40);
            _devilAt = Config.Get(Section, "DevilThreshold", -40);
            _whenWanted = Config.Get(Section, "EscalateWhenWanted", true);
            _whenRecentKill = Config.Get(Section, "EscalateWhenRecentKill", true);
            _recentKillWindowMs = Config.Get(Section, "RecentKillWindowMs", 180000);
            _notify = Config.Get(Section, "NotifyOnChange", false);
            _debug = Config.Get(Section, "Debug", true);
            _skipInMissions = Config.Get(Section, "SkipInMissions", false);
            _testKey = Config.Get(Section, "TestKey", Keys.None);
            _testHook = Config.Get(Section, "TestHook", "kill_civilian");
            _testBroken = Config.Get(Section, "TestBroken", false);
            _witnessExtra = Config.Get(Section, "WitnessedExtra", 3);
            _witnessRange = Config.Get(Section, "WitnessRange", 60f);
            _cleanEscape = Config.Get(Section, "CleanEscape", true);
            _minChaseMs = Config.Get(Section, "CleanEscapeMinSeconds", 20) * 1000;
            _layLowMs = Config.Get(Section, "LayLowMinutes", 5) * 60000;
            _rankProfessional = Config.Get(Section, "RankProfessional", _angelAt);
            _rankAggressive = Config.Get(Section, "RankAggressive", 0);
            _rankPsycho = Config.Get(Section, "RankPsycho", _devilAt);
            if (!_enabled) return;

            _editKey = Config.Get(Section, "EditKey", Keys.None);
            _hud = new HonorHud(Config.Get(Section, "HoldMs", 6000), TierOf)
            {
                PopOffsetX = Config.Get(Section, "PopOffsetX", 912f),
                PopOffsetY = Config.Get(Section, "PopOffsetY", 18f),
                PanelOffsetX = Config.Get(Section, "PanelOffsetX", 0f),
                PanelOffsetY = Config.Get(Section, "PanelOffsetY", 0f),
            };
            if (Config.Get(Section, "ConductWatcher", true))
            {
                _conduct = new HonorConduct(ApplyHook, Trace);
                // Any innocent or cop killed while the stars are up spoils the escape.
                _conduct.Killed = hook => { if (Game.Player.WantedLevel > 0 || _escapeAt > 0) _chaseKilled = true; };
            }
            if (Config.Get(Section, "NpcReactions", true)) _crowd = new HonorCrowd(Trace);
            if (Config.Get(Section, "PetDogs", true)) _dogs = new HonorDogs(ApplyHook, Trace);

            HonorApi.TierFor = TierOf;
            HonorApi.Handler = hook => HonorApi.Ready && ApplyHook(hook) != null;

            Tick += OnTick;
            KeyDown += (s, e) => { if (_testKey != Keys.None && e.KeyCode == _testKey) _testRequested = true; };
            Aborted += (s, e) =>
            {
                if (_editing) StopEditing(save: false);
                _dogs?.Shutdown();
                HonorApi.Handler = null;
                HonorApi.Ready = false;
                _hud.Shutdown();
                SaveFile.Flush();   // a save still waiting is written before the scripts go
            };
        }

        private void Trace(string line)
        {
            if (_debug) Log.Write("Honor", line);
        }

        /// <summary>getBadgeTier: "angel", "devil", or null in between.</summary>
        private string TierOf(int value)
        {
            if (value >= _angelAt) return "angel";
            if (value <= _devilAt) return "devil";
            return null;
        }

        private int Clamp(int value) => value < _min ? _min : value > _max ? _max : value;

        /// <summary>The named rank for a standing: broken is Shattered whatever the number.</summary>
        private string RankOf(int value, bool broken)
        {
            if (broken) return Shattered;
            if (value >= _rankProfessional) return Professional;
            if (value >= _rankAggressive) return Aggressive;
            if (value > _rankPsycho) return Violent;
            return Psycho;
        }

        private void UpdateRank(int value, bool broken, bool announce)
        {
            string rank = RankOf(value, broken);
            if (announce && _rank != null && rank != _rank) Trace("rank " + _rank + " -> " + rank);
            _rank = rank;
            HonorApi.Rank = rank;
        }

        /// <summary>A cop who can see the player: facing them within WitnessRange, or anywhere round them close by.</summary>
        private bool CopsSaw(Ped player)
        {
            Vector3 at = player.Position;
            foreach (Ped ped in World.GetNearbyPeds(player, _witnessRange))
            {
                if (ped == null || !ped.Exists() || ped.IsDead) continue;
                int type = Function.Call<int>(Hash.GET_PED_TYPE, ped);
                if (type != 6 && type != 27) continue;   // COP, SWAT
                bool sees = ped.Position.DistanceTo(at) <= WitnessNearby
                    ? Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, ped, player, 17)
                    : Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY_IN_FRONT, ped, player);
                if (sees) return true;
            }
            return false;
        }

        // ---- server/main.lua -----------------------------------------------------

        /// <summary>
        /// ApplyHook: applies a named hook, honouring its cooldown and session cap,
        /// and swapping to its escalated values when Config.Escalation matches.
        /// Returns honor after the change, or null when nothing was applied.
        /// </summary>
        internal int? ApplyHook(string hookName)
        {
            if (hookName == null || !HonorHooks.All.TryGetValue(hookName, out HonorHook hook))
            {
                Log.Write("Honor", "unknown hook \"" + hookName + "\" - check HonorHooks.cs");
                return null;
            }

            if (!_hookState.TryGetValue(hookName, out HookState entry))
            {
                entry = new HookState();
                _hookState[hookName] = entry;
            }

            int now = Game.GameTime;
            if (hook.CooldownMs > 0 && entry.Last.HasValue && now - entry.Last.Value < hook.CooldownMs)
            {
                Trace("hook \"" + hookName + "\" ON COOLDOWN (" + (hook.CooldownMs - (now - entry.Last.Value)) + "ms left)");
                return null;
            }

            // Escalation swaps the amount, the reason and the severity, but not
            // the throttles: those stay keyed to the base hook.
            int delta = hook.Delta;
            string label = hook.Label, severity = hook.Severity;
            if (ShouldEscalate(hook, now, out string why))
            {
                delta = hook.Escalate.Delta ?? delta;
                label = hook.Escalate.Label ?? label;
                severity = hook.Escalate.Severity ?? "terrible";
                Trace("hook \"" + hookName + "\" ESCALATED (" + why + ") -> " + delta + " / " + severity);
            }

            if (_witnessExtra > 0 && delta < 0 && Witnessable.Contains(hookName) && CopsSaw(Game.Player.Character))
            {
                delta -= _witnessExtra;
                label += ", in front of the police";
                Trace("hook \"" + hookName + "\" WITNESSED by the police -> " + delta);
            }

            if (hook.SessionCap > 0)
            {
                int remaining = hook.SessionCap - entry.Moved;
                if (remaining <= 0)
                {
                    Trace("hook \"" + hookName + "\" HIT ITS SESSION CAP");
                    return null;
                }
                // The last application is clamped so the cap lands exactly.
                if (Math.Abs(delta) > remaining) delta = delta > 0 ? remaining : -remaining;
            }

            int? newValue = AdjustHonor(delta, label, severity);
            if (newValue == null) return null;

            entry.Last = now;
            entry.Moved += Math.Abs(delta);
            if (HonorHooks.Violent.Contains(hookName)) _lastViolence = now;

            // server/main.lua 263: TriggerEvent('qbx_honor:server:hookApplied', src, hookName)
            try { HonorApi.RaiseHookApplied(hookName); }
            catch (Exception ex) { Log.Write("Honor", "a HookApplied handler failed: " + ex.Message); }
            return newValue;
        }

        private bool ShouldEscalate(HonorHook hook, int now, out string why)
        {
            why = null;
            if (hook.Escalate == null) return false;
            if (_whenWanted)
            {
                int wanted = Game.Player.WantedLevel;
                if (wanted > 0)
                {
                    why = "wanted level " + wanted;
                    return true;
                }
            }
            if (_whenRecentKill && _lastViolence.HasValue && now - _lastViolence.Value < _recentKillWindowMs)
            {
                why = "killed someone recently";
                return true;
            }
            return false;
        }

        /// <summary>
        /// AdjustHonor: moves honor by `delta`, clamps it, saves it, latches the
        /// unrepairable floor, and tells the HUD. The HUD hears of every applied
        /// hook, even one the clamp left at the same value.
        /// </summary>
        private int? AdjustHonor(int delta, string reason, string severity)
        {
            if (delta == 0) return null;
            if (severity == null) severity = delta > 0 ? "good" : "bad";
            if (delta < 0) _layLowFrom = Game.GameTime;   // a bad deed starts laying low over

            int current = HonorApi.Value;
            int newValue = Clamp(current + delta);
            HonorApi.Value = newValue;

            // The unrepairable floor: latches once, never unset.
            bool wasBroken = HonorApi.Broken;
            bool nowBroken = wasBroken;
            if (!wasBroken && newValue <= _min)
            {
                nowBroken = true;
                HonorApi.Broken = true;
                Trace("honor hit the floor (" + _min + ") - permanently broken from here");
            }
            HonorStore.Save(_key, newValue, nowBroken);
            Trace("AdjustHonor " + current + " -> " + newValue + " (" + reason + ")");

            HonorUpdated(newValue, current, reason, nowBroken, severity);

            try { HonorApi.RaiseChanged(newValue, current, reason, severity); }
            catch (Exception ex) { Log.Write("Honor", "a Changed handler failed: " + ex.Message); }
            return newValue;
        }

        // ---- client/main.lua -----------------------------------------------------

        /// <summary>
        /// qbx_honor:client:honorUpdated. The centre pop answers "did that
        /// count?", true of every hook. The corner panel answers "what am I
        /// now?", so it only shows when the tier changes or honor has just broken.
        /// </summary>
        private void HonorUpdated(int newHonor, int previousHonor, string reason, bool broken, string severity)
        {
            bool wasBroken = _clientBroken;
            if (broken) _clientBroken = true;

            int delta = newHonor - previousHonor;
            string previousTier = TierOf(previousHonor), newTier = TierOf(newHonor);
            bool justBroke = broken && !wasBroken;

            if (previousTier != newTier || justBroke)
            {
                Trace("standing changed (" + (previousTier ?? "nil") + " -> " + (newTier ?? "nil") + (justBroke ? ", JUST BROKE" : "") + ") - drawing the panel");
                _hud.ShowToast(Game.Player.Character, newHonor, _clientBroken, severity);
            }
            else
            {
                Trace("standing unchanged (" + (newTier ?? "nil") + ") - centre indicator only");
                _hud.ShowDeed(delta, severity, _clientBroken);
            }

            if (_notify) NotifyChange(newHonor, reason, delta);
            UpdateRank(newHonor, _clientBroken, announce: true);
        }

        /// <summary>Config.NotifyOnChange: the game's own feed ticker, as NativeNotify posts it.</summary>
        private static void NotifyChange(int honor, string reason, int delta)
        {
            bool loss = delta < 0;
            Notification.Show((loss ? "~r~" : "~g~") + (loss ? "" : "+") + delta + " HONOR~s~  " + (reason ?? "Honor changed") + " (now " + honor + ")");
        }

        // ---- the character -------------------------------------------------------

        /// <summary>
        /// Loads the standing of whoever is being played, the first time and on
        /// every switch: syncHonor and SetHonorStanding, which draw nothing.
        /// </summary>
        private void TrackCharacter(Ped player)
        {
            int model = player.Model.Hash;
            if (HonorApi.Ready && model == _model) return;

            _model = model;
            _key = HonorStore.KeyFor(model);
            HonorStore.Load(_key, _default, out int value, out bool broken);
            value = Clamp(value);
            HonorApi.Value = value;
            HonorApi.Broken = broken;
            _clientBroken = broken;
            _hud.Seed(value, broken, character: true);
            // A new character starts from their own stars: the last one's chase, or its
            // end, is not theirs (switching away from a chase is no clean escape).
            int now = Game.GameTime;
            _lastWanted = Game.Player.WantedLevel;
            _chaseFrom = now;
            _chaseKilled = false;
            _escapeAt = 0;
            _layLowFrom = now;
            _rank = null;
            UpdateRank(value, broken, announce: false);
            HonorApi.Ready = true;
            Trace("standing for " + _key + ": " + value + (broken ? " (broken)" : ""));
        }

        // ---- every frame ---------------------------------------------------------

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            try
            {
                if (Game.IsLoading) return;
                Ped player = Game.Player.Character;
                if (player == null || !player.Exists()) return;
                TrackCharacter(player);

                int now = Game.GameTime;
                bool quiet = _skipInMissions && Game.IsMissionActive;

                // The wanted watcher: every second; only a rise costs honor.
                if (now >= _wantedAt)
                {
                    _wantedAt = now + 1000;
                    int level = Game.Player.WantedLevel;
                    if (level != _lastWanted)
                    {
                        Trace("wanted level " + _lastWanted + " -> " + level);
                        if (level > _lastWanted && level >= 1 && !quiet) ApplyHook("wanted_level");
                        if (_lastWanted == 0 && level > 0)
                        {
                            _chaseFrom = now;
                            _chaseKilled = false;
                            _escapeAt = 0;
                        }
                        else if (_lastWanted > 0 && level == 0 && _cleanEscape)
                        {
                            if (_chaseKilled) Trace("the police lost you, but someone innocent or a cop died: no clean escape");
                            else if (now - _chaseFrom < _minChaseMs) Trace("the police lost you after " + (now - _chaseFrom) / 1000 + " s: too short for a clean escape");
                            else _escapeAt = now + EscapeSettleMs;
                        }
                        _lastWanted = level;
                    }
                }

                // The clean escape is paid once the stars have been gone a moment and
                // nothing else ended the chase: a death, an arrest, a respawn's fade.
                if (_escapeAt > 0)
                {
                    if (player.IsDead || Game.Player.WantedLevel > 0
                        || Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, true)
                        || Function.Call<bool>(Hash.IS_PED_CUFFED, player)
                        || Function.Call<bool>(Hash.IS_SCREEN_FADED_OUT) || Function.Call<bool>(Hash.IS_SCREEN_FADING_OUT))
                    {
                        Trace("the chase ended in an arrest, a death or new stars: no clean escape");
                        _escapeAt = 0;
                    }
                    else if (now >= _escapeAt)
                    {
                        _escapeAt = 0;
                        if (_chaseKilled) Trace("someone innocent or a cop died as the police left: no clean escape");
                        else if (!quiet) ApplyHook("clean_escape");
                    }
                }

                // Laying low: LayLowMinutes with no stars and no bad deed. Nothing to
                // earn at the top, and the session cap stops it after a few.
                if (_layLowMs > 0)
                {
                    if (Game.Player.WantedLevel > 0 || player.IsDead) _layLowFrom = now;
                    else if (now - _layLowFrom >= _layLowMs)
                    {
                        _layLowFrom = now;
                        if (!quiet && HonorApi.Value < _max) ApplyHook("lay_low");
                    }
                }

                _conduct?.Tick(player, now, quiet);
                _crowd?.Tick(player, now, _rank, quiet);

                if (_testRequested)
                {
                    _testRequested = false;
                    Preview(player);
                }

                bool editDown = _editKey != Keys.None && Game.IsKeyPressed(_editKey);
                if (editDown && !_editKeyWas)
                {
                    if (_editing) StopEditing(save: true);
                    else if (!HudBridge.Paused && !HudBridge.Busy) StartEditing(player);
                }
                _editKeyWas = editDown;
                if (_editing) Edit();

                // vice_hud's whole page is hidden for death, the crash cam and the
                // binoculars; the extras also stand aside for menus and cutscenes.
                bool hidden = !_editing && (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen
                    || player.IsDead || CrashCam.IsPlaying || Binoculars.IsActive);
                _dogs?.Tick(player, now, hidden || _editing);
                _hud.Tick(hidden);
            }
            catch (Exception ex)
            {
                Log.Write("Honor", ex.ToString());
            }
        }

        // ---- the layout editor ------------------------------------------------------

        private void StartEditing(Ped player)
        {
            _editing = true;
            LayoutEditorOpen = true;
            _editWas = new[] { _hud.PopOffsetX, _hud.PopOffsetY, _hud.PanelOffsetX, _hud.PanelOffsetY };
            _hud.Editing = true;
            _hud.EditingPanel = false;
            _keyNext.Clear();
            foreach (Keys k in new[] { Keys.Return, Keys.Back, Keys.Tab, Keys.R }) _keyNext[k] = long.MaxValue;  // wait for release
            // A fresh mugshot for the panel, as a preview: honor is not changed.
            _hud.ShowToast(player, HonorApi.Value, HonorApi.Broken, "bad", preview: true);
        }

        private void StopEditing(bool save)
        {
            _editing = false;
            LayoutEditorOpen = false;
            if (save)
            {
                Config.SetLine(Section, "PopOffsetX", F(_hud.PopOffsetX));
                Config.SetLine(Section, "PopOffsetY", F(_hud.PopOffsetY));
                Config.SetLine(Section, "PanelOffsetX", F(_hud.PanelOffsetX));
                Config.SetLine(Section, "PanelOffsetY", F(_hud.PanelOffsetY));
                Trace("layout saved: popup " + F(_hud.PopOffsetX) + ", " + F(_hud.PopOffsetY) + "; panel " + F(_hud.PanelOffsetX) + ", " + F(_hud.PanelOffsetY));
            }
            else if (_editWas != null)
            {
                _hud.PopOffsetX = _editWas[0]; _hud.PopOffsetY = _editWas[1];
                _hud.PanelOffsetX = _editWas[2]; _hud.PanelOffsetY = _editWas[3];
            }
            _hud.EndEditing();
        }

        private static string F(float v) => v.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);

        // A key press, repeating while held: once, then after 300 ms every 40 ms.
        private bool Pressed(Keys key)
        {
            long now = RealTime.Ms;
            if (!Game.IsKeyPressed(key)) { _keyNext.Remove(key); return false; }
            if (!_keyNext.TryGetValue(key, out long next)) { _keyNext[key] = now + 300; return true; }
            if (now < next) return false;
            _keyNext[key] = now + 40;
            return true;
        }

        private void Edit()
        {
            // The game keeps its hands off the keys while the editor has them.
            Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
            Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 2);

            float step = Game.IsKeyPressed(Keys.ShiftKey) ? 10f : 1f;
            float dx = 0f, dy = 0f;
            if (Pressed(Keys.Left)) dx -= step;
            if (Pressed(Keys.Right)) dx += step;
            if (Pressed(Keys.Up)) dy -= step;
            if (Pressed(Keys.Down)) dy += step;
            if (_hud.EditingPanel) { _hud.PanelOffsetX += dx; _hud.PanelOffsetY += dy; }
            else { _hud.PopOffsetX += dx; _hud.PopOffsetY += dy; }

            if (Pressed(Keys.Tab)) _hud.EditingPanel = !_hud.EditingPanel;
            if (Pressed(Keys.R))
            {
                if (_hud.EditingPanel) _hud.PanelOffsetX = _hud.PanelOffsetY = 0f;
                else _hud.PopOffsetX = _hud.PopOffsetY = 0f;
            }
            if (Pressed(Keys.Return)) { StopEditing(save: true); return; }
            if (Pressed(Keys.Back)) { StopEditing(save: false); return; }

            // What the keys do, along the top.
            Function.Call(Hash.SET_TEXT_FONT, 4);
            Function.Call(Hash.SET_TEXT_SCALE, 0.42f, 0.42f);
            Function.Call(Hash.SET_TEXT_COLOUR, 255, 255, 255, 235);
            Function.Call(Hash.SET_TEXT_CENTRE, true);
            Function.Call(Hash.SET_TEXT_OUTLINE);
            Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_TEXT, "STRING");
            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME,
                "HONOR LAYOUT - moving the " + (_hud.EditingPanel ? "PANEL" : "POPUP") +
                "~n~Arrows move (Shift: faster)   Tab: popup / panel   R: reset   Enter: save   Backspace: cancel");
            Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_TEXT, 0.5f, 0.04f, 0);
        }

        /// <summary>
        /// TestKey: shows what TestHook would do from here, the pop and the panel
        /// both (as /hudtest does), WITHOUT applying it: honor is not changed or
        /// saved, cooldowns are not touched. A real hook can break honor for good.
        /// </summary>
        private void Preview(Ped player)
        {
            if (!HonorHooks.All.TryGetValue(_testHook ?? "", out HonorHook hook))
            {
                Log.Write("Honor", "TestHook \"" + _testHook + "\" is not a hook");
                return;
            }
            int delta = hook.Delta;
            string severity = hook.Severity;
            if (ShouldEscalate(hook, Game.GameTime, out _))
            {
                delta = hook.Escalate.Delta ?? delta;
                severity = hook.Escalate.Severity ?? "terrible";
            }
            int value = Clamp(HonorApi.Value + delta);
            bool broken = HonorApi.Broken || _testBroken || value <= _min;
            Trace("test: " + _testHook + " would take honor " + HonorApi.Value + " -> " + value + (broken ? " (broken)" : ""));
            _hud.ShowToast(player, value, broken, severity ?? (delta > 0 ? "good" : "bad"), preview: true);
        }
    }
}
