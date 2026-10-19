"""Build the GTA VI-Inspired HUD OIV packages.

    python tools/build_oiv.py legacy   --dlcpacks <folder> [--out build]
    python tools/build_oiv.py enhanced --dlcpacks <folder> [--out build]
    python tools/build_oiv.py patch    [--out build]

<folder> holds one sub-folder per pack (luciaped, luciaolf, vihudfonts,
vihudanims), each with its dlc.rpf. Legacy and Enhanced need different
dlcpacks: the Enhanced ones are converted to the gen9 resource format.

The package is a zip laid out like FBICat's originals: assembly.xml and
icon.png at the top, the files under content/. assembly.xml comes from
oiv/<edition>.xml, with the <add> list generated from the files found here.
Only the standard library is used.
"""
import argparse
import hashlib
import os
import sys
import zipfile
from xml.sax.saxutils import escape

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PACKS = ("luciaped", "luciaolf", "vihudfonts", "vihudanims")
ROOT_FILES = ("ViMinimap.asi",)
# Already compressed, or too big to be worth deflating; FBICat's packages store these.
STORED = {".rpf", ".png", ".mp3", ".ogg", ".dds", ".jpg"}
# Never packaged: player saves and caches the HUD writes at run time.
SKIP_DIRS = {"Save", ".muffled"}
# The 2.5 art is drawn from the vihudfonts pack's texture dictionaries (tools/build_txd.py);
# the PNGs those are built from stay in the repo, their manifests ship.
ART_PNGS = "scripts/ViExtras/Art/"
OUTPUT_NAMES = {
    "legacy": "GTA-VI-Inspired-HUD-Legacy.oiv",
    "enhanced": "GTA-VI-Inspired-HUD-Enhanced.oiv",
    "patch": "GTA-VI-Inspired-HUD-RegisterPatch.oiv",
}


def collect_files(dlcpacks):
    """Return (source path on disk, path inside content/, install path) for every file."""
    files = []
    scripts = os.path.join(ROOT, "scripts")
    for dirpath, dirnames, filenames in os.walk(scripts):
        dirnames[:] = sorted(d for d in dirnames if d not in SKIP_DIRS)
        for name in sorted(filenames):
            src = os.path.join(dirpath, name)
            rel = os.path.relpath(src, ROOT).replace(os.sep, "/")
            if rel.startswith(ART_PNGS) and name.lower().endswith(".png"):
                continue
            files.append((src, rel, rel.replace("/", "\\")))
    for name in ROOT_FILES:
        src = os.path.join(ROOT, name)
        if not os.path.isfile(src):
            sys.exit(f"missing {name}")
        files.append((src, name, name))
    if dlcpacks:
        for pack in PACKS:
            src = os.path.join(dlcpacks, pack, "dlc.rpf")
            if not os.path.isfile(src):
                sys.exit(f"missing {src}")
            files.append((src, f"dlcpacks/{pack}/dlc.rpf",
                          f"update\\x64\\dlcpacks\\{pack}\\dlc.rpf"))
    return files


def assembly_xml(edition, files):
    with open(os.path.join(ROOT, "oiv", f"{edition}.xml"), encoding="utf-8") as f:
        template = f.read()
    if "<!-- FILES -->" not in template:
        return template
    lines = [f'    <add source="{escape(content.replace("/", chr(92)))}">{escape(install)}</add>'
             for _, content, install in files]
    return template.replace("    <!-- FILES -->", "\n".join(lines))


def write_entry(zf, arcname, data=None, src=None):
    ext = os.path.splitext(arcname)[1].lower()
    info = zipfile.ZipInfo(arcname, date_time=(2026, 10, 18, 0, 0, 0))
    info.compress_type = zipfile.ZIP_STORED if ext in STORED else zipfile.ZIP_DEFLATED
    info.external_attr = 0o644 << 16
    if data is not None:
        zf.writestr(info, data)
        return
    info.file_size = os.path.getsize(src)
    with open(src, "rb") as fin, zf.open(info, "w", force_zip64=info.file_size > 0x7FFFFFFF) as fout:
        while True:
            chunk = fin.read(1 << 22)
            if not chunk:
                break
            fout.write(chunk)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("edition", choices=sorted(OUTPUT_NAMES))
    ap.add_argument("--dlcpacks", help="folder with luciaped/, luciaolf/, vihudfonts/, vihudanims/")
    ap.add_argument("--out", default=os.path.join(ROOT, "build"))
    args = ap.parse_args()

    if args.edition == "patch":
        files = []
        extra = {"content/readme.txt": b"GTA VI-Inspired HUD gameconfig + dlclist patch: the edits are in assembly.xml.\r\n"}
    else:
        if not args.dlcpacks:
            ap.error(f"{args.edition} needs --dlcpacks")
        files = collect_files(args.dlcpacks)
        extra = {}

    os.makedirs(args.out, exist_ok=True)
    out = os.path.join(args.out, OUTPUT_NAMES[args.edition])
    tmp = out + ".part"
    with zipfile.ZipFile(tmp, "w", allowZip64=True) as zf:
        write_entry(zf, "assembly.xml", data=assembly_xml(args.edition, files).encode("utf-8"))
        write_entry(zf, "icon.png", src=os.path.join(ROOT, "oiv", "icon.png"))
        for arcname, data in extra.items():
            write_entry(zf, arcname, data=data)
        for src, content, _ in files:
            write_entry(zf, "content/" + content, src=src)
    os.replace(tmp, out)

    sha = hashlib.sha256()
    with open(out, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 22), b""):
            sha.update(chunk)
    print(f"{out}  {os.path.getsize(out):,} bytes  {len(files)} files  sha256 {sha.hexdigest()}")


if __name__ == "__main__":
    main()
