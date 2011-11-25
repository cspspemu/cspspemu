using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;

namespace CSPspEmu.Hle.Modules.audio
{
	unsafe public class sceAudio : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		public struct pspAudioInputParams
		{
			/// <summary>
			/// Unknown. Pass 0
			/// </summary>
			public int unknown1;

			/// <summary>
			/// Gain
			/// </summary>
			public int gain;
			
			/// <summary>
			/// Unknown. Pass 0
			/// </summary>
			public int unknown2;

			/// <summary>
			/// Unknown. Pass 0
			/// </summary>
			public int unknown3;
			
			/// <summary>
			/// Unknown. Pass 0
			/// </summary>
			public int unknown4;
			
			/// <summary>
			/// Unknown. Pass 0
			/// </summary>
			public int unknown5;
		}

		/// <summary>
		/// The minimum number of samples that can be allocated to a channel.
		/// </summary>
		public const int PSP_AUDIO_SAMPLE_MIN = 64;

		/// <summary>
		/// The maximum number of samples that can be allocated to a channel.
		/// </summary>
		public const int PSP_AUDIO_SAMPLE_MAX = 65472;

		/// <summary>
		/// Make the given sample count a multiple of 64.
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		//public Type PSP_AUDIO_SAMPLE_ALIGN(Type)(Type s) { return (s + 63) & ~63; }

		/// <summary>
		/// Reserve the audio output and set the output sample count
		/// </summary>
		/// <param name="SamplesCount">The number of samples to output in one output call (min 17, max 4111).</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x01562BA3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceAudioOutput2Reserve(int SamplesCount)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Output audio (blocking)
		/// </summary>
		/// <param name="Volume">The volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
		/// <param name="BufferPointer">Pointer to the PCM data.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x2D53F36E, FirmwareVersion = 150)]
		public int sceAudioOutput2OutputBlocking(int Volume, void* BufferPointer)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Release the audio output
		/// </summary>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x43196845, FirmwareVersion = 150)]
		public int sceAudioOutput2Release()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get count of unplayed samples remaining
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <returns>Number of samples to be played, an error if less than 0.</returns>
		[HlePspFunction(NID = 0xB011922F, FirmwareVersion = 150)]
		public int sceAudioGetChannelRestLength(int Channel)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Output panned audio of the specified channel (blocking)
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <param name="LeftVolume">The left volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
		/// <param name="RightVolume">The right volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
		/// <param name="Buffer">Pointer to the PCM data to output.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x13F592BC, FirmwareVersion = 150)]
		public int sceAudioOutputPannedBlocking(int Channel, int LeftVolume, int RightVolume, short* Buffer)
		{
#if false
			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Timer, "sceAudioOutputPannedBlocking", (WakeUpCallbackDelegate) =>
			{
				HleState.PspRtc.RegisterTimerInOnce(TimeSpan.FromMilliseconds(1), () =>
				{
					WakeUpCallbackDelegate();
				});
			});

#else
			HleState.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Audio, "sceAudioOutputPannedBlocking", (WakeUpCallbackDelegate) =>
			{
				HleState.PspAudio.GetChannel(Channel).Output(Buffer, LeftVolume, RightVolume, () =>
				{
					WakeUpCallbackDelegate();
				});
			});
