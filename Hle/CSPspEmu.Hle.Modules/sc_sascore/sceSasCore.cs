using System;
using System.Linq;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Audio;
using CSharpUtils;

namespace CSPspEmu.Hle.Modules.sc_sascore
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceSasCore : HleModuleHost
	{
		static Logger Logger = Logger.GetLogger("sceSasCore");

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xBD11B7C2, FirmwareVersion = 150)]
		public int __sceSasGetGrain(uint SasCorePointer)
		{
			var SasCore = GetSasCore(SasCorePointer);
			return SasCore.GrainSamples;
		}

		static private void CheckGrains(int GrainSamples)
		{
			if (GrainSamples < 0x40 || GrainSamples > 0x800 || (GrainSamples & 0x1F) != 0)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_SAS_INVALID_GRAIN));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="GrainSamples"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xD1E0A01E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetGrain(uint SasCorePointer, int GrainSamples)
		{
			var SasCore = GetSasCore(SasCorePointer);

			CheckGrains(GrainSamples);

			SasCore.GrainSamples = GrainSamples;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE175EF66, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public OutputMode __sceSasGetOutputmode(uint SasCorePointer)
		{
			var SasCore = GetSasCore(SasCorePointer);

			//throw(new NotImplementedException());
			return SasCore.OutputMode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="OutputMode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE855BF76, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetOutputmode(uint SasCorePointer, OutputMode OutputMode)
		{
			var SasCore = GetSasCore(SasCorePointer);

			SasCore.OutputMode = OutputMode;
			return 0;
		}

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
		/// <param name="SasCorePointer">Pointer to a <see cref="SasCore"/> structure that will contain information.</param>
		/// <param name="GrainSamples">Number of grainSamples</param>
		/// <param name="MaxVoices">Max number of voices</param>
		/// <param name="OutputMode">Out Mode</param>
		/// <param name="SampleRate">Sample Rate</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x42778A9F, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public uint __sceSasInit(uint SasCorePointer, int GrainSamples, int MaxVoices, OutputMode OutputMode, int SampleRate)
		{
			if (SampleRate != 44100)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_SAS_INVALID_SAMPLE_RATE));
			}

			CheckGrains(GrainSamples);

			if (MaxVoices < 1 || MaxVoices > PSP_SAS_VOICES_MAX)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_SAS_INVALID_MAX_VOICES));
			}

			if (OutputMode != sc_sascore.OutputMode.PSP_SAS_OUTPUTMODE_STEREO && OutputMode != sc_sascore.OutputMode.PSP_SAS_OUTPUTMODE_MULTICHANNEL)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_SAS_INVALID_OUTPUT_MODE));
			}

			var SasCore = GetSasCore(SasCorePointer, CreateIfNotExists: true);
			{
				SasCore.Initialized = true;
				SasCore.GrainSamples = GrainSamples;
				SasCore.MaxVoices = MaxVoices;
				SasCore.OutputMode = OutputMode;
				SasCore.SampleRate = SampleRate;
			}

			BufferTemp = new StereoIntSoundSample[SasCore.GrainSamples * 2];
			BufferShort = new StereoShortSoundSample[SasCore.GrainSamples * 2];
			MixBufferShort = new StereoShortSoundSample[SasCore.GrainSamples * 2];

			return 0;
		}

		/// <summary>
		/// Return a bitfield indicating the end of the voices.
		/// </summary>
		/// <param name="SasCorePointer">Core</param>
		/// <returns>A set of flags indiciating the end of the voices.</returns>
		[HlePspFunction(NID = 0x68A46B95, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public uint __sceSasGetEndFlag(uint SasCorePointer)
		{
			var SasCore = GetSasCore(SasCorePointer);
			return SasCore.EndFlags;
		}

		/// <summary>
		/// Sets the <see cref="WaveformEffectType"/> to the specified <see cref="SasCore"/>.
		/// </summary>
		/// <param name="SasCorePointer">Core</param>
		/// <param name="WaveformEffectType">Effect</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x33D4AB37, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public uint __sceSasRevType(uint SasCorePointer, WaveformEffectType WaveformEffectType)
		{
			var SasCore = GetSasCore(SasCorePointer);
			SasCore.WaveformEffectType = WaveformEffectType;
			return 0;
		}

		/// <summary>
		/// Sets the WaveformEffectIsDry and WaveformEffectIsWet to the specified SasCore.
		/// </summary>
		/// <param name="SasCorePointer">SasCore</param>
		/// <param name="WaveformEffectIsDry">WaveformEffectIsDry</param>
		/// <param name="WaveformEffectIsWet">WaveformEffectIsWet</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0xF983B186, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public uint __sceSasRevVON(uint SasCorePointer, bool WaveformEffectIsDry, bool WaveformEffectIsWet)
		{
			var SasCore = GetSasCore(SasCorePointer);
			SasCore.WaveformEffectIsDry = WaveformEffectIsDry;
			SasCore.WaveformEffectIsWet = WaveformEffectIsWet;
			return 0;
		}

		/// <summary>
		/// Sets the effect left and right volumes for the specified SasCore.
		/// </summary>
		/// <param name="SasCorePointer">SasCore</param>
		/// <param name="LeftVolume">Left volume</param>
		/// <param name="RightVolume">Right volume</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xD5A229C9, FirmwareVersion = 150)]
		[HlePspNotImplemented]
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
		/// <param name="SasCorePointer"></param>
		/// <param name="Delay"></param>
		/// <param name="Feedback"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x267A6DD2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasRevParam(uint SasCorePointer, int Delay, int Feedback)
		{
			var SasCore = GetSasCore(SasCorePointer);

			SasCore.Delay = Delay;
			SasCore.Feedback = Feedback;

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Set or reset the pause parameter for the voices.
		/// </summary>
		/// <param name="SasCorePointer">SasCore</param>
		/// <param name="VoiceBits">a bitfield with bit 0 for voice 0, bit 1 for voice 1... Only the bits with 1 are processed.</param>
		/// <param name="SetPause"> when 0: reset the pause flag for all the voices having a bit 1 in the voice_bit field  when non-0: set the pause flag for all the voices having a bit 1 in the voice_bit field</param>
		/// <returns>0 on success. ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided</returns>
		[HlePspFunction(NID = 0x787D04D5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetPause(uint SasCorePointer, uint VoiceBits, bool SetPause)
		{
			var SasCore = GetSasCore(SasCorePointer);
			uint Out = 0;
			foreach (var Voice in SasCore.Voices)
			{
				if ((VoiceBits & (1 << Voice.Index)) != 0)
				{
					Voice.Paused = SetPause;
				}
			}
			return 0;
		}

		/// <summary>
		/// Get the pause flag for all the voices.
		/// </summary>
		/// <param name="SasCorePointer">sasCore handle</param>
		/// <returns>
		/// bitfield with bit 0 for voice 0, bit 1 for voice 1...
		/// bit=0, corresponding voice is not paused
		/// bit=1, corresponding voice is paused
		/// ERROR_SAS_NOT_INIT if an invalid sasCore handle is provided
		/// </returns>
		[HlePspFunction(NID = 0x2C8E6AB3, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int __sceSasGetPauseFlag(uint SasCorePointer)
		{
			//return unchecked((int)0xFFFFFFFF);
			var SasCore = GetSasCore(SasCorePointer);
			uint Out = 0;
			foreach (var Voice in SasCore.Voices)
			{
				if (Voice.Paused) Out |= (uint)(1 << Voice.Index);
			}
			return (int)Out;
		}

		/// <summary>
		/// Process the voices and generate the next samples.
		/// Mix the resulting samples in an exiting buffer.
		/// </summary>
		/// <param name="SasCorePointer">SasCore handle</param>
		/// <param name="SasInOut">
		///		address for the input and output buffer.
		///		Samples are stored as 2 16-bit values
		///		(left then right channel samples)
		/// </param>
		/// <param name="LeftVolume">Left channel volume, [0..0x1000].</param>
		/// <param name="RightVolume">Right channel volume, [0..0x1000].</param>
		/// <returns>
		///		if OK 0
		///		ERROR_SAS_NOT_INIT if an invalid SasCore handle is provided
		/// </returns>
		[HlePspFunction(NID = 0x50A14DFC, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int __sceSasCoreWithMix(uint SasCorePointer, short* SasInOut, int LeftVolume, int RightVolume)
		{
			return __sceSasCore_Internal(GetSasCore(SasCorePointer), SasInOut, SasInOut, LeftVolume, RightVolume);
		}


		/// <summary>
		/// Process the voices and generate the next samples.
		/// </summary>
		/// <param name="SasCorePointer">SasCore handle</param>
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
		public int __sceSasCore(uint SasCorePointer, short* SasOut)
		{
			return __sceSasCore_Internal(GetSasCore(SasCorePointer), SasOut, null, 0x1000, 0x1000);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <param name="SasOut"></param>
		/// <param name="SasIn"></param>
		/// <param name="LeftVolume"></param>
		/// <param name="RightVolume"></param>
		/// <returns></returns>
		public int __sceSasCore_Internal(SasCore SasCore, short* SasOut, short* SasIn, int LeftVolume, int RightVolume)
		{
			fixed (StereoShortSoundSample* BufferShortPtr = BufferShort)
			fixed (StereoIntSoundSample* BufferTempPtr = BufferTemp)
			{
				if (SasCore.OutputMode != OutputMode.PSP_SAS_OUTPUTMODE_STEREO)
				{
					Logger.Unimplemented("SasCore.OutputMode != OutputMode.PSP_SAS_OUTPUTMODE_STEREO");
				}

				int NumberOfChannels = (SasCore.OutputMode == OutputMode.PSP_SAS_OUTPUTMODE_STEREO) ? 2 : 1;
				int NumberOfSamples = SasCore.GrainSamples;
				int NumberOfVoicesPlaying = Math.Max(1, SasCore.Voices.Count(Voice => Voice.OnAndPlaying));

				for (int n = 0; n < NumberOfSamples; n++) BufferTempPtr[n] = default(StereoIntSoundSample);

				int PrevPosDiv = -1;
				foreach (var Voice in SasCore.Voices)
				{
					if (Voice.OnAndPlaying)
					{
						//Console.WriteLine("Voice.Pitch: {0}", Voice.Pitch);
						//for (int n = 0, Pos = 0; n < NumberOfSamples; n++, Pos += Voice.Pitch)
						int Pos = 0;
						while (true)
						{
							if ((Voice.Vag != null) && (Voice.Vag.HasMore))
							{
								int PosDiv = Pos / Voice.Pitch;

								if (PosDiv >= NumberOfSamples) break;

								var Sample = Voice.Vag.GetNextSample().ApplyVolumes(Voice.LeftVolume, Voice.RightVolume);

								for (int m = PrevPosDiv + 1; m <= PosDiv; m++) BufferTempPtr[m] += Sample;
								
								PrevPosDiv = PosDiv;
								Pos += PSP_SAS_PITCH_BASE;
							}
							else
							{
								Voice.SetPlaying(false);
								break;
							}
						}
					}
				}

				for (int n = 0; n < NumberOfSamples; n++) BufferShortPtr[n] = BufferTempPtr[n];

				for (int channel = 0; channel < NumberOfChannels; channel++)
				{
					for (int n = 0; n < NumberOfSamples; n++)
					{
						SasOut[n * NumberOfChannels + channel] = BufferShortPtr[n].ApplyVolumes(LeftVolume, RightVolume).GetByIndex(channel);
					}
				}
			}

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Get the current envelope height for all the voices.
		/// </summary>
		/// <param name="SasCorePointer">SasCore handle</param>
		/// <param name="Heights">
		/// (int *) address where to return the envelope heights,
		/// stored as 32 bit values [0..0x40000000].
		///     heightsAddr[0] = envelope height of voice 0
		///     heightsAddr[1] = envelope height of voice 1
		///     ...
		/// </param>
		/// <returns>
		/// 0 if OK
		/// ERROR_SAS_NOT_INIT if an invalid SasCore handle is provided
		/// </returns>
		[HlePspFunction(NID = 0x07F58C24, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasGetAllEnvelopeHeights(uint SasCorePointer, int* Heights)
		{
			var SasCore = GetSasCore(SasCorePointer);
			foreach (var Voice in SasCore.Voices)
			{
				Voice.EnvelopeHeight = *Heights++;
			}
			return 0;
		}

		[HlePspFunction(NID = 0xE1CD9561, FirmwareVersion = 500)]
		[HlePspNotImplemented]
		public int __sceSasSetVoicePCM()
		{
			return 0;
		}

		[HlePspFunction(NID = 0xD5EBBBCD, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetSteepWave()
		{
			return 0;
		}

		[HlePspFunction(NID = 0xA232CBE6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetTriangularWave()
		{
			return 0;
		}
	}
}
