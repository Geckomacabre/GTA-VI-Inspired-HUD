using System;
using System.Diagnostics;
using System.IO;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// LSPD First Response. On duty the player is the police: nothing they do is a
    /// crime to [Memory], no witness runs to report them, and no wanted level sticks,
    /// whoever gives it (a GTA6Hud crime such as shoplifting or a car smash-in, a
    /// witness, the game itself). Any stars are cleared as they land.
    ///
    /// LSPDFR runs in RAGE Plugin Hook's own AppDomain with nothing we can call, so
    /// being on duty is read off the player the way LSPDFR leaves them: it puts them
    /// in the SECURITY_GUARD relationship group on duty and back in PLAYER off duty
    /// (seen on a real on-duty run). The maximum wanted level it sets to 0 is not
    /// asked: a wanted level that gets through means something raised it again.
    /// </summary>
    public sealed class Lspdfr : Script
    {
        private const int SecurityGuard = unchecked((int)0xF50B51B7);

        private static readonly bool Installed = Find();
        private static bool _onDuty;
        private static int _checkAt;

        /// <summary>The player is an on-duty LSPDFR officer. Read every 500 ms.</summary>
        public static bool OnDuty
        {
            get
            {
                if (!Installed) return false;
                int now = Game.GameTime;
                if (now - _checkAt < 500 && now >= _checkAt) return _onDuty;
                _checkAt = now;
                Ped me = Game.Player.Character;
                bool duty = me != null && me.Exists()
                    && Function.Call<int>(Hash.GET_PED_RELATIONSHIP_GROUP_HASH, me) == SecurityGuard;
                if (duty != _onDuty) Log.Write("LSPDFR", duty ? "on duty: no crimes, witnesses or wanted level" : "off duty");
                _onDuty = duty;
                return duty;
            }
        }

        private static bool Find()
        {
            try
            {
                string game = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                return File.Exists(Path.Combine(game, "plugins", "LSPD First Response.dll"));
            }
            catch { return false; }
        }

        public Lspdfr()
        {
            if (!Installed) return;
            Paths.Init(BaseDirectory);
            Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!OnDuty || Game.Player.WantedLevel == 0) return;
            Log.Write("LSPDFR", "on duty: cleared a " + Game.Player.WantedLevel + " star wanted level");
            Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, Game.Player);
        }
    }
}
