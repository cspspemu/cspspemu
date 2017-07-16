using System;
using System.IO;

namespace CSharpUtils.Fastcgi
{
	public class FascgiResponseOutputStream : Stream
	{
		FastcgiRequest FastcgiRequest;
		Fastcgi.PacketType PacketType;
		MemoryStream Buffer;
        static int MaxChunkLength = 0xFFF8;

		public FascgiResponseOutputStream(FastcgiRequest FastcgiRequest, Fastcgi.PacketType PacketType)
		{
			this.FastcgiRequest = FastcgiRequest;
			this.PacketType = PacketType;
			this.Buffer = new MemoryStream();
		}

		public override bool CanRead
		{
			get { return false; }
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
            if (this.Buffer.Length > 0)
            {
                byte[] Contents = this.Buffer.ToArray();
                int ContentsLength = Contents.Length;
                for (int ChunkOffset = 0; ChunkOffset < ContentsLength; ChunkOffset += MaxChunkLength)
                {
                    int ChunkLength = Math.Min(MaxChunkLength, Contents.Length - ChunkOffset);
                    FastcgiRequest.FastcgiHandler.Writer.WritePacket(
                        this.FastcgiRequest.RequestId,
                        PacketType,
                        Contents,
                        ChunkOffset,
                        ChunkLength
                    );
                }

                this.Buffer = new MemoryStream();
            }
		}

		public override long Length
		{
			get { throw new NotImplementedException(); }
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
			throw new NotImplementedException();
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
			Buffer.Write(buffer, offset, count);
            if (Buffer.Length > MaxChunkLength)
            {
                Flush();
            }
		}
	}
}
