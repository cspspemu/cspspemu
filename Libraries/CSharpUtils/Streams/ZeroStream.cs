using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSharpUtils.Streams
{
    public class ZeroStream : Stream
    {
        protected long _Size;
        protected long _Position;
        protected byte ValueToRepeat;

        public ZeroStream(long Size, byte ValueToRepeat = 0x00)
        {
            this._Size = Size;
            this.Position = 0;
            this.ValueToRepeat = ValueToRepeat;
        }

        public override bool CanRead
        {
            get { return false; }
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
            get { return _Size; }
        }

        public override long Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int Readed = Math.Min(count, (int)(Length - Position));
            for (int n = 0; n < count; n++) buffer[offset + n] = this.ValueToRepeat;
            Position += Readed;
            return Readed;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: Position = offset; break;
                case SeekOrigin.Current: Position = Position + offset; break;
                case SeekOrigin.End: Position = Length + offset;  break;
                default: throw(new NotImplementedException());
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            _Size = value;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
