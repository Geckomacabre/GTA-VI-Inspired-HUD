"""Leonida Customs: the fixed pieces of the mod shop menu, from its own page
(Lenonida_customs/html, next to vice_hud in the FiveM resources).

The menu's text is drawn from the glyph atlases (tools/render/customs_fonts.py,
src/ViExtras/Core/TextKit.cs) and its flat panels, bars and gradients with
DRAW_RECT; what is left is cut out of the page here, each piece styled by the
page's own style.css (the elements are created with its classes):

  customs_tri          a white right triangle, filled under the diagonal from
                       bottom left to top right: the slanted ends of the tabs,
                       the stat value and rating pills (flipped for the others)
  customs_disc         a white disc (2x), tinted for .dot and the card's dots
  customs_ring         .dots i: the card page dot, empty
  customs_have         .have: the check in a white disc with its dark ring
  customs_have_sm      .swatch .have (3vh)
  customs_tag          svg.tag, white (tinted pink / deep pink)
  customs_rate_dirt    svg.rate: the off-road rating icon, white on transparent
  customs_rate_road    svg.rate: the road rating icon (its cut-outs stay dark)
  customs_key_<id>     .key caps: every button the prompts show, keyboard and
                       both pads; <id> is the key's name, _bump / _mini / _hold
                       variants as style.css sizes them
  customs_sw_sheen     the metallic swatch's 135deg light and shade, as an
                       overlay on the flat colour (shade(+-) in colors.js)
  customs_sw_chrome    the chrome swatch
  customs_sw_metal     the brushed metal swatch's stripes, overlay
  customs_sw_none      .swatch .none: the struck-through "no colour" swatch
  customs_sw_mark      .swatch .mark: the "not owned" square, a "%" in it as app.js has
  customs_cham_<index> each chameleon paint's gradient swatch (colors.js
                       chameleonGradient of its name)
  label_upgrade_menu   vice_hud's prompt word for the garage's "Upgrade Menu [E]"
"""
import os
import re

GROUP = "customs"


def leonida_dir(ctx):
    """Lenonida_customs, beside vice_hud in the FiveM resources."""
    root = ctx.index.replace("file:///", "")
    res = os.path.dirname(os.path.dirname(os.path.dirname(root)))
    return os.path.join(res, "Lenonida_customs")


def leonida_index(ctx):
    return "file:///" + os.path.join(leonida_dir(ctx), "html", "index.html").replace("\\", "/")


def chameleon_paints(ctx):
    """config.lua Config.Chameleon: name -> paint index."""
    lua = open(os.path.join(leonida_dir(ctx), "config.lua"), encoding="utf-8").read()
    block = lua[lua.index("Config.Chameleon = {"):]
    block = block[:block.index("\n}")]
    return {m.group(1).strip(): int(m.group(2)) for m in re.finditer(r"\['([^']*)'\]\s*=\s*(\d+)", block)}


# A clean stage on the shop page, with the menu's own stylesheet, and a
# holder for one piece at a time.
STAGE = """
() => {
    document.getElementById('app').hidden = true;
    let s = document.getElementById('shot-stage');
    if (!s) {
        s = document.createElement('div');
        s.id = 'shot-stage';
        s.style.cssText = 'position:fixed;left:0;top:0;width:100vw;height:100vh;';
        document.body.appendChild(s);
    }
    s.innerHTML = '';
}
"""

PUT = """
([html, css]) => {
    const s = document.getElementById('shot-stage');
    s.innerHTML = '';
    const w = document.createElement('div');
    w.id = 'shot';
    w.style.cssText = 'position:absolute;left:200px;top:200px;' + (css || '');
    w.innerHTML = html;
    s.appendChild(w);
    const el = w.firstElementChild || w;
    const r = el.getBoundingClientRect();
    return { x: r.left, y: r.top, width: r.width, height: r.height };
}
"""

CHECK = '<svg viewBox="0 0 24 24"><path d="M5 12.5l4.5 4.5L19 7.5" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/></svg>'
TAG = '<svg class="tag" viewBox="0 0 24 24" style="margin:0;color:#fff"><path d="M3 12.6V4.5A1.5 1.5 0 0 1 4.5 3h8.1a1.5 1.5 0 0 1 1.06.44l7.4 7.4a1.5 1.5 0 0 1 0 2.12l-8.1 8.1a1.5 1.5 0 0 1-2.12 0l-7.4-7.4A1.5 1.5 0 0 1 3 12.6Z" fill="none" stroke="currentColor" stroke-width="2.2"/><circle cx="8.2" cy="8.2" r="1.9" fill="currentColor"/></svg>'
DIRT = '<svg class="rate" viewBox="0 0 24 24" style="color:#fff;margin:0"><rect x="1.5" y="1.5" width="21" height="21" fill="none" stroke="currentColor" stroke-width="2"/><path d="M3 18l5-9 3.5 5 3-7 6.5 11z" fill="currentColor"/></svg>'
ROAD = '<svg class="rate" viewBox="0 0 24 24" style="color:#fff;margin:0"><rect x="1" y="1" width="22" height="22" fill="currentColor"/><path d="M8.5 3L5 21h4.6l.6-5h3.6l.6 5H19L15.5 3z" fill="#22222b"/><path d="M11.4 5.5h1.2v3h-1.2zM11.2 10.5h1.6v3.5h-1.6z" fill="currentColor"/></svg>'

# Every key cap the prompts can show (app.js glyph(): the keyboard's, and the
# Xbox and PlayStation names), by a file-safe id.
KEYS = {
    "enter": "&#8629;", "esc": "ESC", "q": "Q", "e": "E", "r": "R", "tab": "TAB",
    "x_a": "A", "x_b": "B", "x_x": "X", "x_y": "Y", "x_lb": "LB", "x_rb": "RB",
    "ps_cross": "&#10005;", "ps_circle": "&#9675;", "ps_square": "&#9633;", "ps_triangle": "&#9651;", "ps_l1": "L1", "ps_r1": "R1",
}


