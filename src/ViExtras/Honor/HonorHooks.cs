using System;
using System.Collections.Generic;

namespace ViExtras
{
    /// <summary>
    /// One entry of vice_hud/honor/config.lua's Config.Hooks: a named deed and
    /// what it does to honor. Everything that moves honor goes through one of
    /// these (HonorApi.Report / Honor.ApplyHook), so all the tuning is here.
    /// </summary>
    internal sealed class HonorHook
    {
        /// <summary>Honor added (negative to subtract).</summary>
        public int Delta;
        /// <summary>Minimum gap between two applications, game ms. 0: none.</summary>
        public int CooldownMs;
        /// <summary>Most honor this hook may move in one session. 0: no cap.</summary>
        public int SessionCap;
        /// <summary>Short reason, as the FiveM version hands it to the panel.</summary>
        public string Label;
        /// <summary>"good", "bad" or "terrible": the face the centre pop shows for this deed.</summary>
        public string Severity;
        /// <summary>Used instead when Config.Escalation matches (wanted, or a recent kill).</summary>
        public HonorEscalation Escalate;
    }

    /// <summary>A hook's `escalate` block. Severity defaults to "terrible".</summary>
    internal sealed class HonorEscalation
    {
        public int? Delta;
        public string Label;
        public string Severity;
    }

    /// <summary>
    /// Config.Hooks and Config.Escalation.violentHooks from
    /// vice_hud/honor/config.lua, value for value. The robbery, fishing, job
    /// and so on hooks belong to other FiveM resources; they stay so that
    /// HonorApi.Report can apply them, but nothing in single player fires them
    /// on its own.
    /// </summary>
    internal static class HonorHooks
    {
        private static HonorHook H(int delta, string label, string severity, int cooldownMs = 0, int sessionCap = 0, HonorEscalation escalate = null) =>
            new HonorHook { Delta = delta, CooldownMs = cooldownMs, SessionCap = sessionCap, Label = label, Severity = severity, Escalate = escalate };

        private static HonorEscalation E(int delta, string label, string severity = "terrible") =>
            new HonorEscalation { Delta = delta, Label = label, Severity = severity };

        public static readonly Dictionary<string, HonorHook> All = new Dictionary<string, HonorHook>(StringComparer.Ordinal)
        {
            // ---------- Crimes ----------
            // The wanted watcher (Honor.cs), per wanted-star increase.
            ["wanted_level"] = H(-5, "Wanted by police", "bad", cooldownMs: 15000),

            // The conduct watcher (HonorConduct.cs).
            ["kill_civilian"] = H(-8, "Killed a bystander", "terrible", cooldownMs: 4000),
            ["kill_animal"] = H(-2, "Killed an animal", "bad", cooldownMs: 4000),
            ["aim_at_civilian"] = H(-1, "Threatened a civilian", "bad", cooldownMs: 45000, sessionCap: 6),
            // The "Antagonize" prompt on a civilian (um_memory's port reports it).
            ["antagonize_npc"] = H(-2, "Antagonized a civilian", "bad", cooldownMs: 45000, sessionCap: 8),
            ["kill_cop"] = H(-15, "Killed a police officer", "terrible", cooldownMs: 4000),
            ["kill_player"] = H(-15, "Killed another person", "terrible", cooldownMs: 4000),
            // fenix-police / um_livingworld in FiveM: nothing in single player fires it.
            ["complied_with_police"] = H(3, "Complied with police", "good", cooldownMs: 120000),
            // The conduct watcher: IS_PED_JACKING.
            ["carjack"] = H(-4, "Carjacked a driver", "bad", cooldownMs: 30000),

            // ---------- Robbery (other FiveM resources) ----------
            ["jewelry_heist"] = H(-8, "Robbed a jeweller", "bad",
                escalate: E(-16, "Robbed a jeweller at gunpoint")),
            ["truck_robbery"] = H(-10, "Robbed an armoured truck", "bad",
                escalate: E(-18, "Robbed an armoured truck in a firefight")),
            ["store_robbery"] = H(-6, "Robbed a store", "bad",
                escalate: E(-14, "Robbed a store at gunpoint")),
            ["atm_robbery"] = H(-4, "Robbed an ATM", "bad", cooldownMs: 60000,
                escalate: E(-10, "Robbed an ATM with police on you")),
            ["bank_robbery"] = H(-12, "Robbed a bank", "bad",
                escalate: E(-20, "Robbed a bank in a firefight")),
            ["safe_crack"] = H(-6, "Cracked a safe", "bad", cooldownMs: 30000,
                escalate: E(-14, "Cracked a safe with police on you")),

            ["drug_deal_rejected"] = H(-2, "Botched a drug deal", "bad", cooldownMs: 60000, sessionCap: 10),
            ["drug_deal_caught"] = H(-4, "Caught dealing drugs", "bad", cooldownMs: 60000),

            ["window_smash"] = H(-4, "Broke into a vehicle", "bad"),
            ["house_robbery"] = H(-6, "Fenced stolen property", "bad"),
            ["chop_shop"] = H(-5, "Chopped a stolen car", "bad"),
            ["pawn_melt"] = H(-3, "Melted down jewellery", "bad", cooldownMs: 120000),
            ["hobo_fence"] = H(-2, "Sold to a fence", "bad", cooldownMs: 120000),
            ["pickpocket"] = H(-3, "Picked a pocket", "bad", cooldownMs: 120000),

            // ---------- Good conduct ----------
            ["fish_release"] = H(2, "Released a fish", "good", cooldownMs: 30000, sessionCap: 20),
            // The "Greet" prompt on a civilian (um_memory's port reports it).
            ["greet_npc"] = H(1, "Greeted a stranger", "good", cooldownMs: 180000, sessionCap: 6),
            ["geocache"] = H(3, "Found a geocache", "good"),
            ["bounty"] = H(4, "Collected a bounty", "good"),
            ["church_job"] = H(3, "Helped at the church", "good"),
            ["odd_job"] = H(1, "Did an odd job", "good", cooldownMs: 300000, sessionCap: 6),
            ["honest_work"] = H(1, "An honest day's work", "good", cooldownMs: 600000, sessionCap: 8),
            ["barn_find"] = H(2, "Restored a derelict", "good"),
            ["vending_robbery"] = H(-3, "Broke into a vending machine", "bad", cooldownMs: 90000),
            ["meter_theft"] = H(-1, "Broke into a parking meter", "bad", cooldownMs: 60000, sessionCap: 10),
        };

        /// <summary>
        /// Config.Escalation.violentHooks: firing one marks the player as
        /// recently violent, which escalates a robbery in the next few minutes.
        /// </summary>
        public static readonly HashSet<string> Violent = new HashSet<string>(StringComparer.Ordinal)
        {
            "kill_civilian", "kill_cop", "kill_player",
        };
    }
}
