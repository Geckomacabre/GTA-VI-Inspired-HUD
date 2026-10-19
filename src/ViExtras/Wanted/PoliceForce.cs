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
    /// </summary>
    public sealed class PoliceForce : Script
    {
        private const uint StunGun = 0x3656C8C1, Unarmed = 0xA2719263;
        private const int ScanMs = 200;
        private const int GunTypes = 4;           // IS_PED_ARMED: guns
        private const int AnyWeapon = 7;          // melee, explosives and guns
        private const float ComplyRange = 15f;    // a cop this close sees you give up
        private const int ComplyMs = 2000;        // standing still this long, unarmed
        private const float ArrestRange = 60f;    // the cop told to arrest you is within this

        private readonly bool _enabled;
        private readonly int _lethalFrom;
        private readonly int _armedGraceMs, _arrestWindowMs;

        // cops given a stun gun here: handle -> their own weapon
        private readonly Dictionary<int, uint> _own = new Dictionary<int, uint>();
        private int _nextScan, _armedUntil;
        private bool _lethal, _modeKnown, _stunned, _wasDead;
        private int _stillSince;

        // the arrest: until when it waits, the wanted level it dropped from, the cop on it
        private bool _arresting, _arrestStarted;
        private int _arrestUntil, _heldWanted, _nextArrestOrder, _notArrestedSince;
        private Ped _arrestCop;

        // GTA6Hud's police brain, by reflection
        private static bool _looked;
        private static FieldInfo _brainField, _copsField, _copPed, _copOriginal, _copDisarmed;
        private static FieldInfo _brainEnabled;   // GTA6Hud.Ref.PoliceBrainEnabled
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
            if (!_enabled || !GameReady.Now) return;
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;
            int now = Game.GameTime;
            int wanted = Game.Player.Wanted.WantedLevel;

            if (player.IsDead)
            {
                if (!_wasDead) LogDeath(player, wanted);
                _wasDead = true;
                if (_arresting) EndArrest("died", false);
                if (_own.Count > 0) RestoreAll();
                _modeKnown = _stunned = false;
                _armedUntil = _stillSince = 0;
                return;
            }
            _wasDead = false;
            if (_arresting) DriveArrest(player, wanted, now);
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
            bool complying = !_arresting && !lethal && !armedNow && wanted <= 2 && !player.IsInVehicle()
                && player.Velocity.Length() < 1.2f && NearestCop(player, ComplyRange) != null;
            if (!complying) _stillSince = 0;
            else if (_stillSince == 0) _stillSince = now;
            else if (now - _stillSince >= ComplyMs)
            {
                _stillSince = 0;
                StartArrest("gave up", wanted, now);
            }

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
            bool arrested = Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, false);
            if (arrested)
            {
                if (!_arrestStarted) Log.Write("PoliceForce", "arrest started");
                _arrestStarted = true;
                _notArrestedSince = 0;
                return;
            }
            if (_arrestStarted)
            {
                // the arrest has played out (BUSTED): let the brain back once it's over
                if (_notArrestedSince == 0) _notArrestedSince = now;
                if (wanted == 0 || now - _notArrestedSince > 1500) EndArrest("arrest over", false);
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
            if (restoreWanted && player != null && !player.IsDead && Game.Player.Wanted.WantedLevel < _heldWanted)
            {
                Game.Player.Wanted.SetWantedLevel(_heldWanted, false);
                Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
            }
            Log.Write("PoliceForce", why + (restoreWanted && _heldWanted > 1 ? ": back to " + _heldWanted + " stars" : ""));
            _arresting = _arrestStarted = false;
            _arrestCop = null;
            HoldBrain(false);
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
