using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils;
using System.Threading;
using WaveLib;
using System.Collections.Concurrent;

namespace CSPspEmu.Core.Audio.Impl.WaveOut
{
	//[AttributeUsage(AttributeTargets.
	unsafe public class PspAudioWaveOutImpl : PspAudioImpl
	{
		public const int Frequency = 44100;
		//public const int Frequency = 48000;

		//public const double SamplesPerMillisecond = ((double)Frequency) / 1000;
		public const double SamplesPerMillisecond = ((double)Frequency) / 500;
		//public const double SamplesPerMillisecond = ((double)Frequency) / 100;
		//public const int BufferSize = 16384;
		public const int NumberOfBuffers = 4;
		public const int NumberOfChannels = 2;
		public const int BufferMilliseconds = 10;
		public const int SamplesPerBuffer = (int)(SamplesPerMillisecond * BufferMilliseconds * NumberOfChannels);
		public const int BufferSize = (int)SamplesPerBuffer;

		bool Initialized = false;
		static IntPtr WaveOutHandle = IntPtr.Zero;
		static Buffer[] Buffers = new Buffer[NumberOfBuffers];

		ConcurrentQueue<short[]> Queue = new ConcurrentQueue<short[]>();

		WaveOutPlayer m_Player;


		void BufferFillEventHandler(IntPtr data, int size)
		{
			if (Queue.Count > 0)
			{
				short[] Result;
				while (!Queue.TryDequeue(out Result)) ;
				Marshal.Copy(Result, 0, data, size / 2);
				//Marshal.Copy(Array, 0, data, 32);
				//Console.WriteLine("BufferFillEventHandler: {0}", size);
			}
			else
			{
			}
		}

		public override void Update(Action<short[]> ReadStream)
		{
			if (Initialized)
			{
				//Console.WriteLine("aaaaaaaa");
				while (Queue.Count < 4)
				{
					var Data = new short[BufferSize / 2];
					ReadStream(Data);
					Queue.Enqueue(Data);
				}
				/*
				foreach (Buffer Buffer in Buffers)
				{
					if (Buffer != null && Buffer.Initialized && Buffer.Ready)
					{
						ReadStream(Buffer.Data);
						Buffer.Play();
					}
				}
				*/
			}
		}

		public class Buffer : IDisposable
		{
			PspAudioWaveOutImpl PspAudioWaveOutImpl;
			WaveHeaderStruct WaveHeader;
			public short[] Data;

			public GCHandle FixedWaveHeader;
			public GCHandle FixedData;

			internal Buffer(PspAudioWaveOutImpl PspAudioWaveOutImpl)
			{
				this.PspAudioWaveOutImpl = PspAudioWaveOutImpl;
				Data = new short[441 * 4];
				FixedData = GCHandle.Alloc(Data, GCHandleType.Pinned);
				FixedWaveHeader = GCHandle.Alloc(Data, GCHandleType.Pinned);
				Prepare();
			}

			public void Dispose()
			{
				/*
				if (FixedData != IntPtr.Zero)
				{
					Stop();
					Marshal.FreeHGlobal(FixedData);
					FixedData = IntPtr.Zero;
				}
				*/
			}

			~Buffer()
			{
				Dispose();
			}

			public void Prepare()
			{
				if (Initialized)
				{
					WaveHeader = new WaveHeaderStruct()
					{
						Flags = WaveState.WHDR_DONE,
						DataPointer = FixedData.AddrOfPinnedObject(),
						DataLength = (uint)(Data.Length * sizeof(short)),
						BytesRecorded = 0,
						User = 0,
						Loops = 0,
					};
					Enforce(waveOutPrepareHeader(PspAudioWaveOutImpl.WaveOutHandle, ref WaveHeader, (uint)sizeof(WaveHeaderStruct)));

					WaveHeader.Flags |= WaveState.WHDR_DONE;
				}
			}

			public void Stop()
			{
				while (!Ready) Thread.Sleep(1);
			}

			public void Play()
			{
				if (Initialized && Ready)
				{
					//Marshal.Copy(Data, 0, FixedData, Data.Length);

					WaveHeader.Flags &= ~WaveState.WHDR_DONE;
					Enforce(waveOutWrite(PspAudioWaveOutImpl.WaveOutHandle, ref WaveHeader, (uint)sizeof(WaveHeaderStruct)));
				}
			}

			public bool Initialized
			{
				get
				{
					return PspAudioWaveOutImpl.Initialized && (PspAudioWaveOutImpl.WaveOutHandle != IntPtr.Zero);
				}
			}
				
			public bool Ready
			{
				get
				{
					return (WaveHeader.Flags & WaveState.WHDR_DONE) != 0;
				}
			}
		}

