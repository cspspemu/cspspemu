using CSharpUtils;
using Xunit;


namespace CSharpUtilsTests
{
    
    public class ProduceConsumeBufferTest
    {
        [Fact]
        public void ProduceTest()
        {
            var Buffer = new ProduceConsumeBuffer<int>();
            Buffer.Produce(new int[] {10, 20, -2, 3, 15});
            Assert.Equal(Buffer.IndexOf(-2), 2);
        }

        [Fact]
        public void ConsumeTest()
        {
            var Buffer = new ProduceConsumeBuffer<int>();
            Buffer.Produce(new int[] {10, 20, -2, 3, 15});
            Assert.Equal(Buffer.IndexOf(-2), 2);
            Assert.Equal(Buffer.Consume(2), new int[] {10, 20});
            Assert.Equal(Buffer.IndexOf(-2), 0);
            Assert.Equal(Buffer.Consume(1), new int[] {-2});
            Assert.Equal(Buffer.IndexOf(-2), -1);
            Buffer.Produce(new int[] {4, 2});
            Assert.Equal(Buffer.Items, new int[] {3, 15, 4, 2});

            Assert.Equal(-1, Buffer.IndexOf(new int[] {3, 15, 4, 1}));
            Assert.Equal(0, Buffer.IndexOf(new int[] {3, 15, 4}));
            Assert.Equal(1, Buffer.IndexOf(new int[] {15, 4}));
            Assert.Equal(2, Buffer.IndexOf(new int[] {4, 2}));
        }
    }
}