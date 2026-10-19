"""Radio: the FiveM version's radio wheel (resources/vi_radio), rendered from
vi_radio's OWN page (vi_radio/html/index.html), not vice_hud's.

vi_radio is a FiveM port of sej0bec's single-player "VI Radio" mod. The station
logos, icons and sound effects are sej0bec's (nine logos from Logopedia, see
vi_radio/README.md), redistributed with credit; vi_radio's own code is under
the PolyForm Noncommercial licence (src/ViExtras/Radio/LICENSE.md).

The page is driven with the messages vi_radio's client.lua sends it
({action: 'state', hud, list, index, muted, open}) and Config.Hud's values,
read here from vi_radio's config.lua together with Config.Stations, so the
art follows that file. With On Demand switched off (single player has no
xsound), hud.odSwitch is false, exactly as client.lua's hudPayload sends it.

The single-player look (SP_STYLE) follows the user's reference shots of the
wheel rather than vi_radio's own: the logo fills the tile with no frame (the
box stays behind logos with transparent corners), every tile at opacity 1 and
scale 1, and the text plain white without the page's shadow, at the shots'
sizes. The scripts tint the text grey, draw the selected tile's outline and
fade the outer tiles themselves.

Pictures (CSS px at 1080p in manifest_radio.txt):
  radio_tile_<key>      a station's tile
  radio_name_<key>      #station's text for that station (box: the text's
                        advance width x the element's line box)
  radio_genre_<g>       #artist's text for a genre
  radio_icon_left       #icon-left with On Demand available (single player's is
  radio_icon_left_od      your own music, ViExtras CustomMusic): the redrawn
  radio_icon_left_muted   radio / On Demand switch, knob at RADIO, and _od with
                        it thrown to ON DEMAND (#radio.od-on), at opacity 1 (the
                        script applies .side-icon's .9); _muted has the muted
                        filter (grayscale(1) brightness(.62)). The static
                        On_Demand.png is hidden, as applyHud hides it when
                        hud.odSwitch is on.
  radio_icon_mute       #icon-right's mask (Mute.png / Unmute.png) filled white
  radio_icon_unmute       at opacity 1; the script tints it (--mute-color /
                        --muted-color) and applies the opacity

<key> is the station's game name, lower case, without its RADIO_ prefix
(RADIO_01_CLASS_ROCK -> 01_class_rock); "off" is the RADIO OFF tile. Also
writes scripts/ViExtras/Radio/stations.txt (the station table) and copies the
two sound effects and the Anton font beside it.
"""
import os
import re
import shutil

GROUP = "radio"

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
RADIO_OUT = os.path.join(ROOT, "scripts", "ViExtras", "Radio")

# ctx.page's INIT, for vi_radio. Its app.js names itself ('vi_radio') and posts
# only from the On Demand panel, which never opens here.
INIT = """
window.GetParentResourceName = function () { return 'vi_radio'; };
const realFetch = window.fetch;
window.fetch = function (url, opts) {
    if (String(url).startsWith('https://vi_radio/')) return Promise.resolve(new Response('{}'));
    return realFetch(url, opts);
};
"""

# The single-player look, measured off the user's reference shots (tile 128 px
# there = 103 here): logo-filled tiles without a frame, plain text, the station
# name at 25.3 px and the song / artist lines at 20.6 px (Anton's capitals are
# 0.859 em tall).
SP_STYLE = (".tile, .tile.active { background: var(--box) !important; border: 0 !important; "
            "opacity: 1 !important; transform: none !important; } "
            ".tile img { width: 100% !important; height: 100% !important; object-fit: cover !important; } "
            "#station, #title, #artist { text-shadow: none !important; opacity: 1 !important; } "
            ":root { --station-size: 25.3px !important; --title-size: 20.6px !important; "
            "--artist-size: 20.6px !important; }")

