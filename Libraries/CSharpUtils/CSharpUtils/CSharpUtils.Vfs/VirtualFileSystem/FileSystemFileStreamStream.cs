using System;
using System.IO;

namespace CSharpUtils.VirtualFileSystem
{
	public class FileSystemFileStreamStream : FileSystemFileStream
	{
		protected Lazy<Stream> _LazyStream;

		public FileSystemFileStreamStream(FileSystem FileSystem, Stream Stream)
			: base(FileSystem)
		{
			this._LazyStream = new Lazy<Stream>(() => Stream);
		}

		public FileSystemFileStreamStream(FileSystem FileSystem, Lazy<Stream> LazyStream)
			: base(FileSystem)
		{
			this._LazyStream = LazyStream;
		}

		public virtual Stream Stream
		{
			get
			{
				return _LazyStream.Value;
			}
		}

		public override bool CanRead
		{
			get { return this.Stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return this.Stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return this.Stream.CanWrite; }
		}

		public override void Flush()
		{
			this.Stream.Flush();
		}

		public override long Length
		{
			get { return this.Stream.Length; }
		}

		public override long Position
		{
			get
			{
				return this.Stream.Position;
			}
			set
			{
				this.Stream.Position = value;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.Stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.Stream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.Stream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.Stream.Write(buffer, offset, count);
		}

		public override void Close()
		{
			this.Stream.Close();
			base.Close();
		}
	}
}
