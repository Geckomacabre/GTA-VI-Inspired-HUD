"""Skills: the /skills panel and the gym prompts.

Rendered from vice_hud's own page (html/index.html #skills, fed the `skills`
message client_skills.lua sends). FiveM's level-up card (#skillup) is left
out: it is not part of GTA VI. The panel carries live numbers, so it is cut
into its fixed parts and its glyphs here, and src/ViExtras/Skills/SkillsUi.cs lays them out again the way
the page's own CSS does (the measurements it uses are noted there).

Copy comes from vice_hud itself: the skill names, units and blurbs are read
out of skills.lua and the exercise names and set length out of config.lua, so
a change there is picked up by running this again.

Pictures (CSS px at 1080p, layout boxes in manifest_skills.txt):
  skills_scrim                 #skills' full-screen radial scrim (scale 1, stretched to any screen)
  skills_plate                 #skills-panel's surface alone, at its full 78cqh height
  skills_head / skills_foot    the header (title, line) and footer, no surface
  skills_close[_hover]         the close button, and its :hover state
  skills_name_<id>             a row's .sk-name
  skills_track                 .sk-track, empty
  skills_fill / skills_fill_max  the fill at scaleX(1), gold and the maxed green
  skills_fill_body / _cap      the fill unrounded, and its rounded left end
  skills_lvl_<0-9> / _max      .sk-level digits (tabular, so one advance) and MAX
  skills_sub_<0-9> / _slash    .sk-sub digits and " / "
  skills_sub_tail_<id>         " · <unit>"
  skills_blurb_<id>            .sk-sub as the blurb
  skills_thumb_top / _bottom   the ends of #skills-list's scrollbar thumb (CEF draws one)
  label_*                      prompt words for the gym (render_labels)
"""
import os
import re

from render_fivem_art import render_labels

GROUP = "skills"

ROW_PAD = 3


def _vice_hud_dir(ctx):
    path = ctx.index.replace("file:///", "")
    return os.path.dirname(os.path.dirname(path))


def read_skills(vice_hud):
    """id, label, unit and blurb of every skill, in skills.lua's order."""
    with open(os.path.join(vice_hud, "skills.lua"), encoding="utf-8") as f:
        src = f.read()
    body = src[src.index("Skills.List = {"):src.index("Skills.ById")]
    out = []
    for block in re.findall(r"\{(.*?)\n    \}", body, re.S):
        def field(name):
            m = re.search(name + r"\s*=\s*'((?:[^'\\]|\\.)*)'", block)
            return m.group(1).replace("\\'", "'") if m else None
        out.append({"id": field("id"), "label": field("label"), "unit": field("unit"), "blurb": field("blurb")})
    return out


def read_training(vice_hud):
    """(exercise labels in config.lua's order, setReps)."""
    with open(os.path.join(vice_hud, "config.lua"), encoding="utf-8") as f:
        src = f.read()
    block = src[src.index("Config.Training = {"):]
    block = block[:block.index("\n}\n")]
    labels = re.findall(r"^\s*\w+\s*=\s*\{\s*label\s*=\s*'([^']+)'", block, re.M)
    reps = int(re.search(r"setReps\s*=\s*(\d+)", block).group(1))
    return labels, reps


def skills_msg(skills, level=50, maxed=0, show=True):
    rows = []
    for i, s in enumerate(skills):
        m = i < maxed
        rows.append(dict(id=s["id"], label=s["label"], unit=s["unit"], blurb=s["blurb"],
                         level=100 if m else level, into=123, need=1000, frac=1 if m else 0.5, max=m))
    return {"action": "skills", "show": show, "skills": rows}


# Shown elsewhere on the page and nothing to do with this: the HUD stage and the
# arrival animation, which would otherwise be caught half way.
QUIET = """
() => {
  const s = document.createElement('style');
  s.textContent = '#stage > *:not(#skillup){visibility:hidden!important}'
               + '#skillup.slot-in #skillup-card{animation:none!important}';
  document.head.appendChild(s);
}
"""


def text_box(page, selector, text, pre=True):
    """Put `text` in `selector` and return the box its glyph run takes (the
    element's line box, as wide as the run's advance)."""
    return page.evaluate("""([sel, text, pre]) => {
      const el = document.querySelector(sel);
      el.style.whiteSpace = pre ? 'pre' : '';
      el.textContent = text;
      const r = document.createRange(); r.selectNodeContents(el);
      const run = r.getBoundingClientRect(), box = el.getBoundingClientRect();
      return {x: run.x, y: box.y, width: run.width, height: box.height};
    }""", [selector, text, pre])


