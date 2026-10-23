GTA VI-Inspired HUD - Jason pack kit
====================================

The HUD puts Jason on the character wheel (hold Left Alt), next to Lucia, once his ped is in your game.
Jason isn't included with the HUD. Get him from:
  - Lucia's creator: https://www.patreon.com/u10507693/posts/jason-duval-from-168652433
  - or "Jason Duval character from GTA VI" on mod-hub.net:
    https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en

That download is only his model files (jdfinal.ydd, jdfinal.yft, jdfinal.ymt and jdfinal.ytd), so this kit is the
pack to put them in. There's no model in the kit.

What's in here
  jasonped\dlc.rpf   The pack, without Jason: content.xml, setup2.xml, common\data\peds.meta (a ped named jdfinal)
                     and an empty x64\models\cdimages\jasonped.rpf for his files.
  setup\             The same three setup files, to read or change.


LEGACY (your game has GTA5.exe), with OpenIV
--------------------------------------------
1. Copy the jasonped folder into mods\update\x64\dlcpacks in your game folder.
2. In OpenIV, switch on Edit mode and open mods\update\x64\dlcpacks\jasonped\dlc.rpf > x64 > models > cdimages >
   jasonped.rpf.
3. Drag in the 4 files from the download's "No Cap Version" folder (or "Cap Version", for Jason in his cap).
4. Open mods\update\update.rpf > common > data > dlclist.xml (right-click > Edit) and add this line just above the
   closing </Paths> at the bottom of the file, then save:
       <Item>dlcpacks:/jasonped/</Item>
5. Start the game and hold Left Alt: Jason's card is next to Lucia's.


ENHANCED (your game has GTA5_Enhanced.exe), with CodeWalker
-----------------------------------------------------------
Enhanced can't use the files as they come, so convert them first. You need CodeWalker RPF Explorer, version 30_dev48
or newer (from the releases channel of dexyfex's Discord, https://discord.gg/BxfKHkk).

1. In CodeWalker RPF Explorer, open Tools > Asset Converter ("Convert files from GTAV Legacy formats to GTAV Enhanced
   formats"). Pick the download's "No Cap Version" (or "Cap Version") folder and a new, empty folder for the
   converted files, then click Process. If jdfinal.ymt isn't in the new folder, copy it over as it is.
2. Copy the jasonped folder into your game folder:
     - with NaturalVision Enhanced: into onigiri\dlcpacks
     - without it (OpenRPF): into mods\update\x64\dlcpacks
3. In CodeWalker RPF Explorer, switch on Edit mode (Edit > Edit mode), open jasonped\dlc.rpf > x64 > models >
   cdimages > jasonped.rpf, and drag in the 4 converted files.
4. Add this line just above the closing </Paths> at the bottom of your dlclist.xml, then save:
       <Item>dlcpacks:/jasonped/</Item>
     - with NaturalVision Enhanced: onigiri\common\data\dlclist.xml (Notepad is fine)
     - without it: mods\update\update.rpf > common > data > dlclist.xml, in CodeWalker RPF Explorer
5. Start the game and hold Left Alt: Jason's card is next to Lucia's.


PLAYING AS JASON: STORES
------------------------
GTA keeps its stores locked for anyone but Michael, Franklin and Trevor. From the HUD's next update (after 2.6.2),
the clothes stores' and Ammu-Nation's doors open for Jason too, so he can walk in. He can't buy clothes there, though:
the game's own clothes menu only serves Michael, Franklin and Trevor, and the HUD's wardrobe has a clothes list for
Lucia only. To buy clothes, switch to Michael, Franklin or Trevor.


A DIFFERENT JASON MODEL
-----------------------
The HUD looks for the ped named in scripts\GTA6Hud.ini, under [Jason]: Model = jdfinal. Another Jason works too:

- Its files have another name (say jason.ydd, jason.yft, jason.ymt, jason.ytd)?
  Either rename them all to jdfinal (keep the endings: jdfinal.ydd and so on), and nothing else changes.
  Or keep the names, and change jdfinal to the model's name in two places:
    - <Name>jdfinal</Name> in setup\peds.meta (Notepad is fine). Then drag that peds.meta into the pack's
      common\data (OpenIV or CodeWalker RPF Explorer, in Edit mode) to replace the one there.
    - Model = jdfinal under [Jason] in scripts\GTA6Hud.ini
- It's made of many files (jason^head_000_r.ydd, jason^uppr_000_u.ydd and so on)?
  Put them all in jasonped.rpf, and use the name before the ^ (here: jason) in the two places above.
- It comes as a pack of its own (it has its own dlc.rpf)?
  You don't need this kit. Install that pack the way its readme says, then set Model under [Jason] in
  scripts\GTA6Hud.ini to its ped's name: the <Name> in its peds.meta, or its .ydd file's name.

Nothing happens? Check scripts\ViSwitch.log: it says "Jason's model is in the game" once the HUD has found him, or
"no Jason model in the game" if the ped isn't there (the pack isn't in dlclist.xml, or the names don't match).
