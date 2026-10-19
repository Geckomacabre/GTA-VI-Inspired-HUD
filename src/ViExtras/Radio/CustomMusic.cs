// Your own music as stations on the radio wheel, played by GTA6Hud's own
// custom radio player. Credits and licence: see Radio.cs.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ViExtras
{
    /// <summary>
    /// Your own music on the radio wheel: the tracks loose in
    /// scripts\Hud\Radio\CustomMusic are one station (MY MUSIC), and each
    /// subfolder with tracks in it is another, named after the folder. They play
    /// through GTA6Hud's own custom radio player (its CustomRadio: mp3, wav, wma
    /// and m4a on a thread of its own, title and artist read from the files'
    /// tags), the player its own radio's On Demand uses, run from here as an
    /// instance of its own by reflection. Without GTA6Hud there are no custom
    /// stations.
    /// </summary>
    internal sealed class CustomMusic
    {
        internal struct Station { public string Folder, Label; }   // Folder "": the loose tracks

        private const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const int RescanMs = 5000;

        private readonly string _directory;
        private bool _looked, _broken;
        private object _player;
        private MethodInfo _stations, _loose, _select, _update, _next, _previous, _release;
        private PropertyInfo _now;
        private FieldInfo _station, _title, _artist, _status;
        private long _scanAt;
        private bool _scanned;
        private List<Station> _list = new List<Station>();

        public CustomMusic(string directory) { _directory = directory; }

        private bool Look()
        {
            if (_looked) return _player != null;
            _looked = true;
            try
            {
                Type t = null;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                    if ((t = asm.GetType("GTA6Hud.CustomRadio", false)) != null) break;
                if (t == null) return false;
                MethodInfo M(string name, params Type[] args) => t.GetMethod(name, Any, null, args, null);
                _stations = M("Stations");
                _loose = M("HasLooseTracks");
                _select = M("Select", typeof(string));
                _update = M("Update", typeof(bool), typeof(float), typeof(float));
                _next = M("Next");
                _previous = M("Previous");
                _release = M("Release");
                _now = t.GetProperty("Now", Any);
                Type now = _now?.PropertyType;
                _station = now?.GetField("Station", Any);
                _title = now?.GetField("Title", Any);
                _artist = now?.GetField("Artist", Any);
                _status = now?.GetField("Status", Any);
                MethodInfo init = M("Init", typeof(string));
                if (init == null || _stations == null || _loose == null || _select == null || _update == null || _next == null
                    || _previous == null || _now == null || _station == null || _title == null || _artist == null || _status == null)
                {
                    Log.Write("Radio", "GTA6Hud's custom radio player was not as expected; no custom music stations");
                    return false;
                }
                object player = Activator.CreateInstance(t, true);
                init.Invoke(player, new object[] { _directory });
                _player = player;
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "custom music: " + (ex.InnerException ?? ex).Message);
                _player = null;
            }
            return _player != null;
        }

        /// <summary>The stations there are now (the folder is looked at again every 5 s).</summary>
        public List<Station> Stations(long now)
        {
            if (_broken || !Look()) return _list;
            if (_scanned && now - _scanAt < RescanMs) return _list;
            _scanned = true;
            _scanAt = now;
            var list = new List<Station>();
            try
            {
                if ((bool)_loose.Invoke(_player, null)) list.Add(new Station { Folder = "", Label = "MY MUSIC" });
                foreach (string folder in (string[])_stations.Invoke(_player, null))
                    list.Add(new Station { Folder = folder, Label = folder });
            }
            catch (Exception ex) { Fail(ex); }
            _list = list;
            return _list;
        }

        public void Select(string folder) => Call(_select, folder ?? "");

        /// <summary>Every frame: playing or not, at what volume (0..1).</summary>
        public void Update(bool on, float volume)
        {
            if (_player == null) return;
            Call(_update, on, volume, 0f);
        }

        public void Next() => Call(_next);
        public void Previous() => Call(_previous);

        /// <summary>
        /// What plays for this station: the song and artist, or the player's
        /// status (LOADING, NO PLAYABLE TRACKS, ...) as the song. False when it is
        /// not this station's.
        /// </summary>
        public bool NowPlaying(string folder, out string title, out string artist)
        {
            title = artist = null;
            if (_player == null || _broken) return false;
            try
            {
                object now = _now.GetValue(_player);
                if (now == null || !string.Equals((string)_station.GetValue(now) ?? "", folder ?? "", StringComparison.OrdinalIgnoreCase)) return false;
                string status = (string)_status.GetValue(now);
                if (!string.IsNullOrEmpty(status)) { title = status; return true; }
                title = (string)_title.GetValue(now);
                artist = (string)_artist.GetValue(now);
                return !string.IsNullOrEmpty(title);
            }
            catch (Exception ex) { Fail(ex); return false; }
        }

        public void Release()
        {
            if (_player != null) Call(_release);
        }

        private void Call(MethodInfo m, params object[] args)
        {
            if (_player == null || _broken || m == null) return;
            try { m.Invoke(_player, args); }
            catch (Exception ex) { Fail(ex); }
        }

        private void Fail(Exception ex)
        {
            _broken = true;
            Log.Write("Radio", "custom music stopped: " + (ex.InnerException ?? ex).Message);
        }
    }
}
