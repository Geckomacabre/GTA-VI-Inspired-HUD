// Jason, GTA VI's other lead, as a fifth character on the wheel, the way GTA6Hud 2.1 has Lucia: his own ped stays in
// the world, the game's sky camera (START_PLAYER_SWITCH) flies between him and whoever you are, and the player is
// handed over at the top of the flight (switch state 8, where GTA6Hud hands over Lucia).
//
// He borrows the story character you came to him from, his host: that one is left standing where you were, and
// Jason's money is the host's (GTA6Hud shows Lucia's that way, from its _luciaActive and _luciaHost, which say so
// while Jason is the player). From Lucia he takes her host, and to Lucia he gives his. Back to the host is the same
// sky flight; to anyone else it's the game's own switch from the host standing unseen behind Jason, as from Lucia
// (BackToHostAt). A host who isn't in the world any more (GTA6Hud keeps one only for the session it made him in)
// gets a stand-in in his saved clothes for that switch.
//
// His model is the player's own download (GTA6Hud.ini [Jason] Model, jdfinal by default), never shipped with the HUD;
// without it in the game there's no Jason on the wheel. [Jason] Enabled = False leaves him off.
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using GTA;
using GTA.Math;

namespace ViSwitch
{
    /// <summary>
    /// The wheel as ViExtras' Switcher draws it (GTA VI's row of character cards), read by reflection each frame:
    /// how far open (0..1), the card picked and the player's own (0 Michael, 1 Franklin, 2 Trevor, 3 Lucia,
    /// 4 Jason; -1 none), and who can be switched to. While a Switcher is running, ViSwitch doesn't draw GTA6Hud's
    /// wheel.
    /// </summary>
    public static class SwitchState
    {
        public static float Blend;
        public static int Selected = -1, Current = -1;
        public static bool[] Available = { true, true, true, false, false };
    }

    public partial class ViSwitch
    {
        const ulong START_SWITCH = 0xFAA23F2CBA159D67;   // START_PLAYER_SWITCH(from, to, flags, type)
        const ulong IN_CDIMAGE = 0x35B9E0803292B641;     // IS_MODEL_IN_CDIMAGE
        const ulong FADED_IN = 0x5A859503B0C08678;       // IS_SCREEN_FADED_IN
        const ulong DEFAULT_LOOK = 0x45EEE61580806D63;   // SET_PED_DEFAULT_COMPONENT_VARIATION

        // the wheel's order, left to right: Michael, Franklin, Trevor, then Jason and Lucia together
        static readonly int[] RowOrder = { 0, 1, 2, 4, 3 };

        int _jason;                     // his model's hash
        bool _jasonWanted = true, _jasonOn, _jasonChecked;
        Ped _jasonPed, _jasonHostPed;
        int _jasonHost = -1;            // 0..2: the story character he came from, -1 none yet
        int _jasonLeftAt = -1;          // real ms he was last switched away from
        Scene _jasonScene;
        bool _jasonHeld;
        int _jasonSceneUntil, _jasonSettleAt, _jasonMissionAt;

        // the sky flight between two peds: the player goes to _flyTo at switch state 8, then _flyHandOver runs
        Ped _flyTo;
        bool _flyChanged;
        int _flyGiveUpAt;
        Action _flyHandOver;

        // ViExtras' Switcher draws the cards ([Switcher] Enabled; looked for every 2 s until found)
        bool _cardsDrawer;
        int _cardsLookAt;

        void InitJason()
        {
            string ini = Path.Combine(BaseDirectory, "GTA6Hud.ini");
            _jasonWanted = ReadIniBool(ini, "Jason", "Enabled", true);
            string model = ReadIniString(ini, "Jason", "Model", "jdfinal");
            _jason = Call<int>(HASH_KEY, string.IsNullOrEmpty(model) ? "jdfinal" : model);
        }

        // Once the game is up: is his model in it? (as GTA6Hud asks about Lucia's)
        void CheckJason()
        {
            if (_jasonChecked || !Call<bool>(FADED_IN)) return;
            _jasonChecked = true;
            _jasonOn = _jasonWanted && Call<bool>(IN_CDIMAGE, _jason);
            Log(_jasonOn ? "Jason's model is in the game: Jason is on the wheel" : _jasonWanted ? "no Jason model in the game" : "[Jason] Enabled = False");
        }

