using System;
using System.Collections.Generic;

namespace ViExtras
{
    /// <summary>
    /// The FiveM version's skills (vice_hud skills.lua): the list, what each is
    /// worth, and the level curve. Labels, units and blurbs are not here: the
    /// panel and the card draw them from art rendered off vice_hud's own page
    /// (tools/render/skills.py), named by `Id`.
    /// </summary>
    internal sealed class SkillDef
    {
        public string Id;
        /// <summary>The single-player stat this is for Michael, Franklin and Trevor (SPn_ + this), or null.</summary>
        public string SpStat;
        /// <summary>The stat FiveM writes for a freemode ped (prefix + this), or null (Health).</summary>
        public string FiveMStat;
        /// <summary>XP per unit of the tracked activity (skills.lua `rate`).</summary>
        public float Rate;
        /// <summary>Starts at level 0 rather than Config.Skills.startingLevel (skills.lua `noSeed`).</summary>
        public bool NoSeed;
        public int Order;
    }

    internal static class SkillDefs
    {
        // skills.lua Skills.List, in display order. The single-player column is
        // the game's own stat: GTA V calls its Driving stat WHEELIE_ABILITY (as
        // GTA6Hud's pause menu STATS page reads it), so FiveM's Driving maps
        // there and FiveM's separate Wheelie has no single-player stat of its own.
        public static readonly SkillDef[] List =
        {
            new SkillDef { Id = "stamina",  SpStat = "STAMINA",          FiveMStat = "STAMINA",          Rate = 0.5f },
            new SkillDef { Id = "strength", SpStat = "STRENGTH",         FiveMStat = "STRENGTH",         Rate = 12f },
            new SkillDef { Id = "health",   SpStat = null,               FiveMStat = null,               Rate = 0.35f, NoSeed = true },
            new SkillDef { Id = "lung",     SpStat = "LUNG_CAPACITY",    FiveMStat = "LUNG_CAPACITY",    Rate = 12f },
            new SkillDef { Id = "shooting", SpStat = "SHOOTING_ABILITY", FiveMStat = "SHOOTING_ABILITY", Rate = 6f },
            new SkillDef { Id = "stealth",  SpStat = "STEALTH_ABILITY",  FiveMStat = "STEALTH_ABILITY",  Rate = 2f },
            new SkillDef { Id = "driving",  SpStat = "WHEELIE_ABILITY",  FiveMStat = "DRIVING_ABILITY",  Rate = 0.1f },
            new SkillDef { Id = "flying",   SpStat = "FLYING_ABILITY",   FiveMStat = "FLYING_ABILITY",   Rate = 4f },
            new SkillDef { Id = "wheelie",  SpStat = null,               FiveMStat = "WHEELIE_ABILITY",  Rate = 9f },
        };

        public static readonly Dictionary<string, SkillDef> ById = Build();

        private static Dictionary<string, SkillDef> Build()
        {
            var d = new Dictionary<string, SkillDef>(StringComparer.Ordinal);
            for (int i = 0; i < List.Length; i++)
            {
                List[i].Order = i;
                d[List[i].Id] = List[i];
            }
            return d;
        }

        public const int MaxLevel = 100;
        public const int XpBase = 100;      // skills.lua Skills.XP_BASE
        public const int XpGrowth = 18;     // skills.lua Skills.XP_GROWTH

        /// <summary>XP from `level` to `level + 1` (skills.lua Skills.xpToNext).</summary>
        public static long XpToNext(int level)
        {
            if (level >= MaxLevel) return 0;
            if (level < 0) level = 0;
            return (long)Math.Floor(XpBase + XpGrowth * (double)level + 0.5);
        }

        /// <summary>Total XP to reach `level` from zero (Skills.xpForLevel).</summary>
        public static long XpForLevel(int level)
        {
            if (level <= 0) return 0;
            if (level > MaxLevel) level = MaxLevel;
            long total = 0;
            for (int n = 0; n < level; n++) total += XpToNext(n);
            return total;
        }

        /// <summary>A raw XP total as level, XP into it, XP it needs, and the fraction (Skills.resolve).</summary>
        public static void Resolve(long xp, out int level, out long into, out long need, out float frac)
        {
            if (xp < 0) xp = 0;
            level = 0;
            long remaining = xp;
            while (level < MaxLevel)
            {
                long n = XpToNext(level);
                if (remaining < n) break;
                remaining -= n;
                level++;
            }
            if (level >= MaxLevel)
            {
                level = MaxLevel; into = 0; need = 0; frac = 1f;
                return;
            }
            need = XpToNext(level);
            into = remaining;
            frac = need > 0 ? (float)remaining / need : 0f;
        }

        public static int LevelOf(long xp)
        {
            Resolve(xp, out int level, out _, out _, out _);
            return level;
        }
    }
}
