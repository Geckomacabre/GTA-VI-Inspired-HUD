"""Leonida Customs' tables for the single-player mod shop, made from the FiveM
resource itself.

    python tools/gen_customs_data.py --customs <FiveM repo>/resources/Lenonida_customs

Writes src/ViCustoms/CustomsData.g.cs: everything the shop reads out of
config.lua, config_menu.lua, shared/customs.lua, client/menu.lua and
html/colors.js, as plain C# data in `CustomsData` (a static partial class; the
shop's code is the other part).

How each file is read:
  config.lua, config_menu.lua  run in Lua 5.4 (lupa) with stand-ins for the
                               FiveM natives they call, then read back. The
                               paint lists are also read off the text, for
                               their order (Lua tables have none, and one paint
                               index has two names in Config.Metallic).
  shared/customs.lua           ModProps and SlotConfigKey, lifted out and run.
  client/menu.lua              its tables (Labels, LevelLabels, ...) lifted out
                               and run, and its own pretty() / hsv() /
                               rgbOptions() run for the labels and presets.
  html/colors.js               read with regular expressions.
What menu.lua does in code (which lists each swatch slot shows, the finish of
each list) is written out below, next to the line it comes from.

Changed from the FiveM resource (single player):
  - Nitrous sells sk_streetkings' tiers, as the FiveM server runs it
    (NITROUS_TIERS, transcribed from its config); Pro Builds and the card
    stats use the tier ids.
  - Renzu's engine swap, turbo kits and tire compounds are left out, with the
    menu categories and Pro Build parts that were made of them.
  - Only the public Los Santos Customs (shops with no job; not Benny's).

The output is the same for the same input (every Lua table is put in a fixed
order), so a rerun with nothing changed leaves the file as it is.
"""
import argparse
import os
import re
import sys

from lupa import lua54

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "src", "ViCustoms", "CustomsData.g.cs")

# Renzu's mechanic upgrades, not ported
DROPPED_SLOTS = ("custom_engine", "custom_turbo", "custom_tires")

# The Nitrous slot's options when the server runs sk_streetkings (menu.lua
# streetkingsNitrous: "No Nitrous" first, then its tiers). Transcribed from
# sk_streetkings/shared/config.lua Config.Nitrous: prices{} and tiers{}.label,
# cheapest first; checked against that file when it is next to the customs.
NITROUS_TIERS = [
    ("Default", "No Nitrous", 0),
    ("street", "Street NOS", 10000),
    ("sport", "Sport NOS", 17500),
    ("race", "Race NOS", 25000),
]
# Pro Builds name the built-in kits (config_menu.lua Config.Nitrous list); the tier each becomes
KIT_NITROUS = {"Street": "street", "Sport": "sport", "Race": "race"}

# menu.lua paintOptions: finishOf, the finish of every index in each list
FINISH_OF_LIST = (("Metallic", "metallic"), ("Matte", "matte"), ("Metals", "metal"), ("Crome", "chrome"),
                  ("Chameleon", "chameleon"))
# menu.lua buildSlot: the lists each swatch slot shows, the Config.VehicleMod entry they are in,
# and the finish forced on all of them
PAINT_SLOTS = (
    ("paint1", "paint1", ("Metallic", "Matte", "Metals", "Crome", "Chameleon"), None),
    ("paint2", "paint2", ("Metallic", "Matte", "Metals", "Crome"), None),
    ("pearl", "paint1", ("Pearlescent",), "pearl"),
    ("wheelcolor", 23, ("WheelColor",), None),
)

# What config.lua and config_menu.lua call that FiveM provides. sk_streetkings is
# 'started' so Config.NitrousSystem resolves to 'streetkings', as on the server.
STUBS = """
function vector3(x, y, z) return { x = x, y = y, z = z } end
function vector4(x, y, z, w) return { x = x, y = y, z = z, w = w } end
function GetResourceState(name) return name == 'sk_streetkings' and 'started' or 'missing' end
function GetGameBuildNumber() return 3095 end
function IsDuplicityVersion() return false end
Config = {}
"""

FUNC = object()


def read(path):
    with open(path, encoding="utf-8") as f:
        return f.read()


def to_py(v):
    """A Lua value as Python: a table with keys 1..n is a list, any other a dict; functions are dropped."""
    t = lua54.lua_type(v)
    if t == "function":
        return FUNC
    if t != "table":
        return v
    keys = list(v.keys())
    if keys and all(type(k) is int for k in keys) and sorted(keys) == list(range(1, len(keys) + 1)):
        return [to_py(v[i]) for i in range(1, len(keys) + 1)]
    out = {}
    for k in keys:
        val = to_py(v[k])
        if val is not FUNC:
            out[k] = val
    return out


def table_text(src, marker):
    """The Lua table constructor {...} that starts at the first '{' from `marker` on."""
    i = src.index("{", src.index(marker))
    depth, j = 0, i
    while j < len(src):
        c = src[j]
        if c in "'\"":
            j += 1
            while src[j] != c:
                j += 2 if src[j] == "\\" else 1
        elif src.startswith("--", j):
            if src.startswith("--[[", j):
                raise ValueError("long comment in " + marker)
            j = src.index("\n", j)
            continue
        elif c == "{":
            depth += 1
        elif c == "}":
            depth -= 1
            if depth == 0:
                return src[i:j + 1]
        j += 1
    raise ValueError("unclosed table after " + marker)


