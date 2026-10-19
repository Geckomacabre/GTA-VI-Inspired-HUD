using System;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// The player's cash, as GTA6Hud keeps it (GTA6Hud.PlayerCash.Money): Michael,
    /// Franklin and Trevor's own, and for Lucia her host character's. Script Hook
    /// V .NET's Player.Money only knows the three story characters and reads 0 for
    /// anyone else.
    /// </summary>
    internal static class Cash
    {
        private static PropertyInfo _money;
        private static bool _looked;

        private static PropertyInfo Prop()
        {
            if (_looked) return _money;
            _looked = true;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = asm.GetType("GTA6Hud.PlayerCash", false);
                if (t == null) continue;
                _money = t.GetProperty("Money", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                break;
            }
            return _money;
        }

        public static int Money
        {
            get
            {
                try { if (Prop() != null) return (int)_money.GetValue(null); }
                catch { }
                return Game.Player.Money;
            }
            set
            {
                try { if (Prop() != null) { _money.SetValue(null, value); return; } }
                catch { }
                Game.Player.Money = value;
            }
        }
    }
}
