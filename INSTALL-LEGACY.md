# How to install: GTA V **Legacy**

This guide is for the **original PC version** of GTA V. If your game folder has a file called
**`GTA5_Enhanced.exe`**, you have Enhanced. Use the **[Enhanced guide](INSTALL-ENHANCED.md)** instead.

Never modded GTA before? No problem. Follow the steps in order and don't skip any. It takes about 20 minutes.

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
   - **`GTA-VI-Inspired-HUD-Legacy.oiv`** (the mod, about 1.8 GB)
   - **`GTA-VI-Inspired-HUD-RegisterPatch.oiv`** (a small patch that switches the mod on)

❌ Don't download `GTA-VI-Inspired-HUD-Enhanced.oiv`. That one is for the other version of the game.

---

## Step 6: Install the mod with OpenIV

1. In OpenIV, click **Tools** → **Package Installer**.
2. Pick **`GTA-VI-Inspired-HUD-Legacy.oiv`**, then click **Install**.
3. When it asks where to install, choose **mods folder**. If it offers to create the folder, say yes.
4. Wait until it says it's finished, then click **Close**.
5. Do the same for **`GTA-VI-Inspired-HUD-RegisterPatch.oiv`**.

⚠️ Install the RegisterPatch **only once**. Installing it twice can crash the game.

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
| **Play as Lucia** | Hold **Left Alt** (controller: hold **D-pad down**), tap **E** until Lucia's picture is highlighted, then let go. |
| **See the minimap** | Get in a car. It only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Add your own music** | Put `.mp3` files in `scripts\Hud\Music` or `scripts\Hud\Radio\CustomMusic` in your game folder. |

> 💡 **Playing as Lucia?** Switch back to Franklin, Michael or Trevor **before you quit the game**.
> Loading in as Lucia can freeze the game at Franklin's front door. A fix is coming.

---

## Something went wrong?

| What happens | What to do |
| --- | --- |
| A Script Hook V message says **"unknown game version"** | GTA V just updated and Script Hook V hasn't caught up yet. Wait for a new Script Hook V, then redo Step 2. |
| The game starts but **the new HUD doesn't show** | Your Script Hook V .NET is too old. Redo Step 3 with the newest version. |
| **No minimap** in a car | `ViMinimap.asi` isn't next to `GTA5.exe`. Redo Step 7. |
| The game **crashes while loading** | Make sure you installed the RegisterPatch (Step 6). If you have other big mods (extra characters, maps), try turning them off. |
| **Switching characters crashes**, and your game isn't in English | Players report this only happens when the game is set to another language. Until it's fixed, set GTA V's language to **English** in your game launcher's settings for GTA V. |
| **Something else** | [Open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) and attach these files from your game folder: `ScriptHookVDotNet.log`, `ViMinimap.log`, `scripts\GTA6Hud.perf.log` and `scripts\GTA6Hud.error.log`. |

---

## How to remove the mod

1. In your **game folder**, delete `ViMinimap.asi`.
2. Open the **`scripts`** folder and delete `GTA6Hud.dll`, `GTA6Hud.ini`, `ViMinimap.dll`, `LemonUI.SHVDN3.dll`,
   the `MiniMap.*` files and the `Hud` folder.
3. Delete the **`mods`** folder. ⚠️ This also removes any other mods you installed with OpenIV.

Your game is back to normal. Script Hook V and Script Hook V .NET can stay; they don't change anything
on their own.
