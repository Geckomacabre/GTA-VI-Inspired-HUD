# How to install: GTA V **Legacy**

This guide is for the **original PC version** of GTA V. If your game folder has a file called
**`GTA5_Enhanced.exe`**, you have Enhanced. Use the **[Enhanced guide](INSTALL-ENHANCED.md)** instead.

Never modded GTA before? No problem. Follow the steps in order and don't skip any. It takes about 20 minutes.

> [!IMPORTANT]
> ### 💬 Need help? Join our support Discord
> This version is a **fork of FBICat's VI HUD** with extra features, so it has its own support:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, or a [GitHub issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat's Discord is there for his original mod, so please bring questions about this version to ours. Thank you!

> ⚠️ **Mods are for Story Mode only.** Never go into GTA Online with mods installed: you can get banned.
> (Script Hook V, one of the tools below, closes the game if you try.)

---

## Before you start: two quick Windows settings

**1. Show file extensions.** Some files in this guide end in `.asi` or `.dll`, and Windows hides those
endings by default. Open any folder, click **View** at the top, then **Show** → **File name extensions**.

**2. Know how to unzip.** Most downloads are `.zip` files (a folder squeezed into one file). To open
one, right-click it, choose **Extract All…**, then click **Extract**. A normal folder appears next to it.

---

## Step 1: Find your GTA V folder

This is the folder with **`GTA5.exe`** inside. We'll call it your **game folder**. How to open it:

- **Rockstar Games Launcher:** open **Settings**, click **Grand Theft Auto V** on the left, then
  **Open Folder** (or **View Installation Folder**).
- **Steam:** in your **Library**, right-click **Grand Theft Auto V**, then **Manage** → **Browse local files**.
- **Epic Games:** in your **Library**, click the **⋯** under GTA V, then **Manage**, then the
  **folder icon** next to "Installation".

✅ Keep this folder open. You'll copy files into it a few times.

---

## Step 2: Install Script Hook V (lets mods run)

