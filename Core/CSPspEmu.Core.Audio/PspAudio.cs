using System;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Core.Audio
{
    unsafe public sealed class PspAudio : IInjectInitialize, IDisposable
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
			if (!Channels.Any(channel => channel.Available)) throw(new NoChannelsAvailableException());
			return Channels.Reverse().First(channel => channel.Available);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="channelId"></param>
		private void CheckChannelId(int channelId)
		{
			if (channelId < 0 || channelId >= Channels.Length)
			{
				throw(new InvalidChannelException());
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="channelId"></param>
		/// <param name="canAlloc"></param>
		/// <returns></returns>
		public PspAudioChannel GetChannel(int channelId, bool canAlloc = false)
		{
			PspAudioChannel channel;
			if (canAlloc && channelId < 0)
			{
				channel = GetFreeChannel();
			}
			else
			{
				CheckChannelId(channelId);
				channel = Channels[channelId];
			}
			return channel;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Update()
		{
			PspAudioImpl.Update((mixedSamples) =>
			{
				var requiredSamples = mixedSamples.Length;
				fixed (short* mixedSamplesPtr = mixedSamples)
				{
					var mixedSamplesDenormalized = stackalloc int[requiredSamples];

					foreach (var channel in Channels)
					{
						var channelSamples = channel.Read(requiredSamples);

						fixed (short* channelSamplesPtr = channelSamples)
						{
							for (var n = 0; n < channelSamples.Length; n++)
							{
								mixedSamplesDenormalized[n] += channelSamplesPtr[n];
							}
						}
					}

					for (int n = 0; n < requiredSamples; n++)
					{
						mixedSamplesPtr[n] = StereoShortSoundSample.Clamp(mixedSamplesDenormalized[n]);
					}
				}
			});
		}

		private bool _disposed = false;

		public void StopSynchronized()
		{
			if (_disposed) return;
			_disposed = true;
			PspAudioImpl.StopSynchronized();
		}

		void IDisposable.Dispose()
		{
			StopSynchronized();
		}
	}
	public class InvalidChannelException : Exception
	{
	}
	public class NoChannelsAvailableException : Exception
	{
	}
}
