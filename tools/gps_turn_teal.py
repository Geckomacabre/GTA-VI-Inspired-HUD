"""FBICat's turn-by-turn box in the waypoint's teal.

    python tools/gps_turn_teal.py <hud_gps_bg.dds in> <hud_gps_bg.dds out>

GTA6Hud's turn-by-turn box (UpdateTurnNavigation) is its vipause texture
hud_gps_bg: a dark panel with a pink square at its right end for the arrow,
drawn untinted and faded in and out. The route it follows is your own
waypoint's, which the minimap draws teal (ViExtras MinimapColours: vice_hud's
#47aba7), so the square is recoloured teal in the texture itself; anything
drawn over it would let the pink show through while the box fades.

Each pixel of the square moves from the pink towards the teal by how pink it
is (on red and green, against the panel), so its rounded corners, its
highlight and its edge against the panel keep their shading; the panel, its
border included, is left alone. Written as DXT5 with two mip levels,
like FBICat's (his own blocks kept outside the square), into the vipause.ytd of the vihudfonts pack (the CodeWalker XML
export's folder, then cw_to_binary).
"""
import os
import sys

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from build_txd import dds_dxt5  # noqa: E402

PINK, PANEL = (255, 134, 189), (41, 36, 49)   # hud_gps_bg's square and panel
SQUARE_FROM = 472 / 608                        # the square's left edge (the dark rule before it), of the width
TEAL = (71, 171, 167)                          # vice_hud's waypoint #47aba7


def splice(orig, teal, w, h, mips):
    """FBICat's DDS with only the DXT5 blocks that cover the square taken from the
    recoloured one: the panel keeps his exact pixels (re-encoding it would move a
    few). The square starts on a block boundary at every level used."""
    assert orig[84:88] == b"DXT5" and teal[84:88] == b"DXT5"
    out = bytearray(orig)
    at = 128
    for level in range(mips):
        lw, lh = max(1, w >> level), max(1, h >> level)
        bw, bh = (lw + 3) // 4, (lh + 3) // 4
        first = int(lw * SQUARE_FROM) // 4
        assert first * 4 == int(lw * SQUARE_FROM), "the square is not block aligned"
        for row in range(bh):
            start = at + (row * bw + first) * 16
            end = at + (row + 1) * bw * 16
            out[start:end] = teal[start:end]
        at += bw * bh * 16
    return bytes(out)


def main():
    src = Image.open(sys.argv[1]).convert("RGBA")
    out = src.copy()
    px = out.load()
    for y in range(out.height):
        # only the square: the panel's light 1 px border reads a little pink too
        for x in range(int(out.width * SQUARE_FROM), out.width):
            r, g, b, a = px[x, y]
            t = ((r - PANEL[0]) / (PINK[0] - PANEL[0]) + (g - PANEL[1]) / (PINK[1] - PANEL[1])) / 2
            t = max(0.0, min(1.0, t))
            if t <= 0.0:
                continue
            px[x, y] = tuple(int(round(max(0, min(255, c + t * (q - p))))) for c, p, q in zip((r, g, b), PINK, TEAL)) + (a,)
    levels = [out, out.resize((out.width // 2, out.height // 2), Image.LANCZOS)]
    teal = dds_dxt5(levels)
    with open(sys.argv[1], "rb") as f:
        orig = f.read()
    with open(sys.argv[2], "wb") as f:
        f.write(splice(orig, teal, src.width, src.height, len(levels)))
    print(f"hud_gps_bg {out.width}x{out.height} recoloured teal")


if __name__ == "__main__":
    main()
