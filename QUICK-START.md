# ⚡ Quick start: the dead-simple version

*The GTA VI-Inspired HUD is made by **FBICat**, with features from the FiveM version. Credits are in the [README](README.md#-credits).*

Never modded GTA V? Do these 6 steps in order. Each one links to the full guide if you want more detail:
**[Legacy guide](INSTALL-LEGACY.md)** · **[Enhanced guide](INSTALL-ENHANCED.md)**.

> [!IMPORTANT]
> ### 💬 Need help? Join our support Discord
> This version is a **fork of FBICat's VI HUD** with extra features, so it has its own support:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, or a [GitHub issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat's Discord is there for his original mod, so please bring questions about this version to ours. Thank you!

> ⚠️ Story Mode only. Never take mods into GTA Online.

---

## 1. Which GTA V do you have?

Open your GTA V folder (the launcher's "open folder" / "browse local files" button).

- You see **`GTA5.exe`** → you have **Legacy**.
- You see **`GTA5_Enhanced.exe`** → you have **Enhanced**.

Everything below says which one each step is for. Don't mix them up: the wrong files crash the game.

## 2. Copy 3 tools into that folder

| Tool | Legacy | Enhanced |
| :--- | :--- | :--- |
| **[Script Hook V](http://www.dev-c.com/gtav/scripthookv/)** | from its `bin` folder: `ScriptHookV.dll`, `dinput8.dll`, `args.txt` | the same |
| **Script Hook V .NET** | the **[nightly](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases)** zip: its 4 `ScriptHookVDotNet` files | **[SHVDNE](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest)** (the Enhanced one!): its 5 files |
| **Mod loader** | **[OpenIV](https://openiv.com/)**: install it, then **Tools → ASI Manager** → install **ASI Loader** and **OpenIV.asi** | **[OpenRPF](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced)**: copy `OpenRPF.asi`, and make a folder called `mods` |

Using **NaturalVision Enhanced**, **Lively World Expansion** or **AuraV-Lite** on Enhanced? Follow their "Optional" sections in the **[Enhanced guide](INSTALL-ENHANCED.md)** (with NVE, skip OpenRPF; with LWE, don't copy its `dlclist.xml`).

## 3. Download the mod

From **[Releases](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest)**, grab:

- **`GTA-VI-Inspired-HUD-2.5.5-Legacy.oiv`** *or* **`GTA-VI-Inspired-HUD-2.5.5-Enhanced.oiv`** (yours only)
- **`GTA-VI-Inspired-HUD-RegisterPatch-1.7.oiv`**
- *(optional)* **`GTA-VI-Inspired-HUD-Lucia-Legacy.oiv`** or **`-Lucia-Enhanced.oiv`** to play as Lucia

## 4. Install the files, in this order: mod, RegisterPatch, Lucia

- **Legacy:** OpenIV → **Tools → Package Installer** → pick the file → install to the **mods folder**.
- **Enhanced:** the **[OIV Package Installer](https://www.gta5-mods.com/tools/oiv-package-installer)**
  (`CodeWalker.OIVInstaller.exe`) → drag the file in → pick your GTA V folder → **Install**.
- Installed the RegisterPatch before? If it now says it **failed**, that's fine: it's already in.

## 5. One last check

- **Legacy:** make sure **`ViMinimap.asi`** sits next to `GTA5.exe`. If it's in `mods`, move it out.
- **Enhanced:** in the game, **Settings → Graphics → Screen Type → Windowed Borderless** (or the minimap won't show).

## 6. Play 🎉

Start **Story Mode**. The HUD shows up on its own.

- **Hold V:** HUD settings
- **Hold Q in a car:** radio. Press **Down** (or right stick down) for **On Demand**: your own music. Put
  `.mp3` files in `scripts\Hud\Radio\CustomMusic` first.
- **Hold Left Alt:** characters (Lucia too, if you installed her)

All the controls are in the **[README](README.md#-controls)**.
