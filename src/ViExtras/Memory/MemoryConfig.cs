using System.Collections.Generic;
using System.Windows.Forms;

namespace ViExtras.NpcMemory
{
    /// <summary>
    /// um_memory/config.lua for single player: [Memory] in ViExtras.ini for the
    /// numbers, and the animation, reaction and crime tables here in code, as
    /// they were in the Lua. Defaults are the FiveM config's values.
    ///
    /// Dropped with the police side (GTA6Hud has its own police memory): every
    /// Config.Police key, the notoriety per crime, Config.Gossip.minNotoriety,
    /// and the um_livingworld tier terms of Config.Attitude.
    /// </summary>
    internal static class MemCfg
    {
        private const string S = "Memory";

        // ---- general --------------------------------------------------------------
        public static bool Enabled, Debug, DuringMissions, NpcSpeech;
        public static Keys StandingKey;

        // ---- Config.Interact --------------------------------------------------------
        public static bool Interact, Cops, ReportHonor, YieldToConversations;
        public static Keys GreetKey, AntagonizeKey;
        public static int PadGreet, PadAntagonize;
        public static float Range, Cone;
        public static int ScanMs, PlayerCooldownMs, PedCooldownMs, ReactDelayMs, StageResetMs, HideGreetAfterAntagonizeMs;
        public static int DefuseInFightChance, GangWeaponChance, CopObstructionStars, HonorStreakCount, HonorStreakWindowMs;

        // ---- Config.Ambient ---------------------------------------------------------
        public static int AmbientScanMs;
        public static bool Follow, BodyBlock, TauntAsYouLeave, Bystander, WeaponSeen, MaskSeen;
        public static float FollowRange, FollowAngle, FollowCloseRange, FollowTouchRange;
        public static int FollowMeterMs, FollowCloseMs, FollowTouchMs, FollowEscalateMs;
        public static float BodyBlockRange, TauntRange, BystanderRange, WeaponSeenRange, MaskSeenRange;
        public static int BodyBlockMs, TauntWithinMs, BystanderChance, WeaponSeenCooldownMs;

        // ---- Config.Attitude --------------------------------------------------------
        public static float HonorWeight, ZoneWeight, FaceWeight, ArmedPenalty, WantedPenaltyPerStar, MaskedPenalty, NightPenalty;
        public static float RichHonorWeight, GangHonorWeight, BandWarm, BandPolite, BandCold;

        // ---- Config.Memory ----------------------------------------------------------
        public static int MaxFaces, MemoryScanMs, RecogniseChance, ReactCooldownMs, SaveMs;
        public static float ScanRadius, RecogniseRange, FaceHalfLifeDays, ZoneHalfLifeDays;
        public static bool ZoneReactions;
        public static int ZoneReactionChance, ZoneReactionCooldownMs;
        public static float ZoneLiked, ZoneDisliked;

        // ---- Config.Witness / Config.Gossip -----------------------------------------
        public static bool Witnesses, IntimidatePrompt, MaskBlocksIdentity, GangsReport, Bodies, Gossip;
        public static float WitnessRange, IntimidateRange, IntimidatePromptRange, BodyFindRange, BodyNearPlayerRange;
        public static int MaxWitnesses, ReportDelayMinMs, ReportDelayMaxMs, IntimidateMs, InvestigateMs, HeardStarsMax;
        public static int ReportedFeedMs, BrandishMs, BodyScanMs, BodyKeepMs, GossipStars;

