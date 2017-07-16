using System;
using System.IO;

namespace CSharpUtils.Streams
{
	public class ConsumerMemoryStream : Stream
	{
		public byte[] Contents = new byte[0];

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
			get { return true;  }
		}

		public override void Flush()
		{
		}

		public override long Length
		{
			get { return Contents.Length; }
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int readed = Math.Min(count, Contents.Length);
			Array.Copy(Contents, 0, buffer, offset, readed);
			Contents = Contents.Slice(readed);
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
			Contents = Contents.Concat(buffer, offset, count);
		}
	}
}
