using System;
using System.IO;
using ComponentAce.Compression.Libs.zlib;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;

namespace CSPspEmu.Hle.Formats
{
	public class Cso : ICompressedIso
	{
		public struct HeaderStruct
		{
			/// <summary>
			/// +00 : 'C','I','S','O'
			/// </summary>
			public uint Magic;
			
			/// <summary>
			/// +04 : header size (==0x18)
			/// </summary>
			public uint HeaderSize;
			
			/// <summary>
			/// +08 : number of original data size
			/// </summary>
			public ulong TotalBytes;
			
			/// <summary>
			/// +10 : Size in bytes of the uncompressed block
			/// </summary>
			public uint BlockSize;
			
			/// <summary>
			/// +14 : version 01
			/// </summary>
			public byte Version;
			
			/// <summary>
			/// +15 : align of index value
			/// </summary>
			public byte Alignment;
			
			/// <summary>
			/// +16 : reserved
			/// </summary>
			public ushort Reserved;

			/// <summary>
			/// 
			/// </summary>
			public int NumberOfBlocks
			{
				get
				{
					return (int)(TotalBytes / BlockSize);
				}
			}
		}

		public struct BlockInfo
		{
			/// <summary>
			/// Data containing information about the position and about
			/// if the block is compresed or not.
			/// </summary>
			public uint Data;

			/// <summary>
			/// Determines if this block is compressed or not.
			/// </summary>
			public bool IsCompressed
			{
				get
				{
					return (Data & 0x80000000) == 0;
				}
			}

			/// <summary>
			/// Obtains the block position.
			/// </summary>
			public uint Position
			{
				get
				{
					return Data & 0x7FFFFFFF;
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected Stream Stream;

		/// <summary>
		/// 
		/// </summary>
		protected HeaderStruct Header;

		/// <summary>
		/// 
		/// </summary>
		protected BlockInfo[] Blocks;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CsoStream"></param>
		public Cso(Stream CsoStream)
		{
			SetStream(CsoStream);
		}

		/// <summary>
		/// Size of each block.
		/// </summary>
		public int BlockSize
		{
			get
			{
				return (int)this.Header.BlockSize;
			}
		}

		/// <summary>
		/// Total number of blocks in the file
		/// </summary>
		public int NumberOfBlocks
		{
			get
			{
				return this.Header.NumberOfBlocks;
			}
		}

		/// <summary>
		/// Uncompressed length of the file.
		/// </summary>
		public long UncompressedLength
		{
			get
			{
				return BlockSize * NumberOfBlocks;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Stream"></param>
		public void SetStream(Stream Stream)
		{
			//this.Stream = new BufferedStream(Stream, 0x20000);
			this.Stream = Stream;

			// Read the header.
			this.Header = this.Stream.ReadStruct<HeaderStruct>();
			if (this.Header.Magic != 0x4F534943)
			{
				throw (new InvalidDataException("Not a CISO File"));
			}

			// Read the block list
			this.Blocks = this.Stream.ReadStructVector<BlockInfo>((uint)(NumberOfBlocks + 1));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public ArraySegment<byte>[] ReadBlocksCompressed(uint Block, int Count)
		{
			var List = new ArraySegment<byte>[Count];
			var BlockStart = this.Blocks[Block + 0].Position;
			var BlockEnd = this.Blocks[Block + Count].Position;
			var BlockLength = BlockEnd - BlockStart;

			Stream.Position = BlockStart;
			var Data = Stream.ReadBytes((int)BlockLength);
			for (int n = 0; n < Count; n++)
			{
				var Start = (int)(this.Blocks[Block + n + 0].Position - BlockStart);
				var End = (int)(this.Blocks[Block + n + 1].Position - BlockStart);
				List[n] = new ArraySegment<byte>(Data, Start, End - Start);
			}
			return List;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Block"></param>
		/// <returns></returns>
		public ArraySegment<byte>[] ReadBlocksDecompressed(uint Block, int Count)
		{
			if (Block + Count >= NumberOfBlocks)
			{
				Count = (int)(NumberOfBlocks - Block);
			}

			if (Count <= 0)
			{
				return new ArraySegment<byte>[0];
			}
			else
			{
				var Segments = ReadBlocksCompressed(Block, Count);
				for (int n = 0; n < Count; n++)
				{
					if (Blocks[Block + n].IsCompressed)
					{
						Segments[n] = new ArraySegment<byte>(new DeflateStream(
							new MemoryStream(Segments[n].Array, Segments[n].Offset, Segments[n].Count),
							CompressionMode.Decompress
						).ReadBytes((int)this.Header.BlockSize));
					}
				}

				return Segments;
			}
		}
	}
}