# Static captures: no transitions, so every state is its end state. The info
# line is a plain white bar the script draws itself; hidden so it is not in the
# station-name pictures.
STILL = ("*, *::before, *::after { transition: none !important; animation: none !important; } "
         "#info-line { visibility: hidden !important; }")


def resource_dir(ctx):
    """vi_radio sits beside vice_hud in resources/ (--vice-hud's sibling)."""
    index = ctx.index.replace("file:///", "").replace("/", os.sep)
    res = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(index))), "vi_radio")
    if not os.path.isfile(os.path.join(res, "html", "index.html")):
        raise SystemExit(f"radio: no vi_radio beside vice_hud ({res})")
    return res


def read_config(res):
    """Config.Hud and Config.Stations / OffLabel / OnDemand.enabled out of config.lua."""
    with open(os.path.join(res, "config.lua"), encoding="utf-8") as f:
        src = f.read()
    hud_block = re.search(r"Config\.Hud\s*=\s*\{(.*?)\n\}", src, re.S).group(1)
    hud = {}
    for k, v in re.findall(r"^\s*(\w+)\s*=\s*([^,\n]+?)\s*,?\s*(?:--.*)?$", hud_block, re.M):
        v = v.strip()
        if v.startswith("'") or v.startswith('"'):
            hud[k] = v[1:-1]
        elif v in ("true", "false"):
            hud[k] = v == "true"
        else:
            hud[k] = float(v) if "." in v else int(v)
    stations = []
    st_block = re.search(r"Config\.Stations\s*=\s*\{(.*?)\n\}", src, re.S).group(1)
    for name, body in re.findall(r"\[\s*'([^']+)'\s*\]\s*=\s*\{([^}]*)\}", st_block):
        fields = dict(re.findall(r"(\w+)\s*=\s*'([^']*)'", body))
        hidden = re.search(r"hidden\s*=\s*true", body) is not None
        stations.append({"name": name, "label": fields.get("label", name), "logo": fields.get("logo"),
                         "genre": fields.get("genre"), "hidden": hidden})
    off_label = re.search(r"Config\.OffLabel\s*=\s*'([^']*)'", src).group(1)
    return hud, stations, off_label


def key_of(name):
    k = name.lower()
    return k[6:] if k.startswith("radio_") else k


def data_url(res, rel):
    """A page file as a data: URL. The mute indicator is a CSS mask, and a mask
    from a file:// page is cross-origin, so Chrome drops it; in FiveM the page
    and the PNG share an origin. Same file, same pixels."""
    import base64
    with open(os.path.join(res, "html", *rel.split("/")), "rb") as f:
        return "data:image/png;base64," + base64.b64encode(f.read()).decode("ascii")


def slug(text):
    return re.sub(r"[^a-z0-9]+", "_", text.lower()).strip("_")


def radio_page(browser, ctx, res, scale=2):
    p = browser.new_page(viewport={"width": 1920, "height": 1080}, device_scale_factor=scale)
    p.add_init_script(INIT)
    p.goto("file:///" + os.path.join(res, "html", "index.html").replace("\\", "/"))
    p.wait_for_load_state("load")
    p.add_style_tag(content=STILL)
    p.evaluate("document.fonts.ready.then(() => true)")
    return p


def settle(page):
    """Every <img> and mask picture loaded, fonts in, a frame painted."""
    page.evaluate("""() => Promise.all([document.fonts.ready,
        ...[...document.images].map(i => i.complete ? 0 : new Promise(r => { i.onload = i.onerror = r; }))])""")
    page.wait_for_timeout(120)


def state(ctx, page, **msg):
    msg["action"] = "state"
    msg.setdefault("open", True)
    ctx.post(page, msg)
    settle(page)


TILE_BOX = """() => {
    const t = document.querySelector('#track .tile');
    const b = t.getBoundingClientRect();
    const cx = b.left + b.width / 2, cy = b.top + b.height / 2, w = t.offsetWidth, h = t.offsetHeight;
    return { x: cx - w / 2, y: cy - h / 2, width: w, height: h };
}"""

