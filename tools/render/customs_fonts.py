"""Customs fonts: glyph atlases for Leonida Customs' two type faces, so the
single-player shop can set any string the way its FiveM page does.

The shop (resources/Lenonida_customs, beside vice_hud) prints names, prices
and counts that are only known in game, so its text cannot be pre-rendered as
words. Instead every character is rendered here, alone, in Edge with exactly
the shop's CSS (its own @font-face rules and font stacks, so a character a
face lacks comes from the same fallback the shop would use), cut out, and
packed into one atlas per face. src/ViExtras/Core/TextKit.cs lays strings out
from the advances and kerning measured here and draws each letter from the
atlas with DRAW_SPRITE_ARX_WITH_UV.

Faces (style.css):
  display   'CustomsDisplay' (Bahnschrift, src: local) at weight 700 and
            font-stretch 75%, uppercase: h1-h3, tabs, prices, stat labels,
            the pager. No lower case here: the shop uppercases this face and
            TextKit maps a-z onto A-Z.
  body      'GTAArtDeco' 400 (html/fonts/GTAArtDecoRegular.ttf), mixed case:
            list rows, names, hints.
  radio     vi_radio's 'Anton' 400 (vi_radio/html/fonts/Anton.ttf, beside
            vice_hud), upper case, no outline: the radio wheel's song and
            artist lines (and a station or genre it has no picture for), which
            are only known in game. Its capitals take the accented ones from
            Latin-1 too, for song names.
Letter-spacing is not baked in: TextKit adds it (the display rule's .01em is
the caller's to pass), as it adds kerning.

Characters: printable ASCII 32-126 plus NBSP and the punctuation the shop
prints (arrows, middle dot, ellipsis, curly quotes, dashes, times sign).

Base size: every glyph is rendered at a 64 px font size, device scale 1. The
largest text the shop sets is its 4.75vh dialog heading (51.3 px at 1080p),
so the atlas is always drawn at or below its own size there; build_txd.py
gives the atlases a full mip chain for the small sizes (1.6vh = 17 px).

Pictures (atlas pixels = the "CSS px" in manifest_customsfont.txt; box =
the whole picture; the radio face has no _outline atlas, "-" in the metrics):
  customsfont_display          the display glyphs, white
  customsfont_display_outline  the shop's --outline-mid text-shadow alone
                               (eight rgba(20,20,26,.9) copies round the
                               letter, the letter itself left out)
  customsfont_body             the body glyphs, white
  customsfont_body_outline     the same outline for the body face
Each face's two atlases share one layout, so a glyph's cell is the same in
both. The outline is its own layer because CSS paints a text-shadow under the
whole line: a letter-plus-outline picture per glyph would let each letter's
outline cover the edge of the letter before it. TextKit draws every outline
cell first, then every letter over them, as the browser does.

--outline-mid's offsets are in vh (.15vh diagonal, .19vh along the axes), so
their weight against the letter depends on the font size; they are baked in
here for a 2.7vh font (the prompts' 2.75vh, between the stat labels' 2.5vh and
the option name's 3.45vh): 0.056 em diagonal and 0.07 em along the axes.

Metrics go to Art/customs_glyphs.txt; its header says how to read it.
Kerning: every pair of the face's characters is measured as the width of the
pair less the widths of its two letters, as the browser shapes it (so a pair
the browser sets from two fonts, or as a ligature, is caught too); pairs that
move by KERN_MIN or more at 64 px are listed. Pairs with an a-z letter are
left out of the display face, which has no lower case.
"""
import os
import tempfile

GROUP = "customsfont"

BASE = 64          # font size every glyph is rendered at (px)
SLOT = 128         # each glyph is rendered alone in a SLOT x SLOT square
COLS = 16
PEN_X = 32         # where the pen starts inside its slot
LINE_TOP = 32      # top of the line-height: 1 box inside its slot
PAD = 2            # transparent margin kept round each glyph's ink
GAP = 4            # between cells in the atlas, so mip levels do not bleed

OUTLINE_DIAG = 0.056   # --outline-mid's .15vh and .19vh against a 2.7vh font
OUTLINE_AXIS = 0.07

