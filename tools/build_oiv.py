"""Build the GTA VI-Inspired HUD OIV packages.

    python tools/build_oiv.py legacy         --dlcpacks <folder> [--startscreen <folder>] [--out build]
    python tools/build_oiv.py enhanced       --dlcpacks <folder> [--out build]
    python tools/build_oiv.py lucia-legacy   --dlcpacks <folder> [--out build]
    python tools/build_oiv.py lucia-enhanced --dlcpacks <folder> [--out build]
    python tools/build_oiv.py patch          [--out build]

<folder> holds one sub-folder per pack, each with its dlc.rpf: vihud, vihudfonts
and vihudanims for the HUD, luciaped and luciaolf for Lucia. Legacy and Enhanced
need different dlcpacks: the Enhanced ones are converted to the gen9 resource
format. The HUD packages carry the scripts, ViMinimap.asi and the HUD packs;
the Lucia packages only her two packs (and register them in dlclist.xml).

--startscreen (Legacy): every file in that folder goes into the game's
update\\update.rpf\\x64\\data\\cdimages\\scaleform_frontend.rpf (FBICat's VI start
screen and our edits to Rockstar's movies). That archive is NG-encrypted, so
the package asks the installer (OpenIV) to do it, as FBICat's own package does:
a nested <archive> in place of <!-- STARTSCREEN --> in oiv/legacy.xml.

The package is a zip laid out like FBICat's originals: assembly.xml and
icon.png at the top, the files under content/. assembly.xml comes from
oiv/<edition>.xml, with the <add> list generated from the files found here.

File names carry the version: the HUD packages the one in their <tag>
(GTA-VI-Inspired-HUD-2.5.5-Legacy.oiv), the RegisterPatch its own
(GTA-VI-Inspired-HUD-RegisterPatch-1.7.oiv). The Lucia packages keep theirs.

Every file keeps its real modification time in the zip, and the installers
write that time. Script Hook V .NET loads scripts through .NET shadow copies
(%LOCALAPPDATA%\\assembly\\dl3) keyed on path and file time, so one fixed date
for every build made updates keep running the first version a game cached.
Only the standard library is used.
"""
import argparse
import hashlib
import os
import re
import sys
import time
import zipfile
from xml.sax.saxutils import escape

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
HUD_PACKS = ("vihud", "vihudfonts", "vihudanims")
LUCIA_PACKS = ("luciaped", "luciaolf")
ROOT_FILES = ("ViMinimap.asi",)
# Already compressed, or too big to be worth deflating; FBICat's packages store these.
STORED = {".rpf", ".png", ".mp3", ".ogg", ".dds", ".jpg"}
# Never packaged: player saves and caches the HUD writes at run time.
SKIP_DIRS = {"Save", ".muffled"}
# The 2.5 art is drawn from the vihudfonts pack's texture dictionaries (tools/build_txd.py);
# the PNGs those are built from stay in the repo, their manifests ship.
ART_PNGS = "scripts/ViExtras/Art/"
EDITIONS = ("legacy", "enhanced", "lucia-legacy", "lucia-enhanced", "patch")
FIXED_NAMES = {
    "lucia-legacy": "GTA-VI-Inspired-HUD-Lucia-Legacy.oiv",
    "lucia-enhanced": "GTA-VI-Inspired-HUD-Lucia-Enhanced.oiv",
}
STARTSCREEN_ARCHIVE = r"x64\data\cdimages\scaleform_frontend.rpf"


def template(edition):
    with open(os.path.join(ROOT, "oiv", f"{edition}.xml"), encoding="utf-8") as f:
        return f.read()


def output_name(edition):
    if edition in FIXED_NAMES:
        return FIXED_NAMES[edition]
    xml = template(edition)
    if edition == "patch":
        m = re.search(r"<major>(\d+)</major><minor>(\d+)</minor>", xml)
        if not m:
            sys.exit("oiv/patch.xml: no <major>/<minor>")
        return f"GTA-VI-Inspired-HUD-RegisterPatch-{m.group(1)}.{m.group(2)}.oiv"
    m = re.search(r"<tag>[A-Z ]*?(\d+\.\d+\.\d+)</tag>", xml)
    if not m:
        sys.exit(f"oiv/{edition}.xml: no version in <tag>")
    return f"GTA-VI-Inspired-HUD-{m.group(1)}-{edition.capitalize()}.oiv"


