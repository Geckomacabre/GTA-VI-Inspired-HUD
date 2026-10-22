"""Police lights: the lamp of vice_hud's directional police lights (#police), and the pictures of the Round lights'
glow and direction arrow.

vice_hud paints each lamp as a CSS radial gradient on the screen edge facing a
siren: `radial-gradient(ellipse A B at <edge point>, <colour> 0%, transparent
<stop>%)`, red and blue at 68 %, the white takedown at 58 %. Between the centre
and the stop only the alpha changes, in a straight line, so every lamp is the
same picture: this gradient on a circle, white, its alpha 1 at the centre and
0 at the rim. src/ViExtras/PoliceLights/PoliceLights.cs stretches it to each
lamp's ellipse (2 x stop x radius across), tints it, and flashes it the way
style.css's polRed / polBlue / polWhite keyframes do.

  police_lamp   the gradient, 128 x 128, rim to rim

The Round lights are the GTA VI trailer's police chase (its first 3.6 s, a police car right behind), frame by frame,
drawn as light: each lamp a soft Gaussian of its colour standing on the bottom edge, with a tighter, whiter core. The
lamps' light adds up and goes through a film tone curve (ACES), which turns a bright blue cyan-white and a bright red
pink-white, and blue and red together violet to white, the way the trailer's do. Where each lamp is, how strong and
how wide, every frame, and the colours, were fitted to the trailer's frames: police_trailer.json beside this script.
These pictures are drawn from those numbers, here with numpy, not from the page (python tools/render/police.py writes
just them, and src/ViExtras/PoliceLights/PoliceTrailer.g.cs, which frame shows which picture):

  police_row_b_000..   the whole row of lights in one frame, on the bottom edge, at a quarter of the trailer's size
                       (1080p); the top edge's is the same picture half turned. Its middle is the row's (the trailer's
                       camera drift taken out). Light over black: the colour is the light's own, the alpha its
                       strongest channel, so drawn over a dark road it is the trailer's light exactly. The picture
                       goes on past the edge (BELOW, drawn off screen) and its last rows are empty: the game's sprites
                       wrap round, so a picture whose bright foot was its last row showed that row again as a thin
                       line along its far side.
  police_row_l_000..   the same on the left edge, shining right (the right edge's is it half turned)
  police_dot             8 x 8    a soft disc: ViExtras' own direction arrow's curve is drawn as a row of these
  police_tip_0..63      24 x 24   that arrow's point, 15 px wide and 11 px tall at 1080p, its base's middle at the
                                  picture's centre, pointing 360/64 degrees further clockwise each (0: up)
  police_arrow         128 x 32   the clip's direction arrow (driveUPSguy 0:44-0:51) whole, white, at 1:1 for 1080p,
                                  for GTA6Hud's police arrows: a sliver 104 px long and 3.2 px thick, curved round a
                                  point 190 px below it, its ends tapered over the last eighth, and a triangle 15 px
                                  wide and 11 px tall on top, pointing out at the police. The sliver's middle line is
                                  2 px above the centre; it runs 64 +- 51.5 px across, the triangle 64 +- 7.5.

The game's DRAW_SPRITE turns a picture in its 0..1 screen units before stretching them to the screen's width and
height, so any turn but a half turn shears it. ViExtras turns none of these but by a half turn: the bottom and left
edges and each of the point's directions have their own pictures. GTA6Hud turns police_arrow on its own scaleform
layer, which turns it in pixels.
"""
import json
import os

import numpy as np
from PIL import Image

GROUP = "police"

LAMP = """
() => {
    for (const el of document.body.children) el.style.visibility = 'hidden';
    const d = document.createElement('div');
    d.id = 'lamp-shot';
    d.style.cssText = 'position:fixed;left:100px;top:100px;width:128px;height:128px;visibility:visible;'
        + 'background:radial-gradient(circle 64px at 50% 50%, rgba(255,255,255,1) 0%, transparent 100%);';
    document.body.appendChild(d);
}
"""

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
ART = os.path.join(ROOT, "scripts", "ViExtras", "Art")
TABLE = os.path.join(ROOT, "src", "ViExtras", "PoliceLights", "PoliceTrailer.g.cs")

TIPS = 64
ROW_SCALE = 4          # the row pictures are a quarter of the trailer's 1080p size (soft light: nothing is lost)
BELOW = 32             # how far a row picture goes on past the edge, off screen (1080p px), its last 2 rows empty


