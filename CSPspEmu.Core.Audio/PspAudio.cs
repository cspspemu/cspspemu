using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Core.Audio
{
    unsafe public sealed class PspAudio : IInjectInitialize
	{
		/// <summary>
		/// Output formats for PSP audio.
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
		/// 
		/// </summary>
		public const int SamplesMax = 0x10000 - 64;

		/// <summary>
		/// Used to request the next available hardware channel.
		/// </summary>
		//public const int FreeChannel = -1;

		/// <summary>
		/// Maximum number of allowed audio channels
		/// </summary>
		public const int MaxChannels = 8;
		//public const int MaxChannels = 32;

		/// <summary>
		/// Number of audio channels
		/// </summary>
		public PspAudioChannel[] Channels;

		/// <summary>
		/// 
		/// </summary>
		public PspAudioChannel SrcOutput2Channel;

		/// <summary>
		/// 
		/// </summary>
		[Inject]
		public PspAudioImpl PspAudioImpl;

		/// <summary>
		/// 
		/// </summary>
		private PspAudio()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		void IInjectInitialize.Initialize()
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

			SrcOutput2Channel = new PspAudioChannel(this)
			{
				Index = MaxChannels,
				Available = true,
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public PspAudioChannel GetFreeChannel()
		{
			if (!Channels.Any(Channel => Channel.Available)) throw(new NoChannelsAvailableException());
			return Channels.Reverse().First(Channel => Channel.Available);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChannelId"></param>
		protected void CheckChannelId(int ChannelId)
		{
			if (ChannelId < 0 || ChannelId >= Channels.Length)
			{
				throw(new InvalidChannelException());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChannelId"></param>
		/// <param name="CanAlloc"></param>
		/// <returns></returns>
		public PspAudioChannel GetChannel(int ChannelId, bool CanAlloc = false)
		{
			PspAudioChannel Channel;
			if (CanAlloc && ChannelId < 0)
			{
				Channel = GetFreeChannel();
			}
			else
			{
				CheckChannelId(ChannelId);
				Channel = Channels[ChannelId];
			}
			return Channel;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Update()
		{
			PspAudioImpl.Update((MixedSamples) =>
			{
				var RequiredSamples = MixedSamples.Length;
				fixed (short* MixedSamplesPtr = MixedSamples)
				{
					var MixedSamplesDenormalized = stackalloc int[RequiredSamples];
					var NumberOfChannels = stackalloc int[RequiredSamples];
					foreach (var Channel in Channels)
					{
						var ChannelSamples = Channel.Read(RequiredSamples);

						fixed (short* ChannelSamplesPtr = ChannelSamples)
						{
							for (int n = 0; n < ChannelSamples.Length; n++)
							{
								MixedSamplesDenormalized[n] += ChannelSamplesPtr[n];
								NumberOfChannels[n]++;
							}
						}
					}

					for (int n = 0; n < RequiredSamples; n++)
					{
						if (NumberOfChannels[n] != 0)
						{
							MixedSamplesPtr[n] = (short)(MixedSamplesDenormalized[n] / NumberOfChannels[n]);
						}
					}
				}
			});
		}
	}
	public class InvalidChannelException : Exception
	{
	}
	public class NoChannelsAvailableException : Exception
	{
	}
}
