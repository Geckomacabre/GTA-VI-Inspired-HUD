# Fork changelog

This is a list of what this fork changed and fixed on top of **FBICat's** GTA VI HUD for GTA V single player. Everything not listed here is FBICat's work, as he made it.

Newest first. "Unreleased" is what's in testing and not yet on the Releases page.

> [!IMPORTANT]
> ### 💬 Need help? Join our support Discord
> This version is a **fork of FBICat's VI HUD** with extra features, so it has its own support:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, or a [GitHub issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat's Discord is there for his original mod, so please bring questions about this version to ours. Thank you!

## Unreleased

### Added

- **More ways to earn honor back.** Losing the police without killing an innocent or a cop is a clean escape (+4; a chase of at least 20 seconds, and not if you're arrested or die). A few minutes with no stars and no bad deeds counts as laying low (+2, a few times a session). Killing someone who was attacking a bystander is standing up for them (+3), and they no longer cost honor. `[Honor] CleanEscape`, `LayLowMinutes` and `DefendBystanders`.
- **The police seeing it makes it worse.** A bad deed a cop can see (a kill, a carjacking, a threat) costs 3 more. `[Honor] WitnessedExtra = 0` turns it off.
- **People back off from a Psycho.** Once your honor is -40 or below (`[Honor] RankPsycho`), or broken, people nearby who see you now and then keep their distance with a word, and some run, more often once you're Shattered. Never on a mission, in a car or while you're wanted. `[Honor] NpcReactions = False` turns it off.
- **Shops on the map as Lucia and Jason.** The game hides the clothes stores', barbers' and Ammu-Nation's map icons for anyone but Michael, Franklin and Trevor, so as Lucia or Jason there were no shops on the map. While you play one of them, each store the game shows no icon for gets one, and they go again when you switch back. `[Switches] ShopIcons = False` leaves the map alone.
- **Jason can change his look.** At any rack in a clothes store or a barber's, **E** opens Jason's look: every part his model has more than one of (hair, beard, top, legs, shoes, gloves...), style by style and colour by colour, read off the Jason model in your game, so any download works (Jason Duval Extended Look has 5 hairstyles, 4 tops, 2 trousers and 2 pairs of shoes). Up / Down picks the part, Left / Right changes it, Enter keeps it, Backspace puts it back. Kept, it's saved (`scripts\Hud\Save\jason_look.txt`) and he wears it from then on. It's his own clothes, so it's free.
- **Pet dogs.** Walk up to a dog and **PET** comes up on it (**E**, controller **D-pad Right**): your character kneels and pets it with Franklin's petting of Chop, for a little honor (each dog pays once every 5 minutes). A dog a script owns, such as one in a mission, is left alone. `[Honor] PetDogs = False` turns it off.

- **Easy switches at the top of the ini files.** `ViExtras.ini` now opens with `[Switches]`: one plain `True` / `False` line per feature, in groups (on screen, cameras, police, people, honor, skills, cars and gear, radio), each with a one-line note. Everything else (amounts, distances, timings, keys) is below an ADVANCED line you don't need to touch. `GTA6Hud.ini` opens the same way with FBICat's switches people change most (status bars, the pause menu, smarter police, heat, the bounty, the sixth star, street life, carry all weapons, fuel, hunger, the character wheel and more). Every setting reads as before; an older `ViExtras.ini` without `[Switches]` still works.
- **Calmer police.** GTA V's police were on top of you in seconds, and seemed to know where you were even when none of them could see you. Now they take twice as long between sending units and send them from about 300 m away; once they've lost sight of you (the stars grey out), the search stays where they last saw you instead of following you; and staying out of sight for 10 seconds plus 5 a star loses them (25 seconds at 3 stars). Not on story missions, or while RDE or LSR runs the police. `[Switches] CalmerPolice = False` brings back GTA V's police; the numbers are in `[CalmerPolice]`.
- **Switches for stun guns and heists.** `ViExtras.ini` `[Switches] StunGuns = False` leaves the police their own guns, as in GTA V, while surrendering and arrests still work (`PoliceArrests`, the old `StunGunArrests`, turns those off). `GTA6Hud.ini` `[Heists] Enabled = False`, at the top of the file, turns the heists off.
- **The loading screens are their own, optional download.** FBICat's GTA VI loading screens (and the start screen on Legacy) are no longer in the HUD package: they're `GTA-VI-Inspired-HUD-<version>-LoadingScreens-Legacy.oiv` / `-Enhanced.oiv`, installed the same way, if you want them. Neither installer can tick off one part of a package, so this is how you choose. Already have them from an older version and want the game's own back? Your install guide's "Optional: VI loading screens" shows how.
- **Bounty patrols only follow fresh trouble.** FBICat's bounty is saved and only drops $500 a game day, and while there's any, his Heat sends police patrols cruising round you, so a game loaded with an old bounty had patrols closing in from the first minute, for hours. Now they only come while you're wanted and for 10 minutes after (`[BountyPatrols] FreshMinutes`). `[Switches] OldBountyPatrols = True` brings back the old way.

### Fixed

- **HUD OPTIONS no longer scrambles `GTA6Hud.ini`.** Flipping one of FBICat's switches (STATUS BARS, KILL CAMERA and the rest) saved the file through Script Hook V .NET, which writes it again from scratch: every comment went, and the easy switches at the top were merged back into their sections. Now only the lines that changed are written (a key the file doesn't have yet goes under its section), and the rest of the file stays as it was. A small change to how `GTA6Hud.dll` saves, and nothing else.
- **No more Chop on the character cards.** Michael's and Franklin's portraits come from the game's switch-wheel picture, which has Chop (upside down) between them, and an edge of him showed on both cards. That part of the picture is no longer drawn at all; the card's backdrop shows there.
- **The minimap's HUD OPTIONS switches no longer wipe the rest of `MiniMap.settings`.** Both of the HUD's minimap savers (the minimap's own, and the HUD menu's day / night, only-in-a-vehicle and search-circle switches) wrote the file again from their own values alone, so `ShowWhenWanted`, the search circle's sizes, `HiddenBlips`, the file's notes and anything else you'd set were lost. Now only the line that changed is written.
- **Legacy: notifications you can read.** GTA's notification cards came out pale, almost white, so their white text disappeared over a bright street. The HUD tells its own notification movie where to hang the feed with numbers sent as the next card's background colour, but on Legacy GTA's own movie is the one on screen, and it took each number as a colour, which no colour is. On Legacy that's no longer sent: the cards are GTA's own dark ones, where GTA puts them (which is where they already were).

### Changed

- **People don't remember you for ever.** Anyone who saw you commit a crime kept it on record for good, and opinions only faded when a character loaded, over days. Memories are now short, as people's are: what someone saw you do is forgotten 10 minutes after they last saw you, opinions lose half every 20 minutes and a neighbourhood's every 30, while you play and while you don't. So people react to you while you're out causing trouble, and stop once you've laid off. `ViExtras.ini` `[Memory] CrimeForgetMinutes`, `FaceHalfLifeMinutes` and `ZoneHalfLifeMinutes` (they replace `FaceHalfLifeDays` and `ZoneHalfLifeDays`).
- **A quieter minimap, like GTA VI's.** The shop, barber, bar, car wash and gas station icons are left off the minimap (the pause map still has them all). `MiniMap.settings` `HiddenBlips` is the list; empty it to have them all back.
- **The minimap flashes red and blue while you're wanted.** Its red / blue flash was never switched on (and `WantedTint`, which it needs, was off), and the police search circle that fills the map faded slowly between red and blue, so it mostly looked purple. Now the whole map snaps between red and blue every half second. `MiniMap.settings` `ChaseFlash = 0` keeps the tint red, `WantedTint = 0` turns it off.
- **Your gun's only at the top right while it's out.** GTA V keeps your last gun as your weapon while you drive, so its picture and ammo stayed up the whole drive. In a car they now show only while you aim or shoot from it (`GTA6Hud.ini` `[TopRight] GunInVehicle = True`, at the top of the file, keeps them up). The gun's picture is a little smaller too (`GunRowH`, `GunMaxW`).
- **Thinner money.** The money at the top right is in the thinner Art Deco lettering and a touch smaller, as in GTA VI. `[TopRight] MoneyFont` and `MoneyCapHeight` now really set it (the money row had its font and size built in, whatever they said); `MoneyFont = 7` and `MoneyCapHeight = 13.7` bring back the old look.

## 2.6.4 (23 October 2026)

### Added

- **Empty balances show grey.** In the money row at the top right, BuckMe's purple icon and the wallet's green one turn grey while that balance is $0, and take their colour back as soon as there's money in it.

### Fixed

- **BuckMe no longer pays out the same money again.** The HUD keeps your cash at $20,000 and moves the rest into BuckMe, which it saves straight away, while the game only keeps your lower cash when it saves. So switching to a character with more than $20,000 put that money into BuckMe again in every session that ended without saving: a character at GTA's maximum cash added $2.1 billion each time. The HUD now notes what it moved (`scripts\Hud\Save\buckme_moved.txt`) and, when it meets the same cash again, only puts your wallet back to $20,000. Money you pick up while playing goes into BuckMe as before. Already have a huge balance? Close the game and set the number in `scripts\Hud\Save\buckme.txt` to what you want.

## 2.6.3 (23 October 2026)

### Added

