using System;
using System.IO;
using System.Threading.Tasks;

namespace CSharpUtils.Web._45.Fastcgi
{
	public class FastcgiPacket
	{
		public byte Version = 1;
		public Fastcgi.PacketType Type;
		public ushort RequestId;
		public ArraySegment<byte> Content;
		protected static byte[] PaddingWrite = new byte[8];
		protected static byte[] PaddingRead = new byte[8];

		public async Task<FastcgiPacket> WriteToAsync(Stream ClientStream)
		{
			if (Content.Count > ushort.MaxValue) throw(new InvalidDataException("Data too long"));
			var ContentLength = (ushort)Content.Count;
			var PaddingLength = (byte)(MathUtils.Align(ContentLength, 8) - ContentLength);

			var Header = new byte[8];
			Header[0] = Version;
			Header[1] = (byte)Type;
			Header[2] = (byte)((RequestId >> 8) & 0xFF);
			Header[3] = (byte)((RequestId >> 0) & 0xFF);
			Header[4] = (byte)((ContentLength >> 8) & 0xFF);
			Header[5] = (byte)((ContentLength >> 0) & 0xFF);
			Header[6] = PaddingLength;
			Header[7] = 0;
			await ClientStream.WriteAsync(Header, 0, Header.Length);
			if (ContentLength > 0)
			{
				await ClientStream.WriteAsync(Content.Array, Content.Offset, Content.Count);
			}
			if (PaddingLength > 0)
			{
				await ClientStream.WriteAsync(PaddingWrite, 0, PaddingLength);
			}

			//await Console.Out.WriteLineAsync(String.Format("WRITE: Packet({0}): {1}({2}:{3})", RequestId, Type, ContentLength, PaddingLength));

			return this;
		}

		public async Task<FastcgiPacket> ReadFromAsync(Stream ClientStream)
		{
			var Header = new byte[8];
			await ClientStream.ReadAsync(Header, 0, Header.Length);
			var Version = Header[0];
			Type = (Fastcgi.PacketType)Header[1];
			//BitConverter.ToUInt16()
			RequestId = (ushort)((Header[2] << 8) | (Header[3] << 0));
			var ContentLength = (ushort)((Header[4] << 8) | (Header[5] << 0));
			var PaddingLength = Header[6];
			Content = new ArraySegment<byte>(new byte[ContentLength]);
			if (ContentLength + PaddingLength > 0)
			{
				await ClientStream.ReadAsync(Content.Array, 0, ContentLength);
				await ClientStream.ReadAsync(PaddingRead, 0, PaddingLength);
			}

			//await Console.Out.WriteLineAsync(String.Format("READ: Packet({0}): {1}({2}:{3})", RequestId, Type, ContentLength, PaddingLength));

			return this;
		}

		public async static Task WriteMemoryStreamToAsync(ushort RequestId, Fastcgi.PacketType PacketType, MemoryStream From, Stream ClientStream)
		{
			var Buffer = From.GetBuffer();
			var BufferRealLength = (int)From.Length;

			for (int n = 0; n < BufferRealLength; n += ushort.MaxValue)
			{
				await new FastcgiPacket()
				{
					RequestId = RequestId,
					Type = PacketType,
					Content = new ArraySegment<byte>(Buffer, n, Math.Min(ushort.MaxValue, BufferRealLength - n)),
				}.WriteToAsync(ClientStream);
			}
		}
	}
}
