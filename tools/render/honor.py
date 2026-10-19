"""Honor: vice_hud's honor change indicator (#honor-pop) and honor standing
panel (#honor), drawn by onHonorPop() / onHonor() in html/app.js from the
same messages client.lua's ShowHonorDeed / ShowHonorToast send.

Neither has an animation: both are shown and hidden with the .hidden class
(display: none), the pop for 2200 ms, the panel for Config.Honor.holdMs.

honor_pop_*      #honor-pop, one picture per state onHonorPop() can draw:
                 the sign (+ / - / none when broken) and the face.
honor_panel      #honor with Config.Honor.showValue = false (the .bare
                 panel), WITHOUT the mugshot and WITH A HOLE over the mug's
                 padding box. The script draws the mug's gradient and the
                 ped headshot natively underneath: a headshot is a game
                 texture, and every ScriptHookV sprite draws above anything
                 native. The mug's 1px border (over its gradient, as the
                 browser paints it) stays in the picture, so the ring is exact.
honor_badge_*    #honor-badge-wrap alone: the tier face (angel / devil), and
                 the broken one (terrible face, greyed, crack on top).

Every picture is cut on whole CSS pixels (so whole device pixels at scale 2):
a fractional clip is rounded by the screenshot, and the picture would no
longer be the size the manifest says.
"""

import io
import math
import os

from PIL import Image

from render_fivem_art import OUT

GROUP = "honor"

GOOD = "icons/honor_good.png"
BAD = "icons/honor_bad.png"
TERRIBLE = "icons/honor_terrible.png"
FACES = {"angelEmoji": GOOD, "devilEmoji": BAD, "terribleEmoji": TERRIBLE}

# Only the element being drawn shows; everything else on the stage is hidden.
ISOLATE = """
(id) => {
    const target = document.getElementById(id);
    for (const el of document.getElementById('stage').children)
        el.style.visibility = el.contains(target) ? '' : 'hidden';
}
"""

IMAGES_LOADED = """
() => [...document.images].filter(i => i.offsetParent && i.getAttribute('src')).every(i => i.complete && i.naturalWidth > 0)
"""

# The panel's own surface off, leaving only what sits on the mug.
BARE_BADGE = """
() => {
    const h = document.getElementById('honor');
    h.style.background = 'transparent';
    h.style.boxShadow = 'none';
    const m = document.getElementById('honor-mug');
    m.style.background = 'none';
    m.style.borderColor = 'transparent';
}
"""

# Coverage of the mug's padding box (its rounded inner edge), as alpha.
PADDING_MASK = """
() => {
    const h = document.getElementById('honor');
    h.style.background = 'transparent';
    h.style.boxShadow = 'none';
    const m = document.getElementById('honor-mug');
    m.style.borderColor = 'transparent';
    m.style.background = '#fff';
    m.style.backgroundClip = 'padding-box';
}
"""

BOX = """
(sel) => { const b = document.querySelector(sel).getBoundingClientRect(); return [b.left, b.top, b.width, b.height]; }
"""


def _ready(page):
    page.wait_for_timeout(80)
    page.wait_for_function(IMAGES_LOADED, timeout=5000)


def _clip(page, selector, pad):
    """The element's box, and a whole-pixel clip `pad` px round it."""
    x, y, w, h = page.evaluate(BOX, selector)
    x0, y0 = math.floor(x - pad), math.floor(y - pad)
    x1, y1 = math.ceil(x + w + pad), math.ceil(y + h + pad)
    return (x, y, w, h), {"x": x0, "y": y0, "width": x1 - x0, "height": y1 - y0}


def _grab(page, clip):
    return Image.open(io.BytesIO(page.screenshot(clip=clip, omit_background=True))).convert("RGBA")


def _save(ctx, name, img, box, clip):
    x, y, w, h = box
    img.save(os.path.join(OUT, name + ".png"))
    ctx.manifest[name] = (clip["width"], clip["height"], x - clip["x"], y - clip["y"], w, h)


def _element(page, ctx, selector, name, pad):
    box, clip = _clip(page, selector, pad)
    _save(ctx, name, _grab(page, clip), box, clip)


