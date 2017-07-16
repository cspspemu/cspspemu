using NUnit.Framework;
using System;
using System.Text;
using CSharpUtils;
using System.Collections.Generic;
using CSharpUtils.Ext.Compression.Lz;
using CSharpUtils.Extensions;

namespace CSharpUtilsTests.Compression
{
    [TestFixture]
    public class LzBufferTest
    {
        [Test]
        public void FindMaxSequenceTest()
        {
            var data = new byte[]
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18,
                7, 8, 9, 10, 12, 13,
                7, 8, 9, 10, 11, 16
            };
            var LzMatcher = new LzMatcher(data, data.Length - 6, 0x1000, 3, 16, true);

            var Result = LzMatcher.FindMaxSequence();

            Assert.AreEqual("LzMatcher.FindSequenceResult(Offset=7, Size=5)", Result.ToString());
        }

        [Test]
        public void HandleWithOverlappingTest()
        {
            var Data = Encoding.UTF8.GetBytes("abccccccabc");
            var Results = new List<string>();
            Matcher.HandleLz(Data, 0, 2, 15 + 2, ushort.MaxValue, true,
                (int Position, byte Byte) => { Results.Add("PUT(" + Byte + ")"); },
                (int Position, int FoundOffset, int FoundSize) =>
                {
                    Results.Add("REPEAT(" + FoundOffset + "," + FoundSize + ")");
                }
            );

            Assert.AreEqual(
                "PUT(97),PUT(98),PUT(99),REPEAT(-1,5),REPEAT(-8,3)",
                Results.ToStringArray()
            );
        }

        [Test]
        public void HandleWithoutOverlappingTest()
        {
            var Data = Encoding.UTF8.GetBytes("abccccccccccccccccccccccabc");
            var Results = new List<string>();
            Matcher.HandleLz(Data, 0, 3, 9, ushort.MaxValue, false,
                (int Position, byte Byte) => { Results.Add("PUT(" + Byte + ")"); },
                (int Position, int FoundOffset, int FoundSize) =>
                {
                    Results.Add("REPEAT(" + FoundOffset + "," + FoundSize + ")");
                }
            );

            Assert.AreEqual(
                "PUT(97),PUT(98),PUT(99),PUT(99),PUT(99),REPEAT(-3,3),REPEAT(-6,6),REPEAT(-12,9),PUT(99),REPEAT(-24,3)",
                Results.ToStringArray()
            );
        }
    }
}