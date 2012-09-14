using System;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils;

namespace CSPspEmu.Core.Audio
{
	public unsafe class PspAudioChannel
	{
		/// <summary>
		/// 
		/// </summary>
		protected PspAudio PspAudio;

		/// <summary>
		/// 10 ms
		/// </summary>
		//protected const int FillSamples = 4410;
		//protected const int FillSamples = 44100;
		protected const int FillSamples = 4410 * 2;

		/// <summary>
		/// Channel's frequency. One of 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11050, 8000.
		/// </summary>
		public int Frequency = 44100;

		/// <summary>
		/// 
		/// </summary>
		public int Index;

		/// <summary>
		/// 
		/// </summary>
		public bool Available;

		/// <summary>
		/// Total amount of samples in the channel.
		/// </summary>
		public int SampleCount;

		/// <summary>
		/// Format of the audio in the channel.
		/// Can be mono or stereo.
		/// </summary>
		public PspAudio.FormatEnum Format;

		/// <summary>
		/// 
		/// </summary>
		public TimeSpan BufferTimeLength
		{
			get
			{
				return TimeSpan.FromSeconds((double)SampleCount / (double)Frequency);
			}
		}

		private ProduceConsumeBuffer<short> Buffer = new ProduceConsumeBuffer<short>();
		private List<Tuple<long, Action>> BufferEvents = new List<Tuple<long, Action>>();

		/// <summary>
		/// Returns the total number of audio channels present.
		/// </summary>
		public int NumberOfChannels
		{
			get
			{
				switch (Format)
				{
					case PspAudio.FormatEnum.Mono: return 1;
					case PspAudio.FormatEnum.Stereo: return 2;
					default: throw (new NotImplementedException());
				}
			}
		}

		private short[] Samples;
		private short[] StereoSamplesBuffer;

		public void Updated()
		{
			Samples = new short[SampleCount * NumberOfChannels];
			StereoSamplesBuffer = new short[SampleCount * 2];
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="PspAudio"></param>
		public PspAudioChannel(PspAudio PspAudio)
		{
			this.PspAudio = PspAudio;
		}

		/// <summary>
		/// Read 'Count' number of samples
		/// </summary>
		/// <param name="Count">Number of samples to read</param>
		/// <returns></returns>
		public short[] Read(int Count)
		{
			lock (this)
			{
				try
				{
					/*
					short[] Readed = new short[Count];
					int ReadCount = Math.Min(Buffer.ConsumeRemaining, Count);
					Buffer.Consume(Readed, 0, ReadCount);
					return Readed;
					*/
					return Buffer.Consume(Math.Min(Buffer.ConsumeRemaining, Count));
				}
				finally
				{
					foreach (var Event in BufferEvents.Where(ExpectedConsumed => Buffer.TotalConsumed >= ExpectedConsumed.Item1).ToArray())
					{
						BufferEvents.Remove(Event);
						Event.Item2();
					}
				}
			}
		}

		/// <summary>
		/// Converts a buffer containing mono samples
		/// into a buffer containing of stereo samples
		/// </summary>
		/// <param name="MonoSamples">Buffer that contains mono samples.</param>
		/// <returns>A buffer that contains stereo samples.</returns>
		short[] MonoToStereo(short[] MonoSamples)
		{
			var StereoSamples = StereoSamplesBuffer;
			for (int n = 0; n < MonoSamples.Length; n++)
			{
				StereoSamples[n * 2 + 0] = MonoSamples[n];
				StereoSamples[n * 2 + 1] = MonoSamples[n];
			}
			return StereoSamples;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Samples"></param>
		/// <param name="ActionCallbackOnReaded"></param>
		public void Write(short[] Samples, Action ActionCallbackOnReaded)
		{
			short[] StereoSamples;

			switch (Format)
			{
				case Audio.PspAudio.FormatEnum.Mono:
					StereoSamples = MonoToStereo(Samples);
					break;
				default:
				case Audio.PspAudio.FormatEnum.Stereo:
					StereoSamples = Samples;
					break;
			}

			lock (this)
			{
				if (Buffer.ConsumeRemaining < FillSamples)
				{
					ActionCallbackOnReaded();
				}
				else
				{
					BufferEvents.Add(new Tuple<long, Action>(Buffer.TotalConsumed + StereoSamples.Length, ActionCallbackOnReaded));
				}
				//Console.WriteLine(Format);
				Buffer.Produce(StereoSamples);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SamplePointer"></param>
		/// <param name="VolumeLeft"></param>
		/// <param name="VolumeRight"></param>
		/// <param name="ActionCallbackOnReaded"></param>
		public void Write(short* SamplePointer, int VolumeLeft, int VolumeRight, Action ActionCallbackOnReaded)
		{
			if (SamplePointer != null)
			{
				if (NumberOfChannels == 1)
				{
					int Volume = (VolumeLeft + VolumeRight) / 2;
					for (int n = 0; n < Samples.Length; n++)
					{
						Samples[n + 0] = (short)(((int)SamplePointer[n + 0] * Volume) / PspAudio.MaxVolume);
					}
				}
				else
				{
					for (int n = 0; n < Samples.Length; n += 2)
					{
						Samples[n + 0] = (short)(((int)SamplePointer[n + 0] * VolumeLeft) / PspAudio.MaxVolume);
						Samples[n + 1] = (short)(((int)SamplePointer[n + 1] * VolumeRight) / PspAudio.MaxVolume);
					}
				}
			}

			Write(Samples, ActionCallbackOnReaded);
		}

		/// <summary>
		/// Available channels that can be read.
		/// </summary>
		public int AvailableChannelsForRead
		{
			get
			{
				return Buffer.ConsumeRemaining;
			}
		}

		public override string ToString()
		{
			return String.Format("AudioChannel(Index={0},Frequency={1},Format={2},Channels={3},SampleCount={4})", Index, Frequency, Format, NumberOfChannels, SampleCount);
		}
	}
}
