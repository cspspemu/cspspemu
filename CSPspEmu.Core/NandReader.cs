using System;
using System.IO;

namespace CSPspEmu.Core
{
	/// <summary>
	/// 512+16 bytes per page
	/// 32 pages per block (16K+512)
	/// 2048 blocks per device (32MB+1MB)
	/// </summary>
	public class NandReader : Stream
	{
		Stream Stream;

		public NandReader(Stream Stream)
		{
			this.Stream = Stream;
		}

		public byte[] ReadPage(int Index)
		{
			try
			{
				Stream.Position = BytesPerRawPage * Index;
				return Stream.ReadBytes(BytesPerPage);
			}
			catch (Exception)
			{
				return ((byte)0).Repeat(BytesPerPage);
			}
		}

		public byte[] ReadRawPage(int Index)
		{
			Stream.Position = BytesPerRawPage * Index;
			return Stream.ReadBytes(BytesPerRawPage);
		}

		public const int BytesPerPage = 512;
		public const int BytesPerRawPage = BytesPerPage + 16;
		public const int PagesPerBlock = 32;
		public const int BlocksPerDevice = 2048;

		public const int BytesPerBlock = BytesPerPage * PagesPerBlock;

		public override bool CanRead
		{
			get { return Stream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return Stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return Stream.CanWrite; }
		}

		public override void Flush()
		{
			Stream.Flush();
		}

		public override long Length
		{
			get { return (Stream.Length / BytesPerRawPage) * BytesPerPage; }
		}

		long _Position;

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
			if (Position >= Length)
			{
				return 0;
			}

			var PageOffset = (int)(Position % BytesPerPage);
			var ToRead = (BytesPerPage - PageOffset);

			if (count > ToRead)
			{
				int Read1 = Read(buffer, offset, ToRead);
				int Read2 = Read(buffer, offset + ToRead, count - ToRead);
				return Read1 + Read2;
			}

			var PageIndex = (int)(Position / BytesPerPage);
			var PageData = ReadPage(PageIndex);

			Array.Copy(PageData, PageOffset, buffer, offset, count);
			
			Position += count;

			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin: Position = offset; break;
				case SeekOrigin.Current: Position += offset; break;
				case SeekOrigin.End: Position = Length + offset; break;
			}
			return Position;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
		}
	}
}
