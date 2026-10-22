"""Wanted: the GTA VI wanted notification, "The cops are searching for you / and they have your
description", with its red star and, under it, the tells (what the police have on you).

Not vice_hud's #wanted: that one had a line for each of four star states, and GTA VI has only this
one. Built from a GTA VI frame instead, measured at 1080p:
  box    430 x 110, corners 3; a dark tint fading left to right (charcoal at 0.88 to violet-grey at
         0.45: GTA VI blurs what is behind it, which a script can't, so the fade stands in for that),
         and a faint light line along the top only
  text   Segoe UI Variable Display 380, capitals 16 px, letter-spacing 0.065 em, baselines 31 apart,
         rgb(215,205,210); from 16.5 px in, capitals 17.5 px below the top
  star   28 px, dark red rgb(110,36,33) with a thin near-black outline, 377 px in and 12 down
  tells  25 px discs on rgb(104,36,37), 34 px apart, 20 px in and 74 down; the icons are vice_hud's
         (html/icons/tells), cut from GTA VI's HUD definitions sheet

  wanted_box          the box and its text, no star and no tells
  wanted_star         the star alone: a picture of its own so it packs uncompressed and stays sharp
                      (the box is too big for that: DXT5's 4 x 4 blocks broke a star up)
  wanted_tell_<name>  one tell: camera, weapon, person, people, vehicle, hanger

Where the star and the first tell sit in the box, and the step between tells, are printed at the end:
src/ViExtras/Wanted/WantedBox.cs has them as constants.
"""
import base64
import math
import os

GROUP = "wanted"

TELLS = ["camera", "weapon", "person", "people", "vehicle", "hanger"]
W, H = 430, 110
TINT0, TINT1 = (22, 20, 24, 0.88), (40, 34, 44, 0.45)
FONT, WEIGHT, CAP, SPACING, LINE = "Segoe UI Variable Display", 380, 16.0, 0.065, 31.0
TEXT = (215, 205, 210)
TEXT_X, CAP_TOP = 16.5, 17.5
STAR_X, STAR_Y, STAR_W = 377, 12, 28
STAR_FILL, STAR_LINE, STAR_STROKE = (110, 36, 33), (8, 2, 4), 1.6
DISC, PITCH, PLATE, TELLS_X, TELLS_Y = 25, 34, (104, 36, 37), 20, 74


def star_svg():
    # a regular five-point star as wide as the measured one (outline included), point up
    w = STAR_W - 2
    r = w / (2 * math.cos(math.radians(18)))
    h = r * (1 + math.cos(math.radians(36)))
    pts = []
    for i in range(10):
        rad = r if i % 2 == 0 else r * 0.382
        ang = math.radians(-90 + i * 36)
        pts.append("%.2f,%.2f" % (w / 2 + rad * math.cos(ang), r + rad * math.sin(ang)))
    sw, sh = STAR_W, int(round(h)) + 2
    return ("<svg id='star' width='%d' height='%d' viewBox='-1 -1 %d %d'><polygon points='%s' fill='rgb%s' "
            "stroke='rgb%s' stroke-width='%.2f' stroke-linejoin='round'/></svg>") % (sw, sh, sw, sh, " ".join(pts), STAR_FILL, STAR_LINE, STAR_STROKE)


def icon(icons, name):
    with open(os.path.join(icons, "tell_%s.png" % name), "rb") as f:
        return "data:image/png;base64," + base64.b64encode(f.read()).decode("ascii")


