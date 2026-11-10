// The slim jim as a weapon (the user, 2026-11-07: "lets add it as a weapon ... same idea as the poo bag").
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The slim jim in the weapon wheel: WEAPON_SLIMJIM, the vihudanims pack's melee weapon (Rockstar's crowbar, its
    /// moves and its grip, with the slim jim's strip in his hand; built in vi hud\slimjim_weapon). It and GTA6Hud's slim
    /// jim, the one the door prompt's SLIM JIM wants (Hud\Save\slim_jim.txt, "owned"), are one slim jim:
    ///  * owning the slim jim gives the weapon, and gives it back when it's gone (a trainer emptying the weapons,
    ///    another character, an arrest), except during a mission, which may have taken weapons on purpose, and while
    ///    GTA6Hud has it out in the world (thrown with its THROW, lying there, or picked up by someone);
    ///  * the weapon arriving another way (a trainer that lists add-on weapons) makes the slim jim owned;
    ///  * the slim jim going (its file removed, as GTA6Hud's own reset does) takes the weapon away.
    /// It hits about like a punch, not a crowbar: SET_WEAPON_DAMAGE_MODIFIER with [SlimJim] WeaponDamage, for this
    /// weapon only. The game forgets it every frame, so it's set every frame. Measured in game at 0.35: a punch took 22
    /// health, a crowbar 82, the slim jim 28-29. It touches only the slim jim, unlike the player-wide
    /// SET_PLAYER_MELEE_WEAPON_DAMAGE_MODIFIER, which would also need putting back.
    /// GTA6Hud's wheel lists only the weapons Script Hook V .NET has names for, so it's
    /// added to that list (as the Poo Bag is) and shows on the MELEE ring with Hud\Weapons\weapon_3269697463.png.
    /// Without the pack's weapon (an older vihudanims) nothing happens. [Switches] SlimJimWeapon turns it off.
    /// </summary>
    public sealed class SlimJimWeapon : Script
    {
        public const uint WeaponHash = 0xC2E39FB7;                  // joaat("WEAPON_SLIMJIM")
        private static readonly int Weapon = unchecked((int)WeaponHash);
        private const ulong SetWeaponDamageModifier = 0x4757F00BC6323CFE;
        private const int CheckMs = 500, WheelMs = 5000;
        private const string Tag = "SlimJimWeapon";

        private readonly bool _enabled;
        private readonly float _damage;
        private bool _checkedValid, _valid, _inWheel;
        private string _file;
        private bool? _fileBefore;
        private long _checkAt, _wheelAt;
        private int _errors;

        // GTA6Hud's melee weapons out in the world: _dropped (DroppedWeapon: Prop, Hash), _flyingMelee (FlyingMelee:
        // Drop), _meleeCarriers (int -> MeleeCarrier: Ped, Weapon)
        private object _hud;
        private FieldInfo _dropped, _flying, _carriers;

        public SlimJimWeapon()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get(Config.Switches, "SlimJimWeapon", true);
            _damage = Num.Clamp(Config.Get("SlimJim", "WeaponDamage", 0.35f), 0f, 2f);
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled) return;
            // every frame, or it's gone: this weapon's hits at [SlimJim] WeaponDamage
            if (_valid) Function.Call((Hash)SetWeaponDamageModifier, Weapon, _damage);
            long now = RealTime.Ms;
            if (now < _checkAt) return;
            _checkAt = now + CheckMs;
            try
            {
                Step(now);
            }
            catch (Exception ex)
            {
                if (_errors++ < 5) Log.Write(Tag, ex.ToString());
            }
        }

        private void Step(long now)
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;
            if (!_checkedValid)
            {
                _checkedValid = true;
                _valid = Function.Call<bool>(Hash.IS_WEAPON_VALID, Weapon);
                Log.Write(Tag, _valid
                    ? "WEAPON_SLIMJIM is in the game; it hits at " + _damage.ToString("0.##") + " of a crowbar"
                    : "WEAPON_SLIMJIM isn't in the game (the vihudanims pack is older): the slim jim stays out of the wheel");
            }
            if (!_valid) return;

            if (!_inWheel && now >= _wheelAt)
            {
                _wheelAt = now + WheelMs;
                _inWheel = AddToHudWheel();
            }
            if (player.IsDead) return;
            if (_file == null && (_file = FindFile()) == null) return;

            bool owned = File.Exists(_file);
            bool weapon = Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, player, Weapon, false);
            if (owned && !weapon)
            {
                if (!Function.Call<bool>(Hash.GET_MISSION_FLAG) && !OutInTheWorld())
                {
                    Function.Call(Hash.GIVE_WEAPON_TO_PED, player, Weapon, 0, false, false);
                    Log.Write(Tag, _fileBefore == true ? "the slim jim back in the wheel" : "the slim jim is in the wheel");
                }
            }
            else if (!owned && weapon)
            {
                if (_fileBefore == true)
                {
                    Function.Call(Hash.REMOVE_WEAPON_FROM_PED, player, Weapon);
                    Log.Write(Tag, "the slim jim is gone (" + Path.GetFileName(_file) + " removed): out of the wheel too");
                }
                else
                {
                    try
                    {
                        File.WriteAllText(_file, "owned");
                        owned = true;
                        Log.Write(Tag, "WEAPON_SLIMJIM given another way: the slim jim is owned");
                    }
                    catch (Exception ex)
                    {
                        if (_errors++ < 5) Log.Write(Tag, "can't write " + _file + ": " + ex.Message);
                    }
                }
            }
            _fileBefore = owned;
        }

        // GTA6Hud's own SlimJimFile, so it's the file its SLIM JIM reads wherever its folder is; null until it's up.
        private string FindFile()
        {
            object hud = HudBridge.Gta6Hud();
            if (hud == null) return null;
            _hud = hud;
            const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = hud.GetType();
            _dropped = t.GetField("_dropped", Own);
            _flying = t.GetField("_flyingMelee", Own);
            _carriers = t.GetField("_meleeCarriers", Own);
            string file = null;
            try { file = t.GetProperty("SlimJimFile", Own)?.GetValue(hud) as string; } catch { }
            if (string.IsNullOrEmpty(file))
            {
                file = Path.Combine(Paths.Scripts, "Hud", "Save", "slim_jim.txt");
                Log.Write(Tag, "GTA6Hud's SlimJimFile wasn't found; using " + file);
            }
            return file;
        }

        // The slim jim thrown with GTA6Hud's THROW: in the air, lying there, or picked up by someone. It comes back the
        // way GTA6Hud gives it (picking it up), not from here.
        private bool OutInTheWorld()
        {
            if (_hud == null) return false;
            try
            {
                if (_dropped?.GetValue(_hud) is IList dropped)
                    foreach (object d in dropped)
                        if (IsMine(d, "Hash") && Field(d, "Prop") is Entity prop && prop.Exists()) return true;
                if (_flying?.GetValue(_hud) is IList flying)
                    foreach (object f in flying)
                        if (IsMine(Field(f, "Drop"), "Hash")) return true;
                if (_carriers?.GetValue(_hud) is IDictionary carriers)
                    foreach (object c in carriers.Values)
                        if (IsMine(c, "Weapon") && Field(c, "Ped") is Ped ped && ped.Exists()) return true;
            }
            catch { }
            return false;
        }

        private static object Field(object o, string name) =>
            o?.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(o);

        private static bool IsMine(object o, string hashField)
        {
            object h = Field(o, hashField);
            return h != null && unchecked((uint)Convert.ToInt64(h)) == WeaponHash;
        }

        // As DogWalkers does for the Poo Bag: GTA6Hud's wheel (WeaponTransfer.AllHashes) gets WEAPON_SLIMJIM.
        private bool AddToHudWheel()
        {
            try
            {
                object hud = HudBridge.Gta6Hud();
                if (hud == null) return false;
                Type wt = hud.GetType().Assembly.GetType("GTA6Hud.WeaponTransfer", false);
                FieldInfo f = wt?.GetField("AllHashes", BindingFlags.NonPublic | BindingFlags.Static);
                if (!(f?.GetValue(null) is Array have))
                {
                    Log.Write(Tag, "GTA6Hud's weapon list wasn't found: the slim jim won't show in its wheel");
                    return true;
                }
                Type el = have.GetType().GetElementType();
                object mine = Enum.ToObject(el, WeaponHash);
                foreach (object o in have) if (o.Equals(mine)) return true;
                Array more = Array.CreateInstance(el, have.Length + 1);
                Array.Copy(have, more, have.Length);
                more.SetValue(mine, have.Length);
                f.SetValue(null, more);
                Log.Write(Tag, "the slim jim is in GTA6Hud's weapon wheel");
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(Tag, "GTA6Hud's wheel: " + ex.Message);
                return true;
            }
        }
    }
}
