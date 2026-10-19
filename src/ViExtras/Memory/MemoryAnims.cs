using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras.NpcMemory
{
    /// <summary>
    /// um_memory/client/anims.lua: everyone talks with their body. The player's
    /// Greet / Antagonize / Defuse and every NPC reaction are animations from
    /// MemCfg.AnimLibrary (solo) and MemCfg.PairedAnims (two-person).
    ///
    /// The Lua waited for a dictionary inside its own timer thread; here a play
    /// whose dictionary is still streaming waits in a queue (up to the same
    /// 1.5 s) so nothing else stalls. The common dictionaries are requested when
    /// the script starts, so in practice everything plays at once.
    /// </summary>
    internal static class Anims
    {
        private const string GestureM = "gestures@m@standing@casual", GestureF = "gestures@f@standing@casual";
        private const string CelebM = "anim@mp_player_intcelebrationmale@", CelebF = "anim@mp_player_intcelebrationfemale@";

        // Animation flags: 48 = upper body + secondary task (keeps walking), 0 = full body.
        private const int Upper = 48, Full = 0;

        private sealed class Pending
        {
            public Ped Ped;
            public string Dict, Clip;
            public float BlendIn, BlendOut;
            public int Ms, Flag, Deadline;
        }

        private static readonly List<Pending> Queue = new List<Pending>();

        public static bool Loaded(string dict) => Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict);
        public static void Request(string dict) => Function.Call(Hash.REQUEST_ANIM_DICT, dict);

        /// <summary>Ask for the dictionaries most exchanges use, so the first ones play straight away.</summary>
        public static void Preload()
        {
            Request(GestureM);
            Request(GestureF);
            Request("reaction@shove");
            Request("mp_ped_interaction");
            foreach (string g in new[] { "m_m", "f_m", "f_f" }) Request(string.Format(MemCfg.PairedAnims["fistbump"].Dict, g));
        }

        private static bool Resolve(Ped ped, string name, out string dict, out string clip, out int flag, out int ms)
        {
            dict = clip = null;
            flag = Upper;
            ms = 0;
            if (name == null || !MemCfg.AnimLibrary.TryGetValue(name, out MemCfg.Anim a)) return false;
            bool female = UM.IsFemale(ped);

            if (a.Gesture != null)
            {
                dict = female ? GestureF : GestureM;
                clip = a.Gesture;
                ms = a.Ms > 0 ? a.Ms : 2000;
                return true;
            }
            if (a.Celebration != null)
            {
                dict = (female && !a.MaleOnly ? CelebF : CelebM) + a.Celebration;
                clip = a.Celebration;
                flag = a.Full ? Full : Upper;
                ms = a.Ms > 0 ? a.Ms : 2500;
                return true;
            }
            dict = (female && a.FemaleDict != null) ? a.FemaleDict : a.Dict;
            clip = UM.Pick(a.Clips);
            flag = a.Full ? Full : Upper;
            ms = a.Ms > 0 ? a.Ms : 2000;
            return dict != null && clip != null;
        }

        private static void Start(Ped ped, string dict, string clip, float blendIn, float blendOut, int ms, int flag)
        {
            if (flag == Full) Function.Call(Hash.CLEAR_PED_SECONDARY_TASK, ped);
            Function.Call(Hash.TASK_PLAY_ANIM, ped, dict, clip, blendIn, blendOut, ms, flag, 0f, false, false, false);
        }

        private static void PlayOrQueue(Ped ped, string dict, string clip, float blendIn, float blendOut, int ms, int flag)
        {
            if (Loaded(dict))
            {
                Start(ped, dict, clip, blendIn, blendOut, ms, flag);
                return;
            }
            Request(dict);
            Queue.Add(new Pending { Ped = ped, Dict = dict, Clip = clip, BlendIn = blendIn, BlendOut = blendOut, Ms = ms, Flag = flag, Deadline = UM.Now + 1500 });
        }

        /// <summary>Plays queued animations whose dictionary has arrived.</summary>
        public static void Tick()
        {
            if (Queue.Count == 0) return;
            int now = UM.Now;
            for (int i = Queue.Count - 1; i >= 0; i--)
            {
                Pending p = Queue[i];
                if (!UM.Alive(p.Ped) || now > p.Deadline)
                {
                    Queue.RemoveAt(i);
                    continue;
                }
                if (!Loaded(p.Dict)) continue;
                Queue.RemoveAt(i);
                try { Start(p.Ped, p.Dict, p.Clip, p.BlendIn, p.BlendOut, p.Ms, p.Flag); }
                catch (Exception ex) { Log.Write("Memory", "anim: " + ex.Message); }
            }
        }

        public static void Clear() => Queue.Clear();

        /// <summary>Plays one of `names` (a library name) on `ped`; returns how long it runs.</summary>
        public static int Play(Ped ped, params string[] names)
        {
            if (ped == null || !ped.Exists()) return 0;
            string name = UM.Pick(names);
            if (!Resolve(ped, name, out string dict, out string clip, out int flag, out int ms)) return 0;
            PlayOrQueue(ped, dict, clip, 4f, 4f, ms, flag);
            return ms;
        }

        /// <summary>A MemCfg.Reactions entry: pick one animation and play it.</summary>
        public static int React(Ped ped, string key) =>
            MemCfg.Reactions.TryGetValue(key, out string[] list) ? Play(ped, list) : 0;

        private static float HeadingTowards(Vector3 from, Vector3 to) =>
            (float)(Math.Atan2(to.Y - from.Y, to.X - from.X) * 180.0 / Math.PI) - 90f;

        /// <summary>
        /// A two-person animation. The player stays put; the NPC is lined up at
        /// the pair's distance in front of them, facing back, then both halves
        /// start on the same frame. Only when they're already within PairedRange,
        /// so the line-up is a short step. 0 when it couldn't play (then the
        /// caller falls back to a solo gesture, as the Lua did).
        /// </summary>
        public static int Paired(Ped player, Ped ped, string name)
        {
            if (name == null || !MemCfg.PairedAnims.TryGetValue(name, out MemCfg.Pair p) || ped == null || !ped.Exists()) return 0;
            if (player.Position.DistanceTo(ped.Position) > MemCfg.PairedRange) return 0;

            string dict = p.Dict;
            if (p.Gendered)
            {
                bool fp = UM.IsFemale(player), fn = UM.IsFemale(ped);
                dict = string.Format(dict, (fp && fn) ? "f_f" : (fp || fn) ? "f_m" : "m_m");
            }
            if (!Loaded(dict))
            {
                Request(dict);
                return 0;
            }

            Function.Call(Hash.SET_ENTITY_HEADING, player, HeadingTowards(player.Position, ped.Position));
            Vector3 fwd = Function.Call<Vector3>(Hash.GET_ENTITY_FORWARD_VECTOR, player);
            Vector3 pc = player.Position, nc = ped.Position;
            Function.Call(Hash.CLEAR_PED_TASKS, ped);
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, ped, pc.X + fwd.X * p.Offset, pc.Y + fwd.Y * p.Offset, nc.Z, false, false, false);
            Function.Call(Hash.SET_ENTITY_HEADING, ped, player.Heading + 180f);

            Function.Call(Hash.CLEAR_PED_SECONDARY_TASK, player);
            Function.Call(Hash.TASK_PLAY_ANIM, player, dict, p.A, 8f, 8f, p.Ms, Full, 0f, false, false, false);
            Function.Call(Hash.TASK_PLAY_ANIM, ped, dict, p.B, 8f, 8f, p.Ms, Full, 0f, false, false, false);
            return p.Ms;
        }

        /// <summary>The shoved reaction from whichever side `from` is on.</summary>
        public static int Shoved(Ped ped, Ped from)
        {
            if (ped == null || !ped.Exists() || from == null || !from.Exists()) return 0;
            float towards = HeadingTowards(ped.Position, from.Position);
            float rel = (((towards - ped.Heading + 540f) % 360f + 360f) % 360f) - 180f; // -180..180, 0 = in front (Lua's % is never negative)
            string clip = Math.Abs(rel) <= 45f ? "shoved_front"
                : Math.Abs(rel) >= 135f ? "shoved_back"
                : rel > 0f ? "shoved_left" : "shoved_right";
            PlayOrQueue(ped, "reaction@shove", clip, 8f, 4f, 1500, Full);
            return 1500;
        }
    }
}
