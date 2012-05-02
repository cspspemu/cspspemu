using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.mpeg
{
	/// <summary>
	/// AVC: Advanced Video Coding
	/// </summary>
	/// <see cref="http://en.wikipedia.org/wiki/H.264/MPEG-4_AVC"/>
	unsafe public partial class sceMpeg
	{
		protected bool[] AbvEsBufAllocated = new bool[2];

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="AvcDecodeDetail"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0F6C18D7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeDetail(SceMpegPointer* Mpeg, AvcDecodeDetailStruct* AvcDecodeDetail)
		{
			CheckEnabledMpeg();

			var SceMpegData = GetSceMpegData(Mpeg);

			//throw(new NotImplementedException());
			AvcDecodeDetail->AvcDecodeResult = 0;
			AvcDecodeDetail->VideoFrameCount = 0;
			AvcDecodeDetail->AvcDetailFrameWidth = 512;
			AvcDecodeDetail->AvcDetailFrameHeight = 272;
			AvcDecodeDetail->FrameCropRectLeft = 0;
			AvcDecodeDetail->FrameCropRectRight = 0;
			AvcDecodeDetail->FrameCropRectTop = 0;
			AvcDecodeDetail->FrameCropRectBottom = 0;
			AvcDecodeDetail->AvcFrameStatus = SceMpegData->AvcFrameStatus;

			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x4571CC64, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeFlush(SceMpegPointer* Mpeg)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegGetAvcAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">associated stream</param>
		/// <param name="MpegAccessUnit">will contain pointer to Au</param>
		/// <param name="DataAttributes">unknown</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xFE246728, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegGetAvcAu(SceMpegPointer* Mpeg, StreamId StreamId, SceMpegAu* MpegAccessUnit, int* DataAttributes)
		{
			CheckEnabledMpeg();

			if (DataAttributes != null)
			{
				*DataAttributes = 1;
			}

			throw(new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));

			//throw(new NotImplementedException());
			//return 0;
		}

		/// <summary>
		/// Allocates an elementary stream buffer.
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns>
		///		0 if error else a ElementaryStream id.
		/// </returns>
		[HlePspFunction(NID = 0xA780CF7E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegMallocAvcEsBuf(SceMpegPointer* Mpeg)
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
		/// sceMpegFreeAvcEsBuf
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="ElementaryStream">Value returned from sceMpegMallocAvcEsBuf</param>
		[HlePspFunction(NID = 0xCEB870B1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public uint sceMpegFreeAvcEsBuf(SceMpegPointer* Mpeg, int ElementaryStream)
		{
			AbvEsBufAllocated[ElementaryStream - 1] = false;
			return 0;
		}

		/// <summary>
		/// Sets the SceMpegAvcMode to a Mpeg
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="Mode">pointer to SceMpegAvcMode struct defining the decode mode (pixelformat)</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xA11C7026, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeMode(SceMpegPointer* Mpeg, SceMpegAvcMode* Mode)
		{
			CheckEnabledMpeg();

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
		/// <param name="MpegAccessUnit">Video Access Unit</param>
		/// <param name="FrameWidth">Output buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="OutputBuffer">Buffer that will contain the decoded frame</param>
		/// <param name="Init">Will be set to 0 on first call, then 1</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x0E3C2E9D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecode(SceMpegPointer* Mpeg, SceMpegAu* MpegAccessUnit, int FrameWidth, PspPointer* OutputBufferPointer, int* Init)
		{
			CheckEnabledMpeg();

			if (*Init == 1)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
			}
			var SceMpegData = GetSceMpegData(Mpeg);

			
			//Console.Error.WriteLine("0x{0:X}", PspMemory.PointerToPspAddress(OutputBuffer));

			var OutputBuffer = (byte*)PspMemory.PspAddressToPointerSafe(OutputBufferPointer->Address);

			for (int n = 0; n < FrameWidth * 272 * 4; n++)
			{
				OutputBuffer[n] = 0xFF;
			}

			SceMpegData->AvcFrameStatus = 1;
			*Init = SceMpegData->AvcFrameStatus;
			//throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecodeStop
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="FrameWidth">output buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="OutputBuffer">buffer that will contain the decoded frame</param>
		/// <param name="Status">frame number</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x740FCCD1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeStop(SceMpegPointer* Mpeg, int FrameWidth, byte* OutputBuffer, int* Status)
		{
			var SceMpegData = GetSceMpegData(Mpeg);

			//throw(new NotImplementedException());
			return 0;
		}
	}
}
