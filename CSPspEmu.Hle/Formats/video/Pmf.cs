using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpUtils.Endian;

namespace CSPspEmu.Hle.Formats.video
{
	unsafe public partial class Pmf
	{
		/// <summary>
		/// 0000 -
		/// </summary>
		public struct HeaderStruct
		{
			/// <summary>
			/// 0000 - 
			/// </summary>
			public uint Magic;

			/// <summary>
			/// 0004 - 
			/// </summary>
			public uint Version;

			/// <summary>
			/// 0008 - 
			/// </summary>
			public uint_be StreamOffset;

			/// <summary>
			/// 000C - 
			/// </summary>
			public uint_be StreamSize;

			/*
			/// <summary>
			/// 0010 -
			/// </summary>
			public fixed uint_be Pad[0x10];

			/// <summary>
			/// 0050 -
			/// </summary>
			*/
		}

		public struct InfoHeaderStruct
		{
			public uint48_be FirstTimestamp;
			public uint48_be LastTimestamp;
			public fixed byte Unknown1[0x10 + 0x10 + 0xE];
			public byte _WidthShifted;
			public byte _HeightShifted;
			public int Width { get { return (int)(_WidthShifted * 0x10); } }
			public int Height { get { return (int)(_HeightShifted * 0x10); } }
		}

		public class Chunk
		{
			/// <summary>
			/// 
			/// </summary>
			public uint Size;

			/// <summary>
			/// 
			/// </summary>
			//public ushort Type;

			/// <summary>
			/// 
			/// </summary>
			public Stream Stream;
		}

		public HeaderStruct Header;
		public InfoHeaderStruct InfoHeader;

		public Chunk ReadChunk(Stream Stream)
		{
			var ChunkSize = (uint)Stream.ReadStruct<uint_be>();
			//var ChunkType = (ushort)Stream.ReadStruct<ushort_be>();
			var ChunkStream = Stream.ReadStream(ChunkSize);
			return new Chunk()
			{
				Size = ChunkSize,
				//Type = ChunkType,
				Stream = ChunkStream,
			};
		}

		public Pmf Load(Stream Stream)
		{
			Header = Stream.ReadStruct<HeaderStruct>();

			var Chunk = ReadChunk(Stream.SliceWithLength(0x50));
			InfoHeader = Chunk.Stream.ReadStruct<InfoHeaderStruct>();
			/*
			Console.WriteLine("0x{0:X}", (ulong)InfoHeader.FirstTimestamp);
			Console.WriteLine("0x{0:X}", (ulong)InfoHeader.LastTimestamp);
			Console.WriteLine("{0}", (ulong)InfoHeader.Width);
			Console.WriteLine("{0}", (ulong)InfoHeader.Height);
			*/

			return this;
		}
	}
}
