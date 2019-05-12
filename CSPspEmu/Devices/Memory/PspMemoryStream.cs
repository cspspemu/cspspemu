using System;
using System.IO;

namespace CSPspEmu.Core.Memory
{
    public unsafe class PspMemoryStream : Stream
    {
        protected uint _Position;
        public PspMemory Memory { get; protected set; }

        public PspMemoryStream(PspMemory Memory)
        {
            this.Memory = Memory;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override void Flush()
        {
        }

        public override long Length => unchecked(0xFFFFFFF0);

        public override long Position
        {
            get => _Position;
            set => _Position = (uint) value;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;
                case SeekOrigin.End:
                    Position = -offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Memory.Read(_Position, new Span<byte>(buffer, offset, count));
            _Position += (uint) count;
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Memory.Write(_Position, new Span<byte>(buffer, offset, count));
            _Position += (uint) count;
        }
    }
}