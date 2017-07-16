using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CSharpUtils.Http;

namespace CSharpUtils.Scgi
{
	public class ScgiServer
	{
		string BindIp;
		int BindPort;
		TcpListener Listener;

		public ScgiServer(string BindIp, int BindPort)
		{
			this.BindIp = BindIp;
			this.BindPort = BindPort;
		}

		public void Listen()
		{
			Listener = new TcpListener(IPAddress.Parse(this.BindIp), this.BindPort);
			Listener.Start();
		}

		public void AcceptLoop()
		{
			while (true)
			{
				var TcpClient = Listener.AcceptTcpClient();
				new Thread(() =>
				{
					HandleConnectionOnSeparateThread(TcpClient);
				}).Start();
			}
		}

		protected Dictionary<string, string> DecodeHeaders(byte[] HeaderData)
		{
			var HeaderDataStream = new MemoryStream(HeaderData);
			var Headers = new Dictionary<string, string>();
			while (!HeaderDataStream.Eof())
			{
				var Key = HeaderDataStream.ReadStringz();
				var Value = HeaderDataStream.ReadStringz();
				Headers[Key] = Value;
			}
			return Headers;
		}

		protected void HandleConnectionOnSeparateThread(TcpClient TcpClient)
		{
			var Stream = TcpClient.GetStream();
			int HeaderLength = int.Parse(Stream.ReadUntilString((byte)':', Encoding.UTF8));
			var HeaderData = Stream.ReadBytes(HeaderLength);
			int C = Stream.ReadByte();
			if (C != ',') throw (new Exception("Invalid Scgi request"));
			var Headers = DecodeHeaders(HeaderData);
			int ContentLength = int.Parse(Headers["CONTENT_LENGTH"]);
			var ContentData = Stream.ReadBytes(ContentLength);
			HandleRequest(new HttpHeaderList(), Headers, ContentData);
		}

		protected virtual void HandleRequest(HttpHeaderList HttpHeaderList, Dictionary<string, string> Parameters, byte[] PostContent)
		{
		}
	}
}
