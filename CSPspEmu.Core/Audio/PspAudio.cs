using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Audio
{
	unsafe public class PspAudio : PspEmulatorComponent
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

		/// <summary>
		/// The maximum output volume.
		/// </summary>
		public const int MaxVolume = 0x8000;

		/// <summary>
		/// Used to request the next available hardware channel.
		/// </summary>
		public const int FreeChannel = -1;

		/// <summary>
		/// 
		/// </summary>
		//public const int MaxChannels = 8;
		public const int MaxChannels = 32;

		/// <summary>
		/// 
		/// </summary>
		public PspAudioChannel[] Channels;

		/// <summary>
		/// 
		/// </summary>
		public PspAudioImpl PspAudioImpl;

		/// <summary>
		/// 
		/// </summary>
		public override void InitializeComponent()
		{
			this.PspAudioImpl = PspEmulatorContext.GetInstance<PspAudioImpl>();
			Initialize();
		}

		/// <summary>
		/// 
		/// </summary>
		protected void Initialize()
		{
			Channels = new PspAudioChannel[MaxChannels];
			for (int n = 0; n < MaxChannels; n++)
			{
				Channels[n] = new PspAudioChannel(this)
				{
					Index = n,
					Available = true,
				};
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public PspAudioChannel GetFreeChannel()
		{
			return Channels.Where(Channel => Channel.Available).First();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChannelId"></param>
		protected void CheckChannelId(int ChannelId)
		{
			if (ChannelId < 0 || ChannelId >= Channels.Length)
			{
				throw(new InvalidOperationException());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChannelId"></param>
		/// <returns></returns>
		public PspAudioChannel GetChannel(int ChannelId, bool CanAlloc = false)
		{
			PspAudioChannel Channel;
			if (CanAlloc && ChannelId == FreeChannel)
			{
				Channel = GetFreeChannel();
			}
			else
			{
				CheckChannelId(ChannelId);
				Channel = Channels[ChannelId];
			}
			Channel.Available = false;
			return Channel;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Update()
		{
			PspAudioImpl.Update((RequiredSamples) =>
			{
				short[] MixedSamples = new short[RequiredSamples];
				int[] MixedSamplesDenormalized = new int[RequiredSamples];
				int[] NumberOfChannels = new int[RequiredSamples];
				foreach (var Channel in Channels)
				{
					var ChannelSamples = Channel.Read(RequiredSamples);

					for (int n = 0; n < ChannelSamples.Length; n++)
					{
						MixedSamplesDenormalized[n] += ChannelSamples[n];
						NumberOfChannels[n]++;
					}
				}

				for (int n = 0; n < RequiredSamples; n++)
				{
					if (NumberOfChannels[n] != 0)
					{
						MixedSamples[n] = (short)(MixedSamplesDenormalized[n] / NumberOfChannels[n]);
					}
				}

				return MixedSamples;
			});
		}
	}
}
