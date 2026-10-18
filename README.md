# GTA VI-Inspired HUD

A GTA VI-style HUD, minimap and a playable **Lucia** for **GTA V story mode** on PC. Made by **FBICat**.

> **Disclaimer:** fan-made and inspired by GTA VI. Not affiliated with Rockstar Games or Take-Two. Some
> HUD art is taken from Rockstar's official GTA VI media
> ([rockstargames.com/vi/media](https://www.rockstargames.com/VI/media)) and belongs to Rockstar Games.
> No GTA VI game files are included. The Lucia model is fan-made.

Want it for FiveM instead? → [GTA-VI-UI-and-HUD-for-FiveM](https://github.com/Geckomacabre/GTA-VI-UI-and-HUD-for-FiveM)

---

## Which GTA V do I have?

Open your GTA V folder and look for the game file:

| You see... | You have... | Works? |
| --- | --- | --- |
| `GTA5.exe` | **Legacy** | ✅ Yes, tested |
| `GTA5_Enhanced.exe` | **Enhanced** | ❌ Not yet. The game closes about 20 seconds after loading. Use Legacy for now. |

---

## Install guide (for dummies)

### Step 1: Get the 3 tools the mod needs

Skip any you already have.

1. **Script Hook V**: [download here](http://www.dev-c.com/gtav/scripthookv/). Open the zip's `bin` folder
   and copy everything except `NativeTrainer.asi` into your GTA V folder (the one with `GTA5.exe`).
2. **Script Hook V .NET, version 3.7 or newer**: [download the newest nightly here](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases).
   Copy `ScriptHookVDotNet.asi`, `ScriptHookVDotNet2.dll` and `ScriptHookVDotNet3.dll` into your GTA V
   folder. ⚠️ Version 3.6 is too old: the HUD won't show up.
3. **OpenIV**: [download here](https://openiv.com/). Open it, pick GTA V (Windows), then go to
   **Tools → ASI Manager** and install **ASI Loader** and **OpenIV.asi**.

### Step 2: Download the mod

Go to [**Releases**](../../releases) and download these 2 files:

- `GTA-VI-Inspired-HUD-Legacy.oiv`
- `GTA-VI-Inspired-HUD-RegisterPatch.oiv`

### Step 3: Install the mod

1. In OpenIV, go to **Tools → Package Installer**.
2. Pick `GTA-VI-Inspired-HUD-Legacy.oiv` and click **Install**.
3. When it asks where, choose **mods folder**.
4. Do the same with `GTA-VI-Inspired-HUD-RegisterPatch.oiv`. Install it **only once**.

### Step 4: Check one file

Make sure `ViMinimap.asi` is in your GTA V folder, next to `GTA5.exe`. If it isn't, find it in your
`mods` folder and move it there. Without it, the minimap won't show in fullscreen.

### Step 5: Play

Start GTA V and load **Story Mode**. The new HUD shows up on its own. 🎉

---

## How to use it

| I want to... | Do this |
| --- | --- |
| **Play as Lucia** | Hold **Left Alt** (controller: hold **D-pad down**), press **E** until Lucia's picture is highlighted, then let go. |
| **See the minimap** | Get in a car. The map only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Add music** | Put your own `.mp3` files in `scripts\Hud\Music` (wanted music) or `scripts\Hud\Radio\CustomMusic` (radio). No music comes with the mod. |

---

## Something's wrong?

**The HUD doesn't show up.**
Your Script Hook V .NET is too old. Install **3.7 or newer** (Step 1).

**The minimap doesn't show in a car.**
`ViMinimap.asi` isn't next to `GTA5.exe` (Step 4).

**The game crashes while loading.**
You skipped the RegisterPatch, or another big mod is using up the game's limits. Install the
RegisterPatch, then try turning off other big mods: ped packs, map packs, arena mods like MzbArena.

**The game freezes when I load in as Lucia, or when I walk out of Franklin's house.**
Known problem in 2.2. Before you quit the game, switch back to Franklin, Michael or Trevor. A fix is on
the way.

**Something else?**
Open an issue and attach these files from your GTA V folder:
`ScriptHookVDotNet.log`, `ViMinimap.log`, `scripts\GTA6Hud.perf.log` and `scripts\GTA6Hud.error.log`.

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
