using System;
using System.Collections.Generic;
using System.Reflection;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The money at the top right the whole time you're in a store ([Switches] MoneyInStores). In GTA VI it's on screen
    /// from the moment you walk into a store until you walk out (the Game Horde clip shows "$19" the whole visit, the
    /// gas station the register's money as it's taken), where GTA6Hud showed it only with a store's menu open. A store
    /// is where GTA6Hud sells things: a 24/7's shelves (its InConvenienceStore), a clothes rack or a barber's chair
    /// (ShopStands) or an Ammu-Nation till (AmmuCounters), with you indoors and on foot. While you're in one, GTA6Hud's
    /// money row stays up (HudBridge.ShowMoney).
    /// </summary>
    public sealed class StoreMoney : Script
    {
        private const float Reach = 12f, ReachZ = 4f;
        private const int CheckMs = 400;

        private Func<Vector3, bool> _inConvenience;
        private Vector3[] _spots;
        private bool _looked, _inStore, _logged;
        private int _checkAt;

        public StoreMoney()
        {
            if (!Config.Get(Config.Switches, "MoneyInStores", true)) return;
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            try
            {
                int now = Environment.TickCount;
                if (unchecked(now - _checkAt) >= CheckMs)
                {
                    _checkAt = now;
                    _inStore = InStore();
                }
                if (_inStore) HudBridge.ShowMoney();
            }
            catch (Exception ex)
            {
                if (_logged) return;
                _logged = true;
                Log.Write("StoreMoney", ex.Message);
            }
        }

        private bool InStore()
        {
            Ped me = Game.Player.Character;
            if (me == null || !me.Exists() || me.IsInVehicle()) return false;
            if (Function.Call<int>(Hash.GET_INTERIOR_FROM_ENTITY, me) == 0) return false;
            if (!Look()) return false;
            Vector3 at = me.Position;
            if (_inConvenience != null && _inConvenience(at)) return true;
            foreach (Vector3 s in _spots)
                if (Math.Abs(s.X - at.X) < Reach && Math.Abs(s.Y - at.Y) < Reach && Math.Abs(s.Z - at.Z) < ReachZ) return true;
            return false;
        }

        // GTA6Hud's store lists, read once it's running
        private bool Look()
        {
            if (_looked) return _spots != null;
            object hud = HudBridge.Gta6Hud();
            if (hud == null) return false;
            _looked = true;
            const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = hud.GetType();
            var spots = new List<Vector3>();
            try
            {
                MethodInfo inStore = t.GetMethod("InConvenienceStore", Static, null, new[] { typeof(Vector3) }, null);
                if (inStore != null) _inConvenience = (Func<Vector3, bool>)Delegate.CreateDelegate(typeof(Func<Vector3, bool>), inStore);
                if (t.GetField("ShopStands", Static)?.GetValue(null) is Array stands)
                    foreach (object s in stands)
                        if (s.GetType().GetField("Rack")?.GetValue(s) is Vector3 rack) spots.Add(rack);
                if (t.GetField("AmmuCounters", Static)?.GetValue(null) is Array counters)
                    foreach (object c in counters)
                        if (c.GetType().GetField("Item1")?.GetValue(c) is Vector3 till) spots.Add(till);
            }
            catch (Exception ex) { Log.Write("StoreMoney", "GTA6Hud's stores: " + ex.Message); }
            _spots = spots.ToArray();
            Log.Write("StoreMoney", "the money shows in stores: " + _spots.Length + " racks and tills, 24/7 shelves " + (_inConvenience != null ? "found" : "not found"));
            return true;
        }
    }
}
