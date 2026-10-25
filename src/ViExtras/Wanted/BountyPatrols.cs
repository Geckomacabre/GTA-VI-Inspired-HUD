using System;
using System.Reflection;
using GTA;

namespace ViExtras
{
    /// <summary>
    /// GTA6Hud's Heat sends police patrols cruising round you while it has a bounty on you,
    /// and the bounty is saved: it only drops $500 a game day, so a game loaded with one on
    /// you had patrols closing in from the first minute, for hours. With [Switches]
    /// OldBountyPatrols = False, Heat only runs while you're wanted and for FreshMinutes
    /// after (GTA6Hud's Ref.HeatEnabled is held off the rest of the time, and put back as
    /// it was). Nothing counts as fresh when the game loads.
    ///
    /// While Los Santos RED runs, PoliceMods has Heat off and this stays out of it.
    /// </summary>
    public sealed class BountyPatrols : Script
    {
        private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const int CheckMs = 1000;

        private readonly bool _oldBounty;
        private readonly int _freshMs;
        private FieldInfo _heat;
        private bool _looked, _holding, _heatWas;
        private int _troubleAt = int.MinValue;

        public BountyPatrols()
        {
            _oldBounty = Config.Get(Config.Switches, "OldBountyPatrols", false);
            _freshMs = Config.Get("BountyPatrols", "FreshMinutes", 10) * 60000;
            if (_oldBounty) return;
            Interval = CheckMs;
            Tick += OnTick;
            Aborted += (s, e) => Release();
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (!GameReady.Now || !Look()) return;
                int now = Game.GameTime;
                if (Game.Player.WantedLevel > 0) _troubleAt = now;

                if (PoliceMods.Lsr) { _holding = false; return; }
                bool fresh = _troubleAt != int.MinValue && now - _troubleAt < _freshMs;
                if (fresh) Release();
                else Hold();
            }
            catch (Exception ex) { Log.Write("BountyPatrols", ex.Message); }
        }

        private bool Look()
        {
            if (_heat != null) return true;
            if (_looked) return false;
            object hud = HudBridge.Gta6Hud();
            if (hud == null) return false;
            _looked = true;
            _heat = hud.GetType().Assembly.GetType("GTA6Hud.Ref")?.GetField("HeatEnabled", Static);
            if (_heat != null && _heat.FieldType != typeof(bool)) _heat = null;
            if (_heat == null) Log.Write("BountyPatrols", "GTA6Hud's Heat setting was not found; its patrols stay as they are");
            return _heat != null;
        }

        private void Hold()
        {
            bool on = (bool)_heat.GetValue(null);
            if (!_holding)
            {
                _heatWas = on;
                _holding = true;
                if (on) Log.Write("BountyPatrols", "no fresh trouble: GTA6Hud's bounty patrols held off");
            }
            // GTA6Hud reads its settings again on F10; held each second, whatever it read.
            if (on) _heat.SetValue(null, false);
        }

        private void Release()
        {
            if (!_holding || _heat == null) return;
            _holding = false;
            _heat.SetValue(null, _heatWas);
            if (_heatWas) Log.Write("BountyPatrols", "wanted: GTA6Hud's bounty patrols may run again");
        }
    }
}
