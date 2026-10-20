using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Police only shoot to kill when you're armed (a gun in your hand, or one fired in the
    /// last ArmedGraceSeconds) and wanted at LethalFromStars stars or more ([PoliceForce] in
    /// ViExtras.ini). The rest of the time every cop carries a stun gun, and you're arrested
    /// (BUSTED) instead: when a stun gun drops you, or when you stand still, unarmed, near a
    /// cop at 1-2 stars.
    ///
    /// GTA6Hud's police brain gives each cop a role and re-arms him with the weapon it noted
    /// when it took him on (Cop.Original). While force is non-lethal that note points at the
    /// stun gun, by reflection, so the brain hands it out itself, and it's put back to the
    /// cop's own gun after. Cops the brain isn't running get the same weapon directly and are
    /// kept from switching away from it.
    ///
    /// The arrest is the game's own: the wanted level goes to 1 (the game only arrests at
    /// 1 star), the brain is held off (Ref.PoliceBrainEnabled) so it doesn't re-task the cops,
    /// and the nearest cop on foot is told to arrest you (TASK_ARREST_PED). Draw a gun, or let
    /// ArrestWindowSeconds pass without an arrest, and the stars come back.
    ///
    /// GTA6Hud clears the wanted level the moment an arrest begins (ForgetOnDeathOrArrest, at
    /// IS_PLAYER_BEING_ARRESTED's early stage), which makes the game drop the arrest; while one
    /// of ours is under way its "already handled" flag is held up, and let go once the BUSTED
    /// screen shows, so its clean-up still runs then.
    ///
    /// Surrender: GTA6Hud's own SURRENDER prompt can never come up (its UpdateSurrender returns
    /// unless a surrender is already accepted). Near a cop who can see you, with stars, the
    /// prompt is shown here through its HoldWorldItem (hold E, or X / Square). Holding it puts
    /// your hands up and takes you down on your knees, hands on your head (the clips and order
    /// the FiveM server's fenix-police uses: Rockstar's re_arrests), and starts the arrest. You
    /// can't walk off while kneeling; sprint bolts, and the stars come back.
    /// </summary>
    public sealed class PoliceForce : Script
    {
        private const uint StunGun = 0x3656C8C1, Unarmed = 0xA2719263;
        private const int ScanMs = 200;
        private const int GunTypes = 4;           // IS_PED_ARMED: guns
        private const int AnyWeapon = 7;          // melee, explosives and guns
        private const float ComplyRange = 15f;    // a cop this close sees you give up
        private const int ComplyMs = 2000;        // standing still this long, unarmed
        private const int ComplyRetryMs = 10000;  // after an arrest that didn't happen, before giving up counts again
        private const float ArrestRange = 60f;    // the cop told to arrest you is within this
        private const float SurrenderRange = 15f; // a cop this close, who can see you, takes a surrender
        private const float SurrenderHoldSeconds = 0.75f; // as long as GTA6Hud's own hold prompts
        private const int FocusGraceMs = 300;     // the camera may slip off the cop's head this long

        // The surrender pose (fenix-police's clips): hands up, then down on the knees, hands on the head.
        private const string HandsDict = "missheist_agency2ahands_up", KneelDict = "random@arrests", HeadDict = "random@arrests@busted";
        private const int SprintControl = 21;
        private static readonly int[] PoseBlocked = { 21, 22, 23, 24, 25, 30, 31, 32, 33, 34, 35, 37, 44, 140, 141, 142, 143 };
        private int _pose, _poseNext;   // 0 none; 1-6 the steps in; 7 kneeling, hands on the head

        private readonly bool _enabled;
        private readonly int _lethalFrom;
        private readonly int _armedGraceMs, _arrestWindowMs;

        // cops given a stun gun here: handle -> their own weapon
        private readonly Dictionary<int, uint> _own = new Dictionary<int, uint>();
        private int _nextScan, _armedUntil;
        private bool _lethal, _modeKnown, _stunned, _wasDead;
        private int _stillSince;
        private int _complyAfter;
        private string _lastError;

        // the arrest: until when it waits, the wanted level it dropped from, the cop on it
        private bool _arresting, _arrestStarted, _bustedShown;
        private int _arrestUntil, _heldWanted, _nextArrestOrder, _notArrestedSince;
        private Ped _arrestCop;

        // GTA6Hud's police brain, by reflection
        private static bool _looked;
        private static FieldInfo _brainField, _copsField, _copPed, _copOriginal, _copDisarmed;
        private static FieldInfo _brainEnabled;   // GTA6Hud.Ref.PoliceBrainEnabled
        private static FieldInfo _deadOrArrested; // GTA6Hud's "death / arrest already handled"
        private static MethodInfo _holdWorldItem; // GTA6Hud's hold-E prompt on a ped or prop
        // that prompt's pieces, so the surrender keeps a hold timer of its own (SurrenderPrompt)
        private static MethodInfo _focused, _holdDown, _drawMarker, _drawRim, _passiveDot;
        private Ped _surrenderCop;
        private float _surrenderHold;
        private int _unfocusedSince;
        private static bool _hudLooked, _surrenderBroken;
        private bool _brainHeld, _brainWas;

        public PoliceForce()
        {
            Paths.Init(BaseDirectory);
            const string S = "PoliceForce";
            _enabled = Config.Get(S, "Enabled", true);
            _lethalFrom = Math.Max(1, Config.Get(S, "LethalFromStars", 3));
            _armedGraceMs = (int)(Math.Max(0f, Config.Get(S, "ArmedGraceSeconds", 5f)) * 1000f);
            _arrestWindowMs = (int)(Math.Max(3f, Config.Get(S, "ArrestWindowSeconds", 15f)) * 1000f);
            Tick += OnTick;
            Aborted += (s, e) => { HoldBrain(false); RestoreAll(); };
        }

        private void OnTick(object sender, EventArgs e)
        {
            try { Frame(); }
            catch (Exception ex)
            {
                // logged once a kind, and an arrest under way let go, so a bad frame can't stop
                // the script with the stars held at 1 and GTA6Hud's police brain held off
                string what = ex.GetType().Name + ": " + ex.Message;
                if (what != _lastError) Log.Write("PoliceForce", ex.ToString());
                _lastError = what;
                if (_arresting) { try { EndArrest("error", true); } catch { } }
            }
        }

        private void Frame()
        {
            if (!_enabled || !GameReady.Now) return;
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;
            int now = Game.GameTime;
            int wanted = Game.Player.Wanted.WantedLevel;

            if (player.IsDead)
            {
                if (!_wasDead) LogDeath(player, wanted);
                _wasDead = true;
                if (_arresting)
                {
                    EndArrest("died", false);
                    SetHandled(false);   // GTA6Hud's death clean-up runs on its next frame
                }
                if (_own.Count > 0) RestoreAll();
                _modeKnown = _stunned = false;
                _armedUntil = _stillSince = 0;
                return;
            }
            _wasDead = false;
            if (_arresting) DriveArrest(player, wanted, now);
            if (_pose != 0) DrivePose(player, now);
            if (wanted == 0)
            {
                if (_own.Count > 0) RestoreAll();
                _modeKnown = _stunned = false;
                _armedUntil = _stillSince = 0;
                return;
            }

            bool armedNow = Function.Call<bool>(Hash.IS_PED_ARMED, player, GunTypes) || Function.Call<bool>(Hash.IS_PED_SHOOTING, player);
            if (armedNow) _armedUntil = now + _armedGraceMs;
            bool lethal = wanted >= _lethalFrom && (armedNow || now < _armedUntil);
            if (!_modeKnown || lethal != _lethal)
            {
                _lethal = lethal;
                _modeKnown = true;
                _nextScan = 0;
                Log.Write("PoliceForce", (lethal ? "guns" : "stun guns") + " (" + wanted + " stars, " + (armedNow ? "armed" : "unarmed") + ")");
            }

            // a stun-gun hit: the weapon goes away and the arrest starts
            bool stunned = Function.Call<bool>(Hash.IS_PED_BEING_STUNNED, player, 0)
                || Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, player, StunGun, 0);
            if (stunned) Function.Call(Hash.CLEAR_ENTITY_LAST_WEAPON_DAMAGE, player);
            if (stunned && !_stunned && !lethal)
            {
                _armedUntil = 0;
                if (Function.Call<bool>(Hash.IS_PED_ARMED, player, AnyWeapon))
                    Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, Unarmed, true);
                StartArrest("tased", wanted, now);
            }
            _stunned = stunned;

            // giving up: unarmed, on foot and still, a cop close by, at 1-2 stars
            // (not while a wheel, a menu or the phone is up: standing still there is choosing,
            // and not again straight after an arrest that didn't happen)
            bool complying = !_arresting && !lethal && !armedNow && wanted <= 2 && !player.IsInVehicle()
                && now >= _complyAfter && !HudBridge.UiInTheWay
                && player.Velocity.Length() < 1.2f && NearestCop(player, ComplyRange) != null;
            if (!complying) _stillSince = 0;
            else if (_stillSince == 0) _stillSince = now;
            else if (now - _stillSince >= ComplyMs)
            {
                _stillSince = 0;
                StartArrest("gave up", wanted, now);
            }

            if (!_arresting) OfferSurrender(player, wanted, now);

            if (now < _nextScan) return;
            _nextScan = now + ScanMs;
            if (lethal) RestoreAll();
            else ArmWithStunGuns(player);
        }

        private void LogDeath(Ped player, int wanted)
        {
            uint cause = unchecked((uint)Function.Call<int>(Hash.GET_PED_CAUSE_OF_DEATH, player));
            string what = cause == StunGun ? "stun gun" : cause == Unarmed ? "fists" : "weapon 0x" + cause.ToString("X8");
            Log.Write("PoliceForce", "died at " + wanted + " stars (" + (_modeKnown ? (_lethal ? "guns" : "stun guns") : "no police") + "): " + what);
        }

        // ------------------------------------------------------------------ the arrest
        private void StartArrest(string why, int wanted, int now)
        {
            if (_arresting) return;
            _arresting = true;
            _arrestStarted = false;
            _arrestUntil = now + _arrestWindowMs;
            _nextArrestOrder = 0;
            _notArrestedSince = 0;
            _arrestCop = null;
            _heldWanted = wanted;
            if (wanted != 1)
            {
                Game.Player.Wanted.SetWantedLevel(1, false);
                Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
            }
            HoldBrain(true);
            Log.Write("PoliceForce", why + " at " + wanted + " stars: arrest" + (wanted != 1 ? " (down to 1 star)" : ""));
        }

        private void DriveArrest(Ped player, int wanted, int now)
        {
            HoldBrain(true);   // its settings reload would switch it back on
            bool started = Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, true);   // hands going up
            bool busted = Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, false);   // the BUSTED screen
            if (busted && !_bustedShown)
            {
                _bustedShown = true;
                SetHandled(false);   // past the point of no return: GTA6Hud's arrest clean-up may run now
                Log.Write("PoliceForce", "BUSTED");
            }
            else if (!_bustedShown) SetHandled(true);   // keep GTA6Hud from clearing the stars mid-arrest
            if (started || busted)
            {
                _pose = 0;   // the game's arrest has the player now
                if (!_arrestStarted) Log.Write("PoliceForce", "arrest started");
                _arrestStarted = true;
                _notArrestedSince = 0;
                return;
            }
            if (_arrestStarted)
            {
                // over: after BUSTED once it has played out, or broken off before it
                if (_notArrestedSince == 0) _notArrestedSince = now;
                if (now - _notArrestedSince > 1500) EndArrest(_bustedShown ? "arrest over" : "arrest broke off", !_bustedShown);
                return;
            }
            if (wanted == 0) { EndArrest("wanted level cleared", false); return; }
            if (Function.Call<bool>(Hash.IS_PED_ARMED, player, GunTypes)) { EndArrest("drew a gun", true); return; }
            if (now >= _arrestUntil) { EndArrest("no arrest", true); return; }
            if (wanted > 1)
            {
                Game.Player.Wanted.SetWantedLevel(1, false);
                Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
            }
            if (now < _nextArrestOrder) return;
            _nextArrestOrder = now + 1500;
            Ped cop = _arrestCop != null && _arrestCop.Exists() && !_arrestCop.IsDead && !_arrestCop.IsInVehicle()
                ? _arrestCop : NearestCop(player, ArrestRange, onFoot: true);
            if (cop == null) return;
            if (cop != _arrestCop)
            {
                _arrestCop = cop;
                Log.Write("PoliceForce", "cop " + cop.Handle + " at " + cop.Position.DistanceTo(player.Position).ToString("0") + " m told to arrest");
            }
            Function.Call(Hash.TASK_ARREST_PED, cop, player);
        }

        private void EndArrest(string why, bool restoreWanted)
        {
            Ped player = Game.Player.Character;
            if (_pose != 0 && player != null && player.Exists() && !player.IsDead) LeavePose(player, bolt: why == "ran off");
            _pose = 0;
            if (restoreWanted && player != null && !player.IsDead && Game.Player.Wanted.WantedLevel < _heldWanted)
            {
                Game.Player.Wanted.SetWantedLevel(_heldWanted, false);
                Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
            }
            Log.Write("PoliceForce", why + (restoreWanted && _heldWanted > 1 ? ": back to " + _heldWanted + " stars" : ""));
            if (restoreWanted) _complyAfter = Game.GameTime + ComplyRetryMs;
            _arresting = _arrestStarted = _bustedShown = false;
            _arrestCop = null;
            HoldBrain(false);
        }

        // ------------------------------------------------------------------ surrender
        private void OfferSurrender(Ped player, int wanted, int now)
        {
            if (_surrenderBroken || wanted == 0 || player.IsInVehicle() || player.IsRagdoll || HudBridge.UiInTheWay)
            {
                _surrenderCop = null;
                _surrenderHold = 0f;
                return;
            }
            // the same cop while he stays near, on foot and in sight (picking the nearest afresh every
            // frame started the hold over whenever two cops traded places), else the nearest who can see you
            Ped cop = _surrenderCop != null && CanTakeSurrender(_surrenderCop, player) ? _surrenderCop : null;
            if (cop == null)
            {
                float best = SurrenderRange;
                foreach (Ped ped in World.GetNearbyPeds(player, SurrenderRange))
                {
                    if (!CanTakeSurrender(ped, player)) continue;
                    float d = ped.Position.DistanceTo(player.Position);
                    if (d < best) { best = d; cop = ped; }
                }
            }
            if (cop != _surrenderCop)
            {
                _surrenderCop = cop;
                _surrenderHold = 0f;
                _unfocusedSince = 0;
            }
            if (cop == null) return;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !LookHud(hud)) return;
            bool held;
            try
            {
                if (_focused != null && _holdDown != null && _drawMarker != null && _drawRim != null) held = SurrenderPrompt(hud, cop);
                else if (_holdWorldItem != null) held = (bool)_holdWorldItem.Invoke(hud, new object[] { cop.Handle, "SURRENDER" });
                else return;
            }
            catch (Exception ex)
            {
                _surrenderBroken = true;
                Log.Write("PoliceForce", "surrender prompt: " + (ex.InnerException ?? ex).Message);
                return;
            }
            if (!held) return;
            Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, Unarmed, true);
            foreach (string dict in new[] { HandsDict, KneelDict, HeadDict }) Function.Call(Hash.REQUEST_ANIM_DICT, dict);
            player.Heading = (cop.Position - player.Position).ToHeading();
            _pose = 1;
            _poseNext = 0;
            _armedUntil = 0;
            _arrestCop = cop;
            StartArrest("surrendered", wanted, now);
            _arrestCop = cop;
            _nextArrestOrder = 0;
            _surrenderCop = null;
        }

        private bool CanTakeSurrender(Ped ped, Ped player) =>
            ped != null && ped.Exists() && !ped.IsDead && !ped.IsInVehicle() && IsCop(ped)
            && ped.Position.DistanceTo(player.Position) <= SurrenderRange
            && Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, ped, player, 17);

        // GTA6Hud's hold-E prompt (its marker, E glyph and filling rim), drawn here with a hold timer of
        // its own. GTA6Hud's HoldWorldItem keeps one timer for every hold prompt, and GTA6Hud runs it every
        // frame for the loot, priced props and cash bags around you: each of those, or any of them out of
        // the camera's focus, set it back to 0, so the surrender's rim stuck at a frame or two's worth.
        private bool SurrenderPrompt(object hud, Ped cop)
        {
            GTA.Math.Vector3 at = cop.Position + new GTA.Math.Vector3(0f, 0f, 0.7f);   // the head, as GTA6Hud aims at peds
            if (!(bool)_focused.Invoke(null, new object[] { at }))
            {
                if (_unfocusedSince == 0) _unfocusedSince = Environment.TickCount;
                if (Environment.TickCount - _unfocusedSince > FocusGraceMs)
                {
                    _surrenderHold = 0f;
                    _passiveDot?.Invoke(hud, new object[] { at });
                    return false;
                }
            }
            else _unfocusedSince = 0;
            var p = GTA.UI.Screen.WorldToScreen(at, true);
            if (p.X <= 0f || p.Y <= 0f) return false;
            float x = p.X * 1.5f, y = p.Y * 1.5f;   // GTA6Hud's FromSprite
            _drawMarker.Invoke(hud, new object[] { x, y, "SURRENDER", "square", "E", true, false, true });
            _surrenderHold = (bool)_holdDown.Invoke(null, null) ? _surrenderHold + Math.Min(Game.LastFrameTime, 0.1f) : 0f;
            if (_surrenderHold > 0f) _drawRim.Invoke(hud, new object[] { x, y, 48.07f, _surrenderHold / SurrenderHoldSeconds });
            if (_surrenderHold < SurrenderHoldSeconds) return false;
            _surrenderHold = 0f;
            return true;
        }

        // One step of the surrender pose at a time, each started when the last one ends.
        private void DrivePose(Ped player, int now)
        {
            foreach (int c in PoseBlocked) Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, c, true);
            if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, SprintControl))
            {
                EndArrest("ran off", true);
                return;
            }
            if (now < _poseNext) return;
            switch (_pose)
            {
                case 1:   // hands up
                    if (!Loaded(HandsDict)) return;
                    _poseNext = now + Play(player, HandsDict, "handsup_enter", 2, 8f);
                    _pose = 2;
                    break;
                case 2:
                    Play(player, HandsDict, "handsup_loop", 1, 4f);
                    _poseNext = now + 1200;
                    _pose = 3;
                    break;
                case 3:   // down on the knees
                    if (!Loaded(KneelDict) || !Loaded(HeadDict)) return;
                    _poseNext = now + Play(player, KneelDict, "idle_2_hands_up", 2, 4f);
                    _pose = 4;
                    break;
                case 4:
                    Play(player, KneelDict, "kneeling_arrest_idle", 2, 4f);
                    _poseNext = now + 400;
                    _pose = 5;
                    break;
                case 5:   // hands on the head
                    _poseNext = now + Play(player, HeadDict, "enter", 2, 4f);
                    _pose = 6;
                    break;
                case 6:
                    Play(player, HeadDict, "idle_a", 1, 4f);
                    _poseNext = now + 500;
                    _pose = 7;
                    break;
                default:  // kneeling: keep the loop playing (a bump can knock it off)
                    if (!Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, player, HeadDict, "idle_a", 3)) Play(player, HeadDict, "idle_a", 1, 4f);
                    _poseNext = now + 500;
                    break;
            }
        }

        // Out of the pose: bolting from the knees, or getting back up (the hands just come down).
        private void LeavePose(Ped player, bool bolt)
        {
            if (_pose >= 4) Play(player, KneelDict, bolt ? "kneeling_arrest_escape" : "kneeling_arrest_get_up", 0, 4f);
            else if (Loaded(HandsDict)) Play(player, HandsDict, "handsup_exit", 0, 4f);
            else player.Task.ClearAll();
        }

        private static bool Loaded(string dict)
        {
            if (Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict)) return true;
            Function.Call(Hash.REQUEST_ANIM_DICT, dict);
            return false;
        }

        // Plays a clip on the player; returns how long it runs, in ms (a little under, so the next starts as it ends).
        private static int Play(Ped player, string dict, string clip, int flags, float blendIn)
        {
            Function.Call(Hash.TASK_PLAY_ANIM, player, dict, clip, blendIn, -4f, -1, flags, 0f, false, false, false);
            float secs = Function.Call<float>(Hash.GET_ANIM_DURATION, dict, clip);
            return secs > 0f ? Math.Max(200, (int)(secs * 950f)) : 1000;
        }

        private static bool LookHud(object hud)
        {
            if (_hudLooked) return true;
            _hudLooked = true;
            const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = hud.GetType();
            _deadOrArrested = t.GetField("_deadOrArrested", Own);
            _holdWorldItem = t.GetMethod("HoldWorldItem", Own, null, new[] { typeof(int), typeof(string) }, null);
            const BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            _focused = t.GetMethod("Focused", Stat, null, new[] { typeof(GTA.Math.Vector3) }, null);
            _holdDown = t.GetMethod("ServiceHoldDown", Stat, null, Type.EmptyTypes, null);
            _drawMarker = t.GetMethod("DrawWorldMarker", Own, null, new[] { typeof(float), typeof(float), typeof(string), typeof(string),
                typeof(string), typeof(bool), typeof(bool), typeof(bool) }, null);
            _drawRim = t.GetMethod("DrawProgressRim", Own, null, new[] { typeof(float), typeof(float), typeof(float), typeof(float) }, null);
            _passiveDot = t.GetMethod("PassiveDot", Own, null, new[] { typeof(GTA.Math.Vector3) }, null);
            if (_focused == null || _holdDown == null || _drawMarker == null || _drawRim == null)
                Log.Write("PoliceForce", "GTA6Hud's prompt pieces weren't all found: the surrender uses its shared hold prompt");
            if (_deadOrArrested == null) Log.Write("PoliceForce", "GTA6Hud's arrest flag wasn't found: it may cancel arrests");
            if (_holdWorldItem == null) Log.Write("PoliceForce", "GTA6Hud's hold prompt wasn't found: no surrender on E");
            return true;
        }

        // GTA6Hud's "death / arrest already handled" flag (it clears the wanted level when that
        // goes from not handled to handled).
        private static void SetHandled(bool handled)
        {
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !LookHud(hud) || _deadOrArrested == null) return;
            try { _deadOrArrested.SetValue(hud, handled); }
            catch { }
        }

        // GTA6Hud's police brain off while held (it releases its cops when it sees it off), and
        // back to what it was when let go.
        private void HoldBrain(bool hold)
        {
            try
            {
                if (_brainEnabled == null)
                {
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        Type refs = asm.GetType("GTA6Hud.Ref", false);
                        if (refs == null) continue;
                        _brainEnabled = refs.GetField("PoliceBrainEnabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        break;
                    }
                    if (_brainEnabled == null) return;
                }
                if (hold)
                {
                    if (!_brainHeld) { _brainWas = (bool)_brainEnabled.GetValue(null); _brainHeld = true; }
                    if ((bool)_brainEnabled.GetValue(null)) _brainEnabled.SetValue(null, false);
                }
                else if (_brainHeld)
                {
                    _brainEnabled.SetValue(null, _brainWas);
                    _brainHeld = false;
                }
            }
            catch (Exception ex) { Log.Write("PoliceForce", "police brain switch: " + ex.Message); }
        }

        private static Ped NearestCop(Ped player, float range, bool onFoot = false)
        {
            Ped best = null;
            float bestD = range;
            foreach (Ped ped in World.GetNearbyPeds(player, range))
            {
                if (ped == null || !ped.Exists() || ped.IsDead || !IsCop(ped)) continue;
                if (onFoot && ped.IsInVehicle()) continue;
                float d = ped.Position.DistanceTo(player.Position);
                if (d < bestD) { bestD = d; best = ped; }
            }
            return best;
        }

        private static bool Look(object hud)
        {
            if (_looked) return _copsField != null;
            _looked = true;
            try
            {
                const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                _brainField = hud.GetType().GetField("_policeBrain", Own);
                Type brain = _brainField?.FieldType;
                _copsField = brain?.GetField("_cops", Own);
                Type cop = brain?.GetNestedType("Cop", BindingFlags.Public | BindingFlags.NonPublic);
                _copPed = cop?.GetField("Ped", Own);
                _copOriginal = cop?.GetField("Original", Own);
                _copDisarmed = cop?.GetField("Disarmed", Own);
                if (_copsField == null || _copPed == null || _copOriginal == null || _copDisarmed == null)
                {
                    _copsField = null;
                    Log.Write("PoliceForce", "GTA6Hud's police brain was not as expected; only cops outside it get stun guns");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _copsField = null;
                Log.Write("PoliceForce", "GTA6Hud's police brain: " + ex.Message);
                return false;
            }
        }

        // The brain's cop records (null when GTA6Hud or its brain isn't there).
        private static IList BrainCops()
        {
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !Look(hud)) return null;
            try { return _copsField.GetValue(_brainField.GetValue(hud)) as IList; }
            catch { return null; }
        }

        private static bool IsCop(Ped ped)
        {
            int type = Function.Call<int>(Hash.GET_PED_TYPE, ped);
            return type == 6 || type == 27 || type == 29;   // cop, SWAT, army
        }

        private static uint Current(Ped ped) => unchecked((uint)Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped));

        // Its own gun, noted once: what the brain noted, else what's in its hand, else its best.
        private uint OwnWeapon(Ped ped, uint noted)
        {
            if (_own.TryGetValue(ped.Handle, out uint own)) return own;
            own = noted != 0 && noted != StunGun && noted != Unarmed ? noted : Current(ped);
            if (own == StunGun || own == Unarmed) own = unchecked((uint)Function.Call<int>(Hash.GET_BEST_PED_WEAPON, ped, false));
            _own[ped.Handle] = own;
            return own;
        }

        private static void GiveStunGun(Ped ped)
        {
            if (!Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, ped, StunGun, false))
                Function.Call(Hash.GIVE_WEAPON_TO_PED, ped, StunGun, 100, false, false);
        }

        private void ArmWithStunGuns(Ped player)
        {
            var done = new HashSet<int>();
            IList cops = BrainCops();
            if (cops != null)
            {
                try
                {
                    foreach (object cop in cops)
                    {
                        if (!(_copPed.GetValue(cop) is Ped ped) || !ped.Exists() || ped.IsDead) continue;
                        done.Add(ped.Handle);
                        OwnWeapon(ped, Convert.ToUInt32(_copOriginal.GetValue(cop)));
                        GiveStunGun(ped);
                        _copOriginal.SetValue(cop, Enum.ToObject(_copOriginal.FieldType, StunGun));
                        // disarmed (detaining, brawling) stays bare-handed
                        if ((bool)_copDisarmed.GetValue(cop)) continue;
                        if (Current(ped) != StunGun) Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped, StunGun, true);
                        Function.Call(Hash.SET_PED_CAN_SWITCH_WEAPON, ped, false);
                    }
                }
                catch (Exception ex) { Log.Write("PoliceForce", "police brain: " + ex.Message); }
            }
            foreach (Ped ped in World.GetNearbyPeds(player, 150f))
            {
                if (ped == null || !ped.Exists() || ped.IsDead || done.Contains(ped.Handle) || !IsCop(ped)) continue;
                OwnWeapon(ped, 0);
                GiveStunGun(ped);
                if (Current(ped) != StunGun) Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped, StunGun, true);
                Function.Call(Hash.SET_PED_CAN_SWITCH_WEAPON, ped, false);
            }
        }

        // Every cop given a stun gun gets his own gun back, in the brain's note and in his hand.
        private void RestoreAll()
        {
            if (_own.Count == 0) return;
            var notes = new Dictionary<int, object>();
            IList cops = BrainCops();
            if (cops != null)
            {
                try
                {
                    foreach (object cop in cops)
                        if (_copPed.GetValue(cop) is Ped ped && ped.Exists()) notes[ped.Handle] = cop;
                }
                catch { }
            }
            foreach (var kv in _own)
            {
                Entity ent = Entity.FromHandle(kv.Key);
                if (!(ent is Ped ped) || !ped.Exists() || ped.IsDead) continue;
                bool disarmed = false;
                if (notes.TryGetValue(kv.Key, out object cop))
                {
                    try
                    {
                        _copOriginal.SetValue(cop, Enum.ToObject(_copOriginal.FieldType, kv.Value));
                        disarmed = (bool)_copDisarmed.GetValue(cop);
                    }
                    catch { }
                }
                Function.Call(Hash.SET_PED_CAN_SWITCH_WEAPON, ped, !disarmed);
                if (!disarmed && Current(ped) == StunGun) Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped, kv.Value, true);
            }
            _own.Clear();
        }
    }
}
