using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using GTA;

namespace ViExtras.NpcMemory
{
    /// <summary>um_memory's short-term record for a ped that exists right now (client/memory.lua people[ped]).</summary>
    internal sealed class Person
    {
        public int Model;
        public string Key;          // the saved face this ped would be
        public string Zone;         // where you met them
        public float Opinion;
        public int Flags;           // MemCfg.Crimes flags they hold against you
        public int Stage;           // antagonize escalation
        public int StageAt;
        public int HostileUntil;    // squaring up / fighting after antagonize
        public int NextAct;         // per-ped interaction cooldown
        public int LastReact;       // last unprompted reaction (recognition)
        public bool Antagonized;
        public int AntagonizedAt;
        public bool Remembered;
        public int Met;
        public readonly HashSet<string> Counted = new HashSet<string>();      // zone kinds already counted
        public readonly HashSet<string> HonorCounted = new HashSet<string>(); // greet / antagonize streak
    }

    /// <summary>
    /// um_memory/client/memory.lua plus the persistence half of server/main.lua.
    ///
    ///   people [ped]  -> Person, while the ped exists
    ///   faces  [key]  -> opinion, crime flags, times met, recency
    ///   zones  [code] -> neighbourhood reputation -100..100
    ///
    /// The FiveM server kept one record per character and decayed it in real
    /// time when the character loaded. Here each story character (by player
    /// model: Michael, Franklin, Trevor, Lucia) has a file in Save\, decayed
    /// the same way when it loads. Nothing is saved before the record is read,
    /// so an early save can never wipe it.
    ///
    /// Police notoriety (the third table on FiveM) is gone: GTA6Hud keeps its
    /// own police memory in single player.
    /// </summary>
    internal static class Mem
    {
        internal sealed class Face
        {
            public float O;
            public int C, N, S;
        }

        private const double Day = 86400.0;

        public static bool Loaded { get; private set; }
        public static string Character { get; private set; }
        public static readonly Dictionary<string, Face> Faces = new Dictionary<string, Face>();
        public static readonly Dictionary<string, float> Zones = new Dictionary<string, float>();

        private static readonly Dictionary<int, Person> People = new Dictionary<int, Person>();
        private static int _seq;
        private static bool _dirty;

        public static int FaceCount => Faces.Count;

        // ---- people in front of you ---------------------------------------------------

        /// <summary>The short-term record for `ped`, linked to their saved face if they have one.</summary>
        public static Person Of(Ped ped)
        {
            int model = ped.Model.Hash;
            // Entity handles are recycled; a different model means a different person.
            if (People.TryGetValue(ped.Handle, out Person r) && r.Model == model) return r;

            string code = UM.ZoneCode(ped.Position);
            r = new Person { Model = model, Key = UM.Fingerprint(ped, code), Zone = code };
            if (Faces.TryGetValue(r.Key, out Face face))
            {
                r.Remembered = true;
                r.Opinion = face.O;
                r.Flags = face.C;
                r.Met = face.N;
            }
            People[ped.Handle] = r;
            return r;
        }

        public static Person Peek(Ped ped)
        {
            if (ped == null || !People.TryGetValue(ped.Handle, out Person r)) return null;
            return r.Model == ped.Model.Hash ? r : null;
        }

        private static void EvictOne()
        {
            // Prefer forgetting someone with no crime on record, least recently met.
            string victim = null;
            int victimS = int.MaxValue;
            for (int pass = 1; pass <= 2 && victim == null; pass++)
                foreach (KeyValuePair<string, Face> f in Faces)
                    if ((pass == 2 || f.Value.C == 0) && f.Value.S < victimS)
                    {
                        victim = f.Key;
                        victimS = f.Value.S;
                    }
            if (victim != null) Faces.Remove(victim);
        }

        /// <summary>
        /// A meaningful exchange with `ped`: change their opinion, add a crime
        /// flag, and make sure they are saved as a face.
        /// </summary>
        public static void Touch(Ped ped, float opinionDelta, int flag = 0)
        {
            if (ped == null || !ped.Exists()) return;
            Person r = Of(ped);
            r.Opinion = Num.Clamp(r.Opinion + opinionDelta, -100f, 100f);
            r.Flags |= flag;

            if (!Faces.TryGetValue(r.Key, out Face face))
            {
                if (Faces.Count >= MemCfg.MaxFaces) EvictOne();
                face = new Face();
                Faces[r.Key] = face;
            }
            _seq++;
            face.O = r.Opinion;
            face.C = r.Flags;
            face.N++;
            face.S = _seq;
            r.Remembered = true;
            r.Met = face.N;
            _dirty = true;
        }

