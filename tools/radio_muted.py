"""The radio wheel's muted look, made ahead of time.

    python tools/radio_muted.py

vi_radio greys the wheel out while muted with CSS `filter: grayscale(1)
brightness(.62)` on #viewport, #icon-left and #info. The single-player port
used to make those pictures at run time; now that the art is drawn from a
game texture dictionary it has to exist beforehand. For every picture in
Art/manifest_radio.txt this writes <name>_muted.png (same size, same box)
and lists them in Art/manifest_radiomuted.txt.

The filter is a colour matrix on unpremultiplied sRGB, alpha untouched:
luma = 0.2126 R + 0.7152 G + 0.0722 B, then times 0.62.
"""
import os

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "scripts", "ViExtras", "Art")


def main():
    src = os.path.join(ART, "manifest_radio.txt")
    lines = [l.split() for l in open(src, encoding="utf-8") if l.strip() and not l.startswith("#")]
    out = {}
    for parts in lines:
        name = parts[0]
        if name.endswith("_muted"):
            continue          # vi_radio's own muted icon is rendered from the page
        img = Image.open(os.path.join(ART, name + ".png")).convert("RGBA")
        px = img.load()
        for y in range(img.height):
            for x in range(img.width):
                r, g, b, a = px[x, y]
                if a == 0:
                    continue
                v = int(round(min(255.0, (0.2126 * r + 0.7152 * g + 0.0722 * b) * 0.62)))
                px[x, y] = (v, v, v, a)
        img.save(os.path.join(ART, name + "_muted.png"))
        out[name + "_muted"] = parts[1:]
    with open(os.path.join(ART, "manifest_radiomuted.txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("# name  width height  boxX boxY  boxWidth boxHeight   (CSS px at 1080p)\n")
        for name in sorted(out):
            f.write(name + " " + " ".join(out[name]) + "\n")
    print(f"radio: {len(out)} muted pictures")


if __name__ == "__main__":
    main()
