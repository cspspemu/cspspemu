using System;
using System.IO;
using CSPspEmu.Hle.Formats;
using CSharpUtils;

namespace CSPspEmu.Hle.Vfs.Iso
{
    public class CompressedIsoProxyStream : Stream
    {
        protected ICompressedIso CompressedIso;
        protected long _Position;

        public CompressedIsoProxyStream(ICompressedIso CompressedIso)
        {
            this.CompressedIso = CompressedIso;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { return CompressedIso.UncompressedLength; }
        }

        public override long Position
        {
            get { return this._Position; }
            set { this._Position = MathUtils.Clamp(value, 0, Length); }
        }

        protected int SelectedCurrentPositionInMacroBlock;
        protected int SelectedCurrentMacroBlock = -1;
        protected ArraySegment<byte> SelectedCurrentMacroBlockData;

        protected int AvailableBytesInMacroBlock
        {
            get
            {
                return Math.Max(0, (int) (SelectedCurrentMacroBlockData.Count - SelectedCurrentPositionInMacroBlock));
            }
        }

        private const int MacroBlockCount = 0x10;

        protected void PrepareBlock()
        {
            var MacroBlockSize = this.CompressedIso.BlockSize * MacroBlockCount;
            int CurrentMacroBlock = (int) (Position / MacroBlockSize);

            if (CurrentMacroBlock != this.SelectedCurrentMacroBlock)
            {
                this.SelectedCurrentMacroBlock = CurrentMacroBlock;
                //Console.WriteLine("[1]");
                this.SelectedCurrentMacroBlockData = new ArraySegment<byte>(CompressedIso
                    .ReadBlocksDecompressed((uint) (CurrentMacroBlock * MacroBlockCount), MacroBlockCount)
                    .CombineAsASingleByteArray());
                //var Data = CompressedIso.ReadBlocksDecompressed((uint)(CurrentMacroBlock * this.MacroBlockCount), 1);
                //ArrayUtils.HexDump(this.SelectedCurrentMacroBlockData.Array);
                //Console.WriteLine("[2]");
            }

            SelectedCurrentPositionInMacroBlock = (int) (Position % MacroBlockSize);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readed = 0;
            {
                //Console.WriteLine("[a]");
                while (count > 0)
                {
                    PrepareBlock();
                    int BytesToRead = Math.Min(AvailableBytesInMacroBlock, count);
                    if (BytesToRead <= 0) break;
                    //Console.WriteLine("{0}, {1}, {2}, {3}", Position, SelectedCurrentPositionInBlock, offset, bytesToRead);
                    Array.Copy(SelectedCurrentMacroBlockData.Array,
                        SelectedCurrentPositionInMacroBlock + SelectedCurrentMacroBlockData.Offset, buffer, offset,
                        BytesToRead);
                    Position += BytesToRead;
                    count -= BytesToRead;
                    offset += BytesToRead;
                    readed += BytesToRead;
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