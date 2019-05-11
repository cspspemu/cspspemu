using System;
using System.IO;
using System.IO.Compression;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle.Formats
{
    public unsafe class Dax : ICompressedIso
    {
        // 4 sectors
        public const uint DaxfileSignature = 0x00584144;

        public const int DaxFrameSize = 0x800 * 4;

        public const int MaxNcareas = 192;

        public enum Version : uint
        {
            DaxformatVersion0 = 0,
            DaxformatVersion1 = 1,
        }

        public struct HeaderStruct
        {
            /// <summary>
            /// +00 : 'D','A','X','\0'
            /// </summary>
            public uint Magic;

            /// <summary>
            /// +04 : Size of the file
            /// </summary>
            public uint OriginalSize;

            /// <summary>
            /// +08 : number of original data size
            /// </summary>
            public Version Version;

            /// <summary>
            /// +10 : number of compressed block size
            /// On Version 1 or greater.
            /// </summary>
            public uint NcAreas;

            /// <summary>
            /// 
            /// </summary>
            public fixed uint Reserved[4];

            /// <summary>
            /// 
            /// </summary>
            public uint TotalBlocks => (OriginalSize + (DaxFrameSize - 1)) / DaxFrameSize;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct NcArea
        {
            /// <summary>
            /// 
            /// </summary>
            public uint Frame;

            /// <summary>
            /// 
            /// </summary>
            public uint Size;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct BlockInfo
        {
            /// <summary>
            /// Data containing information about the position and about
            /// if the block is compresed or not.
            /// </summary>
            public uint Position;

            /// <summary>
            /// 
            /// </summary>
            public ushort Length;

            /// <summary>
            /// 
            /// </summary>
            public bool IsCompressed;
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
        /// <param name="daxStream"></param>
        public Dax(Stream daxStream) => SetStream(daxStream);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        public void SetStream(Stream stream)
        {
            Stream = stream;

            // Read the header.
            Header = Stream.ReadStruct<HeaderStruct>();
            if (Header.Magic != DaxfileSignature)
            {
                throw new InvalidDataException("Not a DAX File");
            }

            var totalBlocks = Header.TotalBlocks;

            //Header.TotalBlocks
            var offsets = Stream.ReadStructVector<uint>(totalBlocks);
            var sizes = Stream.ReadStructVector<ushort>(totalBlocks);
            NcArea[] ncAreas = null;

            if (Header.Version >= Version.DaxformatVersion1)
            {
                ncAreas = Stream.ReadStructVector<NcArea>(Header.NcAreas);
            }

            Blocks = new BlockInfo[totalBlocks];
            for (var n = 0; n < totalBlocks; n++)
            {
                Blocks[n].Position = offsets[n];
                Blocks[n].Length = sizes[n];
                Blocks[n].IsCompressed = true;
            }
            if (Header.Version < Version.DaxformatVersion1) return;
            if (ncAreas == null) return;

            foreach (var ncArea in ncAreas)
            {
                //Console.WriteLine("{0}-{1}", NCArea.frame, NCArea.size);
                for (var n = 0; n < ncArea.Size; n++)
                {
                    Blocks[ncArea.Frame + n].IsCompressed = false;
                }
            }
        }

        /// <summary>
        /// Size of each block.
        /// </summary>
        public int BlockSize => DaxFrameSize;

        /// <summary>
        /// Total number of blocks in the file
        /// </summary>
        public int NumberOfBlocks => (int) Header.TotalBlocks;

        /// <summary>
        /// Uncompressed length of the file.
        /// </summary>
        public long UncompressedLength => BlockSize * NumberOfBlocks;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[] ReadBlockCompressed(uint block)
        {
            var blockStart = Blocks[block + 0].Position;
            var blockEnd = Blocks[block + 1].Position;
            var blockLength = blockEnd - blockStart;

            Stream.Position = blockStart;
            return Stream.ReadBytes((int) blockLength);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public byte[] ReadBlockDecompressed(uint block)
        {
            if (block >= NumberOfBlocks) return new byte[0];
            var In = ReadBlockCompressed(block);

            // If block is not compressed, get the contents.
            if (!Blocks[block].IsCompressed)
                return In;

            return new DeflateStream(new MemoryStream(In.Concat(new byte[] {0x00})),
                CompressionMode.Decompress).ReadAll();
        }

        long ICompressedIso.UncompressedLength => throw new NotImplementedException();

        int ICompressedIso.BlockSize => throw new NotImplementedException();

        ArraySegment<byte>[] ICompressedIso.ReadBlocksDecompressed(uint block, int count)
        {
            var segments = new ArraySegment<byte>[count];
            for (var n = 0; n < count; n++)
                segments[n] = new ArraySegment<byte>(ReadBlockDecompressed((uint) (block + n)));
            return segments;
        }
    }
}