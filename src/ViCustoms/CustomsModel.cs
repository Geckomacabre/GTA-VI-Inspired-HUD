using System.Collections.Generic;
using System.Drawing;

namespace ViExtras
{
    // The menu as client/menu.lua sends it to the page (D in html/app.js).

    /// <summary>One thing a slot can be set to.</summary>
    internal sealed class CustomsOption
    {
        public string Value;
        public string Label;
        public int Price;
        public bool Stock;          // the factory option: never "Not owned"
        // swatches (colors.js swatchOf)
        public int Paint = -1;      // a GTA paint index
        public string Finish;       // metallic, matte, metal, chrome, chameleon, pearl
        public Color? Hex;          // a flat colour (neon, smoke, xenon, custom RGB)
        public string Group;        // the colour family tab it sits under
    }

    internal struct WheelGroup
    {
        public int Id;
        public string Label;
    }

    /// <summary>One thing that can be changed on the vehicle.</summary>
    internal sealed class CustomsSlot
    {
        public string Id, Label, Camera;
        public string Kind = "list";    // list, swatch, wheels
        public List<CustomsOption> Options = new List<CustomsOption>();
        public List<WheelGroup> Groups;  // wheels: the wheel types
    }

    internal sealed class CustomsCategory
    {
        public string Label, Hint, Camera;
        public bool Kits;
        public List<string> Slots = new List<string>();
    }

    internal sealed class CustomsTab
    {
        public string Id, Label, Camera;
        public List<CustomsCategory> Categories = new List<CustomsCategory>();
    }

    internal sealed class CustomsKit
    {
        public string Id, Label;
        public int Price, Parts;
        public Dictionary<string, string> Values;
    }

    /// <summary>A 0-10 bar on the card: the factory car, the car as owned, and with the option looked at.</summary>
    internal struct CustomsStat
    {
        public float Stock, Owned, Value;
    }

    internal sealed class CustomsStats
    {
        public static readonly string[] Keys = { "speed", "acceleration", "asphalt", "offroad", "strength" };
        public static readonly string[] Labels = { "Speed", "Acceleration", "Asphalt Handling", "Off-Road Handling", "Strength" };
        public CustomsStat[] Bars = new CustomsStat[5];
        public int RoadOwned, Road, DirtOwned, Dirt;
    }

    internal sealed class SwatchGroup
    {
        public string Name;
        public List<CustomsOption> Options = new List<CustomsOption>();
    }

    /// <summary>A screen of the menu (app.js stack entries).</summary>
    internal sealed class MenuView
    {
        public string Type;             // root, slots, groups, item, swatch, kits
        public int Tab;                 // root
        public int[] RootSel;           // root: the row picked on each tab
        public CustomsCategory Category; // slots
        public CustomsSlot Slot;        // groups, item, swatch
        public List<CustomsOption> Options; // item
        public List<SwatchGroup> Groups;    // swatch
        public int G, Sel, Offset;
        public string Crumb, Title, Camera;

        public bool Leaf => Type == "item" || Type == "swatch" || Type == "kits";
    }
}