def page_html(icons, tells):
    discs = "".join("<div class='tell' id='tell_%s' style='left:%dpx'><img src='%s'></div>" % (n, TELLS_X + i * PITCH, icon(icons, n))
                    for i, n in enumerate(tells))
    return """<html><head><style>
body{margin:0;background:transparent}
#box{position:absolute;left:40px;top:40px;width:%dpx;height:%dpx;border-radius:3px;
 background:linear-gradient(90deg,rgba%s,rgba%s);box-shadow:inset 0 1px 0 rgba(255,255,255,0.10)}
#copy{position:absolute;left:%.2fpx;top:0;font-family:'%s';font-weight:%d;font-size:%.3fpx;letter-spacing:%.3fem;
 line-height:%.1fpx;color:rgb%s;white-space:nowrap}
#star{position:absolute;left:%dpx;top:%dpx}
.tell{position:absolute;top:%dpx;width:%dpx;height:%dpx;border-radius:50%%;background:rgb%s;display:flex;align-items:center;justify-content:center}
.tell img{width:74%%;height:74%%;object-fit:contain}
</style></head><body><div id='box'>
<div id='copy'>The cops are searching for you<br>and they have your description</div>%s%s</div></body></html>""" % (
        W, H, TINT0, TINT1, TEXT_X, FONT, WEIGHT, CAP / 0.70, SPACING, LINE, TEXT, STAR_X, STAR_Y, TELLS_Y, DISC, DISC, PLATE, star_svg(), discs)


# the first line's capitals CAP_TOP below the box top, from the font's own metrics
PLACE_TEXT = """
([capTop, line]) => {
    const copy = document.getElementById('copy');
    const cs = getComputedStyle(copy);
    const c = document.createElement('canvas').getContext('2d');
    c.font = cs.fontWeight + ' ' + cs.fontSize + ' ' + cs.fontFamily;
    const t = c.measureText('T');
    const fa = t.fontBoundingBoxAscent, fd = t.fontBoundingBoxDescent, cap = t.actualBoundingBoxAscent;
    const baseline = (line - (fa + fd)) / 2 + fa;
    copy.style.top = (capTop - (baseline - cap)) + 'px';
    return { cap, fa, fd, top: copy.style.top };
}
"""


def render(browser, ctx):
    from render_fivem_art import SCALE
    vice_html = os.path.dirname(ctx.index.replace("file:///", ""))
    icons = os.path.join(vice_html, "icons", "tells")

    def fresh(tells):
        page = browser.new_page(viewport={"width": 600, "height": 240}, device_scale_factor=SCALE)
        page.set_content(page_html(icons, tells))
        page.wait_for_timeout(300)
        page.evaluate(PLACE_TEXT, [CAP_TOP, LINE])
        page.wait_for_timeout(80)
        return page

    # the box and its text: star and tells hidden
    page = fresh([])
    page.evaluate("() => { document.getElementById('star').style.visibility = 'hidden'; }")
    ctx.element(page, "#box", "wanted_box", pad=2)
    # the star alone, the box behind it transparent
    page.evaluate("""() => { const b = document.getElementById('box'); b.style.background = 'transparent'; b.style.boxShadow = 'none';
        document.getElementById('copy').style.visibility = 'hidden'; document.getElementById('star').style.visibility = 'visible'; }""")
    ctx.element(page, "#star", "wanted_star", pad=2)
    page.close()

    # each tell alone, as it sits in the row
    page = fresh(TELLS)
    page.evaluate("""() => { const b = document.getElementById('box'); b.style.background = 'transparent'; b.style.boxShadow = 'none';
        document.getElementById('copy').style.visibility = 'hidden'; document.getElementById('star').style.visibility = 'hidden';
        for (const t of document.querySelectorAll('.tell')) t.style.visibility = 'hidden'; }""")
    for tell in TELLS:
        page.evaluate("n => { for (const t of document.querySelectorAll('.tell')) t.style.visibility = t.id === 'tell_' + n ? 'visible' : 'hidden'; }", tell)
        page.wait_for_timeout(30)
        ctx.element(page, "#tell_" + tell, "wanted_tell_" + tell, pad=2)

    at = page.evaluate("""() => {
        const box = document.getElementById('box').getBoundingClientRect();
        const s = document.getElementById('star').getBoundingClientRect();
        const t = document.querySelectorAll('.tell');
        const a = t[0].getBoundingClientRect(), b = t[1].getBoundingClientRect();
        return { sx: s.left - box.left, sy: s.top - box.top, tx: a.left - box.left, ty: a.top - box.top, step: b.left - a.left };
    }""")
    page.close()
    print("wanted: star at x %.2f y %.2f (WantedBox.cs StarX / StarY); tells row at x %.2f y %.2f, step %.2f (TellX / TellY / TellStep)"
          % (at["sx"], at["sy"], at["tx"], at["ty"], at["step"]))
