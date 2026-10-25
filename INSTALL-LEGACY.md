# How to install: GTA V **Legacy**

This guide is for the **original PC version** of GTA V.

Does your game folder have a file called **`GTA5_Enhanced.exe`**? Then you have Enhanced.
Use the **[Enhanced guide](INSTALL-ENHANCED.md)** instead.

**Never modded GTA before?** No problem. Follow the steps in order. Don't skip any. It takes about 20 minutes.

> [!IMPORTANT]
> ### 💬 Need help? Join our support Discord
> This version is a **fork of FBICat's VI HUD** with extra features. So it has its own support:
> - **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**
> - or a [GitHub issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues)
>
> FBICat's Discord is there for his original mod. Please bring questions about this version to ours. Thank you!

> ⚠️ **Mods are for Story Mode only.** Never go into GTA Online with mods installed. You can get banned.
> (Script Hook V, one of the tools below, closes the game if you try.)

---

## Before you start: two quick Windows settings

### 1. Show file extensions

Some files in this guide end in `.asi` or `.dll`. Windows hides those endings by default.

1. Open any folder.
2. Click **View** at the top.
3. Click **Show** → **File name extensions**.

### 2. Know how to unzip

Most downloads are `.zip` files (a folder squeezed into one file). To open one:

1. Right-click it.
2. Choose **Extract All…**.
3. Click **Extract**.

✅ A normal folder appears next to it.

---

## Step 1: Find your GTA V folder

This is the folder with **`GTA5.exe`** inside. We'll call it your **game folder**.

How to open it:

| Your launcher | What to do |
| --- | --- |
| **Rockstar Games Launcher** | 1. Open **Settings**.<br>2. Click **Grand Theft Auto V** on the left.<br>3. Click **Open Folder** (or **View Installation Folder**). |
| **Steam** | 1. Go to your **Library**.<br>2. Right-click **Grand Theft Auto V**.<br>3. Click **Manage** → **Browse local files**. |
| **Epic Games** | 1. Go to your **Library**.<br>2. Click the **⋯** under GTA V.<br>3. Click **Manage**.<br>4. Click the **folder icon** next to "Installation". |

✅ Keep this folder open. You'll copy files into it a few times.

---

## Step 2: Install Script Hook V (lets mods run)

