using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras.NpcMemory
{
    /// <summary>
    /// um_memory/client/interact.lua: RDR2's Greet / Antagonize, walk-up style.
    /// Get close to someone and look at them and the prompt comes up on them.
    ///
    ///   Greet       a wave, salute, thumbs-up or peace sign; close up to someone
    ///               who likes you, a handshake, fist bump or high five, and a hug
    ///               for a real friend. They answer by mood. Hidden for someone
    ///               you've just antagonized.
    ///   Antagonize  escalates on the same person: bump, feint, shadow-box. Brave
    ///               people and gangs square up and swing; timid ones run. Cops
    ///               warn you twice, then it's obstruction.
    ///   Defuse      replaces Greet while they're squaring up. Always works before
    ///               blows land; mid-fight it's a chance.
    ///   Intimidate  on the witness running to report you.
    ///
    /// On FiveM these were osm-target options on vice_hud's world prompt; with no
    /// target system um_memory fell back to its own prompt and keys, and that
    /// fallback is what this is: the nearest person inside the range and the
    /// camera cone, in line of sight, drawn on vice_hud's world prompt at their
    /// head. Honor is paid the RDR2 way (HonorApi.Report) and priced by Honor.
    /// </summary>
    internal static class Interact
    {
        private const int HeadBone = 31086;
        private static readonly Vector3 HeadOffset = new Vector3(0f, 0f, 0.35f);

        // vice_hud style.css #world-actions, --wa-scale 0.7, in canvas px (1cqh = 10.8):
        private const float WaScale = 0.7f;
        private const float ButtonToWord = 0.8f * 10.8f * WaScale;   // .wa-row gap 0.8cqh
        private const float RowGap = 0.9f * 10.8f * WaScale;         // #world-actions gap 0.9cqh
        private const float CompactDistance = 1.5003f;                  // client_overlays.lua WA_COMPACT_DISTANCE

        private static int _nextPlayerAct;
        private static int _nextScan;
        private static Ped _target;
        private static readonly List<Option> _options = new List<Option>();
        private static bool _greetPressed, _antagonizePressed;

        private enum Verb { Greet, Defuse, Antagonize, Intimidate }

        private struct Option
        {
            public Verb Verb;
            public string Label;
            public bool First;   // on the first key (Greet's), or the second (Antagonize's)
        }

        /// <summary>The prompt is up on someone (other ports can stand their own greet down).</summary>
        public static bool PromptUp => _target != null;

        // Windows repeats KeyDown while a key is held: one press is one action, as on FiveM's key
        // mapping (holding U used to escalate Antagonize to a fight in seconds).
        private static readonly HashSet<Keys> Held = new HashSet<Keys>();

        public static void OnKeyDown(Keys key)
        {
            if (!Held.Add(key)) return;
            if (!MemCfg.Interact) return;
            if (MemCfg.GreetKey != Keys.None && key == MemCfg.GreetKey) _greetPressed = true;
            if (MemCfg.AntagonizeKey != Keys.None && key == MemCfg.AntagonizeKey) _antagonizePressed = true;
        }

        public static void OnKeyUp(Keys key) => Held.Remove(key);

        // ---- who can be talked to ------------------------------------------------------

        private static bool AimHeld() =>
            Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 0, 25) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, 25)
            || (!Glyphs.UsingPad && Game.IsKeyPressed(Keys.RButton));

        /// <summary>
        /// GTA6Hud's own stranger talk (hold aim on someone, then GREET / TAUNT)
        /// has the person while aim is held: this prompt stands aside.
        /// </summary>
        private static bool YieldToGta6Hud() => MemCfg.YieldToConversations && OtherScripts.NpcConversations && AimHeld();

        public static bool PlayerCanInteract(Ped player)
        {
            if (UM.InVehicle(player) || Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, player, true)) return false;
            if (Function.Call<bool>(Hash.IS_PED_RAGDOLL, player) || Function.Call<bool>(Hash.IS_PED_SWIMMING, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_CLIMBING, player) || Function.Call<bool>(Hash.IS_PED_CUFFED, player)) return false;
            if (Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_RUNNING_MOBILE_PHONE_TASK, player)) return false;
            if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen || OtherScripts.ScreenTaken) return false;
            return true;
        }

        /// <summary>
        /// Cops can be greeted and antagonized (MemCfg.Cops), except while you're
        /// wanted: then GTA6Hud's police have them, and its SURRENDER is on them.
        /// </summary>
        private static bool Eligible(Ped ped)
        {
            if (UM.InVehicle(ped)) return false;
            if (UM.IsCop(ped)) return MemCfg.Cops && UM.Wanted == 0;
            return true;
        }

        private static bool CanTalk(Ped ped) =>
            UM.IsPerson(ped) && Eligible(ped) && !Witness.IsWitness(ped) && PlayerCanInteract(UM.Player);

        private static bool CanIntimidate(Ped ped) =>
            MemCfg.IntimidatePrompt && Witness.IsWitness(ped) && UM.IsPerson(ped) && PlayerCanInteract(UM.Player);

        private static bool AntagonizedRecently(Ped ped)
        {
            Person r = Mem.Peek(ped);
            return r != null && r.Antagonized && UM.Now - r.AntagonizedAt < MemCfg.HideGreetAfterAntagonizeMs;
        }

        private static bool CanGreet(Ped ped) => CanTalk(ped) && !Attitude.IsHostile(ped) && !AntagonizedRecently(ped);
        private static bool CanDefuse(Ped ped) => CanTalk(ped) && Attitude.IsHostile(ped);

        // ---- shared bits ---------------------------------------------------------------------

        /// <summary>Player and per-person cooldowns; the person's record when allowed.</summary>
        private static Person Ready(Ped ped)
        {
            int now = UM.Now;
            if (now < _nextPlayerAct) return null;
            Person r = Mem.Of(ped);
            if (now < r.NextAct) return null;
            _nextPlayerAct = now + MemCfg.PlayerCooldownMs;
            r.NextAct = now + MemCfg.PedCooldownMs;
            return r;
        }

        /// <summary>RDR2 only plays your gesture unarmed, at walking pace, roughly facing them.</summary>
        private static bool CanGesture(Ped player, Ped ped)
        {
            if (UM.Armed(player, 7) || Function.Call<float>(Hash.GET_ENTITY_SPEED, player) > 2.5f) return false;
            Vector3 fwd = Function.Call<Vector3>(Hash.GET_ENTITY_FORWARD_VECTOR, player);
            Vector3 to = ped.Position - player.Position;
            float len = (float)Math.Sqrt(to.X * to.X + to.Y * to.Y);
            if (len < 0.01f) return true;
            return (fwd.X * to.X + fwd.Y * to.Y) / len >= 0f; // within 90 degrees
        }

        private static int Gesture(Ped player, Ped ped, string[] names) => CanGesture(player, ped) ? Anims.Play(player, names) : 0;

        // RDR2's honor rule (short_update func_753/754): each person counts once;
        // 3 different people within the window pays one tick; greeting and
        // antagonizing reset each other's streak; cops, gangs and criminals never count.
        private static string _streakKind;
        private static readonly List<int> _streakTimes = new List<int>();

        private static void HonorStreak(string kind, Ped ped, Person r)
        {
            if (UM.IsCop(ped) || UM.IsGang(ped)) return;
            if (!r.HonorCounted.Add(kind)) return;

            if (_streakKind != kind)
            {
                _streakKind = kind;
                _streakTimes.Clear();
            }
            int now = UM.Now;
            _streakTimes.RemoveAll(t => now - t > MemCfg.HonorStreakWindowMs);
            _streakTimes.Add(now);

            if (_streakTimes.Count >= MemCfg.HonorStreakCount)
            {
                _streakTimes.Clear();
                if (MemCfg.ReportHonor)
                {
                    bool applied = HonorApi.Report(kind == "greet" ? "greet_npc" : "antagonize_npc");
                    UM.Debug("honor " + kind + " streak reported, applied=" + applied);
                }
            }
        }

        /// <summary>Back to whatever an ambient ped does, unless they've since decided to run or fight.</summary>
        public static void Release(Ped ped, int ms)
        {
            Later.Run(ped, ms, () =>
            {
                if (UM.Fleeing(ped) || UM.InCombatWith(ped, UM.Player) || UM.InScenario(ped)) return;
                Function.Call(Hash.TASK_WANDER_STANDARD, ped, 10f, 10);
            });
        }

        public static void TurnTo(Ped ped, int ms)
        {
            if (UM.InScenario(ped)) UM.LookAt(ped, UM.Player, ms);
            else UM.TurnTo(ped, UM.Player, ms);
        }

        private static void StartFight(Ped ped, PedClass cls)
        {
            Person r = Mem.Of(ped);
            r.HostileUntil = UM.Now + 60000;
            UM.ClearTasks(ped);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 5, true);   // fight armed peds even when unarmed
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, true);  // always fight, never flee
            Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped, 0, false);
            if (cls == PedClass.Gang && UM.Roll() <= MemCfg.GangWeaponChance)
                Function.Call(Hash.GIVE_WEAPON_TO_PED, ped, Game.GenerateHash(UM.Pick(MemCfg.GangWeapons)), 24, false, true);
            UM.Say(ped, MemCfg.LinesChallenge, "SPEECH_PARAMS_FORCE_SHOUTED");
            UM.Combat(ped, UM.Player);
            Function.Call(Hash.SET_PED_KEEP_TASK, ped, true);
        }

        private static void SquareUp(Ped ped)
        {
            Person r = Mem.Of(ped);
            r.HostileUntil = UM.Now + 45000;
            TurnTo(ped, 2500);
            Later.Run(ped, 400, () => Anims.React(ped, "squareUp"));
        }

        private static void StandDown(Ped ped)
        {
            Person r = Mem.Of(ped);
            r.HostileUntil = 0;
            r.Stage = 0;
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, false);
            Function.Call(Hash.SET_PED_KEEP_TASK, ped, false);
            UM.ClearTasks(ped);
        }

        private static void Flee(Ped ped, float dist, int ms = -1)
        {
            UM.ClearTasks(ped);
            UM.Flee(ped, UM.Player, dist, ms);
        }

        // ---- cops ------------------------------------------------------------------------------
        // The Lua's "The officer knows exactly who you are" and the recognition
        // bump came from um_memory's police notoriety, which GTA6Hud's police
        // memory replaces here; the rest is as it was.

        private static void CopGreet(Ped ped, string band)
        {
            TurnTo(ped, 1500);
            if (band == Attitude.Warm || band == Attitude.Polite)
            {
                Anims.React(ped, "greetPolite");
                UM.Say(ped, MemCfg.LinesPolite);
            }
            else
            {
                Anims.React(ped, "greetCold");
                UM.Say(ped, MemCfg.LinesCold);
            }
            Release(ped, 2500);
        }

        private static void CopAntagonized(Ped ped, int stage)
        {
            Person r = Mem.Of(ped);
            TurnTo(ped, 2000);

            if (stage == 1)
            {
                Anims.React(ped, "copWarn");
                UM.Say(ped, MemCfg.LinesCopWarn);
            }
            else if (stage == 2)
            {
                Anims.React(ped, "copWarn");
                UM.Say(ped, MemCfg.LinesInsult);
                MemoryUi.Notify("Push it any further and you're going in.");
            }
            else
            {
                r.Stage = 0;
                Anims.React(ped, "copKnowsYou");
                UM.Say(ped, MemCfg.LinesCopSpot, "SPEECH_PARAMS_FORCE_SHOUTED");
                Law.RaiseWanted(MemCfg.CopObstructionStars);
                return;
            }
            Release(ped, 3000);
        }

        // ---- greet ------------------------------------------------------------------------------

        private static void Greet(Ped ped)
        {
            if (!CanGreet(ped)) return;
            Person r = Ready(ped);
            if (r == null) return;

            Ped player = UM.Player;
            Attitude.Of(ped, out string band, out Temper temper, out PedClass cls);
            bool remembered = r.Remembered;
            float priorOpinion = r.Opinion;
            int flags = r.Flags;
            HonorStreak("greet", ped, r);
            UM.SayPlayer(MemCfg.PlayerLinesGreet);

            // Someone who likes you, close up and not busy, takes your hand.
            int paired = 0;
            if (band == Attitude.Warm && cls != PedClass.Law && !UM.InScenario(ped) && CanGesture(player, ped))
                paired = Anims.Paired(player, ped, UM.Pick(r.Opinion >= 60f ? MemCfg.PlayerGreetFriend : MemCfg.PlayerGreetPaired));

            if (paired > 0)
            {
                if (remembered && priorOpinion >= 20f) MemoryUi.Notify("They remember you. Good to see you again.");
                Release(ped, paired + 300);
            }
            else
            {
                Gesture(player, ped, MemCfg.PlayerGreet);
                Later.Run(ped, MemCfg.ReactDelayMs, () =>
                {
                    if (cls == PedClass.Law)
                    {
                        CopGreet(ped, band);
                        return;
                    }

                    if (band == Attitude.Warm)
                    {
                        TurnTo(ped, 1500);
                        Anims.React(ped, remembered && priorOpinion >= 25f ? "greetFriend" : "greetWarm");
                        UM.Say(ped, MemCfg.LinesWarm);
                        if (remembered && priorOpinion >= 20f) MemoryUi.Notify("They remember you. Good to see you again.");
                        Release(ped, 2800);
                    }
                    else if (band == Attitude.Polite)
                    {
                        TurnTo(ped, 1000);
                        Anims.React(ped, "greetPolite");
                        UM.Say(ped, MemCfg.LinesPolite);
                        Release(ped, 2000);
                    }
                    else if (band == Attitude.Cold)
                    {
                        UM.LookAt(ped, UM.Player, 1500);
                        Anims.React(ped, "greetCold");
                        UM.Say(ped, MemCfg.LinesCold);
                    }
                    else if (temper == Temper.Timid)
                    {
                        Anims.Play(ped, "startle");
                        UM.Say(ped, MemCfg.LinesScared);
                        Later.Run(ped, 1200, () => Flee(ped, 25f, 6000));
                    }
                    else
                    {
                        TurnTo(ped, 1200);
                        Anims.React(ped, "greetHostile");
                        UM.Say(ped, MemCfg.LinesInsult);
                        Release(ped, 3000);
                    }

                    if (band == Attitude.Hostile && remembered && (flags & MemCfg.AllCrimeFlags) != 0)
                        MemoryUi.Notify("They remember what you did.");
                });
            }

            float delta = MemCfg.OpinionGreet;
            if (band == Attitude.Hostile) delta = (float)Math.Floor(delta / 2f);
            Mem.Touch(ped, delta);
            Mem.AddZoneVia(ped, "greet");
        }

        // ---- antagonize: bump, feint, shadow-box ----------------------------------------------

        private static void Bumped(Ped ped, Temper temper, PedClass cls)
        {
            Ped player = UM.Player;
            Anims.Shoved(ped, player);
            Later.Run(ped, 1200, () =>
            {
                if (temper == Temper.Timid)
                {
                    Anims.React(ped, "shovedTimid");
                    UM.Say(ped, MemCfg.LinesScared);
                    Later.Run(ped, 1300, () => Flee(ped, 15f, 4000));
                }
                else if (temper == Temper.Brave || cls == PedClass.Gang)
                {
                    TurnTo(ped, 1200);
                    // Some shove you straight back.
                    if (UM.Roll() <= 40 && ped.Position.DistanceTo(player.Position) <= 1.6f)
                    {
                        Anims.Play(ped, "shove");
                        Later.Run(player, 350, () => Anims.Shoved(player, ped));
                    }
                    else
                    {
                        Anims.React(ped, "shovedBrave");
                    }
                    UM.Say(ped, MemCfg.LinesInsult);
                    Release(ped, 3000);
                }
                else
                {
                    TurnTo(ped, 1200);
                    Anims.React(ped, "shovedAverage");
                    UM.Say(ped, MemCfg.LinesInsult);
                    Release(ped, 2500);
                }
            });
        }

        private static void Feinted(Ped ped, Temper temper, float score, PedClass cls)
        {
            if (temper == Temper.Timid)
            {
                Anims.React(ped, "feintTimid");
                UM.Say(ped, MemCfg.LinesScared, "SPEECH_PARAMS_FORCE_SHOUTED");
                Later.Run(ped, 1500, () => Flee(ped, 80f));
            }
            else if (temper == Temper.Brave || cls == PedClass.Gang || score < -20f)
            {
                Anims.React(ped, "feintBrave");
                UM.Say(ped, MemCfg.LinesChallenge, "SPEECH_PARAMS_FORCE_SHOUTED");
                SquareUp(ped);
            }
            else
            {
                Anims.React(ped, "feintAverage");
                UM.Say(ped, MemCfg.LinesScared);
                Later.Run(ped, 1500, () =>
                {
                    Anims.React(ped, "shovedAverage");
                    Release(ped, 2000);
                });
            }
        }

        private static void ShadowBoxed(Ped ped, Temper temper, float score, PedClass cls)
        {
            bool fights = temper == Temper.Brave || cls == PedClass.Gang || (temper == Temper.Average && score < -20f);
            if (fights)
            {
                Anims.React(ped, "squareUp");
                Later.Run(ped, 900, () => StartFight(ped, cls));
            }
            else if (temper == Temper.Timid)
            {
                UM.ClearTasks(ped);
                Function.Call(Hash.TASK_COWER, ped, 4000);
                UM.Say(ped, MemCfg.LinesScared, "SPEECH_PARAMS_FORCE_SHOUTED");
                Later.Run(ped, 4200, () => Flee(ped, 120f));
            }
            else
            {
                Anims.Play(ped, "startle");
                Later.Run(ped, 1300, () => Function.Call(Hash.TASK_REACT_AND_FLEE_PED, ped, UM.Player));
            }
        }

        private static void Antagonize(Ped ped)
        {
            if (!CanTalk(ped)) return;
            Person r = Ready(ped);
            if (r == null) return;

            Ped player = UM.Player;
            int now = UM.Now;
            if (now - r.StageAt > MemCfg.StageResetMs) r.Stage = 0;
            r.Stage = Math.Min(r.Stage + 1, 3);
            r.StageAt = now;
            r.Antagonized = true;
            r.AntagonizedAt = now;
            int stage = r.Stage;
            float score = Attitude.Of(ped, out _, out Temper temper, out PedClass cls);

            // Stage 1 is a shoulder bump, which needs them within arm's reach;
            // from further off it's a taunt instead.
            bool close = player.Position.DistanceTo(ped.Position) <= 1.6f;
            if (stage == 1 && close && CanGesture(player, ped)) Anims.Play(player, MemCfg.PlayerAntagonize[1]);
            else Gesture(player, ped, MemCfg.PlayerAntagonize[stage == 1 ? 2 : stage]);

            HonorStreak("antagonize", ped, r);
            UM.SayPlayer(MemCfg.PlayerLinesAntagonize[stage], stage == 3 ? "SPEECH_PARAMS_FORCE_SHOUTED" : "SPEECH_PARAMS_FORCE");
            Mem.Touch(ped, MemCfg.OpinionAntagonize);
            Mem.AddZoneVia(ped, "antagonize");
            Ambient.WatchLeave(ped);

            Later.Run(ped, stage == 1 && close ? 350 : MemCfg.ReactDelayMs, () =>
            {
                if (cls == PedClass.Law)
                {
                    CopAntagonized(ped, stage);
                    return;
                }
                if (stage == 1)
                {
                    if (close) Bumped(ped, temper, cls);
                    else Feinted(ped, temper, score, cls);
                }
                else if (stage == 2)
                {
                    Feinted(ped, temper, score, cls);
                    Ambient.Bystander(ped, stage);
                }
                else
                {
                    ShadowBoxed(ped, temper, score, cls);
                    Ambient.Bystander(ped, stage);
                }
            });
        }

        // ---- intimidate (the witness) -------------------------------------------------------

        private static void Intimidate(Ped ped)
        {
            if (!CanIntimidate(ped)) return;
            Anims.Play(UM.Player, MemCfg.PlayerIntimidate);
            Later.Run(ped, 500, () => Witness.Intimidate(ped));
        }

        // ---- defuse --------------------------------------------------------------------------------

        private static void Defuse(Ped ped)
        {
            if (!CanDefuse(ped)) return;
            Person r = Ready(ped);
            if (r == null) return;

            Ped player = UM.Player;
            Attitude.Of(ped, out _, out Temper temper, out PedClass cls);
            bool fighting = UM.InCombatWith(ped, player);
            Gesture(player, ped, fighting ? MemCfg.PlayerDefuseFighting : MemCfg.PlayerDefuse);

            bool success = true;
            if (fighting)
            {
                float chance = MemCfg.DefuseInFightChance + Attitude.Honor * 0.3f + r.Opinion * 0.2f;
                if (temper == Temper.Brave) chance -= 10f;
                else if (temper == Temper.Timid) chance += 15f;
                if (cls == PedClass.Gang) chance -= 10f;
                chance = Num.Clamp(chance, 5f, 95f);
                success = UM.Roll() <= chance;
                UM.Debug("defuse mid-fight chance " + (int)chance + "% -> " + (success ? "success" : "fail"));
            }

            Later.Run(ped, MemCfg.ReactDelayMs, () =>
            {
                if (success)
                {
                    StandDown(ped);
                    Anims.React(ped, "defuseAccept");
                    UM.Say(ped, MemCfg.LinesBackDown);
                    Release(ped, 2200);
                    Mem.Touch(ped, MemCfg.OpinionDefuse);
                    Mem.AddZoneVia(ped, "defuse");
                    MemoryUi.Notify("They back down.");
                }
                else
                {
                    Anims.React(ped, "defuseRefuse");
                    UM.Say(ped, MemCfg.LinesRefuse, "SPEECH_PARAMS_FORCE_SHOUTED");
                    MemoryUi.Notify("They aren't having it.");
                }
            });
        }

        // ============================================================================
        // The prompt: um_memory's own (findTarget / showOwn), on vice_hud's world prompt
        // ============================================================================

        /// <summary>
        /// The nearest person inside the range and the camera cone, in line of
        /// sight. The witness counts from further off (the osm option's own reach,
        /// MemCfg.IntimidatePromptRange), since Intimidate is offered on them.
        /// </summary>
        private static Ped FindTarget(Ped player)
        {
            Vector3 origin = player.Position;
            float reach = MemCfg.IntimidatePrompt ? Math.Max(MemCfg.Range, MemCfg.IntimidatePromptRange) : MemCfg.Range;
            Ped best = null;
            float bestScore = float.MaxValue;
            foreach (UM.Near e in UM.NearbyPeople(origin, reach, (p, d) => Eligible(p)))
            {
                float range = Witness.IsWitness(e.Ped) ? reach : MemCfg.Range;
                if (e.Dist > range) continue;
                float angle = UM.AngleFromCamera(origin, e.Ped.Position);
                if (angle > MemCfg.Cone) continue;
                float score = angle / MemCfg.Cone + e.Dist / range;
                if (score < bestScore)
                {
                    best = e.Ped;
                    bestScore = score;
                }
            }
            if (best != null && !UM.HasLos(player, best)) return null;
            return best;
        }

        private static void BuildOptions(Ped ped)
        {
            _options.Clear();
            if (CanIntimidate(ped))
            {
                _options.Add(new Option { Verb = Verb.Intimidate, Label = "intimidate", First = true });
                return;
            }
            if (CanDefuse(ped)) _options.Add(new Option { Verb = Verb.Defuse, Label = "defuse", First = true });
            else if (CanGreet(ped)) _options.Add(new Option { Verb = Verb.Greet, Label = "greet", First = true });
            _options.Add(new Option { Verb = Verb.Antagonize, Label = "antagonize", First = false });
        }

        public static void Hide()
        {
            _target = null;
            _options.Clear();
            _greetPressed = _antagonizePressed = false;
        }

        /// <summary>Every frame while Memory runs: find (every ScanMs), take the keys, draw.</summary>
        public static void Frame(Ped player)
        {
            if (!MemCfg.Interact) return;
            int now = UM.Now;
            if (now >= _nextScan)
            {
                _nextScan = now + MemCfg.ScanMs;
                Ped ped = !YieldToGta6Hud() && PlayerCanInteract(player) ? FindTarget(player) : null;
                if (ped == null) Hide();
                else
                {
                    _target = ped;
                    BuildOptions(ped);
                }
            }

            if (_target == null || !_target.Exists())
            {
                _greetPressed = _antagonizePressed = false;
                if (_target != null) Hide();
                return;
            }

            bool pad = Glyphs.UsingPad;
            if (pad)
            {
                // the game's own use of the same buttons waits while the prompt is up (osm-target's
                // blocks), or one press would greet and get you into a car, or antagonize and punch
                BlockShared(MemCfg.PadGreet);
                BlockShared(MemCfg.PadAntagonize);
            }
            bool first = _greetPressed, second = _antagonizePressed;
            _greetPressed = _antagonizePressed = false;
            if (pad && MemCfg.PadGreet >= 0)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, MemCfg.PadGreet, true);
                first |= Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, MemCfg.PadGreet);
            }
            if (pad && MemCfg.PadAntagonize >= 0)
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, MemCfg.PadAntagonize, true);
                second |= Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, MemCfg.PadAntagonize);
            }

            Ped target = _target;
            if (first)
            {
                if (CanIntimidate(target)) Intimidate(target);
                else if (CanDefuse(target)) Defuse(target);
                else Greet(target);
            }
            else if (second)
            {
                Antagonize(target);
            }

            if (_target != null) Draw(player, _target, pad);
        }

        // ---- drawing: vice_hud's #world-actions -------------------------------------------------

        private static string KeyToken(Keys k)
        {
            if (k >= Keys.A && k <= Keys.Z) return k.ToString();
            if (k >= Keys.D0 && k <= Keys.D9) return ((int)(k - Keys.D0)).ToString();
            if (k >= Keys.NumPad0 && k <= Keys.NumPad9) return "NUM " + (int)(k - Keys.NumPad0);
            if (k >= Keys.F1 && k <= Keys.F12) return "F" + (int)(k - Keys.F1 + 1);
            switch (k)
            {
                case Keys.Up: return "UP";
                case Keys.Down: return "DOWN";
                case Keys.Left: return "LEFT";
                case Keys.Right: return "RIGHT";
                case Keys.Space: return "SPACE";
                case Keys.Tab: return "TAB";
                case Keys.Return: return "ENTER";
                case Keys.Back: return "BKSP";
                case Keys.Delete: return "DEL";
                case Keys.Insert: return "INS";
                case Keys.Home: return "HOME";
                case Keys.End: return "END";
                case Keys.PageUp: return "PGUP";
                case Keys.PageDown: return "PGDN";
                case Keys.CapsLock: return "CAPS";
                case Keys.Escape: return "ESC";
                case Keys.ShiftKey: case Keys.LShiftKey: case Keys.RShiftKey: return "SHIFT";
                case Keys.ControlKey: case Keys.LControlKey: case Keys.RControlKey: return "CTRL";
                case Keys.Menu: case Keys.LMenu: case Keys.RMenu: return "ALT";
                case Keys.Add: return "NUM +";
                case Keys.Subtract: return "NUM -";
                case Keys.OemMinus: return "-";
                case Keys.Oemplus: return "=";
                case Keys.OemOpenBrackets: return "[";
                case Keys.OemCloseBrackets: return "]";
                case Keys.OemSemicolon: return ";";
                case Keys.OemQuotes: return "'";
                case Keys.Oemcomma: return ",";
                case Keys.OemPeriod: return ".";
                case Keys.OemQuestion: return "/";
                case Keys.OemBackslash: case Keys.OemPipe: return "\\";
                case Keys.Oemtilde: return "`";
                case Keys.LButton: return "LMB";
                case Keys.RButton: return "RMB";
                case Keys.MButton: return "MMB";
                case Keys.XButton1: return "MOUSE4";
                case Keys.XButton2: return "MOUSE5";
            }
            return null;
        }

        private static string FileName(string token)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in token.Replace(' ', '_'))
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c.ToString() : "x" + ((int)c).ToString("X2"));
            return sb.ToString();
        }

        // What else each pad button does on foot (osm-target's Config.Input blocks).
        private static readonly Dictionary<int, int[]> PadBlocks = new Dictionary<int, int[]>
        {
            [204] = new[] { 23, 75, 53, 56 },     // Y / Triangle: get in or out of a car, weapon special, drop weapon
            [203] = new[] { 22 },                 // X / Square: jump
            [194] = new[] { 45, 140, 141, 142 },  // B / Circle: reload and melee
        };

        private static void BlockShared(int padControl)
        {
            if (padControl < 0 || !PadBlocks.TryGetValue(padControl, out int[] blocks)) return;
            foreach (int c in blocks) Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, c, true);
        }

        /// <summary>
        /// The button for one option, as it is drawn inside the world prompt
        /// (wa_glyph_*). Each option shows the device its own button is on: the
        /// pad's when one is in hand and a pad control is set, otherwise the key.
        /// </summary>
        private static Art.Entry Button(Keys key, int padControl, bool pad)
        {
            if (padControl >= 0 && (pad || key == Keys.None))
            {
                Art.Entry shared = Glyphs.ForControl(padControl);
                if (shared != null) return Art.Get("wa_" + System.IO.Path.GetFileNameWithoutExtension(shared.Path));
            }
            string token = key != Keys.None ? KeyToken(key) : null;
            return token != null ? Art.Get("wa_glyph_kbm_" + FileName(token)) : null;
        }

        private static void Draw(Ped player, Ped ped, bool pad)
        {
            if (_options.Count == 0) return;
            Vector3 head = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, ped, HeadBone, 0f, 0f, 0f) + HeadOffset;
            var sx = new OutputArgument();
            var sy = new OutputArgument();
            // Behind the camera this frame: hidden, not closed (.wa-offscreen).
            if (!Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, head.X, head.Y, head.Z, sx, sy)) return;

            // The words drop away at range (.wa-compact), the buttons stay.
            bool compact = player.Position.DistanceTo(head) > CompactDistance;
            // The box's top-left on the point: left = x * 100cqw, top = y * 100cqh.
            float left = sx.GetResult<float>() * Canvas.W;
            float top = sy.GetResult<float>() * Canvas.H;

            foreach (Option o in _options)
            {
                Art.Entry button = o.First ? Button(MemCfg.GreetKey, MemCfg.PadGreet, pad) : Button(MemCfg.AntagonizeKey, MemCfg.PadAntagonize, pad);
                Art.Entry word = compact ? null : Art.Get("wa_label_" + o.Label);
                float bh = button != null ? button.BoxH : 0f;
                float wh = word != null ? word.BoxH : 0f;
                float rowH = Math.Max(bh, wh);   // .wa-row: align-items center

                float x = left;
                if (button != null)
                {
                    Art.Draw(button, x, top + (rowH - bh) * 0.5f, Color.White);
                    x += button.BoxW + ButtonToWord;
                }
                if (word != null) Art.Draw(word, x, top + (rowH - wh) * 0.5f, Color.White);
                top += rowH + RowGap;
            }
        }
    }
}
