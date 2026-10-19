# Fork changelog

This is a list of what this fork changed and fixed on top of **FBICat's** GTA VI HUD for GTA V single player. Everything not listed here is FBICat's work, as he made it.

Newest first. "Unreleased" is what's in testing and not yet on the Releases page.

## 2.5.1 (unreleased, in testing)

### Changed

- **Nitrous is on Left Shift** (X / Square on a pad). FiveM's Left Alt is also GTA's character wheel. `[Nitrous] KeyboardControl = 19` puts it back on Left Alt, and the wheel then stays shut while you drive a car with a bottle.
- **Lucia is a separate download:** `GTA-VI-Inspired-HUD-Lucia-Legacy.oiv` / `-Lucia-Enhanced.oiv`. Install it after the HUD and the RegisterPatch.
  - The HUD package is now the HUD alone, and much smaller.
  - Lucia's package adds her two packs to `dlclist.xml`, each only if it isn't listed yet.
  - The RegisterPatch (1.5) registers only the HUD's packs. Installs that already list Lucia's keep them.
  - Without Lucia installed, picking her on the character wheel shows FBICat's notice that she isn't registered, and nothing else changes.

### Fixed

- **WASTED / BUSTED only showed once per session.** Releasing the screen's text movie threw an error, which left the screen marked as still playing, so later deaths and arrests got the game's own screen.
- **WASTED in a vehicle:** the camera frames the vehicle from outside its body, follows it if it's still rolling, and comes back if the crash cam hands the view back at the same moment.

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
- **Crouch and crawl** from the FiveM server's emotes (um_emotes / rpemotes, by Mads). It's its own `ViCrouch.dll`, GPL-3.
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
