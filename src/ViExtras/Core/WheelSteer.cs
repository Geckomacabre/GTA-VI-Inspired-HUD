using System;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// GTA V's weapon wheel on a mouse: no pointer on screen, nudge the mouse toward a weapon and it lights up.
    ///
    /// GTA6Hud's wheel already steers like that on a controller: it keeps a pointer (_pointerX / _pointerY) from the
    /// stick, or from the mouse's movement when the stick is still, and on a pad lights the slot in its direction
    /// (SetHighlightByRing, a ring per 45 degrees). On mouse and keyboard it shows Windows' cursor instead
    /// ([Wheel] MouseEnabled), which starts wherever it last was, often the top-left corner, and lights the slot nearest
    /// to it. So on mouse and keyboard its Ref.WheelMouseEnabled is held off here (set every frame: its settings reload
    /// turns it back on; put back when a pad is used), and while the wheel is open the slot in the pointer's direction
    /// is lit, as the stick does. Holding it off before the wheel opens keeps the cursor from picking a slot on the
    /// first frame. ViExtras.ini [WeaponWheel] MouseSteer = False keeps GTA6Hud's cursor.
    /// </summary>
    public sealed class WheelSteer : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Shared = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const float Reach = 0.5f;   // how far the pointer leans before a direction counts (GTA6Hud's stick: 0.2)
        private const int GiveUpMs = 60000; // GTA6Hud not found by then: stop looking

        private bool _looked, _held, _heldWas;
        private long _lookUntil = -1;
        private object _hud;
        private FieldInfo _open, _px, _py, _mouseOn;
        private MethodInfo _byRing;

        public WheelSteer()
        {
            Paths.Init(this);
            if (!Config.Get("WeaponWheel", "MouseSteer", true)) return;
            Tick += OnTick;
            Aborted += (s, e) => Release();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!Look()) return;
            if (Game.LastInputMethod == InputMethod.GamePad) { Release(); return; }   // the stick steers it already
            Hold();
            bool open;
            try { open = (bool)_open.GetValue(_hud); }
            catch { return; }
            if (!open) return;
            float x = (float)_px.GetValue(_hud), y = (float)_py.GetValue(_hud);
            if (x * x + y * y < Reach * Reach) return;
            double deg = Math.Atan2(x, -y) * 180.0 / Math.PI;
            if (deg < 0.0) deg += 360.0;
            try { _byRing.Invoke(_hud, new object[] { (int)Math.Round(deg / 45.0) % 8 }); }
            catch { }
        }

        // GTA6Hud's wheel and its mouse switch, found once; nothing happens if any of them is missing.
        private bool Look()
        {
            if (_looked) return _hud != null;
            if (_lookUntil < 0) _lookUntil = RealTime.Ms + GiveUpMs;
            object hud = HudBridge.Gta6Hud();
            if (hud == null)
            {
                if (RealTime.Ms > _lookUntil) _looked = true;
                return false;
            }
            _looked = true;
            Type t = hud.GetType(), refs = t.Assembly.GetType("GTA6Hud.Ref", false);
            _open = t.GetField("_wheelOpen", Own);
            _px = t.GetField("_pointerX", Own);
            _py = t.GetField("_pointerY", Own);
            _byRing = t.GetMethod("SetHighlightByRing", Own, null, new[] { typeof(int) }, null);
            _mouseOn = refs?.GetField("WheelMouseEnabled", Shared);
            bool ok = _open?.FieldType == typeof(bool) && _px?.FieldType == typeof(float) && _py?.FieldType == typeof(float)
                      && _byRing != null && _mouseOn?.FieldType == typeof(bool);
            Log.Write("WheelSteer", ok ? "found GTA6Hud's wheel; a mouse steers it like GTA V's" : "GTA6Hud's wheel isn't as expected; its cursor stays");
            if (ok) _hud = hud;
            return ok;
        }

        private void Hold()
        {
            try
            {
                if (!_held) { _heldWas = (bool)_mouseOn.GetValue(null); _held = true; }
                _mouseOn.SetValue(null, false);
            }
            catch { }
        }

        private void Release()
        {
            if (!_held) return;
            try { _mouseOn.SetValue(null, _heldWas); }
            catch { }
            _held = false;
        }
    }
}
