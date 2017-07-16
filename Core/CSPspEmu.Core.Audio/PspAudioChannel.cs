using System;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils;
using System.Threading.Tasks;
using System.Threading;

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
		/// 
		/// </summary>
		public bool IsReserved
		{
			get { return !Available; }
			set { Available = !value; }
		}

		/// <summary>
		/// Total amount of samples in the channel.
		/// </summary>
		private int _SampleCount;

		public int SampleCount
		{
			get { return _SampleCount; }
			set { _SampleCount = Math.Max(0, value); }
		}

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
					default: throw (new InvalidAudioFormatException());
				}
			}
			set
			{
				switch (value)
				{
					case 1: Format = PspAudio.FormatEnum.Mono; break;
					case 2: Format = PspAudio.FormatEnum.Stereo; break;
					default: throw (new InvalidAudioFormatException());
				}
			}
		}

		private short[] _samples = new short[0];
		private short[] _stereoSamplesBuffer = new short[0];

		public int VolumeLeft = PspAudio.MaxVolume;
		public int VolumeRight = PspAudio.MaxVolume;

		public void Release()
		{
			IsReserved = false;
			VolumeLeft = PspAudio.MaxVolume;
			VolumeRight = PspAudio.MaxVolume;
		}

		public void Updated()
		{
			if (SampleCount < 1) throw(new InvalidOperationException("SampleCount < 1"));
			if (NumberOfChannels < 1) throw (new InvalidOperationException("NumberOfChannels < 1"));
			_samples = new short[SampleCount * NumberOfChannels];
			_stereoSamplesBuffer = new short[SampleCount * 2];
			VolumeLeft = PspAudio.MaxVolume;
			VolumeRight = PspAudio.MaxVolume;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="pspAudio"></param>
		public PspAudioChannel(PspAudio pspAudio)
		{
			this.PspAudio = pspAudio;
		}

		/// <summary>
		/// Read 'Count' number of samples
		/// </summary>
		/// <param name="count">Number of samples to read</param>
		/// <returns></returns>
		public short[] Read(int count)
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
					return Buffer.Consume(Math.Min(Buffer.ConsumeRemaining, count));
				}
				finally
				{
					foreach (var Event in BufferEvents.Where(expectedConsumed => Buffer.TotalConsumed >= expectedConsumed.Item1).ToArray())
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
		/// <param name="monoSamples">Buffer that contains mono samples.</param>
		/// <returns>A buffer that contains stereo samples.</returns>
		private short[] MonoToStereo(short[] monoSamples)
		{
			var stereoSamples = _stereoSamplesBuffer;
			for (var n = 0; n < monoSamples.Length; n++)
			{
				stereoSamples[n * 2 + 0] = monoSamples[n];
				stereoSamples[n * 2 + 1] = monoSamples[n];
			}
			return stereoSamples;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="samples"></param>
		/// <param name="actionCallbackOnReaded"></param>
		public void Write(short[] samples, Action actionCallbackOnReaded)
		{
			if (samples == null) throw (new InvalidOperationException("short[] Samples is null"));

			short[] stereoSamples;

			switch (Format)
			{
				case Audio.PspAudio.FormatEnum.Mono:
					stereoSamples = MonoToStereo(samples);
					break;
				default:
				case Audio.PspAudio.FormatEnum.Stereo:
					stereoSamples = samples;
					break;
			}

			lock (this)
			{
				if (Buffer.ConsumeRemaining < FillSamples)
				{
					Task.Run(() =>
					{
						Thread.Sleep(1);
						actionCallbackOnReaded();
					});
				}
				else
				{
					BufferEvents.Add(new Tuple<long, Action>(Buffer.TotalConsumed + stereoSamples.Length, actionCallbackOnReaded));
				}
				//Console.WriteLine(Format);
				Buffer.Produce(stereoSamples);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="samplePointer"></param>
		/// <param name="volumeLeft"></param>
		/// <param name="volumeRight"></param>
		/// <param name="actionCallbackOnReaded"></param>
		public void Write(short* samplePointer, int volumeLeft, int volumeRight, Action actionCallbackOnReaded)
		{
			//Console.WriteLine("{0}", this.Frequency);
			volumeLeft = volumeLeft * this.VolumeLeft / PspAudio.MaxVolume;
			volumeRight = volumeRight * this.VolumeRight / PspAudio.MaxVolume;

			if (samplePointer != null)
			{
				if (NumberOfChannels == 1)
				{
					var volume = (volumeLeft + volumeRight) / 2;
					for (var n = 0; n < _samples.Length; n++)
					{
						_samples[n + 0] = (short)(samplePointer[n + 0] * volume / PspAudio.MaxVolume);
					}
				}
				else
				{
					for (var n = 0; n < _samples.Length; n += 2)
					{
						_samples[n + 0] = (short)((samplePointer[n + 0] * volumeLeft) / PspAudio.MaxVolume);
						_samples[n + 1] = (short)((samplePointer[n + 1] * volumeRight) / PspAudio.MaxVolume);
					}
				}
			}

			Write(_samples, actionCallbackOnReaded);
		}

		/// <summary>
		/// Available channels that can be read.
		/// </summary>
		public int AvailableChannelsForRead => Buffer.ConsumeRemaining;

		public override string ToString()
		{
			return
				$"AudioChannel(Index={Index},Frequency={Frequency},Format={Format},Channels={NumberOfChannels},SampleCount={SampleCount})";
		}
	}
	public class InvalidAudioFormatException : Exception
	{
	}
}
