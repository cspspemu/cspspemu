using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CSharpUtils.Web._45.Streams
{
	public class MixedReaderAsync
	{
		protected Stream BaseStream;
		ProduceConsumeBuffer<byte> Buffer;

		public MixedReaderAsync(Stream Stream)
		{
			BaseStream = Stream;
			Buffer = new ProduceConsumeBuffer<byte>();
		}

		async protected Task EnsureDataAsync(int MinimumBytes = 1)
		{
			while (Buffer.ConsumeRemaining < MinimumBytes)
			{
				var Data = new byte[1024];
				var Readed = await BaseStream.ReadAsync(Data, 0, 1024);
				Buffer.Produce(Data, 0, Readed);
			}
		}

		public async Task SkipBytesAsync(int NumberOfBytes)
		{
			await EnsureDataAsync(NumberOfBytes);
			Buffer.Consume(NumberOfBytes);
		}

		public async Task<byte[]> ReadBytesAsync(int NumberOfBytes)
		{
			await EnsureDataAsync(NumberOfBytes);
			return Buffer.Consume(NumberOfBytes);
		}

		public async Task<byte[]> ReadBytesUntilAsync(byte End)
		{
			var Bytes = new List<byte>();
			while (true)
			{
				await EnsureDataAsync(1);
				var SubBytes = Buffer.Consume(1);
				if (SubBytes[0] == End) break;
				Bytes.AddRange(SubBytes);
			}
			return Bytes.ToArray();
		}
	}
}
