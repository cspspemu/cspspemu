using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.sc_sascore
{
	public struct SasCore
	{
	}

	unsafe public class sceSasCore : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xBD11B7C2, FirmwareVersion = 150)]
		public int __sceSasGetGrain(SasCore* sasCore)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="grain"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xD1E0A01E, FirmwareVersion = 150)]
		public int __sceSasSetGrain(SasCore* sasCore, int grain)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE175EF66, FirmwareVersion = 150)]
		public OutputMode __sceSasGetOutputmode(SasCore* sasCore)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="outputMode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE855BF76, FirmwareVersion = 150)]
		public int __sceSasSetOutputmode(SasCore* sasCore, OutputMode outputMode)
		{
			throw(new NotImplementedException());
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
		public uint __sceSasInit(SasCore* sasCore, int grainSamples, int maxVoices, OutputMode outputMode, int sampleRate)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Return a bitfield indicating the end of the voices.
		/// </summary>
		/// <param name="sasCore">Core</param>
		/// <returns>A set of flags indiciating the end of the voices.</returns>
		[HlePspFunction(NID = 0x68A46B95, FirmwareVersion = 150)]
		public uint __sceSasGetEndFlag(SasCore* sasCore)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Sets the WaveformEffectType to the specified sasCore.
		/// </summary>
		/// <param name="sasCore">Core</param>
		/// <param name="waveformEffectType">Effect</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x33D4AB37, FirmwareVersion = 150)]
		public uint __sceSasRevType(SasCore* sasCore, WaveformEffectType waveformEffectType)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Sets the waveformEffectIsDry and waveformEffectIsWet to the specified sasCore.
		/// </summary>
		/// <param name="sasCore">Core</param>
		/// <param name="waveformEffectIsDry">waveformEffectIsDry</param>
		/// <param name="waveformEffectIsWet">waveformEffectIsWet</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0xF983B186, FirmwareVersion = 150)]
		public uint __sceSasRevVON(SasCore* sasCore, bool waveformEffectIsDry, bool waveformEffectIsWet)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Sets the effect left and right volumes for the specified sasCore.
		/// </summary>
		/// <param name="sasCore">Core</param>
		/// <param name="leftVol">Left volume</param>
		/// <param name="rightVol">Right volume</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xD5A229C9, FirmwareVersion = 150)]
		public uint __sceSasRevEVOL(SasCore* sasCore, int leftVol, int rightVol)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Sets the Voice (VAG pointer).
		/// 
		/// 4-bit ADPCM, mono sound format.
		/// 4-bit compressed sound format used by PlayStation and PlayStation Portable games;
		/// compressed using ADPCM (Adaptive Differential Pulse Code Modulation) encoding.
		/// </summary>
		/// <param name="sasCore">Core</param>
		/// <param name="voice">Voice</param>
		/// <param name="vagAddr">Pointer to the wave data</param>
		/// <param name="size">Size in bytes?? (to confirm)</param>
		/// <param name="loopmode">Number of times the voice should play</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x99944089, FirmwareVersion = 150)]
		public int __sceSasSetVoice(SasCore* sasCore, int voice, byte* vagAddr, int size, int loopmode)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Sets the pitch for a sasCore.voice.
		/// </summary>
		/// <param name="sasCore">SasCore</param>
		/// <param name="voice">Voice</param>
		/// <param name="pitch">Pitch to set. A value between 1 and 16384. The default value is 4096.</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xAD84D37F, FirmwareVersion = 150)]
		public int __sceSasSetPitch(SasCore* sasCore, int voice, int pitch)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Sets the stereo volumes for a sasCore.voice.
		/// </summary>
		/// <param name="sasCore">SasCore</param>
		/// <param name="voice">Voice</param>
		/// <param name="leftVolume">Left  Volume 0-0x1000</param>
		/// <param name="rightVolume">Right Volume 0-0x1000</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x440CA7D8, FirmwareVersion = 150)]
		public int __sceSasSetVolume(SasCore* sasCore, int voice, int leftVolume, int rightVolume)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="delay"></param>
		/// <param name="feedback"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x267A6DD2, FirmwareVersion = 150)]
		public int __sceSasRevParam(SasCore* sasCore, int delay, int feedback)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="voice"></param>
		/// <param name="sustainLevel"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x5F9529F6, FirmwareVersion = 150)]
		public int __sceSasSetSL(SasCore* sasCore, int voice, int sustainLevel)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="voice"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x74AE582A, FirmwareVersion = 150)]
		public int __sceSasGetEnvelopeHeight(SasCore* sasCore, int voice)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Pauses a set of voice channels for that sasCore.
		/// </summary>
		/// <param name="sasCore">SasCore</param>
		/// <param name="voice_bits">Voice Bit Set</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x787D04D5, FirmwareVersion = 150)]
		public int __sceSasSetPause(SasCore* sasCore, uint voice_bits)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x2C8E6AB3, FirmwareVersion = 150)]
		public int __sceSasGetPauseFlag(SasCore* sasCore)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Sets the ADSR (Attack Decay Sustain Release) for a sasCore.voice.
		/// </summary>
		/// <param name="sasCore">SasCore</param>
		/// <param name="voice">Voice</param>
		/// <param name="flags">Bitfield to set each envelope on or off.</param>
		/// <param name="attackRate">ADSR Envelope's attack type.</param>
		/// <param name="decayRate">ADSR Envelope's decay type.</param>
		/// <param name="sustainRate">ADSR Envelope's sustain type.</param>
		/// <param name="releaseRate">ADSR Envelope's release type.</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x019B25EB, FirmwareVersion = 150)]
		public int __sceSasSetADSR(SasCore* sasCore, int voice, AdsrFlags flags, uint attackRate, uint decayRate, uint sustainRate, uint releaseRate)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="voice"></param>
		/// <param name="env1Bitfield"></param>
		/// <param name="env2Bitfield"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCBCD4F79, FirmwareVersion = 150)]
		public int __sceSasSetSimpleADSR(SasCore* sasCore, int voice, uint env1Bitfield, uint env2Bitfield)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="voice"></param>
		/// <param name="flags"></param>
		/// <param name="attackCurveMode"></param>
		/// <param name="decayCurveMode"></param>
		/// <param name="sustainCurveMode"></param>
		/// <param name="releaseCurveMode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x9EC3676A, FirmwareVersion = 150)]
		public int __sceSasSetADSRmode(SasCore* sasCore, int voice, AdsrFlags flags, AdsrCurveMode attackCurveMode, AdsrCurveMode decayCurveMode, AdsrCurveMode sustainCurveMode, AdsrCurveMode releaseCurveMode)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="voice"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x76F01ACA, FirmwareVersion = 150)]
		public int __sceSasSetKeyOn(SasCore* sasCore, int voice)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="voice"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA0CF2FA4, FirmwareVersion = 150)]
		public int __sceSasSetKeyOff(SasCore* sasCore, int voice)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="voice"></param>
		/// <param name="noiseFreq"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB7660A23, FirmwareVersion = 150)]
		public int __sceSasSetNoise(SasCore* sasCore, int voice, int noiseFreq)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="sasInOut"></param>
		/// <param name="leftVol"></param>
		/// <param name="rightVol"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x50A14DFC, FirmwareVersion = 150)]
		public int __sceSasCoreWithMix(SasCore* sasCore, void* sasInOut, int leftVol, int rightVol)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sasCore"></param>
		/// <param name="sasOut"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA3589D81, FirmwareVersion = 150)]
		public uint __sceSasCore(SasCore* sasCore, void* sasOut)
		{
			throw(new NotImplementedException());
		}

		public const int PSP_SAS_VOICES_MAX = 32;
		public const int PSP_SAS_GRAIN_SAMPLES = 256;
		public const int PSP_SAS_VOL_MAX = 0x1000;
		public const int PSP_SAS_LOOP_MODE_OFF = 0;
		public const int PSP_SAS_LOOP_MODE_ON = 1;
		public const int PSP_SAS_PITCH_MIN = 0x1;
		public const int PSP_SAS_PITCH_BASE = 0x1000;
		public const int PSP_SAS_PITCH_MAX = 0x4000;
		public const int PSP_SAS_NOISE_FREQ_MAX = 0x3F;
		public const int PSP_SAS_ENVELOPE_HEIGHT_MAX = 0x40000000;
		public const int PSP_SAS_ENVELOPE_FREQ_MAX = 0x7FFFFFFF;
		public const int PSP_SAS_ADSR_ATTACK = 1;
		public const int PSP_SAS_ADSR_DECAY = 2;
		public const int PSP_SAS_ADSR_SUSTAIN = 4;
		public const int PSP_SAS_ADSR_RELEASE = 8;
	}

	public enum WaveformEffectType : int
	{
		PSP_SAS_EFFECT_TYPE_OFF   = -1,
		PSP_SAS_EFFECT_TYPE_ROOM  =  0,
		PSP_SAS_EFFECT_TYPE_UNK1  =  1,
		PSP_SAS_EFFECT_TYPE_UNK2  =  2,
		PSP_SAS_EFFECT_TYPE_UNK3  =  3,
		PSP_SAS_EFFECT_TYPE_HALL  =  4,
		PSP_SAS_EFFECT_TYPE_SPACE =  5,
		PSP_SAS_EFFECT_TYPE_ECHO  =  6,
		PSP_SAS_EFFECT_TYPE_DELAY =  7,
		PSP_SAS_EFFECT_TYPE_PIPE  =  8,
	}

	public enum AdsrFlags : uint 
	{
		hasAttack  = (1 << 0),
		hasDecay   = (1 << 1),
		hasSustain = (1 << 2),
		hasRelease = (1 << 3),
	}

	public enum OutputMode : uint
	{
		PSP_SAS_OUTPUTMODE_STEREO = 0,
		PSP_SAS_OUTPUTMODE_MULTICHANNEL = 1,
	}

	public enum AdsrCurveMode : uint
	{
		PSP_SAS_ADSR_CURVE_MODE_LINEAR_INCREASE = 0,
		PSP_SAS_ADSR_CURVE_MODE_LINEAR_DECREASE = 1,
		PSP_SAS_ADSR_CURVE_MODE_LINEAR_BENT = 2,
		PSP_SAS_ADSR_CURVE_MODE_EXPONENT_REV = 3,
		PSP_SAS_ADSR_CURVE_MODE_EXPONENT = 4,
		PSP_SAS_ADSR_CURVE_MODE_DIRECT = 5,
	}

	public struct SasEnvelope {
		int attackRate;
		int decayRate;
		int sustainRate;
		int releaseRate;
		AdsrCurveMode attackCurveMode;
		AdsrCurveMode decayCurveMode;
		AdsrCurveMode sustainCurveMode;
		AdsrCurveMode releaseCurveMode;
		int sustainLevel;
		int height;
	}
}