        public static void Load()
        {
            Enabled = Config.Get(S, "Enabled", true);
            Debug = Config.Get(S, "Debug", false);
            DuringMissions = Config.Get(S, "DuringMissions", false);
            NpcSpeech = Config.Get(S, "NpcSpeech", false);
            StandingKey = Config.Get(S, "StandingKey", Keys.None);

            Interact = Config.Get(S, "Interact", true);
            // um_memory's own prompt was G / H; GTA6Hud has both in single player.
            GreetKey = Config.Get(S, "GreetKey", Keys.Y);
            AntagonizeKey = Config.Get(S, "AntagonizeKey", Keys.U);
            PadGreet = Config.Get(S, "PadGreet", 204);
            PadAntagonize = Config.Get(S, "PadAntagonize", 194);
            Range = Config.Get(S, "Range", 3.5f);
            Cone = Config.Get(S, "Cone", 30f);
            ScanMs = Config.Get(S, "ScanMs", 150);
            PlayerCooldownMs = Config.Get(S, "PlayerCooldownMs", 1500);
            PedCooldownMs = Config.Get(S, "PedCooldownMs", 3500);
            ReactDelayMs = Config.Get(S, "ReactDelayMs", 900);
            StageResetMs = Config.Get(S, "StageResetMs", 30000);
            HideGreetAfterAntagonizeMs = Config.Get(S, "HideGreetAfterAntagonizeMs", 60000);
            DefuseInFightChance = Config.Get(S, "DefuseInFightChance", 40);
            GangWeaponChance = Config.Get(S, "GangWeaponChance", 20);
            Cops = Config.Get(S, "Cops", true);
            CopObstructionStars = Config.Get(S, "CopObstructionStars", 1);
            ReportHonor = Config.Get(S, "ReportHonor", true);
            HonorStreakCount = Config.Get(S, "HonorStreakCount", 3);
            HonorStreakWindowMs = Config.Get(S, "HonorStreakWindowMs", 60000);
            YieldToConversations = Config.Get(S, "YieldToGta6HudConversations", true);

            AmbientScanMs = Config.Get(S, "AmbientScanMs", 500);
            Follow = Config.Get(S, "Follow", true);
            FollowRange = Config.Get(S, "FollowRange", 10f);
            FollowAngle = Config.Get(S, "FollowAngle", 45f);
            FollowMeterMs = Config.Get(S, "FollowMeterMs", 10000);
            FollowCloseRange = Config.Get(S, "FollowCloseRange", 5f);
            FollowCloseMs = Config.Get(S, "FollowCloseMs", 20000);
            FollowTouchRange = Config.Get(S, "FollowTouchRange", 1f);
            FollowTouchMs = Config.Get(S, "FollowTouchMs", 5000);
            FollowEscalateMs = Config.Get(S, "FollowEscalateMs", 8000);
            BodyBlock = Config.Get(S, "BodyBlock", true);
            BodyBlockRange = Config.Get(S, "BodyBlockRange", 1.5f);
            BodyBlockMs = Config.Get(S, "BodyBlockMs", 2500);
            TauntAsYouLeave = Config.Get(S, "TauntAsYouLeave", true);
            TauntRange = Config.Get(S, "TauntRange", 15f);
            TauntWithinMs = Config.Get(S, "TauntWithinMs", 15000);
            Bystander = Config.Get(S, "Bystander", true);
            BystanderRange = Config.Get(S, "BystanderRange", 20f);
            BystanderChance = Config.Get(S, "BystanderChance", 50);
            WeaponSeen = Config.Get(S, "WeaponSeen", true);
            WeaponSeenRange = Config.Get(S, "WeaponSeenRange", 10f);
            WeaponSeenCooldownMs = Config.Get(S, "WeaponSeenCooldownMs", 60000);
            MaskSeen = Config.Get(S, "MaskSeen", true);
            MaskSeenRange = Config.Get(S, "MaskSeenRange", 15f);

            HonorWeight = Config.Get(S, "HonorWeight", 0.45f);
            ZoneWeight = Config.Get(S, "ZoneWeight", 0.35f);
            FaceWeight = Config.Get(S, "FaceWeight", 0.8f);
            ArmedPenalty = Config.Get(S, "ArmedPenalty", 25f);
            WantedPenaltyPerStar = Config.Get(S, "WantedPenaltyPerStar", 12f);
            MaskedPenalty = Config.Get(S, "MaskedPenalty", 20f);
            NightPenalty = Config.Get(S, "NightPenalty", 8f);
            RichHonorWeight = Config.Get(S, "RichHonorWeight", 0.2f);
            GangHonorWeight = Config.Get(S, "GangHonorWeight", -0.35f);
            BandWarm = Config.Get(S, "BandWarm", 35f);
            BandPolite = Config.Get(S, "BandPolite", 0f);
            BandCold = Config.Get(S, "BandCold", -35f);

            MaxFaces = Config.Get(S, "MaxFaces", 250);
            MemoryScanMs = Config.Get(S, "MemoryScanMs", 2500);
            ScanRadius = Config.Get(S, "ScanRadius", 30f);
            RecogniseRange = Config.Get(S, "RecogniseRange", 18f);
            RecogniseChance = Config.Get(S, "RecogniseChance", 70);
            ReactCooldownMs = Config.Get(S, "ReactCooldownMs", 60000);
            FaceHalfLifeDays = Config.Get(S, "FaceHalfLifeDays", 6f);
            ZoneHalfLifeDays = Config.Get(S, "ZoneHalfLifeDays", 10f);
            SaveMs = Config.Get(S, "SaveMs", 15000);
            ZoneReactions = Config.Get(S, "ZoneReactions", true);
            ZoneReactionChance = Config.Get(S, "ZoneReactionChance", 10);
            ZoneLiked = Config.Get(S, "ZoneLiked", 45f);
            ZoneDisliked = Config.Get(S, "ZoneDisliked", -45f);
            ZoneReactionCooldownMs = Config.Get(S, "ZoneReactionCooldownMs", 90000);

            Witnesses = Config.Get(S, "Witnesses", true);
            WitnessRange = Config.Get(S, "WitnessRange", 35f);
            MaxWitnesses = Config.Get(S, "MaxWitnesses", 8);
            ReportDelayMinMs = Config.Get(S, "ReportDelayMinMs", 9000);
            ReportDelayMaxMs = Config.Get(S, "ReportDelayMaxMs", 15000);
            IntimidateRange = Config.Get(S, "IntimidateRange", 30f);
            IntimidateMs = Config.Get(S, "IntimidateMs", 1500);
            IntimidatePrompt = Config.Get(S, "IntimidatePrompt", true);
            IntimidatePromptRange = Config.Get(S, "IntimidatePromptRange", 6f);
            InvestigateMs = Config.Get(S, "InvestigateMs", 8000);
            HeardStarsMax = Config.Get(S, "HeardStarsMax", 1);
            ReportedFeedMs = Config.Get(S, "ReportedFeedMs", 4000);
            MaskBlocksIdentity = Config.Get(S, "MaskBlocksIdentity", true);
            GangsReport = Config.Get(S, "GangsReport", false);
            BrandishMs = Config.Get(S, "BrandishMs", 1200);
            Bodies = Config.Get(S, "Bodies", true);
            BodyFindRange = Config.Get(S, "BodyFindRange", 12f);
            BodyScanMs = Config.Get(S, "BodyScanMs", 2000);
            BodyKeepMs = Config.Get(S, "BodyKeepMs", 600000);
            BodyNearPlayerRange = Config.Get(S, "BodyNearPlayerRange", 80f);
            Gossip = Config.Get(S, "Gossip", true);
            GossipStars = Config.Get(S, "GossipStars", 1);

            foreach (KeyValuePair<string, Crime> c in Crimes)
                c.Value.Stars = Config.Get(S, Pascal(c.Key) + "Stars", c.Value.Stars);
        }