		public override void StopSynchronized()
		{
			//throw new NotImplementedException();
			/*
			for (int n = 0; n < Buffers.Length; n++) Buffers[n].Dispose();
			try
			{
				Enforce(waveOutClose(WaveOutHandle));
			}
			catch
			{
			}
			*/
			Initialized = false;
			m_Player.Dispose();
		}

		public override void InitializeComponent()
		{
			m_Player = new WaveOutPlayer(-1, new WaveFormat(rate: Frequency, bits: 16, channels: NumberOfChannels), BufferSize, NumberOfBuffers, BufferFillEventHandler);
			Initialized = true;
			/*
			var WaveInitFormat = new WAVEFORMATEX()
			{
				Format = WaveFormatEnum.WAVE_FORMAT_PCM,
				Channels = NumberOfChannels,
				BitsPerSample = 16,
				BlockAlignment = NumberOfChannels * sizeof(short),
				SamplesPerSecond = Frequency,
				AverageBytesPerSecond = Frequency * NumberOfChannels * sizeof(short),
				cbSize = 0,
			};
			Enforce(waveOutOpen(ref WaveOutHandle, WAVE_MAPPER, ref WaveInitFormat, 0, 0, 0));

			for (int n = 0; n < Buffers.Length; n++) Buffers[n] = new Buffer(this);
			*/
		}

		static private void Enforce(MMRESULT Result)
		{
			if (Result != MMRESULT.MMSYSERR_NOERROR)
			{
				//throw (new Exception("Error: " + Result));
				Console.Error.WriteLine("Error: " + Result);
			}
		}

		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutOpen(ref IntPtr phwo, uint uDeviceID, ref WAVEFORMATEX pwfx, uint dwCallback, uint dwInstance, uint fdwOpen);
		
		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutPrepareHeader(IntPtr hwo, ref WaveHeaderStruct pwh, uint cbwh);
	
		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutWrite(IntPtr hwo, ref WaveHeaderStruct pwh, uint cbwh);

		[DllImport("winmm.dll")]
		private static extern MMRESULT waveOutClose(IntPtr hwo);

		public const uint WAVE_MAPPER = 0xFFFFFFFF;

		[StructLayout(LayoutKind.Sequential)]
		public unsafe struct WAVEFORMATEX
		{
			public WaveFormatEnum Format;
			public ushort Channels;
			public uint SamplesPerSecond;
			public uint AverageBytesPerSecond;
			public ushort BlockAlignment;
			public ushort BitsPerSample;
			public ushort cbSize;
		}

		public unsafe struct WaveHeaderStruct
		{
			public IntPtr DataPointer;
			public uint DataLength;
			public uint BytesRecorded;
			public uint User;
			public WaveState Flags;
			public uint Loops;
			public IntPtr Next;
			public uint Reserved;
		}

		public struct MMTIME
		{
			public uint wType;
			public uint Data;
		}

		public enum TimeType
		{
			TIME_MS      = 0x0001,  // time in milliseconds
			TIME_SAMPLES = 0x0002,  // number of wave samples
			TIME_BYTES   = 0x0004,  // current byte offset
			TIME_SMPTE   = 0x0008,  // SMPTE time
			TIME_MIDI    = 0x0010,  // MIDI time
			TIME_TICKS       = 0x0020,  // MIDI ticks
		}

		public enum WaveFormatEnum : ushort
		{
			WAVE_FORMAT_UNKNOWN    = 0x0000,
			WAVE_FORMAT_PCM        = 0x0001,
			WAVE_FORMAT_ADPCM      = 0x0002,
			WAVE_FORMAT_IEEE_FLOAT = 0x0003,
		}

		public enum WaveState : uint
		{
			WHDR_DONE       = 0x00000001,
			WHDR_PREPARED   = 0x00000002,
			WHDR_BEGINLOOP  = 0x00000004,
			WHDR_ENDLOOP    = 0x00000008,
			WHDR_INQUEUE    = 0x00000010,
		}

		public enum MMRESULT : uint
		{
			MMSYSERR_NOERROR        = 0,
			MMSYSERR_ERROR          = 1,
			MMSYSERR_BADDEVICEID    = 2,
			MMSYSERR_NOTENABLED     = 3,
			MMSYSERR_ALLOCATED      = 4,
			MMSYSERR_INVALHANDLE    = 5,
			MMSYSERR_NODRIVER       = 6,
			MMSYSERR_NOMEM          = 7,
			MMSYSERR_NOTSUPPORTED   = 8,
			MMSYSERR_NOMAP          = 7,

			MIDIERR_UNPREPARED      = 64,
			MIDIERR_STILLPLAYING    = 65,
			MIDIERR_NOTREADY        = 66,
			MIDIERR_NODEVICE        = 67,

			WAVERR_BADFORMAT        = 32,
			WAVERR_STILLPLAYING     = 33,
			WAVERR_UNPREPARED       = 34,
			WAVERR_SYNC             = 35,

			MAXERRORLENGTH          = 128,
		}
	}
}
