using System.IO;
using System.Linq;
using CSharpUtils.Streams;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class SliceStreamTest
    {
        MemoryStream BaseStream;
        SliceStream SliceStream;

        public SliceStreamTest()
        {
            BaseStream = new MemoryStream();
            for (var n = 0; n < 16; n++) BaseStream.WriteByte((byte) n);
            BaseStream.Position = 0;

            SliceStream = SliceStream.CreateWithBounds(BaseStream, 2, 6);
        }

        [Fact]
        public void SliceStreamStartsAtCursor0Test()
        {
            Assert.Equal(0, SliceStream.Position);
        }

        [Fact]
        public void SeekDoNotChangeParentCursorTest()
        {
            SliceStream.Position = 1;
            Assert.Equal(0, BaseStream.Position);
            Assert.Equal(1, SliceStream.Position);
        }

        [Fact]
        public void SeekInsideBoundsTest()
        {
            SliceStream.Position = 11;
            Assert.Equal(4, SliceStream.Position);
        }

        [Fact]
        public void ReadSliceSuccessTest()
        {
            var Buffer = new byte[8];
            SliceStream.Position = 1;
            int Readed = SliceStream.Read(Buffer, 0, 8);
            Assert.Equal(3, Readed);
            Readed = SliceStream.Read(Buffer, 0, 8);
            Assert.Equal(0, Readed);
            Assert.Equal(new byte[] {3, 4, 5}, Buffer.Take(3).ToArray());
        }

        [Fact]
        public void ReadDoNotChangeParentCursorTest()
        {
            SliceStream.Position = 2;
            SliceStream.ReadByte();
            Assert.Equal(3, SliceStream.Position);
            Assert.Equal(0, BaseStream.Position);
        }
    }
}