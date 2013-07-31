using CSPspEmu.Hle.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

static public class ICompressedIsoExtensions
{
	static public byte[] CombineAsASingleByteArray(this IEnumerable<ArraySegment<byte>> Segments)
	{
		var OutputArray = new byte[Segments.Sum(Segment => Segment.Count)];
		int OutputOffset = 0;
		foreach (var Segment in Segments)
		{
			//Console.WriteLine("{0}: {1}", OutputOffset, Segment.Count);
			Array.Copy(Segment.Array, Segment.Offset, OutputArray, OutputOffset, Segment.Count);
			OutputOffset += Segment.Count;
		}
		return OutputArray;
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
		ArraySegment<byte>[] ReadBlocksDecompressed(uint Block, int Count);
	}
}
