using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras.NpcMemory
{
    /// <summary>
    /// The wanted level, as um_memory/client/integrations.lua raiseWanted() did it
    /// without fenix-police: raise only, never lower, and only up to whatever
    /// maximum is switched on. The same SetWantedLevel(Max(current, n)) GTA6Hud
    /// uses for its own crimes, so the two never pull against each other.
    /// </summary>
    internal static class Law
    {
        public static bool RaiseWanted(int stars)
        {
            stars = Math.Max(0, Math.Min(5, stars));
            if (stars <= 0 || UM.Wanted >= stars || Lspdfr.OnDuty || PoliceMods.Lsr) return false;
            if (Function.Call<int>(Hash.GET_MAX_WANTED_LEVEL) < stars) return false;
            Game.Player.Wanted.SetWantedLevel(stars, false);
            Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
            UM.Debug("wanted raised to " + stars);
            return true;
        }
    }

    /// <summary>
    /// um_memory/client/witness.lua: someone saw it and is going to tell; someone
    /// who only heard it comes to look. One report runs at a time; a second
    /// crime while it's running only raises the stakes.
    ///
    ///   saw it     runs for it, flashing blip; the report identifies you
    ///   heard it   walks to where it happened, steady blip; spots you and they
    ///              become a "saw it" witness, doesn't and they report an
    ///              unknown suspect
    ///   phone      a remembered face, or someone who found a body, calls it in
    ///
    /// Stop them: kill them, keep a gun on them, or walk up and Intimidate them.
    /// Let them get away and the stars land (raise only).
    /// </summary>
    internal static class Witness
    {
        internal sealed class Opts
        {
            public bool Masked, NoDescription, Phone, Heard;
            public Vector3 At;
            public int Stars = -1;
        }

        private sealed class Active
        {
            public Ped Ped;
            public string Crime;
            public int Stars;
            public bool Masked, NoDescription, Heard, Looking;
            public int LookUntil, DueAt, AimedMs;
            public Blip Blip;
        }

        private static Active _w;
        private static int _lastTick;

        public static bool IsActive => _w != null;
        public static bool IsWitness(Ped ped) => _w != null && ped != null && _w.Ped == ped;

        private static int Delay() => UM.Rng.Next(MemCfg.ReportDelayMinMs, MemCfg.ReportDelayMaxMs + 1);

        private static void RemoveBlip(Active w)
        {
            if (w.Blip != null && w.Blip.Exists()) w.Blip.Delete();
            w.Blip = null;
        }

        private static Blip AddBlip(Ped ped, bool flashing)
        {
            Blip b = ped.AddBlip();
            if (b == null) return null;
            b.Sprite = BlipSprite.Standard;  // sprite 1
            b.Color = BlipColor.Red;         // colour 1
            b.Scale = 0.85f;
            b.IsFlashing = flashing;
            b.IsShortRange = false;
            b.Name = "Witness";
            return b;
        }

        private static void RunToReport(Active w)
        {
            UM.ClearTasks(w.Ped);
            Anims.Play(w.Ped, "startle");
            Later.Run(w.Ped, 1200, () =>
            {
                UM.Flee(w.Ped, UM.Player, 250f, -1);
                Function.Call(Hash.SET_PED_KEEP_TASK, w.Ped, true);
            });
            UM.Say(w.Ped, MemCfg.LinesScared, "SPEECH_PARAMS_FORCE_SHOUTED");
            RemoveBlip(w);
            w.Blip = AddBlip(w.Ped, true);
        }

        public static void Start(Ped ped, string crimeKey, Opts o)
        {
            if (!MemCfg.Witnesses || Lspdfr.OnDuty || PoliceMods.Lsr || !MemCfg.Crimes.TryGetValue(crimeKey, out MemCfg.Crime crime)) return;
            int stars = o.Stars >= 0 ? o.Stars : crime.Stars;

            if (_w != null)
            {
                if (stars > _w.Stars) _w.Stars = stars;
                _w.Masked = _w.Masked && o.Masked;
                _w.NoDescription = _w.NoDescription && o.NoDescription;
                return;
            }

            // Nothing to tell. On FiveM a description alone still fed the police's
            // memory of you; that memory is GTA6Hud's here, so a report worth no
            // stars would change nothing and is not made.
            if (stars <= 0) return;
            if (!UM.IsPerson(ped)) return;

            var w = new Active
            {
                Ped = ped, Crime = crimeKey, Stars = stars, Masked = o.Masked, NoDescription = o.NoDescription,
                Heard = o.Heard, DueAt = UM.Now + Delay(),
            };
            _w = w;

            if (o.Phone)
            {
                Anims.Play(ped, "startle");
                Later.Run(ped, 1300, () => Function.Call(Hash.TASK_USE_MOBILE_PHONE_TIMED, ped, MemCfg.ReportDelayMaxMs));
                UM.Say(ped, MemCfg.LinesCallCops);
                w.Blip = AddBlip(ped, true);
                MemoryUi.Feed("Someone recognised you and is {calling the police}");
            }
            else if (o.Heard)
            {
                // Comes to look first; the report clock starts when they give up.
                w.Looking = true;
                w.LookUntil = UM.Now + MemCfg.InvestigateMs;
                w.DueAt = w.LookUntil + Delay();
                UM.ClearTasks(ped);
                Vector3 at = o.At != Vector3.Zero ? o.At : UM.Player.Position;
                Function.Call(Hash.TASK_GO_TO_COORD_ANY_MEANS, ped, at.X, at.Y, at.Z, 1.5f, 0, false, 786603, 0f);
                w.Blip = AddBlip(ped, false);
                MemoryUi.Feed("Someone {heard} that and is coming to look");
            }
            else
            {
                RunToReport(w);
                MemoryUi.Feed("A {witness} is running to report you");
            }
            UM.Debug("witness started: " + crimeKey + " (" + stars + " stars, masked=" + o.Masked + ", heard=" + o.Heard + ")");
        }

        private static void Finish(string outcome)
        {
            Active w = _w;
            if (w == null) return;
            _w = null;
            RemoveBlip(w);

            if (outcome == "intimidated")
            {
                if (w.Ped != null && w.Ped.Exists())
                {
                    UM.ClearTasks(w.Ped);
                    Function.Call(Hash.TASK_HANDS_UP, w.Ped, 6000, UM.Player, -1, true);
                    UM.Say(w.Ped, MemCfg.LinesScared);
                    Mem.Touch(w.Ped, MemCfg.OpinionIntimidated, MemCfg.Crimes["brandish"].Flag);
                    Ped p = w.Ped;
                    Later.Run(p, 6500, () => UM.Flee(p, UM.Player, 100f, -1));
                }
                MemoryUi.Notify("The witness won't say a word.");
                return;
            }

            if (outcome == "silenced")
            {
                MemoryUi.Notify("The witness never made the call.");
                return;
            }

            // Reported.
            int stars = w.Heard ? Math.Min(w.Stars, MemCfg.HeardStarsMax) : w.Stars;
            if (stars > 0) Law.RaiseWanted(stars);
            if (w.Masked || w.Heard) MemoryUi.Feed("Crime reported. {Unknown suspect}.");
            else if (w.NoDescription) MemoryUi.Feed("Crime {reported}.");
            else MemoryUi.Feed("Crime reported. They gave the police {your description}.");
            UM.Debug("witness reported: " + w.Crime + " (" + stars + " stars)");
        }

        /// <summary>The prompt's Intimidate (or anything else that scares them quiet).</summary>
        public static bool Intimidate(Ped ped)
        {
            if (!IsWitness(ped)) return false;
            Finish("intimidated");
            return true;
        }

        public static void Cancel()
        {
            if (_w != null) RemoveBlip(_w);
            _w = null;
        }

        /// <summary>Every frame; does its work every 100 ms, as the Lua loop did.</summary>
        public static void Tick()
        {
            Active w = _w;
            int now = UM.Now;
            if (w == null)
            {
                _lastTick = now;
                return;
            }
            int step = now - _lastTick;
            if (step < 100) return;
            _lastTick = now;

            Ped player = UM.Player;
            bool exists = w.Ped != null && w.Ped.Exists();

            if (exists && Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, w.Ped, true))
            {
                Finish("silenced");
            }
            else if (exists && Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING_AT_ENTITY, Game.Player, w.Ped)
                && w.Ped.Position.DistanceTo(player.Position) <= MemCfg.IntimidateRange)
            {
                w.AimedMs += step;
                if (w.AimedMs >= MemCfg.IntimidateMs) Finish("intimidated");
            }
            else if (w.Looking && exists)
            {
                // Heard it, came to look: do they find you?
                if (UM.CanSee(w.Ped, player) && w.Ped.Position.DistanceTo(player.Position) <= MemCfg.WitnessRange)
                {
                    w.Looking = false;
                    w.Heard = false;
                    w.Masked = w.Masked || UM.IsMasked(player);
                    if (!w.Masked) Mem.Touch(w.Ped, MemCfg.OpinionWitness, MemCfg.Crimes[w.Crime].Flag);
                    w.DueAt = now + Delay();
                    RunToReport(w);
                    MemoryUi.Feed("They {saw you}. Now they're running to report it");
                }
                else if (now >= w.LookUntil)
                {
                    w.Looking = false;
                    RunToReport(w);
                }
            }
            else if (now >= w.DueAt)
            {
                // Despawned counts as "got away": they still made the call.
                Finish("reported");
            }
        }
    }

    /// <summary>
    /// um_memory/client/crimes.lua: notices what you do, then hands it to the
    /// people who saw it. The victim remembers you; everyone with eyes on you
    /// remembers your face (not if you're masked); the neighbourhood thinks less
    /// of you; a cop who saw it reports at once; otherwise one civilian runs to
    /// report it.
    ///
    /// FiveM told the Lua about every hit (CEventNetworkEntityDamage). Single
    /// player has no such event, so hits are read off the peds around you: a
    /// ped whose health dropped since the last look, that the game says you (or
    /// the car you're driving) damaged, while you were actually fighting,
    /// shooting or driving. Kills are the game's own source of death.
    /// </summary>
    internal static class Crimes
    {
        private const int SelfDefenceMs = 25000;

        private static readonly Dictionary<int, int> Aggressors = new Dictionary<int, int>();   // [ped] = when they last hurt us
        private static readonly Dictionary<int, int> LastAssault = new Dictionary<int, int>();  // [ped] = when we last counted an assault
        private static readonly HashSet<int> Instigated = new HashSet<int>();                  // the player threw the first blow
        private static readonly Dictionary<string, int> Throttle = new Dictionary<string, int>();
        private static readonly Dictionary<int, int> Health = new Dictionary<int, int>();
        private static readonly HashSet<int> DeadSeen = new HashSet<int>();
        private static readonly HashSet<int> NotHuman = new HashSet<int>();                    // animals, skipped by ScanHits
        private static readonly Dictionary<int, int> Brandished = new Dictionary<int, int>();

        private static int _lastShotAt = int.MinValue / 2, _lastActAt = int.MinValue / 2;
        private static int _playerHp = -1;
        private static Ped _aimTarget;
        private static int _aimSince;
        private static int _nextTick, _nextClean, _nextHits;

        private sealed class Body
        {
            public Ped Ped;
            public Vector3 Coords;
            public int At;
        }

        private static readonly List<Body> Bodies = new List<Body>();
        private static int _nextBodyScan;

        public static void Reset()
        {
            Aggressors.Clear();
            LastAssault.Clear();
            Instigated.Clear();
            Throttle.Clear();
            Brandished.Clear();
            Bodies.Clear();
            _aimTarget = null;
        }

        private static bool Throttled(string kind, int ms)
        {
            int now = UM.Now;
            if (Throttle.TryGetValue(kind, out int at) && now < at) return true;
            Throttle[kind] = now + ms;
            return false;
        }

        private static bool HurtUsRecently(Ped ped) => Aggressors.TryGetValue(ped.Handle, out int at) && UM.Now - at < SelfDefenceMs;

        /// <summary>Fair game for a fight: they hurt you, or they're squaring up after you antagonized them.</summary>
        public static bool IsAggressor(Ped ped)
        {
            if (HurtUsRecently(ped)) return true;
            Person r = Mem.Peek(ped);
            return r != null && UM.Now < r.HostileUntil;
        }

        /// <summary>
        /// RDR2's self-defence rule: if you threw the first blow it's murder,
        /// whatever they did after. Otherwise it's self-defence if they hurt you,
        /// were holding a weapon, or had turned on you before you touched them.
        /// </summary>
        private static bool KillWasJustified(Ped ped)
        {
            if (Instigated.Contains(ped.Handle)) return false;
            if (HurtUsRecently(ped)) return true;
            if (Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped) != unchecked((int)(uint)WeaponHash.Unarmed)) return true;
            Person r = Mem.Peek(ped);
            return r != null && UM.Now < r.HostileUntil;
        }

        internal struct Seen
        {
            public Ped Ped;
            public float Dist;
            public bool Saw;
        }

        /// <summary>
        /// Who noticed. Saw = clear line of sight to the player. Gunfire is also
        /// heard by anyone in range (a silenced weapon only up close).
        /// </summary>
        public static List<Seen> Witnesses(string kind, Ped exclude)
        {
            Ped player = UM.Player;
            float range = MemCfg.WitnessRange;
            bool audible = kind == "shooting" && !Function.Call<bool>(Hash.IS_PED_CURRENT_WEAPON_SILENCED, player);
            if (kind == "shooting" && !audible) range = 15f;

            // One blast or spray commits a crime per ped it hits, all in one frame:
            // who is around, and who of them can see the player, is asked once.
            int frame = Game.FrameCount;
            if (frame != _aroundFrame || range != _aroundRange)
            {
                _aroundFrame = frame;
                _aroundRange = range;
                _around = UM.NearbyPeople(player.Position, range);
                SightOf.Clear();
            }

            var outList = new List<Seen>();
            int checkedLos = 0;
            foreach (UM.Near e in _around)
            {
                if (exclude != null && e.Ped == exclude) continue;
                bool saw = false;
                if (checkedLos < MemCfg.MaxWitnesses * 2)
                {
                    checkedLos++;
                    if (!SightOf.TryGetValue(e.Ped.Handle, out saw)) SightOf[e.Ped.Handle] = saw = UM.HasLos(e.Ped, player);
                }
                if (saw || audible) outList.Add(new Seen { Ped = e.Ped, Dist = e.Dist, Saw = saw });
                if (outList.Count >= MemCfg.MaxWitnesses) break;
            }
            return outList;
        }

        private static int _aroundFrame = -1, _ownedCopFrame = -1;
        private static float _aroundRange;
        private static List<UM.Near> _around;
        private static readonly Dictionary<int, bool> SightOf = new Dictionary<int, bool>();
        private static bool _ownedCopSaw;

        /// <summary>
        /// fenix-police's IsWitnessed() stood for its own scripted officers. The
        /// officers a script owns here (GTA6Hud's patrols) are left out of the
        /// list above, so they are looked for on their own. (Once a frame, as above:
        /// it is always asked about where the player stands.)
        /// </summary>
        private static bool OwnedCopSaw(Vector3 at, float range)
        {
            int frame = Game.FrameCount;
            if (frame == _ownedCopFrame) return _ownedCopSaw;
            _ownedCopFrame = frame;
            _ownedCopSaw = false;
            Ped player = UM.Player;
            int checks = 0;
            foreach (Ped ped in World.GetNearbyPeds(at, range))
            {
                if (!UM.Alive(ped) || ped == player || !UM.IsCop(ped)) continue;
                if (!Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, ped)) continue;
                if (++checks > 4) break;
                if (UM.HasLos(ped, player)) return _ownedCopSaw = true;
            }
            return false;
        }

        // ---- bodies ------------------------------------------------------------------------

        private static void RememberBody(Ped victim)
        {
            if (!MemCfg.Bodies || victim == null || !victim.Exists()) return;
            Bodies.Add(new Body { Ped = victim, Coords = victim.Position, At = UM.Now });
        }

        private static void BodyFound(Body body, Ped finder)
        {
            bool near = UM.Player.Position.DistanceTo(body.Coords) <= MemCfg.BodyNearPlayerRange;
            if (UM.IsCop(finder))
            {
                if (near) Law.RaiseWanted(1);
                MemoryUi.Feed("An officer found a {body}.");
            }
            else
            {
                Witness.Start(finder, "petty", new Witness.Opts { Stars = near ? 1 : 0, Masked = true, Phone = true });
            }
            Mem.AddZone(UM.ZoneCode(body.Coords), MemCfg.ZoneDelta["petty"]);
        }

        private static void ScanBodies()
        {
            if (Bodies.Count == 0) return;
            int now = UM.Now;
            for (int i = Bodies.Count - 1; i >= 0; i--)
            {
                Body body = Bodies[i];
                bool fresh = body.Ped != null && body.Ped.Exists() && now - body.At < MemCfg.BodyKeepMs;
                bool found = false;
                if (fresh && !Witness.IsActive)
                {
                    foreach (UM.Near e in UM.NearbyPeople(body.Coords, MemCfg.BodyFindRange))
                    {
                        if (!UM.IsGang(e.Ped) && UM.HasLos(e.Ped, body.Ped))
                        {
                            found = true;
                            BodyFound(body, e.Ped);
                            break;
                        }
                    }
                }
                if (!fresh || found) Bodies.RemoveAt(i);
            }
        }

        // ---- commit ----------------------------------------------------------------------------

        /// <param name="kind">a key of MemCfg.Crimes</param>
        /// <param name="victim">the ped it was done to, if they are still standing</param>
        /// <param name="victimOpinion">override for the victim's opinion change</param>
        /// <param name="victimBody">a ped just killed, so nobody-saw-it can leave a body to find</param>
        public static void Commit(string kind, Ped victim = null, float? victimOpinion = null, Ped victimBody = null)
        {
            // An on-duty LSPDFR officer's force is police work, not a crime; Los Santos RED keeps its own crimes.
            if (Lspdfr.OnDuty || PoliceMods.Lsr || !MemCfg.Crimes.TryGetValue(kind, out MemCfg.Crime crime)) return;

            Ped player = UM.Player;
            Vector3 coords = player.Position;
            bool masked = MemCfg.MaskBlocksIdentity && UM.IsMasked(player);
            int wanted = UM.Wanted;
            bool victimIsPerson = victim != null && UM.IsPerson(victim);

            if (victimIsPerson && !masked) Mem.Touch(victim, victimOpinion ?? MemCfg.OpinionVictim, crime.Flag);
            if (!masked) Mem.AddZoneVia(victimIsPerson ? victim : null, kind);

            bool copSaw = false;
            Ped reporter = null, heardOnly = null;
            foreach (Seen w in Witnesses(kind, victim))
            {
                if (UM.IsCop(w.Ped))
                {
                    if (w.Saw) copSaw = true;
                }
                else if (MemCfg.GangsReport || !UM.IsGang(w.Ped))
                {
                    if (w.Saw)
                    {
                        if (!masked) Mem.Touch(w.Ped, MemCfg.OpinionWitness, crime.Flag);
                        if (reporter == null) reporter = w.Ped;
                    }
                    else if (heardOnly == null)
                    {
                        heardOnly = w.Ped;
                    }
                }
            }
            if (!copSaw) copSaw = OwnedCopSaw(coords, MemCfg.WitnessRange);

            UM.Debug("crime " + kind + ": copSaw=" + copSaw + " reporter=" + (reporter != null) + " masked=" + masked);

            // Mid-pursuit the police already know who you are: a report adds no
            // description (killing an officer is the exception).
            bool describes = !masked && (wanted == 0 || kind == "kill_cop");

            if (copSaw)
            {
                Law.RaiseWanted(crime.Stars);
                return;
            }

            // Nobody saw a killing: the body will be found later.
            if (reporter == null && (kind == "kill" || kind == "kill_cop")) RememberBody(victimBody);

            // Already wanted at least this badly: the police are on it, so a
            // witness has nothing to add.
            if (wanted != 0 && crime.Stars <= wanted) return;
            if (reporter != null)
                Witness.Start(reporter, kind, new Witness.Opts { Masked = masked, NoDescription = !describes });
            else if (heardOnly != null)
                Witness.Start(heardOnly, kind, new Witness.Opts { Masked = masked, NoDescription = !describes, Heard = true, At = coords });
        }

        // ---- what the player does -----------------------------------------------------------

        /// <summary>Every frame: a shot or a swing only shows for a frame or two.</summary>
        public static void Frame(Ped player)
        {
            int now = UM.Now;
            if (Function.Call<bool>(Hash.IS_PED_SHOOTING, player)) _lastShotAt = _lastActAt = now;
            if (Function.Call<bool>(Hash.IS_PED_PERFORMING_MELEE_ACTION, player) || Function.Call<bool>(Hash.IS_PED_IN_MELEE_COMBAT, player))
                _lastActAt = now;
            // a car only acts when it hits something
            if (UM.InVehicle(player) && Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, player.CurrentVehicle))
                _lastActAt = now;
        }

        private static bool DamagedBy(Ped ped, Ped player, Vehicle veh) =>
            Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, ped, player, true)
            || (veh != null && Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, ped, veh, true));

        private static bool KilledBy(Ped ped, Ped player, Vehicle veh)
        {
            int killer = Function.Call<int>(Hash.GET_PED_SOURCE_OF_DEATH, ped);
            if (killer != 0) return killer == player.Handle || (veh != null && killer == veh.Handle);
            return DamagedBy(ped, player, veh);
        }

        /// <summary>The damage handler (crimes.lua gameEventTriggered), for one hit on `victim`.</summary>
        private static void OnHit(Ped victim, bool fatal)
        {
            bool cop = UM.IsCop(victim);
            int h = victim.Handle;
            // Someone a script owns (GTA6Hud's muggers and street fights, a
            // stranger's encounter) has that script's consequences, not these.
            // Officers count whoever spawned them.
            if (!cop && Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, victim)) return;
            if (fatal)
            {
                bool justified = KillWasJustified(victim);
                Aggressors.Remove(h);
                LastAssault.Remove(h);
                Instigated.Remove(h);
                if (cop) Commit("kill_cop", null, null, victim);
                else if (!justified && !UM.IsGang(victim)) Commit("kill", null, null, victim);
                return;
            }

            if (!cop && (IsAggressor(victim) || UM.IsGang(victim))) return;
            Instigated.Add(h);
            int now = UM.Now;
            if (LastAssault.TryGetValue(h, out int at) && now - at < 10000) return;
            LastAssault[h] = now;
            Commit("assault", victim);
        }

        private static void ScanHits(Ped player)
        {
            int now = UM.Now;
            Vehicle veh = UM.InVehicle(player) ? player.CurrentVehicle : null;

            // Someone hurt us: they're fair game for a while.
            int hp = player.Health + player.Armor;
            if (_playerHp >= 0 && hp < _playerHp)
            {
                foreach (Ped ped in World.GetNearbyPeds(player.Position, 50f))
                    if (ped != player && ped.Exists() && Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, player, ped, true)
                        && UM.InCombatWith(ped, player))
                        Aggressors[ped.Handle] = now;
            }
            _playerHp = hp;

            // Not "in a car" on its own: GTA keeps a ped's "damaged by" record, so a ped
            // once clipped would read every later hurt as a fresh assault.
            bool acting = now - _lastActAt <= 1500;
            // (GetNearbyPeds hands back peds that exist, and in single player the one
            // player ped is `player`; an animal stays one, so it is asked about once.)
            foreach (Ped ped in World.GetNearbyPeds(player.Position, 80f))
            {
                if (ped == null || ped == player) continue;
                int h = ped.Handle;
                if (NotHuman.Contains(h)) continue;
                if (!Function.Call<bool>(Hash.IS_PED_HUMAN, ped)) { NotHuman.Add(h); continue; }
                if (Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, ped, true))
                {
                    if (DeadSeen.Add(h) && Health.ContainsKey(h) && KilledBy(ped, player, veh)) OnHit(ped, true);
                    Health.Remove(h);
                    continue;
                }
                DeadSeen.Remove(h);
                int cur = ped.Health;
                if (Health.TryGetValue(h, out int prev) && cur < prev && acting && DamagedBy(ped, player, veh)) OnHit(ped, false);
                Health[h] = cur;
            }
        }

        private static bool RecentlyAttacked()
        {
            int now = UM.Now;
            foreach (int at in Aggressors.Values)
                if (now - at < SelfDefenceMs) return true;
            return false;
        }

        private static Ped AimedAt()
        {
            var arg = new OutputArgument();
            if (!Function.Call<bool>(Hash.GET_ENTITY_PLAYER_IS_FREE_AIMING_AT, Game.Player, arg)) return null;
            return Entity.FromHandle(arg.GetResult<int>()) as Ped;
        }

        /// <summary>Every 100 ms: hits and kills, gunfire, carjacking, a gun held on someone.</summary>
        public static void Tick(Ped player)
        {
            int now = UM.Now;
            if (now >= _nextBodyScan)
            {
                _nextBodyScan = now + MemCfg.BodyScanMs;
                ScanBodies();
            }
            if (now < _nextTick) return;
            _nextTick = now + 100;

            // Hits every tick while the player fights, shoots or drives into things;
            // otherwise none can count as an assault, and every 250 ms finds a body
            // (or who hurt the player) in good time.
            if (now - _lastActAt <= 1500 || now >= _nextHits)
            {
                _nextHits = now + 250;
                ScanHits(player);
            }

            // Shooting in public, unless someone started it. "Public" is the towns:
            // firing at game out in the county is hunting, and neither is shooting at an animal.
            if (now - _lastShotAt <= 250 && !RecentlyAttacked())
            {
                Ped aimed = AimedAt();
                bool animal = aimed != null && !Function.Call<bool>(Hash.IS_PED_HUMAN, aimed);
                UM.ZoneAt(player.Position, out _, out string region);
                if (!animal && MemCfg.ShootingRegions.Contains(region) && !Throttled("shooting", 8000)) Commit("shooting");
            }

            // Dragging someone out of their car.
            if (Function.Call<bool>(Hash.IS_PED_JACKING, player) && !Throttled("carjack", 8000))
            {
                Vehicle veh = Entity.FromHandle(Function.Call<int>(Hash.GET_VEHICLE_PED_IS_TRYING_TO_ENTER, player)) as Vehicle;
                if (veh == null || !veh.Exists()) veh = player.CurrentVehicle;
                Ped victim = null;
                if (veh != null && veh.Exists())
                {
                    Ped driver = Entity.FromHandle(Function.Call<int>(Hash.GET_PED_IN_VEHICLE_SEAT, veh, -1, false)) as Ped;
                    if (driver != null && driver != player && UM.IsPerson(driver)) victim = driver;
                    else
                    {
                        List<UM.Near> near = UM.NearbyPeople(veh.Position, 4f);
                        victim = near.Count > 0 ? near[0].Ped : null;
                    }
                }
                Commit("carjack", victim);
            }

            // Brandishing: a gun held on a civilian long enough for them to know it.
            Ped target = null;
            if (Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player) && UM.Armed(player, 4))
            {
                Ped ent = AimedAt();
                if (ent != null && UM.IsPerson(ent) && !IsAggressor(ent) && !Witness.IsWitness(ent)) target = ent;
            }

            if (target != _aimTarget)
            {
                _aimTarget = target;
                _aimSince = now;
            }
            else if (target != null && !Brandished.ContainsKey(target.Handle) && now - _aimSince >= MemCfg.BrandishMs)
            {
                Brandished[target.Handle] = now;
                // GTA6Hud robs a ped held at gunpoint within 8 m and puts their
                // hands up itself; only further off does this do it.
                bool gta6HudRobs = target.Position.DistanceTo(player.Position) <= 8f;
                if (!UM.IsCop(target) && !UM.IsGang(target) && !gta6HudRobs)
                {
                    UM.ClearTasks(target);
                    if (UM.TemperOf(target) == Temper.Brave) UM.Flee(target, player, 120f, -1);
                    else Function.Call(Hash.TASK_HANDS_UP, target, 5000, player, -1, true);
                    UM.Say(target, MemCfg.LinesScared);
                }
                Commit("brandish", target, MemCfg.OpinionIntimidated);
            }

            if (now >= _nextClean)
            {
                _nextClean = now + 30000;
                Prune(Aggressors, SelfDefenceMs);
                Prune(LastAssault, 10000);
                Prune(Brandished, 30000);
                var gone = new List<int>();
                foreach (int h in Instigated) if (!Exists(h)) gone.Add(h);
                foreach (int h in gone) Instigated.Remove(h);
                gone.Clear();
                foreach (int h in Health.Keys) if (!Exists(h)) gone.Add(h);
                foreach (int h in gone) Health.Remove(h);
                gone.Clear();
                foreach (int h in DeadSeen) if (!Exists(h)) gone.Add(h);
                foreach (int h in gone) DeadSeen.Remove(h);
                gone.Clear();
                foreach (int h in NotHuman) if (!Exists(h)) gone.Add(h);
                foreach (int h in gone) NotHuman.Remove(h);
            }
        }

        private static bool Exists(int handle) => Function.Call<bool>(Hash.DOES_ENTITY_EXIST, handle);

        private static void Prune(Dictionary<int, int> map, int ms)
        {
            int now = UM.Now;
            var gone = new List<int>();
            foreach (KeyValuePair<int, int> kv in map)
                if (now - kv.Value >= ms || !Exists(kv.Key)) gone.Add(kv.Key);
            foreach (int h in gone) map.Remove(h);
        }

        /// <summary>
        /// A deed named to honor elsewhere (a robbery, an honest job): it happened
        /// where the player is standing (crimes.lua um_memory:client:deed).
        /// </summary>
        public static void Deed(string hook)
        {
            if (hook == null || !MemCfg.Deeds.TryGetValue(hook, out string kind)) return;
            if (kind == "honest") Mem.AddZoneVia(null, "honest");
            else Commit(kind);
        }
    }
}
