#define DUMP_STREAMS

using cscodec;
using cscodec.h264.player;
using CSharpUtils;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Hle.Formats.video;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using CSharpUtils.Drawing.Extensions;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Utils.Utils;

namespace CSPspEmu.Hle.Modules.mpeg
{
    unsafe class MpegAu
    {
        public SceMpegAu SceMpegAu;
    }

    unsafe class Mpeg
    {
        [Inject] public PspMemory Memory;

        [Inject] public GpuImpl GpuImpl;

        [Inject] public PspDisplay PspDisplay;

        public SceMpegPointer* _Mpeg;
        public SceMpeg* Data;
        public MpegAu AvcAu = new MpegAu();

        public MpegAu AtracAu = new MpegAu();

        //public ByteRingBufferWrapper RingBuffer;
        //public readonly ProduceConsumeBuffer<byte> MpegProduceCosumer;
        private ProduceConsumerBufferStream MpegStream;

        private ProduceConsumerBufferStream AudioStream;
        private ProduceConsumerBufferStream VideoStream;
        private MpegPsDemuxer MpegPsDemuxer;
        public H264FrameDecoder H264FrameDecoder;

        public Func<int, int> ReadPackets;

        //private const bool SaveBitmapFrame = true;
        private const bool SaveBitmapFrame = false;

        int FrameIndex;


        private const bool DumpStreams = false;

        public Mpeg(InjectContext InjectContext)
        {
            InjectContext.InjectDependencesTo(this);
            Create();
        }

        public void ParsePmfHeader(byte* PmfHeader)
        {
        }

        public void Create()
        {
            FrameIndex = 0;

            MpegStream = new ProduceConsumerBufferStream();
            AudioStream = new ProduceConsumerBufferStream();
            VideoStream = new ProduceConsumerBufferStream();
            MpegPsDemuxer = new MpegPsDemuxer(MpegStream);
            H264FrameDecoder = new H264FrameDecoder(VideoStream);

            //PspDisplay.CurrentInfo.PlayingVideo = true;
        }


        public void Delete()
        {
            PspDisplay.CurrentInfo.PlayingVideo = false;
        }

        public void Stop()
        {
            PspDisplay.CurrentInfo.PlayingVideo = false;
        }

        public void AvcFlush()
        {
        }

        public void FlushAllStream()
        {
        }

        private void UpdateAuFromPacketInfo(MpegAu MpegAu, MpegPsDemuxer.PacketizedStream Info)
        {
            MpegAu.SceMpegAu.DecodeTimestamp = Info.dts.Value;
            MpegAu.SceMpegAu.PresentationTimestamp = Info.pts.Value;
            //MpegAu.SceMpegAu.DecodeTimestampHigh = (uint)Info.dts.Value;
            //MpegAu.SceMpegAu.DecodeTimestampLow = MpegPsDemuxer.MpegTimestampPerSecond;
            //MpegAu.SceMpegAu.PresentationTimestampHigh = (uint)Info.pts.Value;
            //MpegAu.SceMpegAu.PresentationTimestampLow = MpegPsDemuxer.MpegTimestampPerSecond;
            MpegAu.SceMpegAu.AuSize = (int) Info.Stream.Length;
        }

        private bool DecodePsPacket()
        {
            MpegStream.ReadTransactionBegin();
            try
            {
                if (MpegPsDemuxer.HasMorePackets)
                {
                    var Packet = MpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
                    var Info = MpegPsDemuxer.ParsePacketizedStream(Packet.Stream);

                    //ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Green, () => { Console.WriteLine("DecodePsPacket: {0}:{1:X4}: {2}", Packet.Type, (int)Packet.Type, Info); });

                    switch (Packet.Type)
                    {
                        case Formats.video.MpegPsDemuxer.ChunkType.ST_Video1:
                            UpdateAuFromPacketInfo(AvcAu, Info);
                            if (DumpStreams)
                                FileUtils.CreateAndAppendStream(@"c:\isos\psp\out\video.stream", Info.Stream.Slice());
                            Info.Stream.Slice().CopyToFast(VideoStream);
                            break;
                        case Formats.video.MpegPsDemuxer.ChunkType.ST_Private1:
                            if (DumpStreams)
                                FileUtils.CreateAndAppendStream(@"c:\isos\psp\out\audio.stream", Info.Stream.Slice());
                            UpdateAuFromPacketInfo(AtracAu, Info);
                            Info.Stream.Slice().CopyToFast(AudioStream);
                            break;
                        default:
                            ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red,
                                () => { Console.WriteLine("Unknown PacketType: {0}", Packet.Type); });
                            break;
                    }
                }

                MpegStream.ReadTransactionCommit();
                return true;
            }
            catch (EndOfStreamException EndOfStreamException)
            {
                MpegStream.ReadTransactionRevert();
                return false;
            }
        }

