using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// A local walking a dog, and the dog's one poop of the walk:
    ///
    ///  Walking   the dog follows its owner (the game's own pair, or one DogWalkers made);
    ///  Squat     the owner stops and watches; the dog squats (Rockstar's dump_enter / dump_loop / dump_exit from
    ///            chop.c, creatures@rottweiler@move, or the small dogs' own when the game has them) with chop.c's
    ///            ent_anim_dog_poo, and leaves prop_big_shit_02 behind;
    ///  Decide    the owner looks at it, then picks it up (PickUpChance) or walks on and leaves it there;
    ///  Approach  walks over to it;
    ///  Pickup    pickup_object / pickup_low: at the grab the poop goes and a tied bag is in their right hand;
    ///  Carrying  back to the walk, bag in hand.
    ///
    /// Through all of it, the owner getting hurt by the player, knocked over or stunned drops the bag (it pops out of
    /// the hand and falls) and the pair is let go: their reaction, and with Euphoria Ragdoll Overhaul installed its
    /// ragdoll, is the game's own, untouched.
    /// </summary>
    internal sealed class Walker
    {
        public enum St { Walking, Squat, Decide, Approach, Pickup, Carrying, Gone }

        private const string PickDict = "pickup_object", PickClip = "pickup_low";
        private const string BigDict = "creatures@rottweiler@move", SmallDict = "creatures@pug@move";
        private const string Enter = "dump_enter", Loop = "dump_loop", Exit = "dump_exit";
        private static readonly int FollowTask = Game.GenerateHash("script_task_follow_to_offset_of_entity");

        public readonly Ped Owner, Dog;
        public readonly bool Spawned;
        public readonly bool Big;
        public St Now { get; private set; } = St.Walking;
        public int NextPoopAt;
        public bool Pooped;
        public PoopBag Bag { get; private set; }

        private Prop _poop;
        private bool _held;              // made mission entities for the sequence
        // Owner and dog are held (mission entities) from the moment they're found until the player is far away:
        // handed back to the game near the player, a local who'd just bagged a poop was deleted by the population
        // clean-up in front of them (the user, 2026-11-04).
        private bool _following;         // we gave the dog its follow task
        private int _stepAt, _sub, _followCheckAt;
        private int _fx;
        private bool _fxStarted;
        private string _dict;
        private int _enterMs, _loopMs, _exitMs;
        private Vector3 _spot;           // where the owner stands to pick it up

        public Walker(Ped owner, Ped dog, bool spawned, int now)
        {
            Owner = owner;
            Dog = dog;
            Spawned = spawned;
            Big = DogCfg.IsBig(dog);
            NextPoopAt = now + DogCfg.Rng.Next(DogCfg.PoopMinMs, Math.Max(DogCfg.PoopMinMs + 1, DogCfg.PoopMaxMs));
            Function.Call(Hash.CLEAR_ENTITY_LAST_DAMAGE_ENTITY, owner);
            Function.Call(Hash.CLEAR_ENTITY_LAST_DAMAGE_ENTITY, dog);
            Tough.Make(owner);
            // pugs, poodles and westies have no dump clips in GTA V: they'd just stand there
            if (!Big && !DogCfg.SmallDogsPoop) Pooped = true;
            owner.IsPersistent = true;
            dog.IsPersistent = true;
            DogWalkers.HeldDogs.Add(dog.Handle);
            if (spawned) Follow();
        }

        public bool Alive => Owner != null && Owner.Exists() && !Owner.IsDead && Dog != null && Dog.Exists() && !Dog.IsDead;

        /// <summary>Busy with the poop: its owner isn't to be picked for anything else.</summary>
        public bool InSequence => Now == St.Squat || Now == St.Decide || Now == St.Approach || Now == St.Pickup;

        /// <summary>One frame. False once the pair is let go (the owner got hurt, one of them is gone).</summary>
        public bool Tick(DogWalkers mod, Ped player, int now)
        {
            if (Now == St.Gone) return false;
            if (!Alive || Hurt(Owner, player) || Hurt(Dog, player))
            {
                Spook(mod);
                return false;
            }

            switch (Now)
            {
                case St.Walking:
                    KeepFollowing(player, now);
                    if (!Pooped && now >= NextPoopAt && CanStart(player)) StartSquat(now);
                    break;
                case St.Squat: Squat(mod, now); break;
                case St.Decide: Decide(mod, now); break;
                case St.Approach: Approach(mod, now); break;
                case St.Pickup: Pickup(mod, now); break;
                case St.Carrying:
                    KeepFollowing(player, now);
                    if (Bag != null && !Bag.Exists) Bag = null;
                    if (Bag != null) Bag.KeepHanging(Game.LastFrameTime);
                    break;
            }
            return true;
        }

        /// <summary>The test key: this dog goes now.</summary>
        public void PoopNow(int now)
        {
            Pooped = false;
            NextPoopAt = now;
            if (Now == St.Carrying) Now = St.Walking;
        }

        private static bool Hurt(Ped p, Ped player)
        {
            if (p.IsDead) return true;
            if (Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, p, player, true)) return true;
            if (Tough.Punched(p, 900)) return true;
            if (Function.Call<bool>(Hash.IS_PED_RAGDOLL, p)) return true;
            if (Function.Call<bool>(Hash.IS_PED_BEING_STUNNED, p, 0)) return true;
            return false;
        }

        private bool CanStart(Ped player)
        {
            if (player.Position.DistanceTo(Dog.Position) > DogCfg.PoopRange) return false;
            if (Owner.IsInVehicle() || Dog.IsInVehicle()) return false;
            if (Function.Call<bool>(Hash.IS_PED_IN_COMBAT, Owner, 0) || Function.Call<bool>(Hash.IS_PED_FLEEING, Owner)) return false;
            if (Function.Call<bool>(Hash.IS_PED_SWIMMING, Dog) || Function.Call<bool>(Hash.IS_ENTITY_IN_WATER, Dog)) return false;
            if (Owner.Position.DistanceTo(Dog.Position) > 6f) return false;
            // not in the middle of the road: try again in a few seconds
            Vector3 d = Dog.Position;
            if (Function.Call<bool>(Hash.IS_POINT_ON_ROAD, d.X, d.Y, d.Z, 0))
            {
                NextPoopAt = Game.GameTime + 4000;
                return false;
            }
            return true;
        }

        // ---- the squat --------------------------------------------------------------------------------------------

        private void StartSquat(int now)
        {
            Hold(true);
            _dict = Big ? BigDict : SmallDict;
            if (Function.Call<bool>(Hash.DOES_ANIM_DICT_EXIST, _dict)) Function.Call(Hash.REQUEST_ANIM_DICT, _dict);
            else _dict = null;
            Function.Call(Hash.REQUEST_ANIM_DICT, PickDict);
            PoopFx.Request();

            Function.Call(Hash.CLEAR_PED_TASKS, Dog);
            Function.Call(Hash.TASK_STAND_STILL, Dog, 3000);
            // the owner stops, turns to the dog and waits
            using (var seq = new TaskSequence())
            {
                seq.AddTask.TurnTo(Dog, 900);
                seq.AddTask.StandStill(25000);
                seq.Close();
                Owner.Task.PerformSequence(seq);
            }
            Function.Call(Hash.TASK_LOOK_AT_ENTITY, Owner, Dog, 14000, 0, 2);
            _following = false;
            _fxStarted = false;
            Now = St.Squat;
            _sub = 0;
            _stepAt = now;
            DogCfg.Trace("dog " + Dog.Handle + " squats (" + (Big ? "big" : "small") + ")");
        }

        private float Len(string clip)
        {
            float s = Function.Call<float>(Hash.GET_ANIM_DURATION, _dict, clip);
            return s > 0.05f ? s : 0f;
        }

        private void Play(Ped ped, string dict, string clip, int flags) =>
            Function.Call(Hash.TASK_PLAY_ANIM, ped, dict, clip, 4f, -4f, -1, flags, 0f, false, false, false);

        private void Squat(DogWalkers mod, int now)
        {
            int t = now - _stepAt;
            if (_sub == 0)
            {
                // waiting for the clips (and the owner's turn)
                bool loaded = _dict != null && Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, _dict);
                if (!loaded && t < 2500) return;
                if (loaded)
                {
                    float e = Len(Enter), l = Len(Loop), x = Len(Exit);
                    if (e > 0f && l > 0f && x > 0f)
                    {
                        // Rockstar's rottweiler clips run 6.3 s (sniff, turn, squat), 11 s (squatting), 1.3 s: the
                        // squat is held SquatMs, not the loop's whole length (chop.c's poo runs from 4 s to 10 s).
                        _enterMs = (int)(e * 1000f);
                        _loopMs = DogCfg.SquatMs;
                        _exitMs = (int)(x * 1000f);
                        Play(Dog, _dict, Enter, 2);
                        _sub = 1;
                        _stepAt = now;
                        DogCfg.Trace("  clips " + _dict + " " + e.ToString("0.00") + "/" + l.ToString("0.00") + "/" + x.ToString("0.00") + " s");
                        return;
                    }
                }
                // no clips for this dog: it just stands there for it
                if (_dict != null) DogCfg.Trace("  no dump clips in " + _dict + ": standing");
                _dict = null;
                _enterMs = 600;
                _loopMs = DogCfg.SquatMs;
                _exitMs = 600;
                Function.Call(Hash.TASK_STAND_STILL, Dog, _enterMs + _loopMs + _exitMs);
                _sub = 1;
                _stepAt = now;
                return;
            }

            int loopFrom = _enterMs, loopTo = _enterMs + _loopMs, end = loopTo + _exitMs;
            // The dump clips drop their own poo (an ent_anim effect, fired by the clip), so none is added for them:
            // two looked wrong (the user, in game). Only a dog without clips gets chop.c's, from 0.8 s; SquatFx = True
            // adds it to the clips' too, from 4 s as chop.c has it.
            int fxFrom = _dict != null ? Math.Min(4000, loopFrom) : 800;
            if (!_fxStarted && t >= fxFrom && (_dict == null || DogCfg.SquatFx))
            {
                Vector3 off = Big ? new Vector3(0f, -0.15f, -0.2f) : new Vector3(0f, -0.10f, -0.08f);   // chop.c's, smaller for small dogs
                _fx = PoopFx.OnEntity(Dog, off, Big ? 1f : 0.6f);
                _fxStarted = true;
            }
            if (_sub == 1 && t >= loopFrom)
            {
                if (_dict != null) Play(Dog, _dict, Loop, 1);
                _sub = 2;
            }
            if (_sub == 2 && t >= loopTo - 300)
            {
                PoopFx.Stop(_fx);
                _fx = 0;
                PlacePoop();
                if (_dict != null) Play(Dog, _dict, Exit, 0);
                _sub = 3;
            }
            if (_sub == 3 && t >= end)
            {
                Pooped = true;
                Function.Call(Hash.TASK_STAND_STILL, Dog, 15000);    // it waits while the owner deals with it
                Function.Call(Hash.TASK_LOOK_AT_COORD, Owner, PoopPos.X, PoopPos.Y, PoopPos.Z, 2500, 0, 2);
                Now = St.Decide;
                _stepAt = now;
                _sub = 0;
            }
        }

        private Vector3 PoopPos => _poop != null && _poop.Exists() ? _poop.Position : Dog.Position;

        private void PlacePoop()
        {
            // behind the dog, on the ground
            Vector3 at = Dog.GetOffsetPosition(new Vector3(0f, Big ? -0.42f : -0.24f, 0f));
            at.Z = PoopBag.GroundZ(at + new Vector3(0f, 0f, 0.8f), at.Z - 0.4f);
            Model m = Big ? DogCfg.PoopModelBig : DogCfg.PoopModelSmall;
            if (!m.IsInCdImage || !m.Request(800)) return;
            _poop = World.CreateProp(m, at, false, false);
            if (_poop == null || !_poop.Exists()) { _poop = null; return; }
            _poop.Heading = Dog.Heading;
            Function.Call(Hash.PLACE_OBJECT_ON_GROUND_PROPERLY, _poop);
            Function.Call(Hash.FREEZE_ENTITY_POSITION, _poop, true);
        }

        // ---- the owner's choice ------------------------------------------------------------------------------------

        private void Decide(DogWalkers mod, int now)
        {
            if (now - _stepAt < 1300) return;
            bool pick = _poop != null && DogCfg.Rng.NextDouble() < DogCfg.PickUpChance;
            if (!pick)
            {
                DogCfg.Trace("  owner " + Owner.Handle + " leaves it");
                if (_poop != null) mod.LeavePoop(_poop);
                _poop = null;
                Resume(St.Walking);
                return;
            }
            // stand just short of it, on the owner's side
            Vector3 p = _poop.Position;
            Vector3 dir = Owner.Position - p;
            dir.Z = 0f;
            dir = dir.Length() > 0.05f ? dir.Normalized : -Owner.ForwardVector;
            _spot = p + dir * DogCfg.PickUpReach;
            float heading = Heading(_spot, p);
            Function.Call(Hash.TASK_GO_STRAIGHT_TO_COORD, Owner, _spot.X, _spot.Y, _spot.Z, 1f, 10000, heading, 0.05f);
            Now = St.Approach;
            _stepAt = now;
        }

        private void Approach(DogWalkers mod, int now)
        {
            int t = now - _stepAt;
            if (_poop == null || !_poop.Exists()) { Resume(St.Walking); return; }
            Vector3 o = Owner.Position;
            float d = new Vector2(o.X - _spot.X, o.Y - _spot.Y).Length();
            bool there = d < 0.35f || (t > 1500 && d < 0.8f && Owner.Velocity.Length() < 0.15f);
            if (!there)
            {
                if (t > 10000)
                {
                    DogCfg.Trace("  owner " + Owner.Handle + " couldn't get to it");
                    mod.LeavePoop(_poop);
                    _poop = null;
                    Resume(St.Walking);
                }
                return;
            }
            Vector3 p = _poop.Position;
            Function.Call(Hash.TASK_TURN_PED_TO_FACE_COORD, Owner, p.X, p.Y, p.Z, 500);
            Now = St.Pickup;
            _stepAt = now;
            _sub = 0;
        }

        private void Pickup(DogWalkers mod, int now)
        {
            int t = now - _stepAt;
            if (_sub == 0)
            {
                if (t < 500) return;
                if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, PickDict))
                {
                    Function.Call(Hash.REQUEST_ANIM_DICT, PickDict);
                    if (t < 3000) return;
                    // no pickup clip: the bag just turns up in their hand
                    Grab(mod);
                    Resume(St.Carrying);
                    return;
                }
                Play(Owner, PickDict, PickClip, 0);
                _sub = 1;
                _stepAt = now;
                return;
            }
            bool playing = Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Owner, PickDict, PickClip, 3);
            float at = playing ? Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, Owner, PickDict, PickClip) : 1f;
            if (_sub == 1 && (at >= DogCfg.GrabAt || (!playing && t > 400) || t > 3000))
            {
                Grab(mod);
                _sub = 2;
            }
            if (_sub == 2 && (at >= 0.97f || (!playing && t > 400) || t > 4000))
                Resume(St.Carrying);
        }

        private void Grab(DogWalkers mod)
        {
            if (_poop != null && _poop.Exists()) _poop.Delete();
            _poop = null;
            Bag = PoopBag.InHand(Owner);
            DogCfg.Trace("  owner " + Owner.Handle + " bagged it" + (Bag == null ? " (no bag model: " + DogCfg.BagModelName + ")" : ""));
        }

        // ---- after -----------------------------------------------------------------------------------------------

        private void Resume(St next)
        {
            if (Owner.Exists() && Owner.IsAlive)
            {
                Function.Call(Hash.TASK_CLEAR_LOOK_AT, Owner);
                Function.Call(Hash.TASK_WANDER_STANDARD, Owner, 10f, 10);
                Function.Call(Hash.SET_PED_KEEP_TASK, Owner, true);
            }
            Follow();
            Hold(false);
            Now = next;
        }

        private void Follow()
        {
            if (!Dog.Exists() || Dog.IsDead || !Owner.Exists()) return;
            Function.Call(Hash.TASK_FOLLOW_TO_OFFSET_OF_ENTITY, Dog, Owner, 0.6f, -0.9f, 0f, 1f, -1, 1.4f, true);
            Function.Call(Hash.SET_PED_KEEP_TASK, Dog, true);
            _following = true;
        }

        // A dog we sent after its owner stays after them (petting, a bark at the player, a car going by).
        private void KeepFollowing(Ped player, int now)
        {
            if (!_following || now < _followCheckAt) return;
            _followCheckAt = now + 2000;
            if (player.Position.DistanceTo(Dog.Position) < 2.5f) return;        // being petted, maybe: leave it
            if (Function.Call<bool>(Hash.IS_PED_IN_COMBAT, Dog, 0) || Function.Call<bool>(Hash.IS_PED_FLEEING, Dog)) return;
            int status = Function.Call<int>(Hash.GET_SCRIPT_TASK_STATUS, Dog, FollowTask);
            if (status != 0 && status != 1) Follow();
        }

        private void Hold(bool on)
        {
            if (on == _held) return;
            _held = on;
            foreach (Ped p in new[] { Owner, Dog })
            {
                if (p == null || !p.Exists()) continue;
                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, p, on);   // they stay held either way
            }
        }

        /// <summary>The player hurt one of them (or they're gone): the bag drops, and the game takes over.</summary>
        private void Spook(DogWalkers mod)
        {
            PoopFx.Stop(_fx);
            _fx = 0;
            if (Bag != null && Bag.Exists && Bag.Now == PoopBag.State.Held && (Owner == null || !Owner.Exists()))
            {
                Bag.Delete();                                       // the game cleaned them up, out of sight
            }
            else if (Bag != null && Bag.Exists && Bag.Now == PoopBag.State.Held && DogCfg.BurstInHand)
            {
                // GTA VI's clip: punched, the bag bursts in their hand and the poo flies everywhere
                mod.BurstInHand(Bag, Owner);
            }
            else if (Bag != null && Bag.Exists && Bag.Now == PoopBag.State.Held)
            {
                var r = DogCfg.Rng;
                Bag.Drop(new Vector3((float)(r.NextDouble() * 2 - 1) * 1.2f, (float)(r.NextDouble() * 2 - 1) * 1.2f, 2.2f));
                mod.AddLoose(Bag);
                DogCfg.Trace("owner " + (Owner != null ? Owner.Handle : 0) + " dropped the bag");
            }
            if (Owner != null && Owner.Exists() && !Owner.IsDead && Hurt(Owner, Game.Player.Character)) mod.FightBack(Owner);
            Bag = null;
            if (_poop != null) mod.LeavePoop(_poop);
            _poop = null;
            Hold(false);
            mod.KeepUntilFar(Owner);
            mod.KeepUntilFar(Dog);
            Now = St.Gone;
        }

        /// <summary>Far away or the script is stopping: everything of ours goes, the pair goes back to the game.</summary>
        public void Release(DogWalkers mod, bool deleteAll)
        {
            PoopFx.Stop(_fx);
            _fx = 0;
            if (Bag != null) Bag.Delete();
            Bag = null;
            if (_poop != null)
            {
                if (deleteAll) { if (_poop.Exists()) _poop.Delete(); }
                else mod.LeavePoop(_poop);
            }
            _poop = null;
            Hold(false);
            LetGo(Owner);
            LetGo(Dog);
            Now = St.Gone;
        }

        internal static void LetGo(Ped p)
        {
            if (p == null || !p.Exists()) return;
            DogWalkers.HeldDogs.Remove(p.Handle);
            p.MarkAsNoLongerNeeded();
        }

        internal static float Heading(Vector3 from, Vector3 to)
        {
            float dx = to.X - from.X, dy = to.Y - from.Y;
            return (float)(Math.Atan2(-dx, dy) * 180.0 / Math.PI);
        }
    }

    /// <summary>
    /// The people in this (dog walkers, anyone hit with poop) take a punch or two and stay in it, as the owner in
    /// GTA VI's clip does: more health (ToughHealth, against GTA's 200 for a local; 100 is dead), no critical hits,
    /// no knockouts the game decides on its own (CPED_CONFIG_FLAG_ForceScriptControlledKnockout, 114) and no writhing
    /// on the ground (DisableGoToWritheWhenInjured, 281). They still stagger and fall (ragdolls, Euphoria Ragdoll
    /// Overhaul's too) and get back up. Once a ped: doing it again would heal them.
    /// </summary>
    internal static class Tough
    {
        private sealed class Entry { public Ped Ped; public int Punches, LastPunch; public bool Proof; }
        private static readonly Dictionary<int, Entry> Peds = new Dictionary<int, Entry>();
        private static bool _wasSwinging;
        private static int _swingTarget, _swingLandsAt, _pruneAt;

        // The first PunchesBeforeHurt punches do no harm at all (melee-proof), and with
        // CPED_CONFIG_FLAG_AllowMeleeReactionIfMeleeProofIsOn (404) they still take the hit: they reel, stagger,
        // Euphoria Ragdoll Overhaul's reactions and all. A beating past that hurts as GTA's does.
        private static void Proof(Ped p, bool melee) =>
            Function.Call(Hash.SET_ENTITY_PROOFS, p, false, false, false, false, melee, false, false, false);

        /// <summary>AutoTest, when the game's melee task never lands: a punch on them as Tick counts one, and a shove.</summary>
        public static void SimulatePunch(Ped p, Ped player)
        {
            if (p == null || !p.Exists()) return;
            Make(p);
            if (!Peds.TryGetValue(p.Handle, out Entry e)) return;
            e.Punches++;
            e.LastPunch = Game.GameTime;
            Vector3 push = p.Position - player.Position;
            push.Z = 0f;
            if (push.Length() > 0.01f) push.Normalize();
            Function.Call(Hash.SET_PED_TO_RAGDOLL, p, 600, 900, 0, false, false, false);
            Function.Call(Hash.APPLY_FORCE_TO_ENTITY, p, 1, push.X * 6f, push.Y * 6f, 1f, 0f, 0f, 0f, 0, false, true, true, false, true);
            DogCfg.Trace("punch " + e.Punches + " on " + p.Handle + " (simulated)");
        }

        /// <summary>The player's fist landed on them within `ms` (a melee-proof hit does no damage to see).</summary>
        public static bool Punched(Ped p, int ms) =>
            p != null && Peds.TryGetValue(p.Handle, out Entry e) && e.LastPunch != 0 && Game.GameTime - e.LastPunch <= ms;

        /// <summary>Every frame: the player's punches at the people here, counted.</summary>
        public static void Tick(Ped player, int now)
        {
            if (Peds.Count == 0) return;
            bool swinging = Function.Call<bool>(Hash.IS_PED_PERFORMING_MELEE_ACTION, player);
            if (swinging && !_wasSwinging)
            {
                int target = Function.Call<int>(Hash.GET_MELEE_TARGET_FOR_PED, player);
                if (target != 0 && Peds.ContainsKey(target)) { _swingTarget = target; _swingLandsAt = now + 200; }
            }
            _wasSwinging = swinging;
            if (_swingTarget != 0 && now >= _swingLandsAt)
            {
                if (Peds.TryGetValue(_swingTarget, out Entry e) && e.Ped.Exists() && e.Ped.Position.DistanceTo(player.Position) < 2.4f)
                {
                    e.Punches++;
                    e.LastPunch = now;
                    if (e.Proof && e.Punches >= DogCfg.PunchesBeforeHurt) { e.Proof = false; Proof(e.Ped, false); }
                    DogCfg.Trace("punch " + e.Punches + " on " + e.Ped.Handle + (e.Proof ? " (no harm)" : ""));
                }
                _swingTarget = 0;
            }
            if (now >= _pruneAt)
            {
                _pruneAt = now + 3000;
                var gone = new List<int>();
                foreach (var kv in Peds) if (!kv.Value.Ped.Exists() || kv.Value.Ped.IsDead) gone.Add(kv.Key);
                foreach (int k in gone) Peds.Remove(k);
            }
        }

        public static void Make(Ped ped)
        {
            if (!DogCfg.Toughen || ped == null || !ped.Exists() || ped.IsDead || !ped.IsHuman) return;
            if (Peds.ContainsKey(ped.Handle)) return;
            var entry = new Entry { Ped = ped, Proof = DogCfg.PunchesBeforeHurt > 0 };
            Peds[ped.Handle] = entry;
            if (entry.Proof)
            {
                Proof(ped, true);
                Function.Call(Hash.SET_PED_CONFIG_FLAG, ped, 404, true);   // AllowMeleeReactionIfMeleeProofIsOn
            }
            int max = Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, ped);
            int now = Function.Call<int>(Hash.GET_ENTITY_HEALTH, ped);
            if (max < DogCfg.ToughHealth)
            {
                // the same share of health they had, out of the new maximum (100 is dead for a ped)
                float share = max > 100 ? Num.Clamp01((now - 100f) / (max - 100f)) : 1f;
                Function.Call(Hash.SET_ENTITY_MAX_HEALTH, ped, DogCfg.ToughHealth);
                Function.Call(Hash.SET_ENTITY_HEALTH, ped, 100 + (int)((DogCfg.ToughHealth - 100) * share), 0);
            }
            Function.Call(Hash.SET_PED_SUFFERS_CRITICAL_HITS, ped, false);
            Function.Call(Hash.SET_PED_CONFIG_FLAG, ped, 114, true);   // ForceScriptControlledKnockout
            Function.Call(Hash.SET_PED_CONFIG_FLAG, ped, 281, true);   // DisableGoToWritheWhenInjured
        }
    }

    /// <summary>
    /// The drunk driver's puke (re_drunkdriver.c's scr_puke_in_car from the head, 31086). Rockstar loads it as that
    /// script's own effects asset, which has no name in the script, so the likely assets are tried in turn and the one
    /// that works is kept (logged once with Debug).
    /// </summary>
    internal static class PukeFx
    {
        private static readonly (string asset, string fx)[] Tries =
        {
            ("core", "scr_puke_in_car"), ("scr_familyscenem", "scr_puke_in_car"), ("scr_familyscenem", "scr_trev_amb_puke"),
            ("scr_trevor1", "scr_trev_amb_puke"), ("scr_trevor2", "scr_trev_amb_puke"),
        };
        private static int _works = -1;
        private static bool _warned;

        public static void Request()
        {
            foreach (var t in Tries) Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, t.asset);
        }

        public static void OnHead(Ped ped)
        {
            Request();
            for (int i = 0; i < Tries.Length; i++)
            {
                int k = _works >= 0 ? _works : i;
                var t = Tries[k];
                if (Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, t.asset))
                {
                    Function.Call(Hash.USE_PARTICLE_FX_ASSET, t.asset);
                    if (Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_ON_PED_BONE, t.fx, ped, 0f, 0f, 0f, 0f, 0f, 0f, 31086, 1f, false, false, false))
                    {
                        if (_works < 0) DogCfg.Trace("puke effect: " + t.fx + " in " + t.asset);
                        _works = k;
                        return;
                    }
                }
                if (_works >= 0) return;          // the one that worked before isn't loaded this moment
            }
            if (!_warned) { _warned = true; Log.Write("DogWalkers", "no puke effect found (" + Tries.Length + " tried); the retch plays without it"); }
        }
    }

    /// <summary>
    /// The snowball's own impact puff, brown: mpchristmas2's explosionfx.dat gives EXP_VFXTAG_SNOWBALL as
    /// exp_grd_snowball (on the ground) and exp_air_snowball (in the air); it's fired here, tinted, where a thrown bag
    /// bursts, with a splat on what it hit. Returns a looped effect to stop if it had to fall back to Chop's poo.
    /// </summary>
    internal static class PoopPoof
    {
        private static bool _told, _warned;

        public static void Request() => Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "core");

        private static bool Fire(string fx, Vector3 at, float scale)
        {
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "core")) return false;
            Function.Call(Hash.USE_PARTICLE_FX_ASSET, "core");
            Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_COLOUR, 0.36f, 0.22f, 0.09f);
            return Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, fx, at.X, at.Y, at.Z, 0f, 0f, 0f, scale, false, false, false);
        }

        public static int At(Vector3 at, Vector3 normal, bool onPed)
        {
            Request();
            float ground = PoopBag.GroundZ(at + new Vector3(0f, 0f, 0.5f), at.Z - 5f);
            bool low = !onPed && at.Z - ground < 0.4f;
            string fx = low ? "exp_grd_snowball" : "exp_air_snowball";
            bool ok = Fire(fx, at, 1.2f) || Fire(low ? "exp_air_snowball" : "exp_grd_snowball", at, 1.2f);
            if (ok && !_told) { _told = true; DogCfg.Trace("brown snowball puff from core: " + fx); }
            int fallback = 0;
            if (!ok)
            {
                if (!_warned) { _warned = true; Log.Write("DogWalkers", "the snowball puff isn't in core here: Chop's poo instead"); }
                fallback = PoopFx.At(at, 2.2f);
            }
            if (!onPed) PoopSplat.Surface(at, normal, DogCfg.Rng);
            return fallback;
        }
    }

    /// <summary>
    /// The native Poo Bag's burst: the snowball's ground or air puff, browned and renamed exp_grd_poobag /
    /// exp_air_poobag, from the weapon's own effects (proj_vihud_poobag, in the vihudanims pack). Only that asset has
    /// these names, so what plays is brown. Without it: the core puff tinted brown (PoopPoof).
    /// </summary>
    internal static class PoobagPoof
    {
        public const string Asset = "proj_vihud_poobag";
        private static bool _told, _warned;

        /// <summary>The last burst's puff came from proj_vihud_poobag (not the tinted core one).</summary>
        public static bool LastBrown;

        public static void Request() => Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, Asset);

        public static int At(Vector3 at, Vector3 normal, bool onPed)
        {
            Request();
            float ground = PoopBag.GroundZ(at + new Vector3(0f, 0f, 0.5f), at.Z - 5f);
            bool low = !onPed && at.Z - ground < 0.4f;
            string fx = low ? "exp_grd_poobag" : "exp_air_poobag";
            bool loaded = Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, Asset);
            bool ok = false;
            if (loaded)
            {
                Function.Call(Hash.USE_PARTICLE_FX_ASSET, Asset);
                ok = Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, fx, at.X, at.Y, at.Z, 0f, 0f, 0f, 1f, false, false, false);
            }
            LastBrown = ok;
            if (ok)
            {
                if (!_told) { _told = true; DogCfg.Trace("brown puff from " + Asset + ": " + fx); }
                if (!onPed) PoopSplat.Surface(at, normal, DogCfg.Rng);
                return 0;
            }
            if (!_warned)
            {
                _warned = true;
                Log.Write("DogWalkers", Asset + (loaded ? " has no " + fx : " isn't loaded") + ": the core puff, tinted, instead");
            }
            return PoopPoof.At(at, normal, onPed);
        }
    }

    /// <summary>The bag's brown spray: the drunk driver's puke (scr_puke_in_car, in scr_familyscenem), browned, four ways.</summary>
    internal static class PoopSpray
    {
        private const string Asset = "scr_familyscenem", Fx = "scr_puke_in_car";
        private static bool _warned;

        public static void At(Vector3 at, Vector3 normal, Random rng)
        {
            Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, Asset);
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, Asset))
            {
                if (!_warned) { _warned = true; DogCfg.Trace("brown spray: " + Asset + " not loaded yet"); }
                return;
            }
            float yaw0 = (float)(rng.NextDouble() * 360.0);
            for (int i = 0; i < 4; i++)
            {
                Function.Call(Hash.USE_PARTICLE_FX_ASSET, Asset);
                Function.Call(Hash.SET_PARTICLE_FX_NON_LOOPED_COLOUR, 0.34f, 0.2f, 0.07f);
                float pitch = 20f + (float)rng.NextDouble() * 35f;      // up and out
                Function.Call<bool>(Hash.START_PARTICLE_FX_NON_LOOPED_AT_COORD, Fx, at.X, at.Y, at.Z + 0.05f,
                    pitch, 0f, yaw0 + i * 90f, 1.5f, false, false, false);
            }
        }
    }

    /// <summary>A poop splat on the ground: GTA's blood splatter decal (1010), browned.</summary>
    internal static class PoopSplat
    {
        /// <summary>On whatever was hit, facing out of it (a wall, a car, the ground).</summary>
        public static void Surface(Vector3 at, Vector3 normal, Random rng)
        {
            if (normal.Length() < 0.1f) normal = new Vector3(0f, 0f, 1f);
            normal.Normalize();
            Vector3 side = Math.Abs(normal.Z) > 0.9f ? new Vector3(1f, 0f, 0f) : Vector3.Cross(normal, new Vector3(0f, 0f, 1f));
            side.Normalize();
            float a = (float)(rng.NextDouble() * Math.PI * 2);
            Vector3 up2 = Vector3.Cross(normal, side);
            Vector3 s2 = side * (float)Math.Cos(a) + up2 * (float)Math.Sin(a);
            float size = 0.4f + (float)rng.NextDouble() * 0.25f;
            Vector3 p = at + normal * 0.05f;
            Function.Call<int>(Hash.ADD_DECAL, 1010, p.X, p.Y, p.Z, -normal.X, -normal.Y, -normal.Z, s2.X, s2.Y, s2.Z,
                size, size, 0.32f, 0.21f, 0.08f, 1f, 180f, false, false, false);
        }

        public static void Ground(Vector3 at, Random rng)
        {
            float a = (float)(rng.NextDouble() * Math.PI * 2);
            float size = 0.45f + (float)rng.NextDouble() * 0.25f;
            Function.Call<int>(Hash.ADD_DECAL, 1010, at.X, at.Y, at.Z + 0.05f, 0f, 0f, -1f, (float)Math.Cos(a), (float)Math.Sin(a), 0f,
                size, size, 0.32f, 0.21f, 0.08f, 1f, 180f, false, false, false);
        }
    }

    /// <summary>chop.c's poo effect, ent_anim_dog_poo: from the core asset, else Chop's own.</summary>
    internal static class PoopFx
    {
        private const string Effect = "ent_anim_dog_poo";
        private static readonly string[] Assets = { "core", "scr_amb_chop" };
        private static string _works;
        private static bool _warned;

        public static void Request()
        {
            foreach (string a in Assets) Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, a);
        }

        private static int Try(Func<int> start)
        {
            foreach (string a in _works != null ? new[] { _works } : Assets)
            {
                if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, a)) continue;
                Function.Call(Hash.USE_PARTICLE_FX_ASSET, a);
                int h = start();
                if (h != 0) { _works = a; return h; }
            }
            if (!_warned) { _warned = true; Log.Write("DogWalkers", "no " + Effect + " effect found (core, scr_amb_chop)"); }
            return 0;
        }

        public static int OnEntity(Entity e, Vector3 off, float scale) =>
            Try(() => Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_ON_ENTITY, Effect, e, off.X, off.Y, off.Z, 0f, 0f, 0f, scale, false, false, false));

        public static int At(Vector3 p, float scale) =>
            Try(() => Function.Call<int>(Hash.START_PARTICLE_FX_LOOPED_AT_COORD, Effect, p.X, p.Y, p.Z, 0f, 0f, 0f, scale, false, false, false, false));

        public static void Stop(int h)
        {
            if (h != 0 && Function.Call<bool>(Hash.DOES_PARTICLE_FX_LOOPED_EXIST, h)) Function.Call(Hash.STOP_PARTICLE_FX_LOOPED, h, false);
        }
    }
}