def local_function(src, name):
    """A top-level `local function name(...)` of menu.lua, up to its `end` at the start of a line."""
    m = re.search(r"^local function %s\(.*?^end$" % re.escape(name), src, re.S | re.M)
    if not m:
        raise ValueError("no local function " + name)
    return m.group(0)


def slot_order(key):
    """'mod:<n>' slots by n, then the rest by name; ints before strings."""
    if isinstance(key, int):
        return (0, key, "")
    m = re.match(r"mod:(\d+)$", key)
    return (1, int(m.group(1)), "") if m else (2, 0, key)


def hex_rgb(h):
    h = h.strip().lstrip("#")
    if not re.fullmatch(r"[0-9a-fA-F]{6}", h):
        raise ValueError("not a colour: " + h)
    n = int(h, 16)
    return ((n >> 16) & 255, (n >> 8) & 255, n & 255)


# ---------------------------------------------------------------- reading


def load(customs):
    lua = lua54.LuaRuntime()
    lua.execute(STUBS)
    config_src = read(os.path.join(customs, "config.lua"))
    lua.execute(config_src)
    lua.execute(read(os.path.join(customs, "config_menu.lua")))
    cfg = lua.globals().Config
    get = lambda name: to_py(cfg[name])
    d = {}

    # shops: the public ones (no job), in the file's order
    customs_tbl = get("Customs")
    labels = get("ShopLabels")
    shops = []
    for sid in sorted((k for k, s in customs_tbl.items() if not s.get("job")),
                      key=lambda k: config_src.index("['%s']" % k)):
        s = customs_tbl[sid]
        if len(s["mod"]) != 1:
            raise ValueError(sid + ": expected one upgrade spot")
        c = s["mod"][0]["coord"]
        b = s["Blips"]
        shops.append(dict(id=sid, label=labels.get(sid, sid), x=c["x"], y=c["y"], z=c["z"], heading=c["w"],
                          radius=s["radius"], blip_label=b.get("label", "Mechanic Shop: " + sid),
                          blip_sprite=b["sprite"], blip_color=b["color"], blip_scale=b["scale"]))
    d["shops"] = shops
    d["skipped_shops"] = sorted(k for k, s in customs_tbl.items() if s.get("job"))

    d["repair_cost"] = cfg.RepairCost
    d["free_classes"] = sorted(k for k, v in get("FreeUpgradeToClass").items() if v)
    d["extra_prices"] = get("ExtraPrices")
    d["menu_keys"] = get("MenuKeys")
    d["pad_glyphs"] = cfg.PadGlyphs
    d["menu_dof"] = bool(cfg.MenuDepthOfField)
    if cfg.NitrousSystem != "streetkings":
        raise ValueError("Config.NitrousSystem did not resolve to streetkings")

    # Config.VehicleMod
    vm = get("VehicleMod")
    if "nitrous" in vm:
        raise ValueError("Config.VehicleMod has the built-in nitrous kits")
    costs = {}
    for key in sorted(vm, key=slot_order):
        if key in DROPPED_SLOTS:
            continue
        e = vm[key]
        if e["cost"] != int(e["cost"]):
            raise ValueError("cost of %s is not whole" % key)
        costs[str(key)] = (e["label"], int(e["cost"]), bool(e.get("multicostperlvl")))
    d["mod_costs"] = costs

    wheels = vm[23]["list"]
    d["wheel_types"] = sorted(wheels["WheelType"].items(), key=lambda kv: kv[1])
    d["wheel_accessories"] = sorted(k for k, v in wheels["Accessories"].items() if v)
    d["xenon_list"] = sorted(vm["headlight"]["list"]["XenonColor"].items(), key=lambda kv: kv[1])
    d["plate_list"] = sorted(vm["plate"]["list"].items(), key=lambda kv: kv[1])
    d["window_list"] = sorted(vm["window"]["list"].items(), key=lambda kv: kv[1])

    # paint lists in the file's order, checked against what Lua made of them
    paints = {}
    for name in ("Metallic", "Matte", "Metals", "Chameleon", "Crome"):
        block = table_text(config_src, "Config.%s = {" % name)
        pairs = [(m.group(1), int(m.group(2))) for m in re.finditer(r"\[\s*'([^']*)'\s*\]\s*=\s*(\d+)", block)]
        if len({n for n, _ in pairs}) != len(pairs) or dict(pairs) != get(name):
            raise ValueError("Config.%s: the text and Lua disagree" % name)
        paints[name] = pairs
    if dict(paints["Metallic"]) != vm["paint1"]["list"]["Pearlescent"]:
        raise ValueError("Pearlescent is no longer Config.Metallic")
    paints["Pearlescent"] = list(paints["Metallic"])
    allcolors = {}  # config.lua allcolors: Metallic, Matte, Metals by name, later lists replacing
    for name in ("Metallic", "Matte", "Metals"):
        for n, v in paints[name]:
            allcolors[n] = v
    if allcolors != wheels["WheelColor"]:
        raise ValueError("WheelColor is no longer allcolors")
    paints["WheelColor"] = list(allcolors.items())
    for slot, key, lists, _ in PAINT_SLOTS:
        for name in lists:
            if dict(paints[name]) != vm[key]["list"][name]:
                raise ValueError("%s list %s differs from Config.%s" % (slot, name, name))
    d["paints"] = paints

    finish = {}
    for name, fin in FINISH_OF_LIST:
        for _, index in paints[name]:
            if index in finish and finish[index] != fin:
                raise ValueError("paint %d is in two finishes" % index)
            finish[index] = fin
    d["finish"] = dict(sorted(finish.items()))

    options = {}  # menu.lua paintOptions: first name of an index wins, sorted by index
    for slot, _, lists, forced in PAINT_SLOTS:
        seen, opts = set(), []
        for name in lists:
            for n, index in paints[name]:
                if index not in seen:
                    seen.add(index)
                    opts.append((index, n.lstrip(), forced or finish.get(index, "metallic")))
        options[slot] = sorted(opts)
    d["paint_options"] = options

    # menu, minus the dropped slots and the categories left empty
    tabs, dropped_cats = [], []
    for tab in get("Menu"):
        cats = []
        for cat in tab["categories"]:
            had = cat.get("slots", [])
            slots = [s for s in had if s not in DROPPED_SLOTS]
            if had and not slots:
                dropped_cats.append(cat["label"])
                continue
            cats.append(dict(label=cat["label"], hint=cat.get("hint", ""), camera=cat.get("camera"),
                             kits=bool(cat.get("kits")), slots=slots))
        tabs.append(dict(id=tab["id"], label=tab["label"], camera=tab["camera"], categories=cats))
    d["menu"] = tabs
    d["dropped_categories"] = dropped_cats

    kits = []
    for kit in get("ProBuilds"):
        slots = {}
        for slot in sorted(kit["slots"], key=slot_order):
            if slot in DROPPED_SLOTS:
                continue
            v = kit["slots"][slot]
            if slot == "nitrous":
                v = KIT_NITROUS[v]
            slots[slot] = v
        kits.append(dict(id=kit["id"], label=kit["label"], price=kit["price"], slots=slots))
    d["kits"] = kits

    # card stats; nitrous keeps only the StreetKings tier ids, in tier order
    effects = get("StatEffects")
    flat, listed = {}, {}
    for slot in sorted(effects, key=slot_order):
        if slot in DROPPED_SLOTS:
            continue
        fx = effects[slot]
        if all(isinstance(v, (int, float)) for v in fx.values()):
            flat[slot] = dict(sorted(fx.items()))
        elif all(isinstance(v, dict) for v in fx.values()):
            if slot == "nitrous":
                order = [t[0] for t in NITROUS_TIERS if t[0] in fx]
            else:
                order = sorted(fx)
            listed[slot] = {k: dict(sorted(fx[k].items())) for k in order}
        else:
            raise ValueError("StatEffects " + slot + " mixes amounts and lists")
    d["stat_flat"], d["stat_list"] = flat, listed

    # shared/customs.lua
    shared = read(os.path.join(customs, "shared", "customs.lua"))
    d["mod_props"] = dict(sorted(to_py(lua.eval(table_text(shared, "Customs.ModProps = {"))).items()))
    slot_keys, dropped_keys = {}, []
    for slot, key in sorted(to_py(lua.eval(table_text(shared, "local SlotConfigKey = {"))).items(),
                            key=lambda kv: slot_order(kv[0])):
        if slot in DROPPED_SLOTS or str(key) not in costs:
            dropped_keys.append(slot)
        else:
            slot_keys[slot] = str(key)
    d["slot_keys"], d["dropped_slot_keys"] = slot_keys, dropped_keys

    # client/menu.lua
    menu = read(os.path.join(customs, "client", "menu.lua"))
    tbl = lambda name: to_py(lua.eval(table_text(menu, "local %s = {" % name)))
    d["labels"] = {k: v for k, v in sorted(tbl("Labels").items(), key=lambda kv: slot_order(kv[0]))
                   if k not in DROPPED_SLOTS}
    d["level_labels"] = dict(sorted(tbl("LevelLabels").items()))
    d["wheel_type_labels"] = dict(sorted(tbl("WheelTypeLabels").items()))
    d["slot_cameras"] = dict(sorted(tbl("SlotCameras").items(), key=lambda kv: slot_order(kv[0])))
    d["views"] = dict(sorted(tbl("Views").items()))
    d["neon"] = tbl("NeonColors")
    d["smoke"] = tbl("SmokeColors")
    d["xenon_rgb"] = {k: hex_rgb(v) for k, v in sorted(tbl("XenonColors").items())}

    rgb_src = local_function(menu, "rgbOptions")
    d["rgb_hues"] = to_py(lua.eval(table_text(rgb_src, "local hues = {")))
    d["rgb_tones"] = to_py(lua.eval(table_text(rgb_src, "local tones = {")))
    d["rgb_mono"] = to_py(lua.eval(table_text(rgb_src, "for _, c in ipairs({")))
    fns = lua.execute("\n".join(local_function(menu, n) for n in ("pretty", "hex", "hsv", "rgbOptions"))
                      + "\nreturn { pretty = pretty, rgbOptions = rgbOptions }")
    pretty = fns.pretty
    d["rgb_presets"] = to_py(fns.rgbOptions())
    for key in ("xenon_list", "plate_list", "window_list"):
        d[key] = [(n, v, pretty(n)) for n, v in d[key]]
    return d


