# GTA VI-Inspired HUD

A GTA VI-style HUD, minimap and a playable **Lucia** for **GTA V story mode** on PC. Made by **FBICat**.
Version 2.5 adds features ported from the [FiveM version](https://github.com/Geckomacabre/GTA-VI-UI-and-HUD-for-FiveM).

> **Disclaimer:** fan-made and inspired by GTA VI. Not affiliated with Rockstar Games or Take-Two. Some
> HUD art is taken from Rockstar's official GTA VI media
> ([rockstargames.com/vi/media](https://www.rockstargames.com/VI/media)) and belongs to Rockstar Games.
> No GTA VI game files are included. The Lucia model is fan-made.

Want it for FiveM instead? → [GTA-VI-UI-and-HUD-for-FiveM](https://github.com/Geckomacabre/GTA-VI-UI-and-HUD-for-FiveM)

---

## How to install

There are two versions of GTA V on PC, and each has its own install guide. Open your GTA V game folder
and look at the game file:

| Your game folder has... | You have... | Your guide |
| --- | --- | --- |
| `GTA5.exe` | **Legacy** (the original PC version) | 👉 **[Legacy install guide](INSTALL-LEGACY.md)** |
| `GTA5_Enhanced.exe` | **Enhanced** (the 2025 PC version, tested on 1.0.1158.13 and 1.0.1158.16) | 👉 **[Enhanced install guide](INSTALL-ENHANCED.md)** |

Both guides are written for people who've never modded GTA before. Follow only the guide for your
version: the two use different tools, and mixing them up crashes the game.

---

## How to use it

| I want to... | Do this |
| --- | --- |
| **Play as Lucia** (Legacy) | Hold **Left Alt** (controller: hold **D-pad down**), press **E** until Lucia's picture is highlighted, then let go. Not on Enhanced yet: it crashes there for now. |
| **See the minimap** | Get in a car. The map only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Use nitrous** | Hold **Left Alt** in a car that has a bottle (X / Square on a controller). Buy bottles at Los Santos Customs. |
| **Mod your car** | Drive into a Los Santos Customs garage and stop: Leonida Customs opens by itself. |
| **Crouch / lie down** | **Left Ctrl** twice to crouch, **Right Ctrl** to lie down (W / S crawl). |
| **Binoculars** | **B**. |
| **Radio** | Hold GTA's radio button in a car (**Q**, or D-pad left). **O** mutes it. |
| **Skills / gym** | **K** for skills, **N** for push-ups, **Shift+N** for sit-ups. |
| **Greet / antagonize people** | **Y** / **U** while looking at someone close. |
| **Add music** | Put your own `.mp3` files in `scripts\Hud\Music` (wanted music) or `scripts\Hud\Radio\CustomMusic` (radio). No music comes with the mod. |

Something not working? Each install guide ends with a **"Something went wrong?"** section.

---

## What's new

**2.5** brings over features from the [FiveM version](https://github.com/Geckomacabre/GTA-VI-UI-and-HUD-for-FiveM):
- WASTED / BUSTED screens, honor, the crosshair with kill marks, Focus, skills and gym, and NPC memory.
- The FiveM crash cam, binoculars and radio.
- Nitrous, Leonida Customs, and crouch and crawl.
- The four-state wanted stars and wanted box, and directional police lights.

It also fixes the Legacy black screen after dying and has a lighter Lucia on Legacy. FBICat's `GTA6Hud.dll` is
left as it is; the new features are separate script files. Everything is listed in the
[changelog](CHANGELOG.md), including the known issues.

**2.2** (compared to FBICat's 2.1):
- **Minimap works in fullscreen.** The part that draws it inside the game (`ViMinimap.asi`) was missing.
- **Lucia no longer crashes the game while loading.** The RegisterPatch gives the game more room for her
  files.
- **No commercial music** is included.

---

## For modders: rebuilding the packages

`scripts\` and `ViMinimap.asi` in this repo are exactly what the packages install. The DLC packs are
too big for git, so they're only in the releases. Python 3.8+, no extra modules:

```bash
python tools/build_oiv.py legacy --dlcpacks path/to/legacy/dlcpacks
python tools/build_oiv.py enhanced --dlcpacks path/to/gen9/dlcpacks
python tools/build_oiv.py patch
```

The packages go into `build\`. The install script comes from `oiv\<edition>.xml`.

The 2.5 script files are built from `src\` (.NET Framework 4.8, `dotnet build -c Release`). Their art is
rendered from the FiveM version's pages (`tools\render_fivem_art.py`) and packed into the `vihudfonts`
pack's texture dictionaries (`tools\build_txd.py`).

---

## Credits

- **FBICat**: the HUD (first released as "VI HUD"), the custom Lucia ped and wardrobe, the minimap
  additions, and everything else in this mod.
- **moddims** (Discord `@moddims`): the original "GTA VI MiniMap (3D Buildings)" mod the minimap is built
  on, including `ViMinimap.asi`.
- **Grand Figma Kit VI**: the UI design kit behind the HUD art,
  [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
- **[LemonUI](https://github.com/LemonUIbyLemon/LemonUI)** by Lemon (MIT), included as `scripts\LemonUI.SHVDN3.dll`.
- **2.5 ports:** from [GTA-VI-UI-and-HUD-for-FiveM](https://github.com/Geckomacabre/GTA-VI-UI-and-HUD-for-FiveM)
  by Geckomacabre, and the FiveM resources it builds on:
  - **Leonida Customs** after renzu_customs by Renzuzu (GPL-3).
  - **Crouch and crawl** after rpemotes by Mads (GPL-3).
  - **WASTED / BUSTED** from vi_wasted (GPL-3).
  - **Nitrous** from StreetKings.
  - **NPC memory** from um_memory.
  - **The radio** after sej0bec's "VI Radio". Some station logos are from Logopedia; the stations are
    Rockstar Games' trademarks.
- **Anton** font by Vernon Adams (SIL Open Font License, `scripts\ViExtras\Radio\Anton-OFL.txt`).
- Runs on Script Hook V by Alexander Blade and Script Hook V .NET by crosire and contributors.

## License

FBICat's work, shared here with their permission, for personal use. Rockstar's artwork belongs to
Rockstar Games. Grand Figma Kit VI is under CC BY 4.0.

The 2.5 script files have their own licences:
- `ViCustoms.dll`, `ViCrouch.dll` and `ViWasted.dll` are GPL-3. Their source is in `src\`, and the licence
  ships next to them.
- The radio code is under the PolyForm Noncommercial License 1.0.0 (`scripts\ViExtras\Radio\LICENSE.md`).
