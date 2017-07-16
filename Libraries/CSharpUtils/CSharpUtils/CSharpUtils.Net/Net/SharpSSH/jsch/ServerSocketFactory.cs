using System;
using System.Net;
using System.Net.Sockets;

namespace Tamir.SharpSsh.jsch
{
	/// <summary>
	/// Summary description for ServerSocketFactory.
	/// </summary>
	public interface ServerSocketFactory
	{
		TcpListener createServerSocket(int port, int backlog, IPAddress bindAddr);
	}
}
