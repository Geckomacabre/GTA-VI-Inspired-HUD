GTA VI-Inspired HUD - Lucia kit
===============================

WHAT YOU GET:  Lucia on the character wheel (hold Left Alt, or D-pad Down on a controller), the last card.
WHAT YOU NEED: her model (NOT included, get it below) and this kit. Do the parts in order.

What's in this kit
  A - 4 files\luciaped\           The pack for a Lucia that comes as 4 files. There is no model in it.
  B - with a folder\luciaped\     The pack for a Lucia that comes with a folder of parts. No model in it either.
  setup\                          Their setup files, only for the "advanced" part at the bottom.

You use ONE of the two packs: Part 2 tells you which.

Already have a "luciaped" folder in your dlcpacks folder (mods\update\x64\dlcpacks, or onigiri\dlcpacks)?
Then a Lucia is already installed, and you don't need this kit.


PART 1: GET LUCIA
-----------------
1. Download Lucia from ONE of these:
     - her creator, on Patreon:  https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256
     - gta5-mods.com:            https://www.gta5-mods.com/player/mp-jason-lucia-v
   Or use another Lucia model.
2. Unzip it, and find the folder with her files in it. Their names end in .yft, .ymt and so on.

   Can't see the endings (.yft and so on)? In File Explorer: View > Show > File name extensions.


PART 2: WHICH PACK, AND HER FILE NAMES
--------------------------------------
Look at what's in her folder:

  4 files, ending in .ydd, .yft, .ymt and .ytd?
      You use pack A.
      Look at the part of each name BEFORE the dot. Already lucia_s (lucia_s.ydd, lucia_s.yft, ...)? Do nothing.
      Called anything else, for example Lucia.ydd, Lucia.yft, Lucia.ymt, Lucia.ytd?
      Rename each file to lucia_s, keeping the ending:
          Lucia.ydd  becomes  lucia_s.ydd
          Lucia.yft  becomes  lucia_s.yft
          Lucia.ymt  becomes  lucia_s.ymt
          Lucia.ytd  becomes  lucia_s.ytd
      Change nothing else.

  A .yft file, a .ymt file and a FOLDER with the same name (full of files like head_000_r.ydd, uppr_000_r.ydd)?
      You use pack B.
      Already called lucia_s (lucia_s.yft, lucia_s.ymt and the folder lucia_s)? Do nothing.
      Called anything else, for example Lucia.yft, Lucia.ymt and a folder Lucia?
      Rename all three to lucia_s:
          Lucia.yft      becomes  lucia_s.yft
          Lucia.ymt      becomes  lucia_s.ymt
          folder Lucia   becomes  folder lucia_s
      Don't rename the files INSIDE the folder.
      A second folder with _p at the end (like Lucia_p, her hats and glasses)? Rename it to lucia_s_p.

  Something else? See "Other Lucia models (advanced)" at the bottom.

The HUD looks for lucia_s, so once her files are called that, she's found. You don't edit any settings.

In the parts below, "your pack" is the kit's luciaped folder from A or B, and "her archive" is:
  - pack A:  luciaped.rpf
  - pack B:  luciaped_streamed.rpf


PART 3 (LEGACY: your game has GTA5.exe): PUT HER IN THE GAME WITH OPENIV
------------------------------------------------------------------------
1. Copy your pack's "luciaped" folder (from "A - 4 files" or "B - with a folder") into mods\update\x64\dlcpacks in
   your game folder.
2. Open OpenIV and click "Edit mode" at the top, so it's on.
3. In OpenIV, open these one after another:
       mods > update > x64 > dlcpacks > luciaped > dlc.rpf > x64 > models > cdimages > her archive
4. Drag her lucia_s files into that window (pack B: the folder lucia_s too, and lucia_s_p if you have it).
5. In OpenIV, open: mods > update > update.rpf > common > data. Right-click dlclist.xml > Edit.
6. Scroll to the VERY BOTTOM. Put your cursor at the start of the line that says </Paths>, press Enter, and paste
   this into the new empty line above it:
       <Item>dlcpacks:/luciaped/</Item>
7. Press Ctrl + S to save, and close the window.
8. Start the game and hold Left Alt (controller: D-pad Down). Lucia's card is the last one.


PART 3 (ENHANCED: your game has GTA5_Enhanced.exe): CONVERT HER FILES
--------------------------------------------------------------------
Enhanced can't use the files as they come, so convert them first.
Her download has an "enhanced" folder? Those files are converted already: use them and skip to Part 4.