- **The driving camera has weight.** Land a jump, hit a bump hard or crash, and the camera dips with your car or bike, as hard as the hit, then eases back to your view in about a second, as in GTA VI. It's only in GTA's third-person driving camera, never first person, aiming or looking behind, and your mouse or stick keeps the camera all the way through. The crash camera still cuts in on a big crash. `ViExtras.ini` `[WeightedCam] Strength` sets how far it dips (0.5 for half as far), and `Enabled = False` turns it off.
- **Prompts in the world for more of what you do, and sooner.** The store hold-up's THREATEN and a mugging's ROB now sit on the person, and the counter prompts (Ammu-Nation's WEAPONS, the clothes stands, the fence, the tool shop, the bag stash, the drills) at the shop's clerk or just in front of you. They used to sit at the bottom right of the screen. The game's own "Press E to ..." prompts (shops, barbers, vending machines, the till) come up in the world too, in place of the top-left box. And every prompt comes up sooner: you don't have to look straight at the spot, and a car door's ENTER and break-in choices reach 3.5 m instead of 1.9 m. `GTA6Hud.ini` `[Interact] PromptConeScale`, `GamePromptsInWorld` and `VehiclePromptRange` set these.
- **Greet and Antagonize have voices.** Greeting someone (Y), Michael, Franklin and Trevor say hello in their own voices, and antagonizing (U) gets them insulting, harsher each time, the last one shouted. People answer out loud too (their replies were there but switched off). `ViExtras.ini` `[Memory] PlayerSpeech = False` / `NpcSpeech = False` keep either quiet.
- **Rebalanced Dispatch Enhanced next to the HUD (new, only tested briefly).** We've tried it in one short run on Legacy, so tell us how it goes. While RDE's police run, GTA6Hud's police brain, GTA6Hud's own six stars (and the military they call in) and the stun-gun arrests stay out of their way, and the stars show RDE's level up to six: levels 6 to 10 all light the sixth star. `ViExtras.ini` `[Compat] Rde = False` leaves the HUD as it is.
- **Los Santos RED next to the HUD (new, only tested briefly).** We've tried it in one short run on Legacy, so tell us how it goes. While LSR runs (in RAGE Plugin Hook), it has the police, the crimes and the wanted level, and the HUD adds none of its own: no witnesses, police brain, police memory, heat, six stars or stun-gun arrests. `extras/lsr-stars` has LSR's star pictures redrawn as the HUD's GTA VI stars. `ViExtras.ini` `[Compat] Lsr = False` leaves the HUD as it is.

- **The minimap in helicopters and planes.** The 3D minimap left aircraft out, and since it switches GTA's radar off, flying had no map at all. It now shows in every vehicle, and in the air it zooms out the higher you fly (from about 2x at 50 m to most of the city at 1200 m), whether or not `AltitudeZoom` is on in `MiniMap.settings`. Cars and on foot are as before.
- **FBICat's loading screens on GTA V Enhanced, with whole pictures on both versions.** Enhanced now gets his loading screens too (its landing page stays the game's own). On both versions the Vice City pictures fill the screen: they're Rockstar's whole screenshots (Vice City 01 to 12 from rockstargames.com/VI/media), where they used to be strips cut from the middle. On Legacy his landing page now has a whole picture behind it as well. With NaturalVision Enhanced, copy one file into `onigiri` (see the Enhanced guide).

### Fixed

- **The crash camera hands your view back where it was.** After its slow-motion shot, the driving camera came back tilted a couple of degrees lower than before (more on a bike), because GTA sets a vehicle camera's tilt a little under what it's asked for. It now checks where the camera landed and puts it right.
- **Switching characters works without Lucia.** On 2.6.2, which comes without Lucia, the keyboard and the controller switched nobody (only a story mission's own switches worked). Without her, or Jason, the HUD left switching to GTA6Hud's own wheel, which can't start the game's switch. The HUD's wheel now runs with whoever's in the game.
- **Fewer street fights.** People near you started an argument every few seconds, shouting insults at each other, and four in ten ended in a fight to the death, mostly with bats: 70% of the people on the street carried a knife, bat or crowbar. Arguments now come at least 30 to 60 seconds apart, 15% of them (and of fender benders) end in a fight, and one person in five carries a melee weapon. `GTA6Hud.ini` `[World] ArgumentMinSeconds`, `ArgumentMaxSeconds` and `AmbienceFightChance`, and `[Interactions] MeleeChance`, set these.
- **Cops let you go when your stars do.** The HUD's police give each cop an order of their own (hold you at gunpoint, take cover, cut you off, arrest you) and keep him to it. When your stars went he got his gun back, but the order kept running, so a cop could keep walking up to arrest you, hang around you and draw his gun with no stars. The order now ends as he's let go, and the cop told to arrest you stops when the arrest does, so the police go back to what they were doing.
- **Jason can walk into the clothes stores and Ammu-Nation.** GTA keeps its stores locked for anyone but Michael, Franklin and Trevor, and the HUD only opened them for Lucia. Their doors now open for Jason too (there are no clothes to buy for him there).
- **Switching characters works on a controller.** With a controller the row of cards came up and a character could be picked, but letting go never switched: the check for a rebound F5 / F6 / F7 key read the controller's buttons instead of the keyboard's, so the switch was only simulated, which the game ignores. It now reads the keyboard's key.
- **On Demand switches stations with Left / Right.** In the radio wheel your own stations (each folder in `CustomMusic`) now step with **Left / Right** (the right stick on a controller), as the radio's do, and the **mouse wheel** (D-pad Right / Up) or GTA's next / previous track keys change the song. Left / Right used to change the song, so trying to reach another station only restarted the one playing.
- **Installing the HUD never lowers another mod's limits.** The package set five of `gameconfig.xml`'s limits to the HUD's values, so installing or updating it after a mod with bigger ones (Rebalanced Dispatch Enhanced's are far bigger) cut them down, and the game could crash while loading. It now only raises a limit that's lower than the HUD needs.

### Guides

- **Optional: Rebalanced Dispatch Enhanced** and **Optional: Los Santos RED** in the Legacy guide: the install order, what the HUD leaves to them, and for LSR the settings and keys to change next to the HUD.
- **An old 3D minimap's files.** Both install guides' troubleshooting tables now say to delete `MiniMap.asi` or `MiniMapHudPresent.asi` (and `scripts\MiniMap.dll`) left by another 3D minimap mod or an older install. The HUD brings its own copy as `ViMinimap.asi`, and two copies at once get in each other's way.
- **Switching characters on a controller.** The README, quick start and both install guides now say a controller picks a card with **LB / RB** (D-pad Left / Right is under the thumb that holds D-pad Down).
- **Script Hook V .NET's minimum.** The Legacy guide now says 3.7.0 nightly 193 or newer (it said 3.7 or newer), with a troubleshooting row for the "Unhandled exception in script" / FileNotFoundException an older one gives.
- **NaturalVision Enhanced: the loading screens.** The Enhanced guide's NVE section has a step for copying them into `onigiri\platform\data\cdimages` with CodeWalker RPF Explorer, troubleshooting rows (the game's own loading screens; after a game update), and the file to delete when removing the mod.

## 2.6.2 (22 October 2026)

### Fixed

- **Notifications where the wanted box goes.** 2.6.1's move beside the minimap didn't take: GTA ignores a script moving its notifications, so in a car they stayed under the 3D map. The HUD's own notification movie (in the `vihud` pack) now hangs them at the left, a quarter of the way down, where GTA VI's wanted box goes, newest on top, and moves them under the box while it's up. They stop short of the map and the vehicle card, on any screen shape, so `FeedBesideMapWide` is gone. `ViExtras.ini` `[Minimap] FeedBesideMap = False` leaves them where GTA puts them. With NVE, copy the new `vihud` into `onigiri\dlcpacks` too.
- **World badges stay off the notifications.** The house over a safehouse and your car's badge are drawn over the whole picture, GTA's notifications included, so one could sit on a notification. They're no longer drawn in the notifications' column.

## 2.6.1 (22 October 2026)

### Added

- **The Jason kit.** Jason's download is only his model files, so there was no pack to list in `dlclist.xml`. [GTA-VI-Inspired-HUD-Jason-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Jason-Kit.zip) is that pack without Jason: drop his 4 files into it, add its line to `dlclist.xml`, and he's on the character wheel. It has no model in it.
- **The Lucia kit.** Like Jason's, a Lucia download is only her model files, with no pack to list in `dlclist.xml`. [GTA-VI-Inspired-HUD-Lucia-Kit.zip](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/raw/main/extras/GTA-VI-Inspired-HUD-Lucia-Kit.zip) is that pack without Lucia, in two kinds: **A** for a Lucia that comes as 4 files, **B** for one with a folder of parts (the same pack as the HUD's old Lucia, without her). Rename her files to `lucia_s`, drop them in, add `<Item>dlcpacks:/luciaped/</Item>` to `dlclist.xml`, and she's on the character wheel. It has no model in it.

### Fixed

