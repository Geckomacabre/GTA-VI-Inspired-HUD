"""Render the 2.5 overlay art from the FiveM version's own HUD page.

    python tools/render_fivem_art.py --vice-hud <path to resources/vice_hud> [--only GROUP ...]

The single-player scripts draw what vice_hud draws in FiveM. Rather than
redrawing that by hand, this opens vice_hud's html/index.html in Edge (through
Playwright), sends it the same NUI messages the FiveM Lua sends, and saves the
result as transparent PNGs in scripts/ViExtras/Art. Each group writes its own
Art/manifest_<group>.txt giving every picture's size and where its layout box
sits inside it, in CSS pixels at 1080p. Run it again whenever vice_hud's look
changes. Needs: pip install playwright, and Microsoft Edge.

The groups are the modules in tools/render/ (each has a GROUP name and a
render(browser, ctx) function), plus the shared ones defined here: overlays
(full-screen art), glyphs (every button) and labels (prompt words).
"""
import argparse
import importlib.util
import math
import os
import re
import sys

from playwright.sync_api import sync_playwright

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "scripts", "ViExtras", "Art")
RENDER_DIR = os.path.join(ROOT, "tools", "render")

# Loaded before the page's own scripts: a resource name so app.js does not fall
# into its browser demo mode, and a fetch that swallows the page's posts.
INIT = """
window.GetParentResourceName = function () { return 'vice_hud'; };
const realFetch = window.fetch;
window.fetch = function (url, opts) {
    if (String(url).startsWith('https://vice_hud/')) return Promise.resolve(new Response('{}'));
    return realFetch(url, opts);
};
"""

SCALE = 2     # device pixels per CSS px for small art (buttons, labels, panels)
ASPECTS = {"16x9": (1920, 1080), "16x10": (1728, 1080), "21x9": (2520, 1080), "32x9": (3840, 1080)}


class Ctx:
    """What a render module gets: the page address and a manifest to fill."""

    def __init__(self, index, group):
        self.index = index
        self.group = group
        self.manifest = {}

    def page(self, browser, width=1920, height=1080, scale=SCALE):
        p = browser.new_page(viewport={"width": width, "height": height}, device_scale_factor=scale)
        p.add_init_script(INIT)
        p.goto(self.index)
        p.wait_for_load_state("load")
        return p

    @staticmethod
    def fresh(page):
        """A clean page: everything the last shot showed is gone."""
        page.reload()
        page.wait_for_load_state("load")

    @staticmethod
    def post(page, msg):
        page.evaluate("m => window.postMessage(m, '*')", msg)

    def full(self, page, name, w, h):
        """The whole page, at its own size (scale 1 pages only)."""
        page.screenshot(path=os.path.join(OUT, name + ".png"), omit_background=True)
        self.manifest[name] = (w, h, 0, 0, w, h)

    def element(self, page, selector, name, pad=8):
        """One element, `pad` CSS px of room round it for shadows and outlines.
        Records the element's own box inside the picture."""
        el = page.query_selector(selector)
        if not el:
            print("  missing", selector, "for", name)
            return None
        box = el.bounding_box()
        if not box or box["width"] <= 0:
            print("  empty", selector, "for", name)
            return None
        return self.box(page, box, name, pad)

    def box(self, page, box, name, pad=8):
        # Whole CSS pixels: a fractional clip is rounded by the screenshot, which
        # would leave the picture a little smaller than the size recorded here
        # and draw it stretched. The box's true offset inside it is recorded.
        x0, y0 = math.floor(box["x"] - pad), math.floor(box["y"] - pad)
        x1 = math.ceil(box["x"] + box["width"] + pad)
        y1 = math.ceil(box["y"] + box["height"] + pad)
        vw, vh = page.viewport_size["width"], page.viewport_size["height"]
        x0, y0, x1, y1 = max(0, x0), max(0, y0), min(vw, x1), min(vh, y1)
        clip = {"x": x0, "y": y0, "width": x1 - x0, "height": y1 - y0}
        page.screenshot(path=os.path.join(OUT, name + ".png"), omit_background=True, clip=clip)
        self.manifest[name] = (clip["width"], clip["height"], box["x"] - x0, box["y"] - y0, box["width"], box["height"])
        return self.manifest[name]

    def pad_to_4(self):
        """Script Hook V on GTA V Enhanced (DirectX 12) cannot make a texture
        whose sides are not multiples of 4, and stops the game when asked to.
        Every picture is padded on the right and bottom with transparency;
        its box keeps its place, so only the recorded picture size changes."""
        from PIL import Image
        padded = 0
        for name, (w, h, bx, by, bw, bh) in list(self.manifest.items()):
            path = os.path.join(OUT, name + ".png")
            if not os.path.isfile(path):
                continue
            img = Image.open(path)
            pw, ph = img.size
            w4, h4 = (pw + 3) & ~3, (ph + 3) & ~3
            if (w4, h4) == (pw, ph):
                continue
            out = Image.new("RGBA", (w4, h4), (0, 0, 0, 0))
            out.paste(img.convert("RGBA"), (0, 0))
            out.save(path)
            self.manifest[name] = (w * w4 / pw, h * h4 / ph, bx, by, bw, bh)
            padded += 1
        if padded:
            print(f"{self.group}: padded {padded} pictures to multiples of 4")

    def write(self):
        self.pad_to_4()
        path = os.path.join(OUT, f"manifest_{self.group}.txt")
        with open(path, "w", encoding="utf-8", newline="\n") as f:
            f.write("# name  width height  boxX boxY  boxWidth boxHeight   (CSS px at 1080p)\n")
            for name in sorted(self.manifest):
                f.write(name + " " + " ".join("%.2f" % v for v in self.manifest[name]) + "\n")
        print(f"{self.group}: {len(self.manifest)} pictures")


