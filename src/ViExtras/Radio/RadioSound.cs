// The radio wheel's open / close sounds (vi_radio/html/sfx/radioopen.wav and
// radioclose.wav: sej0bec's, from the original VI Radio mod, redistributed with
// credit; see Radio.cs).
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace ViExtras
{
    /// <summary>
    /// Plays a .wav the way vi_radio's page does (app.js, action 'sound'):
    /// <c>new Audio(file)</c> with <c>audio.volume = Config.SoundVolume / 100</c>.
    /// HTML audio volume is a straight gain on the samples, so the samples are
    /// scaled by it here. Each sound gets its own waveOut device, so the close
    /// sound does not cut off an open sound still playing, as two Audio objects
    /// in the page would not either. (System.Media.SoundPlayer has no volume and
    /// only one sound at a time.)
    /// </summary>
    internal static class RadioSound
    {
        private sealed class Clip
        {
            public WaveFormat Format;
            public byte[] Data;
        }

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
        [DllImport("winmm.dll")] private static extern int waveOutReset(IntPtr hwo);
        [DllImport("winmm.dll")] private static extern int waveOutClose(IntPtr hwo);

        private const uint WaveMapper = 0xFFFFFFFF;
        private const uint WhdrDone = 1;

        private static readonly Dictionary<string, Clip> Clips = new Dictionary<string, Clip>(StringComparer.OrdinalIgnoreCase);

        public static void Play(string path, float volume)
        {
            try
            {
                volume = Math.Max(0f, Math.Min(1f, volume));
                Clip clip = Load(path, volume);
                if (clip == null || clip.Data.Length == 0 || volume <= 0f) return;
                // a pool thread rather than a new one a click; still one a sound, so two can overlap
                ThreadPool.QueueUserWorkItem(_ => Run(clip));
            }
            catch (Exception ex)
            {
                Log.Write("Radio", "sound " + Path.GetFileName(path) + ": " + ex.Message);
            }
        }

        private static Clip Load(string path, float volume)
        {
            string key = path + "|" + volume.ToString("0.###");
            if (Clips.TryGetValue(key, out Clip clip)) return clip;
            Clips[key] = null;
            if (!File.Exists(path))
            {
                Log.Write("Radio", "missing sound " + path);
                return null;
            }

            byte[] file = File.ReadAllBytes(path);
            WaveFormat fmt = default;
            byte[] data = null;
            bool haveFmt = false;
            int at = 12; // "RIFF" size "WAVE"
            while (at + 8 <= file.Length)
            {
                string id = System.Text.Encoding.ASCII.GetString(file, at, 4);
                int len = BitConverter.ToInt32(file, at + 4);
                int body = at + 8;
                if (len < 0 || body + len > file.Length) len = file.Length - body;
                if (id == "fmt ")
                {
                    fmt.Tag = BitConverter.ToUInt16(file, body);
                    fmt.Channels = BitConverter.ToUInt16(file, body + 2);
                    fmt.SamplesPerSec = BitConverter.ToUInt32(file, body + 4);
                    fmt.AvgBytesPerSec = BitConverter.ToUInt32(file, body + 8);
                    fmt.BlockAlign = BitConverter.ToUInt16(file, body + 12);
                    fmt.BitsPerSample = BitConverter.ToUInt16(file, body + 14);
                    fmt.Size = 0;
                    haveFmt = true;
                }
                else if (id == "data")
                {
                    data = new byte[len];
                    Buffer.BlockCopy(file, body, data, 0, len);
                }
                at = body + len + (len & 1);
            }
            if (!haveFmt || data == null || fmt.Tag != 1)
            {
                Log.Write("Radio", "not a PCM wave file: " + path);
                return null;
            }

            // audio.volume: every sample times the gain.
            if (fmt.BitsPerSample == 16)
            {
                for (int i = 0; i + 1 < data.Length; i += 2)
                {
                    int s = (short)(data[i] | (data[i + 1] << 8));
                    s = (int)Math.Round(s * volume);
                    data[i] = (byte)(s & 0xFF);
                    data[i + 1] = (byte)((s >> 8) & 0xFF);
                }
            }
            else if (fmt.BitsPerSample == 8)
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] = (byte)Math.Round(128 + (data[i] - 128) * volume);
            }

            clip = new Clip { Format = fmt, Data = data };
            Clips[key] = clip;
            return clip;
        }

        private static void Run(Clip clip)
        {
            IntPtr hwo = IntPtr.Zero, buffer = IntPtr.Zero, header = IntPtr.Zero;
            uint headerSize = (uint)Marshal.SizeOf(typeof(WaveHeader));
            bool prepared = false;
            try
            {
                WaveFormat fmt = clip.Format;
                if (waveOutOpen(out hwo, WaveMapper, ref fmt, IntPtr.Zero, IntPtr.Zero, 0) != 0) { hwo = IntPtr.Zero; return; }
                buffer = Marshal.AllocHGlobal(clip.Data.Length);
                Marshal.Copy(clip.Data, 0, buffer, clip.Data.Length);
                header = Marshal.AllocHGlobal((int)headerSize);
                Marshal.StructureToPtr(new WaveHeader { Data = buffer, BufferLength = (uint)clip.Data.Length }, header, false);
                if (waveOutPrepareHeader(hwo, header, headerSize) != 0) return;
                prepared = true;
                if (waveOutWrite(hwo, header, headerSize) != 0) return;

                double seconds = clip.Data.Length / (double)Math.Max(1u, clip.Format.AvgBytesPerSec);
                DateTime giveUp = DateTime.UtcNow.AddSeconds(seconds + 2.0);
                while (DateTime.UtcNow < giveUp)
                {
                    var h = (WaveHeader)Marshal.PtrToStructure(header, typeof(WaveHeader));
                    if ((h.Flags & WhdrDone) != 0) break;
                    Thread.Sleep(20);
                }
            }
            catch { }
            finally
            {
                try
                {
                    if (hwo != IntPtr.Zero)
                    {
                        waveOutReset(hwo);
                        if (prepared) waveOutUnprepareHeader(hwo, header, headerSize);
                        waveOutClose(hwo);
                    }
                }
                catch { }
                if (header != IntPtr.Zero) Marshal.FreeHGlobal(header);
                if (buffer != IntPtr.Zero) Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
