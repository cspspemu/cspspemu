using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core;

namespace CSPspEmu.Hle.Modules.audio
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public unsafe class sceVaudio : HleModuleHost
	{
		[Inject]
		public sceAudio sceAudio;

		public class PspVaudioChannelClass
		{
			public bool IsReserved;
			internal void Release()
			{
			}

			public int SampleCount { get; set; }

			public int SampleRate { get; set; }

			public int Format { get; set; }
		}

		PspVaudioChannelClass PspVaudioChannel = new PspVaudioChannelClass();

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
			PspVaudioChannel.IsReserved = false;

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SampleCount"></param>
		/// <param name="SampleRate"></param>
		/// <param name="Format"></param>
		[HlePspFunction(NID = 0x03B6807D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceVaudioChReserve(int SampleCount, int SampleRate, int Format)
		{
			if (PspVaudioChannel.IsReserved)
			{
				throw (new SceKernelException((SceKernelErrors)unchecked(-1)));
			}

			PspVaudioChannel.IsReserved = true;
			PspVaudioChannel.SampleCount = SampleCount;
			PspVaudioChannel.SampleRate = SampleRate;
			PspVaudioChannel.Format = Format;

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Volume"></param>
		/// <param name="Buffer"></param>
		[HlePspFunction(NID = 0x8986295E,  FirmwareVersion = 150)]
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
		[HlePspFunction(NID = 0x346FBE94,  FirmwareVersion = 150)]
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
		[HlePspFunction(NID = 0xCBD4AC51,  FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceVaudioSetAlcMode(int AlcMode)
		{
			return 0;
		}
	}
}