def _white(alpha):
    img = np.zeros(alpha.shape + (4,), np.uint8)
    img[..., :3] = 255
    img[..., 3] = (np.clip(alpha, 0, 1) * 255).round().astype(np.uint8)
    return Image.fromarray(img, "RGBA")


def _aces(x):
    """The film tone curve (Narkowicz's fit of ACES)."""
    x = np.maximum(x, 0)
    return np.clip((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0, 1)


def _row_light(look, lamps, xs, ys):
    """The lamps' light before the tone curve, at x along the edge from the row's middle and y in from the edge
    (1080p px)."""
    x, y = np.meshgrid(xs, ys)
    blue = np.array([look["b_r"], look["b_g"], 1.0]), np.array([look["bc_r"], look["bc_g"], 1.0])
    red = np.array([1.0, look["r_g"], look["r_b"]]), np.array([1.0, look["rc_g"], look["rc_b"]])
    light = np.zeros(x.shape + (3,))
    for c, lx, amp, s, k in lamps:
        sy = s * k * (look["aspect_b"] if c == "B" else look["aspect_r"])
        q = ((x - lx) / s) ** 2 + ((y + look["dy"]) / sy) ** 2
        col, core = blue if c == "B" else red
        light += amp * (np.exp(-0.5 * q)[..., None] * col
                        + look["core_k"] * np.exp(-0.5 * q / look["core_s"] ** 2)[..., None] * core)
    return light


def _row(look, lamps, along, inward):
    """One frame's row of lights on the bottom edge, as an RGBA picture (see police_row_b above): inward above the
    edge, BELOW under it."""
    w, h = along // ROW_SCALE, (inward + BELOW) // ROW_SCALE
    xs = ((np.arange(w) + 0.5) * ROW_SCALE) - along / 2
    ys = inward - (np.arange(h) + 0.5) * ROW_SCALE
    rgb = _aces(_row_light(look, lamps, xs, ys))
    a = rgb.max(axis=2)
    # an empty border all round, two rows deep under the edge, for the sprite's wrap round to land on
    a[0], a[-2:], a[:, 0], a[:, -1] = 0, 0, 0, 0
    img = np.zeros((h, w, 4))
    img[..., :3] = rgb / np.maximum(rgb.max(axis=2), 1e-6)[..., None]
    img[..., 3] = a
    return Image.fromarray((np.clip(img, 0, 1) * 255).round().astype(np.uint8), "RGBA")


def trailer_rows(out=ART):
    """Writes the Round lights' row pictures and PoliceTrailer.g.cs; returns the pictures' manifest entries."""
    data = json.load(open(os.path.join(HERE, "police_trailer.json"), encoding="utf-8"))
    look, along, inward = data["look"], data["along"], data["in"]
    pictures, picture = {}, []
    for f in data["frames"]:
        lamps = tuple((l["c"], l["x"], l["amp"], l["s"], l["k"]) for l in f["lamps"])
        if not lamps:
            picture.append(-1)
            continue
        if lamps not in pictures:
            pictures[lamps] = len(pictures)
        picture.append(pictures[lamps])
    entries = {}
    for lamps, k in pictures.items():
        img = _row(look, lamps, along, inward)
        for name, pic in (("police_row_b_%03d" % k, img), ("police_row_l_%03d" % k, img.transpose(Image.ROTATE_270))):
            pic.save(os.path.join(out, name + ".png"))
            entries[name] = pic.size
    rows = ",\n".join("            " + ", ".join("%d" % v for v in picture[i:i + 24]) for i in range(0, len(picture), 24))
    with open(TABLE, "w", encoding="utf-8", newline="\n") as f:
        f.write("// <auto-generated>\n"
                "// The round police lights, frame by frame: which row picture (police_row_b_NNN / police_row_l_NNN) each\n"
                "// frame of the GTA VI trailer's police chase shows. Written by tools/render/police.py from\n"
                "// tools/render/police_trailer.json; regenerate rather than editing by hand.\n"
                "// </auto-generated>\n"
                "namespace ViExtras\n{\n    internal static class PoliceTrailer\n    {\n"
                "        public const int Fps = %d;\n" % data["fps"] +
                "        // the pictures' size at the trailer's own (NearSize 19), 1080p px: along the edge, in from it, and on\n"
                "        // past it (off screen)\n"
                "        public const float Along = %df, In = %df, Below = %df;\n" % (along, inward, BELOW) +
                "        public const int Pictures = %d;\n\n" % len(pictures) +
                "        // each frame's picture, -1 for none (the lights between flashes)\n"
                "        public static readonly short[] Picture =\n        {\n" + rows + "\n        };\n    }\n}\n")
    return {n: (w, h, 0, 0, w, h) for n, (w, h) in entries.items()}


def _dot(n=8):
    c = (np.arange(n) + 0.5) - n / 2
    x, y = np.meshgrid(c, c)
    return _white(np.clip(n / 2 - 0.3 - np.sqrt(x ** 2 + y ** 2), 0, 1))


def _tip(k, n=24, ss=8):
    """The arrow's point turned k/TIPS of a turn clockwise from up, its base's middle at the centre."""
    a = 2 * np.pi * k / TIPS
    ux, uy = np.sin(a), -np.cos(a)            # where it points, on screen (y down)
    px, py = -uy, ux                          # along its base
    s = (np.arange(n * ss) + 0.5) / ss - n / 2
    x, y = np.meshgrid(s, s)
    up = x * ux + y * uy                      # out from the base
    across = x * px + y * py
    tri = (up >= -0.6) & (up <= 11) & (np.abs(across) <= 7.5 * (1 - np.clip(up, 0, 11) / 11))
    alpha = tri.reshape(n, ss, n, ss).mean(axis=(1, 3))
    return _white(alpha)


def _arrow(w=128, h=32, ss=4):
    """GTA6Hud's police arrow, white: the sliver and its triangle."""
    bend, half, thick = 190.0, 52.0 / 190.0, 3.2
    cx, top = w / 2, 14.0                    # the sliver's middle line at its middle
    s_ = (np.arange(w * ss) + 0.5) / ss
    x, y = np.meshgrid(s_, (np.arange(h * ss) + 0.5) / ss)
    dx, dy = x - cx, (top + bend) - y        # from the sliver's centre of curve, up
    rho = np.sqrt(dx ** 2 + dy ** 2)
    phi = np.arctan2(dx, dy)
    s = (phi / half + 1) / 2                 # 0..1 along it
    th = thick * np.clip((1 - np.abs(2 * s - 1)) / 0.25, 0, 1) ** 0.7
    arc = (np.abs(phi) <= half) & (np.abs(rho - bend) <= th / 2)
    outer = top - thick / 2
    # the triangle: its base on the sliver's outer edge, 15 px wide, its point 11 px above it
    up = outer + 0.6 - y
    tri = (up >= 0) & (up <= 11.6) & (np.abs(x - cx) <= 7.5 * (1 - np.clip(up, 0, 11.6) / 11.6))
    alpha = (arc | tri).reshape(h, ss, w, ss).mean(axis=(1, 3))
    return _white(alpha)


def glow_art(out=ART):
    """Writes the Round lights' and the arrow's pictures to `out`; returns their manifest entries."""
    entries = {}
    pics = [("police_dot", _dot()), ("police_arrow", _arrow())]
    pics += [("police_tip_%d" % k, _tip(k)) for k in range(TIPS)]
    for name, img in pics:
        img.save(os.path.join(out, name + ".png"))
        entries[name] = img.size
    found = {n: (w, h, 0, 0, w, h) for n, (w, h) in entries.items()}
    found.update(trailer_rows(out))
    return found


def render(browser, ctx):
    page = ctx.page(browser, 1920, 1080, scale=1)
    page.evaluate(LAMP)
    page.wait_for_timeout(100)
    ctx.element(page, "#lamp-shot", "police_lamp", pad=0)
    page.close()
    ctx.manifest.update(glow_art())


if __name__ == "__main__":
    # just the numpy pictures, added to manifest_police.txt beside the lamp
    entries = glow_art()
    path = os.path.join(ART, "manifest_%s.txt" % GROUP)
    lines = [l for l in open(path, encoding="utf-8").read().splitlines() if l.strip()]
    # the lamp stays (it comes from the page); every other police picture is this script's, made afresh
    keep = [l for l in lines if l.startswith("#") or l.split()[0] == "police_lamp"]
    for f in os.listdir(ART):
        if f.startswith("police_") and f.endswith(".png") and f[:-4] not in entries and f != "police_lamp.png":
            os.remove(os.path.join(ART, f))
    keep += [n + " " + " ".join("%.2f" % v for v in entries[n]) for n in sorted(entries)]
    head = [l for l in keep if l.startswith("#")]
    body = sorted(l for l in keep if not l.startswith("#"))
    open(path, "w", encoding="utf-8", newline="\n").write("\n".join(head + body) + "\n")
    print("%s: %d pictures" % (GROUP, len(body)))
