using System;
using System.IO;
using System.IO.Compression;
using CSharpUtils.Extensions;

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
            public int NumberOfBlocks => (int) (TotalBytes / BlockSize);
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
            public bool IsCompressed => (Data & 0x80000000) == 0;

            /// <summary>
            /// Obtains the block position.
            /// </summary>
            public uint Position => Data & 0x7FFFFFFF;
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
        /// <param name="csoStream"></param>
        public Cso(Stream csoStream)
        {
            SetStream(csoStream);
        }

        /// <summary>
        /// Size of each block.
        /// </summary>
        public int BlockSize => (int) Header.BlockSize;

        /// <summary>
        /// Total number of blocks in the file
        /// </summary>
        public int NumberOfBlocks => Header.NumberOfBlocks;

        /// <summary>
        /// Uncompressed length of the file.
        /// </summary>
        public long UncompressedLength => BlockSize * NumberOfBlocks;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void SetStream(Stream stream)
        {
            //this.Stream = new BufferedStream(Stream, 0x20000);
            Stream = stream;

            // Read the header.
            Header = Stream.ReadStruct<HeaderStruct>();
            if (Header.Magic != 0x4F534943)
            {
                throw new InvalidDataException("Not a CISO File");
            }

            // Read the block list
            Blocks = Stream.ReadStructVector<BlockInfo>((uint) (NumberOfBlocks + 1));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ArraySegment<byte>[] ReadBlocksCompressed(uint block, int count)
        {
            var list = new ArraySegment<byte>[count];
            var blockStart = Blocks[block + 0].Position;
            var blockEnd = Blocks[block + count].Position;
            var blockLength = blockEnd - blockStart;

            Stream.Position = blockStart;
            var data = Stream.ReadBytes((int) blockLength);
            for (var n = 0; n < count; n++)
            {
                var start = (int) (Blocks[block + n + 0].Position - blockStart);
                var end = (int) (Blocks[block + n + 1].Position - blockStart);
                list[n] = new ArraySegment<byte>(data, start, end - start);
            }
            return list;
        }

        /// <summary>
        /// </summary>
        /// <param name="block"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public ArraySegment<byte>[] ReadBlocksDecompressed(uint block, int count)
        {
            if (block + count >= NumberOfBlocks)
            {
                count = (int) (NumberOfBlocks - block);
            }

            if (count <= 0)
            {
                return new ArraySegment<byte>[0];
            }
            var segments = ReadBlocksCompressed(block, count);
            for (var n = 0; n < count; n++)
            {
                if (Blocks[block + n].IsCompressed)
                {
                    segments[n] = new ArraySegment<byte>(new DeflateStream(
                        new MemoryStream(segments[n].Array, segments[n].Offset, segments[n].Count),
                        CompressionMode.Decompress
                    ).ReadBytes((int) Header.BlockSize));
                }
            }

            return segments;
        }
    }
}