- **GTA's tips show again.** FBICat's HUD hid every one of GTA's help boxes (the tips and tutorials at the top left, like "You can now buy properties") and redrew only the ones it has a label for, so the rest only played their sound. A box it can't redraw is now left to the game, and GTA's own box shows. With `GTA6Hud.ini` `[HelpText] Enabled = False`, every box is GTA's own.
- **Notifications beside the minimap.** The 3D minimap switches GTA's radar off, so GTA put its notifications in the minimap's corner, where the map covered them in a car. While the map is on screen they now sit to its right. Wider than 16:9, GTA's notifications are already clear of the map; `ViExtras.ini` `[Minimap] FeedBesideMapWide = True` moves them there too, and `FeedBesideMap = False` leaves them where GTA puts them. *This didn't take: GTA ignores the move. Fixed in 2.6.2.*
- **The minimap's mission circle stays put.** In a mission, the yellow circle of the mission's blip flickered, sometimes jumped far from the mission, and sat on the pink circle at the end of the mission route. The minimap now checks each blip is still the one it found, keeps a dot through the moment between its blip scans, counts a flashing blip as shown, and leaves the route's end to the pink circle.
- **A Lucia or Jason under another model name keeps their own honor.** Honor looked for `lucia_s` and `jdfinal` only, so with another model (`GTA6Hud.ini` `[Lucia] Model` or `[Jason] Model`) their honor was kept under the model's code instead. It now follows those settings.

### Guides

- **Optional: Jason.** Both install guides have a section for putting Jason in the game with the kit: OpenIV on Legacy; on Enhanced, CodeWalker's Asset Converter first, then into `onigiri\dlcpacks` with NVE or `mods\update\x64\dlcpacks` without it. It also covers another Jason model: renaming its files, or setting its name in the kit's `peds.meta` and `GTA6Hud.ini` `[Jason] Model`, and models that come as their own pack.
- **Optional: Lucia.** Both install guides have a section for putting Lucia in the game with the kit, as for Jason: OpenIV on Legacy; on Enhanced, CodeWalker's Asset Converter first, then into `onigiri\dlcpacks` with NVE or `mods\update\x64\dlcpacks` without it. It also covers FiveM-style files (with a `^`), models that come as their own pack, and keeping her own model name.

## 2.6.0 (22 October 2026)

### Added

- **Rob the Fleeca banks, Pacific Standard and Vangelico.** FBICat's heists were in his HUD but never ran. Now they do. Case the place first: walk around without aiming and look at each camera and guard (and a Fleeca's vault door) until a ring fills over it; once you've spotted everything, the place is cased until you rob it. Pull a gun to start, and the line at the bottom of the screen says what's next: smash Vangelico's cases, or at a bank drill the vault (buy a drill at You Tool), grab the cash and drill the deposit boxes. The alarm brings the police on a timer, so get out, lose them and take the bag to a safehouse (STASH THE BAG on the map) to get paid. Guards stand where the game's own people stand, and Vangelico's is the jewel heist's own store guard. The shops keep 2.1's hold-ups.
- **Vangelico is open.** Outside the story, the jewel store is a locked shop front (or boarded up after the Jewel Store Job), so it could never be robbed. Outside story missions, the store the Jewel Store Job uses is now there, with its manageress and customers, and the story's version comes back when you leave. `ViExtras.ini` `[Vangelico] Enabled = False` leaves it closed.
- **No character switching while you're on duty in LSPDFR.** Switching away from your cop mid-shift pulls LSPDFR's character out from under it, so while LSPDFR has you on duty the character wheel (ours and FBICat's) stays shut and the game's own switch keys do nothing. It's back as soon as you go off duty.
- **No witnesses or wanted level while you're on duty in LSPDFR.** On duty you're the police, so what you do isn't a crime to the HUD: nobody runs or phones to report you, people and neighbourhoods don't hold it against you, and any wanted level that lands anyway (a GTA6Hud crime such as shoplifting, or the game's own) is cleared at once. A witness already running when you go on duty is called off. Off duty it all works as before.
- **Carry every weapon, like GTA V (on by default).** FBICat's 2.1 lets you carry 2 pistols, 2 long guns (rifles, SMGs, shotguns, machine guns, snipers and heavy weapons), 1 thrown weapon and 1 other, and drops the rest. With **CARRY ALL WEAPONS** on (HUD OPTIONS, hold V; `GTA6Hud.ini` `[Wheel] AllWeapons = True`) you keep everything you pick up, and picking up a weapon never swaps one out. On the weapon wheel's WEAPONS page, Left / Right (D-pad, arrow keys or the mouse wheel) step the highlighted slot through every weapon of its kind, and "WEAPON 2 / 4" shows where you are. Switch it off for FBICat's limit.
- **A game in Program Files gets fixed for you.** Windows lets only administrators change files under Program Files, where the Epic and Rockstar launchers put GTA V, so the HUD couldn't save its settings and saves there (and ReShade couldn't save its own). When the game folder can't be written to, the HUD now says so in the feed, and Windows asks with its administrator prompt to let your Windows user change the game folder. Click Yes once and restart the game, and it stays fixed. Nothing changes if you click No; you're asked again next time. `ViExtras.ini` `[GameFolder] FixAccess = False` stops the question.

- **GTA VI's character switcher.** Hold the character wheel key (Left Alt / D-pad down) and the characters are a row of cards, as in GTA VI: Michael, Franklin and Trevor, then Jason and Lucia side by side with a heart between them. Michael, Franklin and Trevor are GTA V's own switch-wheel portraits, and under each card is that character's honor face (good, bad, broken, or a plain face in between). Q / E or Left / Right pick, letting go switches. `ViExtras.ini` `[Switcher] Enabled = False` brings back FBICat's wheel.
- **Jason on the character wheel.** Like Lucia, Jason isn't included: with a Jason ped in your game, such as [Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433) on Patreon from Lucia's creator or [Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en) on mod-hub.net (`GTA6Hud.ini` `[Jason] Model`, `jdfinal` by default), he's on the wheel beside her. Switching to him is the game's sky camera, he borrows the character you came from (his money is theirs, as Lucia's is), and from him you go on to anyone in one flight. He has his own honor.

### Fixed

- **Sniper rifles show their scope.** The HUD's crosshair hid the game's reticle for every gun, and a sniper rifle's scope is part of that reticle, so aiming one showed no scope at all. Sniper rifles now keep the game's own scope, as FBICat's HUD does.
- **The stun gun no longer jumps into your hands in a vehicle.** Getting into a car or onto a bike with a stun gun on you, the game kept putting the stun gun in your hands, and swapped it back in whenever it didn't allow the gun you had (a two-handed SMG on a bike). Now a stun gun you didn't pick gives way, for the whole ride, to the gun you held on foot if it can be used there, then your one-handed SMGs and pistols, then the two-handed SMGs. Picking the stun gun yourself is left alone, and getting out gives you back what you held before. `ViExtras.ini` `[CarWeapon] Enabled = False` leaves it to the game.
- **The Los Santos Customs garage doors work as in GTA V.** FBICat's HUD hid the doors whenever you were near, so the garage looked always open. They're shut again, roll up as you drive close and back down once you've gone, and stay shut while you're wanted. `ViCustoms.ini` `[Customs] GarageDoors = False` leaves them to FBICat's HUD.
- **From Lucia to Michael, Franklin or Trevor in one switch.** Switching from Lucia to anyone but the character you came from flew back to that character first and then switched again. It's now the game's own sky switch, from where Lucia stands straight to the one you picked, wherever the game has them. The character you came to Lucia from stays where you left him, so switching to him later finds him there, not next to Lucia, and the game no longer holds that switch back for several seconds. When the game does refuse a switch, the message no longer blames the story if you've unlocked everyone.
- **Lucia is somewhere when you switch to her.** She's at one of 13 of the game's own street spots, doing what people do there (smoking, leaning, browsing the shops), instead of standing alone in one place. Switch back within a minute and she's where you left her.
- **The map legend tells businesses from stash points.** Properties for sale were listed as STASH THE BAG; they now show as BUSINESSES, and STASH THE BAG is only the heist stash points, while you carry a bag.
- **The radio on a controller steps one station a push.** Held, the right stick kept cycling through the stations; it now steps once each time you push it. Clicking the stick in to mute no longer moves you to the next station as well (which unmuted it again), so the radio goes grey and stays muted.
- **Story missions keep their HUD and their people.** The HUD and minimap no longer vanish while a mission's next cutscene is only waiting to load, and the HUD's hold-ups, gunpoint reactions, conversations, witnesses and police leave a mission's own characters and cars alone, so they don't run off or get arrested mid-mission.
- **The crosshair shows when you lock on.** With Assisted Aim or Traditional GTA targeting, aiming at someone locks on instead of free-aiming, and the HUD's crosshair only came up after the first shot. It now shows as soon as the gun is up.
- **The wanted tells are back.** Since 2.5.5 switched FBICat's police memory off, only the gun could light under the wanted box. The tells now come from what the police saw this pursuit, the way the FiveM version does it: your outfit, your face (unless it's masked) and your car's plate while a cop has eyes on you, a gun you hold in their sight or fired in the last 12 seconds, and the camera when you're wanted inside a building. Change clothes, mask up or swap cars and that tell goes out, and it's all forgotten when the stars are gone. The police still don't come after you again later. They show in two places, as on FiveM: under the wanted box's text, and in a row of red discs under the stars, which GTA6Hud's top-right stack makes room for (it held FBICat's police icons, dark since his memory went off). `ViExtras.ini` `[WantedBox] IndoorCameras = False` leaves the camera out; `GTA6Hud.ini` `[Police] Enabled = False` hides the row under the stars.
- **No more "Lucia … not registered" message when you didn't install Lucia.** She's an optional download, but the HUD's start-up check warned everyone without her for 20 seconds, and told them to reinstall the HUD. It now only warns when Lucia's package is in your game but the game hasn't registered it, and then it names her package (`GTA-VI-Inspired-HUD-Lucia-Legacy.oiv` or `-Lucia-Enhanced.oiv`) instead of the HUD's.
- **No more stripes over Leonida Customs' menu.** Its dark fades are drawn as thin see-through strips, and each overlapped the next by half a pixel, so every overlap showed as a darker line, like a fine grid down the left of the screen. The strips now meet on whole screen pixels.
- **The "Turn Left / Turn Right" hint goes away in Leonida Customs.** With a waypoint set, FBICat's turn hint stayed on screen over the menu; it's held off while the menu is up, like the HUD's other panels.
- **CRASH CAMERA works from the pause menu's HUD page too.** On that page (new in FBICat's 2.1) the row stayed OFF and couldn't be switched on, and switching KILL CAMERA there turned the crash camera off. It now works there just like in HUD OPTIONS (hold V).
- **Mk II and other weapons show their pictures on the weapon wheel.** FBICat's art for 25 of them was there but under GTA's own weapon names (`weapon_assaultrifle_mk2.png`), while the HUD looks for Script Hook V .NET's (`weapon_assaultriflemk2.png`), so their slots were empty: every Mk II, the Double Barrel and Sweeper shotguns, the Up-n-Atomizer, Unholy Hellbringer and Widowmaker, the Perico pistol, the Compact and smoke grenade launchers, the proximity mine, knuckle dusters, stone hatchet and hazardous jerry can. Copies under the HUD's names are in `scripts\Hud\Weapons`; FBICat's files are unchanged. The Railgun (XM3) and the online Stun Gun use his Railgun and Stun Gun pictures.
- **No more Nitrous crash in Program Files.** While Windows won't let the HUD write to your game folder (Program Files, before you click Yes on the HUD's prompt), the nitrous script stopped with a red "Unhandled exception" message, and the mod shop could do the same: they couldn't make the HUD's save folder and gave up. They now carry on, and save once the folder can be written to.

