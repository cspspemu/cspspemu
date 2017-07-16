using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;

namespace CSharpUtils.Net
{
	public class NetworkUtilities
	{
		/*
		public static int GetAvailablePort()
		{
			throw(new NotImplementedException());
		}
		*/

		public static ushort GetAvailableTcpPort(ushort StartingPort = 10101)
		{
			var IsPortBusy = new Dictionary<ushort, bool>();

			foreach (var TcpConnectionInfo in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
			{
				IsPortBusy[(ushort)TcpConnectionInfo.LocalEndPoint.Port] = true;
				/*
				Console.WriteLine(
					"{0} -> {1} :: {2}",
					tcpi.LocalEndPoint,
					tcpi.RemoteEndPoint,
					Enum.GetName(typeof(TcpState), tcpi.State)
				);
				*/
			}

			for (int Port = StartingPort; Port < UInt16.MaxValue; Port++)
			{
				if (!IsPortBusy.ContainsKey((ushort)Port))
				{
					var TestTcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), (ushort)Port);
					try
					{
						TestTcpListener.Start();

						return (ushort)Port;
					}
					finally
					{
						TestTcpListener.Stop();
					}
				}
			}

			throw (new KeyNotFoundException("Can't find any free port"));
		}
	}
}