        void LookForCardsDrawer()
        {
            if (_cardsDrawer || RealNow() < _cardsLookAt) return;
            _cardsLookAt = RealNow() + 2000;
            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("ViExtras.Switcher", false);
                var on = t?.GetField("Drawing", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (on == null || !(bool)on.GetValue(null)) continue;
                _cardsDrawer = true;
                Log("found ViExtras' Switcher: it draws the wheel's cards");
                return;
            }
        }

        // The next card along the row from sel (dir +1 / -1) that can be picked.
        int RowStep(int sel, int dir, bool lucia)
        {
            int at = Array.IndexOf(RowOrder, sel);
            if (at < 0) at = 0;
            for (int n = 0; n < RowOrder.Length; n++)
            {
                at = (at + dir + RowOrder.Length) % RowOrder.Length;
                int c = RowOrder[at];
                if (c < 3 || (c == 3 && lucia) || (c == 4 && _jasonOn)) return c;
            }
            return sel;
        }

        void PublishWheel(bool lucia)
        {
            SwitchState.Blend = _blend;
            if (_open) SwitchState.Selected = _sel;
            SwitchState.Current = _current;
            SwitchState.Available[3] = lucia;
            SwitchState.Available[4] = _jasonOn;
        }

        // ---------------- the switches ----------------

        void JasonSwitch(int ped, int cur, int target)
        {
            if (!_jasonOn) { Log("  Jason isn't in the game"); return; }
            if (_flyTo != null || Call<bool>(SWITCH_IN_PROGRESS)) { Log("  a switch is still flying"); return; }
            Ped me = (Ped)Entity.FromHandle(ped);
            if (me == null || !me.Exists()) return;
            if (target == 4) { ToJason(me, cur); return; }
            if (target == 3) { JasonToLucia(me); return; }
            int host = _jasonHost >= 0 ? _jasonHost : (target == 1 ? 0 : 1);
            Ped hostPed = Alive(_jasonHostPed) ? _jasonHostPed : null;
            if (target == host)
            {
                // back to his host: the same sky flight; a host who's gone comes back as a stand-in at one of his spots
                if (hostPed == null)
                {
                    Scene s = PickScene(host, me.Position, 200f);
                    hostPed = StandIn(host, Land(s.At), s.Heading);
                    if (hostPed == null) { Log("  no " + StoryNames[host] + " to switch back to"); return; }
                    _mLeaveInWorld?.Invoke(null, new object[] { hostPed });
                }
                Ped to = hostPed;
                Log("  sky flight from Jason back to " + StoryNames[host]);
                FlyTo(me, to, () =>
                {
                    LeaveInWorld(me);
                    _jasonLeftAt = RealNow();
                    HudLucia(false, host);
                });
                return;
            }
            // to anyone else: the game's own switch, from his host unseen behind him
            BackToHostAt(me, target, hostPed, host);
        }

        void ToJason(Ped me, int cur)
        {
            int host;
            Ped hostPed;
            if (cur >= 0 && cur < 3) { host = cur; hostPed = me; }
            else if (cur == 3)
            {
                host = HostIndex();
                hostPed = null;
                try { hostPed = _fHostPed?.GetValue(_hud) as Ped; } catch { }
            }
            else { Log("  not on a story character or Lucia"); return; }
            Ped jason = PlaceJason(me);
            if (jason == null) return;
            Log("  sky flight to Jason" + (cur == 3 ? " from Lucia" : "") + "; his host " + (host >= 0 ? StoryNames[host] : "none"));
            FlyTo(me, jason, () =>
            {
                LeaveInWorld(me);                       // the host (or Lucia) stays standing where you were
                if (cur == 3) _luciaLeftAt = RealNow();
                _jasonHost = host;
                _jasonHostPed = hostPed;
                HudLucia(true, host);                   // his money is the host's
            });
        }

