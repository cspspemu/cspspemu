using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CSharpUtils.Web._45.Fastcgi
{
	public class FastcgiServerClientHandlerAsync
	{
		public FastcgiServerAsync FastcgiServerAsync { get; set; }
		protected TcpClient Client;
		protected Dictionary<ushort, FastcgiServerClientRequestHandlerAsync> Handlers;

		public FastcgiServerClientHandlerAsync(FastcgiServerAsync FastcgiServerAsync, TcpClient Client)
		{
			this.FastcgiServerAsync = FastcgiServerAsync;
			this.Client = Client;
			this.Handlers = new Dictionary<ushort, FastcgiServerClientRequestHandlerAsync>();
		}

		public async Task Handle()
		{
			if (FastcgiServerAsync.Debug) await Console.Out.WriteLineAsync(String.Format("Handling Client"));
			var ClientStream = Client.GetStream();

			try
			{
				while (Client.Connected)
				{
					FastcgiPacket Packet;
					try
					{
						Packet = await new FastcgiPacket().ReadFromAsync(ClientStream);
					}
					catch (IOException)
					{
						Console.Error.WriteLineAsync("Error Reading");
						break;
					}
					FastcgiServerClientRequestHandlerAsync Handler;
					if (!this.Handlers.TryGetValue(Packet.RequestId, out Handler))
					{
						Handler = this.Handlers[Packet.RequestId] = new FastcgiServerClientRequestHandlerAsync(this, ClientStream, Packet.RequestId);
					}
					await Handler.HandlePacket(Client, Packet);
				}
			}
			catch (IOException IOException)
			{
				if (FastcgiServerAsync.Debug)
				{
					Console.Error.WriteAsync(IOException.ToString());
				}
			}
		}
	}
}
