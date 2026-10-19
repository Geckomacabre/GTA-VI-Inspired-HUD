using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The menu for one vehicle: what it can be fitted with, what each option
    /// costs, the Pro Builds it can take and what the card's bars read. A port of
    /// client/menu.lua (buildSlot, buildMenu, buildKits, computeStats) and
    /// shared/customs.lua (BasePrice, Price, Cart), with Leonida Customs' own
    /// tables (CustomsData, generated from them).
    /// </summary>
    internal sealed class CustomsLogic
    {
        public readonly Vehicle Vehicle;
        public readonly Dictionary<string, CustomsSlot> Slots = new Dictionary<string, CustomsSlot>();
        public readonly Dictionary<string, Dictionary<string, string>> KitParts = new Dictionary<string, Dictionary<string, string>>();
        public List<CustomsTab> Tabs = new List<CustomsTab>();
        public List<CustomsKit> Kits = new List<CustomsKit>();
        public Dictionary<string, string> Owned;
        public readonly int StockWheelType;
        private readonly bool _free, _chameleon;
        private readonly Action<int> _wait;
        private BaseSpec _base;

        public static readonly PlateStore Nitrous = new PlateStore("nitrous.txt");
        public static readonly PlateStore Builds = new PlateStore("probuilds.txt");

        public string Plate => Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, Vehicle);

        public CustomsLogic(Vehicle vehicle, bool chameleon, Action<int> wait)
        {
            Vehicle = vehicle;
            _wait = wait;
            _chameleon = chameleon;
            _free = CustomsData.FreeUpgradeToClass.Contains((int)vehicle.ClassType);
            StockWheelType = Function.Call<int>(Hash.GET_VEHICLE_WHEEL_TYPE, vehicle);
        }

        // =============================================================================
        // Labels
        // =============================================================================

        private static string GameLabel(string key)
        {
            if (string.IsNullOrEmpty(key) || key == "NULL") return null;
            string label = Game.GetLocalizedString(key);
            return string.IsNullOrEmpty(label) || label == "NULL" ? null : label;
        }

        private static string ConfigKey(string slot)
        {
            int mod = CustomsCar.ModIndex(slot);
            if (mod >= 0) return CustomsCar.Str(mod);
            if (CustomsCar.ExtraIndex(slot) >= 0) return "extra";
            return CustomsData.SlotConfigKey.TryGetValue(slot, out string key) ? key : null;
        }

        private static CustomsData.ModCost Cfg(string slot)
        {
            string key = ConfigKey(slot);
            return key != null && CustomsData.ModCosts.TryGetValue(key, out CustomsData.ModCost c) ? c : null;
        }

        public static string SlotLabel(string slot)
        {
            if (CustomsData.Labels.TryGetValue(slot, out string label)) return label;
            int extra = CustomsCar.ExtraIndex(slot);
            if (extra >= 0) return "Extra " + extra;
            return Cfg(slot)?.Label ?? slot;
        }

        // =============================================================================
        // Prices (shared/customs.lua BasePrice / Price)
        // =============================================================================

        private static bool IsRgb(string v) => CustomsCar.ParseRgb(v) != null;

        /// <summary>The price before the free classes; null = not something the shop sells.</summary>
        public static int? BasePrice(string slot, string value)
        {
            if (slot == "nitrous")
            {
                foreach (var t in CustomsData.NitrousTiers) if (t.Id == value) return t.Price;
                return null;
            }
            CustomsData.ModCost cfg = Cfg(slot);
            if (cfg == null) return null;
            var extra = CustomsData.ExtraPrices;
            int mod = CustomsCar.ModIndex(slot);
            if (mod >= 0)
            {
                if (!int.TryParse(value, out int n)) return null;
                if (n < 0) return 0;
                return cfg.MultiCostPerLevel ? cfg.Cost * (n + 1) : cfg.Cost;
            }
            switch (slot)
            {
                case "turbo": return CustomsCar.Bool(value) ? cfg.Cost : 0;
                case "wheels": return value == "stock" ? 0 : value != null && value.Contains(":") ? cfg.Cost : (int?)null;
                case "paint1":
                    if (!int.TryParse(value, out int p1)) return null;
                    return CustomsData.PaintFinish.TryGetValue(p1, out string f) && f == "chameleon" ? extra["chameleon"] : cfg.Cost;
                case "paint2":
                case "plate": return int.TryParse(value, out _) ? cfg.Cost : (int?)null;
                case "pearl": return int.TryParse(value, out _) ? extra["pearl"] : (int?)null;
                case "wheelcolor": return int.TryParse(value, out _) ? extra["wheelcolor"] : (int?)null;
                case "rgb1":
                case "rgb2": return value == "off" ? 0 : IsRgb(value) ? extra["rgb"] : (int?)null;
                case "tyresmoke": return value == "off" ? 0 : IsRgb(value) ? extra["tyresmoke"] : (int?)null;
                case "neon": return value == "off" ? 0 : IsRgb(value) ? cfg.Cost : (int?)null;
                case "xenon": return value == "off" ? 0 : int.TryParse(value, out _) ? cfg.Cost : (int?)null;
                case "window": return int.TryParse(value, out int w) ? (w > 0 ? cfg.Cost : 0) : (int?)null;
                case "drift":
                case "bulletproof":
                case "customtire": return CustomsCar.Bool(value) ? extra[slot] : 0;
            }
            // extra:<id>, turning an extra off is free
            return CustomsCar.Bool(value) ? cfg.Cost : 0;
        }

        public int? Price(string slot, string value)
        {
            int? b = BasePrice(slot, value);
            if (b == null) return null;
            return _free || b <= 0 ? 0 : b;
        }

        // =============================================================================
        // Options of every slot (menu.lua)
        // =============================================================================

        private List<CustomsOption> ModOptions(int mod)
        {
            int count = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, Vehicle, mod);
            bool livery = false;
            if (mod == 48 && count <= 0)
            {
                count = Function.Call<int>(Hash.GET_VEHICLE_LIVERY_COUNT, Vehicle);
                livery = true;
            }
            if (count <= 0) return null;
            string cfgLabel = Cfg("mod:" + mod)?.Label ?? ("Mod " + mod);
            var options = new List<CustomsOption> { new CustomsOption { Value = "-1", Label = mod == 48 ? "No Livery" : "Stock " + cfgLabel } };
            for (int i = 0; i < count; i++)
            {
                string label = GameLabel(livery ? Function.Call<string>(Hash.GET_LIVERY_NAME, Vehicle, i) : Function.Call<string>(Hash.GET_MOD_TEXT_LABEL, Vehicle, mod, i));
                if (label == null)
                    label = CustomsData.LevelLabels.TryGetValue(mod, out string[] levels) && i < levels.Length ? levels[i] : cfgLabel + " " + (i + 1);
                options.Add(new CustomsOption { Value = CustomsCar.Str(i), Label = label });
            }
            return options;
        }

        private static List<CustomsOption> Toggle(string label, string stock = "None") => new List<CustomsOption>
        {
            new CustomsOption { Value = CustomsCar.Off, Label = stock },
            new CustomsOption { Value = CustomsCar.On, Label = label },
        };

        private static List<CustomsOption> Presets((string Name, int R, int G, int B)[] colours, string group)
        {
            var list = new List<CustomsOption>();
            foreach (var c in colours)
                list.Add(new CustomsOption { Value = CustomsCar.Rgb(c.R, c.G, c.B), Label = c.Name, Hex = Color.FromArgb(c.R, c.G, c.B), Group = group });
            return list;
        }

        private List<CustomsOption> PaintOptions(string key)
        {
            var list = new List<CustomsOption>();
            if (!CustomsData.PaintOptions.TryGetValue(key, out var paints)) return list;
            foreach (var p in paints)
            {
                // The gen9 chameleon paints need Leonida's carcols, which single player has not got.
                if (p.Finish == "chameleon" && !_chameleon) continue;
                list.Add(new CustomsOption { Value = CustomsCar.Str(p.Index), Label = p.Label, Paint = p.Index, Finish = p.Finish });
            }
            return list;
        }

        private List<WheelGroup> WheelGroups()
        {
            bool bike = Vehicle.ClassType == VehicleClass.Motorcycles;
            var groups = new List<WheelGroup>();
            var types = new List<(string Name, int Value)>(CustomsData.WheelTypes);
            types.Sort((a, b) => a.Value.CompareTo(b.Value));
            foreach (var t in types)
                if ((t.Value == 6) == bike)
                    groups.Add(new WheelGroup { Id = t.Value, Label = CustomsData.WheelTypeLabels.TryGetValue(t.Name, out string l) ? l : t.Name });
            return groups;
        }

        /// <summary>The rims of one wheel type (the game only counts the type fitted, so it is fitted for a moment).</summary>
        public List<CustomsOption> WheelOptions(int wheelType)
        {
            int oldType = Function.Call<int>(Hash.GET_VEHICLE_WHEEL_TYPE, Vehicle);
            int oldIndex = Function.Call<int>(Hash.GET_VEHICLE_MOD, Vehicle, 23);
            bool custom = Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, Vehicle, 23);
            Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, Vehicle, wheelType);
            _wait(0);
            var options = new List<CustomsOption> { new CustomsOption { Value = "stock", Label = "Stock Wheels", Stock = true } };
            int count = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, Vehicle, 23);
            for (int i = 0; i < count; i++)
                options.Add(new CustomsOption { Value = wheelType + ":" + i, Label = GameLabel(Function.Call<string>(Hash.GET_MOD_TEXT_LABEL, Vehicle, 23, i)) ?? "Wheel " + (i + 1) });
            Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, Vehicle, oldType);
            CustomsCar.SetMod(Vehicle, 23, oldIndex, custom);
            foreach (CustomsOption o in options) o.Price = Price("wheels", o.Value) ?? 0;
            return options;
        }

        private static bool HasAccessory(string name) => Array.IndexOf(CustomsData.WheelAccessories, name) >= 0;

        /// <summary>buildSlot: the slot as the menu shows it, or null when the vehicle has nothing for it.</summary>
        private CustomsSlot BuildSlot(string slot)
        {
            var def = new CustomsSlot { Id = slot, Label = SlotLabel(slot) };
            int mod = CustomsCar.ModIndex(slot);
            if (mod >= 0) def.Options = ModOptions(mod);
            else if (CustomsCar.ExtraIndex(slot) >= 0) def.Options = Toggle("On", "Off");
            else switch (slot)
            {
                case "turbo": def.Options = Toggle("Turbo Upgrade", "Stock Induction"); break;
                case "wheels":
                    def.Kind = "wheels";
                    def.Groups = WheelGroups();
                    def.Options = new List<CustomsOption> { new CustomsOption { Value = "stock", Label = "Stock Wheels" } };
                    if (def.Groups.Count == 0) return null;
                    break;
                case "wheelcolor": def.Kind = "swatch"; def.Options = PaintOptions("wheelcolor"); break;
                case "customtire": if (!HasAccessory("CustomTire")) return null; def.Options = Toggle("Custom Tires"); break;
                case "bulletproof": if (!HasAccessory("BulletProof")) return null; def.Options = Toggle("Bulletproof Tires"); break;
                case "drift": if (!HasAccessory("DriftTires")) return null; def.Options = Toggle("Drift Tires"); break;
                case "tyresmoke":
                    if (!HasAccessory("SmokeColor")) return null;
                    def.Kind = "swatch";
                    def.Options = Presets(CustomsData.SmokeColors, "Smoke");
                    def.Options.Insert(0, new CustomsOption { Value = "off", Label = "None", Group = "Smoke" });
                    break;
                case "paint1": def.Kind = "swatch"; def.Options = PaintOptions("paint1"); break;
                case "paint2": def.Kind = "swatch"; def.Options = PaintOptions("paint2"); break;
                case "pearl": def.Kind = "swatch"; def.Options = PaintOptions("pearl"); break;
                case "rgb1":
                case "rgb2":
                    def.Kind = "swatch";
                    def.Options = new List<CustomsOption>();
                    foreach (var p in CustomsData.RgbPresets)
                        def.Options.Add(new CustomsOption { Value = p.Value, Label = p.Label, Group = p.Group, Hex = p.Value == "off" ? (Color?)null : Color.FromArgb(p.R, p.G, p.B) });
                    break;
                case "xenon":
                    def.Kind = "swatch";
                    def.Options = new List<CustomsOption>
                    {
                        new CustomsOption { Value = "off", Label = "Stock Lights", Group = "Xenon" },
                        new CustomsOption { Value = "-1", Label = "Xenon Lights", Hex = XenonHex(-1), Group = "Xenon" },
                    };
                    var xenons = new List<(string Name, int Value, string Label)>(CustomsData.XenonColorList);
                    xenons.Sort((a, b) => a.Value.CompareTo(b.Value));
                    foreach (var x in xenons)
                        if (x.Value >= 0) def.Options.Add(new CustomsOption { Value = CustomsCar.Str(x.Value), Label = x.Label, Hex = XenonHex(x.Value), Group = "Xenon" });
                    break;
                case "neon":
                    def.Kind = "swatch";
                    def.Options = Presets(CustomsData.NeonColors, "Neon");
                    def.Options.Insert(0, new CustomsOption { Value = "off", Label = "None", Group = "Neon" });
                    break;
                case "plate":
                case "window":
                    {
                        var list = new List<(string Name, int Value, string Label)>(slot == "plate" ? CustomsData.PlateList : CustomsData.WindowList);
                        list.Sort((a, b) => a.Value.CompareTo(b.Value));
                        def.Options = new List<CustomsOption>();
                        foreach (var e in list) def.Options.Add(new CustomsOption { Value = CustomsCar.Str(e.Value), Label = e.Label });
                        break;
                    }
                case "nitrous":
                    def.Options = new List<CustomsOption>();
                    foreach (var t in CustomsData.NitrousTiers) def.Options.Add(new CustomsOption { Value = t.Id, Label = t.Label });
                    break;
                default: return null;
            }
            if (def.Options == null || def.Options.Count < 2 && def.Kind != "wheels") return null;
            foreach (CustomsOption o in def.Options)
            {
                o.Price = Price(slot, o.Value) ?? 0;
                o.Stock = BasePrice(slot, o.Value) == 0;  // the factory option, never "not owned"
            }
            def.Camera = CustomsData.SlotCameras.TryGetValue(slot, out string cam) ? cam : null;
            return def;
        }

        private static Color XenonHex(int index) =>
            CustomsData.XenonColors.TryGetValue(index, out var c) ? Color.FromArgb(c.R, c.G, c.B) : Color.White;

        // =============================================================================
        // The menu (buildMenu)
        // =============================================================================

        public void Build()
        {
            // what the car has now, every slot the menu could show
            var all = new List<string>();
            foreach (CustomsData.Tab tab in CustomsData.Menu)
                foreach (CustomsData.Category c in tab.Categories)
                    foreach (string slot in c.Slots)
                    {
                        if (slot == "extras") { for (int e = 0; e <= 12; e++) all.Add("extra:" + e); }
                        else all.Add(slot);
                    }
            Owned = new Dictionary<string, string>();
            foreach (string slot in all)
            {
                if (slot == "nitrous") Owned[slot] = Nitrous.Get(Plate, "Default");
                else Owned[slot] = CustomsCar.ReadSlot(Vehicle, slot);
            }
            Owned["kit"] = Builds.Get(Plate, "none");
            _base = BaseSpec.Of(Vehicle);

            var built = new Dictionary<string, CustomsSlot>();
            CustomsSlot SlotFor(string id)
            {
                if (built.TryGetValue(id, out CustomsSlot s)) return s;
                s = BuildSlot(id);
                built[id] = s;
                if (s != null) Slots[id] = s;
                return s;
            }
            var layout = new List<List<List<string>>>();
            foreach (CustomsData.Tab tab in CustomsData.Menu)
            {
                var cats = new List<List<string>>();
                foreach (CustomsData.Category c in tab.Categories)
                {
                    var ids = new List<string>();
                    foreach (string slot in c.Slots)
                    {
                        if (slot == "extras")
                        {
                            for (int e = 0; e <= 12; e++)
                                if (Function.Call<bool>(Hash.DOES_EXTRA_EXIST, Vehicle, e) && SlotFor("extra:" + e) != null) ids.Add("extra:" + e);
                        }
                        else if (SlotFor(slot) != null) ids.Add(slot);
                    }
                    cats.Add(ids);
                }
                layout.Add(cats);
            }
            BuildKits();
            Tabs = new List<CustomsTab>();
            for (int t = 0; t < CustomsData.Menu.Length; t++)
            {
                CustomsData.Tab tab = CustomsData.Menu[t];
                var outTab = new CustomsTab { Id = tab.Id, Label = tab.Label, Camera = tab.Camera };
                for (int c = 0; c < tab.Categories.Length; c++)
                {
                    CustomsData.Category cat = tab.Categories[c];
                    var entry = new CustomsCategory { Label = cat.Label, Hint = cat.Hint ?? "", Camera = cat.Camera ?? tab.Camera };
                    if (cat.Kits)
                    {
                        if (Kits.Count > 0) { entry.Kits = true; outTab.Categories.Add(entry); }
                    }
                    else if (layout[t][c].Count > 0)
                    {
                        entry.Slots = layout[t][c];
                        outTab.Categories.Add(entry);
                    }
                }
                if (outTab.Categories.Count > 0) Tabs.Add(outTab);
            }
        }

        // =============================================================================
        // Pro Builds (resolveKit / kitPrice / buildKits)
        // =============================================================================

        private static string ValueOf(object want) =>
            want is bool b ? CustomsCar.Str(b) : want is int i ? CustomsCar.Str(i) : Convert.ToString(want, CultureInfo.InvariantCulture);

        private Dictionary<string, string> ResolveKit(CustomsData.ProBuild kit)
        {
            var parts = new Dictionary<string, string>();
            foreach (var kv in kit.Slots)
            {
                if (!Slots.TryGetValue(kv.Key, out CustomsSlot def)) continue;
                string value = null;
                if (CustomsCar.ModIndex(kv.Key) >= 0)
                {
                    int best = CustomsCar.Int(def.Options[def.Options.Count - 1].Value, -1);
                    value = CustomsCar.Str(ValueOf(kv.Value) == "max" ? best : Math.Min(CustomsCar.Int(ValueOf(kv.Value)), best));
                }
                else if (def.Kind == "wheels") value = ValueOf(kv.Value);
                else
                {
                    string want = ValueOf(kv.Value);
                    foreach (CustomsOption o in def.Options) if (o.Value == want) value = want;
                }
                if (value != null) parts[kv.Key] = value;
            }
            return parts;
        }

        /// <summary>What a kit costs on this car now: its price, or less when the parts it would change cost less (Customs.Cart).</summary>
        public int KitPrice(CustomsData.ProBuild kit, Dictionary<string, string> parts)
        {
            if (_free) return 0;
            int sum = 0;
            foreach (var kv in parts)
            {
                if (Owned.TryGetValue(kv.Key, out string have) && have == kv.Value) continue;
                sum += Price(kv.Key, kv.Value) ?? 0;
            }
            return Math.Min(kit.Price, sum);
        }

        public void BuildKits()
        {
            Kits = new List<CustomsKit>();
            KitParts.Clear();
            foreach (CustomsData.ProBuild kit in CustomsData.ProBuilds)
            {
                Dictionary<string, string> parts = ResolveKit(kit);
                if (parts.Count == 0) continue;
                KitParts[kit.Id] = parts;
                Kits.Add(new CustomsKit { Id = kit.Id, Label = kit.Label, Price = KitPrice(kit, parts), Parts = parts.Count, Values = parts });
            }
        }

        public static CustomsData.ProBuild KitDef(string id)
        {
            foreach (CustomsData.ProBuild k in CustomsData.ProBuilds) if (k.Id == id) return k;
            return null;
        }

        // =============================================================================
        // The card's bars (baseSpec / computeStats / ratings / statsPayload)
        // =============================================================================

        private struct BaseSpec
        {
            public float MaxVel, Force, Mass, TcMax, LossMult, Brake, Travel;

            public static BaseSpec Of(Vehicle v)
            {
                var b = new BaseSpec();
                try
                {
                    HandlingData h = v.HandlingData;
                    b.MaxVel = h.InitialDriveMaxFlatVelocity;
                    if (b.MaxVel < 75f) b.MaxVel *= 3.6f;  // stored in m/s
                    b.Force = h.InitialDriveForce;
                    b.Mass = h.Mass;
                    b.TcMax = h.TractionCurveMax;
                    b.LossMult = h.TractionLossMultiplier;
                    b.Brake = h.BrakeForce;
                    b.Travel = h.SuspensionUpperLimit - h.SuspensionLowerLimit;
                }
                catch (Exception ex) { Log.Write("Customs", "handling: " + ex.Message); }
                return b;
            }
        }

        private float[] Compute(IDictionary<string, string> view)
        {
            BaseSpec b = _base;
            var s = new float[]
            {
                (b.MaxVel - 80f) / 12f,
                (b.Force - 0.065f) / 0.05f,
                (b.TcMax - 1.1f) * 4f + b.Brake,
                2.2f + (1f - b.LossMult) * 4.5f + b.Travel * 12.5f,
                (float)Math.Sqrt(Math.Max(0f, b.Mass)) / 13.75f,
            };
            void Add(Dictionary<string, float> effect, float times)
            {
                for (int i = 0; i < CustomsStats.Keys.Length; i++)
                    if (effect.TryGetValue(CustomsStats.Keys[i], out float amount)) s[i] += amount * times;
            }
            foreach (var kv in CustomsData.StatEffects)
            {
                if (view == null || !view.TryGetValue(kv.Key, out string value)) continue;
                if (CustomsCar.ModIndex(kv.Key) >= 0)
                {
                    int n = CustomsCar.Int(value, -1);
                    if (n >= 0) Add(kv.Value, n + 1);
                }
                else if (value == CustomsCar.On) Add(kv.Value, 1f);
            }
            foreach (var kv in CustomsData.ListStatEffects)
                if (view != null && view.TryGetValue(kv.Key, out string value) && value != null && kv.Value.TryGetValue(value, out var effect))
                    Add(effect, 1f);
            for (int i = 0; i < s.Length; i++) s[i] = (float)Math.Floor(Num.Clamp(s[i], 0.1f, 10f) * 10f + 0.5f) / 10f;
            return s;
        }

        private static void Ratings(float[] s, out int road, out int dirt)
        {
            road = (int)Math.Floor((s[0] + s[1] + s[2] * 1.1f) / 3.1f * 100f + 0.5f);
            dirt = (int)Math.Floor((s[3] * 0.5f + s[4] * 0.2f + s[1] * 0.15f + s[0] * 0.15f) * 100f + 0.5f);
        }

        /// <summary>stock = the factory car (the tick), owned = as it is, value = with what is being looked at.</summary>
        public CustomsStats Stats(IDictionary<string, string> view)
        {
            float[] stock = Compute(null), owned = Compute(Owned), now = Compute(view);
            var stats = new CustomsStats();
            for (int i = 0; i < 5; i++) stats.Bars[i] = new CustomsStat { Stock = stock[i], Owned = owned[i], Value = now[i] };
            Ratings(now, out stats.Road, out stats.Dirt);
            Ratings(owned, out stats.RoadOwned, out stats.DirtOwned);
            return stats;
        }

        // =============================================================================
        // Previewing and buying
        // =============================================================================

        // a respray covers the custom RGB paint on the same panel
        public static readonly Dictionary<string, string> PaintLinks = new Dictionary<string, string> { ["paint1"] = "rgb1", ["paint2"] = "rgb2" };

        public Dictionary<string, string> ViewWith(string slot, string value)
        {
            var view = new Dictionary<string, string>(Owned) { [slot] = value };
            if (PaintLinks.TryGetValue(slot, out string link)) view[link] = "off";
            return view;
        }

        public void Apply(string slot, IDictionary<string, string> view) => CustomsCar.Apply(Vehicle, slot, view, StockWheelType);

        /// <summary>Makes `changes` the car's own: fitted, and the ones kept by plate saved.</summary>
        public void Commit(Dictionary<string, string> changes)
        {
            foreach (var kv in changes) Owned[kv.Key] = kv.Value;
            foreach (var kv in changes)
            {
                if (kv.Key == "nitrous") Nitrous.Set(Plate, kv.Value == "Default" ? null : kv.Value);
                else if (kv.Key == "kit") Builds.Set(Plate, kv.Value == "none" ? null : kv.Value);
                else Apply(kv.Key, Owned);
            }
            BuildKits();   // buying a part can lower what a kit containing it costs
        }

        // =============================================================================
        // Swatches (html/colors.js swatchOf / familyOf)
        // =============================================================================

        public static Color PaintColour(int index) =>
            CustomsData.GtaColors.TryGetValue(index, out var c) ? Color.FromArgb(c.R, c.G, c.B) : Color.FromArgb(0x88, 0x88, 0x88);

        public static string FamilyOf(CustomsOption o)
        {
            if (o.Group != null) return o.Group;
            if (o.Finish == "chameleon") return "CHAMELEON";
            if (o.Finish == "metal" || o.Finish == "chrome") return "METALS";
            foreach (var w in CustomsData.FamilyWords)
                if (Regex.IsMatch(o.Label ?? "", w.Pattern, RegexOptions.IgnoreCase)) return w.Family;
            Color c = PaintColour(o.Paint);
            float r = c.R / 255f, g = c.G / 255f, b = c.B / 255f;
            float max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
            float light = (max + min) / 2f;
            float sat = max == min ? 0f : (max - min) / (1f - Math.Abs(2f * light - 1f));
            if (sat < 0.16f || light < 0.08f) return light < 0.17f ? "BLACK" : light > 0.78f ? "WHITE" : "SILVER";
            float hue = max == r ? ((g - b) / (max - min)) % 6f : max == g ? (b - r) / (max - min) + 2f : (r - g) / (max - min) + 4f;
            hue = (hue * 60f + 360f) % 360f;
            if (hue < 15f || hue >= 345f) return "RED";
            if (hue < 45f) return light < 0.38f ? "BROWN" : "ORANGE";
            if (hue < 70f) return light < 0.4f ? "BROWN" : "YELLOW";
            if (hue < 165f) return "GREEN";
            if (hue < 255f) return "BLUE";
            if (hue < 290f) return "PURPLE";
            return "PINK";
        }

        /// <summary>swatchView: the slot's options in their colour family tabs, in colors.js's order.</summary>
        public static List<SwatchGroup> SwatchGroups(CustomsSlot slot)
        {
            var groups = new List<SwatchGroup>();
            foreach (CustomsOption o in slot.Options)
            {
                string family = FamilyOf(o);
                SwatchGroup g = groups.Find(x => x.Name == family);
                if (g == null) groups.Add(g = new SwatchGroup { Name = family });
                g.Options.Add(o);
            }
            var order = new List<string>(CustomsData.FamilyOrder);
            // a stable sort (List.Sort is not), as the page's Array.sort is
            var sorted = new List<SwatchGroup>();
            var indexed = new List<(SwatchGroup G, int I)>();
            for (int i = 0; i < groups.Count; i++) indexed.Add((groups[i], i));
            indexed.Sort((a, b) =>
            {
                int ia = order.IndexOf(a.G.Name), ib = order.IndexOf(b.G.Name);
                int c = ia == -1 || ib == -1 ? 0 : ia.CompareTo(ib);
                return c != 0 ? c : a.I.CompareTo(b.I);
            });
            foreach (var x in indexed) sorted.Add(x.G);
            return sorted;
        }
    }
}