EXTRA = "\u2191\u2193\u00b7\u2026\u2019\u201c\u201d\u2013\u2014\u00d7\u00a0"
ASCII = "".join(chr(c) for c in range(32, 127))

UPPER_LATIN1 = "".join(chr(c) for c in range(0xC0, 0xDF) if c != 0xD7) + "\u00a1\u00bf"

FACES = [
    # name, CSS class, characters (kerning is measured for every pair of them), outline layer
    ("display", "display", "".join(c for c in ASCII if not ("a" <= c <= "z")) + EXTRA, True),
    ("body", "body", ASCII + EXTRA, True),
    ("radio", "radio", "".join(c for c in ASCII if not ("a" <= c <= "z")) + EXTRA + UPPER_LATIN1, False),
]

# Each face's font stack, weight and whether it is set condensed (METRICS).
FAMILY = {
    "display": ("'CustomsDisplay', 'Bahnschrift Condensed', 'Arial Narrow', 'GTAArtDeco', sans-serif", 700, True),
    "body": ("'GTAArtDeco', 'Segoe UI', Arial, sans-serif", 400, False),
    "radio": ("'Anton', Impact, sans-serif", 400, False),
}
KERN_MIN = 0.05    # px at BASE; the page measures widths to 1/64 px

EDGE = "rgba(20, 20, 26, .9)"


def outline_shadow():
    d, a = OUTLINE_DIAG * BASE, OUTLINE_AXIS * BASE
    offs = [(-d, -d), (d, -d), (-d, d), (d, d), (0, -a), (0, a), (-a, 0), (a, 0)]
    return ", ".join(f"{x:.3f}px {y:.3f}px 0 {EDGE}" for x, y in offs)


def page_html(fonts, anton):
    """A blank page with the shop's font rules, vi_radio's Anton and the faces' CSS."""
    url = lambda f: "file:///" + os.path.join(fonts, f).replace("\\", "/")
    return f"""<!DOCTYPE html><html><head><meta charset="utf-8"><style>
@font-face {{ font-family: 'GTAArtDeco'; src: url('{url("GTAArtDecoRegular.ttf")}') format('truetype'); font-weight: 400; font-style: normal; font-display: block; }}
@font-face {{ font-family: 'GTAArtDeco'; src: url('{url("GTAArtDecoMedium.ttf")}') format('truetype'); font-weight: 500; font-style: normal; font-display: block; }}
@font-face {{ font-family: 'GTAArtDeco'; src: url('{url("GTAArtDecoBold.ttf")}') format('truetype'); font-weight: 700; font-style: normal; font-display: block; }}
@font-face {{ font-family: 'CustomsDisplay'; src: local('Bahnschrift'); font-weight: 300 700; font-stretch: 75% 100%; }}
@font-face {{ font-family: 'Anton'; src: url('{"file:///" + anton.replace(chr(92), "/")}') format('truetype'); font-weight: 400; font-style: normal; font-display: block; }}
html, body {{ margin: 0; padding: 0; background: transparent; }}
body {{ color: #fff; -webkit-font-smoothing: antialiased; }}
.display {{ font-family: 'CustomsDisplay', 'Bahnschrift Condensed', 'Arial Narrow', 'GTAArtDeco', sans-serif;
           font-weight: 700; font-stretch: 75%; text-transform: uppercase; }}
.body {{ font-family: 'GTAArtDeco', 'Segoe UI', Arial, sans-serif; font-weight: 400; }}
.radio {{ font-family: 'Anton', Impact, sans-serif; font-weight: 400; text-transform: uppercase; }}
.g {{ position: absolute; font-size: {BASE}px; line-height: 1; white-space: pre; letter-spacing: 0; }}
.mark {{ display: inline-block; width: 0; height: 0; vertical-align: baseline; }}
.outline .g {{ color: transparent; text-shadow: {outline_shadow()}; }}
</style></head><body><div id="slots"></div></body></html>"""


