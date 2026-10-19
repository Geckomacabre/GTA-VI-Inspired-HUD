"""Nitrous: the vehicle panel's nitrous pip and the "Hold [key] to activate
nitrous" tip, from vice_hud's own page (index.html #pip-nitrous and
#nitro-hint, fed the `nitrousHint` message client.lua sends).

In single player the pip sits in GTA6Hud's own gauge row, next to its engine
and fuel gauges, so it is drawn the way that row draws a gauge: GTA6Hud's disc
(vipause hud_gauge_bg) and its fill ring frames, with vice_hud's nitrous mark
in the middle. The mark is vice_hud's; the ring frames are FBICat's
(scripts/Hud/Gauge), copied into this group's dictionary because Script Hook V
pictures cannot be made on GTA V Enhanced.

The tip's key cap is whatever the player has bound, so the tip is cut in three
round it and src/ViExtras/Nitrous/Nitrous.cs puts any glyph between them:

Pictures (CSS px at 1080p, layout boxes in manifest_nitrous.txt):
  nitrous_mark          #pip-nitrous's two chevrons alone, white; its box is the 32 px pip
  nitrous_hint_head     #nitro-hint from its left end to the key cap: the plate's end and "Hold"
  nitrous_hint_mid      two px of the plate where the key cap sits, stretched under the cap
  nitrous_hint_tail     from the key cap to the plate's right end: "to activate nitrous" and its icon
  nitrous_fill_<01-48>  GTA6Hud's gauge fill ring, 1/48 to 48/48, white, drawn 28 px across
"""
import os

GROUP = "nitrous"

# vice_hud's pip on its own: its SVG, the ring left out, the mark white, on
# a 32 px box (.pip is 1.667 --w across) away from everything else.
PIP = """
() => {
    const src = document.querySelector('#pip-nitrous svg');
    for (const el of document.body.children) el.style.visibility = 'hidden';
    const box = document.createElement('div');
    box.id = 'nitro-shot';
    box.style.cssText = 'position:fixed;left:200px;top:200px;width:32px;height:32px;visibility:visible;';
    const svg = src.cloneNode(true);
    svg.style.cssText = 'width:100%;height:100%;display:block;overflow:visible;';
    svg.querySelector('.pip-ring').style.display = 'none';
    for (const p of svg.querySelectorAll('path')) p.style.fill = '#ffffff';
    box.appendChild(svg);
    document.body.appendChild(box);
}
"""

# Only the tip, everything else hidden; its key cap shows `glyph` for now.
HINT_ONLY = """
() => {
    for (const el of document.body.children) el.style.visibility = 'hidden';
    const hint = document.getElementById('nitro-hint');
    hint.style.visibility = 'visible';
    for (const el of hint.querySelectorAll('*')) el.style.transition = 'none';
    const r = hint.getBoundingClientRect();
    const k = document.getElementById('nitro-hint-key').getBoundingClientRect();
    return { x: r.left, y: r.top, w: r.width, h: r.height, kx: k.left, kw: k.width };
}
"""


def render(browser, ctx):
    page = ctx.page(browser, 1920, 1080)

    # ---- the pip's mark ----------------------------------------------------------
    page.evaluate(PIP)
    page.wait_for_timeout(100)
    ctx.element(page, "#nitro-shot", "nitrous_mark", pad=2)

    # ---- the tip -----------------------------------------------------------------
    ctx.fresh(page)
    ctx.post(page, {"action": "nitrousHint", "show": True, "holdMs": 0, "glyph": "ALT", "device": "kbm"})
    page.wait_for_timeout(400)
    r = page.evaluate(HINT_ONLY)
    page.wait_for_timeout(100)
    ctx.box(page, {"x": r["x"], "y": r["y"], "width": r["kx"] - r["x"], "height": r["h"]}, "nitrous_hint_head", pad=0)
    right = r["kx"] + r["kw"]
    ctx.box(page, {"x": right, "y": r["y"], "width": r["x"] + r["w"] - right, "height": r["h"]}, "nitrous_hint_tail", pad=0)
    page.evaluate("() => { document.getElementById('nitro-hint-key').style.visibility = 'hidden'; }")
    page.wait_for_timeout(50)
    mid = round(r["kx"] + r["kw"] * 0.5)
    ctx.box(page, {"x": mid - 1, "y": r["y"], "width": 2, "height": r["h"]}, "nitrous_hint_mid", pad=0)
    page.close()

    # ---- GTA6Hud's gauge fill frames ---------------------------------------------
    from PIL import Image
    from render_fivem_art import OUT, ROOT
    src = os.path.join(ROOT, "scripts", "Hud", "Gauge")
    for step in range(1, 49):
        img = Image.open(os.path.join(src, "veh_gauge_%02d.png" % step)).convert("RGBA")
        name = "nitrous_fill_%02d" % step
        img.resize((64, 64), Image.LANCZOS).save(os.path.join(OUT, name + ".png"))
        ctx.manifest[name] = (28.0, 28.0, 0.0, 0.0, 28.0, 28.0)