TEXT_BOX = """sel => {
    const el = document.querySelector(sel);
    const e = el.getBoundingClientRect();
    const g = document.createRange(); g.selectNodeContents(el);
    const r = g.getBoundingClientRect();
    const left = Math.max(e.left, r.left), right = Math.min(e.right, r.right);
    return { x: left, y: e.top, width: right - left, height: e.height };
}"""


def render(browser, ctx):
    res = resource_dir(ctx)
    hud, stations, off_label = read_config(res)
    hud["odSwitch"] = False   # Config.OnDemand.enabled forced off: no xsound in single player
    hud["rightIcon"] = data_url(res, hud["rightIcon"])
    hud["unmuteIcon"] = data_url(res, hud["unmuteIcon"])
    page = radio_page(browser, ctx, res)
    page.add_style_tag(content=SP_STYLE)

    entries = [s for s in stations if not s["hidden"]]
    entries.append({"name": "OFF", "label": off_label, "logo": None, "genre": None, "hidden": False})
    genres_done = set()
    rows = []

    for st in entries:
        key = "off" if st["name"] == "OFF" else key_of(st["name"])
        item = {"label": st["label"], "logo": ("logos/" + st["logo"]) if st["logo"] else None,
                "genre": st["genre"], "off": st["name"] == "OFF"}
        # One tile on its own, so nothing else is in the shot.
        state(ctx, page, hud=hud, list=[item], index=0, muted=False)
        ctx.box(page, page.evaluate(TILE_BOX), f"radio_tile_{key}", pad=4)
        ctx.box(page, page.evaluate(TEXT_BOX, "#station"), f"radio_name_{key}", pad=12)
        genre_key = ""
        if st["genre"]:
            genre_key = slug(st["genre"])
            if genre_key not in genres_done:
                genres_done.add(genre_key)
                ctx.box(page, page.evaluate(TEXT_BOX, "#artist"), f"radio_genre_{genre_key}", pad=12)


        rows.append("\t".join([st["name"], st["label"], st["genre"] or "", key, genre_key]))

    # Side icons, at opacity 1. On Demand is on in single player (your own
    # music): the left icon is the switch, knob at RADIO, then at ON DEMAND.
    state(ctx, page, hud=dict(hud, odSwitch=True), list=[], index=0, muted=False, odOn=False)
    page.add_style_tag(content="#icon-left { opacity: 1 !important; } "
                               "#icon-left-img { display: none !important; } "
                               "#icon-right { opacity: 1 !important; background-color: #fff !important; }")
    settle(page)
    ctx.element(page, "#icon-left", "radio_icon_left", pad=12)
    ctx.element(page, "#icon-right", "radio_icon_mute", pad=4)
    state(ctx, page, index=0, muted=False, odOn=True)
    page.wait_for_timeout(250)   # the knob's 160 ms background transition
    ctx.element(page, "#icon-left", "radio_icon_left_od", pad=12)
    state(ctx, page, index=0, muted=True, odOn=False)
    page.wait_for_timeout(250)
    page.wait_for_timeout(200)   # the mask picture's aspect ratio is read off the file
    ctx.element(page, "#icon-left", "radio_icon_left_muted", pad=12)
    ctx.element(page, "#icon-right", "radio_icon_unmute", pad=4)
    page.close()

    os.makedirs(RADIO_OUT, exist_ok=True)
    with open(os.path.join(RADIO_OUT, "stations.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("# Written by tools/render/radio.py from vi_radio's config.lua (Config.Stations, Config.OffLabel).\n")
        f.write("# game name\tlabel\tgenre\ttile key\tgenre key\n")
        f.write("\n".join(rows) + "\n")
    for rel, name in (("html/sfx/radioopen.wav", "radioopen.wav"), ("html/sfx/radioclose.wav", "radioclose.wav"),
                      ("html/fonts/Anton.ttf", "Anton.ttf")):
        shutil.copyfile(os.path.join(res, *rel.split("/")), os.path.join(RADIO_OUT, name))
