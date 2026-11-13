using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Dog walkers, as in GTA VI: now and then a local's dog stops and poops, and the owner bags it (or doesn't).
    /// Hit an owner who has a bag in hand and it drops; pick it up (E) and throw it (attack) at someone.
    ///
    ///  * Walkers: the game's own (a dog sticking with one person for a few seconds), plus, with MoreDogWalkers,
    ///    a few people on the pavement given a dog, out of sight (GTA V has few).
    ///  * The dog's poop and the owner's pickup: Walker.cs. The bag: PoopBag.cs, vi_prop_poopbag from the
    ///    vihudanims pack (vi hud\poopbag\prop\make_bag.py).
    ///  * Holding the bag the player is unarmed; the throw locks onto the person nearest the middle of the screen
    ///    (a marker over their head) and leads them. A hit: the poop effect, dirt on their clothes, a stumble
    ///    (SET_PED_TO_RAGDOLL type 3, the wide-leg stagger, which Euphoria Ragdoll Overhaul's behaviours.xml
    ///    turns into its balance reaction), a shout, then they come at you or run. A cop hit gives one star.
    ///  * The bag lands where it falls and can be picked up and thrown again.
    ///
    /// [Switches] DogPoop in ViExtras.ini; the rest in [DogWalkers].
    /// </summary>
    public sealed partial class DogWalkers : Script
    {
        private const int Context = 51;                  // INPUT_CONTEXT: E / d-pad right
        private const int Attack = 24;                   // INPUT_ATTACK: left mouse / RT
        private const int HeadBone = 31086, ChestBone = 24818;
        private static readonly int Unarmed = unchecked((int)0xA2719263);   // WEAPON_UNARMED
        private const float ButtonToWord = 0.8f * 10.8f * 0.7f;   // as MemoryInteract's world prompt
        private const string PickDict = "pickup_object", PickClip = "pickup_low";

        private readonly bool _enabled;
        private readonly Keys _testKey;
        private bool _testRequested;

        private readonly List<Walker> _walkers = new List<Walker>();
        private readonly List<PoopBag> _loose = new List<PoopBag>();
        private readonly List<(Prop prop, int at)> _poops = new List<(Prop, int)>();
        private readonly Dictionary<int, (Ped owner, int since, Vector3 at)> _pairs = new Dictionary<int, (Ped, int, Vector3)>();
        private readonly HashSet<int> _pooped = new HashSet<int>();     // dogs that went already this session
        private readonly List<Reaction> _reactions = new List<Reaction>();
        private readonly List<(int fx, int until)> _splats = new List<(int, int)>();
        private int _scanAt, _spawnAt;

        // the player and the bag
        private PoopBag _carried, _reach, _picking;
        private int _pickAt, _pickSub;
        private int _throwSub, _throwAt;

        public DogWalkers()
        {
            Paths.Init(this);
            _enabled = Config.Get(Config.Switches, "DogPoop", true);
            if (!_enabled) return;
            DogCfg.Load();
            _haveWeapon = Function.Call<bool>(Hash.IS_WEAPON_VALID, PoobagWeapon);
            _native = _haveWeapon && PoobagModel.IsInCdImage;
            Log.Write("DogWalkers", _native ? "WEAPON_POOBAG is in the game with its own model and brown effects: bags go in the wheel"
                                  : _haveWeapon ? "WEAPON_POOBAG is in the game (snowball model): bags go in the wheel, ViExtras shows the bag"
                                                : "no WEAPON_POOBAG (old vihudanims pack?): bags are thrown as a snowball");
            _testKey = Config.Get(DogCfg.Section, "TestKey", Keys.None);
            AutoTestInit();
            KeyDown += (s, e) => { if (_testKey != Keys.None && e.KeyCode == _testKey) _testRequested = true; };
            Tick += OnTick;
            Aborted += (s, e) => Shutdown();
        }

        // ---- what Walker hands over ----------------------------------------------------------------------------------

        internal void AddLoose(PoopBag bag)
        {
            if (bag != null && !_loose.Contains(bag)) _loose.Add(bag);
        }

        /// <summary>Dogs held by this script (HonorDogs pets them, though they're mission entities).</summary>
        internal static readonly HashSet<int> HeldDogs = new HashSet<int>();
        private readonly List<Ped> _keep = new List<Ped>();

        /// <summary>A hurt walker or their dog: still held, so they don't vanish mid-fight; let go once the player's far.</summary>
        internal void KeepUntilFar(Ped p)
        {
            if (p != null && p.Exists() && !_keep.Contains(p)) _keep.Add(p);
        }

        internal void LeavePoop(Prop poop)
        {
            if (poop != null && poop.Exists()) _poops.Add((poop, Game.GameTime));
        }

        /// <summary>
        /// GTA VI's clip: the owner gets punched and the bag bursts in their hand. A spray of poo, a splat at their
        /// feet, dirt on them, their hat knocked off, and BurstLumps lumps of it flying out with the baseball's
        /// physics, to be picked up and thrown.
        /// </summary>
        internal void BurstInHand(PoopBag bag, Ped owner)
        {
            int now = Game.GameTime;
            Vector3 at = bag.Centre;
            bag.Delete();
            int fx = PoopFx.At(at, 2.4f);
            if (fx != 0) _splats.Add((fx, now + 900));
            if (owner != null && owner.Exists())
            {
                int fx2 = PoopFx.OnEntity(owner, new Vector3(0f, 0.2f, 0.3f), 1.6f);
                if (fx2 != 0) _splats.Add((fx2, now + 1100));
                if (!string.IsNullOrEmpty(DogCfg.DamagePack)) Function.Call(Hash.APPLY_PED_DAMAGE_PACK, owner, DogCfg.DamagePack, 0f, 1f);
                if (Function.Call<int>(Hash.GET_PED_PROP_INDEX, owner, 0) >= 0) Function.Call(Hash.KNOCK_OFF_PED_PROP, owner, true, true, true, true);
                Vector3 feet = owner.Position;
                feet.Z = PoopBag.GroundZ(feet + new Vector3(0f, 0f, 0.5f), feet.Z - 1f);
                PoopSplat.Ground(feet + owner.ForwardVector * 0.3f, DogCfg.Rng);
            }
            var r = DogCfg.Rng;
            for (int i = 0; i < DogCfg.BurstLumps; i++)
            {
                double a = r.NextDouble() * Math.PI * 2;
                float out_ = 1.5f + (float)r.NextDouble() * 2.0f;
                var v = new Vector3((float)Math.Cos(a) * out_, (float)Math.Sin(a) * out_, 3.0f + (float)r.NextDouble() * 2.5f);
                PoopBag lump = PoopBag.Lump(at + new Vector3(0f, 0f, 0.05f * i), v, owner);
                if (lump != null) _loose.Add(lump);
            }
            DogCfg.Trace("the bag burst in owner " + (owner != null ? owner.Handle : 0) + "'s hand: " + DogCfg.BurstLumps + " lumps");
        }

        /// <summary>A hurt owner: back on their feet, they square up to you (or, now and then, run).</summary>
        internal void FightBack(Ped owner)
        {
            foreach (Reaction rr in _reactions) if (rr.Ped.Handle == owner.Handle) return;
            _reactions.Add(new Reaction { Ped = owner, At = Game.GameTime + 600, FightBack = true });
        }

        // ---- the tick ------------------------------------------------------------------------------------------------

        private void OnTick(object sender, EventArgs e)
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists()) return;
            int now = Game.GameTime;
            float dt = Game.LastFrameTime;
            bool hidden = HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen;

            if (_testRequested)
            {
                _testRequested = false;
                Test(player, now);
            }
            AutoTestTick(player, now);
            if (now >= _scanAt)
            {
                _scanAt = now + 1000;
                Scan(player, now);
                if (DogCfg.MoreWalkers && now >= _spawnAt) { _spawnAt = now + DogCfg.SpawnEveryMs; TrySpawn(player, now, false); }
                CleanUp(player, now);
            }

            for (int i = _walkers.Count - 1; i >= 0; i--)
                if (!_walkers[i].Tick(this, player, now)) _walkers.RemoveAt(i);

            Tough.Tick(player, now);
            StepBags(player, dt, now);
            Reactions(player, now);
            if (player.IsDead)
            {
                if (_carried != null) DropCarried(player);
                _picking = null;
                return;
            }
            if (_haveWeapon) { PoobagTick(player, now); RidesTick(player, now, dt); }
            if (_carried != null) Carry(player, now, hidden);
            else PickUp(player, now, hidden);
        }

        // ---- WEAPON_POOBAG: the poo bag as a weapon of its own, in the wheel ---------------------------------------
        //
        // The vihudanims pack's weaponpoobag.meta (vi hud\poopbag\weapon\make_weapon.py) is Rockstar's snowball,
        // renamed: same throw, aim and lock-on, up to 10. GTA6Hud's wheel lists it with the thrown weapons
        // (Hud\Weapons\weapon_2566209544.png, "Poo Bag" from the pack's text). Picking up a bag adds one.
        //
        // Native (the pack has w_ex_poobag, make_native.py): the snowball copied, with its own model (the bag) and its
        // own effects, the snowball's browned and renamed *_poobag (proj_vihud_poobag). The game holds it, throws it
        // and hits with it. It has no explosion of its own: GTA V's explosion types are a fixed list (a new one was a
        // grenade), and the snowball's plays the snowball's white puff from the game's shared effects. So at its first
        // knock ViExtras takes it out and plays the brown puff from proj_vihud_poobag itself (PoobagPoof), with the
        // stain and how the person hit takes it.
        //
        // Otherwise (an older pack: w_ex_snowball for its model) the snowball in the hand is hidden and the bag shows;
        // when one leaves the hand, the bag rides the hidden projectile and bursts brown where it goes (RidesTick).

        internal const int PoobagWeapon = unchecked((int)0x98F54008);   // joaat("WEAPON_POOBAG")
        private static readonly Model PoobagModel = new Model("w_ex_poobag");
        private bool _haveWeapon, _native;
        private PoopBag _heldBag;
        private int _lastPoobagAmmo = -1, _poobagThrownAt = -1;
        private bool _wasHoldingPoobag;
        private int _scanThrowAt, _reactionLoadAt;

        // GTA6Hud's wheel lists only the weapons Script Hook V .NET has names for (WeaponTransfer.AllHashes, from
        // Enum.GetValues(WeaponHash)), so the Poo Bag is added to that list once GTA6Hud is up. After that the wheel
        // shows it with the thrown weapons, its icon (Hud\Weapons\weapon_2566209544.png) and its name.
        private bool _inWheel;
        private int _wheelTryAt, _giveAllAt, _ownedBefore = -1;
        private int[] _allWeapons;

        private void WheelAndGiveAll(Ped player, int now)
        {
            if (!_haveWeapon) return;
            if (!_inWheel && now >= _wheelTryAt)
            {
                _wheelTryAt = now + 5000;
                _inWheel = AddToHudWheel();
            }
            // "give all weapons" from any trainer: ten or more weapons arriving at once bring a full stack of poo bags
            if (now < _giveAllAt) return;
            _giveAllAt = now + 500;
            if (_allWeapons == null)
            {
                var list = new List<int>();
                foreach (WeaponHash h in Weapon.GetAllWeaponHashesForHumanPeds())
                    if ((int)h != PoobagWeapon && (int)h != Unarmed) list.Add((int)h);
                _allWeapons = list.ToArray();
            }
            int owned = 0;
            foreach (int h in _allWeapons) if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, player, h, false)) owned++;
            if (_ownedBefore >= 0 && owned - _ownedBefore >= 10 && !player.IsDead)
            {
                Function.Call(Hash.GIVE_WEAPON_TO_PED, player, PoobagWeapon, 10, false, false);
                Function.Call(Hash.SET_PED_AMMO, player, PoobagWeapon, 10, false);
                _lastPoobagAmmo = 10;
                DogCfg.Trace("all weapons given (" + _ownedBefore + " -> " + owned + "): WEAPON_POOBAG x10 too");
            }
            _ownedBefore = owned;
        }

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
                    Log.Write("DogWalkers", "GTA6Hud's weapon list wasn't found: the Poo Bag won't show in its wheel");
                    return true;
                }
                Type el = have.GetType().GetElementType();
                object mine = Enum.ToObject(el, unchecked((uint)PoobagWeapon));
                foreach (object o in have) if (o.Equals(mine)) return true;
                Array more = Array.CreateInstance(el, have.Length + 1);
                Array.Copy(have, more, have.Length);
                more.SetValue(mine, have.Length);
                f.SetValue(null, more);
                Log.Write("DogWalkers", "the Poo Bag is in GTA6Hud's weapon wheel");
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("DogWalkers", "GTA6Hud's wheel: " + ex.Message);
                return true;
            }
        }

        private void GivePoobag(Ped player)
        {
            if (!Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, player, PoobagWeapon, false))
                Function.Call(Hash.GIVE_WEAPON_TO_PED, player, PoobagWeapon, 1, false, true);
            else
                Function.Call(Hash.ADD_AMMO_TO_PED, player, PoobagWeapon, 1);
            Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, PoobagWeapon, true);
            _lastPoobagAmmo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, player, PoobagWeapon);
        }

        private void PoobagTick(Ped player, int now)
        {
            bool holding = !player.IsDead && !player.IsInVehicle() &&
                           Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, player) == PoobagWeapon;
            int ammo = Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, player, PoobagWeapon, false)
                ? Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, player, PoobagWeapon) : 0;

            // one just left the hand (it was out, and the ammo went down): the game's projectile out, the bag in its
            // place. Ammo gone any other way (an arrest, a shop) throws nothing.
            // Thrown: the ammo went down while it was out, or (infinite ammo, which "give all weapons" trainers turn on,
            // never goes down) GTA says the player is throwing, or a fresh snowball-model projectile is in flight by them.
            bool thrown = _wasHoldingPoobag && _lastPoobagAmmo > 0 && ammo < _lastPoobagAmmo;
            if (!thrown && holding && Function.Call<bool>(Hash.IS_PED_SHOOTING, player)) thrown = true;
            if (!thrown && (holding || _wasHoldingPoobag) && now >= _scanThrowAt)
            {
                _scanThrowAt = now + 100;
                thrown = FindThrown(player) != null;
            }
            if (thrown && _poobagThrownAt < 0) _poobagThrownAt = now;
            _lastPoobagAmmo = ammo;
            _wasHoldingPoobag = holding;
            if (_poobagThrownAt >= 0 && PoobagLaunch(player, now)) _poobagThrownAt = -1;

            if (holding && _native) PoobagPoof.Request();
            if (holding && now >= _reactionLoadAt) { _reactionLoadAt = now + 5000; Reaction.Request(true); }
            if (holding && _poobagThrownAt < 0 && !_native)
            {
                if (_heldBag == null || !_heldBag.Exists || _heldBag.Now != PoopBag.State.Held)
                {
                    _heldBag = PoopBag.InHand(player);
                }
                int held = Function.Call<int>(Hash.GET_CURRENT_PED_WEAPON_ENTITY_INDEX, player, 0);
                if (held != 0) Function.Call(Hash.SET_ENTITY_VISIBLE, held, false, false);
                if (_heldBag != null) _heldBag.KeepHanging(Game.LastFrameTime);
            }
            else if (!holding && _poobagThrownAt < 0 && _heldBag != null)
            {
                _heldBag.Delete();
                _heldBag = null;
            }
        }

        // The game's poo bag projectile (it has the snowball's model) just thrown: made invisible, the bag rides it. The
        // game flies it, hits with it and hurts with it as it does a snowball; when it goes (DestroyOnImpact), the bag
        // bursts brown right there (RidesTick).
        private sealed class Ride { public Prop Proj; public PoopBag Bag; public Vector3 Last, Vel; public int At, StillSince; }
        private readonly List<Ride> _rides = new List<Ride>();

        private bool PoobagLaunch(Ped player, int now)
        {
            int t = now - _poobagThrownAt;
            Prop proj = FindThrown(player);
            if (proj == null)
            {
                if (t < 400) return false;
                DogCfg.Trace("no poo bag projectile found after the throw");
                if (_heldBag != null) { _heldBag.Delete(); _heldBag = null; }
                return true;
            }
            if (_native)
            {
                // the game's own: watched only, for where it goes
                _rides.Add(new Ride { Proj = proj, Last = proj.Position, Vel = proj.Velocity, At = now });
                DogCfg.Trace("WEAPON_POOBAG thrown (native): projectile " + proj.Handle);
                AtThrown();
                return true;
            }
            Function.Call(Hash.SET_ENTITY_VISIBLE, proj, false, false);
            PoopBag bag = _heldBag != null && _heldBag.Exists ? _heldBag : PoopBag.InHand(player);
            _heldBag = null;
            if (bag == null) return true;
            bag.Ride(proj);
            _rides.Add(new Ride { Proj = proj, Bag = bag, Last = proj.Position, Vel = proj.Velocity, At = now });
            AtThrown();
            DogCfg.Trace("WEAPON_POOBAG thrown: the bag rides projectile " + proj.Handle);
            return true;
        }

        // The one in flight: with more poo bags left, the next one is already back in the hand (the weapon's object,
        // attached to the player) and is the nearest; the thrown one is free, moving, and not one we ride already.
        private Prop FindThrown(Ped player)
        {
            Prop proj = null;
            float best = 0f;
            int inHand = Function.Call<int>(Hash.GET_CURRENT_PED_WEAPON_ENTITY_INDEX, player, 0);
            foreach (Prop p in World.GetNearbyProps(player.Position, 20f, _native ? PoobagModel : SnowballModel))
            {
                if (p == null || !p.Exists() || p.Handle == inHand) continue;
                if (Function.Call<bool>(Hash.IS_ENTITY_ATTACHED, p)) continue;
                bool known = false;
                foreach (Ride r in _rides) if (r.Proj != null && r.Proj.Handle == p.Handle) { known = true; break; }
                if (known) continue;
                float speed = p.Velocity.Length();
                if (speed > 2f && speed > best) { best = speed; proj = p; }
            }
            return proj;
        }

        private void RidesTick(Ped player, int now, float dt)
        {
            for (int i = _rides.Count - 1; i >= 0; i--)
            {
                Ride r = _rides[i];
                if (r.Proj != null && r.Proj.Exists())
                {
                    Vector3 v = r.Proj.Velocity;
                    bool knocked = false;
                    // (the first 80 ms are skipped while it's still by the thrower's hand; once 1.2 m clear it counts at once:
                    // a car hit from close up was missed, and it burst on the wall behind, 5 Nov test)
                    if (r.Bag == null && (now - r.At > 80 || r.Proj.Position.DistanceTo(player.Position) > 1.2f))
                    {
                        // native: nothing blows it up, so the first thing it hits (it slows or turns sharply, or the
                        // game says it touched something away from the thrower's hand) is where it bursts
                        float was = r.Vel.Length(), speed = v.Length();
                        knocked = was > 3f && (speed < was * 0.55f || Vector3.Dot(v, r.Vel) < 0.6f * was * speed);
                        if (!knocked && Function.Call<bool>(Hash.HAS_ENTITY_COLLIDED_WITH_ANYTHING, r.Proj)
                            && r.Proj.Position.DistanceTo(player.Position) > 1.2f) knocked = true;
                    }
                    r.Last = r.Proj.Position;
                    if (!knocked)
                    {
                        if (v.Length() > 0.5f) { r.Vel = v; r.StillSince = 0; }
                        else if (r.StillSince == 0) r.StillSince = now;
                        if (r.Bag != null)
                        {
                            Function.Call(Hash.SET_ENTITY_VISIBLE, r.Proj, false, false);
                            r.Bag.Follow(r.Last, v, dt);
                        }
                        bool stopped = r.StillSince != 0 && now - r.StillSince > 250;
                        if (!stopped && now - r.At < 10000) continue;
                    }
                    r.Proj.Delete();                      // it hit something, or landed without going off: it goes now
                }
                // It went off: what it hit, just ahead along its path
                _rides.RemoveAt(i);
                Vector3 dir = r.Vel.Length() > 0.1f ? r.Vel.Normalized : new Vector3(0f, 0f, -1f);
                Vector3 at = r.Last;
                Vector3 normal = -dir;
                // (a wall or the ground is no entity: its point and facing count too, so the stain lands on it)
                bool struck = PoopBag.RayAll(r.Last - dir * 0.3f, r.Last + dir * 1.2f, player, 1 | 2 | 4 | 8 | 16, out Vector3 point, out Vector3 nrm, out Entity what);
                Ped hitPed = what as Ped;
                if (struck)
                {
                    at = point;
                    float nl = nrm.Length();
                    if (!float.IsNaN(nl) && nl > 0.5f && nl < 1.5f) normal = nrm / nl;   // (a ray once gave 7e25 for one)
                }
                if (hitPed == null) hitPed = NearestPed(at, player, 1.1f);
                if (r.Bag != null) r.Bag.Delete();
                if (r.Bag == null && _native)
                {
                    // the snowball's puff, brown, from the weapon's own effects, and a brown splat where it hit (the
                    // snowball's own white snow splat is turned off in proj_vihud_poobag); a car it hits gets dirtier
                    int fb = PoobagPoof.At(at, normal, hitPed != null);
                    if (fb != 0) _splats.Add((fb, now + 700));
                    if (what is Vehicle car && car.Exists())
                    {
                        float dirt = Function.Call<float>(Hash.GET_VEHICLE_DIRT_LEVEL, car);
                        Function.Call(Hash.SET_VEHICLE_DIRT_LEVEL, car, Math.Min(15f, dirt + 3f));
                    }
                    // the hit hurts as the snowball's does (25); if the game's impact didn't, this does
                    if (hitPed != null && hitPed.Exists() && !hitPed.IsDead)
                    {
                        if (!Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_WEAPON, hitPed, PoobagWeapon, 0))
                            Function.Call(Hash.APPLY_DAMAGE_TO_PED, hitPed, 25, false, 0, PoobagWeapon);
                        Function.Call(Hash.CLEAR_ENTITY_LAST_WEAPON_DAMAGE, hitPed);
                    }
                }
                else Burst(at, normal, hitPed, player, now);
                AtBurst(hitPed, what, struck);
                if (hitPed != null && hitPed.Exists()) Splat(player, hitPed, at, now);
                DogCfg.Trace("the poo bag burst on " + (hitPed != null ? "ped " + hitPed.Handle : what is Vehicle ? "a vehicle"
                             : struck ? "the world (normal " + normal.X.ToString("0.0") + "," + normal.Y.ToString("0.0") + "," + normal.Z.ToString("0.0") + ")" : "the world"));
            }
        }

        /// <summary>
        /// The bag going off, as in GTA VI's clip (the owner punched, 21 s in): a brown spray, lumps of poo flying
        /// out of it, a splat. The spray is the drunk driver's puke (scr_puke_in_car, scr_familyscenem: it's in the
        /// game) browned and fired four ways, over the snowball's puff browned and Chop's poo; the lumps are
        /// vi_prop_poop on the ball's physics, left lying where they land (pick one up: it's a poo bag).
        /// </summary>
        private void Burst(Vector3 at, Vector3 normal, Ped onPed, Ped player, int now)
        {
            int pf = PoopPoof.At(at, normal, onPed != null);
            if (pf != 0) _splats.Add((pf, now + 700));
            int chunks = PoopFx.At(at, 2.2f);
            if (chunks != 0) _splats.Add((chunks, now + 500));
            PoopSpray.At(at, normal, DogCfg.Rng);
            if (normal.Length() < 0.1f) normal = new Vector3(0f, 0f, 1f);
            normal.Normalize();
            var r = DogCfg.Rng;
            for (int i = 0; i < DogCfg.BurstChunks; i++)
            {
                double a = (i + r.NextDouble() * 0.6) * Math.PI * 2 / Math.Max(1, DogCfg.BurstChunks);
                var outward = new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f) * (1.6f + (float)r.NextDouble() * 1.6f);
                Vector3 v = outward + normal * (2.0f + (float)r.NextDouble() * 1.5f) + new Vector3(0f, 0f, 1.5f);
                PoopBag lump = PoopBag.Lump(at + normal * 0.12f, v, onPed ?? player);
                if (lump != null) _loose.Add(lump);
            }
            // a pile of old lumps goes, oldest first
            int lumps = 0;
            for (int i = _loose.Count - 1; i >= 0; i--) if (_loose[i].IsLump) lumps++;
            for (int i = 0; i < _loose.Count && lumps > DogCfg.MaxLumps; i++)
            {
                if (!_loose[i].IsLump || _loose[i].Now != PoopBag.State.Lying) continue;
                _loose[i].Delete();
                _loose.RemoveAt(i);
                i--;
                lumps--;
            }
        }

        // The person whose head, chest or hips are nearest `at`, within `radius`, not the player.
        private static Ped NearestPed(Vector3 at, Ped player, float radius)
        {
            Ped best = null;
            float bestD = radius;
            foreach (Ped p in World.GetNearbyPeds(at, radius + 1.5f))
            {
                if (p == null || !p.Exists() || p.Handle == player.Handle || p.IsDead) continue;
                foreach (int b in new[] { 31086, 24818, 11816 })
                {
                    float d = Function.Call<Vector3>(Hash.GET_PED_BONE_COORDS, p, b, 0f, 0f, 0f).DistanceTo(at);
                    if (d < bestD) { bestD = d; best = p; }
                }
            }
            return best;
        }


        // ---- finding walkers -----------------------------------------------------------------------------------------

        private bool Known(Ped ped)
        {
            foreach (Walker w in _walkers)
                if ((w.Dog != null && w.Dog.Handle == ped.Handle) || (w.Owner != null && w.Owner.Handle == ped.Handle)) return true;
            return false;
        }

        private static bool GoodOwner(Ped p, Ped player)
        {
            if (p == null || !p.Exists() || p == player || p.IsDead || !p.IsHuman) return false;
            if (p.IsInVehicle() || Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, p)) return false;
            int type = Function.Call<int>(Hash.GET_PED_TYPE, p);
            if (type == 6 || type == 27 || type == 29) return false;           // cop, swat, army
            if (Function.Call<bool>(Hash.IS_PED_IN_COMBAT, p, 0) || Function.Call<bool>(Hash.IS_PED_FLEEING, p)) return false;
            return true;
        }

        // The game's own pairs: a dog that has stuck with one person, both walking, for a few seconds.
        private void Scan(Ped player, int now)
        {
            if (_walkers.Count >= DogCfg.MaxWalkers) return;
            Ped[] near = World.GetNearbyPeds(player, DogCfg.ScanRange);
            foreach (Ped dog in near)
            {
                if (dog == null || !dog.Exists() || dog.IsDead || !DogCfg.IsDog(dog)) continue;
                if (Known(dog) || _pooped.Contains(dog.Handle)) continue;
                if (Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, dog)) continue;   // Chop, a mission's dog
                Ped owner = null;
                float best = 3.5f;
                foreach (Ped p in near)
                {
                    if (!GoodOwner(p, player) || Known(p)) continue;
                    float d = p.Position.DistanceTo(dog.Position);
                    if (d < best) { best = d; owner = p; }
                }
                if (owner == null) { _pairs.Remove(dog.Handle); continue; }

                int gd = Function.Call<int>(Hash.GET_PED_GROUP_INDEX, dog), go = Function.Call<int>(Hash.GET_PED_GROUP_INDEX, owner);
                bool sameGroup = gd != 0 && gd == go && Function.Call<bool>(Hash.IS_PED_IN_GROUP, dog);
                if (!_pairs.TryGetValue(dog.Handle, out var seen) || seen.owner == null || seen.owner.Handle != owner.Handle)
                {
                    _pairs[dog.Handle] = (owner, now, dog.Position);
                    if (!sameGroup) continue;
                    seen = _pairs[dog.Handle];
                }
                bool walked = dog.Position.DistanceTo(seen.at) > 1.5f;
                if (sameGroup || (now - seen.since > 3000 && walked))
                {
                    _pairs.Remove(dog.Handle);
                    _walkers.Add(new Walker(owner, dog, false, now));
                    DogCfg.Trace("walker found: owner " + owner.Handle + ", dog " + dog.Handle + (sameGroup ? " (group)" : ""));
                    if (_walkers.Count >= DogCfg.MaxWalkers) return;
                }
            }
        }

        // MoreDogWalkers: someone walking along gets a dog, out of sight. `test`: right here, now.
        private Walker TrySpawn(Ped player, int now, bool test)
        {
            if (!test)
            {
                int spawned = 0;
                foreach (Walker w in _walkers) if (w.Spawned) spawned++;
                if (spawned >= DogCfg.MaxSpawned || _walkers.Count >= DogCfg.MaxWalkers) return null;
                if (DogCfg.Rng.NextDouble() > DogCfg.SpawnChance) return null;
                if (Function.Call<bool>(Hash.GET_MISSION_FLAG)) return null;
                int hour = World.CurrentTimeOfDay.Hours;
                if (hour < DogCfg.FromHour || hour >= DogCfg.ToHour) return null;
                if (player.IsInVehicle() && player.CurrentVehicle != null && player.CurrentVehicle.Speed > 12f) return null;
            }

            var picks = new List<Ped>();
            foreach (Ped p in World.GetNearbyPeds(player, test ? 30f : DogCfg.SpawnMax))
            {
                if (!GoodOwner(p, player) || Known(p)) continue;
                int type = Function.Call<int>(Hash.GET_PED_TYPE, p);
                if (type != 4 && type != 5) continue;                                    // civilians only
                if (Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, p) || Function.Call<bool>(Hash.IS_PED_IN_GROUP, p)) continue;
                float d = p.Position.DistanceTo(player.Position);
                if (test) { if (d < 2f) continue; }
                else
                {
                    if (d < DogCfg.SpawnMin) continue;
                    if (Function.Call<bool>(Hash.IS_ENTITY_ON_SCREEN, p) && d < DogCfg.SpawnMax * 0.7f) continue;
                    float speed = p.Velocity.Length();
                    if (speed < 0.5f || speed > 2.4f) continue;                         // walking, not standing or running
                }
                picks.Add(p);
            }
            if (picks.Count == 0) return null;
            if (test) picks.Sort((a, b) => a.Position.DistanceTo(player.Position).CompareTo(b.Position.DistanceTo(player.Position)));
            Ped owner = test ? picks[0] : picks[DogCfg.Rng.Next(picks.Count)];
            return GiveDog(owner, now);
        }

        /// <summary>A big dog behind `owner`, on a lead: a new walker.</summary>
        private Walker GiveDog(Ped owner, int now)
        {
            Model m = DogCfg.SpawnModels[DogCfg.Rng.Next(DogCfg.SpawnModels.Length)];
            if (!m.IsInCdImage || !m.Request(1500)) return null;
            Vector3 at = owner.Position - owner.ForwardVector * 1.1f + owner.RightVector * 0.5f;
            at.Z = PoopBag.GroundZ(at + new Vector3(0f, 0f, 1.0f), owner.Position.Z - 1f);
            Ped dog = World.CreatePed(m, at, owner.Heading);
            m.MarkAsNoLongerNeeded();
            if (dog == null || !dog.Exists()) return null;
            // kept until the player has seen it (Walker), then left to the game; its follow task stays
            var walker = new Walker(owner, dog, true, now);
            _walkers.Add(walker);
            DogCfg.Trace("walker made: owner " + owner.Handle + " got a " + m.Hash.ToString("X8") + " (dog " + dog.Handle + ")");
            return walker;
        }

        private void Test(Ped player, int now)
        {
            Walker best = null;
            float bestD = 50f;
            foreach (Walker w in _walkers)
            {
                if (!w.Alive || w.InSequence || (!w.Big && !DogCfg.SmallDogsPoop)) continue;
                float d = w.Dog.Position.DistanceTo(player.Position);
                if (d < bestD) { bestD = d; best = w; }
            }
            if (best == null) best = TrySpawn(player, now, true);
            if (best == null)
            {
                Feed.Post("DogWalkers test: nobody walking near you to give a dog to");
                return;
            }
            _pooped.Remove(best.Dog.Handle);
            best.PoopNow(now + 1500);
            Feed.Post("DogWalkers test: dog " + best.Dog.Handle + " goes in a moment");
        }

        // ---- bags in the air -----------------------------------------------------------------------------------------

        private void StepBags(Ped player, float dt, int now)
        {
            for (int i = _loose.Count - 1; i >= 0; i--)
            {
                PoopBag b = _loose[i];
                if (!b.Exists) { _loose.RemoveAt(i); continue; }
                bool thrown = b.Now == PoopBag.State.Flying && b.Thrower == player;
                PoopBag.Hit h = b.Step(dt);
                if (h.Happened && h.Burst)
                {
                    int pf = PoopPoof.At(h.At, h.Normal, h.Ped != null);
                    if (pf != 0) _splats.Add((pf, now + 700));
                    if (thrown && h.Ped != null) Splat(player, h.Ped, h.At, now);
                    DogCfg.Trace("it burst on " + (h.Ped != null ? "ped " + h.Ped.Handle : "the world"));
                    _loose.RemoveAt(i);
                }
                else if (h.Happened && thrown && h.Ped != null)
                {
                    Splat(player, h.Ped, h.At, now);
                    if (DogCfg.BurstOnHit)
                    {
                        b.Delete();                 // it burst on them
                        _loose.RemoveAt(i);
                    }
                }
            }
            for (int i = _splats.Count - 1; i >= 0; i--)
            {
                if (now < _splats[i].until) continue;
                PoopFx.Stop(_splats[i].fx);
                _splats.RemoveAt(i);
            }
        }

        private void Splat(Ped player, Ped ped, Vector3 at, int now)
        {
            // It burst on them (the puff is PoopPoof's): poo down their front, and a splat at their feet.
            if (DogCfg.BurstOnHit)
            {
                int fx2 = PoopFx.OnEntity(ped, new Vector3(0f, 0.15f, 0.25f), 1.6f);
                if (fx2 != 0) _splats.Add((fx2, now + 1200));
                Vector3 feet = ped.Position;
                feet.Z = PoopBag.GroundZ(feet + new Vector3(0f, 0f, 0.5f), feet.Z - 1f);
                Vector3 toward = at - feet;
                toward.Z = 0f;
                toward = toward.Length() > 0.05f ? toward.Normalized : ped.ForwardVector;
                PoopSplat.Ground(feet + toward * 0.35f, DogCfg.Rng);
            }
            if (!ped.IsHuman)
            {
                Function.Call(Hash.TASK_SMART_FLEE_PED, ped, player, 40f, 10000, false, false);
                DogCfg.Trace("bag hit an animal " + ped.Handle);
                return;
            }
            Tough.Make(ped);                         // they'll likely come at you: a punch or two shouldn't end it
            if (!string.IsNullOrEmpty(DogCfg.DamagePack))
                Function.Call(Hash.APPLY_PED_DAMAGE_PACK, ped, DogCfg.DamagePack, 0f, 1f);
            Function.Call(Hash.PLAY_PED_AMBIENT_SPEECH_NATIVE, ped, DogCfg.Rng.NextDouble() < 0.5 ? "GENERIC_SHOCKED_HIGH" : "GENERIC_CURSE_HIGH", "SPEECH_PARAMS_FORCE_SHOUTED");
            // (the old fall: HitRagdoll; they went down at every hit, the user's clip of 5 Nov)
            if (DogCfg.HitRagdoll && !ped.IsInVehicle())
                Function.Call(Hash.SET_PED_TO_RAGDOLL, ped, DogCfg.StumbleMs, DogCfg.StumbleMs + 400, 3, false, false, false);
            int type = Function.Call<int>(Hash.GET_PED_TYPE, ped);
            if ((type == 6 || type == 27) && DogCfg.CopsGiveStar && Game.Player.WantedLevel == 0)
                Game.Player.WantedLevel = 1;
            else
            {
                foreach (Reaction r in _reactions)
                    if (r.Ped.Handle == ped.Handle) { r.Rehit(now); DogCfg.Trace("bag hit " + ped.Handle + " again"); return; }
                bool retch = DogCfg.Rng.NextDouble() < DogCfg.VomitChance;
                Reaction.Request(retch);
                _reactions.Add(new Reaction { Ped = ped, At = now + (DogCfg.HitRagdoll ? DogCfg.StumbleMs + 500 : 0), Retch = retch });
            }
            DogCfg.Trace("bag hit " + ped.Handle);
        }

        /// <summary>
        /// Someone the bag burst on, all of it on their feet (Rockstar's own clips, each checked standing in the
        /// game's files: hip, knee and spine bends against a standing clip's):
        ///
        ///   1. the recoil: a step away from it (reaction@male_stand@big_intro, the side it came from), with a shout;
        ///   2. disgust: they look down at their clothes and hate it (the clothes shop's try_shirt_negative), and curse;
        ///   3. the retch (VomitChance): Trevor's puking into a fountain (switch@trevor@puking_into_fountain), the
        ///      game's one retch on its feet: once round its loop, bent over with straight legs, with puke from the head,
        ///      then its exit from 55% to 85%, where he straightens up. The body's own height in each clip (SKEL_ROOT's
        ///      and the mover's z) ruled the others out: the drunk driver's vomit_outside sits at his car seat's height
        ///      (they sat in mid-air, the user's screenshot), Michael's missfam5_blackout vomit is 0.91 m down, on the
        ///      floor, for its first 40% (the 5 Nov test measured the hips 8 cm off the ground), and the nightclub's is
        ///      on its knees;
        ///   4. they turn on you: an angry gesture (upper body) and an insult;
        ///   5. then they come at you or run.
        ///
        /// Through 1-4 the game's own reactions are held off (SET_BLOCKING_OF_NON_TEMPORARY_EVENTS): before, the
        /// thrower's aim made them put their hands up and beg ("Don't hurt me!"). Another hit before the retch starts
        /// it again from the recoil. A punched owner (FightBack) goes straight to 5.
        /// </summary>
        private sealed class Reaction
        {
            public const string VomitDict = "switch@trevor@puking_into_fountain", VomitClip = "trev_fountain_puke_loop", VomitExit = "trev_fountain_puke_exit";
            private const int LoopMs = 3700;                                             // once round the loop
            private const float ExitFrom = 0.55f, ExitTo = 0.85f;                        // bent over until 0.70, upright by 0.80
            private static readonly int[] PukeAtMs = { 700, 2300 };
            private const string RecoilDict = "reaction@male_stand@big_intro@";       // + backward / forward / left / right
            private const string ShirtDictM = "clothingshirt", ShirtClipM = "try_shirt_negative_b";        // 3.2 s
            private const string ShirtDictF = "mp_clothing@female@shirt", ShirtClipF = "try_shirt_negative_a";   // 7.1 s, cut at 4.5
            private const string GestDictM = "gestures@m@standing@casual", GestDictF = "gestures@f@standing@casual";
            private static readonly string[] Gestures = { "gesture_what_hard", "gesture_damn", "gesture_no_way" };
            private const int UpperBody = 16 | 32;                                       // upper body only, over what the legs do

            public static void Request(bool retch)
            {
                foreach (string d in new[] { RecoilDict + "backward", RecoilDict + "forward", RecoilDict + "left", RecoilDict + "right",
                                             ShirtDictM, ShirtDictF, GestDictM, GestDictF })
                    Function.Call(Hash.REQUEST_ANIM_DICT, d);
                if (retch)
                {
                    Function.Call(Hash.REQUEST_ANIM_DICT, VomitDict);
                    PukeFx.Request();
                }
            }

            public Ped Ped;
            public int At, Stage, StageAt, Puked, Since;
            public bool Retch, FightBack;
            private bool _entered, _blocked, _gestured;
            private string _dict, _clip;

            /// <summary>Hit again: before the retch, it starts again from the recoil; the retch carries on.</summary>
            public void Rehit(int now)
            {
                if (FightBack)
                {
                    // a punched owner, hit with it before they turned on you: the whole reaction, then
                    FightBack = false;
                    Retch = DogCfg.Rng.NextDouble() < DogCfg.VomitChance;
                    Request(Retch);
                }
                else if (Stage == 3 || Stage == 11) return;
                Stage = 0;
                _entered = false;
                At = now;
            }

            private void Go(int stage, int now)
            {
                Stage = stage;
                StageAt = now;
                _entered = false;
            }

            private bool Loaded(string dict)
            {
                if (Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict)) return true;
                Function.Call(Hash.REQUEST_ANIM_DICT, dict);
                return false;
            }

            private void Play(string dict, string clip, int flags, float start, float blendIn)
            {
                _dict = dict;
                _clip = clip;
                Function.Call(Hash.TASK_PLAY_ANIM, Ped, dict, clip, blendIn, -4f, -1, flags, start, false, false, false);
            }

            private bool Playing() => _dict != null && Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Ped, _dict, _clip, 3);

            private void Block(bool on)
            {
                if (_blocked == on) return;
                _blocked = on;
                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, Ped, on);
            }

            private void Say(string line) => Function.Call(Hash.PLAY_PED_AMBIENT_SPEECH_NATIVE, Ped, line, "SPEECH_PARAMS_FORCE_SHOUTED");

            // which way they step: away from the thrower (the clip family is named for the way they move)
            private string RecoilSide(Ped player)
            {
                Vector3 to = player.Position - Ped.Position;
                to.Z = 0f;
                if (to.Length() < 0.01f) return "backward";
                to.Normalize();
                float ahead = Vector3.Dot(to, Ped.ForwardVector), right = Vector3.Dot(to, Ped.RightVector);
                if (ahead > 0.5f) return "backward";
                if (ahead < -0.5f) return "forward";
                return right > 0f ? "left" : "right";
            }

            /// <summary>One frame; true once it's over.</summary>
            public bool Tick(Ped player, int now)
            {
                if (Ped == null || !Ped.Exists() || Ped.IsDead || Ped.IsInVehicle()) { if (Ped != null && Ped.Exists()) Block(false); return true; }
                if (now < At) return false;
                if (Since == 0) Since = now;
                if (Function.Call<bool>(Hash.IS_PED_RAGDOLL, Ped) && now - Since < 6000) { At = now + 300; return false; }
                if (FightBack) return Finish(player);
                if (StageAt == 0) StageAt = now;
                int t = now - StageAt;
                bool male = Function.Call<bool>(Hash.IS_PED_MALE, Ped);

                switch (Stage)
                {
                    case 0:   // 1. the recoil
                    {
                        Block(true);
                        if (!DogCfg.Stumble) { Go(1, now); return false; }
                        if (!_entered) { StageAt = now; _entered = true; t = 0; }   // (a new reaction's clock starts here)
                        string dict = RecoilDict + RecoilSide(player), clip = "reaction_" + dict.Substring(RecoilDict.Length) + "_big_intro_a";
                        if (!Loaded(dict)) { if (t > 3000) { DogCfg.Trace("reaction " + Ped.Handle + ": " + dict + " didn't load, no recoil"); Go(1, now); } return false; }
                        Function.Call(Hash.CLEAR_PED_TASKS, Ped);
                        Play(dict, clip, 0, 0f, 8f);
                        DogCfg.Trace("reaction " + Ped.Handle + ": recoil " + clip);
                        Go(10, now);
                        return false;
                    }
                    case 10:  // (the recoil playing)
                        if (t < 300 || (Playing() && t < 2800)) return false;
                        Go(1, now);
                        return false;

                    case 1:   // 2. disgust: a look down at what's on them
                    {
                        string dict = male ? ShirtDictM : ShirtDictF, clip = male ? ShirtClipM : ShirtClipF;
                        if (!_entered)
                        {
                            if (!Loaded(dict)) { if (t > 3000) { DogCfg.Trace("reaction " + Ped.Handle + ": " + dict + " didn't load, no disgust"); Go(2, now); } return false; }
                            Function.Call(Hash.CLEAR_PED_TASKS, Ped);
                            Play(dict, clip, 0, 0f, 4f);
                            DogCfg.Trace("reaction " + Ped.Handle + ": disgust " + clip);
                            Function.Call(Hash.SET_FACIAL_IDLE_ANIM_OVERRIDE, Ped, "mood_frustrated_1", 0);
                            Say("GENERIC_CURSE_HIGH");
                            _entered = true;
                            StageAt = now;
                            return false;
                        }
                        if (t < 300 || (Playing() && t < (male ? 3400 : 4500))) return false;
                        Go(2, now);
                        return false;
                    }

                    case 2:   // 3. the retch: bent over, on their feet
                        if (!Retch) { Go(4, now); return false; }
                        if (!Loaded(VomitDict)) { if (t > 3000) { Retch = false; Go(4, now); } return false; }
                        Function.Call(Hash.CLEAR_PED_TASKS, Ped);
                        Play(VomitDict, VomitClip, 1, 0f, 3f);                      // (flag 1: the loop goes round)
                        DogCfg.Trace("reaction " + Ped.Handle + ": retch");
                        Puked = 0;
                        Go(3, now);
                        return false;
                    case 3:
                        while (Puked < PukeAtMs.Length && t >= PukeAtMs[Puked])
                        {
                            PukeFx.OnHead(Ped);
                            Puked++;
                        }
                        if (t < LoopMs) return false;
                        // (the 8 argument is where in the clip it starts, 0 to 1)
                        Play(VomitDict, VomitExit, 0, ExitFrom, 3f);
                        Go(11, now);
                        return false;
                    case 11:  // straightening up
                    {
                        bool playing = Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, Ped, VomitDict, VomitExit, 3);
                        float ph = playing ? Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, Ped, VomitDict, VomitExit) : 1f;
                        if (ph < ExitTo && (playing || t < 600) && t < 5000) return false;
                        if (playing) Function.Call(Hash.STOP_ANIM_TASK, Ped, VomitDict, VomitExit, -3f);
                        Go(4, now);
                        return false;
                    }

                    case 4:   // 4. they turn on you: a gesture and an insult
                    {
                        string dict = male ? GestDictM : GestDictF;
                        if (!_entered)
                        {
                            Function.Call(Hash.CLEAR_PED_TASKS, Ped);
                            Function.Call(Hash.TASK_TURN_PED_TO_FACE_ENTITY, Ped, player, 1200);
                            Function.Call(Hash.SET_FACIAL_IDLE_ANIM_OVERRIDE, Ped, "mood_angry_1", 0);
                            _entered = true;
                            _gestured = false;
                            StageAt = now;
                            return false;
                        }
                        if (!_gestured && t > 450)
                        {
                            _gestured = true;
                            string g = Gestures[DogCfg.Rng.Next(Gestures.Length)];
                            if (Loaded(dict)) Play(dict, g, UpperBody, 0f, 4f);
                            Say("GENERIC_INSULT_HIGH");
                            DogCfg.Trace("reaction " + Ped.Handle + ": insult " + g);
                        }
                        if (t < 2200) return false;
                        return Finish(player);
                    }
                }
                return Finish(player);
            }

            // 5. they come at you, or run
            private bool Finish(Ped player)
            {
                Block(false);
                Function.Call(Hash.CLEAR_FACIAL_IDLE_ANIM_OVERRIDE, Ped);
                // already at it (or, after a bag burst on them, already off): leave it; a punched owner turns round to fight
                if (Function.Call<bool>(Hash.IS_PED_IN_COMBAT, Ped, 0)) return true;
                if (!FightBack && Function.Call<bool>(Hash.IS_PED_FLEEING, Ped)) return true;
                bool fight = FightBack ? DogCfg.Rng.NextDouble() < DogCfg.FightBackChance
                    : DogCfg.Rng.NextDouble() < (Function.Call<bool>(Hash.IS_PED_MALE, Ped) ? DogCfg.FightChance : DogCfg.FightChance * 0.5);
                if (fight) Function.Call(Hash.TASK_COMBAT_PED, Ped, player, 0, 16);
                else Function.Call(Hash.TASK_SMART_FLEE_PED, Ped, player, 80f, -1, false, false);
                Function.Call(Hash.SET_PED_KEEP_TASK, Ped, true);
                return true;
            }
        }

        private void Reactions(Ped player, int now)
        {
            for (int i = _reactions.Count - 1; i >= 0; i--)
                if (_reactions[i].Tick(player, now)) _reactions.RemoveAt(i);
        }

        // ---- the player picks it up ------------------------------------------------------------------------------------

        private static bool OnFootAndFree(Ped player)
        {
            if (player.IsDead || player.IsInVehicle() || Function.Call<bool>(Hash.IS_PED_GETTING_INTO_A_VEHICLE, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_RAGDOLL, player) || Function.Call<bool>(Hash.IS_PED_SWIMMING, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_CLIMBING, player) || Function.Call<bool>(Hash.IS_PED_CUFFED, player)) return false;
            if (Function.Call<bool>(Hash.IS_PED_FALLING, player) || Function.Call<bool>(Hash.IS_PED_JUMPING, player)) return false;
            return true;
        }

        // E / d-pad right, this frame. GTA6Hud turns INPUT_CONTEXT off on foot for its own prompts, so it's read the
        // way the fuel nozzle reads it: the disabled control's state (true either way), or the E key itself.
        private bool _contextWasDown;

        private bool ContextJustPressed()
        {
            if (_autoPress) { _autoPress = false; _contextWasDown = true; return true; }   // AutoTest's press
            bool down = Game.IsKeyPressed(Keys.E) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, Context);
            bool just = down && !_contextWasDown;
            _contextWasDown = down;
            return just;
        }

        private void PickUp(Ped player, int now, bool hidden)
        {
            bool pressed = ContextJustPressed();      // every frame, so a press is only ever counted once
            if (_picking != null) { Picking(player, now); return; }
            _reach = null;
            if (!OnFootAndFree(player) || HudBridge.Busy) return;
            float best = DogCfg.PickUpRange;
            foreach (PoopBag b in _loose)
            {
                if (!b.Exists || b.Now != PoopBag.State.Lying) continue;
                float d = b.Centre.DistanceTo(player.Position - new Vector3(0f, 0f, 0.9f));
                if (d < best) { best = d; _reach = b; }
            }
            if (_reach == null || hidden) return;
            DrawPrompt(_reach.Centre + new Vector3(0f, 0f, 0.25f), Context, "wa_label_pick_up");
            if (!pressed) return;

            _picking = _reach;
            _pickAt = now;
            _pickSub = 0;
            Function.Call(Hash.REQUEST_ANIM_DICT, PickDict);
            Vector3 c = _picking.Centre;
            Function.Call(Hash.TASK_TURN_PED_TO_FACE_COORD, player, c.X, c.Y, c.Z, 400);
        }

        private void Picking(Ped player, int now)
        {
            int t = now - _pickAt;
            if (!_picking.Exists || !OnFootAndFree(player)) { _picking = null; return; }
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, Attack, true);
            if (_pickSub == 0)
            {
                if (t < 400) return;
                if (Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, PickDict))
                {
                    Function.Call(Hash.TASK_PLAY_ANIM, player, PickDict, PickClip, 8f, -8f, -1, 0, 0f, false, false, false);
                    _pickSub = 1;
                    _pickAt = now;
                }
                else if (t > 2500) Take(player);
                return;
            }
            bool playing = Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, player, PickDict, PickClip, 3);
            float at = playing ? Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, player, PickDict, PickClip) : 1f;
            if (at >= DogCfg.GrabAt || (!playing && t > 300) || t > 2500) Take(player);
        }

        private void Take(Ped player)
        {
            PoopBag b = _picking;
            _picking = null;
            if (b == null || !b.Exists) return;
            _loose.Remove(b);
            if (_haveWeapon)
            {
                // into the inventory: one more poo bag in the wheel, and it's what you're holding
                b.Delete();
                GivePoobag(player);
                DogCfg.Trace("player picked up the " + (b.IsLump ? "poop" : "bag") + ": WEAPON_POOBAG x" + _lastPoobagAmmo);
                return;
            }
            b.Attach(player);
            _carried = b;
            _carryFrom = Game.GameTime;
            GiveSnowball(player);
            DogCfg.Trace("player picked up the " + (b.IsLump ? "poop" : "bag"));
        }

        // ---- the player carries and throws it: GTA's own snowball, brown -------------------------------------------------
        //
        // In hand, it's a snowball (WEAPON_SNOWBALL, one): GTA's aim, lock-on and throw. The snowball in the hand is
        // hidden and the bag shows there instead. The moment the game lets its snowball go, the snowball is taken out
        // (REMOVE_ALL_PROJECTILES_OF_TYPE, no white puff) and the bag flies on from where it was, at its speed, with
        // its physics (PoopBag.ThrowLikeSnowball), to burst brown on whatever it hits first.

        private const int SnowballWeapon = 0x787F0BB;    // WEAPON_SNOWBALL (mpchristmas2)
        private static readonly Model SnowballModel = new Model("w_ex_snowball");
        private bool _snowGiven, _hadSnow;
        private int _snowBefore, _carryFrom;

        private void GiveSnowball(Ped player)
        {
            _hadSnow = Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, player, SnowballWeapon, false);
            _snowBefore = _hadSnow ? Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, player, SnowballWeapon) : 0;
            if (!_hadSnow) Function.Call(Hash.GIVE_WEAPON_TO_PED, player, SnowballWeapon, 1, false, true);
            Function.Call(Hash.SET_PED_AMMO, player, SnowballWeapon, 1, false);
            Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, SnowballWeapon, true);
            _snowGiven = true;
        }

        // What the player had before: their own snowballs back, or none.
        private void PutSnowballBack(Ped player)
        {
            if (!_snowGiven) return;
            _snowGiven = false;
            bool has = Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON, player, SnowballWeapon, false);
            if (_hadSnow && _snowBefore > 0)
            {
                if (!has) Function.Call(Hash.GIVE_WEAPON_TO_PED, player, SnowballWeapon, _snowBefore, false, false);
                Function.Call(Hash.SET_PED_AMMO, player, SnowballWeapon, _snowBefore, false);
            }
            else if (has) Function.Call(Hash.REMOVE_WEAPON_FROM_PED, player, SnowballWeapon);
            if (Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, player) == SnowballWeapon && !(_hadSnow && _snowBefore > 0))
                Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, Unarmed, true);
        }

        private void DropCarried(Ped player)
        {
            if (_carried == null) return;
            if (_carried.Exists)
            {
                _carried.Drop(new Vector3(0f, 0f, 0.5f));
                _loose.Add(_carried);
            }
            _carried = null;
            _throwSub = 0;
            PutSnowballBack(player);
        }

        private void Carry(Ped player, int now, bool hidden)
        {
            if (!_carried.Exists) { _carried = null; _throwSub = 0; PutSnowballBack(player); return; }
            if (_throwSub != 0) { Throwing(player, now); return; }
            if (!OnFootAndFree(player)) { DropCarried(player); return; }

            // thrown: the snowball has left the hand
            int ammo = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, player, SnowballWeapon);
            if (_snowGiven && ammo <= 0 && now - _carryFrom > 300)
            {
                _throwSub = 1;
                _throwAt = now;
                _snowPrev = Vector3.Zero;
                Throwing(player, now);
                return;
            }
            int cur = Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, player);
            if (cur != SnowballWeapon)
            {
                if (now - _carryFrom < 1500) Function.Call(Hash.SET_CURRENT_PED_WEAPON, player, SnowballWeapon, true);
                else { DropCarried(player); return; }      // they took out something else: the bag goes down
            }
            // no weapon wheel or weapon swaps with a bag in hand
            foreach (int c in new[] { 37, 14, 15, 16, 17, 99, 100, 157, 158, 159, 160, 161, 162, 163, 164, 165 })
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, c, true);
            // the snowball in the hand stays hidden; the bag is what's in it
            int held = Function.Call<int>(Hash.GET_CURRENT_PED_WEAPON_ENTITY_INDEX, player, 0);
            if (held != 0) Function.Call(Hash.SET_ENTITY_VISIBLE, held, false, false);
            if (!hidden && !Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, Game.Player))
                DrawPrompt(_carried.Centre + new Vector3(0f, 0f, 0.35f), Attack, "wa_label_throw");
        }

        private Vector3 _snowPrev;
        private int _snowPrevAt;

        private void Throwing(Ped player, int now)
        {
            int t = now - _throwAt;
            Prop snow = null;
            float best = 15f;
            foreach (Prop p in World.GetNearbyProps(player.Position, 15f, SnowballModel))
            {
                if (p == null || !p.Exists()) continue;
                float d = p.Position.DistanceTo(player.Position);
                if (d < best) { best = d; snow = p; }
            }
            Vector3 from, v;
            if (snow != null)
            {
                from = snow.Position;
                v = snow.Velocity;
                if (v.Length() < 2f)
                {
                    // no speed on it yet: take it from two frames of its path
                    if (_snowPrev == Vector3.Zero || now == _snowPrevAt) { _snowPrev = from; _snowPrevAt = now; if (t < 400) return; }
                    else v = (from - _snowPrev) / Math.Max(0.001f, (now - _snowPrevAt) / 1000f);
                }
            }
            else
            {
                if (t < 350) return;              // a few frames for it to show up
                // never found: the snowball's own launch, along the camera
                from = _carried.Centre;
                v = GameplayCamera.Direction * 25f;
                DogCfg.Trace("no snowball found after the throw: thrown along the camera");
            }
            Function.Call(Hash.REMOVE_ALL_PROJECTILES_OF_TYPE, SnowballWeapon, false);   // no white puff
            _carried.ThrowLikeSnowball(player, from, v);
            _loose.Add(_carried);
            DogCfg.Trace("player threw the " + (_carried.IsLump ? "poop" : "bag") + " at " + v.Length().ToString("0.0") + " m/s");
            _carried = null;
            _throwSub = 0;
            PutSnowballBack(player);
        }

        // ---- the world prompt: button, then the word (HonorDogs.Draw, MemoryInteract.Draw) -----------------------

        private static void DrawPrompt(Vector3 at, int control, string label)
        {
            var sx = new OutputArgument();
            var sy = new OutputArgument();
            if (!Function.Call<bool>(Hash.GET_SCREEN_COORD_FROM_WORLD_COORD, at.X, at.Y, at.Z, sx, sy)) return;
            float x = sx.GetResult<float>() * Canvas.W;
            float y = sy.GetResult<float>() * Canvas.H;
            Art.Entry shared = Glyphs.ForControl(control);
            Art.Entry button = shared != null ? Art.Get("wa_" + System.IO.Path.GetFileNameWithoutExtension(shared.Path)) : null;
            Art.Entry word = Art.Get(label);
            float bh = button != null ? button.BoxH : 0f;
            float wh = word != null ? word.BoxH : 0f;
            float rowH = Math.Max(bh, wh);
            if (button != null)
            {
                Art.Draw(button, x, y + (rowH - bh) * 0.5f, Color.White);
                x += button.BoxW + ButtonToWord;
            }
            if (word != null) Art.Draw(word, x, y + (rowH - wh) * 0.5f, Color.White);
        }

        // ---- tidying -------------------------------------------------------------------------------------------------

        private void CleanUp(Ped player, int now)
        {
            Vector3 me = player.Position;
            for (int i = _walkers.Count - 1; i >= 0; i--)
            {
                Walker w = _walkers[i];
                if (w.Pooped && w.Dog != null && w.Dog.Exists()) _pooped.Add(w.Dog.Handle);
                bool far = w.Owner == null || !w.Owner.Exists() || w.Owner.Position.DistanceTo(me) > DogCfg.ReleaseRange;
                if (far && !w.InSequence)
                {
                    w.Release(this, false);
                    _walkers.RemoveAt(i);
                }
            }
            for (int i = _loose.Count - 1; i >= 0; i--)
            {
                PoopBag b = _loose[i];
                if (!b.Exists) { _loose.RemoveAt(i); continue; }
                if (b.Now != PoopBag.State.Lying) continue;
                if (b.Centre.DistanceTo(me) > DogCfg.DespawnRange || now - b.LyingSince > DogCfg.LooseBagMs)
                {
                    b.Delete();
                    _loose.RemoveAt(i);
                }
            }
            for (int i = _poops.Count - 1; i >= 0; i--)
            {
                var (prop, at) = _poops[i];
                if (prop == null || !prop.Exists()) { _poops.RemoveAt(i); continue; }
                if (prop.Position.DistanceTo(me) > DogCfg.DespawnRange || now - at > DogCfg.PoopStaysMs)
                {
                    prop.Delete();
                    _poops.RemoveAt(i);
                }
            }
            for (int i = _keep.Count - 1; i >= 0; i--)
            {
                Ped p = _keep[i];
                if (p == null || !p.Exists()) { _keep.RemoveAt(i); continue; }
                if (p.Position.DistanceTo(me) > DogCfg.ReleaseRange) { Walker.LetGo(p); _keep.RemoveAt(i); }
            }
            WheelAndGiveAll(player, now);
            var stale = new List<int>();
            foreach (var kv in _pairs) if (now - kv.Value.since > 10000) stale.Add(kv.Key);
            foreach (int k in stale) _pairs.Remove(k);
        }

        private void Shutdown()
        {
            if (_heldBag != null) _heldBag.Delete();
            _heldBag = null;
            foreach (Ride r in _rides) if (r.Bag != null) r.Bag.Delete();
            _rides.Clear();
            foreach (Walker w in _walkers) w.Release(this, true);
            _walkers.Clear();
            foreach (Ped p in _keep) Walker.LetGo(p);
            _keep.Clear();
            if (_carried != null) _carried.Delete();
            _carried = null;
            foreach (PoopBag b in _loose) b.Delete();
            _loose.Clear();
            foreach (var (prop, _) in _poops) if (prop != null && prop.Exists()) prop.Delete();
            _poops.Clear();
            foreach (var (fx, _) in _splats) PoopFx.Stop(fx);
            _splats.Clear();
        }
    }

    /// <summary>[DogWalkers] in ViExtras.ini.</summary>
    internal static class DogCfg
    {
        public const string Section = "DogWalkers";
        public static readonly Random Rng = new Random();

        public static bool MoreWalkers, Stumble, HitRagdoll, CopsGiveStar, Debug, BurstOnHit, BurstInHand, SmallDogsPoop, Toughen, SquatFx, HoldSwing;
        public static int BurstChunks, MaxLumps;
        public static int ToughHealth, PunchesBeforeHurt;
        public static float VomitChance, FightBackChance;
        public static int BurstLumps;
        public static Model LumpModel, LumpFallback;
        public static Vector3 LumpHoldOffset, LumpHoldRotation;
        public static int MaxWalkers, MaxSpawned, SpawnEveryMs, FromHour, ToHour;
        public static float SpawnChance, SpawnMin, SpawnMax, ScanRange, ReleaseRange, DespawnRange;
        public static int PoopMinMs, PoopMaxMs, SquatMs, PoopStaysMs, LooseBagMs, StumbleMs;
        public static float PoopRange, PickUpChance, PickUpReach, GrabAt, PickUpRange;
        public static float ThrowSpeed, TargetRange, MagnetRadius, FightChance;
        public static string DamagePack, BagModelName, CarrierModelName;
        public static Model BagModel, PoopModelBig, PoopModelSmall, CarrierModel;
        public static Model[] SpawnModels;
        public static Vector3 HoldOffset, HoldRotation;

        private static HashSet<int> _big, _dogs;

        public static void Load()
        {
            string S = Section;
            MoreWalkers = Config.Get(S, "MoreDogWalkers", true);
            MaxWalkers = Config.Get(S, "MaxWalkers", 6);
            MaxSpawned = Config.Get(S, "MaxSpawned", 2);
            SpawnEveryMs = Config.Get(S, "SpawnEveryMs", 20000);
            SpawnChance = Config.Get(S, "SpawnChance", 0.4f);
            SpawnMin = Config.Get(S, "SpawnMin", 30f);
            SpawnMax = Config.Get(S, "SpawnMax", 70f);
            FromHour = Config.Get(S, "FromHour", 6);
            ToHour = Config.Get(S, "ToHour", 21);
            ScanRange = Config.Get(S, "ScanRange", 60f);
            ReleaseRange = Config.Get(S, "ReleaseRange", 130f);
            DespawnRange = Config.Get(S, "DespawnRange", 90f);
            PoopMinMs = Config.Get(S, "PoopMinMs", 40000);
            PoopMaxMs = Config.Get(S, "PoopMaxMs", 120000);
            PoopRange = Config.Get(S, "PoopRange", 40f);
            SquatMs = Config.Get(S, "SquatMs", 4000);
            PoopStaysMs = Config.Get(S, "PoopStaysMs", 600000);
            PickUpChance = Config.Get(S, "PickUpChance", 0.8f);
            PickUpReach = Config.Get(S, "PickUpReach", 0.45f);
            GrabAt = Config.Get(S, "GrabAt", 0.42f);
            PickUpRange = Config.Get(S, "PlayerPickUpRange", 1.3f);
            LooseBagMs = Config.Get(S, "LooseBagMs", 300000);
            ThrowSpeed = Config.Get(S, "ThrowSpeed", 14f);
            TargetRange = Config.Get(S, "TargetRange", 25f);
            MagnetRadius = Config.Get(S, "HitRadius", 0.45f);
            Stumble = Config.Get(S, "Stumble", true);
            HitRagdoll = Config.Get(S, "HitRagdoll", false);
            StumbleMs = Config.Get(S, "StumbleMs", 800);
            FightChance = Config.Get(S, "FightChance", 0.4f);
            CopsGiveStar = Config.Get(S, "CopsGiveStar", true);
            BurstOnHit = Config.Get(S, "BurstOnHit", true);
            BurstInHand = Config.Get(S, "BurstInHand", false);
            PunchesBeforeHurt = Config.Get(S, "PunchesBeforeHurt", 3);
            Toughen = Config.Get(S, "TakeAPunch", true);
            ToughHealth = Math.Max(200, Config.Get(S, "TakeAPunchHealth", 400));
            BurstLumps = Config.Get(S, "BurstLumps", 3);
            FightBackChance = Config.Get(S, "FightBackChance", 0.9f);
            SmallDogsPoop = Config.Get(S, "SmallDogsPoop", false);
            SquatFx = Config.Get(S, "SquatFx", false);
            HoldSwing = Config.Get(S, "HoldSwing", true);
            BurstChunks = Config.Get(S, "BurstChunks", 4);
            MaxLumps = Config.Get(S, "MaxLumps", 24);
            LumpModel = new Model(Config.Get(S, "LumpModel", "vi_prop_poop"));
            LumpFallback = new Model("prop_big_shit_01");
            LumpHoldOffset = Vec(Config.Get(S, "LumpHoldOffset", "0.07, 0.02, -0.02"), new Vector3(0.07f, 0.02f, -0.02f));
            LumpHoldRotation = Vec(Config.Get(S, "LumpHoldRotation", "0, 0, 0"), Vector3.Zero);
            VomitChance = Config.Get(S, "VomitChance", 1f);
            DamagePack = Config.Get(S, "DamagePack", "SCR_Dumpster");
            Debug = Config.Get(S, "Debug", false);

            BagModelName = Config.Get(S, "BagModel", "vi_prop_poopbag");
            BagModel = new Model(BagModelName);
            CarrierModelName = Config.Get(S, "CarrierModel", "w_am_baseball");
            CarrierModel = new Model(CarrierModelName);
            PoopModelBig = new Model(Config.Get(S, "PoopModel", "prop_big_shit_02"));
            PoopModelSmall = new Model(Config.Get(S, "PoopModelSmallDog", "prop_big_shit_01"));
            HoldOffset = Vec(Config.Get(S, "HoldOffset", "0.075, 0.0, -0.003"), new Vector3(0.075f, 0f, -0.003f));
            HoldRotation = Vec(Config.Get(S, "HoldRotation", "0, -90, 0"), new Vector3(0f, -90f, 0f));

            string dogs = Config.Get(S, "SpawnDogs", "a_c_shepherd, a_c_retriever, a_c_husky, a_c_rottweiler");
            var list = new List<Model>();
            foreach (string n in dogs.Split(',')) if (n.Trim().Length > 0) list.Add(new Model(n.Trim()));
            SpawnModels = list.Count > 0 ? list.ToArray() : new[] { new Model("a_c_retriever") };

            _big = Hashes("a_c_chop", "a_c_rottweiler", "a_c_shepherd", "a_c_retriever", "a_c_husky");
            _dogs = Hashes("a_c_chop", "a_c_rottweiler", "a_c_shepherd", "a_c_retriever", "a_c_husky", "a_c_poodle", "a_c_pug", "a_c_westy");
            Log.Write("DogWalkers", "on: bag " + BagModelName + (BagModel.IsInCdImage ? "" : " NOT in the game (is the vihudanims pack installed?)"));
        }

        private static HashSet<int> Hashes(params string[] models)
        {
            var set = new HashSet<int>();
            foreach (string m in models) set.Add(Game.GenerateHash(m));
            return set;
        }

        public static bool IsDog(Ped p) => _dogs.Contains(p.Model.Hash);
        public static bool IsBig(Ped p) => _big.Contains(p.Model.Hash);

        private static Vector3 Vec(string s, Vector3 fallback)
        {
            string[] p = (s ?? "").Split(',');
            if (p.Length != 3) return fallback;
            if (float.TryParse(p[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(p[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(p[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                return new Vector3(x, y, z);
            return fallback;
        }

        public static void Trace(string text)
        {
            if (Debug) Log.Write("DogWalkers", text);
        }
    }
}
