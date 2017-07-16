using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Hle.Modules.threadman;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.audio
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceVaudio : HleModuleHost
    {
        [Inject] public sceAudio sceAudio;

        [Inject] public PspAudio PspAudio;

        [Inject] public HleThreadManager HleThreadManager;

        PspAudioChannel PspVaudioChannel
        {
            get { return PspAudio.SrcOutput2Channel; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x67585DFD, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceVaudioChRelease()
        {
            if (!PspVaudioChannel.IsReserved)
            {
                throw (new SceKernelException(SceKernelErrors.ERROR_AUDIO_CHANNEL_NOT_RESERVED));
            }

            PspVaudioChannel.Release();
            VAudioReserved = false;

            return 0;
        }


        bool VAudioReserved = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SampleCount"></param>
        /// <param name="Frequency"></param>
        /// <param name="Channels"></param>
        [HlePspFunction(NID = 0x03B6807D, FirmwareVersion = 150)]
        public int sceVaudioChReserve(int SampleCount, int Frequency, int Channels)
        {
            try
            {
                if (VAudioReserved)
                {
                    throw (new SceKernelException(SceKernelErrors.ERROR_BUSY));
                }

                if (PspVaudioChannel.IsReserved)
                {
                    throw (new SceKernelException(SceKernelErrors.ERROR_AUDIO_CHANNEL_ALREADY_RESERVED));
                }

                if (SampleCount != 256 && SampleCount != 1024 && SampleCount != 2048)
                {
                    throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_SIZE));
                }

                if (Channels != 2)
                {
                    throw (new SceKernelException(SceKernelErrors.ERROR_INVALID_FORMAT));
                }

                VAudioReserved = true;
                PspVaudioChannel.IsReserved = true;
                PspVaudioChannel.SampleCount = SampleCount;
                PspVaudioChannel.Frequency = Frequency;
                PspVaudioChannel.NumberOfChannels = Channels;

                return 0;
            }
            catch (InvalidAudioFormatException)
            {
                throw (new SceKernelException(SceKernelErrors.ERROR_AUDIO_CHANNEL_ALREADY_RESERVED));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Volume"></param>
        /// <param name="Buffer"></param>
        [HlePspFunction(NID = 0x8986295E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceVaudioOutputBlocking(int Volume, byte* Buffer)
        {
            if (Buffer == null) throw (new SceKernelException(SceKernelErrors.ERROR_AUDIO_PRIV_REQUIRED));

#if false
			if (!pspVaudioChannel.isOutputBlocking()) {
				if (log.isDebugEnabled()) {
					log.debug("sceVaudioOutputBlocking[not blocking] " + pspVaudioChannel.toString());
				}
				if((vol & PSP_VAUDIO_VOLUME_BASE) != PSP_VAUDIO_VOLUME_BASE) {
					changeChannelVolume(pspVaudioChannel, vol, vol);
				}
				cpu.gpr[2] = doAudioOutput(pspVaudioChannel, buf);
				if (log.isDebugEnabled()) {
					log.debug("sceVaudioOutputBlocking[not blocking] returning " + cpu.gpr[2] + " (" + pspVaudioChannel.toString() + ")");
				}
				Modules.ThreadManForUserModule.hleRescheduleCurrentThread();
			} else {
				if (log.isDebugEnabled()) {
					log.debug("sceVaudioOutputBlocking[blocking] " + pspVaudioChannel.toString());
				}
				blockThreadOutput(pspVaudioChannel, buf, vol, vol);
			}
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Type"></param>
        /// <param name="Volume"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0x346FBE94, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceVaudioSetEffectType(int Type, int Volume)
        {
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AlcMode"></param>
        /// <returns></returns>
        [HlePspFunction(NID = 0xCBD4AC51, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceVaudioSetAlcMode(int AlcMode)
        {
            return 0;
        }
    }
}