"""Build the GTA VI-Inspired HUD OIV packages.

    python tools/build_oiv.py legacy         --dlcpacks <folder> [--startscreen <folder>] [--out build]
    python tools/build_oiv.py enhanced       --dlcpacks <folder> [--startscreen <folder>] [--out build]
    python tools/build_oiv.py <edition> --loadingscreens --startscreen <folder> [--out build]
    python tools/build_oiv.py <edition> --uninstall [--out build]

--uninstall: the uninstaller (GTA-VI-Inspired-HUD-<version>-Uninstall-<Edition>.oiv,
from oiv/uninstall-<edition>.xml; the guides link to copies without the version
in extras/, so rebuild and copy them there when the HUD's file list changes).
It deletes every file the HUD package of this build installs, which takes in
every older version's (none was ever dropped), and takes the packs out of
dlclist.xml (oiv/unregister.xml, first).
The scripts DLLs and ViMinimap.asi go first, then the packs, then the rest, so
the HUD no longer loads even if a step after them stops. It leaves what the
package never installed (scripts\\Hud\\Save, logs), the files other mods share
(SHARED: LemonUI.SHVDN3.dll, the MiniMap.* data the standalone minimap mod
uses too), the raised gameconfig.xml pools (other mods may need them) and
the loading screens (putting Rockstar's movies back would mean shipping them).
The OIV Package Installer copies a package's update\\ files into mods\\ but
deletes a path as written, from the game folder, so the Enhanced packs are
deleted as mods\\update\\...; OpenIV deletes them from where it installed them,
as Rebalanced Dispatch Enhanced's uninstaller does. OpenIV won't open a package
without a content folder ("Unable to find "content" folder"), so a package
with nothing to install carries content/PLACEHOLDER, as other uninstallers do.

--loadingscreens: the start screen on its own, as an optional package
(GTA-VI-Inspired-HUD-<version>-LoadingScreens-<Edition>.oiv, from
oiv/loadingscreens-<edition>.xml): no scripts, packs or registration. Neither
installer has a tick-box for one part of a package, so players who don't want
FBICat's loading screens leave this one out; the HUD package is then built
without --startscreen.

<folder> holds one sub-folder per pack, each with its dlc.rpf: vihud, vihudfonts
and vihudanims. Legacy and Enhanced need different dlcpacks: the Enhanced ones
are converted to the gen9 resource format. The packages carry the scripts,
ViMinimap.asi and the HUD packs.

There are no Lucia packages any more: her model is someone else's paid work,
shared without their permission, so it isn't ours to publish (removed from
every release on 2026-10-22).

The HUD packages also register their packs, which the separate RegisterPatch
did up to 2.5.5: oiv/register.xml goes in place of <!-- REGISTER -->, after
every file. It raises four gameconfig.xml pools and MaxExtraPedModelInfos to
what the HUD needs, never lowering one a bigger mod set higher (Rebalanced
Dispatch Enhanced's are far higher): each value's element gets a scratch child,
the <replace> finds the value when it's lower and the scratch when it isn't,
and the <remove> takes whichever is left over (the scratch, or the second copy
the replace put in its place). It adds each pack to dlclist.xml
once. Each pack's <add> targets Paths when the pack isn't listed yet and an
empty <ViHudScratch /> when it is, and the scratch element is removed at the
end. That way every step finds its node: OpenIV calls the install FAILED when
one doesn't ("Unable to find XML Node"), which a plain guarded add did on
every update. The OIV installer for Enhanced runs all adds before the removes,
which this order needs too.

--startscreen: every file in that folder goes into the game's
update\\update.rpf\\x64\\data\\cdimages\\scaleform_frontend.rpf (FBICat's VI start
screen and our edits to Rockstar's movies). That archive is NG-encrypted, so
the package asks the installer (OpenIV, or the OIV Package Installer on
Enhanced) to do it, as FBICat's own package does: a nested <archive> in place
of <!-- STARTSCREEN --> in oiv/<edition>.xml. Legacy gets his landing page and
loading screens; Enhanced only the loading screens (its landing page is a
different movie), with the .ytd files in the gen9 format. A .ytd in the other
edition's format stops the build. With NaturalVision Enhanced the game doesn't
read the mods folder, so its players copy that scaleform_frontend.rpf into
onigiri\\platform\\data\\cdimages (INSTALL-ENHANCED.md).

The package is a zip laid out like FBICat's originals: assembly.xml and
icon.png at the top, the files under content/. assembly.xml comes from
oiv/<edition>.xml, with the <add> list generated from the files found here.

File names carry the version: the HUD packages the one in their <tag>
(GTA-VI-Inspired-HUD-2.5.5-Legacy.oiv).

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
ROOT_FILES = ("ViMinimap.asi",)
# Already compressed, or too big to be worth deflating; FBICat's packages store these.
STORED = {".rpf", ".png", ".mp3", ".ogg", ".dds", ".jpg"}
# Never packaged: player saves and caches the HUD writes at run time.
SKIP_DIRS = {"Save", ".muffled"}
# The 2.5 art is drawn from the vihudfonts pack's texture dictionaries (tools/build_txd.py);
# the PNGs those are built from stay in the repo, their manifests ship.
ART_PNGS = "scripts/ViExtras/Art/"
EDITIONS = ("legacy", "enhanced")
STARTSCREEN_ARCHIVE = r"x64\data\cdimages\scaleform_frontend.rpf"
# Never deleted by the uninstallers, which take off only what is the HUD's alone: other mods use LemonUI, and the
# standalone GTA VI MiniMap (3D Buildings) mod keeps its settings and learned map data in the same MiniMap.* files.
SHARED = {"scripts\\lemonui.shvdn3.dll", "scripts\\minimap.heights", "scripts\\minimap.levels",
          "scripts\\minimap.settings", "scripts\\minimap.water"}


def template(name):
    with open(os.path.join(ROOT, "oiv", f"{name}.xml"), encoding="utf-8") as f:
        return f.read()


def output_name(edition, part=""):
    xml = template(edition)
    m = re.search(r"<tag>[A-Z ]*?(\d+\.\d+\.\d+)</tag>", xml)
    if not m:
        sys.exit(f"oiv/{edition}.xml: no version in <tag>")
    return f"GTA-VI-Inspired-HUD-{m.group(1)}-{part}{edition.capitalize()}.oiv"


def collect_files(dlcpacks):
    """Return (source path on disk, path inside content/, install path) for every file."""
    files = collect_scripts()
    add_packs(files, dlcpacks, HUD_PACKS)
    return files


def collect_scripts():
    """collect_files without the packs: the scripts and ViMinimap.asi."""
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
    return files


def uninstall_paths(edition):
    """What the uninstaller deletes, in order: ViMinimap.asi and the scripts DLLs, the packs, then everything else."""
    files = [install for _, _, install in collect_scripts() if install.lower() not in SHARED]
    first = [f for f in files if "\\" not in f or (f.count("\\") == 1 and f.lower().endswith(".dll"))]
    prefix = "mods\\" if edition == "enhanced" else ""
    packs = [f"{prefix}update\\x64\\dlcpacks\\{pack}\\dlc.rpf" for pack in HUD_PACKS]
    return first + packs + [f for f in files if f not in first]


def add_packs(files, dlcpacks, packs):
    for pack in packs:
        src = os.path.join(dlcpacks, pack, "dlc.rpf")
        if not os.path.isfile(src):
            sys.exit(f"missing {src}")
        files.append((src, f"dlcpacks/{pack}/dlc.rpf",
                      f"update\\x64\\dlcpacks\\{pack}\\dlc.rpf"))


# resource version in a .ytd's RSC7 header: Legacy's format, and Enhanced's gen9 one
YTD_VERSION = {"legacy": 13, "enhanced": 5}


def collect_startscreen(edition, folder):
    """(source path, path inside content/, name inside scaleform_frontend.rpf) for every start screen file."""
    names = sorted(n for n in os.listdir(folder) if os.path.isfile(os.path.join(folder, n)))
    if not names:
        sys.exit(f"no files in {folder}")
    for n in names:
        if n.lower().endswith(".ytd"):
            with open(os.path.join(folder, n), "rb") as f:
                head = f.read(8)
            version = int.from_bytes(head[4:8], "little") if head[:4] == b"RSC7" else None
            if version != YTD_VERSION[edition]:
                sys.exit(f"{n} is resource version {version}; {edition} needs {YTD_VERSION[edition]}")
    return [(os.path.join(folder, n), f"startscreen/{n}", n) for n in names]


def assembly_xml(edition, files, startscreen, name=None, deletes=()):
    xml = template(name or edition)
    if "<!-- UNREGISTER -->" in xml:
        with open(os.path.join(ROOT, "oiv", "unregister.xml"), encoding="utf-8") as f:
            xml = xml.replace("    <!-- UNREGISTER -->", f.read().rstrip("\n"))
    if "<!-- DELETES -->" in xml:
        xml = xml.replace("    <!-- DELETES -->", "\n".join(f"    <delete>{escape(d)}</delete>" for d in deletes))
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
    if "<!-- REGISTER -->" in xml:
        with open(os.path.join(ROOT, "oiv", "register.xml"), encoding="utf-8") as f:
            xml = xml.replace("    <!-- REGISTER -->", f.read().rstrip("\n"))
    elif name is None:
        sys.exit(f"oiv/{edition}.xml has no <!-- REGISTER --> place for the pack registration")
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
    ap.add_argument("--dlcpacks", help="folder with vihud/, vihudfonts/ and vihudanims/")
    ap.add_argument("--startscreen", help="folder of files for update.rpf's scaleform_frontend.rpf")
    ap.add_argument("--loadingscreens", action="store_true", help="build the optional loading screens package only")
    ap.add_argument("--uninstall", action="store_true", help="build the uninstaller")
    ap.add_argument("--out", default=os.path.join(ROOT, "build"))
    args = ap.parse_args()

    startscreen = []
    deletes = []
    part = ""
    if args.uninstall:
        if args.loadingscreens or args.startscreen or args.dlcpacks:
            ap.error("--uninstall takes no --dlcpacks, --startscreen or --loadingscreens")
        files = []
        name = f"uninstall-{args.edition}"
        deletes = uninstall_paths(args.edition)
        part = "Uninstall-"
    elif args.loadingscreens:
        if not args.startscreen:
            ap.error("--loadingscreens needs --startscreen")
        files = []
        name = f"loadingscreens-{args.edition}"
        part = "LoadingScreens-"
    else:
        if not args.dlcpacks:
            ap.error(f"{args.edition} needs --dlcpacks")
        files = collect_files(args.dlcpacks)
        name = None
    if args.startscreen:
        startscreen = collect_startscreen(args.edition, args.startscreen)

    os.makedirs(args.out, exist_ok=True)
    out = os.path.join(args.out, output_name(args.edition, part))
    tmp = out + ".part"
    with zipfile.ZipFile(tmp, "w", allowZip64=True) as zf:
        write_entry(zf, "assembly.xml", data=assembly_xml(args.edition, files, startscreen, name, deletes).encode("utf-8"))
        write_entry(zf, "icon.png", src=os.path.join(ROOT, "oiv", "icon.png"))
        for src, content, _ in files + startscreen:
            write_entry(zf, "content/" + content, src=src)
        if not files and not startscreen:
            write_entry(zf, "content/PLACEHOLDER", data=b"OpenIV needs a content folder; this package installs nothing.\r\n")
    os.replace(tmp, out)

    sha = hashlib.sha256()
    with open(out, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 22), b""):
            sha.update(chunk)
    extra_note = f" + {len(startscreen)} start screen" if startscreen else ""
    if deletes:
        extra_note += f" + {len(deletes)} deletes"
    print(f"{out}  {os.path.getsize(out):,} bytes  {len(files)} files{extra_note}  sha256 {sha.hexdigest()}")


if __name__ == "__main__":
    main()