        void JasonToLucia(Ped me)
        {
            PlaceLucia(me.Handle);                       // her ped (GTA6Hud's), at a haunt unless left under a minute ago
            Ped lucia = null;
            try { lucia = _fLuciaPed?.GetValue(_hud) as Ped; } catch { }
            if (!Alive(lucia))
            {
                try { lucia = _mEnsureLucia?.Invoke(_hud, null) as Ped; } catch { }
                if (!Alive(lucia)) { Log("  no Lucia ped"); return; }
            }
            int host = _jasonHost;
            Ped hostPed = _jasonHostPed;
            Log("  sky flight from Jason to Lucia");
            Ped to = lucia;
            FlyTo(me, to, () =>
            {
                LeaveInWorld(me);
                _jasonLeftAt = RealNow();
                // GTA6Hud's Lucia, as if it had switched her in from the host: active, her ped, the host
                try
                {
                    _fLuciaActive?.SetValue(_hud, true);
                    _fLuciaPed?.SetValue(_hud, to);
                    if (Alive(hostPed)) _fHostPed?.SetValue(_hud, hostPed);
                    if (host >= 0) _fLuciaHost?.SetValue(_hud, host);
                    _mLuciaSave?.Invoke(_hud, null);
                }
                catch (Exception ex) { Log("  GTA6Hud's Lucia state wasn't set: " + (ex.InnerException ?? ex).Message); }
            });
        }

        // Before the flight to him: unless he was left under a minute ago, his ped goes to a random haunt away from
        // the player (made there if there isn't one), held frozen until its ground has loaded (SettleJason).
        Ped PlaceJason(Ped me)
        {
            _jasonHeld = false;
            if (Alive(_jasonPed) && _jasonLeftAt >= 0 && RealNow() - _jasonLeftAt < LuciaKeepMs)
            {
                Log("  Jason was left " + (RealNow() - _jasonLeftAt) / 1000 + " s ago: he's where you left him");
                return _jasonPed;
            }
            Scene scene = PickScene(-1, me.Position, 200f);
            if (!Alive(_jasonPed))
            {
                var model = new Model(_jason);
                if (!model.Request(4000)) { Log("  Jason's model didn't load"); return null; }
                _jasonPed = World.CreatePed(model, scene.At, scene.Heading);
                model.MarkAsNoLongerNeeded();
                if (_jasonPed == null) { Log("  Jason's ped wasn't made"); return null; }
                Call(DEFAULT_LOOK, _jasonPed.Handle);
                if (ApplyLook(_jasonPed)) Log("  Jason in his saved look");
                _mLeaveInWorld?.Invoke(null, new object[] { _jasonPed });
            }
            int j = _jasonPed.Handle;
            Log("  Jason at " + scene.Name + " (" + scene.Scenario + ")");
            Call(CLEAR_TASKS, j);
            Call(FREEZE, j, true);   // until his ground has loaded: a ped on unloaded ground drops through the map
            Call(SET_COORDS, j, scene.At.X, scene.At.Y, scene.At.Z, false, false, false, false);
            Call(SET_HEADING, j, scene.Heading);
            _jasonScene = scene;
            _jasonHeld = true;
            _jasonSceneUntil = RealNow() + LuciaSettleMs;
            return _jasonPed;
        }

        // As SettleLucia: once he's the player and the haunt's collision is in, let go on the ground right there and
        // start the haunt's activity; if the flight never got to him, he waits frozen until the player comes near.
        void SettleJason(int player)
        {
            if (!_jasonHeld) return;
            if (!Alive(_jasonPed)) { _jasonHeld = false; return; }
            int j = _jasonPed.Handle;
            Scene s = _jasonScene;
            bool handedOver = player == j;
            bool waiting = RealNow() < _jasonSceneUntil;
            if (waiting) Call(REQUEST_COLLISION, s.At.X, s.At.Y, s.At.Z);
            else
            {
                if (RealNow() < _jasonSettleAt) return;
                _jasonSettleAt = RealNow() + 500;
            }
            if (!Call<bool>(COLLISION_LOADED, j)) return;
            if (waiting && !handedOver && Call<bool>(SWITCH_IN_PROGRESS)) return;   // the hand-over clears his tasks
            Vector3 at = Land(s.At);
            Call(SET_COORDS, j, at.X, at.Y, at.Z, false, false, false, false);
            Call(SET_HEADING, j, s.Heading);
            Call(FREEZE, j, false);
            Call(SCENARIO_IN_PLACE, j, s.Scenario, 0, true);
            if (handedOver) _sceneActive = true;
            Log("Jason landed at " + s.Name + (handedOver ? "" : " (not the player: the flight didn't get to him)"));
            _jasonHeld = false;
        }

