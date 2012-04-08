using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Memory
{
	unsafe public class PspMemoryStream : Stream
	{
		protected uint _Position;
		public PspMemory Memory { get; protected set; }

		public PspMemoryStream(PspMemory Memory)
		{
			this.Memory = Memory;
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return true; } }

		public override void Flush()
		{
		}

		public override long Length
		{
			get { return unchecked(0xFFFFFFF0); }
		}

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
			byte* Ptr = (byte*)Memory.PspAddressToPointerSafe(_Position, count);
			Marshal.Copy(new IntPtr(Ptr), buffer, offset, count);
			_Position += (uint)count;
			return count;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			byte* Ptr = (byte*)Memory.PspAddressToPointerSafe(_Position, count);
			Marshal.Copy(buffer, offset, new IntPtr(Ptr), count);
			_Position += (uint)count;
		}
	}
}