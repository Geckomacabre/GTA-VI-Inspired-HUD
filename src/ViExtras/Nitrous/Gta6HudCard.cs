using System;
using System.Drawing;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's vehicle card, drawn from here for a car that has a gauge GTA6Hud
    /// knows nothing about (nitrous).
    ///
    /// GTA6Hud centres its gauge row on the gauges it draws itself, so a gauge
    /// added after the row from outside sits off centre. While Begin holds,
    /// GTA6Hud's own card is switched off ([VehicleCard] setting, the same one
    /// its options menu flips) and Draw lays the card out exactly as its
    /// DrawVehicleCard does, with GTA6Hud's own methods (background, brand
    /// emblem, make and model in its font, engine and fuel gauges), and one more
    /// slot in the row. GTA6Hud's route gauge (a waypoint is set) is left out:
    /// beside the nitrous gauge it read as a second one, both a double chevron,
    /// and vice_hud's card has none (engine, fuel, nitrous). Nothing of GTA6Hud's is changed or copied; End hands
    /// the card back mid-drive without restarting it.
    ///
    /// GTA6Hud's DrawVehicleCard still runs every frame its card would be up, and
    /// the first thing it does is clear _vehCardTop. Draw leaves -1 there and
    /// only draws when it has been cleared since: so the card shows exactly when
    /// GTA6Hud would have shown it (not in its menus, kill cam, pause, ...).
    /// </summary>
    internal static class Gta6HudCard
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const float Pitch = 38f;    // DrawVehicleGauges: gauge centres 38 px apart

        private static bool _looked;
        private static object _hud;
        private static MethodInfo _kitReady, _stackWidth, _stackLeft, _cardBottom, _make, _brand, _tex, _text, _gauge, _gaugeColour, _fuel;
        private static FieldInfo _cardTop, _fullFor, _lastHandle, _assetsReady, _fullSeconds, _idle;
        private static int _topFrame = -1;
        private static float _topThisFrame;
        private static object[] _textTail;   // KitText's optional anchor and tracking, at their defaults

        private static bool _held;
        private static int _handle;
        private static float _fullLeft;

        private static bool Look()
        {
            if (_hud != null) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            try
            {
                Type t = hud.GetType();
                Assembly asm = t.Assembly;
                MethodInfo M(string name) => t.GetMethod(name, Own | BindingFlags.Static);
                FieldInfo F(string name) => t.GetField(name, Own);
                _kitReady = M("KitReady");
                _stackWidth = M("KitStackWidth");
                _stackLeft = M("KitStackLeft");
                _cardBottom = M("KitCardBottom");
                _make = M("VehicleMake");
                _brand = M("BrandTexture");
                _tex = M("PmTexAt");
                _text = M("KitText");
                _gauge = M("DrawKitGauge");
                _gaugeColour = M("KitGaugeColour");
                _fuel = M("FuelFraction");
                _cardTop = F("_vehCardTop");
                _fullFor = F("_vehCardFullFor");
                _lastHandle = F("_lastVehicleHandle");
                _assetsReady = F("_assetsReady");
                _fullSeconds = asm.GetType("GTA6Hud.Ref")?.GetField("VehCardFullSeconds", Static);
                _idle = asm.GetType("GTA6Hud.RefColor")?.GetField("VehGaugeIdle", Static);
                if (_text != null)
                {
                    ParameterInfo[] p = _text.GetParameters();
                    _textTail = new object[p.Length - 9];
                    for (int i = 9; i < p.Length; i++) _textTail[i - 9] = p[i].DefaultValue;
                }
                bool all = _kitReady != null && _stackWidth != null && _stackLeft != null && _cardBottom != null && _make != null
                    && _brand != null && _tex != null && _text != null && _gauge != null && _gaugeColour != null && _fuel != null
                    && _cardTop != null && _fullFor != null && _lastHandle != null && _assetsReady != null && _fullSeconds != null && _idle != null;
                if (all) _hud = hud;
                else Log.Write("Gta6HudCard", "GTA6Hud’s vehicle card methods were not all found; nitrous goes after its gauge row instead");
            }
            catch (Exception ex) { Log.Write("Gta6HudCard", ex.Message); }
            return _hud != null;
        }

        /// <summary>
        /// Takes the card over for `veh`, carrying on GTA6Hud's own "full card"
        /// countdown if it was already showing this car. False if GTA6Hud's card
        /// cannot be drawn from here (nothing is held then).
        /// </summary>
        public static bool Begin(Vehicle veh)
        {
            if (_held) return true;
            if (!Look()) return false;
            _handle = veh.Handle;
            _fullLeft = (int)_lastHandle.GetValue(_hud) == _handle ? (float)_fullFor.GetValue(_hud) : (float)_fullSeconds.GetValue(null);
            _cardTop.SetValue(_hud, -1f);
            HudBridge.HoldVehicleCardOff(true);
            _held = true;
            return true;
        }

        /// <summary>Gives the card back to GTA6Hud, where it left off.</summary>
        public static void End()
        {
            if (!_held) return;
            _held = false;
            HudBridge.HoldVehicleCardOff(false);
            try
            {
                _lastHandle.SetValue(_hud, _handle);
                _fullFor.SetValue(_hud, _fullLeft);
            }
            catch { }
        }

        /// <summary>
        /// GTA6Hud DrawVehicleCard for `veh`, with `extra` more gauges at the end
        /// of the row. Returns false when no card is up this frame; otherwise the
        /// centre of the first extra slot, for the caller to draw its gauge in.
        /// </summary>
        public static bool Draw(Vehicle veh, int extra, out float slotX, out float slotY)
        {
            slotX = slotY = 0f;
            if (!_held || _hud == null || veh == null || !veh.Exists()) return false;
            try
            {
                float top0 = CardTopThisFrame();
                // GTA6Hud put its own card back on (it does when it reloads): off again.
                HudBridge.ReassertVehicleCardOff();
                // 0: GTA6Hud reached its card this frame and, held off, drew nothing.
                // Below 0 it did not get to it; above 0 it drew its own (switched
                // back on): either way no card from here, so there is never two.
                if (top0 != 0f) return false;
                if (!HudBridge.VehicleCardWanted || HudBridge.Gta6HudPanelsHeld) return false;
                // a GTA6Hud shop or camera, or Leonida Customs' menu, has the screen
                if (HudBridge.HudMenuOpen) return false;
                if (!(bool)_assetsReady.GetValue(_hud)) return false;

                if (_fullLeft > 0f) _fullLeft -= Game.LastFrameTime;
                bool full = _fullLeft > 0f;
                bool gauges = !veh.Model.IsBicycle;
                if ((!full && !gauges) || !(bool)_kitReady.Invoke(_hud, null)) return false;

                float w = (float)_stackWidth.Invoke(_hud, null);
                float left = (float)_stackLeft.Invoke(_hud, null);
                float cx = left + w * 0.5f;
                float bottom = (float)_cardBottom.Invoke(_hud, null);
                string text = "", text2 = "", make = "";
                if (full)
                {
                    text = (string)_make.Invoke(_hud, new object[] { veh }) ?? "";
                    make = text;
                    text2 = veh.LocalizedName;
                    if (string.IsNullOrEmpty(text2) || text2 == "NULL") text2 = veh.DisplayName;
                    text2 = (text2 ?? "VEHICLE").ToUpperInvariant();
                    if (text.Length == 0 || text2.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                    {
                        text = text2;
                        text2 = "";
                    }
                }
                float h = full ? 110f : 36.7f;
                float top = bottom - h;
                if (full)
                {
                    Tex("hud_vehinfo_bg", left, top, w, 110f, Color.White);
                    string brand = (string)_brand.Invoke(_hud, new object[] { make });
                    if (brand != null) Tex(brand, cx - 60f, top + 36.6f - 60f, 120f, 120f, Color.FromArgb(70, 255, 255, 255));
                    Text(text, 28f, left, top + 4f, w, Color.White);
                    if (text2.Length > 0) Text(text2, 27f, left, top + 33f, w, Color.FromArgb(186, 255, 255, 255));
                }
                if (!gauges) return false;

                // DrawVehicleGauges without its route gauge, with the extra slots counted in the centring.
                float cy = top + h - 18.35f;
                int n = 2 + extra;
                float x0 = cx - Pitch * (n - 1) * 0.5f;
                int i = 0;
                bool running = veh.IsEngineRunning;
                Color idle = (Color)_idle.GetValue(null);
                float engine = Num.Clamp01(veh.EngineHealth / 1000f);
                float fuel = (float)_fuel.Invoke(_hud, new object[] { veh });
                Gauge(x0 + Pitch * i++, cy, engine, "hud_gauge_engine", 14.4f, 9.3f, running ? GaugeColour(engine) : idle);
                Gauge(x0 + Pitch * i++, cy, fuel, "hud_gauge_fuel", 12.4f, 13.3f, running ? GaugeColour(fuel) : idle);
                slotX = x0 + Pitch * i;
                slotY = cy;
                return extra > 0;
            }
            catch (Exception ex)
            {
                Log.Write("Gta6HudCard", ex.Message);
                return false;
            }
        }

        /// <summary>GTA6Hud PmTexAt: a picture from its vipause dictionary, canvas px.</summary>
        /// <summary>
        /// GTA6Hud's _vehCardTop as its DrawVehicleCard left it this frame: 0 or
        /// more when it got to its card, -1 when it did not. Read once a frame and
        /// left at -1 for the next frame; nothing in GTA6Hud reads it.
        /// </summary>
        private static float CardTopThisFrame()
        {
            int frame = Game.FrameCount;
            if (frame != _topFrame)
            {
                _topFrame = frame;
                _topThisFrame = (float)_cardTop.GetValue(_hud);
                _cardTop.SetValue(_hud, -1f);
            }
            return _topThisFrame;
        }

        public static void Tex(string name, float x, float y, float w, float h, Color c) =>
            _tex.Invoke(_hud, new object[] { name, x, y, w, h, c });

        private static void Text(string s, float size, float x, float boxTop, float boxW, Color c)
        {
            var args = new object[9 + _textTail.Length];
            args[0] = s; args[1] = 'C'; args[2] = size; args[3] = x; args[4] = boxTop; args[5] = boxW; args[6] = 30f; args[7] = c; args[8] = 1;
            Array.Copy(_textTail, 0, args, 9, _textTail.Length);
            _text.Invoke(_hud, args);
        }

        private static void Gauge(float cx, float cy, float fraction, string icon, float iw, float ih, Color c) =>
            _gauge.Invoke(_hud, new object[] { cx, cy, fraction, icon, iw, ih, c });

        private static Color GaugeColour(float fraction) => (Color)_gaugeColour.Invoke(null, new object[] { fraction });
    }
}
