using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CSharpUtils.Net
{
	/// <summary>
	/// Creates a TcpListener and connects a Socket to it.
	/// It stores the two TcpClient on the object: the local to the listener and the remote one.
	/// Very useful for unittesting components that requires sockets.
	/// </summary>
	public class TcpTestServer
	{
		public TcpListener TcpListener;
		public TcpClient LocalTcpClient;
		public TcpClient RemoteTcpClient;

		public static TcpTestServer Create()
		{
			var TcpTestServer = new TcpTestServer();
			{
				var BindIp = "127.0.0.1";
				var BindPort = NetworkUtilities.GetAvailableTcpPort();
				TcpTestServer.TcpListener = new TcpListener(IPAddress.Parse(BindIp), BindPort);
				TcpTestServer.TcpListener.Start();
				var Event = new ManualResetEvent(false);
				TcpTestServer.TcpListener.BeginAcceptTcpClient((AsyncResult) =>
				{
					TcpTestServer.LocalTcpClient = TcpTestServer.TcpListener.EndAcceptTcpClient(AsyncResult);
					Event.Set();
				}, null);
				TcpTestServer.RemoteTcpClient = new TcpClient(BindIp, BindPort);
				Event.WaitOne();
			}
			return TcpTestServer;
		}
	}
}
