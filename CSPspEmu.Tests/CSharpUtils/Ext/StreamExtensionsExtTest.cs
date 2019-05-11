using System.IO;
using CSharpUtils.Ext.Extensions;
using CSharpUtils.Ext.SpaceAssigner;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using Xunit;


namespace CSharpUtilsTests.Extensions
{
    
    public class StreamExtensionsExtTest
    {
        [Fact]
        public void TestJoinWithThresold()
        {
            var Spaces = new SpaceAssigner1D.Space[]
            {
                new SpaceAssigner1D.Space(0, 4),
                new SpaceAssigner1D.Space(6, 9),
                new SpaceAssigner1D.Space(11, 16),
                new SpaceAssigner1D.Space(60, 80),
                new SpaceAssigner1D.Space(84, 99),
            };

            var JoinedSpaces = Spaces.JoinWithThresold(thresold: 4);
            Assert.Equal("Space(Min=0, Max=16),Space(Min=60, Max=99)", JoinedSpaces.ToStringArray());
        }

        [Fact]
        public void TestConvertSpacesToMapStream()
        {
            var Stream = new MemoryStream(new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10});
            var MapStream = Stream.ConvertSpacesToMapStream(new SpaceAssigner1D.Space[]
            {
                new SpaceAssigner1D.Space(1, 3),
                new SpaceAssigner1D.Space(6, 9)
            });

            Assert.Equal(
                "StreamEntry(1, 2, CSharpUtils.Streams.SliceStream),StreamEntry(6, 3, CSharpUtils.Streams.SliceStream)",
                MapStream.StreamEntries.ToStringArray()
            );

            var SerializedData = SerializerUtils.SerializeToMemoryStream(MapStream.Serialize).ToArray();

            //Console.WriteLine(SerializedData.ToHexString());
            //SerializerUtils.SerializeToMemoryStream(MapStream.Serialize).CopyToFile(@"c:\temp\test.bin");

            Assert.Equal(
                new byte[]
                {
                    // Magic
                    0x4D, 0x41, 0x50, 0x53,
                    // Version
                    0x01,
                    // Count
                    0x02,
                    // EntryHeader1
                    0x01, 0x02,
                    // EntryHeader2
                    0x06, 0x03,
                    // EntryContent1
                    0x02, 0x03,
                    // EntryContent2
                    0x07, 0x08, 0x09
                },
                SerializedData
            );

            var MapStream2 = MapStream.Unserialize(new MemoryStream(SerializedData));
            Assert.Equal(
                "StreamEntry(1, 2, CSharpUtils.Streams.SliceStream),StreamEntry(6, 3, CSharpUtils.Streams.SliceStream)",
                MapStream2.StreamEntries.ToStringArray()
            );

            var ZeroStream = new MemoryStream(((byte) 0).Repeat(10));
            MapStream2.WriteSegmentsToStream(ZeroStream);
            Assert.Equal(
                new byte[] {0, 2, 3, 0, 0, 0, 7, 8, 9, 0},
                ZeroStream.ToArray()
            );
        }
    }
}