def token_file(token):
    """A button token as it appears in a file name ("NUM 1" -> NUM_1, "." -> x2E)."""
    return re.sub(r"[^A-Za-z0-9_]", lambda m: "x%02X" % ord(m.group(0)), token.replace(" ", "_"))


def render_labels(browser, ctx, labels):
    """Prompt words, drawn as vice_hud draws a .prompt-label. Saved as label_<word>."""
    page = ctx.page(browser)
    for label in labels:
        ctx.fresh(page)
        ctx.post(page, {"action": "prompt", "id": "l", "label": label})
        page.wait_for_timeout(60)
        ctx.element(page, "#prompts .prompt-label", "label_" + token_file(label.lower()), pad=8)
    page.close()


# ---- the shared groups ----------------------------------------------------------

def overlays(browser, ctx):
    # The crash cam's overlay (SetCinematicFx) without its bars: the scripts
    # draw the bars themselves so they can breathe with the vignette.
    page = ctx.page(browser, scale=1)
    ctx.post(page, {"action": "hudVisible", "show": False})
    ctx.post(page, {"action": "cineFx", "show": True, "strength": 1, "bars": 0, "pulse": False})
    page.wait_for_timeout(300)
    ctx.full(page, "cine_vignette", 1920, 1080)
    page.close()
    # The binocular lens: its inset is in cqw and its radius in cqh, so it is
    # drawn for each common shape of screen and the closest one is used.
    for aspect, (w, h) in ASPECTS.items():
        page = ctx.page(browser, w, h, scale=1)
        ctx.post(page, {"action": "binoculars", "show": True})
        page.wait_for_timeout(300)
        ctx.full(page, f"bino_lens_{aspect}", w, h)
        page.close()


# Every button a prompt can show. Pad tokens are Xbox names; the page draws
# them as PlayStation buttons when told the pad is one.
KBM_TOKENS = (
    [chr(c) for c in range(ord("A"), ord("Z") + 1)] + [str(d) for d in range(10)]
    + list("-=[];',./\\`")
    + ["UP", "DOWN", "LEFT", "RIGHT", "LMB", "RMB", "MMB", "MOUSE4", "MOUSE5", "MOUSE6",
       "MOUSE7", "MOUSE8", "WHEEL_UP", "WHEEL_DOWN", "NUM -", "NUM +"]
    + [f"NUM {d}" for d in range(10)] + [f"F{n}" for n in range(1, 13)]
    + ["DEL", "ESC", "INS", "END", "SHIFT", "TAB", "ENTER", "BKSP", "HOME", "PGUP", "PGDN",
       "CAPS", "CTRL", "ALT", "SPACE"]
)
PAD_TOKENS = (
    ["A", "B", "X", "Y", "LB", "RB", "LT", "RT", "START", "BACK"]
    + [f"{s}{d}" for s in ("LS", "RS", "DPAD") for d in ("", "_UP", "_DOWN", "_LEFT", "_RIGHT", "_X", "_Y")]
)


def glyphs(browser, ctx):
    page = ctx.page(browser)
    jobs = [("kbm", None, t) for t in KBM_TOKENS] + \
           [("pad", s, t) for s in ("xbox", "ps") for t in PAD_TOKENS]
    for device, style, token in jobs:
        ctx.fresh(page)
        if style:
            ctx.post(page, {"action": "inputDevice", "pad": style})
        ctx.post(page, {"action": "prompt", "id": "g", "label": "", "glyph": token, "device": device})
        page.wait_for_timeout(60)
        name = f"glyph_{device}_{style + '_' if style else ''}{token_file(token)}"
        ctx.element(page, "#prompts .prompt-glyph-wrap", name, pad=4)
    page.close()


def labels(browser, ctx):
    render_labels(browser, ctx, ["Zoom", "Exit", "Vision"])


SHARED = {"overlays": overlays, "glyphs": glyphs, "labels": labels}


def load_modules():
    mods = {}
    if os.path.isdir(RENDER_DIR):
        for name in sorted(os.listdir(RENDER_DIR)):
            if not name.endswith(".py") or name.startswith("_"):
                continue
            spec = importlib.util.spec_from_file_location("render_" + name[:-3], os.path.join(RENDER_DIR, name))
            mod = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(mod)
            # switcher.py is a script of its own (no browser): it has no render().
            if hasattr(mod, "render"):
                mods[mod.GROUP] = mod.render
    return mods


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--vice-hud", required=True, help="path to the FiveM resources/vice_hud folder")
    ap.add_argument("--only", nargs="*", help="groups to render (default: all)")
    args = ap.parse_args()
    index = os.path.join(os.path.abspath(args.vice_hud), "html", "index.html")
    if not os.path.isfile(index):
        sys.exit(f"no {index}")
    index = "file:///" + index.replace("\\", "/")
    os.makedirs(OUT, exist_ok=True)
    groups = dict(SHARED)
    groups.update(load_modules())
    with sync_playwright() as p:
        browser = p.chromium.launch(channel="msedge")
        for group, fn in groups.items():
            if args.only and group not in args.only:
                continue
            ctx = Ctx(index, group)
            fn(browser, ctx)
            ctx.write()
        browser.close()


if __name__ == "__main__":
    sys.modules.setdefault("render_fivem_art", sys.modules[__name__])
    main()
