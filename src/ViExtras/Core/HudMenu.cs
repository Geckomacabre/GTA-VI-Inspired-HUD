// The fork's rows in GTA6Hud's HUD OPTIONS menu that are ViExtras settings.
using System;
using ViExtras.NpcMemory;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's HUD OPTIONS menu (hold V, or the camera button; and the pause
    /// menu's HUD page) has the fork's rows under FBICat's: GREET / ANTAGONIZE,
    /// SPIN-OUTS, VI RADIO WHEEL and POLICE LIGHTS. Spin-outs is GTA6Hud's own
    /// setting; the others are ViExtras', so GTA6Hud asks here, by reflection,
    /// what they are (Value) and to change them (Toggle: flip, or for POLICE
    /// LIGHTS the next size). A change takes at once, and goes into ViExtras.ini
    /// with the file's comments kept (Config.SetLine).
    /// </summary>
    public static class HudMenu
    {
        // POLICE LIGHTS: each press goes to the next size up, from the largest back to the smallest
        private static readonly int[] PoliceSizes = { 60, 80, 100, 120, 140 };

        public static string Value(string key)
        {
            switch (key)
            {
                case "GREET": return MemCfg.Interact ? "ON" : "OFF";
                case "RADIO": return RadioWheel.On ? "ON" : "OFF";
                case "POLICE": return (int)Math.Round(PoliceLights.SizeScale * 100f) + "%";
                default: return "N/A";
            }
        }

        public static void Toggle(string key)
        {
            switch (key)
            {
                case "GREET":
                    MemCfg.Interact = !MemCfg.Interact;
                    if (!MemCfg.Interact) Interact.Hide();
                    Config.SetLine("Memory", "Interact", MemCfg.Interact ? "True" : "False");
                    Log.Write("Memory", "Greet / Antagonize " + (MemCfg.Interact ? "on" : "off") + " (HUD OPTIONS)");
                    break;
                case "RADIO":
                    bool on = !RadioWheel.On;
                    RadioWheel.Switch(on);
                    Config.SetLine("Radio", "Enabled", on ? "True" : "False");
                    Log.Write("Radio", "VI radio wheel " + (on ? "on" : "off") + " (HUD OPTIONS)");
                    break;
                case "POLICE":
                    int now = (int)Math.Round(PoliceLights.SizeScale * 100f), next = PoliceSizes[0];
                    foreach (int s in PoliceSizes) if (s > now) { next = s; break; }
                    PoliceLights.SizeScale = next / 100f;
                    Config.SetLine("PoliceLights", "Size", next.ToString());
                    Log.Write("PoliceLights", "size " + next + "% (HUD OPTIONS)");
                    break;
            }
        }
    }
}
