using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using CSharpUtils.Net;
using CSharpUtils.Web._45.Fastcgi;

namespace CSharpUtils._45.Tests
{
	[TestFixture]
	public class FastcgiServerAsyncTest
	{
		protected class TestFastcgiServerAsync : FastcgiServerAsync
		{
			public override Task HandleRequestAsync(FastcgiRequestAsync Request, FastcgiResponseAsync Response)
			{
				throw new NotImplementedException();
			}
		}

		async protected Task TestSimpleConnectionAsync()
		{
			var FastcgiServerAsync = new TestFastcgiServerAsync();
			FastcgiServerAsync.Debug = true;
			var ServerPort = NetworkUtilities.GetAvailableTcpPort();
			FastcgiServerAsync.ListenAsync(ServerPort, "127.0.0.1");
			var Client = new TcpClient();
			await Client.ConnectAsync("127.0.0.1", ServerPort);
			var ClientStream = Client.GetStream();
			
			await new FastcgiPacket()
			{
				Version = 1,
				Type = CSharpUtils.Web._45.Fastcgi.Fastcgi.PacketType.FCGI_BEGIN_REQUEST,
				RequestId = 1,
				Content = new ArraySegment<byte>(new byte[] { 0, 0, 0 }),
			}.WriteToAsync(ClientStream);

			await new FastcgiPacket()
			{
				Version = 1,
				Type = CSharpUtils.Web._45.Fastcgi.Fastcgi.PacketType.FCGI_PARAMS,
				RequestId = 1,
				//Content = new ArraySegment<byte>(new byte[] { 0, 0 }),
				Content = new ArraySegment<byte>(new byte[] { }),
			}.WriteToAsync(ClientStream);

			await new FastcgiPacket()
			{
				Version = 1,
				Type = CSharpUtils.Web._45.Fastcgi.Fastcgi.PacketType.FCGI_STDIN,
				RequestId = 1,
				//Content = new ArraySegment<byte>(new byte[] { 0, 0 }),
				Content = new ArraySegment<byte>(new byte[] { }),
			}.WriteToAsync(ClientStream);

			await ClientStream.FlushAsync();

			for (int n = 0; n < 10; n++)
			{
				if (!Client.Connected) return;
				Thread.Sleep(TimeSpan.FromMilliseconds(10));
			}

			throw (new Exception("Socket not disconnected"));
		}

		[Test]
		public void TestSimpleConnection()
		{
			TestSimpleConnectionAsync().Wait();
		}
	}
}
