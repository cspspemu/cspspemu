using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Audio.Impl.WaveOut
{
	unsafe public class PspAudioWaveOutImpl : PspAudioImpl
	{
		IntPtr waveOutHandle;
		WAVEFORMATEX pcmwf;
		WAVEHDR wavehdr;

		public override void Update(Func<int, short[]> ReadStream)
		{
			/*
			int ReadCount = 1024;
			short[] Data = ReadStream(1024);

			wavehdr.dwFlags         = WHDR_DONE;
			wavehdr.lpData          = new IntPtr(&Data[0]);
			wavehdr.dwBufferLength  = Data.Length * sizeof(short);
			wavehdr.dwBytesRecorded = 0;
			wavehdr.dwUser          = 0;
			wavehdr.dwLoops         = 0;
			waveOutPrepareHeader(waveOutHandle, ref wavehdr, (uint)sizeof(WAVEHDR));
			*/
		}

		public override void StopSynchronized()
		{
			//throw new NotImplementedException();
		}

		public override void InitializeComponent()
		{
			pcmwf.wFormatTag      = WAVE_FORMAT_PCM; 
			pcmwf.nChannels       = 2;
			pcmwf.wBitsPerSample  = 16;
			pcmwf.nBlockAlign     = 2 * sizeof(short);
			pcmwf.nSamplesPerSec  = 44100;
			pcmwf.nAvgBytesPerSec = pcmwf.nSamplesPerSec * pcmwf.nBlockAlign; 
			pcmwf.cbSize          = 0;
			waveOutOpen(out waveOutHandle, WAVE_MAPPER, ref pcmwf, 0, 0, 0);
			//throw new NotImplementedException();
		}

		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutOpen(out IntPtr phwo, uint uDeviceID, ref WAVEFORMATEX pwfx, uint dwCallback, uint dwInstance, uint fdwOpen);
		
		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutPrepareHeader(IntPtr hwo, ref WAVEHDR pwh, uint cbwh);
	
		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutWrite(IntPtr hwo, WAVEHDR* pwh, uint cbwh);

		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutClose(IntPtr hwo);

		public const ushort WAVE_FORMAT_UNKNOWN = 0x0000;
		public const ushort WAVE_FORMAT_PCM = 0x0001;
		public const ushort WAVE_FORMAT_ADPCM = 0x0002;
		public const ushort WAVE_FORMAT_IEEE_FLOAT = 0x0003;
		public const uint WAVE_MAPPER = 0xFFFFFFFF;

		private const int MMRESULT_BASE = 0;
		public enum MMRESULT : int
		{
			NOERROR = 0,
			ERROR = (MMRESULT_BASE + 1),
			BADDEVICEID = (MMRESULT_BASE + 2),
			NOTENABLED = (MMRESULT_BASE + 3),
			ALLOCATED = (MMRESULT_BASE + 4),
			INVALHANDLE = (MMRESULT_BASE + 5),
			NODRIVER = (MMRESULT_BASE + 6),
			NOMEM = (MMRESULT_BASE + 7),
			NOTSUPPORTED = (MMRESULT_BASE + 8),
			BADERRNUM = (MMRESULT_BASE + 9),
			INVALFLAG = (MMRESULT_BASE + 10),
			INVALPARAM = (MMRESULT_BASE + 11),
			HANDLEBUSY = (MMRESULT_BASE + 12),
			INVALIDALIAS = (MMRESULT_BASE + 13),
			BADDB = (MMRESULT_BASE + 14),
			KEYNOTFOUND = (MMRESULT_BASE + 15),
			READERROR = (MMRESULT_BASE + 16),
			WRITEERROR = (MMRESULT_BASE + 17),
			DELETEERROR = (MMRESULT_BASE + 18),
			VALNOTFOUND = (MMRESULT_BASE + 19),
			NODRIVERCB = (MMRESULT_BASE + 20),
			LASTERROR = (MMRESULT_BASE + 20),
			BADFORMAT = (MMRESULT_BASE + 32)
		}

		public const uint WHDR_DONE       = 0x00000001;
		public const uint WHDR_PREPARED = 0x00000002;
		public const uint WHDR_BEGINLOOP = 0x00000004;
		public const uint WHDR_ENDLOOP = 0x00000008;
		public const uint WHDR_INQUEUE = 0x00000010;

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct WAVEFORMATEX
		{
			public ushort wFormatTag;
			public ushort nChannels;
			public uint nSamplesPerSec;
			public uint nAvgBytesPerSec;
			public ushort nBlockAlign;
			public ushort wBitsPerSample;
			public ushort cbSize;
		}

		public unsafe struct WAVEHDR
		{
			public IntPtr lpData;
			public uint dwBufferLength;
			public uint dwBytesRecorded;
			public uint dwUser;
			public uint dwFlags;
			public uint dwLoops;
			public IntPtr lpNext;
			public uint reserved;
		}
	}
}
