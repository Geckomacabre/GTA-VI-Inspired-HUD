using System;
using System.Collections.Generic;
using System.IO;

namespace ViExtras
{
    /// <summary>
    /// A value per number plate, kept in scripts\ViExtras\Save\&lt;file&gt; as
    /// "PLATE=value" lines: what Leonida Customs' server keeps by plate (the
    /// nitrous kit, the Pro Build). The shop (ViCustoms.dll) writes it and the
    /// nitrous script (ViExtras.dll) reads it, so the file is read again
    /// whenever it has changed on disk.
    /// </summary>
    internal sealed class PlateStore
    {
        private readonly string _path;
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private DateTime _stamp = DateTime.MinValue;
        // The nitrous script asks every frame it drives; the disk is asked once a
        // second (a kit bought in the shop shows up within that).
        private const int LookMs = 1000;
        private long _lookAt;

        public PlateStore(string file) { _path = Paths.Save(file); }

        /// <summary>Customs.PlateKey: the plate trimmed and upper case.</summary>
        public static string Key(string plate) => (plate ?? "").Trim().ToUpperInvariant();

        private void Refresh(bool now = false)
        {
            long ms = RealTime.Ms;
            if (!now && ms < _lookAt) return;
            _lookAt = ms + LookMs;
            try
            {
                DateTime stamp = File.Exists(_path) ? File.GetLastWriteTimeUtc(_path) : DateTime.MinValue;
                if (stamp == _stamp) return;
                _stamp = stamp;
                _values.Clear();
                if (!File.Exists(_path)) return;
                foreach (string line in File.ReadAllLines(_path))
                {
                    int eq = line.LastIndexOf('=');
                    if (eq <= 0) continue;
                    _values[Key(line.Substring(0, eq))] = line.Substring(eq + 1).Trim();
                }
            }
            catch (Exception ex) { Log.Write("Save", Path.GetFileName(_path) + ": " + ex.Message); }
        }

        public string Get(string plate, string fallback = null)
        {
            Refresh();
            return _values.TryGetValue(Key(plate), out string v) ? v : fallback;
        }

        /// <summary>Sets (or, with null, removes) the value for a plate and writes the file.</summary>
        public void Set(string plate, string value)
        {
            Refresh(now: true);   // what is written is the file as it is now, plus this
            string key = Key(plate);
            if (key.Length == 0) return;
            if (value == null) _values.Remove(key); else _values[key] = value;
            try
            {
                var lines = new List<string>();
                foreach (var kv in _values) lines.Add(kv.Key + "=" + kv.Value);
                SaveFile.Write(_path, string.Join(Environment.NewLine, lines) + Environment.NewLine);
                _stamp = File.GetLastWriteTimeUtc(_path);
            }
            catch (Exception ex) { Log.Write("Save", Path.GetFileName(_path) + ": " + ex.Message); }
        }
    }
}
