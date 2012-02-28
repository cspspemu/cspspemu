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

		ConcurrentQueue<short[]> Queue = new ConcurrentQueue<short[]>();

		WaveOutPlayer m_Player;


		void BufferFillEventHandler(IntPtr data, int size)
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

		public override void InitializeComponent()
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
	}
}
