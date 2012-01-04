using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.pmfplayer
{
	//[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class scePsmfPlayer : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="psmfPlayerDataAddr"></param>
		[HlePspFunction(NID = 0x235D8787, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerCreate(int psmfPlayer, void* psmfPlayerDataAddr)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x9B71A274, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerDelete(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="FileName"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x3D6D25A9, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSetPsmf(int psmfPlayer, string FileName)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="FileName"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x58B83577, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSetPsmfCB(int psmfPlayer, string FileName)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE792CD94, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerReleasePsmf(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processor"></param>
		[HlePspFunction(NID = 0x95A84EE5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerStart(int psmfPlayer, PlayInfoStruct* initPlayInfoAddr, int initPts)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processor"></param>
		[HlePspFunction(NID = 0x3EA82A4B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetAudioOutSize(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processor"></param>
		[HlePspFunction(NID = 0x1078C008, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerStop(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		[HlePspFunction(NID = 0xA0B8CA55, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerUpdate(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="processor"></param>
		[HlePspFunction(NID = 0x46F61F8B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetVideoData(int psmfPlayer, void* VideoData)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="AudioData"></param>
		[HlePspFunction(NID = 0xB9848A74, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetAudioData(int psmfPlayer, void* AudioData)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF8EF08A6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetCurrentStatus(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="Info"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xDF089680, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetPsmfInfo(int psmfPlayer, PmfInfoStruct* Info)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="configMode"></param>
		/// <param name="configAttr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x1E57A8E7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerConfigPlayer(int psmfPlayer, PSMF_PLAYER_CONFIG_MODE configMode, int configAttr)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="newPlayMode"></param>
		/// <param name="newPlaySpeed"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA3D81169, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerChangePlayMode(int psmfPlayer, int newPlayMode, int newPlaySpeed)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="audioCodecAddr"></param>
		/// <param name="audioStreamNumAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x68F07175, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetCurrentAudioStream(int psmfPlayer, uint* audioCodecAddr, uint* audioStreamNumAddr)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="playModeAddr"></param>
		/// <param name="playSpeedAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF3EFAA91, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetCurrentPlayMode(int psmfPlayer, uint* playModeAddr, uint* playSpeedAddr)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="currentPtsAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x3ED62233, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetCurrentPts(int psmfPlayer, uint* currentPtsAddr)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="videoCodecAddr"></param>
		/// <param name="videoStreamNumAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x9FF2B2E7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerGetCurrentVideoStream(int psmfPlayer, uint* videoCodecAddr, uint* videoStreamNumAddr)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		[HlePspFunction(NID = 0x2BEB1569, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerBreak(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="FileName"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x76C0F4AE, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSetPsmfOffset(int psmfPlayer, string FileName)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="FileName"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA72DB4F9, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSetPsmfOffsetCB(int psmfPlayer, string FileName)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x2D0E4E0A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSetTempBuf(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <param name="newVideoCodec"></param>
		/// <param name="newVideoStreamNum"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x75F03FA2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSelectSpecificVideo(int psmfPlayer, uint newVideoCodec, uint newVideoStreamNum)
		{
			return 0;
		}

		[HlePspFunction(NID = 0x85461EFF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSelectSpecificAudio(int psmfPlayer, uint newAudioCodec, uint newAudioStreamNum)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		[HlePspFunction(NID = 0x8A9EBDCD, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSelectVideo(int psmfPlayer)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmfPlayer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB8D10C56, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfPlayerSelectAudio(int psmfPlayer)
		{
			return 0;
		}

		public struct PlayInfoStruct
		{
			public uint videoCodec;
			public uint videoStreamNum;
			public uint audioCodec;
			public uint audioStreamNum;
			public uint playMode;
			public uint playSpeed;
		}

		public struct PmfInfoStruct
		{
			public uint psmfCurrentPts;
			public uint psmfAvcStreamNum;
			public uint psmfAtracStreamNum;
			public uint psmfPcmStreamNum;
			public uint psmfPlayerVersion;
		}

		public enum PSMF_PLAYER_CONFIG_MODE : uint
		{
			Loop = 0,
			PixelType = 1,
		}
	}
}
