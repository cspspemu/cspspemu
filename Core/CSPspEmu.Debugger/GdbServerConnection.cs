using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils.Extensions;

namespace CSPspEmu.Debugger
{
	public class GdbServerConnection : GdbServerConnectionBase
	{
		internal Stream outputStream;
		internal byte[] bufferedData;
	
		public GdbServerConnection(Stream outputStream)
		{
			this.outputStream = outputStream;
		}

		internal void sendPacket(string packet)
		{
			var sendPacket = generatePacketWithChecksum(packet);
		
			if (debugData)
			{
				Console.WriteLine("sendData: +%s", sendPacket);
			}
		
			outputStream.WriteByte((byte)'+');
			outputStream.WriteBytes(Encoding.ASCII.GetBytes(sendPacket));
			outputStream.Flush();
		}

		internal void handleRawPacket(byte[] data)
		{
			while (data[0] == '+') data = data.Slice(1);
			if (data[data.Length - 3] != '#')
			{
				Console.WriteLine("(2)'{0}'", Encoding.ASCII.GetString(data));
				throw(new Exception("Invalid exception"));
			}
			if (data[0] != '$')
			{
				Console.WriteLine("(1)'{0}'", Encoding.ASCII.GetString(data));
				throw(new Exception("Invalid exception"));
			}
			// @TODO: Check checksum. Not useful on a TCP.
			handlePacket(Encoding.ASCII.GetString(data.SliceWithBounds(1, -3)));
		}
	
		internal void handleRawData(byte[] data)
		{
			bufferedData = bufferedData.Concat(data);
		
			if (bufferedData.Length >= 2 && bufferedData[1] == 3)
			{
				outputStream.Close();
			}
		
			for (int n = 0; n < bufferedData.Length; n++)
			{
				if (bufferedData[n] == '#')
				{
					try
					{
						handleRawPacket(bufferedData.Slice(0, n + 3));
					}
					finally
					{
						bufferedData = bufferedData.Slice(n + 3);
						n = 0;
					}
				}
			}
		}
	}
}
