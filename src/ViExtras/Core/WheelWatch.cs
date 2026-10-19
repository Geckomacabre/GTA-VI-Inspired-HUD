using System;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Writes down why GTA6Hud's weapon wheel stayed shut while its button was
    /// held (reported: "sometimes the wheel doesn't show"). GTA6Hud opens it after
    /// 180 ms on Tab / LB on foot, or [Wheel] VehicleOpenControl in a vehicle
    /// (Tab too on a keyboard), unless an item is being used (_consumeIndex), the
    /// trunk, its settings or its own radio wheel is open, or the player is dead.
    /// Once a press, after 600 ms: those, and what else had the keys. Nothing is
    /// changed.
    /// </summary>
    public sealed class WheelWatch : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int SelectWeapon = 37;

        private bool _looked;
        private FieldInfo _open, _consume, _trunk, _settings, _radio, _vehicleControl;
        private PropertyInfo _radioOpen;
        private long _heldSince;
        private bool _logged;

        public WheelWatch()
        {
            Paths.Init(BaseDirectory);
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now || Game.IsPaused) { _heldSince = 0; return; }
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !Look(hud)) return;
            try
            {
                Ped player = Game.Player.Character;
                bool inVehicle = player.IsInVehicle();
                int control = inVehicle ? (int)_vehicleControl.GetValue(null) : SelectWeapon;
                bool held = Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, control)
                    || (inVehicle && !Glyphs.UsingPad && Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, SelectWeapon));
                if (!held) { _heldSince = 0; _logged = false; return; }
                long now = RealTime.Ms;
                if (_heldSince == 0) _heldSince = now;
                if (_logged || now - _heldSince < 600 || (bool)_open.GetValue(hud)) return;
                _logged = true;
                object radio = _radio?.GetValue(hud);
                Log.Write("WheelWatch", "wheel button held 0.6 s and GTA6Hud's wheel is shut: "
                    + (inVehicle ? "in a vehicle, its open control " + control : "on foot")
                    + ", item in use " + ((int)_consume.GetValue(hud) >= 0)
                    + ", trunk " + _trunk.GetValue(hud)
                    + ", settings " + _settings.GetValue(hud)
                    + ", its radio " + (radio != null && _radioOpen != null && (bool)_radioOpen.GetValue(radio))
                    + ", dead " + player.IsDead
                    + ", in the way: " + (HudBridge.InTheWay() ?? "nothing"));
            }
            catch (Exception ex)
            {
                Log.Write("WheelWatch", ex.Message);
                _open = null;
            }
        }

        private bool Look(object hud)
        {
            if (_looked) return _open != null;
            _looked = true;
            Type t = hud.GetType();
            _open = t.GetField("_wheelOpen", Own);
            _consume = t.GetField("_consumeIndex", Own);
            _trunk = t.GetField("_trunkOpen", Own);
            _settings = t.GetField("_settingsOpen", Own);
            _radio = t.GetField("_radio", Own);
            _radioOpen = _radio?.FieldType.GetProperty("IsOpen", Own);
            _vehicleControl = t.Assembly.GetType("GTA6Hud.Ref")?.GetField("WheelVehicleOpenControl", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (_open == null || _consume == null || _trunk == null || _settings == null || _vehicleControl == null)
            {
                _open = null;
                Log.Write("WheelWatch", "GTA6Hud's wheel fields were not found; not watching");
            }
            return _open != null;
        }
    }
}
