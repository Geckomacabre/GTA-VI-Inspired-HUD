using System;
using System.Collections.Generic;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The weapon in your hands in a vehicle. The game picks the one you'll use from it as you get in, and
    /// with a stun gun on you it picks that (the user: "when i get in a vehicle it auto sets my held weapon to
    /// a tazer"); none of the HUD's scripts sets it. And it keeps doing so: a weapon it won't allow there (a
    /// two-handed SMG on a bike) is swapped straight back for the stun gun ("literally just got on the bike
    /// and its still setting a stun gun").
    ///
    /// So for the whole ride, whenever the stun gun is in your hands and you didn't choose it, the next weapon
    /// in line goes in instead: the gun you held on foot if it can be used from there, then your one-handed
    /// SMGs and pistols (first on a bike), then the two-handed SMGs, then nothing. One the game takes back for
    /// the stun gun is crossed off for the ride. Choosing the stun gun yourself (GTA6Hud's weapon wheel, or the
    /// game's weapon keys, or holding it as you got in) is left alone until you get out. Getting out with a stun
    /// gun you didn't choose, you get back what you held before the ride. [CarWeapon] Enabled = False leaves it
    /// to the game.
    /// </summary>
    public sealed class CarWeapon : Script
    {
        private const uint StunGun = 0x3656C8C1, Unarmed = 0xA2719263;
        private const uint GroupPistol = 0x18D5FA97, GroupSmg = 0xC6E9A5C5;
        private const long TouchMs = 1500;   // a stun gun this soon after a weapon key or the wheel is yours

        private static readonly WeaponHash[] OneHanded =
        {
            WeaponHash.MicroSMG, WeaponHash.MachinePistol, WeaponHash.MiniSMG, WeaponHash.APPistol, WeaponHash.CombatPistol,
            WeaponHash.Pistol50, WeaponHash.HeavyPistol, WeaponHash.Pistol, WeaponHash.SNSPistol, WeaponHash.VintagePistol,
            WeaponHash.Revolver,
        };
        private static readonly WeaponHash[] TwoHanded = { WeaponHash.SMG, WeaponHash.AssaultSMG, WeaponHash.CombatPDW };
        // the game's weapon keys: the wheel, next / previous, and the in-vehicle next / previous
        private static readonly int[] WeaponKeys = { 37, 14, 15, 16, 17, 99, 100 };

        private uint _onFoot = Unarmed;
        private bool _riding, _yours, _gaveUp;
        private long _touchedAt = -TouchMs;
        private uint _lastSet;
        private readonly HashSet<uint> _refused = new HashSet<uint>();

        public CarWeapon()
        {
            Paths.Init(BaseDirectory);
            if (!Config.Get("CarWeapon", "Enabled", true)) return;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                Ped me = Game.Player.Character;
                if (me == null || !me.Exists() || me.IsDead) return;
                int ped = me.Handle;
                long now = RealTime.Ms;
                foreach (int k in WeaponKeys)
                    if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, k)) { _touchedAt = now; break; }
                if (HudBridge.Gta6HudUiOpen) _touchedAt = now;

                if (!Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, ped, true))   // true: getting in counts
                {
                    if (_riding)
                    {
                        _riding = false;
                        if (!_yours && _lastSet != 0 && Selected(ped) == StunGun) Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped, _onFoot, true);
                        return;
                    }
                    _onFoot = Selected(ped);
                    return;
                }
                if (!_riding)
                {
                    _riding = true;
                    _yours = _gaveUp = false;
                    _lastSet = 0;
                    _refused.Clear();
                }
                if (Selected(ped) != StunGun)
                {
                    if (now - _touchedAt < TouchMs) _lastSet = 0;   // you picked something else: not ours to defend
                    return;
                }
                if (_yours || _gaveUp) return;
                if (now - _touchedAt < TouchMs || (_onFoot == StunGun && _lastSet == 0 && _refused.Count == 0))
                {
                    _yours = true;
                    return;
                }
                if (_lastSet != 0) _refused.Add(_lastSet);   // the game swapped ours back for the stun gun
                uint next = Next(ped, Function.Call<bool>(Hash.IS_PED_ON_ANY_BIKE, ped));
                if (next == 0)
                {
                    _gaveUp = true;
                    Log.Write("CarWeapon", "the game keeps the stun gun in your hands here; nothing else you carry is allowed");
                    return;
                }
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, ped, next, true);
                if (_lastSet == 0) Log.Write("CarWeapon", "the game gave you the stun gun getting in; " + WeaponName(next) + " instead");
                else Log.Write("CarWeapon", "the game took back " + WeaponName(_lastSet) + " for the stun gun; " + WeaponName(next) + " instead");
                _lastSet = next;
            }
            catch (Exception ex) { Log.Write("CarWeapon", ex.Message); _gaveUp = true; }
        }

        private uint Next(int ped, bool bike)
        {
            if (Usable(ped, _onFoot) && (!bike || !IsTwoHanded(_onFoot))) return _onFoot;
            foreach (WeaponHash w in OneHanded) if (Usable(ped, (uint)w)) return (uint)w;
            foreach (WeaponHash w in TwoHanded) if (Usable(ped, (uint)w)) return (uint)w;
            if (Usable(ped, _onFoot)) return _onFoot;
            return _refused.Contains(Unarmed) ? 0 : Unarmed;
        }

        private bool Usable(int ped, uint weapon)
        {
            if (weapon == Unarmed || weapon == StunGun || _refused.Contains(weapon)) return false;
            if (!Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, ped, weapon, false)) return false;
            uint group = unchecked((uint)Function.Call<int>(Hash.GET_WEAPONTYPE_GROUP, weapon));
            return group == GroupPistol || group == GroupSmg;
        }

        private static bool IsTwoHanded(uint weapon)
        {
            foreach (WeaponHash w in TwoHanded) if ((uint)w == weapon) return true;
            return false;
        }

        private static string WeaponName(uint weapon) => weapon == Unarmed ? "nothing" : ((WeaponHash)weapon).ToString();

        private static uint Selected(int ped) => unchecked((uint)Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped));
    }
}