1. Get CodeWalker RPF Explorer, version 30_dev48 or newer, from the releases channel of dexyfex's Discord
   (https://discord.gg/BxfKHkk).
2. Make a new, empty folder on your Desktop called "Lucia converted".
3. In CodeWalker RPF Explorer, click Tools > Asset Converter.
4. For the INPUT folder, pick the folder with her lucia_s files. For the OUTPUT folder, pick "Lucia converted".
   Click Process.
5. Pack B only: make an empty folder called lucia_s inside "Lucia converted". Run the Asset Converter again, with
   her lucia_s FOLDER as the input and that new lucia_s folder as the output. Got a lucia_s_p folder? Do the same
   for it (an empty lucia_s_p folder inside "Lucia converted" as the output).
6. Open "Lucia converted". If lucia_s.ymt isn't in it, copy it in from the input folder yourself.


PART 4 (ENHANCED): PUT HER IN THE GAME
--------------------------------------
1. Copy your pack's "luciaped" folder (from "A - 4 files" or "B - with a folder") into your game folder:
     - WITH NaturalVision Enhanced:  into onigiri\dlcpacks
     - WITHOUT it:                   into mods\update\x64\dlcpacks
2. In CodeWalker RPF Explorer, click Edit > Edit mode, so it's on.
3. Open the luciaped folder you just copied, then: dlc.rpf > x64 > models > cdimages > her archive
4. Drag everything in "Lucia converted" into that window (pack B: the lucia_s folder too, and lucia_s_p if you
   have it).
5. Open your dlclist.xml:
     - WITH NaturalVision Enhanced:  open onigiri\common\data\dlclist.xml with Notepad
     - WITHOUT it:                   in CodeWalker RPF Explorer, open mods > update > update.rpf > common > data >
                                     dlclist.xml
6. Scroll to the VERY BOTTOM. Put your cursor at the start of the line that says </Paths>, press Enter, and paste
   this into the new empty line above it:
       <Item>dlcpacks:/luciaped/</Item>
7. Press Ctrl + S to save, and close it.
8. Start the game and hold Left Alt (controller: D-pad Down). Lucia's card is the last one.


NOT WORKING?
------------
A message says "Lucia is not registered"?
  dlclist.xml is missing the line <Item>dlcpacks:/luciaped/</Item> (with NaturalVision Enhanced: NVE's
  onigiri\common\data\dlclist.xml). Add it (Part 3 or 4) and start the game again.

Lucia isn't on the wheel?
  Open scripts\GTA6Hud.perf.log in your game folder with Notepad. If it says "lucia: no Lucia model in the game",
  one of these is wrong:
    - her files aren't called lucia_s (Part 2)
    - they aren't inside her archive (luciaped.rpf for pack A, luciaped_streamed.rpf for pack B)
    - you used the wrong pack: 4 files need pack A, files with a folder need pack B
    - dlclist.xml is missing the line <Item>dlcpacks:/luciaped/</Item>
  Fix it and start the game again.

Lucia is on the wheel but invisible, or only partly there?
  It's the wrong pack for her files (Part 2). Delete the luciaped folder you copied and start again with the other
  one.


OTHER LUCIA MODELS (ADVANCED)
-----------------------------
You only need this if your Lucia download isn't one of the two kinds in Part 2.

- Lots of files with a ^ in their names (like Lucia^head_000_r.ydd, Lucia^uppr_000_u.ydd)?
  Those are the parts of a pack B Lucia, laid out for FiveM. Use pack B:
    1. Make a folder called lucia_s and move every file with a ^ into it.
    2. In that folder, rename each file to the part AFTER the ^ (Lucia^head_000_r.ydd becomes head_000_r.ydd).
    3. Rename the files without a ^ to lucia_s, keeping the ending (Lucia.yft becomes lucia_s.yft, Lucia.ymt
       becomes lucia_s.ymt).
    4. Carry on with Part 3, putting the lucia_s folder and files into luciaped_streamed.rpf.
  Files with _p^ in their names (like Lucia_p^p_head_000.ydd) are her hats and glasses: the same again, into a
  folder called lucia_s_p.
- It comes as a pack of its own (it has its own dlc.rpf)?
  You don't need this kit. Install that pack the way its readme says. Then open scripts\GTA6Hud.ini with Notepad,
  find [Lucia], and change Model = lucia_s to its ped's name (the name of its .yft file, before the dot).
- Want to keep her own name instead of renaming her files to lucia_s?
  Open your pack's setup files in the kit's setup folder ("A - 4 files" or "B - with a folder"). In peds.meta,
  change lucia_s in <Name>lucia_s</Name> to her name (pack B: lucia_s_p in <PropsName> too, to her name with _p).
  Put that peds.meta into the pack's dlc.rpf at common > data, in place of the one there (OpenIV or CodeWalker,
  in Edit mode). Then set Model to her name under [Lucia] in scripts\GTA6Hud.ini.
