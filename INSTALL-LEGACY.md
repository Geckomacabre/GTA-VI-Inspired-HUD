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

The HUD needs **3.7.0 nightly 193 or newer** (the top release on the nightly page always is). With an older one,
the HUD's scripts don't start, or one stops with an error (see "Something went wrong?").

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
2. Under **Assets**, download **`GTA-VI-Inspired-HUD-2.6.2-Legacy.oiv`** (the mod, about 100 MB). It also switches
   the mod on in the game, so there's no separate patch to get.
3. **Want to play as Lucia?** She isn't included: her model is someone else's paid work. Get her from her creator,
   **[Lucia Caminos on Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** or **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** on gta5-mods.com, or use another
   Lucia model. For Jason, get **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** from the same creator, or **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** on mod-hub.net (see "Optional: Jason" below to put him in the game). The HUD
   works without her.

❌ Don't download `GTA-VI-Inspired-HUD-2.6.2-Enhanced.oiv`. That one is for the other version of the game.

💡 The number in the file name is the version (2.6.2 here), so you can always tell which one you installed. Mention it when you ask for help.

---

## Step 6: Install the mod with OpenIV

⚠️ **Game in Program Files?** If your game folder's path starts with `C:\Program Files` (the Rockstar Games
Launcher, Epic Games and Steam put it there), right-click **OpenIV** → **Run as administrator** and click
**Yes**. Windows lets only administrators change files there, and without it the install can leave the old files
in place.

1. In OpenIV, click **Tools** → **Package Installer**.
2. Pick **`GTA-VI-Inspired-HUD-2.6.2-Legacy.oiv`**, then click **Install**.
3. When it asks where to install, choose **mods folder**. If it offers to create the folder, say yes.
4. Wait until it says it's finished, then click **Close**.

✅ Installing the mod again, or over an older version, is fine: it never lists a pack twice.

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
| **Switch character** | Hold **Left Alt** (controller: hold **D-pad down**), pick with **Q / E** or **Left / Right** (controller: **LB / RB**), then let go. Jason and Lucia need their models (not included, see Step 5). |
| **See the minimap** | Get in a car. It only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Play your own music** | Put `.mp3` files in `scripts\Hud\Radio\CustomMusic` in your game folder. In a car, hold the radio button (**Q**) and press **Down** (controller: right stick down) for **On Demand**. A folder in `CustomMusic` is a station of its own: **Left / Right** (right stick) switch stations, the **mouse wheel** (D-pad Right / Up) changes the song. |

---

## Optional: Jason

