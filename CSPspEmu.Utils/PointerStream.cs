using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SafeILGenerator
{
	unsafe public class PointerStream : Stream
	{
		byte* StartPointer;
		byte* EndPointer;
		byte* CurrentPointer;

		public PointerStream(byte* Pointer, long MaxLength = 32 * 1024 * 1024)
		{
			this.StartPointer = Pointer;
			this.CurrentPointer = Pointer;
			this.EndPointer = this.StartPointer + MaxLength;
		}

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
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
			get { return EndPointer - StartPointer; }
		}

		public override long Position
		{
			get
			{
				return CurrentPointer - StartPointer;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int readed = 0;
			while (count-- > 0)
			{
				if (CurrentPointer >= EndPointer) break;
				buffer[offset++] = *CurrentPointer++;
				readed++;
			}
			return readed;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			while (count-- > 0)
			{
				if (CurrentPointer >= EndPointer) break;
				*CurrentPointer++ = buffer[offset++];
			}
		}
	}
}
