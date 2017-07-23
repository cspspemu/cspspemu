using System;
using System.Collections.Generic;
using System.Linq;

public static class CompressedIsoExtensions
{
    public static byte[] CombineAsASingleByteArray(this IEnumerable<ArraySegment<byte>> segments)
    {
        var enumerable = segments as ArraySegment<byte>[] ?? segments.ToArray();
        var outputArray = new byte[enumerable.Sum(segment => segment.Count)];
        var outputOffset = 0;
        foreach (var segment in enumerable)
        {
            //Console.WriteLine("{0}: {1}", OutputOffset, Segment.Count);
            Array.Copy(segment.Array, segment.Offset, outputArray, outputOffset, segment.Count);
            outputOffset += segment.Count;
        }
        return outputArray;
    }

    //static public byte[] ReadBlocksDecompressedAsByteArray(this ICompressedIso CompressedIso, uint Block, int Count)
    //{
    //	var Segments = CompressedIso.ReadBlocksDecompressed(Block, Count);
    //	
    //}
}

namespace CSPspEmu.Hle.Formats
{
    public interface ICompressedIso
    {
        long UncompressedLength { get; }
        int BlockSize { get; }
        ArraySegment<byte>[] ReadBlocksDecompressed(uint block, int count);
    }
}