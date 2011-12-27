using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.mpeg
{
	unsafe public partial class sceMpeg
	{
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
	}
}
