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
		/// <param name="NumberOfPackets">number of packets in the ringbuffer</param>
		/// <returns>Less than 0 if error else ringbuffer data size.</returns>
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
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
		/// <param name="Packets">number of packets in the ringbuffer</param>
		/// <param name="Data">pointer to allocated memory</param>
		/// <param name="Size">size of allocated memory, shoud be sceMpegRingbufferQueryMemSize(iPackets)</param>
		/// <param name="Callback">ringbuffer callback</param>
		/// <param name="CallbackParam">param passed to callback</param>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x37295ED8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferConstruct(SceMpegRingbuffer* Ringbuffer, int Packets, PspPointer Data, int Size, PspPointer Callback, PspPointer CallbackParam)
		{
			//return -1;
			//throw(new NotImplementedException());
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
			if (Ringbuffer->PacketsFree > 0) Ringbuffer->PacketsFree--;
			//if (Ringbuffer->avai > 0) Ringbuffer->PacketsFree--;
			return Ringbuffer->PacketsFree;
		}

		/// <summary>
		/// sceMpegRingbufferPut
		/// </summary>
		/// <param name="Ringbuffer">pointer to a sceMpegRingbuffer struct</param>
		/// <param name="NumPackets">num packets to put into the ringbuffer</param>
		/// <param name="Available">free packets in the ringbuffer, should be sceMpegRingbufferAvailableSize()</param>
		/// <returns>
		///		Less than 0 if error else number of packets.
		/// </returns>
		[HlePspFunction(NID = 0xB240A59E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegRingbufferPut(SceMpegRingbuffer* Ringbuffer, int NumPackets, int Available)
		{
			//throw(new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_MEMORY));

			if (NumPackets < 0)
			{
				return 0;
			}

			NumPackets = Math.Min(Available, NumPackets);

			var SceMpegPointer = (SceMpegPointer *)Ringbuffer->SceMpeg.GetPointer<SceMpegPointer>(PspMemory);
			var SceMpeg = SceMpegPointer->GetSceMpeg(PspMemory);
			var MpegStreamPackets = (int)MathUtils.RequiredBlocks(SceMpeg->StreamSize, Ringbuffer->PacketSize);
			var RemainingPackets = Math.Max(0, MpegStreamPackets - Ringbuffer->PacketsRead);

			NumPackets = Math.Min(NumPackets, RemainingPackets);

			//Ringbuffer->Data
			var Result = (int)HleInterop.ExecuteFunctionNow(
				// Functions
				Ringbuffer->Callback,
				// Arguments
				Ringbuffer->Data,
				NumPackets,
				Ringbuffer->CallbackParameter
			);

			//Ringbuffer->PacketsFree -= NumPackets;
			//Ringbuffer->PacketsWritten += NumPackets;

			return Result;
		}
	}
}
