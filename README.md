# GTA VI-Inspired HUD

**A GTA VI-inspired HUD with a custom Lucia ped**, for **GTA V single player**. It includes a 3D
minimap and a fan-made, playable Lucia, and works on both the Legacy and the Enhanced PC versions.
Made by **FBICat**.

To play as Lucia, hold the character-switch button (**Left Alt**, or **D-pad down** on a controller),
press **E** until the fourth portrait is highlighted, then let go.

> **Disclaimer.** This is a fan-made mod inspired by GTA VI. It is not affiliated with or endorsed by
> Rockstar Games or Take-Two Interactive. Some of the HUD artwork is taken from Rockstar's official
> GTA VI media ([rockstargames.com/vi/media](https://www.rockstargames.com/VI/media)) and belongs to
> Rockstar Games. No GTA VI game files are included. The Lucia model is fan-made. Grand Theft Auto and
> related names and marks belong to Take-Two Interactive.

Looking for FiveM? See the FiveM version:
[Geckomacabre/GTA-VI-UI-and-HUD-for-FiveM](https://github.com/Geckomacabre/GTA-VI-UI-and-HUD-for-FiveM).

## Download

Get the packages from the [Releases](../../releases) page.

| File | For |
| --- | --- |
| `GTA-VI-Inspired-HUD-Legacy.oiv` | GTA V **Legacy**, the original PC version (`GTA5.exe`) |
| `GTA-VI-Inspired-HUD-Enhanced.oiv` | GTA V **Enhanced**, the 2025 PC version (`GTA5_Enhanced.exe`). Not tested in game yet. |
| `GTA-VI-Inspired-HUD-RegisterPatch.oiv` | Registers the HUD's DLC packs in `dlclist.xml` and raises the `gameconfig.xml` pool sizes. Works for both versions. |

## Requirements

Not included:

- **Legacy:** [Script Hook V](http://www.dev-c.com/gtav/scripthookv/),
  [Script Hook V .NET 3](https://github.com/scripthookvdotnet/scripthookvdotnet), and
  [OpenIV](https://openiv.com/) with the mods folder set up (OpenIV.asi).
- **Enhanced:** Script Hook V for Enhanced, a Script Hook V .NET 3 build that supports Enhanced, and a
  mods-folder loader for Enhanced (OpenIV.asi only works with Legacy).

## Install

1. In OpenIV, open **Tools → Package Installer** and install `GTA-VI-Inspired-HUD-Legacy.oiv` or
   `GTA-VI-Inspired-HUD-Enhanced.oiv` to the **mods folder**.
2. Install `GTA-VI-Inspired-HUD-RegisterPatch.oiv` to the **mods folder** the same way.
3. Check that `ViMinimap.asi` ended up next to `GTA5.exe` (or `GTA5_Enhanced.exe`). The minimap
   needs it to draw in fullscreen.

If the packs aren't registered, the HUD tells you in game. The in-game tip calls the patch by its
original name, `VIHud-RegisterPatch.oiv`; that's `GTA-VI-Inspired-HUD-RegisterPatch.oiv` here.

## What it installs

- **HUD:** `scripts\GTA6Hud.dll`, `scripts\GTA6Hud.ini` and `scripts\Hud\` (art, sounds, data).
  Your progress is kept in `scripts\Hud\Save`, which the packages never touch.
- **3D minimap:** `scripts\ViMinimap.dll`, the `scripts\MiniMap.*` map data, and `ViMinimap.asi`,
  which draws the map inside the game frame.
- **DLC packs:** `vihudfonts` (pause menu, HUD art, fonts, vehicle maker emblems), `vihudanims`
  (carry and loot animations), `luciaped` (Lucia as a fourth character) and `luciaolf` (her wardrobe
  and hairstyles).
- **No music.** Put your own `.mp3` or `.wav` files in `scripts\Hud\Music` (wanted-level music) and
  `scripts\Hud\Radio\CustomMusic` (the custom radio station).

## Changes from FBICat's 2.1 packages

- **The minimap now shows in fullscreen.** The 2.1 packages left out the plugin that draws the
  minimap inside the game (`ViMinimap.asi`). Without it, the map fell back to a desktop overlay window
  that can't appear over a fullscreen game. Because the minimap only shows in a vehicle, players saw it
  as "the minimap doesn't show when I'm in a car".
- **Lucia no longer crashes the game.** While loading her ped files, the game ran out of room in its
  `MetaDataStore` pool (3,200 slots by default). The register patch (1.3) raises it to 4,000, along
  with the texture and model pools her wardrobe needs, and registers Lucia again. Her pack was
  cleaned up too:
  - The unused expression dictionary and the oversized second copy of the model (`lucia_nophys`) are gone.
  - She now uses the freemode expression set.
  - On Enhanced, her body parts are back. The gen9 conversion had dropped them.
- **Commercial music removed.** The radio and wanted-level songs aren't included.

## Troubleshooting

Send these files with any bug report:

- `ViMinimap.log` in the game folder for the minimap. It should say `first frame drawn inside the game`
  once you're in a car. Also send `scripts\MiniMap.log` if it exists.
- `scripts\GTA6Hud.perf.log` and `scripts\GTA6Hud.error.log` for the HUD.
- `ScriptHookVDotNet.log` in the game folder.

Big add-on mods (ped packs, large map or arena packs) use up the same game limits the HUD's packs need.
If the game crashes, especially when switching to Lucia, try disabling those first. Known conflict:
MzbArena. With it installed, switching to Lucia crashes, and it also crashes on its own while driving.

## Building the packages

The `scripts\` folder and `ViMinimap.asi` in this repo are exactly what the packages install. The DLC
packs are too big for git, so they're only in the releases. To rebuild a package (Python 3.8+, no
extra modules):

```bash
python tools/build_oiv.py legacy --dlcpacks path/to/legacy/dlcpacks
python tools/build_oiv.py enhanced --dlcpacks path/to/gen9/dlcpacks
python tools/build_oiv.py patch
```

The packages are written to `build\`. The install script comes from `oiv\<edition>.xml`, and the
file list is generated from what's in the repo.

## Credits

- **FBICat:** the HUD (first released as "VI HUD"), the custom Lucia ped and wardrobe packs, the minimap additions (day/night
  palette, wanted tint, search circle, clock, map survey data), and everything else in this mod.
- **moddims** (Discord `@moddims`): the original "GTA VI MiniMap (3D Buildings)" mod that `ViMinimap`
  is built on, including the in-game drawing plugin shipped here as `ViMinimap.asi`.
- **Grand Figma Kit VI:** UI design kit used for the HUD art, licensed
  [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/).
- **[LemonUI](https://github.com/LemonUIbyLemon/LemonUI)** by Lemon (MIT), included as
  `scripts\LemonUI.SHVDN3.dll`.
- Built on Script Hook V by Alexander Blade and Script Hook V .NET by crosire and contributors.

## License

This mod is FBICat's work, shared here with their permission, for personal use. Rockstar's artwork
remains the property of Rockstar Games. Grand Figma Kit VI is under CC BY 4.0.
