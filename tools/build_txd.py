"""Pack the 2.5 art into game texture dictionaries.

    python tools/build_txd.py [--out build/viextras_txd]

Script Hook V's own textures (SHVDN CustomSprite) stop GTA V Enhanced
outright when one cannot be made (a side that is not a multiple of 4, a
full-screen picture), so the 2.5 art is drawn the way GTA6Hud draws its
main HUD instead: from streamed texture
dictionaries, with the game's DRAW_SPRITE. Every Art/manifest_<group>.txt
becomes one dictionary, vix_<group>, holding a texture per picture, named
as the picture. The scripts find a picture's dictionary from the manifest
it is listed in.

This writes CodeWalker XML: <out>/vix_<group>.ytd.xml beside a folder of
.dds files, ready for CodeWalker's XML import (cw_to_binary). The .ytd files
then go into the vihudfonts pack's vihudfonts.rpf, next to GTA6Hud's own.

Formats:
- up to 128 x 128 pixels (every button and prompt word): uncompressed A8R8G8B8, so small type stays crisp;
- larger: DXT5, as all of GTA6Hud's art is.
- art rendered at 2x gets a half-size mip level; full-screen art has none.
- glyph atlases (TEXT_GROUPS: customsfont, tools/render/customs_fonts.py):
  A8R8G8B8 whatever their size, with a full mip chain down to about 4 px.
  DXT5's 4 x 4 blocks would smear letters' edges, and one atlas is drawn at
  every size from 17 to 51 px, so each level is half the one before it, box
  filtered from the full picture (plain averaging: no ringing round letters).
Both sides of every picture are already multiples of 4 (the renderer pads).
"""
import argparse
import io
import os
import struct
import sys
from xml.sax.saxutils import escape

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ART = os.path.join(ROOT, "scripts", "ViExtras", "Art")
SMALL = 128 * 128
FULL_SCREEN_GROUPS = {"overlays", "focus"}
TEXT_GROUPS = {"customsfont"}


def read_manifests():
    groups = {}
    order = []
    for f in sorted(os.listdir(ART)):
        if not (f.startswith("manifest_") and f.endswith(".txt")):
            continue
        group = f[len("manifest_"):-4]
        for line in open(os.path.join(ART, f), encoding="utf-8"):
            if line.startswith("#") or not line.strip():
                continue
            name = line.split()[0]
            if name in groups:
                continue          # listed twice: the first group keeps it
            groups[name] = group
            order.append(name)
    return groups, order


def dds_rgba(levels):
    """Uncompressed A8R8G8B8 DDS with the given mip levels (PIL RGBA images)."""
    w, h = levels[0].size
    flags = 0x1 | 0x2 | 0x4 | 0x8 | 0x1000 | (0x20000 if len(levels) > 1 else 0)
    caps = 0x1000 | ((0x8 | 0x400000) if len(levels) > 1 else 0)
    pf = struct.pack("<II4sIIIII", 32, 0x41, b"\0\0\0\0", 32, 0x00FF0000, 0x0000FF00, 0x000000FF, 0xFF000000)
    head = struct.pack("<4sIIIIIII", b"DDS ", 124, flags, h, w, w * 4, 0, len(levels)) + b"\0" * 44 + pf
    head += struct.pack("<IIIII", caps, 0, 0, 0, 0)
    assert len(head) == 128
    data = b""
    for img in levels:
        r, g, b, a = img.split()
        data += Image.merge("RGBA", (b, g, r, a)).tobytes()
    return head + data


def dds_dxt5(levels):
    def enc(img):
        buf = io.BytesIO()
        img.save(buf, format="DDS", pixel_format="DXT5")
        d = buf.getvalue()
        assert d[84:88] == b"DXT5"
        return d[:128], d[128:]
    head, first = enc(levels[0])
    hdr = bytearray(head)
    if len(levels) > 1:
        struct.pack_into("<I", hdr, 8, struct.unpack("<I", hdr[8:12])[0] | 0x20000)
        struct.pack_into("<I", hdr, 28, len(levels))
        struct.pack_into("<I", hdr, 108, struct.unpack("<I", hdr[108:112])[0] | 0x8 | 0x400000)
    return bytes(hdr) + first + b"".join(enc(l)[1] for l in levels[1:])


def item(name, w, h, mips, fmt):
    return (" <Item>\n"
            f"  <Name>{escape(name)}</Name>\n"
            "  <Unk32 value=\"0\" />\n"
            "  <Usage>UNKNOWN</Usage>\n"
            "  <UsageFlags>0</UsageFlags>\n"
            "  <ExtraFlags value=\"0\" />\n"
            f"  <Width value=\"{w}\" />\n"
            f"  <Height value=\"{h}\" />\n"
            f"  <MipLevels value=\"{mips}\" />\n"
            f"  <Format>{fmt}</Format>\n"
            f"  <FileName>{escape(name)}.dds</FileName>\n"
            " </Item>\n")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default=os.path.join(ROOT, "build", "viextras_txd"))
    args = ap.parse_args()
    groups, order = read_manifests()
    by_group = {}
    for name in order:
        by_group.setdefault(groups[name], []).append(name)
    os.makedirs(args.out, exist_ok=True)
    total = 0
    for group, names in sorted(by_group.items()):
        txd = "vix_" + group
        tex_dir = os.path.join(args.out, txd)
        os.makedirs(tex_dir, exist_ok=True)
        items = []
        size = 0
        for name in names:
            png = os.path.join(ART, name + ".png")
            if not os.path.isfile(png):
                print("  missing", png)
                continue
            img = Image.open(png).convert("RGBA")
            w, h = img.size
            assert w % 4 == 0 and h % 4 == 0, (name, w, h)
            levels = [img]
            if group in TEXT_GROUPS:
                lw, lh = w, h
                while min(lw, lh) // 2 >= 4:
                    lw, lh = lw // 2, lh // 2
                    levels.append(img.resize((lw, lh), Image.BOX))
            elif group not in FULL_SCREEN_GROUPS and w >= 8 and h >= 8 and (w // 2) % 4 == 0 and (h // 2) % 4 == 0:
                levels.append(img.resize((w // 2, h // 2), Image.LANCZOS))
            if w * h <= SMALL or group in TEXT_GROUPS:
                data, fmt = dds_rgba(levels), "D3DFMT_A8R8G8B8"
            else:
                data, fmt = dds_dxt5(levels), "D3DFMT_DXT5"
            with open(os.path.join(tex_dir, name + ".dds"), "wb") as f:
                f.write(data)
            size += len(data) - 128
            items.append(item(name, w, h, len(levels), fmt))
        with open(os.path.join(args.out, txd + ".ytd.xml"), "w", encoding="utf-8", newline="\n") as f:
            f.write('<?xml version="1.0" encoding="UTF-8"?>\n<TextureDictionary>\n' + "".join(items) + "</TextureDictionary>")
        total += size
        print(f"{txd}: {len(items)} textures, {size / 2**20:.1f} MiB")
    print(f"total {total / 2**20:.1f} MiB")


if __name__ == "__main__":
    main()
