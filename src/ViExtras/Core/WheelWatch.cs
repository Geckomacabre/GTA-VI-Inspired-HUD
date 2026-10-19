using System;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Keeps GTA6Hud's weapon wheel opening, and writes down why it didn't.
    ///
    /// GTA6Hud only runs its wheel on frames where its weapon pickup prompt is not
    /// up, and that prompt is up whenever a dropped weapon is in view within
    /// [Interact] PickupPromptRange (6 m), "FULL" ones too. Its own carry limit
    /// drops the weapons over it at your feet, so giving yourself weapons or
    /// picking one up left the wheel dead until you walked away. While the wheel
    /// button is held on foot (and while the wheel is open) its
    /// Ref.PickupPromptEnabled is held off here, set again every frame (its
    /// settings reload turns it back on), and put back after: the prompt comes
    /// back when you let go. [WeaponWheel] OverPickups = False leaves it alone.
    ///
    /// GTA6Hud opens the wheel after 180 ms on Tab / LB on foot, or [Wheel]
    /// VehicleOpenControl in a vehicle (Tab too on a keyboard), unless an item is
    /// being used (_consumeIndex), the trunk, its settings or its own radio wheel
    /// is open, or the player is dead. Once a press, if it is still shut after
    /// 600 ms: those, and what else had the keys, go to the log.
    /// </summary>
    public sealed class WheelWatch : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int SelectWeapon = 37;

        private readonly bool _overPickups;
        private bool _looked;
        private FieldInfo _open, _consume, _trunk, _settings, _radio, _vehicleControl, _pickupPrompt;
        private PropertyInfo _radioOpen;
        private long _heldSince;
        private bool _logged;
        private bool _promptHeld, _promptWas;

        public WheelWatch()
        {
            Paths.Init(BaseDirectory);
            _overPickups = Config.Get("WeaponWheel", "OverPickups", true);
            Tick += OnTick;
            Aborted += (s, e) => HoldPrompt(false);
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now || Game.IsPaused) { _heldSince = 0; HoldPrompt(false); return; }
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !Look(hud)) return;
            try
            {
                Ped player = Game.Player.Character;
                bool inVehicle = player.IsInVehicle();
                int control = inVehicle ? (int)_vehicleControl.GetValue(null) : SelectWeapon;
                bool held = Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, control)
                    || (inVehicle && !Glyphs.UsingPad && Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, SelectWeapon));
                bool open = (bool)_open.GetValue(hud);
                // the prompt only stands in the wheel's way on foot
                HoldPrompt(_overPickups && !inVehicle && (held || open));
                if (!held) { _heldSince = 0; _logged = false; return; }
                long now = RealTime.Ms;
                if (_heldSince == 0) _heldSince = now;
                if (_logged || now - _heldSince < 600 || open) return;
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
                HoldPrompt(false);
                _open = null;
            }
        }

        // GTA6Hud's weapon pickup prompt off while held, back to what it was when let go.
        private void HoldPrompt(bool hold)
        {
            if (_pickupPrompt == null) return;
            try
            {
                if (hold)
                {
                    if (!_promptHeld) { _promptWas = (bool)_pickupPrompt.GetValue(null); _promptHeld = true; }
                    if ((bool)_pickupPrompt.GetValue(null)) _pickupPrompt.SetValue(null, false);
                }
                else if (_promptHeld)
                {
                    _pickupPrompt.SetValue(null, _promptWas);
                    _promptHeld = false;
                }
            }
            catch { }
        }

        private bool Look(object hud)
        {
            if (_looked) return _open != null;
            _looked = true;
            Type t = hud.GetType();
            const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            Type refs = t.Assembly.GetType("GTA6Hud.Ref");
            _open = t.GetField("_wheelOpen", Own);
            _consume = t.GetField("_consumeIndex", Own);
            _trunk = t.GetField("_trunkOpen", Own);
            _settings = t.GetField("_settingsOpen", Own);
            _radio = t.GetField("_radio", Own);
            _radioOpen = _radio?.FieldType.GetProperty("IsOpen", Own);
            _vehicleControl = refs?.GetField("WheelVehicleOpenControl", Static);
            _pickupPrompt = refs?.GetField("PickupPromptEnabled", Static);
            if (_pickupPrompt != null && _pickupPrompt.FieldType != typeof(bool)) _pickupPrompt = null;
            if (_overPickups && _pickupPrompt == null) Log.Write("WheelWatch", "GTA6Hud's pickup prompt switch was not found; dropped weapons nearby can still keep the wheel shut");
            if (_open == null || _consume == null || _trunk == null || _settings == null || _vehicleControl == null)
            {
                _open = null;
                Log.Write("WheelWatch", "GTA6Hud's wheel fields were not found; not watching");
            }
            return _open != null;
        }
    }
}