def _pops(page, ctx):
    states = [
        ("honor_pop_up_good", {"delta": 5, "severity": "good"}),
        ("honor_pop_down_bad", {"delta": -5, "severity": "bad"}),
        ("honor_pop_up_terrible", {"delta": 5, "severity": "terrible"}),
        ("honor_pop_down_terrible", {"delta": -8, "severity": "terrible"}),
        ("honor_pop_broken", {"delta": -8, "severity": "terrible", "broken": True}),
    ]
    for name, extra in states:
        ctx.fresh(page)
        msg = {"action": "honor", "popOnly": True}
        msg.update(FACES)
        msg.update(extra)
        ctx.post(page, msg)
        page.evaluate(ISOLATE, "honor-pop")
        _ready(page)
        # text-shadow: 0 0.25cqh 0.5cqh -> reaches ~8 px past the box
        _element(page, ctx, "#honor-pop", name, pad=16)


def _show_panel(page, ctx, extra):
    ctx.fresh(page)
    msg = {"action": "honor", "showValue": False, "holdMs": 0, "honor": 50, "emoji": GOOD}
    msg.update(FACES)
    msg.update(extra)
    ctx.post(page, msg)
    page.evaluate(ISOLATE, "honor")
    # Moved in off the corner, keeping its sub-pixel phase: a screenshot is cut
    # at the viewport, which would crop the shadow margin on the right.
    page.evaluate("() => { const h = document.getElementById('honor');"
                  " h.style.right = 'calc(0.25cqw + 400px)'; h.style.bottom = 'calc(5.24cqh + 300px)'; }")
    # The pop fires with the panel whenever there is a delta; none is sent here,
    # and it is hidden by ISOLATE anyway.
    _ready(page)


def _badges(page, ctx):
    states = [
        ("honor_badge_angel", {"honor": 50, "emoji": GOOD}),
        ("honor_badge_devil", {"honor": -50, "emoji": BAD}),
        ("honor_badge_broken", {"honor": -100, "emoji": BAD, "broken": True}),
    ]
    for name, extra in states:
        _show_panel(page, ctx, extra)
        page.evaluate(BARE_BADGE)
        page.wait_for_timeout(60)
        # filter: drop-shadow(0 1px 2px) -> a few px past the box
        _element(page, ctx, "#honor-badge-wrap", name, pad=6)


def _panel(page, ctx):
    _show_panel(page, ctx, {})
    page.evaluate("() => { document.getElementById('honor-badge-wrap').style.visibility = 'hidden'; }")
    page.wait_for_timeout(60)
    hx, hy, hw, hh = page.evaluate(BOX, "#honor")
    mx, my, mw, mh = page.evaluate(BOX, "#honor-mug")
    bx, by, bw, bh = page.evaluate(BOX, "#honor-badge-wrap")
    radius = page.evaluate("() => getComputedStyle(document.getElementById('honor-mug')).borderTopLeftRadius")
    # These offsets are the constants Honor/HonorHud.cs places the native mug
    # and the badge with; print them so a change to the CSS is noticed.
    print("  #honor %.3f x %.3f | mug border box +%.3f,+%.3f %.3f x %.3f r %s | badge wrap +%.3f,+%.3f %.3f x %.3f"
          % (hw, hh, mx - hx, my - hy, mw, mh, radius, bx - hx, by - hy, bw, bh))

    # box-shadow: 0 0.25cqh 0.70cqh -> reaches ~10 px past the box
    box, clip = _clip(page, "#honor", 16)
    panel = _grab(page, clip)
    page.evaluate(PADDING_MASK)
    page.wait_for_timeout(60)
    mask = _grab(page, clip).getchannel("A")

    # destination-out: wherever the mug's padding box covers, the picture goes
    # clear, so the natively drawn gradient and headshot show through it.
    r, g, b, a = panel.split()
    alpha = bytes(int(av * (255 - mv) / 255 + 0.5) for av, mv in zip(a.tobytes(), mask.tobytes()))
    _save(ctx, "honor_panel", Image.merge("RGBA", (r, g, b, Image.frombytes("L", panel.size, alpha))), box, clip)


def render(browser, ctx):
    page = ctx.page(browser)
    _pops(page, ctx)
    _badges(page, ctx)
    _panel(page, ctx)
    page.close()
