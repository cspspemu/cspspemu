using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Audio;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.sc_sascore
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceSasCore : HleModuleHost
	{
		static Logger Logger = Logger.GetLogger("sceSasCore");

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xBD11B7C2, FirmwareVersion = 150)]
		public int __sceSasGetGrain(uint SasCorePointer)
		{
			var SasCore = GetSasCore(SasCorePointer);
			return SasCore.GrainSamples;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <param name="Grain"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xD1E0A01E, FirmwareVersion = 150)]
		public int __sceSasSetGrain(uint SasCorePointer, int Grain)
		{
			var SasCore = GetSasCore(SasCorePointer);
			try
			{
				return 0;
				//return SasCore.GrainSamples;
			}
			finally
			{
				SasCore.GrainSamples = Grain;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE175EF66, FirmwareVersion = 150)]
		public OutputMode __sceSasGetOutputmode(uint SasCorePointer)
		{
			var SasCore = GetSasCore(SasCorePointer);

			//throw(new NotImplementedException());
			return SasCore.OutputMode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <param name="OutputMode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE855BF76, FirmwareVersion = 150)]
		public int __sceSasSetOutputmode(uint SasCorePointer, OutputMode OutputMode)
		{
			var SasCore = GetSasCore(SasCorePointer);

			//throw(new NotImplementedException());
			try
			{
				return 0;
				//return SasCore.OutputMode;
			}
			finally
			{
				SasCore.OutputMode = OutputMode;
			}
		}

		protected int[] VoiceOnCount;
		protected StereoIntSoundSample[] BufferTemp;
		protected StereoShortSoundSample[] BufferShort;
		protected StereoShortSoundSample[] MixBufferShort;


		/// <summary>
		/// Initialized a sasCore structure.
		/// </summary>
		/// <remarks>
		/// PSP can only handle one at a time.
		/// </remarks>
		/// <example>
		/// __sceSasInit(&sasCore, PSP_SAS_GRAIN_SAMPLES, PSP_SAS_VOICES_MAX, OutputMode.PSP_SAS_OUTPUTMODE_STEREO, 44100);
		/// </example>
		/// <param name="sasCore">Pointer to a SasCore structure that will contain information.</param>
		/// <param name="grainSamples">Number of grainSamples</param>
		/// <param name="maxVoices">Max number of voices</param>
		/// <param name="outputMode">Out Mode</param>
		/// <param name="sampleRate">Sample Rate</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x42778A9F, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public uint __sceSasInit(uint SasCorePointer, int GrainSamples, int MaxVoices, OutputMode OutputMode, int SampleRate)
		{
			if (SampleRate != 44100) throw (new NotImplementedException("(SampleRate != 44100)"));
			if (MaxVoices != 32) throw (new NotImplementedException("(MaxVoices != 32)"));
			//if (MaxVoices != 32) throw (new NotImplementedException("(MaxVoices != 32)"));

			var SasCore = GetSasCore(SasCorePointer, CreateIfNotExists: true);
			{
				SasCore.Initialized = true;
				SasCore.GrainSamples = GrainSamples;
				SasCore.MaxVoices = MaxVoices;
				SasCore.OutputMode = OutputMode;
				SasCore.SampleRate = SampleRate;
			}

			VoiceOnCount = new int[SasCore.GrainSamples];
			BufferTemp = new StereoIntSoundSample[SasCore.GrainSamples];
			BufferShort = new StereoShortSoundSample[SasCore.GrainSamples];
			MixBufferShort = new StereoShortSoundSample[SasCore.GrainSamples];

			return 0;
		}

		/// <summary>
		/// Return a bitfield indicating the end of the voices.
		/// </summary>
		/// <param name="SasCore">Core</param>
		/// <returns>A set of flags indiciating the end of the voices.</returns>
		[HlePspFunction(NID = 0x68A46B95, FirmwareVersion = 150)]
		public uint __sceSasGetEndFlag(uint SasCorePointer)
		{
			var SasCore = GetSasCore(SasCorePointer);
			return SasCore.EndFlags;
		}

		/// <summary>
		/// Sets the WaveformEffectType to the specified sasCore.
		/// </summary>
		/// <param name="SasCore">Core</param>
		/// <param name="WaveformEffectType">Effect</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x33D4AB37, FirmwareVersion = 150)]
		public uint __sceSasRevType(uint SasCorePointer, WaveformEffectType WaveformEffectType)
		{
			var SasCore = GetSasCore(SasCorePointer);
			SasCore.WaveformEffectType = WaveformEffectType;
			return 0;
		}

		/// <summary>
		/// Sets the waveformEffectIsDry and waveformEffectIsWet to the specified sasCore.
		/// </summary>
		/// <param name="SasCore">Core</param>
		/// <param name="WaveformEffectIsDry">waveformEffectIsDry</param>
		/// <param name="WaveformEffectIsWet">waveformEffectIsWet</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0xF983B186, FirmwareVersion = 150)]
		public uint __sceSasRevVON(uint SasCorePointer, bool WaveformEffectIsDry, bool WaveformEffectIsWet)
		{
			var SasCore = GetSasCore(SasCorePointer);
			SasCore.WaveformEffectIsDry = WaveformEffectIsDry;
			SasCore.WaveformEffectIsWet = WaveformEffectIsWet;
			return 0;
		}

		/// <summary>
		/// Sets the effect left and right volumes for the specified sasCore.
		/// </summary>
		/// <param name="SasCore">Core</param>
		/// <param name="LeftVolume">Left volume</param>
		/// <param name="RightVolume">Right volume</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xD5A229C9, FirmwareVersion = 150)]
		public uint __sceSasRevEVOL(uint SasCorePointer, int LeftVolume, int RightVolume)
		{
			var SasCore = GetSasCore(SasCorePointer);
			SasCore.LeftVolume = LeftVolume;
			SasCore.RightVolume = RightVolume;
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <param name="Delay"></param>
		/// <param name="Feedback"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x267A6DD2, FirmwareVersion = 150)]
		public int __sceSasRevParam(uint SasCorePointer, int Delay, int Feedback)
		{
			var SasCore = GetSasCore(SasCorePointer);

			SasCore.Delay = Delay;
			SasCore.Feedback = Feedback;

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Pauses a set of voice channels for that sasCore.
		/// </summary>
		/// <param name="sasCore">SasCore</param>
		/// <param name="voice_bits">Voice Bit Set</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x787D04D5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetPause(uint SasCorePointer, uint voice_bits)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x2C8E6AB3, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int __sceSasGetPauseFlag(uint SasCorePointer)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Process the voices and generate the next samples.
		/// Mix the resulting samples in an exiting buffer.
		/// </summary>
		/// <param name="SasCore">sasCore handle</param>
		/// <param name="SasInOut">
		///		address for the input and output buffer.
		///		Samples are stored as 2 16-bit values
		///		(left then right channel samples)
		/// </param>
		/// <param name="LeftVolume">Left channel volume, [0..0x1000].</param>
		/// <param name="RightVolume">Right channel volume, [0..0x1000].</param>
		/// <returns>
		///		if OK 0
		///		ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		/// </returns>
		[HlePspFunction(NID = 0x50A14DFC, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int __sceSasCoreWithMix(uint SasCorePointer, StereoShortSoundSample* SasInOut, int LeftVolume, int RightVolume)
		{
#if false
			var SasCore = GetSasCore(SasCorePointer);
			int NumberOfChannels = SasCore.OutputMode == OutputMode.PSP_SAS_OUTPUTMODE_STEREO ? 2 : 1;
			int NumberOfSamples = SasCore.GrainSamples * NumberOfChannels;

			fixed (short* FixedMixBufferShort = MixBufferShort)
			{
				__sceSasCore(SasCorePointer, FixedMixBufferShort);
			}

			int MaxVolume = 0x1000;

			int LeftVolumeComp = MaxVolume - LeftVolume;
			int RightVolumeComp = MaxVolume - RightVolume;

			for (int n = 0; n < NumberOfSamples; n += 2)
			{
				SasInOut[n + 0] = (short)(((int)SasInOut[n + 0] * LeftVolumeComp + (int)MixBufferShort[n + 0] * LeftVolume) * short.MaxValue / MaxVolume);
				SasInOut[n + 1] = (short)(((int)SasInOut[n + 1] * RightVolumeComp + (int)MixBufferShort[n + 1] * RightVolume) * short.MaxValue / MaxVolume);
			}

			//throw (new NotImplementedException());
			return 0;
#else
			return __sceSasCore(SasCorePointer, SasInOut);
#endif
		}

		/// <summary>
		/// Process the voices and generate the next samples.
		/// </summary>
		/// <param name="SasCore">sasCore handle</param>
		/// <param name="SasOut">
		///		address for the output buffer.
		///		Samples are stored as 2 16-bit values
		///		(left then right channel samples)
		/// </param>
		/// <returns>
		///		if OK 0
		///		ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		/// </returns>
		[HlePspFunction(NID = 0xA3589D81, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int __sceSasCore(uint SasCorePointer, StereoShortSoundSample* SasOut)
		{
			var SasCore = GetSasCore(SasCorePointer);

			if (SasCore.OutputMode != OutputMode.PSP_SAS_OUTPUTMODE_STEREO)
			{
				Logger.Unimplemented("SasCore.OutputMode != OutputMode.PSP_SAS_OUTPUTMODE_STEREO");
			}

			int NumberOfChannels = SasCore.OutputMode == OutputMode.PSP_SAS_OUTPUTMODE_STEREO ? 2 : 1;
			int NumberOfSamples = SasCore.GrainSamples;

			for (int n = 0; n < NumberOfSamples; n++)
			{
				BufferTemp[n] = default(StereoIntSoundSample);
				VoiceOnCount[n] = 0;
			}

			// Read and mix voices.
			foreach (var Voice in SasCore.Voices)
			{
				if (Voice.OnAndPlaying)
				{
					for (int n = 0; n < NumberOfSamples; n++)
					{
						if (Voice.SampleOffset < Voice.Vag.SamplesCount)
						{
							VoiceOnCount[n]++;
							BufferTemp[n] += Voice.Vag.GetSampleAt(Voice.SampleOffset++);
						}
						else
						{
							Voice.SetPlaying(false);
							break;
						}
					}
				}
			}

			// Normalize output
			for (int n = 0; n < NumberOfSamples; n++)
			{
				if (VoiceOnCount[n] > 0)
				{
					BufferShort[n] = (BufferTemp[n] / VoiceOnCount[n]);
				}
				else
				{
					BufferShort[n] = default(StereoShortSoundSample);
				}
			}

			// Output converted 44100 data
			for (int n = 0; n < NumberOfSamples; n++)
			{
				SasOut[n] = BufferShort[n];
			}
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Get the current envelope height for all the voices.
		/// </summary>
		/// <param name="sasCore">sasCore handle</param>
		/// <param name="heightsAddr">
		/// (int *) address where to return the envelope heights,
		/// stored as 32 bit values [0..0x40000000].
		///     heightsAddr[0] = envelope height of voice 0
		///     heightsAddr[1] = envelope height of voice 1
		///     ...
		/// </param>
		/// <returns>
		/// 0 if OK
		/// ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		/// </returns>
		[HlePspFunction(NID = 0x07F58C24, FirmwareVersion = 150)]
		public int __sceSasGetAllEnvelopeHeights(uint SasCorePointer, int* Heights)
		{
			var SasCore = GetSasCore(SasCorePointer);
			foreach (var Voice in SasCore.Voices)
			{
				Voice.EnvelopeHeight = *Heights++;
			}
			return 0;
		}
	}
}
