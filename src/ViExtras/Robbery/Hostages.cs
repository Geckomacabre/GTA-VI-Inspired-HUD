using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GTA;
using GTA.Math;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// Hostages in GTA6Hud's bank robberies (the six Fleecas and Pacific Standard) and at Vangelico. GTA6Hud.dll itself
    /// is not changed: its robbery job is read and written here by reflection.
    ///
    ///  * Take a hostage: aim at one of the job's witnesses (a customer or teller, cowering or not) and hold E. They
    ///    play GTA Online's crowd control from the heists (fm_mission_controller.c): down on their knees (intro_ped_x),
    ///    kneeling (idle), flinching while you aim at them, cowering under fire, and the hit reactions if they're hurt.
    ///    The Fleecas use the Fleeca's own three (ped_a, ped_d standing, ped_e); Pacific Standard and Vangelico the
    ///    Pacific's eight. A hostage is no longer a witness, so they can't phone the police, and GTA6Hud's street
    ///    mugging leaves them alone.
    ///  * The police hold off: GTA6Hud's police wait outside 90 s (its standoff) before they storm in. Each hostage
    ///    taken adds [Hostages] SecondsPerHostage. Hurting a hostage, by anyone, makes them storm at once.
    ///  * Let one go: look at a kneeling hostage without aiming and hold E. They get up and run, and the police wait
    ///    [Hostages] SecondsPerRelease longer.
    ///  * Human shield: aim a pistol at a kneeling hostage and hold E. You hold them in front of you at gunpoint, with
    ///    the Doomsday Heist's pair (anim@gangops@hostage@ perp_idle and victim_idle), at the place the pair's own
    ///    clips put them, and walk. Police near you aim but hold their fire. E lets them go (they run, as a release);
    ///    if they're hurt, the hold breaks.
    ///
    /// When the job ends, every hostage gets up and runs. [Switches] Hostages turns it off.
    /// </summary>
    public sealed class Hostages : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Stat = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const string OrnateIntro = "anim@heists@ornate_bank@hostages@intro";
        private const string FleecaIntro = "anim@heists@fleeca_bank@hostages@intro";
        private const string HitDict = "anim@heists@ornate_bank@hostages@hit";
        private const string ShieldDict = "anim@gangops@hostage@";
        private const float TakeReach = 4f, NearReach = 1.8f, HoldSeconds = 0.6f, CopReach = 60f;

        // the Fleeca's three hostages, and the Pacific's eight (ped_a last: it has no flinch_to_underfire or
        // flinch_under_fire_outro)
        private static readonly string[] FleecaTypes = { "ped_a", "ped_d", "ped_e" };
        private static readonly string[] PacificTypes = { "ped_b", "ped_c", "ped_d", "ped_e", "ped_f", "ped_g", "ped_h", "ped_a" };

        private enum Pose { Intro, Idle, FlinchIn, Flinch, FlinchOut, FireIn, Fire, FireOut, Hit, HitOut, Shield }

        private sealed class Hostage
        {
            public Ped Ped;
            public string Type, IntroDict, IntroClip, Dict;
            public bool FullSet;              // has flinch_to_underfire and flinch_under_fire_outro
            public Pose Pose;
            public string ClipDict, Clip;
            public int PoseAt, AimedAt, ShotAt, Health;
        }

        private readonly bool _enabled, _shieldOn;
        private readonly int _perHostage, _perRelease;
        private readonly Random _rng = new Random();
        private readonly List<Hostage> _hostages = new List<Hostage>();
        private object _jobSeen;
        private int _handedOut;          // hostage types given out this job
        private int _standoffBase;       // RobJob.BreachAt as GTA6Hud set it when its police arrived; 0 before
        private int _bonus;              // ms the police wait longer
        private bool _stormed, _stormSent;
        private Hostage _shield;
        private readonly List<Ped> _heldCops = new List<Ped>();
        private int _copScanAt;
        private int _holdId;
        private float _hold;
        private bool _holdAgain;
        private bool _letGoDown;
        private int _errors;

        // GTA6Hud
        private bool _looked;
        private object _hud;
        private FieldInfo _job, _robbed, _robVictim;
        private FieldInfo _kind, _ended, _policeHere, _breachAt, _police, _witnesses, _wPed, _wState;
        private MethodInfo _stopRobbery;
        private Func<Vector3, bool> _focused;
        private Func<bool> _holdDown;
        private Action<float, float, string, string, string, bool, bool, bool> _drawMarker;
        private Action<float, float, float, float> _drawRim;
        private Func<float, float, string, string, float> _drawHint;
        private Func<string, string, string> _btn;
        private Func<float> _canvasWidth;
        private Func<float, float> _u;
        private Action<string> _robLog, _notify;

        public Hostages()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get(Config.Switches, "Hostages", true);
            _perHostage = (int)(Num.Clamp(Config.Get("Hostages", "SecondsPerHostage", 60f), 0f, 600f) * 1000f);
            _perRelease = (int)(Num.Clamp(Config.Get("Hostages", "SecondsPerRelease", 45f), 0f, 600f) * 1000f);
            _shieldOn = Config.Get("Hostages", "HumanShield", true);
            if (!_enabled) return;
            Tick += OnTick;
            Aborted += (s, e) => { try { LetEveryoneGo(); } catch { } };
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now || Game.IsPaused || !Look()) return;
            try
            {
                Run();
            }
            catch (Exception ex)
            {
                if (_errors++ < 5) Log.Write("Hostages", ex.ToString());
                try { LetEveryoneGo(); } catch { }
                _jobSeen = null;
            }
        }

        private void Run()
        {
            Ped player = Game.Player.Character;
            int now = Game.GameTime;
            object j = _job.GetValue(_hud);
            string kind = j == null ? null : (string)_kind.GetValue(j);
            bool ours = kind == "bank" || kind == "pacific" || kind == "jewel";
            if (!ReferenceEquals(j, _jobSeen))
            {
                LetEveryoneGo();
                _jobSeen = ours ? j : null;
                _handedOut = 0;
                _standoffBase = 0;
                _bonus = 0;
                _stormed = _stormSent = false;
                if (ours)
                {
                    Function.Call(Hash.REQUEST_ANIM_DICT, kind == "bank" ? FleecaIntro : OrnateIntro);
                    Function.Call(Hash.REQUEST_ANIM_DICT, HitDict);
                    if (_shieldOn) Function.Call(Hash.REQUEST_ANIM_DICT, ShieldDict);
                }
            }
            if (!ours) return;
            if ((bool)_ended.GetValue(j) || player.IsDead)
            {
                if (_hostages.Count > 0) LetEveryoneGo();
                return;
            }

            // the police's wait: GTA6Hud's 90 s, and more for each hostage
            IList police = (IList)_police.GetValue(j);
            if ((bool)_policeHere.GetValue(j) && police.Count > 0)
            {
                if (_standoffBase == 0)
                {
                    _standoffBase = (int)_breachAt.GetValue(j);
                    if (_hostages.Count > 0 || _bonus > 0)
                        RobLog("hostages: the police wait " + (_bonus / 1000) + " s longer");
                }
                if (_stormed)
                {
                    if (!_stormSent)
                    {
                        _breachAt.SetValue(j, now);
                        _stormSent = true;
                    }
                }
                else if ((int)_breachAt.GetValue(j) != _standoffBase + _bonus)
                    _breachAt.SetValue(j, _standoffBase + _bonus);
            }

            bool shooting = player.IsShooting;
            for (int i = _hostages.Count - 1; i >= 0; i--)
                UpdateHostage(_hostages[i], player, now, shooting);
            if (_shield != null)
            {
                UpdateShield(player, now, police);
                return;
            }
            Prompts(player, j);
        }

        // ---- taking, letting go, the shield: GTA6Hud's own hold-E markers ----

        private void Prompts(Ped player, object j)
        {
            bool armed = Armed(player);
            bool aiming = armed && AimHeld();
            Hostage near = null;
            float best = NearReach;
            foreach (Hostage h in _hostages)
            {
                if (h.Pose == Pose.Shield || !h.Ped.Exists() || h.Ped.IsDead) continue;
                float d = Vector3.Distance(player.Position, h.Ped.Position);
                if (d >= NearReach || !_focused(h.Ped.Position + new Vector3(0f, 0f, 0.4f))) continue;
                // the one whose E is being held stays chosen (Sticky)
                if (h.Ped.Handle == _holdId) { near = h; break; }
                if (d < best) { best = d; near = h; }
            }
            if (near != null && !aiming)
            {
                if (HoldAt(near.Ped.Position + new Vector3(0f, 0f, 0.4f), "LET GO", near.Ped.Handle))
                    LetGo(near, true, "let go");
                return;
            }
            if (near != null && aiming && _shieldOn && Pistol(player) && !player.IsInVehicle())
            {
                if (HoldAt(near.Ped.Position + new Vector3(0f, 0f, 0.4f), "HUMAN SHIELD", near.Ped.Handle))
                    StartShield(near, player);
                return;
            }
            if (!aiming || player.IsInVehicle())
            {
                _holdId = 0;
                return;
            }
            IList witnesses = (IList)_witnesses.GetValue(j);
            object target = null;
            Ped targetPed = null;
            best = TakeReach;
            foreach (object w in witnesses)
            {
                Ped p = (Ped)_wPed.GetValue(w);
                int state = (int)_wState.GetValue(w);
                if (state > 1 || p == null || !p.Exists() || p.IsDead || p.IsInVehicle() || p.IsRagdoll) continue;
                float d = Vector3.Distance(player.Position, p.Position);
                if (d >= TakeReach || !_focused(p.Position + new Vector3(0f, 0f, 0.6f))) continue;
                // Sticky: two people side by side (Vangelico's manageress and the customer she talks to) are each
                // the nearer one by turns as they move; switching restarted the hold every time, so it never filled.
                // The one the prompt is on stays chosen while it's still in reach and looked at.
                if (p.Handle == _holdId) { target = w; targetPed = p; break; }
                if (d < best) { best = d; target = w; targetPed = p; }
            }
            if (target == null)
            {
                _holdId = 0;
                return;
            }
            if (HoldAt(targetPed.Position + new Vector3(0f, 0f, 0.6f), "TAKE HOSTAGE", targetPed.Handle))
                Take(j, witnesses, target, targetPed);
        }

        // RobHoldAt's way: the marker, the rim filling while E is held, and done once
        private bool HoldAt(Vector3 at, string label, int id)
        {
            if (!GTA.UI.Screen.IsSphereVisible(at, 0.4f)) return false;
            var p = GTA.UI.Screen.WorldToScreen(at, true);
            if (p.X <= 0f && p.Y <= 0f) return false;
            float x = p.X * 1.5f, y = p.Y * 1.5f;   // GTA6Hud's FromSprite
            if (_holdId != id)
            {
                _holdId = id;
                _hold = 0f;
            }
            _drawMarker(x, y, label, "square", "E", true, false, true);
            bool down = _holdDown();
            if (!down) _holdAgain = false;
            _hold = down && !_holdAgain ? _hold + Math.Min(Game.LastFrameTime, 0.1f) : 0f;
            if (_hold > 0f) _drawRim?.Invoke(x, y, _u(30f), Num.Clamp01(_hold / HoldSeconds));
            if (_hold < HoldSeconds) return false;
            // let go of E before the next one: taking a hostage while still holding it doesn't go on into the shield
            _hold = 0f;
            _holdAgain = true;
            return true;
        }

        private void Take(object j, IList witnesses, object w, Ped p)
        {
            witnesses.Remove(w);
            if (_robVictim?.GetValue(_hud) is Ped mugged && mugged == p) _stopRobbery?.Invoke(_hud, null);
            if (_robbed?.GetValue(_hud) is IDictionary robbed) robbed[p.Handle] = p;
            string kind = (string)_kind.GetValue(j);
            bool fleeca = kind == "bank";
            string[] types = fleeca ? FleecaTypes : PacificTypes;
            string t = types[_handedOut++ % types.Length];
            var h = new Hostage
            {
                Ped = p,
                Type = t,
                IntroDict = fleeca ? FleecaIntro : OrnateIntro,
                IntroClip = fleeca && t == "ped_d" ? "intro_standing_ped_d" : "intro_" + t,
                Dict = fleeca && t == "ped_d" ? "anim@heists@fleeca_bank@hostages@ped_d@" : "anim@heists@ornate_bank@hostages@" + t + "@",
                FullSet = t != "ped_a",
                Health = p.Health,
            };
            Function.Call(Hash.REQUEST_ANIM_DICT, h.Dict);
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, p, true);
            p.Task.ClearAll();
            _hostages.Add(h);
            Play(h, Pose.Intro);
            _bonus += _perHostage;
            RobLog("hostage taken (" + _hostages.Count + ", " + t + ")");
            Log.Write("Hostages", "hostage taken at " + kind + ": " + _hostages.Count + " held, the police wait " + (_bonus / 1000) + " s longer");
            if (_perHostage > 0) Say("Hostage taken: the police hold off " + (_perHostage / 1000) + " s longer");
        }

        private void LetGo(Hostage h, bool release, string why)
        {
            Ped player = Game.Player.Character;
            if (h == _shield) EndShield(player);
            _hostages.Remove(h);
            if (h.Ped != null && h.Ped.Exists() && !h.Ped.IsDead)
            {
                h.Ped.Task.ClearAll();
                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, h.Ped, false);
                Function.Call(Hash.TASK_SMART_FLEE_PED, h.Ped, player, 150f, -1, false, false);
            }
            if (!release) return;
            _bonus += _perRelease;
            RobLog("hostage released (" + why + "), " + _hostages.Count + " left");
            if (_perRelease > 0) Say("Hostage released: the police hold off " + (_perRelease / 1000) + " s longer");
        }

        private void LetEveryoneGo()
        {
            for (int i = _hostages.Count - 1; i >= 0; i--) LetGo(_hostages[i], false, "the job ended");
            if (_shield != null) EndShield(Game.Player.Character);
            FreeCops();
            _holdId = 0;
        }

        // a hostage hurt, by anyone: the police come in
        private void Harmed(bool dead)
        {
            RobLog(dead ? "a hostage was killed" : "a hostage was hurt");
            if (_stormed) return;
            _stormed = true;
            Log.Write("Hostages", "a hostage was " + (dead ? "killed" : "hurt") + ": the police storm");
            Say("You hurt a hostage: the police are coming in");
        }

        // ---- a kneeling hostage: GTA Online's crowd control ----

        private void UpdateHostage(Hostage h, Ped player, int now, bool shooting)
        {
            if (h.Pose == Pose.Shield) return;
            Ped p = h.Ped;
            if (p == null || !p.Exists() || p.IsDead)
            {
                _hostages.Remove(h);
                if (p != null && p.Exists()) Harmed(true);
                return;
            }
            int health = p.Health;
            if (health < h.Health)
            {
                h.Health = health;
                Harmed(false);
                Play(h, Pose.Hit);
                return;
            }
            h.Health = health;
            bool aimed = Armed(player) && AimHeld() && Vector3.Distance(player.Position, p.Position) < 15f
                && LookingAt(p.Position + new Vector3(0f, 0f, 0.5f), 0.97f);
            if (aimed) h.AimedAt = now;
            bool fire = shooting && Vector3.Distance(player.Position, p.Position) < 30f
                || Function.Call<bool>(Hash.HAS_BULLET_IMPACTED_IN_AREA, p.Position.X, p.Position.Y, p.Position.Z, 10f, false, false);
            if (fire) h.ShotAt = now;
            bool firing = now - h.ShotAt < 3000;   // the under-fire loop holds 3 s after the last shot
            switch (h.Pose)
            {
                case Pose.Intro:
                    if (Done(h, now)) Play(h, Pose.Idle);
                    break;
                case Pose.Idle:
                    if (fire) Play(h, Pose.FireIn);
                    else if (aimed) Play(h, Pose.FlinchIn);
                    else Keep(h, now);
                    break;
                case Pose.FlinchIn:
                    if (fire) Play(h, Pose.FireIn);
                    else if (Done(h, now)) Play(h, Pose.Flinch);
                    break;
                case Pose.Flinch:
                    if (fire) Play(h, Pose.FireIn);
                    else if (now - h.AimedAt > 400) Play(h, Pose.FlinchOut);
                    else Keep(h, now);
                    break;
                case Pose.FlinchOut:
                case Pose.FireOut:
                case Pose.HitOut:
                    if (Done(h, now)) Play(h, Pose.Idle);
                    break;
                case Pose.FireIn:
                    if (Done(h, now)) Play(h, Pose.Fire);
                    break;
                case Pose.Fire:
                    if (!firing) Play(h, Pose.FireOut);
                    else Keep(h, now);
                    break;
                case Pose.Hit:
                    if (Done(h, now)) Play(h, Pose.HitOut);
                    break;
            }
        }

        private void Play(Hostage h, Pose pose)
        {
            string dict = h.Dict, clip;
            int flags = 2;   // hold the last frame
            switch (pose)
            {
                case Pose.Intro: dict = h.IntroDict; clip = h.IntroClip; break;
                case Pose.Idle: clip = "idle"; flags = 1; break;
                case Pose.FlinchIn: clip = "flinch_intro"; break;
                case Pose.Flinch: clip = "flinch_loop"; flags = 1; break;
                case Pose.FlinchOut: clip = "flinch_outro"; break;
                case Pose.FireIn: clip = h.Pose == Pose.Flinch && h.FullSet ? "flinch_to_underfire" : "flinch_under_fire_intro"; break;
                case Pose.Fire: clip = "flinch_loop_underfire"; flags = 1; break;
                case Pose.FireOut: clip = h.FullSet ? "flinch_under_fire_outro" : "flinch_outro"; break;
                case Pose.Hit: dict = HitDict; clip = "hit_react_" + (_rng.Next(2) == 0 ? "a_" : "b_") + h.Type; break;
                case Pose.HitOut: dict = HitDict; clip = "hit_react_outro_" + h.Type; break;
                default: return;
            }
            h.Pose = pose;
            h.ClipDict = dict;
            h.Clip = clip;
            h.PoseAt = Game.GameTime;
            Function.Call(Hash.REQUEST_ANIM_DICT, dict);
            if (Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, dict))
                Function.Call(Hash.TASK_PLAY_ANIM, h.Ped, dict, clip, 8f, -8f, -1, flags, 0f, false, false, false);
            else
                h.Clip = null;   // Keep / Done play it once it's in
        }

        // a clip played through (or one that never started, after 6 s)
        private bool Done(Hostage h, int now)
        {
            if (h.Clip == null) return Retry(h, now);
            if (now - h.PoseAt < 250) return false;
            if (!Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, h.Ped, h.ClipDict, h.Clip, 3)) return true;
            return Function.Call<float>(Hash.GET_ENTITY_ANIM_CURRENT_TIME, h.Ped, h.ClipDict, h.Clip) >= 0.98f || now - h.PoseAt > 15000;
        }

        // a loop: played again if something stopped it
        private void Keep(Hostage h, int now)
        {
            if (h.Clip == null) { Retry(h, now); return; }
            if (now - h.PoseAt > 500 && !Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, h.Ped, h.ClipDict, h.Clip, 3))
                Play(h, h.Pose);
        }

        private bool Retry(Hostage h, int now)
        {
            if (now - h.PoseAt > 6000) return true;
            Pose pose = h.Pose;
            int at = h.PoseAt;
            Play(h, pose);
            if (h.Clip == null) h.PoseAt = at;
            return false;
        }

        // ---- the human shield ----

        private void StartShield(Hostage h, Ped player)
        {
            Function.Call(Hash.REQUEST_ANIM_DICT, ShieldDict);
            if (!Function.Call<bool>(Hash.HAS_ANIM_DICT_LOADED, ShieldDict)) return;
            // where the pair's clips put the victim, seen from the perp: one synced scene's two starting places
            Vector3 zero = Vector3.Zero;
            Vector3 perp = Function.Call<Vector3>(Hash.GET_ANIM_INITIAL_OFFSET_POSITION, ShieldDict, "perp_idle", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 2);
            Vector3 victim = Function.Call<Vector3>(Hash.GET_ANIM_INITIAL_OFFSET_POSITION, ShieldDict, "victim_idle", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 2);
            Vector3 perpRot = Function.Call<Vector3>(Hash.GET_ANIM_INITIAL_OFFSET_ROTATION, ShieldDict, "perp_idle", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 2);
            Vector3 victimRot = Function.Call<Vector3>(Hash.GET_ANIM_INITIAL_OFFSET_ROTATION, ShieldDict, "victim_idle", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 2);
            Vector3 d = victim - perp;
            double r = -perpRot.Z * Math.PI / 180.0;
            var local = new Vector3((float)(d.X * Math.Cos(r) - d.Y * Math.Sin(r)), (float)(d.X * Math.Sin(r) + d.Y * Math.Cos(r)), d.Z);
            float turn = victimRot.Z - perpRot.Z;
            if (local.Length() < 0.05f || local.Length() > 1.2f)
            {
                Log.Write("Hostages", "the shield pair's offset looked wrong (" + local + "), using FiveM's take-hostage one");
                local = new Vector3(-0.24f, 0.11f, 0f);
                turn = 0f;
            }
            Ped p = h.Ped;
            p.Task.ClearAll();
            Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, p, player, 0, local.X, local.Y, local.Z, 0f, 0f, turn, false, false, false, false, 2, true, 0);
            Function.Call(Hash.TASK_PLAY_ANIM, p, ShieldDict, "victim_idle", 8f, -8f, -1, 1, 0f, false, false, false);
            Function.Call(Hash.TASK_PLAY_ANIM, player, ShieldDict, "perp_idle", 8f, -8f, -1, 49, 0f, false, false, false);
            h.Pose = Pose.Shield;
            h.Health = p.Health;
            h.PoseAt = Game.GameTime;
            _shield = h;
            _letGoDown = true;   // the E that took them
            _copScanAt = 0;
            RobLog("hostage held as a shield");
            Log.Write("Hostages", "human shield: victim at " + local + ", turned " + turn.ToString("0.#"));
        }

        private void UpdateShield(Ped player, int now, IList standoffCops)
        {
            Hostage h = _shield;
            Ped p = h.Ped;
            if (p == null || !p.Exists() || p.IsDead || p.Health < h.Health)
            {
                bool dead = p == null || !p.Exists() || p.IsDead;
                EndShield(player);
                _hostages.Remove(h);
                if (p != null && p.Exists() && !dead)
                {
                    Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, p, false);
                    Function.Call(Hash.SET_PED_TO_RAGDOLL, p, 1500, 2500, 0, false, false, false);
                }
                Harmed(dead);
                return;
            }
            if (player.IsRagdoll || player.IsInVehicle() || player.IsGettingIntoVehicle || !Pistol(player))
            {
                LetGo(h, false, "the hold broke");
                return;
            }
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 24, true);   // attack
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 25, true);   // aim
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 21, true);   // sprint
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 22, true);   // jump
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 23, true);   // enter
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 36, true);   // duck
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 37, true);   // weapon wheel
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 44, true);   // cover
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, 45, true);   // reload
            for (int c = 140; c <= 143; c++) Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, c, true);   // melee
            for (int c = 14; c <= 17; c++) Function.Call(Hash.DISABLE_CONTROL_ACTION, 0, c, true);     // weapon next / previous
            if (now - h.PoseAt > 500)
            {
                if (!Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, player, ShieldDict, "perp_idle", 3))
                    Function.Call(Hash.TASK_PLAY_ANIM, player, ShieldDict, "perp_idle", 8f, -8f, -1, 49, 0f, false, false, false);
                if (!Function.Call<bool>(Hash.IS_ENTITY_PLAYING_ANIM, p, ShieldDict, "victim_idle", 3))
                    Function.Call(Hash.TASK_PLAY_ANIM, p, ShieldDict, "victim_idle", 8f, -8f, -1, 1, 0f, false, false, false);
            }
            HoldCops(player, now, standoffCops);
            float right = _canvasWidth() - _u(24f);
            _drawHint(right, 1010f, "LET GO", _btn("square", "E"));
            bool down = _holdDown();
            if (down && !_letGoDown) LetGo(h, true, "let go from the shield");
            _letGoDown = down;
        }

        private void EndShield(Ped player)
        {
            Hostage h = _shield;
            _shield = null;
            if (h != null && h.Ped != null && h.Ped.Exists())
            {
                Function.Call(Hash.DETACH_ENTITY, h.Ped, true, true);
                Function.Call(Hash.STOP_ANIM_TASK, h.Ped, ShieldDict, "victim_idle", 4f);
            }
            if (player != null && player.Exists())
                Function.Call(Hash.STOP_ANIM_TASK, player, ShieldDict, "perp_idle", 4f);
            FreeCops();
        }

        // police on foot near you aim and hold their fire (GTA6Hud's own standoff police already do)
        private void HoldCops(Ped player, int now, IList standoffCops)
        {
            if (now < _copScanAt) return;
            _copScanAt = now + 500;
            foreach (Ped c in World.GetNearbyPeds(player, CopReach))
            {
                if (c == null || !c.Exists() || c.IsDead || c.IsInVehicle() || c == player) continue;
                int type = Function.Call<int>(Hash.GET_PED_TYPE, c);
                if (type != 6 && type != 27 && type != 29) continue;   // cop, SWAT, army
                if (standoffCops.Contains(c) || _heldCops.Contains(c)) continue;
                _heldCops.Add(c);
                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, c, true);
                Function.Call(Hash.TASK_AIM_GUN_AT_ENTITY, c, player, -1, false);
            }
        }

        private void FreeCops()
        {
            if (_heldCops.Count == 0) return;
            Ped player = Game.Player.Character;
            bool wanted = Game.Player.Wanted.WantedLevel > 0;
            foreach (Ped c in _heldCops)
            {
                if (c == null || !c.Exists() || c.IsDead) continue;
                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, c, false);
                if (wanted) Function.Call(Hash.TASK_COMBAT_PED, c, player, 0, 16);
                else c.Task.ClearAll();
            }
            _heldCops.Clear();
        }

        // ---- GTA6Hud's own checks ----

        private static bool Armed(Ped player)
        {
            Weapon w = player.Weapons.Current;
            return w != null && w.Hash != WeaponHash.Unarmed && w.Group != WeaponGroup.Melee && w.Group != WeaponGroup.Thrown;
        }

        private static bool Pistol(Ped player)
        {
            Weapon w = player.Weapons.Current;
            return w != null && w.Group == WeaponGroup.Pistol;
        }

        private static bool AimHeld()
        {
            return Game.IsControlPressed(GTA.Control.Aim) || Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 0, 25);
        }

        private static bool LookingAt(Vector3 at, float dot)
        {
            Vector3 to = at - GameplayCamera.Position;
            to.Normalize();
            return Vector3.Dot(to, GameplayCamera.Direction) > dot;
        }

        private void RobLog(string s)
        {
            try { _robLog?.Invoke(s); } catch { }
        }

        private void Say(string text)
        {
            if (_notify != null) _notify(text);
            else Feed.Post(text);
        }

        private bool Look()
        {
            if (_hud != null) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            Type t = hud.GetType();
            Type lay = t.Assembly.GetType("GTA6Hud.Lay", false);
            _job = t.GetField("_job", Own);
            _robbed = t.GetField("_robbed", Own);
            _robVictim = t.GetField("_robVictim", Own);
            _stopRobbery = t.GetMethod("StopRobbery", Own, null, Type.EmptyTypes, null);
            Type job = _job?.FieldType;
            if (job != null)
            {
                _kind = job.GetField("Kind", Own);
                _ended = job.GetField("Ended", Own);
                _policeHere = job.GetField("PoliceHere", Own);
                _breachAt = job.GetField("BreachAt", Own);
                _police = job.GetField("Police", Own);
                _witnesses = job.GetField("Witnesses", Own);
                Type witness = _witnesses?.FieldType.IsGenericType == true ? _witnesses.FieldType.GetGenericArguments()[0] : null;
                _wPed = witness?.GetField("Ped", Own);
                _wState = witness?.GetField("State", Own);
            }
            _focused = HudBridge.Bind<Func<Vector3, bool>>(hud, t.GetMethod("Focused", Stat, null, new[] { typeof(Vector3) }, null));
            _holdDown = HudBridge.Bind<Func<bool>>(hud, t.GetMethod("ServiceHoldDown", Stat, null, Type.EmptyTypes, null));
            _drawMarker = HudBridge.Bind<Action<float, float, string, string, string, bool, bool, bool>>(hud, t.GetMethod("DrawWorldMarker", Own, null,
                new[] { typeof(float), typeof(float), typeof(string), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(bool) }, null));
            _drawRim = HudBridge.Bind<Action<float, float, float, float>>(hud, t.GetMethod("DrawProgressRim", Own, null, new[] { typeof(float), typeof(float), typeof(float), typeof(float) }, null));
            _drawHint = HudBridge.Bind<Func<float, float, string, string, float>>(hud, t.GetMethod("DrawHint", Own, null, new[] { typeof(float), typeof(float), typeof(string), typeof(string) }, null));
            _btn = HudBridge.Bind<Func<string, string, string>>(null, t.GetMethod("Btn", Stat, null, new[] { typeof(string), typeof(string) }, null));
            _robLog = HudBridge.Bind<Action<string>>(hud, t.GetMethod("RobLog", Own, null, new[] { typeof(string) }, null));
            _notify = HudBridge.Bind<Action<string>>(null, t.GetMethod("Notify", Stat, null, new[] { typeof(string) }, null));
            _canvasWidth = HudBridge.Bind<Func<float>>(null, lay?.GetProperty("CanvasWidth", Stat)?.GetGetMethod(true));
            _u = HudBridge.Bind<Func<float, float>>(null, lay?.GetMethod("U", Stat, null, new[] { typeof(float) }, null));
            if (_job == null || _kind == null || _ended == null || _policeHere == null || _breachAt == null || _breachAt.FieldType != typeof(int)
                || _police == null || _witnesses == null || _wPed == null || _wState == null || _focused == null || _holdDown == null
                || _drawMarker == null || _drawHint == null || _btn == null || _canvasWidth == null || _u == null)
            {
                Log.Write("Hostages", "GTA6Hud's robbery job or prompts weren't all found; no hostages");
                return false;
            }
            _hud = hud;
            Log.Write("Hostages", "on: +" + (_perHostage / 1000) + " s a hostage, +" + (_perRelease / 1000) + " s a release, human shield " + (_shieldOn ? "on" : "off"));
            return true;
        }
    }
}
