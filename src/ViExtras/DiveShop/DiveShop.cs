// Dive shops: where scuba gear is bought. The user, 2026-11-13: "the scooba suits should be something you gotta buy",
// then picked dive shops by the water, $1,500, in 2.8.2.
using System;
using System.IO;
using System.Reflection;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Scuba gear is bought at a dive shop: the boathouses at the LSYMC marina (Puerto Del Sol), Paleto Bay and
    /// Millars on the Alamo Sea, each with the game's scuba gear icon on the map. On foot at one, GTA6Hud's own
    /// prompt says BUY SCUBA GEAR ($1500) and E (or Square) pays from the cash and then BuckMe, as the pump's nozzle
    /// does. Owning it is Hud\Save\scuba_gear.txt ("owned"), one for every character as the slim jim is: GTA6Hud's
    /// DIVE (patched) only comes up once that file is there. Swimming over deep water without it, the feed says once
    /// a session where to get it.
    /// </summary>
    public class DiveShop : Script
    {
        private const string Tag = "DiveShop";
        private const float Reach = 2.2f, PromptLift = 0.9f;
        private const int OwnedCheckMs = 2000;

        // the boathouses' access points (the server's qbx_garages boathouses, on the dock by the water)
        private static readonly Vector3[] Shops =
        {
            new Vector3(-794.64f, -1510.89f, 1.6f),   // LSYMC marina, Puerto Del Sol
            new Vector3(-277.4f, 6637.01f, 7.5f),     // Paleto Bay
            new Vector3(1299.02f, 4216.42f, 33.91f),  // Millars, Alamo Sea
        };

        private readonly bool _enabled;
        private readonly int _price;
        private readonly bool _hint;
        private readonly Blip[] _blips = new Blip[3];
        private string _file;
        private bool _owned;
        private long _ownedAt = -OwnedCheckMs;
        private bool _hinted;
        private bool _wasDown;

        // GTA6Hud's prompt pieces (as Nozzle)
        private bool _looked, _ready;
        private Func<Vector3, bool> _focused;
        private Func<bool> _contextDown;
        private Action<float, float, string, string, string, bool, bool, bool> _drawMarker;
        private Action<Vector3> _passiveDot;
        private Action<string> _notify;
        private FieldInfo _scubaOn, _scubaDepth;

        public DiveShop()
        {
            Paths.Init(this);
            _enabled = Config.Get(Config.Switches, "DiveShops", true);
            _price = Math.Max(0, Config.Get(Tag, "Price", 1500));
            _hint = Config.Get(Tag, "Hint", true);
            if (!_enabled) return;
            _file = Path.Combine(Paths.Scripts, "Hud", "Save", "scuba_gear.txt");
            Tick += OnTick;
            Aborted += (s, e) => RemoveBlips();
        }

        private bool Look()
        {
            if (_ready) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            const BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = hud.GetType();
            Type refs = t.Assembly.GetType("GTA6Hud.Ref", false);
            _focused = HudBridge.Bind<Func<Vector3, bool>>(hud, t.GetMethod("Focused", Stat, null, new[] { typeof(Vector3) }, null));
            _contextDown = HudBridge.Bind<Func<bool>>(hud, t.GetMethod("ContextDown", Stat, null, Type.EmptyTypes, null));
            _drawMarker = HudBridge.Bind<Action<float, float, string, string, string, bool, bool, bool>>(hud, t.GetMethod("DrawWorldMarker", Own, null,
                new[] { typeof(float), typeof(float), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool) }, null));
            _passiveDot = HudBridge.Bind<Action<Vector3>>(hud, t.GetMethod("PassiveDot", Own, null, new[] { typeof(Vector3) }, null));
            _notify = HudBridge.Bind<Action<string>>(null, t.GetMethod("Notify", Stat, null, new[] { typeof(string) }, null));
            _scubaOn = refs?.GetField("ScubaEnabled", Stat);
            _scubaDepth = refs?.GetField("ScubaDepth", Stat);
            try
            {
                // GTA6Hud's own save path for it, if it has one (the DIVE patch reads Hud\Save\scuba_gear.txt)
                MethodInfo save = t.Assembly.GetType("GTA6Hud.HudPaths", false)?.GetMethod("Save", Stat, null, new[] { typeof(string), typeof(string) }, null);
                string dir = t.GetProperty("HudDir", Own)?.GetValue(hud) as string;
                if (save != null && !string.IsNullOrEmpty(dir)) _file = (string)save.Invoke(null, new object[] { dir, "scuba_gear.txt" });
            }
            catch { }
            if (_focused == null || _contextDown == null || _drawMarker == null)
            {
                Log.Write(Tag, "GTA6Hud's prompts weren't all found; the dive shops stay closed");
                return false;
            }
            _ready = true;
            Log.Write(Tag, "on: scuba gear for $" + _price + " at " + Shops.Length + " dive shops");
            return true;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now || !Look()) return;
            Blips();
            if (HudBridge.Paused) return;
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists() || player.IsDead) return;
            long now = RealTime.Ms;
            if (now - _ownedAt >= OwnedCheckMs)
            {
                _ownedAt = now;
                try { _owned = File.Exists(_file); } catch { _owned = false; }
            }
            if (!_owned && _hint && !_hinted && DeepSwim(player))
            {
                _hinted = true;
                Say("~b~Scuba gear~s~: buy it at a dive shop (the diver on the map) to DIVE.");
            }

            Vector3 shop = Vector3.Zero;
            bool near = false;
            if (!player.IsInVehicle() && !HudBridge.Busy && !HudBridge.UiInTheWay)
            {
                foreach (Vector3 s in Shops)
                {
                    if (player.Position.DistanceTo(s) <= Reach) { shop = s; near = true; break; }
                }
            }
            // GTA6Hud's ContextDown takes E from the game for the frame it's asked in: only asked here, at a shop
            bool down = near && _contextDown();
            bool pressed = down && !_wasDown;
            _wasDown = down;
            if (!near) return;
            string label = _owned ? "SCUBA GEAR (OWNED)" : "BUY SCUBA GEAR ($" + _price + ")";
            if (!Prompt(shop + new Vector3(0f, 0f, PromptLift), label, !_owned) || !pressed) return;
            Buy();
        }

        private void Buy()
        {
            if (_owned)
            {
                Say("You already have scuba gear.");
                return;
            }
            if (Cash.Spendable < _price)
            {
                Say("Not enough cash");
                return;
            }
            if (_price > 0 && !Cash.Spend(_price))
            {
                Say("Payment failed");
                return;
            }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_file));
                File.WriteAllText(_file, "owned");
                _owned = true;
            }
            catch (Exception ex)
            {
                Log.Write(Tag, "couldn't save scuba_gear.txt: " + ex.Message);
            }
            Audio.PlaySoundFrontend("PURCHASE", "HUD_LIQUOR_STORE_SOUNDSET");
            Say("~g~Scuba gear bought.~s~ Swim out to deep water and DIVE.");
            Log.Write(Tag, "scuba gear bought for $" + _price);
        }

        // swimming over water at least GTA6Hud's [Activities] ScubaDepth deep, with its scuba on
        private bool DeepSwim(Ped player)
        {
            if (!player.IsSwimming) return false;
            if (_scubaOn != null && _scubaOn.GetValue(null) is bool on && !on) return false;
            float depth = _scubaDepth?.GetValue(null) is float d ? d : 6f;
            Vector3 at = player.Position;
            float ground = World.GetGroundHeight(new Vector3(at.X, at.Y, at.Z - 1f));
            return at.Z - ground >= depth;
        }

        // the game's scuba gear icon at each shop, near ones only on the radar
        private void Blips()
        {
            for (int i = 0; i < Shops.Length; i++)
            {
                if (_blips[i] != null && _blips[i].Exists()) continue;
                Blip b = World.CreateBlip(Shops[i]);
                b.Sprite = BlipSprite.ScubaGear;
                b.IsShortRange = true;
                b.Scale = 0.85f;
                b.Name = "Dive Shop";
                _blips[i] = b;
            }
        }

        private void RemoveBlips()
        {
            for (int i = 0; i < _blips.Length; i++)
            {
                if (_blips[i] != null && _blips[i].Exists()) _blips[i].Delete();
                _blips[i] = null;
            }
        }

        // GTA6Hud's world marker, as Nozzle's: the word and E, a dot while it's off to the side
        private bool Prompt(Vector3 at, string label, bool enabled)
        {
            if (!GTA.UI.Screen.IsSphereVisible(at, 0.4f)) return false;
            if (!_focused(at))
            {
                _passiveDot?.Invoke(at);
                return false;
            }
            var p = GTA.UI.Screen.WorldToScreen(at, true);
            if (p.X <= 0f && p.Y <= 0f) return false;
            _drawMarker(p.X * 1.5f, p.Y * 1.5f, label, "square", "E", false, false, enabled);
            return true;
        }

        private void Say(string text)
        {
            if (_notify != null) _notify(text);
            else GTA.UI.Notification.Show(text);
        }
    }
}
