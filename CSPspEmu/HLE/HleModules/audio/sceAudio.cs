using System;
using System.Linq;
using CSPspEmu.Core.Audio;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.audio
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceAudio : HleModuleHost
    {
        [Inject] PspAudio PspAudio;

        [Inject] HleThreadManager ThreadManager;

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

        /*
        /// <summary>
        /// Make the given sample count a multiple of 64.
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        //public Type PSP_AUDIO_SAMPLE_ALIGN(Type)(Type s) { return (s + 63) & ~63; }
        */

        //protected int Output2ChannelId = -1;
        //protected bool Output2Reserved = false;

        /// <summary>
        /// Reserve the audio output and set the output sample count
        /// </summary>
        /// <param name="SampleCount">The number of samples to output in one output call (min 17, max 4111).</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x01562BA3, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceAudioOutput2Reserve(int SampleCount)
        {
            if (!IsValidSampleCountOutput2(SampleCount))
                throw new SceKernelException(SceKernelErrors.ERROR_INVALID_SIZE);
            if (!PspAudio.SrcOutput2Channel.Available)
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_CHANNEL_ALREADY_RESERVED);
            PspAudio.SrcOutput2Channel.Available = false;
            PspAudio.SrcOutput2Channel.SampleCount = SampleCount;
            PspAudio.SrcOutput2Channel.Format = PspAudio.FormatEnum.Stereo;
            //Output2Reserved = true;
            //_sceAudioChReserve(-1, SamplesCount, PspAudio.FormatEnum.Stereo, false);
            return 0;
            //throw(new NotImplementedException());
        }

        /// <summary>
        /// Output audio (blocking)
        /// </summary>
        /// <param name="Volume">The volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
        /// <param name="Buffer">Pointer to the PCM data.</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x2D53F36E, FirmwareVersion = 150)]
        public int sceAudioOutput2OutputBlocking(int Volume, short* Buffer)
        {
            return _sceAudioOutputPannedBlocking(PspAudio.SrcOutput2Channel, Volume, Volume, Buffer, Blocking: true);
        }

        /// <summary>
        /// Release the audio output
        /// </summary>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x43196845, FirmwareVersion = 150)]
        public int sceAudioOutput2Release()
        {
            return _sceAudioChRelease(PspAudio.SrcOutput2Channel);
        }

        /// <summary>
        /// Get count of unplayed samples remaining
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <returns>Number of samples to be played, an error if less than 0.</returns>
        [HlePspFunction(NID = 0xB011922F, FirmwareVersion = 150)]
        public int sceAudioGetChannelRestLength(int ChannelId)
        {
#if true
            return 844;
#else
			var Channel = PspAudio.GetChannel(ChannelId);
			int RestLength = Channel.AvailableChannelsForRead;
			//Console.Error.WriteLine(RestLength);
			return RestLength;
			//return -1;
#endif
        }

        private int _sceAudioOutputPannedBlocking(PspAudioChannel Channel, int LeftVolume, int RightVolume,
            short* Buffer, bool Blocking)
        {
            ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Audio,
                $"_sceAudioOutputPannedBlocking({Channel}, Volume({LeftVolume}, {RightVolume}), Blocking({Blocking}))", Channel, WakeUpCallback =>
                {
                    Channel.Write(Buffer, LeftVolume, RightVolume, () =>
                    {
                        if (Blocking) WakeUpCallback();
                    });
                    /*
                    if (Blocking)
                    {
                        PspRtc.RegisterTimerInOnce(TimeSpan.FromMilliseconds(1), () =>
                        {
                            WakeUpCallback();
                        });
                    }
                    */
                    if (!Blocking) WakeUpCallback();
                });
            return Channel.SampleCount;
        }

        private PspAudioChannel GetChannel(int ChannelId, bool CanAlloc = false)
        {
            try
            {
                return PspAudio.GetChannel(ChannelId, CanAlloc);
            }
            catch (NoChannelsAvailableException)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_NO_CHANNELS_AVAILABLE);
            }
            catch (InvalidChannelException)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_INVALID_CHANNEL);
            }
        }

        /// <summary>
        /// Output panned audio of the specified channel (blocking)
        /// </summary>
        /// <param name="ChannelId">The channel number</param>
        /// <param name="LeftVolume">The left volume</param>
        /// <param name="RightVolume">The right volume</param>
        /// <param name="Buffer">Pointer to the PCM data to output</param>
        /// <param name="Blocking"></param>
        /// <returns>
        ///		Number of samples played.
        ///		A negative value on error.
        /// </returns>
        private int _sceAudioOutputPannedBlocking(int ChannelId, int LeftVolume, int RightVolume, short* Buffer,
            bool Blocking)
        {
            //Console.WriteLine(ChannelId);
            return _sceAudioOutputPannedBlocking(
                GetChannel(ChannelId),
                LeftVolume,
                RightVolume,
                Buffer,
                Blocking
            );
        }

        /// <summary>
        /// Output panned audio of the specified channel (blocking)
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <param name="LeftVolume">The left volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
        /// <param name="RightVolume">The right volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
        /// <param name="Buffer">Pointer to the PCM data to output.</param>
        /// <returns>
        ///		Number of samples played.
        ///		A negative value on error.
        /// </returns>
        [HlePspFunction(NID = 0x13F592BC, FirmwareVersion = 150)]
        public int sceAudioOutputPannedBlocking(int ChannelId, int LeftVolume, int RightVolume, short* Buffer)
        {
            return _sceAudioOutputPannedBlocking(
                ChannelId: ChannelId,
                LeftVolume: LeftVolume,
                RightVolume: RightVolume,
                Buffer: Buffer,
                Blocking: true
            );
        }


        /// <summary>
        /// Output audio of the specified channel (blocking)
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <param name="Volume">The volume.</param>
        /// <param name="Buffer">Pointer to the PCM data to output.</param>
        /// <returns>
        ///		Number of samples played.
        ///		A negative value on error.
        ///	</returns>
        [HlePspFunction(NID = 0x136CAF51, FirmwareVersion = 150)]
        public int sceAudioOutputBlocking(int ChannelId, int Volume, short* Buffer)
        {
            return _sceAudioOutputPannedBlocking(
                ChannelId: ChannelId,
                LeftVolume: Volume,
                RightVolume: Volume,
                Buffer: Buffer,
                Blocking: true
            );
        }

        /// <summary>
        /// Output panned audio of the specified channel
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <param name="LeftVolume">The left volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
        /// <param name="RightVolume">The right volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
        /// <param name="Buffer">Pointer to the PCM data to output.</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0xE2D56B2D, FirmwareVersion = 150)]
        public int sceAudioOutputPanned(int ChannelId, int LeftVolume, int RightVolume, short* Buffer)
        {
            return _sceAudioOutputPannedBlocking(
                ChannelId: ChannelId,
                LeftVolume: LeftVolume,
                RightVolume: RightVolume,
                Buffer: Buffer,
                Blocking: false
            );
        }

        /// <summary>
        /// Output audio of the specified channel
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <param name="Volume">The volume. A value between 0 and PSP_AUDIO_VOLUME_MAX.</param>
        /// <param name="Buffer">Pointer to the PCM data to output.</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x8C1009B2, FirmwareVersion = 150)]
        public int sceAudioOutput(int ChannelId, int Volume, short* Buffer)
        {
            return _sceAudioOutputPannedBlocking(
                ChannelId: ChannelId,
                LeftVolume: Volume,
                RightVolume: Volume,
                Buffer: Buffer,
                Blocking: false
            );
        }

        /// <summary>
        /// Get count of unplayed samples remaining
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <returns>Number of samples to be played, an error if less than 0.</returns>
        [HlePspFunction(NID = 0xE9D97901, FirmwareVersion = 150)]
        public int sceAudioGetChannelRestLen(int ChannelId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change the output sample count, after it's already been reserved
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <param name="SampleCount">The number of samples to output in one output call.</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0xCB2E439E, FirmwareVersion = 150)]
        public int sceAudioSetChannelDataLen(int ChannelId, int SampleCount)
        {
            var Channel = GetChannel(ChannelId);
            Channel.SampleCount = SampleCount;
            Channel.Updated();
            return 0;
        }

        /// <summary>
        /// Change the format of a channel
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <param name="Format">One of PspAudioFormats</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x95FD0C2D, FirmwareVersion = 150)]
        public int sceAudioChangeChannelConfig(int ChannelId, PspAudio.FormatEnum Format)
        {
            var Channel = GetChannel(ChannelId);
            Channel.Format = Format;
            Channel.Updated();
            return 0;
        }

        public bool IsValidSampleCountOutput2(int SampleCount)
        {
            return !(SampleCount < 17 || SampleCount > 4111);
        }

        private bool IsValidSampleCount(int SampleCount)
        {
            return !((SampleCount & 63) != 0 || SampleCount <= 0 || SampleCount > PspAudio.SamplesMax);
        }

        private int _sceAudioChReserve(Func<PspAudioChannel> ChannelGet, int SampleCount, PspAudio.FormatEnum Format)
        {
            if (!Enum.IsDefined(typeof(PspAudio.FormatEnum), Format))
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_INVALID_FORMAT);
            }

            try
            {
                var Channel = ChannelGet();
                if (!Channel.Available) throw new InvalidChannelException();
                Channel.Available = false;
                Channel.SampleCount = SampleCount;
                Channel.Format = Format;
                Channel.Updated();
                return Channel.Index;
            }
            catch (NoChannelsAvailableException)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_NO_CHANNELS_AVAILABLE);
            }
            catch (InvalidChannelException)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_INVALID_CHANNEL);
            }
        }

        private int _sceAudioChReserve(int ChannelId, int SampleCount, PspAudio.FormatEnum Format)
        {
            if (!IsValidSampleCount(SampleCount))
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_OUTPUT_SAMPLE_DATA_SIZE_NOT_ALIGNED);
            }

            return _sceAudioChReserve(
                () => GetChannel(ChannelId, CanAlloc: true),
                SampleCount,
                Format
            );
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
        ///		output call.  It must be a value between <see cref="PSP_AUDIO_SAMPLE_MIN"/>
        ///		and <see cref="PSP_AUDIO_SAMPLE_MAX"/>, and it must be aligned to 64 bytes
        ///		(use the ::PSP_AUDIO_SAMPLE_ALIGN macro to align it).
        /// </param>
        /// <param name="Format">The output format to use for the channel.  One of ::PspAudioFormats.</param>
        /// <returns>The channel number on success, an error code if less than 0.</returns>
        [HlePspFunction(NID = 0x5EC81C55, FirmwareVersion = 150)]
        public int sceAudioChReserve(CpuThreadState CpuThreadState, int ChannelId, int SampleCount,
            PspAudio.FormatEnum Format)
        {
            var RetChannelId = _sceAudioChReserve(ChannelId, SampleCount, Format);
            CpuThreadState.Reschedule();
            //ThreadManager.
            return RetChannelId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Channel"></param>
        /// <returns></returns>
        public int _sceAudioChRelease(PspAudioChannel Channel)
        {
            Channel.Available = true;
            //throw (new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Release a hardware output channel.
        /// </summary>
        /// <param name="Channel">The channel to release.</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x6FC46853, FirmwareVersion = 150)]
        public int sceAudioChRelease(int ChannelId)
        {
            try
            {
                _sceAudioChRelease(GetChannel(ChannelId));
                return 0;
            }
            catch (InvalidChannelException)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_INVALID_CHANNEL);
            }
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change the volume of a channel
        /// </summary>
        /// <param name="ChannelId">The channel number.</param>
        /// <param name="VolumeLeft">The left volume.</param>
        /// <param name="VolumeRight">The right volume.</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0xB7E1D8E7, FirmwareVersion = 150)]
        //[HlePspNotImplemented()]
        public int sceAudioChangeChannelVolume(int ChannelId, int VolumeLeft, int VolumeRight)
        {
            var Channel = GetChannel(ChannelId);

            if (VolumeLeft > 0xFFFF || VolumeRight > 0xFFFF)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_INVALID_VOLUME);
            }

            if (!Channel.IsReserved)
            {
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_RESERVED);
            }

            if (VolumeLeft >= 0) Channel.VolumeLeft = VolumeLeft;
            if (VolumeRight >= 0) Channel.VolumeRight = VolumeRight;

            return 0;
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
            if (Frequency != 44100) throw new Exception($"sceAudioSRCChReserve: {Frequency}");
            var ValidFrequencies = new int[] {0, 8000, 11025, 12000, 16000, 22050, 24000, 32000, 48000};
            if (!ValidFrequencies.Contains(Frequency))
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_INVALID_FREQUENCY);
            if (Channels == 4) throw new SceKernelException(SceKernelErrors.PSP_AUDIO_ERROR_SRC_FORMAT_4);
            if (Channels != 2) throw new SceKernelException(SceKernelErrors.ERROR_INVALID_SIZE);
            if (!PspAudio.SrcOutput2Channel.Available)
                throw new SceKernelException(SceKernelErrors.ERROR_AUDIO_CHANNEL_ALREADY_RESERVED);
            if (!IsValidSampleCountOutput2(SampleCount))
                throw new SceKernelException(SceKernelErrors.ERROR_INVALID_SIZE);
            _sceAudioChReserve(() => PspAudio.SrcOutput2Channel, SampleCount,
                Channels == 2 ? Core.Audio.PspAudio.FormatEnum.Stereo : Core.Audio.PspAudio.FormatEnum.Mono);
            PspAudio.SrcOutput2Channel.Frequency = Frequency;
            return 0;
        }

        /// <summary>
        /// Release the audio output
        /// </summary>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x5C37C0AE, FirmwareVersion = 150)]
        public int sceAudioSRCChRelease()
        {
            PspAudio.SrcOutput2Channel.Available = true;
            //throw (new NotImplementedException());
            return 0;
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change the output sample count, after it's already been reserved
        /// </summary>
        /// <param name="SampleCount">The number of samples to output in one output call (min 17, max 4111).</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x63F2889C, FirmwareVersion = 150)]
        public int sceAudioOutput2ChangeLength(int SampleCount)
        {
            var Channel = PspAudio.SrcOutput2Channel;
            try
            {
                //return Channel.SampleCount;
                return 0;
            }
            finally
            {
                Channel.SampleCount = SampleCount;
            }
            //return 0;
        }

        /// <summary>
        /// Get count of unplayed samples remaining
        /// </summary>
        /// <returns>Number of samples to be played, an error if less than 0.</returns>
        [HlePspFunction(NID = 0x647CEF33, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceAudioOutput2GetRestSample()
        {
            //throw (new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Init audio input (with extra arguments)
        /// </summary>
        /// <param name="parameters">A pointer to a <see cref="pspAudioInputParams"/> struct.</param>
        /// <returns>0 on success, an error if less than 0.</returns>
        [HlePspFunction(NID = 0xE926D3FB, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceAudioInputInitEx(pspAudioInputParams*parameters)
        {
            return 0;
        }
    }
}