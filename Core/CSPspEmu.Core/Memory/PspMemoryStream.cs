using System;
using System.IO;

namespace CSPspEmu.Core.Memory
{
	public unsafe class PspMemoryStream : Stream
	{
		protected uint _Position;
		public PspMemory Memory { get; protected set; }

		public PspMemoryStream(PspMemory memory)
		{
			this.Memory = memory;
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return true; } }

		public override void Flush()
		{
		}

		public override long Length => unchecked(0xFFFFFFF0);

		public override long Position
		{
			get { return _Position; }
			set { _Position = (uint)value; }
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin: Position = offset; break;
				case SeekOrigin.Current: Position = Position + offset; break;
				case SeekOrigin.End: Position = -offset; break;
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			fixed (byte* bufferPtr = buffer) Memory.ReadBytes(_Position, bufferPtr + offset, count);
			_Position += (uint)count;
			return count;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			fixed (byte* bufferPtr = buffer) Memory.WriteBytes(_Position, bufferPtr + offset, count);
			_Position += (uint)count;
		}
	}
}