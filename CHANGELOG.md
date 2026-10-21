# Fork changelog

This is a list of what this fork changed and fixed on top of **FBICat's** GTA VI HUD for GTA V single player. Everything not listed here is FBICat's work, as he made it.

Newest first. "Unreleased" is what's in testing and not yet on the Releases page.

> [!IMPORTANT]
> ### 💬 Need help? Join our support Discord
> This version is a **fork of FBICat's VI HUD** with extra features, so it has its own support:
> **[discord.gg/C7gE5SHEzH](https://discord.gg/C7gE5SHEzH)**, or a [GitHub issue](https://github.com/Geckomacabre/GTA-VI-Inspired-HUD/issues).
> FBICat's Discord is there for his original mod, so please bring questions about this version to ours. Thank you!

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
