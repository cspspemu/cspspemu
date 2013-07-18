using cscodec.h264.player;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;
using System;

namespace CSPspEmu.Hle.Modules.mpeg
{
	/// <summary>
	/// AVC: Advanced Video Coding
	/// </summary>
	/// <see cref="http://en.wikipedia.org/wiki/H.264/MPEG-4_AVC"/>
	public unsafe partial class sceMpeg
	{
		protected bool[] AbvEsBufAllocated = new bool[2];

		/// <summary>
		/// sceMpegAvcDecodeDetail
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="AvcDecodeDetail">AvcDecodeDetail</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x0F6C18D7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeDetail(SceMpegPointer* Mpeg, AvcDecodeDetailStruct* AvcDecodeDetail)
		{
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
		/// sceMpegAvcDecodeFlush
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x4571CC64, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeFlush(SceMpegPointer* Mpeg)
		{
			var SceMpegData = GetSceMpegData(Mpeg);

			// Finish the Mpeg only if we are not at the start of a new video,
			// otherwise the analyzed video could be lost.
			if (SceMpegData->VideoFrameCount > 0 || SceMpegData->AudioFrameCount > 0)
			{
				_FinishMpeg(SceMpegData);
			}

			//throw(new NotImplementedException());
			return 0;
		}

		private void _FinishMpeg(SceMpeg* SceMpegData)
		{
			Console.WriteLine("_FinishMpeg");
		}

		/// <summary>
		/// Get Mpeg AVC Access Unit
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="StreamId">Associated stream</param>
		/// <param name="MpegAccessUnit">Will contain pointer to Au</param>
		/// <param name="DataAttributes">Unknown</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0xFE246728, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegGetAvcAu(SceMpegPointer* Mpeg, StreamId StreamId, SceMpegAu* MpegAccessUnit, int* DataAttributes)
		{
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
		///		0 if error, else a ElementaryStream ID.
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
		/// <param name="ElementaryStream">Value returned from <see cref="sceMpegMallocAvcEsBuf"/></param>
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
		/// <param name="Mode">Pointer to <see cref="SceMpegAvcMode"/> struct defining the decode mode (pixelformat)</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xA11C7026, FirmwareVersion = 150)]
		public int sceMpegAvcDecodeMode(SceMpegPointer* Mpeg, SceMpegAvcMode* Mode)
		{
			var SceMpegData = GetSceMpegData(Mpeg);

			if (Mode != null)
			{
				switch (Mode->PixelFormat)
				{
					case Core.Types.GuPixelFormats.RGBA_5650:
					case Core.Types.GuPixelFormats.RGBA_8888:
						SceMpegData->SceMpegAvcMode = *Mode;
						break;
					default:
						throw (new Exception("Invalid PixelFormat in sceMpegAvcDecodeMode: " + Mode->Mode + ", " + Mode->PixelFormat));
				}
			}

			return 0;
		}

		/// <summary>
		/// sceMpegAvcDecode
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="MpegAccessUnit">Video Access Unit</param>
		/// <param name="FrameWidth">Output buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="OutputBufferPointer">Buffer that will contain the decoded frame</param>
		/// <param name="Init">Will be set to 0 on first call, then 1</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x0E3C2E9D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecode(SceMpegPointer* Mpeg, SceMpegAu* MpegAccessUnit, int FrameWidth, PspPointer* OutputBufferPointer, int* Init)
		{
			if (*Init == 1)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_DATA));
			}
			var SceMpegData = GetSceMpegData(Mpeg);

			// Dummy
			var VideoPacket = new VideoPacket();
			
			//Console.Error.WriteLine("0x{0:X}", PspMemory.PointerToPspAddress(OutputBuffer));

			var OutputBuffer = (byte*)PspMemory.PspAddressToPointerSafe(OutputBufferPointer->Address);

			int TotalBytes = PixelFormatDecoder.GetPixelsSize(SceMpegData->SceMpegAvcMode.PixelFormat, FrameWidth * 272);
			for (int n = 0; n < TotalBytes; n++)
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
		/// <param name="FrameWidth">Output buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="OutputBuffer">Buffer that will contain the decoded frame</param>
		/// <param name="Status">Frame number</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x740FCCD1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAvcDecodeStop(SceMpegPointer* Mpeg, int FrameWidth, byte* OutputBuffer, int* Status)
		{
			var SceMpegData = GetSceMpegData(Mpeg);

			*Status = 0;

			//throw(new NotImplementedException());
			return 0;
		}
	}
}