def load_colors(customs):
    js = read(os.path.join(customs, "html", "colors.js"))

    def block(marker, close):
        i = js.index(marker)
        return js[i:js.index(close, i)]

    d = {}
    d["gta"] = {int(m.group(1)): hex_rgb(m.group(2))
                for m in re.finditer(r"(\d+):\s*'(#[0-9a-fA-F]{6})'", block("const GTA_COLORS", "};"))}
    d["chameleon_words"] = [(m.group(1), [hex_rgb(h) for h in m.group(2).split(",")])
                            for m in re.finditer(r"([A-Z0-9]+):\s*'([^']+)'", block("const CHAMELEON_WORDS", "};"))]
    d["rainbow"] = [hex_rgb(h) for h in re.search(r"const RAINBOW = '([^']+)'", js).group(1).split(",")]
    d["family_order"] = re.findall(r"'([^']+)'", block("const FAMILY_ORDER", "];"))
    d["family_words"] = re.findall(r"\[/(.+?)/i,\s*'(\w+)'\]", block("const FAMILY_WORDS", "];"))
    d["finish_labels"] = re.findall(r"(\w+):\s*'([^']+)'", block("const FINISH_LABELS", "};"))
    for pattern, _ in d["family_words"]:
        re.compile(pattern, re.I)
    if not d["gta"] or not d["chameleon_words"] or not d["family_words"] or not d["finish_labels"]:
        raise ValueError("colors.js: a table was not found")
    return d


