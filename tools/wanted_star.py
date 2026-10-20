"""Cut the wanted box's star out into a picture of its own, so it stays sharp.

    python tools/wanted_star.py

The wanted boxes (wanted_box_<state> and wanted_boxt_<state>, 936 x 252 / 936 x 304
at 2x) are bigger than 128 x 128, so tools/build_txd.py packs them as DXT5, and
DXT5's 4 x 4 blocks broke the small star in the top right corner up. This cuts a
60 x 60 square round the star (on DXT5's 4 x 4 grid) out of each state into
wanted_star_<state>, which packs uncompressed, and leaves that square empty in
both boxes of the state, so the box packs empty blocks there. WantedBox draws the
star over the box at StarX / StarY (the square's place at 1080p, 406 x 26).

Run it after tools/render/wanted.py. It is safe to run again: a state whose
square is already empty keeps the star it was cut to.
"""
import os
import re
from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "scripts", "ViExtras", "Art")
MANIFEST = os.path.join(ART, "manifest_wanted.txt")
STATES = ("contact", "hollow", "red", "searching", "searching_on")
SQUARE = (812, 52, 872, 112)   # x0, y0, x1, y1 in the 2x pictures; both boxes of a state share it


def main():
    x0, y0, x1, y1 = SQUARE
    assert all(v % 4 == 0 for v in SQUARE), "the square must sit on DXT5's 4 x 4 grid"
    lines = []
    for state in STATES:
        boxes = [os.path.join(ART, f"wanted_{kind}_{state}.png") for kind in ("box", "boxt")]
        star_path = os.path.join(ART, f"wanted_star_{state}.png")
        first = Image.open(boxes[0]).convert("RGBA")
        cut = first.crop(SQUARE)
        if cut.getchannel("A").getextrema()[1] > 0:
            # both boxes show the same star on the same plate there
            for other in boxes[1:]:
                assert Image.open(other).convert("RGBA").crop(SQUARE).tobytes() == cut.tobytes(), other
            cut.save(star_path)
            empty = Image.new("RGBA", cut.size, (0, 0, 0, 0))
            for path in boxes:
                im = Image.open(path).convert("RGBA")
                im.paste(empty, (x0, y0))
                im.save(path)
            print("cut", os.path.basename(star_path))
        elif not os.path.exists(star_path):
            raise SystemExit(f"{state}: the square is empty but there is no {os.path.basename(star_path)}")
        w, h = (x1 - x0) / 2, (y1 - y0) / 2
        lines.append(f"wanted_star_{state} {w:.2f} {h:.2f} 0.00 0.00 {w:.2f} {h:.2f}")

    with open(MANIFEST, encoding="utf-8") as f:
        kept = [l.rstrip("\n") for l in f if not re.match(r"wanted_star_", l)]
    with open(MANIFEST, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(kept + lines) + "\n")
    print("manifest_wanted.txt:", len(lines), "stars")


if __name__ == "__main__":
    main()
