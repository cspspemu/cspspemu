using System;
using System.IO;
using CSharpUtils;

namespace CSPspEmu.Hle.Formats
{
    public class CompressedIsoProxyStream : Stream
    {
        protected ICompressedIso CompressedIso;
        protected long Position2;

        public CompressedIsoProxyStream(ICompressedIso compressedIso) => CompressedIso = compressedIso;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override void Flush()
        {
        }

        public override long Length => CompressedIso.UncompressedLength;

        public override long Position
        {
            get => Position2;
            set => Position2 = MathUtils.Clamp(value, 0, Length);
        }

        protected int SelectedCurrentPositionInMacroBlock;
        protected int SelectedCurrentMacroBlock = -1;
        protected ArraySegment<byte> SelectedCurrentMacroBlockData;

        protected int AvailableBytesInMacroBlock => Math.Max(0, SelectedCurrentMacroBlockData.Count - SelectedCurrentPositionInMacroBlock);

        private const int MacroBlockCount = 0x10;

        protected void PrepareBlock()
        {
            var macroBlockSize = CompressedIso.BlockSize * MacroBlockCount;
            var currentMacroBlock = (int) (Position / macroBlockSize);

            if (currentMacroBlock != SelectedCurrentMacroBlock)
            {
                SelectedCurrentMacroBlock = currentMacroBlock;
                //Console.WriteLine("[1]");
                SelectedCurrentMacroBlockData = new ArraySegment<byte>(CompressedIso
                    .ReadBlocksDecompressed((uint) (currentMacroBlock * MacroBlockCount), MacroBlockCount)
                    .CombineAsASingleByteArray());
                //var Data = CompressedIso.ReadBlocksDecompressed((uint)(CurrentMacroBlock * this.MacroBlockCount), 1);
                //ArrayUtils.HexDump(this.SelectedCurrentMacroBlockData.Array);
                //Console.WriteLine("[2]");
            }

            SelectedCurrentPositionInMacroBlock = (int) (Position % macroBlockSize);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readed = 0;
            {
                //Console.WriteLine("[a]");
                while (count > 0)
                {
                    PrepareBlock();
                    var bytesToRead = Math.Min(AvailableBytesInMacroBlock, count);
                    if (bytesToRead <= 0) break;
                    //Console.WriteLine("{0}, {1}, {2}, {3}", Position, SelectedCurrentPositionInBlock, offset, bytesToRead);
                    Array.Copy(SelectedCurrentMacroBlockData.Array,
                        SelectedCurrentPositionInMacroBlock + SelectedCurrentMacroBlockData.Offset, buffer, offset,
                        bytesToRead);
                    Position += bytesToRead;
                    count -= bytesToRead;
                    offset += bytesToRead;
                    readed += bytesToRead;
                }
                //Console.WriteLine("[b]");
            }
            return readed;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position = 0 + offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}