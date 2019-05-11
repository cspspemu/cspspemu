using System;

using CSharpUtils.Ext.Compression.Lz;
using Xunit;

namespace Tests.CSharpUtils.Ext.Compression
{
    
    public class MatcherTest
    {
        [Fact]
        public void TestMethod1()
        {
            var Bytes = "aaaaaaaaaaaabcdefffffabc".GetBytes();

            Matcher.HandleLzRle(Bytes, 2, 2, 16, 1024, 2, 256, true,
                (Position, Byte) => { Console.WriteLine("Byte({0})", Byte); },
                (Position, Offset, Length) => { Console.WriteLine("LZ({0}, {1})", Offset, Length); },
                (Position, Byte, Length) => { Console.WriteLine("RLE({0}, {1})", Byte, Length); }
            );
        }
    }
}