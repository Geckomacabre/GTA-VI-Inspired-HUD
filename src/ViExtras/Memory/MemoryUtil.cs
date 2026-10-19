using System;
using System.Collections.Generic;
using System.Reflection;
using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;

namespace ViExtras.NpcMemory
{
    /// <summary>
    /// um_memory/client/util.lua: who a ped is, where, what you look like, sight,
    /// speech. Same natives as the Lua, through Function.Call, so they behave the
    /// same; single-player differences are marked.
    /// </summary>
    internal static class UM
    {
        public static readonly Random Rng = new Random();

        /// <summary>GetGameTimer(): stops while the game is paused, as the Lua timers did.</summary>
        public static int Now => Game.GameTime;

        public static Ped Player => Game.Player.Character;

        public static void Debug(string text)
        {
            if (MemCfg.Debug) Log.Write("Memory", text);
        }

        public static T Pick<T>(T[] list) => list == null || list.Length == 0 ? default(T) : list[Rng.Next(list.Length)];

        /// <summary>math.random(100): 1..100.</summary>
        public static int Roll() => Rng.Next(1, 101);

        public static uint ModelOf(Ped ped) => (uint)ped.Model.Hash;

        // ---- who is this ped? ---------------------------------------------------------

        // Gang and criminal ped types, which honor (and so this) treats as fair game.
        private static readonly HashSet<int> GangPedTypes = new HashSet<int> { 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 22 };

        // GTA6Hud runs its shop and its store hold-up on these (GTA6Hud ClerkModels).
        private static readonly HashSet<int> ClerkModels = new HashSet<int>
        {
            Game.GenerateHash("mp_m_shopkeep_01"), Game.GenerateHash("s_m_y_ammucity_01"), Game.GenerateHash("s_m_m_ammucountry"),
        };

        public static int PedType(Ped ped) => Function.Call<int>(Hash.GET_PED_TYPE, ped);

        public static void Temperament(Ped ped, out Temper temper, out PedClass cls)
        {
            temper = Temper.Average;
            cls = PedClass.Common;
            if (MemoryData.Temperaments.TryGetValue(ModelOf(ped), out KeyValuePair<Temper, PedClass> t))
            {
                temper = t.Key;
                cls = t.Value;
            }
            if (GangPedTypes.Contains(PedType(ped))) cls = PedClass.Gang;
            if (IsCop(ped)) cls = PedClass.Law;
        }

        public static Temper TemperOf(Ped ped)
        {
            Temperament(ped, out Temper t, out _);
            return t;
        }

        public static bool IsCop(Ped ped)
        {
            int type = PedType(ped);
            if (type == 6 || type == 27) return true;
            return MemoryData.CopModels.Contains(ModelOf(ped));
        }

        public static bool IsGang(Ped ped)
        {
            Temperament(ped, out _, out PedClass cls);
            return cls == PedClass.Gang;
        }

        public static bool Alive(Ped ped) =>
            ped != null && ped.Exists() && !Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, ped, true);

        /// <summary>
        /// A living, non-player human this may deal with. Single player also
        /// leaves out the other protagonists, GTA6Hud's shop clerks, and anyone
        /// a script owns (a mission entity: story and stranger characters,
        /// GTA6Hud's patrols and robbery crowds), so nothing here re-tasks them.
        /// </summary>
        public static bool IsPerson(Ped ped)
        {
            if (!Alive(ped) || ped == Player || ped.IsPlayer) return false;
            if (!Function.Call<bool>(Hash.IS_PED_HUMAN, ped)) return false;
            int type = PedType(ped);
            if (type == 0 || type == 1 || type == 2 || type == 3) return false;
            if (ClerkModels.Contains(ped.Model.Hash)) return false;
            if (Function.Call<bool>(Hash.IS_ENTITY_A_MISSION_ENTITY, ped)) return false;
            return true;
        }

        // ---- where, when, what you look like -----------------------------------------

        public static string ZoneCode(Vector3 c) => Function.Call<string>(Hash.GET_NAME_OF_ZONE, c.X, c.Y, c.Z) ?? "";

        /// <summary>The zone's code, display name and fenix-police region.</summary>
        public static string ZoneAt(Vector3 c, out string name, out string region)
        {
            string code = ZoneCode(c);
            if (MemoryData.Zones.TryGetValue(code, out KeyValuePair<string, string> z))
            {
                name = z.Key;
                region = z.Value;
            }
            else
            {
                name = code;
                region = "losSantos";
            }
            return code;
        }

