using System;
using System.IO;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using Xunit;


namespace CSharpUtilsTests.Streams
{
    
    public class MapStreamTest
    {
        [Fact]
        public void TestRead()
        {
            var Stream1 = new ZeroStream(5, 0x11);
            var Stream2 = new ZeroStream(3, 0x22);
            var MapStream = new MapStream();
            byte[] Readed1, Readed2, Readed3, Readed4;

            MapStream.Map(3, Stream1);
            MapStream.Map(3 + 5, Stream2);

            MapStream.Position = 4;

            Readed1 = MapStream.ReadBytesUpTo(3);
            Assert.Equal(new byte[] {0x11, 0x11, 0x11}, Readed1);

            Readed2 = MapStream.ReadBytesUpTo(3);
            Assert.Equal(new byte[] {0x11, 0x22, 0x22}, Readed2);

            Readed3 = MapStream.ReadBytesUpTo(1);
            Assert.Equal(new byte[] {0x22}, Readed3);

            MapStream.Position = 3;
            Readed4 = MapStream.ReadBytesUpTo(8);
            Assert.Equal(new byte[] {0x11, 0x11, 0x11, 0x11, 0x11, 0x22, 0x22, 0x22}, Readed4);
        }

        [Fact]
        public void TestLongRead()
        {
            var Position = (long) 5197762560L;
            var Array = new byte[] {0x11, 0x12, 0x13, 0x14, 0x15};
            var Stream1 = new MemoryStream(Array);
            var MapStream = new MapStream();

            MapStream.Map(Position, Stream1);

            var SerializedData = SerializerUtils.SerializeToMemoryStream(MapStream.Serialize).ToArray();

            var MapStream2 = MapStream.Unserialize(new MemoryStream(SerializedData));
            MapStream2.Position = Position;
            Assert.Equal(Array, MapStream2.ReadBytes(5));
        }

        [Fact]
        public void TestReadUnmapped0()
        {
            var MapStream = new MapStream();
            MapStream.Read(new byte[1], 0, 0);
        }

        [Fact]
        public void TestReadUnmapped1()
        {
            try
            {
                var MapStream = new MapStream();
                MapStream.Read(new byte[1], 0, 1);
                Assert.False(true);
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}