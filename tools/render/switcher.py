"""The character switcher: GTA VI's row of character cards, drawn by ViExtras' Switcher for ViSwitch's wheel.

Michael, Franklin and Trevor, then Jason and Lucia side by side with a heart between them, as GTA VI shows the
pair; each card has its character's honor face on its bottom edge and the name under it, and the one picked has a
white border. Measured from the user's GTA VI crop (2026-10-22): square cards 106 px at 1080p, the pair 2 px apart,
the heart 31 px on their seam a little above the middle, the face 29 px centred on the card's bottom edge.
ViExtras draws them at CARD (Switcher.cs) and everything here scales with it.

  switch_card        256 x 256  a card's backdrop: a dark gradient, lighter at the top, as GTA VI's portraits sit on
  switch_heart        96 x 96   the pink heart between Jason and Lucia, outlined dark (our own drawing)
  switch_face_good    96 x 96   the honor faces under the cards: good (angel), bad (devil), terrible (purple devil)
  switch_face_bad                and broken (terrible, greyed, cracked) are vice_hud's own icons (html/icons/honor_*),
  switch_face_terrible           neutral is drawn here in their style (no halo or horns, a level mouth)
  switch_face_neutral
  switch_face_broken
  switch_name_*      the names under the cards, in GTA Art Deco, white
  switch_lucia       256 x 256  Lucia's portrait: FBICat's (scripts/Hud/Swap/lucia.png) on the card backdrop
  switch_jason       256 x 256  Jason's portrait on the card backdrop, from art/switch_jason.png beside this script
                                 (JASON_ART names another folder with a switch_jason.png)

Michael's, Franklin's and Trevor's portraits are the game's own, drawn from its texture dictionaries at run time
(Switcher.cs), not pictures of ours.

    python tools/render/switcher.py
"""
import os

import numpy as np
from PIL import Image, ImageDraw, ImageFont

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(os.path.dirname(HERE))
ART = os.path.join(ROOT, "scripts", "ViExtras", "Art")
GROUP = "switch"
ICONS = os.path.join(os.path.dirname(ROOT), "GTA-VI-Inspired-HUD-for-FiveM", "resources", "vice_hud", "html", "icons")
FONT = os.path.join(ROOT, "scripts", "ViExtras", "Fonts", "GTAArtDecoMedium.ttf")
CARD = 256
FACE = 96
OUTLINE = (8, 32, 40, 255)      # vice_hud's faces' outline
NAMES = ("Michael", "Franklin", "Trevor", "Jason", "Lucia")
JASON_ART = os.environ.get("JASON_ART", os.path.join(HERE, "art"))


def _card_backdrop(n=CARD):
    y = np.linspace(0, 1, n)[:, None]
    x = np.linspace(-1, 1, n)[None, :]
    v = 40 - 24 * y - 8 * np.abs(x)
    img = np.zeros((n, n, 4), np.uint8)
    img[..., 0] = np.clip(v, 0, 255)
    img[..., 1] = np.clip(v - 2, 0, 255)
    img[..., 2] = np.clip(v + 5, 0, 255)
    img[..., 3] = 255
    return Image.fromarray(img, "RGBA")