The HUD puts **Jason** on the character wheel, next to Lucia, once his ped is in your game. He isn't included: get
**[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** from Lucia's creator, or **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** on mod-hub.net. That
download is only his model files (`jdfinal.ydd`, `jdfinal.yft`, `jdfinal.ymt` and `jdfinal.ytd`), so the **Jason kit**
is the pack to put them in. There's no model in the kit, just the pack around him.

1. Download **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** and unzip it.
2. Copy its **`jasonped`** folder into **`mods\update\x64\dlcpacks`** in your game folder.
3. In OpenIV, switch on **Edit mode** and open **`mods\update\x64\dlcpacks\jasonped\dlc.rpf`** → `x64` →
   `models` → `cdimages` → **`jasonped.rpf`**. Drag in the 4 files from the download's **`No Cap Version`** folder
   (or **`Cap Version`**, for Jason in his cap).
4. Open **`mods\update\update.rpf`** → `common` → `data` → **`dlclist.xml`** (right-click → **Edit**), add this line
   just above the closing `</Paths>` at the bottom of the file, and save:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
5. Start the game and hold **Left Alt**: Jason's card is next to Lucia's.

**A different Jason model?** The HUD looks for the ped named under `[Jason]` in `scripts\GTA6Hud.ini`
(`Model = jdfinal`), so another Jason works too:
- **Its files have another name** (say `jason.ydd`)? Rename them all to `jdfinal`, keeping their endings, and nothing
  else changes. Or keep the names and change `jdfinal` to that name in two places: in the kit's `setup\peds.meta`
  (`<Name>jdfinal</Name>`, in Notepad), which you then drag into the pack's `common\data` in OpenIV to replace the
  one there, and in `Model =` under `[Jason]` in `scripts\GTA6Hud.ini`.
- **It's made of many files** (`jason^head_000_r.ydd`, `jason^uppr_000_u.ydd` and so on)? Put them all in
  `jasonped.rpf`, and use the name before the `^` (here `jason`) in those two places.
- **It comes as a pack of its own** (it has its own `dlc.rpf`)? You don't need the kit: install that pack the way its
  readme says, then set `Model =` under `[Jason]` in `scripts\GTA6Hud.ini` to its ped's name (the `<Name>` in its
  `peds.meta`, or its `.ydd` file's name).

**Jason not on the wheel?** `scripts\ViSwitch.log` says **"Jason's model is in the game"** once the HUD has found him,
or **"no Jason model in the game"** when it hasn't: the pack isn't in `dlclist.xml`, or the names don't match.

---

## Optional: Forests of San Andreas and GTA V Remastered

**Forests of San Andreas** and **GTA V Remastered** by **Larcius** (built on JRod's and le__AK's maps) fill the map
with tens of thousands of extra trees, bushes and props, and they work with this HUD. They come as OIVs too, so they
go in the same way:

1. Install the HUD first (Steps 1 to 7).
2. In each mod's zip, open the **`GTA V Legacy`** folder. In OpenIV, use **Tools** → **Package Installer** on the
   ones you want, and pick **mods folder** each time, like in Step 6:
   - **`Forests of San Andreas (North) Ultimate v….oiv`**
   - **`Forests of San Andreas (South) Ultimate v….oiv`**
   - **`GTA V Remastered Ultimate v….oiv`**

   Each one adds its own pack to the game's pack list, next to the HUD's, so there's nothing to edit.
3. Using **Liberty City V Remix** as well? Also install the mods' **`Compatibility Liberty City V Remix`** OIVs.
4. **Leave `gameconfig.xml` as it is.** The maps don't need a different one, and installing another
   `gameconfig.xml` would replace the one the HUD set up, which the HUD needs. See "Something went wrong?" if
   the game crashes while loading with the maps in.

To take them off again, install the mods' **`Uninstaller`** OIVs the same way, to the mods folder.

---

## Optional: Rebalanced Dispatch Enhanced

**Rebalanced Dispatch Enhanced** (RDE) by **the RDE Team** replaces GTA's police response: new agencies, roadblocks,
helicopters, and up to **10 stars**. It works with this HUD:

1. Install the HUD first (Steps 1 to 7). Step 3's Script Hook V .NET is the nightly RDE asks for.
2. In OpenIV, use **Tools** → **Package Installer** on **`RDE 4.1.5.oiv`** from RDE's download, and pick
   **mods folder**, like in Step 6.
3. Skip RDE's **`Compatibility - Community HUD`** files: they're for another HUD mod.

RDE brings its own `gameconfig.xml` with much bigger limits than the HUD needs, and that's fine. Installing or
updating the HUD afterwards only raises a limit that's too low, so it never undoes RDE's.

While RDE runs, its police take over: GTA6Hud's police brain, GTA6Hud's own six stars (and the military they call
in) and the HUD's stun-gun arrests stay out. The HUD's stars show RDE's level up to **six**: levels 6 to 10 all light
the sixth star. `ViExtras.ini` `[Compat] Rde = False` leaves the HUD as it is.

To take RDE off again, install its **`Uninstaller.oiv`** the same way, then install the HUD again (Step 6), so its
limits and packs are back.

---

## Optional: Los Santos RED

**Los Santos RED** (LSR) by **Greskrendtregk** turns the game into a crime sandbox with its own police, crimes,
wanted level and much more. It runs in **RAGE Plugin Hook**, not Script Hook V .NET, and works next to this HUD.
Use it on its own: not together with LSPDFR or Rebalanced Dispatch Enhanced, which run the police too.

1. Install the HUD first (Steps 1 to 7).
2. Install **[RAGE Plugin Hook](https://ragepluginhook.net/)** and **[RAGENativeUI](https://github.com/alexguirre/RAGENativeUI/releases)**
   (LSR's menus need it), then LSR itself, as LSR's own page says.
3. Start the game with **`RAGEPluginHook.exe`**. LSR loads once Story Mode has loaded.

While LSR runs, it has the police, the crimes and the wanted level, so the HUD adds none of its own: no witnesses,
police brain, police memory, heat, six stars or stun-gun arrests. `ViExtras.ini` `[Compat] Lsr = False` leaves the
HUD as it is.

**Stars.** LSR keeps GTA's own wanted level at zero and draws stars of its own, so the HUD's stars and wanted box
don't come up with LSR. For GTA VI stars in LSR's place, copy the 4 pictures from the HUD's
[`extras/lsr-stars/wantedlevel`](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/tree/main/extras/lsr-stars/wantedlevel)
into **`Plugins\LosSantosRED\images\wantedlevel`** and replace LSR's.

**LSR's settings.** LSR writes `Plugins\LosSantosRED\Settings.xml` the first time it runs. Close the game, open that
file in Notepad and change these values:

| Setting | Set to | Why |
| --- | --- | --- |
| `AlwaysShowRadar` | `false` | The HUD's 3D minimap replaces GTA's radar. LSR switching the radar on puts it back under the map. |
| `NeverShowRadar` | `true` | The same. |
| `TopDisplayShowWeapon` | `false` | The HUD shows your weapon at the top right already. |
| `ShowStreetDisplay` | `false` | The HUD shows the street under the minimap already. |

**Keys.** LSR uses some of the HUD's keys. Change one side of each pair you use:

| Key | HUD | LSR |
| --- | --- | --- |
| **Left Ctrl** | Crouch (`ViCrouch.ini` `[Crouch] Key`) | Crouch (`CrouchKey` in LSR's settings) |
| **K** | Skills panel (`ViExtras.ini` `[Skills] Key`) | No, in conversations |
| **O** | Radio mute (`ViExtras.ini` `[Radio] MuteKey`) | Start an interaction |
| **Shift + N** | Sit-ups (`ViExtras.ini` `[Gym] SitUpsKey`) | Group mode |

---

## Something went wrong?

**Still stuck?** Ask in **[our support Discord](https://discord.gg/C7gE5SHEzH)**, the one for this version, or
[open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| What happens | What to do |
| --- | --- |
| A Script Hook V message says **"unknown game version"** | GTA V just updated and Script Hook V hasn't caught up yet. Wait for a new Script Hook V, then redo Step 2. |
| The game starts but **the new HUD doesn't show** | Your Script Hook V .NET is too old. Redo Step 3 with the newest version. |
| A message says **Unhandled exception in script** (for example **"MiniMap"**) with a **FileNotFoundException** | Your Script Hook V .NET is older than the HUD needs (3.7.0 nightly 193). Redo Step 3 with the **top** release on the nightly page, then close the game, press **Win + R**, type `%LOCALAPPDATA%\assembly` and delete the **`dl3`** folder (it's only a cache). `ScriptHookVDotNet.log` in your game folder says **"Unable to resolve API version"** when this is it. |
| You **updated** the mod, but the game still acts like the **old version** | Script Hook V .NET can keep running its saved copy of the old scripts. Close the game, press **Win + R**, type `%LOCALAPPDATA%\assembly`, press Enter and delete the **`dl3`** folder (it's only a cache). Still the old version? Install it again with OpenIV run as administrator (Step 6), or copy the scripts in yourself: see "Updating by hand" below. |
| **No minimap** in a car | `ViMinimap.asi` isn't next to `GTA5.exe`. Redo Step 7. |
| Your game folder has **`MiniMap.asi`** or **`MiniMapHudPresent.asi`** (from another 3D minimap mod or an older install) | Delete it. It's an older copy of the minimap plugin the HUD brings itself as `ViMinimap.asi`, and two copies loaded at once get in each other's way. Delete `scripts\MiniMap.dll` too if it's there (the HUD's is `scripts\ViMinimap.dll`). Keep `ViMinimap.asi` next to `GTA5.exe`. |
| The game **crashes while loading** | Install the mod again (Step 6): it raises the game limits the HUD needs. If you have other big mods (extra characters, maps), try turning them off. |
| The game **crashes while loading** since you added **Forests of San Andreas** or **GTA V Remastered** | The maps' many trees can fill three of the game's pools. In OpenIV, switch on **Edit mode**, open `mods\update\update.rpf\common\data\gameconfig.xml` and set the `PoolSize` `value` of **`Building`** to `70000`, **`phInstGta`** to `20000` and **`StaticBounds`** to `20000` (leave a value that's already higher). Don't install a different `gameconfig.xml` instead: it would replace the one the HUD set up (if you did, install the mod again, as in Step 6). Still crashing? Add the [Heap Adjuster](https://www.gta5-mods.com/tools/heapadjuster), which the maps' own guide suggests too. |
| A message says **Lucia is not registered** | Lucia isn't included with the HUD any more (see Step 5). Don't have her? Set `Enabled = False` under `[Lucia]` in `scripts\GTA6Hud.ini` and the message goes away. Have a Lucia model? Her pack is missing from `dlclist.xml`: add its line. If the message mentions a `VIHudSetup.exe`, ignore that part: this mod doesn't use it. |
| **Switching characters crashes**, and your game isn't in English | Players report this only happens when the game is set to another language. Until it's fixed, set GTA V's language to **English** in your game launcher's settings for GTA V. |
| The HUD says your game **won't save in its folder**, and a **Windows prompt** asks to let **icacls** make changes | Your game is in **Program Files**, where Windows lets only administrators change files, so the HUD can't save its settings (and ReShade can't save its own). Click **Yes**: it lets your Windows user change the game folder, the same as doing it by hand in the folder's **Properties** → **Security**. Then restart the game once. Clicked **No**? You're asked again the next time you start the game. `ViExtras.ini` `[GameFolder] FixAccess = False` stops the question. |
| **Something else** | Ask in the **[support Discord](https://discord.gg/C7gE5SHEzH)**, or [open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) and attach these files from your game folder: `ScriptHookVDotNet.log`, `ViMinimap.log`, `scripts\GTA6Hud.perf.log` and `scripts\GTA6Hud.error.log`. |

---

## Updating by hand (game in Program Files)

If your game is in **Program Files** (where the Rockstar Games Launcher and Steam put it), Windows guards that folder,
and an update through OpenIV doesn't always replace the old files unless you run it as administrator (Step 6).
You can also copy the new scripts in yourself. (From 2.6, the HUD offers to fix the folder for you the first time it runs: see the Windows prompt
row above. After that, updates go in normally.) An `.oiv` is just a zip:

1. Close the game.
2. Copy **`GTA-VI-Inspired-HUD-2.6.2-Legacy.oiv`** to your Desktop and rename the copy to **`.zip`**.
   Can't see the `.oiv` part of the name? In File Explorer, turn on **View** → **Show** → **File name extensions**.
3. Open the zip → **`content`** → **`scripts`**, select everything in there and copy it.
4. Paste it into the **`scripts`** folder in your game folder, for example
   `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts`. Pick **Replace the files in the destination**, then
   **Continue** when Windows asks for admin permission.
5. Clear Script Hook V .NET's cache, so it loads the new files: press **Win + R**, type `%LOCALAPPDATA%\assembly`
   and press Enter, then delete the **`dl3`** folder. It's only a cache, and it's made again the next time you start
   the game.
6. Coming from 2.5.4 or older? 2.5.5 adds a pack: also copy the zip's **`content`** → **`dlcpacks`** → **`vihud`**
   folder into **`mods\update\x64\dlcpacks`** in your game folder, and install the mod once with OpenIV as in
   Step 6, which switches the new pack on.
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