        private static string Pascal(string key)
        {
            var sb = new System.Text.StringBuilder();
            bool up = true;
            foreach (char ch in key)
            {
                if (ch == '_') { up = true; continue; }
                sb.Append(up ? char.ToUpperInvariant(ch) : ch);
                up = false;
            }
            return sb.ToString();
        }

        // =============================================================================
        // Body language (config.lua Config.AnimLibrary, every clip checked present in
        // GTA V's animation list by um_memory). gesture = gestures@m/f@standing@casual
        // (upper body, keeps walking); celebration = anim@mp_player_intcelebration
        // male@ / female@ <name> (salute has no female twin, so MaleOnly). Full stops
        // the ped for it.
        // =============================================================================
        internal sealed class Anim
        {
            public string Gesture, Celebration, Dict, FemaleDict;
            public string[] Clips;
            public bool Full, MaleOnly;
            public int Ms;
        }

        private static Anim G(string clip) => new Anim { Gesture = clip };
        private static Anim C(string name, int ms = 0, bool maleOnly = false) => new Anim { Celebration = name, Ms = ms, MaleOnly = maleOnly };

        public static readonly Dictionary<string, Anim> AnimLibrary = new Dictionary<string, Anim>
        {
            ["hello"] = G("gesture_hello"),
            ["what"] = G("gesture_what_hard"),
            ["shrug"] = G("gesture_shrug_hard"),
            ["nope"] = G("gesture_no_way"),
            ["easy"] = G("gesture_easy_now"),
            ["bring_it"] = G("gesture_bring_it_on"),
            ["point"] = G("gesture_point"),
            ["you"] = G("gesture_you_soft"),
            ["dismiss"] = G("gesture_hand_down"),
            ["damn"] = G("gesture_damn"),
            ["come_here"] = G("gesture_come_here_soft"),

            ["wave"] = C("wave"),
            ["salute"] = C("salute", maleOnly: true),
            ["thumbs_up"] = C("thumbs_up"),
            ["peace"] = C("peace"),
            ["bro_love"] = C("bro_love"),
            ["finger"] = C("finger"),
            ["you_loco"] = C("you_loco"),
            ["chin_brush"] = C("chin_brush"),
            ["cry_baby"] = C("cry_baby"),
            ["face_palm"] = C("face_palm"),
            ["no_way"] = C("no_way"),
            ["slow_clap"] = C("slow_clap"),
            ["shush"] = C("shush"),
            ["knuckle_crunch"] = C("knuckle_crunch"),
            ["shadow_boxing"] = C("shadow_boxing", 3000),
            ["surrender"] = C("surrender"),

            // The shove half of reaction@shove; the shoved half picks its side in code.
            ["shove"] = new Anim { Dict = "reaction@shove", Clips = new[] { "shove_var_a", "shove_var_b", "shove_var_c" }, Full = true, Ms = 1200 },
            ["startle"] = new Anim
            {
                Dict = "reaction@male_stand@big_intro@backward",
                Clips = new[] { "reaction_backward_big_intro_a", "reaction_backward_big_intro_b", "reaction_backward_big_intro_c" },
                Full = true, Ms = 1500,
            },
            ["flinch"] = new Anim
            {
                Dict = "amb@code_human_cower@male@react_cowering", FemaleDict = "amb@code_human_cower@female@react_cowering",
                Clips = new[] { "flinch" }, Full = true, Ms = 1500,
            },
            ["radio"] = new Anim { Dict = "random@arrests", Clips = new[] { "generic_radio_chatter" }, Ms = 5000 },
        };

