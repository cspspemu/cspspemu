using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using CSharpUtils;
using WaveLib;

namespace CSPspEmu.Core.Audio.Impl.WaveOut
{
	public unsafe class PspAudioWaveOutImpl : PspAudioImpl
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

		private bool Initialized = false;
		private static IntPtr WaveOutHandle = IntPtr.Zero;

		private ConcurrentQueue<short[]> Queue = new ConcurrentQueue<short[]>();

		private WaveOutPlayer m_Player;


		private void BufferFillEventHandler(IntPtr data, int size)
		{
			if (Queue.Count > 0)
			{
				short[] Result;
				while (!Queue.TryDequeue(out Result))
				{
					if (m_Player.Disposing) return;
				}
				Marshal.Copy(Result, 0, data, size / 2);
			}
			else
			{
				PointerUtils.Memset((byte*)data.ToPointer(), 0, size);
			}
		}

		public override void Update(Action<short[]> ReadStream)
		{
			if (Initialized)
			{
				while (Queue.Count < 2)
				{
					var Data = new short[BufferSize / 2];
					ReadStream(Data);
					//for (int n = 0; n < Data.Length; n++) Console.Write(Data[n]);
					Queue.Enqueue(Data);
				}
			}
		}

		public override void StopSynchronized()
		{
			Initialized = false;
			m_Player.Stop();
		}

		public PspAudioWaveOutImpl()
		{
			m_Player = new WaveOutPlayer(-1, new WaveFormat(rate: Frequency, bits: 16, channels: NumberOfChannels), BufferSize, NumberOfBuffers, BufferFillEventHandler);
			Initialized = true;
		}

		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "WaveOut",
					Version = "1.0",
				};
			}
		}

		public override bool IsWorking
		{
			get
			{
				if (Platform.OperatingSystem != Platform.OS.Windows) return false;
				if (WaveOutPlayer.DeviceCount == 0) return false;
				return true;
			}
		}
	}
}
