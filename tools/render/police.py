"""Police lights: the lamp of vice_hud's directional police lights (#police).

vice_hud paints each lamp as a CSS radial gradient on the screen edge facing a
siren: `radial-gradient(ellipse A B at <edge point>, <colour> 0%, transparent
<stop>%)`, red and blue at 68 %, the white takedown at 58 %. Between the centre
and the stop only the alpha changes, in a straight line, so every lamp is the
same picture: this gradient on a circle, white, its alpha 1 at the centre and
0 at the rim. src/ViExtras/PoliceLights/PoliceLights.cs stretches it to each
lamp's ellipse (2 x stop x radius across), tints it, and flashes it the way
style.css's polRed / polBlue / polWhite keyframes do.

  police_lamp   the gradient, 128 x 128, rim to rim
"""

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


def render(browser, ctx):
    page = ctx.page(browser, 1920, 1080, scale=1)
    page.evaluate(LAMP)
    page.wait_for_timeout(100)
    ctx.element(page, "#lamp-shot", "police_lamp", pad=0)
    page.close()
