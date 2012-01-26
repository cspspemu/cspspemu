using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Debugger
{
	public class TcpServer
	{
		Socket serverSocket;
		string ip;
		short port;
		Dictionary<Socket, bool> clients;
		//SocketSet checkRead, checkWrite, checkError;

		public TcpServer(string ip, short port)
		{
			/*
			this.ip = ip;
			this.port = port;
			this.serverSocket = new TcpSocket();
			this.serverSocket.blocking = false;
			this.checkRead = new SocketSet();
			this.checkWrite = new SocketSet();
			this.checkError = new SocketSet();
			*/
		}

		public void listen(int backlog = 16)
		{
			/*
			writef("Starting server %s:%d...", ip, port);
			serverSocket.bind(new InternetAddress(ip, port));
			serverSocket.listen(backlog);
			writefln("Ok");
		
			while (true) {
				checkRead.reset();
				checkWrite.reset();
				checkError.reset();
			
				checkRead.add(serverSocket);
				checkWrite.add(serverSocket);
				checkError.add(serverSocket);
			
				foreach (ref clientSocket; clients.keys) {
					checkRead.add(clientSocket);
					//checkWrite.add(clientSocket);
					checkError.add(clientSocket);
				}
			
				serverSocket.select(checkRead, checkWrite, checkError);
			
				// New connection available.
				if (checkRead.isSet(serverSocket)) {
					Socket clientSocket = serverSocket.accept();
					clientSocket.blocking = false;
					clients[clientSocket] = true;
					try {
						handleConnect(clientSocket);
					} catch (Throwable o) {
						writefln("%s", o);
					}
				}
			
				foreach (ref clientSocket; clients.keys.dup) {
					// Data available.
					if (checkRead.isSet(clientSocket)) {
						ubyte[] buffer = new ubyte[1024];
						int bufferLen;
						while (true) {
							bufferLen = clientSocket.receive(buffer);
							if (bufferLen <= 0) break;
							try {
								handleData(clientSocket, buffer[0..bufferLen]);
							} catch (Throwable o) {
								writefln("%s", o);
							}
						}
						if (bufferLen == 0) {
							clients.remove(clientSocket);
							try {
								handleDisconnect(clientSocket);
							} catch (Throwable o) {
								writefln("%s", o);
							}
						}
					}
				}
			
				Thread.sleep(dur!"msecs"(1));
			}
			*/
		}
	
		protected virtual void handleConnect(Socket socket)
		{
			Console.WriteLine("onConnect: %s", socket);
		}
	
		protected virtual void handleData(Socket socket, byte[] data) {
			Console.WriteLine("onData: %s : %s : %s", socket, data, BitConverter.ToString(data));
		}

		protected virtual void handleDisconnect(Socket socket)
		{
			Console.WriteLine("onDisconnect: %s", socket);
		}
	}
}
