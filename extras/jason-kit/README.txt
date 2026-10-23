GTA VI-Inspired HUD - Jason kit
===============================

WHAT YOU GET:  Jason on the character wheel (hold Left Alt, or D-pad Down on a controller), after Trevor.
WHAT YOU NEED: his model (NOT included, get it below) and this kit. Do the parts in order.

What's in this kit
  jasonped\   The pack you put Jason's files into. There is no model in it.
  setup\      Its setup files, only for the "advanced" part at the bottom.


PART 1: GET JASON
-----------------
1. Download Jason from ONE of these:
     - Lucia's creator:  https://www.patreon.com/u10507693/posts/jason-duval-from-168652433
     - mod-hub.net:      https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en
2. Unzip it.
3. Open the folder "No Cap Version" (or "Cap Version" if you want him in his cap).
4. Inside are 4 files. Their names end in .ydd, .yft, .ymt and .ytd.

   Can't see the endings (.ydd and so on)? In File Explorer: View > Show > File name extensions.


PART 2: CHECK THE 4 FILE NAMES
------------------------------
Look at the part of each name BEFORE the dot.

  Called jdfinal.ydd, jdfinal.yft, jdfinal.ymt, jdfinal.ytd?
      Do nothing. Go to Part 3.

  Called anything else, for example Jason.ydd, Jason.yft, Jason.ymt, Jason.ytd?
      Rename each file to jdfinal, keeping the ending:
          Jason.ydd  becomes  jdfinal.ydd
          Jason.yft  becomes  jdfinal.yft
          Jason.ymt  becomes  jdfinal.ymt
          Jason.ytd  becomes  jdfinal.ytd
      Change nothing else.

The HUD looks for jdfinal, so once your files are called that, he's found. You don't edit any settings.


PART 3 (LEGACY: your game has GTA5.exe): PUT HIM IN THE GAME WITH OPENIV
------------------------------------------------------------------------
1. Copy this kit's "jasonped" folder into mods\update\x64\dlcpacks in your game folder.
2. Open OpenIV and click "Edit mode" at the top, so it's on.
3. In OpenIV, open these one after another:
       mods > update > x64 > dlcpacks > jasonped > dlc.rpf > x64 > models > cdimages > jasonped.rpf
4. Drag your 4 jdfinal files into that window.
5. In OpenIV, open: mods > update > update.rpf > common > data. Right-click dlclist.xml > Edit.
6. Scroll to the VERY BOTTOM. Put your cursor at the start of the line that says </Paths>, press Enter, and paste
   this into the new empty line above it:
       <Item>dlcpacks:/jasonped/</Item>
7. Press Ctrl + S to save, and close the window.
8. Start the game and hold Left Alt (controller: D-pad Down). Jason's card is after Trevor's.


PART 3 (ENHANCED: your game has GTA5_Enhanced.exe): CONVERT HIS FILES
--------------------------------------------------------------------
Enhanced can't use the files as they come, so convert them first.

1. Get CodeWalker RPF Explorer, version 30_dev48 or newer, from the releases channel of dexyfex's Discord
   (https://discord.gg/BxfKHkk).
2. Make a new, empty folder on your Desktop called "Jason converted".
3. In CodeWalker RPF Explorer, click Tools > Asset Converter.
4. For the INPUT folder, pick the folder with your 4 jdfinal files. For the OUTPUT folder, pick "Jason converted".
   Click Process.
5. Open "Jason converted". If jdfinal.ymt isn't in it, copy it in from the input folder yourself.


PART 4 (ENHANCED): PUT HIM IN THE GAME
--------------------------------------
1. Copy this kit's "jasonped" folder into your game folder:
     - WITH NaturalVision Enhanced:  into onigiri\dlcpacks
     - WITHOUT it:                   into mods\update\x64\dlcpacks
2. In CodeWalker RPF Explorer, click Edit > Edit mode, so it's on.
3. Open the jasonped folder you just copied, then: dlc.rpf > x64 > models > cdimages > jasonped.rpf
4. Drag the 4 files from "Jason converted" into that window.
5. Open your dlclist.xml:
     - WITH NaturalVision Enhanced:  open onigiri\common\data\dlclist.xml with Notepad
     - WITHOUT it:                   in CodeWalker RPF Explorer, open mods > update > update.rpf > common > data >
                                     dlclist.xml
6. Scroll to the VERY BOTTOM. Put your cursor at the start of the line that says </Paths>, press Enter, and paste
   this into the new empty line above it:
       <Item>dlcpacks:/jasonped/</Item>
7. Press Ctrl + S to save, and close it.
8. Start the game and hold Left Alt (controller: D-pad Down). Jason's card is after Trevor's.


NOT WORKING?
------------
Jason isn't on the wheel?
  Open scripts\ViSwitch.log in your game folder with Notepad. If it says "no Jason model in the game", one of these
  is wrong:
    - your 4 files aren't called jdfinal (Part 2)
    - they aren't inside jasonped.rpf
    - dlclist.xml is missing the line <Item>dlcpacks:/jasonped/</Item>
  Fix it and start the game again.


PLAYING AS JASON: STORES
------------------------
GTA keeps its stores locked for anyone but Michael, Franklin and Trevor. From the HUD's 2.6.3 on, the clothes
stores' and Ammu-Nation's doors open for Jason too, so he can walk in. He can't buy clothes there, though:
the game's own clothes menu only serves Michael, Franklin and Trevor, and the HUD's wardrobe has a clothes list for
Lucia only. To buy clothes, switch to Michael, Franklin or Trevor.


OTHER JASON MODELS (ADVANCED)
-----------------------------
You only need this if your Jason download isn't the 4 files above.

- Lots of files with a ^ in their names (like Jason^head_000_r.ydd, Jason^uppr_000_u.ydd)?
  Rename the part BEFORE the ^ to jdfinal in EVERY file (Jason^head_000_r.ydd becomes jdfinal^head_000_r.ydd), and
  the files without a ^ too (Jason.ymt becomes jdfinal.ymt). Put them all in jasonped.rpf.
- It comes as a pack of its own (it has its own dlc.rpf)?
  You don't need this kit. Install that pack the way its readme says. Then open scripts\GTA6Hud.ini with Notepad,
  find [Jason], and change Model = jdfinal to its ped's name (the name of its .ydd file, before the dot).