        // Two-person animations (Config.PairedAnims). The NPC is lined up Offset
        // metres in front of you, only when already within PairedRange.
        internal sealed class Pair
        {
            public string Dict, A, B;
            public float Offset;
            public int Ms;
            public bool Gendered;
        }

        public const float PairedRange = 2.0f;

        public static readonly Dictionary<string, Pair> PairedAnims = new Dictionary<string, Pair>
        {
            ["handshake"] = new Pair { Dict = "mp_ped_interaction", A = "handshake_guy_a", B = "handshake_guy_b", Offset = 0.9f, Ms = 3000 },
            ["highfive"] = new Pair { Dict = "mp_ped_interaction", A = "highfive_guy_a", B = "highfive_guy_b", Offset = 0.9f, Ms = 2500 },
            ["hug"] = new Pair { Dict = "mp_ped_interaction", A = "hugs_guy_a", B = "hugs_guy_b", Offset = 1.14f, Ms = 3500 },
            ["fistbump"] = new Pair { Dict = "anim@mp_player_intcelebrationpaired@{0}_fist_bump", A = "fist_bump_left", B = "fist_bump_right", Offset = 1.0f, Ms = 2500, Gendered = true },
        };

        // What you do (Config.PlayerAnims).
        public static readonly string[] PlayerGreet = { "hello", "wave", "salute", "thumbs_up", "peace" };
        public static readonly string[] PlayerGreetPaired = { "handshake", "fistbump", "highfive" };
        public static readonly string[] PlayerGreetFriend = { "hug", "handshake" };
        public static readonly string[][] PlayerAntagonize =
        {
            null,
            new[] { "shove" },                                              // shoulder-bump them
            new[] { "knuckle_crunch", "chin_brush", "finger", "you_loco" }, // feint, make them flinch
            new[] { "shadow_boxing" },                                      // come on then
        };
        public static readonly string[] PlayerDefuse = { "easy", "surrender" };
        public static readonly string[] PlayerDefuseFighting = { "surrender" };
        public static readonly string[] PlayerIntimidate = { "knuckle_crunch", "point", "you" };