def check_streetkings(customs):
    """NITROUS_TIERS against sk_streetkings/shared/config.lua, when it is beside the customs."""
    path = os.path.join(os.path.dirname(os.path.abspath(customs)), "sk_streetkings", "shared", "config.lua")
    if not os.path.isfile(path):
        print("note: no sk_streetkings beside the customs, nitrous tiers not checked")
        return
    src = read(path)
    nitrous = src[src.index("Nitrous = {"):]
    prices, tiers = table_text(nitrous, "prices = {"), table_text(nitrous, "tiers = {")
    for tid, label, price in NITROUS_TIERS[1:]:
        p = re.search(r"\b%s\s*=\s*(\d+)" % tid, prices)
        lab = re.search(r"\b%s\s*=\s*\{[^}]*label\s*=\s*'([^']*)'" % tid, tiers)
        if not p or int(p.group(1)) != price or not lab or lab.group(1) != label:
            raise ValueError("NITROUS_TIERS %s no longer matches sk_streetkings" % tid)


# ---------------------------------------------------------------- writing


def cs_str(s):
    out = []
    for ch in s:
        if ch == '"':
            out.append('\\"')
        elif ch == "\\":
            out.append("\\\\")
        elif ord(ch) < 32 or ord(ch) > 126:
            out.append("\\u%04x" % ord(ch))
        else:
            out.append(ch)
    return '"' + "".join(out) + '"'


def cs_verbatim(s):
    return '@"' + s.replace('"', '""') + '"'


def cs_float(v):
    r = repr(float(v))
    if not re.fullmatch(r"-?\d+\.\d+", r):
        raise ValueError("float out of range: " + r)
    return r + "f"


def cs_value(v):
    if isinstance(v, bool):
        return "true" if v else "false"
    if isinstance(v, int):
        return str(v)
    if isinstance(v, str):
        return cs_str(v)
    raise ValueError("unsupported value %r" % (v,))


def rgb(c):
    return "(%d, %d, %d)" % tuple(c)


class Writer:
    def __init__(self):
        self.lines = []

    def __call__(self, indent, text=""):
        self.lines.append(("    " * indent + text) if text else "")

    def pack(self, indent, items, width=118):
        """Comma-separated items, as many to a line as fit."""
        line = ""
        for i, item in enumerate(items):
            piece = item + ("," if i < len(items) - 1 else "")
            if line and len("    " * indent + line + " " + piece) > width:
                self(indent, line)
                line = piece
            else:
                line = (line + " " + piece) if line else piece
        if line:
            self(indent, line)

    def array(self, indent, decl, items):
        self(indent, decl)
        self(indent, "{")
        self.pack(indent + 1, items)
        self(indent, "};")

    def dict(self, indent, decl, ctor, pairs):
        if len("    " * indent + decl + " = new " + ctor) <= 118:
            self(indent, decl + " = new " + ctor)
        else:
            self(indent, decl + " =")
            self(indent + 1, "new " + ctor)
        self(indent, "{")
        self.pack(indent + 1, ["[%s] = %s" % kv for kv in pairs])
        self(indent, "};")

    def text(self):
        return "\r\n".join(self.lines) + "\r\n"


