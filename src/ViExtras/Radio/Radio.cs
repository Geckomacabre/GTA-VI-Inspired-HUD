// ---------------------------------------------------------------------------
// Radio: the FiveM version's radio wheel (resources/vi_radio), for single player.
//
// vi_radio is itself a FiveM port of "VI Radio", the GTA V single-player mod by
// sej0bec (GTA5-Mods user sbc17, https://www.gta5-mods.com/scripts/vi-radio-v1-0):
// the concept, the HUD design, the station logos, icons and sound effects are
// sej0bec's, redistributed with credit. Nine of the logos (West Coast Talk
// Radio, Blonded Los Santos 97.8, LS Underground Radio, iFruit Radio, Still
// Slipping Los Santos, Kult FM, The Music Locker, Media Player, MOTOMAMI Los
// Santos) come from Logopedia (logos.fandom.com); the station names and logos
// are Rockstar Games' trademarks. Anton is by Vernon Adams (SIL Open Font
// License 1.1). vi_radio's own code, which this follows, is under the PolyForm
// Noncommercial License 1.0.0 (LICENSE.md in this folder); the artwork and
// sounds are not covered by that licence.
// ---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// vi_radio's client.lua, for single player.
    ///
    /// Hold the game's own radio wheel button in a vehicle (Q, D-pad Left on a
    /// pad, or wherever the game's settings put it), as for GTA's radio wheel,
    /// which this one replaces: the world drops into slow motion behind a blur,
    /// the minimap goes, and the station carousel comes up at the top of the
    /// screen. Left / Right or the mouse wheel step through the stations (right
    /// stick, D-pad Right / Up on a pad), each one tuned as it is reached. Space
    /// (D-pad Down) mutes while the wheel is open; O mutes anywhere, flashing the
    /// HUD. Letting go closes it. GTA's next / previous station keys still
    /// change station without the wheel on a keyboard. Nothing opens or mutes
    /// while something else has the keys (HudBridge.UiInTheWay): a GTA6Hud
    /// menu or wheel, the phone, the on-screen keyboard, a 2.5 panel.
    ///
    /// On Demand (YouTube through the car radio) needs xsound and a server, so it
    /// is off here exactly as vi_radio switches it off when xsound is missing:
    /// the switch keys do nothing and the left icon is what the page shows with
    /// On Demand off.
    ///
    /// GTA6Hud has a radio wheel of its own on the same control. This one stands
    /// down while GTA6Hud.ini [Radio] Enabled is True; set it False to use it.
    /// </summary>
    public sealed class RadioWheel : Script
    {
        // client.lua's control ids
        private const int VehRadioWheel = 85, VehNextRadio = 81, VehPrevRadio = 82, VehNextTrack = 83, VehPrevTrack = 84;
        private const int Phone = 27, Handbrake = 76;
        private const int CellUp = 172, CellDown = 173, CellLeft = 174, CellRight = 175;
        private const int WheelNext = 14, WheelPrev = 15;
        private const int LookLR = 1, LookUD = 2, RightX = 220, RightY = 221;
        private const int CharacterWheel = 19, MultiplayerInfo = 20, Headlight = 74, VehNextWeapon = 99, VehPrevWeapon = 100;
        private const int HudRadioStations = 16;
        private const string OffName = "OFF";

        // ---- settings, [Radio] in ViExtras.ini (defaults: vi_radio's config.lua) --------
        private readonly bool _enabled;
        private readonly Keys _key, _muteKey;
        private readonly bool _scrollRadio, _autoRepeat;
        private readonly int _autoRepeatDelay, _autoRepeatRate;
        private readonly bool _pad, _lockCamera;
        private readonly float _deadzone;
        private readonly bool _muteInWheel, _disableVanillaControls, _driverOnly, _allowOnFoot;
        private readonly float _timeScale;
        private readonly int _timeScaleFade;
        private readonly string _timecycle;
        private readonly float _timecycleStrength;
        private readonly bool _hiDof, _hideRadar, _disableRadioHud, _sounds, _hudEnabled, _includeOff, _loud;
        private readonly float _soundVolume;
        private readonly string _offLabel;
        private readonly HashSet<string> _hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly string[] _mutingScenes;
        // Config.OnDemand.enabled is false: client.lua's startup check switches it
        // off when xsound is not started, and single player has no xsound. Every
        // branch it gates (the switch keys, the panel, the commit on release, the
        // ON DEMAND label) is therefore left out.

        // ---- state, as client.lua's ------------------------------------------------------
        private readonly Dictionary<string, RadioHud.Item> _table = new Dictionary<string, RadioHud.Item>(StringComparer.OrdinalIgnoreCase);
        private string _tableOffLabel;
        private readonly RadioHud _hud;
        private List<RadioHud.Item> _stations = new List<RadioHud.Item>();
        private int _index;
        private bool _wheelOpen, _muted, _openedByControl;
        // (client.lua's stationPicked / panelOnClose only steer On Demand, which is off.)
        private int _mutedVehicle;
        private long _navHeldSince, _navLastStep;
        private int _navDir;
        private bool _stickLatched, _muteWasDown;
        private bool _started;
        private long _nextHousekeeping, _nextScenes;
        private string _lastStation = "\0";
        private readonly HashSet<string> _suspendedScenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool _minimapHeld;
        private bool _timeOwned;
        private float _timeScaleNow = 1f;
        private long _lastEase;
        private bool _standingDown;
        private string _lastError;

        /// <summary>exports IsOpen / IsMuted.</summary>
        public static bool IsOpen { get; private set; }
        public static bool IsMuted { get; private set; }

        public RadioWheel()
        {
            Paths.Init(BaseDirectory);
            const string S = "Radio";
            _enabled = Config.Get(S, "Enabled", true);
            _key = Config.Get(S, "Key", Keys.None);
            _muteKey = Config.Get(S, "MuteKey", Keys.O);
            _scrollRadio = Config.Get(S, "ScrollRadio", true);
            _autoRepeat = Config.Get(S, "ArrowAutoRepeat", true);
            _autoRepeatDelay = Config.Get(S, "AutoRepeatDelay", 350);
            _autoRepeatRate = Config.Get(S, "AutoRepeatRate", 120);
            _pad = Config.Get(S, "Controller", true);
            _deadzone = Config.Get(S, "ControllerDeadzone", 0.5f);
            _lockCamera = Config.Get(S, "ControllerLockCamera", true);
            _muteInWheel = Config.Get(S, "MuteInWheel", true);
            _disableVanillaControls = Config.Get(S, "DisableVanillaRadioControls", true);
            _driverOnly = Config.Get(S, "DriverOnly", true);
            _allowOnFoot = Config.Get(S, "AllowOnFoot", false);
            _timeScale = Config.Get(S, "TimeScale", 0.075f);
            _timeScaleFade = Config.Get(S, "TimeScaleFade", 180);
            _timecycle = Config.Get(S, "Timecycle", "hud_def_blur") ?? "";
            _timecycleStrength = Config.Get(S, "TimecycleStrength", 1f);
            _hiDof = Config.Get(S, "HiDof", true);
            _hideRadar = Config.Get(S, "HideRadar", true);
            _disableRadioHud = Config.Get(S, "DisableRadioHUD", true);
            _sounds = Config.Get(S, "RadioSounds", true);
            _soundVolume = Config.Get(S, "SoundVolume", 50) / 100f;
            _hudEnabled = Config.Get(S, "Hud", true);
            _includeOff = Config.Get(S, "IncludeOff", true);
            _offLabel = Config.Get(S, "OffLabel", "RADIO OFF") ?? "RADIO OFF";
            _loud = Config.Get(S, "LoudRadio", true);
            foreach (string name in Split(Config.Get(S, "HiddenStations", ""))) _hidden.Add(name);
            _mutingScenes = Split(Config.Get(S, "RadioMutingScenes", "CHARACTER_CHANGE_IN_SKY_SCENE")).ToArray();
            _hud = new RadioHud(Config.Get(S, "SlideTime", 110), Config.Get(S, "MuteDim", true), Config.Get(S, "SideIcons", true),
                !string.Equals(Config.Get(S, "Subtitle", "genre"), "none", StringComparison.OrdinalIgnoreCase));

            LoadTable();
            if (_enabled)
                Log.Write("Radio", "On Demand is disabled: it plays through xsound on a FiveM server, which single player does not have.");

            Tick += OnTick;
            Aborted += (s, e) => Cleanup();
        }

        private static IEnumerable<string> Split(string list) =>
            (list ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Where(x => x.Length > 0);

        // Config.Stations, as tools/render/radio.py wrote it beside the art.
        private void LoadTable()
        {
            string path = Path.Combine(Paths.Root, "Radio", "stations.txt");
            try
            {
                foreach (string line in File.ReadAllLines(path))
                {
                    if (line.StartsWith("#") || line.Trim().Length == 0) continue;
                    string[] p = line.Split('\t');
                    if (p.Length < 5) continue;
                    if (p[0] == OffName) { _tableOffLabel = p[1]; continue; }
                    _table[p[0]] = new RadioHud.Item
                    {
                        Name = p[0], Label = p[1], Genre = p[2].Length > 0 ? p[2] : null,
                        Key = p[3].Length > 0 ? p[3] : null, GenreKey = p[4].Length > 0 ? p[4] : null,
                    };
                }
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "station table " + path + ": " + ex.Message);
            }
        }

        // ---- the frame --------------------------------------------------------------------

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now)
            {
                if (_wheelOpen) CloseWheel();   // its slow motion and blur must not outlast a switch or loading screen
                return;
            }
            if (!_enabled) return;
            try
            {
                long now = RealTime.Ms;

                // GTA6Hud's radio wheel uses the same control. Both at once would
                // fight over it, the station and the slow motion: stand down.
                if (Gta6HudRadio.Enabled)
                {
                    if (!_standingDown)
                    {
                        _standingDown = true;
                        Log.Write("Radio", "GTA6Hud.ini [Radio] Enabled is True, so GTA6Hud's radio is in charge; set it False to use this one.");
                    }
                    if (_wheelOpen) CloseWheel();
                    EaseTime(now);
                    return;
                }
                _standingDown = false;

                if (!_started)
                {
                    // the startup thread: build the list and hand it to the page
                    _started = true;
                    BuildStations();
                    PushState(true);
                }

                SuppressVanilla();
                if (now >= _nextHousekeeping)
                {
                    _nextHousekeeping = now + 500;
                    Housekeeping();
                }
                MainLoop(now);
                EaseTime(now);
                if (now >= _nextScenes)
                {
                    _nextScenes = now + 500;
                    MutingScenes();
                }
                if (_hudEnabled && !HudBridge.Paused && !HudBridge.Busy) _hud.Draw();
            }
            catch (Exception ex)
            {
                if (ex.Message != _lastError) Log.Write("Radio", ex.ToString());
                _lastError = ex.Message;
            }
        }

        // ---- helpers (client.lua) ------------------------------------------------------------

        private int CurrentVehicle()
        {
            int ped = Function.Call<int>(Hash.PLAYER_PED_ID);
            if (!Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, ped, false)) return 0;
            int veh = Function.Call<int>(Hash.GET_VEHICLE_PED_IS_IN, ped, false);
            if (veh == 0) return 0;
            if (_driverOnly && Function.Call<int>(Hash.GET_PED_IN_VEHICLE_SEAT, veh, -1, false) != ped) return 0;
            return veh;
        }

        private bool CanUseRadio()
        {
            if (CurrentVehicle() != 0) return true;
            return _allowOnFoot && Function.Call<bool>(Hash.IS_MOBILE_PHONE_RADIO_ACTIVE);
        }

        private static string PlayerStation() => Function.Call<string>(Hash.GET_PLAYER_RADIO_STATION_NAME);

        private RadioHud.Item StationInfo(string name)
        {
            if (_table.TryGetValue(name, out RadioHud.Item cfg))
                return new RadioHud.Item { Name = name, Label = cfg.Label, Genre = cfg.Genre, Key = cfg.Key, GenreKey = cfg.GenreKey };
            // Unknown / add-on station: a readable version of the raw name.
            string pretty = Regex.Replace(name, "^RADIO_[0-9]+_", "");
            pretty = Regex.Replace(pretty, "^DLC_", "").Replace('_', ' ');
            return new RadioHud.Item { Name = name, Label = pretty };
        }

        private void BuildStations()
        {
            var list = new List<RadioHud.Item>();
            bool seenOff = false;
            int count = Function.Call<int>(Hash.GET_NUM_UNLOCKED_RADIO_STATIONS);
            for (int i = 0; i < count; i++)
            {
                string name = Function.Call<string>(Hash.GET_RADIO_STATION_NAME, i);
                if (string.IsNullOrEmpty(name)) continue;
                if (name == OffName) seenOff = true;
                else if (!name.StartsWith("HIDDEN_", StringComparison.Ordinal) && !_hidden.Contains(name))
                    list.Add(StationInfo(name));
            }
            if (_includeOff || seenOff)
            {
                // the RADIO OFF tile; its art was rendered for vi_radio's OffLabel
                bool baked = _tableOffLabel != null && _tableOffLabel == _offLabel;
                list.Add(new RadioHud.Item { Name = OffName, Label = _offLabel, Off = true, Key = baked ? "off" : null });
            }
            _stations = list;

            // keep the highlight on whatever is actually playing
            string playing = PlayerStation() ?? OffName;
            for (int i = 0; i < _stations.Count; i++)
                if (string.Equals(_stations[i].Name, playing, StringComparison.OrdinalIgnoreCase)) { _index = i; break; }
            if (_index >= _stations.Count) _index = 0;
        }

        private void ApplyStation(RadioHud.Item entry)
        {
            int veh = CurrentVehicle();
            if (veh != 0)
            {
                Function.Call(Hash.SET_VEH_RADIO_STATION, veh, entry.Name);
                // vi_radio makes the car's radio carry outside it, as the game's
                // "big speaker" cars do. (Its invisible stand-in driver, which keeps
                // an empty car's radio playing on a server, is not ported: GTA6Hud's
                // [Radio] OutsideCar already keeps your car's radio audible on foot.)
                if (_loud) Function.Call(Hash.SET_VEHICLE_RADIO_LOUD, veh, entry.Name != OffName);
            }
            Function.Call(Hash.SET_RADIO_TO_STATION_NAME, entry.Name);
        }

        private void ApplyMute()
        {
            int veh = CurrentVehicle();
            // the car muted before gets its radio back: on unmute, and when the mute moves to another
            // car (unmuting on foot or from another car used to leave it silent for good)
            if (_mutedVehicle != 0 && (!_muted || veh != _mutedVehicle))
            {
                if (Function.Call<bool>(Hash.DOES_ENTITY_EXIST, _mutedVehicle)) Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, _mutedVehicle, true);
                _mutedVehicle = 0;
            }
            if (veh != 0)
            {
                Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, veh, !_muted);
                _mutedVehicle = _muted ? veh : 0;
            }
            // The mobile radio is only this wheel's business when it may be used on
            // foot; otherwise unmuting would switch the on-foot radio on in story mode.
            if (_allowOnFoot) Function.Call(Hash.SET_MOBILE_RADIO_ENABLED_DURING_GAMEPLAY, !_muted);
            IsMuted = _muted;
        }

        // ---- NUI (the HUD) ------------------------------------------------------------------

        private void PushState(bool withList)
        {
            if (!_hudEnabled) return;
            _hud.State(withList ? new List<RadioHud.Item>(_stations) : null, _index, _muted, _wheelOpen);
        }

        private void PlaySound(string file)
        {
            if (!_sounds) return;
            RadioSound.Play(Path.Combine(Paths.Root, "Radio", file), _soundVolume);
        }

        // ---- the wheel ------------------------------------------------------------------------

        private void Step(int dir)
        {
            if (_stations.Count == 0) return;
            _index += dir;
            if (_index < 0) _index = _stations.Count - 1;
            else if (_index >= _stations.Count) _index = 0;

            // Browsing unmutes. Done before the station is applied (client.lua does
            // it after), so the new station is not set on a radio that is still
            // switched off.
            if (_muted)
            {
                _muted = false;
                ApplyMute();
            }
            // No On Demand session can exist, so every station is applied as it is reached.
            ApplyStation(_stations[_index]);
            PushState(false);
        }

        private bool _wheelButtonHeld;
        private bool _blockLogged;

        private void OpenWheel()
        {
            if (_wheelOpen || !CanUseRadio()) return;
            BuildStations();
            _wheelOpen = true;
            IsOpen = true;
            _navDir = 0;
            _navHeldSince = _navLastStep = 0;
            PlaySound("radioopen.wav");
            // GTA6Hud's in-car weapon wheel listens on the button that steps the
            // station here (see HudBridge.HoldVehicleWheelButton).
            if (!_wheelButtonHeld)
            {
                _wheelButtonHeld = true;
                HudBridge.HoldVehicleWheelButton(true);
            }
            if (_hideRadar && !_minimapHeld)
            {
                // the 3D minimap; DISPLAY_RADAR below covers the game's own
                _minimapHeld = true;
                HudBridge.HoldMinimapHidden(true);
            }
            PushState(true);
        }

        private void CloseWheel()
        {
            if (!_wheelOpen) return;
            _wheelOpen = false;
            IsOpen = false;
            _stickLatched = false;
            _openedByControl = false;
            PlaySound("radioclose.wav");

            if (_timecycle.Length > 0) Function.Call(Hash.CLEAR_TIMECYCLE_MODIFIER);
            if (_hideRadar) Function.Call(Hash.DISPLAY_RADAR, true);
            if (_minimapHeld)
            {
                _minimapHeld = false;
                HudBridge.HoldMinimapHidden(false);
            }
            if (_wheelButtonHeld)
            {
                _wheelButtonHeld = false;
                HudBridge.HoldVehicleWheelButton(false);
            }
            // Releasing the key commits an On Demand choice in client.lua; with On
            // Demand off there is nothing to commit.
            PushState(false);
        }

        private void ToggleMute()
        {
            if (!CanUseRadio()) return;
            _muted = !_muted;
            ApplyMute();
            PushState(false);
            // brief HUD flash so the player sees the mute state change
            if (!_wheelOpen && _hudEnabled) _hud.Flash(_muted);
        }

        private static bool JustPressed(int control) => Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, control);
        private static bool Pressed(int control) => Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, control);
        private static void Disable(int control) => Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, control, true);

        private void MainLoop(long now)
        {
            Ped ped = Game.Player.Character;
            bool blocked = HudBridge.UiInTheWay || OtherFeature.CrashCamPlaying || Binoculars.IsActive
                           || ped == null || !ped.Exists() || ped.IsDead;

            // The game's own radio wheel control, on the keyboard and the pad
            // alike, so it follows the player's binds as GTA's radio does
            // (SuppressVanilla keeps GTA's own wheel shut). [Radio] Key adds a
            // keyboard key of its own if wanted; Controller = False leaves pads out.
            bool usingPad = Glyphs.UsingPad;
            bool keyHeld = !usingPad && _key != Keys.None && Game.IsKeyPressed(_key);
            bool ctrlHeld = (_pad || !usingPad) && Pressed(VehRadioWheel);
            bool wantOpen = (keyHeld || ctrlHeld) && !blocked;
            // Pressed but kept shut: say why, once a press.
            if ((keyHeld || ctrlHeld) && blocked && !_wheelOpen && !_blockLogged)
            {
                _blockLogged = true;
                Log.Write("Radio", "the radio button was pressed while " + (HudBridge.InTheWay() ?? (OtherFeature.CrashCamPlaying ? "the crash cam" : Binoculars.IsActive ? "the binoculars" : "the player could not use it")) + " had the keys");
            }
            if (!keyHeld && !ctrlHeld) _blockLogged = false;

            if (wantOpen && !_wheelOpen)
            {
                // a pad steps with the stick and D-pad; a keyboard with the arrows
                _openedByControl = usingPad;
                OpenWheel();
            }
            else if (_wheelOpen && (!wantOpen || !CanUseRadio()))
            {
                CloseWheel();
            }

            // vi_radio_mute (O), anywhere
            bool muteDown = _muteKey != Keys.None && Game.IsKeyPressed(_muteKey);
            if (muteDown && !_muteWasDown && !blocked) ToggleMute();
            _muteWasDown = muteDown;

            if (!_wheelOpen) return;

            // effects
            if (_timecycle.Length > 0)
            {
                Function.Call(Hash.SET_TIMECYCLE_MODIFIER, _timecycle);
                Function.Call(Hash.SET_TIMECYCLE_MODIFIER_STRENGTH, _timecycleStrength);
            }
            if (_hiDof) Function.Call(Hash.SET_USE_HI_DOF);
            if (_hideRadar) Function.Call(Hash.DISPLAY_RADAR, false);

            // swallow the inputs being replaced
            Disable(VehRadioWheel);
            Disable(VehNextRadio);
            Disable(VehPrevRadio);
            Disable(VehNextTrack);
            Disable(VehPrevTrack);
            Disable(Phone);
            Disable(Handbrake);   // Spacebar = mute here
            Disable(CellUp);
            Disable(CellDown);
            Disable(CellLeft);
            Disable(CellRight);
            if (_scrollRadio)
            {
                Disable(WheelNext);
                Disable(WheelPrev);
                // single player: the car's own drive-by weapon select is on the wheel too
                Disable(VehNextWeapon);
                Disable(VehPrevWeapon);
            }
            // Single player only: the keys the wheel uses also open the character
            // wheel (D-pad Down, held) and switch the headlights (D-pad Right).
            Disable(CharacterWheel);
            Disable(MultiplayerInfo);
            Disable(Headlight);

            bool pad = _openedByControl;
            if (pad && _lockCamera)
            {
                Disable(LookLR);
                Disable(LookUD);
                Disable(RightX);
                Disable(RightY);
            }

            // (The radio / On Demand switch on up / down only works with On Demand on.)

            // Mute: Spacebar on a keyboard, D-pad Down on a pad.
            if (_muteInWheel)
            {
                if (pad) { if (JustPressed(CellDown)) ToggleMute(); }
                else if (JustPressed(Handbrake)) ToggleMute();
            }

            // navigation
            int dir = 0, held = 0;
            if (pad)
            {
                // D-pad Left holds the wheel open, so it cannot also mean "previous":
                // the right stick, D-pad Right and D-pad Up.
                float axis = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, RightX);
                if (Math.Abs(axis) >= _deadzone)
                {
                    held = axis > 0f ? 1 : -1;
                    if (!_stickLatched)
                    {
                        dir = held;
                        _stickLatched = true;
                    }
                }
                else
                {
                    _stickLatched = false;
                    if (JustPressed(CellRight)) dir = 1;
                    else if (JustPressed(CellUp)) dir = -1;
                    if (Pressed(CellRight)) held = 1;
                    else if (Pressed(CellUp)) held = -1;
                }
            }
            else
            {
                _stickLatched = false;
                if (JustPressed(CellRight)) dir = 1;
                else if (JustPressed(CellLeft)) dir = -1;
                else if (_scrollRadio && JustPressed(WheelNext)) dir = 1;
                else if (_scrollRadio && JustPressed(WheelPrev)) dir = -1;

                if (Pressed(CellRight)) held = 1;
                else if (Pressed(CellLeft)) held = -1;
            }

            if (dir != 0)
            {
                Step(dir);
                _navDir = dir;
                _navHeldSince = _navLastStep = now;
            }
            else if (_autoRepeat && held != 0 && held == _navDir)
            {
                if (now - _navHeldSince > _autoRepeatDelay && now - _navLastStep > _autoRepeatRate)
                {
                    Step(held);
                    _navLastStep = now;
                }
            }
            else if (held == 0)
            {
                _navDir = 0;
            }
        }

        // ---- slow motion -------------------------------------------------------------------
        // client.lua's own thread: eased toward TimeScale while the wheel is open,
        // back to 1 when it closes, TimeScaleFade ms either way (client.lua steps
        // 16 / TimeScaleFade a frame, i.e. at 60 fps; here by real time). It
        // leaves time alone while the crash cam or Focus is running it.

        private void EaseTime(long now)
        {
            long dt = Math.Max(0, Math.Min(100, now - _lastEase));
            _lastEase = now;
            if (_timeScale >= 1f) return;
            if (OtherFeature.CrashCamPlaying || OtherFeature.FocusActive)
            {
                _timeOwned = false;
                return;
            }
            if (!_timeOwned)
            {
                if (!_wheelOpen) return;
                _timeOwned = true;
                float current = Game.TimeScale;
                _timeScaleNow = current > 0.001f && current <= 1f ? current : 1f;
            }

            float target = _wheelOpen ? _timeScale : 1f;
            if (Math.Abs(_timeScaleNow - target) > 0.001f)
            {
                float step = dt / (float)Math.Max(_timeScaleFade, 1);
                _timeScaleNow = _timeScaleNow < target ? Math.Min(target, _timeScaleNow + step) : Math.Max(target, _timeScaleNow - step);
                Function.Call(Hash.SET_TIME_SCALE, _timeScaleNow);
            }
            else if (!_wheelOpen)
            {
                if (_timeScaleNow != 1f) Function.Call(Hash.SET_TIME_SCALE, 1f);
                _timeScaleNow = 1f;
                _timeOwned = false;
            }
        }

        // ---- vanilla radio HUD and controls ----------------------------------------------------
        // The stock wheel shares its button with this one (Q, D-pad Left), so it is
        // suppressed every frame, not only while open. client.lua hides HUD
        // component 20 here, which is the weapon wheel's stats panel; the radio
        // stations HUD it means (HUD_RADIO_STATIONS) is 16.

        private void SuppressVanilla()
        {
            if (_disableRadioHud) Function.Call(Hash.HIDE_HUD_COMPONENT_THIS_FRAME, HudRadioStations);
            if (_disableVanillaControls)
            {
                Disable(VehRadioWheel);
                // A keyboard's next / previous station keys keep working as in GTA
                // (the HUD follows the station). On a pad they share D-pad Left
                // with the wheel, which owns it here.
                if (Glyphs.UsingPad)
                {
                    Disable(VehNextRadio);
                    Disable(VehPrevRadio);
                }
            }
        }

        // ---- housekeeping ------------------------------------------------------------------------
        // Re-apply mute when the player changes vehicle, and keep the idle HUD on
        // whatever station is playing.

        private void Housekeeping()
        {
            if (_muted && CurrentVehicle() != _mutedVehicle) ApplyMute();

            string playing = PlayerStation();
            if (playing != _lastStation)
            {
                _lastStation = playing;
                if (!_wheelOpen && _stations.Count > 0)
                {
                    for (int i = 0; i < _stations.Count; i++)
                        if (string.Equals(_stations[i].Name, playing, StringComparison.OrdinalIgnoreCase)) { _index = i; break; }
                    PushState(false);
                }
            }
        }

        // ---- audio scenes that silence the radio ---------------------------------------------------
        // Config.RadioMutingScenes: a scene some other script leaves running that
        // takes the car radio down with it is stopped while a station plays, and
        // started again when it stops. Left alone during a character switch,
        // where the game runs CHARACTER_CHANGE_IN_SKY_SCENE itself and would
        // otherwise have it started again after it had finished with it.

        private void MutingScenes()
        {
            if (_mutingScenes.Length == 0) return;
            string station = CurrentVehicle() != 0 ? PlayerStation() : null;
            bool playing = station != null && station != OffName && !_muted;
            if (playing)
            {
                if (HudBridge.Busy) return;
                foreach (string scene in _mutingScenes)
                {
                    if (Function.Call<bool>(Hash.IS_AUDIO_SCENE_ACTIVE, scene))
                    {
                        Function.Call(Hash.STOP_AUDIO_SCENE, scene);
                        _suspendedScenes.Add(scene);
                    }
                }
            }
            else if (_suspendedScenes.Count > 0)
            {
                RestoreScenes();
            }
        }

        private void RestoreScenes()
        {
            foreach (string scene in _suspendedScenes)
                if (!Function.Call<bool>(Hash.IS_AUDIO_SCENE_ACTIVE, scene)) Function.Call(Hash.START_AUDIO_SCENE, scene);
            _suspendedScenes.Clear();
        }

        // ---- cleanup ------------------------------------------------------------------------------

        private void Cleanup()
        {
            try
            {
                RestoreScenes();
                if (_timeOwned) Function.Call(Hash.SET_TIME_SCALE, 1f);
                if (_wheelOpen)
                {
                    if (_timecycle.Length > 0) Function.Call(Hash.CLEAR_TIMECYCLE_MODIFIER);
                    Function.Call(Hash.DISPLAY_RADAR, true);
                }
                if (_minimapHeld)
                {
                    _minimapHeld = false;
                    HudBridge.HoldMinimapHidden(false);
                }
                if (_wheelButtonHeld)
                {
                    _wheelButtonHeld = false;
                    HudBridge.HoldVehicleWheelButton(false);
                }
                if (_muted)
                {
                    _muted = false;
                    ApplyMute();
                }
                IsOpen = false;
            }
            catch { }
        }

        // ---- GTA6Hud's radio -----------------------------------------------------------------------

        /// <summary>
        /// Whether GTA6Hud's radio wheel is on: its live setting (GTA6Hud.Ref.
        /// RadioUiEnabled, read by reflection), or GTA6Hud.ini [Radio] Enabled
        /// until GTA6Hud is found. Off when GTA6Hud is not installed.
        /// </summary>
        private static class Gta6HudRadio
        {
            private static FieldInfo _field;
            private static long _nextLook;
            private static int _looks;
            private static bool? _ini;

            public static bool Enabled
            {
                get
                {
                    if (_field == null && _looks < 30 && RealTime.Ms >= _nextLook)
                    {
                        _looks++;
                        _nextLook = RealTime.Ms + 1000;
                        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            try
                            {
                                Type t = asm.GetType("GTA6Hud.Ref", false);
                                FieldInfo f = t?.GetField("RadioUiEnabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                                if (f != null && f.FieldType == typeof(bool)) { _field = f; break; }
                            }
                            catch { }
                        }
                    }
                    if (_field != null)
                    {
                        try { return (bool)_field.GetValue(null); } catch { }
                    }
                    if (_ini == null)
                    {
                        string path = Path.Combine(Paths.Scripts ?? ".", "GTA6Hud.ini");
                        bool on = false;
                        try { if (File.Exists(path)) on = ScriptSettings.Load(path).GetValue("Radio", "Enabled", true); } catch { }
                        _ini = on;
                    }
                    return _ini.Value;
                }
            }
        }

        /// <summary>The other 2.5 features that run time themselves, looked up by name.</summary>
        private static class OtherFeature
        {
            private static PropertyInfo _crash, _focus;
            private static bool _looked;

            private static void Look()
            {
                if (_looked) return;
                _looked = true;
                Assembly asm = typeof(RadioWheel).Assembly;
                _crash = asm.GetType("ViExtras.CrashCam", false)?.GetProperty("IsPlaying", BindingFlags.Public | BindingFlags.Static);
                _focus = asm.GetType("ViExtras.Focus", false)?.GetProperty("IsActive", BindingFlags.Public | BindingFlags.Static);
            }

            private static bool Read(PropertyInfo p)
            {
                try { return p != null && (bool)p.GetValue(null); } catch { return false; }
            }

            public static bool CrashCamPlaying { get { Look(); return Read(_crash); } }
            public static bool FocusActive { get { Look(); return Read(_focus); } }
        }
    }
}
