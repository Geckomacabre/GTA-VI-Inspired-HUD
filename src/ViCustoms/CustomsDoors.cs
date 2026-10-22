using System;
using System.Reflection;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The Los Santos Customs garage doors, run the way GTA runs them: each is in the game's door system, unlocked
    /// with an automatic distance of 25 m, so the game rolls it up as you come close and lets it down once you've
    /// gone (building_controller's door table, cases 54-60). Wanted, it stays shut, as GTA keeps the shop shut then.
    ///
    /// GTA6Hud ends GTA's carmod_shop to keep its own shop in charge, and opens the doors itself instead: it forces
    /// them open every frame near the garage and, as a garage door doesn't swing open that way, hides the door a
    /// moment later, so it looks always open (the user: "the garage door doesnt open when i get close to it, its just
    /// always open"). Its door list (CustomsDoors) is pointed far below the map here so it leaves them alone, and any
    /// door it had hidden is shown again. [Customs] GarageDoors = False leaves the doors to GTA6Hud.
    /// </summary>
    public sealed class CustomsDoors : Script
    {
        private struct Door
        {
            public string Model;
            public Vector3 Pos;
            public int Hash;   // the door system's hash for it, as building_controller registers it
        }

        private static readonly Door[] Doors =
        {
            new Door { Model = "prop_com_ls_door_01", Pos = new Vector3(-1145.9f, -1991.14f, 14.18f), Hash = -1871080926 },
            new Door { Model = "prop_id2_11_gdoor", Pos = new Vector3(723.12f, -1088.83f, 23.28f), Hash = 1168079979 },
            new Door { Model = "prop_com_ls_door_01", Pos = new Vector3(-356.09f, -134.77f, 40.01f), Hash = 1206354175 },
            new Door { Model = "v_ilev_carmod3door", Pos = new Vector3(108.8502f, 6617.876f, 32.673f), Hash = -1038180727 },
            new Door { Model = "v_ilev_carmod3door", Pos = new Vector3(114.3206f, 6623.226f, 32.7161f), Hash = 1200466273 },
            new Door { Model = "v_ilev_carmod3door", Pos = new Vector3(1182.305f, 2645.242f, 38.807f), Hash = 1391004277 },
            new Door { Model = "v_ilev_carmod3door", Pos = new Vector3(1174.654f, 2645.242f, 38.6826f), Hash = -459199009 },
        };

        private const float Automatic = 25f;   // building_controller's automatic distance for these doors
        private const float Near = 90f;        // looked after from this close
        private const int Unlocked = 0, Locked = 1;

        private readonly bool _enabled;
        private readonly int[] _state = new int[Doors.Length];
        private bool _hudDone;
        private long _next;

        public CustomsDoors()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Customs", "GarageDoors", true);
            for (int i = 0; i < _state.Length; i++) _state[i] = -1;
            if (_enabled) Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            long now = RealTime.Ms;
            if (now < _next) return;
            _next = now + 250;
            try
            {
                if (!_hudDone) KeepGta6HudOff();
                Ped me = Game.Player.Character;
                if (me == null || !me.Exists()) return;
                Vector3 p = me.Position;
                int want = Game.Player.Wanted.WantedLevel > 0 ? Locked : Unlocked;
                for (int i = 0; i < Doors.Length; i++)
                {
                    Door d = Doors[i];
                    if (Vector3.Distance2D(p, d.Pos) > Near)
                    {
                        _state[i] = -1;   // set again on the way back: the game may have changed it meanwhile
                        continue;
                    }
                    if (!Function.Call<bool>(Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, d.Hash))
                        Function.Call(Hash.ADD_DOOR_TO_SYSTEM, d.Hash, Game.GenerateHash(d.Model), d.Pos.X, d.Pos.Y, d.Pos.Z, false, false, false);
                    if (_state[i] == -1) Show(d);
                    if (_state[i] == want && Function.Call<int>(Hash.DOOR_SYSTEM_GET_DOOR_STATE, d.Hash) == want) continue;
                    Function.Call(Hash.DOOR_SYSTEM_SET_AUTOMATIC_DISTANCE, d.Hash, Automatic, false, false);
                    Function.Call(Hash.DOOR_SYSTEM_SET_DOOR_STATE, d.Hash, want, false, true);
                    if (_state[i] != want) Log.Write("CustomsDoors", (want == Unlocked ? "unlocked " : "locked ") + d.Model + " at " + (int)d.Pos.X + ", " + (int)d.Pos.Y);
                    _state[i] = want;
                }
            }
            catch (Exception ex) { Log.Write("CustomsDoors", ex.Message); _next = now + 5000; }
        }

        // A door GTA6Hud hid (no collision, not drawn) is put back.
        private static void Show(Door d)
        {
            int obj = Function.Call<int>(Hash.GET_CLOSEST_OBJECT_OF_TYPE, d.Pos.X, d.Pos.Y, d.Pos.Z, 2.5f, Game.GenerateHash(d.Model), false, false, false);
            if (obj == 0) return;
            Function.Call(Hash.SET_ENTITY_COLLISION, obj, true, true);
            Function.Call(Hash.SET_ENTITY_VISIBLE, obj, true, false);
        }

        // GTA6Hud's door list, each entry moved far below the map: its door code finds none near a garage and does
        // nothing. The list itself is changed, not swapped for another, so code it has already compiled sees it too.
        private void KeepGta6HudOff()
        {
            object hud = HudBridge.Gta6Hud();
            if (hud == null) return;   // looked for again every few seconds until it has started
            _hudDone = true;
            FieldInfo f = hud.GetType().GetField("CustomsDoors", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (!(f?.GetValue(null) is Array list) || list.Length == 0)
            {
                Log.Write("CustomsDoors", "GTA6Hud's door list was not found; it may still hide the doors");
                return;
            }
            Type entry = list.GetType().GetElementType();
            int moved = 0;
            for (int i = 0; i < list.Length; i++)
            {
                try
                {
                    object far = Activator.CreateInstance(entry, "vihud_no_door", new Vector3(0f, 0f, -5000f));
                    list.SetValue(far, i);
                    moved++;
                }
                catch (Exception ex) { Log.Write("CustomsDoors", "GTA6Hud's door " + i + ": " + ex.Message); }
            }
            Log.Write("CustomsDoors", "GTA6Hud leaves the garage doors alone (" + moved + " of " + list.Length + ")");
        }
    }
}
