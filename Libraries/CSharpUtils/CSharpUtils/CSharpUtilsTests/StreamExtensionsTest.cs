using System.IO;
using System.Text;
using CSharpUtils.Extensions;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class StreamExtensionsTest
    {
        [Fact]
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

            Assert.Equal(Output.Length, Input.Length);
            Assert.Equal(Input.ToArray(), Output.ToArray());
            Assert.Equal(Input.ToArray(), Output2.ToArray());
        }

        [Fact]
        public void SliceStreamTest()
        {
            MemoryStream MemoryStream = new MemoryStream(Encoding.ASCII.GetBytes("Hello World"));
            MemoryStream.ReadBytes(6);
            var SliceStream = MemoryStream.ReadStream();
            Assert.Equal("Wo", Encoding.ASCII.GetString(SliceStream.ReadBytes(2)));
            var SliceStream2 = SliceStream.ReadStream();
            Assert.Equal(0, SliceStream.Available());
            Assert.Equal("rld", Encoding.ASCII.GetString(SliceStream2.ReadBytes(3)));
            Assert.Equal(0, SliceStream2.Available());
        }

        [Fact]
        public void PreservePositionAndLockWithNewMemoryStreamTest()
        {
            Assert.Equal(
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