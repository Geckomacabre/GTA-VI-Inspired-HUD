using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// Script Hook V .NET runs every script from a copy in Windows' cache (%LOCALAPPDATA%\assembly\dl3) and
    /// makes a new copy only when the DLL's path or date changes. A DLL replaced by one with the same date
    /// (packages up to 2.5.4 stamped every file 2026-10-18 00:00) keeps running the old copy, and the player
    /// sees an update that "didn't install". An OIV can't clear that cache: it's outside the game folder.
    ///
    /// Once per game start this compares each of the HUD's scripts that runs from the cache with its DLL in
    /// scripts\. When one differs, it sets that DLL's date to now, so the next start copies it again, and says
    /// so in the feed; if Windows won't let it, the feed says how to clear the cache by hand.
    /// ViExtras.ini [CacheCheck] Enabled = False turns it off.
    /// </summary>
    public sealed class CacheCheck : Script
    {
        private static readonly string[] Ours = { "GTA6Hud", "ViExtras", "ViCustoms", "ViCrouch", "ViSwitch", "ViWasted", "ViMinimap" };

        private bool _started, _told;
        private volatile string _message;   // set by the thread that compares

        public CacheCheck()
        {
            Paths.Init(BaseDirectory);
            if (!Config.Get("CacheCheck", "Enabled", true)) return;
            Tick += OnTick;
        }

        // Once the world is up, every script has been loaded: the comparing runs off the game's thread.
        private void OnTick(object sender, EventArgs e)
        {
            if (_told || !GameReady.Now) return;
            if (!_started)
            {
                _started = true;
                var compare = new Thread(Compare) { IsBackground = true, Name = "ViExtras cache check" };
                compare.Start();
                return;
            }
            string message = _message;
            if (message == null) return;
            _told = true;
            if (message.Length > 0) Feed.Post(message);
        }

        private void Compare()
        {
            var stale = new List<string>();
            bool fixedAll = true;
            try
            {
                DateTime started = Process.GetCurrentProcess().StartTime;
                foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string name = a.GetName().Name;
                    if (Array.IndexOf(Ours, name) < 0 || a.IsDynamic) continue;
                    string running = a.Location, onDisk;
                    try { onDisk = new Uri(a.CodeBase).LocalPath; }
                    catch { continue; }
                    // not run from the cache, or the DLL was changed after the game started (a restart is all it needs)
                    if (string.IsNullOrEmpty(running) || string.Equals(Path.GetFullPath(running), Path.GetFullPath(onDisk), StringComparison.OrdinalIgnoreCase)
                        || !File.Exists(running) || !File.Exists(onDisk) || File.GetLastWriteTime(onDisk) > started)
                        continue;
                    if (Same(running, onDisk)) continue;
                    stale.Add(name);
                    Log.Write("CacheCheck", name + " runs from an old copy in Windows' cache (" + running + "), not " + onDisk);
                    try { File.SetLastWriteTime(onDisk, DateTime.Now); }
                    catch (Exception ex)
                    {
                        fixedAll = false;
                        Log.Write("CacheCheck", "couldn't renew the date of " + onDisk + ": " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("CacheCheck", "the check stopped: " + ex.Message);
            }
            if (stale.Count == 0) { _message = ""; return; }
            string which = string.Join(", ", stale);
            _message = fixedAll
                ? "GTA VI HUD: an old copy of " + which + " was still running from Windows' cache, so your update hadn't taken effect. Fixed: restart GTA V once."
                : "GTA VI HUD: an old copy of " + which + " is still running from Windows' cache. Close GTA V, press Win + R, type %LOCALAPPDATA%\\assembly, press Enter and delete the dl3 folder.";
        }

        private static bool Same(string a, string b)
        {
            if (new FileInfo(a).Length != new FileInfo(b).Length) return false;
            using (SHA256 sha = SHA256.Create())
            {
                byte[] x, y;
                using (FileStream f = new FileStream(a, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) x = sha.ComputeHash(f);
                using (FileStream f = new FileStream(b, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete)) y = sha.ComputeHash(f);
                for (int i = 0; i < x.Length; i++) if (x[i] != y[i]) return false;
                return true;
            }
        }
    }
}