### Changed

- **Lucia is no longer included.** Her model is someone else's paid work, and it was shared with this mod without their permission, which we didn't know, so her packages are gone from every release (2.2's and 2.5.0's HUD packages too, which had her built in). To play as Lucia, get her from her creator: [Lucia Caminos on Patreon](https://www.patreon.com/u10507693/posts/lucia-caminos-6-170646256) or [MP Jason & Lucia V](https://www.gta5-mods.com/player/mp-jason-lucia-v) on gta5-mods.com, or use another Lucia model. For Jason, get [Jason Duval](https://www.patreon.com/u10507693/posts/jason-duval-from-168652433) from the same creator, or [Jason Duval character from GTA VI](https://mod-hub.net/mod/personazh-jason-duval-z-gta-vi-dlya-gta-5/?lang=en) on mod-hub.net. The HUD's Lucia features (her character switching, wardrobe and portrait) stay in: her slot on the character wheel is off by default (`GTA6Hud.ini` `[Lucia] Enabled = False`) and turns on by itself once the game has loaded, when a Lucia ped named `lucia_s` (or the `[Lucia] Model` name) is in the game. The "not registered" message no longer names a Lucia package of ours.
- **One file to install: no more RegisterPatch.** The HUD package now does what `GTA-VI-Inspired-HUD-RegisterPatch` did: it switches the HUD's three packs on in `dlclist.xml` and raises the `gameconfig.xml` limits the HUD and Lucia need. It never lists a pack twice, and installing it again or over an older version no longer ends in "Installation FAILED" ("Unable to find XML Node"), the message the RegisterPatch gave everyone who updated. Lucia is still her own, optional package. With NaturalVision Enhanced, NVE's own `dlclist.xml` still needs the HUD's lines, as before.
- **Smoother frame times.** The HUD does the same with less work every frame:
  - **Controller button pictures no longer stall the game.** Every pad prompt, the weapon wheel, shops and pickups asked FBICat's HUD which picture to show, and each time it polled all 16 joystick slots through Windows: up to a quarter of the time with a prompt on screen went there, in stalls of 25-35 ms. With `[Controller] Style = PlayStation` or `Xbox` it no longer polls at all; with `Auto`, every 2 seconds.
  - **No half-second freeze every 30 seconds when you drive somewhere new.** The minimap saved what it learned about the roads (a 31 MB file) on the game's thread; it's now written in the background. Its 3D buildings, road heights and water also fill in with about 1 ms a frame instead of up to 4.6, so new areas fill in a little more slowly.
  - **FBICat's HUD:** the street name, the basketball hoop search and the personal-car badge look things up a few times a second instead of every frame, and NPC spin-outs watch cars within 100 m instead of 150.
  - **Our scripts:** fewer game calls each frame across the board: kill tracking reads each ped once in four frames outside a firefight, the honor and mod shop panels work their gradients out once, nitrous, witnesses, honor, the gym, police lights and stun guns scan less often or less, and the UI checks every script makes are answered once a frame. Skills and honor saves are written in the background. `ViExtras.ini` `[Honor] Debug` is now `False`.
- **The wanted box is GTA VI's.** "The cops are searching for you / and they have your description", with GTA VI's dark red star and the tells under it, rebuilt from a GTA VI frame: its size, the dark tint that fades towards the right, the faint top edge, the lighter, spaced-out lettering, and the darker red of the star and tell discs (the discs under the stars match). The other three messages ("A crime has been reported" and the rest) came from the FiveM version and aren't in GTA VI, so they're gone: the box shows while you're wanted and the police have at least one tell on you, for about seven seconds when a new one lights or your stars go up.
- **GTA VI's police lights.** The siren glow at the screen's edge is now GTA VI's, rebuilt frame by frame from the police chase in its trailer: a row of blue and red lights glowing up from the edge, one to four lit at a time, each flashing on its own beat. The brightest burn white in the middle, and where blue and red meet their light turns violet. They're biggest when the police are close (within 5 m) and shrink from 60 m out, so you can tell how near they are. **POLICE LIGHTS** in HUD OPTIONS (hold V) makes them smaller or bigger (60-140%). `ViExtras.ini` `[PoliceLights] Size`, `NearSize`, `FarSize`, `TallDistance`, `ShortDistance` and `RoundOpacity` set the same; `Shape = Wide` brings back the old flat lights.
- **GTA VI's police arrow.** When a police car with its siren on is coming, GTA VI's arrow points at it: a thin curved line on a ring round the middle of the screen, with a small point aimed at the car. It's white while the car is far, fills blue from the point as it closes in (all blue by 15 m) and goes once the police are beside you, the car or a cop on foot (inside 8 m). `ViExtras.ini` `[PoliceLights] Arrow`, `ArrowFull` and `ArrowGone` set it. FBICat's police arrows stay off (`GTA6Hud.ini` `[PoliceArrow] Enabled = False`).

### Guides

- **Game in Program Files? Run the installer as administrator.** Both install guides (Step 6), the quick start and the README's update box say to right-click OpenIV or the OIV Package Installer → Run as administrator when the game is under `C:\Program Files`, where an install without it can leave the old files in place.
- **AuraV-Lite with NaturalVision Enhanced.** The Enhanced guide has a section of its own for the two together, step by step: NVE and the HUD first, AuraV's files in the game folder (not `onigiri`), `AuraV\AuraV.ini` made before the first start with AuraV's start-up shortcuts off and its menu on F11, RenoDX's add-ons moved out, and Ray Tracing and DLSS on. The AuraV-Lite section is now for games without NVE.
- **Where the HUD's lines go in NVE's `dlclist.xml`.** The Enhanced guide and the README now say to add them just above the closing `</Paths>` at the bottom of the file, not under `<Paths>` at the top, where the game skips them.
- **Forests of San Andreas and GTA V Remastered.** Both install guides have a section for Larcius' tree and prop maps. On Legacy their OIVs go in after the HUD, and a troubleshooting row says which three pools to raise if the game then crashes while loading, instead of replacing the `gameconfig.xml` the HUD sets up. On Enhanced the guide covers NaturalVision Enhanced (packs in `onigiri`) and plain OpenRPF installs, and says not to mix them with Lively World Expansion.
- **Which Script Hook V .NET do you have?** The Enhanced guide's row for a game that closes by itself shows how to check (`ScriptHookVDotNet.asi` → Properties → Details says "Script Hook V .Net Enhanced") and to clear the old one's cache after swapping.
- **Central Real-Life Traffic and Environment.** The Enhanced guide has a section for CentralMods' NVE builds (tested with Traffic 2.9.1, Environment 0.2, NVE and Lively World Expansion). Their `dlclist.xml` replaces yours and leaves out the HUD's and Lucia's packs, so the guide says to add those lines back, to install ResourceExtensions for Traffic, and to keep Traffic's stock `gameconfig.xml`.

## 2.5.5 (20 October 2026)

### FBICat's VI HUD 2.1

**FBICat's 2.1 is in**, with this fork's changes kept on top. What his 2.1 brings:

- **Character switching like GTA V.** Michael, Franklin and Trevor swap exactly like the base game, and **Lucia is her own character in the world**, switched to with the same sky camera. The portraits lost their white background.
- **A new weapon wheel.** It just appears (no pop-in), with outlined text. Q (RB on a controller) changes tabs and E only uses. Gun pictures are sized by the gun, all snacks share one slot (and can be eaten in missions too), the fishing rod is gone, and the flashlight slot stays empty until you own one.
- **His VI pause menu is back** (CHALLENGES / COLLECTABLES), with new **ROBBER challenges**: rob tills, hold up stores, rob all 19 stores, steal cars and one of every vehicle type.
- **The kill cam never freezes you:** a slow-motion pan across the victim and you while you keep aiming, moving and turning. The crash camera got a "car crash cinematic" sound.
- **Clothes:** an OUTFITS rack with the full outfits the stores sell Michael, Franklin and Trevor. Hats and glasses only show what you own (Lucia's too).
- **Hats:** a hat or helmet knocked off anyone, you included, can be picked up and worn for a while. More people and nearly all guards wear hats.
- **New sounds** for notifications, picking up a weapon, "Crime reported", cash and the crash camera (`scripts\Hud\*.wav`).
- **Smaller fixes:** picking things up uses a full-body crouch, and the "+ item" lines sit on the right. GTA's own HUD pieces no longer come back in some parts of the map. Traffic cars no longer change colour in front of you, and the maker's emblem stays inside the vehicle card. Race and rental markers only show within 50 m. The HUD no longer stalls for 3-4 seconds after you spawn while police patrols wait for their models.

**Kept as in 2.5.4:** the 3D minimap (FBICat's 2.1 minimap isn't included), and the look of GTA's own pause screens and the character wheel (his restyled versions of those game screens aren't included). His VicePoint crosshair is built into 2.1 but switched off (`GTA6Hud.ini` `[Reticle] Enabled = False`), because this fork has its own crosshair.

### Added

- **FBICat's VI start screen on GTA V Legacy.** The landing page (STORY MODE / ONLINE) and the startup loading screen are his: a black screen, the GTA VI logo and a drifting Vice City picture strip. The GTA Online overlay has been removed from the Story Mode loading screen. GTA V Enhanced keeps its own start screen.
- **RegisterPatch 1.7, for FBICat's new `vihud` pack.** His 2.1 HUD art and movies come as a new pack, `vihud`, and `GTA-VI-Inspired-HUD-RegisterPatch-1.7.oiv` switches it on. Updating from 2.5.4 or older? Install the RegisterPatch once more. If it then says the install failed ("Unable to find XML Node"), that only means your older packs are registered already: it adds `vihud` first. NaturalVision Enhanced: copy `vihud` into `onigiri\dlcpacks` too and add `dlcpacks:/vihud/` to NVE's `dlclist.xml`.

### Fixed

- **Updating now really updates.** Up to 2.5.4, every file in the OIVs carried the same date, so Script Hook V .NET kept running the copy it had cached from your first install, and an update could look like it did nothing (HUD OPTIONS rows showing N/A, for example). The 2.5.5 files carry their real dates. If an earlier update still looks old, the install guides' troubleshooting has a row for clearing that cache.
- **The HUD no longer says Lucia is missing when she's installed.** FBICat's 2.1 checks for her `lucia_nophys` model, which this fork's Lucia pack doesn't have. It now checks for the model she actually uses.
- **The radio stays shut while you pick a character in a car.** Q / LB and E / RB step through the character wheel, but in a vehicle they are also the radio's keys, so picking someone opened the radio wheel or changed the station. The radio now waits until the character wheel is closed.
- **No more interaction dots in cutscenes.** FBICat's small white dots on people, car doors, dropped weapons and shop items now stay hidden during cutscenes, character switches and any moment you don't have control, and only show within 2.5 m of you (they used to show from 5-6 m).
- **Switching to Lucia no longer traps you inside a building.** In FBICat's 2.1 she waits for you somewhere in the world, and her first spot was inside the building under Floyd's apartment in Vespucci, with no way out. If her spot is inside a building, she now waits on Vespucci Beach instead.
- **The fence keeps the cars it buys.** After you sell a car, it can't be sold again (you could get back in and sell the same car over and over), and it's gone once you walk away from it.
- **No more Vangelico marker.** The jewel store is closed for refurbishment in single player, so its robbery could never start. Its diamond is gone from the map (`Hud\Robbery\sites.txt`); the banks and stores are unchanged.

### Changed

- **The police no longer remember you.** FBICat's police memory (cops recognising your face, outfit or car later and coming after you again) is off (`GTA6Hud.ini` `[Police] MemoryEnabled = False`), and its POLICE MEMORY switch is gone from HUD OPTIONS.
- **People report you less often.** Someone who saw an old crime no longer phones the police later when they spot you again (`ViExtras.ini` `[Memory] Gossip = False`), so no more stars out of nowhere. Witnesses now notice crimes from 25 m (was 35 m), recognise you half the time (was 70%), and take 12-20 seconds to make the call (was 9-15), so you have longer to stop them.
- **The OIV files say their version.** They're called `GTA-VI-Inspired-HUD-2.5.5-Legacy.oiv` and `GTA-VI-Inspired-HUD-2.5.5-Enhanced.oiv`, and the installer shows "GTA VI-Inspired HUD 2.5.5", so you can always tell which one you have. Handy when asking for help.

### Guides

- **Game in Program Files?** Both install guides have an "Updating by hand" section for when the installer can't write there, and a troubleshooting row for an update that still shows the old version.
- **The update box, the install guides and the NaturalVision Enhanced steps** name RegisterPatch 1.7 and the `vihud` pack.
- **The Enhanced guide names the right mod loader.** On GTA V Enhanced, Script Hook V's `xinput1_4.dll` is what loads `.asi` mods; `dinput8.dll` is the Legacy loader and Enhanced doesn't use it. Step 2 had them the other way round and called `xinput1_4.dll` optional.
- **Lively World Expansion with NaturalVision Enhanced.** The Enhanced guide has a section and a troubleshooting row for it. LWE's own `dlclist.xml` replaces NVE's and leaves out the HUD's packs, Lucia, three of NVE's newer packs and GTA's 2026 update, so the guide copies only LWE's packs and adds its lines to the list you already have.

## 2.5.4 (20 October 2026)

### Fixed

- **Your own music (On Demand) no longer freezes the game.** It played through GTA6Hud's player, which starts Windows' WPF media player inside GTA; on GTA V Legacy the game stopped responding as soon as it did. ViExtras now plays your music itself (Windows' Media Foundation decodes mp3, wav, wma and m4a, waveOut plays them) in the same order, with the same skipping and the same song and artist names, and it still remembers where each station was for half an hour.
- **Ammu-Nation works on GTA V Enhanced again** (no more black screen looping at the counter). FBICat's gun shop read the weapon stats in a way only Legacy's script hook supports; it now reads them on both.
- **Surrendering (hold E near a cop) fills up and works every time.** The prompt shared GTA6Hud's single hold timer with its loot, shop and cash prompts, which it runs every frame for anything nearby, so the circle kept starting over at about 10%. The surrender now keeps its own timer, stays on the same cop while he can see you, and forgives the camera slipping off his head for a moment.
- **The star in the wanted box is sharp again.** The box is packed compressed, which broke the small red star up into blocks; the star is now a picture of its own, packed uncompressed and drawn over the box (`tools/wanted_star.py`).
- **On Demand no longer jumps straight back to the radio** after the radio was muted.
- **The radio wheel clears a leftover character-switch sound scene when you pick a station.** A switch can leave that scene running, and it keeps the car radio silent.

### Added

- **Four new switches in HUD OPTIONS** (hold V, or the camera button; the pause menu's HUD page has them too): **GREET / ANTAGONIZE**, **POLICE MEMORY**, **SPIN-OUTS** and **VI RADIO WHEEL**. Each one takes effect at once and is saved. Police memory off also makes the police forget you there and then. The VI radio wheel off gives you GTA's own radio wheel back.
- **Change the song from the radio wheel.** While your own music plays, **Left / Right** (the right stick on a controller) skip to the next or previous song; the mouse wheel (D-pad Right / Up) still steps between your music's stations.
- **Greet and antagonize work on a controller:** Y / Triangle greets (or defuses, or intimidates), B / Circle antagonizes. While the prompt is up, those buttons don't also get you into a car, reload or punch. X stays free for the HUD's shop and loot prompts. (`[Memory] PadGreet` / `PadAntagonize` in `ViExtras.ini`; they were off.)
- **On a controller, a right-stick click mutes the radio** while the wheel is open. D-pad Down did it before, and it's the character switcher's button too.
- **The logs say which build you have.** `ViExtras.log`, `ViCustoms.log` and `ViCrouch.log` start every session with a line like `==== ViExtras 2.5.4+202610201742.550baac on GTA V Enhanced 1.0.1158.16 ... ====`: the version, when it was built, and the game it runs on. Handy when asking for help.

### Changed

- **Smoother in traffic.** Every frame, the HUD's spin-out check read the driver and all four tyres of every car within 150 m, parked ones included: about 4 ms a frame in town. It now skips cars that are parked or slow (they can't spin out), and the HUD's whole share of a frame dropped from about 5-6 ms to about 2 ms. Spin-outs work as before.
- **Fewer police patrols closing in on you after a crime.** While the police remember your face, outfit or plate, FBICat's Heat now sends 2 patrols at most, a minute apart, from 250-350 m away instead of 6 every 12 s from 90-160 m, and the police forget you twice as fast (`GTA6Hud.ini` `[Heat]` and `[Police]`). They still only recognise you when a cop can see you.
- **The Greet / Antagonize prompt is easy to switch off.** Set `Interact = False` under `[Memory]` in `scripts\ViExtras.ini`; the settings file and the README now say so. People still notice and remember you.

### Guides

- **A big "Updating? You only need one file" box** in the README and the release notes.
- **The Enhanced guide has a NaturalVision Enhanced section** (no OpenRPF; the HUD's packs go into `onigiri\dlcpacks`) and more on **AuraV-Lite**: move RenoDX's add-ons out (its menu crashes with them, and NVE comes with them), and move its menu key off F10, the HUD's settings-reload key.
- **AuraV-Lite with NVE: make `AuraV\AuraV.ini` before the first start**, with its three `[boot]` lines at `0`, so the game doesn't crash once first (the Enhanced guide's AuraV step 3).
- **A "Debug Assertion Failed!" box naming `renodx-dlssfix.addon64`** has a row in the Enhanced guide's troubleshooting table: that's RenoDX's DLSS fix add-on, not the HUD; without DLSS 5 you can delete it, and Visual C++ doesn't need reinstalling.

## 2.5.3 (20 October 2026)

### Fixed

- **Switching to Michael, Franklin and Trevor works again**, from the HUD's character wheel (hold Left Alt, or D-pad down) and with F5-F7. The wheel held GTA's switch input off, so the game ignored it. A new script, `ViSwitch.dll`, now runs the HUD's wheel: Michael, Franklin and Trevor switch with the game's own sky camera, and Lucia goes through the sky too, landing mid-scene at one of the characters' spots. It needs `[Lucia] Enabled = True` in `GTA6Hud.ini`, the default. With Lucia switched off, switch with GTA's own wheel (`[Swap] Enabled = False`).
- **Enhanced: no more crash right after switching to or from Lucia.** GTA6Hud's police code read the player's info in the middle of the swap; `ViSwitch` turns that one memory read off on Enhanced.

### Added

- **AuraV-Lite (path tracing) with the HUD** in the [Enhanced guide](INSTALL-ENHANCED.md): how to install it, and the one setting that keeps it from crashing with NaturalVision Enhanced.

## 2.5.2 (19 October 2026)

### Fixed

- **Enhanced: GTA6Hud's notifications show.** Messages like "Not enough money.", "Lose the cops first." and "Registered. It is yours now." never appeared on Enhanced, and `GTA6Hud.error.log` filled with a PostTicker error every 10 s: Script Hook V .NET Enhanced moved the type that call returns. ViExtras now posts them to the game's feed itself on Enhanced. Legacy is unchanged.
- **Leonida Customs always lets you go when it closes.** If putting the car back as paid for went wrong while closing, the car stayed frozen with the shop's camera on. Now the car, the camera and the HUD are let go whatever happens.
- **Leaving Leonida Customs with Esc or B held doesn't carry on into the game** (Esc's pause menu, B's cinematic camera): the key stays off until you let go of it.
- **D-pad Down in the radio wheel (mute, on a pad) no longer opens the character wheel**, the same fix the binoculars got: the character wheel stays shut while the radio wheel is open, and until you let go of D-pad Down.
- **On Demand goes quiet over the pause menu, cutscenes, black screens and death**, as FBICat's own radio player does. Your music used to keep playing over the pause menu.
- **Leaving the car while your music plays gives the car its station back.** Its radio was left switched off, so getting back in was silent.
- **Throwing the switch to On Demand while the radio is muted unmutes it first**, as browsing stations does, so going back to the radio brings back the station you had (it came back as RADIO OFF).
- **A loading screen or a character switch pauses your music at once** and ends the radio wheel's slow motion there and then. If GTA6Hud's own radio takes over (`[Radio] Enabled = True`), it gets the car unmuted.
- **A nitrous kit changed in the shop works at once:** an upgrade gives the new bottle there and then. It used to need getting out of the car and back in.
- **Standing still with the weapon wheel, a menu or the phone open near a cop no longer counts as giving up** at 1-2 stars, and an arrest that didn't happen isn't tried again for 10 s.
- **The vehicle card can't go missing after Leonida Customs** with `[VehicleCard] RouteGauge = True`: the shop put GTA6Hud's card setting back to an old value when it closed.
- **RegisterPatch 1.6: installing it again sets the pool sizes.** OpenIV stops at the first pack that is already listed in `dlclist.xml` and calls the install failed, and that used to come before the pool sizes, so a second install (or one over an older patch) never set them. The pools come first now, and the guides say a "failed" second install just means it is already in.
- **Steadier scripts:** a one-off error no longer stops the police script for the session (with the stars held at 1), or leaves crouch ignoring its keys.

### Changed

- **`GTA6Hud.ini` `[MapIcons] Sprites` is FBICat's list again.** 2.5.1 added LS Customs (72) to it, but that didn't make LS Customs show on the map from further away, so the change is undone.
- **Binoculars tell other scripts when D-pad down is theirs** (`Binoculars.HoldsCharacterKey`, like nitrous's), so a character wheel of another script stays shut while you zoom out.
- **Nitrous logs which input started each boost**, to track down reports of the wrong button boosting.
- `ViExtras.log` no longer says On Demand is disabled at every start; it says where your own music goes.

## 2.5.1 (19 October 2026)

### Changed

- **Support Discord:** [discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH), at the top of the README and both install guides, for questions about this version. FBICat's Discord is for his original.
- **Police only shoot to kill when you're armed and wanted at 3+ stars.** The rest of the time every cop carries a stun gun, and you're arrested (BUSTED) instead: when a stun gun drops you, or when you stand still, unarmed, near a cop at 1-2 stars.
  - "Armed" is a gun in your hand, or one fired in the last 5 seconds.
  - The arrest is the game's own. You drop to 1 star (it only arrests at 1), FBICat's police AI pauses, and the nearest cop on foot is told to arrest you. Draw a gun, or go 15 seconds without an arrest, and your stars come back.
  - **Surrender:** with stars, near a cop who can see you, look at him and hold **E** (X / Square on a pad) on the SURRENDER prompt. Your hands go up, you go down on your knees with your hands on your head, and you're arrested. Sprint to bolt (the stars come back). The pose uses the same Rockstar clips as the FiveM server's fenix-police.
  - FBICat's own SURRENDER prompt never came up: his code returns before it can offer one. It's shown from here, through his prompt.
  - The weapons work with FBICat's police AI: the weapon it re-arms each cop with is pointed at the stun gun, and back at the cop's own gun at 3+ stars when you're armed.
  - Settings: `[PoliceForce]` in `ViExtras.ini` (`LethalFromStars`, `ArmedGraceSeconds`, `ArrestWindowSeconds`, `Enabled`).
- **Binoculars are on the weapon wheel:** hold Tab (LB on a pad) and let go on the binoculars tile.
  - The tile is bottom right, or the next empty tile clockwise when a weapon sits there. It uses FBICat's binoculars icon and opens the FiveM binoculars.
  - B no longer opens them. `[Binoculars] Key = B` brings the key back, and `WheelSlot = False` takes them off the wheel.
- **The radio wheel has a new look:** each station's logo fills its tile, the highlighted station is a little larger with a thin light outline, and the line and text are light grey.
  - Under the station name are the song playing and its artist, the game's own names for them. A talk station shows its genre instead.
  - Just after you change station the old song's name stays off for up to 3 seconds, until the new station's song is the one playing.
  - The song and artist are set in vi_radio's Anton from a glyph atlas in the HUD pack, so they show on Enhanced too (it can't draw pictures made while the game runs).
  - The open and close clicks are quieter (`[Radio] SoundVolume` 50 → 20).
- **On Demand plays your own music.** In the radio wheel, press **Down** (arrow key) or push the **right stick down** to throw the switch to ON DEMAND, and **Up** to go back to the radio, the way the FiveM wheel does. The left icon's knob shows where it is.
  - Put `.mp3` / `.wav` / `.wma` / `.m4a` files in `scripts\Hud\Radio\CustomMusic`: they're the station MY MUSIC. A subfolder with tracks in it is a station of its own, named after the folder.
  - The song and artist show under the station name. GTA's next / previous track keys skip. It stops when you leave the car, and going back to the radio puts back the station you had on.
  - It plays through FBICat's own custom music player. `[Radio] CustomMusicVolume` (0.20) sets how loud.
  - A station with no logo (yours, or one the game adds) gets a tile with its name on it, drawn live, so on Enhanced too.
- **Chase music takes over from the car radio:** while you're wanted and chase music plays (the game's wanted music, or GTA6Hud's `[Music]` tracks), the car radio goes quiet, and comes back when it ends. `[Radio] ChaseMusicTakesOver = False` lets them play together.
- **Minimap routes in the FiveM HUD's colours:** your own waypoint's route is teal (#47aba7) and a mission's or a race's is pink (#fc74a4), each with the FiveM HUD's lighter rim. They were the minimap's own light blue and a different pink. `[Minimap] RouteColours = False` in `ViExtras.ini` brings those back; it replaces `WaypointRoutePink`.
- **The turn-by-turn box matches the route:** the square behind the arrow is teal like your waypoint's route, not FBICat's pink. The box follows your own waypoint, so it's always teal. The square is recoloured in FBICat's own texture (`hud_gps_bg`, in the HUD pack's `vipause`; the rest of it is his, pixel for pixel), so it fades in and out cleanly.
- **Crash cam: the black bars top and bottom hold still.** They used to swell and shrink with the vignette's slow breathing, which looked like the frame pulsing. The vignette still breathes (`[CrashCam] OverlayPulse`).
- **GTA's own on-foot camera.** FBICat's camera pulled in whenever you aimed or fired (LT / RT, right mouse) and held the walking camera at one distance, so V couldn't zoom out. It's off in `GTA6Hud.ini`; `[Camera] Enabled = True` brings it back.
- **Nitrous is on Left Shift** (X / Square on a pad). FiveM's Left Alt is also GTA's character wheel. `[Nitrous] KeyboardControl = 19` puts it back on Left Alt, and the wheel then stays shut while you drive a car with a bottle.
- **Smoother crawling:**
  - Holding W or S crawls continuously, one cycle running into the next. It used to start and stop on a timetable that didn't match the animation.
  - Letting go finishes the cycle before resting.
  - Turning eases in and out at 75° a second, at any frame rate, and pivots at the shoulders so the hips swing round. It used to be a degree a frame.
  - Holding A / D while lying still turns you on the spot the same way. It used to be a 10° step a second.
  - The blends are softer.
- **The legs crawl too.** On your front, each knee draws up along the ground as the opposite arm reaches, then pushes as that arm pulls. Rockstar's crawl dragged them. The hips roll and the waist bends with them.
  - These are our own clips, made from Rockstar's `move_crawl` (`vihud_crawl`, in the `vihudanims` pack). Arms, body and speed are Rockstar's.
  - `[Crawl] OwnClips = False` in `ViCrouch.ini` goes back to Rockstar's crawl, and so does a missing pack.
- **Crawling uses a body-sized collision capsule**, to keep the lying body out of slopes, steps and walls. `[Crawl] BodyCapsule = False` turns it off.
- **Lucia is a separate download:** `GTA-VI-Inspired-HUD-Lucia-Legacy.oiv` / `-Lucia-Enhanced.oiv`. Install it after the HUD and the RegisterPatch.
  - The HUD package is now the HUD alone, and much smaller.
  - Lucia's package adds her two packs to `dlclist.xml`, each only if it isn't listed yet.
  - The RegisterPatch (1.5) registers only the HUD's packs. Installs that already list Lucia's keep them.
  - Without Lucia installed, picking her on the character wheel shows FBICat's notice that she isn't registered, and nothing else changes.

### Fixed

- **Nitrous: a look-alike circle** sat on the vehicle card whenever a waypoint was set, with or without a kit. It was FBICat's route gauge, with nearly the same double-chevron icon. The card is now drawn the FiveM HUD's way on every vehicle: engine and fuel, plus the nitrous gauge only when the car has a kit. `[VehicleCard] RouteGauge = True` in `ViExtras.ini` gives cars without a kit FBICat's own card back.
- **Nitrous fired on A as well as X on a controller.** On a keyboard it read the sprint control (Left Shift), which is also A on a pad, and a pad can read as a keyboard for a moment (a nudged mouse). The keyboard side now reads the Shift key itself; on a pad only X / Square boosts.
- **Leonida Customs could leave you stuck:** camera on the car, no controls, nothing answering (seen at the repair question). The menu ignores input until the keys held driving in come up, and an input that never read as up kept it ignoring you for good. It now waits 1.5 seconds at most, and holding Back (Backspace, Esc, B / Circle) for 2 seconds always leaves the shop. Its log notes what it asked and what you pressed.
- **The weapon wheel wouldn't open near dropped weapons.** FBICat's HUD skips its wheel on any frame its "TAKE / SWAP" prompt for a weapon on the ground is up, and its carry limit drops your extra weapons right at your feet. So giving yourself weapons or picking one up left the wheel dead until you walked away. The prompt now steps aside while you hold the wheel button, and comes back when you let go (`[WeaponWheel] OverPickups` in `ViExtras.ini`).
- **Binoculars: D-pad down (zoom out) opened the character switcher.** FBICat's HUD reads that button even while it's switched off, so its switcher is now held shut while the binoculars are up, and until you let go of the button.
- **BUSTED never happened with FBICat's HUD.** It clears the wanted level the moment an arrest begins, and the game then drops the arrest (the cop walks back to his car). While one of our arrests is under way that's held off until the BUSTED screen shows, so the arrest completes.
- **Lucia: loading a save you quit as her could freeze the game**, while the HUD switched you back to her. FBICat's automatic Lucia swap is switched off in `GTA6Hud.dll`: a 2-byte change in `UpdateLuciaMode`, the only change to his DLL. Lucia now only changes from the character wheel, and you load in as your story character.
- **Install guides:** a "Lucia is not registered" row (her two `dlclist.xml` lines are missing; the message's `VIHudSetup.exe` isn't part of this mod), and NVE's `dlclist.xml` lines listed one per line, so all four get copied.
- **WASTED / BUSTED only showed once per session.** Releasing the screen's text movie threw an error, which left the screen marked as still playing, so later deaths and arrests got the game's own screen.
- **WASTED in a vehicle:** the camera frames the vehicle from outside its body, follows it if it's still rolling, and comes back if the crash cam hands the view back at the same moment.

### Known issues

- **Switching to Michael, Franklin or Trevor doesn't work while the HUD's character wheel is on**, from the wheel or with F5-F7. The wheel holds GTA's character-switch input off every frame, and that also blocks the F keys. To switch, set `[Swap] Enabled = False` in `scripts\GTA6Hud.ini` and use GTA's own wheel (Lucia is then off the wheel). A fix is being tested.

## 2.5.0 (2026-10-19)

### Added: ported from the FiveM version

These live in new script files next to FBICat's `GTA6Hud.dll`, which is left as it is: `ViExtras.dll`, `ViWasted.dll`, `ViCustoms.dll` and `ViCrouch.dll`.

- **WASTED / BUSTED screens.** Single player had none. From vi_wasted (GPL-3, see `src/ViWasted/LICENSE`).
- **Honor.**
  - Good and bad deeds raise and lower your honor.
  - A corner panel shows your mugshot and standing.
  - A +/- popup appears on the right edge, under "Crime reported".
  - The cracked "broken" state.
  - A layout editor moves the popup and panel, and saves where you put them. Set `[Honor] EditKey` in `ViExtras.ini` (Z, say) to open it.
- **Crosshair and kill marks.** A ring in vehicles, a 3-tick reticle on foot that spreads as you fire, and red, yellow or white kill marks.
- **Focus.**
  - Michael, Franklin and Trevor's special ability gets the FiveM look (vignette and frames).
  - Lucia gets FiveM's Focus itself.
- **Skills panel and gym.**
  - **K** opens the skills panel.
  - Push-ups and sit-ups on **N** / **Shift+N**.
  - A Health skill raises max health.
- **NPC memory (um_memory).**
  - People remember what you did to them, and witnesses report crimes.
  - Greet on **Y**, Antagonize on **U**.
- **Crash cam (FiveM design).** Roadside shots, a slow-motion hold, the vignette and bars, then back to your view. It hides the HUD, waypoint and vehicle card while it plays. FBICat's own crash cam is still there, off by default.
- **Binoculars (FiveM design)**, on **B**. Smooth zoom, walking, night and thermal vision, and the FiveM lens, reticle and prompts. FBICat's binoculars on the item wheel still work.
- **Radio wheel and stations** from the FiveM version. They replace FBICat's radio carousel. On Demand is off, because it needs a FiveM server.
- **Nitrous** (StreetKings).
  - Hold **Left Alt** (X / Square on a pad) in a car with a bottle: boost, exhaust flames, screen effect.
  - Left Alt also opens FBICat's character wheel, so the wheel stays shut while you drive a car with a bottle. Switch on foot, or use a controller, where nitrous is on X / Square.
  - The bottle refills from drifting, near misses and driving into oncoming traffic.
  - Its gauge sits in the vehicle card's gauge row, after engine and fuel. The row stays centered: while a car has nitrous, the card is drawn with FBICat's own card code, with one more gauge.
  - A "Hold [key] to activate nitrous" tip shows when a bottle is fitted.
  - Bottles are bought per car at Leonida Customs (Street, Sport or Race NOS) and kept by plate (`[Nitrous] FittedBy` in `ViExtras.ini`).
- **Leonida Customs**, the FiveM version's mod shop, in place of FBICat's LS Customs menu. It's its own `ViCustoms.dll`, GPL-3 like the FiveM resource (after renzu_customs by Renzuzu).
  - At the five Los Santos Customs: drive into the garage and stop, and it opens by itself.
  - The full-screen menu: Pro Builds, Cosmetics and Performance tabs, with categories, parts and paint.
  - Every option is previewed on the car and bought one at a time with your cash.
  - The vehicle card shows five stat bars with the factory tick, the green or red change, and road / off-road ratings.
  - The orbit camera has a view per category: drag or use the right stick to look round, scroll or use the triggers to zoom. **R** revs the engine.
  - Respray swatches by colour family, plus pearlescent, wheel colour, custom paint presets, neon and tire smoke.
  - Pro Builds fit a whole set of parts at once.
  - A repair prompt appears when the car is damaged.
  - Prices, labels and menus come straight from the FiveM config (`tools/gen_customs_data.py`). The text uses the shop's own fonts, rendered into glyph atlases.
  - Left out: the mechanic job (stock room, parts, spray cans), engine swaps, turbo kits, tire compounds and the custom colour picker.
  - FBICat's shop still opens the garage doors and keeps GTA's own shop shut; only its menu is kept from opening.
- **Crouch and crawl**, its script ported from the FiveM server's emotes (um_emotes / rpemotes' crouch and crawl, by Mads). The animations are Rockstar's own, from GTA V. It's its own `ViCrouch.dll`, GPL-3.
  - **Left Ctrl crouches.** As on the server, it shares GTA's stealth key: tap once for stealth, again within a second to crouch.
  - **Right Ctrl lies down.** You dive if running. W / S crawl, A / D turn, Space rolls onto your back.
  - Keys are in `ViCrouch.ini`.
- **Wanted stars in the FiveM version's four states:**
  - white while the police can see you;
  - **flashing** when they've lost sight of you but are still chasing (new);
  - hollow when a crime was reported but nobody has seen you;
  - red once they're only searching.
  FBICat's stars went red the moment no cop could see you.
- **Wanted box from the FiveM version** (vice_hud's #wanted). It says what the police are doing, with its star in the same state as the stars, and under it what they have on you that still fits: seen on camera, a gun (while they can see you, or fired in the last 12 s), the outfit you're wearing, your unmasked face (or you and another protagonist), the car you're in. It shows for about seven seconds when your stars or their state change. It replaces FBICat's "cops are searching for you" box.
- **Directional police lights.** A siren nearby lights the screen edges facing it: red and blue lamps firing in bursts with a white pop, like a real lightbar. Only the nearest siren lights the screen, at the edge it's coming from (vice_hud's radius mode), with the lamps closer together than vice_hud's. Ambulances, fire trucks and lifeguard trucks don't count. They replace FBICat's flat police arrows.

### Changed

- `GTA6Hud.ini` changes, so FBICat's features don't double up with the ports:
  - `[Radio] Enabled = False`: the FiveM radio replaces it.
  - `[Hitmarkers] Enabled = False`: the kill marks replace it.
  - `[Interactions] NpcConversations = False`: NPC memory's Greet / Antagonize replace it.
- **Vehicle brand logos** are cropped to the card's title box, top and bottom, as on the FiveM HUD, and sharper (256 px). They no longer reach down into the gauge row.
- **Crash cam:** FBICat's own crash cam no longer runs. The CRASH CAMERA toggle in the HUD's settings menu (hold V) now switches ours, and it's on by default (`[Cinematics] CrashCamera = True`).
- **Radio** opens on GTA's own radio wheel button (Q / D-pad Left, as bound in the game's settings), like GTA's radio, instead of a fixed Q key.
  - GTA's next / previous station keys still work on a keyboard.
  - The radio and its mute key don't react while a menu, the phone, the on-screen keyboard, or one of FBICat's wheels or menus is open.
  - Nitrous also stands down while those are open.
- `[PoliceArrow] Enabled = False` in `GTA6Hud.ini`: the police lights replace it.
- `[Traffic] VarietyEnabled = False` in `GTA6Hud.ini`: see the traffic crash under Fixed. Passing cars keep the game's own looks.
- **3D minimap routes** keep the minimap's own colours: your waypoint cyan, a mission objective pink. `[Minimap] WaypointRoutePink = True` in `ViExtras.ini` swaps them: your waypoint pink, a mission vice_hud's teal.
- All 2.5 art is drawn from game texture dictionaries (`vix_*.ytd` in the vihudfonts pack), not from Script Hook V textures.

### Fixed

- **Legacy: stuck on a black screen after dying.** The game's respawn now takes over after the WASTED screen. Confirmed in game.
- **Enhanced: "texture creation failed" crashes** when 2.5 art loaded. Fixed by the texture dictionaries above.
- **Radio:** the radio showed twice, with the On Demand switch.
- **Radio didn't open:** a check for the on-screen keyboard read as "typing" all the time, which also blocked nitrous. It's removed. A blocked radio press is now logged with what had the keys.
- **Double vehicle card:** FBICat's card could come back on (his settings reload switches it on) while the nitrous card was drawn too. Now only one is ever drawn.
- **Wanted box flickering:** in a close chase the game restarts its "lost sight" timer every frame or two. FBICat's "cops are searching for you" box followed it and flashed. The FiveM version's wanted box (above) replaces it. It only comes up when your stars or their state change, so it can't flicker.
- **Enhanced: random crashes, as in 2.2.** On game build 1.0.1158.16, Script Hook V .NET Enhanced looks for the player's info at the wrong spot in memory. Every read of it can crash the game, and FBICat's police features read it every frame. The 2.5 scripts switch that read off on Enhanced, so it returns a safe default for every script. They also leave the player alone during loading screens and character switches, and for 3 seconds after them or a script reload.
- **Enhanced: gym and NPC memory messages didn't show.** They used a notification call that Script Hook V .NET Enhanced doesn't have. They now post to GTA's own feed, on both versions.
- **Found in a review against the FiveM version, before release:**
  - **Leonida Customs:**
    - Lucia's cash works: she pays from her host character's money.
    - Cars without a mod kit are never sent a mod, which could crash the game.
    - Keys already held as the menu opens are ignored.
    - The screen always fades back in.
    - The camera keeps your view between screens.
    - A factory window tint is kept.
    - Small layout fixes against the shop's CSS.
  - **Minimap (`WaypointRoutePink = True`):** the swapped colours are vice_hud's pink (#fc74a4) for your waypoint and teal (#47aba7) for a mission, its destination disc included.
  - **Radio:** unmuting gives the muted car its radio back.
  - **Switches and loading screens:** the radio wheel's slow motion, Focus, the nitrous boost and crouch/crawl are all ended cleanly when one starts.
  - **Crouch / crawl:**
    - It belongs to the ped who took it.
    - Its keys only work with the game in front, and not while a menu, the phone or the binoculars are up.
    - A script reload no longer leaves weapon switching blocked.
  - **Saves:** skills XP, NPC memory and plate data are written safely. A save that can't be read is kept under a new name instead of being overwritten.
  - **Skills:** the driving skill's grip change is undone through the car model, even after the car is gone. Single player shares handling across every car of a model.
  - **NPC memory:**
    - Greet / Antagonize act once per key press.
    - In a car, only a hit counts as an attack.
  - **Nitrous:** the "Hold [key]" tip goes when you leave the car.
- **RegisterPatch 1.4 is safe to install again.** Each pack is only added to `dlclist.xml` when it isn't listed yet, so installing it twice, or over an older version, can't list a pack twice. Older versions added the lines every time.
- **Crash or freeze while driving.** FBICat's traffic variety gives passing cars custom paint, wheels and maxed-out mods. Fitting those mods (SET_VEHICLE_MOD) crashes the game on some cars. It's switched off (`[Traffic] VarietyEnabled = False`).

- **Lucia (Legacy): lighter ped pack (v5).** 2.2's full-size Lucia could crash outdoors while the world streamed in.
  - Meshes are decimated and textures are Rockstar-sized (512, spec 256).
  - Only the head keeps its own skeleton, as on Rockstar's freemode peds.
  - Her default outfit is 18.4 MiB, down from ~110 MB.
  - Enhanced keeps 2.2's pack.
- **Lucia isn't lent to Franklin, Michael or Trevor near their mission markers any more** (`GTA6Hud.ini` `[Lucia] MissionRange = 0`). You become Lucia when you pick her on the character wheel.

### Removed

- The skills **level-up card** (FiveM's #skillup). It isn't part of GTA VI.

### Known issues

- **Switching to Michael or Trevor from the HUD's character wheel may not work, on Legacy and Enhanced.** The game ignores the wheel's simulated key press. A fix is being tested.
- **The minimap's waypoint route can look shaky.** A smoother route is being tested.
- **Greet / Antagonize** are keyboard only for now (Y / U).
- **Enhanced:** the random crashes have a fix (above), still being confirmed in play. If Enhanced crashes, send the logs listed in the install guide.
- **Lucia:** switching to her on Legacy can freeze the game. A fix is being tested.
- **Enhanced with NaturalVision Enhanced:** NVE's own `dlclist.xml` and `gameconfig.xml` take over from the RegisterPatch, so the HUD's packs don't load. Raising NVE's pools as well makes the game run out of memory while loading Story Mode. `INSTALL-ENHANCED.md` says what to add to NVE's `dlclist.xml` and to leave its `gameconfig.xml` alone.

## 2.2.0 (2026-10-18)

- **Minimap shows in fullscreen.** Added `ViMinimap.asi`, the plugin that draws it inside the game. It was missing from the 2.1 packages.
- **Lucia no longer crashes the game on load.** Register patch 1.3 raises `MetaDataStore` to 4000, plus the texture, model and expression pools, and registers her packs. Her ped pack was fixed for Legacy and Enhanced.
- **Commercial music removed.** Add your own tracks in `scripts\Hud\Music` and `scripts\Hud\Radio\CustomMusic`.
- Packaged as three OIVs (Legacy, Enhanced, RegisterPatch), built by `tools/build_oiv.py`.
- New README and beginner install guides for Legacy and Enhanced (Enhanced uses SHVDNE).

## Base

FBICat's 2.1 packages.
