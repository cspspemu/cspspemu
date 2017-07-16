using System;
using System.IO;

namespace CSharpUtils.Streams
{
	public class ProxyStream : Stream
	{
		protected Stream ParentStream;
		protected bool CloseParent;

		public ProxyStream(Stream BaseStream, bool CloseParent = false)
		{
			this.ParentStream = BaseStream;
			this.CloseParent = CloseParent;
		}

		public override bool CanRead { get { return ParentStream.CanRead; } }
		public override bool CanSeek { get { return ParentStream.CanSeek; } }
		public override bool CanWrite { get { return ParentStream.CanWrite; } }

		public override void Flush()
		{
			ParentStream.Flush();
		}

		public override long Length
		{
			get { return ParentStream.Length; }
		}

		public override long Position
		{
			get
			{
				return ParentStream.Position;
			}
			set
			{
				ParentStream.Position = value;
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return ParentStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return ParentStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			ParentStream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			ParentStream.Write(buffer, offset, count);
		}

		public override void Close()
		{
			if (CloseParent) ParentStream.Close();
			base.Close();
		}

		/*
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		*/
	}
}