# Every glyph alone in its slot. Returns, per glyph, the pen x (the box's
# left), its advance (the shrink-to-fit box's width) and the baseline y (the
# bottom of an empty inline-block sitting on it).
SLOTS = """
async ({chars, cls, slot, cols, penX, top}) => {
    const host = document.getElementById('slots');
    host.className = cls;
    host.textContent = '';
    chars.forEach((c, i) => {
        const d = document.createElement('div');
        d.className = 'g';
        d.style.left = ((i % cols) * slot + penX) + 'px';
        d.style.top = (Math.floor(i / cols) * slot + top) + 'px';
        d.appendChild(document.createTextNode(c));
        const m = document.createElement('i');
        m.className = 'mark';
        d.appendChild(m);
        host.appendChild(d);
    });
    host.getBoundingClientRect();
    await document.fonts.ready;
    await new Promise(r => requestAnimationFrame(() => requestAnimationFrame(r)));
    return [...host.children].map(d => {
        const r = d.getBoundingClientRect(), m = d.querySelector('.mark').getBoundingClientRect();
        return [r.left, r.width, m.top];
    });
}
"""

# The face's vertical metrics as fractions of the font size, measured at
# 1000 px so the browser's whole-pixel rounding is negligible: ascent and
# descent (what CSS centres in a line box), the line gap line-height: normal
# adds, cap height and x-height.
METRICS = """
async ({cls, family, weight, condensed}) => {
    const S = 1000;
    const c = document.createElement('canvas').getContext('2d');
    c.font = weight + ' ' + S + 'px ' + family;
    if (condensed) c.fontStretch = 'condensed';
    const h = c.measureText('H'), x = c.measureText('x');
    const host = document.createElement('div');
    host.className = cls;
    document.body.appendChild(host);
    const d = document.createElement('div');
    d.className = 'g';
    d.style.cssText = 'left:0;top:0;font-size:' + S + 'px;line-height:normal';
    d.textContent = 'H';
    host.appendChild(d);
    await document.fonts.ready;
    const normal = d.getBoundingClientRect().height;
    host.remove();
    return {ascent: h.fontBoundingBoxAscent / S, descent: h.fontBoundingBoxDescent / S,
            gap: (normal - h.fontBoundingBoxAscent - h.fontBoundingBoxDescent) / S,
            cap: h.actualBoundingBoxAscent / S, xh: x.actualBoundingBoxAscent / S};
}
"""

# Kerning: each pair's width less its two letters' widths, as shaped by the page.
KERN = """
async ({cls, set, min}) => {
    const host = document.createElement('div');
    host.className = cls;
    document.body.appendChild(host);
    const mk = t => {
        const d = document.createElement('div');
        d.className = 'g';
        d.style.left = '0px';
        d.style.top = '0px';
        d.textContent = t;
        host.appendChild(d);
        return d;
    };
    const chars = [...set];
    const singles = chars.map(mk);
    const pairs = [];
    for (const a of chars) for (const b of chars) pairs.push([a, b, mk(a + b)]);
    host.getBoundingClientRect();
    await document.fonts.ready;
    const w = {};
    chars.forEach((c, i) => { w[c] = singles[i].getBoundingClientRect().width; });
    const out = [];
    for (const [a, b, d] of pairs) {
        const k = d.getBoundingClientRect().width - w[a] - w[b];
        if (Math.abs(k) >= min) out.push([a, b, k]);
    }
    host.remove();
    return out;
}
"""


def leonida_dir(ctx):
    """Lenonida_customs sits beside vice_hud in resources/ (--vice-hud's sibling)."""
    index = ctx.index.replace("file:///", "").replace("/", os.sep)
    res = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(index))), "Lenonida_customs")
    if not os.path.isfile(os.path.join(res, "html", "fonts", "GTAArtDecoRegular.ttf")):
        raise SystemExit(f"customsfont: no Lenonida_customs beside vice_hud ({res})")
    return res


def pack(sizes, width):
    """Shelf packing, tallest first. Returns each cell's corner and the height used."""
    order = sorted(range(len(sizes)), key=lambda i: (-sizes[i][1], -sizes[i][0], i))
    x = y = GAP
    shelf = 0
    pos = [(0, 0)] * len(sizes)
    for i in order:
        w, h = sizes[i]
        if w == 0 or h == 0:
            continue
        if x + w + GAP > width:
            x, y, shelf = GAP, y + shelf + GAP, 0
        pos[i] = (x, y)
        x += w + GAP
        shelf = max(shelf, h)
    return pos, y + shelf + GAP


