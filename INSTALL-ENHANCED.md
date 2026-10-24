# How to install: GTA V **Enhanced**

This guide is for the **2025 PC version** of GTA V. Check which file your game folder has:

| Your game folder has | You have | Use |
| --- | --- | --- |
| **`GTA5_Enhanced.exe`** | Enhanced | **This guide** |
| **`GTA5.exe`** | Legacy | The **[Legacy guide](INSTALL-LEGACY.md)** |

Never modded GTA before? No problem. Follow the steps in order. Don't skip any. It takes about 20 minutes.

> [!IMPORTANT]
> ### 💬 Need help? Join our support Discord
> This version is a **fork of FBICat's VI HUD** with extra features. So it has its own support:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, or a [GitHub issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat's Discord is there for his original mod. Please bring questions about this version to ours. Thank you!

> ⚠️ **Mods are for Story Mode only.** Never go into GTA Online with mods installed. You can get banned.
> (Script Hook V, one of the tools below, closes the game if you try.)

> ⚠️ **Enhanced uses different tools than Legacy.** Only use the downloads linked in this guide.
> Legacy tools and Legacy mods make Enhanced crash.

---

## Before you start: two quick Windows settings

### 1. Show file extensions

Some files in this guide end in `.asi` or `.dll`. Windows hides those endings until you turn this on.

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

## Step 1: Find your GTA V Enhanced folder

This is the folder with **`GTA5_Enhanced.exe`** inside. We'll call it your **game folder**.

How to open it:

| Your launcher | Do this |
| --- | --- |
| **Rockstar Games Launcher** | Open **Settings**.<br>Click **Grand Theft Auto V Enhanced** on the left.<br>Click **Open Folder** (or **View Installation Folder**). |
| **Steam** | Open your **Library**.<br>Right-click **Grand Theft Auto V Enhanced**.<br>Click **Manage** → **Browse local files**. |
| **Epic Games** | Open your **Library**.<br>Click the **⋯** under GTA V Enhanced.<br>Click **Manage**.<br>Click the **folder icon** next to "Installation". |

✅ Keep this folder open. You'll copy files into it a few times.

---

## Step 2: Install Script Hook V (lets mods run)

