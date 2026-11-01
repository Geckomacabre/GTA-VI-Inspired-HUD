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

        private static object _wallet;
        private static PropertyInfo _inShop, _available;
        private static MethodInfo _trySpend;

        /// <summary>
        /// GTA6Hud's Wallet, which holds BuckMe. Null without GTA6Hud, and while one
        /// of the game's own shops is up: BeginShop has lent BuckMe to the cash then,
        /// so Money already counts it.
        /// </summary>
        private static object OpenWallet()
        {
            try
            {
                if (_wallet == null)
                {
                    object hud = HudBridge.Gta6Hud();
                    if (hud == null) return null;
                    const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    object wallet = hud.GetType().GetField("_wallet", Own)?.GetValue(hud);
                    if (wallet == null) return null;
                    _inShop = wallet.GetType().GetProperty("InShop", Own);
                    _available = wallet.GetType().GetProperty("Available", Own);
                    _trySpend = wallet.GetType().GetMethod("TrySpend", Own);
                    _wallet = wallet;
                }
                return (bool)_inShop.GetValue(_wallet) ? null : _wallet;
            }
            catch { return null; }
        }

        /// <summary>What the player can pay with: the cash and BuckMe together.</summary>
        public static long Spendable
        {
            get
            {
                object wallet = OpenWallet();
                if (wallet != null)
                {
                    try { return (long)_available.GetValue(wallet); }
                    catch { }
                }
                return Money;
            }
        }

        /// <summary>Takes the amount from the cash, and what the cash can't cover from BuckMe. False if the two together can't.</summary>
        public static bool Spend(int amount)
        {
            object wallet = OpenWallet();
            if (wallet != null)
            {
                try { return (bool)_trySpend.Invoke(wallet, new object[] { amount }); }
                catch { return false; }
            }
            if (Money < amount) return false;
            Money -= amount;
            return true;
        }
    }
}
