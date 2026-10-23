# How to install: GTA V **Enhanced**

This guide is for the **2025 PC version** of GTA V, the one whose game folder has a file called
**`GTA5_Enhanced.exe`**. If yours has **`GTA5.exe`** instead, you have Legacy. Use the
**[Legacy guide](INSTALL-LEGACY.md)**.

Never modded GTA before? No problem. Follow the steps in order and don't skip any. It takes about 20 minutes.

> [!IMPORTANT]
> ### 💬 Need help? Join our support Discord
> This version is a **fork of FBICat's VI HUD** with extra features, so it has its own support:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, or a [GitHub issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat's Discord is there for his original mod, so please bring questions about this version to ours. Thank you!

> ⚠️ **Mods are for Story Mode only.** Never go into GTA Online with mods installed: you can get banned.
> (Script Hook V, one of the tools below, closes the game if you try.)

> ⚠️ **Enhanced uses different tools than Legacy.** Only use the downloads linked in this guide. Legacy
> tools and Legacy mods make Enhanced crash.

---

## Before you start: two quick Windows settings

**1. Show file extensions.** Some files in this guide end in `.asi` or `.dll`, and Windows hides those
endings by default. Open any folder, click **View** at the top, then **Show** → **File name extensions**.

**2. Know how to unzip.** Most downloads are `.zip` files (a folder squeezed into one file). To open
one, right-click it, choose **Extract All…**, then click **Extract**. A normal folder appears next to it.

---

## Step 1: Find your GTA V Enhanced folder

This is the folder with **`GTA5_Enhanced.exe`** inside. We'll call it your **game folder**. How to open it:

- **Rockstar Games Launcher:** open **Settings**, click **Grand Theft Auto V Enhanced** on the left,
  then **Open Folder** (or **View Installation Folder**).
- **Steam:** in your **Library**, right-click **Grand Theft Auto V Enhanced**, then **Manage** → **Browse local files**.
- **Epic Games:** in your **Library**, click the **⋯** under GTA V Enhanced, then **Manage**, then the
  **folder icon** next to "Installation".

✅ Keep this folder open. You'll copy files into it a few times.

---

## Step 2: Install Script Hook V (lets mods run)

