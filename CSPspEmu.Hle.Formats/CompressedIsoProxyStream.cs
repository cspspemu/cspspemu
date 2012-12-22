using System;
using System.IO;
using CSPspEmu.Hle.Formats;
using CSharpUtils;

namespace CSPspEmu.Hle.Vfs.Iso
{
	public class CompressedIsoProxyStream : Stream
	{
		protected ICompressedIso CompressedIso;
		protected long _Position;

		public CompressedIsoProxyStream(ICompressedIso CompressedIso)
		{
			this.CompressedIso = CompressedIso;
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
			get { return false; }
		}

		public override void Flush()
		{
		}

		public override long Length
		{
			get { return CompressedIso.UncompressedLength; }
		}

		public override long Position
		{
			get
			{
				return this._Position;
			}
			set
			{
				this._Position = MathUtils.Clamp(value, 0, Length);
			}
		}

		protected int SelectedCurrentPositionInBlock;
		protected int SelectedCurrentBlock = -1;
		protected byte[] SelectedCurrentBlockData;

		protected int AvailableBytesInBlock
		{
			get
			{
				return Math.Max(0, (int)(SelectedCurrentBlockData.Length - SelectedCurrentPositionInBlock));
			}
		}

		protected void PrepareBlock()
		{
			int CurrentBlock = (int)(Position / this.CompressedIso.BlockSize);

			if (CurrentBlock != this.SelectedCurrentBlock)
			{
				this.SelectedCurrentBlock = CurrentBlock;
				this.SelectedCurrentBlockData = CompressedIso.ReadBlockDecompressed((uint)CurrentBlock);
			}

			SelectedCurrentPositionInBlock = (int)(Position % this.CompressedIso.BlockSize);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int readed = 0;
			{
				while (count > 0)
				{
					PrepareBlock();
					int bytesToRead = Math.Min(AvailableBytesInBlock, count);
					if (bytesToRead <= 0) break;
					//Console.WriteLine("{0}, {1}, {2}, {3}", Position, SelectedCurrentPositionInBlock, offset, bytesToRead);
					Array.Copy(SelectedCurrentBlockData, SelectedCurrentPositionInBlock, buffer, offset, bytesToRead);
					Position += bytesToRead;
					count -= bytesToRead;
					offset += bytesToRead;
					readed += bytesToRead;
				}
			}
			return readed;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin: Position = offset; break;
				case SeekOrigin.Current: Position = 0 + offset; break;
				case SeekOrigin.End: Position = Length + offset; break;
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}
}
