// Opening and closing waveOut devices, for MusicPlayer and RadioSound. Credits and licence: see Radio.cs.
using System;
using System.Runtime.InteropServices;

namespace ViExtras
{
    /// <summary>
    /// Every waveOut device ViExtras opens or closes goes through here, one at a time.
    ///
    /// A freeze in a test run (2026-11-06, a dump of the frozen game): MusicPlayer's thread sat in waveOutOpen,
    /// inside the wave mapper (WAVE_MAPPER: msacm32.drv, then wdmaud.drv), while three new threads of the game's
    /// waited to start in msacm32's DllMain. The mapper waited on them and they on it, and the game's own thread
    /// waited behind them, for good. So the device is asked for by its number, Windows' preferred one
    /// (DRVM_MAPPER_PREFERRED_GET, answered by winmm itself), which goes straight to wdmaud.drv without the mapper;
    /// since Windows Vista that path takes any PCM format and converts it in the audio engine. The mapper stays as
    /// the fallback if the device won't open. And an open never overlaps another (Gate), so two sounds starting
    /// together don't race in the driver either.
    /// </summary>
    internal static class WaveDevice
    {
        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        internal struct Format
        {
            public ushort Tag, Channels;
            public uint SamplesPerSec, AvgBytesPerSec;
            public ushort BlockAlign, BitsPerSample, Size;
        }

        [DllImport("winmm.dll")] private static extern int waveOutOpen(out IntPtr hwo, uint device, ref Format fmt, IntPtr callback, IntPtr instance, uint flags);
        [DllImport("winmm.dll")] private static extern int waveOutClose(IntPtr hwo);
        [DllImport("winmm.dll")] private static extern int waveOutReset(IntPtr hwo);
        [DllImport("winmm.dll")] private static extern int waveOutMessage(IntPtr hwo, uint msg, out uint dw1, out uint dw2);
        [DllImport("winmm.dll")] private static extern uint waveOutGetNumDevs();

        private const uint WaveMapper = 0xFFFFFFFF;
        private const uint DrvmMapperPreferredGet = 0x2000 + 21;   // DRVM_MAPPER_PREFERRED_GET

        internal static readonly object Gate = new object();
        private static bool _mapperLogged;

        public static Format Pcm(int rate, int channels, int bits)
        {
            int block = channels * bits / 8;
            return new Format
            {
                Tag = 1, Channels = (ushort)channels, SamplesPerSec = (uint)rate, BitsPerSample = (ushort)bits,
                BlockAlign = (ushort)block, AvgBytesPerSec = (uint)(rate * block)
            };
        }

        /// <summary>Windows' preferred playback device by number, or -1 if it won't say.</summary>
        public static int Preferred()
        {
            try
            {
                if (waveOutGetNumDevs() == 0) return -1;
                if (waveOutMessage(new IntPtr(-1), DrvmMapperPreferredGet, out uint dev, out _) == 0 && dev != WaveMapper) return (int)dev;
                return 0;
            }
            catch { return -1; }
        }

        /// <summary>
        /// Opens a device for this format: the preferred one, else the wave mapper. 0 when it opened (device: the
        /// number it opened on, -1 for the mapper), else waveOutOpen's error.
        /// </summary>
        public static int Open(ref Format fmt, out IntPtr hwo, out int device)
        {
            lock (Gate)
            {
                device = Preferred();
                int err = -1;
                hwo = IntPtr.Zero;
                if (device >= 0)
                {
                    err = waveOutOpen(out hwo, (uint)device, ref fmt, IntPtr.Zero, IntPtr.Zero, 0);
                    if (err == 0) return 0;
                }
                if (!_mapperLogged)
                {
                    _mapperLogged = true;
                    Log.Write("Radio", "sound: device " + device + " wouldn't open (" + err + "), using Windows' wave mapper");
                }
                device = -1;
                err = waveOutOpen(out hwo, WaveMapper, ref fmt, IntPtr.Zero, IntPtr.Zero, 0);
                if (err != 0) hwo = IntPtr.Zero;
                return err;
            }
        }

        /// <summary>Stops what's playing and closes it (its headers already unprepared).</summary>
        public static void Close(IntPtr hwo)
        {
            if (hwo == IntPtr.Zero) return;
            lock (Gate)
            {
                waveOutReset(hwo);
                waveOutClose(hwo);
            }
        }
    }
}