def write(d, c):
    w = Writer()
    w(0, "// generated by tools/gen_customs_data.py from Lenonida_customs; do not edit")
    w(0, "//")
    w(0, "// Leonida Customs' tables for the single-player mod shop: config.lua, config_menu.lua,")
    w(0, "// shared/customs.lua, client/menu.lua and html/colors.js of the FiveM resource. Changed")
    w(0, "// for single player: Nitrous sells sk_streetkings' tiers (NitrousTiers); the engine swap,")
    w(0, "// turbo kit and tire compound upgrades are left out, with their menu categories and Pro")
    w(0, "// Build parts; only the public Los Santos Customs are here (not Benny's).")
    w(0)
    w(0, "using System.Collections.Generic;")
    w(0)
    w(0, "namespace ViExtras")
    w(0, "{")
    w(1, "internal static partial class CustomsData")
    w(1, "{")
    w(2, "internal sealed class Shop")
    w(2, "{")
    w(3, "public string Id, Label, BlipLabel;")
    w(3, "public float X, Y, Z, Heading, Radius, BlipScale;")
    w(3, "public int BlipSprite, BlipColor;")
    w(2, "}")
    w(0)
    w(2, "internal sealed class ModCost")
    w(2, "{")
    w(3, "public string Label;")
    w(3, "public int Cost;")
    w(3, "public bool MultiCostPerLevel;")
    w(2, "}")
    w(0)
    w(2, "internal sealed class Tab")
    w(2, "{")
    w(3, "public string Id, Label, Camera;")
    w(3, "public Category[] Categories;")
    w(2, "}")
    w(0)
    w(2, "internal sealed class Category")
    w(2, "{")
    w(3, "public string Label, Hint;")
    w(3, "public string Camera; // null: the tab's")
    w(3, "public bool Kits; // the Pro Builds list, no slots")
    w(3, "public string[] Slots;")
    w(2, "}")
    w(0)
    w(2, "internal sealed class ProBuild")
    w(2, "{")
    w(3, "public string Id, Label;")
    w(3, "public int Price;")
    w(3, "// slot > \"max\" (best part), an int (that part, 0 = first upgrade), true / false, or an option value")
    w(3, "public Dictionary<string, object> Slots;")
    w(2, "}")
    w(0)
    w(2, "internal sealed class View")
    w(2, "{")
    w(3, "public float Yaw, Pitch;")
    w(3, "public float? Dist; // times the vehicle length, plus 1.4 m (wheel: plus 1.0 m); null = 1.0")
    w(3, "public float? Metres; // a fixed distance instead (cabin)")
    w(3, "public string Bone;")
    w(2, "}")

    # shops
    w(0)
    w(2, "// SHOPS (config.lua Config.Customs, config_menu.lua Config.ShopLabels): the upgrade spot of each")
    w(2, "public static readonly Shop[] Shops =")
    w(2, "{")
    for s in d["shops"]:
        w(3, "new Shop { Id = %s, Label = %s," % (cs_str(s["id"]), cs_str(s["label"])))
        w(4, "X = %s, Y = %s, Z = %s, Heading = %s, Radius = %s," % tuple(
            cs_float(s[k]) for k in ("x", "y", "z", "heading", "radius")))
        w(4, "BlipLabel = %s, BlipSprite = %d, BlipColor = %d, BlipScale = %s }," % (
            cs_str(s["blip_label"]), s["blip_sprite"], s["blip_color"], cs_float(s["blip_scale"])))
    w(2, "};")

    w(0)
    w(2, "public const int RepairCost = %d;" % d["repair_cost"])
    w(0)
    w(2, "// vehicle classes upgraded for free (Config.FreeUpgradeToClass)")
    w(2, "public static readonly HashSet<int> FreeUpgradeToClass = new HashSet<int> { %s };"
      % ", ".join(str(x) for x in d["free_classes"]))
    w(0)
    w(2, "// prices of options with no Config.VehicleMod entry of their own (Config.ExtraPrices)")
    w.dict(2, "public static readonly Dictionary<string, int> ExtraPrices", "Dictionary<string, int>",
           [(cs_str(k), str(v)) for k, v in sorted(d["extra_prices"].items())])

    # costs
    w(0)
    w(2, "// Config.VehicleMod by its key: \"0\"..\"48\" for the 'mod:<index>' slots, then the named entries.")
    w(2, "// A part costs Cost, times (part + 1) with MultiCostPerLevel; a free choice (stock, off) costs 0.")
    w(2, "public static readonly Dictionary<string, ModCost> ModCosts = new Dictionary<string, ModCost>")
    w(2, "{")
    for key, (label, cost, multi) in d["mod_costs"].items():
        w(3, "[%s] = new ModCost { Label = %s, Cost = %d, MultiCostPerLevel = %s }," % (
            cs_str(key), cs_str(label), cost, "true" if multi else "false"))
    w(2, "};")
    w(0)
    w(2, "// the ModCosts key of each slot that is not 'mod:<index>' (shared/customs.lua SlotConfigKey);")
    w(2, "// 'extra:<id>' slots use \"extra\". nitrous has none: NitrousTiers holds its prices.")
    w.dict(2, "public static readonly Dictionary<string, string> SlotConfigKey", "Dictionary<string, string>",
           [(cs_str(k), cs_str(v)) for k, v in d["slot_keys"].items()])
    w(0)
    w(2, "// vehicle props key of each 'mod:<index>' slot (shared/customs.lua Customs.ModProps)")
    w.dict(2, "public static readonly Dictionary<int, string> ModProps", "Dictionary<int, string>",
           [(str(k), cs_str(v)) for k, v in d["mod_props"].items()])

    # wheels, lights, plates, windows
    w(0)
    w(2, "// WHEELS (Config.VehicleMod[23].list): wheel types by value, and the tire options on offer")
    w.array(2, "public static readonly (string Name, int Value)[] WheelTypes =",
            ["(%s, %d)" % (cs_str(n), v) for n, v in d["wheel_types"]])
    w.array(2, "public static readonly string[] WheelAccessories =", [cs_str(a) for a in d["wheel_accessories"]])
    w(0)
    w(2, "// headlight colours, plates and window tints by value (Config.VehicleMod headlight / plate /")
    w(2, "// window lists), each with the label menu.lua's pretty() makes of its name")
    for name, key in (("XenonColorList", "xenon_list"), ("PlateList", "plate_list"), ("WindowList", "window_list")):
        w.array(2, "public static readonly (string Name, int Value, string Label)[] %s =" % name,
                ["(%s, %d, %s)" % (cs_str(n), v, cs_str(lab)) for n, v, lab in d[key]])

    # paint
    w(0)
    w(2, "// PAINT (config.lua): name and paint index in the file's order (Chameleon names without their")
    w(2, "// leading tab). Pearlescent is Config.Metallic; WheelColor is config.lua's allcolors, Metallic,")
    w(2, "// Matte and Metals merged by name, a Matte name taking over the Metallic one.")
    for name in ("Metallic", "Matte", "Metals", "Crome", "Chameleon", "Pearlescent", "WheelColor"):
        w.array(2, "public static readonly (string Name, int Index)[] %s =" % name,
                ["(%s, %d)" % (cs_str(n.lstrip()), v) for n, v in d["paints"][name]])
    w(0)
    w(2, "// finish of each paint index (menu.lua paintOptions finishOf); any other index is metallic")
    w.dict(2, "public static readonly Dictionary<int, string> PaintFinish", "Dictionary<int, string>",
           [(str(k), cs_str(v)) for k, v in d["finish"].items()])
    w(0)
    w(2, "// each paint slot's swatches as menu.lua paintOptions lists them: by index, the first name of")
    w(2, "// an index kept (pearl: the Pearlescent list, all with the pearl finish)")
    w(2, "public static readonly Dictionary<string, (int Index, string Label, string Finish)[]> PaintOptions =")
    w(3, "new Dictionary<string, (int Index, string Label, string Finish)[]>")
    w(2, "{")
    for slot, opts in d["paint_options"].items():
        w(3, "[%s] = new[]" % cs_str(slot))
        w(3, "{")
        w.pack(4, ["(%d, %s, %s)" % (i, cs_str(n), cs_str(f)) for i, n, f in opts])
        w(3, "},")
    w(2, "};")

    # menu
    w(0)
    w(2, "// MENU (config_menu.lua Config.Menu): tabs > categories > slots. Slots: 'mod:<index>', turbo,")
    w(2, "// wheels, wheelcolor, customtire, tyresmoke, drift, bulletproof, paint1, paint2, pearl, rgb1, rgb2,")
    w(2, "// xenon, neon, plate, window, nitrous, and extras (one 'extra:<id>' slot per extra the vehicle has).")
    w(2, "public static readonly Tab[] Menu =")
    w(2, "{")
    for tab in d["menu"]:
        w(3, "new Tab")
        w(3, "{")
        w(4, "Id = %s, Label = %s, Camera = %s," % (cs_str(tab["id"]), cs_str(tab["label"]), cs_str(tab["camera"])))
        w(4, "Categories = new[]")
        w(4, "{")
        for cat in tab["categories"]:
            w(5, "new Category { Label = %s, Hint = %s," % (cs_str(cat["label"]), cs_str(cat["hint"])))
            head = "Camera = %s, Kits = %s, Slots = " % (cs_str(cat["camera"]) if cat["camera"] else "null",
                                                       "true" if cat["kits"] else "false")
            items = [cs_str(s) for s in cat["slots"]]
            line = head + ("new[] { %s } }," % ", ".join(items) if items else "new string[0] },")
            if len("    " * 6 + line) <= 118:
                w(6, line)
            else:
                w(6, head + "new[]")
                w(6, "{")
                w.pack(7, items)
                w(6, "} },")
        w(4, "},")
        w(3, "},")
    w(2, "};")

    w(0)
    w(2, "// PRO BUILDS (config_menu.lua Config.ProBuilds): one purchase fitting every listed part the vehicle")
    w(2, "// takes, for Price or what those parts cost one by one, whichever is less")
    w(2, "public static readonly ProBuild[] ProBuilds =")
    w(2, "{")
    for kit in d["kits"]:
        w(3, "new ProBuild")
        w(3, "{")
        w(4, "Id = %s, Label = %s, Price = %d," % (cs_str(kit["id"]), cs_str(kit["label"]), kit["price"]))
        w(4, "Slots = new Dictionary<string, object>")
        w(4, "{")
        w.pack(5, ["[%s] = %s" % (cs_str(k), cs_value(v)) for k, v in kit["slots"].items()])
        w(4, "},")
        w(3, "},")
    w(2, "};")

    w(0)
    w(2, "// NITROUS: the options of the Nitrous slot, sk_streetkings' tiers (its shared/config.lua")
    w(2, "// Config.Nitrous prices and labels) after menu.lua's \"No Nitrous\"")
    w.array(2, "public static readonly (string Id, string Label, int Price)[] NitrousTiers =",
            ["(%s, %s, %d)" % (cs_str(i), cs_str(lab), p) for i, lab, p in NITROUS_TIERS])

    w(0)
    w(2, "// VEHICLE CARD (config_menu.lua Config.StatEffects): what a slot adds to the 0-10 bars (speed,")
    w(2, "// acceleration, asphalt, offroad, strength); a 'mod:<index>' slot per level (part + 1), a toggle once")
    w(2, "public static readonly Dictionary<string, Dictionary<string, float>> StatEffects =")
    w(3, "new Dictionary<string, Dictionary<string, float>>")
    w(2, "{")
    for slot, fx in d["stat_flat"].items():
        w(3, "[%s] = new Dictionary<string, float> { %s }," % (
            cs_str(slot), ", ".join("[%s] = %s" % (cs_str(k), cs_float(v)) for k, v in fx.items())))
    w(2, "};")
    w(0)
    w(2, "// the same for list slots, by the option fitted")
    w(2, "public static readonly Dictionary<string, Dictionary<string, Dictionary<string, float>>> ListStatEffects =")
    w(3, "new Dictionary<string, Dictionary<string, Dictionary<string, float>>>")
    w(2, "{")
    for slot, entries in d["stat_list"].items():
        w(3, "[%s] = new Dictionary<string, Dictionary<string, float>>" % cs_str(slot))
        w(3, "{")
        for value, fx in entries.items():
            w(4, "[%s] = new Dictionary<string, float> { %s }," % (
                cs_str(value), ", ".join("[%s] = %s" % (cs_str(k), cs_float(v)) for k, v in fx.items())))
        w(3, "},")
    w(2, "};")

    # menu.lua
    w(0)
    w(2, "// MENU TEXT AND CAMERA (client/menu.lua)")
    w(2, "// slot names where they are not Config.VehicleMod's label (Labels)")
    w.dict(2, "public static readonly Dictionary<string, string> Labels", "Dictionary<string, string>",
           [(cs_str(k), cs_str(v)) for k, v in d["labels"].items()])
    w(0)
    w(2, "// names of the performance parts by mod index, the game has no text for them (LevelLabels)")
    w(2, "public static readonly Dictionary<int, string[]> LevelLabels = new Dictionary<int, string[]>")
    w(2, "{")
    for k, v in d["level_labels"].items():
        line = "[%d] = new[] { %s }," % (k, ", ".join(cs_str(x) for x in v))
        if len("    " * 3 + line) <= 118:
            w(3, line)
        else:
            w(3, "[%d] = new[]" % k)
            w(3, "{")
            w.pack(4, [cs_str(x) for x in v])
            w(3, "},")
    w(2, "};")
    w(0)
    w(2, "// wheel type names where they are not the WheelTypes name (WheelTypeLabels)")
    w.dict(2, "public static readonly Dictionary<string, string> WheelTypeLabels", "Dictionary<string, string>",
           [(cs_str(k), cs_str(v)) for k, v in d["wheel_type_labels"].items()])
    w(0)
    w(2, "// camera view of a slot, when it is not the one of its category (SlotCameras)")
    w.dict(2, "public static readonly Dictionary<string, string> SlotCameras", "Dictionary<string, string>",
           [(cs_str(k), cs_str(v)) for k, v in d["slot_cameras"].items()])
    w(0)
    w(2, "// the camera views (Views): yaw and pitch in degrees round the vehicle")
    w(2, "public static readonly Dictionary<string, View> Views = new Dictionary<string, View>")
    w(2, "{")
    for name, v in d["views"].items():
        parts = ["Yaw = " + cs_float(v["yaw"]), "Pitch = " + cs_float(v["pitch"])]
        if "dist" in v:
            parts.append("Dist = " + cs_float(v["dist"]))
        if "metres" in v:
            parts.append("Metres = " + cs_float(v["metres"]))
        if "bone" in v:
            parts.append("Bone = " + cs_str(v["bone"]))
        extra = set(v) - {"yaw", "pitch", "dist", "metres", "bone"}
        if extra:
            raise ValueError("view %s has %s" % (name, sorted(extra)))
        w(3, "[%s] = new View { %s }," % (cs_str(name), ", ".join(parts)))
    w(2, "};")
    w(0)
    w(2, "// neon and tyre smoke presets (NeonColors, SmokeColors), after menu.lua's \"None\"")
    for name, key in (("NeonColors", "neon"), ("SmokeColors", "smoke")):
        w.array(2, "public static readonly (string Name, int R, int G, int B)[] %s =" % name,
                ["(%s, %d, %d, %d)" % (cs_str(e[0]), e[1], e[2], e[3]) for e in d[key]])
    w(0)
    w(2, "// headlight swatch colour by xenon colour value, -1 = plain xenon (XenonColors)")
    w.dict(2, "public static readonly Dictionary<int, (int R, int G, int B)> XenonColors",
           "Dictionary<int, (int R, int G, int B)>", [(str(k), rgb(v)) for k, v in d["xenon_rgb"].items()])
    w(0)
    w(2, "// custom RGB paint (rgbOptions): every hue in each tone (HSV saturation, value), then the greys")
    w.array(2, "public static readonly string[] RgbHues =", [cs_str(h) for h in d["rgb_hues"]])
    w.array(2, "public static readonly (string Group, string Prefix, float Saturation, float Value)[] RgbTones =",
            ["(%s, %s, %s, %s)" % (cs_str(t[0]), cs_str(t[1]), cs_float(t[2]), cs_float(t[3])) for t in d["rgb_tones"]])
    w.array(2, "public static readonly (string Name, int R, int G, int B)[] RgbMono =",
            ["(%s, %d, %d, %d)" % (cs_str(e[0]), e[1], e[2], e[3]) for e in d["rgb_mono"]])
    w(0)
    w(2, "// rgbOptions() itself, as menu.lua builds it: value \"r,g,b\", label, group; the first is the")
    w(2, "// factory paint (value \"off\", R G B -1)")
    presets = []
    for o in d["rgb_presets"]:
        r, g, b = hex_rgb(o["hex"]) if "hex" in o else (-1, -1, -1)
        presets.append("(%s, %s, %s, %d, %d, %d)" % (cs_str(o["value"]), cs_str(o["label"]), cs_str(o["group"]), r, g, b))
    w.array(2, "public static readonly (string Value, string Label, string Group, int R, int G, int B)[] RgbPresets =",
            presets)

    # colors.js
    w(0)
    w(2, "// SWATCH COLOURS (html/colors.js)")
    w(2, "// GTA paint index > colour (GTA_COLORS); colors.js shows any other index as #888888")
    w.dict(2, "public static readonly Dictionary<int, (int R, int G, int B)> GtaColors",
           "Dictionary<int, (int R, int G, int B)>", [(str(k), rgb(v)) for k, v in sorted(c["gta"].items())])
    w(0)
    w(2, "// the colour words in chameleon paint names > gradient stops (CHAMELEON_WORDS); a name with none")
    w(2, "// of them uses Rainbow")
    w(2, "public static readonly Dictionary<string, (int R, int G, int B)[]> ChameleonWords =")
    w(3, "new Dictionary<string, (int R, int G, int B)[]>")
    w(2, "{")
    w.pack(3, ["[%s] = new[] { %s }" % (cs_str(k), ", ".join(rgb(s) for s in v)) for k, v in c["chameleon_words"]])
    w(2, "};")
    w.array(2, "public static readonly (int R, int G, int B)[] Rainbow =", [rgb(s) for s in c["rainbow"]])
    w(0)
    w(2, "// colour families, in the order the swatches are grouped (FAMILY_ORDER)")
    w.array(2, "public static readonly string[] FamilyOrder =", [cs_str(f) for f in c["family_order"]])
    w(0)
    w(2, "// a paint's family by its name, first match wins (FAMILY_WORDS); Regex patterns, use IgnoreCase")
    w(2, "public static readonly (string Pattern, string Family)[] FamilyWords =")
    w(2, "{")
    for pattern, family in c["family_words"]:
        w(3, "(%s, %s)," % (cs_verbatim(pattern), cs_str(family)))
    w(2, "};")
    w(0)
    w(2, "// finish > the name shown for it (FINISH_LABELS)")
    w.dict(2, "public static readonly Dictionary<string, string> FinishLabels", "Dictionary<string, string>",
           [(cs_str(k), cs_str(v)) for k, v in c["finish_labels"]])

    # keys
    w(0)
    w(2, "// MENU KEYS (config_menu.lua): keyboard keys as KeyboardEvent.code, the pad glyph style, and")
    w(2, "// whether the background is blurred while the menu is open")
    w.dict(2, "public static readonly Dictionary<string, string> MenuKeys", "Dictionary<string, string>",
           [(cs_str(k), cs_str(v)) for k, v in sorted(d["menu_keys"].items())])
    w(2, "public const string PadGlyphs = %s;" % cs_str(d["pad_glyphs"]))
    w(2, "public const bool MenuDepthOfField = %s;" % ("true" if d["menu_dof"] else "false"))
    w(1, "}")
    w(0, "}")
    return w.text()


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--customs", required=True, help="path to the FiveM resources/Lenonida_customs folder")
    args = ap.parse_args()
    customs = os.path.abspath(args.customs)
    if not os.path.isfile(os.path.join(customs, "config_menu.lua")):
        sys.exit(f"no config_menu.lua in {customs}")
    check_streetkings(customs)
    d = load(customs)
    c = load_colors(customs)
    text = write(d, c)
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, "wb") as f:
        f.write(text.encode("utf-8"))
    print(f"customs: {len(d['shops'])} shops (left out: {', '.join(d['skipped_shops'])}), "
          f"{len(d['mod_costs'])} mod costs, {sum(len(t['categories']) for t in d['menu'])} categories "
          f"(dropped: {', '.join(d['dropped_categories'])}), {len(d['kits'])} pro builds")
    print(f"slot keys left out: {', '.join(d['dropped_slot_keys'])}")
    print("wrote " + os.path.relpath(OUT, ROOT))


if __name__ == "__main__":
    main()
