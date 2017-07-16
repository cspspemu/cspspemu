using System.IO;
using System.Linq;
using CSharpUtils.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class SliceStreamTest
    {
        MemoryStream BaseStream;
        SliceStream SliceStream;

       [TestInitialize]
        public void InitializeTest()
        {
            BaseStream = new MemoryStream();
            for (var n = 0; n < 16; n++) BaseStream.WriteByte((byte)n);
            BaseStream.Position = 0;

            SliceStream = SliceStream.CreateWithBounds(BaseStream, 2, 6);
        }

        [TestMethod]
        public void SliceStreamStartsAtCursor0Test()
        {
            Assert.AreEqual(0, SliceStream.Position);
        }

        [TestMethod]
        public void SeekDoNotChangeParentCursorTest()
        {
            SliceStream.Position = 1;
            Assert.AreEqual(0, BaseStream.Position);
            Assert.AreEqual(1, SliceStream.Position);
        }

        [TestMethod]
        public void SeekInsideBoundsTest()
        {
            SliceStream.Position = 11;
            Assert.AreEqual(4, SliceStream.Position);
        }

        [TestMethod]
        public void ReadSliceSuccessTest()
        {
            var Buffer = new byte[8];
            SliceStream.Position = 1;
            int Readed = SliceStream.Read(Buffer, 0, 8);
            Assert.AreEqual(3, Readed);
            Readed = SliceStream.Read(Buffer, 0, 8);
            Assert.AreEqual(0, Readed);
            CollectionAssert.AreEqual(new byte[] { 3, 4, 5 }, Buffer.Take(3).ToArray());
        }

        [TestMethod]
        public void ReadDoNotChangeParentCursorTest()
        {
            SliceStream.Position = 2;
            SliceStream.ReadByte();
            Assert.AreEqual(3, SliceStream.Position);
            Assert.AreEqual(0, BaseStream.Position);
        }
    }
}
