using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's Heat patrols (PoliceHeat) are made persistent, and their driver is told to cruise
    /// (TASK_VEHICLE_DRIVE_WANDER) once, when the car is made. Nothing gives that order again, and a
    /// patrol is only let go when Heat ends or its car is destroyed. So once the order is gone the
    /// car sits where it is, kept by the game, for as long as Heat runs: GTA6Hud's police brain
    /// clears every cop's tasks when a chase ends, and the first time it takes a cop on while the
    /// police still remember you (within 45 m, no stars) - a patrol that drove past you stopped
    /// dead - and a crash can leave the driver out of his car or the car beyond driving.
    ///
    /// Here, while you aren't wanted: a patrol whose car has stood still for StillSeconds is told
    /// to cruise again. One that still hasn't moved after the third time, whose driver has been out
    /// of the car for StillSeconds, or whose car can't be driven, is let go the way GTA6Hud lets one
    /// go (PoliceHeat.Retire: no longer persistent, and deleted once it's out of sight). A patrol the brain has a job for (looking
    /// you over) is left to it. [BountyPatrols] KeepMoving = False leaves them as they were.
    /// </summary>
    public sealed class PatrolsKeepMoving : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int CheckMs = 1000;
        private const int Tries = 3;
        private const float StillSpeed = 1f;
        private const int DrivingStyle = 786603;   // GTA6Hud's, for its patrols

        private readonly int _stillMs;
        private bool _looked, _broken;
        private FieldInfo _heatField, _patrolsField, _vehicleField, _crewField, _speedField;
        private MethodInfo _retire;
        private FieldInfo _brainField, _copsField, _copPed, _copRole;

        private sealed class Watch { public int StillMs, Tries; }
        // patrol car handle -> how long it has stood still, and how often it was sent off again
        private readonly Dictionary<int, Watch> _watch = new Dictionary<int, Watch>();
        private readonly HashSet<int> _seen = new HashSet<int>();

        public PatrolsKeepMoving()
        {
            Paths.Init(this);
            if (!Config.Get("BountyPatrols", "KeepMoving", true)) return;
            _stillMs = (int)(Math.Max(3f, Config.Get("BountyPatrols", "StillSeconds", 10f)) * 1000f);
            Interval = CheckMs;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_broken || !GameReady.Now) return;
            try { Check(); }
            catch (Exception ex)
            {
                // the patrols are left to GTA6Hud from here on
                Log.Write("PatrolsKeepMoving", ex.ToString());
                _broken = true;
            }
        }

        private void Check()
        {
            // in a chase the brain drives them; story missions and the police mods run their own police
            if (Game.Player.WantedLevel > 0 || Game.IsMissionActive || PoliceMods.TakesPolice) { _watch.Clear(); return; }
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !Look(hud)) return;
            object heat = _heatField.GetValue(hud);
            if (!(_patrolsField.GetValue(heat) is IList patrols) || patrols.Count == 0) { _watch.Clear(); return; }
            HashSet<int> busy = BrainBusy(hud);

            _seen.Clear();
            for (int i = patrols.Count - 1; i >= 0; i--)
            {
                object patrol = patrols[i];
                if (!(_vehicleField.GetValue(patrol) is Vehicle car) || !car.Exists() || car.IsDead) continue;   // GTA6Hud's PruneDead has those
                List<Ped> crew = _crewField.GetValue(patrol) as List<Ped>;
                if (crew != null && crew.Exists(c => c != null && busy.Contains(c.Handle))) continue;
                _seen.Add(car.Handle);
                if (!_watch.TryGetValue(car.Handle, out Watch w)) _watch[car.Handle] = w = new Watch();

                if (!car.IsDriveable) { Retire(heat, patrols, i, car, "its car can't be driven"); continue; }
                Ped driver = crew != null && crew.Count > 0 ? crew[0] : null;
                bool atWheel = driver != null && driver.Exists() && !driver.IsDead && car.Driver == driver;
                if (atWheel && car.Speed >= StillSpeed) { w.StillMs = 0; w.Tries = 0; continue; }
                w.StillMs += CheckMs;
                if (w.StillMs < _stillMs) continue;
                w.StillMs = 0;
                // a driver out of his car this long (a crash, a fight) isn't getting back in
                if (!atWheel) { Retire(heat, patrols, i, car, "its driver left the car"); continue; }
                if (w.Tries >= Tries) { Retire(heat, patrols, i, car, "stuck"); continue; }
                w.Tries++;
                Function.Call(Hash.TASK_VEHICLE_DRIVE_WANDER, driver, car, PatrolSpeed(), DrivingStyle);
                Log.Write("PatrolsKeepMoving", "patrol " + car.Handle + " stood still " + (_stillMs / 1000) + " s: cruising again (" + w.Tries + ")");
            }
            if (_watch.Count > _seen.Count)
            {
                List<int> gone = new List<int>();
                foreach (int handle in _watch.Keys) if (!_seen.Contains(handle)) gone.Add(handle);
                foreach (int handle in gone) _watch.Remove(handle);
            }
        }

        private void Retire(object heat, IList patrols, int index, Vehicle car, string why)
        {
            _retire.Invoke(heat, new[] { patrols[index] });
            patrols.RemoveAt(index);
            _watch.Remove(car.Handle);
            Log.Write("PatrolsKeepMoving", "patrol " + car.Handle + " let go: " + why);
        }

        // The cops the brain has a job for now (its Role isn't None).
        private HashSet<int> BrainBusy(object hud)
        {
            HashSet<int> busy = new HashSet<int>();
            if (_copsField == null) return busy;
            if (!(_copsField.GetValue(_brainField.GetValue(hud)) is IList cops)) return busy;
            foreach (object cop in cops)
                if (_copPed.GetValue(cop) is Ped ped && ped.Exists() && _copRole.GetValue(cop)?.ToString() != "None") busy.Add(ped.Handle);
            return busy;
        }

        private float PatrolSpeed()
        {
            try { if (_speedField?.GetValue(null) is float speed && speed > 0f) return speed; }
            catch { }
            return 17f;   // GTA6Hud's [Heat] PatrolSpeed default
        }

        private bool Look(object hud)
        {
            if (_retire != null) return true;
            if (_looked) return false;
            _looked = true;
            Type t = hud.GetType();
            _heatField = t.GetField("_heat", Own);
            Type heat = _heatField?.FieldType;
            _patrolsField = heat?.GetField("_patrols", Own);
            Type patrol = heat?.GetNestedType("Patrol", BindingFlags.Public | BindingFlags.NonPublic);
            _vehicleField = patrol?.GetField("Vehicle", Own);
            _crewField = patrol?.GetField("Crew", Own);
            _retire = patrol == null ? null : heat.GetMethod("Retire", Own, null, new[] { patrol }, null);
            _speedField = t.Assembly.GetType("GTA6Hud.Ref")?.GetField("HeatPatrolSpeed", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (_patrolsField == null || _vehicleField == null || _crewField == null || _retire == null)
            {
                _retire = null;
                Log.Write("PatrolsKeepMoving", "GTA6Hud's Heat patrols were not as expected; left as they are");
                return false;
            }
            // without the brain every patrol counts as free
            _brainField = t.GetField("_policeBrain", Own);
            _copsField = _brainField?.FieldType.GetField("_cops", Own);
            Type cop = _brainField?.FieldType.GetNestedType("Cop", BindingFlags.Public | BindingFlags.NonPublic);
            _copPed = cop?.GetField("Ped", Own);
            _copRole = cop?.GetField("Role", Own);
            if (_copPed == null || _copRole == null) _copsField = null;
            return true;
        }
    }
}