1. Go to the [Script Hook V page](http://www.dev-c.com/gtav/scripthookv/).
2. Click **Download**. The same download works for Legacy and Enhanced.
3. Unzip it.
4. Open the **`bin`** folder inside.
5. Copy these files into your **game folder**:
   - **`ScriptHookV.dll`**
   - **`xinput1_4.dll`** (loads mods ending in `.asi` on Enhanced)
   - **`args.txt`** (turns off BattlEye, Rockstar's anti-cheat, which blocks mods)
   - **`dinput8.dll`** (the mod loader for Legacy. Enhanced doesn't use it, so copying it does no harm.)
6. ⚠️ **Don't** copy `NativeTrainer.asi`. It's a cheat menu you don't need.

✅ Your game folder now has `ScriptHookV.dll` and `xinput1_4.dll` next to `GTA5_Enhanced.exe`.

---

## Step 3: Install Script Hook V .NET **Enhanced** (lets this HUD run)

⚠️ This is **not** the normal Script Hook V .NET. The normal one makes Enhanced close by itself,
about 20 seconds after you load in. You need the **Enhanced** edition, called **SHVDNE**.

1. Go to the [SHVDNE releases page](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest).
2. Under **Assets**, download the **`.zip`** file.
3. Unzip it.
4. Copy the **5 files** from its main folder into your **game folder**:
   - **`ScriptHookVDotNet.asi`**
   - **`ScriptHookVDotNet.ini`**
   - **`ScriptHookVDotNet2.dll`**
   - **`ScriptHookVDotNet3.dll`**
   - **`MinHook.x64.dll`**
5. Skip the `Debug`, `Docs` and `Licenses` folders and `README.txt`. You don't need them.

Windows asks to replace files? Click **Replace**. (It means you had the normal version before.)

✅ Those 5 files are now next to `GTA5_Enhanced.exe`.

---

## Step 4: Install OpenRPF (keeps the mod in a safe `mods` folder)

OpenRPF lets the game load mods from a separate **`mods` folder**. Your original game files are never changed.

1. Go to the [OpenRPF page](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced).
2. Download it.
3. Unzip it.
4. Copy **`OpenRPF.asi`** into your **game folder**.
5. ⚠️ Does the download also have a `dsound.dll`? **Don't copy it.** Script Hook V already gave you a mod
   loader: `xinput1_4.dll` from Step 2. Two loaders at once cause crashes.
6. In your game folder, make a new folder called **`mods`**, if there isn't one already.
   (Right-click → **New** → **Folder**.)

✅ `OpenRPF.asi` is next to `GTA5_Enhanced.exe`, and there's a `mods` folder.

---

## Step 5: Download the mod and the installer

### Part 1: Get the mod

1. Go to the [**Releases page**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
2. Under **Assets**, download **`GTA-VI-Inspired-HUD-2.6.4-Enhanced.oiv`**. It's the mod, about 100 MB.
   It also switches the mod on in the game, so there's no separate patch to get.

❌ Don't download `GTA-VI-Inspired-HUD-2.6.4-Legacy.oiv`. It's for the other version and will crash Enhanced.

💡 The number in the file name is the version (2.6.4 here). That way you can always tell which one you installed.
Mention it when you ask for help.

### Want to play as Lucia?

She isn't included: her model is someone else's paid work. The HUD works without her.

Get her from her creator:
- **[Lucia Caminos on Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)**
- **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** on gta5-mods.com

Or use another Lucia model.

**For Jason**, get one of these:
- **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** from the same creator
- **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** on mod-hub.net

Then see "Optional: Jason" below to put him in the game.

### Part 2: Get the installer

OpenIV doesn't install mods on Enhanced. So you'll use the **OIV Package Installer** instead.

1. Go to [its gta5-mods page](https://www.gta5-mods.com/tools/oiv-package-installer).
2. Download it.
3. Unzip it anywhere, like your Desktop.

---

## Step 6: Install the mod

First, look at your game folder's path. It decides how you start the installer:

| Your game folder's path starts with | How to start the installer |
| --- | --- |
| `C:\Program Files` (the Rockstar Games Launcher, Epic Games and Steam put it there) | Right-click **`CodeWalker.OIVInstaller.exe`** → **Run as administrator**.<br>Click **Yes**. |
| Anything else | Run **`CodeWalker.OIVInstaller.exe`**. |

⚠️ **Game in Program Files?** Windows lets only administrators change files there. Without **Run as administrator**,
the install can leave the old files in place.

1. Open the OIV Package Installer folder.
2. Run **`CodeWalker.OIVInstaller.exe`** the way the table says.
3. Drag **`GTA-VI-Inspired-HUD-2.6.4-Enhanced.oiv`** onto its window. (Or click **Browse** and pick it.)
4. It asks for your game folder. Pick the folder with **`GTA5_Enhanced.exe`**.
5. Click **Install**.
6. Wait until it's finished.

✅ Installing the mod again, or over an older version, is fine. It never lists a pack twice.

---

## Step 7: Change one game setting

1. Start the game.
2. Go to **Settings** → **Graphics**.
3. Set **Screen Type** to **Windowed Borderless**.

💡 On Enhanced, the minimap is drawn in a little window on top of the game. That window only shows in
Borderless mode.

---

## Step 8: Play! 🎉

Load **Story Mode**. The new HUD appears on its own.

| To... | Do this |
| --- | --- |
| **Switch character** | Hold **Left Alt** (controller: hold **D-pad down**).<br>Pick with **Q / E** or **Left / Right** (controller: **LB / RB**).<br>Let go.<br>Jason and Lucia need their models (not included, see Step 5). |
| **See the minimap** | Get in a car.<br>It only shows in vehicles, or when you're wanted. |
| **Open the HUD settings** | Hold **V**. |
| **Play your own music** | Put `.mp3` files in **`scripts\Hud\Radio\CustomMusic`** in your game folder.<br>In a car, hold the radio button (**Q**).<br>Press **Down** (controller: right stick down) for **On Demand**.<br>A folder in `CustomMusic` is a station of its own.<br>**Left / Right** (right stick) switch stations.<br>The **mouse wheel** (D-pad Right / Up) changes the song. |

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

### Part 3: Make his files work on Enhanced

Enhanced can't use the files as they come, so convert them first.

1. Get **CodeWalker RPF Explorer** (version **30_dev48** or newer) from the releases channel of
   [dexyfex's Discord](https://discord.gg/BxfKHkk).
2. Make a new, empty folder on your Desktop called **`Jason converted`**.
3. In CodeWalker RPF Explorer, click **Tools** → **Asset Converter**.
4. For the **input** folder, pick the folder with your 4 **`jdfinal`** files. For the **output** folder, pick
   **`Jason converted`**. Click **Process**.
5. Open **`Jason converted`**. If **`jdfinal.ymt`** isn't in it, copy it in from the input folder yourself.

### Part 4: Put him in the game

1. Download **[GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip)** and unzip it.
2. Copy the kit's **`jasonped`** folder into your game folder:
   - **With NaturalVision Enhanced:** into **`onigiri\dlcpacks`**
   - **Without it:** into **`mods\update\x64\dlcpacks`**
3. In CodeWalker RPF Explorer, click **Edit** → **Edit mode**, so it's on.
4. Open the **`jasonped`** folder you just copied, then: **`dlc.rpf`** → `x64` → `models` → `cdimages` →
   **`jasonped.rpf`**.
5. Drag the 4 files from **`Jason converted`** into that window.
6. Open your **`dlclist.xml`**:
   - **With NaturalVision Enhanced:** open **`onigiri\common\data\dlclist.xml`** with **Notepad**.
   - **Without it:** in CodeWalker RPF Explorer, open `mods` → `update` → **`update.rpf`** → `common` → `data` →
     **`dlclist.xml`**.
7. Scroll to the **very bottom**. Put your cursor at the start of the line that says **`</Paths>`**, press **Enter**,
   and paste this into the new empty line above it:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
8. Press **Ctrl + S** to save, and close it.
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

## Optional: NaturalVision Enhanced

**What you get:** **NaturalVision Enhanced** (NVE) by **Razed**, working with this HUD.
**What you need:** NVE's zip. NVE loads mods with its own loader (onigiri), so a few steps go differently.

1. Do Steps 1 to 3 of this guide (Script Hook V and SHVDNE).
   - ⚠️ **Skip Step 4: don't install OpenRPF.** NVE warns about `OpenRPF.asi` when the game starts.
   - Already copied `OpenRPF.asi`? Delete it from your game folder.
2. Install NVE:
   1. Open its zip.
   2. Go into the **`Main Package`** folder.
   3. Copy everything inside it into your **game folder**. (NVE's own `README Install Guide.html` has the details.)
   4. Windows asks to replace **`xinput1_4.dll`**? Click **Replace**. It's the same file Script Hook V uses.
3. Install the HUD with Steps 5 and 6.
4. Copy the HUD's packs into **`onigiri\dlcpacks`**:
   1. In your game folder, open **`mods\update\x64\dlcpacks`**. The installer puts the HUD's packs here.
      NVE's loader doesn't read this folder.
   2. Copy the **`vihud`**, **`vihudfonts`** and **`vihudanims`** folders.
   3. Paste them into **`onigiri\dlcpacks`**.
5. NVE brings its own **`dlclist.xml`**, and the game uses that one. Add the HUD's packs to it:
   1. Open **`onigiri\common\data\dlclist.xml`** (Notepad is fine).
   2. Scroll to the **bottom**.
   3. Add these lines just above the closing **`</Paths>`**, each on its own line:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   - ⚠️ Not under `<Paths>` at the top: the game skips lines there.
   - Got a Lucia model? Add her pack's lines too.
   - Leave NVE's `gameconfig.xml` as it is.
6. **The loading screens.** You can skip this: you'll get the game's own loading screens. The installer put the
   HUD's loading screens in **`mods\update\update.rpf`**. NVE's loader doesn't read that either, so copy them out:
   1. Get **CodeWalker RPF Explorer** (version **30_dev48** or newer) from the releases channel of
      [dexyfex's Discord](https://discord.gg/BxfKHkk).
   2. In your game folder, open **`onigiri\platform\data`** and make a new folder there called **`cdimages`**.
   3. Open **CodeWalker RPF Explorer** and go into: `mods` → `update` → **`update.rpf`** → `x64` → `data` →
      `cdimages`.
   4. Right-click **`scaleform_frontend.rpf`** → **Extract Raw...**. Pick the **`onigiri\platform\data\cdimages`**
      folder you just made, and click **Save**.

   💡 That file is a copy of the game's menu screens with the HUD's loading screens in it.
   **After a game update,** delete it, install the HUD again, and do this step again.
   That way you get the updated game's menu screens.
7. Do Step 7 (Windowed Borderless), and play.

---

## Optional: Lively World Expansion (with NaturalVision Enhanced)

**What you get:** **Lively World Expansion** (LWE). It fills the map with more plants and props.
**What you need:** NVE and the HUD set up first. LWE's NVE package loads through NVE's onigiri too.

LWE's zip comes with its own **`dlclist.xml`**. That file **replaces** the one you already have.
LWE's copy leaves out:
- the HUD's packs
- three of NVE's newer packs (`NVE_lights`, `NVE_vegetation`, `NVE_vehicles_addon`)
- GTA's 2026 update

So add LWE's packs to your list instead:

1. Set up NVE and the HUD first ("Optional: NaturalVision Enhanced" above).
2. Copy LWE's packs:
   1. Open LWE's zip.
   2. Pick a density folder: **`LWE Enhanced - High Density`**, **`Medium Density`** or **`Low Density`**.
   3. Go into its **`onigiri\dlcpacks`** folder.
   4. Copy the **5 folders** in there into **`onigiri\dlcpacks`** in your game folder.
3. ⚠️ **Don't copy LWE's `onigiri\common\data\dlclist.xml`.** Use your own list instead:
   1. Open your own **`onigiri\common\data\dlclist.xml`**.
   2. Scroll to the bottom.
   3. Add these lines just above the closing **`</Paths>`** (not under `<Paths>` at the top), each on its own line.
      These are for High density:
   ```xml
   <Item>dlcpacks:/lwecityhigh/</Item>
   <Item>dlcpacks:/lwedeserthigh/</Item>
   <Item>dlcpacks:/lweforesthigh/</Item>
   <Item>dlcpacks:/lwemountainshigh/</Item>
   <Item>dlcpacks:/lweprops/</Item>
   ```
   For Medium or Low, change `high` to `medium` or `low` in the first 4 lines. `lweprops` stays the same.
4. **Already copied LWE's `dlclist.xml` over yours?** Put NVE's back:
   1. From NVE's zip, copy **`Main Package\onigiri\common\data\dlclist.xml`** into `onigiri\common\data` in your
      game folder.
   2. Add the HUD's lines (step 5 of the NVE section).
   3. Add LWE's lines (step 3 here).
5. Leave NVE's `gameconfig.xml` as it is, and play.

LWE's [installation page](https://livelyworldexpansion.com/installation/) has its own graphics tips.

---

## Optional: Central Real-Life Traffic and Environment (with NaturalVision Enhanced)

**What you get:** **Central Real-Life Traffic** and **Central Real-Life Environment** by **CentralMods**.
They fill Los Santos with real-life cars and real-life places.
**What you need:** NVE and the HUD set up first, and Central's **"Enhanced (for NVE)"** builds.
These load through NVE's onigiri.

They work with this HUD. But each one's **`Main Files`** comes with its own **`dlclist.xml`**. It **replaces**
yours, and that list leaves out the HUD's packs. So add those back once Central is in:

1. Set up NVE and the HUD first ("Optional: NaturalVision Enhanced" above).
2. **For Traffic:** install [ResourceExtensions](https://www.gta5-mods.com/scripts/resource-extensions) by **Koukol**:
   - Copy **`ResourceExtensions.asi`** and **`ResourceExtensions.ini`** into your game folder.
   - ⚠️ Central Traffic needs it. Without it, the game crashes right after loading.
3. Install Central the way its ReadMe says:
   1. Copy everything in its **`Main Files`** folder into your game folder.
   2. Click **Replace**.
   3. Using both? Do the same for the other Central mod.
4. **Using Lively World Expansion too?**
   1. Open Central's **`Optional`** folder.
   2. Open **`LWE Compatibility`**. It's `2. LWE Compatibility` in Traffic and `1. LWE Compatibility` in
      Environment. They're the same.
   3. Open the folder for your LWE density: **`High density`**, **`Medium density`** or **`Low density`**.
   4. Copy the `onigiri` folder from there into your game folder.
   5. Click **Replace**.
5. Add the HUD's lines back:
   1. Open **`onigiri\common\data\dlclist.xml`**.
   2. Scroll to the bottom.
   3. Add these lines just above the closing **`</Paths>`** (not under `<Paths>` at the top), each on its own line:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   Had other packs in your list, like Forests of San Andreas, GTA V Remastered or a Lucia model? Add their lines back too.
6. **Keep the `gameconfig.xml` Central Traffic put in `onigiri\common\data`.**
   - It's the game's own. ResourceExtensions raises the limits instead.
   - Don't put NVE's back.
   - Don't raise any pool sizes in it.

Good to know:
- **Tested** with Central Real-Life Traffic 2.9.1 and Environment 0.2 (both "Enhanced (for NVE)"), NVE, Lively World
  Expansion High and the HUD. Story Mode loads with every HUD script running.
- **Installing both?** They come with the same `dlclist.xml`, so the order doesn't matter. Add the HUD's lines once,
  after the second one.
- **Only using Environment?** Its list also leaves out NVE's car packs, because Central Traffic replaces those cars.
  To keep NVE's cars, add `<Item>dlcpacks:/NVE_vehicles_addon/</Item>` and `<Item>dlcpacks:/NVE_vehicles/</Item>`
  back as well.
- **To remove them**, follow Central's ReadMe. If you put NVE's `dlclist.xml` back, add the HUD's lines to it again.

---

## Optional: Forests of San Andreas and GTA V Remastered

**What you get:** **Forests of San Andreas** and **GTA V Remastered** by **Larcius** (built on JRod's and le__AK's maps).
They fill the map with tens of thousands of extra trees, bushes and props. They work with this HUD.
**What you need:** the HUD installed first.

Each mod's zip has a **`GTA V Enhanced`** folder with its packs. Each pack is a folder with a `dlc.rpf` inside:

| Pack folder | From |
| --- | --- |
| **`forest_n`** | Forests (north) |
| **`forest_s`** | Forests (south) |
| **`vremastered`** | Remastered |

Install the HUD first. Then pick your setup below.

### With NaturalVision Enhanced (no OpenRPF, no extra tools)

1. Copy the pack folders you want (`forest_n`, `forest_s`, `vremastered`) into **`onigiri\dlcpacks`**.
2. Open **`onigiri\common\data\dlclist.xml`**.
3. Add a line for each one just above `</Paths>`:
   ```xml
   <Item>dlcpacks:/forest_n/</Item>
   <Item>dlcpacks:/forest_s/</Item>
   <Item>dlcpacks:/vremastered/</Item>
   ```
4. Leave NVE's `gameconfig.xml` as it is.

### Without NVE (with OpenRPF, Step 4)

1. Copy the pack folders you want into **`mods\update\x64\dlcpacks`** in your game folder, next to the HUD's.
2. Get **CodeWalker RPF Explorer** (version 30_dev48 or newer, from the releases channel of
   [dexyfex's Discord](https://discord.gg/BxfKHkk)).
   You need it because their lines go in `mods\update\update.rpf\common\data\dlclist.xml`, which is inside an archive.
3. In CodeWalker RPF Explorer, click **File** → **Configure Game...**.
4. Pick your game folder.
5. Open `mods\update\update.rpf` → `common` → `data` → **`dlclist.xml`**.
6. Add the same lines as above, just above `</Paths>`.
7. Save.

Good to know:
- **Pick these or Lively World Expansion, not both.** They plant trees in the same countryside, so you'd get trees
  inside trees. Forests and Remastered are made to go together.
- **Reflections:** on Enhanced the mods can't bring their reflection shader. So car paint, puddles and wet roads can
  show boxes around trees and bushes. That doesn't happen if **Ray Traced Reflections** is set to **Very High** or **Ultra**.
- **To remove them:** take their lines out of `dlclist.xml` and delete the pack folders.

---

## Optional: AuraV-Lite (path tracing)

**What you get:** path tracing in GTA V Enhanced, from [AuraV-Lite](https://kyposmods.com) by **Kypos**.
It works alongside this HUD.
**What you need:** AuraV-Lite. Download it from Kypos's site.

⚠️ **Using NaturalVision Enhanced?** Follow "Optional: AuraV-Lite with NaturalVision Enhanced" below instead.

1. Copy **`AuraV.asi`**, **`msdmo.dll`** and **`nvngx_dlssd.dll`** into your **game folder**.
2. Copy the **`AuraV`** folder into your game folder as well.
3. In the game, go to **Settings** → **Graphics**.
4. Turn on **Ray Tracing** and **NVIDIA DLSS**. Path tracing needs both.
5. **Using RenoDX** (ReShade add-ons named `renodx-*.addon64`)? Opening AuraV's menu crashes the game while
   they're in your game folder. Move the `renodx-*.addon64` files out of the game folder while you use AuraV.
6. **Move AuraV's menu key off F10.** F10 is also the HUD's own key for reloading its settings.
   1. Open AuraV's menu once with **F10**.
   2. Change **"The key that opens and closes this menu"** to another key, like **F11**.

Good to know:
- Don't let **DLSS Swapper** replace GTA's DLSS files. Path tracing can freeze.
- Go easy on the heaviest settings on a weaker graphics card.
- If something goes wrong, `AuraV\AuraV.log` says why.

**To remove it:** delete those 3 files and the `AuraV` folder.

---

## Optional: AuraV-Lite with NaturalVision Enhanced

**What you get:** AuraV-Lite and **NaturalVision Enhanced** working together with this HUD.
**What you need:** NVE and the HUD set up, and AuraV's download.

Two things get in the way:
- AuraV's start-up shortcuts crash NVE's loader. (NVE already skips the game's start-up screens itself.)
- NVE comes with RenoDX's ReShade add-ons, which crash AuraV's menu.

Set it up in this order:

1. Set up NVE and the HUD first ("Optional: NaturalVision Enhanced" above).
2. Start the game once to check that the HUD shows.
3. Copy **`AuraV.asi`**, **`msdmo.dll`** and **`nvngx_dlssd.dll`** from AuraV's download into your **game folder**,
   next to `NVE.asi` and `onigiri.asi`.
4. Copy the **`AuraV`** folder into your game folder as well. ⚠️ Nothing of AuraV's goes in `onigiri`.
5. **Before you start the game**, make a text file called **`AuraV.ini`** in the **`AuraV`** folder, with just
   these lines:
   ```ini
   [boot]
   skip_legal=0
   cut_logos=0
   skip_menu=0
   story=0

   [AuraV]
   Key=F11
   ```
   - The `[boot]` lines switch AuraV's start-up shortcuts off, so NVE's loader doesn't crash.
   - `Key=F11` moves AuraV's menu off **F10**, the HUD's key for reloading its settings.
   - AuraV keeps these lines. It adds the rest of its settings to the file the first time it runs.
   - ⚠️ Make sure the file is called `AuraV.ini`, not `AuraV.ini.txt`
     (File Explorer: **View** → **Show** → **File name extensions**).
   - **Already started the game with AuraV?** Then `AuraV\AuraV.ini` is already there. Open it, set the `[boot]`
     lines to `0` and `Key` (under `[AuraV]`) to `F11`, and save.
6. **Move RenoDX's add-ons out.** NVE puts `renodx-*.addon64` files in your game folder. AuraV's menu crashes the
   game while they're there.
   1. Move them to a folder outside the game folder.
   2. Keep them there, in case you remove AuraV later.
   3. Leave NVE's own ReShade (`ReShade.asi`, with NVE's look in `NVE.ini`) where it is. It works with AuraV.
7. In the game, go to **Settings** → **Graphics**.
8. Turn on **Ray Tracing** and **NVIDIA DLSS**. Path tracing needs both.
9. Keep **Windowed Borderless** (Step 7).
10. Play. **F11** opens AuraV's menu.

Good to know:
- **Tested** with AuraV-Lite 0.2.7, NVE and the HUD. Story Mode loads with path tracing on and the HUD showing.
- **Is AuraV running?** `asiloader.log` in your game folder lists `AuraV.asi`. `AuraV\AuraV.log` says "in the
  world" once Story Mode has loaded. If the game crashes, that log says why.
- The tips under "Optional: AuraV-Lite" above (DLSS Swapper, weaker graphics cards) apply here too.
- **To remove it:** delete AuraV's 3 files and the `AuraV` folder. Put RenoDX's add-ons back in the game folder.

---

## Optional: LSPDFR

**LSPDFR** (play as a police officer) is made for **Legacy**.
LSPDFR for Enhanced is still an early preview. We haven't tested the HUD with it.

Trying it anyway? The **[Legacy guide's LSPDFR part](INSTALL-LEGACY.md#optional-lspdfr)** says:

- what the HUD does on duty (you don't need to change anything)
- which keys do two things at once, and how to change them

---

## Something went wrong?

Find what happens to you in the left column. Do what the right column says.

**Still stuck?** Ask in **[our support Discord](https://discord.gg/C7gE5SHEzH)**, the one for this version. Or
[open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| What happens | What to do |
| --- | --- |
| The game **closes by itself about 20 seconds after loading** | You have the normal Script Hook V .NET.<br>1. Redo Step 3 with **SHVDNE**.<br>2. Delete the **`dl3`** folder in `%LOCALAPPDATA%\assembly` (a cache of the old one).<br>**To check which one you have:** in your game folder, right-click **`ScriptHookVDotNet.asi`** → **Properties** → **Details**. SHVDNE says **"Script Hook V .Net Enhanced"** there. The normal one doesn't (its version is 3.6.0.0).<br>**Still happening?** A ReShade add-on (a file ending in `.addon64` in your game folder) may be clashing. Move those files somewhere else and try again. |
| Your game folder has **`MiniMap.asi`** or **`MiniMapHudPresent.asi`** (from another 3D minimap mod or an older install) | 1. Delete it.<br>2. Delete `scripts\MiniMap.dll` too, if it's there. (The HUD's is `scripts\ViMinimap.dll`.)<br>3. Keep `ViMinimap.asi` next to `GTA5_Enhanced.exe`.<br>It's an older copy of the minimap plugin the HUD brings itself as `ViMinimap.asi`. Two copies loaded at once get in each other's way. |
| The game **crashes when you open AuraV-Lite's menu** | You have RenoDX's ReShade add-ons (`renodx-*.addon64`) in your game folder. NVE comes with them.<br>Move them out while you use AuraV.<br>See "Optional: AuraV-Lite" above, or "Optional: AuraV-Lite with NaturalVision Enhanced". |
| A **"Debug Assertion Failed!"** box from Microsoft Visual C++ names **`renodx-dlssfix.addon64`** | That's RenoDX's DLSS fix add-on, not the HUD. It's a debug build of it.<br>Don't use DLSS 5? Then you don't need it: delete `renodx-dlssfix.addon64` from your game folder.<br>`renodx-gtav-enhanced.addon64` can stay.<br>You don't need to reinstall Visual C++ for this one. |
| You use **AuraV-Lite** with **NaturalVision Enhanced**, and the game **crashes at start-up** | 1. In `AuraV\AuraV.ini`, under `[boot]`, set `skip_legal`, `cut_logos`, `skip_menu` and `story` to `0`.<br>2. Start the game again.<br>See "Optional: AuraV-Lite with NaturalVision Enhanced" above. |
| You use **NaturalVision Enhanced**, and the game **crashes while loading Story Mode** or **the HUD doesn't show** | Follow "Optional: NaturalVision Enhanced" above:<br>• no OpenRPF<br>• the HUD's packs copied into `onigiri\dlcpacks`<br>• NVE's `dlclist.xml` listing them<br>NVE brings its own `dlclist.xml` and `gameconfig.xml` (in `onigiri\common\data`). The game uses those instead of the ones the HUD's package set up.<br>Open NVE's `dlclist.xml`. Add these lines just above `</Paths>`, each on its own line:<br>`<Item>dlcpacks:/vihud/</Item>`<br>`<Item>dlcpacks:/vihudfonts/</Item>`<br>`<Item>dlcpacks:/vihudanims/</Item>`<br>⚠️ Leave NVE's `gameconfig.xml` as NVE ships it. Raising its pool sizes any further makes the game run out of memory while loading. |
| You use **NaturalVision Enhanced** and the **loading screens are the game's own**, not the HUD's GTA VI ones | Do step 6 of "Optional: NaturalVision Enhanced" above.<br>NVE's loader doesn't read the `mods` folder. So the loading screens have to be copied into `onigiri\platform\data\cdimages`. |
| You use **NaturalVision Enhanced**, and **since a game update** the game's menus or loading screens look wrong | 1. Delete `onigiri\platform\data\cdimages\scaleform_frontend.rpf`. It's a copy of the game's menu screens from before the update.<br>2. Install the HUD again.<br>3. Do step 6 of "Optional: NaturalVision Enhanced" again. |
| You installed **Lively World Expansion** (LWE), and now **parts of the HUD are missing** | LWE's `dlclist.xml` replaced NVE's. It doesn't list the HUD's packs (or some of NVE's own).<br>1. Put NVE's `dlclist.xml` back.<br>2. Add LWE's lines and the HUD's lines to it.<br>See "Optional: Lively World Expansion" above. |
| You installed **Central Real-Life Traffic** or **Environment**, and now **parts of the HUD are missing** | Central's `dlclist.xml` replaced yours. It doesn't list the HUD's packs.<br>Add the HUD's lines back to it: see "Optional: Central Real-Life Traffic and Environment" above. |
| The game **crashes right after the loading screen** since you added **Central Real-Life Traffic** | 1. Install **ResourceExtensions** (Central needs it).<br>2. Keep the stock `gameconfig.xml` Central put in `onigiri\common\data`.<br>ResourceExtensions doesn't support a modded one, NVE's included. |
| You added **Forests of San Andreas** or **GTA V Remastered**, and **no new trees show** | Their packs need to be where your game loads packs from:<br>• **With NaturalVision Enhanced:** in `onigiri\dlcpacks`, with their lines in `onigiri\common\data\dlclist.xml`.<br>• **Without it:** in `mods\update\x64\dlcpacks`, with their lines in `mods\update\update.rpf`'s `dlclist.xml`.<br>See "Optional: Forests of San Andreas and GTA V Remastered" above. |
| A Script Hook V message says **"unknown game version"** | GTA V just updated, and Script Hook V hasn't caught up yet.<br>1. Wait for a new Script Hook V.<br>2. Redo Step 2. |
| The game starts but **the new HUD doesn't show** | Check that the 5 SHVDNE files from Step 3 are next to `GTA5_Enhanced.exe`. |
| You **updated** the mod, but the game still acts like the **old version** | Script Hook V .NET can keep running its saved copy of the old scripts.<br>1. Close the game.<br>2. Press **Win + R**.<br>3. Type `%LOCALAPPDATA%\assembly` and press Enter.<br>4. Delete the **`dl3`** folder. It's only a cache.<br>**Still the old version?** Install it again with the OIV Package Installer run as administrator (Step 6). Or copy the scripts in yourself: see "Updating by hand" below. |
| **No minimap** in a car | Set the game to **Windowed Borderless** (Step 7). |
| An error about **.NET** or **Visual C++** | 1. Install [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer).<br>2. Install the [Visual C++ Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe).<br>3. Restart your PC. |
| A message says **Lucia is not registered** | Lucia isn't included with the HUD any more (see Step 5).<br>**Don't have her?** Set `Enabled = False` under `[Lucia]` in `scripts\GTA6Hud.ini`. The message goes away.<br>**Have a Lucia model?** Her pack is missing from `dlclist.xml` (with **NaturalVision Enhanced**, from NVE's). Add its line.<br>If the message mentions a `VIHudSetup.exe`, ignore that part: this mod doesn't use it. |
| **Switching characters crashes**, and your game isn't in English | Players report this only happens when the game is set to another language.<br>Until it's fixed: set GTA V's language to **English** in your game launcher's settings for GTA V. |
| The HUD says your game **won't save in its folder**, and a **Windows prompt** asks to let **icacls** make changes | Your game is in **Program Files**, where Windows lets only administrators change files. So the HUD can't save its settings (and ReShade can't save its own).<br>1. Click **Yes**. It lets your Windows user change the game folder. That's the same as doing it by hand in the folder's **Properties** → **Security**.<br>2. Restart the game once.<br>**Clicked No?** You're asked again the next time you start the game.<br>`ViExtras.ini` `[GameFolder] FixAccess = False` stops the question. |
| **Something else** | Ask in the **[support Discord](https://discord.gg/C7gE5SHEzH)**.<br>Or [open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) and attach these files from your game folder:<br>• `ScriptHookVDotNet.log`<br>• `scripts\GTA6Hud.perf.log`<br>• `scripts\GTA6Hud.error.log` |

---

## Updating by hand (game in Program Files)

**When you need this:** your game is in **Program Files** (where the Rockstar Games Launcher and Steam put it).
Windows guards that folder. Unless you run the OIV Package Installer as administrator (Step 6), an update through
it doesn't always replace the old files.

You can also copy the new scripts in yourself. An `.oiv` is just a zip.

💡 From 2.6, the HUD offers to fix the folder for you the first time it runs. See the Windows prompt row above.
After that, updates go in normally.

1. Close the game.
2. Copy **`GTA-VI-Inspired-HUD-2.6.4-Enhanced.oiv`** to your Desktop.
3. Rename the copy to **`.zip`**.
   Can't see the `.oiv` part of the name? In File Explorer, turn on **View** → **Show** → **File name extensions**.
4. Open the zip → **`content`** → **`scripts`**.
5. Select everything in there and copy it.
6. Paste it into the **`scripts`** folder in your game folder. For example:
   `C:\Program Files\Rockstar Games\Grand Theft Auto V Enhanced\scripts`.
7. Pick **Replace the files in the destination**.
8. Click **Continue** when Windows asks for admin permission.
9. Clear Script Hook V .NET's cache, so it loads the new files:
   1. Press **Win + R**.
   2. Type `%LOCALAPPDATA%\assembly` and press Enter.
   3. Delete the **`dl3`** folder. It's only a cache. It's made again the next time you start the game.
10. Coming from 2.5.4 or older? 2.5.5 adds a pack:
    1. Copy the zip's **`content`** → **`dlcpacks`** → **`vihud`** folder into **`mods\update\x64\dlcpacks`** in
       your game folder.
    2. Install the mod once with the OIV Package Installer, as in Step 6. That switches the new pack on.
11. Start the game. `scripts\ViExtras.log` now begins with a `==== ViExtras …` line.
    It shows the version you downloaded.

---

## How to remove the mod

### The easiest way

1. Open the **OIV Package Installer**.
2. Go to **Manage Mods**.
3. Uninstall both packages.

### Or by hand

1. In your **game folder**, delete **`ViMinimap.asi`**.
2. Open the **`scripts`** folder.
3. Delete these:
   - **`GTA6Hud.dll`**
   - **`GTA6Hud.ini`**
   - **`ViMinimap.dll`**
   - **`LemonUI.SHVDN3.dll`**
   - the **`MiniMap.*`** files
   - the **`Hud`** folder
4. Delete the **`mods`** folder. ⚠️ This also removes any other mods in it.
5. With NaturalVision Enhanced, also delete:
   - **`onigiri\platform\data\cdimages\scaleform_frontend.rpf`** (the HUD's loading screens)
   - the HUD's packs in `onigiri\dlcpacks`

✅ Your game is back to normal.

Script Hook V, SHVDNE and OpenRPF can stay. They don't change anything on their own.
