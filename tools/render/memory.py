"""Memory: vice_hud's world-action prompt (#world-actions), the walk-up prompt
um_memory puts on a person you look at (exports.vice_hud:ShowWorldActions from
um_memory/client/integrations.lua, without a target system).

What app.js onWorldActions builds, one row per option: the button (.wa-btn, a
.glyph drawn by setGlyph), then its word (.wa-label). style.css sizes all of
it by --wa-scale (0.7) against the corner prompts it borrows its look from:

    button       --g-h  3.05cqh x 0.7
    word         2.07cqh x 0.7, bold, font-stretch 86%, uppercase, outlined
    button->word gap 0.8cqh x 0.7
    row->row     gap 0.9cqh x 0.7
    compact      .wa-compact hides the words (player over 1.5 m from the point)

The words are rendered here (wa_label_<word>), and every button as it is drawn
inside the world prompt (wa_glyph_<device>_[style_]<token>): its drop shadow
is in cqh and is not scaled by --wa-scale, so the corner prompts' glyph art
scaled down would not be the same picture. The script lays the row out from
the boxes recorded in manifest_memory.txt.
"""
from render_fivem_art import KBM_TOKENS, PAD_TOKENS, token_file

GROUP = "memory"
# "Pet" is single player's own (HonorDogs.cs), drawn on the same prompt.
LABELS = ["Greet", "Defuse", "Antagonize", "Intimidate", "Pet"]

PROBE = """
() => {
    const box = document.getElementById('world-actions');
    const rows = box.querySelectorAll('.wa-row');
    const r = (el) => { const b = el.getBoundingClientRect(); return [b.x, b.y, b.width, b.height]; };
    return {
        box: r(box),
        rows: Array.from(rows).map((row) => ({
            row: r(row), btn: r(row.querySelector('.wa-btn')), label: r(row.querySelector('.wa-label')),
        })),
    };
}
"""


def show(ctx, page, options):
    ctx.post(page, {"action": "worldActions", "show": True, "options": options})
    page.wait_for_timeout(60)


def render(browser, ctx):
    page = ctx.page(browser)

    # The measures the script uses, printed for its comments.
    ctx.fresh(page)
    show(ctx, page, [{"label": "Greet", "glyph": "E", "device": "kbm"},
                     {"label": "Antagonize", "glyph": "SPACE", "device": "kbm"}])
    print("  layout:", page.evaluate(PROBE))

    # The words, with the button kept in the layout but not drawn.
    for label in LABELS:
        ctx.fresh(page)
        show(ctx, page, [{"label": label, "glyph": "E", "device": "kbm"}])
        page.evaluate("() => document.querySelector('#world-actions .wa-btn').style.visibility = 'hidden'")
        ctx.element(page, "#world-actions .wa-label", "wa_label_" + token_file(label.lower()), pad=8)

    # Every button, alone in the prompt.
    jobs = [("kbm", None, t) for t in KBM_TOKENS] + \
           [("pad", s, t) for s in ("xbox", "ps") for t in PAD_TOKENS]
    for device, style, token in jobs:
        ctx.fresh(page)
        if style:
            ctx.post(page, {"action": "inputDevice", "pad": style})
        show(ctx, page, [{"label": "", "glyph": token, "device": device}])
        name = f"wa_glyph_{device}_{style + '_' if style else ''}{token_file(token)}"
        ctx.element(page, "#world-actions .wa-btn", name, pad=6)
    page.close()
