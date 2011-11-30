using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ComponentAce.Compression.Libs.zlib;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
	public class Cso
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
			/// +10 : number of compressed block size
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
		public byte[] ReadBlockCompressed(uint Block)
		{
			var BlockStart = this.Blocks[Block + 0].Position;
			var BlockEnd = this.Blocks[Block + 1].Position;
			var BlockLength = BlockEnd - BlockStart;

			Stream.Position = BlockStart;
			return Stream.ReadBytes((int)BlockLength);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Block"></param>
		/// <returns></returns>
		public byte[] ReadBlockDecompressed(uint Block)
		{
			if (Block >= NumberOfBlocks)
			{
				return new byte[0];
			}
			var In = ReadBlockCompressed(Block);

			// If block is not compressed, get the contents.
			if (!Blocks[Block].IsCompressed)
			{
				return In;
			}

			var Out = new byte[this.Header.BlockSize];

			In = In.Concat(new byte[] { 0x00 });

			var ZStream = new ZStream();

			if (ZStream.inflateInit(-15) != zlibConst.Z_OK)
			{
				throw (new InvalidProgramException("Can't initialize inflater"));
			}
			try
			{
				ZStream.next_in = In;
				ZStream.next_in_index = 0;
				ZStream.avail_in = In.Length;

				ZStream.next_out = Out;
				ZStream.next_out_index = 0;
				ZStream.avail_out = Out.Length;

				int Status = ZStream.inflate(zlibConst.Z_FULL_FLUSH);
				if (Status != zlibConst.Z_STREAM_END) throw (new InvalidDataException("" + ZStream.msg));
			}
			finally
			{
				ZStream.inflateEnd();
			}

			return Out;
		}
	}
}
