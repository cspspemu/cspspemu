using System.IO;
using System.Linq;
using CSharpUtils.Streams;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUtils;
using System;
using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
    [TestClass]
    public class RingBufferTest
    {
        RingBuffer<byte> RingBuffer;

        [TestInitialize]
        public void InitializeTest()
        {
            RingBuffer = new RingBuffer<byte>(32);
        }

        [TestMethod]
        public void TestInitialState()
        {
            Assert.AreEqual(32, RingBuffer.Capacity);
            Assert.AreEqual(32, RingBuffer.WriteAvailable);
            Assert.AreEqual(0, RingBuffer.ReadAvailable);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void ReadEmpty()
        {
            RingBuffer.Read();
        }

        [TestMethod]
        public void WriteReadSingle()
        {
            Assert.AreEqual(32, RingBuffer.Capacity);
            Assert.AreEqual(32, RingBuffer.WriteAvailable);
            Assert.AreEqual(0, RingBuffer.ReadAvailable);

            RingBuffer.Write(1);

            Assert.AreEqual(32, RingBuffer.Capacity);
            Assert.AreEqual(31, RingBuffer.WriteAvailable);
            Assert.AreEqual(1, RingBuffer.ReadAvailable);

            Assert.AreEqual(1, RingBuffer.Read());
        }

        [TestMethod]
        public void WriteFull()
        {
            foreach (var n in Enumerable.Range(0, RingBuffer.Capacity)) RingBuffer.Write(0);
        }

        [TestMethod]
        [ExpectedException(typeof(OverflowException))]
        public void WriteFullPlus1()
        {
            foreach (var n in Enumerable.Range(0, RingBuffer.Capacity + 1)) RingBuffer.Write(0);
        }
    }
}