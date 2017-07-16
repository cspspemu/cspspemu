using System;
using System.IO;
using System.IO.Compression;

namespace CSPspEmu.Hle.Formats
{
    public unsafe class Dax : ICompressedIso
    {
        // 4 sectors
        public const uint DAXFILE_SIGNATURE = 0x00584144;

        public const int DAX_FRAME_SIZE = 0x800 * 4;

        public const int MAX_NCAREAS = 192;

        public enum Version : uint
        {
            DAXFORMAT_VERSION_0 = 0,
            DAXFORMAT_VERSION_1 = 1,
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
            public uint NCAreas;

            /// <summary>
            /// 
            /// </summary>
            public fixed uint Reserved[4];

            /// <summary>
            /// 
            /// </summary>
            public uint TotalBlocks
            {
                get { return (OriginalSize + (DAX_FRAME_SIZE - 1)) / DAX_FRAME_SIZE; }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public struct NCArea
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
        /// <param name="DaxStream"></param>
        public Dax(Stream DaxStream)
        {
            SetStream(DaxStream);
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
            if (this.Header.Magic != DAXFILE_SIGNATURE)
            {
                throw (new InvalidDataException("Not a DAX File"));
            }

            var TotalBlocks = Header.TotalBlocks;

            //Header.TotalBlocks
            var Offsets = this.Stream.ReadStructVector<uint>(TotalBlocks);
            var Sizes = this.Stream.ReadStructVector<ushort>(TotalBlocks);
            NCArea[] NCAreas = null;

            if (Header.Version >= Version.DAXFORMAT_VERSION_1)
            {
                NCAreas = this.Stream.ReadStructVector<NCArea>(Header.NCAreas);
            }

            Blocks = new BlockInfo[TotalBlocks];
            for (int n = 0; n < TotalBlocks; n++)
            {
                Blocks[n].Position = Offsets[n];
                Blocks[n].Length = Sizes[n];
                Blocks[n].IsCompressed = true;
            }
            if (Header.Version >= Version.DAXFORMAT_VERSION_1)
            {
                foreach (var NCArea in NCAreas)
                {
                    //Console.WriteLine("{0}-{1}", NCArea.frame, NCArea.size);
                    for (int n = 0; n < NCArea.Size; n++)
                    {
                        Blocks[NCArea.Frame + n].IsCompressed = false;
                    }
                }
            }
        }

        /// <summary>
        /// Size of each block.
        /// </summary>
        public int BlockSize
        {
            get { return (int) DAX_FRAME_SIZE; }
        }

        /// <summary>
        /// Total number of blocks in the file
        /// </summary>
        public int NumberOfBlocks
        {
            get { return (int) this.Header.TotalBlocks; }
        }

        /// <summary>
        /// Uncompressed length of the file.
        /// </summary>
        public long UncompressedLength
        {
            get { return BlockSize * NumberOfBlocks; }
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
            return Stream.ReadBytes((int) BlockLength);
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

            return new DeflateStream(new MemoryStream((byte[]) In.Concat(new byte[] {0x00})),
                CompressionMode.Decompress).ReadAll();
        }

        long ICompressedIso.UncompressedLength
        {
            get { throw new NotImplementedException(); }
        }

        int ICompressedIso.BlockSize
        {
            get { throw new NotImplementedException(); }
        }

        ArraySegment<byte>[] ICompressedIso.ReadBlocksDecompressed(uint Block, int Count)
        {
            var Segments = new ArraySegment<byte>[Count];
            for (int n = 0; n < Count; n++)
            {
                Segments[n] = new ArraySegment<byte>(ReadBlockDecompressed((uint) (Block + n)));
            }
            return Segments;
        }
    }
}