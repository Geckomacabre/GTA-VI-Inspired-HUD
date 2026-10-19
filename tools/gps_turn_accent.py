"""The turn box's coloured square, as a mask to tint.

    python tools/gps_turn_accent.py <hud_gps_bg.dds or .png>

FBICat's turn-by-turn box (GTA6Hud UpdateTurnNavigation) is his vipause
texture hud_gps_bg: a dark panel with a pink square at its right end (the
arrow sits on it), drawn untinted. ViExtras' TurnBoxColour paints that square
in the route's colour by drawing this mask over it, at the same place and size.

The mask is the pink square alone, white, its alpha how pink each pixel of
hud_gps_bg is (the square's rounded corners and its edge against the panel
included). Written to scripts/ViExtras/Art/gps_turn_accent.png and listed in
Art/manifest_gps.txt (same size as hud_gps_bg, drawn 304 x 60 like it).
"""
import os
import sys

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "scripts", "ViExtras", "Art")
PINK, PANEL = (255, 134, 189), (41, 36, 49)   # hud_gps_bg's square and panel


def main():
    src = Image.open(sys.argv[1]).convert("RGBA")
    w, h = src.size
    out = Image.new("RGBA", (w, h), (255, 255, 255, 0))
    sp, op = src.load(), out.load()
    for y in range(h):
        for x in range(w):
            r, g, b, a = sp[x, y]
            # how far this pixel is from the panel towards the pink, on red and green
            t = ((r - PANEL[0]) / (PINK[0] - PANEL[0]) + (g - PANEL[1]) / (PINK[1] - PANEL[1])) / 2
            t = max(0.0, min(1.0, t))
            op[x, y] = (255, 255, 255, int(round(a * t)))
    out.save(os.path.join(ART, "gps_turn_accent.png"))
    with open(os.path.join(ART, "manifest_gps.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("# name  width height  boxX boxY  boxWidth boxHeight   (CSS px at 1080p)\n")
        f.write("gps_turn_accent 304.00 60.00 0.00 0.00 304.00 60.00\n")
    print(f"gps_turn_accent {w}x{h}")


if __name__ == "__main__":
    main()
