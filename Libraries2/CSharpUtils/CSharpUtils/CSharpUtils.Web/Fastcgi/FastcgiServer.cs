using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Reflection;
using System.Threading;

namespace CSharpUtils.Fastcgi
{
	public class FastcgiServer
	{
		ManualResetEvent AcceptMutex;
		String Address;
		ushort Port;
		//TcpListener TcpListener;
		Socket ServerSocket;
		bool Async = false;
		bool Debug = false;
		//bool Debug = true;

		//[DllImport("Kernel32")]
		//public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

		// A delegate type to be used as the handler routine 
		// for SetConsoleCtrlHandler.
		//public delegate bool HandlerRoutine(CtrlTypes CtrlType);

		// An enumerated type for the control messages
		// sent to the handler routine.
		public enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT
		}

		event Action OnShutdown;

		/// <summary>
		/// Handles the shutdown of the server to be able to execute code.
		/// </summary>
		/// <param name="CtrlType"></param>
		/// <returns>True to avoid exiting. False to allow exiting.</returns>
		private bool HandleShutdown(CtrlTypes CtrlType)
		{
			Console.WriteLine("Closing... ({0})", CtrlType);
			if (OnShutdown != null) OnShutdown();
			return false;
		}

		public void Listen(string UnixEndPoint)
		{
			ServerSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);

			//EndPoint EndPoint = (EndPoint)Assembly.LoadWithPartialName("Mono.Posix").GetType("Mono.Unix.UnixEndPoint").GetConstructor(new Type[] { typeof(String) }).Invoke(null, new object[] { UnixEndPoint });
			//EndPoint EndPoint = (EndPoint)Assembly.LoadWithPartialName("Mono.Posix").GetType("Mono.Unix.UnixEndPoint").GetConstructor(new Type[] { typeof(String) }).Invoke(new object[] { UnixEndPoint });
			EndPoint EndPoint = (EndPoint)Assembly.Load("Mono.Posix").GetType("Mono.Unix.UnixEndPoint").GetConstructor(new Type[] { typeof(String) }).Invoke(new object[] { UnixEndPoint });

			Console.WriteLine(EndPoint);

			ServerSocket.Bind(EndPoint);
			Console.WriteLine("Listen at '{0}'", UnixEndPoint);
			ServerSocket.Listen(128);

			ListenCommon();
		}

		private void ListenCommon()
		{
			//SetConsoleCtrlHandler(new HandlerRoutine(HandleShutdown), true);

			while (true)
			{
				if (Debug)
				{
					Console.WriteLine("Waiting a connection...");
				}

				if (Async)
				{
					/*
					AcceptMutex.Reset();
					{
						TcpListener.BeginAcceptSocket(new AsyncCallback(HandleAcceptedCallbackAsync), TcpListener);
					}
					AcceptMutex.WaitOne();
					*/
				}
				else
				{
					//ThreadPool.QueueUserWorkItem(HandleAcceptedSocket, TcpListener.AcceptSocket());
					//ThreadPool.
					HandleAcceptedSocketInNewThread(ServerSocket.Accept());
				}
			}
		}

		public void Listen(ushort Port, string Address = "0.0.0.0")
		{
			this.Port = Port;
			this.Address = Address;
			AcceptMutex = new ManualResetEvent(false);

			ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

			ServerSocket.Bind(new IPEndPoint(IPAddress.Parse(Address), Port));
			Console.WriteLine("Listen at {0}:{1}", Address, Port);
			ServerSocket.Listen(128);

			ListenCommon();
		}

		protected void HandleAcceptedSocketInNewThread(Socket AcceptedSocket)
		{
			ThreadPool.QueueUserWorkItem(HandleAcceptedSocket, AcceptedSocket);
			/*
			new Thread(delegate()
			{
				HandleAcceptedSocket(AcceptedSocket);
			}).Start();
			*/
		}

		/*
		class ConnectionState
		{
			public AsyncCallback ReadAsyncCallback;
			public Socket ClientSocket;
			public const int DataSize = 1024;
			public byte[] Data = new byte[DataSize];
			public ProduceConsumeBuffer<byte> Buffer = new ProduceConsumeBuffer<byte>();

			public ConnectionState(Socket ClientSocket, AsyncCallback ReadAsyncCallback)
			{
				this.ClientSocket = ClientSocket;
				this.ReadAsyncCallback = ReadAsyncCallback;
			}

			public void BeginReceive()
			{
				ClientSocket.BeginReceive(Data, 0, DataSize, 0, ReadAsyncCallback, this);
			}
		}

		void HandleAcceptedCallbackAsync(IAsyncResult AsyncResult) {
			Socket ClientSocket = TcpListener.EndAcceptSocket(AsyncResult);

			AcceptMutex.Set();

			var ConnectionState = new ConnectionState(ClientSocket, new AsyncCallback(HandleReadCallbackAsync));
			ConnectionState.BeginReceive();

			Console.WriteLine("HandleAcceptedCallbackAsync");
		}

		void HandleReadCallbackAsync(IAsyncResult AsyncResult)
		{
			ConnectionState ConnectionState = (ConnectionState)AsyncResult.AsyncState;
			Socket ClientSocket = ConnectionState.ClientSocket;
			int ReadedCount = ClientSocket.EndReceive(AsyncResult);

			Console.WriteLine("HandleReadCallbackAsync");

			if (ReadedCount > 0)
			{
				ConnectionState.Buffer.Produce(ConnectionState.Data, 0, ReadedCount);
				Console.WriteLine(ConnectionState.Buffer.ConsumeRemaining);
				ConnectionState.BeginReceive();
			}
			else
			{
				ClientSocket.Close();
			}
		}
		*/

		protected void HandleAcceptedSocket(Object _Socket)
		{
			var Socket = (Socket)_Socket;
			if (Debug)
			{
				Console.WriteLine("HandleAcceptedSocket: " + Socket);
			}
			//Console.Write(".");
			var FastcgiHandler = new FastcgiHandler(new FastcgiPipeSocket(Socket), Debug);
			FastcgiHandler.HandleFastcgiRequest += HandleFascgiRequest;
			FastcgiHandler.Reader.ReadAllPackets();
		}

		protected virtual void HandleFascgiRequest(FastcgiRequest FastcgiRequest)
		{
			using (var TextWriter = new StreamWriter(FastcgiRequest.StdoutStream))
			{
				TextWriter.WriteLine("X-Dynamic: C#");
				TextWriter.WriteLine("Content-Type: text/html; charset=utf-8");
				TextWriter.WriteLine("");
				TextWriter.WriteLine("Not Implemented");
			}
		}
	}
}
