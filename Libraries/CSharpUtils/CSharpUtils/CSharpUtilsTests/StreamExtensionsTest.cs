using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class StreamExtensionsTest
    {
        [TestMethod]
        public void CopyToFastTest()
        {
            var Input = new MemoryStream();
            var Output = new MemoryStream();
            var Output2 = new MemoryStream();

            for (int n = 0; n < 0x100; n++)
            {
                Input.WriteByte((byte) n);
            }

            Input.Position = 0;
            Input.CopyTo(Output2);
            Input.Position = 0;
            Input.CopyToFast(Output);

            Assert.AreEqual(Output.Length, Input.Length);
            CollectionAssert.AreEqual(Input.ToArray(), Output.ToArray());
            CollectionAssert.AreEqual(Input.ToArray(), Output2.ToArray());
        }

        [TestMethod]
        public void SliceStreamTest()
        {
            MemoryStream MemoryStream = new MemoryStream(Encoding.ASCII.GetBytes("Hello World"));
            MemoryStream.ReadBytes(6);
            var SliceStream = MemoryStream.ReadStream();
            Assert.AreEqual("Wo", Encoding.ASCII.GetString(SliceStream.ReadBytes(2)));
            var SliceStream2 = SliceStream.ReadStream();
            Assert.AreEqual(0, SliceStream.Available());
            Assert.AreEqual("rld", Encoding.ASCII.GetString(SliceStream2.ReadBytes(3)));
            Assert.AreEqual(0, SliceStream2.Available());
        }

        [TestMethod]
        public void PreservePositionAndLockWithNewMemoryStreamTest()
        {
            Assert.AreEqual(
                "010203",
                new MemoryStream().PreservePositionAndLock((Stream) =>
                {
                    Stream.WriteByte(1);
                    Stream.WriteByte(2);
                    Stream.WriteByte(3);
                }).ToArray().ToHexString()
            );
        }
    }
}