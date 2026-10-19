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
    /// ViExtras.ini). The rest of the time every cop carries a stun gun, and a hit puts your
    /// weapon away and brings the arrest (BUSTED).
    ///
    /// GTA6Hud's police brain gives each cop a role and re-arms him with the weapon it noted
    /// when it took him on (Cop.Original). While force is non-lethal that note points at the
    /// stun gun, by reflection, so the brain hands it out itself, and it's put back to the
    /// cop's own gun after. Cops the brain isn't running get the same weapon directly and are
    /// kept from switching away from it. The brain already arrests an unarmed suspect who
    /// stays still at 1-2 stars (its Detain role). At 3+ stars a stun-gun hit drops you to
    /// 1 star so that arrest can happen; get away, or draw a gun, and the stars come back.
    /// </summary>
    public sealed class PoliceForce : Script
    {
        private const uint StunGun = 0x3656C8C1, Unarmed = 0xA2719263;
        private const int ScanMs = 200;
        private const int GunTypes = 4;           // IS_PED_ARMED: guns
        private const int AnyWeapon = 7;          // melee, explosives and guns

        private readonly bool _enabled;
        private readonly int _lethalFrom;
        private readonly int _armedGraceMs, _arrestWindowMs;

        // cops given a stun gun here: handle -> their own weapon
        private readonly Dictionary<int, uint> _own = new Dictionary<int, uint>();
        private int _nextScan, _armedUntil;
        private bool _lethal, _modeKnown, _stunned;
        // a 3+ star wanted level dropped to 1 for the arrest, and until when it waits
        private int _heldWanted, _heldUntil;

        // GTA6Hud's police brain, by reflection
        private static bool _looked;
        private static FieldInfo _brainField, _copsField, _copPed, _copOriginal, _copDisarmed;

        public PoliceForce()
        {
            Paths.Init(BaseDirectory);
            const string S = "PoliceForce";
            _enabled = Config.Get(S, "Enabled", true);
            _lethalFrom = Math.Max(1, Config.Get(S, "LethalFromStars", 3));
            _armedGraceMs = (int)(Math.Max(0f, Config.Get(S, "ArmedGraceSeconds", 5f)) * 1000f);
            _arrestWindowMs = (int)(Math.Max(3f, Config.Get(S, "ArrestWindowSeconds", 15f)) * 1000f);
            Tick += OnTick;
            Aborted += (s, e) => RestoreAll();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled || !GameReady.Now) return;
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;
            int now = Game.GameTime;
            int wanted = Game.Player.Wanted.WantedLevel;
            if (_heldUntil != 0) WatchArrest(player, wanted, now);
            if (wanted == 0 || player.IsDead)
            {
                if (_own.Count > 0) RestoreAll();
                _modeKnown = _stunned = false;
                _armedUntil = 0;
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

            bool stunned = Function.Call<bool>(Hash.IS_PED_BEING_STUNNED, player, 0);
            if (stunned && !_stunned && !lethal) OnStunned(player, wanted, now);
            _stunned = stunned;

            if (now < _nextScan) return;
            _nextScan = now + ScanMs;
            if (lethal) RestoreAll();
            else ArmWithStunGuns(player);
        }

        // A stun-gun hit: the weapon goes away, and above 2 stars the wanted level drops to 1
        // for GTA6Hud's police to arrest you (they only do at 1-2 stars).
        private void OnStunned(Ped player, int wanted, int now)
        {
            _armedUntil = 0;
            if (Function.Call<bool>(Hash.IS_PED_ARMED, player, AnyWeapon))
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, Unarmed, true);
            if (wanted > 2 && _heldUntil == 0)
            {
                _heldWanted = wanted;
                _heldUntil = now + _arrestWindowMs;
                Game.Player.Wanted.SetWantedLevel(1, false);
                Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
                Log.Write("PoliceForce", "stunned at " + wanted + " stars: down to 1 for the arrest");
            }
            else Log.Write("PoliceForce", "stunned at " + wanted + " stars");
        }

        // The arrest window: over once the arrest starts, or the wanted level goes; the stars
        // come back if it runs out, or a gun comes out, before then.
        private void WatchArrest(Ped player, int wanted, int now)
        {
            bool arrested = Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, false);
            if (arrested || wanted == 0 || player.IsDead)
            {
                if (arrested) Log.Write("PoliceForce", "arrested");
                _heldUntil = 0;
                return;
            }
            bool drewGun = Function.Call<bool>(Hash.IS_PED_ARMED, player, GunTypes);
            if (now < _heldUntil && !drewGun) return;
            if (wanted < _heldWanted)
            {
                Game.Player.Wanted.SetWantedLevel(_heldWanted, false);
                Game.Player.Wanted.ApplyWantedLevelChangeNow(false);
                Log.Write("PoliceForce", (drewGun ? "drew a gun" : "no arrest") + ": back to " + _heldWanted + " stars");
            }
            _heldUntil = 0;
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
