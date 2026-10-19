using System;
using System.IO;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's CRASH CAMERA setting (its settings menu, held camera button;
    /// GTA6Hud.ini [Cinematics] CrashCamera) switches this crash cam, and
    /// GTA6Hud's own crash camera no longer runs.
    ///
    /// GTA6Hud starts its crash cinematic only while its _crashCameraEnabled is
    /// set, and only reads that field for the setting's ON / OFF in its menu. So
    /// the field is kept false, except while the settings menu is open, when it
    /// holds the player's choice for the menu to show and flip (GTA6Hud saves it
    /// to its ini itself). For those moments GTA6Hud's last impact speed is
    /// zeroed, which its crash check needs above 16, so no crash can start its
    /// cinematic with the menu up. GTA6Hud.dll itself is not changed.
    /// </summary>
    internal static class Gta6HudCrashToggle
    {
        private static bool _looked, _wasOpen;
        private static object _hud;
        private static FieldInfo _enabled, _settingsOpen, _impactSpeed;
        private static bool _wanted = ReadIni();

        /// <summary>The player has the crash camera switched on in GTA6Hud's settings.</summary>
        public static bool On => _wanted;

        private static bool ReadIni()
        {
            try { return ScriptSettings.Load(Path.Combine(Paths.Scripts ?? ".", "GTA6Hud.ini")).GetValue("Cinematics", "CrashCamera", true); }
            catch { return true; }
        }

        /// <summary>Every frame, after GTA6Hud's own tick.</summary>
        public static void Tick()
        {
            if (_hud == null)
            {
                object hud = HudBridge.Gta6Hud();
                if (hud == null || _looked) return;
                _looked = true;
                const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                Type t = hud.GetType();
                _enabled = t.GetField("_crashCameraEnabled", Own);
                _settingsOpen = t.GetField("_settingsOpen", Own);
                _impactSpeed = t.GetField("_impactSpeed", Own);
                if (_enabled == null || _settingsOpen == null || _impactSpeed == null)
                {
                    Log.Write("CrashCam", "GTA6Hud's crash camera setting was not found; its own crash camera may still run");
                    return;
                }
                _hud = hud;
            }
            try
            {
                bool open = (bool)_settingsOpen.GetValue(_hud);
                if (open)
                {
                    if (!_wasOpen) _enabled.SetValue(_hud, _wanted);   // the menu shows the player's choice
                    else _wanted = (bool)_enabled.GetValue(_hud);       // and takes their changes
                    _impactSpeed.SetValue(_hud, 0f);
                }
                else
                {
                    if (_wasOpen) _wanted = (bool)_enabled.GetValue(_hud);
                    _enabled.SetValue(_hud, false);
                }
                _wasOpen = open;
            }
            catch { }
        }
    }
}
