using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.sc_sascore
{
	unsafe public partial class sceSasCore : HleModuleHost
	{
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
		[HlePspNotImplemented]
		public int __sceSasGetPauseFlag(uint SasCorePointer)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <param name="SasInOut"></param>
		/// <param name="LeftVolume"></param>
		/// <param name="RightVolume"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x50A14DFC, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasCoreWithMix(uint SasCorePointer, void* SasInOut, int LeftVolume, int RightVolume)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCore"></param>
		/// <param name="SasOut"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA3589D81, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public uint __sceSasCore(uint SasCorePointer, short* SasOut)
		{
			int NumberOfChannels = 2;
			var SasCore = GetSasCore(SasCorePointer);
			var VoiceOnCount = new int[SasCore.GrainSamples * NumberOfChannels];
			var BufferTemp = new int[SasCore.GrainSamples * NumberOfChannels];
			var BufferShort = new short[SasCore.GrainSamples * NumberOfChannels];

			// Read and mix voices.
			foreach (var Voice in SasCore.Voices)
			{
				if (Voice.OnAndPlaying)
				{
					for (int n = 0; n < BufferTemp.Length; n++)
					{
						if (Voice.SampleOffset < Voice.Vag.DecodedSamples.Length)
						{
							VoiceOnCount[n]++;
							BufferTemp[n] += Voice.Vag.DecodedSamples[Voice.SampleOffset++];
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
			for (int n = 0; n < BufferTemp.Length; n++)
			{
				if (VoiceOnCount[n] > 0)
				{
					BufferShort[n] = (short)(BufferTemp[n] / VoiceOnCount[n]);
				}
				else
				{
					BufferShort[n] = 0;
				}
			}

			// Output converted 44100 data
			for (int n = 0; n < BufferShort.Length; n++)
			{
				SasOut[n] = BufferShort[n];
			}
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