        public void WriteData(void* DataPointer, int DataLength)
        {
            Console.Out.WriteLineColored(ConsoleColor.Cyan, "{0}: {1}", new IntPtr(DataPointer), DataLength);
            try
            {
                var Data = PointerUtils.PointerToByteArray((byte*) DataPointer, DataLength);
                if (DumpStreams)
                    FileUtils.CreateAndAppendStream(@"c:\isos\psp\out\mpeg.stream", new MemoryStream(Data));
                MpegStream.WriteBytes(Data);
            }
            catch (Exception Exception)
            {
                Console.Error.WriteLineColored(ConsoleColor.Red, "{0}", Exception);
            }
        }

        public bool HasData
        {
            get { return true; }
        }

        public SceMpegAu GetAtracAu(StreamId StreamId)
        {
            while (MpegPsDemuxer.HasMorePackets && AudioStream.Length <= 0) DecodePsPacket();
            return AtracAu.SceMpegAu;
        }

        public SceMpegAu GetAvcAu(StreamId StreamId)
        {
            while (MpegPsDemuxer.HasMorePackets && VideoStream.Length <= 0) DecodePsPacket();
            return AvcAu.SceMpegAu;
        }

        public void AvcDecode(SceMpegAu* MpegAccessUnit, int FrameWidth, GuPixelFormats GuPixelFormat,
            PspPointer OutputBuffer)
        {
            if (MpegAccessUnit != null) *MpegAccessUnit = GetAvcAu(StreamId.Avc);

            while (MpegPsDemuxer.HasMorePackets)
            {
                if (!DecodePsPacket()) return;
            }

            if (VideoStream.Length <= 0) return;

            // Buffer 1MB
            //if (VideoStream.Length <= 1 * 1024 * 1024) return;

            try
            {
                //if (H264FrameDecoder.HasMorePackets)
                {
                    //Console.WriteLine("VideoStream.Length: {0}", VideoStream.Length);
                    var Frame = H264FrameDecoder.DecodeFrame();

                    ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.DarkGreen,
                        () => { Console.WriteLine("DecodedFrame: {0}", FrameIndex); });

                    var Bitmap = FrameUtils.imageFromFrameWithoutEdges(Frame, FrameWidth, 272);

                    var TempBuffer =
                        new byte[PixelFormatDecoder.GetPixelsSize(GuPixelFormat, Bitmap.Width * Bitmap.Height)];
                    fixed (byte* TempBufferPtr = TempBuffer)
                    {
                        var TempBufferPtr2 = TempBufferPtr;
                        Bitmap.LockBitsUnlock(PixelFormat.Format32bppArgb, (BitmapData) =>
                        {
                            var InputBuffer = (OutputPixel*) BitmapData.Scan0.ToPointer();
                            int Count = Bitmap.Width * Bitmap.Height;

                            for (int n = 0; n < Count; n++)
                            {
                                var Color = InputBuffer[n];
                                InputBuffer[n].R = Color.B;
                                InputBuffer[n].G = Color.G;
                                InputBuffer[n].B = Color.R;
                                InputBuffer[n].A = 0xFF;
                            }

                            PixelFormatDecoder.Encode(GuPixelFormat, InputBuffer, TempBufferPtr2,
                                Bitmap.Width * Bitmap.Height);
                            PixelFormatDecoder.Encode(PspDisplay.CurrentInfo.PixelFormat, InputBuffer,
                                (byte*) Memory.PspAddressToPointerSafe(PspDisplay.CurrentInfo.FrameAddress), 512,
                                Bitmap.Width, Bitmap.Height);
                            PspDisplay.CurrentInfo.PlayingVideo = true;
                        });
                        PspDisplay.CurrentInfo.PlayingVideo = true;
                        Memory.WriteBytes(OutputBuffer.Address, TempBufferPtr, TempBuffer.Length);
                        GpuImpl.InvalidateCache(OutputBuffer.Address, TempBuffer.Length);
                    }

                    if (SaveBitmapFrame) Bitmap.Save(@"c:\temp\frame" + (FrameIndex) + ".png");
                    FrameIndex++;
                }
                //PixelFormat

                return;
            }
            catch (EndOfStreamException)
            {
                ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red,
                    () => { Console.WriteLine("H264FrameDecoder.DecodeFrame: EndOfStreamException"); });
            }
        }

        public void AtracDecode(SceMpegAu* MpegAccessUnit, byte* OutputBuffer, bool Init)
        {
            if (MpegAccessUnit != null) *MpegAccessUnit = GetAtracAu(StreamId.Atrac);
        }
    }

    public enum StreamId : int
    {
        /// <summary>
        /// MPEG_AVC_STREAM
        /// </summary>
        Avc = 0,

        /// <summary>
        /// MPEG_ATRAC_STREAM
        /// </summary>
        Atrac = 1,

        /// <summary>
        /// MPEG_PCM_STREAM
        /// </summary>
        Pcm = 2,

        /// <summary>
        /// MPEG_DATA_STREAM
        /// </summary>
        Data = 3,

        /// <summary>
        /// MPEG_AUDIO_STREAM
        /// </summary>
        Audio = 15,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SceMpegPointer
    {
        public PspPointer SceMpeg;

        public SceMpeg* GetSceMpeg(PspMemory PspMemory)
        {
            return (SceMpeg*) PspMemory.PspPointerToPointerSafe(SceMpeg);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10000)] // 64KB
    public unsafe struct SceMpeg
    {
        /// <summary>
        /// 0000 - LIBMPEG\0
        /// </summary>
        public fixed byte MagicBytes[8];

        /// <summary>
        /// 0008 - 001\0
        /// </summary>
        public fixed byte VersionBytes[4];

        /// <summary>
        /// 000C - 
        /// </summary>
        public int Pad;

        /// <summary>
        /// 0010 - 
        /// </summary>
        public PspPointer RingBufferAddress;

        /// <summary>
        /// 0014 - 
        /// </summary>
        public PspPointer RingBufferAddressDataUpper;

        /// <summary>
        /// 
        /// </summary>
        public SceMpegAvcMode SceMpegAvcMode;

        /// <summary>
        /// 
        /// </summary>
        public int FrameWidth;

        /// <summary>
        /// 
        /// </summary>
        public int VideoFrameCount;

        /// <summary>
        /// 
        /// </summary>
        public int AudioFrameCount;

        /// <summary>
        /// 
        /// </summary>
        public int AvcFrameStatus;

        /// <summary>
        /// 
        /// </summary>
        public int StreamSize;

        //public fixed byte Data[0x10000];
    }

    /*
    public struct SceMpegStream
    {
    }
    */

    /// <summary>
    /// Access Unit
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SceMpegAu
    {
        /// <summary>
        /// 0000 - presentation timestamp (PTS) MSB
        /// </summary>
        public uint PresentationTimestampHigh;

        /// <summary>
        /// 0004 - presentation timestamp (PTS) LSB
        /// </summary>
        public uint PresentationTimestampLow;

        public ulong PresentationTimestamp
        {
            get { return StructUtils.GetULongFrom2UInt(PresentationTimestampLow, PresentationTimestampHigh); }
            set { StructUtils.ConvertULongTo2UInt(value, out PresentationTimestampLow, out PresentationTimestampHigh); }
        }

        public ulong DecodeTimestamp
        {
            get { return StructUtils.GetULongFrom2UInt(DecodeTimestampLow, DecodeTimestampHigh); }
            set { StructUtils.ConvertULongTo2UInt(value, out DecodeTimestampLow, out DecodeTimestampHigh); }
        }

        /// <summary>
        /// 0008 - decode timestamp (DTS) MSB
        /// </summary>
        public uint DecodeTimestampHigh;

        /// <summary>
        /// 000C - decode timestamp (DTS) LSB
        /// </summary>
        public uint DecodeTimestampLow;

        /// <summary>
        /// 0010 - Es buffer handle
        /// </summary>
        public int EsBuffer;

        /// <summary>
        /// 0014 - Au size
        /// </summary>
        public int AuSize;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SceMpegRingbuffer
    {
        /// <summary>
        /// 00 - Packets
        /// </summary>
        public int PacketsTotal;

        /// <summary>
        /// 04 - PacketsRead
        /// </summary>
        public int PacketsRead;

        /// <summary>
        /// 08 - packetsWritten
        /// </summary>
        public int PacketsWritten;

        /// <summary>
        /// 0C - PacketsFree - Returned by sceMpegRingbufferAvailableSize
        /// </summary>
        public int PacketsAvailable;

        /// <summary>
        /// 10 - PacketSize = 0x800
        /// </summary>
        public uint PacketSize;

        /// <summary>
        /// 14 - Data Start Pointer
        /// </summary>
        public PspPointer Data;

        /// <summary>
        /// 18 - Callback
        /// </summary>
        //sceMpegRingbufferCB Callback;
        //typedef SceInt32 (*sceMpegRingbufferCB)(ScePVoid pData, SceInt32 iNumPackets, ScePVoid pParam);
        public uint Callback;

        /// <summary>
        /// 1C - CallbackParameter
        /// </summary>
        public PspPointer CallbackParameter;

        /// <summary>
        /// 20 - DataUpperBound
        /// </summary>
        public PspPointer DataEnd;

        /// <summary>
        /// 24 - SemaId
        /// </summary>
        public int SemaId;

        /// <summary>
        /// 28 - Pointer to SceMpeg
        /// </summary>
        public PspPointer SceMpeg;

        /// <summary>
        /// 2C - GP
        /// </summary>
        public uint GP;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct SceMpegAvcMode
    {
        /// <summary>
        /// 0000 - unknown, set to -1
        /// </summary>
        public int Mode;

        /// <summary>
        /// 0004 - Decode pixelformat
        /// </summary>
        public GuPixelFormats PixelFormat;
    }

    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct AvcDecodeDetailStruct
    {
        /// <summary>
        /// 0000 - Stores the result.
        /// </summary>
        public int AvcDecodeResult;

        /// <summary>
        /// 0004 - Last decoded frame.
        /// </summary>
        public int VideoFrameCount;

        /// <summary>
        /// 0008 - Frame width.
        /// </summary>
        public int AvcDetailFrameWidth;

        /// <summary>
        /// 000C - Frame height.
        /// </summary>
        public int AvcDetailFrameHeight;

        /// <summary>
        /// 0010 - Frame crop rect (left).
        /// </summary>
        public int FrameCropRectLeft;

        /// <summary>
        /// 0014 - Frame crop rect (right).
        /// </summary>
        public int FrameCropRectRight;

        /// <summary>
        /// 0018 - Frame crop rect (top).
        /// </summary>
        public int FrameCropRectTop;

        /// <summary>
        /// 001C - Frame crop rect (bottom).
        /// </summary>
        public int FrameCropRectBottom;

        /// <summary>
        /// 0x20 - Status of the last decoded frame.
        /// </summary>
        public int AvcFrameStatus;
    }

    public enum QueryYCbCrSizeModeEnum : int
    {
        LoadedFromFile = 1,
        LoadedFromMemory = 2,
    }
}