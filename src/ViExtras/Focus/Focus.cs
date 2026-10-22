using System;
using System.Drawing;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The FiveM version's Focus (vice_hud client_vitals.lua, style.css #focus-fx),
    /// fitted to single player's special ability:
    ///
    ///  * Michael, Franklin and Trevor keep their own ability. While it runs,
    ///    the screen gets Focus's look: the FocusIn effect, the purple vignette
    ///    with its lilac inner glow breathing every 1100 ms, and FocusOut when it
    ///    ends.
    ///  * Anyone without an ability of their own (Lucia) gets FiveM's Focus
    ///    itself on the same button: a meter that empties in 8 s and refills in
    ///    12 s, bullet time at 0.4 eased in and out, the player kept at normal
    ///    speed and steadier aim, and the same look. (GTA6Hud has no bar for a
    ///    character without an ability, so her meter is not drawn.)
    /// </summary>
    public sealed class Focus : Script
    {
        private const int FadeMs = 450;          // #focus-fx transition: opacity 450ms ease
        private const int Frames = 9;            // focus_fx_0..8, see tools/render/focus.py
        private const int SpecialAbility = 28, SpecialAbilityPc = 171;

        private readonly bool _enabled, _abilityLook, _replaceAbilityFx, _luciaFocus;
        private readonly float _drainSeconds, _refillSeconds, _timeScale, _minToActivate;
        private readonly int _regenDelayMs, _rampSteps, _rampStepMs, _accuracyBoost, _pulseMs;
        private readonly float _strength, _pulse;

        // the look
        private bool _fxOn;
        private float _fxOpacity;
        private float _fadeFrom;
        private long _fadeAt = -1;
        private long _pulseSince;
        private bool _abilityWasActive;

        // FiveM Focus, for characters without an ability of their own
        private float _meter = 100f;
        private bool _active;
        private int _regenWaitMs;
        private long _lastTick;
        private int _accuracyBefore = -1;

        public static bool IsActive { get; private set; }

        // The game's own ability effects, stopped while Focus's look stands in for them.
        private static readonly string[] AbilityFx = { "BulletTime", "BulletTimeDark", "DrivingFocus", "REDMIST" };

        public Focus()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("Focus", "Enabled", true);
            _abilityLook = Config.Get("Focus", "AbilityLook", true);
            _replaceAbilityFx = Config.Get("Focus", "ReplaceAbilityEffects", true);
            _luciaFocus = Config.Get("Focus", "FocusWithoutAbility", true);
            _drainSeconds = Config.Get("Focus", "DrainSeconds", 8f);
            _refillSeconds = Config.Get("Focus", "RefillSeconds", 12f);
            _regenDelayMs = Config.Get("Focus", "RegenDelayMs", 1500);
            _timeScale = Config.Get("Focus", "TimeScale", 0.4f);
            _rampSteps = Config.Get("Focus", "RampSteps", 10);
            _rampStepMs = Config.Get("Focus", "RampStepMs", 35);
            _minToActivate = Config.Get("Focus", "MinToActivate", 15f);
            _accuracyBoost = Config.Get("Focus", "AccuracyBoost", 90);
            _strength = Config.Get("Focus", "VignetteStrength", 1f);
            _pulse = Config.Get("Focus", "PulseSize", 1f);
            _pulseMs = Config.Get("Focus", "PulseSpeedMs", 1100);

            Tick += OnTick;
            Aborted += (s, e) => { if (_active) Exit(instant: true); };
        }

        // hashed once, not three times a frame
        private static readonly int Michael = Game.GenerateHash("player_zero"), Franklin = Game.GenerateHash("player_one"), Trevor = Game.GenerateHash("player_two");

        private static bool IsStoryModel(Ped ped)
        {
            int h = ped.Model.Hash;
            return h == Michael || h == Franklin || h == Trevor;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now)
            {
                if (_active) Exit(instant: true);   // no slow motion left running through a switch or loading screen
                return;
            }
            if (!_enabled) return;
            Ped ped = Game.Player.Character;
            if (ped == null || !ped.Exists()) return;

            long now = RealTime.Ms;
            int dt = _lastTick == 0 ? 0 : (int)Math.Min(250, now - _lastTick);
            _lastTick = now;

            try
            {
                if (IsStoryModel(ped)) TickAbilityLook(ped);
                else if (_luciaFocus) TickFocus(ped, dt);
                DrawFx();
            }
            catch (Exception ex)
            {
                // An exception out of a tick stops the script for the session;
                // log it, put the world back to normal speed, and carry on.
                Log.Write("Focus", ex.ToString());
                try { Exit(instant: true); } catch { }
            }
        }

        // ---- Michael, Franklin, Trevor: their ability, Focus's look -------------

        private void TickAbilityLook(Ped ped)
        {
            if (_active) Exit(instant: false);   // swapped away from Lucia mid-Focus
            if (!_abilityLook) return;
            bool on = Game.Player.IsSpecialAbilityActive && !ped.IsDead;
            if (on && _replaceAbilityFx)
                foreach (string fx in AbilityFx) Function.Call(Hash.ANIMPOSTFX_STOP, fx);
            if (on == _abilityWasActive) return;
            _abilityWasActive = on;
            if (on)
            {
                Function.Call(Hash.ANIMPOSTFX_PLAY, "FocusIn", 0, true);
                ShowFx(true);
            }
            else
            {
                Function.Call(Hash.ANIMPOSTFX_STOP, "FocusIn");
                Function.Call(Hash.ANIMPOSTFX_PLAY, "FocusOut", 0, false);
                ShowFx(false);
            }
        }

        // ---- everyone else: FiveM's Focus ------------------------------------------

        private void TickFocus(Ped ped, int dt)
        {
            if (_abilityWasActive)
            {
                _abilityWasActive = false;
                ShowFx(false);
            }

            bool pressed = !HudBridge.Paused && !HudBridge.Busy && !HudBridge.HudMenuOpen && !ped.IsDead &&
                (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, SpecialAbility) ||
                 Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 0, SpecialAbilityPc));
            if (pressed)
            {
                if (_active) Exit(instant: false);
                else if (_meter >= _minToActivate) Enter(ped);
            }

            if (_active && (ped.IsDead || HudBridge.Busy)) Exit(instant: true);

            // The meter, on real time: slow motion must not stretch it.
            if (_active)
            {
                _meter = Math.Max(0f, _meter - dt / (_drainSeconds * 1000f) * 100f);
                if (_meter <= 0f) Exit(instant: false);
                // SET_PED_MOVE_RATE_OVERRIDE resets itself unless set every frame:
                // the world slows, the player does not.
                Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped, 1f / _timeScale);
                ped.Accuracy = _accuracyBoost;
            }
            else if (_meter < 100f)
            {
                if (_regenWaitMs > 0) _regenWaitMs = Math.Max(0, _regenWaitMs - dt);
                else _meter = Math.Min(100f, _meter + dt / (_refillSeconds * 1000f) * 100f);
            }

        }

        private void Enter(Ped ped)
        {
            _active = true;
            IsActive = true;
            _accuracyBefore = ped.Accuracy;
            ped.Accuracy = _accuracyBoost;
            Function.Call(Hash.ANIMPOSTFX_PLAY, "FocusIn", 0, true);
            ShowFx(true);
            Ramp(1f, _timeScale);
        }

        private void Exit(bool instant)
        {
            if (!_active) return;
            _active = false;
            IsActive = false;
            _regenWaitMs = _regenDelayMs;
            Ped ped = Game.Player.Character;
            if (ped != null && ped.Exists())
            {
                Function.Call(Hash.SET_PED_MOVE_RATE_OVERRIDE, ped, 1f);
                if (_accuracyBefore >= 0) ped.Accuracy = _accuracyBefore;
            }
            Function.Call(Hash.ANIMPOSTFX_STOP, "FocusIn");
            ShowFx(false);
            if (instant)
            {
                Game.TimeScale = 1f;
                return;
            }
            Function.Call(Hash.ANIMPOSTFX_PLAY, "FocusOut", 0, false);
            Ramp(_timeScale, 1f);
        }

        // SetTimeScale stepped rather than snapped: a wind-down / wind-up.
        private void Ramp(float from, float to)
        {
            for (int i = 1; i <= _rampSteps; i++)
            {
                Game.TimeScale = from + (to - from) * i / _rampSteps;
                long until = RealTime.Ms + _rampStepMs;
                while (RealTime.Ms < until)
                {
                    DrawFx();
                    Wait(0);
                }
            }
            Game.TimeScale = to;
        }

        // ---- #focus-fx ------------------------------------------------------------

        private void ShowFx(bool on)
        {
            if (on == _fxOn) return;
            _fxOn = on;
            _fadeFrom = _fxOpacity;
            _fadeAt = RealTime.Ms;
            if (on) _pulseSince = RealTime.Ms;
        }

        private void DrawFx()
        {
            if (_fadeAt >= 0)
            {
                float t = Num.Clamp01((RealTime.Ms - _fadeAt) / (float)FadeMs);
                _fxOpacity = Num.Lerp(_fadeFrom, _fxOn ? 1f : 0f, Num.Ease(t));
                if (t >= 1f) _fadeAt = -1;
            }
            if (_fxOpacity <= 0.002f) return;
            if (HudBridge.Paused) return;

            // The pulse only runs while active: 0% and 100% plain, 50% at
            // brightness(1 + 0.45p) saturate(1 + 0.35p) scale(1 + 0.05p), ease-in-out.
            float amount = 0f;
            if (_fxOn && _pulseMs > 0)
            {
                float t = (RealTime.Ms - _pulseSince) % _pulseMs / (float)_pulseMs;
                float half = t < 0.5f ? t * 2f : (t - 0.5f) * 2f;
                float e = Num.EaseInOut(half);
                amount = (t < 0.5f ? e : 1f - e) * Num.Clamp01(_pulse);
            }
            int frame = (int)Math.Round(amount * (Frames - 1));
            float scale = 1f + 0.05f * amount;
            string art = Paths.Art("focus_fx_" + frame + ".png");
            float w = Canvas.W, h = Canvas.H;
            float a = _fxOpacity * Num.Clamp01(_strength);
            Canvas.Image(art, w * 0.5f, h * 0.5f, w * scale, h * scale, Canvas.Alpha(Color.White, a), centered: true);
        }
    }
}
