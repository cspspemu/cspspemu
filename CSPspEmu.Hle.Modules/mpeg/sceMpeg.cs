using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.mpeg
{
	unsafe public partial class sceMpeg : HleModuleHost
	{
		/// <summary>
		/// sceMpegInit
		/// </summary>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x682A619B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegInit()
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegFinish
		/// </summary>
		[HlePspFunction(NID = 0x874624D6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegFinish()
		{
			//throw (new NotImplementedException());
		}

		/// <summary>
		/// sceMpegCreate
		/// </summary>
		/// <param name="Mpeg">will be filled</param>
		/// <param name="MpegData">pointer to allocated memory of size = sceMpegQueryMemSize()</param>
		/// <param name="MpegSize">size of data, should be = sceMpegQueryMemSize()</param>
		/// <param name="SceMpegRingbuffer">a ringbuffer</param>
		/// <param name="FrameWidth">display buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="Mode">unknown, set to 0</param>
		/// <param name="DdrTop">unknown, set to 0</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xD8C5F121, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegCreate(SceMpeg** Mpeg, void* MpegData, int MpegSize, SceMpegRingbuffer* SceMpegRingbuffer, int FrameWidth, int Mode, int DdrTop)
		{
			if (MpegSize < sceMpegQueryMemSize(0))
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_MEMORY));
			}

			// Update the ring buffer struct.
			if (SceMpegRingbuffer->PacketSize == 0)
			{
				SceMpegRingbuffer->PacketsFree = 0;
			}
			else
			{
				SceMpegRingbuffer->PacketsFree = (uint)(SceMpegRingbuffer->DataUpperBound.Address - SceMpegRingbuffer->Data.Address) / SceMpegRingbuffer->PacketSize;
			}
			//SceMpegRingbuffer->SceMpeg = SceMpeg;

			SceMpeg* SceMpegData = (SceMpeg *)&((byte *)MpegData)[0x30];

			PointerUtils.StoreStringOnPtr("LIBMPEG.001", Encoding.UTF8, SceMpegData->MagicBytes);
			SceMpegData->Unknown1 = -1;
			SceMpegData->RingBufferAddress = HleState.CpuProcessor.Memory.PointerToPspPointer(SceMpegRingbuffer);
			SceMpegData->RingBufferAddressDataUpper = SceMpegRingbuffer->DataUpperBound;

			// Write mpeg system handle.
			/*
			mpegHandle = data + 0x30;
			mem.write32(mpeg, mpegHandle);
			// Initialise fake mpeg struct.
			Utilities.writeStringZ(mem, mpegHandle, "LIBMPEG.001");
			mem.write32(mpegHandle + 12, -1);
			mem.write32(mpegHandle + 16, ringbuffer_addr);
			mem.write32(mpegHandle + 20, ringbuffer.dataUpperBound);
			// Initialise mpeg values.
			mpegRingbufferAddr = ringbuffer_addr;
			mpegRingbuffer = ringbuffer;
			videoFrameCount = 0;
			audioFrameCount = 0;
			videoPixelMode = PSP_DISPLAY_PIXEL_FORMAT_8888;
			defaultFrameWidth = frameWidth;
			*/

			//throw (new NotImplementedException());
			//Ringbuffer.iPackets = 0;
			return 0;
		}

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
		/// sceMpegQueryAtracEsSize
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="iEsSize">will contain size of Es</param>
		/// <param name="iOutSize">will contain size of decoded data</param>
		/// <returns>0 if success.</returns>
	    [HlePspFunction(NID = 0xF8DCB679, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryAtracEsSize(SceMpeg* Mpeg, int* iEsSize, int* iOutSize)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegGetAtracAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">associated stream</param>
		/// <param name="pAu">will contain pointer to Au</param>
		/// <param name="pUnk">unknown</param>
		/// <returns>0 if success.</returns>
	    [HlePspFunction(NID = 0xE1CE83A7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegGetAtracAu(SceMpeg* Mpeg, SceMpegStream* pStream, SceMpegAu* pAu, void* pUnk)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegFreeAvcEsBuf
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <param name="pBuf"></param>
	    [HlePspFunction(NID = 0xCEB870B1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegFreeAvcEsBuf(SceMpeg* Mpeg, void* pBuf)
		{
			//throw(new NotImplementedException());
			return;
		}

		/// <summary>
		/// sceMpegQueryMemSize
		/// </summary>
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
		/// <returns>
		///		Less than 0 if error else number of free packets in the ringbuffer.
		/// </returns>
	    [HlePspFunction(NID = 0xB5F6DC87, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferAvailableSize(SceMpegRingbuffer* Ringbuffer)
		{
			//throw(new NotImplementedException());
			//return -1;
			return 0;
		}

		/// <summary>
		/// sceMpegRingbufferPut
		/// </summary>
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
		/// <param name="iNumPackets">num packets to put into the ringbuffer</param>
		/// <param name="iAvailable">free packets in the ringbuffer, should be sceMpegRingbufferAvailableSize()</param>
		/// <returns>
		///		Less than 0 if error else number of packets.
		/// </returns>
	    [HlePspFunction(NID = 0xB240A59E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferPut(SceMpegRingbuffer* Ringbuffer, int iNumPackets, int iAvailable)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegMallocAvcEsBuf
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns>
		///		0 if error else pointer to buffer.
		/// </returns>
	    [HlePspFunction(NID = 0xA780CF7E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void* sceMpegMallocAvcEsBuf(SceMpeg* Mpeg)
		{
			//throw(new NotImplementedException());
			return null;
		}

		/// <summary>
		/// sceMpegAtracDecode
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pAu">video Au</param>
		/// <param name="pBuffer">buffer that will contain the decoded frame</param>
		/// <param name="iInit">set this to 1 on first call</param>
		/// <returns>
		///		0 if success.
		/// </returns>
	    [HlePspFunction(NID = 0x800C44DF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegAtracDecode(SceMpeg* Mpeg, SceMpegAu* pAu, void* pBuffer, int iInit)
		{
			//throw(new NotImplementedException());
			return -1;
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
		/// sceMpegFlushAllStreams
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns>0 if success.</returns>
	    [HlePspFunction(NID = 0x707B7629, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegFlushAllStream(SceMpeg* Mpeg)
		{
			//throw(new NotImplementedException());
			return -1;
		}

		/// <summary>
		/// sceMpegQueryStreamSize
		/// </summary>
		/// <param name="pBuffer">pointer to file header</param>
		/// <param name="iSize">will contain stream size in bytes</param>
		/// <returns>0 if success.</returns>
	    [HlePspFunction(NID = 0x611E9E11, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamSize(void* pBuffer, int* iSize)
		{
			//throw(new NotImplementedException());

			*iSize = 0;

			return -1;
		}

		/// <summary>
		/// sceMpegDelete
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
	    [HlePspFunction(NID = 0x606A4649, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegDelete(SceMpeg* Mpeg)
		{
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// sceMpegUnRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pStream">pointer to stream</param>
	    [HlePspFunction(NID = 0x591A4AA2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegUnRegistStream(SceMpeg Mpeg, SceMpegStream* pStream)
		{
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// sceMpegRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="iStreamID">stream id, 0 for video, 1 for audio</param>
		/// <param name="iUnk">unknown, set to 0</param>
		/// <returns>0 if error.</returns>
	    [HlePspFunction(NID = 0x42560F23, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public SceMpegStream* sceMpegRegistStream(SceMpeg* Mpeg, int iStreamID, int iUnk)
		{
			//throw(new NotImplementedException());
			return null;
		}

		/// <summary>
		/// sceMpegRingbufferDestruct
		/// </summary>
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
	    [HlePspFunction(NID = 0x13407F13, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferDestruct(SceMpegRingbuffer* Ringbuffer)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegInitAu
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pEsBuffer">prevously allocated Es buffer</param>
		/// <param name="pAu">will contain pointer to Au</param>
		/// <returns>0 if success.</returns>
	    [HlePspFunction(NID = 0x167AFD9E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegInitAu(SceMpeg* Mpeg, void* pEsBuffer, SceMpegAu* pAu)
		{
			//throw(new NotImplementedException());
			return -1;
		}

		/// <summary>
		/// sceMpegQueryStreamOffset
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="pBuffer">pointer to file header</param>
		/// <param name="iOffset">will contain stream offset in bytes, usually 2048</param>
		/// <returns>0 if success.</returns>
	    [HlePspFunction(NID = 0x21FF80E4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamOffset(SceMpeg* Mpeg, void* pBuffer, int* iOffset)
		{
			//throw(new NotImplementedException());
			*iOffset = 0;
			return 0;
		}

		/// <summary>
		/// sceMpegRingbufferQueryMemSize
		/// </summary>
		/// <param name="NumberOfPackets">number of packets in the ringbuffer</param>
		/// <returns>Less than 0 if error else ringbuffer data size.</returns>
		[HlePspFunction(NID = 0xD7A29F46, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferQueryMemSize(int NumberOfPackets)
		{
			return 0x868 * NumberOfPackets;
		}

		/// <summary>
		/// sceMpegRingbufferConstruct
		/// </summary>
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
		/// <param name="iPackets">number of packets in the ringbuffer</param>
		/// <param name="pData">pointer to allocated memory</param>
		/// <param name="iSize">size of allocated memory, shoud be sceMpegRingbufferQueryMemSize(iPackets)</param>
		/// <param name="Callback">ringbuffer callback</param>
		/// <param name="pCBparam">param passed to callback</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x37295ED8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferConstruct(SceMpegRingbuffer* Ringbuffer, int iPackets, void* pData, int iSize, int/*sceMpegRingbufferCB*/ Callback, void* pCBparam)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegQueryMemSize
		/// </summary>
		/// <param name="Mode">Unknown, set to 0</param>
		/// <returns>
		///		Less than 0 if error else decoder data size.
		/// </returns>
		[HlePspFunction(NID = 0xC132E22F, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceMpegQueryMemSize(int Mode)
		{
			//return sizeof(SceMpeg);
			return 0x10000;
		}

		/// <summary>
		/// sceMpegAvcDecodeMode
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="Mode">pointer to SceMpegAvcMode struct defining the decode mode (pixelformat)</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0xA11C7026, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceMpegAvcDecodeMode(SceMpeg* Mpeg, SceMpegAvcMode* Mode)
		{
			if (Mode != null)
			{
				Mpeg->SceMpegAvcMode = *Mode;
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
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