        // ---- stores ----
        // GTA keeps its stores locked for anyone but Michael, Franklin and Trevor, and GTA6Hud unlocks the clothes
        // stores and Ammu-Nation for Lucia only (UpdateLuciaShopDoors), so as Jason every one was shut (the user: "all
        // the clothes stores are locked i cant walk in them"). The same doors open for him, as GTA6Hud opens them:
        // within 35 m of a clothes rack or an Ammu-Nation door, the closest door of each of its door models, four times
        // a second. Its own lists (ShopStands' racks, AmmuCounters' doors, StoreDoorModels, AmmuDoorModels).
        const ulong DOOR_OF_TYPE = 0xF82D8F1926A02C3D;   // SET_STATE_OF_CLOSEST_DOOR_OF_TYPE
        const float StoreReach = 35f;
        Vector3[] _storeRacks, _ammuDoors;
        int[] _storeDoorModels, _ammuDoorModels;
        bool _storesLooked;
        int _jasonDoorsAt;

        void JasonStoreDoors(int player)
        {
            if (!_jasonOn || player == 0 || Call<int>(ENTITY_MODEL, player) != _jason) return;
            int now = RealNow();
            if (now < _jasonDoorsAt) return;
            _jasonDoorsAt = now + 250;
            if (!LookStores()) return;
            Vector3 at = Call<Vector3>(ENTITY_COORDS, player, true);
            UnlockNear(at, _storeRacks, _storeDoorModels);
            UnlockNear(at, _ammuDoors, _ammuDoorModels);
        }

        static void UnlockNear(Vector3 at, Vector3[] places, int[] models)
        {
            foreach (Vector3 p in places)
            {
                float dx = p.X - at.X, dy = p.Y - at.Y;
                if (dx * dx + dy * dy >= StoreReach * StoreReach) continue;
                foreach (int m in models) Call(DOOR_OF_TYPE, m, at.X, at.Y, at.Z, false, 0f, false);
                return;
            }
        }

        bool LookStores()
        {
            if (_storesLooked) return _storeRacks != null;
            if (_hud == null) return false;
            _storesLooked = true;
            try
            {
                const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                Type t = _hud.GetType();
                var stands = (Array)t.GetField("ShopStands", Static).GetValue(null);
                var counters = (Array)t.GetField("AmmuCounters", Static).GetValue(null);
                var racks = new Vector3[stands.Length];
                for (int i = 0; i < racks.Length; i++)
                {
                    object s = stands.GetValue(i);
                    racks[i] = (Vector3)s.GetType().GetField("Rack").GetValue(s);
                }
                var doors = new Vector3[counters.Length];
                for (int i = 0; i < doors.Length; i++)
                {
                    object c = counters.GetValue(i);
                    doors[i] = (Vector3)c.GetType().GetField("Item4").GetValue(c);
                }
                _storeDoorModels = (int[])t.GetField("StoreDoorModels", Static).GetValue(null);
                _ammuDoorModels = (int[])t.GetField("AmmuDoorModels", Static).GetValue(null);
                _storeRacks = racks;
                _ammuDoors = doors;
                Log("stores: " + racks.Length + " clothes racks and " + doors.Length + " Ammu-Nation doors to open for Jason");
            }
            catch (Exception ex)
            {
                _storeRacks = null;
                Log("stores: GTA6Hud's store lists were not found (" + ex.Message + "); stores stay locked for Jason");
            }
            return _storeRacks != null;
        }

        // As GTA6Hud does for Lucia: a mission starting while you're Jason takes you back to his host.
        void JasonMission(int player)
        {
            if (!_jasonOn || _flyTo != null || player == 0 || Call<int>(ENTITY_MODEL, player) != _jason) return;
            if (RealNow() < _jasonMissionAt) return;
            _jasonMissionAt = RealNow() + 400;
            if (!Call<bool>(MISSION_FLAG) || Call<bool>(SWITCH_IN_PROGRESS) || !Alive(_jasonHostPed)) return;
            Log("a mission is on: back from Jason to " + StoryNames[_jasonHost]);
            JasonSwitch(player, 4, _jasonHost);
        }

        // ---------------- the sky flight ----------------

