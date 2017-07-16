using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace CSharpUtils.Http.Server
{
	public class HttpServer
	{
		String Address;
		ushort Port;
		TcpListener TcpListener;

		public void Listen(ushort Port = 80, string Address = "0.0.0.0", bool Launch = false)
		{
			this.Port = Port;
			this.Address = Address;
			TcpListener = new TcpListener(IPAddress.Parse(Address), Port);
			Console.Write("Listening: {0}:{1}...", Address, Port);
			TcpListener.Start(16);
			//TcpListener.Start();
			Console.WriteLine("Ok");

			if (Launch)
			{
				System.Diagnostics.Process.Start("http://localhost:8080/");
			}

			while (true)
			{
				ThreadPool.QueueUserWorkItem(HandleAcceptedSocket, TcpListener.AcceptSocket());
			}
		}

		protected void HandleAcceptedSocket(Object _Socket)
		{
			Stopwatch Stopwatch = new Stopwatch();
			Stopwatch.Start();

			using (var Socket = (Socket)_Socket)
			//using (var SocketStream = new BufferedStream(new SocketStream(Socket)))
			using (var SocketStream = new NetworkStream(Socket))
			using (var SocketStreamReader = new StreamReader(SocketStream))
			using (var SocketStreamWriter = new StreamWriter(SocketStream))
			{
				SocketStreamWriter.NewLine = "\r\n";

				List<string> HeaderLines = new List<string>();

				while (Socket.Connected)
				{
					string Line = SocketStreamReader.ReadLine();
					if (Line == null || Line.Length == 0) break;
					HeaderLines.Add(Line.Trim());
				}

				//HeaderLines[0];

				foreach (var Line in HeaderLines.Skip(1))
				{
					string[] Components = Line.Split(new string[] { ":" }, 2, StringSplitOptions.None);

					string NormalizedKey = Components[0].Trim().ToLower();
					string NormalizedValue = Components[1].Trim();

					switch (NormalizedKey)
					{
						case "connection":
							switch (NormalizedValue.ToLower())
							{
								case "keep-alive":
									break;
								case "close":
									break;
							}
							break;
					}
				}

				SocketStreamWriter.WriteLine("HTTP/1.1 200 OK");
				SocketStreamWriter.WriteLine("Content-Type: text/html");
				SocketStreamWriter.WriteLine("Connection: close");
				SocketStreamWriter.WriteLine("");
				SocketStreamWriter.WriteLine("Hello World!");
				SocketStreamWriter.Flush();
				Socket.Close();
			}

			Stopwatch.Stop();
			Console.WriteLine(Stopwatch.Elapsed);
		}
	}
}
