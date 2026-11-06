using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// A hands-free run of the dog walkers and the Poo Bag, for testing with nobody at the controls
    /// ([DogWalkers] AutoTest = True; off by default). Once the game is up it takes the player to Legion Square at
    /// noon (invincible, no police, no ragdoll), and goes through it all as a player would:
    ///
    ///   1. the pack: WEAPON_POOBAG and its own model are in the game, and the Poo Bag is in GTA6Hud's wheel;
    ///   2. someone in front of the player gets a dog, which poops; the owner bags it;
    ///   3. the player punches the owner (the game's own melee, given as a task; a simulated punch if that never
    ///      lands): the full bag drops;
    ///   4. the player picks it up (the E press is simulated): a Poo Bag in hand;
    ///   5. it's thrown (TASK_THROW_PROJECTILE; else the game fires one from the hand) at the owner, at a bystander,
    ///      at a car and at the ground: each bursts there, brown, from the Poo Bag's own effects; a person hit retches
    ///      on their feet; the car gets dirtier.
    ///
    /// Every check is written to ViExtras.log as "[DogTest] PASS ..." or "[DogTest] FAIL ...", with a total at the
    /// end (also in the feed). Then everything is put back: the player's invincibility, ragdoll and wanted level; the
    /// test car and the people it made go a little later. Runs once per game session.
    /// </summary>
    public sealed partial class DogWalkers
    {
        private bool _autoTest, _autoDone, _autoPress;
        private int _atStep = -1, _atStepAt, _atReadyAt, _atPass, _atFail, _atThrows, _atTries, _atCleanAt;
        private Walker _atWalker;
        private Ped _atOwner, _atBystander, _atMadeOwner;
        private Vehicle _atCar;
        private float _atDirt;
        private bool _atRealPunch;
        private readonly List<string> _atLines = new List<string>();
        private (int ped, bool vehicle, bool world, bool brown, int at) _atBurst;

        // Legion Square, the middle of the plaza: open pavement, people about, no road
        private static readonly Vector3 TestSpot = new Vector3(195.2f, -934.0f, 30.7f);
        private const float TestHeading = 150f;

        private void AutoTestInit()
        {
            _autoTest = Config.Get(DogCfg.Section, "AutoTest", false);
            if (!_autoTest) return;
            DogCfg.Debug = true;             // every step in the log
            DogCfg.PickUpChance = 1f;        // the owner always bags it
            Log.Write("DogTest", "AutoTest is on: the test starts once the game is up (hands off)");
        }

        private void AtCheck(bool ok, string what)
        {
            if (ok) _atPass++; else _atFail++;
            string line = (ok ? "PASS " : "FAIL ") + what;
            _atLines.Add(line);
            Log.Write("DogTest", line);
        }

        private void AtNext(int now, int step)
        {
            _atStep = step;
            _atStepAt = now;
            _atTries = 0;
        }

        /// <summary>A burst of a thrown Poo Bag (RidesTick): what it hit, and whether the puff was the brown one.</summary>
        internal void AtBurst(Ped hitPed, Entity what, bool struck)
        {
            if (!_autoTest || _atBurst.at != 0) return;     // only the first burst after a throw
            _atBurst = (hitPed != null ? hitPed.Handle : 0, what is Vehicle, hitPed == null && !(what is Vehicle), PoobagPoof.LastBrown, Game.GameTime);
        }

        internal void AtThrown()
        {
            if (_autoTest) _atThrows++;
        }

        private void AtFace(Ped player, Vector3 at)
        {
            player.Heading = Walker.Heading(player.Position, at);
        }

        private void AtPut(Ped player, Vector3 at, Vector3 lookAt)
        {
            float z = PoopBag.GroundZ(at + new Vector3(0f, 0f, 1.0f), at.Z);
            Function.Call(Hash.SET_ENTITY_COORDS, player, at.X, at.Y, z + 0.05f, false, false, false, false);
            AtFace(player, lookAt);
        }

        // A throw at `target`: the game's own throw task, else one fired from the hand (both are seen as a throw).
        private int _atThrowSub, _atThrowsBefore;

        private bool AtThrow(Ped player, Vector3 target, int now, int sinceStep)
        {
            if (_atThrowSub == 0)
            {
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, PoobagWeapon, true);
                if (Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, player, PoobagWeapon) < 3)
                    Function.Call(Hash.SET_PED_AMMO, player, PoobagWeapon, 10, false);
                AtFace(player, target);
                _atThrowsBefore = _atThrows;
                _atBurst = (0, false, false, false, 0);
                Function.Call(Hash.TASK_THROW_PROJECTILE, player, target.X, target.Y, target.Z, 0, false);
                _atThrowSub = 1;
                return false;
            }
            if (_atThrowSub == 1)
            {
                if (_atThrows > _atThrowsBefore)
                {
                    Function.Call(Hash.CLEAR_PED_TASKS, player);     // the task throws on and on otherwise (5 Nov run)
                    Log.Write("DogTest", "  thrown by the game's throw task");
                    _atThrowSub = 3;
                    return true;
                }
                if (sinceStep < 3500) return false;
                // the task didn't throw: one fired from the hand, at the Poo Bag's own speed (25 m/s)
                Function.Call(Hash.CLEAR_PED_TASKS, player);
                Vector3 hand = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, player, 57005, 0f, 0f, 0f);
                Vector3 dir = target - hand;
                float dist = dir.Length();
                dir.Normalize();
                // from 3 m short of the target (or the hand, if it's closer): a moving target got missed from 6 m (5 Nov run)
                Vector3 from = dist > 3.6f ? target - dir * 3f : hand + dir * 0.6f;
                Function.Call(Hash.SHOOT_SINGLE_BULLET_BETWEEN_COORDS, from.X, from.Y, from.Z, target.X, target.Y, target.Z,
                    0, true, PoobagWeapon, player, true, false, 25f);
                Log.Write("DogTest", "  the throw task didn't throw: one fired from the hand");
                _atThrowSub = 2;
                return false;
            }
            if (_atThrowSub == 2)
            {
                if (_atThrows > _atThrowsBefore) { _atThrowSub = 3; return true; }
                return false;
            }
            return true;
        }

        private static Ped AtMakePed(string model, Vector3 at, float heading)
        {
            var m = new Model(model);
            if (!m.IsInCdImage || !m.Request(3000)) return null;
            float z = PoopBag.GroundZ(at + new Vector3(0f, 0f, 1.0f), at.Z);
            Ped p = World.CreatePed(m, new Vector3(at.X, at.Y, z), heading);
            m.MarkAsNoLongerNeeded();
            return p;
        }

        private bool StandingRetch(Ped p, out string how)
        {
            how = "";
            if (p == null || !p.Exists()) return false;
            bool loop = Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, p, Reaction.VomitDict, Reaction.VomitClip, 3);
            bool exit = Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, p, Reaction.VomitDict, Reaction.VomitExit, 3);
            if (!loop && !exit) return false;
            Vector3 pel = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, p, 11816, 0f, 0f, 0f);
            Vector3 lf = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, p, 14201, 0f, 0f, 0f);   // SKEL_L_Foot
            Vector3 rf = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, p, 52301, 0f, 0f, 0f);   // SKEL_R_Foot
            float g = PoopBag.GroundZ(p.Position + new Vector3(0f, 0f, 0.5f), p.Position.Z - 1f);
            float hip = pel.Z - g, foot = Math.Min(lf.Z, rf.Z) - g;
            float t = Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, p, Reaction.VomitDict, loop ? Reaction.VomitClip : Reaction.VomitExit);
            how = "hips " + hip.ToString("0.00") + " m, lowest foot " + foot.ToString("0.00") + " m above the ground, at " + t.ToString("0.00") + " of " + (loop ? "the loop" : "the exit");
            if (_atRetchSeen == 0) _atRetchSeen = Game.GameTime;
            if (hip < 0.6f) _atRetchLow = true;
            return Game.GameTime - _atRetchSeen > 1500;          // well into it, bent over
        }

        private string _atRetchHow = "";
        private int _atRetchSeen;
        private bool _atRetchLow;
        private int _atStageMask, _atStagePed;

        // which parts of the reaction (Reaction.Stage) were seen on `p`, and that they never sat or lay down for it
        private bool _atDown;

        private void AtStages(Ped p)
        {
            if (p == null || !p.Exists()) return;
            if (_atStagePed != p.Handle) { _atStagePed = p.Handle; _atStageMask = 0; _atDown = false; }
            foreach (Reaction r in _reactions) if (r.Ped.Handle == p.Handle) _atStageMask |= 1 << r.Stage;
            Vector3 pel = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, p, 11816, 0f, 0f, 0f);
            float g = PoopBag.GroundZ(p.Position + new Vector3(0f, 0f, 0.5f), p.Position.Z - 1f);
            if (pel.Z - g < 0.55f || Function.Call<bool>(Hash.IS_PED_RAGDOLL, p)) _atDown = true;
        }

        private void AtStagesCheck(string who)
        {
            AtCheck((_atStageMask & (1 << 10)) != 0, who + " recoiled (reaction@male_stand@big_intro)");
            AtCheck((_atStageMask & (1 << 1)) != 0, who + " looked down at their clothes in disgust (try_shirt_negative)");
            AtCheck(!_atDown, who + " stayed on their feet the whole time (no fall)");
        }

        private void AutoTestTick(Ped player, int now)
        {
            if (!_autoTest || _autoDone) return;
            if (_atStep == -1)
            {
                // the game up, the player about, the HUD's start-up done
                if (Game.IsLoading || player.IsDead || HudBridge.Paused) { _atReadyAt = 0; return; }
                if (_atReadyAt == 0) { _atReadyAt = now; return; }
                if (now - _atReadyAt < 12000) return;
                Feed.Post("DogWalkers test: starting, hands off (about 2 minutes)");
                Log.Write("DogTest", "==== start: " + (_native ? "native Poo Bag" : _haveWeapon ? "Poo Bag on the snowball model" : "no Poo Bag weapon"));
                Function.Call(Hash.SET_PLAYER_INVINCIBLE, Game.Player, true);
                Function.Call(Hash.SET_PED_CAN_RAGDOLL, player, false);
                Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player, true);
                Function.Call(Hash.SET_MAX_WANTED_LEVEL, 0);
                Game.Player.WantedLevel = 0;
                Function.Call(Hash.SET_CLOCK_TIME, 12, 0, 0);
                Function.Call(Hash.CLEAR_PED_TASKS_IMMEDIATELY, player);
                Function.Call(Hash.SET_ENTITY_COORDS, player, TestSpot.X, TestSpot.Y, TestSpot.Z, false, false, false, false);
                player.Heading = TestHeading;
                Function.Call(Hash.REQUEST_COLLISION_AT_COORD, TestSpot.X, TestSpot.Y, TestSpot.Z);
                AtNext(now, 0);
                return;
            }
            int t = now - _atStepAt;
            switch (_atStep)
            {
                case 0:   // settle in, then the pack
                    if (t < 5000) return;
                    AtCheck(_haveWeapon, "WEAPON_POOBAG is in the game");
                    AtCheck(_native, "its own model w_ex_poobag is in the game");
                    AtCheck(_inWheel, "the Poo Bag is in GTA6Hud's weapon wheel");
                    Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, PoobagPoof.Asset);
                    AtNext(now, 1);
                    return;

                case 1:   // someone in front gets a dog
                {
                    Vector3 front = player.Position + player.ForwardVector * 6f;
                    _atWalker = TrySpawn(player, now, true);
                    if (_atWalker == null || _atWalker.Owner.Position.DistanceTo(player.Position) > 15f)
                    {
                        _atMadeOwner = AtMakePed("a_m_y_hipster_01", front, player.Heading + 180f);
                        if (_atMadeOwner != null) _atWalker = GiveDog(_atMadeOwner, now);
                    }
                    AtCheck(_atWalker != null, "a dog walker" + (_atWalker == null ? "" : _atMadeOwner != null ? " (a person made for it)" : " (a local)"));
                    if (_atWalker == null) { AtNext(now, 90); return; }
                    _atOwner = _atWalker.Owner;
                    _pooped.Remove(_atWalker.Dog.Handle);
                    _atWalker.PoopNow(now + 1500);
                    AtNext(now, 2);
                    return;
                }

                case 2:   // it squats
                    if (_atWalker.Now == Walker.St.Squat) { AtCheck(true, "the dog squats"); AtNext(now, 3); return; }
                    if (!_atWalker.Alive) { AtCheck(false, "the dog squats (the walker's gone)"); AtNext(now, 90); return; }
                    if (t > 8000) { AtCheck(false, "the dog squats (it never did: on the road? too far?)"); AtNext(now, 90); }
                    return;

                case 3:   // the owner bags it
                    if (_atWalker.Now == Walker.St.Carrying && _atWalker.Bag != null) { AtCheck(true, "the owner bagged it and carries it"); AtNext(now, 4); return; }
                    if (_atWalker.Now == Walker.St.Gone || !_atWalker.Alive) { AtCheck(false, "the owner bags it (the walker's gone)"); AtNext(now, 90); return; }
                    if (t > 50000) { AtCheck(false, "the owner bags it (state " + _atWalker.Now + " after 50 s)"); AtNext(now, 90); }
                    return;

                case 4:   // the player walks up and punches them
                    if (_atTries == 0)
                    {
                        Vector3 o = _atOwner.Position;
                        AtPut(player, o + _atOwner.ForwardVector * 1.1f, o);
                        // fists only (whatever the save has in hand), then the game's own fight
                        Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, Unarmed, true);
                        Function.Call(Hash.TASK_COMBAT_PED, player, _atOwner, 0, 16);
                        _atTries = 1;
                        return;
                    }
                    if (_atWalker.Now == Walker.St.Gone || Tough.Punched(_atOwner, 3000))
                    {
                        _atRealPunch = _atTries == 1;
                        Function.Call(Hash.CLEAR_PED_TASKS, player);
                        AtNext(now, 5);
                        return;
                    }
                    if (_atTries == 1 && t > 7000)
                    {
                        // the game's melee task never landed: the punch as the script counts one
                        Function.Call(Hash.CLEAR_PED_TASKS, player);
                        Tough.SimulatePunch(_atOwner, player);
                        Log.Write("DogTest", "  the melee task didn't land a punch: a simulated one");
                        _atTries = 2;
                        return;
                    }
                    if (t > 12000) { Function.Call(Hash.CLEAR_PED_TASKS, player); AtCheck(false, "the punch drops the bag (no punch)"); AtNext(now, 90); }
                    return;

                case 5:   // the bag drops, full
                {
                    PoopBag dropped = null;
                    foreach (PoopBag b in _loose) if (b.Exists && !b.IsLump) { dropped = b; break; }
                    if (dropped != null && dropped.Now == PoopBag.State.Lying)
                    {
                        AtCheck(true, "punched" + (_atRealPunch ? " (the game's melee)" : " (simulated)") + ", the owner dropped the full bag, and it landed");
                        AtCheck(_atOwner.Exists() && !_atOwner.IsDead, "the owner is still alive after the punch");
                        AtNext(now, 6);
                        return;
                    }
                    if (t > 6000) { AtCheck(false, "the bag drops and lands (" + (dropped == null ? "none loose" : "state " + dropped.Now) + ")"); AtNext(now, 90); }
                    return;
                }

                case 6:   // the player picks it up (E)
                {
                    bool have = Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, player, PoobagWeapon, false)
                                && Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, player, PoobagWeapon) > 0
                                && Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, player) == PoobagWeapon;
                    if (have && t > 300) { AtCheck(true, "picked up: a Poo Bag in the player's hand"); AtNext(now, 7); return; }
                    if (t >= _atTries * 1500 && _atTries < 5 && _picking == null)
                    {
                        PoopBag bag = null;
                        foreach (PoopBag b in _loose) if (b.Exists && b.Now == PoopBag.State.Lying) { bag = b; break; }
                        if (bag != null)
                        {
                            Vector3 c = bag.Centre;
                            Vector3 away = player.Position - c; away.Z = 0f;
                            if (away.Length() < 0.1f) away = new Vector3(1f, 0f, 0f);
                            away.Normalize();
                            AtPut(player, c + away * 0.7f, c);
                            _autoPress = true;
                        }
                        _atTries++;
                    }
                    if (t > 9000) { AtCheck(false, "picked up (no Poo Bag in hand)"); AtNext(now, 90); }
                    return;
                }

                case 7:   // thrown at the owner
                {
                    if (t < 2500) return;          // let them get up and come on
                    if (!_atOwner.Exists() || _atOwner.IsDead) { AtCheck(false, "a throw at the owner (they're gone)"); AtNext(now, 9); return; }
                    Vector3 chest = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, _atOwner, 24818, 0f, 0f, 0f);
                    if (_atThrowSub == 0 && player.Position.DistanceTo(_atOwner.Position) > 9f) AtPut(player, _atOwner.Position + _atOwner.ForwardVector * 6f, _atOwner.Position);
                    if (!AtThrow(player, chest, now, t - 2500)) { if (t > 12000) { AtCheck(false, "a throw at the owner (nothing thrown)"); _atThrowSub = 0; AtNext(now, 9); } return; }
                    AtNext(now, 8);
                    return;
                }

                case 8:   // it burst on them, brown; they retch on their feet
                    if (_atBurst.at != 0 && _atBurst.ped == 0 && !_atBurst.vehicle && t > 2500)
                    {
                        AtCheck(false, "it hit the owner (it burst on the world instead)");
                        AtCheck(_atBurst.brown, "the puff is the Poo Bag's own brown one");
                        _atThrowSub = 0; AtNext(now, 9); return;
                    }
                    if (_atBurst.ped == _atOwner.Handle && _atTries == 0)
                    {
                        AtCheck(true, "it hit the owner and burst");
                        AtCheck(_atBurst.brown, "the puff is the Poo Bag's own brown one (proj_vihud_poobag)");
                        _atTries = 1;
                    }
                    if (_atTries == 1)
                    {
                        AtStages(_atOwner);
                        if (StandingRetch(_atOwner, out string how)) { AtCheck(!_atRetchLow, "the owner retches on their feet: " + how); AtStagesCheck("the owner"); _atRetchSeen = 0; _atRetchLow = false; _atThrowSub = 0; AtNext(now, 9); return; }
                        if (how != "") _atRetchHow = how;
                    }
                    if (t > 18000)
                    {
                        if (_atTries == 0) AtCheck(false, "it hit the owner (no burst seen)");
                        else AtCheck(false, "the owner retches on their feet (" + (_atRetchHow == "" ? "no retch clip played" : _atRetchHow) + ")");
                        _atThrowSub = 0; AtNext(now, 9);
                    }
                    return;

                case 9:   // a bystander
                {
                    if (_atTries == 0)
                    {
                        // back to the open middle of the square, and a passer-by (a woman: her own clips) made in front,
                        // facing the player, standing still (one found nearby was behind something, 5 Nov run)
                        Function.Call(Hash.CLEAR_PED_TASKS, player);
                        AtPut(player, TestSpot, TestSpot + new Vector3(0f, 1f, 0f));
                        player.Heading = TestHeading;
                        _atBystander = AtMakePed("a_f_y_tourist_01", player.Position + player.ForwardVector * 6f, player.Heading + 180f);
                        if (_atBystander == null) { AtCheck(false, "a bystander to throw at"); AtNext(now, 11); return; }
                        Function.Call(Hash.TASK_STAND_STILL, _atBystander, 15000);
                        _atTries = 1;
                        _atThrowSub = 0;
                        return;
                    }
                    if (t < 800) return;
                    Vector3 chest = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, _atBystander, 24818, 0f, 0f, 0f);
                    if (!AtThrow(player, chest, now, t - 800)) { if (t > 10000) { AtCheck(false, "a throw at a bystander (nothing thrown)"); _atThrowSub = 0; AtNext(now, 11); } return; }
                    AtNext(now, 10);
                    return;
                }

                case 10:
                    if (_atBurst.ped == _atBystander.Handle && _atTries == 0)
                    {
                        AtCheck(true, "it hit a bystander and burst");
                        _atTries = 1;
                    }
                    if (_atTries == 1) AtStages(_atBystander);
                    if (_atTries == 1 && StandingRetch(_atBystander, out string how2)) { AtCheck(!_atRetchLow, "the bystander retches on their feet: " + how2); AtStagesCheck("the bystander"); _atRetchSeen = 0; _atRetchLow = false; _atThrowSub = 0; AtNext(now, 11); return; }
                    if (t > 18000)
                    {
                        AtCheck(false, _atTries == 0 ? "it hit a bystander (" + (_atBurst.at == 0 ? "no burst" : "burst elsewhere") + ")" : "the bystander retches on their feet");
                        _atThrowSub = 0; AtNext(now, 11);
                    }
                    return;

                case 11:  // a car
                {
                    if (_atTries == 0)
                    {
                        var m = new Model("blista");
                        if (m.IsInCdImage && m.Request(3000))
                        {
                            Vector3 at = player.Position + player.RightVector * 7f;
                            at.Z = PoopBag.GroundZ(at + new Vector3(0f, 0f, 1.5f), player.Position.Z);
                            _atCar = World.CreateVehicle(m, at, player.Heading);
                            m.MarkAsNoLongerNeeded();
                        }
                        if (_atCar == null || !_atCar.Exists()) { AtCheck(false, "a car to throw at"); AtNext(now, 13); return; }
                        Function.Call(Hash.SET_VEHICLE_DIRT_LEVEL, _atCar, 0f);
                        _atDirt = 0f;
                        _atTries = 1;
                        _atThrowSub = 0;
                        return;
                    }
                    if (t < 1500) return;
                    Vector3 side = _atCar.Position + new Vector3(0f, 0f, 0.5f);
                    if (!AtThrow(player, side, now, t - 1500)) { if (t > 10000) { AtCheck(false, "a throw at a car (nothing thrown)"); _atThrowSub = 0; AtNext(now, 13); } return; }
                    AtNext(now, 12);
                    return;
                }

                case 12:
                    if (_atBurst.at != 0 && t > 300)
                    {
                        float dirt = Function.Call<float>(Hash.GET_VEHICLE_DIRT_LEVEL, _atCar);
                        AtCheck(_atBurst.vehicle, "it burst on the car" + (_atBurst.vehicle ? "" : " (it burst on " + (_atBurst.ped != 0 ? "a person" : "the world") + ")"));
                        AtCheck(dirt > _atDirt + 1f, "the car got dirtier (" + _atDirt.ToString("0.0") + " to " + dirt.ToString("0.0") + ")");
                        _atThrowSub = 0; AtNext(now, 13); return;
                    }
                    if (t > 6000) { AtCheck(false, "it burst on the car (no burst)"); _atThrowSub = 0; AtNext(now, 13); }
                    return;

                case 13:  // the ground
                {
                    if (t < 1000) return;
                    Vector3 g = player.Position - player.RightVector * 6f + player.ForwardVector * 1f;
                    g.Z = PoopBag.GroundZ(g + new Vector3(0f, 0f, 1.5f), player.Position.Z - 1f);
                    if (!AtThrow(player, g, now, t - 1000)) { if (t > 10000) { AtCheck(false, "a throw at the ground (nothing thrown)"); _atThrowSub = 0; AtNext(now, 90); } return; }
                    AtNext(now, 14);
                    return;
                }

                case 14:
                    if (_atBurst.at != 0 && t > 300)
                    {
                        AtCheck(_atBurst.world, "it burst on the ground");
                        AtCheck(_atBurst.brown, "the puff on the ground is the brown one");
                        AtNext(now, 90); return;
                    }
                    if (t > 6000) { AtCheck(false, "it burst on the ground (no burst)"); AtNext(now, 90); }
                    return;

                case 90:  // the total, and everything put back
                {
                    string total = "DogWalkers test: " + _atPass + " of " + (_atPass + _atFail) + " passed";
                    Log.Write("DogTest", "==== " + total + (_atFail == 0 ? "" : " (failed: " + string.Join("; ", _atLines.FindAll(l => l.StartsWith("FAIL")).ConvertAll(l => l.Substring(5))) + ")"));
                    Feed.Post(total + (_atFail == 0 ? "" : ". The log has what failed."));
                    Function.Call(Hash.CLEAR_PED_TASKS, player);
                    Function.Call(Hash.SET_PLAYER_INVINCIBLE, Game.Player, false);
                    Function.Call(Hash.SET_PED_CAN_RAGDOLL, player, true);
                    Function.Call(Hash.SET_POLICE_IGNORE_PLAYER, Game.Player, false);
                    Function.Call(Hash.SET_MAX_WANTED_LEVEL, 5);
                    _atCleanAt = now + 20000;
                    AtNext(now, 91);
                    return;
                }

                case 91:  // a while later the test car and the people it made go
                    if (now < _atCleanAt) return;
                    if (_atCar != null && _atCar.Exists()) _atCar.MarkAsNoLongerNeeded();
                    if (_atMadeOwner != null && _atMadeOwner.Exists()) _atMadeOwner.MarkAsNoLongerNeeded();
                    if (_atBystander != null && _atBystander.Exists() && Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, _atBystander)) _atBystander.MarkAsNoLongerNeeded();
                    _autoDone = true;
                    return;
            }
        }
    }
}
