// Your own music as stations on the radio wheel. Credits and licence: see Radio.cs.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ViExtras
{
    /// <summary>
    /// Your own music on the radio wheel: the tracks loose in
    /// scripts\Hud\Radio\CustomMusic are one station (MY MUSIC), and each
    /// subfolder with tracks in it is another, named after the folder. They play
    /// through MusicPlayer (mp3, wav, wma and m4a, on a thread of its own). The
    /// song and artist come from GTA6Hud's own tag reader (TrackTags: the file's
    /// ID3 title and artist, else its name), so they read as they did on GTA6Hud's
    /// player; without GTA6Hud, from the file name.
    /// </summary>
    internal sealed class CustomMusic
    {
        internal struct Station { public string Folder, Label; }   // Folder "": the loose tracks

        private const int RescanMs = 5000;

        private readonly string _directory;
        private readonly MusicPlayer _player;
        private bool _tagsLooked;
        private MethodInfo _tagRead;
        private FieldInfo _tagTitle, _tagArtist;
        private long _scanAt;
        private bool _scanned;
        private List<Station> _list = new List<Station>();

        public CustomMusic(string directory)
        {
            _directory = directory;
            _player = new MusicPlayer(directory, Tags);
        }

        /// <summary>CustomRadio.IsTrack: what counts as a track.</summary>
        public static bool IsTrack(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".mp3": case ".wav": case ".wma": case ".m4a":
                    return !path.EndsWith(".part.mp3", StringComparison.OrdinalIgnoreCase);
                default:
                    return false;
            }
        }

        /// <summary>The stations there are now (the folder is looked at again every 5 s).</summary>
        public List<Station> Stations(long now)
        {
            if (_scanned && now - _scanAt < RescanMs) return _list;
            _scanned = true;
            _scanAt = now;
            var list = new List<Station>();
            try
            {
                if (Directory.Exists(_directory))
                {
                    if (Directory.GetFiles(_directory).Any(IsTrack)) list.Add(new Station { Folder = "", Label = "MY MUSIC" });
                    foreach (string folder in Directory.GetDirectories(_directory).Select(Path.GetFileName)
                                 .Where(n => !n.StartsWith(".") && Directory.GetFiles(Path.Combine(_directory, n)).Any(IsTrack))
                                 .OrderBy(n => n, StringComparer.OrdinalIgnoreCase))
                        list.Add(new Station { Folder = folder, Label = folder });
                }
            }
            catch (Exception ex) { Log.Write("Radio", "custom music: " + ex.Message); }
            _list = list;
            return _list;
        }

        public void Select(string folder) => _player.Select(folder ?? "");

        /// <summary>Every frame: playing or not, at what volume (0..1).</summary>
        public void Update(bool on, float volume) => _player.Update(on, volume);

        public void Next() => _player.Next();
        public void Previous() => _player.Previous();

        /// <summary>
        /// What plays for this station: the song and artist, or the player's
        /// status (LOADING, NO PLAYABLE TRACKS, ...) as the song. False when it is
        /// not this station's.
        /// </summary>
        public bool NowPlaying(string folder, out string title, out string artist)
        {
            title = artist = null;
            MusicPlayer.State now = _player.Now;
            if (now == null || !string.Equals(now.Station ?? "", folder ?? "", StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrEmpty(now.Status)) { title = now.Status; return true; }
            title = now.Title;
            artist = now.Artist;
            return !string.IsNullOrEmpty(title);
        }

        public void Release() => _player.Release();

        // GTA6Hud.TrackTags.Read, found once; the music thread calls this
        private (string title, string artist) Tags(string path)
        {
            if (!_tagsLooked)
            {
                _tagsLooked = true;
                try
                {
                    Type t = null;
                    foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                        if ((t = asm.GetType("GTA6Hud.TrackTags", false)) != null) break;
                    _tagRead = t?.GetMethod("Read", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(string) }, null);
                    Type tag = _tagRead?.ReturnType;
                    _tagTitle = tag?.GetField("Title");
                    _tagArtist = tag?.GetField("Artist");
                    if (_tagRead == null || _tagTitle == null || _tagArtist == null) _tagRead = null;
                }
                catch { _tagRead = null; }
            }
            if (_tagRead != null)
            {
                try
                {
                    object tag = _tagRead.Invoke(null, new object[] { path });
                    return ((string)_tagTitle.GetValue(tag) ?? "", (string)_tagArtist.GetValue(tag) ?? "");
                }
                catch { }
            }
            return (Path.GetFileNameWithoutExtension(path).Replace('_', ' ').Trim().ToUpperInvariant(), "");
        }
    }
}
