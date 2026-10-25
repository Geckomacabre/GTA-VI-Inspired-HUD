using System;
using System.Collections.Generic;
using GTA;
using GTA.Native;
using ViExtras.NpcMemory;

namespace ViExtras
{
    /// <summary>
    /// People know a bad name when they see one, single player's own: while the
    /// player's rank is Psycho or Shattered (Honor.RankOf), people on foot nearby
    /// who can see them now and then back off with a line (shocked, scared, a
    /// curse), and some run. Shattered is noticed more often than Psycho.
    ///
    /// Only Memory's "people" are touched (UM.IsPerson: no cops, no one a script
    /// owns, no clerks), and only one at a time, each at most once every
    /// ReactAgainMs, so a street does not empty all at once. Nothing happens on
    /// a mission, in a car, or while wanted (people run from that already).
    /// </summary>
    internal sealed class HonorCrowd
    {
        private const float Range = 12f;
        private const int ScanMs = 1500;
        private const int ReactAgainMs = 120000;

        private static readonly string[] Scared = { "GENERIC_FRIGHTENED_HIGH", "GENERIC_FRIGHTENED_MED", "GENERIC_SHOCKED_HIGH" };
        private static readonly string[] Wary = { "GENERIC_SHOCKED_MED", "GENERIC_CURSE_MED", "GENERIC_INSULT_MED" };

        private readonly Action<string> _trace;
        private readonly int _psychoGapMs, _shatteredGapMs;
        private readonly Random _rng = new Random();
        private readonly Dictionary<int, int> _reactedAt = new Dictionary<int, int>();
        private int _scanAt, _nextAt, _expireAt;

        public HonorCrowd(Action<string> trace)
        {
            _trace = trace;
            _psychoGapMs = Config.Get("Honor", "ReactEveryMsPsycho", 9000);
            _shatteredGapMs = Config.Get("Honor", "ReactEveryMsShattered", 4500);
        }

        public void Tick(Ped player, int now, string rank, bool quiet)
        {
            if (now >= _expireAt)
            {
                _expireAt = now + 30000;
                var old = new List<int>();
                foreach (KeyValuePair<int, int> r in _reactedAt) if (now - r.Value >= ReactAgainMs) old.Add(r.Key);
                foreach (int h in old) _reactedAt.Remove(h);
            }

            bool shattered = rank == Honor.Shattered;
            if (!shattered && rank != Honor.Psycho) return;
            if (quiet || now < _nextAt || now < _scanAt) return;
            _scanAt = now + ScanMs;
            if (Game.IsMissionActive || Game.Player.WantedLevel > 0 || player.IsDead) return;
            if (Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, player, false)) return;

            List<UM.Near> near = UM.NearbyPeople(player.Position, Range, (ped, d) =>
                !_reactedAt.ContainsKey(ped.Handle) && !UM.IsCop(ped) && !UM.InVehicle(ped)
                && !UM.Fleeing(ped) && !Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped, 0)
                && !Witness.IsWitness(ped) && UM.CanSee(ped, player));
            if (near.Count == 0) return;

            Ped who = near[0].Ped;
            _reactedAt[who.Handle] = now;
            _nextAt = now + (shattered ? _shatteredGapMs : _psychoGapMs);

            // Shattered: most run. Psycho: some do, the rest keep their distance and say so.
            bool run = _rng.Next(100) < (shattered ? 65 : 35);
            string[] lines = run ? Scared : Wary;
            Function.Call(Hash.PLAY_PED_AMBIENT_SPEECH_NATIVE, who, lines[_rng.Next(lines.Length)], "SPEECH_PARAMS_FORCE");
            if (run) Function.Call(Hash.TASK_SMART_FLEE_PED, who, player, 40f, 10000, false, false);
            else
            {
                Function.Call(Hash.TASK_LOOK_AT_ENTITY, who, player, 3000, 2048, 3);
                Function.Call(Hash.TASK_TURN_PED_TO_FACE_ENTITY, who, player, 1500);
            }
            _trace("ped " + who.Handle + (run ? " ran" : " backed off") + " (" + rank + ")");
        }
    }
}
