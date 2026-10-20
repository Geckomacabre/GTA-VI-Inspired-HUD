using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Leonida Customs (the FiveM version's mod shop, Lenonida_customs) at the
    /// five Los Santos Customs, in place of GTA6Hud's own shop.
    ///
    /// Drive in and stop: the screen fades and the full-screen menu opens with
    /// the orbit camera on the car, as GTA's own shop and GTA6Hud's open. It
    /// opens again once the car has left the garage and come back. Pro Builds, Cosmetics and Performance;
    /// every option is fitted to the car while it is looked at and bought one at
    /// a time with the character's cash. The card shows the car's stats and how
    /// the option would change them; nitrous kits are kept by plate for the
    /// nitrous script.
    ///
    /// GTA6Hud keeps opening the garage doors and keeping GTA's own shop shut
    /// (its [Customs] settings stay on); only its menu is kept from opening.
    /// </summary>
    public sealed class LeonidaCustoms : Script
    {
        private readonly bool _enabled, _chameleon;
        private readonly float _radius;
        private CustomsMenu _menu;
        private bool _swallowBack;   // the menu just closed: Back / Esc / B stay off until let go
        private bool _armed = true;   // the car has left the garage since the menu last opened
        private FieldInfo _leftSince;
        private bool _lookedLeftSince;

        public LeonidaCustoms()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Customs", "Enabled", true);
            _chameleon = Config.Get("Customs", "ChameleonPaints", false);
            _radius = Config.Get("Customs", "SpotRadius", 7f);
            Tick += OnTick;
            Aborted += (s, e) =>
            {
                try { _menu?.Close(restore: true, instant: true); } catch { }
                if (Function.Call<bool>(Hash.IS_SCREEN_FADED_OUT) || Function.Call<bool>(Hash.IS_SCREEN_FADING_OUT)) Function.Call(Hash.DO_SCREEN_FADE_IN, 0);
            };
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled) return;
            if (_menu == null && !GameReady.Now) return;
            try
            {
                if (_menu != null)
                {
                    if (!_menu.Tick())
                    {
                        _menu = null;
                        _swallowBack = true;
                    }
                    return;
                }
                if (_swallowBack) SwallowBack();
                KeepGta6HudShopShut();
                Approach();
            }
            catch (Exception ex)
            {
                Log.Write("Customs", ex.ToString());
                try { _menu?.Close(restore: true, instant: true); } catch { }
                _menu = null;
            }
        }

        // Back held to leave (or tapped at the top of the menu) is still down when the menu has
        // gone: until it's let go it stays away from the game, so Esc doesn't open the pause menu
        // and B doesn't switch to the cinematic camera on the way out.
        private void SwallowBack()
        {
            bool held = Game.IsKeyPressed(Keys.Back) || Game.IsKeyPressed(Keys.Escape)
                || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 2, 202);
            if (!held) { _swallowBack = false; return; }
            foreach (int control in new[] { 199, 200, 202, 177, 80 })
            {
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, control, true);
                Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, control, true);
            }
        }

        /// <summary>
        /// GTA6Hud opens its own shop when a car stops inside one, once it has left
        /// and come back (_customsLeftSince). Kept false, it never does; everything
        /// else it does at the garages (doors, GTA's own shop kept shut, "lose the
        /// cops") carries on.
        /// </summary>
        private void KeepGta6HudShopShut()
        {
            object hud = HudBridge.Gta6Hud();
            if (hud == null) return;
            if (!_lookedLeftSince)
            {
                _lookedLeftSince = true;
                _leftSince = hud.GetType().GetField("_customsLeftSince", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (_leftSince == null) Log.Write("Customs", "GTA6Hud’s shop could not be kept shut; set GTA6Hud.ini [Customs] Enabled = False");
            }
            try { _leftSince?.SetValue(hud, false); } catch { }
        }

        private void Approach()
        {
            Ped ped = Game.Player.Character;
            Vehicle v = ped.IsInVehicle() ? ped.CurrentVehicle : null;
            if (v == null || !v.Exists() || v.GetPedOnSeat(VehicleSeat.Driver) != ped) return;
            Model model = v.Model;
            if ((!model.IsCar && !model.IsBike && !model.IsQuadBike) || model.IsBicycle) return;
            // FiveM's marker spot, or anywhere in the garage (its interior) round it,
            // as GTA6Hud's own shop finds a car stopped in the bay
            CustomsData.Shop shop = null;
            bool inside = Function.Call<int>(Hash.GET_INTERIOR_FROM_ENTITY, v) != 0;
            foreach (CustomsData.Shop s in CustomsData.Shops)
            {
                var spot = new Vector3(s.X, s.Y, s.Z);
                float d = v.Position.DistanceTo2D(spot);
                if ((d < _radius || inside && d < 25f) && Math.Abs(v.Position.Z - s.Z) < 4f) { shop = s; break; }
            }
            if (shop == null) { _armed = true; return; }
            // GTA6Hud tells a wanted player to lose the cops first
            if (!_armed || v.Speed > 2.5f || Game.Player.WantedLevel > 0 || HudBridge.UiInTheWay) return;
            _armed = false;
            Function.Call(Hash.DO_SCREEN_FADE_OUT, 300);
            long until = RealTime.Ms + 2000;
            while (!Function.Call<bool>(Hash.IS_SCREEN_FADED_OUT) && RealTime.Ms < until) Wait(0);
            try
            {
                _menu = new CustomsMenu(v, shop, _chameleon, Wait);
                if (!_menu.Open()) _menu = null;
            }
            catch (Exception ex)
            {
                Log.Write("Customs", "opening: " + ex);
                try { _menu?.Close(restore: true, instant: true); } catch { }
                _menu = null;
            }
            finally
            {
                Function.Call(Hash.DO_SCREEN_FADE_IN, 400);
            }
        }
    }

    /// <summary>The open menu: html/app.js's state and actions, and menu.lua's session.</summary>
    internal sealed class CustomsMenu
    {
        public readonly CustomsLogic Logic;
        public readonly string ShopLabel;
        public string Brand = "", Model = "", EmblemTex;
        public readonly List<MenuView> Stack = new List<MenuView>();
        public MenuView Top => Stack[Stack.Count - 1];
        public CustomsStats Shown, OwnedStats;
        public bool PadMode;
        public string CardOverride;
        public long ToastAt = -1;
        public DialogState Dialog;

        private readonly Vehicle _veh;
        private readonly CustomsCamera _camera;
        private readonly Gta6HudQuiet _quiet = new Gta6HudQuiet();
        private string _previewKit;
        private long _previewAt = -1;
        private bool _revving;
        private float _rpm;
        private readonly Input _input = new Input();
        private bool _waitRelease = true;
        private long _openedAt, _backHeldSince;
        private const int ReleaseWaitMs = 1500;   // the keys held driving in get this long to come up
        private const int ForceExitMs = 2000;     // Back held this long leaves from anywhere

        internal sealed class DialogState
        {
            public string Title;
            public List<string> Lines;
            public Action Ok, Back;
        }

        public CustomsMenu(Vehicle veh, CustomsData.Shop shop, bool chameleon, Action<int> wait)
        {
            _veh = veh;
            ShopLabel = shop.Label;
            Logic = new CustomsLogic(veh, chameleon, wait);
            _camera = new CustomsCamera(veh);
        }

        private CustomsTab Tab => Logic.Tabs[Stack[0].Tab];

        // =============================================================================
        // Open / close (OpenCustomsMenu / closeMenu)
        // =============================================================================

        public bool Open()
        {
            // SetModable: no mod kit, no SET_VEHICLE_MOD (the game crashes on that)
            if (Function.Call<int>(Hash.GET_NUM_MOD_KITS, _veh) > 0) Function.Call(Hash.SET_VEHICLE_MOD_KIT, _veh, 0);
            Logic.Build();
            int model = _veh.Model.Hash;
            string make = Function.Call<string>(Hash.GET_MAKE_NAME_FROM_VEHICLE_MODEL, model);
            string name = Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, model);
            Brand = Localized(make) ?? make ?? "";
            Model = Localized(name) ?? name ?? "";
            if (string.Equals(Brand, Model, StringComparison.OrdinalIgnoreCase)) Brand = "";
            EmblemTex = EmblemOf(Brand);

            _veh.IsPositionFrozen = true;
            Function.Call(Hash.SET_VEHICLE_ENGINE_ON, _veh, true, true, false);
            _quiet.Hold(true);
            Log.Write("Customs", "menu opened: " + Brand + " " + Model);
            _openedAt = RealTime.Ms;
            OwnedStats = Shown = Logic.Stats(Logic.Owned);
            PadMode = Glyphs.UsingPad;
            Stack.Add(new MenuView { Type = "root", Tab = 0, RootSel = new int[Math.Max(1, Logic.Tabs.Count)] });
            _camera.Start(Logic.Tabs.Count > 0 ? Logic.Tabs[0].Camera : "rear34");
            if (Logic.Tabs.Count == 0)
            {
                Logic.Tabs.Add(new CustomsTab { Id = "none", Label = "", Camera = "rear34", Categories = { new CustomsCategory { Label = "", Hint = "" } } });
                Ask("Nothing to fit", new List<string> { "This shop has no parts for this vehicle." }, () => Close(true), null);
            }
            else if (_veh.BodyHealth < 1000f) RepairDialog();
            return true;
        }

        private static string Localized(string key)
        {
            if (string.IsNullOrEmpty(key) || key == "CARNOTFOUND") return null;
            string s = Game.GetLocalizedString(key);
            return string.IsNullOrEmpty(s) || s == "NULL" ? null : s;
        }

        // the maker's mark: GTA6Hud's car_<make> in its vipause dictionary (BrandKey)
        private static string EmblemOf(string make)
        {
            if (string.IsNullOrEmpty(make)) return null;
            var sb = new System.Text.StringBuilder();
            foreach (char c in make.Normalize(System.Text.NormalizationForm.FormD))
                if (char.IsLetterOrDigit(c) && c < 128) sb.Append(char.ToLowerInvariant(c));
            if (sb.Length == 0) return null;
            string tex = "car_" + sb;
            Vector3 res = Function.Call<Vector3>(Hash.GET_TEXTURE_RESOLUTION, "vipause", tex);
            return res.X > 4f ? tex : null;
        }

        public void Close(bool restore, bool instant = false)
        {
            Log.Write("Customs", "menu closed" + (instant ? " at once" : ""));
            try
            {
                if (_veh.Exists())
                {
                    if (restore)
                    {
                        // back to what was last paid for
                        try { foreach (var kv in Logic.Owned) if (kv.Key != "nitrous" && kv.Key != "kit") Logic.Apply(kv.Key, Logic.Owned); }
                        catch (Exception ex) { Log.Write("Customs", "putting the car back as paid for: " + ex.Message); }
                    }
                    for (int door = 0; door <= 5; door++) Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, _veh, door, false);
                    Function.Call(Hash.SET_VEHICLE_LIGHTS, _veh, 0);
                }
            }
            catch (Exception ex) { Log.Write("Customs", "closing: " + ex.Message); }
            // whatever went wrong above, the car, the camera and the HUD are let go
            try { if (_veh.Exists()) _veh.IsPositionFrozen = false; }
            catch (Exception ex) { Log.Write("Customs", "unfreezing the car: " + ex.Message); }
            try { _camera.Stop(!instant); }
            catch (Exception ex)
            {
                Log.Write("Customs", "camera: " + ex.Message);
                Function.Call(Hash.RENDER_SCRIPT_CAMS, false, false, 0, true, false);
            }
            try { _quiet.Hold(false); }
            catch (Exception ex) { Log.Write("Customs", "HUD: " + ex.Message); }
            _closed = true;
        }

        private bool _closed;

        /// <summary>One frame of the open menu. False once it has closed.</summary>
        public bool Tick()
        {
            if (_closed) return false;
            if (!_veh.Exists() || _veh.IsDead || Game.Player.Character.IsDead || Game.Player.Character.CurrentVehicle != _veh)
            {
                Close(true, true);
                return false;
            }
            float dt = Game.LastFrameTime;
            Function.Call(Hash.DISABLE_ALL_CONTROL_ACTIONS, 0);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 199, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 200, true);
            // A way out whatever the menu is doing: Back held 2 s (Backspace, Esc, B / Circle).
            long now = RealTime.Ms;
            if (Game.IsKeyPressed(Keys.Back) || Game.IsKeyPressed(Keys.Escape) || PadPressed(202))
            {
                if (_backHeldSince == 0) _backHeldSince = now;
                else if (now - _backHeldSince >= ForceExitMs)
                {
                    Log.Write("Customs", "Back held " + ForceExitMs / 1000 + " s" + (Dialog != null ? " on \"" + Dialog.Title + "\"" : "") + (_waitRelease ? ", input still held off" : ""));
                    Close(true);
                    return false;
                }
            }
            else _backHeldSince = 0;
            ReadInput(dt);
            if (_closed) return false;
            if (_previewAt >= 0 && RealTime.Ms >= _previewAt) { _previewAt = -1; Preview(); }
            if (_revving)
            {
                _rpm = Math.Min(_rpm + 0.03f, 1f);
                _veh.CurrentRPM = _rpm;   // SetVehicleCurrentRpm is FiveM's; Script Hook V .NET writes it
            }
            _camera.Update(dt);
            CustomsUi.Draw(this);
            return !_closed;
        }

        // =============================================================================
        // Input: the page's keys, the controller (padLoop) and the mouse
        // =============================================================================

        private sealed class Input
        {
            public string Direction;
            public long NextRepeat;
            public readonly Dictionary<Keys, bool> Was = new Dictionary<Keys, bool>();
        }

        private bool KeyJust(Keys k)
        {
            bool now = Game.IsKeyPressed(k);
            _input.Was.TryGetValue(k, out bool was);
            _input.Was[k] = now;
            return now && !was;
        }

        private static bool PadPressed(int c) => Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 2, c) || Function.Call<bool>(Hash.IS_CONTROL_PRESSED, 2, c);
        private static bool PadJust(int c) => Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 2, c) || Function.Call<bool>(Hash.IS_CONTROL_JUST_PRESSED, 2, c);
        private static float Normal(int c) => Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 2, c);

        private static readonly Keys[] MenuKeys =
        {
            Keys.Up, Keys.Down, Keys.Left, Keys.Right, Keys.W, Keys.A, Keys.S, Keys.D, Keys.Enter, Keys.Space,
            Keys.Back, Keys.Escape, Keys.Tab, Keys.Q, Keys.E, Keys.R,
        };

        // The menu input still held from driving in (a key, or a pad control by number), or null.
        private static string HeldMenuInput()
        {
            foreach (Keys k in MenuKeys) if (Game.IsKeyPressed(k)) return k.ToString();
            for (int c = 187; c <= 206; c++) if (c != 191 && c != 192 && c != 193 && c != 194 && c != 195 && c != 196 && c != 197 && c != 198 && c != 199 && c != 200 && PadPressed(c)) return "control " + c;
            if (Math.Abs(Normal(195)) > 0.6f || Math.Abs(Normal(196)) > 0.6f) return "the left stick";
            if (Normal(207) > 0.1f || Normal(208) > 0.1f) return "a trigger";
            return null;
        }

        private void ReadInput(float dt)
        {
            if (_waitRelease)
            {
                // Nothing acts until what was held driving in comes up, for a moment at most:
                // an input that never reads as up (a stuck key, a drifting trigger) no longer locks the menu.
                string held = HeldMenuInput();
                if (held != null && RealTime.Ms - _openedAt < ReleaseWaitMs) return;
                if (held != null) Log.Write("Customs", held + " still read as held after " + ReleaseWaitMs / 1000f + " s; taking input anyway");
                _waitRelease = false;
            }
            bool usingPad = Glyphs.UsingPad;
            if (usingPad != PadMode) PadMode = usingPad;
            var actions = new List<string>();
            long now = RealTime.Ms;

            // up / down / left / right, repeating while held (pad: d-pad or left stick)
            string want = null;
            if (usingPad)
            {
                float lx = Normal(195), ly = Normal(196);
                want = PadPressed(188) || ly < -0.6f ? "up" : PadPressed(187) || ly > 0.6f ? "down"
                    : PadPressed(189) || lx < -0.6f ? "left" : PadPressed(190) || lx > 0.6f ? "right" : null;
            }
            else
            {
                want = Game.IsKeyPressed(Keys.Up) || Game.IsKeyPressed(Keys.W) ? "up"
                    : Game.IsKeyPressed(Keys.Down) || Game.IsKeyPressed(Keys.S) ? "down"
                    : Game.IsKeyPressed(Keys.Left) || Game.IsKeyPressed(Keys.A) ? "left"
                    : Game.IsKeyPressed(Keys.Right) || Game.IsKeyPressed(Keys.D) ? "right" : null;
            }
            if (want != _input.Direction)
            {
                _input.Direction = want;
                _input.NextRepeat = now + 380;
                if (want != null) actions.Add(want);
            }
            else if (want != null && now >= _input.NextRepeat)
            {
                _input.NextRepeat = now + (usingPad ? 130 : 80);
                actions.Add(want);
            }

            bool rev;
            if (usingPad)
            {
                if (PadJust(201)) actions.Add("ok");
                if (PadJust(202)) actions.Add("back");
                if (PadJust(204)) actions.Add("card");
                if (PadJust(205)) actions.Add("prev");
                if (PadJust(206)) actions.Add("next");
                rev = PadPressed(203);
                // right stick looks round the car, the triggers zoom
                float rx = Normal(197), ry = Normal(198);
                float zoom = Normal(208) - Normal(207);
                if (Math.Abs(rx) > 0.15f) _camera.Orbit(-rx * 150f * dt, 0f, 0f);
                if (Math.Abs(ry) > 0.15f) _camera.Orbit(0f, -ry * 80f * dt, 0f);
                if (Math.Abs(zoom) > 0.1f) _camera.Orbit(0f, 0f, 1f - zoom * 1.2f * dt);
            }
            else
            {
                if (KeyJust(Keys.Enter) || KeyJust(Keys.Space)) actions.Add("ok");
                if (KeyJust(Keys.Back) || KeyJust(Keys.Escape)) actions.Add("back");
                if (KeyJust(Keys.Tab)) actions.Add("card");
                if (KeyJust(Keys.Q)) actions.Add("prev");
                if (KeyJust(Keys.E)) actions.Add("next");
                rev = Game.IsKeyPressed(Keys.R);
                // drag with a mouse button to look round the car, the wheel zooms
                if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, 24) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, 25))
                {
                    float mx = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, 1), my = Function.Call<float>(Hash.GET_DISABLED_CONTROL_NORMAL, 0, 2);
                    _camera.Orbit(-mx * 60f, my * 40f, 0f);
                }
                if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, 241)) _camera.Orbit(0f, 0f, 0.9f);
                if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, 242)) _camera.Orbit(0f, 0f, 1.1f);
            }
            Rev(rev && Dialog == null);
            foreach (string a in actions)
            {
                Act(a);
                if (_closed) return;
            }
        }

        private void Rev(bool on)
        {
            if (on == _revving) return;
            _revving = on;
            _rpm = 0.2f;
        }

        // =============================================================================
        // State (app.js)
        // =============================================================================

        public string StatusOf(string slot, CustomsOption option)
        {
            if (option.Value == null) return "none";
            if (Logic.Owned.TryGetValue(slot, out string owned) && option.Value == owned) return "applied";
            return option.Stock ? "stock" : "none";
        }

        public string KitStatus(string id) => Logic.Owned.TryGetValue("kit", out string k) && k == id ? "applied" : "none";

        private string OptionLabel(CustomsSlot slot, string value)
        {
            foreach (CustomsOption o in slot.Options) if (o.Value == value) return o.Label;
            return slot.Kind == "wheels" ? "Custom Wheels" : "Custom";
        }

        private CustomsOption Current(MenuView v)
        {
            if (v.Type == "item") return v.Options[v.Sel];
            if (v.Type == "swatch") return v.Groups[v.G].Options[v.Sel];
            return null;
        }

        internal struct Row { public string Label, Right; public bool Have; }

        public List<Row> Rows(MenuView v, out int sel)
        {
            sel = v.Sel;
            var rows = new List<Row>();
            if (v.Type == "root")
            {
                foreach (CustomsCategory c in Tab.Categories) rows.Add(new Row { Label = c.Label });
                sel = v.RootSel[v.Tab];
            }
            else if (v.Type == "slots")
            {
                foreach (string id in v.Category.Slots)
                {
                    CustomsSlot s = Logic.Slots[id];
                    Logic.Owned.TryGetValue(id, out string owned);
                    rows.Add(new Row { Label = s.Label, Right = OptionLabel(s, owned) });
                }
            }
            else if (v.Type == "groups")
            {
                Logic.Owned.TryGetValue(v.Slot.Id, out string owned);
                string current = (owned ?? "").Split(':')[0];
                foreach (WheelGroup g in v.Slot.Groups) rows.Add(new Row { Label = g.Label, Have = CustomsCar.Str(g.Id) == current });
            }
            else return null;
            return rows;
        }

        public string Hint(MenuView v) => v.Type == "root" ? Tab.Categories[v.RootSel[v.Tab]].Hint : v.Type == "slots" ? v.Category.Hint : "";

        public List<(string Label, string Option)> Fitted()
        {
            var rows = new List<(string, string)>();
            foreach (var kv in Logic.Slots)
            {
                CustomsOption stock = kv.Value.Options.Find(o => o.Stock);
                Logic.Owned.TryGetValue(kv.Key, out string owned);
                if (stock != null && owned != stock.Value) rows.Add((kv.Value.Label, OptionLabel(kv.Value, owned)));
            }
            return rows;
        }

        private bool HasDelta(CustomsStats s)
        {
            foreach (CustomsStat b in s.Bars) if (Math.Abs(b.Value - b.Owned) >= 0.05f) return true;
            return false;
        }

        public string CardPage()
        {
            if (CardOverride != null) return CardOverride;
            return Tab.Id == "performance" || Top.Type == "kits" || HasDelta(Shown) ? "stats" : "name";
        }

        public List<(string Label, string Act, bool Hold)> PromptList()
        {
            MenuView v = Top;
            var list = new List<(string, string, bool)>();
            bool engine = Tab.Id == "performance" && !(v.Slot != null && v.Slot.Id == "nitrous");
            if (v.Type == "kits" || (engine && (v.Type == "root" || v.Type == "item"))) list.Add(("Rev", "rev", false));
            if (!v.Leaf) list.Add(("Select", "ok", false));
            else if (v.Type == "kits") { if (KitStatus(Logic.Kits[v.Sel].Id) != "applied") list.Add(("Buy", "ok", false)); }
            else
            {
                CustomsOption o = Current(v);
                if (o != null && StatusOf(v.Slot.Id, o) != "applied") list.Add(("Buy", "ok", false));
            }
            list.Add(Stack.Count > 1 ? ("Back", "back", false) : ("Exit", "back", true));
            return list;
        }

        // =============================================================================
        // Views (openSlot / openCategory / openGroup / push / enter)
        // =============================================================================

        private void Push(MenuView v)
        {
            Stack.Add(v);
            Enter();
        }

        // camera and screen for the view that just came to the front
        private string _shownView;

        private void Enter()
        {
            MenuView v = Top;
            string view = v.Type == "root" ? Tab.Camera : v.Camera ?? Tab.Camera;
            if (view == _shownView) return;
            _shownView = view;
            _camera.SetView(view);
        }

        private void OpenSlot(string id, string crumb, string title, string camera)
        {
            CustomsSlot slot = Logic.Slots[id];
            var v = new MenuView { Slot = slot, Crumb = crumb, Title = title, Camera = slot.Camera ?? camera };
            Logic.Owned.TryGetValue(id, out string owned);
            if (slot.Kind == "swatch")
            {
                v.Type = "swatch";
                v.Groups = CustomsLogic.SwatchGroups(slot);
                for (int g = 0; g < v.Groups.Count; g++)
                    for (int i = 0; i < v.Groups[g].Options.Count; i++)
                        if (v.Groups[g].Options[i].Value != null && v.Groups[g].Options[i].Value == owned) { v.G = g; v.Sel = i; }
            }
            else if (slot.Kind == "wheels")
            {
                v.Type = "groups";
                string current = (owned ?? "").Split(':')[0];
                v.Sel = Math.Max(0, slot.Groups.FindIndex(g => CustomsCar.Str(g.Id) == current));
            }
            else
            {
                v.Type = "item";
                v.Options = slot.Options;
                v.Sel = Math.Max(0, slot.Options.FindIndex(o => o.Value == owned));
            }
            Push(v);
        }

        private void OpenCategory(CustomsCategory c)
        {
            string crumb = Tab.Label;
            string camera = c.Camera ?? Tab.Camera;
            if (c.Kits)
            {
                Logic.Owned.TryGetValue("kit", out string kit);
                Push(new MenuView { Type = "kits", Sel = Math.Max(0, Logic.Kits.FindIndex(k => k.Id == kit)), Crumb = crumb, Title = c.Label, Camera = camera });
                QueuePreview();
            }
            else if (c.Slots.Count == 1) OpenSlot(c.Slots[0], crumb, c.Label, camera);
            else Push(new MenuView { Type = "slots", Category = c, Sel = 0, Crumb = crumb, Title = c.Label, Camera = camera });
        }

        private void OpenGroup(MenuView v)
        {
            WheelGroup group = v.Slot.Groups[v.Sel];
            List<CustomsOption> options = Logic.WheelOptions(group.Id);
            Logic.Apply("wheels", Logic.Owned);
            if (options.Count == 0) return;
            Logic.Owned.TryGetValue(v.Slot.Id, out string owned);
            Push(new MenuView { Type = "item", Slot = v.Slot, Options = options, Sel = Math.Max(0, options.FindIndex(o => o.Value == owned)), Crumb = v.Title, Title = group.Label, Camera = v.Camera });
        }

        // =============================================================================
        // Preview, buy, back (preview / buy / select / back / move / shift)
        // =============================================================================

        private void QueuePreview() => _previewAt = RealTime.Ms + 70;

        private void Preview()
        {
            MenuView v = Top;
            if (v.Type == "kits")
            {
                CustomsKit kit = Logic.Kits[v.Sel];
                // a kit that was being looked at comes off before the next one goes on
                if (_previewKit != null && Logic.KitParts.TryGetValue(_previewKit, out var was))
                    foreach (var kv in was) Logic.Apply(kv.Key, Logic.Owned);
                _previewKit = kit.Id;
                var view = new Dictionary<string, string>(Logic.Owned);
                foreach (var kv in Logic.KitParts[kit.Id]) view[kv.Key] = kv.Value;
                foreach (var kv in Logic.KitParts[kit.Id]) Logic.Apply(kv.Key, view);
                Shown = Logic.Stats(view);
                return;
            }
            CustomsOption o = Current(v);
            if (o == null || o.Value == null) return;
            Dictionary<string, string> preview = Logic.ViewWith(v.Slot.Id, o.Value);
            Logic.Apply(v.Slot.Id, preview);
            Shown = Logic.Stats(preview);
        }

        private void Purchased()
        {
            OwnedStats = Shown = Logic.Stats(Logic.Owned);
            ToastAt = RealTime.Ms;
        }

        private bool Pay(int price)
        {
            if (price <= 0) return true;
            if (Cash.Money < price)
            {
                Ask("Payment declined", new List<string> { "Not enough money, $" + price.ToString(CultureInfo.InvariantCulture) + " required." }, () => Dialog = null, null);
                return false;
            }
            Cash.Money -= price;
            return true;
        }

        private void Buy()
        {
            MenuView v = Top;
            if (v.Type == "kits")
            {
                CustomsKit kit = Logic.Kits[v.Sel];
                if (KitStatus(kit.Id) == "applied") return;
                Sfx.Play("click");
                if (!Pay(kit.Price)) return;
                var changes = new Dictionary<string, string>(Logic.KitParts[kit.Id]) { ["kit"] = kit.Id };
                Logic.Commit(changes);
                Log.Write("Customs", "bought " + kit.Label + " for $" + kit.Price);
                Purchased();
                return;
            }
            CustomsOption o = Current(v);
            if (o == null) return;
            Logic.Owned.TryGetValue(v.Slot.Id, out string owned);
            if (o.Value == owned) return;   // already on the vehicle
            Sfx.Play("click");
            if (!Pay(o.Price)) return;
            var change = new Dictionary<string, string> { [v.Slot.Id] = o.Value };
            if (CustomsLogic.PaintLinks.TryGetValue(v.Slot.Id, out string link)) change[link] = "off";
            Logic.Commit(change);
            Log.Write("Customs", "bought " + v.Slot.Label + ": " + o.Label + " for $" + o.Price);
            Purchased();
        }

        private void Select()
        {
            MenuView v = Top;
            if (v.Type == "root") { Sfx.Play("click"); OpenCategory(Tab.Categories[v.RootSel[v.Tab]]); }
            else if (v.Type == "slots") { Sfx.Play("click"); string id = v.Category.Slots[v.Sel]; OpenSlot(id, v.Title, Logic.Slots[id].Label, v.Camera); }
            else if (v.Type == "groups") { Sfx.Play("click"); OpenGroup(v); }
            else Buy();
        }

        private void Back()
        {
            MenuView v = Top;
            if (Stack.Count == 1) { Close(true); return; }
            Stack.RemoveAt(Stack.Count - 1);
            _previewAt = -1;
            Shown = Logic.Stats(Logic.Owned);
            if (v.Type == "item" || v.Type == "swatch") Logic.Apply(v.Slot.Id, Logic.Owned);
            if (v.Type == "kits" && _previewKit != null && Logic.KitParts.TryGetValue(_previewKit, out var parts))
            {
                foreach (var kv in parts) Logic.Apply(kv.Key, Logic.Owned);
                _previewKit = null;
            }
            Enter();
        }

        private void Move(int delta)
        {
            MenuView v = Top;
            int count = 0;
            if (v.Type == "root") count = Tab.Categories.Count;
            else if (v.Type == "slots") count = v.Category.Slots.Count;
            else if (v.Type == "groups") count = v.Slot.Groups.Count;
            else if (v.Type == "item") count = v.Options.Count;
            else if (v.Type == "kits") count = Logic.Kits.Count;
            else if (v.Type == "swatch") count = v.Groups[v.G].Options.Count;
            if (count < 2) return;
            int now = v.Type == "root" ? v.RootSel[v.Tab] : v.Sel;
            int next = (now + delta + count) % count;
            if (v.Type == "root") v.RootSel[v.Tab] = next; else v.Sel = next;
            Sfx.Play("menu");
            if (v.Leaf) QueuePreview();
        }

        // tabs of the root view, colour groups of a swatch view
        private void Shift(int delta)
        {
            MenuView v = Top;
            if (v.Type == "root" && Logic.Tabs.Count > 1)
            {
                v.Tab = (v.Tab + delta + Logic.Tabs.Count) % Logic.Tabs.Count;
                CardOverride = null;
                Sfx.Play("menu");
                Enter();
            }
            else if (v.Type == "swatch" && v.Groups.Count > 1)
            {
                v.G = (v.G + delta + v.Groups.Count) % v.Groups.Count;
                v.Sel = 0;
                v.Offset = 0;
                Sfx.Play("menu");
                QueuePreview();
            }
        }

        private void Act(string action)
        {
            if (Dialog != null)
            {
                if (action == "ok" || action == "back") Log.Write("Customs", "\"" + Dialog.Title + "\": " + action);
                if (action == "ok") { Sfx.Play("click"); Dialog.Ok(); }
                else if (action == "back") (Dialog.Back ?? Dialog.Ok)();
                return;
            }
            MenuView v = Top;
            switch (action)
            {
                case "up": if (v.Type == "swatch") Shift(-1); else Move(-1); break;
                case "down": if (v.Type == "swatch") Shift(1); else Move(1); break;
                case "left": if (v.Leaf) Move(-1); else Shift(-1); break;
                case "right": if (v.Leaf) Move(1); else Shift(1); break;
                case "prev": Shift(-1); break;
                case "next": Shift(1); break;
                case "ok": Select(); break;
                case "back": Back(); break;
                case "card":
                    {
                        var pages = new[] { "name", "stats", "parts" };
                        CardOverride = pages[(Array.IndexOf(pages, CardPage()) + 1) % pages.Length];
                        break;
                    }
            }
        }

        // =============================================================================
        // Dialogs (ask / repairDialog)
        // =============================================================================

        private void Ask(string title, List<string> lines, Action ok, Action back)
        {
            Dialog = new DialogState { Title = title, Lines = lines, Ok = ok, Back = back };
            Log.Write("Customs", "asks: " + title);
        }

        private void RepairDialog()
        {
            int cost = CustomsData.FreeUpgradeToClass.Contains((int)_veh.ClassType) ? 0 : CustomsData.RepairCost;
            var lines = new List<string> { "You'll need to repair this vehicle before making modifications." };
            Ask("Repair for " + (cost > 0 ? "$" + cost.ToString("N0", CultureInfo.InvariantCulture) : "free") + "?", lines, () =>
            {
                if (Cash.Money < cost)
                {
                    Ask("Repair declined", new List<string> { "You need $" + CustomsData.RepairCost.ToString("N0", CultureInfo.InvariantCulture) + " to repair this vehicle." }, () => Close(true), null);
                    return;
                }
                Cash.Money -= cost;
                _veh.Repair();
                Function.Call(Hash.SET_VEHICLE_DIRT_LEVEL, _veh, 0f);
                _veh.BodyHealth = 1000f;
                _veh.EngineHealth = 1000f;
                _veh.PetrolTankHealth = 1000f;
                Function.Call(Hash.SET_VEHICLE_ENGINE_ON, _veh, true, true, false);
                Dialog = null;
                Enter();
            }, () => Close(true));
        }
    }

    /// <summary>
    /// GTA6Hud's HUD out of the way while the menu is up, as vice_hud hides
    /// itself for the shop: its status bars, interaction markers,
    /// notifications, vehicle card, area plate, police arrows and the 3D
    /// minimap. Its money and wanted stars stay: the menu shows no money itself.
    /// </summary>
    internal sealed class Gta6HudQuiet
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly List<(FieldInfo Field, object Target, bool Was)> _held = new List<(FieldInfo, object, bool)>();
        private bool _on;

        public void Hold(bool on)
        {
            if (on == _on) return;
            _on = on;
            HudBridge.HoldMinimapHidden(on);
            HudBridge.HoldGta6HudPanelsHidden(on);
            if (on)
            {
                _held.Clear();
                object hud = HudBridge.Gta6Hud();
                Type refs = null;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    if ((refs = asm.GetType("GTA6Hud.Ref", false)) != null) break;
                Off(hud?.GetType().GetField("_barsOn", Own), hud);
                Off(hud?.GetType().GetField("_markersOn", Own), hud);
                Off(refs?.GetField("NotificationsEnabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), null);
            }
            else
            {
                foreach (var h in _held) try { h.Field.SetValue(h.Target, h.Was); } catch { }
                _held.Clear();
            }
        }

        private void Off(FieldInfo f, object target)
        {
            if (f == null || f.FieldType != typeof(bool)) return;
            try
            {
                _held.Add((f, target, (bool)f.GetValue(target)));
                f.SetValue(target, false);
            }
            catch { }
        }
    }

    /// <summary>The shop page's sounds (html/audio click / menu), at its volumes.</summary>
    internal static class Sfx
    {
        [DllImport("winmm.dll", CharSet = CharSet.Unicode)]
        private static extern bool PlaySound(string file, IntPtr module, uint flags);
        private const uint Async = 0x0001, NoDefault = 0x0002, FileName = 0x00020000;

        public static void Play(string name)
        {
            try
            {
                string path = Path.Combine(Paths.Scripts, "ViCustoms", name + ".wav");
                if (File.Exists(path)) PlaySound(path, IntPtr.Zero, Async | NoDefault | FileName);
            }
            catch { }
        }
    }
}