1. Go to the [Script Hook V page](http://www.dev-c.com/gtav/scripthookv/) and click **Download**.
2. Unzip it and open the **`bin`** folder inside.
3. Copy these files into your **game folder**:
   - `ScriptHookV.dll`
   - `dinput8.dll` (loads mods ending in `.asi`)
   - `args.txt` (turns off BattlEye, Rockstar's anti-cheat, which blocks mods)
   - `xinput1_4.dll`, if it's there
4. **Don't** copy `NativeTrainer.asi` (a cheat menu you don't need).

✅ Your game folder now has `ScriptHookV.dll` and `dinput8.dll` next to `GTA5.exe`.

---

## Step 3: Install Script Hook V .NET (lets this HUD run)

The HUD needs **version 3.7 or newer**. Older versions won't work.

1. Go to the [Script Hook V .NET nightly page](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases).
2. Under the **top** release, open **Assets** and download the **`.zip`** file.
3. Unzip it and copy these into your **game folder**:
   - `ScriptHookVDotNet.asi`
   - `ScriptHookVDotNet2.dll`
   - `ScriptHookVDotNet3.dll`
   - `ScriptHookVDotNet.ini`

✅ Those 4 files are now next to `GTA5.exe`.

---

## Step 4: Install OpenIV (installs the mod safely)

OpenIV puts mods in a separate **`mods` folder**, so your original game files are never changed.

1. Download OpenIV from [openiv.com](https://openiv.com/) and install it.
2. Open OpenIV. Choose **GTA V** → **Windows**, and point it to your **game folder** if it asks.
3. At the top, click **Tools** → **ASI Manager**.
4. Click **Install** next to **ASI Loader** and next to **OpenIV.asi**. Close the window.

✅ OpenIV is ready.

---

## Step 5: Download the mod

1. Go to the [**Releases page**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. Under **Assets**, download these **2 files**:
   - **`GTA-VI-Inspired-HUD-2.5.5-Legacy.oiv`** (the mod, about 100 MB)
   - **`GTA-VI-Inspired-HUD-RegisterPatch-1.7.oiv`** (a small patch that switches the mod on)
3. **Want to play as Lucia?** She isn't included: her model is someone else's paid work. Get her from her creator, **[Lucia Caminos on Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)**, or get **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** on gta5-mods.com, or use another Lucia model. **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** is there too. The HUD works without her.

❌ Don't download `GTA-VI-Inspired-HUD-2.5.5-Enhanced.oiv`. That one is for the other version of the game.

💡 The number in the file name is the version (2.5.5 here), so you can always tell which one you installed. Mention it when you ask for help.

---

## Step 6: Install the mod with OpenIV

1. In OpenIV, click **Tools** → **Package Installer**.
2. Pick **`GTA-VI-Inspired-HUD-2.5.5-Legacy.oiv`**, then click **Install**.
3. When it asks where to install, choose **mods folder**. If it offers to create the folder, say yes.
4. Wait until it says it's finished, then click **Close**.
5. Do the same for **`GTA-VI-Inspired-HUD-RegisterPatch-1.7.oiv`**.

✅ Installing the RegisterPatch again, or over an older one, is fine: it never lists a pack twice. If it then
says it **failed**, that only means it's already in.

---

## Step 7: Check one file

Look in your **game folder** for **`ViMinimap.asi`**, right next to `GTA5.exe`.

- **It's there?** Great, go to Step 8.
- **It's not there?** Open the **`mods`** folder in your game folder, find `ViMinimap.asi`, and move it
  next to `GTA5.exe`. Without it, the minimap won't show.

---

## Step 8: Play! 🎉

Start GTA V and choose **Story Mode**. The new HUD appears on its own.

| To... | Do this |
| --- | --- |
| **Play as Lucia** | Hold **Left Alt** (controller: hold **D-pad down**), tap **E** until Lucia's picture is highlighted, then let go. Needs a Lucia model (not included, see Step 5). |
| **See the minimap** | Get in a car. It only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Play your own music** | Put `.mp3` files in `scripts\Hud\Radio\CustomMusic` in your game folder. In a car, hold the radio button (**Q**) and press **Down** (controller: right stick down) for **On Demand**. |

---

## Something went wrong?

**Still stuck?** Ask in **[our support Discord](https://discord.gg/C7gE5SHEzH)**, the one for this version, or
[open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| What happens | What to do |
| --- | --- |
| Installing the **RegisterPatch a second time** says it **failed** ("Unable to find XML Node") | It's already installed: `dlclist.xml` lists the HUD's packs, so there was nothing to add. Your game is set up, carry on. |
| A Script Hook V message says **"unknown game version"** | GTA V just updated and Script Hook V hasn't caught up yet. Wait for a new Script Hook V, then redo Step 2. |
| The game starts but **the new HUD doesn't show** | Your Script Hook V .NET is too old. Redo Step 3 with the newest version. |
| You **updated** the mod, but the game still acts like the **old version** | Script Hook V .NET can keep running its saved copy of the old scripts. Close the game, press **Win + R**, type `%LOCALAPPDATA%\assembly`, press Enter and delete the **`dl3`** folder (it's only a cache). Still the old version? Copy the scripts in yourself: see "Updating by hand" below. |
| **No minimap** in a car | `ViMinimap.asi` isn't next to `GTA5.exe`. Redo Step 7. |
| The game **crashes while loading** | Make sure you installed the RegisterPatch (Step 6). If you have other big mods (extra characters, maps), try turning them off. |
| A message says **Lucia is not registered** | Lucia isn't included with the HUD any more (see Step 5). Don't have her? Set `Enabled = False` under `[Lucia]` in `scripts\GTA6Hud.ini` and the message goes away. Have a Lucia model? Her pack is missing from `dlclist.xml`: add its line. If the message mentions a `VIHudSetup.exe`, ignore that part: this mod doesn't use it. |
| **Switching characters crashes**, and your game isn't in English | Players report this only happens when the game is set to another language. Until it's fixed, set GTA V's language to **English** in your game launcher's settings for GTA V. |
| **Something else** | Ask in the **[support Discord](https://discord.gg/C7gE5SHEzH)**, or [open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) and attach these files from your game folder: `ScriptHookVDotNet.log`, `ViMinimap.log`, `scripts\GTA6Hud.perf.log` and `scripts\GTA6Hud.error.log`. |

---

## Updating by hand (game in Program Files)

If your game is in **Program Files** (where the Rockstar Games Launcher and Steam put it), Windows guards that folder,
and an update through OpenIV doesn't always replace the old files. You can copy the new scripts in yourself
instead. An `.oiv` is just a zip:

1. Close the game.
2. Copy **`GTA-VI-Inspired-HUD-2.5.5-Legacy.oiv`** to your Desktop and rename the copy to **`.zip`**.
   Can't see the `.oiv` part of the name? In File Explorer, turn on **View** → **Show** → **File name extensions**.
3. Open the zip → **`content`** → **`scripts`**, select everything in there and copy it.
4. Paste it into the **`scripts`** folder in your game folder, for example
   `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts`. Pick **Replace the files in the destination**, then
   **Continue** when Windows asks for admin permission.
5. Clear Script Hook V .NET's cache, so it loads the new files: press **Win + R**, type `%LOCALAPPDATA%\assembly`
   and press Enter, then delete the **`dl3`** folder. It's only a cache, and it's made again the next time you start
   the game.
6. Coming from 2.5.4 or older? 2.5.5 adds a pack: also copy the zip's **`content`** → **`dlcpacks`** → **`vihud`**
   folder into **`mods\update\x64\dlcpacks`** in your game folder, and install
   **`GTA-VI-Inspired-HUD-RegisterPatch-1.7.oiv`** as in Step 6.
7. Start the game. `scripts\ViExtras.log` now begins with a `==== ViExtras …` line that shows the version you
   downloaded.

---

## How to remove the mod

1. In your **game folder**, delete `ViMinimap.asi`.
2. Open the **`scripts`** folder and delete `GTA6Hud.dll`, `GTA6Hud.ini`, `ViMinimap.dll`, `LemonUI.SHVDN3.dll`,
   the `MiniMap.*` files and the `Hud` folder.
3. Delete the **`mods`** folder. ⚠️ This also removes any other mods you installed with OpenIV.

Your game is back to normal. Script Hook V and Script Hook V .NET can stay; they don't change anything
on their own.
