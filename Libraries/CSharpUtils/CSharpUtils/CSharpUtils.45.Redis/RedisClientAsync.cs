using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils.Web._45.Streams;

namespace CSharpUtils._45.Redis
{

	/// <summary>
	/// Simple non-binary-safe Redis Async Client.
	/// </summary>
	/// <see cref="http://redis.io/topics/protocol"/>
	public class RedisClientAsync
	{
		public class RedisResponseException : Exception
		{
			public RedisResponseException(String Message) : base(Message) { }
		}

		protected TcpClient TcpClient;
		protected NetworkStream NetworkStream;
		protected Encoding Encoding;

		public RedisClientAsync()
		{
			this.Encoding = Encoding.UTF8;
		}

		public RedisClientAsync(Encoding Encoding)
		{
			this.Encoding = Encoding;
		}

		public async Task Connect(string Host, ushort Port)
		{
			TcpClient = new TcpClient();
			await TcpClient.ConnectAsync(Host, Port);
			NetworkStream = TcpClient.GetStream();
		}

		async protected Task<object> ReadValue(MixedReaderAsync MixedReaderAsync)
		{
			var FirstLine = Encoding.ASCII.GetString(await MixedReaderAsync.ReadBytesUntilAsync((byte)'\r'));
			await MixedReaderAsync.SkipBytesAsync(1);

			switch (FirstLine[0])
			{
				// Status reply
				case '+': return FirstLine.Substring(1);
				// Error reply
				case '-': throw (new RedisResponseException(FirstLine.Substring(1)));
				// Integer reply
				case ':': return Convert.ToInt64(FirstLine.Substring(1));
				// Bulk replies
				case '$':
					var BytesToRead = Convert.ToInt32(FirstLine.Substring(1));
					if (BytesToRead == -1) return null;
					var Data = await MixedReaderAsync.ReadBytesAsync(BytesToRead);
					await MixedReaderAsync.SkipBytesAsync(2);
					return Encoding.GetString(Data);
				case '*':
					var BulksToRead = Convert.ToInt64(FirstLine.Substring(1));
					var Bulks = new object[BulksToRead];
					for (int n = 0; n < BulksToRead; n++)
					{
						Bulks[n] = await ReadValue(MixedReaderAsync);
					}
					return Bulks;

				default:
					throw(new RedisResponseException("Unknown param type '" + FirstLine[0] + "'"));
			}
		}

		public async Task<object> Command(params string[] Arguments)
		{
			var MemoryStream = new MemoryStream();

			var Command = "*" + Arguments.Length + "\r\n";
			foreach (var Argument in Arguments)
			{
				// Length of the argument.
				Command += "$" + Encoding.GetByteCount(Argument) + "\r\n";
				Command += Argument + "\r\n";
			}

			var Data = Encoding.GetBytes(Command);
			await NetworkStream.WriteAsync(Data, 0, (int)Data.Length);

			return await ReadValue(new MixedReaderAsync(NetworkStream));
		}
	}
}
