// The fork's rows in GTA6Hud's HUD OPTIONS menu that are ViExtras settings.
using ViExtras.NpcMemory;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's HUD OPTIONS menu (hold V, or the camera button; and the pause
    /// menu's HUD page) has four rows of the fork's under FBICat's: GREET /
    /// ANTAGONIZE, POLICE MEMORY, SPIN-OUTS and VI RADIO WHEEL. Police memory and
    /// spin-outs are GTA6Hud's own settings; the other two are ViExtras', so
    /// GTA6Hud asks here, by reflection, what they are (Value) and to flip them
    /// (Toggle). A change takes at once, and goes into ViExtras.ini with the
    /// file's comments kept (Config.SetLine).
    /// </summary>
    public static class HudMenu
    {
        public static string Value(string key)
        {
            switch (key)
            {
                case "GREET": return MemCfg.Interact ? "ON" : "OFF";
                case "RADIO": return RadioWheel.On ? "ON" : "OFF";
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
            }
        }
    }
}