        public static bool IsNight()
        {
            int h = Function.Call<int>(Hash.GET_CLOCK_HOURS);
            return h >= 22 || h < 5;
        }

        public static bool IsFemale(Ped ped) => !Function.Call<bool>(Hash.IS_PED_MALE, ped);

        /// <summary>
        /// Anything in the mask slot (component 1). um_memory only looked there
        /// on the freemode peds; the story characters keep their masks in the
        /// same slot (GTA6Hud's own face test reads it the same way), so here it
        /// is any model.
        /// </summary>
        public static bool IsMasked(Ped ped) => Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped, 1) > 0;

        // GET_PED_HAIR_COLOR (0xE3DD5F2A84B42281), not named in SHVDN's list.
        private const Hash GetPedHairColor = (Hash)0xE3DD5F2A84B42281UL;

        /// <summary>
        /// GTA keeps no persistent NPCs, so a "person" is how they look plus
        /// where you met them: model, face, hair, top and legs (drawable and
        /// texture each), hair colour, zone.
        /// </summary>
        public static string Fingerprint(Ped ped, string zoneCode)
        {
            int D(int c) => Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, ped, c);
            int T(int c) => Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, ped, c);
            return ModelOf(ped).ToString("x") + ":" + D(0) + "." + T(0) + ":" + D(2) + "." + T(2) + ":" + D(3) + "." + T(3)
                + ":" + D(4) + "." + T(4) + ":" + Function.Call<int>(GetPedHairColor, ped) + "@" + zoneCode;
        }

        // ---- sight -----------------------------------------------------------------------

        /// <summary>Can `ped` see `target` from where it is facing? The expensive one.</summary>
        public static bool CanSee(Ped ped, Entity target) => Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY_IN_FRONT, ped, target);

        public static bool HasLos(Entity a, Entity b) => Function.Call<bool>(Hash.HAS_ENTITY_CLEAR_LOS_TO_ENTITY, a, b, 17);

        /// <summary>Degrees between the camera's heading and the direction to `point`, on the ground.</summary>
        public static float AngleFromCamera(Vector3 origin, Vector3 point)
        {
            Vector3 rot = GameplayCamera.Rotation;
            double rx = rot.X * Math.PI / 180.0, rz = rot.Z * Math.PI / 180.0;
            double c = Math.Abs(Math.Cos(rx));
            double fx = -Math.Sin(rz) * c, fy = Math.Cos(rz) * c;
            double dx = point.X - origin.X, dy = point.Y - origin.Y;
            double len = Math.Sqrt(dx * dx + dy * dy), flen = Math.Sqrt(fx * fx + fy * fy);
            if (len < 0.001 || flen < 0.001) return 0f;
            double dot = (dx * fx + dy * fy) / (len * flen);
            return (float)(Math.Acos(Math.Max(-1.0, Math.Min(1.0, dot))) * 180.0 / Math.PI);
        }

        internal struct Near
        {
            public Ped Ped;
            public float Dist;
        }

        /// <summary>People within `radius` of `origin`, nearest first.</summary>
        public static List<Near> NearbyPeople(Vector3 origin, float radius, Func<Ped, float, bool> filter = null)
        {
            var list = new List<Near>();
            foreach (Ped ped in World.GetNearbyPeds(origin, radius))
            {
                if (!IsPerson(ped)) continue;
                float d = ped.Position.DistanceTo(origin);
                if (d <= radius && (filter == null || filter(ped, d))) list.Add(new Near { Ped = ped, Dist = d });
            }
            list.Sort((a, b) => a.Dist.CompareTo(b.Dist));
            return list;
        }

        public static float FlatDistance(Vector3 a, Vector3 b)
        {
            float dx = a.X - b.X, dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // ---- speech and tasks ----------------------------------------------------------------

        /// <summary>NPC speech on top of their animation; silent unless NpcSpeech.</summary>
        public static void Say(Ped ped, string[] lines, string param = "SPEECH_PARAMS_FORCE")
        {
            if (!MemCfg.NpcSpeech || ped == null || !ped.Exists()) return;
            Function.Call(Hash.PLAY_PED_AMBIENT_SPEECH_NATIVE, ped, Pick(lines), param);
        }

        public static bool Fleeing(Ped ped) => Function.Call<bool>(Hash.IS_PED_FLEEING, ped);
        public static bool InCombatWith(Ped ped, Ped target) => Function.Call<bool>(Hash.IS_PED_IN_COMBAT, ped, target);
        public static bool InScenario(Ped ped) => Function.Call<bool>(Hash.IS_PED_USING_ANY_SCENARIO, ped);
        public static bool InVehicle(Ped ped) => Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, ped, false);
        public static bool Armed(Ped ped, int flags) => Function.Call<bool>(Hash.IS_PED_ARMED, ped, flags);

        public static void TurnTo(Ped ped, Entity target, int ms) => Function.Call(Hash.TASK_TURN_PED_TO_FACE_ENTITY, ped, target, ms);
        public static void LookAt(Ped ped, Entity target, int ms) => Function.Call(Hash.TASK_LOOK_AT_ENTITY, ped, target, ms, 0, 2);
        public static void Flee(Ped ped, Ped from, float dist, int ms) => Function.Call(Hash.TASK_SMART_FLEE_PED, ped, from, dist, ms, false, false);
        public static void ClearTasks(Ped ped) => Function.Call(Hash.CLEAR_PED_TASKS, ped);
        public static void Combat(Ped ped, Ped target) => Function.Call(Hash.TASK_COMBAT_PED, ped, target, 0, 16);

        public static int Wanted => Game.Player.Wanted.WantedLevel;
    }

    /// <summary>SetTimeout / UM.later: run `fn` after `ms` only if `ped` still exists and is alive.</summary>
    internal static class Later
    {
        private sealed class Item
        {
            public int Due;
            public Ped Ped;
            public Action Fn;
        }

        private static readonly List<Item> Items = new List<Item>();

        public static void Run(Ped ped, int ms, Action fn) => Items.Add(new Item { Due = UM.Now + ms, Ped = ped, Fn = fn });

        public static void Tick()
        {
            if (Items.Count == 0) return;
            int now = UM.Now;
            var due = new List<Item>();
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                if (now < Items[i].Due) continue;
                due.Add(Items[i]);
                Items.RemoveAt(i);
            }
            for (int i = due.Count - 1; i >= 0; i--)
            {
                Item it = due[i];
                if (!UM.Alive(it.Ped)) continue;
                try { it.Fn(); }
                catch (Exception ex) { Log.Write("Memory", "later: " + ex.Message); }
            }
        }

        public static void Clear() => Items.Clear();
    }

    /// <summary>
    /// Where um_memory spoke to the player: vice_hud's feed, qbx_core's notify.
    /// Neither exists here, so this is the Lua's own last resort
    /// (Integr.notify): the game's feed, with vice_hud's {braces} taken out.
    /// GTA6Hud posts its own notices to the same feed.
    /// </summary>
    internal static class MemoryUi
    {
        public static void Notify(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            ViExtras.Feed.Post(text.Replace("{", "").Replace("}", "").Replace("**", ""));
        }

        public static void Feed(string text) => Notify(text);
    }

    /// <summary>
    /// What this needs to know about the other scripts, read by reflection so the
    /// build needs none of them: GTA6Hud's [Interactions] NpcConversations, and
    /// whether the binoculars or the crash cam (this assembly) have the screen.
    /// </summary>
    internal static class OtherScripts
    {
        private static bool _looked;
        private static FieldInfo _npcConversations;
        private static PropertyInfo _binoculars, _crashCam;

        private static void Look()
        {
            if (_looked) return;
            _looked = true;
            const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Type r = asm.GetType("GTA6Hud.Ref", false);
                    if (r != null) _npcConversations = r.GetField("NpcConversationsEnabled", Static);
                }
                catch { }
            }
            try
            {
                Assembly me = typeof(OtherScripts).Assembly;
                _binoculars = me.GetType("ViExtras.Binoculars", false)?.GetProperty("IsActive", Static);
                _crashCam = me.GetType("ViExtras.CrashCam", false)?.GetProperty("IsPlaying", Static);
            }
            catch { }
        }

        /// <summary>GTA6Hud.ini [Interactions] NpcConversations, as GTA6Hud read it.</summary>
        public static bool NpcConversations
        {
            get
            {
                Look();
                try { return _npcConversations != null && (bool)_npcConversations.GetValue(null); }
                catch { return false; }
            }
        }

        public static bool ScreenTaken
        {
            get
            {
                Look();
                try
                {
                    return (_binoculars != null && (bool)_binoculars.GetValue(null))
                        || (_crashCam != null && (bool)_crashCam.GetValue(null));
                }
                catch { return false; }
            }
        }
    }
}