def key_html(text, cls):
    wide = len(text) > 1 and not text.startswith("&")
    return '<i class="key%s%s">%s</i>' % (" wide" if wide else "", cls, text)


def shot(ctx, page, html, name, css="", pad=2):
    box = page.evaluate(PUT, [html, css])
    page.wait_for_timeout(30)
    return ctx.box(page, box, name, pad=pad)


def render(browser, ctx):
    page = browser.new_page(viewport={"width": 1920, "height": 1080}, device_scale_factor=2)
    page.goto(leonida_index(ctx))
    page.wait_for_load_state("load")
    page.wait_for_timeout(300)  # fonts
    page.evaluate(STAGE)

    # Shapes, tinted in game.
    shot(ctx, page, '<div style="width:64px;height:64px;background:#fff;clip-path:polygon(0 100%, 100% 0, 100% 100%)"></div>', "customs_tri", pad=0)
    shot(ctx, page, '<div style="width:64px;height:64px;border-radius:50%;background:#fff"></div>', "customs_disc", pad=1)
    shot(ctx, page, '<div class="dots" style="position:static"><i></i></div>', "customs_ring", pad=2)

    # .have, in the list / status (2.7vh) and on a swatch (3vh).
    shot(ctx, page, '<i class="have">%s</i>' % CHECK, "customs_have", pad=4)
    shot(ctx, page, '<div class="swatch" style="position:relative;background:none;border:0"><i class="have">%s</i></div>' % CHECK,
         "customs_have_sm_holder", pad=0)
    ctx.manifest.pop("customs_have_sm_holder", None)
    os.remove(os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), "scripts", "ViExtras", "Art", "customs_have_sm_holder.png"))
    box = page.evaluate("() => { const r = document.querySelector('#shot .have').getBoundingClientRect(); return {x:r.left,y:r.top,width:r.width,height:r.height}; }")
    ctx.box(page, box, "customs_have_sm", pad=4)

    shot(ctx, page, TAG, "customs_tag", pad=1)
    shot(ctx, page, DIRT, "customs_rate_dirt", pad=1)
    shot(ctx, page, ROAD, "customs_rate_road", pad=1)

    # Key caps: normal (prompts), bump (tab strip ends), mini (the card's page key), hold (Exit).
    for kid, text in KEYS.items():
        shot(ctx, page, key_html(text, ""), "customs_key_" + kid, pad=1)
        shot(ctx, page, '<nav class="tabs"><span>%s</span></nav>' % key_html(text, " bump"), "customs_keyholder", pad=0)
        box = page.evaluate("() => { const r = document.querySelector('#shot .key').getBoundingClientRect(); return {x:r.left,y:r.top,width:r.width,height:r.height}; }")
        ctx.box(page, box, "customs_key_%s_bump" % kid, pad=1)
        shot(ctx, page, key_html(text, " mini"), "customs_key_%s_mini" % kid, pad=1)
        shot(ctx, page, key_html(text, " hold"), "customs_key_%s_hold" % kid, pad=8)
    ctx.manifest.pop("customs_keyholder", None)

    # Swatches (10.6vh square), at 64 CSS px: they are smooth gradients.
    sw = 'width:64px;height:64px;'
    shot(ctx, page, '<div style="%sbackground:linear-gradient(135deg, rgba(255,255,255,.22) 0%%, rgba(255,255,255,0) 45%%, rgba(0,0,0,0) 45%%, rgba(0,0,0,.18) 100%%)"></div>' % sw,
         "customs_sw_sheen", pad=0)
    shot(ctx, page, '<div style="%sbackground:linear-gradient(160deg, #f5f7fa, #9aa3ad 40%%, #e9edf1 55%%, #6d7680)"></div>' % sw, "customs_sw_chrome", pad=0)
    shot(ctx, page, '<div style="%sbackground:repeating-linear-gradient(90deg, rgba(255,255,255,.09) 0 1px, rgba(0,0,0,0) 1px 3px)"></div>' % sw,
         "customs_sw_metal", pad=0)
    shot(ctx, page, '<div class="swatch" style="%sbackground:none;border:0"><span class="none"></span></div>' % sw, "customs_sw_none", pad=0)
    # app.js swatchHtml: a "%" in the light square (style.css still styles a tag there, unused).
    shot(ctx, page, '<div class="swatch" style="background:none;border:0;position:relative"><i class="mark">%</i></div>',
         "customs_markholder", pad=0)
    ctx.manifest.pop("customs_markholder", None)
    box = page.evaluate("() => { const r = document.querySelector('#shot .mark').getBoundingClientRect(); return {x:r.left,y:r.top,width:r.width,height:r.height}; }")
    ctx.box(page, box, "customs_sw_mark", pad=0)

    # The chameleon paints' gradient swatches, by paint index.
    for label, index in sorted(chameleon_paints(ctx).items(), key=lambda kv: kv[1]):
        bg = page.evaluate("l => chameleonGradient(l)", label)
        shot(ctx, page, '<div style="%sbackground:%s"></div>' % (sw, bg), "customs_cham_%d" % index, pad=0)

    page.close()
    # pieces that were only holders for a measurement
    art = os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), "scripts", "ViExtras", "Art")
    for junk in ("customs_keyholder.png", "customs_markholder.png"):
        p = os.path.join(art, junk)
        if os.path.isfile(p):
            os.remove(p)

    # The garage's prompt, a vice_hud prompt word like every other in single player.
    from render_fivem_art import render_labels
    render_labels(browser, ctx, ["Upgrade Menu"])
