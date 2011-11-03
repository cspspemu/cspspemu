using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CSPspEmu.Core.Memory
{
	unsafe public class PspMemoryStream : Stream
	{
		protected uint _Position;
		protected AbstractPspMemory PspMemory;

		public PspMemoryStream(AbstractPspMemory AbstractPspMemory)
		{
			this.PspMemory = AbstractPspMemory;
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
			get { return true; }
		}

		public override void Flush()
		{
		}

		public override long Length
		{
			get { return unchecked(0xFFFFFFFF + 1); }
		}

		public override long Position
		{
			get
			{
				return _Position;
			}
			set
			{
				_Position = (uint)value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			byte *Ptr = (byte*)PspMemory.PspAddressToPointer(_Position);
			for (int n = 0; n < count; n++) buffer[n + offset] = Ptr[n];
			_Position += (uint)count;
			return count;
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

		public override void Write(byte[] buffer, int offset, int count)
		{
			byte* Ptr = (byte*)PspMemory.PspAddressToPointer(_Position);
			for (int n = 0; n < count; n++) Ptr[n] = buffer[n + offset];
			_Position += (uint)count;
		}
	}
}
