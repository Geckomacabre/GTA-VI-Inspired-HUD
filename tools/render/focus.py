"""Focus: vice_hud's #focus-fx, the purple vignette and inner glow shown while
Focus is active (style.css #focus-fx and @keyframes focusFxPulse).

The pulse swings filter: brightness(1 -> 1.45) saturate(1 -> 1.35) and
transform: scale(1 -> 1.05) over 1100 ms ease-in-out. Scale is applied at draw
time; the filter cannot be, so it is rendered here at evenly spaced points of
its swing (focus_fx_0 .. focus_fx_8) and the script picks the nearest. The
picture is smooth, so it is rendered at half size and drawn full screen.
"""

GROUP = "focus"
FRAMES = 9

FREEZE = """
([b, s]) => {
    const fx = document.getElementById('focus-fx');
    for (const el of document.getElementById('stage').children)
        if (el !== fx) el.style.visibility = 'hidden';
    fx.style.transition = 'none';
    fx.style.animation = 'none';
    fx.style.opacity = '1';
    fx.style.transform = 'none';
    fx.style.filter = `brightness(${b}) saturate(${s})`;
}
"""


def render(browser, ctx):
    page = ctx.page(browser, 1920, 1080, scale=0.5)
    ctx.post(page, {"action": "status", "health": 100, "stamina": 100, "focus": 60, "focusActive": True})
    page.wait_for_timeout(200)
    for i in range(FRAMES):
        t = i / (FRAMES - 1)
        page.evaluate(FREEZE, [1 + 0.45 * t, 1 + 0.35 * t])
        page.wait_for_timeout(50)
        ctx.full(page, f"focus_fx_{i}", 1920, 1080)
    page.close()
