using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Debugger
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="gdb/m32r-stub.c"/>
	/// <seealso cref="http://www.embecosm.com/appnotes/ean4/html/ch04s07s03.html"/>
	/// <seealso cref="http://davis.lbl.gov/Manuals/GDB/gdb_31.html"/>
	/// <seealso cref="http://www.embecosm.com/appnotes/ean4/embecosm-howto-rsp-server-ean4-issue-2.html"/>
	public class GdbServer : TcpServer
	{
		Dictionary<Socket, GdbServerConnection> clientBySocket;

		public GdbServer()
			: base("127.0.0.1", 23946)
		{
		}
	
		override protected void handleConnect(Socket socket) {
			base.handleConnect(socket);
			clientBySocket[socket] = new GdbServerConnection(new NetworkStream(socket, FileAccess.Write));
		}

		override protected void handleData(Socket socket, byte[] data)
		{
			//super.handleData(socket, data);
			clientBySocket[socket].handleRawData(data);
		}

		override protected void handleDisconnect(Socket socket)
		{
			base.handleDisconnect(socket);
			clientBySocket.Remove(socket);
		}
	}
}
