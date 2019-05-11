using System.Linq;

using CSharpUtils;
using System;
using Xunit;

namespace CSharpUtilsTests
{
    
    public class RingBufferTest
    {
        RingBuffer<byte> RingBuffer;

        public RingBufferTest()
        {
            RingBuffer = new RingBuffer<byte>(32);
        }

        [Fact]
        public void TestInitialState()
        {
            Assert.Equal(32, RingBuffer.Capacity);
            Assert.Equal(32, RingBuffer.WriteAvailable);
            Assert.Equal(0, RingBuffer.ReadAvailable);
        }

        [Fact]
        public void ReadEmpty()
        {
            Assert.Throws<OverflowException>(() => { RingBuffer.Read(); });
        }

        [Fact]
        public void WriteReadSingle()
        {
            Assert.Equal(32, RingBuffer.Capacity);
            Assert.Equal(32, RingBuffer.WriteAvailable);
            Assert.Equal(0, RingBuffer.ReadAvailable);

            RingBuffer.Write(1);

            Assert.Equal(32, RingBuffer.Capacity);
            Assert.Equal(31, RingBuffer.WriteAvailable);
            Assert.Equal(1, RingBuffer.ReadAvailable);

            Assert.Equal(1, RingBuffer.Read());
        }

        [Fact]
        public void WriteFull()
        {
            foreach (var n in Enumerable.Range(0, RingBuffer.Capacity)) RingBuffer.Write(0);
        }

        [Fact]
        public void WriteFullPlus1()
        {
            Assert.Throws<OverflowException>(() =>
            {
                foreach (var n in Enumerable.Range(0, RingBuffer.Capacity + 1)) RingBuffer.Write(0);
            });
        }
    }
}