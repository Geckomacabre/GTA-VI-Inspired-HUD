using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Vangelico, open for GTA6Hud's jewel store robbery (Hud\Robbery\sites.txt, SITE JEWEL).
    ///
    /// The story keeps the store as one of its building states (flow_controller, building 88), three versions
    /// of it, one loaded at a time:
    ///   jewel2fake          the shop front with no way in: before the Jewel Store Job, and after a failed one
    ///   post_hiest_unload   the store itself, glass cases and all: only during the Jewel Store Job
    ///   bh1_16_refurb       boarded up, "closed for refurbishment": after it
    /// and it locks the two front doors through the door system. So outside that one mission there was no
    /// store to rob, and GTA6Hud's robbery could never start.
    ///
    /// Within LoadRange of the store, outside story missions and the jewel heist's own scripts, this loads the
    /// store, takes the other two versions away, lets the interior run and unlocks the doors. Past UnloadRange,
    /// or once a mission is running and you're out of the store, everything goes back exactly as it was, so the
    /// story's own state is never changed. The cases come back with the store; GTA6Hud's own cooldown keeps it
    /// from being robbed again straight away. [Vangelico] Enabled = False in ViExtras.ini leaves it closed.
    ///
    /// While it's open, the store has the jewel heist's own shop floor: its manageress and four customers, where
    /// the heist has them before the crew walks in (jewelry_heist.c). Its store guard is GTA6Hud's first guard
    /// there (sites.txt).
    /// </summary>
    public sealed class Vangelico : Script
    {
        private static readonly Vector3 Front = new Vector3(-630.42f, -236.78f, 37.06f);   // flow_controller's spot for it
        private static readonly Vector3 Inside = new Vector3(-622.22f, -230.62f, 38.22f);  // sites.txt: the interior
        private const float LoadRange = 150f, UnloadRange = 220f;
        private const string Store = "post_hiest_unload";
        private static readonly string[] Others = { "jewel2fake", "bh1_16_refurb" };
        private static readonly string[] HeistScripts = { "jewelry_heist", "jewelry_setup1", "jewelry_prep1a", "jewelry_prep1b", "jewelry_prep2a" };

        private sealed class Door
        {
            public int Hash; public string Model; public Vector3 At; public int Was = -1;
        }
        // flow_controller's door-system entries 141 and 142
        private static readonly Door[] Doors =
        {
            new Door { Hash = 1874948872, Model = "p_jewel_door_l", At = new Vector3(-631.96f, -236.33f, 38.21f) },
            new Door { Hash = -1965020851, Model = "p_jewel_door_r1", At = new Vector3(-630.43f, -238.44f, 38.21f) },
        };

        private const ulong DOOR_SET_STATE = 0x6BAB9442830C7F53;   // DOOR_SYSTEM_SET_DOOR_STATE
        private const ulong DOOR_GET_STATE = 0x160AA1B32F6139B8;   // DOOR_SYSTEM_GET_DOOR_STATE
        private const ulong DOOR_REGISTERED = 0xC153C43EA202C8C1;  // IS_DOOR_REGISTERED_WITH_SYSTEM
        private const ulong DOOR_ADD = 0x6F8838D03D1DC226;         // ADD_DOOR_TO_SYSTEM
        private const ulong INTERIOR_DISABLED = 0xBC5115A5A939DD15, DISABLE_INTERIOR = 0x6170941419D7D8EC;
        private const ulong INTERIOR_CAPPED = 0x92BAC8ACF88CEC26, CAP_INTERIOR = 0xD9175F941610DB54;
        private const ulong THREADS = 0x2C83A9DA6BFFC4F9;          // GET_NUMBER_OF_THREADS_RUNNING_THE_SCRIPT_WITH_THIS_HASH

        private sealed class Person
        {
            public string Model, Scenario; public Vector3 At; public float Heading; public Ped Ped;
        }
        // jewelry_heist.c's shop floor (its Local_1750/fLocal_1766, models f_4[0..4], the stealth version's): all
        // face a case. The manageress and the customer beside her talk; the other three look at the cases.
        private static readonly Person[] People =
        {
            new Person { Model = "a_f_y_business_02", At = new Vector3(-622.0082f, -230.5164f, 37.208f), Heading = 57.44f, Scenario = "WORLD_HUMAN_WINDOW_SHOP_BROWSE" },
            new Person { Model = "ig_jewelass", At = new Vector3(-625.4323f, -235.8507f, 37.208f), Heading = 41.51f, Scenario = "WORLD_HUMAN_HANG_OUT_STREET" },
            new Person { Model = "a_m_y_bevhills_01", At = new Vector3(-626.0464f, -236.6019f, 37.208f), Heading = 18.80f, Scenario = "WORLD_HUMAN_HANG_OUT_STREET" },
            new Person { Model = "a_m_y_bevhills_01", At = new Vector3(-623.9794f, -232.241f, 37.208f), Heading = 6.11f, Scenario = "WORLD_HUMAN_WINDOW_SHOP_BROWSE" },
            new Person { Model = "a_f_y_business_02", At = new Vector3(-623.5298f, -234.9338f, 37.208f), Heading = 337.51f, Scenario = "WORLD_HUMAN_WINDOW_SHOP_BROWSE" },
        };
        private const float StaffRange = 90f;    // GTA6Hud puts its guards in from the same distance
        private const float HandOverRange = 30f, AgainRange = 60f;

        private readonly int[] _heistHashes = new int[HeistScripts.Length];
        private bool _staffIn, _staffHandedOver;
        private bool _relockSeen, _doorErrorSeen;   // logged once an opening
        private bool _open, _storeWas, _disabledWas, _cappedWas;
        private readonly bool[] _othersWere = new bool[Others.Length];
        private int _interior;
        private long _lookAt;

        public Vangelico()
        {
            Paths.Init(BaseDirectory);
            if (!Config.Get("Vangelico", "Enabled", true)) return;
            for (int i = 0; i < HeistScripts.Length; i++) _heistHashes[i] = Game.GenerateHash(HeistScripts[i]);
            Tick += OnTick;
            Aborted += (s, e) => { if (_open) Close("scripts stopped"); };
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_open) HoldDoors();
            long now = RealTime.Ms;
            if (now < _lookAt) return;
            _lookAt = now + 1000;   // a look a second will do: the ranges are wide
            try
            {
                if (!GameReady.Now) return;
                Ped player = Game.Player.Character;
                if (player == null || !player.Exists()) return;
                float d = player.Position.DistanceTo(Front);
                bool story = StoryBusy();
                if (!_open)
                {
                    if (d < LoadRange && !story) OpenUp();
                }
                else if (d > UnloadRange || (story && !InStore(player)))
                {
                    Close(story ? "a story mission" : "you left");
                }
                else
                {
                    Keep();
                    Staff(d, story);
                }
            }
            catch (Exception ex) { Log.Write("Vangelico", ex.Message); }
        }

        // A story mission, or one of the jewel heist's scripts (they run the store themselves).
        private bool StoryBusy()
        {
            if (Game.IsMissionActive) return true;
            foreach (int h in _heistHashes)
                if (Function.Call<int>((Hash)THREADS, h) > 0) return true;
            return false;
        }

        private bool InStore(Ped player) =>
            _interior != 0 && Function.Call<int>(Hash.GET_INTERIOR_FROM_ENTITY, player) == _interior;

        private static bool Active(string ipl) => Function.Call<bool>(Hash.IS_IPL_ACTIVE, ipl);

        private void OpenUp()
        {
            _storeWas = Active(Store);
            for (int i = 0; i < Others.Length; i++) _othersWere[i] = Active(Others[i]);
            foreach (string o in Others) Function.Call(Hash.REMOVE_IPL, o);
            Function.Call(Hash.REQUEST_IPL, Store);

            _interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, Inside.X, Inside.Y, Inside.Z);
            if (_interior != 0)
            {
                _disabledWas = Function.Call<bool>((Hash)INTERIOR_DISABLED, _interior);
                _cappedWas = Function.Call<bool>((Hash)INTERIOR_CAPPED, _interior);
                if (_disabledWas) Function.Call((Hash)DISABLE_INTERIOR, _interior, false);
                if (_cappedWas) Function.Call((Hash)CAP_INTERIOR, _interior, false);
            }

            foreach (Door door in Doors)
            {
                if (!Function.Call<bool>((Hash)DOOR_REGISTERED, door.Hash))
                    Function.Call((Hash)DOOR_ADD, door.Hash, Game.GenerateHash(door.Model), door.At.X, door.At.Y, door.At.Z, false, false, false);
                door.Was = Function.Call<int>((Hash)DOOR_GET_STATE, door.Hash);
                Function.Call((Hash)DOOR_SET_STATE, door.Hash, 0, false, true);   // 0: unlocked
            }
            _relockSeen = _doorErrorSeen = false;
            _open = true;
            Log.Write("Vangelico", "open: the store in (was " + (_storeWas ? "in" : "out") + "), "
                + string.Join(", ", Array.ConvertAll(Others, o => o + " was " + (_othersWere[Array.IndexOf(Others, o)] ? "in" : "out")))
                + ", interior " + (_interior != 0 ? _interior + (_disabledWas ? " (was disabled)" : "") + (_cappedWas ? " (was capped)" : "") : "not found yet")
                + ", doors " + string.Join("/", Array.ConvertAll(Doors, x => x.Was.ToString())));
        }

        // Every frame while open. The story's door controller (building_controller) looks at one of its ~200
        // doors a frame, so it's back at these two every few seconds (6.5 s at 30 fps), and locks them: always
        // once the Jewel Store Job is done (the store is "closed for refurbishment"), and before it from 21:00
        // to 7:00 when you're 18 m off. Unlocked again the same frame, so they never hold you out, or in.
        private void HoldDoors()
        {
            try
            {
                foreach (Door door in Doors)
                {
                    if (Function.Call<int>((Hash)DOOR_GET_STATE, door.Hash) == 0) continue;
                    Function.Call((Hash)DOOR_SET_STATE, door.Hash, 0, false, true);
                    if (!_relockSeen)
                    {
                        _relockSeen = true;
                        Log.Write("Vangelico", "the story locked the doors again (it keeps doing that after the Jewel Store Job, and at night); unlocked each time");
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_doorErrorSeen) { _doorErrorSeen = true; Log.Write("Vangelico", "doors: " + ex.Message); }
            }
        }

        // While open: the story's building controller could put its own version back; the store wins, and
        // an interior that wasn't there at the first look gets let run once it is.
        private void Keep()
        {
            string putBack = null;
            foreach (string o in Others)
                if (Active(o)) { Function.Call(Hash.REMOVE_IPL, o); putBack = o; }
            if (!Active(Store)) { Function.Call(Hash.REQUEST_IPL, Store); putBack = (putBack == null ? "" : putBack + ", ") + "no " + Store; }
            if (_interior == 0)
            {
                _interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, Inside.X, Inside.Y, Inside.Z);
                if (_interior != 0)
                {
                    _disabledWas = Function.Call<bool>((Hash)INTERIOR_DISABLED, _interior);
                    _cappedWas = Function.Call<bool>((Hash)INTERIOR_CAPPED, _interior);
                    if (_disabledWas) Function.Call((Hash)DISABLE_INTERIOR, _interior, false);
                    if (_cappedWas) Function.Call((Hash)CAP_INTERIOR, _interior, false);
                    Log.Write("Vangelico", "interior " + _interior + (_disabledWas ? " (was disabled)" : "") + (_cappedWas ? " (was capped)" : ""));
                }
            }
            if (putBack != null) Log.Write("Vangelico", "something put the closed store back (" + putBack + "); opened again");
        }

        // The shop floor goes in once the interior is ready and you're within StaffRange. Its people stay this
        // script's until you're within HandOverRange, then go to the game, keeping their scenario: GTA6Hud's
        // robbery only takes peds that aren't any script's as its witnesses (they cower, and can call it in).
        // Once handed over the game clears them when you're far, and if they're all gone when you come back
        // (past AgainRange), the store gets a new shop floor.
        private void Staff(float d, bool story)
        {
            if (!_staffIn)
            {
                if (story || d > StaffRange || _interior == 0 || !Function.Call<bool>(Hash.IS_INTERIOR_READY, _interior)) return;
                bool loaded = true;
                foreach (Person p in People)
                {
                    var m = new Model(p.Model);
                    if (!m.IsLoaded) { m.Request(); loaded = false; }
                }
                if (!loaded) return;
                int made = 0;
                foreach (Person p in People)
                {
                    var m = new Model(p.Model);
                    int h = Function.Call<int>(Hash.CREATE_PED, 26, m.Hash, p.At.X, p.At.Y, p.At.Z, p.Heading, false, false);
                    p.Ped = Entity.FromHandle(h) as Ped;
                    if (p.Ped == null) continue;
                    if (p.Model.StartsWith("a_")) Function.Call(Hash.SET_PED_RANDOM_COMPONENT_VARIATION, p.Ped, 0);
                    Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, p.Ped, p.Scenario, 0, true);
                    made++;
                }
                foreach (Person p in People) new Model(p.Model).MarkAsNoLongerNeeded();
                _staffIn = true;
                _staffHandedOver = false;
                Log.Write("Vangelico", "shop floor in: " + made + " of " + People.Length + " (" + d.ToString("0") + " m out)");
            }
            else if (!_staffHandedOver && d < HandOverRange)
            {
                foreach (Person p in People)
                {
                    if (p.Ped == null || !p.Ped.Exists()) continue;
                    Function.Call(Hash.SET_PED_KEEP_TASK, p.Ped, true);
                    p.Ped.MarkAsNoLongerNeeded();
                }
                _staffHandedOver = true;
                Log.Write("Vangelico", "shop floor handed to the game");
            }
            else if (_staffHandedOver && d > AgainRange && Array.TrueForAll(People, p => p.Ped == null || !p.Ped.Exists()))
            {
                _staffIn = false;
                Log.Write("Vangelico", "shop floor cleared by the game; a new one when you're back");
            }
        }

        // People still this script's go; handed-over ones are the game's (or the robbery's) now.
        private void ClearStaff()
        {
            foreach (Person p in People)
            {
                if (!_staffHandedOver && p.Ped != null && p.Ped.Exists()) p.Ped.Delete();
                p.Ped = null;
            }
            _staffIn = _staffHandedOver = false;
        }

        private void Close(string why)
        {
            _open = false;
            ClearStaff();
            foreach (Door door in Doors)
                if (door.Was >= 0) Function.Call((Hash)DOOR_SET_STATE, door.Hash, door.Was, false, true);
            if (_interior != 0)
            {
                if (_cappedWas) Function.Call((Hash)CAP_INTERIOR, _interior, true);
                if (_disabledWas) Function.Call((Hash)DISABLE_INTERIOR, _interior, true);
            }
            if (!_storeWas) Function.Call(Hash.REMOVE_IPL, Store);
            for (int i = 0; i < Others.Length; i++)
                if (_othersWere[i]) Function.Call(Hash.REQUEST_IPL, Others[i]);
            _interior = 0;
            Log.Write("Vangelico", "closed again (" + why + ")");
        }
    }
}