1. Go to the [Script Hook V page](http://www.dev-c.com/gtav/scripthookv/) and click **Download**. The
   same download works for Legacy and Enhanced.
2. Unzip it and open the **`bin`** folder inside.
3. Copy these files into your **game folder**:
   - `ScriptHookV.dll`
   - `xinput1_4.dll` (loads mods ending in `.asi` on Enhanced)
   - `args.txt` (turns off BattlEye, Rockstar's anti-cheat, which blocks mods)
   - `dinput8.dll` (the mod loader for Legacy; Enhanced doesn't use it, so copying it does no harm)
4. **Don't** copy `NativeTrainer.asi` (a cheat menu you don't need).

✅ Your game folder now has `ScriptHookV.dll` and `xinput1_4.dll` next to `GTA5_Enhanced.exe`.

---

## Step 3: Install Script Hook V .NET **Enhanced** (lets this HUD run)

⚠️ This is **not** the normal Script Hook V .NET. The normal one makes Enhanced close by itself about
20 seconds after you load in. You need the **Enhanced** edition, called **SHVDNE**.

1. Go to the [SHVDNE releases page](https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/latest).
2. Under **Assets**, download the **`.zip`** file.
3. Unzip it. Copy the **5 files** from its main folder into your **game folder**:
   - `ScriptHookVDotNet.asi`
   - `ScriptHookVDotNet.ini`
   - `ScriptHookVDotNet2.dll`
   - `ScriptHookVDotNet3.dll`
   - `MinHook.x64.dll`
4. Skip the `Debug`, `Docs` and `Licenses` folders and `README.txt`. You don't need them.

If Windows asks to replace files (you had the normal version before), click **Replace**.

✅ Those 5 files are now next to `GTA5_Enhanced.exe`.

---

## Step 4: Install OpenRPF (keeps the mod in a safe `mods` folder)

OpenRPF lets the game load mods from a separate **`mods` folder**, so your original game files are
never changed.

1. Go to the [OpenRPF page](https://www.gta5-mods.com/tools/openrpf-openiv-asi-for-gta-v-enhanced) and
   download it.
2. Unzip it and copy **`OpenRPF.asi`** into your **game folder**.
3. If the download also has a `dsound.dll`, **don't copy it**. Script Hook V already gave you a mod
   loader (`xinput1_4.dll` from Step 2). Two loaders at once cause crashes.
4. In your game folder, create a new folder called **`mods`** if there isn't one already
   (right-click → **New** → **Folder**).

✅ `OpenRPF.asi` is next to `GTA5_Enhanced.exe`, and there's a `mods` folder.

---

## Step 5: Download the mod and the installer

**The mod.** Go to the [**Releases page**](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/releases/latest).
Under **Assets**, download **`GTA-VI-Inspired-HUD-2.6.2-Enhanced.oiv`** (the mod, about 100 MB). It also switches
the mod on in the game, so there's no separate patch to get.

**Want to play as Lucia?** She isn't included: her model is someone else's paid work. Get her from her creator,
**[Lucia Caminos on Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256)** or **[MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v)** on gta5-mods.com, or use another Lucia
model. For Jason, get **[Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433)** from the same creator, or **[Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en)** on mod-hub.net (see "Optional: Jason" below to put him in the game). The HUD works
without her.

❌ Don't download `GTA-VI-Inspired-HUD-2.6.2-Legacy.oiv`. It's for the other version and will crash Enhanced.

💡 The number in the file name is the version (2.6.2 here), so you can always tell which one you installed. Mention it when you ask for help.

**The installer.** OpenIV doesn't install mods on Enhanced, so you'll use the **OIV Package Installer**
instead. Download it from [its gta5-mods page](https://www.gta5-mods.com/tools/oiv-package-installer)
and unzip it anywhere, like your Desktop.

---

## Step 6: Install the mod

⚠️ **Game in Program Files?** If your game folder's path starts with `C:\Program Files` (the Rockstar Games
Launcher, Epic Games and Steam put it there), right-click **`CodeWalker.OIVInstaller.exe`** → **Run as administrator** and click
**Yes**. Windows lets only administrators change files there, and without it the install can leave the old files
in place.

1. Open the OIV Package Installer folder and run **`CodeWalker.OIVInstaller.exe`**.
2. Drag **`GTA-VI-Inspired-HUD-2.6.2-Enhanced.oiv`** onto its window (or click **Browse** and pick it).
3. When it asks for your game folder, pick the folder with **`GTA5_Enhanced.exe`**.
4. Click **Install** and wait until it's finished.

✅ Installing the mod again, or over an older version, is fine: it never lists a pack twice.

---

## Step 7: Change one game setting

Start the game, go to **Settings** → **Graphics**, and set **Screen Type** to **Windowed Borderless**.

On Enhanced, the minimap is drawn in a little window on top of the game, and that window only shows in
Borderless mode.

---

## Step 8: Play! 🎉

Load **Story Mode**. The new HUD appears on its own.

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
2. **Convert his files for Enhanced.** You need **CodeWalker RPF Explorer** (version 30_dev48 or newer, from the
   releases channel of [dexyfex's Discord](https://discord.gg/BxfKHkk)). Open **Tools** → **Asset Converter** ("Convert
   files from GTAV Legacy formats to GTAV Enhanced formats"), pick the download's **`No Cap Version`** folder (or
   **`Cap Version`**, for Jason in his cap) and a new, empty folder for the converted files, then click **Process**.
   If `jdfinal.ymt` isn't in the new folder, copy it over as it is.
3. Copy the kit's **`jasonped`** folder into your game folder: into **`onigiri\dlcpacks`** with NaturalVision Enhanced,
   or into **`mods\update\x64\dlcpacks`** without it.
4. In CodeWalker RPF Explorer, switch on **Edit mode** (**Edit** → **Edit mode**), open **`jasonped\dlc.rpf`** → `x64` →
   `models` → `cdimages` → **`jasonped.rpf`**, and drag in the 4 converted files.
5. Add this line just above the closing `</Paths>` at the bottom of your `dlclist.xml`, and save:
   ```xml
   <Item>dlcpacks:/jasonped/</Item>
   ```
   With NaturalVision Enhanced that's **`onigiri\common\data\dlclist.xml`** (Notepad is fine). Without it, it's
   `mods\update\update.rpf` → `common` → `data` → `dlclist.xml`, which you edit in CodeWalker RPF Explorer.
6. Start the game and hold **Left Alt**: Jason's card is next to Lucia's.

**A different Jason model?** The HUD looks for the ped named under `[Jason]` in `scripts\GTA6Hud.ini`
(`Model = jdfinal`), so another Jason works too:
- **Its files have another name** (say `jason.ydd`)? Rename them all to `jdfinal`, keeping their endings, and nothing
  else changes. Or keep the names and change `jdfinal` to that name in two places: in the kit's `setup\peds.meta`
  (`<Name>jdfinal</Name>`, in Notepad), which you then drag into the pack's `common\data` in CodeWalker RPF Explorer to replace the
  one there, and in `Model =` under `[Jason]` in `scripts\GTA6Hud.ini`.
- **It's made of many files** (`jason^head_000_r.ydd`, `jason^uppr_000_u.ydd` and so on)? Put them all in
  `jasonped.rpf`, and use the name before the `^` (here `jason`) in those two places.
- **It comes as a pack of its own** (it has its own `dlc.rpf`)? You don't need the kit: install that pack the way its
  readme says, then set `Model =` under `[Jason]` in `scripts\GTA6Hud.ini` to its ped's name (the `<Name>` in its
  `peds.meta`, or its `.ydd` file's name).

**Jason not on the wheel?** `scripts\ViSwitch.log` says **"Jason's model is in the game"** once the HUD has found him,
or **"no Jason model in the game"** when it hasn't: the pack isn't in `dlclist.xml`, or the names don't match.

---

## Optional: NaturalVision Enhanced

**NaturalVision Enhanced** (NVE) by **Razed** works with this HUD. NVE loads mods with its own loader (onigiri),
so a few steps go differently:

1. Do Steps 1 to 3 of this guide (Script Hook V and SHVDNE). **Skip Step 4: don't install OpenRPF.** NVE warns
   about `OpenRPF.asi` when the game starts; if you already copied it, delete it from your game folder.
2. Install NVE: open its zip, go into the **`Main Package`** folder and copy everything inside it into your
   **game folder** (NVE's own `README Install Guide.html` has the details). If Windows asks to replace
   `xinput1_4.dll`, click **Replace**: it's the same file Script Hook V uses.
3. Install the HUD with Steps 5 and 6.
4. The installer puts the HUD's packs in **`mods\update\x64\dlcpacks`**, which NVE's loader doesn't read. Copy
   the **`vihud`**, **`vihudfonts`** and **`vihudanims`** folders from there into **`onigiri\dlcpacks`**.
5. NVE brings its own **`dlclist.xml`**, and the game uses that one. Open **`onigiri\common\data\dlclist.xml`**
   (Notepad is fine), scroll to the **bottom**, and add these lines just above the closing **`</Paths>`**, each on
   its own line. Not under `<Paths>` at the top: the game skips lines there.
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   Got a Lucia model? Add her pack's lines too. Leave NVE's `gameconfig.xml` as it is.
6. Do Step 7 (Windowed Borderless), and play.

---

## Optional: Lively World Expansion (with NaturalVision Enhanced)

**Lively World Expansion** (LWE) fills the map with more plants and props, and its NVE package loads through NVE's
onigiri too. Its zip comes with its own **`dlclist.xml`**, though, and that file **replaces** the one you already
have. LWE's copy leaves out the HUD's packs, three of NVE's newer packs (`NVE_lights`, `NVE_vegetation`,
`NVE_vehicles_addon`) and GTA's 2026 update. So add LWE's packs to your list instead:

1. Set up NVE and the HUD first ("Optional: NaturalVision Enhanced" above).
2. Open LWE's zip and pick a density folder: **`LWE Enhanced - High Density`**, **`Medium Density`** or
   **`Low Density`**. Go into its **`onigiri\dlcpacks`** folder and copy the **5 folders** in there into
   **`onigiri\dlcpacks`** in your game folder.
3. **Don't copy LWE's `onigiri\common\data\dlclist.xml`.** Open your own **`onigiri\common\data\dlclist.xml`** instead,
   scroll to the bottom, and add these lines just above the closing **`</Paths>`** (not under `<Paths>` at the top),
   each on its own line (these are for High density):
   ```xml
   <Item>dlcpacks:/lwecityhigh/</Item>
   <Item>dlcpacks:/lwedeserthigh/</Item>
   <Item>dlcpacks:/lweforesthigh/</Item>
   <Item>dlcpacks:/lwemountainshigh/</Item>
   <Item>dlcpacks:/lweprops/</Item>
   ```
   For Medium or Low, change `high` to `medium` or `low` in the first 4 lines. `lweprops` stays the same.
4. **Already copied LWE's `dlclist.xml` over yours?** Put NVE's back: from NVE's zip, copy
   **`Main Package\onigiri\common\data\dlclist.xml`** into `onigiri\common\data` in your game folder. Then add
   the HUD's lines (step 5 of the NVE section) and LWE's lines (step 3 here).
5. Leave NVE's `gameconfig.xml` as it is, and play.

LWE's [installation page](https://livelyworldexpansion.com/installation/) has its own graphics tips.

---

## Optional: Central Real-Life Traffic and Environment (with NaturalVision Enhanced)

**Central Real-Life Traffic** and **Central Real-Life Environment** by **CentralMods** fill Los Santos with real-life
cars and real-life places, and their **"Enhanced (for NVE)"** builds load through NVE's onigiri. They work with this
HUD, but each one's **`Main Files`** comes with its own **`dlclist.xml`** that **replaces** yours, and that list
leaves out the HUD's packs. So add those back once Central is in:

1. Set up NVE and the HUD first ("Optional: NaturalVision Enhanced" above).
2. **For Traffic:** install [ResourceExtensions](https://www.gta5-mods.com/scripts/resource-extensions) by **Koukol**
   (copy `ResourceExtensions.asi` and `ResourceExtensions.ini` into your game folder). Central Traffic needs it:
   without it the game crashes right after loading.
3. Install Central the way its ReadMe says: copy everything in its **`Main Files`** folder into your game folder
   and click **Replace**. Do the same for the other Central mod if you use both.
4. **Using Lively World Expansion too?** Open Central's **`Optional`** folder, then **`LWE Compatibility`** (it's
   `2. LWE Compatibility` in Traffic and `1. LWE Compatibility` in Environment, and they're the same), then the
   folder for your LWE density: **`High density`**, **`Medium density`** or **`Low density`**. Copy the `onigiri`
   folder from there into your game folder and click **Replace**.
5. Open **`onigiri\common\data\dlclist.xml`**, scroll to the bottom, and add the HUD's lines back just above the
   closing **`</Paths>`** (not under `<Paths>` at the top), each on its own line:
   ```xml
   <Item>dlcpacks:/vihud/</Item>
   <Item>dlcpacks:/vihudfonts/</Item>
   <Item>dlcpacks:/vihudanims/</Item>
   ```
   Had other packs in your list, like Forests of San Andreas, GTA V Remastered or a Lucia model? Add their lines back too.
6. **Keep the `gameconfig.xml` Central Traffic put in `onigiri\common\data`.** It's the game's own, and
   ResourceExtensions raises the limits instead. Don't put NVE's back, and don't raise any pool sizes in it.

Good to know:
- **Tested** with Central Real-Life Traffic 2.9.1 and Environment 0.2 (both "Enhanced (for NVE)"), NVE, Lively World
  Expansion High and the HUD: Story Mode loads with every HUD script running.
- **Installing both?** They come with the same `dlclist.xml`, so the order doesn't matter. Add the HUD's lines once,
  after the second one.
- **Only using Environment?** Its list also leaves out NVE's car packs, because Central Traffic replaces those cars.
  To keep NVE's cars, add `<Item>dlcpacks:/NVE_vehicles_addon/</Item>` and `<Item>dlcpacks:/NVE_vehicles/</Item>`
  back as well.
- **To remove them**, follow Central's ReadMe. If you put NVE's `dlclist.xml` back, add the HUD's lines to it again.

---

## Optional: Forests of San Andreas and GTA V Remastered

**Forests of San Andreas** and **GTA V Remastered** by **Larcius** (built on JRod's and le__AK's maps) fill the map
with tens of thousands of extra trees, bushes and props, and they work with this HUD. Each mod's zip has a
**`GTA V Enhanced`** folder with its packs: **`forest_n`** and **`forest_s`** (Forests, north and south) and
**`vremastered`** (Remastered), each a folder with a `dlc.rpf` inside. Install the HUD first, then:

**With NaturalVision Enhanced** (no OpenRPF, no extra tools):

1. Copy the pack folders you want (`forest_n`, `forest_s`, `vremastered`) into **`onigiri\dlcpacks`**.
2. Open **`onigiri\common\data\dlclist.xml`** and add a line for each one just above `</Paths>`:
   ```xml
   <Item>dlcpacks:/forest_n/</Item>
   <Item>dlcpacks:/forest_s/</Item>
   <Item>dlcpacks:/vremastered/</Item>
   ```
3. Leave NVE's `gameconfig.xml` as it is.

**Without NVE** (with OpenRPF, Step 4):

1. Copy the pack folders you want into **`mods\update\x64\dlcpacks`** in your game folder, next to the HUD's.
2. Their lines go in `mods\update\update.rpf\common\data\dlclist.xml`, which is inside an archive, so you need
   **CodeWalker RPF Explorer** (version 30_dev48 or newer, from the releases channel of
   [dexyfex's Discord](https://discord.gg/BxfKHkk)). In it, click **File** → **Configure Game...** and pick your game
   folder, open `mods\update\update.rpf` → `common` → `data` → `dlclist.xml`, add the same lines as above just
   above `</Paths>`, and save.

Good to know:
- **Pick these or Lively World Expansion, not both.** They plant trees in the same countryside, so you'd get trees
  inside trees. Forests and Remastered are made to go together.
- **Reflections:** on Enhanced the mods can't bring their reflection shader, so car paint, puddles and wet roads can
  show boxes around trees and bushes, unless **Ray Traced Reflections** is set to **Very High** or **Ultra**.
- **To remove them:** take their lines out of `dlclist.xml` and delete the pack folders.

---

## Optional: AuraV-Lite (path tracing)

[AuraV-Lite](https://kyposmods.com) by **Kypos** adds path tracing to GTA V Enhanced, and it works alongside this
HUD. Download it from Kypos's site. **Using NaturalVision Enhanced?** Follow "Optional: AuraV-Lite with
NaturalVision Enhanced" below instead.

1. Copy **`AuraV.asi`**, **`msdmo.dll`** and **`nvngx_dlssd.dll`** into your **game folder**, and the **`AuraV`**
   folder as well.
2. In the game, go to **Settings** → **Graphics** and turn on **Ray Tracing** and **NVIDIA DLSS**. Path tracing
   needs both.
3. **Using RenoDX** (ReShade add-ons named `renodx-*.addon64`)? Opening AuraV's menu crashes the game while
   they're in your game folder. Move the `renodx-*.addon64` files out of the game folder while you use AuraV.
4. **Move AuraV's menu key off F10.** F10 is also the HUD's own key for reloading its settings. Open AuraV's menu
   once with F10, and change **"The key that opens and closes this menu"** to another key, like **F11**.

Good to know: don't let **DLSS Swapper** replace GTA's DLSS files (path tracing can freeze), and go easy on the
heaviest settings on a weaker graphics card. If something goes wrong, `AuraV\AuraV.log` says why.
**To remove it:** delete those 3 files and the `AuraV` folder.

---

## Optional: AuraV-Lite with NaturalVision Enhanced

AuraV-Lite and **NaturalVision Enhanced** work together with this HUD, but two things get in the way: AuraV's
start-up shortcuts crash NVE's loader (NVE already skips the game's start-up screens itself), and NVE comes with
RenoDX's ReShade add-ons, which crash AuraV's menu. Set it up in this order:

1. Set up NVE and the HUD first ("Optional: NaturalVision Enhanced" above), and start the game once to check that
   the HUD shows.
2. Copy **`AuraV.asi`**, **`msdmo.dll`** and **`nvngx_dlssd.dll`** from AuraV's download into your **game folder**,
   next to `NVE.asi` and `onigiri.asi`, and the **`AuraV`** folder as well. Nothing of AuraV's goes in `onigiri`.
3. **Before you start the game**, make a text file called **`AuraV.ini`** in the **`AuraV`** folder, with just
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
   The `[boot]` lines switch AuraV's start-up shortcuts off, so NVE's loader doesn't crash. `Key=F11` moves AuraV's
   menu off **F10**, the HUD's key for reloading its settings. AuraV keeps these lines and adds the rest of its
   settings to the file the first time it runs. Make sure the file is called `AuraV.ini`, not `AuraV.ini.txt`
   (File Explorer: **View** → **Show** → **File name extensions**).
   Already started the game with AuraV? Then `AuraV\AuraV.ini` is already there: open it, set the `[boot]` lines
   to `0` and `Key` (under `[AuraV]`) to `F11`, and save.
4. **Move RenoDX's add-ons out.** NVE puts `renodx-*.addon64` files in your game folder, and AuraV's menu crashes
   the game while they're there. Move them to a folder outside the game folder, and keep them in case you remove
   AuraV later. NVE's own ReShade (`ReShade.asi`, with NVE's look in `NVE.ini`) stays: it works with AuraV.
5. In the game, go to **Settings** → **Graphics** and turn on **Ray Tracing** and **NVIDIA DLSS** (path tracing
   needs both), and keep **Windowed Borderless** (Step 7).
6. Play. **F11** opens AuraV's menu.

Good to know:
- **Tested** with AuraV-Lite 0.2.7, NVE and the HUD: Story Mode loads with path tracing on and the HUD showing.
- **Is AuraV running?** `asiloader.log` in your game folder lists `AuraV.asi`, and `AuraV\AuraV.log` says "in the
  world" once Story Mode has loaded. If the game crashes, that log says why.
- The tips under "Optional: AuraV-Lite" above (DLSS Swapper, weaker graphics cards) apply here too.
- **To remove it:** delete AuraV's 3 files and the `AuraV` folder, and put RenoDX's add-ons back in the game folder.

---

## Something went wrong?

**Still stuck?** Ask in **[our support Discord](https://discord.gg/C7gE5SHEzH)**, the one for this version, or
[open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).

| What happens | What to do |
| --- | --- |
| The game **closes by itself about 20 seconds after loading** | You have the normal Script Hook V .NET. Redo Step 3 with **SHVDNE**, then delete the `dl3` folder in `%LOCALAPPDATA%\assembly` (a cache of the old one). **To check which one you have:** in your game folder, right-click **`ScriptHookVDotNet.asi`** → **Properties** → **Details**. SHVDNE says **"Script Hook V .Net Enhanced"** there; the normal one doesn't (its version is 3.6.0.0). Still happening? A ReShade add-on (a file ending in `.addon64` in your game folder) may be clashing. Move those files somewhere else and try again. |
| Your game folder has **`MiniMap.asi`** or **`MiniMapHudPresent.asi`** (from another 3D minimap mod or an older install) | Delete it. It's an older copy of the minimap plugin the HUD brings itself as `ViMinimap.asi`, and two copies loaded at once get in each other's way. Delete `scripts\MiniMap.dll` too if it's there (the HUD's is `scripts\ViMinimap.dll`). Keep `ViMinimap.asi` next to `GTA5_Enhanced.exe`. |
| The game **crashes when you open AuraV-Lite's menu** | You have RenoDX's ReShade add-ons (`renodx-*.addon64`, NVE comes with them) in your game folder. Move them out while you use AuraV (see "Optional: AuraV-Lite" above, or "Optional: AuraV-Lite with NaturalVision Enhanced"). |
| A **"Debug Assertion Failed!"** box from Microsoft Visual C++ names **`renodx-dlssfix.addon64`** | That's RenoDX's DLSS fix add-on, not the HUD, and it's a debug build of it. If you don't use DLSS 5, you don't need it: delete `renodx-dlssfix.addon64` from your game folder (`renodx-gtav-enhanced.addon64` can stay). You don't need to reinstall Visual C++ for this one. |
| You use **AuraV-Lite** with **NaturalVision Enhanced**, and the game **crashes at start-up** | Set `skip_legal`, `cut_logos`, `skip_menu` and `story` to `0` under `[boot]` in `AuraV\AuraV.ini` (see "Optional: AuraV-Lite with NaturalVision Enhanced" above), then start the game again. |
| You use **NaturalVision Enhanced**, and the game **crashes while loading Story Mode** or **the HUD doesn't show** | Follow "Optional: NaturalVision Enhanced" above: no OpenRPF, the HUD's packs copied into `onigiri\dlcpacks`, and NVE's `dlclist.xml` listing them. NVE brings its own `dlclist.xml` and `gameconfig.xml` (in `onigiri\common\data`), and the game uses those instead of the ones the HUD's package set up. Open NVE's `dlclist.xml` and add these lines just above `</Paths>`, each on its own line:<br>`<Item>dlcpacks:/vihud/</Item>`<br>`<Item>dlcpacks:/vihudfonts/</Item>`<br>`<Item>dlcpacks:/vihudanims/</Item>`<br> Leave NVE's `gameconfig.xml` as NVE ships it: raising its pool sizes any further makes the game run out of memory while loading. |
| You installed **Lively World Expansion** (LWE), and now **parts of the HUD are missing** | LWE's `dlclist.xml` replaced NVE's, and it doesn't list the HUD's packs (or some of NVE's own). Put NVE's `dlclist.xml` back and add LWE's lines and the HUD's lines to it: see "Optional: Lively World Expansion" above. |
| You installed **Central Real-Life Traffic** or **Environment**, and now **parts of the HUD are missing** | Central's `dlclist.xml` replaced yours, and it doesn't list the HUD's packs. Add the HUD's lines back to it: see "Optional: Central Real-Life Traffic and Environment" above. |
| The game **crashes right after the loading screen** since you added **Central Real-Life Traffic** | Install **ResourceExtensions** (Central needs it), and keep the stock `gameconfig.xml` Central put in `onigiri\common\data`. ResourceExtensions doesn't support a modded one, NVE's included. |
| You added **Forests of San Andreas** or **GTA V Remastered**, and **no new trees show** | Their packs need to be where your game loads packs from: with NaturalVision Enhanced in `onigiri\dlcpacks` with their lines in `onigiri\common\data\dlclist.xml`, without it in `mods\update\x64\dlcpacks` with their lines in `mods\update\update.rpf`'s `dlclist.xml`. See "Optional: Forests of San Andreas and GTA V Remastered" above. |
| A Script Hook V message says **"unknown game version"** | GTA V just updated and Script Hook V hasn't caught up yet. Wait for a new Script Hook V, then redo Step 2. |
| The game starts but **the new HUD doesn't show** | Check that the 5 SHVDNE files from Step 3 are next to `GTA5_Enhanced.exe`. |
| You **updated** the mod, but the game still acts like the **old version** | Script Hook V .NET can keep running its saved copy of the old scripts. Close the game, press **Win + R**, type `%LOCALAPPDATA%\assembly`, press Enter and delete the **`dl3`** folder (it's only a cache). Still the old version? Install it again with the OIV Package Installer run as administrator (Step 6), or copy the scripts in yourself: see "Updating by hand" below. |
| **No minimap** in a car | Set the game to **Windowed Borderless** (Step 7). |
| An error about **.NET** or **Visual C++** | Install [.NET Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/thank-you/net48-web-installer) and the [Visual C++ Redistributable (x64)](https://aka.ms/vs/17/release/vc_redist.x64.exe), then restart your PC. |
| A message says **Lucia is not registered** | Lucia isn't included with the HUD any more (see Step 5). Don't have her? Set `Enabled = False` under `[Lucia]` in `scripts\GTA6Hud.ini` and the message goes away. Have a Lucia model? Her pack is missing from `dlclist.xml` (with **NaturalVision Enhanced**, from NVE's): add its line. If the message mentions a `VIHudSetup.exe`, ignore that part: this mod doesn't use it. |
| **Switching characters crashes**, and your game isn't in English | Players report this only happens when the game is set to another language. Until it's fixed, set GTA V's language to **English** in your game launcher's settings for GTA V. |
| The HUD says your game **won't save in its folder**, and a **Windows prompt** asks to let **icacls** make changes | Your game is in **Program Files**, where Windows lets only administrators change files, so the HUD can't save its settings (and ReShade can't save its own). Click **Yes**: it lets your Windows user change the game folder, the same as doing it by hand in the folder's **Properties** → **Security**. Then restart the game once. Clicked **No**? You're asked again the next time you start the game. `ViExtras.ini` `[GameFolder] FixAccess = False` stops the question. |
| **Something else** | Ask in the **[support Discord](https://discord.gg/C7gE5SHEzH)**, or [open an issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues) and attach these files from your game folder: `ScriptHookVDotNet.log`, `scripts\GTA6Hud.perf.log` and `scripts\GTA6Hud.error.log`. |

---

## Updating by hand (game in Program Files)

If your game is in **Program Files** (where the Rockstar Games Launcher and Steam put it), Windows guards that folder,
and an update through the OIV Package Installer doesn't always replace the old files unless you run it as administrator
(Step 6). You can also copy the new scripts in yourself. (From 2.6, the HUD offers to fix the folder for you the first time it runs: see the Windows prompt
row above. After that, updates go in normally.) An `.oiv` is just a zip:

1. Close the game.
2. Copy **`GTA-VI-Inspired-HUD-2.6.2-Enhanced.oiv`** to your Desktop and rename the copy to **`.zip`**.
   Can't see the `.oiv` part of the name? In File Explorer, turn on **View** → **Show** → **File name extensions**.
3. Open the zip → **`content`** → **`scripts`**, select everything in there and copy it.
4. Paste it into the **`scripts`** folder in your game folder, for example
   `C:\Program Files\Rockstar Games\Grand Theft Auto V Enhanced\scripts`. Pick **Replace the files in the destination**, then
   **Continue** when Windows asks for admin permission.
5. Clear Script Hook V .NET's cache, so it loads the new files: press **Win + R**, type `%LOCALAPPDATA%\assembly`
   and press Enter, then delete the **`dl3`** folder. It's only a cache, and it's made again the next time you start
   the game.
6. Coming from 2.5.4 or older? 2.5.5 adds a pack: also copy the zip's **`content`** → **`dlcpacks`** → **`vihud`**
   folder into **`mods\update\x64\dlcpacks`** in your game folder, and install the mod once with the OIV Package
   Installer as in Step 6, which switches the new pack on.
7. Start the game. `scripts\ViExtras.log` now begins with a `==== ViExtras …` line that shows the version you
   downloaded.

---

## How to remove the mod

The easiest way: open the **OIV Package Installer**, go to **Manage Mods**, and uninstall both packages.

Or by hand:

1. In your **game folder**, delete `ViMinimap.asi`.
2. Open the **`scripts`** folder and delete `GTA6Hud.dll`, `GTA6Hud.ini`, `ViMinimap.dll`, `LemonUI.SHVDN3.dll`,
   the `MiniMap.*` files and the `Hud` folder.
3. Delete the **`mods`** folder. ⚠️ This also removes any other mods in it.

Your game is back to normal. Script Hook V, SHVDNE and OpenRPF can stay; they don't change anything on
their own.