def vi_radio_anton(ctx):
    """vi_radio sits beside vice_hud too; its page's Anton."""
    path = os.path.join(os.path.dirname(leonida_dir(ctx)), "vi_radio", "html", "fonts", "Anton.ttf")
    if not os.path.isfile(path):
        raise SystemExit(f"customsfont: no vi_radio Anton beside vice_hud ({path})")
    return path


def render_face(browser, ctx, html_path, name, cls, chars, outline, out_dir):
    from PIL import Image

    rows = (len(chars) + COLS - 1) // COLS
    page = browser.new_page(viewport={"width": COLS * SLOT, "height": rows * SLOT}, device_scale_factor=1)
    page.goto("file:///" + html_path.replace("\\", "/"))
    page.wait_for_load_state("load")
    family, weight, condensed = FAMILY[cls]
    page.evaluate("() => Promise.all([document.fonts.load(\"700 64px CustomsDisplay\"), document.fonts.load(\"400 64px GTAArtDeco\"), document.fonts.load(\"400 64px Anton\")])")
    placed = page.evaluate(SLOTS, {"chars": list(chars), "cls": cls, "slot": SLOT, "cols": COLS, "penX": PEN_X, "top": LINE_TOP})

    with tempfile.TemporaryDirectory() as tmp:
        shots = {}
        for layer, body_cls in (("plain", ""), ("outline", "outline")) if outline else (("plain", ""),):
            page.evaluate("c => { document.body.className = c; }", body_cls)
            page.wait_for_timeout(100)
            path = os.path.join(tmp, layer + ".png")
            page.screenshot(path=path, omit_background=True)
            shots[layer] = Image.open(path).convert("RGBA")
            shots[layer].load()
    page.evaluate("() => { document.body.className = ''; }")
    m = page.evaluate(METRICS, {"cls": cls, "family": family, "weight": weight, "condensed": condensed})
    kerns = page.evaluate(KERN, {"cls": cls, "set": chars, "min": KERN_MIN})
    page.close()

    # Each glyph's ink, letter and outline together, plus a margin.
    cells = []
    for i, c in enumerate(chars):
        sx, sy = (i % COLS) * SLOT, (i // COLS) * SLOT
        box = None
        for layer in shots.values():
            b = layer.crop((sx, sy, sx + SLOT, sy + SLOT)).getchannel("A").getbbox()
            if b:
                box = b if box is None else (min(box[0], b[0]), min(box[1], b[1]), max(box[2], b[2]), max(box[3], b[3]))
        if box is None:
            cells.append(None)
            continue
        if box[0] == 0 or box[1] == 0 or box[2] == SLOT or box[3] == SLOT:
            print(f"  {name}: U+{ord(c):04X} touches its slot's edge; make SLOT bigger")
        x0, y0 = max(0, box[0] - PAD), max(0, box[1] - PAD)
        x1, y1 = min(SLOT, box[2] + PAD), min(SLOT, box[3] + PAD)
        cells.append((sx + x0, sy + y0, x1 - x0, y1 - y0))

    # The narrowest power-of-two width that keeps the atlas no taller than wide;
    # the height is a multiple of 32 so the first mip levels halve exactly.
    sizes = [(c[2], c[3]) if c else (0, 0) for c in cells]
    for width in (256, 512, 1024, 2048):
        pos, used = pack(sizes, width)
        if used <= width:
            break
    height = (used + 31) & ~31

    atlases = {layer: Image.new("RGBA", (width, height), (0, 0, 0, 0)) for layer in shots}
    glyphs = []
    for i, c in enumerate(chars):
        pen_x, advance, base_y = placed[i]
        if cells[i] is None:
            glyphs.append((c, advance, 0, 0, 0, 0, 0.0, 0.0))
            continue
        sx, sy, w, h = cells[i]
        ax, ay = pos[i]
        for layer, img in shots.items():
            atlases[layer].paste(img.crop((sx, sy, sx + w, sy + h)), (ax, ay))
        glyphs.append((c, advance, ax, ay, w, h, sx - pen_x, base_y - sy))

    atlas = "customsfont_" + name
    names = {"plain": atlas, "outline": atlas + "_outline"}
    for layer, img in atlases.items():
        img.save(os.path.join(out_dir, names[layer] + ".png"))
        ctx.manifest[names[layer]] = (float(width), float(height), 0.0, 0.0, float(width), float(height))
    print(f"  {name}: {len(chars)} glyphs, atlas {width} x {height}, {len(kerns)} kerning pairs")
    return {"name": name, "atlas": atlas, "outline": atlas + "_outline" if outline else "-", "w": width, "h": height,
            "metrics": m, "glyphs": glyphs, "kerns": kerns}


HEADER = """# Leonida Customs glyph metrics, written by tools/render/customs_fonts.py with
# the atlases customsfont_<face>.png (letters) and customsfont_<face>_outline.png
# (the --outline-mid text-shadow alone). Read by src/ViExtras/Core/TextKit.cs.
#
# Lengths are pixels at the face's base font size (`size`); multiply by
# (font size / size) to draw at another size. Atlas cells are in atlas pixels.
# Characters are UTF-16 code units in hex.
#
# face <face> <size> <ascent> <descent> <lineGap> <capHeight> <xHeight> <atlas> <outlineAtlas> <atlasW> <atlasH>
#   ascent, descent  the font's ascent and descent (not rounded). The browser
#                    rounds each to whole pixels at the size it draws, then puts
#                    the baseline round(ascent) + floor((L - round(ascent) -
#                    round(descent)) / 2) below the top of a line box L tall
#                    (line-height: 1 means L = the font size).
#   lineGap          what line-height: normal adds: its box is round(ascent) +
#                    round(descent) + round(lineGap) tall.
#   capHeight, xHeight  ink height of H and x above the baseline.
# glyph <face> <code> <advance> <x> <y> <w> <h> <ox> <oy>
#   advance   pen advance, letter-spacing not included
#   x y w h   the glyph's cell in both of the face's atlases; w = h = 0 for a
#             glyph with no ink (space, no-break space)
#   ox        the cell's left edge less the pen's x (negative: left of the pen)
#   oy        how far the baseline is below the cell's top
# kern <face> <codeA> <codeB> <adjust>
#   added to the pen between A and B (the face's kerning as the browser shapes
#   it); pairs not listed are 0. The display and radio faces list upper case only;
#   the radio face has no outline atlas ("-").
"""


def render(browser, ctx):
    from render_fivem_art import OUT

    fonts = os.path.join(leonida_dir(ctx), "html", "fonts")
    faces = []
    with tempfile.TemporaryDirectory() as tmp:
        html_path = os.path.join(tmp, "customs_fonts.html")
        with open(html_path, "w", encoding="utf-8") as f:
            f.write(page_html(fonts, vi_radio_anton(ctx)))
        for name, cls, chars, outline in FACES:
            faces.append(render_face(browser, ctx, html_path, name, cls, chars, outline, OUT))

    with open(os.path.join(OUT, "customs_glyphs.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write(HEADER)
        for fd in faces:
            m = fd["metrics"]
            f.write("\nface %s %d %.3f %.3f %.3f %.3f %.3f %s %s %d %d\n" % (
                fd["name"], BASE, m["ascent"] * BASE, m["descent"] * BASE, m["gap"] * BASE,
                m["cap"] * BASE, m["xh"] * BASE, fd["atlas"], fd["outline"], fd["w"], fd["h"]))
            for c, adv, x, y, w, h, ox, oy in fd["glyphs"]:
                f.write("glyph %s %04X %.4f %d %d %d %d %.3f %.3f\n" % (fd["name"], ord(c), adv, x, y, w, h, ox, oy))
            for a, b, k in sorted(fd["kerns"], key=lambda p: (ord(p[0]), ord(p[1]))):
                f.write("kern %s %04X %04X %.4f\n" % (fd["name"], ord(a), ord(b), k))
