using System;
using System.Collections.Generic;
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
		private Stream Stream;

		public NandReader(Stream stream)
		{
			Stream = stream;
		}

		public byte[] ReadPage(int index)
		{
			try
			{
				Stream.Position = BytesPerRawPage * index;
				var bytes = Stream.ReadBytes(BytesPerPage);
				//ArrayUtils.HexDump(Bytes);
				return bytes;
			}
			catch (Exception)
			{
				return ((byte)0).Repeat(BytesPerPage);
			}
		}

		public byte[] ReadRawPage(int index)
		{
			Stream.Position = BytesPerRawPage * index;
			return Stream.ReadBytes(BytesPerRawPage);
		}

		public const int BytesPerPage = 512;
		public const int BytesPerRawPage = BytesPerPage + 16;
		public const int PagesPerBlock = 32;
		public const int BlocksPerDevice = 2048;

		public const int BytesPerBlock = BytesPerPage * PagesPerBlock;

		public override bool CanRead => Stream.CanRead;
		public override bool CanSeek => Stream.CanSeek;
		public override bool CanWrite => Stream.CanWrite;
		public override void Flush()
		{
			Stream.Flush();
		}

		public override long Length => (Stream.Length / BytesPerRawPage) * BytesPerPage;

		public override long Position
		{
			get; set;
		}

		int _lastPageIndex = -1;
		byte[] _lastPageData;

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (Position >= Length)
			{
				return 0;
			}

			var pageOffset = (int)(Position % BytesPerPage);
			var toRead = (BytesPerPage - pageOffset);

			if (count > toRead)
			{
				int read1 = Read(buffer, offset, toRead);
				int read2 = Read(buffer, offset + toRead, count - toRead);
				return read1 + read2;
			}

			var pageIndex = (int)(Position / BytesPerPage);

			if (pageIndex != _lastPageIndex)
			{
				_lastPageIndex = pageIndex;
				_lastPageData = ReadPage(pageIndex);
			}

			byte[] pageData = _lastPageData;

			Array.Copy(pageData, pageOffset, buffer, offset, count);
			
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