def collect_files(dlcpacks, lucia):
    """Return (source path on disk, path inside content/, install path) for every file."""
    files = []
    if lucia:
        add_packs(files, dlcpacks, LUCIA_PACKS)
        return files
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
    add_packs(files, dlcpacks, HUD_PACKS)
    return files


def add_packs(files, dlcpacks, packs):
    for pack in packs:
        src = os.path.join(dlcpacks, pack, "dlc.rpf")
        if not os.path.isfile(src):
            sys.exit(f"missing {src}")
        files.append((src, f"dlcpacks/{pack}/dlc.rpf",
                      f"update\\x64\\dlcpacks\\{pack}\\dlc.rpf"))


def collect_startscreen(folder):
    """(source path, path inside content/, name inside scaleform_frontend.rpf) for every start screen file."""
    names = sorted(n for n in os.listdir(folder) if os.path.isfile(os.path.join(folder, n)))
    if not names:
        sys.exit(f"no files in {folder}")
    return [(os.path.join(folder, n), f"startscreen/{n}", n) for n in names]


def assembly_xml(edition, files, startscreen):
    xml = template(edition)
    if "<!-- FILES -->" in xml:
        lines = [f'    <add source="{escape(content.replace("/", chr(92)))}">{escape(install)}</add>'
                 for _, content, install in files]
        xml = xml.replace("    <!-- FILES -->", "\n".join(lines))
    if "<!-- STARTSCREEN -->" in xml:
        block = ""
        if startscreen:
            lines = ['    <archive path="update\\update.rpf" createIfNotExist="True" type="RPF7">',
                     f'      <archive path="{STARTSCREEN_ARCHIVE}" createIfNotExist="False" type="RPF7">']
            lines += [f'        <add source="{escape(content.replace("/", chr(92)))}">{escape(name)}</add>'
                      for _, content, name in startscreen]
            lines += ['      </archive>', '    </archive>']
            block = "\n".join(lines)
        xml = xml.replace("    <!-- STARTSCREEN -->", block)
    elif startscreen:
        sys.exit(f"oiv/{edition}.xml has no <!-- STARTSCREEN --> place for --startscreen")
    return xml


def zip_time(src=None):
    t = time.localtime(os.path.getmtime(src) if src else time.time())
    return (max(t.tm_year, 1980), t.tm_mon, t.tm_mday, t.tm_hour, t.tm_min, t.tm_sec)


def write_entry(zf, arcname, data=None, src=None):
    ext = os.path.splitext(arcname)[1].lower()
    info = zipfile.ZipInfo(arcname, date_time=zip_time(src))
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
    ap.add_argument("edition", choices=EDITIONS)
    ap.add_argument("--dlcpacks", help="folder with vihud/, vihudfonts/, vihudanims/ (HUD) or luciaped/, luciaolf/ (Lucia)")
    ap.add_argument("--startscreen", help="Legacy: folder of files for update.rpf's scaleform_frontend.rpf")
    ap.add_argument("--out", default=os.path.join(ROOT, "build"))
    args = ap.parse_args()

    startscreen = []
    if args.edition == "patch":
        files = []
        extra = {"content/readme.txt": b"GTA VI-Inspired HUD gameconfig + dlclist patch: the edits are in assembly.xml.\r\n"}
    else:
        if not args.dlcpacks:
            ap.error(f"{args.edition} needs --dlcpacks")
        files = collect_files(args.dlcpacks, lucia=args.edition.startswith("lucia"))
        extra = {}
    if args.startscreen:
        if args.edition != "legacy":
            ap.error("--startscreen is for the Legacy package")
        startscreen = collect_startscreen(args.startscreen)

    os.makedirs(args.out, exist_ok=True)
    out = os.path.join(args.out, output_name(args.edition))
    tmp = out + ".part"
    with zipfile.ZipFile(tmp, "w", allowZip64=True) as zf:
        write_entry(zf, "assembly.xml", data=assembly_xml(args.edition, files, startscreen).encode("utf-8"))
        write_entry(zf, "icon.png", src=os.path.join(ROOT, "oiv", "icon.png"))
        for arcname, data in extra.items():
            write_entry(zf, arcname, data=data)
        for src, content, _ in files + startscreen:
            write_entry(zf, "content/" + content, src=src)
    os.replace(tmp, out)

    sha = hashlib.sha256()
    with open(out, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 22), b""):
            sha.update(chunk)
    extra_note = f" + {len(startscreen)} start screen" if startscreen else ""
    print(f"{out}  {os.path.getsize(out):,} bytes  {len(files)} files{extra_note}  sha256 {sha.hexdigest()}")


if __name__ == "__main__":
    main()
