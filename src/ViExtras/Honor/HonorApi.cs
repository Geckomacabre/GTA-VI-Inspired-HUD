using System;

namespace ViExtras
{
    /// <summary>
    /// Honor, as the other extras see it: the FiveM version's honor module
    /// (vice_hud/honor, formerly qbx_honor) for single player. The Honor script
    /// owns the value and its rules (Config.Hooks: amounts, cooldowns, caps,
    /// escalation); anything else only names what happened, as FiveM's
    /// exports.qbx_honor:ApplyHook(source, 'greet_npc') did.
    ///
    /// These public members are the contract other scripts (um_memory's port)
    /// code against. Keep their names and meanings.
    /// </summary>
    public static class HonorApi
    {
        /// <summary>Current honor of the character being played (MinHonor..MaxHonor).</summary>
        public static int Value { get; internal set; }

        /// <summary>MinHonor and MaxHonor (-1000 and 1000 unless the ini says otherwise).</summary>
        public static int MinValue { get; internal set; }
        public static int MaxValue { get; internal set; }

        /// <summary>
        /// Value as a share of the way to MaxHonor (up to 100) or MinHonor (down to -100): the scale honor had
        /// before it went to 1000, for anything weighed against that (um_memory's mood).
        /// </summary>
        public static float Per100 => Value >= 0
            ? (MaxValue > 0 ? Value * 100f / MaxValue : 0f)
            : (MinValue < 0 ? Value * -100f / MinValue : 0f);

        /// <summary>Latched once honor has hit the floor; never clears (Config "Unrepairable floor").</summary>
        public static bool Broken { get; internal set; }

        /// <summary>True once the Honor script is running and has loaded this character's standing.</summary>
        public static bool Ready { get; internal set; }

        /// <summary>"angel", "devil", or null in between, for a value (default: the current one).</summary>
        public static string TierOf(int value) => TierFor != null ? TierFor(value) : null;
        public static string Tier => TierOf(Value);

        /// <summary>The named rank: Honor.Professional, Aggressive, Violent, Psycho or Shattered (null until Ready).</summary>
        public static string Rank { get; internal set; }

        /// <summary>
        /// A deed happened: apply the named hook (a key of Config.Hooks, e.g.
        /// "greet_npc", "antagonize_npc", "kill_civilian"). Returns false when the
        /// hook is unknown, cooling down, capped, or Honor is off.
        /// </summary>
        public static bool Report(string hook) => Handler != null && Handler(hook);

        /// <summary>Raised after every applied hook: (new value, previous value, label, severity).</summary>
        public static event Action<int, int, string, string> Changed;

        /// <summary>
        /// Raised with the hook's name after it is applied, as FiveM's
        /// qbx_honor:server:hookApplied: lets other systems (um_memory's
        /// neighbourhood reputation) piggyback on every deed. Handlers run in the
        /// Honor script's turn, so queue anything that calls natives or waits.
        /// </summary>
        public static event Action<string> HookApplied;

        // Set by the Honor script.
        internal static Func<string, bool> Handler;
        internal static Func<int, string> TierFor;
        internal static void RaiseChanged(int value, int previous, string label, string severity) =>
            Changed?.Invoke(value, previous, label, severity);
        internal static void RaiseHookApplied(string hook) => HookApplied?.Invoke(hook);
    }
}
