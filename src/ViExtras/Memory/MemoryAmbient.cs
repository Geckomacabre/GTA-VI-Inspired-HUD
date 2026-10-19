using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras.NpcMemory
{
    /// <summary>
    /// um_memory/client/ambient.lua: things people do without being spoken to,
    /// with RDR2's own thresholds (beat_town_confrontation).
    ///
    ///   * Follow someone and they notice: they turn and tell you off, then walk
    ///     away fast; keep following and the brave ones swing, the rest run.
    ///   * Stand in someone's way and they shove past you or wave you aside.
    ///   * Walk off from someone you antagonized and they get the last word in.
    ///   * Push an argument and a bystander may step in.
    ///   * Pull a gun near people, or put a mask on in front of them, and those
    ///     who see it react.
    ///
    /// Cops are left to GTA6Hud's police while you're wanted.
    /// </summary>
    internal static class Ambient
    {
        private sealed class FollowState
        {
            public Ped Ped;
            public int Meter, Close, Touch;
            public bool Told;
            public int ToldAt;
        }

        private sealed class BlockState
        {
            public int Ms, NextAt;
        }

        private static readonly Dictionary<int, FollowState> Following = new Dictionary<int, FollowState>();
        private static readonly Dictionary<int, BlockState> Blocked = new Dictionary<int, BlockState>();
        private static readonly Dictionary<int, KeyValuePair<Ped, int>> Leaving = new Dictionary<int, KeyValuePair<Ped, int>>();
        private static int _weaponNoticed;
        private static bool _wasArmed, _wasMasked;
        private static int _nextTick, _nextClean;

        public static void Reset()
        {
            Following.Clear();
            Blocked.Clear();
            Leaving.Clear();
        }

        private static bool Busy(Ped ped) =>
            UM.Fleeing(ped) || UM.InCombatWith(ped, UM.Player) || UM.InScenario(ped) || UM.InVehicle(ped) || Witness.IsWitness(ped);

        /// <summary>GTA6Hud's police brain drives every officer near you while you're wanted.</summary>
        private static bool CopsBusy(Ped ped) => UM.Wanted > 0 && UM.IsCop(ped);

        /// <summary>Called by Antagonize: watch for the player walking off.</summary>
        public static void WatchLeave(Ped ped)
        {
            if (MemCfg.TauntAsYouLeave) Leaving[ped.Handle] = new KeyValuePair<Ped, int>(ped, UM.Now);
        }

        /// <summary>Called by Antagonize at stages 2 and 3: someone nearby may step in.</summary>
        public static void Bystander(Ped target, int stage)
        {
            if (!MemCfg.Bystander || UM.Roll() > MemCfg.BystanderChance) return;
            Ped player = UM.Player;

            foreach (UM.Near e in UM.NearbyPeople(player.Position, MemCfg.BystanderRange))
            {
                Ped ped = e.Ped;
                if (ped == target || Busy(ped) || CopsBusy(ped) || UM.IsGang(ped) || UM.TemperOf(ped) == Temper.Timid || !UM.HasLos(ped, player))
                    continue;

                UM.ClearTasks(ped);
                if (stage >= 3 || e.Dist <= 4f) UM.TurnTo(ped, player, 2000);
                else Function.Call(Hash.TASK_GO_TO_ENTITY, ped, player, 6000, 3f, 1.5f, 0, 0);
                Later.Run(ped, stage >= 3 ? 600 : 2500, () =>
                {
                    bool cop = UM.IsCop(ped);
                    Anims.React(ped, cop ? "copWarn" : "bystanderStepsIn");
                    if (cop) UM.Say(ped, MemCfg.LinesCopWarn);
                });
                Mem.Touch(ped, -8f);
                UM.Debug("bystander stepped in (stage " + stage + ")");
                return;
            }
        }

        // ---- following and body-blocking ----------------------------------------------------

        private static void ToldOff(Ped ped, FollowState s)
        {
            Ped player = UM.Player;
            s.Told = true;
            s.ToldAt = UM.Now;
            UM.ClearTasks(ped);
            UM.TurnTo(ped, player, 1500);
            Later.Run(ped, 500, () =>
            {
                bool cop = UM.IsCop(ped);
                Anims.React(ped, cop ? "copWarn" : "toldOffFollower");
                UM.Say(ped, cop ? MemCfg.LinesCopWarn : MemCfg.LinesCold);
            });
            Later.Run(ped, 2500, () =>
            {
                if (!UM.IsCop(ped)) UM.Flee(ped, UM.Player, 30f, 8000);
            });
            Mem.Touch(ped, -6f);
        }

        private static void Escalate(Ped ped)
        {
            // An officer: on FiveM this filled their recognition meter, which is
            // GTA6Hud's police memory here. They've told you; that's it.
            if (UM.IsCop(ped)) return;
            UM.Temperament(ped, out Temper temper, out PedClass cls);
            if (temper == Temper.Brave || cls == PedClass.Gang)
            {
                Person r = Mem.Of(ped);
                r.HostileUntil = UM.Now + 60000;
                Anims.React(ped, "squareUp");
                Later.Run(ped, 900, () => UM.Combat(ped, UM.Player));
            }
            else
            {
                UM.Say(ped, MemCfg.LinesScared, "SPEECH_PARAMS_FORCE_SHOUTED");
                UM.Flee(ped, UM.Player, 150f, -1);
            }
        }

        private static void TickFollow(Ped player, Vector3 pc, int dtMs)
        {
            Vector3 vel = player.Velocity;
            float speed = (float)Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y);
            var ahead = new Vector3(pc.X + vel.X, pc.Y + vel.Y, pc.Z); // where you'll be in 1 s
            bool moving = speed > 0.3f;
            float hx = moving ? vel.X / speed : 0f, hy = moving ? vel.Y / speed : 0f;
            float cosLimit = (float)Math.Cos(MemCfg.FollowAngle * Math.PI / 180.0);

            foreach (UM.Near e in UM.NearbyPeople(pc, MemCfg.FollowRange))
            {
                Ped ped = e.Ped;
                if (CopsBusy(ped)) continue;
                Following.TryGetValue(ped.Handle, out FollowState s);
                if (s != null && s.Ped != ped) s = null;
                if (Busy(ped) && (s == null || !s.Told)) continue;
                if (s == null)
                {
                    s = new FollowState { Ped = ped };
                    Following[ped.Handle] = s;
                }
                Vector3 p = ped.Position;

                bool toward = false;
                if (moving)
                {
                    float dx = p.X - pc.X, dy = p.Y - pc.Y;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (len > 0.01f)
                    {
                        float cos = (hx * dx + hy * dy) / len;
                        toward = cos >= cosLimit && ahead.DistanceTo(p) <= MemCfg.FollowRange;
                    }
                }
                s.Meter = toward ? s.Meter + dtMs : Math.Max(0, s.Meter - dtMs);
                s.Close = e.Dist <= MemCfg.FollowCloseRange ? s.Close + dtMs : 0;
                s.Touch = e.Dist <= MemCfg.FollowTouchRange ? s.Touch + dtMs : 0;

                bool tripped = s.Meter >= MemCfg.FollowMeterMs || s.Close >= MemCfg.FollowCloseMs || s.Touch >= MemCfg.FollowTouchMs;
                if (tripped && !s.Told)
                {
                    ToldOff(ped, s);
                }
                else if (s.Told && UM.Now - s.ToldAt >= MemCfg.FollowEscalateMs)
                {
                    if (e.Dist <= MemCfg.FollowCloseRange) Escalate(ped);
                    Following.Remove(ped.Handle);
                }
            }
        }

        private static void TickBodyBlock(Ped player, Vector3 pc, int dtMs)
        {
            foreach (UM.Near e in UM.NearbyPeople(pc, MemCfg.BodyBlockRange))
            {
                Ped ped = e.Ped;
                if (CopsBusy(ped)) continue;
                if (!Blocked.TryGetValue(ped.Handle, out BlockState s))
                {
                    s = new BlockState();
                    Blocked[ped.Handle] = s;
                }
                Vector3 fwd = Function.Call<Vector3>(Hash.GET_ENTITY_FORWARD_VECTOR, ped);
                Vector3 d = pc - ped.Position;
                float len = (float)Math.Sqrt(d.X * d.X + d.Y * d.Y);
                bool inTheWay = !Busy(ped) && Function.Call<float>(Hash.GET_ENTITY_SPEED, ped) > 0.3f && len > 0.01f
                    && (fwd.X * d.X + fwd.Y * d.Y) / len > 0.7f;

                s.Ms = inTheWay ? s.Ms + dtMs : 0;
                if (s.Ms >= MemCfg.BodyBlockMs && UM.Now >= s.NextAt)
                {
                    s.Ms = 0;
                    s.NextAt = UM.Now + 20000;
                    UM.Temperament(ped, out Temper temper, out PedClass cls);
                    bool cop = UM.IsCop(ped);
                    if ((temper == Temper.Brave || cls == PedClass.Gang) && !cop)
                    {
                        Anims.Play(ped, "shove");
                        Later.Run(player, 350, () => Anims.Shoved(player, ped));
                    }
                    else
                    {
                        Anims.React(ped, cop ? "copWarn" : "bodyBlocked");
                    }
                    UM.Say(ped, MemCfg.LinesCold);
                    Mem.Touch(ped, -4f);
                }
            }
        }

        // ---- weapon drawn, mask on ------------------------------------------------------------

        private static List<Ped> WitnessesOf(Ped player, float range, int limit)
        {
            var list = new List<Ped>();
            foreach (UM.Near e in UM.NearbyPeople(player.Position, range))
            {
                if (list.Count >= limit) break;
                if (!Busy(e.Ped) && !UM.IsCop(e.Ped) && UM.CanSee(e.Ped, player)) list.Add(e.Ped);
            }
            return list;
        }

        private static void TickWeaponAndMask(Ped player)
        {
            bool armed = UM.Armed(player, 4) && !UM.InVehicle(player);
            if (armed && !_wasArmed && MemCfg.WeaponSeen && UM.Now >= _weaponNoticed)
            {
                _weaponNoticed = UM.Now + MemCfg.WeaponSeenCooldownMs;
                foreach (Ped ped in WitnessesOf(player, MemCfg.WeaponSeenRange, 3))
                {
                    if (UM.TemperOf(ped) == Temper.Brave || UM.IsGang(ped))
                    {
                        UM.LookAt(ped, player, 3000);
                        Anims.React(ped, "weaponSeenBrave");
                    }
                    else
                    {
                        Anims.React(ped, "weaponSeenTimid");
                        Later.Run(ped, 1500, () => UM.Flee(ped, player, 20f, 6000));
                    }
                }
            }
            _wasArmed = armed;

            bool masked = UM.IsMasked(player);
            if (masked && !_wasMasked && MemCfg.MaskSeen)
            {
                foreach (Ped ped in WitnessesOf(player, MemCfg.MaskSeenRange, 3))
                {
                    Anims.React(ped, "maskSeen");
                    if (UM.TemperOf(ped) == Temper.Timid) Later.Run(ped, 1500, () => UM.Flee(ped, player, 25f, 6000));
                }
            }
            _wasMasked = masked;
        }

        // ---- taunting as you walk away -------------------------------------------------------------

        private static void TickLeaving(Vector3 pc)
        {
            if (Leaving.Count == 0) return;
            int now = UM.Now;
            var done = new List<int>();
            foreach (KeyValuePair<int, KeyValuePair<Ped, int>> kv in Leaving)
            {
                Ped ped = kv.Value.Key;
                if (ped == null || !ped.Exists() || now - kv.Value.Value > MemCfg.TauntWithinMs)
                {
                    done.Add(kv.Key);
                }
                else if (ped.Position.DistanceTo(pc) > MemCfg.TauntRange)
                {
                    done.Add(kv.Key);
                    if (UM.Alive(ped) && !Busy(ped) && UM.TemperOf(ped) != Temper.Timid && !UM.IsCop(ped))
                    {
                        UM.TurnTo(ped, UM.Player, 1500);
                        Later.Run(ped, 400, () => Anims.React(ped, "tauntAsYouLeave"));
                        UM.Say(ped, MemCfg.LinesInsult, "SPEECH_PARAMS_FORCE_SHOUTED");
                    }
                }
            }
            foreach (int h in done) Leaving.Remove(h);
        }

        public static void Tick(Ped player)
        {
            int now = UM.Now;
            if (now < _nextTick) return;
            _nextTick = now + MemCfg.AmbientScanMs;

            if (!Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, player, true))
            {
                Vector3 pc = player.Position;
                bool onFoot = !UM.InVehicle(player);
                if (onFoot && MemCfg.Follow) TickFollow(player, pc, MemCfg.AmbientScanMs);
                if (onFoot && MemCfg.BodyBlock) TickBodyBlock(player, pc, MemCfg.AmbientScanMs);
                TickWeaponAndMask(player);
                TickLeaving(pc);
            }

            if (now >= _nextClean)
            {
                _nextClean = now + 20000;
                var gone = new List<int>();
                foreach (KeyValuePair<int, FollowState> kv in Following) if (kv.Value.Ped == null || !kv.Value.Ped.Exists()) gone.Add(kv.Key);
                foreach (int h in gone) Following.Remove(h);
                gone.Clear();
                foreach (int h in Blocked.Keys) if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, h)) gone.Add(h);
                foreach (int h in gone) Blocked.Remove(h);
            }
        }
    }

    /// <summary>
    /// um_memory/client/recognition.lua: people remembering you without being asked.
    ///
    ///   * A saved face that spawns near you and can see you reacts by what they
    ///     remember: a wave for a friend, a glare for someone you wronged, and -
    ///     if they watched you kill - a phone call to the police, through the
    ///     same witness flow, so you can still scare them off the phone.
    ///   * In a neighbourhood that likes or loathes you, a passer-by now and then
    ///     greets you first or makes a point of it the other way.
    ///
    /// The Lua also let them call for lesser crimes once the police knew you well
    /// (agency notoriety); that half is GTA6Hud's police memory here, so only the
    /// serious flags (a killing) make the call.
    /// </summary>
    internal static class Recognition
    {
        private static readonly Dictionary<int, int> Scanned = new Dictionary<int, int>(); // [ped] = model already checked
        private static readonly HashSet<string> Noted = new HashSet<string>();             // notes shown this session
        private static int _nextZoneReact, _nextTick, _nextClean;

        public static void Reset()
        {
            Scanned.Clear();
            Noted.Clear();
        }

        private static bool Busy(Ped ped) => UM.Fleeing(ped) || UM.InCombatWith(ped, UM.Player) || UM.InVehicle(ped);

        private static void NoteOnce(string key, string text)
        {
            if (!Noted.Add(key)) return;
            MemoryUi.Notify(text);
        }

        private static bool GossipWorthIt(Person r) => MemCfg.Gossip && (r.Flags & MemCfg.SeriousFlags) != 0;

        /// <summary>A remembered person has just spotted the player.</summary>
        private static void React(Ped ped, Person r)
        {
            Ped player = UM.Player;
            r.LastReact = UM.Now;
            Temper temper = UM.TemperOf(ped);
            bool masked = UM.IsMasked(player);

            // They saw you do something. A mask hides who you are from them too.
            if ((r.Flags & MemCfg.AllCrimeFlags) != 0 && !masked)
            {
                if (temper == Temper.Brave)
                {
                    UM.TurnTo(ped, player, 1500);
                    Later.Run(ped, 400, () => Anims.React(ped, "grudgeSpotsYou"));
                    UM.Say(ped, MemCfg.LinesInsult);
                    NoteOnce(r.Key, "Someone recognises you, and remembers what you did.");
                }
                else if (GossipWorthIt(r) && !Witness.IsActive && UM.Wanted == 0)
                {
                    Witness.Start(ped, "petty", new Witness.Opts { Stars = MemCfg.GossipStars, Phone = true });
                }
                else
                {
                    Anims.Play(ped, "startle");
                    UM.Say(ped, MemCfg.LinesScared);
                    Later.Run(ped, 1300, () => UM.Flee(ped, UM.Player, 40f, 8000));
                    NoteOnce(r.Key, "Someone recognises you and hurries away.");
                }
                return;
            }

            if (masked) return;

            if (r.Opinion >= 25f)
            {
                UM.LookAt(ped, player, 2500);
                UM.Say(ped, MemCfg.LinesWarm);
                Later.Run(ped, 400, () => Anims.React(ped, "friendSpotsYou"));
                NoteOnce(r.Key, "A familiar face waves at you.");
            }
            else if (r.Opinion <= -25f)
            {
                UM.LookAt(ped, player, 2500);
                if (temper == Temper.Timid)
                {
                    Later.Run(ped, 400, () => Anims.React(ped, "greetCold"));
                    UM.Say(ped, MemCfg.LinesCold);
                }
                else
                {
                    UM.Say(ped, MemCfg.LinesInsult);
                    Later.Run(ped, 400, () => Anims.React(ped, "grudgeSpotsYou"));
                }
                NoteOnce(r.Key, "Someone remembers you, and not fondly.");
            }
        }

        /// <summary>The neighbourhood's mood, shown by whoever happens to be passing.</summary>
        private static void ZoneReaction(Ped player, Vector3 pc)
        {
            if (!MemCfg.ZoneReactions || UM.Now < _nextZoneReact || UM.IsMasked(player)) return;
            float rep = Mem.ZoneRep(UM.ZoneCode(pc));
            if (rep < MemCfg.ZoneLiked && rep > MemCfg.ZoneDisliked) return;
            if (UM.Roll() > MemCfg.ZoneReactionChance) return;

            foreach (UM.Near e in UM.NearbyPeople(pc, 8f))
            {
                Ped ped = e.Ped;
                if (UM.IsCop(ped) || Busy(ped) || !UM.CanSee(ped, player)) continue;
                _nextZoneReact = UM.Now + MemCfg.ZoneReactionCooldownMs;
                UM.LookAt(ped, player, 2000);
                if (rep >= MemCfg.ZoneLiked)
                {
                    UM.Say(ped, MemCfg.LinesWarm);
                    Later.Run(ped, 400, () => Anims.React(ped, "zoneLiked"));
                }
                else
                {
                    UM.Say(ped, UM.TemperOf(ped) == Temper.Timid ? MemCfg.LinesCold : MemCfg.LinesInsult);
                    Later.Run(ped, 400, () => Anims.React(ped, "zoneDisliked"));
                }
                return;
            }
        }

        public static void Tick(Ped player)
        {
            int now = UM.Now;
            if (now < _nextTick) return;
            _nextTick = now + MemCfg.MemoryScanMs;

            if (Mem.Loaded && !Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, player, true))
            {
                Vector3 pc = player.Position;
                int losChecks = 0;
                foreach (UM.Near e in UM.NearbyPeople(pc, MemCfg.ScanRadius))
                {
                    Ped ped = e.Ped;
                    int model = ped.Model.Hash;
                    if (!Scanned.TryGetValue(ped.Handle, out int seen) || seen != model)
                    {
                        Scanned[ped.Handle] = model;
                        // Mem.Of links the saved face when the fingerprint matches.
                        Person fresh = Mem.Of(ped);
                        if (fresh.Remembered && UM.Roll() > MemCfg.RecogniseChance) fresh.LastReact = now; // this one doesn't place you
                    }

                    // Officers: GTA6Hud's police memory recognises you, not this.
                    Person r = Mem.Peek(ped);
                    if (r != null && r.Remembered && e.Dist <= MemCfg.RecogniseRange && !UM.IsCop(ped)
                        && now - r.LastReact > MemCfg.ReactCooldownMs && !Busy(ped) && !Witness.IsWitness(ped) && losChecks < 3)
                    {
                        losChecks++;
                        if (UM.CanSee(ped, player)) React(ped, r);
                    }
                }
                ZoneReaction(player, pc);
            }

            if (now >= _nextClean)
            {
                _nextClean = now + 20000;
                var gone = new List<int>();
                foreach (int h in Scanned.Keys) if (!Function.Call<bool>(Hash.DOES_ENTITY_EXIST, h)) gone.Add(h);
                foreach (int h in gone) Scanned.Remove(h);
            }
        }
    }
}
