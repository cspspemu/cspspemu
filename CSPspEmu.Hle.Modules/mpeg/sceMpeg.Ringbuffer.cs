using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

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
			//Console.WriteLine("{0}:{1}:{2}", 0x868, NumberOfPackets, 0x868 * NumberOfPackets);
			return RingBufferPacketSize * NumberOfPackets;
		}

		/// <summary>
		/// sceMpegRingbufferConstruct
		/// </summary>
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
		/// <param name="Packets">number of packets in the ringbuffer</param>
		/// <param name="Data">pointer to allocated memory</param>
		/// <param name="Size">size of allocated memory, shoud be sceMpegRingbufferQueryMemSize(iPackets)</param>
		/// <param name="Callback">ringbuffer callback</param>
		/// <param name="CallbackParam">param passed to callback</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x37295ED8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferConstruct(SceMpegRingbuffer* Ringbuffer, int Packets, uint Data, int Size, uint/*sceMpegRingbufferCB*/ Callback, uint CallbackParam)
		{
			//return -1;
			//throw(new NotImplementedException());
			Ringbuffer->Packets = Packets;
			Ringbuffer->PacketsRead = 0;
			Ringbuffer->PacketsWritten = 0;
			Ringbuffer->PacketsFree = 0; // set later
			Ringbuffer->PacketSize = 2048;
			Ringbuffer->Data = Data;
			Ringbuffer->DataUpperBound = (uint)(Data + Ringbuffer->Packets * Ringbuffer->PacketSize);
			Ringbuffer->Callback = Callback;
			Ringbuffer->CallbackParameter = CallbackParam;
			Ringbuffer->SemaId = -1;
			Ringbuffer->SceMpeg = 0;

			if (Ringbuffer->DataUpperBound > Ringbuffer->Data + Size)
			{
				throw(new InvalidOperationException());
			}

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
		public int sceMpegRingbufferAvailableSize(SceMpegRingbuffer* Ringbuffer)
		{
			return Ringbuffer->PacketsFree;
		}

		/// <summary>
		/// sceMpegRingbufferPut
		/// </summary>
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
		/// <param name="NumPackets">num packets to put into the ringbuffer</param>
		/// <param name="PacketsFree">free packets in the ringbuffer, should be sceMpegRingbufferAvailableSize()</param>
		/// <returns>
		///		Less than 0 if error else number of packets.
		/// </returns>
		[HlePspFunction(NID = 0xB240A59E, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceMpegRingbufferPut(SceMpegRingbuffer* Ringbuffer, int NumPackets, int PacketsFree)
		{
			//Ringbuffer->Data
			var ResultCpuThreadState = HleState.HleInterop.ExecuteFunctionNow(
				Ringbuffer->Callback,
				Ringbuffer->Data,
				NumPackets,
				Ringbuffer->CallbackParameter
			);
			//throw(new NotImplementedException());
			//return 1;
			//return -1;
			return (int)ResultCpuThreadState;
		}
	}
}
