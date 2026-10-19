using System;
using System.Collections.Generic;
using System.Globalization;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// What is on a vehicle, slot by slot, and putting a slot's value on it:
    /// Leonida Customs' Customs.StateFromProps (shared/customs.lua) and applySlot
    /// (client/menu.lua), read and written with the game's natives instead of
    /// through vehicle props.
    ///
    /// A slot's value is a string here, so every kind compares the same way:
    ///   mod:N      "-1" (stock) .. "n-1"           turbo, drift, bulletproof,
    ///   wheels     "stock" or "type:index"          customtire, extra:N  "true" / "false"
    ///   paint1/2, pearl, wheelcolor, plate, window  the game's index
    ///   rgb1/2, neon, tyresmoke                     "r,g,b" or "off"
    ///   xenon      "off", "-1" (plain xenon) or 0..12 (a colour)
    ///   nitrous    "Default" or a StreetKings tier; kit "none" or a Pro Build id
    /// </summary>
    internal static class CustomsCar
    {
        /// <summary>SET_VEHICLE_MOD, only on a car with a mod kit and only with an index it has.</summary>
        public static void SetMod(Vehicle v, int mod, int index, bool custom)
        {
            if (v == null || !v.Exists() || Function.Call<int>(Hash.GET_NUM_MOD_KITS, v) <= 0) return;
            if (index >= Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, v, mod)) return;
            Function.Call(Hash.SET_VEHICLE_MOD, v, mod, Math.Max(-1, index), custom);
        }

        public const string On = "true", Off = "false";

        public static int ModIndex(string slot) =>
            slot != null && slot.StartsWith("mod:") && int.TryParse(slot.Substring(4), out int n) ? n : -1;

        public static int ExtraIndex(string slot) =>
            slot != null && slot.StartsWith("extra:") && int.TryParse(slot.Substring(6), out int n) ? n : -1;

        public static int Int(string v, int fallback = 0) =>
            int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : fallback;

        public static bool Bool(string v) => v == On;

        public static string Str(int n) => n.ToString(CultureInfo.InvariantCulture);

        public static string Str(bool b) => b ? On : Off;

        /// <summary>"r,g,b" from three ints, as Customs.Rgb.</summary>
        public static string Rgb(int r, int g, int b) => r + "," + g + "," + b;

        /// <summary>Customs.ParseRgb: null for anything that is not "r,g,b".</summary>
        public static int[] ParseRgb(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            string[] p = s.Split(',');
            if (p.Length != 3) return null;
            var c = new int[3];
            for (int i = 0; i < 3; i++)
            {
                if (!int.TryParse(p[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out c[i]) || c[i] < 0) return null;
                c[i] = Math.Min(c[i], 255);
            }
            return c;
        }

        // ---- reading (StateFromProps) ------------------------------------------------------

        public static Dictionary<string, string> Read(Vehicle v, IEnumerable<string> slots)
        {
            var s = new Dictionary<string, string>();
            foreach (string slot in slots) s[slot] = ReadSlot(v, slot);
            return s;
        }

        public static string ReadSlot(Vehicle v, string slot)
        {
            int mod = ModIndex(slot);
            if (mod >= 0)
            {
                if (mod == 48 && Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, v, 48) <= 0)
                    return Str(Function.Call<int>(Hash.GET_VEHICLE_LIVERY, v));
                return Str(Function.Call<int>(Hash.GET_VEHICLE_MOD, v, mod));
            }
            int extra = ExtraIndex(slot);
            if (extra >= 0) return Str(Function.Call<bool>(Hash.IS_VEHICLE_EXTRA_TURNED_ON, v, extra));
            switch (slot)
            {
                case "turbo": return Str(Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, v, 18));
                case "wheels":
                    {
                        int wheel = Function.Call<int>(Hash.GET_VEHICLE_MOD, v, 23);
                        return wheel < 0 ? "stock" : Function.Call<int>(Hash.GET_VEHICLE_WHEEL_TYPE, v) + ":" + wheel;
                    }
                case "wheelcolor": ExtraColours(v, out _, out int wheelColour); return Str(wheelColour);
                case "pearl": ExtraColours(v, out int pearl, out _); return Str(pearl);
                case "paint1": Colours(v, out int p1, out _); return Str(p1);
                case "paint2": Colours(v, out _, out int p2); return Str(p2);
                case "rgb1":
                    if (!Function.Call<bool>(Hash.GET_IS_VEHICLE_PRIMARY_COLOUR_CUSTOM, v)) return "off";
                    return ReadRgb(v, Hash.GET_VEHICLE_CUSTOM_PRIMARY_COLOUR);
                case "rgb2":
                    if (!Function.Call<bool>(Hash.GET_IS_VEHICLE_SECONDARY_COLOUR_CUSTOM, v)) return "off";
                    return ReadRgb(v, Hash.GET_VEHICLE_CUSTOM_SECONDARY_COLOUR);
                case "xenon":
                    {
                        if (!Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, v, 22)) return "off";
                        int c = Function.Call<int>(Hash.GET_VEHICLE_XENON_LIGHT_COLOR_INDEX, v);
                        return Str(c < 0 || c > 12 ? -1 : c);
                    }
                case "neon":
                    {
                        bool any = false;
                        for (int i = 0; i < 4; i++) any |= Function.Call<bool>(Hash.GET_VEHICLE_NEON_ENABLED, v, i);
                        return any ? ReadRgb(v, Hash.GET_VEHICLE_NEON_COLOUR) : "off";
                    }
                case "plate": return Str(Function.Call<int>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, v));
                case "window": return Str(Math.Max(0, Function.Call<int>(Hash.GET_VEHICLE_WINDOW_TINT, v)));
                case "tyresmoke":
                    if (!Function.Call<bool>(Hash.IS_TOGGLE_MOD_ON, v, 20)) return "off";
                    return ReadRgb(v, Hash.GET_VEHICLE_TYRE_SMOKE_COLOR);
                case "drift": return Str(Function.Call<bool>(Hash.GET_DRIFT_TYRES_SET, v));
                case "bulletproof": return Str(!Function.Call<bool>(Hash.GET_VEHICLE_TYRES_CAN_BURST, v));
                case "customtire": return Str(Function.Call<bool>(Hash.GET_VEHICLE_MOD_VARIATION, v, 23));
            }
            return null;
        }

        private static void Colours(Vehicle v, out int primary, out int secondary)
        {
            var a = new OutputArgument();
            var b = new OutputArgument();
            Function.Call(Hash.GET_VEHICLE_COLOURS, v, a, b);
            primary = a.GetResult<int>();
            secondary = b.GetResult<int>();
        }

        private static void ExtraColours(Vehicle v, out int pearl, out int wheel)
        {
            var a = new OutputArgument();
            var b = new OutputArgument();
            Function.Call(Hash.GET_VEHICLE_EXTRA_COLOURS, v, a, b);
            pearl = a.GetResult<int>();
            wheel = b.GetResult<int>();
        }

        private static string ReadRgb(Vehicle v, Hash native)
        {
            var r = new OutputArgument();
            var g = new OutputArgument();
            var b = new OutputArgument();
            Function.Call(native, v, r, g, b);
            return Rgb(r.GetResult<int>(), g.GetResult<int>(), b.GetResult<int>());
        }

        // ---- writing (applySlot) ----------------------------------------------------------

        /// <summary>
        /// Puts `slot` on the vehicle as `view` has it (what is owned, or that plus
        /// the option being looked at). `stockWheelType` is the wheel type the car
        /// rolled in with, for "Stock Wheels".
        /// </summary>
        public static void Apply(Vehicle v, string slot, IDictionary<string, string> view, int stockWheelType)
        {
            view.TryGetValue(slot, out string value);
            int mod = ModIndex(slot);
            if (mod == 48)
            {
                if (Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, v, 48) > 0) SetMod(v, 48, Int(value, -1), false);
                else Function.Call(Hash.SET_VEHICLE_LIVERY, v, Int(value, -1));
                return;
            }
            if (mod >= 0)
            {
                bool custom = mod == 24 && view.TryGetValue("customtire", out string ct) && Bool(ct);
                SetMod(v, mod, Int(value, -1), custom);
                return;
            }
            int extra = ExtraIndex(slot);
            if (extra >= 0)
            {
                Function.Call(Hash.SET_VEHICLE_EXTRA, v, extra, Bool(value) ? 0 : 1);
                return;
            }
            switch (slot)
            {
                case "turbo":
                    Function.Call(Hash.TOGGLE_VEHICLE_MOD, v, 18, Bool(value));
                    break;
                case "wheels":
                case "customtire":
                    {
                        view.TryGetValue("wheels", out string wheels);
                        bool custom = view.TryGetValue("customtire", out string ct) && Bool(ct);
                        string[] p = (wheels ?? "").Split(':');
                        if (p.Length == 2 && int.TryParse(p[0], out int type) && int.TryParse(p[1], out int index))
                        {
                            Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, v, type);
                            SetMod(v, 23, index, custom);
                        }
                        else
                        {
                            Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, v, stockWheelType);
                            SetMod(v, 23, -1, custom);
                        }
                        break;
                    }
                case "wheelcolor":
                    ExtraColours(v, out int pearl, out _);
                    Function.Call(Hash.SET_VEHICLE_EXTRA_COLOURS, v, pearl, Int(value));
                    break;
                case "pearl":
                    ExtraColours(v, out _, out int wheel);
                    Function.Call(Hash.SET_VEHICLE_EXTRA_COLOURS, v, Int(value), wheel);
                    break;
                case "paint1":
                case "paint2":
                case "rgb1":
                case "rgb2":
                    ApplyPaint(v, view);
                    break;
                case "xenon":
                    Function.Call(Hash.TOGGLE_VEHICLE_MOD, v, 22, value != "off");
                    if (value != "off")
                    {
                        int c = Int(value, -1);
                        Function.Call(Hash.SET_VEHICLE_XENON_LIGHT_COLOR_INDEX, v, c < 0 ? 255 : c);
                    }
                    Function.Call(Hash.SET_VEHICLE_LIGHTS, v, 2);
                    break;
                case "neon":
                    {
                        int[] c = value != "off" ? ParseRgb(value) : null;
                        for (int i = 0; i < 4; i++) Function.Call(Hash.SET_VEHICLE_NEON_ENABLED, v, i, c != null);
                        if (c != null) Function.Call(Hash.SET_VEHICLE_NEON_COLOUR, v, c[0], c[1], c[2]);
                        break;
                    }
                case "plate":
                    Function.Call(Hash.SET_VEHICLE_NUMBER_PLATE_TEXT_INDEX, v, Int(value));
                    break;
                case "window":
                    // a factory tint reads -1 and shows as 0; putting 0 back would strip it
                    if (Int(value) == 0 && Function.Call<int>(Hash.GET_VEHICLE_WINDOW_TINT, v) < 0) break;
                    Function.Call(Hash.SET_VEHICLE_WINDOW_TINT, v, Int(value));
                    break;
                case "tyresmoke":
                    {
                        int[] c = value != "off" ? ParseRgb(value) : null;
                        Function.Call(Hash.TOGGLE_VEHICLE_MOD, v, 20, c != null);
                        if (c != null) Function.Call(Hash.SET_VEHICLE_TYRE_SMOKE_COLOR, v, c[0], c[1], c[2]);
                        break;
                    }
                case "drift":
                    Function.Call(Hash.SET_DRIFT_TYRES, v, Bool(value));
                    break;
                case "bulletproof":
                    Function.Call(Hash.SET_VEHICLE_TYRES_CAN_BURST, v, !Bool(value));
                    break;
                // nitrous and Pro Builds live in the save, not on the vehicle
            }
        }

        // a respray covers the custom RGB paint on the same panel (menu.lua applyPaint)
        private static void ApplyPaint(Vehicle v, IDictionary<string, string> view)
        {
            Function.Call(Hash.CLEAR_VEHICLE_CUSTOM_PRIMARY_COLOUR, v);
            Function.Call(Hash.CLEAR_VEHICLE_CUSTOM_SECONDARY_COLOUR, v);
            view.TryGetValue("paint1", out string p1);
            view.TryGetValue("paint2", out string p2);
            Colours(v, out int was1, out int was2);
            Function.Call(Hash.SET_VEHICLE_COLOURS, v, p1 != null ? Int(p1) : was1, p2 != null ? Int(p2) : was2);
            if (view.TryGetValue("rgb1", out string r1) && r1 != "off" && ParseRgb(r1) is int[] c1)
                Function.Call(Hash.SET_VEHICLE_CUSTOM_PRIMARY_COLOUR, v, c1[0], c1[1], c1[2]);
            if (view.TryGetValue("rgb2", out string r2) && r2 != "off" && ParseRgb(r2) is int[] c2)
                Function.Call(Hash.SET_VEHICLE_CUSTOM_SECONDARY_COLOUR, v, c2[0], c2[1], c2[2]);
        }
    }
}
