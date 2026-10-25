using System;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// GTA V's police, toned down ([Switches] CalmerPolice): players found them on top of
    /// them in seconds, and seeming to know where you are when none of them can see you.
    ///
    ///   * They arrive slower and from further off: the game's dispatch waits
    ///     SpawnDelayMultiplier times as long between sending units, and places them
    ///     around SpawnDistance metres away.
    ///   * Once none of them can see you (the stars grey out), the search stays where
    ///     they last saw you. The game moves it to wherever you are every so often,
    ///     which is how they "just know"; it is put back each second.
    ///   * They're easier to lose: hidden for EscapeSeconds, plus EscapeSecondsPerStar
    ///     for each star, and the stars go.
    ///
    /// Not on a story mission (missions run their own police), with LSPDFR on duty, or
    /// while another mod runs the police (PoliceMods: RDE, LSR). The game's own values
    /// are put back then, and when the scripts stop.
    /// </summary>
    public sealed class CalmerPolice : Script
    {
        private const int CheckMs = 1000;
        private const int LateMs = 5000;

        private readonly float _delayMul, _spawnDistance;
        private readonly int _escapeMs, _escapePerStarMs;
        private bool _applied;
        private int _escapeFor = -1, _hiddenSince = -1;

        private static bool Busted() =>
            Function.Call<bool>(Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, true) || Game.Player.Character.IsDead;

        public CalmerPolice()
        {
            if (!Config.Get(Config.Switches, "CalmerPolice", true)) return;
            _delayMul = Math.Max(1f, Config.Get("CalmerPolice", "SpawnDelayMultiplier", 2f));
            _spawnDistance = Config.Get("CalmerPolice", "SpawnDistance", 300f);
            _escapeMs = Math.Max(0, Config.Get("CalmerPolice", "EscapeSeconds", 10)) * 1000;
            _escapePerStarMs = Math.Max(0, Config.Get("CalmerPolice", "EscapeSecondsPerStar", 5)) * 1000;
            Interval = CheckMs;
            Tick += OnTick;
            Aborted += (s, e) => Reset();
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                if (!GameReady.Now) return;
                if (Game.IsMissionActive || Lspdfr.OnDuty || PoliceMods.TakesPolice)
                {
                    Reset();
                    return;
                }
                Apply();

                Wanted wanted = Game.Player.Wanted;
                int level = wanted.WantedLevel;
                if (level > 0 && wanted.HasGrayedOutStars)
                {
                    int now = Game.GameTime;
                    if (_hiddenSince < 0) _hiddenSince = now;

                    // Where they last saw you, not where you are now: put back only when the
                    // game has moved it, so the search isn't restarted every second.
                    Vector3 seen = wanted.LastPositionSpottedByPolice;
                    Vector3 centre = Function.Call<Vector3>(Hash.GET_PLAYER_WANTED_CENTRE_POSITION, Game.Player);
                    if (seen != Vector3.Zero && centre.DistanceTo(seen) > 5f) wanted.SetRadiusCenter(seen);

                    // The game's own escape timer should have let you go by now; if anything held
                    // it back, the stars go here, a few seconds late.
                    if (now - _hiddenSince > _escapeMs + _escapePerStarMs * level + LateMs && !Busted())
                    {
                        wanted.SetWantedLevel(0, false);
                        wanted.ApplyWantedLevelChangeNow(false);
                        Log.Write("CalmerPolice", "lost them (hidden " + (now - _hiddenSince) / 1000 + " s at " + level + " stars)");
                        _hiddenSince = -1;
                    }
                }
                else _hiddenSince = -1;
            }
            catch (Exception ex) { Log.Write("CalmerPolice", ex.Message); }
        }

        private void Apply()
        {
            if (!_applied)
            {
                _applied = true;
                Function.Call(Hash.SET_DISPATCH_TIME_BETWEEN_SPAWN_ATTEMPTS_MULTIPLIER, _delayMul);
                if (_spawnDistance > 0f) Function.Call(Hash.SET_DISPATCH_IDEAL_SPAWN_DISTANCE, _spawnDistance);
                Log.Write("CalmerPolice", "on: dispatch x" + _delayMul + " slower, from " + _spawnDistance + " m; lost after "
                    + _escapeMs / 1000 + " s + " + _escapePerStarMs / 1000 + " s a star hidden");
                _escapeFor = -1;
            }
            // The hidden-escape time is per level; set again whenever the stars change.
            int level = Game.Player.Wanted.WantedLevel;
            if (level != _escapeFor && level > 0)
            {
                _escapeFor = level;
                Function.Call(Hash.SET_WANTED_LEVEL_HIDDEN_ESCAPE_TIME, Game.Player, level, _escapeMs + _escapePerStarMs * level);
            }
        }

        private void Reset()
        {
            _hiddenSince = -1;
            if (!_applied) return;
            _applied = false;
            _escapeFor = -1;
            Function.Call(Hash.SET_DISPATCH_TIME_BETWEEN_SPAWN_ATTEMPTS_MULTIPLIER, 1f);
            Function.Call(Hash.RESET_DISPATCH_IDEAL_SPAWN_DISTANCE);
            Function.Call(Hash.RESET_WANTED_LEVEL_HIDDEN_ESCAPE_TIME, Game.Player);
            Log.Write("CalmerPolice", "off for now: the game's own police");
        }
    }
}
