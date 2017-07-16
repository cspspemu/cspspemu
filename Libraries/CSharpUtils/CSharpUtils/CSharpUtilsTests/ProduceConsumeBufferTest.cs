using CSharpUtils;
using NUnit.Framework;

namespace CSharpUtilsTests
{
    [TestFixture]
    public class ProduceConsumeBufferTest
    {
        [Test]
        public void ProduceTest()
        {
            var Buffer = new ProduceConsumeBuffer<int>();
            Buffer.Produce(new int[] {10, 20, -2, 3, 15});
            Assert.AreEqual(Buffer.IndexOf(-2), 2);
        }

        [Test]
        public void ConsumeTest()
        {
            var Buffer = new ProduceConsumeBuffer<int>();
            Buffer.Produce(new int[] {10, 20, -2, 3, 15});
            Assert.AreEqual(Buffer.IndexOf(-2), 2);
            CollectionAssert.AreEqual(Buffer.Consume(2), new int[] {10, 20});
            Assert.AreEqual(Buffer.IndexOf(-2), 0);
            CollectionAssert.AreEqual(Buffer.Consume(1), new int[] {-2});
            Assert.AreEqual(Buffer.IndexOf(-2), -1);
            Buffer.Produce(new int[] {4, 2});
            CollectionAssert.AreEqual(Buffer.Items, new int[] {3, 15, 4, 2});

            Assert.AreEqual(-1, Buffer.IndexOf(new int[] {3, 15, 4, 1}));
            Assert.AreEqual(0, Buffer.IndexOf(new int[] {3, 15, 4}));
            Assert.AreEqual(1, Buffer.IndexOf(new int[] {15, 4}));
            Assert.AreEqual(2, Buffer.IndexOf(new int[] {4, 2}));
        }
    }
}