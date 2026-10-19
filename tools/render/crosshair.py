"""Crosshair: vice_hud's #crosshair (index.html), the aim reticle and the kill
mark, as style.css draws them under "Aim crosshair".

Every picture is the same square of the page, XHAIR_R CSS px each way from
the screen centre, so the script draws each one centred on the screen centre
and moves it from there, exactly as the CSS translates it:

  xhair_vehicle        #cross-circle: .cc-ring + .cc-dot (static, no spread)
  xhair_tick_left      .ct-left   at --spread 0
  xhair_tick_right     .ct-right  at --spread 0
  xhair_tick_bottom    .ct-bottom at --spread 0
  xhair_kill_red       #cross-kill.kill-red.show    (opacity 1, scale 1)
  xhair_kill_yellow    #cross-kill.kill-yellow.show
  xhair_kill_white     #cross-kill.kill-white.show

The motion is not baked: --spread (0 -> 0.35 --w, transform 160ms ease-out)
and the kill mark's opacity 0 -> 1 / scale 0.6 -> 1 (320ms) are played by
the script. The kill mark's drop-shadow is part of the element's own
rendering, which its transform then scales, so scaling the baked picture
reproduces it.

vice_hud's #third-eye is not rendered: it belongs to the targeting system
(ox_target drives it), which single player does not have.
"""

GROUP = "crosshair"
XHAIR_R = 36  # CSS px; the widest piece is the kill mark, 2.2 --w + its shadow

# Leave only the crosshair on the page, with every transition off so each
# picture is the settled state.
ISOLATE = """
() => {
    const stage = document.getElementById('stage');
    const box = document.getElementById('crosshair');
    for (const el of stage.children) if (el !== box) el.style.visibility = 'hidden';
    for (const el of box.querySelectorAll('*')) el.style.transition = 'none';
    const r = box.getBoundingClientRect();
    return { x: r.left, y: r.top };
}
"""

# Show one part of the crosshair and hide the rest. `keep` is a selector.
ONLY = """
(keep) => {
    const box = document.getElementById('crosshair');
    const parts = box.querySelectorAll('.cc-ring, .cc-dot, .ct, #cross-kill');
    const kept = new Set(box.querySelectorAll(keep));
    for (const el of parts) el.style.visibility = kept.has(el) ? 'visible' : 'hidden';
}
"""

# The kill mark as #cross-kill.show leaves it once its transition has run.
KILL_SETTLED = """
() => {
    const x = document.getElementById('cross-kill');
    x.style.opacity = '1';
    x.style.transform = 'translate(-50%, -50%) scale(1)';
}
"""


def _shot(ctx, page, centre, name):
    ctx.box(page, {"x": centre["x"] - XHAIR_R, "y": centre["y"] - XHAIR_R,
                   "width": XHAIR_R * 2, "height": XHAIR_R * 2}, name, pad=0)


def render(browser, ctx):
    page = ctx.page(browser, 1920, 1080)

    # Vehicle: the ring and its dot, as one static picture.
    ctx.fresh(page)
    ctx.post(page, {"action": "crosshair", "active": True, "mode": "vehicle"})
    page.wait_for_timeout(100)
    centre = page.evaluate(ISOLATE)
    page.evaluate(ONLY, ".cc-ring, .cc-dot")
    page.wait_for_timeout(50)
    _shot(ctx, page, centre, "xhair_vehicle")

    # Foot: each tick on its own, at rest (--spread 0).
    ctx.fresh(page)
    ctx.post(page, {"action": "crosshair", "active": True, "mode": "foot"})
    page.wait_for_timeout(100)
    centre = page.evaluate(ISOLATE)
    for side in ("left", "right", "bottom"):
        page.evaluate(ONLY, ".ct-" + side)
        page.wait_for_timeout(50)
        _shot(ctx, page, centre, "xhair_tick_" + side)

    # The kill mark in each colour, alone, fully shown.
    for quality, colour in (("headshot", "red"), ("sloppy", "yellow"), ("clean", "white")):
        ctx.fresh(page)
        ctx.post(page, {"action": "crosshair", "active": True, "mode": "foot"})
        page.wait_for_timeout(100)
        ctx.post(page, {"action": "crossKill", "quality": quality})
        page.wait_for_timeout(50)
        centre = page.evaluate(ISOLATE)
        page.evaluate(KILL_SETTLED)
        page.evaluate(ONLY, "#cross-kill")
        page.wait_for_timeout(300)  # the background picture loads
        _shot(ctx, page, centre, "xhair_kill_" + colour)

    page.close()
