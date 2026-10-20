using System;
using System.Collections.Generic;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's notifications ("Not enough money.", "Lose the cops first.", "Registered. It is
    /// yours now." ...) on Script Hook V .NET Enhanced. GTA6Hud posts them with SHVDN's
    /// Notification.PostTicker(string, bool, bool), which Enhanced doesn't have, so the method
    /// that posts them can't run there: none ever showed, and GTA6Hud logged the error every 10 s.
    ///
    /// Where that call is missing, GTA6Hud's "Notifications" system is kept paused (the pause it
    /// gives itself for 10 s after an error, made to last) and its queue is posted here through the
    /// game's feed natives, the way PostTicker posts it (Feed.Post). With Script Hook V .NET 3,
    /// where GTA6Hud posts them itself, this does nothing.
    /// </summary>
    public sealed class Gta6HudFeed : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const string SystemName = "Notifications";   // GTA6Hud's Safely(..., "Notifications")

        private bool _looked;
        private FieldInfo _paused;        // GTA6Hud._systemPaused: Dictionary<string, int>, game time to resume at
        private Queue<string> _pending;   // GTA6Hud._pending (static), filled by GTA6Hud.Notify
        private object _lock;             // GTA6Hud._pendingLock (static)

        public Gta6HudFeed()
        {
            Paths.Init(BaseDirectory);
            if (HasPostTicker()) return;
            Tick += OnTick;
        }

        private static bool HasPostTicker()
        {
            try
            {
                MethodInfo m = typeof(GTA.UI.Notification).GetMethod("PostTicker", BindingFlags.Public | BindingFlags.Static, null,
                    new[] { typeof(string), typeof(bool), typeof(bool) }, null);
                return m != null && m.ReturnType.FullName == "GTA.UI.FeedPost";
            }
            catch { return true; }
        }

        private bool Look(object hud)
        {
            if (_looked) return _pending != null;
            _looked = true;
            Type t = hud.GetType();
            _paused = t.GetField("_systemPaused", Own);
            _pending = t.GetField("_pending", Stat)?.GetValue(null) as Queue<string>;
            _lock = t.GetField("_pendingLock", Stat)?.GetValue(null);
            if (_paused == null || _pending == null || _lock == null)
            {
                Log.Write("Feed", "GTA6Hud's notification queue was not found; its notifications stay off on Script Hook V .NET Enhanced");
                _pending = null;
                return false;
            }
            Log.Write("Feed", "GTA6Hud's notifications go through the game's feed here: Script Hook V .NET Enhanced has no Notification.PostTicker(string, bool, bool)");
            return true;
        }

        private void OnTick(object sender, EventArgs e)
        {
            object hud = HudBridge.Gta6Hud();
            if (hud == null || !Look(hud)) return;
            try
            {
                // its own pause after an error, kept up: it never tries the missing call, so nothing to log
                if (_paused.GetValue(hud) is Dictionary<string, int> paused) paused[SystemName] = int.MaxValue;
                if (!GameReady.Now) return;   // they wait in the queue until the game is back
                List<string> batch = null;
                lock (_lock)
                {
                    while (_pending.Count > 0)
                    {
                        string text = _pending.Dequeue();
                        if (!string.IsNullOrEmpty(text)) (batch ?? (batch = new List<string>())).Add(text);
                    }
                }
                if (batch == null) return;
                foreach (string text in batch) Feed.Post(text);
            }
            catch (Exception ex)
            {
                Log.Write("Feed", "GTA6Hud's notifications: " + ex.Message);
                _pending = null;   // stops here; GTA6Hud's own pause runs out and it goes back to its 10 s errors
            }
        }
    }
}
