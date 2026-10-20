using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using GTA;
using GTA.Native;

namespace ViExtras
{
    /// <summary>
    /// vice_hud's wanted notification (index.html #wanted, app.js onWanted), in
    /// place of GTA6Hud's "The cops are searching for you" box.
    ///
    /// It says what the police are doing, in the same four states as the stars
    /// (WantedStars.State): they can see you, they are searching the area, a crime
    /// was reported, you have lost them but are still in the search area. Its
    /// star is in that state (the searching one blinking), and under it are the
    /// tells: what the police have on you. It comes up for six seconds whenever
    /// the stars or the state change, and a change only starts the six seconds
    /// again, so it never flickers.
    ///
    /// The tells come from GTA6Hud's police memory, in vice_hud's order: seen on
    /// camera, a weapon in hand, the outfit, the face (or, with another
    /// protagonist, the pair), the plate.
    ///
    /// GTA6Hud's own box (DrawPoliceSearchTip) is kept from drawing: it followed
    /// the game's hidden-evasion timer, which a close chase restarts every frame
    /// or two, and flickered.
    /// </summary>
    public sealed class WantedBox : Script
    {
        private const BindingFlags Own = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int HoldMs = 6000 + 1200;   // app.js WANTED_HOLD_MS, then setVisible's HIDE_HOLD_MS
        // #wanted: left 1.5cqw, top 26.7cqh; the tells row inside it (padding
        // 1.1 --w and 1.5cqh, gap 1.1cqh) and one .tell plus its 0.55 --w gap
        private const float Top = 26.7f * 10.8f;
        private const float TellX = 21.1137f, TellY = 86.5f, TellStep = 35.5f;
        // The star, cut out of the box art (tools/wanted_star.py) so it packs uncompressed: in the box,
        // which packs as DXT5, its 4 x 4 blocks broke the star up. Its square there is left empty; this is
        // where the square sits in the box picture (1080p units, from the picture's top-left corner).
        private const float StarX = 406f, StarY = 26f;

        private readonly bool _enabled;
        private readonly int _blinkMs;            // style.css @keyframes starHunt, in step with the stars
        private bool _looked, _failed;
        private object _hud;
        private FieldInfo _tipActive, _tipTime, _police;
        private PropertyInfo _camera, _face, _pair;
        private MethodInfo _outfitNow, _vehicleNow, _faceHidden;
        private FieldInfo _rememberFace;   // GTA6Hud.ini [Police] RememberFace
        private long _lastShot = long.MinValue / 2;
        private const int WeaponMemoryMs = 12000;   // Config.WantedTells.weaponMemoryMs

        private string _lastKey;
        private long _hideAt = -1;

        public WantedBox()
        {
            Paths.Init(BaseDirectory);
            _enabled = Config.Get("WantedBox", "Enabled", true);
            _blinkMs = Math.Max(100, Config.Get("WantedStars", "FlashMs", 900));
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
            _police = t.GetField("_police", Own);
            Type mem = _police?.FieldType;
            _camera = mem?.GetProperty("SeenOnCamera", Own);
            _face = mem?.GetProperty("KnowsFace", Own);
            _pair = mem?.GetProperty("WithAnotherProtagonist", Own);
            // What the police know that still fits you now (client_awareness.lua): the outfit
            // you're wearing, the car you're in, a face that isn't masked.
            _outfitNow = mem?.GetMethod("OutfitIdentified", Own, null, new[] { typeof(Ped) }, null);
            _vehicleNow = mem?.GetMethod("VehicleIdentified", Own, null, new[] { typeof(Ped) }, null);
            _rememberFace = t.Assembly.GetType("GTA6Hud.Ref", false)?.GetField("PoliceRememberFace", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            _faceHidden = mem?.GetMethod("FaceHidden", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(Ped) }, null);
            if (_tipActive == null || _tipTime == null) Log.Write("WantedBox", "GTA6Hud's search box could not be kept shut");
            _hud = hud;
            return true;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!GameReady.Now) return;
            if (!_enabled || !Look()) return;
            try
            {
                // GTA6Hud's own box stays shut: never a rising edge, never time left
                _tipActive?.SetValue(_hud, true);
                _tipTime?.SetValue(_hud, 0f);

                long now = RealTime.Ms;
                if (Function.Call<bool>(Hash.IS_PED_SHOOTING, Game.Player.Character)) _lastShot = now;
                int stars = Game.Player.WantedLevel;
                string state = WantedStars.State;
                string key = stars + "|" + (state ?? "");
                if (stars <= 0 || state == null)
                {
                    _lastKey = key;
                    _hideAt = -1;
                    return;
                }
                if (key != _lastKey)
                {
                    _lastKey = key;
                    _hideAt = now + HoldMs;
                }
                if (now >= _hideAt) return;
                if (HudBridge.Paused || HudBridge.Busy || HudBridge.HudMenuOpen || CrashCam.IsPlaying || Binoculars.IsActive) return;
                Draw(state, Tells(state, now), now, _blinkMs);
            }
            catch (Exception ex)
            {
                if (!_failed) Log.Write("WantedBox", ex.Message);
                _failed = true;
            }
        }

        /// <summary>getWantedTells (client.lua), from GTA6Hud's police memory.</summary>
        private List<string> Tells(string state, long now)
        {
            var tells = new List<string>();
            Ped me = Game.Player.Character;
            object m = _police?.GetValue(_hud);
            bool Has(PropertyInfo p) => m != null && p != null && p.GetValue(m) is bool b && b;
            bool Now(MethodInfo f) => m != null && f != null && f.Invoke(m, new object[] { me }) is bool b && b;
            if (Has(_camera)) tells.Add("camera");
            // A gun in hand while they can see you, or one fired in the last 12 s
            if (me.Weapons.Current.Hash != WeaponHash.Unarmed && (state == "contact" || now - _lastShot <= WeaponMemoryMs))
                tells.Add("weapon");
            if (Now(_outfitNow)) tells.Add("hanger");
            // The face, unmasked; with another protagonist along, the pair
            bool masked = _faceHidden != null && _faceHidden.Invoke(null, new object[] { me }) is bool h && h;
            bool faces = _rememberFace == null || _rememberFace.GetValue(null) is bool r && r;
            if (faces && Has(_face) && !masked) tells.Add(Has(_pair) ? "people" : "person");
            if (Now(_vehicleNow)) tells.Add("vehicle");
            return tells;
        }

        private static void Draw(string state, List<string> tells, long now, int blinkMs)
        {
            string look = state;
            if (state == "searching" && now % blinkMs >= blinkMs / 2) look += "_on";   // starHunt: hollow, then filled
            Art.Entry box = Art.Get((tells.Count > 0 ? "wanted_boxt_" : "wanted_box_") + look);
            if (box == null) return;
            float x = 0.015f * Canvas.W;
            Art.Draw(box, x, Top, Color.White);
            Art.Entry star = Art.Get("wanted_star_" + look);
            if (star != null) Art.Draw(star, x - box.BoxX + StarX, Top - box.BoxY + StarY, Color.White);
            for (int i = 0; i < tells.Count; i++)
            {
                Art.Entry tell = Art.Get("wanted_tell_" + tells[i]);
                if (tell != null) Art.Draw(tell, x + TellX + i * TellStep, Top + TellY, Color.White);
            }
        }
    }
}
