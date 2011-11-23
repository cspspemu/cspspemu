using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	public interface IPspAudioImpl
	{
		void OutputStereo16_48000(int ChannelId, short[] Samples, Action Callback);
	}

	unsafe public class PspAudio
	{
		/// <summary>
		/// 
		/// </summary>
		public enum FormatEnum
		{
			/// <summary>
			/// Channel is set to stereo output (2 channels).
			/// </summary>
			Stereo = 0x00,

			/// <summary>
			/// Channel is set to mono output (1 channel).
			/// </summary>
			Mono = 0x10,
		}

		public class Channel
		{
			/// <summary>
			/// 
			/// </summary>
			protected PspAudio PspAudio;

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
			public FormatEnum Format;

			/// <summary>
			/// 
			/// </summary>
			public int NumberOfChannels
			{
				get
				{
					switch (Format)
					{
						case FormatEnum.Mono: return 1;
						case FormatEnum.Stereo: return 2;
						default: throw(new NotImplementedException());
					}
				}
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="PspAudio"></param>
			public Channel(PspAudio PspAudio)
			{
				this.PspAudio = PspAudio;
			}

			/// <summary>
			/// 
			/// </summary>
			public void Output(short* BufferPointer, int LeftVolume, int RightVolume, Action Callback)
			{
				var Buffer = new short[SampleCount * NumberOfChannels];
				for (int n = 0; n < Buffer.Length; n += 2)
				{
					//Console.WriteLine(BufferPointer[n + 0]);
					//Console.WriteLine(BufferPointer[n + 1]);
					Buffer[n + 0] = (short)(((int)BufferPointer[n + 0] * LeftVolume) / PspAudio.MaxVolume);
					Buffer[n + 1] = (short)(((int)BufferPointer[n + 1] * RightVolume) / PspAudio.MaxVolume);
				}
				PspAudio.PspAudioImpl.OutputStereo16_48000(Index, Buffer, Callback);
				//OutputStereo16_48000
				//Callback();
			}
		}

		/// <summary>
		/// The maximum output volume.
		/// </summary>
		public const int MaxVolume = 0x8000;

		/// <summary>
		/// Used to request the next available hardware channel.
		/// </summary>
		public const int FreeChannel = -1;


		public const int MaxChannels = 8;

		public Channel[] Channels;

		public IPspAudioImpl PspAudioImpl;

		public PspAudio(IPspAudioImpl PspAudioImpl)
		{
			this.PspAudioImpl = PspAudioImpl;
			Initialize();
		}

		protected void Initialize()
		{
			Channels = new Channel[MaxChannels];
			for (int n = 0; n < MaxChannels; n++)
			{
				Channels[n] = new Channel(this)
				{
					Index = n,
					Available = true,
				};
			}
		}

		public Channel GetFreeChannel()
		{
			return Channels.Where(Channel => Channel.Available).First();
		}

		protected void CheckChannelId(int ChannelId)
		{
			if (ChannelId < 0 || ChannelId >= Channels.Length)
			{
				throw(new InvalidOperationException());
			}
		}

		public Channel GetChannel(int ChannelId)
		{
			if (ChannelId == FreeChannel) return GetFreeChannel();
			CheckChannelId(ChannelId);
			return Channels[ChannelId];
		}
	}
}
