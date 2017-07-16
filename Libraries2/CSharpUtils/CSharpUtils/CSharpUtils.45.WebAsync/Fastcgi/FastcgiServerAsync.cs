using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpUtils.Web._45.Fastcgi
{
	public abstract class FastcgiServerAsync
	{
		protected TcpListener TcpListener;
		protected CancellationToken CancellationToken;
		public bool Debug = false;

		public abstract Task HandleRequestAsync(FastcgiRequestAsync Request, FastcgiResponseAsync Response);

		public void ListenAsyncAndWait(ushort Port, string Address = "0.0.0.0")
		{
			ListenAsync(Port, Address).Wait();
		}

		public async Task ListenAsync(ushort Port, string Address = "0.0.0.0")
		{
			TcpListener = new TcpListener(IPAddress.Parse(Address), Port);
			TcpListener.Start();
			//if (Debug)
			{
				await Console.Out.WriteLineAsync(String.Format("Listening {0}:{1}", Address, Port));
			}
			
			while (true)
			{
				var FastcgiServerClientHandlerAsync = new FastcgiServerClientHandlerAsync(this, await TcpListener.AcceptTcpClientAsync());
				FastcgiServerClientHandlerAsync.Handle();
				//await FastcgiServerClientHandlerAsync.Handle();
			}
		}

		public void Listen(ushort Port, string Address = "0.0.0.0")
		{
			ListenAsync(Port, Address);
			//new Mutex().WaitOne();
			while (true)
			{
				Thread.Sleep(int.MaxValue);
			}
		}
	}
}