def _portrait(src):
    """A portrait on the card backdrop: scaled to fill the card's width, standing on its bottom edge."""
    card = _card_backdrop()
    im = Image.open(src).convert("RGBA")
    s = CARD / min(im.size)
    im = im.resize((round(im.width * s), round(im.height * s)), Image.LANCZOS)
    card.alpha_composite(im, ((CARD - im.width) // 2, CARD - im.height))
    return card


def _fit(im, n=FACE):
    """An icon fitted into an n x n square, centred, its aspect kept."""
    im = im.convert("RGBA")
    s = n / max(im.size)
    im = im.resize((max(1, round(im.width * s)), max(1, round(im.height * s))), Image.LANCZOS)
    out = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    out.alpha_composite(im, ((n - im.width) // 2, (n - im.height) // 2))
    return out


def _neutral(ss=4):
    """vice_hud's face style without halo or horns: a pale face, a thick dark outline, plain eyes, a level mouth."""
    n = 320 * ss
    im = Image.new("RGBA", (n, n), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    o = 26 * ss                                   # outline thickness, as the icons'
    box = (14 * ss, 30 * ss, n - 14 * ss, n - 6 * ss)
    d.ellipse(box, fill=OUTLINE)
    d.ellipse((box[0] + o, box[1] + o, box[2] - o, box[3] - o), fill=(246, 238, 206, 255))
    cx, cy = n / 2, (box[1] + box[3]) / 2
    for side in (-1, 1):                          # the eyes: upright ovals
        ex = cx + side * 52 * ss
        d.ellipse((ex - 15 * ss, cy - 46 * ss, ex + 15 * ss, cy + 2 * ss), fill=OUTLINE)
    d.rounded_rectangle((cx - 50 * ss, cy + 42 * ss, cx + 50 * ss, cy + 58 * ss), radius=8 * ss, fill=OUTLINE)
    return im.resize((320, 320), Image.LANCZOS)


def _broken(terrible):
    """The terrible face greyed with a crack across it, as the HUD's broken badge."""
    a = np.asarray(terrible.convert("RGBA")).astype(float)
    grey = a[..., :3] @ np.array([0.30, 0.59, 0.11])
    a[..., :3] = np.clip(grey * 0.85 + 18, 0, 255)[..., None]
    im = Image.fromarray(a.astype(np.uint8), "RGBA")
    w, h = im.size
    d = ImageDraw.Draw(im)
    pts = [(w * 0.58, h * 0.04), (w * 0.47, h * 0.30), (w * 0.60, h * 0.44), (w * 0.44, h * 0.66), (w * 0.55, h * 0.96)]
    d.line(pts, fill=OUTLINE, width=max(4, w // 28), joint="curve")
    return im


def _heart(n=96, ss=4):
    s = n * ss
    im = Image.new("RGBA", (s, s), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)

    def shape(col, grow):
        r = s * 0.235 + grow
        cy = s * 0.36
        d.ellipse((s * 0.5 - 2 * r + grow * 0.15, cy - r, s * 0.5 + grow * 0.15, cy + r), fill=col)
        d.ellipse((s * 0.5 - grow * 0.15, cy - r, s * 0.5 + 2 * r - grow * 0.15, cy + r), fill=col)
        d.polygon([(s * 0.5 - 2 * r * 0.93, cy + r * 0.38), (s * 0.5 + 2 * r * 0.93, cy + r * 0.38),
                   (s * 0.5, s * 0.88 + grow * 1.2)], fill=col)

    shape((24, 22, 26, 255), s * 0.045)
    shape((225, 92, 133, 255), 0)
    return im.resize((n, n), Image.LANCZOS)


def _name(text, h=40):
    font = ImageFont.truetype(FONT, int(h * 0.62))
    l, t, r, b = font.getbbox(text.upper())
    w = (r - l) + 8
    w += (-w) % 4
    im = Image.new("RGBA", (w, h), (255, 255, 255, 0))
    d = ImageDraw.Draw(im)
    d.text(((w - (r - l)) / 2 - l, (h - (b - t)) / 2 - t), text.upper(), font=font, fill=(255, 255, 255, 255))
    return im


def switch_art(out=ART):
    pics = {"switch_card": _card_backdrop(), "switch_heart": _heart()}
    icons = {k: Image.open(os.path.join(ICONS, "honor_%s.png" % k)) for k in ("good", "bad", "terrible")}
    for k, im in icons.items():
        pics["switch_face_" + k] = _fit(im)
    pics["switch_face_neutral"] = _fit(_neutral())
    pics["switch_face_broken"] = _fit(_broken(icons["terrible"]))
    for n in NAMES:
        pics["switch_name_" + n.lower()] = _name(n)
    pics["switch_lucia"] = _portrait(os.path.join(ROOT, "scripts", "Hud", "Swap", "lucia.png"))
    pics["switch_jason"] = _portrait(os.path.join(JASON_ART, "switch_jason.png"))
    entries = {}
    for name, img in pics.items():
        img.save(os.path.join(out, name + ".png"))
        w, h = img.size
        entries[name] = (w, h, 0, 0, w, h)
    return entries


if __name__ == "__main__":
    entries = switch_art()
    path = os.path.join(ART, "manifest_%s.txt" % GROUP)
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write("# name width height content-x content-y content-w content-h (tools/render/switcher.py)\n")
        for n in sorted(entries):
            f.write(n + " " + " ".join("%.2f" % v for v in entries[n]) + "\n")
    print("%s: %d pictures" % (GROUP, len(entries)))