1. Go to the [Script Hook V page](http://www.dev-c.com/gtav/scripthookv/).
2. Click **Download**.
3. Unzip it.
4. Open the **`bin`** folder inside.
5. Copy these files into your **game folder**:
   - **`ScriptHookV.dll`**
   - **`dinput8.dll`** (loads mods ending in `.asi`)
   - **`args.txt`** (turns off BattlEye, Rockstar's anti-cheat, which blocks mods)
   - **`xinput1_4.dll`**, if it's there

⚠️ **Don't** copy `NativeTrainer.asi`. It's a cheat menu you don't need.

✅ Your game folder now has `ScriptHookV.dll` and `dinput8.dll` next to `GTA5.exe`.

---

## Step 3: Install Script Hook V .NET (lets this HUD run)

The HUD needs **3.7.0 nightly 193 or newer**. The **top** release on the nightly page always is.
With an older one, the HUD's scripts don't start, or one stops with an error (see "Something went wrong?").

1. Go to the [Script Hook V .NET nightly page](https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases).
2. Find the **top** release.
3. Open **Assets**.
4. Download the **`.zip`** file.
5. Unzip it.
6. Copy these into your **game folder**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`ScriptHookVDotNet.ini`**

✅ Those 4 files are now next to `GTA5.exe`.

---

## Step 4: Install OpenIV (installs the mod safely)

OpenIV puts mods in a separate **`mods` folder**. Your original game files are never changed.

1. Download OpenIV from [openiv.com](https://openiv.com/).
2. Install it.
3. Open OpenIV.
4. Choose **GTA V** → **Windows**.
5. If it asks, point it to your **game folder**.
6. At the top, click **Tools** → **ASI Manager**.
7. Click **Install** next to **ASI Loader**.
8. Click **Install** next to **OpenIV.asi**.
9. Close the window.

✅ OpenIV is ready.

---

## Step 5: Download the mod

1. Go to the [**Releases page**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. Under **Assets**, download **`GTA-VI-Inspired-HUD-2.6.4-Legacy.oiv`** (the mod, about 100 MB).
   It also switches the mod on in the game. There's no separate patch to get.

❌ Don't download `GTA-VI-Inspired-HUD-2.6.4-Enhanced.oiv`. That one is for the other version of the game.

**Want to play as Lucia or Jason?** Their models aren't included.

| Character | Where to get the model |
| --- | --- |
| **Lucia** | Her model is someone else's paid work. Get her from her creator:<br>**[Lucia Caminos on Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** or **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** on gta5-mods.com.<br>Or use another Lucia model.<br>To put her in the game, see "Optional: Lucia" below.<br>The HUD works without her. |
| **Jason** | **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** from the same creator, or **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** on mod-hub.net.<br>To put him in the game, see "Optional: Jason" below. |

💡 The number in the file name is the version (2.6.4 here). So you can always tell which one you installed.
Mention it when you ask for help.

---

## Step 6: Install the mod with OpenIV

⚠️ **Game in Program Files?** Does your game folder's path start with `C:\Program Files`?
(The Rockstar Games Launcher, Epic Games and Steam put it there.)
Then right-click **OpenIV** → **Run as administrator**, and click **Yes**.

💡 Windows lets only administrators change files there. Without it, the install can leave the old files in place.

1. In OpenIV, click **Tools** → **Package Installer**.
2. Pick **`GTA-VI-Inspired-HUD-2.6.4-Legacy.oiv`**.
3. Click **Install**.
4. When it asks where to install, choose **mods folder**.
5. If it offers to create the folder, say yes.
6. Wait until it says it's finished.
7. Click **Close**.

✅ Installing the mod again, or over an older version, is fine. It never lists a pack twice.

---

## Step 7: Check one file

Look in your **game folder** for **`ViMinimap.asi`**. It should be right next to `GTA5.exe`.

| Is it there? | What to do |
| --- | --- |
| **Yes** | Great, go to Step 8. |
| **No** | 1. Open the **`mods`** folder in your game folder.<br>2. Find **`ViMinimap.asi`**.<br>3. Move it next to **`GTA5.exe`**. |

💡 Without it, the minimap won't show.

---

## Step 8: Play! 🎉

1. Start GTA V.
2. Choose **Story Mode**.

✅ The new HUD appears on its own.

| To... | Do this |
| --- | --- |
| **Switch character** | Hold **Left Alt** (controller: hold **D-pad down**).<br>Pick with **Q / E** or **Left / Right** (controller: **LB / RB**).<br>Then let go.<br>Jason and Lucia need their models (not included, see Step 5). |
| **See the minimap** | Get in a car. It only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Play your own music** | Put `.mp3` files in **`scripts\Hud\Radio\CustomMusic`** in your game folder.<br>In a car, hold the radio button (**Q**) and press **Down** (controller: right stick down) for **On Demand**.<br>A folder in `CustomMusic` is a station of its own.<br>**Left / Right** (right stick) switch stations.<br>The **mouse wheel** (D-pad Right / Up) changes the song. |

---

## Optional: Jason

**What you get:** Jason on the character wheel, after Trevor.
**What you need:** his model (it isn't included) and the free **Jason kit**. Follow the parts in order.

### Part 1: Get Jason

1. Download Jason from **one** of these:
   - **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** from Lucia's creator
   - **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** on mod-hub.net
2. Unzip it.
3. Open the folder **`No Cap Version`** (or **`Cap Version`** if you want him in his cap).
4. Inside are **4 files**. Their names end in **`.ydd`**, **`.yft`**, **`.ymt`** and **`.ytd`**.

> Can't see the endings (`.ydd` and so on)? In File Explorer, click **View** → **Show** → **File name extensions**.

### Part 2: Check the 4 file names

Look at the part of each name **before the dot**:

| Your 4 files are called | What to do |
| --- | --- |
| `jdfinal.ydd`, `jdfinal.yft`, `jdfinal.ymt`, `jdfinal.ytd` | **Nothing.** Go to Part 3. |
| Anything else, for example `Jason.ydd`, `Jason.yft`, `Jason.ymt`, `Jason.ytd` | **Rename each file to `jdfinal`**, keeping the ending: `Jason.ydd` becomes **`jdfinal.ydd`**, `Jason.yft` becomes **`jdfinal.yft`**, `Jason.ymt` becomes **`jdfinal.ymt`**, `Jason.ytd` becomes **`jdfinal.ytd`**. Change nothing else. |

That's it: the HUD looks for **`jdfinal`**, so once your files are called that, he's found. You don't need to edit
any settings.

### Part 3: Put him in the game

1. Download **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** and unzip it.
2. Copy the kit's **`jasonped`** folder into **`mods\update\x64\dlcpacks`** in your game folder.
3. Open **OpenIV** and click **Edit mode** at the top, so it's on.
4. In OpenIV, open these one after another: `mods` → `update` → `x64` → `dlcpacks` → `jasonped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **`jasonped.rpf`**.
5. Drag your 4 **`jdfinal`** files into that window.
6. In OpenIV, open: `mods` → `update` → **`update.rpf`** → `common` → `data`. Right-click **`dlclist.xml`** → **Edit**.
7. Scroll to the **very bottom**. Put your cursor at the start of the line that says **`</Paths>`**, press **Enter**,
   and paste this into the new empty line above it:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Press **Ctrl + S** to save, and close the window.
9. Start the game and hold **Left Alt** (controller: hold **D-pad Down**). Jason's card is after Trevor's.

### Not working?

| What you see | What to do |
| --- | --- |
| Jason isn't on the wheel | Open **`scripts\ViSwitch.log`** in your game folder with Notepad. If it says **"no Jason model in the game"**, one of these is wrong: your 4 files aren't called `jdfinal` (Part 2), they aren't inside **`jasonped.rpf`**, or `dlclist.xml` is missing the line **`<Item>dlcpacks:/jasonped/</Item>`**. Fix it and start the game again. |

### Other Jason models (advanced)

You only need this if your Jason download isn't the 4 files above.

- **Lots of files** with a `^` in their names (like `Jason^head_000_r.ydd`, `Jason^uppr_000_u.ydd`): rename the part
  **before the `^`** to `jdfinal` in **every** file (`Jason^head_000_r.ydd` becomes `jdfinal^head_000_r.ydd`), and
  the files without a `^` too (`Jason.ymt` becomes `jdfinal.ymt`). Put them all in `jasonped.rpf`.
- **It comes as a pack of its own** (it has its own `dlc.rpf`): you don't need the kit. Install that pack the way its
  readme says. Then open **`scripts\GTA6Hud.ini`** with Notepad, find **`[Jason]`**, and change **`Model = jdfinal`**
  to its ped's name (the name of its `.ydd` file, before the dot).

---

## Optional: Lucia

**What you get:** Lucia on the character wheel, the last card.
**What you need:** her model (it isn't included) and the free **Lucia kit**. Follow the parts in order.

💡 Already have a **`luciaped`** folder in your dlcpacks folder? Then a Lucia is already installed: skip this part.

### Part 1: Get Lucia

1. Download Lucia from **one** of these, or use another Lucia model:
   - **[Lucia Caminos on Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** from her creator
   - **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** on gta5-mods.com
2. Unzip it, and find the folder with her files in it. Their names end in `.yft`, `.ymt` and so on.

> Can't see the endings (`.yft` and so on)? In File Explorer, click **View** → **Show** → **File name extensions**.

### Part 2: Which pack, and her file names

The kit has two packs, **A** and **B**. Look at what's in her folder:

| Her folder has | Pack | What to do |
| --- | --- | --- |
| **4 files**, ending in `.ydd`, `.yft`, `.ymt` and `.ytd` | **A** (`A - 4 files`) | Called `lucia_s` already? **Nothing.** Called anything else, for example `Lucia.ydd`? **Rename each file to `lucia_s`**, keeping the ending: `Lucia.ydd` becomes **`lucia_s.ydd`**, and the same for `.yft`, `.ymt` and `.ytd`. Change nothing else. |
| A **`.yft`**, a **`.ymt`** and a **folder** with the same name (full of files like `head_000_r.ydd`) | **B** (`B - with a folder`) | Called `lucia_s` already? **Nothing.** Called anything else, for example `Lucia`? **Rename all three to `lucia_s`**: `Lucia.yft` becomes **`lucia_s.yft`**, `Lucia.ymt` becomes **`lucia_s.ymt`** and the folder `Lucia` becomes **`lucia_s`**. Don't rename the files inside the folder. A second folder ending in `_p` (her hats and glasses)? Rename it to **`lucia_s_p`**. |

That's it: the HUD looks for **`lucia_s`**, so once her files are called that, she's found. You don't need to edit
any settings.

Below, **her archive** is **`luciaped.rpf`** for pack A, and **`luciaped_streamed.rpf`** for pack B.

### Part 3: Put her in the game

1. Download **[GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip)** and unzip it.
2. Copy the **`luciaped`** folder from your pack (**`A - 4 files`** or **`B - with a folder`**) into
   **`mods\update\x64\dlcpacks`** in your game folder.
3. Open **OpenIV** and click **Edit mode** at the top, so it's on.
4. In OpenIV, open these one after another: `mods` → `update` → `x64` → `dlcpacks` → `luciaped` → **`dlc.rpf`** → `x64`
   → `models` → `cdimages` → **her archive**.
5. Drag her **`lucia_s`** files into that window (pack B: the **`lucia_s`** folder too, and **`lucia_s_p`** if you
   have it).
6. In OpenIV, open: `mods` → `update` → **`update.rpf`** → `common` → `data`. Right-click **`dlclist.xml`** → **Edit**.
7. Scroll to the **very bottom**. Put your cursor at the start of the line that says **`</Paths>`**, press **Enter**,
   and paste this into the new empty line above it:
   ```xml
   <Item>dlcpacks:/luciaped/</Item>
   ```
8. Press **Ctrl + S** to save, and close the window.
9. Start the game and hold **Left Alt** (controller: hold **D-pad Down**). Lucia's card is the last one.

### Not working?

| What you see | What to do |
| --- | --- |
| A message says **Lucia is not registered** | `dlclist.xml` is missing the line **`<Item>dlcpacks:/luciaped/</Item>`**. Add it and start the game again. |
| Lucia isn't on the wheel | Open **`scripts\GTA6Hud.perf.log`** in your game folder with Notepad. If it says **"lucia: no Lucia model in the game"**, one of these is wrong: her files aren't called `lucia_s` (Part 2), they aren't inside **her archive**, you used the other pack (4 files need **A**, files with a folder need **B**), or `dlclist.xml` is missing the line **`<Item>dlcpacks:/luciaped/</Item>`**. Fix it and start the game again. |
| Lucia is invisible, or only partly there | It's the wrong pack for her files (Part 2). Delete the `luciaped` folder you copied and start again with the other one. |

### Other Lucia models (advanced)

You only need this if your Lucia download isn't one of the two kinds in Part 2.

- **Lots of files** with a `^` in their names (like `Lucia^head_000_r.ydd`): they're the parts of a pack **B** Lucia,
  laid out for FiveM. Make a folder called **`lucia_s`**, move every file with a `^` into it, and rename each to the
  part **after** the `^` (`Lucia^head_000_r.ydd` becomes `head_000_r.ydd`). Rename the files without a `^` to
  `lucia_s` (`Lucia.yft` becomes `lucia_s.yft`), then use pack B. Files with `_p^` (her hats and glasses) go the same
  way into a folder called **`lucia_s_p`**.
- **It comes as a pack of its own** (it has its own `dlc.rpf`): you don't need the kit. Install that pack the way its
  readme says. Then open **`scripts\GTA6Hud.ini`** with Notepad, find **`[Lucia]`**, and change **`Model = lucia_s`**
  to its ped's name (the name of its `.yft` file, before the dot).
- **Keep her own name** instead of renaming her files: the kit's `README.txt` shows how, with the pack's `peds.meta`.

---

## Optional: Forests of San Andreas and GTA V Remastered

**What you get:** tens of thousands of extra trees, bushes and props on the map.
**What you need:** **Forests of San Andreas**, **GTA V Remastered**, or both, by **Larcius** (built on JRod's and le__AK's maps). They work with this HUD.
They come as OIVs too, so they go in the same way as the HUD.

1. Install the HUD first (Steps 1 to 7).
2. In each mod's zip, open the **`GTA V Legacy`** folder.
3. In OpenIV, click **Tools** → **Package Installer**.
4. Pick one you want:
   - **`Forests of San Andreas (North) Ultimate v….oiv`**
   - **`Forests of San Andreas (South) Ultimate v….oiv`**
   - **`GTA V Remastered Ultimate v….oiv`**
5. Pick **mods folder**, like in Step 6.
6. Do steps 3 to 5 again for each one you want.

   💡 Each one adds its own pack to the game's pack list, next to the HUD's. So there's nothing to edit.
7. Using **Liberty City V Remix** as well? Also install the mods' **`Compatibility Liberty City V Remix`** OIVs.
8. ⚠️ **Leave `gameconfig.xml` as it is.** The maps don't need a different one.
   Installing another `gameconfig.xml` would replace the one the HUD set up, which the HUD needs.
   Crashes while loading with the maps in? See "Something went wrong?" below.

**To take them off again:** install the mods' **`Uninstaller`** OIVs the same way, to the mods folder.

---

## Optional: Rebalanced Dispatch Enhanced

**What you get:** **Rebalanced Dispatch Enhanced** (RDE) by **the RDE Team**. It replaces GTA's police response:
new agencies, roadblocks, helicopters, and up to **10 stars**. The HUD steps out of its way.
**What you need:** the HUD installed first, and **`RDE 4.1.5.oiv`** from RDE's download.

> [!NOTE]
> New in 2.6.3, and only tested briefly in game.
> Something off with RDE and the HUD together? Tell us on Discord or in a GitHub issue.

### Part 1: Install RDE

1. Install the HUD first (Steps 1 to 7).

   💡 Step 3's Script Hook V .NET is the nightly RDE asks for.
2. In OpenIV, click **Tools** → **Package Installer**.
3. Pick **`RDE 4.1.5.oiv`** from RDE's download.
4. Pick **mods folder**, like in Step 6.

⚠️ Skip RDE's **`Compatibility - Community HUD`** files. They're for another HUD mod.

💡 RDE brings its own `gameconfig.xml` with much bigger limits than the HUD needs. That's fine.
Installing or updating the HUD afterwards only raises a limit that's too low. So it never undoes RDE's.

### Part 2: What changes while RDE runs

- RDE's police take over.
- These stay out:
  - GTA6Hud's police brain
  - GTA6Hud's own six stars (and the military they call in)
  - the HUD's stun-gun arrests
- The HUD's stars show RDE's level up to **six**. Levels 6 to 10 all light the sixth star.
- To leave the HUD as it is, set `[Compat] Rde = False` in `ViExtras.ini`.

### Part 3: Take RDE off again

1. Install RDE's **`Uninstaller.oiv`** the same way.
2. Install the HUD again (Step 6), so its limits and packs are back.

---

## Optional: Los Santos RED

**What you get:** **Los Santos RED** (LSR) by **Greskrendtregk**. It turns the game into a crime sandbox.
It has its own police, crimes, wanted level and much more. The HUD steps out of its way.
**What you need:** **RAGE Plugin Hook** and **RAGENativeUI**. LSR runs in RAGE Plugin Hook, not Script Hook V .NET.

> [!NOTE]
> New in 2.6.3, and only tested briefly in game.
> Something off with LSR and the HUD together? Tell us on Discord or in a GitHub issue.

⚠️ Use LSR on its own. Not together with LSPDFR or Rebalanced Dispatch Enhanced: they run the police too.

### Part 1: Install LSR

1. Install the HUD first (Steps 1 to 7).
2. Install **[RAGE Plugin Hook](https://ragepluginhook.net/)**.
3. Install **[RAGENativeUI](https://github.com/alexguirre/RAGENativeUI/releases)**. LSR's menus need it.
4. Install LSR itself, as LSR's own page says.
5. Start the game with **`RAGEPluginHook.exe`**.

✅ LSR loads once Story Mode has loaded.

### Part 2: What changes while LSR runs

LSR has the police, the crimes and the wanted level. So the HUD adds none of its own:

- no witnesses
- no police brain or police memory
- no heat
- no six stars
- no stun-gun arrests

To leave the HUD as it is, set `[Compat] Lsr = False` in `ViExtras.ini`.

### Part 3: Stars

LSR keeps GTA's own wanted level at zero and draws stars of its own.
So the HUD's stars and wanted box don't come up with LSR.

Want GTA VI stars in LSR's place?

1. Find the 4 pictures in the HUD's [`extras/lsr-stars/wantedlevel`](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/tree/main/extras/lsr-stars/wantedlevel) folder.
2. Copy them into **`Plugins\LosSantosRED\images\wantedlevel`**.
3. Replace LSR's pictures with them.

### Part 4: LSR's settings

LSR writes `Plugins\LosSantosRED\Settings.xml` the first time it runs.

1. Close the game.
2. Open **`Plugins\LosSantosRED\Settings.xml`** in Notepad.
3. Change these values:

| Setting | Set to | Why |
| --- | --- | --- |
| `AlwaysShowRadar` | `false` | The HUD's 3D minimap replaces GTA's radar. LSR switching the radar on puts it back under the map. |
| `NeverShowRadar` | `true` | The same. |
| `TopDisplayShowWeapon` | `false` | The HUD shows your weapon at the top right already. |
| `ShowStreetDisplay` | `false` | The HUD shows the street under the minimap already. |

### Part 5: Keys

LSR uses some of the HUD's keys. Change one side of each pair you use:

| Key | HUD | LSR |
| --- | --- | --- |
| **Left Ctrl** | Crouch (`ViCrouch.ini` `[Crouch] Key`) | Crouch (`CrouchKey` in LSR's settings) |
| **K** | Skills panel (`ViExtras.ini` `[Skills] Key`) | No, in conversations |
| **O** | Radio mute (`ViExtras.ini` `[Radio] MuteKey`) | Start an interaction |
| **Shift + N** | Sit-ups (`ViExtras.ini` `[Gym] SitUpsKey`) | Group mode |

---

## Optional: LSPDFR

**What you get:** **LSPDFR** (LSPD First Response) by **G17 Media**. You play as a police officer.
Go on duty, take callouts and pull people over. The HUD keeps working, and it knows when you're on duty.
**What you need:** the HUD installed first, and LSPDFR from **[lcpdfr.com](https://www.lcpdfr.com/)**.
LSPDFR runs in RAGE Plugin Hook, not Script Hook V .NET.

> [!NOTE]
> Tested on **Legacy** only.

⚠️ Don't use LSPDFR with **Los Santos RED**. Both run the police.

💡 **Rebalanced Dispatch Enhanced** comes with its own LSPDFR files, so those two can go together.
We haven't tested the HUD with both at once.

### Part 1: Install LSPDFR

1. Install the HUD first (Steps 1 to 7).
2. Install LSPDFR, as LSPDFR's own page says.
3. Start the game with **`RAGEPluginHook.exe`**.

✅ LSPDFR loads once Story Mode has loaded.

💡 Playing without LSPDFR? Start the game the normal way. LSPDFR only loads through `RAGEPluginHook.exe`.

### Part 2: What the HUD does on duty

**You don't need to change anything.** The HUD finds LSPDFR on its own. While you're on duty:

- **No character switching.** The character wheel stays shut, and the switch keys do nothing.
  Switching away would take LSPDFR's officer away from it.
- **No witnesses.** Nobody runs or phones to report you. You're the police.
- **No wanted level.** If one comes up anyway, it's cleared at once.

Go off duty, and it all works as before.

💡 Want to switch characters? Go off duty first.

### Part 3: Keys

Three keys do two things at once. Change one side of each pair you use:

| Key | HUD side | LSPDFR side |
| --- | --- | --- |
| **Y** | Greet someone (`ViExtras.ini` `[Memory] GreetKey`) | Accept a callout (`ACCEPT_CALLOUT_Key` in `lspdfr\keys.ini`) |
| **N** | Push-ups (`ViExtras.ini` `[Gym] PushUpsKey`) | Pursuit menu (`PURSUIT_MENU_Key` in `lspdfr\keys.ini`) |
| **F4** | Script Hook V .NET's console (`ConsoleKeyBinding` in `ScriptHookVDotNet.ini`) | RAGE Plugin Hook's console (`ConsoleKey` in `RagePluginHook.ini`) |

To change a key:

1. Close the game.
2. Open the file in Notepad.
   - `ViExtras.ini` is in your **`scripts`** folder.
   - The other files are in your **game folder**.
3. Change the letter after the `=`. Pick a key nothing else uses.
4. Save the file.

---

## Something went wrong?

**Still stuck?** Ask in **[our support Discord](https://discord.gg/C7gE5SHEzH)**, the one for this version.
Or [open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| What happens | What to do |
| --- | --- |
| A Script Hook V message says **"unknown game version"** | GTA V just updated. Script Hook V hasn't caught up yet.<br>1. Wait for a new Script Hook V.<br>2. Redo Step 2. |
| The game starts but **the new HUD doesn't show** | Your Script Hook V .NET is too old.<br>Redo Step 3 with the newest version. |
| A message says **Unhandled exception in script** (for example **"MiniMap"**) with a **FileNotFoundException** | Your Script Hook V .NET is older than the HUD needs (3.7.0 nightly 193).<br>1. Redo Step 3 with the **top** release on the nightly page.<br>2. Close the game.<br>3. Press **Win + R**.<br>4. Type `%LOCALAPPDATA%\assembly` and press **Enter**.<br>5. Delete the **`dl3`** folder (it's only a cache).<br>💡 `ScriptHookVDotNet.log` in your game folder says **"Unable to resolve API version"** when this is it. |
| You **updated** the mod, but the game still acts like the **old version** | Script Hook V .NET can keep running its saved copy of the old scripts.<br>1. Close the game.<br>2. Press **Win + R**.<br>3. Type `%LOCALAPPDATA%\assembly` and press **Enter**.<br>4. Delete the **`dl3`** folder (it's only a cache).<br>Still the old version? Install it again with OpenIV run as administrator (Step 6).<br>Or copy the scripts in yourself: see "Updating by hand" below. |
| **No minimap** in a car | `ViMinimap.asi` isn't next to `GTA5.exe`.<br>Redo Step 7. |
| Your game folder has **`MiniMap.asi`** or **`MiniMapHudPresent.asi`** (from another 3D minimap mod or an older install) | 1. Delete it.<br>2. Delete **`scripts\MiniMap.dll`** too, if it's there. (The HUD's is `scripts\ViMinimap.dll`.)<br>3. Keep **`ViMinimap.asi`** next to `GTA5.exe`.<br>💡 It's an older copy of the minimap plugin. The HUD brings its own copy as `ViMinimap.asi`. Two copies loaded at once get in each other's way. |
| The game **crashes while loading** | 1. Install the mod again (Step 6). It raises the game limits the HUD needs.<br>2. Have other big mods (extra characters, maps)? Try turning them off. |
| The game **crashes while loading** since you added **Forests of San Andreas** or **GTA V Remastered** | The maps' many trees can fill three of the game's pools.<br>1. In OpenIV, switch on **Edit mode**.<br>2. Open: `mods` → `update` → `update.rpf` → `common` → `data` → **`gameconfig.xml`**.<br>3. Set the `PoolSize` `value` of **`Building`** to `70000`.<br>4. Set **`phInstGta`** to `20000`.<br>5. Set **`StaticBounds`** to `20000`.<br>Leave a value that's already higher.<br>⚠️ Don't install a different `gameconfig.xml` instead. It would replace the one the HUD set up. (If you did, install the mod again, as in Step 6.)<br>Still crashing? Add the [Heap Adjuster](https://www.gta5-mods.com/tools/heapadjuster), which the maps' own guide suggests too. |
| A message says **Lucia is not registered** | Lucia isn't included with the HUD any more (see Step 5).<br>**Don't have her?** Set `Enabled = False` under `[Lucia]` in `scripts\GTA6Hud.ini`. The message goes away.<br>**Have a Lucia model?** Her pack is missing from `dlclist.xml`. Add its line.<br>💡 If the message mentions a `VIHudSetup.exe`, ignore that part. This mod doesn't use it. |
| **Switching characters crashes**, and your game isn't in English | Players report this only happens when the game is set to another language.<br>Until it's fixed, set GTA V's language to **English** in your game launcher's settings for GTA V. |
| The HUD says your game **won't save in its folder**, and a **Windows prompt** asks to let **icacls** make changes | Your game is in **Program Files**. Windows lets only administrators change files there. So the HUD can't save its settings (and ReShade can't save its own).<br>1. Click **Yes**. It lets your Windows user change the game folder.<br>(The same as doing it by hand in the folder's **Properties** → **Security**.)<br>2. Restart the game once.<br>**Clicked No?** You're asked again the next time you start the game.<br>To stop the question, set `[GameFolder] FixAccess = False` in `ViExtras.ini`. |
| **Something else** | Ask in the **[support Discord](https://discord.gg/C7gE5SHEzH)**.<br>Or [open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) and attach these files from your game folder:<br>`ScriptHookVDotNet.log`<br>`ViMinimap.log`<br>`scripts\GTA6Hud.perf.log`<br>`scripts\GTA6Hud.error.log` |

---

## Updating by hand (game in Program Files)

Is your game in **Program Files**? (The Rockstar Games Launcher and Steam put it there.)
Windows guards that folder. An update through OpenIV doesn't always replace the old files there.
Run OpenIV as administrator (Step 6), or copy the new scripts in yourself.

💡 From 2.6, the HUD offers to fix the folder for you the first time it runs: see the Windows prompt row above.
After that, updates go in normally.

An `.oiv` is just a zip:

1. Close the game.
2. Copy **`GTA-VI-Inspired-HUD-2.6.4-Legacy.oiv`** to your Desktop.
3. Rename the copy to **`.zip`**.

   💡 Can't see the `.oiv` part of the name? In File Explorer, turn on **View** → **Show** → **File name extensions**.
4. Open the zip → **`content`** → **`scripts`**.
5. Select everything in there and copy it.
6. Paste it into the **`scripts`** folder in your game folder.
   For example: `C:\Program Files\Rockstar Games\Grand Theft Auto V\scripts`.
7. Pick **Replace the files in the destination**.
8. Click **Continue** when Windows asks for admin permission.
9. Clear Script Hook V .NET's cache, so it loads the new files:
   - Press **Win + R**.
   - Type `%LOCALAPPDATA%\assembly` and press Enter.
   - Delete the **`dl3`** folder.

   💡 It's only a cache. It's made again the next time you start the game.
10. **Coming from 2.5.4 or older?** 2.5.5 adds a pack. Also do this:
    - Copy the zip's **`content`** → **`dlcpacks`** → **`vihud`** folder into **`mods\update\x64\dlcpacks`** in your game folder.
    - Install the mod once with OpenIV, as in Step 6. This switches the new pack on.
11. Start the game.

✅ `scripts\ViExtras.log` now begins with a `==== ViExtras …` line. It shows the version you downloaded.

---

## How to remove the mod

1. In your **game folder**, delete **`ViMinimap.asi`**.
2. Open the **`scripts`** folder.
3. Delete these:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - the **`MiniMap.*`** files
   - the **`Hud`** folder
4. Delete the **`mods`** folder.

   ⚠️ This also removes any other mods you installed with OpenIV.

✅ Your game is back to normal. Script Hook V and Script Hook V .NET can stay. They don't change anything
on their own.
