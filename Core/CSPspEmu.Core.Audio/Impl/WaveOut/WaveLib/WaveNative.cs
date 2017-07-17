//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE.
//
//  This material may not be duplicated in whole or in part, except for 
//  personal use, without the express written consent of the author. 
//
//  Email:  ianier@hotmail.com
//
//  Copyright (C) 1999-2003 Ianier Munoz. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Audio.Impl.WaveOut.WaveLib
{
    public enum WaveFormats
    {
        Pcm = 1,
        Float = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    public class WaveFormat
    {
        public short wFormatTag;
        public short nChannels;
        public int nSamplesPerSec;
        public int nAvgBytesPerSec;
        public short nBlockAlign;
        public short wBitsPerSample;
        public short cbSize;

        public WaveFormat(int rate, int bits, int channels)
        {
            wFormatTag = (short) WaveFormats.Pcm;
            nChannels = (short) channels;
            nSamplesPerSec = rate;
            wBitsPerSample = (short) bits;
            cbSize = 0;

            nBlockAlign = (short) (channels * (bits / 8));
            nAvgBytesPerSec = nSamplesPerSec * nBlockAlign;
        }
    }

    internal class WaveNative
    {
        // consts
        public const int MmsyserrNoerror = 0; // no error

        public const int MmWomOpen = 0x3BB;
        public const int MmWomClose = 0x3BC;
        public const int MmWomDone = 0x3BD;

        public const int MmWimOpen = 0x3BE;
        public const int MmWimClose = 0x3BF;
        public const int MmWimData = 0x3C0;

        public const int CallbackFunction = 0x00030000; // dwCallback is a FARPROC 

        public const int TimeMs = 0x0001; // time in milliseconds 
        public const int TimeSamples = 0x0002; // number of wave samples 
        public const int TimeBytes = 0x0004; // current byte offset 

        // callbacks
        public delegate void WaveDelegate(IntPtr hdrvr, int uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2);

        // structs 

        [StructLayout(LayoutKind.Sequential)]
        public struct WaveHdr
        {
            public IntPtr lpData; // pointer to locked data buffer
            public int dwBufferLength; // length of data buffer
            public int dwBytesRecorded; // used for input only
            public IntPtr dwUser; // for client's use
            public int dwFlags; // assorted flags (see defines)
            public int dwLoops; // loop control counter
            public IntPtr lpNext; // PWaveHdr, reserved for driver
            public int reserved; // reserved for driver
        }

        private const string Mmdll = "winmm.dll";

        // WaveOut calls
        [DllImport(Mmdll)]
        public static extern int waveOutGetNumDevs();

        [DllImport(Mmdll)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

        [DllImport(Mmdll)]
        public static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

        [DllImport(Mmdll)]
        public static extern int waveOutWrite(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

        [DllImport(Mmdll)]
        public static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceId, WaveFormat lpFormat,
            WaveDelegate dwCallback, int dwInstance, int dwFlags);

        [DllImport(Mmdll)]
        public static extern int waveOutReset(IntPtr hWaveOut);

        [DllImport(Mmdll)]
        public static extern int waveOutClose(IntPtr hWaveOut);

        [DllImport(Mmdll)]
        public static extern int waveOutPause(IntPtr hWaveOut);

        [DllImport(Mmdll)]
        public static extern int waveOutRestart(IntPtr hWaveOut);

        [DllImport(Mmdll)]
        public static extern int waveOutGetPosition(IntPtr hWaveOut, out int lpInfo, int uSize);

        [DllImport(Mmdll)]
        public static extern int waveOutSetVolume(IntPtr hWaveOut, int dwVolume);

        [DllImport(Mmdll)]
        public static extern int waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);

        // WaveIn calls
        [DllImport(Mmdll)]
        public static extern int waveInGetNumDevs();

        [DllImport(Mmdll)]
        public static extern int waveInAddBuffer(IntPtr hwi, ref WaveHdr pwh, int cbwh);

        [DllImport(Mmdll)]
        public static extern int waveInClose(IntPtr hwi);

        [DllImport(Mmdll)]
        public static extern int waveInOpen(out IntPtr phwi, int uDeviceId, WaveFormat lpFormat,
            WaveDelegate dwCallback, int dwInstance, int dwFlags);

        [DllImport(Mmdll)]
        public static extern int waveInPrepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);

        [DllImport(Mmdll)]
        public static extern int waveInUnprepareHeader(IntPtr hWaveIn, ref WaveHdr lpWaveInHdr, int uSize);

        [DllImport(Mmdll)]
        public static extern int waveInReset(IntPtr hwi);

        [DllImport(Mmdll)]
        public static extern int waveInStart(IntPtr hwi);

        [DllImport(Mmdll)]
        public static extern int waveInStop(IntPtr hwi);
    }
}