using CSPspEmu.Hle.Formats.video;

using System;
using System.IO;
using CSharpUtils.Extensions;
using Xunit;

namespace CSPspEmu.Core.Tests
{
    
    public class MpegPsDemuxerTest
    {
        [Fact(Skip = "file not found")]
        public void GetNextPacketAndSyncTest()
        {
            var MpegPsDemuxer = new MpegPsDemuxer(File.OpenRead("../../../TestInput/test.pmf").SliceWithLength(0x800));
            Assert.Equal(MpegPsDemuxer.ChunkType.Start, MpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x1BB, (uint) MpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x1BF, (uint) MpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x1E0, (uint) MpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x109, (uint) MpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x127, (uint) MpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x128, (uint) MpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x106, (uint) MpegPsDemuxer.GetNextPacketAndSync());
        }

        [Fact(Skip = "file not found")]
        public void ReadPacketizedElementaryStreamHeaderTest()
        {
            MpegPsDemuxer.Packet Packet;
            var MpegPsDemuxer = new MpegPsDemuxer(File.OpenRead("../../../TestInput/test.pmf").SliceWithLength(0x800));
            for (int n = 0; n < 32; n++)
            {
                Packet = MpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
                //if ((Packet.Type & 0xFF0) == 0x1E0)
                //if (Packet.Type == 0x1E0)
                {
                    Console.WriteLine("0x{0:X}", Packet.Type);
                    MpegPsDemuxer.ParsePacketizedStream(Packet.Stream);
                }
            }
        }
    }
}