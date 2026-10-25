using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// The FiveM version's aim crosshair and kill mark (vice_hud: the top of
    /// client_overlays.lua, onCrosshair / onCrossFire / onKillMark in app.js,
    /// #crosshair in index.html and style.css), for single player.
    ///
    /// While a gun is aimed: on foot, three ticks (left, right, bottom) that
    /// kick outward on every shot and ease back; in a vehicle, a ring with a
    /// dot. The game's own reticle is hidden only while this one shows. A kill
    /// by the player flashes a four-wedge mark over it: red for a one-hit
    /// headshot, yellow when it took more than one hit, white otherwise.
    ///
    /// vice_hud's third eye (#third-eye, SetThirdEyeActive) is not here: it is
    /// the targeting system's reticle, shown only when ox_target says so, and
    /// single player has no targeting system to drive it.
    ///
    /// GTA6Hud draws hit markers of its own in the same place (GTA6Hud.ini
    /// [Hitmarkers]); turn those off while the kill marks here are on.
    /// </summary>
    public sealed class Crosshair : Script
    {
        // ---- vice_hud's numbers, in canvas px (--w = 1.7778cqh = 19.2) ------------
        private const float W = 19.2f;
        // .ct transform: translate by calc(0.55 * --w + --spread); onCrossFire sets
        // --spread to calc(0.35 * --w) on a shot and back to 0px 220ms after the
        // last one; .ct { transition: transform 160ms ease-out }.
        private const float SpreadKick = 0.35f * W;
        private const int SpreadHoldMs = 220;
        private const float SpreadMs = 160f;
        // #cross-kill: transform translate(-50%, -50%) scale(0.6), opacity 0;
        // .show: scale(1), opacity 1; transition opacity 320ms ease, transform
        // 320ms cubic-bezier(0.2, 0.8, 0.3, 1.3). onKillMark removes .show 550ms
        // after the last kill.
        private const float KillMs = 320f;
        private const int KillHoldMs = 550;
        private const float KillScaleFrom = 0.60029f;

        // SKEL_Head's bone id, what GET_PED_LAST_DAMAGE_BONE hands back.
        private const int HeadBone = 31086;
        // GROUP_SNIPER. A sniper rifle's scope is the game's reticle (HUD component 14), so those are left to the
        // game, as GTA6Hud's own reticle leaves them; hiding it took the scope away (a player: "the scope of the
        // sniper doesnt load").
        private const uint GroupSniper = 0xB7BBD827;
        // A ped's death can land a frame or two after the hit that caused it.
        private const int DeathLagFrames = 15;
        private const int CombatWindowMs = 10000;
        private long _combatUntil;
        // Every ped every frame from a shot or a blow until FullSweepMs after the
        // last, so hits that land a frame or two apart are each counted; the rest of
        // the window a quarter of them a frame (TrackKills).
        private const int FullSweepMs = 2000;
        private long _fullSweepUntil;

        private enum Mode { None, Foot, Vehicle }

        // ---- settings, [Crosshair] in ViExtras.ini ---------------------------------
        private readonly bool _debug;
        // HUD OPTIONS (HudMenu) changes these at once; read from ViExtras.ini at start
        internal static bool On, KillMarksOn;
        private readonly int _armedWith, _reticleComponent;
        private readonly Keys _testKey;

        // ---- the page's state ------------------------------------------------------
        // _active is client_overlays.lua's crossActive; _shown is #crosshair
        // without .hidden, which a kill also sets (onKillMark's show(box, true)).
        private bool _active, _shown;
        private Mode _mode = Mode.None;          // [data-mode]; unset until first aimed
        private bool _wasShooting;               // crossWasShooting
        private long _lastFireMs = -100000;
        private int _lastWeapon, _lastClip = -1;
        private readonly OutputArgument _clipOut = new OutputArgument();   // one for every frame's read

        private readonly CssTransition _spread = new CssTransition(0f, SpreadMs, Num.EaseOut);
        private long _spreadSettleAt;            // crossFireTimer, 0 = none

        private readonly CssTransition _markOpacity = new CssTransition(0f, KillMs, Num.Ease);
        private readonly CssTransition _markScale = new CssTransition(KillScaleFrom, KillMs, x => Num.Bezier(0.2f, 0.8f, 0.3f, 1.3f, x));
        private string _markColour;              // kill-red / kill-yellow / kill-white
        private long _killHideAt;                // killMarkTimer, 0 = none
        private long _forcedUntil;               // see Tick: the kill's own show(box, true)

        private int _testStep;
        private bool _testRequested;
        private bool _artMissingLogged;
        private int _errors;

        // ---- kill tracking -----------------------------------------------------------
        private sealed class Victim
        {
            public int LastHp;        // health + armour when last read
            public bool Armoured;     // had armour when last read
            public int Hits;          // killHits[victim]
            public int LastHitFrame;
            public bool Done;         // dead and dealt with
            public int Seen;
        }

        private readonly Dictionary<int, Victim> _victims = new Dictionary<int, Victim>();
        private int _sweep;

        public Crosshair()
        {
            Paths.Init(BaseDirectory);
            On = Config.Get("Crosshair", "Enabled", true);
            KillMarksOn = Config.Get("Crosshair", "KillMarks", true);
            _armedWith = Config.Get("Crosshair", "ArmedWith", 4);
            _reticleComponent = Config.Get("Crosshair", "ReticleComponent", 14);
            _debug = Config.Get("Crosshair", "Debug", false);
            _testKey = Config.Get("Crosshair", "TestKey", Keys.None);

            Tick += OnTick;
            KeyDown += (s, e) => { if (_testKey != Keys.None && e.KeyCode == _testKey) _testRequested = true; };
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            if (!On) return;
            try
            {
                long now = RealTime.Ms;
                RunTimers(now);

                Player player = Game.Player;
                Ped ped = player.Character;
                if (ped == null || !ped.Exists()) return;
                bool blocked = Blocked();

                // ---- client_overlays.lua, the crosshair loop ----
                bool armedGun = Function.Call<bool>(Hash.IS_PED_ARMED, ped, _armedWith);
                bool shootingNow = Function.Call<bool>(Hash.IS_PED_SHOOTING, ped);
                // Locked on (the game's Assisted Aim and Traditional GTA targeting) isn't free aiming, but the
                // gun is up all the same; GTA6Hud hides the game's reticle, so without this there was nothing
                // on screen until the first shot.
                bool freeAiming = Function.Call<bool>(Hash.IS_PLAYER_FREE_AIMING, player)
                                  || Function.Call<bool>(Hash.IS_PLAYER_TARGETTING_ANYTHING, player);
                bool scoped = armedGun && Function.Call<uint>(Hash.GET_WEAPONTYPE_GROUP,
                    Function.Call<uint>(Hash.GET_SELECTED_PED_WEAPON, ped)) == GroupSniper;
                bool aiming = !blocked && armedGun && !scoped && (freeAiming || shootingNow);
                Mode mode = Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, ped, false) ? Mode.Vehicle : Mode.Foot;

                if (aiming)
                {
                    if (_reticleComponent >= 0) Function.Call(Hash.HIDE_HUD_COMPONENT_THIS_FRAME, _reticleComponent);
                    if (!_active || mode != _mode)
                    {
                        _active = true;
                        OnCrosshair(true, mode);
                    }
                }
                else if (_active)
                {
                    _active = false;
                    OnCrosshair(false, mode);
                }

                // Foot reticle only: the vehicle ring has no spread.
                bool shooting = aiming && mode == Mode.Foot && shootingNow;
                bool fired = shooting && !_wasShooting;
                _wasShooting = shooting;
                // IS_PED_SHOOTING can stay up through a burst of automatic fire,
                // and then only the first round would kick the ticks out; the
                // round leaving the clip catches the rest. Each one re-arms the
                // settle, as onCrossFire's own clearTimeout does.
                // Only with a gun in hand: without one there is no clip to watch.
                int weapon = 0, clip = -1;
                if (armedGun)
                {
                    weapon = Function.Call<int>(Hash.GET_SELECTED_PED_WEAPON, ped);
                    if (Function.Call<bool>(Hash.GET_AMMO_IN_CLIP, ped, weapon, _clipOut)) clip = _clipOut.GetResult<int>();
                }
                if (aiming && mode == Mode.Foot && weapon == _lastWeapon && clip >= 0 && _lastClip >= 0 && clip < _lastClip && now - _lastFireMs > 50)
                    fired = true;
                _lastWeapon = weapon;
                _lastClip = clip;
                if (fired) OnCrossFire(now);

                // ---- the kill mark: the gameEventTriggered handler ----
                // The ped sweep below reads the peds each frame, so it only runs
                // while the player is fighting: shooting, aiming, in melee, or
                // within CombatWindowMs of having been (a grenade's fuse, a body
                // going down a moment after the last hit). Out of a fight the
                // records are dropped and start afresh at the next one.
                bool fighting = shootingNow || Function.Call<bool>(Hash.IS_PED_IN_MELEE_COMBAT, ped);
                if (fighting) _fullSweepUntil = now + FullSweepMs;
                if (fighting || freeAiming) _combatUntil = now + CombatWindowMs;
                if (KillMarksOn && now < _combatUntil) TrackKills(ped, now, now < _fullSweepUntil);
                else if (_victims.Count > 0) _victims.Clear();

                if (_testRequested)
                {
                    _testRequested = false;
                    // /hudkillmark: the foot reticle, then the next colour in turn.
                    if (!_active) _mode = Mode.Foot;
                    string[] order = { "headshot", "sloppy", "clean" };
                    OnKillMark(order[_testStep++ % order.Length], now);
                }

                // The kill's show(box, true) was never undone in vice_hud unless the
                // crosshair went from active to inactive afterwards, which left the
                // reticle standing on screen after, say, a kill with no gun up. Here
                // it lasts as long as the mark's own flash and fade.
                if (_shown && !_active && _forcedUntil > 0 && now >= _forcedUntil) Show(false);

                if (!blocked) Draw(now);
            }
            catch (Exception ex)
            {
                // Every frame runs this; one fault should not fill the log.
                if (_errors++ < 5) Log.Write("Crosshair", ex.ToString());
            }
        }

        // Nothing of ours over a pause menu, a cutscene, a GTA6Hud menu or
        // camera, or a camera another feature is filming with.
        private static bool Blocked()
        {
            if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen) return true;
            if (CrashCam.IsPlaying || Binoculars.IsActive) return true;
            Camera cam = World.RenderingCamera;
            return cam != null && cam.Exists() && !FollowsGameplay(cam);
        }

        // GTA6Hud's over-the-shoulder view (GTA6Hud.ini [Shoulder]) is a scripted
        // camera too, but it is the aim view itself moved sideways: the game
        // camera's own direction and field of view, a few tenths of a metre off.
        // The aim point is still the middle of the screen, so keep drawing.
        private static bool FollowsGameplay(Camera cam)
        {
            Vector3 a = cam.Direction, b = GameplayCamera.Direction;
            float cos = Vector3.Dot(a.Normalized, b.Normalized);
            return cos > 0.906f   // within 25 degrees: a frame behind on a quick turn
                && Math.Abs(cam.FieldOfView - GameplayCamera.FieldOfView) < 5f
                && cam.Position.DistanceTo(GameplayCamera.Position) < 3f;
        }

        // ---- app.js ------------------------------------------------------------------

        // onCrosshair
        private void OnCrosshair(bool active, Mode mode)
        {
            if (!active)
            {
                Show(false);
                return;
            }
            if (mode != _mode && mode != Mode.Foot) _spread.Snap();   // #cross-tips goes display:none
            _mode = mode;
            Show(true);
        }

        // show($('crosshair'), visible). display:none cancels every transition
        // inside, and nothing transitions in from it.
        private void Show(bool visible)
        {
            if (_shown == visible) return;
            _shown = visible;
            _forcedUntil = 0;
            if (!visible)
            {
                _spread.Snap();
                _markOpacity.Snap();
                _markScale.Snap();
            }
        }

        // onCrossFire
        private void OnCrossFire(long now)
        {
            _lastFireMs = now;
            _spread.Set(SpreadKick, now, _shown && _mode == Mode.Foot);
            _spreadSettleAt = now + SpreadHoldMs;
        }

        // onKillMark
        private void OnKillMark(string quality, long now)
        {
            bool wasShown = _shown;
            if (!_shown) Show(true);
            // classList.remove(...KILL_CLASSES, 'show'); void x.offsetWidth;
            _markOpacity.Set(0f, now, wasShown);
            _markScale.Set(KillScaleFrom, now, wasShown);
            // classList.add(cls, 'show')
            _markColour = quality == "headshot" ? "red" : quality == "sloppy" ? "yellow" : "white";
            _markOpacity.Set(1f, now, true);
            _markScale.Set(1f, now, true);
            _killHideAt = now + KillHoldMs;
            _forcedUntil = _killHideAt + (long)KillMs;
        }

        // The page's two setTimeouts, each run at the moment it was due.
        private void RunTimers(long now)
        {
            if (_spreadSettleAt > 0 && now >= _spreadSettleAt)
            {
                _spread.Set(0f, _spreadSettleAt, _shown && _mode == Mode.Foot);
                _spreadSettleAt = 0;
            }
            if (_killHideAt > 0 && now >= _killHideAt)
            {
                _markOpacity.Set(0f, _killHideAt, _shown);
                _markScale.Set(KillScaleFrom, _killHideAt, _shown);
                _killHideAt = 0;
            }
        }

        // ---- drawing -------------------------------------------------------------------

        private void Draw(long now)
        {
            if (!_shown) return;
            float cx = Canvas.W * 0.5f, cy = Canvas.H * 0.5f;

            if (_mode == Mode.Vehicle)
            {
                DrawCentred("xhair_vehicle", cx, cy, 1f, Color.White);
            }
            else if (_mode == Mode.Foot)
            {
                float s = _spread.At(now);
                DrawCentred("xhair_tick_left", cx - s, cy, 1f, Color.White);
                DrawCentred("xhair_tick_right", cx + s, cy, 1f, Color.White);
                DrawCentred("xhair_tick_bottom", cx, cy + s, 1f, Color.White);
            }

            float opacity = _markOpacity.At(now);
            if (_markColour != null && opacity > 0f)
                DrawCentred("xhair_kill_" + _markColour, cx, cy, _markScale.At(now),
                    Color.FromArgb((int)Num.Clamp(opacity * 255f, 0f, 255f), 255, 255, 255));
        }

        // Every crosshair picture is a square centred on the screen centre
        // (tools/render/crosshair.py), so each is drawn centred on its spot.
        private void DrawCentred(string name, float x, float y, float scale, Color color)
        {
            Art.Entry e = Art.Get(name);
            if (e == null)
            {
                if (!_artMissingLogged) Log.Write("Crosshair", "missing art " + name + " (Art\\manifest_crosshair.txt)");
                _artMissingLogged = true;
                return;
            }
            Canvas.Image(e.Path, x, y, e.W * scale, e.H * scale, color, centered: true);
        }

        // ---- kills ---------------------------------------------------------------------
        //
        // vice_hud counts CEventNetworkEntityDamage events with the player as the
        // attacker, per victim, and on the one that leaves the victim dead or
        // dying asks GET_PED_LAST_DAMAGE_BONE whether it was the head:
        //   hits > 1                   -> sloppy   (yellow)
        //   one hit, and it the head   -> headshot (red)
        //   otherwise                  -> clean    (white)
        // Single player has no damage events, so a hit is a frame in which a
        // ped's health and armour went down while the player has damaged it,
        // and a kill is a ped going down with the player as its source of death.
        //
        // `full`: every ped is read this frame. Otherwise each is read every fourth
        // frame (its handle picks which): a death is still found within four
        // frames, and one landing a frame or two after the hit is still inside
        // DeathLagFrames. A ped seen for the first time is read at once. Armour is
        // only read for peds that had some: the game and GTA6Hud give it as a ped
        // is made, which is when a ped is first read.
        private void TrackKills(Ped player, long now, bool full)
        {
            int me = player.Handle;
            int frame = Game.FrameCount;
            int sweep = ++_sweep;
            foreach (Ped p in World.GetAllPeds())
            {
                if (p == null) continue;
                int h = p.Handle;
                if (h == me) continue;

                if (!_victims.TryGetValue(h, out Victim v))
                {
                    // First sight: a body already on the floor is no kill of ours.
                    int armour = Function.Call<int>(Hash.GET_PED_ARMOUR, h);
                    _victims[h] = new Victim
                    {
                        LastHp = Function.Call<int>(Hash.GET_ENTITY_HEALTH, h) + armour,
                        Armoured = armour > 0,
                        Done = Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, h, true),
                        Seen = sweep,
                    };
                    continue;
                }
                v.Seen = sweep;
                if (!full && ((h ^ (h >> 8)) & 3) != (frame & 3)) continue;
                int health = Function.Call<int>(Hash.GET_ENTITY_HEALTH, h);

                if (v.Done)
                {
                    // Back on its feet, or the handle went to a new ped: count afresh,
                    // as killHits does for a target that comes back.
                    if (health > 0 && !Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, h, true))
                    {
                        int armour = Function.Call<int>(Hash.GET_PED_ARMOUR, h);
                        v.Done = false;
                        v.Hits = 0;
                        v.Armoured = armour > 0;
                        v.LastHp = health + armour;
                    }
                    continue;
                }

                int hp = health;
                if (v.Armoured)
                {
                    int armour = Function.Call<int>(Hash.GET_PED_ARMOUR, h);
                    v.Armoured = armour > 0;
                    hp += armour;
                }
                bool dropped = hp < v.LastHp;
                v.LastHp = hp;
                if (dropped && Function.Call<bool>(Hash.HAS_ENTITY_BEEN_DAMAGED_BY_ENTITY, h, me, true))
                {
                    v.Hits++;
                    v.LastHitFrame = frame;
                }

                bool recentHit = v.Hits > 0 && frame - v.LastHitFrame <= DeathLagFrames;
                if (!dropped && !recentHit) continue;
                if (!Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, h, true)) continue;

                v.Done = true;
                int source = Function.Call<int>(Hash.GET_PED_SOURCE_OF_DEATH, h);
                bool ours = source == me || (source == 0 && recentHit);
                if (!ours) continue;

                int hits = Math.Max(v.Hits, 1);
                var boneOut = new OutputArgument();
                bool boneOk = Function.Call<bool>(Hash.GET_PED_LAST_DAMAGE_BONE, h, boneOut);
                int bone = boneOk ? boneOut.GetResult<int>() : -1;
                bool headshot = boneOk && bone == HeadBone;

                string quality = "clean";
                if (hits > 1) quality = "sloppy";
                else if (headshot) quality = "headshot";
                OnKillMark(quality, now);
                if (_debug) Log.Write("Crosshair", "kill: hits=" + hits + " headshot=" + headshot + " bone=" + bone + " -> " + quality);
            }

            // Peds the game has let go of.
            if (sweep % 120 == 0 && _victims.Count > 0)
            {
                var gone = new List<int>();
                foreach (KeyValuePair<int, Victim> kv in _victims)
                    if (kv.Value.Seen != sweep) gone.Add(kv.Key);
                foreach (int h in gone) _victims.Remove(h);
            }
        }

        /// <summary>
        /// One CSS transition on one property, as the browser runs it: a change
        /// of value starts from wherever the property is now, and a change back
        /// to where a running transition came from is shortened the way the CSS
        /// Transitions spec reverses one.
        /// </summary>
        private sealed class CssTransition
        {
            private readonly float _ms;
            private readonly Func<float, float> _timing;
            private float _from, _to, _revStart, _factor = 1f, _dur;
            private long _start;
            private bool _running;

            public CssTransition(float value, float ms, Func<float, float> timing)
            {
                _from = _to = _revStart = value;
                _ms = ms;
                _timing = timing;
            }

            public float At(long now)
            {
                if (!_running) return _to;
                float p = (now - _start) / _dur;
                if (p >= 1f)
                {
                    _running = false;
                    return _to;
                }
                return p <= 0f ? _from : _from + (_to - _from) * _timing(p);
            }

            /// <summary>The property becomes `value` at `now`. Not rendered: no transition.</summary>
            public void Set(float value, long now, bool rendered)
            {
                float current = At(now);
                if (!rendered)
                {
                    _running = false;
                    _from = _to = value;
                    return;
                }
                if (value == _to) return;
                if (_running)
                {
                    if (current == value)
                    {
                        _running = false;
                        _from = _to = value;
                        return;
                    }
                    if (value == _revStart)
                    {
                        float p = Num.Clamp01((now - _start) / _dur);
                        float factor = Num.Clamp01(Math.Abs(_timing(p) * _factor + 1f - _factor));
                        Begin(current, value, now, factor, _to);
                        return;
                    }
                }
                Begin(current, value, now, 1f, current);
            }

            private void Begin(float from, float to, long now, float factor, float revStart)
            {
                _from = from;
                _to = to;
                _start = now;
                _factor = factor;
                _revStart = revStart;
                _dur = _ms * factor;
                _running = _dur > 0f && from != to;
            }

            /// <summary>display:none: whatever was running is over, at its end value.</summary>
            public void Snap()
            {
                _running = false;
                _from = _to;
            }
        }
    }
}