        // How they answer (Config.Reactions).
        public static readonly Dictionary<string, string[]> Reactions = new Dictionary<string, string[]>
        {
            ["greetWarm"] = new[] { "wave", "hello", "thumbs_up", "peace", "salute" },
            ["greetFriend"] = new[] { "bro_love", "wave", "thumbs_up" },
            ["greetPolite"] = new[] { "hello" },
            ["greetCold"] = new[] { "shrug", "dismiss", "nope" },
            ["greetHostile"] = new[] { "what", "finger", "you_loco", "no_way" },
            ["greetSnub"] = new[] { "dismiss", "nope" },

            ["shovedTimid"] = new[] { "startle" },
            ["shovedAverage"] = new[] { "what", "shrug", "face_palm" },
            ["shovedBrave"] = new[] { "what", "bring_it" },
            ["feintTimid"] = new[] { "flinch" },
            ["feintAverage"] = new[] { "startle" },
            ["feintBrave"] = new[] { "knuckle_crunch", "bring_it" },
            ["squareUp"] = new[] { "shadow_boxing", "bring_it" },

            ["defuseAccept"] = new[] { "shrug", "dismiss", "face_palm", "nope" },
            ["defuseRefuse"] = new[] { "finger", "bring_it" },

            ["copWarn"] = new[] { "point", "nope", "you" },
            ["copKnowsYou"] = new[] { "point", "you" },

            ["friendSpotsYou"] = new[] { "wave", "thumbs_up", "salute", "bro_love" },
            ["grudgeSpotsYou"] = new[] { "finger", "what", "no_way", "cry_baby", "slow_clap" },
            ["zoneLiked"] = new[] { "wave", "hello", "thumbs_up" },
            ["zoneDisliked"] = new[] { "nope", "dismiss", "finger" },

            ["tauntAsYouLeave"] = new[] { "what", "bring_it", "cry_baby", "you_loco" },
            ["toldOffFollower"] = new[] { "what", "nope", "you" },
            ["bodyBlocked"] = new[] { "what", "nope" },
            ["bystanderStepsIn"] = new[] { "easy", "nope", "point" },
            ["weaponSeenTimid"] = new[] { "startle", "nope" },
            ["weaponSeenBrave"] = new[] { "what", "easy" },
            ["maskSeen"] = new[] { "startle", "nope", "what" },
        };

