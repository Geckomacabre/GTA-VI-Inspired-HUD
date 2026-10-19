# GTA VI-Inspired HUD

A GTA VI-style HUD, minimap and a playable **Lucia** for **GTA V story mode** on PC. Made by **FBICat**.

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
| `GTA5_Enhanced.exe` | **Enhanced** (the 2025 PC version) | 👉 **[Enhanced install guide](INSTALL-ENHANCED.md)** |

Both guides are written for people who've never modded GTA before. Follow only the guide for your
version: the two use different tools, and mixing them up crashes the game.

---

## How to use it

| I want to... | Do this |
| --- | --- |
| **Play as Lucia** (Legacy) | Hold **Left Alt** (controller: hold **D-pad down**), press **E** until Lucia's picture is highlighted, then let go. Not on Enhanced yet: it crashes there for now. |
| **See the minimap** | Get in a car. The map only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Add music** | Put your own `.mp3` files in `scripts\Hud\Music` (wanted music) or `scripts\Hud\Radio\CustomMusic` (radio). No music comes with the mod. |

Something not working? Each install guide ends with a **"Something went wrong?"** section.

---

## What's new compared to FBICat's 2.1

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

---

## Credits

- **FBICat**: the HUD (first released as "VI HUD"), the custom Lucia ped and wardrobe, the minimap
  additions, and everything else in this mod.
- **moddims** (Discord `@moddims`): the original "GTA VI MiniMap (3D Buildings)" mod the minimap is built
  on, including `ViMinimap.asi`.
- **Grand Figma Kit VI**: the UI design kit behind the HUD art,
  [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
- **[LemonUI](https://github.com/LemonUIbyLemon/LemonUI)** by Lemon (MIT), included as `scripts\LemonUI.SHVDN3.dll`.
- Runs on Script Hook V by Alexander Blade and Script Hook V .NET by crosire and contributors.

## License

FBICat's work, shared here with their permission, for personal use. Rockstar's artwork belongs to
Rockstar Games. Grand Figma Kit VI is under CC BY 4.0.
