// Your own music's player: Media Foundation decodes, waveOut plays, on a plain
// background thread. Credits and licence: see Radio.cs.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace ViExtras
{
    /// <summary>
    /// Plays the tracks of one CustomMusic folder, as GTA6Hud's own player
    /// (CustomRadio) does: in name order, the next one when a track ends, the
    /// game's next / previous track keys skipping, the track and place
    /// remembered per station for half an hour, a track that won't open skipped,
    /// and silent while the game is paused, in the background or not asking for
    /// it.
    ///
    /// GTA6Hud's player runs WPF's MediaPlayer, which starts WPF's whole media
    /// stack (and its Direct3D) inside the game; GTA V Legacy stopped responding
    /// the moment it started. This one only decodes (Media Foundation's source
    /// reader, the decoders Windows plays mp3, wav, wma and m4a with) and hands
    /// the samples to waveOut, the way RadioSound plays the wheel's clicks.
    /// Volume is a gain on the samples, as WPF's linear MediaPlayer.Volume.
    /// </summary>
    internal sealed class MusicPlayer
    {
        public sealed class State
        {
            public string Station = "", Title = "", Artist = "", Status = "LOADING";
        }

        private struct Resume { public string Track; public long Pos100ns; public long At; }

        private const int ResumeForMs = 1800000;   // CustomRadio: a station's place is kept 30 minutes
        private const int RescanMs = 10000;
        private const int HeartbeatMs = 1500;
        private const int BufferMs = 100, Buffers = 4;

        private readonly string _directory;
        private readonly Func<string, (string title, string artist)> _tags;
        private readonly ConcurrentQueue<(int cmd, string arg)> _commands = new ConcurrentQueue<(int, string)>();
        private const int CmdSelect = 0, CmdNext = 1, CmdPrevious = 2;

        private volatile bool _enabled, _closed;
        private volatile float _volume;
        private long _heartbeat;
        private volatile State _now = new State();
        private Thread _thread;
        private string _selected;

        public MusicPlayer(string directory, Func<string, (string title, string artist)> tags)
        {
            _directory = directory;
            _tags = tags;
        }

        public State Now => _now;

        public void Select(string folder)
        {
            folder = folder ?? "";
            if (_selected == folder) return;
            _selected = folder;
            _commands.Enqueue((CmdSelect, folder));
        }

        public void Next() => _commands.Enqueue((CmdNext, null));
        public void Previous() => _commands.Enqueue((CmdPrevious, null));

        /// <summary>Every frame, from the script: playing or not, at what volume (0..1).</summary>
        public void Update(bool enabled, float volume)
        {
            _enabled = enabled;
            _volume = Math.Max(0f, Math.Min(1f, volume));
            Interlocked.Exchange(ref _heartbeat, RealTime.Ms);
            if (enabled && !_closed && (_thread == null || !_thread.IsAlive))
            {
                if (_thread != null) Log.Write("Radio", "music player was not running: restarting it");
                if (_selected != null) _commands.Enqueue((CmdSelect, _selected));
                _thread = new Thread(Run) { IsBackground = true, Name = "ViExtras music" };
                _thread.Start();
            }
        }

        public void Release()
        {
            _enabled = false;
            _closed = true;
        }

        private void Publish(Action<State> edit)
        {
            State was = _now;
            var s = new State { Station = was.Station, Title = was.Title, Artist = was.Artist, Status = was.Status };
            edit(s);
            _now = s;
        }

        // ---- the player's thread ---------------------------------------------------------------

        private string _folder;
        private string[] _tracks = new string[0];
        private int _cursor = -1;
        private readonly List<int> _history = new List<int>();
        private readonly Dictionary<string, Resume> _resume = new Dictionary<string, Resume>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, long> _bad = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        private string _current;
        private Track _track;
        private WaveOut _out;
        private bool _paused;
        private int _failStreak;
        private long _retryAt, _scanAt;

        private void Run()
        {
            bool mf = false;
            try
            {
                mf = Mf.MFStartup(Mf.Version, Mf.NoSocket) >= 0;
                if (!mf) { Publish(n => n.Status = "CAN'T PLAY MUSIC ON THIS PC"); Log.Write("Radio", "music: Media Foundation would not start"); return; }
                while (!_closed)
                {
                    Step();
                    Thread.Sleep(15);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "music player stopped: " + ex.Message);
                Publish(n => n.Status = "MUSIC PLAYER RESTARTING");
            }
            finally
            {
                StopTrack();
                if (mf) Mf.MFShutdown();
            }
        }

        private void Step()
        {
            while (_commands.TryDequeue(out var c)) Handle(c.cmd, c.arg);
            if (_folder == null) return;
            long now = RealTime.Ms;
            if (now >= _scanAt) Rescan();

            if (_tracks.Length == 0)
            {
                Publish(n => { n.Status = "ADD MUSIC TO HUD\\RADIO\\CUSTOMMUSIC"; n.Title = n.Artist = ""; });
                StopTrack();
                return;
            }
            bool want = _enabled && now - Interlocked.Read(ref _heartbeat) < HeartbeatMs && GameIsForeground();
            if (!want)
            {
                if (_out != null && !_paused) { _out.Pause(); _paused = true; RememberPosition(); }
                return;
            }
            if (_track == null)
            {
                if (now >= _retryAt) Advance(1, false);
                return;
            }
            if (_paused) { _out.Restart(); _paused = false; }
            try
            {
                if (!_out.Feed(_track, _volume) && _out.Drained)
                {
                    Advance(1, false);   // the track has ended
                }
            }
            catch (Exception ex)
            {
                Fail("decoding failed (" + ex.Message + ")");
            }
        }

        private void Handle(int cmd, string arg)
        {
            switch (cmd)
            {
                case CmdSelect:
                    RememberPosition();
                    _folder = arg;
                    _history.Clear();
                    _cursor = -1;
                    StopTrack();
                    Rescan();
                    Log.Write("Radio", "music: station " + (arg.Length == 0 ? "MY MUSIC" : arg) + ", " + _tracks.Length + " tracks");
                    Publish(n => { n.Station = arg; n.Title = n.Artist = ""; n.Status = "LOADING"; });
                    if (_resume.TryGetValue(arg, out Resume r) && RealTime.Ms - r.At < ResumeForMs)
                    {
                        int i = Array.FindIndex(_tracks, t => string.Equals(t, r.Track, StringComparison.OrdinalIgnoreCase));
                        if (i >= 0) { _cursor = i; Open(i, r.Pos100ns); }
                    }
                    break;
                case CmdNext:
                    if (_folder != null) Advance(1, true);
                    break;
                case CmdPrevious:
                    if (_folder != null) Advance(-1, true);
                    break;
            }
        }

        private void Rescan()
        {
            _scanAt = RealTime.Ms + RescanMs;
            string dir = string.IsNullOrEmpty(_folder) ? _directory : Path.Combine(_directory, _folder);
            string[] found;
            try
            {
                found = Directory.Exists(dir)
                    ? Directory.GetFiles(dir).Where(CustomMusic.IsTrack).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray()
                    : new string[0];
            }
            catch (Exception ex) { Log.Write("Radio", "music: can't read " + dir + ": " + ex.Message); return; }
            if (found.SequenceEqual(_tracks, StringComparer.OrdinalIgnoreCase)) return;
            _tracks = found;
            if (_current != null)
            {
                _cursor = Array.FindIndex(_tracks, t => string.Equals(t, _current, StringComparison.OrdinalIgnoreCase));
                if (_cursor < 0) { Log.Write("Radio", "music: the track playing was removed"); StopTrack(); }
            }
            _history.RemoveAll(h => h >= _tracks.Length);
        }

        // CustomRadio.Advance without its shuffle (nothing here turns shuffle on): the
        // next one in name order, wrapping round; back goes the way you came first.
        private void Advance(int dir, bool user)
        {
            int count = _tracks.Length;
            if (count == 0) return;
            RememberPosition();
            int was = _cursor, pick = -1;
            if (dir < 0 && _history.Count > 0)
            {
                pick = _history[_history.Count - 1];
                _history.RemoveAt(_history.Count - 1);
            }
            else
            {
                int at = _cursor;
                for (int i = 0; i < count; i++)
                {
                    at = at < 0 || at >= count ? (dir > 0 ? 0 : count - 1) : (at + dir + count) % count;
                    if (!IsBad(_tracks[at])) { pick = at; break; }
                }
                if (pick < 0)
                {
                    Publish(n => n.Status = "NO PLAYABLE TRACKS");
                    StopTrack();
                    _retryAt = RealTime.Ms + 10000;
                    return;
                }
                if (dir > 0 && was >= 0 && was != pick)
                {
                    _history.Add(was);
                    if (_history.Count > 50) _history.RemoveAt(0);
                }
            }
            if (user) _failStreak = 0;
            _cursor = pick;
            Open(pick, 0);
        }

        private void Open(int i, long at100ns)
        {
            StopTrack();
            _current = _tracks[i];
            try
            {
                _track = new Track(_current, at100ns);
                _out = new WaveOut(_track.Rate, _track.Channels, BufferMs, Buffers);
                _paused = false;
            }
            catch (Exception ex)
            {
                Fail("won't open (" + ex.Message + ")");
                return;
            }
            var tag = _tags(_current);
            Log.Write("Radio", "music: " + Path.GetFileName(_current) + (at100ns > 0 ? " from " + at100ns / 10000000 + " s" : ""));
            Publish(n => { n.Title = tag.title; n.Artist = tag.artist; n.Station = _folder ?? ""; n.Status = ""; });
        }

        private void Fail(string why)
        {
            Log.Write("Radio", "music: " + (_current == null ? "-" : Path.GetFileName(_current)) + " " + why);
            if (_current != null)
            {
                try { _bad[_current] = new FileInfo(_current).Length; } catch { _bad[_current] = -1; }
            }
            StopTrack();
            if (++_failStreak > Math.Max(3, _tracks.Length))
            {
                Publish(n => n.Status = "NO PLAYABLE TRACKS");
                _retryAt = _scanAt = RealTime.Ms + 10000;
                _failStreak = 0;
            }
            else
            {
                Publish(n => n.Status = "SKIPPING UNPLAYABLE TRACK");
                Advance(1, false);
            }
        }

        // a track that failed stays skipped until the file changes
        private bool IsBad(string t)
        {
            if (!_bad.TryGetValue(t, out long len)) return false;
            long now = -1;
            try { now = new FileInfo(t).Length; } catch { }
            if (now != len) { _bad.Remove(t); return false; }
            return true;
        }

        private void RememberPosition()
        {
            if (_folder != null && _current != null && _track != null && _out != null)
                _resume[_folder] = new Resume { Track = _current, Pos100ns = _track.Start100ns + _out.Played100ns, At = RealTime.Ms };
        }

        private void StopTrack()
        {
            _out?.Dispose();
            _out = null;
            _track?.Dispose();
            _track = null;
            _current = null;
            _paused = false;
        }

        private static readonly uint Pid = (uint)Process.GetCurrentProcess().Id;
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);

        // GameFocus.IsForeground: GTA's own audio goes quiet in the background, so does this
        private static bool GameIsForeground()
        {
            IntPtr w = GetForegroundWindow();
            if (w == IntPtr.Zero) return true;
            GetWindowThreadProcessId(w, out uint pid);
            return pid == Pid;
        }

        // ---- one track, decoded to 16-bit PCM ---------------------------------------------------

        private sealed class Track : IDisposable
        {
            public readonly int Rate, Channels;
            public readonly long Start100ns;
            private Mf.IMFSourceReader _reader;
            private byte[] _pending = new byte[0];
            private int _pendingAt, _pendingLen;
            private bool _ended;

            public Track(string path, long start100ns)
            {
                Mf.Check(Mf.MFCreateSourceReaderFromURL(path, IntPtr.Zero, out _reader), "open");
                // the audio alone: an mp3's cover picture is a stream of its own
                _reader.SetStreamSelection(Mf.AllStreams, 0);
                Mf.Check(_reader.SetStreamSelection(Mf.FirstAudio, 1), "no audio");
                // 16-bit PCM in stereo, else 16-bit in the file's own channels, else PCM as it comes
                if (!SetPcm(true, true) && !SetPcm(true, false)) Mf.Check(SetPcm(false, false) ? 0 : -1, "can't decode");
                Mf.Check(_reader.GetCurrentMediaType(Mf.FirstAudio, out Mf.IMFMediaType type), "format");
                try
                {
                    Mf.Check(type.GetUINT32(Mf.AudioSamplesPerSecond, out Rate), "rate");
                    Mf.Check(type.GetUINT32(Mf.AudioChannels, out Channels), "channels");
                    Mf.Check(type.GetUINT32(Mf.AudioBitsPerSample, out int bits), "bits");
                    if (bits != 16 || Channels < 1 || Channels > 2) throw new InvalidOperationException(bits + "-bit, " + Channels + " channels");
                }
                finally { Marshal.ReleaseComObject(type); }
                if (start100ns > 0)
                {
                    var pos = new Mf.PropVariant { vt = 20, hVal = start100ns };   // VT_I8
                    if (_reader.SetCurrentPosition(Guid.Empty, ref pos) >= 0) Start100ns = start100ns;
                }
            }

            private bool SetPcm(bool sixteen, bool stereo)
            {
                Mf.Check(Mf.MFCreateMediaType(out Mf.IMFMediaType t), "media type");
                try
                {
                    t.SetGUID(Mf.MajorType, Mf.Audio);
                    t.SetGUID(Mf.Subtype, Mf.Pcm);
                    if (sixteen) t.SetUINT32(Mf.AudioBitsPerSample, 16);
                    if (stereo) t.SetUINT32(Mf.AudioChannels, 2);
                    return _reader.SetCurrentMediaType(Mf.FirstAudio, IntPtr.Zero, t) >= 0;
                }
                finally { Marshal.ReleaseComObject(t); }
            }

            /// <summary>Up to count bytes of PCM; fewer only at the end of the track.</summary>
            public int Read(byte[] dst, int count)
            {
                int n = 0;
                while (n < count)
                {
                    if (_pendingAt < _pendingLen)
                    {
                        int take = Math.Min(count - n, _pendingLen - _pendingAt);
                        Buffer.BlockCopy(_pending, _pendingAt, dst, n, take);
                        _pendingAt += take;
                        n += take;
                        continue;
                    }
                    if (_ended) break;
                    Mf.Check(_reader.ReadSample(Mf.FirstAudio, 0, out _, out int flags, out _, out Mf.IMFSample sample), "read");
                    if ((flags & Mf.ReaderEndOfStream) != 0) _ended = true;
                    if ((flags & (Mf.ReaderError | Mf.ReaderTypeChanged)) != 0)
                    {
                        if (sample != null) Marshal.ReleaseComObject(sample);
                        throw new InvalidOperationException((flags & Mf.ReaderError) != 0 ? "stream error" : "format changed");
                    }
                    if (sample == null) continue;
                    try
                    {
                        Mf.Check(sample.ConvertToContiguousBuffer(out Mf.IMFMediaBuffer buf), "buffer");
                        try
                        {
                            Mf.Check(buf.Lock(out IntPtr p, out _, out int len), "lock");
                            try
                            {
                                if (_pending.Length < len) _pending = new byte[len];
                                Marshal.Copy(p, _pending, 0, len);
                                _pendingAt = 0;
                                _pendingLen = len;
                            }
                            finally { buf.Unlock(); }
                        }
                        finally { Marshal.ReleaseComObject(buf); }
                    }
                    finally { Marshal.ReleaseComObject(sample); }
                }
                return n;
            }

            public void Dispose()
            {
                if (_reader != null) { Marshal.ReleaseComObject(_reader); _reader = null; }
            }
        }

        // ---- waveOut, a few short buffers kept full -----------------------------------------------

        private sealed class WaveOut : IDisposable
        {
            [StructLayout(LayoutKind.Sequential, Pack = 2)]
            private struct WaveFormat
            {
                public ushort Tag, Channels;
                public uint SamplesPerSec, AvgBytesPerSec;
                public ushort BlockAlign, BitsPerSample, Size;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct WaveHeader
            {
                public IntPtr Data;
                public uint BufferLength, BytesRecorded;
                public IntPtr User;
                public uint Flags, Loops;
                public IntPtr Next, Reserved;
            }

            [DllImport("winmm.dll")] private static extern int waveOutOpen(out IntPtr hwo, uint device, ref WaveFormat fmt, IntPtr callback, IntPtr instance, uint flags);
            [DllImport("winmm.dll")] private static extern int waveOutPrepareHeader(IntPtr hwo, IntPtr hdr, uint size);
            [DllImport("winmm.dll")] private static extern int waveOutWrite(IntPtr hwo, IntPtr hdr, uint size);
            [DllImport("winmm.dll")] private static extern int waveOutUnprepareHeader(IntPtr hwo, IntPtr hdr, uint size);
            [DllImport("winmm.dll")] private static extern int waveOutPause(IntPtr hwo);
            [DllImport("winmm.dll")] private static extern int waveOutRestart(IntPtr hwo);
            [DllImport("winmm.dll")] private static extern int waveOutReset(IntPtr hwo);
            [DllImport("winmm.dll")] private static extern int waveOutClose(IntPtr hwo);

            private const uint WaveMapper = 0xFFFFFFFF, WhdrDone = 1, WhdrPrepared = 2;
            private static readonly uint HeaderSize = (uint)Marshal.SizeOf(typeof(WaveHeader));

            private IntPtr _hwo;
            private readonly IntPtr[] _data, _header;
            private readonly bool[] _queued;
            private readonly int[] _length;
            private readonly byte[] _scratch;
            private readonly int _bytesPerSec;
            private long _playedBytes;
            private bool _sourceDone;

            public WaveOut(int rate, int channels, int bufferMs, int count)
            {
                var fmt = new WaveFormat
                {
                    Tag = 1, Channels = (ushort)channels, SamplesPerSec = (uint)rate, BitsPerSample = 16,
                    BlockAlign = (ushort)(channels * 2), AvgBytesPerSec = (uint)(rate * channels * 2)
                };
                int err = waveOutOpen(out _hwo, WaveMapper, ref fmt, IntPtr.Zero, IntPtr.Zero, 0);
                if (err != 0) { _hwo = IntPtr.Zero; throw new InvalidOperationException("sound device (" + err + ")"); }
                _bytesPerSec = (int)fmt.AvgBytesPerSec;
                int size = Math.Max(fmt.BlockAlign, _bytesPerSec * bufferMs / 1000 / fmt.BlockAlign * fmt.BlockAlign);
                _scratch = new byte[size];
                _data = new IntPtr[count];
                _header = new IntPtr[count];
                _queued = new bool[count];
                _length = new int[count];
                for (int i = 0; i < count; i++)
                {
                    _data[i] = Marshal.AllocHGlobal(size);
                    _header[i] = Marshal.AllocHGlobal((int)HeaderSize);
                    Marshal.StructureToPtr(new WaveHeader { Data = _data[i], BufferLength = (uint)size }, _header[i], false);
                    if (waveOutPrepareHeader(_hwo, _header[i], HeaderSize) != 0) throw new InvalidOperationException("sound buffer");
                }
            }

            private uint Flags(int i) => (uint)Marshal.ReadInt32(_header[i], Marshal.OffsetOf(typeof(WaveHeader), "Flags").ToInt32());

            /// <summary>Refills every buffer the device has finished with. False once the track has no more.</summary>
            public bool Feed(Track track, float volume)
            {
                for (int i = 0; i < _data.Length; i++)
                {
                    if (_queued[i])
                    {
                        if ((Flags(i) & WhdrDone) == 0) continue;
                        _queued[i] = false;
                        _playedBytes += _length[i];
                    }
                    if (_sourceDone) continue;
                    int n = track.Read(_scratch, _scratch.Length);
                    if (n < _scratch.Length) _sourceDone = true;
                    if (n <= 0) continue;
                    if (volume < 0.999f)
                    {
                        for (int b = 0; b + 1 < n; b += 2)
                        {
                            int s = (int)((short)(_scratch[b] | (_scratch[b + 1] << 8)) * volume);
                            _scratch[b] = (byte)s;
                            _scratch[b + 1] = (byte)(s >> 8);
                        }
                    }
                    Marshal.Copy(_scratch, 0, _data[i], n);
                    Marshal.WriteInt32(_header[i], Marshal.OffsetOf(typeof(WaveHeader), "BufferLength").ToInt32(), n);
                    Marshal.WriteInt32(_header[i], Marshal.OffsetOf(typeof(WaveHeader), "Flags").ToInt32(), (int)WhdrPrepared);
                    if (waveOutWrite(_hwo, _header[i], HeaderSize) != 0) throw new InvalidOperationException("sound output");
                    _queued[i] = true;
                    _length[i] = n;
                }
                return !_sourceDone;
            }

            /// <summary>Nothing left in the device.</summary>
            public bool Drained => _queued.All(q => !q);

            public long Played100ns => _playedBytes * 10000000L / Math.Max(1, _bytesPerSec);

            public void Pause() { if (_hwo != IntPtr.Zero) waveOutPause(_hwo); }
            public void Restart() { if (_hwo != IntPtr.Zero) waveOutRestart(_hwo); }

            public void Dispose()
            {
                if (_hwo != IntPtr.Zero)
                {
                    waveOutReset(_hwo);
                    for (int i = 0; i < _header.Length; i++)
                        if (_header[i] != IntPtr.Zero) waveOutUnprepareHeader(_hwo, _header[i], HeaderSize);
                    waveOutClose(_hwo);
                    _hwo = IntPtr.Zero;
                }
                for (int i = 0; i < _header.Length; i++)
                {
                    if (_header[i] != IntPtr.Zero) Marshal.FreeHGlobal(_header[i]);
                    if (_data[i] != IntPtr.Zero) Marshal.FreeHGlobal(_data[i]);
                    _header[i] = _data[i] = IntPtr.Zero;
                }
            }
        }

        // ---- the Media Foundation calls used above --------------------------------------------------

        private static class Mf
        {
            public const int Version = 0x00020070, NoSocket = 1;   // MF_VERSION (Windows 7 on), MFSTARTUP_NOSOCKET
            public const int FirstAudio = unchecked((int)0xFFFFFFFD), AllStreams = unchecked((int)0xFFFFFFFE);
            public const int ReaderError = 0x1, ReaderEndOfStream = 0x2, ReaderTypeChanged = 0x20;

            public static readonly Guid MajorType = new Guid("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
            public static readonly Guid Subtype = new Guid("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
            public static readonly Guid Audio = new Guid("73647561-0000-0010-8000-00aa00389b71");
            public static readonly Guid Pcm = new Guid("00000001-0000-0010-8000-00aa00389b71");
            public static readonly Guid AudioChannels = new Guid("37e48bf5-645e-4c5b-89de-ada9e29b696a");
            public static readonly Guid AudioSamplesPerSecond = new Guid("5faeeae7-0290-4c31-9e8a-c534f68d9dba");
            public static readonly Guid AudioBitsPerSample = new Guid("f2deb57f-40fa-4764-aa33-ed4f2d1ff669");

            public static void Check(int hr, string what)
            {
                if (hr < 0) throw new InvalidOperationException(what + " 0x" + hr.ToString("X8"));
            }

            [DllImport("mfplat.dll")] public static extern int MFStartup(int version, int flags);
            [DllImport("mfplat.dll")] public static extern int MFShutdown();
            [DllImport("mfplat.dll")] public static extern int MFCreateMediaType(out IMFMediaType type);
            [DllImport("mfreadwrite.dll")]
            public static extern int MFCreateSourceReaderFromURL([MarshalAs(UnmanagedType.LPWStr)] string url, IntPtr attributes, out IMFSourceReader reader);

            [StructLayout(LayoutKind.Explicit, Size = 24)]
            public struct PropVariant
            {
                [FieldOffset(0)] public ushort vt;
                [FieldOffset(8)] public long hVal;
            }

            [ComImport, Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IMFSourceReader
            {
                [PreserveSig] int GetStreamSelection(int stream, out int selected);
                [PreserveSig] int SetStreamSelection(int stream, int selected);
                [PreserveSig] int GetNativeMediaType(int stream, int index, out IMFMediaType type);
                [PreserveSig] int GetCurrentMediaType(int stream, out IMFMediaType type);
                [PreserveSig] int SetCurrentMediaType(int stream, IntPtr reserved, IMFMediaType type);
                [PreserveSig] int SetCurrentPosition([MarshalAs(UnmanagedType.LPStruct)] Guid timeFormat, ref PropVariant position);
                [PreserveSig] int ReadSample(int stream, int control, out int actualStream, out int flags, out long timestamp, out IMFSample sample);
            }

            // IMFAttributes' 30 methods come first in both of these; only the ones called are typed.
            [ComImport, Guid("44ae0fa8-ea31-4109-8d2e-4cae4997c555"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IMFMediaType
            {
                void GetItem(); void GetItemType(); void CompareItem(); void Compare();
                [PreserveSig] int GetUINT32([MarshalAs(UnmanagedType.LPStruct)] Guid key, out int value);
                void GetUINT64(); void GetDouble(); void GetGUID(); void GetStringLength(); void GetString();
                void GetAllocatedString(); void GetBlobSize(); void GetBlob(); void GetAllocatedBlob(); void GetUnknown();
                void SetItem(); void DeleteItem(); void DeleteAllItems();
                [PreserveSig] int SetUINT32([MarshalAs(UnmanagedType.LPStruct)] Guid key, int value);
                void SetUINT64(); void SetDouble();
                [PreserveSig] int SetGUID([MarshalAs(UnmanagedType.LPStruct)] Guid key, [MarshalAs(UnmanagedType.LPStruct)] Guid value);
            }

            [ComImport, Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IMFSample
            {
                void GetItem(); void GetItemType(); void CompareItem(); void Compare(); void GetUINT32();
                void GetUINT64(); void GetDouble(); void GetGUID(); void GetStringLength(); void GetString();
                void GetAllocatedString(); void GetBlobSize(); void GetBlob(); void GetAllocatedBlob(); void GetUnknown();
                void SetItem(); void DeleteItem(); void DeleteAllItems(); void SetUINT32(); void SetUINT64();
                void SetDouble(); void SetGUID(); void SetString(); void SetBlob(); void SetUnknown();
                void LockStore(); void UnlockStore(); void GetCount(); void GetItemByIndex(); void CopyAllItems();
                void GetSampleFlags(); void SetSampleFlags(); void GetSampleTime(); void SetSampleTime();
                void GetSampleDuration(); void SetSampleDuration(); void GetBufferCount(); void GetBufferByIndex();
                [PreserveSig] int ConvertToContiguousBuffer(out IMFMediaBuffer buffer);
            }

            [ComImport, Guid("045fa593-8799-42b8-bc8d-8968c6453507"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            public interface IMFMediaBuffer
            {
                [PreserveSig] int Lock(out IntPtr buffer, out int maxLength, out int currentLength);
                [PreserveSig] int Unlock();
            }
        }
    }
}
