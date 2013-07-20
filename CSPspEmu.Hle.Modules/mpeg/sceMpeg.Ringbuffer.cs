using System;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.mpeg
{
	public unsafe partial class sceMpeg
	{
		[Inject]
		HleInterop HleInterop;

		/// <summary>
		/// sceMpegRingbufferQueryMemSize
		/// </summary>
		/// <param name="NumberOfPackets">Number of packets in the ringbuffer</param>
		/// <returns>Less than 0 if error, else ringbuffer data size.</returns>
		[HlePspFunction(NID = 0xD7A29F46, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferQueryMemSize(int NumberOfPackets)
		{
			//Console.WriteLine("{0}:{1}:{2}", 0x868, NumberOfPackets, 0x868 * NumberOfPackets);
			return (RingBufferPacketSize + 0x68) * NumberOfPackets;
		}

		/// <summary>
		/// sceMpegRingbufferConstruct
		/// </summary>
		/// <param name="Ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
		/// <param name="Packets">Number of packets in the ringbuffer</param>
		/// <param name="Data">Pointer to allocated memory</param>
		/// <param name="Size">Size of allocated memory, shoud be sceMpegRingbufferQueryMemSize(iPackets)</param>
		/// <param name="Callback">Ringbuffer callback</param>
		/// <param name="CallbackParam">Param passed to callback</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x37295ED8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferConstruct(SceMpegRingbuffer* Ringbuffer, int Packets, PspPointer Data, int Size, PspPointer Callback, PspPointer CallbackParam)
		{
			Ringbuffer->Packets = Packets;
			Ringbuffer->PacketsRead = 0;
			Ringbuffer->PacketsWritten = 0;
			Ringbuffer->PacketsFree = 0; // set later
			Ringbuffer->PacketSize = RingBufferPacketSize;
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
		/// <param name="Ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
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
		/// <param name="Ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
		/// <returns>
		///		Less than 0 if error, else number of free packets in the ringbuffer.
		/// </returns>
		[HlePspFunction(NID = 0xB5F6DC87, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferAvailableSize(SceMpegRingbuffer* Ringbuffer)
		{
			return Ringbuffer->PacketsFree;
		}

		/// <summary>
		/// sceMpegRingbufferPut
		/// </summary>
		/// <param name="Ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
		/// <param name="NumPackets">Num packets to put into the ringbuffer</param>
		/// <param name="Available">Free packets in the ringbuffer, should be sceMpegRingbufferAvailableSize()</param>
		/// <returns>
		///		Less than 0 if error, else number of packets.
		/// </returns>
		[HlePspFunction(NID = 0xB240A59E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferPut(SceMpegRingbuffer* Ringbuffer, int NumPackets, int Available)
		{
			if (NumPackets < 0) return 0;

			NumPackets = Math.Min(Available, NumPackets);

			var SceMpegPointer = (SceMpegPointer *)Ringbuffer->SceMpeg.GetPointer<SceMpegPointer>(Memory);
			var Mpeg = GetMpeg(SceMpegPointer);
			var SceMpeg = SceMpegPointer->GetSceMpeg(Memory);
			var MpegStreamPackets = (int)MathUtils.RequiredBlocks(SceMpeg->StreamSize, Ringbuffer->PacketSize);
			var RemainingPackets = Math.Max(0, MpegStreamPackets - Ringbuffer->PacketsRead);

			NumPackets = Math.Min(NumPackets, RemainingPackets);

			//Ringbuffer->Data
			var packetsAdded = (int)HleInterop.ExecuteFunctionNow(
				// Functions
				Ringbuffer->Callback,
				// Arguments
				Ringbuffer->Data,
				NumPackets,
				Ringbuffer->CallbackParameter
			);

			if (packetsAdded > 0)
			{
				var addr = Ringbuffer->Data.GetPointer(Memory, 0);
				var length = packetsAdded * Ringbuffer->PacketSize;
				
				if (length > 0)
				{
					//if (checkMediaEngineState())
					//{
					//	if (meChannel == null)
					//	{
					//		// If no MPEG header has been provided by the application (and none could be found),
					//		// just use the MPEG stream as it is, without header analysis.
					//		me.init(addr, Math.max(length, mpegStreamSize), 0);
					//		meChannel = new PacketChannel();
					//	}
					//	meChannel.write(addr, length);
					//}
					//else if (isEnableConnector())
					//{
					//	mpegCodec.writeVideo(addr, length);
					//}
				}

				if (packetsAdded > Ringbuffer->PacketsFree)
				{
					packetsAdded = Ringbuffer->PacketsFree;
				}
				//mpegRingbuffer.addPackets(packetsAdded);
				//mpegRingbuffer.write(mpegRingbufferAddr);
				//throw(new NotImplementedException());
				Console.Error.WriteLine("sceMpegRingbufferPut.NotImplemented");
			}

			//Ringbuffer->PacketsFree -= NumPackets;
			//Ringbuffer->PacketsWritten += NumPackets;

			return packetsAdded;
		}
	}
}
