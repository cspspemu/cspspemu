using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Types;
using System;
using System.IO;
using CSPspEmu.Hle.Formats.video;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Interop;
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
        [Inject] HleConfig HleConfig;

        /// <summary>
        /// 
        /// </summary>
        [Inject] HleInterop HleInterop;

        /// <summary>
        /// 
        /// </summary>
        private Mpeg __SingleInstance = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mpeg"></param>
        /// <returns></returns>
        private Mpeg GetMpeg(SceMpegPointer* mpeg)
        {
            if (__SingleInstance == null) __SingleInstance = new Mpeg(InjectContext);
            return __SingleInstance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceMpeg"></param>
        /// <returns></returns>
        public SceMpeg* GetSceMpegData(SceMpegPointer* sceMpeg)
        {
            return sceMpeg->GetSceMpeg(Memory);
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
        /// <param name="mode">Unknown, set to 0</param>
        /// <returns>
        ///		Less than 0 if error else decoder data size.
        /// </returns>
        [HlePspFunction(NID = 0xC132E22F, FirmwareVersion = 150)]
        public int sceMpegQueryMemSize(int mode)
        {
            return sizeof(SceMpeg);
        }

        /// <summary>
        /// sceMpegCreate
        /// </summary>
        /// <param name="sceMpegPointer">Will be filled</param>
        /// <param name="mpegData">Pointer to allocated memory of size = sceMpegQueryMemSize()</param>
        /// <param name="mpegSize">Size of data, should be = sceMpegQueryMemSize()</param>
        /// <param name="sceMpegRingbuffer">A ringbuffer</param>
        /// <param name="frameWidth">Display buffer width, set to 512 if writing to framebuffer</param>
        /// <param name="mode">Unknown, set to 0</param>
        /// <param name="ddrTop">Unknown, set to 0</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0xD8C5F121, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegCreate(SceMpegPointer* sceMpegPointer, void* mpegData, int mpegSize,
            SceMpegRingbuffer* sceMpegRingbuffer, int frameWidth, int mode, int ddrTop)
        {
            //return -1;

            var mpeg = GetMpeg(sceMpegPointer);

            if (mpegSize < sceMpegQueryMemSize(0))
            {
                throw (new SceKernelException(SceKernelErrors.ERROR_MPEG_NO_MEMORY));
            }

            // Update the ring buffer struct.
            if (sceMpegRingbuffer->PacketSize == 0)
            {
                sceMpegRingbuffer->PacketsAvailable = 0;
            }
            else
            {
                sceMpegRingbuffer->PacketsAvailable =
                    (int) ((sceMpegRingbuffer->DataEnd.Address - sceMpegRingbuffer->Data.Address) /
                           sceMpegRingbuffer->PacketSize);
            }

            sceMpegRingbuffer->SceMpeg = Memory.PointerToPspPointer(sceMpegPointer);

            SceMpeg* sceMpegData = (SceMpeg*) (((byte*) mpegData) + 0x30);

            sceMpegPointer->SceMpeg = Memory.PointerToPspPointer(sceMpegData);

            PointerUtils.StoreStringOnPtr("LIBMPEG", Encoding.UTF8, sceMpegData->MagicBytes);
            PointerUtils.StoreStringOnPtr("001", Encoding.UTF8, sceMpegData->VersionBytes);
            sceMpegData->Pad = -1;
            sceMpegData->RingBufferAddress = Memory.PointerToPspPointer(sceMpegRingbuffer);
            sceMpegData->RingBufferAddressDataUpper = sceMpegRingbuffer->DataEnd;
            sceMpegData->FrameWidth = frameWidth;
            sceMpegData->SceMpegAvcMode.Mode = -1;
            sceMpegData->SceMpegAvcMode.PixelFormat = GuPixelFormats.Rgba8888;
            sceMpegData->VideoFrameCount = 0;
            sceMpegData->AudioFrameCount = 0;

            sceMpegRingbuffer->PacketsTotal = 0;

            mpeg.ReadPackets = numPackets => (int) HleInterop.ExecuteFunctionNow(sceMpegRingbuffer->Callback,
                sceMpegRingbuffer->Data,
                numPackets, sceMpegRingbuffer->CallbackParameter);

            mpeg._Mpeg = sceMpegPointer;
            mpeg.Data = sceMpegData;
            mpeg.Create();

            return 0;
        }

        /// <summary>
        /// sceMpegDelete
        /// </summary>
        /// <param name="sceMpegPointer">SceMpeg handle</param>
        [HlePspFunction(NID = 0x606A4649, FirmwareVersion = 150)]
        public int sceMpegDelete(SceMpegPointer* sceMpegPointer)
        {
            GetMpeg(sceMpegPointer).Delete();

            return 0;
        }

        /// <summary>
        /// Initializes a Mpeg Access Unit from an ElementaryStreamBuffer.
        /// </summary>
        /// <param name="sceMpegPointer"></param>
        /// <param name="elementaryStreamBuffer">Prevously allocated Es buffer</param>
        /// <param name="mpegAccessUnit">Will contain pointer to Au</param>
        /// <returns>0 if successful.</returns>
        /// <seealso cref="http://en.wikipedia.org/wiki/Presentation_and_access_units"/>
        [HlePspFunction(NID = 0x167AFD9E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegInitAu(SceMpegPointer* sceMpegPointer, int elementaryStreamBuffer,
            out SceMpegAu mpegAccessUnit)
        {
            var mpeg = GetMpeg(sceMpegPointer);
            mpegAccessUnit = default(SceMpegAu);
            mpegAccessUnit.EsBuffer = elementaryStreamBuffer;

            if (elementaryStreamBuffer >= 1 && elementaryStreamBuffer <= AbvEsBufAllocated.Length &&
                AbvEsBufAllocated[elementaryStreamBuffer - 1])
            {
                mpegAccessUnit.AuSize = MPEG_AVC_ES_SIZE;
                mpeg.AvcAu.SceMpegAu = mpegAccessUnit;
            }
            else
            {
                mpegAccessUnit.AuSize = MPEG_ATRAC_ES_SIZE;
                mpeg.AtracAu.SceMpegAu = mpegAccessUnit;
            }

            return 0;
        }


        /// <summary>
        /// sceMpegRingbufferQueryMemSize
        /// </summary>
        /// <param name="numberOfPackets">Number of packets in the ringbuffer</param>
        /// <returns>Less than 0 if error, else ringbuffer data size.</returns>
        [HlePspFunction(NID = 0xD7A29F46, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceMpegRingbufferQueryMemSize(int numberOfPackets)
        {
            return (RingBufferPacketSize + 0x68) * numberOfPackets;
        }

        /// <summary>
        /// sceMpegRingbufferConstruct
        /// </summary>
        /// <param name="ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
        /// <param name="packets">Number of packets in the ringbuffer</param>
        /// <param name="data">Pointer to allocated memory</param>
        /// <param name="size">Size of allocated memory, shoud be sceMpegRingbufferQueryMemSize(iPackets)</param>
        /// <param name="callback">Ringbuffer callback</param>
        /// <param name="callbackParam">Param passed to callback</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x37295ED8, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceMpegRingbufferConstruct(SceMpegRingbuffer* ringbuffer, int packets, PspPointer data, int size,
            PspPointer callback, PspPointer callbackParam)
        {
            ringbuffer->PacketsTotal = packets;
            ringbuffer->PacketsRead = 0;
            ringbuffer->PacketsWritten = 0;
            ringbuffer->PacketsAvailable = 0; // set later
            ringbuffer->PacketSize = RingBufferPacketSize;
            ringbuffer->Data = data;
            ringbuffer->DataEnd = (uint) (data + ringbuffer->PacketsTotal * ringbuffer->PacketSize);
            ringbuffer->Callback = callback;
            ringbuffer->CallbackParameter = callbackParam;
            ringbuffer->SemaId = -1;
            ringbuffer->SceMpeg = 0;

            if (ringbuffer->DataEnd > ringbuffer->Data + size)
            {
                throw (new InvalidOperationException());
            }

            return 0;
        }

        /// <summary>
        /// sceMpegRingbufferDestruct
        /// </summary>
        /// <param name="ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
        [HlePspFunction(NID = 0x13407F13, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceMpegRingbufferDestruct(SceMpegRingbuffer* ringbuffer)
        {
            ringbuffer->PacketsAvailable = ringbuffer->PacketsTotal;
            ringbuffer->PacketsRead = 0;
            ringbuffer->PacketsWritten = 0;
            return 0;
        }

        /// <summary>
        /// sceMpegQueryMemSize
        /// </summary>
        /// <param name="ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
        /// <returns>
        ///		Less than 0 if error, else number of free packets in the ringbuffer.
        /// </returns>
        [HlePspFunction(NID = 0xB5F6DC87, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceMpegRingbufferAvailableSize(SceMpegRingbuffer* ringbuffer)
        {
            return ringbuffer->PacketsAvailable;
        }

        /// <summary>
        /// sceMpegRingbufferPut
        /// </summary>
        /// <param name="ringbuffer">Pointer to a sceMpegRingbuffer struct</param>
        /// <param name="numPackets">Num packets to put into the ringbuffer</param>
        /// <param name="available">Free packets in the ringbuffer, should be sceMpegRingbufferAvailableSize()</param>
        /// <returns>
        ///		Less than 0 if error, else number of packets.
        /// </returns>
        [HlePspFunction(NID = 0xB240A59E, FirmwareVersion = 150)]
        //[HlePspNotImplemented]
        public int sceMpegRingbufferPut(SceMpegRingbuffer* ringbuffer, int numPackets, int available)
        {
            if (numPackets < 0) return 0;

            numPackets = Math.Min(available, numPackets);

            var sceMpegPointer = (SceMpegPointer*) ringbuffer->SceMpeg.GetPointer<SceMpegPointer>(Memory);
            var mpeg = GetMpeg(sceMpegPointer);
            var sceMpeg = sceMpegPointer->GetSceMpeg(Memory);
            var mpegStreamPackets = (int) MathUtils.RequiredBlocks(sceMpeg->StreamSize, ringbuffer->PacketSize);
            var remainingPackets = Math.Max(0, mpegStreamPackets - ringbuffer->PacketsRead);

            var packetsAdded = mpeg.ReadPackets(numPackets);
            var dataLength = (int) (packetsAdded * ringbuffer->PacketSize);
            mpeg.WriteData(ringbuffer->Data.GetPointer(Memory, dataLength), dataLength);

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

            return packetsAdded;
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
        /// <param name="mpeg">SceMpeg handle</param>
        /// <param name="streamInfoId">Pointer to stream</param>
        [HlePspFunction(NID = 0x591A4AA2, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public void sceMpegUnRegistStream(SceMpegPointer* mpeg, int streamInfoId)
        {
            RegisteredStreams.Remove(streamInfoId);
            //throw(new NotImplementedException());
        }

        /// <summary>
        /// sceMpegRegistStream
        /// </summary>
        /// <param name="Mpeg">SceMpeg handle</param>
        /// <param name="streamId">Stream ID, 0 for video, 1 for audio</param>
        /// <param name="streamIndex">Unknown, set to 0</param>
        /// <returns>The ID, 0 on error.</returns>
        [HlePspFunction(NID = 0x42560F23, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        //public SceMpegStream* sceMpegRegistStream(SceMpeg* Mpeg, int iStreamID, int iUnk)
        public int sceMpegRegistStream(SceMpegPointer* Mpeg, StreamId streamId, int streamIndex)
        {
            var streamInfoId = RegisteredStreams.Create(new StreamInfo()
            {
                StreamId = streamId,
                StreamIndex = streamIndex,
            });
            //Console.WriteLine(iStreamID);
            //return 0;

            //var SceMpegData = GetSceMpegData(Mpeg);

            //throw(new NotImplementedException());
            return streamInfoId;
        }

        /// <summary>
        /// sceMpegQueryStreamOffset
        /// </summary>
        /// <param name="mpegPointer">SceMpeg handle</param>
        /// <param name="pmfHeader">Pointer to file header</param>
        /// <param name="offset">Will contain the stream offset in bytes, usually 2048</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x21FF80E4, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegQueryStreamOffset(SceMpegPointer* mpegPointer, byte* pmfHeader, out uint offset)
        {
            var pmf = new Pmf().Load(new MemoryStream(PointerUtils.PointerToByteArray(pmfHeader, 2048)));

            var mpeg = GetMpeg(mpegPointer);
            var sceMpeg = mpegPointer->GetSceMpeg(Memory);

            mpeg.ParsePmfHeader(pmfHeader);
            sceMpeg->StreamSize = (int) (uint) pmf.Header.StreamSize;

            offset = pmf.Header.StreamOffset;
            return 0;
        }

        /// <summary>
        /// sceMpegQueryStreamSize
        /// </summary>
        /// <param name="pmfHeader">Pointer to file header</param>
        /// <param name="size">Will contain stream size in bytes</param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x611E9E11, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegQueryStreamSize(byte* pmfHeader, out uint size)
        {
            var pmf = new Pmf().Load(new MemoryStream(PointerUtils.PointerToByteArray(pmfHeader, 2048)));
            size = pmf.Header.StreamSize;
            //*Size = 0;
            return 0;
        }


        /// <summary>
        /// sceMpegFlushAllStreams
        /// </summary>
        /// <param name="sceMpegPointer"></param>
        /// <returns>0 if successful.</returns>
        [HlePspFunction(NID = 0x707B7629, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceMpegFlushAllStream(SceMpegPointer* sceMpegPointer)
        {
            var mpeg = GetMpeg(sceMpegPointer);
            mpeg.FlushAllStream();
            //throw(new NotImplementedException());
            return 0;
        }
    }
}