using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Core.Audio
{
	unsafe public class PspAudioChannel
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
		/// 
		/// </summary>
		public int SampleCount;

		/// <summary>
		/// 
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
		/// 
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

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="PspAudio"></param>
		public PspAudioChannel(PspAudio PspAudio)
		{
			this.PspAudio = PspAudio;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Count"></param>
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
		/// 
		/// </summary>
		/// <param name="Samples"></param>
		/// <param name="ActionCallbackOnReaded"></param>
		public void Write(short[] Samples, Action ActionCallbackOnReaded)
		{
			if (Format != Audio.PspAudio.FormatEnum.Stereo) throw(new NotImplementedException());
			lock (this)
			{
				if (Buffer.ConsumeRemaining < FillSamples)
				{
					ActionCallbackOnReaded();
				}
				else
				{
					BufferEvents.Add(new Tuple<long, Action>(Buffer.TotalConsumed + Samples.Length, ActionCallbackOnReaded));
				}
				//Console.WriteLine(Format);
				Buffer.Produce(Samples);
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
			var Samples = new short[SampleCount * 2];
			bool HasSignal = false;
			for (int n = 0; n < Samples.Length; n += 2)
			{
				Samples[n + 0] = (short)(((int)SamplePointer[n + 0] * VolumeLeft) / PspAudio.MaxVolume);
				Samples[n + 1] = (short)(((int)SamplePointer[n + 1] * VolumeRight) / PspAudio.MaxVolume);
				if (Samples[n + 0] != 0 || Samples[n + 1] != 0)
				{
					HasSignal = true;
				}
			}
			if (HasSignal)
			{
				for (int n = 0; n < Samples.Length; n++)
				{
					//Console.Write("{0},", Samples[n]);
				}
			}
			Write(Samples, ActionCallbackOnReaded);
		}
	}
}
