using System;
using System.IO;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats.video
{
    /// <summary>
    /// MPEG Program Stream
    /// 
    /// Glosary:
    /// - AU : Access Unit
    /// - TS : Transport Stream
    /// - PS : Program Stream
    /// - PSI: Program Specific Information
    /// - PAT: Program Association Table
    /// - PMT: Program Map Tables
    /// - PES: Packetized Elementary Stream
    /// - GOP: Group Of Pictures
    /// - PTS: Presentation TimeStamp
    /// - DTS: Decode TimeStamp
    /// - PID: Packet IDentifier
    /// - PCR: Program Clock Reference
    /// </summary>
    /// <see cref="http://en.wikipedia.org/wiki/MPEG_program_stream"/>
    /// <see cref="http://en.wikipedia.org/wiki/MPEG_transport_stream"/>
    public unsafe class MpegPsDemuxer : IDemuxer
    {
        public const int MpegTimestampPerSecond = 90000;

        public const int PspVideoTimestampStep = 3003
            ; // Value based on pmfplayer ((mpegTimestampPerSecond * 1) / 29.970 (fps)).

        public const int PspAudioTimestampStep = 4180
            ; // For audio play at 44100 Hz ((mpegTimestampPerSecond * 2048 samples) / 44100 == 4180)

        public TimeSpan PspVideoTimeSpanStep = TimeSpan.FromSeconds(1.0 / 29.970);
        public TimeSpan PspAudioTimeSpanStep = TimeSpan.FromSeconds(2048.0 / 44100.0);

        public string Name => "mpeg";

        public string LongName => "MPEG-PS format";

        //public struct PTS_PresentationTimeStamp
        //public struct DTS_DecodeTimeStamp
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TimeStamp
        {
            public fixed byte Bytes[5];

            public byte[] ManagedBytes
            {
                get
                {
                    var ManagedBytes = new byte[5];
                    fixed (byte* BytesPtr = this.Bytes)
                    {
                        PointerUtils.Memcpy(ManagedBytes, BytesPtr, 5);
                    }
                    return ManagedBytes;
                }
            }

            public ulong Value
            {
                get
                {
                    fixed (byte* buf = this.Bytes)
                    {
                        return (ulong) (
                            (ulong) ((buf[0] & 0x0E) << 29) |
                            (ulong) ((PointerUtils.PtrToShort_BE(buf + 1) >> 1) << 15) |
                            (ulong) ((PointerUtils.PtrToShort_BE(buf + 3) >> 1))
                        );
                    }
                }
            }

            public TimeSpan PresentationTimeSpan => TimeSpan.FromSeconds(((double) Value) / (double) MpegTimestampPerSecond);

            public override string ToString()
            {
                return string.Format("TimeStamp(Bytes={0}, Value={1}, TimeSpan={2})",
                    BitConverter.ToString(ManagedBytes), Value, PresentationTimeSpan);
            }
        }

        public enum ChunkType : uint
        {
            Start = 0x000001BA,
            SystemHeader = 0x000001BB,
            ST_PSMapTable = 0x000001BC,
            ST_Private1 = 0x000001BD,
            ST_Padding = 0x000001BE,
            ST_Private2 = 0x000001BF,
            ST_Audio1 = 0x000001C0,
            ST_Audio2 = 0x000001DF,
            ST_Video1 = 0x000001E0,
            ST_Video2 = 0x000001EF,
            ST_ECM = 0x000001F0,
            ST_EMM = 0x000001F1,
            ST_DSMCC = 0x000001F2,
            ST_ISO_13522 = 0x000001F3,
            ST_ITUT_A = 0x000001F4,
            ST_ITUT_B = 0x000001F5,
            ST_ITUT_C = 0x000001F6,
            ST_ITUT_D = 0x000001F7,
            ST_ITUT_E = 0x000001F8,
            ST_PSDirectory = 0x000001FF,
            Invalid = 0xFFFFFFFF,
        }

        protected Stream Stream;

        public MpegPsDemuxer(Stream Stream)
        {
            this.Stream = Stream;
        }

        public class Packet
        {
            public ChunkType Type;
            public MemoryStream Stream;

            public override string ToString()
            {
                return this.ToStringDefault();
            }
        }

        public bool HasMorePackets => !Stream.Eof();

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Packetized_elementary_stream"/>
        public Packet ReadPacketizedElementaryStreamHeader()
        {
            while (!Stream.Eof())
            {
                var StartCode = (uint) GetNextPacketAndSync();
                var ChunkCodeType = (ChunkType) StartCode;
                //ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Yellow, () => { Console.Error.WriteLine("ReadPacketizedElementaryStreamHeader: {0}: {1:X2}", (ChunkType)StartCode, StartCode); });

                switch (ChunkCodeType)
                {
                    // PACK_START_CODE
                    case ChunkType.Start:
                        Stream.Skip(10);
                        continue;
                    // SYSTEM_HEADER_START_CODE
                    case ChunkType.SystemHeader:
                        Stream.Skip(14);
                        continue;
                    // PADDING_STREAM
                    // PRIVATE_STREAM_2
                    case ChunkType.ST_Private2:
                    case ChunkType.ST_Padding:
                        Stream.Skip(Read16());
                        continue;
                }

                if (
                    // Audio Stream
                    (StartCode >= 0x1C0 && StartCode <= 0x1CF) ||
                    // Video Stream
                    (StartCode >= 0x1E0 && StartCode <= 0x1EF) ||
                    // Private Stream (Atrac3+)
                    ChunkCodeType == ChunkType.ST_Private1 ||
                    // ???
                    StartCode == 0x1fd
                )
                {
                    // Found packet.
                    //Console.WriteLine("Position: 0x{0:X}", Stream.Position);
                    ushort PacketSize = Read16();
                    var PacketStream = Stream.ReadStreamCopy(PacketSize);
                    if (PacketStream.Length != PacketSize) throw(new Exception("Didn't read the entire packet"));
                    return new Packet()
                    {
                        Type = (ChunkType) StartCode,
                        Stream = PacketStream,
                    };
                }

                //throw(new NotImplementedException(String.Format("Invalid stream 0x{0:X}", StartCode)));
                Console.Error.WriteLine("Invalid stream 0x{0:X}", StartCode);
            }
            throw(new EndOfStreamException());
        }

        public struct PacketizedStream
        {
            public TimeStamp dts, pts;
            public MemoryStream Stream;

            public override string ToString()
            {
                return string.Format("PacketizedStream(dts={0}, pts={1}, length={2})", dts.Value, pts.Value,
                    Stream.Length);
            }
        }

        public PacketizedStream ParsePacketizedStream(Stream PacketStream)
        {
            var c = (byte) PacketStream.ReadByte();
            //Console.WriteLine("c: 0x{0:X}", c);

            var PacketizedStream = default(PacketizedStream);

            // mpeg 2 PES
            if ((c & 0xC0) == 0x80)
            {
                var flags = (byte) PacketStream.ReadByte();
                var header_len = (byte) PacketStream.ReadByte();
                var HeaderStream = PacketStream.ReadStreamCopy(header_len);
                if (HeaderStream.Length != header_len) throw (new Exception("Didn't read the entire packet"));

                //PacketStream.Skip(header_len);

                //Console.WriteLine("flags: 0x{0:X}", flags);

                // Has PTS/DTS
                if ((flags & 0x80) != 0)
                {
                    PacketizedStream.dts = PacketizedStream.pts = HeaderStream.ReadStruct<TimeStamp>();

                    // Has DTS
                    // On PSP, video DTS is always 1 frame behind PTS
                    if ((flags & 0x40) != 0)
                    {
                        PacketizedStream.dts = HeaderStream.ReadStruct<TimeStamp>();
                    }

                    //Console.WriteLine("pts: {0}, dts: {1}", PacketizedStream.pts, PacketizedStream.dts);
                }
            }

            PacketizedStream.Stream = PacketStream.ReadStreamCopy();

            return PacketizedStream;
        }

        public ushort Read16()
        {
            var Out = new byte[2];
            Stream.Read(Out, 0, 2);
            byte Hi = Out[0];
            byte Lo = Out[1];
            return (ushort) (((ushort) Hi << 8) | (ushort) Lo);
        }

        public ChunkType GetNextPacketAndSync()
        {
            uint Value = 0xFFFFFFFF;
            int Byte;
            while ((Byte = Stream.ReadByte()) != -1)
            {
                Value <<= 8;
                Value |= (byte) Byte;
                if ((Value & 0xFFFFFF00) == 0x00000100)
                {
                    return (ChunkType) Value;
                }
            }
            return (ChunkType) 0xFFFFFFFF;
        }

        public void ReadStartPacket()
        {
        }

        // From libavformat/mpeg.h
        /*
        #define PACK_START_CODE             ((unsigned int)0x000001ba)
        #define SYSTEM_HEADER_START_CODE    ((unsigned int)0x000001bb)
        #define SEQUENCE_END_CODE           ((unsigned int)0x000001b7)
        #define PACKET_START_CODE_MASK      ((unsigned int)0xffffff00)
        #define PACKET_START_CODE_PREFIX    ((unsigned int)0x00000100)
        #define ISO_11172_END_CODE          ((unsigned int)0x000001b9)

        // mpeg2
        #define PROGRAM_STREAM_MAP 0x1bc
        #define PRIVATE_STREAM_1   0x1bd
        #define PADDING_STREAM     0x1be
        #define PRIVATE_STREAM_2   0x1bf

        #define AUDIO_ID 0xc0
        #define VIDEO_ID 0xe0
        #define AC3_ID   0x80
        #define DTS_ID   0x8a
        #define LPCM_ID  0xa0
        #define SUB_ID   0x20

        #define STREAM_TYPE_VIDEO_MPEG1     0x01
        #define STREAM_TYPE_VIDEO_MPEG2     0x02
        #define STREAM_TYPE_AUDIO_MPEG1     0x03
        #define STREAM_TYPE_AUDIO_MPEG2     0x04
        #define STREAM_TYPE_PRIVATE_SECTION 0x05
        #define STREAM_TYPE_PRIVATE_DATA    0x06
        #define STREAM_TYPE_AUDIO_AAC       0x0f
        #define STREAM_TYPE_VIDEO_MPEG4     0x10
        #define STREAM_TYPE_VIDEO_H264      0x1b

        #define STREAM_TYPE_AUDIO_AC3       0x81
        #define STREAM_TYPE_AUDIO_DTS       0x8a

        static const int lpcm_freq_tab[4] = { 48000, 96000, 44100, 32000 };

        static inline int64_t ff_parse_pes_pts(const uint8_t *buf) {
            return (int64_t)(*buf & 0x0e) << 29 |
                    (AV_RB16(buf+1) >> 1) << 15 |
                     AV_RB16(buf+3) >> 1;
        }
        */

        public float Probe(string FileName, Stream ProbeStream)
        {
            throw new NotImplementedException();
        }
    }
}