#endif
			return 0;
		}


		/// <summary>
		/// Output audio of the specified channel (blocking)
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <param name="Volume">The volume.</param>
		/// <param name="Buffer">Pointer to the PCM data to output.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x136CAF51, FirmwareVersion = 150)]
		public int sceAudioOutputBlocking(int Channel, int Volume, short* Buffer)
		{
			return sceAudioOutputPannedBlocking(Channel, Volume, Volume, Buffer);
		}

		/// <summary>
		/// Output panned audio of the specified channel
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <param name="LeftVolume">The left volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
		/// <param name="RightVolume">The right volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
		/// <param name="Buffer">Pointer to the PCM data to output.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0xE2D56B2D, FirmwareVersion = 150)]
		public int sceAudioOutputPanned(int Channel, int LeftVolume, int RightVolume, short* Buffer)
		{
			HleState.PspAudio.GetChannel(Channel).Output(Buffer, LeftVolume, RightVolume, () =>
			{
			});
			return 0;
		}

		/// <summary>
		/// Output audio of the specified channel
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <param name="Volume">The volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
		/// <param name="Buffer">Pointer to the PCM data to output.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x8C1009B2, FirmwareVersion = 150)]
		public int sceAudioOutput(int Channel, int Volume, short* Buffer)
		{
			return sceAudioOutputPanned(Channel, Volume, Volume, Buffer);
		}

		/// <summary>
		/// Get count of unplayed samples remaining
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <returns>Number of samples to be played, an error if less than 0.</returns>
		[HlePspFunction(NID = 0xE9D97901, FirmwareVersion = 150)]
		public int sceAudioGetChannelRestLen(int Channel)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Change the output sample count, after it's already been reserved
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <param name="SampleCount">The number of samples to output in one output call.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0xCB2E439E, FirmwareVersion = 150)]
		public int sceAudioSetChannelDataLen(int Channel, int SampleCount)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Change the format of a channel
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <param name="Format">One of ::PspAudioFormats</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x95FD0C2D, FirmwareVersion = 150)]
		public int sceAudioChangeChannelConfig(int Channel, PspAudio.FormatEnum Format)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Allocate and initialize a hardware output channel.
		/// </summary>
		/// <param name="ChannelId">
		///		Use a value between 0 - 7 to reserve a specific channel.
		///		Pass PSP_AUDIO_NEXT_CHANNEL to get the first available channel.
		/// </param>
		/// <param name="SampleCount">
		///		The number of samples that can be output on the channel per
		///		output call.  It must be a value between ::PSP_AUDIO_SAMPLE_MIN
		///		and ::PSP_AUDIO_SAMPLE_MAX, and it must be aligned to 64 bytes
		///		(use the ::PSP_AUDIO_SAMPLE_ALIGN macro to align it).
		/// </param>
		/// <param name="Format">The output format to use for the channel.  One of ::PspAudioFormats.</param>
		/// <returns>The channel number on success, an error code if less than 0.</returns>
		[HlePspFunction(NID = 0x5EC81C55, FirmwareVersion = 150)]
		public int sceAudioChReserve(int ChannelId, int SampleCount, PspAudio.FormatEnum Format)
		{
			var Channel = HleState.PspAudio.GetChannel(ChannelId);
			Channel.SampleCount = SampleCount;
			Channel.Format = Format;
			return Channel.Index;
		}

		/// <summary>
		/// Release a hardware output channel.
		/// </summary>
		/// <param name="Channel">The channel to release.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x6FC46853, FirmwareVersion = 150)]
		public int sceAudioChRelease(int Channel)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Perform audio input (blocking)
		/// </summary>
		/// <param name="SampleCount">Number of samples.</param>
		/// <param name="Frequency">Either 44100, 22050 or 11025.</param>
		/// <param name="Buffer">Pointer to where the audio data will be stored.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x086E5895, FirmwareVersion = 150)]
		public int sceAudioInputBlocking(int SampleCount, int Frequency, void* Buffer)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Init audio input
		/// </summary>
		/// <param name="Unknown1">Unknown. Pass 0.</param>
		/// <param name="Gain">Gain</param>
		/// <param name="Unknown2">Unknown. Pass 0.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x7DE61688, FirmwareVersion = 150)]
		public int sceAudioInputInit(int Unknown1, int Gain, int Unknown2)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Change the volume of a channel
		/// </summary>
		/// <param name="Channel">The channel number.</param>
		/// <param name="LeftVolume">The left volume.</param>
		/// <param name="RightVolume">The right volume.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0xB7E1D8E7, FirmwareVersion = 150)]
		public int sceAudioChangeChannelVolume(int Channel, int LeftVolume, int RightVolume)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Reserve the audio output
		/// </summary>
		/// <param name="SampleCount">The number of samples to output in one output call (min 17, max 4111).</param>
		/// <param name="Frequency">The frequency. One of 48000, 44100, 32000, 24000, 22050, 16000, 12000, 11050, 8000.</param>
		/// <param name="Channels">Number of channels. Pass 2 (stereo).</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x38553111, FirmwareVersion = 150)]
		public int sceAudioSRCChReserve(int SampleCount, int Frequency, int Channels)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Release the audio output
		/// </summary>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0x5C37C0AE, FirmwareVersion = 150)]
		public int sceAudioSRCChRelease()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Output audio
		/// </summary>
		/// <param name="Volume">The volume.</param>
		/// <param name="Buffer">Pointer to the PCM data to output.</param>
		/// <returns>0 on success, an error if less than 0.</returns>
		[HlePspFunction(NID = 0xE0727056, FirmwareVersion = 150)]
		public int sceAudioSRCOutputBlocking(int Volume, void* Buffer)
		{
			throw(new NotImplementedException());
		}

	}
}