        void FlyTo(Ped from, Ped to, Action handOver)
        {
            float d = from.Position.DistanceTo(to.Position);
            Call(START_SWITCH, from.Handle, to.Handle, 0, d > 1000f ? 1 : 2);
            _flyTo = to;
            _flyChanged = false;
            _flyHandOver = handOver;
            _flyGiveUpAt = RealNow() + 4000;
        }

        void UpdateFlight()
        {
            if (_flyTo == null) return;
            bool flying = Call<bool>(SWITCH_IN_PROGRESS);
            if (flying && !_flyChanged && Call<int>(SWITCH_STATE) >= 8 && _flyTo.Exists())
            {
                Call(CHANGE_PLAYER_PED, Call<int>(PLAYER_ID), _flyTo.Handle, false, false);
                Call(BLOCK_EVENTS, _flyTo.Handle, false);
                Call(CLEAR_TASKS, _flyTo.Handle);
                _flyChanged = true;
                _lastModel = Call<int>(ENTITY_MODEL, _flyTo.Handle);   // ViSwitch's own swap, not an outside one
                Log("  handed over to " + ModelName(_lastModel) + " in the sky");
                try { _flyHandOver?.Invoke(); } catch (Exception ex) { Log("  after the hand-over: " + (ex.InnerException ?? ex).Message); }
            }
            if (flying || (!_flyChanged && RealNow() < _flyGiveUpAt)) return;
            if (!_flyChanged) Log("  the sky camera didn't start");
            _flyTo = null;
            _flyHandOver = null;
        }

        // ---------------- helpers ----------------

        static bool Alive(Ped p) { return p != null && p.Exists() && !p.IsDead; }

        void LeaveInWorld(Ped p)
        {
            try { _mLeaveInWorld?.Invoke(null, new object[] { p }); } catch (Exception ex) { Log("  couldn't leave " + ModelName(p.Model.Hash) + " in the world: " + (ex.InnerException ?? ex).Message); }
        }

        // GTA6Hud's Lucia fields, which say whose money shows: Jason's is his host's, as Lucia's is.
        void HudLucia(bool active, int host)
        {
            try
            {
                _fLuciaActive?.SetValue(_hud, active);
                if (host >= 0) _fLuciaHost?.SetValue(_hud, host);
                if (!active) _mLuciaSave?.Invoke(_hud, null);
            }
            catch (Exception ex) { Log("  GTA6Hud's host money wasn't set: " + (ex.InnerException ?? ex).Message); }
        }

        // A stand-in for a story character who isn't in the world: his model in the clothes GTA6Hud last saw him in
        // (its saved outfits; the game's default look without one), at the spot given.
        Ped StandIn(int who, Vector3 at, float heading)
        {
            var model = new Model((int)StoryModels[who]);
            if (!model.Request(4000)) { Log("  " + StoryNames[who] + "'s model didn't load"); return null; }
            Ped p = World.CreatePed(model, at, heading);
            model.MarkAsNoLongerNeeded();
            if (p == null) return null;
            bool dressed = false;
            try
            {
                if (_mOutfitApply != null && _fOutfits?.GetValue(_hud) is IDictionary outfits && outfits.Contains(model.Hash))
                {
                    _mOutfitApply.Invoke(null, new object[] { p, outfits[model.Hash] });
                    dressed = true;
                }
            }
            catch (Exception ex) { Log("  the stand-in's clothes: " + (ex.InnerException ?? ex).Message); }
            if (!dressed) Call(DEFAULT_LOOK, p.Handle);
            Log("  a stand-in for " + StoryNames[who] + (dressed ? " in his saved clothes" : ""));
            return p;
        }

        static string ReadIniString(string path, string section, string key, string fallback)
        {
            try
            {
                string cur = null;
                foreach (string raw in File.ReadAllLines(path))
                {
                    string line = raw.Trim();
                    if (line.StartsWith("[") && line.EndsWith("]")) { cur = line.Substring(1, line.Length - 2); continue; }
                    if (cur == null || !cur.Equals(section, StringComparison.OrdinalIgnoreCase)) continue;
                    int eq = line.IndexOf('=');
                    if (eq > 0 && line.Substring(0, eq).Trim().Equals(key, StringComparison.OrdinalIgnoreCase))
                        return line.Substring(eq + 1).Trim();
                }
            }
            catch { }
            return fallback;
        }
    }
}
