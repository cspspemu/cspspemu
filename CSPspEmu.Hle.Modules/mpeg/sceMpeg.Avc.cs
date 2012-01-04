using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.mpeg
{
	/// <summary>
	/// AVC: Advanced Video Coding
	/// </summary>
	unsafe public partial class sceMpeg
	{
		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x0F6C18D7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegAvcDecodeDetail()
		{
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x4571CC64, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegAvcDecodeFlush()
		{
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// sceMpegGetAvcAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">associated stream</param>
		/// <param name="pAu">will contain pointer to Au</param>
		/// <param name="iUnk">unknown</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xFE246728, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegGetAvcAu(SceMpeg* Mpeg, SceMpegStream* pStream, SceMpegAu* pAu, int* iUnk)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegFreeAvcEsBuf
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="EsBuffer"></param>
		[HlePspFunction(NID = 0xCEB870B1, FirmwareVersion = 150)]
		public uint sceMpegFreeAvcEsBuf(SceMpeg* Mpeg, int EsBuffer)
		{
			AbvEsBufAllocated[EsBuffer - 1] = false;
			return 0;
		}

		protected bool[] AbvEsBufAllocated = new bool[2];

		/// <summary>
		/// sceMpegMallocAvcEsBuf
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns>
		///		0 if error else a id.
		/// </returns>
		[HlePspFunction(NID = 0xA780CF7E, FirmwareVersion = 150)]
		public int sceMpegMallocAvcEsBuf(SceMpeg* Mpeg)
		{
			for (int n = 0; n < 2; n++)
			{
				if (!AbvEsBufAllocated[n])
				{
					AbvEsBufAllocated[n] = true;
					return n + 1;
				}
			}
			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeStop
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="iFrameWidth">output buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="pBuffer">buffer that will contain the decoded frame</param>
		/// <param name="iStatus">frame number</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x740FCCD1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeStop(SceMpeg* Mpeg, int iFrameWidth, void* pBuffer, int* iStatus)
		{
			//throw(new NotImplementedException());
			return -1;
		}


		/// <summary>
		/// Sets the SceMpegAvcMode to a Mpeg
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="Mode">pointer to SceMpegAvcMode struct defining the decode mode (pixelformat)</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xA11C7026, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeMode(SceMpeg* Mpeg, SceMpegAvcMode* Mode)
		{
			var SceMpegData = GetSceMpegData(Mpeg);

			if (Mode != null)
			{
				SceMpegData->SceMpegAvcMode = *Mode;
			}
			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecode
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pAu">video Au</param>
		/// <param name="iFrameWidth">output buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="pBuffer">buffer that will contain the decoded frame</param>
		/// <param name="iInit">will be set to 0 on first call, then 1</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x0E3C2E9D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecode(SceMpeg* Mpeg, SceMpegAu* pAu, int iFrameWidth, void* pBuffer, int* iInit)
		{
			throw(new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
