using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpUtils.Web._45.Streams
{
	public class BinaryReaderAsync
	{
		public Stream BaseStream;
		protected CancellationToken CancellationToken;

		public BinaryReaderAsync(Stream BaseStream)
		{
			this.BaseStream = BaseStream;
		}

		public BinaryReaderAsync(Stream BaseStream, CancellationToken CancellationToken)
		{
			this.BaseStream = BaseStream;
			this.CancellationToken = CancellationToken;
		}

		public async Task<ushort> ReadUInt16()
		{
			var Data = new byte[2];
			if (await this.BaseStream.ReadAsync(Data, 0, 2) != 2)
			{
				throw(new Exception("Can't read 2 bytes from Stream."));
			}
			return BitConverter.ToUInt16(Data, 0);
		}
	}
}
