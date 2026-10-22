using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// GTA VI's wanted notification, in place of GTA6Hud's "The cops are searching for you" box:
    /// "The cops are searching for you / and they have your description", its red star, and under it
    /// the tells, what the police have on you (tools/render/wanted.py has the look, measured off a
    /// GTA VI frame).
    ///
    /// GTA VI shows it when the police have your description, so it shows only while you're wanted and
    /// at least one tell is lit. It comes up for about seven seconds when a tell lights that wasn't lit
    /// before, or your stars go up, and goes at once when the last tell goes out. While it's up its
    /// tells follow what the police have on you.
    ///
    /// The tells are what the police saw this pursuit (WantedMemory, vice_hud's ViceWantedMemory), in
    /// vice_hud's order: seen on camera, a weapon in hand, the outfit, the face (or, with another
    /// protagonist close by, the pair), the plate. They're worked out here every 250 ms whether or not
    /// the box is on: the row under the stars (WantedTells) shows them too.
    ///
    /// GTA6Hud's own box (DrawPoliceSearchTip) is kept from drawing: it followed the game's
    /// hidden-evasion timer, which a close chase restarts every frame or two, and flickered.
    /// </summary>
    public sealed class WantedBox : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int HoldMs = 6000 + 1200;   // vice_hud's hold (app.js WANTED_HOLD_MS, then HIDE_HOLD_MS)
        // Where the box sits (vice_hud's #wanted: left 1.5cqw, top 26.7cqh), and inside it, from its top
        // left corner: the star, and the first tell and the step between tells (wanted.py prints them).
        internal const float Top = 26.7f * 10.8f;
        private const float StarX = 377f, StarY = 12f;
        private const float TellX = 20.0037f, TellY = 74f, TellStep = 34f;
        private static readonly string[] TellNames = { "camera", "weapon", "hanger", "person", "people", "vehicle" };

        private readonly bool _enabled;
        private readonly bool _indoorCameras;
        private bool _looked, _failed;
        private object _hud;
        private FieldInfo _tipActive, _tipTime;
        private long _lastShot = long.MinValue / 2;
        private const int WeaponMemoryMs = 12000;   // Config.WantedTells.weaponMemoryMs

        private const int TellsMs = 250;   // client.lua's main loop (Config.Tick), which works the tells out
        private long _nextTells;

        private int _lastStars, _lastTells;   // what the box last saw: stars, and the tells as bits of TellNames
        private long _hideAt = -1;

        /// <summary>On screen this frame (FeedBesideMap puts GTA's feed under it).</summary>
        internal static bool Up;

        /// <summary>Its bottom edge, on the canvas.</summary>
        internal static float Bottom => Top + (Art.Get("wanted_box")?.BoxH ?? 110f);

        public WantedBox()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("WantedBox", "Enabled", true);
            _indoorCameras = Config.Get("WantedBox", "IndoorCameras", true);
            Tick += OnTick;
        }

        private bool Look()
        {
            if (_hud != null) return true;
            object hud = HudBridge.Gta6Hud();
            if (hud == null || _looked) return false;
            _looked = true;
            Type t = hud.GetType();
            _tipActive = t.GetField("_searchTipActive", Own);
            _tipTime = t.GetField("_searchTipTime", Own);
            if (_tipActive == null || _tipTime == null) Log.Write("WantedBox", "GTA6Hud's search box could not be kept shut");
            _hud = hud;
            return true;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Up = false;
            if (!GameReady.Now) return;
            try
            {
                long now = RealTime.Ms;
                Ped me = Game.Player.Character;
                // a shot leaves the barrel for one frame, so this one is every frame (client_awareness.lua)
                if (Function.Call<bool>(Hash.IS_PED_SHOOTING, me)) _lastShot = now;
                int stars = Game.Player.WantedLevel;
                string state = WantedStars.State;
                if (stars <= 0 || state == null)
                {
                    // the police have lost you: every sighting is spent, and the row under the stars goes at once
                    if (stars <= 0) WantedMemory.Clear();
                    WantedTells.Set(null);
                    _nextTells = 0;
                }
                else if (now >= _nextTells)
                {
                    _nextTells = now + TellsMs;
                    // What they see while they can see you (wantedState's contact branch)
                    if (state == "contact") WantedMemory.Note(me);
                    // Wanted inside a building: its cameras have you (FBICat's CCTV rule), until the stars go
                    if (_indoorCameras && Function.Call<int>(Hash.GET_INTERIOR_FROM_ENTITY, me) != 0) WantedMemory.SeenOnCamera();
                    WantedTells.Set(Tells(state, now, _lastShot));
                }

                if (!_enabled || !Look()) return;
                // GTA6Hud's own box stays shut: never a rising edge, never time left
                _tipActive?.SetValue(_hud, true);
                _tipTime?.SetValue(_hud, 0f);
                string[] tells = WantedTells.Now;
                int lit = Bits(tells);
                if (stars <= 0 || lit == 0)
                {
                    _lastStars = Math.Max(stars, 0);
                    _lastTells = 0;
                    _hideAt = -1;
                    return;
                }
                // up again for a tell that wasn't lit, or more stars
                if ((lit & ~_lastTells) != 0 || stars > _lastStars) _hideAt = now + HoldMs;
                _lastTells = lit;
                _lastStars = stars;
                if (now >= _hideAt) return;
                if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen || CrashCam.IsPlaying || Binoculars.IsActive) return;
                Draw(tells);
                Up = true;
            }
            catch (Exception ex)
            {
                if (!_failed) Log.Write("WantedBox", ex.Message);
                _failed = true;
            }
        }

        private static int Bits(string[] tells)
        {
            int bits = 0;
            foreach (string t in tells)
            {
                int i = Array.IndexOf(TellNames, t);
                if (i >= 0) bits |= 1 << i;
            }
            return bits;
        }

        /// <summary>getWantedTells (client.lua), from what the police saw this pursuit (WantedMemory).</summary>
        private static List<string> Tells(string state, long now, long lastShot)
        {
            var tells = new List<string>();
            Ped me = Game.Player.Character;
            if (WantedMemory.Camera) tells.Add("camera");
            // A gun in hand while they can see you, or one fired in the last 12 s
            if (me.Weapons.Current.Hash != WeaponHash.Unarmed && (state == "contact" || now - lastShot < WeaponMemoryMs))
                tells.Add("weapon");
            if (WantedMemory.Outfit(me)) tells.Add("hanger");
            // The face they saw, still uncovered; with another protagonist close by, the pair
            if (WantedMemory.Face(me)) tells.Add(WantedMemory.SuspectNearby(me) ? "people" : "person");
            if (WantedMemory.Vehicle(me)) tells.Add("vehicle");
            return tells;
        }

        private static void Draw(string[] tells)
        {
            Art.Entry box = Art.Get("wanted_box");
            if (box == null) return;
            float x = 0.015f * Canvas.W;
            Art.Draw(box, x, Top, Color.White);
            Art.Entry star = Art.Get("wanted_star");
            if (star != null) Art.Draw(star, x + StarX, Top + StarY, Color.White);
            for (int i = 0; i < tells.Length; i++)
            {
                Art.Entry tell = Art.Get("wanted_tell_" + tells[i]);
                if (tell != null) Art.Draw(tell, x + TellX + i * TellStep, Top + TellY, Color.White);
            }
        }
    }
}