def render(browser, ctx):
    vice_hud = _vice_hud_dir(ctx)
    skills = read_skills(vice_hud)
    exercises, reps = read_training(vice_hud)

    # ---- the scrim, full screen at scale 1 --------------------------------------
    page = ctx.page(browser, scale=1)
    page.evaluate(QUIET)
    ctx.post(page, skills_msg(skills))
    page.wait_for_timeout(150)
    page.evaluate("() => { document.getElementById('skills-panel').style.visibility = 'hidden'; }")
    ctx.full(page, "skills_scrim", 1920, 1080)
    page.close()

    page = ctx.page(browser)
    page.evaluate(QUIET)
    ctx.post(page, skills_msg(skills))
    page.wait_for_timeout(150)

    # ---- the panel surface, alone --------------------------------------------------
    page.evaluate("""() => {
      document.getElementById('skills').style.background = 'transparent';
      for (const id of ['skills-head', 'skills-list', 'skills-foot']) document.getElementById(id).style.visibility = 'hidden';
    }""")
    ctx.element(page, "#skills-panel", "skills_plate", pad=14)

    # ---- header, close button, footer: no surface under them ----------------------
    page.evaluate("""() => {
      const p = document.getElementById('skills-panel');
      p.style.background = 'none'; p.style.boxShadow = 'none';
      for (const id of ['skills-head', 'skills-foot']) document.getElementById(id).style.visibility = '';
      document.getElementById('skills-close').style.visibility = 'hidden';
    }""")
    ctx.element(page, "#skills-head", "skills_head", pad=4)
    ctx.element(page, "#skills-foot", "skills_foot", pad=4)
    page.evaluate("() => { document.getElementById('skills-close').style.visibility = 'visible'; document.getElementById('skills-head').style.visibility = 'hidden'; }")
    page.mouse.move(5, 5)
    ctx.element(page, "#skills-close", "skills_close", pad=4)
    page.hover("#skills-close")
    page.wait_for_timeout(100)
    ctx.element(page, "#skills-close", "skills_close_hover", pad=4)
    page.mouse.move(5, 5)

    # ---- rows ------------------------------------------------------------------------
    # One skill at a time in the list, and nothing but the part being cut visible.
    def one_row(skill, level=50, maxed=False, frac=0.5):
        ctx.post(page, {"action": "skills", "show": True, "skills": [dict(
            id=skill["id"], label=skill["label"], unit=skill["unit"], blurb=skill["blurb"],
            level=100 if maxed else level, into=123, need=1000, frac=1 if maxed else frac, max=maxed)]})
        page.wait_for_timeout(40)
        page.evaluate("""() => {
          document.getElementById('skills-list').style.visibility = 'visible';
          document.getElementById('skills-head').style.visibility = 'hidden';
          document.getElementById('skills-foot').style.visibility = 'hidden';
          for (const c of document.querySelectorAll('#skills-list .sk *')) c.style.visibility = 'hidden';
        }""")

    def only(selector):
        page.evaluate("""sel => {
          const el = document.querySelector(sel);
          el.style.visibility = 'visible';
          for (const c of el.querySelectorAll('*')) c.style.visibility = 'visible';
        }""", selector)

    for s in skills:
        one_row(s)
        only("#skills-list .sk-name")
        ctx.element(page, "#skills-list .sk-name", "skills_name_" + s["id"], pad=ROW_PAD)

    one_row(skills[0])
    page.evaluate("() => { document.querySelector('#skills-list .sk-track').style.visibility = 'visible'; }")
    ctx.element(page, "#skills-list .sk-track", "skills_track", pad=2)

    for maxed, name in ((False, "skills_fill"), (True, "skills_fill_max")):
        one_row(skills[0], maxed=maxed, frac=1)
        only("#skills-list .sk-track")
        page.evaluate("() => { document.querySelector('#skills-list .sk-track').style.background = 'transparent'; }")
        box = ctx.element(page, "#skills-list .sk-track", name, pad=2)
        if not maxed:
            el = page.query_selector("#skills-list .sk-track").bounding_box()
            # The rounded left end, 8px of it; the rest is drawn from the unrounded body.
            ctx.box(page, {"x": el["x"], "y": el["y"], "width": 8, "height": el["height"]}, "skills_fill_cap", pad=0)
            page.evaluate("() => { document.querySelector('#skills-list .sk-track').style.borderRadius = '0'; }")
            ctx.box(page, el, "skills_fill_body", pad=0)

    for d in range(10):
        one_row(skills[0], level=d)
        only("#skills-list .sk-level")
        ctx.element(page, "#skills-list .sk-level", "skills_lvl_%d" % d, pad=ROW_PAD)
    one_row(skills[0], maxed=True)
    only("#skills-list .sk-level")
    ctx.element(page, "#skills-list .sk-level", "skills_lvl_max", pad=ROW_PAD)

    # The sub line: digits (tabular, so each is the same advance), " / ", and the
    # unit after its " · ", each cut as wide as its own run so they can be laid
    # end to end. The page writes it as into + ' / ' + need + '  ·  ' + unit.
    one_row(skills[0])
    only("#skills-list .sk-sub")
    for d in range(10):
        ctx.box(page, text_box(page, "#skills-list .sk-sub", str(d)), "skills_sub_%d" % d, pad=ROW_PAD)
    ctx.box(page, text_box(page, "#skills-list .sk-sub", " / "), "skills_sub_slash", pad=ROW_PAD)
    for s in skills:
        # Two spaces either side of the dot in app.js; white-space: normal makes them one.
        ctx.box(page, text_box(page, "#skills-list .sk-sub", " · " + s["unit"]), "skills_sub_tail_" + s["id"], pad=ROW_PAD)
        ctx.box(page, text_box(page, "#skills-list .sk-sub", s["blurb"], pre=False), "skills_blurb_" + s["id"], pad=ROW_PAD)

    # Check the cut-up line lays out as the real one does.
    one_row(skills[0])
    probe = page.evaluate("""() => {
      const el = document.querySelector('#skills-list .sk-sub');
      el.style.whiteSpace = ''; el.textContent = '123 / 1000  ·  ' + 'x';
      const t = el.firstChild, x0 = el.getBoundingClientRect().x, at = i => {
        const r = document.createRange(); r.setStart(t, i); r.setEnd(t, i + 1); return r.getBoundingClientRect().x - x0; };
      const s = t.textContent; return {slash: at(s.indexOf('/') - 1), need: at(s.indexOf('1000')), dot: at(s.indexOf('1000') + 4)};
    }""")
    m = ctx.manifest
    dw, sw = m["skills_sub_1"][4], m["skills_sub_slash"][4]
    print("  sub line check (page vs laid end to end): slash %.2f/%.2f need %.2f/%.2f dot %.2f/%.2f"
          % (probe["slash"], 3 * dw, probe["need"], 3 * dw + sw, probe["dot"], 7 * dw + sw))

    # ---- the list's scrollbar thumb -------------------------------------------------
    # FiveM's CEF draws #skills-list::-webkit-scrollbar; headless Edge hides
    # scrollbars, so the thumb is drawn from that rule onto a stand-in div.
    # file:// sheets cannot be read back from the page, so the two rules come
    # out of style.css itself.
    with open(os.path.join(vice_hud, "html", "style.css"), encoding="utf-8") as f:
        css = f.read()
    def rule(sel):
        m = re.search(re.escape(sel) + r"\s*\{([^}]*)\}", css)
        if not m:
            raise RuntimeError("no rule " + sel)
        return m.group(1)
    page.evaluate("""([thumb, bar]) => {
      const d = document.createElement('div');
      d.id = 'sk-thumb-standin';
      d.style.cssText = thumb + ';' + bar + ';position:fixed;left:100px;top:100px;height:120px;z-index:999';
      document.body.appendChild(d);
      document.getElementById('skills').style.visibility = 'hidden';
    }""", [rule("#skills-list::-webkit-scrollbar-thumb"), rule("#skills-list::-webkit-scrollbar")])
    t = page.query_selector("#sk-thumb-standin").bounding_box()
    end = t["width"]
    ctx.box(page, {"x": t["x"], "y": t["y"], "width": t["width"], "height": end}, "skills_thumb_top", pad=0)
    ctx.box(page, {"x": t["x"], "y": t["y"] + t["height"] - end, "width": t["width"], "height": end}, "skills_thumb_bottom", pad=0)
    page.close()

    # ---- gym prompts ---------------------------------------------------------------
    # The exercise on its own (next to a bench or bar), the set's running count
    # (FiveM's text UI "Push-ups  3/25  ·  [X] stop"), and Stop.
    words = list(exercises) + ["Stop"]
    words += ["%s %d/%d" % (ex, i, reps) for ex in exercises for i in range(reps + 1)]
    render_labels(browser, ctx, words)