        // NPC speech on top of the animations, off unless NpcSpeech (Config.Lines).
        public static readonly string[] LinesWarm = { "GENERIC_HOWS_IT_GOING", "GENERIC_HI", "GENERIC_THANKS" };
        public static readonly string[] LinesPolite = { "GENERIC_HI" };
        public static readonly string[] LinesCold = { "GENERIC_WHATEVER", "GENERIC_NO" };
        public static readonly string[] LinesInsult = { "GENERIC_INSULT_HIGH", "GENERIC_INSULT_MED", "GENERIC_CURSE_HIGH", "GENERIC_FUCK_YOU" };
        public static readonly string[] LinesScared = { "GENERIC_FRIGHTENED_HIGH", "GENERIC_FRIGHTENED_MED", "GENERIC_SHOCKED_HIGH" };
        public static readonly string[] LinesChallenge = { "CHALLENGE_THREATEN", "FIGHT", "GENERIC_WAR_CRY" };
        public static readonly string[] LinesBackDown = { "GENERIC_WHATEVER", "APOLOGY_NO_TROUBLE" };
        public static readonly string[] LinesRefuse = { "GENERIC_FUCK_YOU", "GENERIC_INSULT_HIGH" };
        public static readonly string[] LinesCallCops = { "PHONE_CALL_COPS", "CALL_COPS_THREAT" };
        public static readonly string[] LinesCopWarn = { "CRIMINAL_WARNING" };
        public static readonly string[] LinesCopSpot = { "SUSPECT_SPOTTED" };

        // Gang members you push to a fight may pull one of these.
        public static readonly string[] GangWeapons = { "WEAPON_KNIFE", "WEAPON_BAT", "WEAPON_PISTOL" };

        // =============================================================================
        // Memory: opinion of the one person involved, and the neighbourhood's
        // (each person counts once per kind of deed).
        // =============================================================================
        public const float OpinionGreet = 10, OpinionDefuse = 8, OpinionAntagonize = -16;
        public const float OpinionVictim = -70, OpinionWitness = -45, OpinionIntimidated = -35;

        public static readonly Dictionary<string, float> ZoneDelta = new Dictionary<string, float>
        {
            ["greet"] = 1, ["antagonize"] = -2, ["defuse"] = 1,
            ["assault"] = -6, ["shooting"] = -4, ["brandish"] = -2, ["carjack"] = -6,
            ["kill"] = -18, ["kill_cop"] = -14, ["robbery"] = -8, ["petty"] = -3,
            ["honest"] = 2,
        };

        // =============================================================================
        // Crimes (Config.Crimes, without notoriety). Stars are what a report is
        // worth; they only ever RAISE the wanted level.
        // =============================================================================
        internal sealed class Crime
        {
            public int Stars;
            public int Flag;
            public Crime(int stars, int flag) { Stars = stars; Flag = flag; }
        }

        public static readonly Dictionary<string, Crime> Crimes = new Dictionary<string, Crime>
        {
            ["assault"] = new Crime(1, 1),
            ["shooting"] = new Crime(2, 2),
            ["carjack"] = new Crime(2, 4),
            ["kill"] = new Crime(3, 8),
            ["kill_cop"] = new Crime(4, 16),
            ["robbery"] = new Crime(0, 32),
            ["brandish"] = new Crime(0, 64),
            ["petty"] = new Crime(0, 128),
        };

        public const int AllCrimeFlags = 1 | 2 | 4 | 8 | 16 | 32;
        public const int SeriousFlags = 8 | 16; // Config.Gossip.seriousFlags: kill / kill_cop

        // Where gunfire counts as shooting in public (fenix-police region keys).
        // Out in the countryside it's taken for hunting.
        public static readonly HashSet<string> ShootingRegions = new HashSet<string> { "losSantos", "sandyShores", "paletoBay" };

        // Honor hooks that also leave a mark on the neighbourhood (Config.Deeds).
        // complied_with_police only fed police notoriety, which is gone.
        public static readonly Dictionary<string, string> Deeds = new Dictionary<string, string>
        {
            ["store_robbery"] = "robbery", ["atm_robbery"] = "robbery", ["bank_robbery"] = "robbery",
            ["jewelry_heist"] = "robbery", ["truck_robbery"] = "robbery", ["house_robbery"] = "robbery",
            ["vending_robbery"] = "petty", ["meter_theft"] = "petty", ["pickpocket"] = "petty",
            ["window_smash"] = "petty", ["chop_shop"] = "petty", ["drug_deal_caught"] = "petty",
            ["honest_work"] = "honest", ["odd_job"] = "honest", ["church_job"] = "honest", ["bounty"] = "honest",
        };
    }
}
