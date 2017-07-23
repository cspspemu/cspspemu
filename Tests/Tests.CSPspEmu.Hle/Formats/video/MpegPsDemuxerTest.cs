using System;
using System.IO;
using CSharpUtils.Extensions;
using CSPspEmu.Hle.Formats.video;
using Xunit;

namespace Tests.CSPspEmu.Hle.Formats.video
{
    
    public class MpegPsDemuxerTest
    {
        [Fact(Skip = "file not found")]
        public void GetNextPacketAndSyncTest()
        {
            var mpegPsDemuxer = new MpegPsDemuxer(File.OpenRead("../../../TestInput/test.pmf").SliceWithLength(0x800));
            Assert.Equal(MpegPsDemuxer.ChunkType.Start, mpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x1BB, (uint) mpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x1BF, (uint) mpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x1E0, (uint) mpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x109, (uint) mpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x127, (uint) mpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x128, (uint) mpegPsDemuxer.GetNextPacketAndSync());
            Assert.Equal((uint) 0x106, (uint) mpegPsDemuxer.GetNextPacketAndSync());
        }

        [Fact(Skip = "file not found")]
        public void ReadPacketizedElementaryStreamHeaderTest()
        {
            MpegPsDemuxer.Packet packet;
            var mpegPsDemuxer = new MpegPsDemuxer(File.OpenRead("../../../TestInput/test.pmf").SliceWithLength(0x800));
            for (int n = 0; n < 32; n++)
            {
                packet = mpegPsDemuxer.ReadPacketizedElementaryStreamHeader();
                //if ((Packet.Type & 0xFF0) == 0x1E0)
                //if (Packet.Type == 0x1E0)
                {
                    Console.WriteLine("0x{0:X}", packet.Type);
                    mpegPsDemuxer.ParsePacketizedStream(packet.Stream);
                }
            }
        }
    }
}