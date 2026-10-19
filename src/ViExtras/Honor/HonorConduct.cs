using System;
using System.Collections.Generic;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// vice_hud/honor/client/conduct.lua for single player: honor reacts to how
    /// the player treats the people and animals around them. It never decides
    /// how much anything is worth; it names what happened (a hook) and hands it
    /// to Honor.ApplyHook, which owns the amounts, cooldowns and caps.
    ///
    ///   * Killing a bystander, a cop or an animal costs honor (classifyKill).
    ///   * A ped that hurt the player within SelfDefenceGraceMs, or that dies
    ///     holding a weapon, is free (the cop is not: kill_cop has no exemptions).
    ///   * Pulling a driver out of a car (IS_PED_JACKING) costs honor.
    ///   * Optionally, aiming at a civilian does (off by default).
    ///
    /// FiveM hears of kills through CEventNetworkEntityDamage. Single player has
    /// no such event, so the ped pool is polled: a ped seen dead for the first
    /// time whose GET_PED_SOURCE_OF_DEATH is the player, or a vehicle the player
    /// is in or just drove, is the player's kill, and it is reported once.
    /// Who hurt the player is read the same way, off HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY.
    /// </summary>
    internal sealed class HonorConduct
    {
        private const int ScanMs = 150;
        // The ped can already be out of the car when its victim is found dead.
        private const int LastVehicleMs = 10000;
        // A ped can be found dead a moment before its source of death is set.
        private const int SourceWaitMs = 1000;
        private static readonly int Unarmed = unchecked((int)0xA2719263);   // WEAPON_UNARMED

        // Config.ConductWatcher's ped type lists (GTA V PED_TYPE_*).
        // Civilians: the player-model types (0-3), CIVMALE, CIVFEMALE, COP, MEDIC,
        // FIREMAN, BUM, PROSTITUTE, SWAT. Gangs and criminals (7-19, 22) are free.
        private static readonly HashSet<int> CivilianTypes = new HashSet<int> { 0, 1, 2, 3, 4, 5, 6, 20, 21, 23, 24, 27 };
        private static readonly HashSet<int> AnimalTypes = new HashSet<int> { 28 };
        private static readonly HashSet<int> CopTypes = new HashSet<int> { 6 };

        // The game's own hunting: the activity and Trevor's hunting with Cletus.
        private static readonly string[] HuntingScriptNames = { "hunting_ambient", "hunting1", "hunting2" };
        private static int[] _huntingScripts;

        private readonly Func<string, int?> _report;
        private readonly Action<string> _trace;

        private readonly int _graceMs;
        private readonly bool _armedFair, _penaliseAiming, _penaliseCarjacking, _penalisePlayerKills;
        private readonly bool _huntingIsLegal;

        /// <summary>[ped] = game time of the last hit the player took from it.</summary>
        private readonly Dictionary<int, int> _aggressors = new Dictionary<int, int>();
        private HashSet<int> _damagedBy = new HashSet<int>(), _damagedNow = new HashSet<int>();
        private float _lastHurt = -1f;
        /// <summary>Whether each living ped was holding a weapon when last seen.</summary>
        private Dictionary<int, bool> _armed = new Dictionary<int, bool>(), _armedNow = new Dictionary<int, bool>();
        /// <summary>Dead peds already dealt with (kept while they are still in the pool).</summary>
        private HashSet<int> _seenDead = new HashSet<int>(), _seenNow = new HashSet<int>();
        private readonly Dictionary<int, int> _deadSince = new Dictionary<int, int>();
        private bool _primed;

        private int _scanAt, _expireAt, _jackAt, _aimAt;
        private int _lastDriven, _lastDrivenAt = int.MinValue / 2;

        public HonorConduct(Func<string, int?> report, Action<string> trace)
        {
            _report = report;
            _trace = trace;
            _graceMs = Config.Get("Honor", "SelfDefenceGraceMs", 25000);
            _armedFair = Config.Get("Honor", "ArmedPedsAreFairGame", true);
            _penaliseAiming = Config.Get("Honor", "PenaliseAiming", false);
            _penaliseCarjacking = Config.Get("Honor", "PenaliseCarjacking", true);
            _penalisePlayerKills = Config.Get("Honor", "PenalisePlayerKills", true);
            _huntingIsLegal = Config.Get("Honor", "HuntingIsLegal", true);
        }

        /// <summary>
        /// One frame. `quiet` (a mission, with SkipInMissions on) keeps watching,
        /// so nothing is reported late once it ends, but reports nothing.
        /// </summary>
        public void Tick(Ped player, int now, bool quiet)
        {
            if (now >= _scanAt)
            {
                _scanAt = now + ScanMs;
                Scan(player, now, quiet);
            }

            // Entity handles are recycled, so a stale entry could excuse a later
            // kill; conduct.lua expires them on a 30 s timer.
            if (now >= _expireAt)
            {
                _expireAt = now + 30000;
                var old = new List<int>();
                foreach (KeyValuePair<int, int> a in _aggressors)
                    if (now - a.Value >= _graceMs) old.Add(a.Key);
                foreach (int h in old) _aggressors.Remove(h);
            }

            // Carjacking: polled every 500 ms; one report per jack, then 8 s off.
            if (_penaliseCarjacking && now >= _jackAt)
            {
                _jackAt = now + 500;
                if (Function.Call<bool>(Hash.IS_PED_JACKING, player))
                {
                    _trace("carjacking detected");
                    if (!quiet) _report("carjack");
                    _jackAt = now + 8000 + 500;
                }
            }

            // Aiming at a civilian (off by default): every 500 ms, 5 s off after a report.
            if (_penaliseAiming && now >= _aimAt)
            {
                _aimAt = now + 500;
                if (AimingAtCivilian(player, now))
                {
                    if (!quiet) _report("aim_at_civilian");
                    _aimAt = now + 5000 + 500;
                }
            }
        }

        private bool IsAggressor(int ped, int now) =>
            _aggressors.TryGetValue(ped, out int hitAt) && now - hitAt < _graceMs;

        // conduct.lua getIsArmed: the selected weapon. A dead ped soon drops it,
        // and here the death is found up to a scan late, so what the ped held
        // when last seen alive counts too.
        private bool IsArmed(Ped ped, int handle)
        {
            if (!_armedFair) return false;
            if (_armed.TryGetValue(handle, out bool was) && was) return true;
            return Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped) != Unarmed;
        }

        private static bool HuntingRunning()
        {
            if (_huntingScripts == null) _huntingScripts = Array.ConvertAll(HuntingScriptNames, Game.GenerateHash);
            foreach (int script in _huntingScripts)
                if (Function.Call<int>(Hash.GET_NUMBER_OF_THREADS_RUNNING_THE_SCRIPT_WITH_THIS_HASH, script) > 0) return true;
            return false;
        }

        /// <summary>classifyKill: which hook a kill is, or null when it costs nothing.</summary>
        private string Classify(Ped ped, int handle, int type, int now)
        {
            if (Function.Call<bool>(Hash.IS_PED_A_PLAYER, ped))
            {
                if (!_penalisePlayerKills) return null;
                if (IsAggressor(handle, now)) return null;
                return "kill_player";
            }

            if (AnimalTypes.Contains(type))
            {
                // FiveM lets an animal a hunting resource spawned as game go free;
                // here, an animal taken while the game's own hunting is running.
                if (_huntingIsLegal && HuntingRunning()) return null;
                // A cougar mid-lunge is not an innocent animal.
                return IsAggressor(handle, now) ? null : "kill_animal";
            }

            // Before the civilian exemptions on purpose: a cop shooting back is
            // doing the job, not an innocent victim.
            if (CopTypes.Contains(type)) return "kill_cop";

            if (!CivilianTypes.Contains(type)) return null;   // gangs, criminals: free
            if (IsAggressor(handle, now) || IsArmed(ped, handle)) return null;
            return "kill_civilian";
        }

        // getIsOurs: the player, a vehicle the player is in, or one the player
        // drove a moment ago (a car runs someone over; the vehicle is the killer).
        private bool IsOurs(int source, Ped player, Vehicle current, int now)
        {
            if (source == 0) return false;
            if (source == player.Handle) return true;
            if (!Function.Call<bool>(Hash.IS_ENTITY_A_VEHICLE, source)) return false;
            if (current != null && source == current.Handle) return true;
            return source == _lastDriven && now - _lastDrivenAt < LastVehicleMs;
        }

        private void Scan(Ped player, int now, bool quiet)
        {
            int self = player.Handle;
            Vehicle current = player.CurrentVehicle;
            if (current != null && current.Exists() && current.Driver == player)
            {
                _lastDriven = current.Handle;
                _lastDrivenAt = now;
            }

            // Who hurt the player. The flag the game keeps is not cleared here
            // (GTA6Hud reads the player's own for its ambient "hero" fights), so
            // a ped counts as having just hit the player when its flag turns on,
            // or when it is still on and the player has just lost health.
            float hp = player.HealthFloat + player.ArmorFloat;
            bool hurt = _lastHurt >= 0f && hp < _lastHurt - 0.01f;
            _lastHurt = hp;
            bool anyPed = Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ANY_PED, player);

            _damagedNow.Clear();
            _armedNow.Clear();
            _seenNow.Clear();

            foreach (Ped ped in World.GetAllPeds())
            {
                if (ped == null) continue;
                int h = ped.Handle;
                if (h == self) continue;

                if (anyPed && Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, player, ped, true))
                {
                    _damagedNow.Add(h);
                    if (!_damagedBy.Contains(h) || hurt) _aggressors[h] = now;
                }

                bool dead = Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, ped, true) || Function.Call<bool>(Hash.IS_ENTITY_DEAD, ped);
                if (!dead)
                {
                    _deadSince.Remove(h);
                    if (_armedFair) _armedNow[h] = Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped) != Unarmed;
                    continue;
                }

                if (_seenDead.Contains(h) || !_primed)
                {
                    // Already dealt with, or dead before Honor started watching.
                    _seenNow.Add(h);
                    continue;
                }

                int source = Function.Call<int>(Hash.GET_PED_SOURCE_OF_DEATH, ped);
                if (source == 0)
                {
                    if (!_deadSince.TryGetValue(h, out int since)) _deadSince[h] = now;
                    else if (now - since > SourceWaitMs) { _deadSince.Remove(h); _seenNow.Add(h); }
                    continue;
                }
                _deadSince.Remove(h);
                _seenNow.Add(h);

                if (IsOurs(source, player, current, now))
                {
                    int type = Function.Call<int>(Hash.GET_PED_TYPE, ped);
                    string hook = Classify(ped, h, type, now);
                    _trace("fatal hit by us on ped " + h + " (type " + type + ") -> " + (hook ?? "no honor cost"));
                    if (hook != null && !quiet) _report(hook);
                }
                _aggressors.Remove(h);
            }

            // Only what is still in the pool is kept.
            Swap(ref _damagedBy, ref _damagedNow);
            Swap(ref _armed, ref _armedNow);
            Swap(ref _seenDead, ref _seenNow);
            _primed = true;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }

        private bool AimingAtCivilian(Ped player, int now)
        {
            if (!Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player)) return false;
            var target = new OutputArgument();
            if (!Function.Call<bool>(Hash.GET_ENTITY_PLAYER_IS_FREE_AIMING_AT, Game.Player, target)) return false;
            int e = target.GetResult<int>();
            if (e == 0 || e == player.Handle || !Function.Call<bool>(Hash.IS_ENTITY_A_PED, e)) return false;
            if (Function.Call<bool>(Hash.IS_PED_A_PLAYER, e)) return false;
            if (Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, e, true)) return false;
            if (!CivilianTypes.Contains(Function.Call<int>(Hash.GET_PED_TYPE, e))) return false;
            return !IsAggressor(e, now);
        }
    }
}