        /// <summary>Drops short-term records for peds that are gone.</summary>
        public static void Prune()
        {
            var gone = new List<int>();
            foreach (int handle in People.Keys)
                if (!Function_DoesEntityExist(handle)) gone.Add(handle);
            foreach (int h in gone) People.Remove(h);
        }

        private static bool Function_DoesEntityExist(int handle) =>
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, handle);

        // ---- neighbourhoods ----------------------------------------------------------------

        public static float ZoneRep(string code) => code != null && Zones.TryGetValue(code, out float v) ? v : 0f;

        public static void AddZone(string code, float delta)
        {
            if (string.IsNullOrEmpty(code) || delta == 0f) return;
            Zones[code] = Num.Clamp(ZoneRep(code) + delta, -100f, 100f);
            _dirty = true;
        }

        /// <summary>Each person moves their neighbourhood's opinion once per kind of deed.</summary>
        public static void AddZoneVia(Ped ped, string kind)
        {
            if (!MemCfg.ZoneDelta.TryGetValue(kind, out float delta)) return;
            if (ped != null && ped.Exists())
            {
                Person r = Of(ped);
                if (!r.Counted.Add(kind)) return;
                AddZone(r.Zone, delta);
            }
            else
            {
                AddZone(UM.ZoneCode(UM.Player.Position), delta);
            }
        }

        // ---- load / save ----------------------------------------------------------------------

        /// <summary>Michael, Franklin, Trevor, Lucia: whose memory this is, by the player's model.</summary>
        private static readonly int Michael = Game.GenerateHash("player_zero"), Franklin = Game.GenerateHash("player_one"),
            Trevor = Game.GenerateHash("player_two"), Lucia = Game.GenerateHash("lucia_s");

        public static string CharacterOf(Ped player)
        {
            int m = player.Model.Hash;
            if (m == Michael) return "michael";
            if (m == Franklin) return "franklin";
            if (m == Trevor) return "trevor";
            if (m == Lucia) return "lucia";
            return "ped_" + ((uint)m).ToString("x8");
        }

        private static string FileFor(string character) => Paths.Save("memory_" + character + ".txt");

        private static void Reset()
        {
            Loaded = false;
            Faces.Clear();
            Zones.Clear();
            People.Clear();
            _seq = 0;
            _dirty = false;
        }

