using System;
using CSPspEmu.Hle.Formats.audio;

namespace CSPspEmu.Hle.Modules.sc_sascore
{
	public unsafe partial class sceSasCore
	{
		/// <summary>
		/// Sets the Voice (VAG pointer).
		/// 
		/// 4-bit ADPCM, mono sound format.
		/// 4-bit compressed sound format used by PlayStation and PlayStation Portable games;
		/// compressed using ADPCM (Adaptive Differential Pulse Code Modulation) encoding.
		/// </summary>
		/// <param name="SasCorePointer">Core</param>
		/// <param name="Voice">Voice</param>
		/// <param name="VagPointer">Pointer to the wave data</param>
		/// <param name="VagSize">Size in bytes?? (to confirm)</param>
		/// <param name="LoopCount">Number of times the voice should play</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x99944089, FirmwareVersion = 150)]
		public int __sceSasSetVoice(uint SasCorePointer, int Voice, byte* VagPointer, int VagSize, int LoopCount)
		{
			try
			{
				var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);
				SasVoice.Vag = new Vag();
				SasVoice.Vag.Load(VagPointer, VagSize);
				SasVoice.SampleOffset = 0;
				SasVoice.LoopCount = LoopCount;

				//var VagPointer = (byte *)MemoryManager.Memory.PspAddressToPointerSafe(VagAddress);
				//File.WriteAllBytes("test.vag", PointerUtils.PointerToByteArray(VagPointer, VagSize));

				return 0;
			}
			catch (Exception Exception)
			{
				Console.Error.WriteLine(Exception);
				return 0;
			}
		}

		/// <summary>
		/// Sets the pitch for a SasCore.voice.
		/// </summary>
		/// <param name="SasCorePointer">SasCore</param>
		/// <param name="Voice">Voice</param>
		/// <param name="Pitch">Pitch to set. A value between 1 and 16384. The default value is 4096.</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xAD84D37F, FirmwareVersion = 150)]
		public int __sceSasSetPitch(uint SasCorePointer, int Voice, int Pitch)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);

			SasVoice.Pitch = Pitch;

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Sets the stereo volumes for a SasCore.voice.
		/// </summary>
		/// <param name="SasCorePointer">SasCore</param>
		/// <param name="Voice">Voice</param>
		/// <param name="LeftVolume">Left  Volume 0-0x1000</param>
		/// <param name="RightVolume">Right Volume 0-0x1000</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x440CA7D8, FirmwareVersion = 150)]
		public int __sceSasSetVolume(uint SasCorePointer, int Voice, int LeftVolume, int RightVolume)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);

			SasVoice.LeftVolume = LeftVolume;
			SasVoice.RightVolume = RightVolume;

			//throw(new NotImplementedException());
			return 0;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="Voice"></param>
		/// <param name="SustainLevel"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x5F9529F6, FirmwareVersion = 150)]
		public int __sceSasSetSL(uint SasCorePointer, int Voice, int SustainLevel)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);

			SasVoice.SustainLevel = SustainLevel;

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="Voice"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x74AE582A, FirmwareVersion = 150, SkipLog = true)]
		public int __sceSasGetEnvelopeHeight(uint SasCorePointer, int Voice)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);

			return SasVoice.EnvelopeHeight;

			//throw(new NotImplementedException());
			//return 0;
		}


		/// <summary>
		/// Sets the ADSR (Attack Decay Sustain Release) for a SasCore.voice.
		/// </summary>
		/// <param name="SasCorePointer">SasCore</param>
		/// <param name="voice">Voice</param>
		/// <param name="flags">Bitfield to set each envelope on or off.</param>
		/// <param name="attackRate">ADSR Envelope's attack type.</param>
		/// <param name="decayRate">ADSR Envelope's decay type.</param>
		/// <param name="sustainRate">ADSR Envelope's sustain type.</param>
		/// <param name="releaseRate">ADSR Envelope's release type.</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x019B25EB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetADSR(uint SasCorePointer, int voice, AdsrFlags flags, uint attackRate, uint decayRate, uint sustainRate, uint releaseRate)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="Voice"></param>
		/// <param name="Env1Bitfield"></param>
		/// <param name="Env2Bitfield"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCBCD4F79, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetSimpleADSR(uint SasCorePointer, int Voice, ushort Env1Bitfield, ushort Env2Bitfield)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);


			// The bitfields represent every value except for the decay curve shape,
			// which seems to be unchanged in simple mode.
			/*
			SasVoice.Envelope.SustainLevel = (int)((BitUtils.Extract(Env1Bitfield, 0, 4) + 1) << 26);
			SasVoice.Envelope.DecayRate = (int)(0x80000000 >> (int)(BitUtils.Extract(Env1Bitfield, 4, 4));
			SasVoice.Envelope.DecayCurveMode = AdsrCurveMode.PSP_SAS_ADSR_CURVE_MODE_EXPONENT_DECREASE;
			SasVoice.Envelope.AttackRate = getSimpleAttackRate(env1Bitfield);
			SasVoice.Envelope.AttackCurveMode = getSimpleAttackCurveType(env1Bitfield);


			SasVoice.Envelope.ReleaseRate = getSimpleReleaseRate(env2Bitfield);
			SasVoice.Envelope.ReleaseCurveMode = getSimpleReleaseCurveType(env2Bitfield);
			SasVoice.Envelope.SustainRate = getSimpleSustainRate(env2Bitfield);
			SasVoice.Envelope.SustainCurveMode = getSimpleSustainCurveType(env2Bitfield);
			*/

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="voice"></param>
		/// <param name="flags"></param>
		/// <param name="attackCurveMode"></param>
		/// <param name="decayCurveMode"></param>
		/// <param name="sustainCurveMode"></param>
		/// <param name="releaseCurveMode"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x9EC3676A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetADSRmode(uint SasCorePointer, int voice, AdsrFlags flags, AdsrCurveMode attackCurveMode, AdsrCurveMode decayCurveMode, AdsrCurveMode sustainCurveMode, AdsrCurveMode releaseCurveMode)
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="Voice"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x76F01ACA, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int __sceSasSetKeyOn(uint SasCorePointer, int Voice)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);
			SasVoice.SetOn(true);
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="Voice"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA0CF2FA4, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int __sceSasSetKeyOff(uint SasCorePointer, int Voice)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);
			SasVoice.SetOn(false);
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SasCorePointer"></param>
		/// <param name="Voice"></param>
		/// <param name="NoiseFrequency"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB7660A23, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetNoise(uint SasCorePointer, int Voice, int NoiseFrequency)
		{
			return 0;
			//throw (new NotImplementedException());
		}
	}
}
