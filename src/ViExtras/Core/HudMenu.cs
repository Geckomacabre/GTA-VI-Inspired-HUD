// The fork's rows in GTA6Hud's HUD OPTIONS menu that are ViExtras settings.
using System;
using ViExtras.NpcMemory;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's HUD OPTIONS menu (hold V, or the camera button; and the pause
    /// menu's HUD page) has the fork's rows under FBICat's: GREET / ANTAGONIZE,
    /// SPIN-OUTS, VI RADIO WHEEL, CARRY ALL WEAPONS and POLICE LIGHTS, then the
    /// ViExtras switches a player flips most: CROSSHAIR, KILL MARKS, VI WANTED
    /// STARS, WANTED BOX, CAMERA WEIGHT, BINOCULARS, CALMER POLICE and MONEY IN
    /// STORES. Spin-outs and all weapons are GTA6Hud's own settings; the others
    /// are ViExtras', so GTA6Hud asks here, by reflection, what they are (Value)
    /// and to change them (Toggle: flip, or for POLICE LIGHTS the next size). A
    /// change takes at once (each feature reads its switch every tick) and goes
    /// into ViExtras.ini, on its [Switches] line, with the file's comments kept
    /// (Config.SetLine).
    /// </summary>
    public static class HudMenu
    {
        // POLICE LIGHTS: each press goes to the next size up, from the largest back to 0 (no lights)
        private static readonly int[] PoliceSizes = { 0, 60, 80, 100, 120, 140 };

        public static string Value(string key)
        {
            switch (key)
            {
                case "GREET": return MemCfg.Interact ? "ON" : "OFF";
                case "RADIO": return RadioWheel.On ? "ON" : "OFF";
                case "POLICE": return (int)Math.Round(PoliceLights.SizeScale * 100f) + "%";
                case "CROSSHAIR": return OnOff(Crosshair.On);
                case "KILLMARKS": return OnOff(Crosshair.KillMarksOn);
                case "STARS": return OnOff(WantedStars.On);
                case "WANTEDBOX": return OnOff(WantedBox.On);
                case "CAMWEIGHT": return OnOff(WeightedCam.On);
                case "BINOCULARS": return OnOff(Binoculars.On);
                case "CALMER": return OnOff(CalmerPolice.On);
                case "STOREMONEY": return OnOff(StoreMoney.On);
                default: return "N/A";
            }
        }

        private static string OnOff(bool on) => on ? "ON" : "OFF";

        // A switch flipped from the menu: on its [Switches] line in ViExtras.ini, and in the log
        private static bool Flip(bool now, string section, string key, string what)
        {
            bool on = !now;
            Config.SetLine(section, key, on ? "True" : "False");
            Log.Write("HudMenu", what + " " + (on ? "on" : "off") + " (HUD OPTIONS)");
            return on;
        }

        public static void Toggle(string key)
        {
            switch (key)
            {
                case "CROSSHAIR": Crosshair.On = Flip(Crosshair.On, "Crosshair", "Enabled", "crosshair"); return;
                case "KILLMARKS": Crosshair.KillMarksOn = Flip(Crosshair.KillMarksOn, "Crosshair", "KillMarks", "kill marks"); return;
                case "STARS": WantedStars.On = Flip(WantedStars.On, "WantedStars", "Enabled", "VI wanted stars"); return;
                case "WANTEDBOX": WantedBox.On = Flip(WantedBox.On, "WantedBox", "Enabled", "wanted box"); return;
                case "CAMWEIGHT": WeightedCam.On = Flip(WeightedCam.On, "WeightedCam", "Enabled", "camera weight"); return;
                case "BINOCULARS": Binoculars.On = Flip(Binoculars.On, "Binoculars", "Enabled", "binoculars"); return;
                case "CALMER": CalmerPolice.On = Flip(CalmerPolice.On, Config.Switches, "CalmerPolice", "calmer police"); return;
                case "STOREMONEY": StoreMoney.On = Flip(StoreMoney.On, Config.Switches, "MoneyInStores", "money in stores"); return;
            }
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
