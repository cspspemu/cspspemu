using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Types;
using System;
using CSharpUtils.Streams;
using System.IO;
using CSPspEmu.Hle.Formats.video;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.mpeg
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceMpeg : HleModuleHost
	{
		/// <summary>
		/// MPEG AVC elementary stream.
		/// MPEG packet size.
		/// </summary>
		protected const int MPEG_AVC_ES_SIZE = 2048;

		/// <summary>
		/// MPEG ATRAC elementary stream.
		/// </summary>
		protected const int MPEG_ATRAC_ES_SIZE = 2112;

		/// <summary>
		/// 
		/// </summary>
		protected const int RingBufferPacketSize = 0x800;

		/// <summary>
		/// 
		/// </summary>
		public const int MPEG_ATRAC_ES_OUTPUT_SIZE = 8192;

		/// <summary>
		/// 
		/// </summary>
		[Inject]
		HleConfig HleConfig;

		/// <summary>
		/// 
		/// </summary>
		[Inject]
		HleInterop HleInterop;

		/// <summary>
		/// 
		/// </summary>
		private Mpeg __SingleInstance = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns></returns>
		private Mpeg GetMpeg(SceMpegPointer* Mpeg)
		{
			if (__SingleInstance == null) __SingleInstance = new Mpeg(InjectContext);
			return __SingleInstance;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SceMpeg"></param>
		/// <returns></returns>
		public SceMpeg* GetSceMpegData(SceMpegPointer* SceMpeg)
		{
			return SceMpeg->GetSceMpeg(Memory);
		}

		/// <summary>
		/// sceMpegInit
		/// </summary>
		/// <returns>0 if success.</returns>
		[HlePspFunction(NID = 0x682A619B, FirmwareVersion = 150)]
		public int sceMpegInit()
		{
			//throw (new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// sceMpegFinish
		/// </summary>
		[HlePspFunction(NID = 0x874624D6, FirmwareVersion = 150)]
		public int sceMpegFinish()
		{
			//throw (new NotImplementedException());
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
		public int sceMpegQueryMemSize(int Mode)
		{
			return sizeof(SceMpeg);
		}

		/// <summary>
		/// sceMpegCreate
		/// </summary>
		/// <param name="SceMpegPointer">Will be filled</param>
		/// <param name="MpegData">Pointer to allocated memory of size = sceMpegQueryMemSize()</param>
		/// <param name="MpegSize">Size of data, should be = sceMpegQueryMemSize()</param>
		/// <param name="SceMpegRingbuffer">A ringbuffer</param>
		/// <param name="FrameWidth">Display buffer width, set to 512 if writing to framebuffer</param>
		/// <param name="Mode">Unknown, set to 0</param>
		/// <param name="DdrTop">Unknown, set to 0</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0xD8C5F121, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegCreate(SceMpegPointer* SceMpegPointer, void* MpegData, int MpegSize, SceMpegRingbuffer* SceMpegRingbuffer, int FrameWidth, int Mode, int DdrTop)
		{
			//return -1;

			var Mpeg = GetMpeg(SceMpegPointer);

			if (MpegSize < sceMpegQueryMemSize(0))
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_MEMORY));
			}

			// Update the ring buffer struct.
			if (SceMpegRingbuffer->PacketSize == 0)
			{
				SceMpegRingbuffer->PacketsAvailable = 0;
			}
			else
			{
				SceMpegRingbuffer->PacketsAvailable = (int)((SceMpegRingbuffer->DataEnd.Address - SceMpegRingbuffer->Data.Address) / SceMpegRingbuffer->PacketSize);
			}

			SceMpegRingbuffer->SceMpeg = Memory.PointerToPspPointer(SceMpegPointer);

			SceMpeg* SceMpegData = (SceMpeg*)&((byte*)MpegData)[0x30];

			SceMpegPointer->SceMpeg = Memory.PointerToPspPointer(SceMpegData);

			PointerUtils.StoreStringOnPtr("LIBMPEG", Encoding.UTF8, SceMpegData->MagicBytes);
			PointerUtils.StoreStringOnPtr("001", Encoding.UTF8, SceMpegData->VersionBytes);
			SceMpegData->Pad = -1;
			SceMpegData->RingBufferAddress = Memory.PointerToPspPointer(SceMpegRingbuffer);
			SceMpegData->RingBufferAddressDataUpper = SceMpegRingbuffer->DataEnd;
			SceMpegData->FrameWidth = FrameWidth;
			SceMpegData->SceMpegAvcMode.Mode = -1;
			SceMpegData->SceMpegAvcMode.PixelFormat = GuPixelFormats.RGBA_8888;
			SceMpegData->VideoFrameCount = 0;
			SceMpegData->AudioFrameCount = 0;

			SceMpegRingbuffer->PacketsTotal = 0;

			Mpeg.ReadPackets = (int NumPackets) =>
			{
				return (int)HleInterop.ExecuteFunctionNow(SceMpegRingbuffer->Callback, SceMpegRingbuffer->Data, NumPackets, SceMpegRingbuffer->CallbackParameter);
			};

			Mpeg._Mpeg = SceMpegPointer;
			Mpeg.Data = SceMpegData;
			Mpeg.Create();

			return 0;
		}

		/// <summary>
		/// sceMpegDelete
		/// </summary>
		/// <param name="SceMpegPointer">SceMpeg handle</param>
	    [HlePspFunction(NID = 0x606A4649, FirmwareVersion = 150)]
		public int sceMpegDelete(SceMpegPointer* SceMpegPointer)
		{
			GetMpeg(SceMpegPointer).Delete();

			return 0;
		}

		/// <summary>
		/// Initializes a Mpeg Access Unit from an ElementaryStreamBuffer.
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="ElementaryStreamBuffer">Prevously allocated Es buffer</param>
		/// <param name="MpegAccessUnit">Will contain pointer to Au</param>
		/// <returns>0 if successful.</returns>
		/// <seealso cref="http://en.wikipedia.org/wiki/Presentation_and_access_units"/>
		[HlePspFunction(NID = 0x167AFD9E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegInitAu(SceMpegPointer* SceMpegPointer, int ElementaryStreamBuffer, out SceMpegAu MpegAccessUnit)
		{
			var Mpeg = GetMpeg(SceMpegPointer);
			MpegAccessUnit = default(SceMpegAu);
			MpegAccessUnit.EsBuffer = ElementaryStreamBuffer;

			if (ElementaryStreamBuffer >= 1 && ElementaryStreamBuffer <= AbvEsBufAllocated.Length && AbvEsBufAllocated[ElementaryStreamBuffer - 1])
			{
				MpegAccessUnit.AuSize = MPEG_AVC_ES_SIZE;
				Mpeg.AvcAu.SceMpegAu = MpegAccessUnit;
			}
			else
			{
				MpegAccessUnit.AuSize = MPEG_ATRAC_ES_SIZE;
				Mpeg.AtracAu.SceMpegAu = MpegAccessUnit;
			}

			return 0;
		}


		/// <summary>
		/// sceMpegRingbufferQueryMemSize
		/// </summary>
		/// <param name="NumberOfPackets">Number of packets in the ringbuffer</param>
		/// <returns>Less than 0 if error, else ringbuffer data size.</returns>
		[HlePspFunction(NID = 0xD7A29F46, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceMpegRingbufferQueryMemSize(int NumberOfPackets)
		{
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
		//[HlePspNotImplemented]
		public int sceMpegRingbufferConstruct(SceMpegRingbuffer* Ringbuffer, int Packets, PspPointer Data, int Size, PspPointer Callback, PspPointer CallbackParam)
		{
			Ringbuffer->PacketsTotal = Packets;
			Ringbuffer->PacketsRead = 0;
			Ringbuffer->PacketsWritten = 0;
			Ringbuffer->PacketsAvailable = 0; // set later
			Ringbuffer->PacketSize = RingBufferPacketSize;
			Ringbuffer->Data = Data;
			Ringbuffer->DataEnd = (uint)(Data + Ringbuffer->PacketsTotal * Ringbuffer->PacketSize);
			Ringbuffer->Callback = Callback;
			Ringbuffer->CallbackParameter = CallbackParam;
			Ringbuffer->SemaId = -1;
			Ringbuffer->SceMpeg = 0;

			if (Ringbuffer->DataEnd > Ringbuffer->Data + Size)
			{
				throw (new InvalidOperationException());
			}

			return 0;
		}

		/// <summary>
		/// sceMpegRingbufferDestruct
		/// </summary>
		/// <param name="Ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
		[HlePspFunction(NID = 0x13407F13, FirmwareVersion = 150)]
		//[HlePspNotImplemented]
		public int sceMpegRingbufferDestruct(SceMpegRingbuffer* Ringbuffer)
		{
			Ringbuffer->PacketsAvailable = Ringbuffer->PacketsTotal;
			Ringbuffer->PacketsRead = 0;
			Ringbuffer->PacketsWritten = 0;
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
		//[HlePspNotImplemented]
		public int sceMpegRingbufferAvailableSize(SceMpegRingbuffer* Ringbuffer)
		{
			return Ringbuffer->PacketsAvailable;
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
		//[HlePspNotImplemented]
		public int sceMpegRingbufferPut(SceMpegRingbuffer* Ringbuffer, int NumPackets, int Available)
		{
			if (NumPackets < 0) return 0;

			NumPackets = Math.Min(Available, NumPackets);

			var SceMpegPointer = (SceMpegPointer*)Ringbuffer->SceMpeg.GetPointer<SceMpegPointer>(Memory);
			var Mpeg = GetMpeg(SceMpegPointer);
			var SceMpeg = SceMpegPointer->GetSceMpeg(Memory);
			var MpegStreamPackets = (int)MathUtils.RequiredBlocks(SceMpeg->StreamSize, Ringbuffer->PacketSize);
			var RemainingPackets = Math.Max(0, MpegStreamPackets - Ringbuffer->PacketsRead);

			var PacketsAdded = Mpeg.ReadPackets(NumPackets);
			var DataLength = (int)(PacketsAdded * Ringbuffer->PacketSize);
			Mpeg.WriteData(Ringbuffer->Data.GetPointer(Memory, DataLength), DataLength);

			//
			//NumPackets = Math.Min(NumPackets, RemainingPackets);
			//
			//var PacketsAdded = (int)HleInterop.ExecuteFunctionNow(Ringbuffer->Callback, Ringbuffer->Data, NumPackets, Ringbuffer->CallbackParameter);
			//
			//if (PacketsAdded > 0)
			//{
			//	var DataLength = (int)(PacketsAdded * Ringbuffer->PacketSize);
			//	var DataPointer = Ringbuffer->Data.GetPointer(Memory, DataLength);
			//
			//	Mpeg.WriteData(DataPointer, DataLength);
			//
			//	//if (PacketsAdded > Ringbuffer->PacketsFree)
			//	//{
			//	//	PacketsAdded = Ringbuffer->PacketsFree;
			//	//}
			//
			//	//Ringbuffer->PacketsFree -= packetsAdded;
			//	//Ringbuffer->Data.Address += (uint)(Ringbuffer->PacketSize * packetsAdded);
			//	
			//	//throw(new NotImplementedException());
			//	Console.Error.WriteLine("sceMpegRingbufferPut.NotImplemented");
			//}
			//
			////Ringbuffer->PacketsFree -= NumPackets;
			////Ringbuffer->PacketsWritten += NumPackets;

			return PacketsAdded;
		}

		public class StreamInfo : IDisposable
		{
			public StreamId StreamId;
			public int StreamIndex;

			void IDisposable.Dispose()
			{
			}
		}

		HleUidPoolSpecial<StreamInfo, int> RegisteredStreams = new HleUidPoolSpecial<StreamInfo, int>(FirstId: 0x17);

		/// <summary>
		/// sceMpegUnRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="StreamInfoId">Pointer to stream</param>
		[HlePspFunction(NID = 0x591A4AA2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public void sceMpegUnRegistStream(SceMpegPointer* Mpeg, int StreamInfoId)
		{
			RegisteredStreams.Remove(StreamInfoId);
			//throw(new NotImplementedException());
		}

		/// <summary>
		/// sceMpegRegistStream
		/// </summary>
		/// <param name="Mpeg">SceMpeg handle</param>
		/// <param name="StreamId">Stream ID, 0 for video, 1 for audio</param>
		/// <param name="StreamIndex">Unknown, set to 0</param>
		/// <returns>The ID, 0 on error.</returns>
		[HlePspFunction(NID = 0x42560F23, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		//public SceMpegStream* sceMpegRegistStream(SceMpeg* Mpeg, int iStreamID, int iUnk)
		public int sceMpegRegistStream(SceMpegPointer* Mpeg, StreamId StreamId, int StreamIndex)
		{
			var StreamInfoId = RegisteredStreams.Create(new StreamInfo()
			{
				StreamId = StreamId,
				StreamIndex = StreamIndex,
			});
			//Console.WriteLine(iStreamID);
			//return 0;

			//var SceMpegData = GetSceMpegData(Mpeg);

			//throw(new NotImplementedException());
			return StreamInfoId;
		}

		/// <summary>
		/// sceMpegQueryStreamOffset
		/// </summary>
		/// <param name="MpegPointer">SceMpeg handle</param>
		/// <param name="PmfHeader">Pointer to file header</param>
		/// <param name="Offset">Will contain the stream offset in bytes, usually 2048</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x21FF80E4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamOffset(SceMpegPointer* MpegPointer, byte* PmfHeader, out uint Offset)
		{
			var Pmf = new Pmf().Load(new MemoryStream(PointerUtils.PointerToByteArray(PmfHeader, 2048)));

			var Mpeg = GetMpeg(MpegPointer);
			var SceMpeg = MpegPointer->GetSceMpeg(Memory);

			Mpeg.ParsePmfHeader(PmfHeader);
			SceMpeg->StreamSize = (int)(uint)Pmf.Header.StreamSize;

			Offset = (uint)Pmf.Header.StreamOffset;
			return 0;
		}

		/// <summary>
		/// sceMpegQueryStreamSize
		/// </summary>
		/// <param name="PmfHeader">Pointer to file header</param>
		/// <param name="Size">Will contain stream size in bytes</param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x611E9E11, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegQueryStreamSize(byte* PmfHeader, out uint Size)
		{
			var Pmf = new Pmf().Load(new MemoryStream(PointerUtils.PointerToByteArray(PmfHeader, 2048)));
			Size = Pmf.Header.StreamSize;
			//*Size = 0;
			return 0;
		}


		/// <summary>
		/// sceMpegFlushAllStreams
		/// </summary>
		/// <param name="Mpeg"></param>
		/// <returns>0 if successful.</returns>
		[HlePspFunction(NID = 0x707B7629, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceMpegFlushAllStream(SceMpegPointer* SceMpegPointer)
		{
			var Mpeg = GetMpeg(SceMpegPointer);
			Mpeg.FlushAllStream();
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
