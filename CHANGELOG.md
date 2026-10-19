# Fork changelog

This is a list of what this fork changed and fixed on top of **FBICat's** GTA VI HUD for GTA V single player. Everything not listed here is FBICat's work, as he made it.

Newest first. "Unreleased" is what's in testing and not yet on the Releases page.

## 2.5.1 (unreleased, in testing)

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
- **Your own music on the radio wheel:** put `.mp3` / `.wav` / `.wma` / `.m4a` files in `scripts\Hud\Radio\CustomMusic` and they're a station called MY MUSIC, just before RADIO OFF; a subfolder with tracks in it is a station of its own, named after the folder. The song and artist show under the name, GTA's next / previous track keys skip, and it stops when you leave the car. It plays through FBICat's own custom music player. `[Radio] CustomMusicVolume` sets how loud.
  - A station with no logo (yours, or one the game adds) gets a tile with its name on it, drawn live, so on Enhanced too.
- **Chase music takes over from the car radio:** while you're wanted and chase music plays (the game's wanted music, or GTA6Hud's `[Music]` tracks), the car radio goes quiet, and comes back when it ends. `[Radio] ChaseMusicTakesOver = False` lets them play together.
- **Minimap routes in the FiveM HUD's colours:** your own waypoint's route is teal (#47aba7) and a mission's or a race's is pink (#fc74a4), each with the FiveM HUD's lighter rim. They were the minimap's own light blue and a different pink. `[Minimap] RouteColours = False` in `ViExtras.ini` brings those back; it replaces `WaypointRoutePink`.
- **The turn-by-turn box matches the route:** the square behind the arrow is teal like your waypoint's route, not FBICat's pink. The box follows your own waypoint, so it's always teal. The square is recoloured in FBICat's own texture (`hud_gps_bg`, in the HUD pack's `vipause`; the rest of it is his, pixel for pixel), so it fades in and out cleanly.
- **LS Customs shows on the map from any distance**, like the stores, barbers and Ammu-Nation: blip 72 is added to `GTA6Hud.ini` `[MapIcons] Sprites`. It used to appear only once you were close.
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
