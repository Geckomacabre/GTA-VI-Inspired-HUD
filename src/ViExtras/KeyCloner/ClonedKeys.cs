// The key cloner's dial-up on the cars you cloned the key of (ROMAN2K's idea, on the Discord).
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// A car whose key you cloned with GTA6Hud's key cloner answers to it like a phone
    /// key: starting to get in, and starting to get out, play the cloner's dial-up
    /// (scripts\ViExtras\KeyCloner\pin_code_loop.wav, once). Other cars stay quiet.
    ///
    /// GTA6Hud keeps no list of cloned cars, so the clone is watched: its key cloner's
    /// static _kcCar and _kcResult (1: KEY CLONED, the door's and Waink's alike). The
    /// cars are remembered for the session, while they're around. GTA6Hud.dll itself is
    /// not changed; without it (or a build without the key cloner) nothing plays.
    /// [Switches] ClonedKeySound turns it off; [ClonedKeys] Volume sets how loud.
    /// </summary>
    public sealed class ClonedKeys : Script
    {
        private const int ExitVehicleTask = 2;   // CTaskExitVehicle
        private const int Cloned = 1;            // GTA6Hud's _kcResult: KEY CLONED
        private const int PruneMs = 10000;

        private readonly bool _enabled;
        private readonly float _volume;
        private readonly string _sound;
        private readonly HashSet<int> _cars = new HashSet<int>();

        private bool _looked;
        private FieldInfo _kcCar, _kcResult;
        private int _entering;
        private bool _exiting;
        private long _pruneAt;
        private int _errors;

        public ClonedKeys()
        {
            Paths.Init(this);
            _enabled = Config.Get(Config.Switches, "ClonedKeySound", true);
            _volume = Num.Clamp(Config.Get("ClonedKeys", "Volume", 100), 0, 100) / 100f;
            _sound = Path.Combine(Paths.Root, "KeyCloner", "pin_code_loop.wav");
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!_enabled) return;
            try
            {
                WatchClone();
                WatchDoors();
            }
            catch (Exception ex)
            {
                if (_errors++ < 5) Log.Write("ClonedKeys", ex.ToString());
            }
        }

        // a clone that worked: its car goes on the list
        private void WatchClone()
        {
            if (!_looked)
            {
                object hud = HudBridge.Gta6Hud();
                if (hud == null) return;
                _looked = true;
                const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                Type t = hud.GetType();
                _kcCar = t.GetField("_kcCar", Static);
                _kcResult = t.GetField("_kcResult", Static);
                if (_kcCar == null || _kcResult == null || _kcResult.FieldType != typeof(int))
                {
                    _kcCar = _kcResult = null;
                    Log.Write("ClonedKeys", "GTA6Hud's key cloner was not found: no dial-up on cloned cars");
                }
            }
            if (_kcCar == null) return;
            if ((int)_kcResult.GetValue(null) == Cloned && _kcCar.GetValue(null) is Vehicle car && car.Exists() && _cars.Add(car.Handle))
                Log.Write("ClonedKeys", "key cloned: " + car.DisplayName + " (" + _cars.Count + " this session)");

            long now = RealTime.Ms;
            if (now >= _pruneAt)
            {
                _pruneAt = now + PruneMs;
                _cars.RemoveWhere(h => !Function.Call<bool>(Hash.DOES_ENTITY_EXIST, h));
            }
        }

        // the dial-up as you start to get into one of them, and as you start to get out
        private void WatchDoors()
        {
            Ped player = Game.Player.Character;
            if (player == null || !player.Exists() || player.IsDead || _cars.Count == 0)
            {
                _entering = 0;
                _exiting = false;
                return;
            }
            int trying = Function.Call<int>(Hash.GET_VEHICLE_PED_IS_TRYING_TO_ENTER, player);
            if (trying != _entering)
            {
                _entering = trying;
                if (trying != 0 && _cars.Contains(trying)) Play("getting in", trying);
            }
            bool exiting = Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, player, ExitVehicleTask);
            if (exiting && !_exiting)
            {
                int car = Function.Call<int>(Hash.GET_VEHICLE_PED_IS_IN, player, true);
                if (_cars.Contains(car)) Play("getting out", car);
            }
            _exiting = exiting;
        }

        private void Play(string when, int car)
        {
            Log.Write("ClonedKeys", "dial-up: " + when + " (car " + car + ")");
            RadioSound.Play(_sound, _volume);
        }
    }
}