        /// <summary>Saves the outgoing character, then reads (and decays) `character`'s record.</summary>
        public static void Switch(string character)
        {
            if (character == Character && Loaded) return;
            Flush();
            Reset();
            Character = character;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long last = now;
            string path = FileFor(character);
            try
            {
                if (File.Exists(path))
                {
                    foreach (string line in File.ReadAllLines(path))
                    {
                        string[] p = line.Split(' ');
                        if (p.Length == 2 && p[0] == "t") long.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out last);
                        else if (p.Length == 6 && p[0] == "f" && p[1].Length <= 96 && Faces.Count < MemCfg.MaxFaces)
                        {
                            var f = new Face
                            {
                                O = Num.Clamp(F(p[2]), -100f, 100f),
                                C = (int)Num.Clamp(F(p[3]), 0f, 65535f),
                                N = Math.Max(1, (int)F(p[4])),
                                S = Math.Max(0, (int)F(p[5])),
                            };
                            Faces[p[1]] = f;
                            if (f.S > _seq) _seq = f.S;
                        }
                        else if (p.Length == 3 && p[0] == "z" && p[1].Length <= 16 && Zones.Count < 200)
                        {
                            Zones[p[1]] = Num.Clamp(F(p[2]), -100f, 100f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Memory", "load " + character + ": " + ex.Message);
                if (File.Exists(path)) SaveFile.SetAside(path, "Memory");   // the next flush mustn't overwrite it
            }

            Decay(Math.Max(0.0, (now - last) / Day));
            Loaded = true;
            _dirty = true; // writes the decayed record back, as the server did
            UM.Debug("memory loaded for " + character + ": " + Faces.Count + " faces");
        }

        private static float F(string s) =>
            float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) && !float.IsNaN(v) ? v : 0f;

        /// <summary>server/main.lua decay(): opinions fade; a face only leaves memory if they never saw you do anything.</summary>
        private static void Decay(double days)
        {
            if (days <= 0) return;
            float faceMul = (float)Math.Pow(0.5, days / MemCfg.FaceHalfLifeDays);
            var drop = new List<string>();
            foreach (KeyValuePair<string, Face> f in Faces)
            {
                f.Value.O *= faceMul;
                if (Math.Abs(f.Value.O) < 3f && f.Value.C == 0) drop.Add(f.Key);
            }
            foreach (string k in drop) Faces.Remove(k);

            float zoneMul = (float)Math.Pow(0.5, days / MemCfg.ZoneHalfLifeDays);
            var codes = new List<string>(Zones.Keys);
            foreach (string code in codes)
            {
                float rep = Zones[code] * zoneMul;
                if (Math.Abs(rep) >= 1f) Zones[code] = rep;
                else Zones.Remove(code);
            }
        }

        public static void Flush()
        {
            if (!Loaded || !_dirty || Character == null) return;
            _dirty = false;
            var sb = new StringBuilder();
            sb.Append("# ViExtras Memory: faces and neighbourhoods this character has met. Delete to forget.\n");
            sb.Append("t ").Append(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (KeyValuePair<string, Face> f in Faces)
                sb.Append("f ").Append(f.Key).Append(' ')
                  .Append(Math.Floor(f.Value.O + 0.5).ToString(CultureInfo.InvariantCulture)).Append(' ')
                  .Append(f.Value.C.ToString(CultureInfo.InvariantCulture)).Append(' ')
                  .Append(f.Value.N.ToString(CultureInfo.InvariantCulture)).Append(' ')
                  .Append(f.Value.S.ToString(CultureInfo.InvariantCulture)).Append('\n');
            foreach (KeyValuePair<string, float> z in Zones)
                sb.Append("z ").Append(z.Key).Append(' ').Append(z.Value.ToString("0.###", CultureInfo.InvariantCulture)).Append('\n');
            try { SaveFile.Write(FileFor(Character), sb.ToString()); }
            catch (Exception ex) { Log.Write("Memory", "save: " + ex.Message); }
        }
    }

    /// <summary>
    /// um_memory/client/attitude.lua: how someone feels about you before you say
    /// a word. Your honor, this neighbourhood, what this person remembers, what
    /// you look like right now, and who they are.
    ///
    /// The Lua also took the police agency's notoriety off a cop's mood and
    /// um_livingworld's tiers off everyone's; neither exists here, and with
    /// both at their resting values those terms were zero anyway.
    /// </summary>
    internal static class Attitude
    {
        public const string Warm = "warm", Polite = "polite", Cold = "cold", Hostile = "hostile";

        public static string Band(float score)
        {
            if (score >= MemCfg.BandWarm) return Warm;
            if (score >= MemCfg.BandPolite) return Polite;
            if (score >= MemCfg.BandCold) return Cold;
            return Hostile;
        }

        /// <summary>qbx_honor's value, 0 while Honor isn't running.</summary>
        public static float Honor => HonorApi.Ready ? HonorApi.Value : 0f;

        public static float Of(Ped ped, out string band, out Temper temper, out PedClass cls)
        {
            Person r = Mem.Of(ped);
            Ped player = UM.Player;
            UM.Temperament(ped, out temper, out cls);
            float honor = Honor;

            float score = honor * MemCfg.HonorWeight
                + Mem.ZoneRep(UM.ZoneCode(ped.Position)) * MemCfg.ZoneWeight
                + r.Opinion * MemCfg.FaceWeight;

            if (cls == PedClass.Rich) score += honor * MemCfg.RichHonorWeight;
            else if (cls == PedClass.Gang) score += honor * MemCfg.GangHonorWeight;

            if (UM.Armed(player, 4)) score -= MemCfg.ArmedPenalty;
            score -= UM.Wanted * MemCfg.WantedPenaltyPerStar;
            if (UM.IsMasked(player)) score -= MemCfg.MaskedPenalty;
            if (UM.IsNight()) score -= MemCfg.NightPenalty;

            // Someone squaring up to you is hostile whatever the sums say.
            if (UM.Now < r.HostileUntil) score = Math.Min(score, MemCfg.BandCold - 1f);

            score = Num.Clamp(score, -100f, 100f);
            band = Band(score);
            return score;
        }

        /// <summary>Is `ped` currently squaring up to / fighting the player?</summary>
        public static bool IsHostile(Ped ped)
        {
            Person r = Mem.Peek(ped);
            if (r != null && UM.Now < r.HostileUntil) return true;
            return UM.InCombatWith(ped, UM.Player);
        }
    }
}
