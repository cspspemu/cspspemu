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
		//[HlePspNotImplemented]
		public int __sceSasSetVoice(uint SasCorePointer, int Voice, byte* VagPointer, int VagSize, int LoopCount)
		{
			try
			{
				var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);
				SasVoice.Vag = new Vag(VagPointer, VagSize);
				SasVoice.Vag.Reset();
				SasVoice.Vag.SetLoopCount(LoopCount);

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
		//[HlePspNotImplemented]
		public int __sceSasSetPitch(uint SasCorePointer, int Voice, int Pitch)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);

			if (Pitch < PSP_SAS_PITCH_MIN || Pitch > PSP_SAS_PITCH_MAX)
			{
				return -1;
			}

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
		/// <param name="EffectLeftVol">Left  Volume 0-0x1000</param>
		/// <param name="EffectRightVol">Right Volume 0-0x1000</param>
		/// <returns>0 on success.</returns>
		[HlePspFunction(NID = 0x440CA7D8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetVolume(uint SasCorePointer, int Voice, int LeftVolume, int RightVolume, int EffectLeftVol, int EffectRightVol)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);

			LeftVolume = Math.Abs(LeftVolume);
			RightVolume = Math.Abs(RightVolume);
			EffectLeftVol = Math.Abs(EffectLeftVol);
			EffectRightVol = Math.Abs(EffectRightVol);

			if (LeftVolume > PSP_SAS_VOL_MAX || RightVolume > PSP_SAS_VOL_MAX || EffectLeftVol > PSP_SAS_VOL_MAX || EffectRightVol > PSP_SAS_VOL_MAX)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_SAS_INVALID_VOLUME_VAL));
			}

			SasVoice.LeftVolume = LeftVolume;
			SasVoice.RightVolume = RightVolume;
			SasVoice.EffectLeftVolume = EffectLeftVol;
			SasVoice.EffectRightVolume = EffectRightVol;

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
		[HlePspNotImplemented]
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
		//[HlePspNotImplemented]
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
		/// <param name="Voice">Voice</param>
		/// <param name="Flags">Bitfield to set each envelope on or off.</param>
		/// <param name="AttackRate">ADSR Envelope's attack type.</param>
		/// <param name="DecayRate">ADSR Envelope's decay type.</param>
		/// <param name="SustainRate">ADSR Envelope's sustain type.</param>
		/// <param name="ReleaseRate">ADSR Envelope's release type.</param>
		/// <returns>0 on success.</returns>
		/// <seealso cref="http://en.wikipedia.org/wiki/Synthesizer#ADSR_envelope"/>
		[HlePspFunction(NID = 0x019B25EB, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int __sceSasSetADSR(uint SasCorePointer, int Voice, AdsrFlags Flags, int AttackRate, int DecayRate, int SustainRate, int ReleaseRate)
		{
			var SasVoice = GetSasCoreVoice(SasCorePointer, Voice);
			
			if (Flags.HasFlag(AdsrFlags.HasAttack)) SasVoice.Envelope.AttackRate = AttackRate;
			if (Flags.HasFlag(AdsrFlags.HasDecay)) SasVoice.Envelope.DecayRate = DecayRate;
			if (Flags.HasFlag(AdsrFlags.HasSustain)) SasVoice.Envelope.SustainRate = SustainRate;
			if (Flags.HasFlag(AdsrFlags.HasRelease)) SasVoice.Envelope.ReleaseRate = ReleaseRate;

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
