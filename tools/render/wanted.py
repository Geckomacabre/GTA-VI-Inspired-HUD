"""Wanted: vice_hud's wanted notification (index.html #wanted), the box that
tells you what the police are doing, fed the `wanted` message client.lua sends.

It says one thing per star state (app.js WANTED_COPY) with the box's own star
in that state, and under it the tells (#wanted-tells: what the police have on
you). The box is cut once per state without tells and once with the tells
row's room in it; each tell is cut as it sits in that row, and where the
first one sits and how far apart they are is in manifest_wanted.txt, so
src/ViExtras/Wanted/WantedBox.cs lays any set of them out:

  wanted_box_<state>      #wanted with no tells: contact, searching, hollow, red
  wanted_boxt_<state>     the same with the tells row's room, the tells left out
  wanted_box(t)_searching_on   searching with its star on the filled half of
                          its blink (starHunt, 900 ms: hollow, then filled)
  wanted_tell_<name>      one .tell as it sits in the box: camera, weapon,
                          person, people, vehicle, hanger

Where the tells row starts in the box and the step between tells come from the
CSS (padding 1.1 --w, 1.5cqh; gap 1.1cqh; .tell plus a 0.55 --w gap) and are
checked here against the page: WantedBox.cs has them as constants.
"""
import os

GROUP = "wanted"

STATES = ["contact", "searching", "hollow", "red"]
TELLS = ["camera", "weapon", "person", "people", "vehicle", "hanger"]

# The box alone, every transition settled; the searching star's blink frozen
# on its filled half so the cut is the same every run.
ISOLATE = """
() => {
    const box = document.getElementById('wanted');
    for (const el of document.body.querySelectorAll('*')) el.style.transition = 'none';
    box.classList.remove('hidden');
    box.style.opacity = '1';
    for (const el of document.getElementById('stage').children) if (el !== box && !el.contains(box)) el.style.visibility = 'hidden';
    box.style.visibility = 'visible';
    for (const p of box.querySelectorAll('.star path')) { p.style.animation = 'none'; }
}
"""


def render(browser, ctx):
    page = ctx.page(browser, 1920, 1080)

    for state in STATES:
        for with_tells in (False, True):
            ctx.fresh(page)
            ctx.post(page, {"action": "wanted", "active": True, "stars": 3, "maxStars": 6, "state": state,
                            "tells": ["hanger"] if with_tells else []})
            page.wait_for_timeout(250)
            page.evaluate(ISOLATE)
            if with_tells:
                page.evaluate("() => { for (const t of document.querySelectorAll('#wanted-tells .tell')) t.style.visibility = 'hidden'; }")
            page.wait_for_timeout(80)
            name = ("wanted_boxt_" if with_tells else "wanted_box_") + state
            ctx.element(page, "#wanted", name, pad=12)
            if state == "searching":
                # @keyframes starHunt: the star's other half, filled #f2f2f0
                page.evaluate("() => { for (const p of document.querySelectorAll('#wanted .star path')) p.style.fill = '#f2f2f0'; }")
                page.wait_for_timeout(50)
                ctx.element(page, "#wanted", name + "_on", pad=12)

    # Each tell as it sits in the box, and where the row starts.
    for tell in TELLS:
        ctx.fresh(page)
        ctx.post(page, {"action": "wanted", "active": True, "stars": 3, "maxStars": 6, "state": "contact", "tells": [tell]})
        page.wait_for_timeout(250)
        page.evaluate(ISOLATE)
        # the tell alone: the box behind it is drawn already, and a second copy of
        # its translucent plate would show as a darker square round the circle
        page.evaluate("() => { const b = document.getElementById('wanted'); b.style.background = 'transparent'; b.style.boxShadow = 'none'; }")
        page.wait_for_timeout(150)   # the icon loads
        ctx.element(page, "#wanted-tells .tell", "wanted_tell_" + tell, pad=2)

    ctx.fresh(page)
    ctx.post(page, {"action": "wanted", "active": True, "stars": 3, "maxStars": 6, "state": "contact", "tells": ["camera", "weapon"]})
    page.wait_for_timeout(250)
    page.evaluate(ISOLATE)
    row = page.evaluate("""() => {
        const box = document.getElementById('wanted').getBoundingClientRect();
        const t = document.querySelectorAll('#wanted-tells .tell');
        const a = t[0].getBoundingClientRect(), b = t[1].getBoundingClientRect();
        return { x: a.left - box.left, y: a.top - box.top, step: b.left - a.left };
    }""")
    page.close()
    print("wanted: tells row at x %.2f y %.2f, step %.2f (WantedBox.cs TellX / TellY / TellStep)" % (row["x"], row["y"], row["step"]))
