using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// People in the Fleecas, so a robbery there has someone in it: two tellers behind the counter and three
    /// customers in the lobby. Story mode leaves the Fleecas empty (a probe of the Legion Square branch found only
    /// GTA6Hud's guard), and GTA6Hud's robbery only takes the people already inside as its witnesses, so a bank
    /// robbery had nobody to cower, call the police or be taken hostage. Vangelico's shop floor does the same for
    /// the jewel store; Pacific Standard has its own staff.
    ///
    /// The six Fleecas are one interior placed six times, so the spots are written once, in the interior's own
    /// frame (x across the room, y from the entrance in, from the SITE line of Hud\Robbery\sites.txt, the file
    /// GTA6Hud reads its banks from): the counter runs across y 0.3 to 0.65 with its four windows at x -2.2, -3.7,
    /// -5.2 and -6.6, the entrance doors are at y -3.4, and the vault door at y 3.9 (the site's own door, gate and
    /// guard lines, and the counter's props, measured in the Legion Square branch).
    ///
    /// As at Vangelico: the people go in once the bank's interior is ready and you're within StaffRange, outside
    /// missions; they stay this script's until you're within HandOverRange, then go to the game, keeping their
    /// scenario, because GTA6Hud's robbery only takes peds no script owns. Once the game has cleared them (you went
    /// away), the bank gets new ones next time. [Switches] BankCustomers in ViExtras.ini turns it off.
    /// </summary>
    public sealed class BankCustomers : Script
    {
        private const float StaffRange = 90f, HandOverRange = 30f, AgainRange = 60f, DropRange = 130f;

        private sealed class Spot
        {
            public string Model, Scenario; public float X, Y; public bool Faces;   // Faces: toward the counter (+y)
        }
        // tellers behind the counter, facing the lobby; customers at the windows and one waiting her turn
        private static readonly Spot[] Spots =
        {
            new Spot { Model = "a_f_y_business_01", X = -3.7f, Y = 1.3f, Faces = false, Scenario = "WORLD_HUMAN_STAND_IMPATIENT" },
            new Spot { Model = "a_m_y_business_02", X = -5.2f, Y = 1.3f, Faces = false, Scenario = "WORLD_HUMAN_STAND_IMPATIENT" },
            new Spot { Model = "a_m_m_bevhills_02", X = -3.7f, Y = -0.9f, Faces = true, Scenario = "WORLD_HUMAN_STAND_IMPATIENT" },
            new Spot { Model = "a_f_m_bevhills_01", X = -5.2f, Y = -1.0f, Faces = true, Scenario = "WORLD_HUMAN_STAND_MOBILE" },
            new Spot { Model = "a_m_y_hipster_01", X = -4.4f, Y = -2.4f, Faces = true, Scenario = "WORLD_HUMAN_STAND_MOBILE" },
        };

        private sealed class Bank
        {
            public string Name; public Vector3 Origin; public float Heading;
            public Ped[] People = new Ped[Spots.Length];
            public bool In, HandedOver;
        }

        private readonly bool _enabled;
        private readonly List<Bank> _banks = new List<Bank>();
        private long _lookAt;
        private int _errors;

        public BankCustomers()
        {
            Paths.Init(this);
            _enabled = Config.Get(Config.Switches, "BankCustomers", true);
            if (!_enabled) return;
            LoadSites();
            if (_banks.Count == 0) return;
            Tick += OnTick;
            Aborted += (s, e) => { foreach (Bank b in _banks) Clear(b); };
        }

        // The Fleeca SITE lines of GTA6Hud's sites.txt: "SITE FLEECA x y z heading ymap"
        private void LoadSites()
        {
            string path = Path.Combine(Paths.Scripts, "Hud", "Robbery", "sites.txt");
            try
            {
                if (!File.Exists(path)) { Log.Write("BankCustomers", "no " + path + ": the banks stay as the game has them"); return; }
                foreach (string raw in File.ReadAllLines(path))
                {
                    string[] f = raw.Split(new[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (f.Length < 6 || f[0] != "SITE" || f[1] != "FLEECA") continue;
                    var c = CultureInfo.InvariantCulture;
                    _banks.Add(new Bank
                    {
                        Name = f.Length > 6 ? f[6] : "fleeca",
                        Origin = new Vector3(float.Parse(f[2], c), float.Parse(f[3], c), float.Parse(f[4], c)),
                        Heading = float.Parse(f[5], c),
                    });
                }
                Log.Write("BankCustomers", _banks.Count + " Fleecas get tellers and customers");
            }
            catch (Exception ex) { Log.Write("BankCustomers", "sites.txt: " + ex.Message); }
        }

        private void OnTick(object sender, EventArgs e)
        {
            long now = RealTime.Ms;
            if (now < _lookAt) return;
            _lookAt = now + 1000;   // a look a second, as Vangelico's
            try
            {
                if (!GameReady.Now) return;
                Ped player = Game.Player.Character;
                if (player == null || !player.Exists()) return;
                bool mission = Game.IsMissionActive;
                foreach (Bank b in _banks) Tend(b, player, mission);
            }
            catch (Exception ex)
            {
                if (_errors++ < 5) Log.Write("BankCustomers", ex.ToString());
            }
        }

        private void Tend(Bank b, Ped player, bool mission)
        {
            float d = player.Position.DistanceTo(b.Origin);
            if (!b.In)
            {
                if (mission || d > StaffRange) return;
                Vector3 lobby = AtSpot(b, -4.4f, -1.0f);
                int interior = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, lobby.X, lobby.Y, lobby.Z + 1f);
                if (interior == 0 || !Function.Call<bool>(Hash.IS_INTERIOR_READY, interior)) return;
                bool loaded = true;
                foreach (Spot s in Spots)
                {
                    var m = new Model(s.Model);
                    if (!m.IsLoaded) { m.Request(); loaded = false; }
                }
                if (!loaded) return;
                int made = 0;
                for (int i = 0; i < Spots.Length; i++)
                {
                    Spot s = Spots[i];
                    Vector3 at = AtSpot(b, s.X, s.Y);
                    float heading = b.Heading + (s.Faces ? 0f : 180f);
                    int h = Function.Call<int>(Hash.CREATE_PED, 26, new Model(s.Model).Hash, at.X, at.Y, at.Z, heading, false, false);
                    Ped p = Entity.FromHandle(h) as Ped;
                    b.People[i] = p;
                    if (p == null) continue;
                    Function.Call(Hash.SET_PED_RANDOM_COMPONENT_VARIATION, p, 0);
                    Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, p, s.Scenario, 0, true);
                    made++;
                }
                foreach (Spot s in Spots) new Model(s.Model).MarkAsNoLongerNeeded();
                b.In = true;
                b.HandedOver = false;
                Log.Write("BankCustomers", b.Name + ": " + made + " of " + Spots.Length + " in (" + d.ToString("0") + " m out)");
            }
            else if (!b.HandedOver)
            {
                if (mission || d > DropRange) { Clear(b); return; }
                if (d >= HandOverRange) return;
                foreach (Ped p in b.People)
                {
                    if (p == null || !p.Exists()) continue;
                    Function.Call(Hash.SET_PED_KEEP_TASK, p, true);
                    p.MarkAsNoLongerNeeded();
                }
                b.HandedOver = true;
                Log.Write("BankCustomers", b.Name + ": handed to the game");
            }
            else if (d > AgainRange && Array.TrueForAll(b.People, p => p == null || !p.Exists()))
            {
                b.In = b.HandedOver = false;
                Log.Write("BankCustomers", b.Name + ": cleared by the game; new ones when you're back");
            }
        }

        // People still this script's go; handed-over ones are the game's (or the robbery's) now.
        private static void Clear(Bank b)
        {
            for (int i = 0; i < b.People.Length; i++)
            {
                if (!b.HandedOver && b.People[i] != null && b.People[i].Exists()) b.People[i].Delete();
                b.People[i] = null;
            }
            b.In = b.HandedOver = false;
        }

        // A spot in the bank's own frame to the world: x across, y in from the entrance, on the floor
        private static Vector3 AtSpot(Bank b, float x, float y)
        {
            double r = b.Heading * Math.PI / 180.0;
            var right = new Vector3((float)Math.Cos(r), (float)Math.Sin(r), 0f);
            var fwd = new Vector3(-(float)Math.Sin(r), (float)Math.Cos(r), 0f);
            return b.Origin + right * x + fwd * y + new Vector3(0f, 0f, 0.05f);
        }
    }
}
