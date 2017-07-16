using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;

namespace CSharpUtils.Streams
{
	public class SocketStream : Stream
	{
		Socket Socket;

		public SocketStream(Socket Socket)
		{
			this.Socket = Socket;
			this.Socket.ReceiveBufferSize = 4096;
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

		/*
		ProduceConsumeBuffer<byte> ProduceConsumeBuffer = new ProduceConsumeBuffer<byte>();

		protected void FillBuffer()
		{
			int Available = Socket.Available;
			if (Available > 0) {
				var TempBuffer = new byte[Available];
				ProduceConsumeBuffer.Produce(TempBuffer, 0, Socket.Receive(TempBuffer));
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			FillBuffer();

			count = Math.Min(count, ProduceConsumeBuffer.ConsumeRemaining);
			//if (count == 0) return 0;

			return ProduceConsumeBuffer.Consume(buffer, offset, count);
		}
		*/

		public override int Read(byte[] buffer, int offset, int count)
		{
			return Socket.Receive(buffer, offset, count, SocketFlags.None);
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
			Socket.Send(buffer, offset, count, SocketFlags.None);
		}
	}
}
