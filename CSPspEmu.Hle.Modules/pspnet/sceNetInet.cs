using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;
using System.Net.Sockets;
using CSPspEmu.Hle.Managers;
using System.Net;
using CSharpUtils.Endian;
using CSharpUtils;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public class sceNetInet : HleModuleHost
	{
		HleUidPool<Socket> Sockets = new HleUidPool<Socket>();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x17943399, FirmwareVersion = 150)]
		public int sceNetInetInit()
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xA9ED66B9, FirmwareVersion = 150)]
		public int sceNetInetTerm()
		{
			return 0;
		}

		public struct sockaddr_in
		{
			/// <summary>
			/// 
			/// </summary>
			public byte sin_len;

			/// <summary>
			/// 
			/// </summary>
			public byte	sin_family;

			/// <summary>
			/// 
			/// </summary>
			public ushort_be sin_port;

			/// <summary>
			/// 
			/// </summary>
			public sceNetResolver.in_addr sin_addr;

			/// <summary>
			/// 
			/// </summary>
			public fixed byte sin_zero[8];
		};

		public struct sockaddr
		{
			/// <summary>
			/// total length
			/// </summary>
			public byte	sa_len;

			/// <summary>
			/// address family 
			/// </summary>
			public byte sa_family;

			/// <summary>
			/// actually longer; address value
			/// </summary>
			public fixed byte sa_data[14];
		}

		public enum socklen_t : int {
		}

		public enum size_t : int {
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="Address"></param>
		/// <param name="AddressLength"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xDB094E1B, FirmwareVersion = 150)]
		public int sceNetInetAccept(int SocketId, sockaddr *Address, socklen_t *AddressLength)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="Address"></param>
		/// <param name="AddressLength"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x1A33F9AE, FirmwareVersion = 150)]
		public int sceNetInetBind(int SocketId, sockaddr *Address, socklen_t AddressLength)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x8D7284EA, FirmwareVersion = 150)]
		public int sceNetInetClose(int SocketId)
		{
			Sockets.Get(SocketId).Close();
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x805502DD, FirmwareVersion = 150)]
		public void sceNetInetCloseWithRST()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="serv_addr"></param>
		/// <param name="addrlen"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x410B34AA, FirmwareVersion = 150)]
		public int sceNetInetConnect(int SocketId, sockaddr_in *serv_addr, socklen_t addrlen)
		{
			var Socket = Sockets.Get(SocketId);
			Console.WriteLine("{0}", serv_addr->sin_addr);
			Socket.Connect(new IPEndPoint(serv_addr->sin_addr.Address, serv_addr->sin_port));
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xE247B6D6, FirmwareVersion = 150)]
		public void sceNetInetGetpeername()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x162E6FD5, FirmwareVersion = 150)]
		public void sceNetInetGetsockname()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="s"></param>
		/// <param name="level"></param>
		/// <param name="optname"></param>
		/// <param name="optval"></param>
		/// <param name="optlen"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x4A114C7C, FirmwareVersion = 150)]
		public int sceNetInetGetsockopt(int s, int level, int optname, void *optval, socklen_t *optlen)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="BackLog"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xD10A1A7A, FirmwareVersion = 150)]
		public int sceNetInetListen(int SocketId, int BackLog)
		{
			Sockets.Get(SocketId).Listen(BackLog);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xFAABB1DD, FirmwareVersion = 150)]
		public void sceNetInetPoll()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="BufferPointer"></param>
		/// <param name="BufferLength"></param>
		/// <param name="Flags"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCDA85C99, FirmwareVersion = 150)]
		public int sceNetInetRecv(int SocketId, void* BufferPointer, int BufferLength, SocketFlags Flags)
		{
			var Socket = Sockets.Get(SocketId);
			var RecvBuffer = new byte[BufferLength];
			int Received = Socket.Receive(RecvBuffer, Flags);
			PointerUtils.Memcpy((byte*)BufferPointer, RecvBuffer, Received);
			return Received;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="BufferPointer"></param>
		/// <param name="BufferLength"></param>
		/// <param name="SocketFlags"></param>
		/// <param name="From"></param>
		/// <param name="FromLength"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xC91142E4, FirmwareVersion = 150)]
		public int sceNetInetRecvfrom(int SocketId, void* BufferPointer, int BufferLength, SocketFlags SocketFlags, sockaddr_in* From, socklen_t* FromLength)
		{
			var Socket = Sockets.Get(SocketId);
			EndPoint EndPoint = new IPEndPoint(From->sin_addr.Address, From->sin_port);
			return Socket.ReceiveFrom(ArrayUtils.CreateArray<byte>(BufferPointer, BufferLength), SocketFlags, ref EndPoint);
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xEECE61D2, FirmwareVersion = 150)]
		public void sceNetInetRecvmsg()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x5BE8D595, FirmwareVersion = 150)]
		public void sceNetInetSelect()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="BufferPointer"></param>
		/// <param name="BufferLength"></param>
		/// <param name="Flags"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x7AA671BC, FirmwareVersion = 150)]
		public int sceNetInetSend(int SocketId, void *BufferPointer, int BufferLength, SocketFlags Flags)
		{
			var Socket = Sockets.Get(SocketId);
			return Socket.Send(ArrayUtils.CreateArray<byte>(BufferPointer, BufferLength), Flags);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="BufferPointer"></param>
		/// <param name="BufferLength"></param>
		/// <param name="SocketFlags"></param>
		/// <param name="To"></param>
		/// <param name="ToLength"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x05038FC7, FirmwareVersion = 150)]
		public int sceNetInetSendto(int SocketId, void *BufferPointer, int BufferLength, SocketFlags SocketFlags, sockaddr_in *To, socklen_t ToLength)
		{
			var Socket = Sockets.Get(SocketId);
			return Socket.SendTo(ArrayUtils.CreateArray<byte>(BufferPointer, BufferLength), SocketFlags, new IPEndPoint(To->sin_addr.Address, To->sin_port));
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x774E36F4, FirmwareVersion = 150)]
		public void sceNetInetSendmsg()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="SocketOptionLevel"></param>
		/// <param name="SocketOptionName"></param>
		/// <param name="OptionValue"></param>
		/// <param name="OptionValueLength"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x2FE71FE7, FirmwareVersion = 150)]
		public int sceNetInetSetsockopt(int SocketId, SocketOptionLevel SocketOptionLevel, SocketOptionName SocketOptionName, void *OptionValue, int OptionValueLength)
		{
			var Socket = Sockets.Get(SocketId);
			Socket.SetSocketOption(SocketOptionLevel, SocketOptionName, ArrayUtils.CreateArray<byte>(OptionValue, OptionValueLength));
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SocketId"></param>
		/// <param name="How"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x4CFE4E56, FirmwareVersion = 150)]
		public int sceNetInetShutdown(int SocketId, SocketShutdown How)
		{
			Sockets.Get(SocketId).Shutdown(How);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="AddressFamily"></param>
		/// <param name="SocketType"></param>
		/// <param name="ProtocolType"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x8B7B220F, FirmwareVersion = 150)]
		public int sceNetInetSocket(AddressFamily AddressFamily, SocketType SocketType, ProtocolType ProtocolType)
		{
			var Socket = new Socket(AddressFamily, SocketType, ProtocolType);
			var SocketId = Sockets.Create(Socket);
			return SocketId;
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x80A21ABD, FirmwareVersion = 150)]
		public void sceNetInetSocketAbort()
		{
			//Sockets.Get(SocketId).abo
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xFBABE411, FirmwareVersion = 150)]
		public int sceNetInetGetErrno()
		{
			// 119 - sceNetInetConnect
			// 128 - send wait
			// 11 - recv wait
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xB3888AD4, FirmwareVersion = 150)]
		public void sceNetInetGetTcpcbstat()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x39B0C7D3, FirmwareVersion = 150)]
		public void sceNetInetGetUdpcbstat()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xB75D5B0A, FirmwareVersion = 150)]
		public void sceNetInetInetAddr()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x1BDF5D13, FirmwareVersion = 150)]
		public void sceNetInetInetAton()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xD0792666, FirmwareVersion = 150)]
		public void sceNetInetInetNtop()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0xE30B8C19, FirmwareVersion = 150)]
		public void sceNetInetInetPton()
		{
			throw(new NotImplementedException());
		}
	}
}
