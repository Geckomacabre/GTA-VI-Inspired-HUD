using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// A game installed under Program Files (where the Epic and Rockstar launchers put it)
    /// can't write to its own folder: Windows lets only administrators change files there.
    /// Then the HUD can't save its settings, the scripts can't write their saves and logs,
    /// and ReShade can't save its settings either, and players report that the mod doesn't
    /// work.
    ///
    /// When the game folder can't be written to, this tells the player in the feed and then
    /// asks Windows, with its own administrator prompt, to give the signed-in Windows user
    /// "modify" rights on the game folder: the same fix as Properties > Security > Edit by
    /// hand. Nothing changes unless the player clicks Yes. Once per game start until it's
    /// fixed; ViExtras.ini [GameFolder] FixAccess = False turns it off.
    /// </summary>
    public sealed class GameFolder : Script
    {
        private const int WarnMs = 6000;   // the feed message is up this long before Windows asks

        private readonly string _game;
        private int _step;                 // 0 waiting for the world, 1 told the player, 2 Windows is asking, 3 done
        private long _askAt;
        private volatile string _outcome;  // set by the thread that waits for Windows

        public GameFolder()
        {
            Paths.Init(BaseDirectory);
            if (!Config.Get("GameFolder", "FixAccess", true)) return;
            try { _game = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName); }
            catch { return; }
            if (string.IsNullOrEmpty(_game) || Writable()) return;
            Tick += OnTick;
        }

        private bool Writable() => CanWrite(_game) && CanWrite(Paths.Scripts)
            && CanChange(Path.Combine(Paths.Scripts, "GTA6Hud.ini")) && CanChange(Paths.Ini);

        // A file made and gone again in one go: whether new files can go in `dir`.
        private static bool CanWrite(string dir)
        {
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir)) return true;
            string probe = Path.Combine(dir, ".vihud-write-test-" + Process.GetCurrentProcess().Id + ".tmp");
            try
            {
                using (new FileStream(probe, FileMode.Create, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch (UnauthorizedAccessException) { return false; }
            catch { return true; }   // anything else isn't Windows saying no
        }

        // Opened for writing and closed untouched: whether an existing file can be saved over.
        private static bool CanChange(string file)
        {
            if (!File.Exists(file)) return true;
            try
            {
                using (new FileStream(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite)) { }
                return true;
            }
            catch (UnauthorizedAccessException) { return false; }
            catch { return true; }
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (_step == 3 || !GameReady.Now) return;
            if (_step == 0)
            {
                Feed.Post("GTA VI HUD: Windows won't let your game save in its folder (" + _game + "). Click Yes on the Windows prompt to fix it.");
                _askAt = RealTime.Ms + WarnMs;
                _step = 1;
            }
            else if (_step == 1)
            {
                if (RealTime.Ms < _askAt) return;
                _step = 2;
                var asking = new Thread(Ask) { IsBackground = true, Name = "ViExtras game folder" };
                asking.Start();
            }
            else if (_outcome != null)
            {
                _step = 3;
                Feed.Post(_outcome);
            }
        }

        // icacls "<game folder>" /grant *<user SID>:(OI)(CI)M *<user SID>:M /T, run as administrator.
        // The SID is the player's own, read here before Windows elevates, so the rights go to the
        // player even when an administrator account is the one typing its password into the prompt.
        // (OI)(CI)M passes them on to whatever is added later. /T puts them on every file and folder
        // there now, the ones that don't take their rights from the folder above included (a mod
        // installer can leave those); icacls skips (OI)(CI) on a file, so plain M is there for files.
        private void Ask()
        {
            string done;
            try
            {
                string sid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;
                string dir = _game.TrimEnd('\\');
                if (dir.EndsWith(":")) dir += "\\.";   // a drive's root; "D:\" would escape the closing quote
                var start = new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "icacls.exe"),
                    "\"" + dir + "\" /grant *" + sid + ":(OI)(CI)M *" + sid + ":M /T /C /Q")
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                int code;
                using (Process p = Process.Start(start))
                {
                    if (!p.WaitForExit(300000)) code = -1;   // /T walks every file in the game folder
                    else code = p.ExitCode;
                }
                if (Writable())
                {
                    done = "GTA VI HUD: fixed. Your game folder can save now. Restart GTA V once so every mod picks it up.";
                    Log.Write("GameFolder", "the game folder couldn't be written to; the player allowed it and it can now (" + _game + ")");
                }
                else
                {
                    done = "GTA VI HUD: Windows didn't allow it, so the HUD can't save. Move GTA V out of Program Files, or see the install guide.";
                    Log.Write("GameFolder", "icacls ended with " + code + " and the game folder still can't be written to (" + _game + ")");
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)   // ERROR_CANCELLED: the player clicked No
            {
                done = "GTA VI HUD: not fixed, so the HUD can't save its settings. You'll be asked again next time you start GTA V.";
            }
            catch (Exception ex)
            {
                done = "GTA VI HUD: couldn't ask Windows (" + ex.Message + "). Move GTA V out of Program Files, or see the install guide.";
            }
            _outcome = done;
        }
    }
}
