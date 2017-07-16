using System;
using System.Linq;
using CSPspEmu.Hle.Attributes;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using System.Net.NetworkInformation;
using CSPspEmu.Hle.Managers;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | (ModuleFlags)0x00010011)]
	public unsafe class sceNetAdhocMatching : HleModuleHost
	{
		/// <summary>
		/// Linked list for sceNetAdhocMatchingGetMembers
		/// </summary>
		public struct pspAdhocPoolStat
		{
			/// <summary>
			/// Size of the pool
			/// </summary>
			public int size;

			/// <summary>
			/// Maximum size of the pool
			/// </summary>
			public int maxsize;

			/// <summary>
			/// Unused memory in the pool
			/// </summary>
			public int freesize;
		};

		pspAdhocPoolStat PoolStat;

		/// <summary>
		/// Initialise the Adhoc matching library
		/// </summary>
		/// <param name="MemSize">Internal memory pool size. Lumines uses 0x20000</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x2A2A1E07, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingInit(int MemSize)
		{
			PoolStat.size = MemSize;
			PoolStat.maxsize = MemSize;
			PoolStat.freesize = MemSize;
			return 0;
		}

		/// <summary>
		/// Terminate the Adhoc matching library
		/// </summary>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x7945ECDA, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingTerm()
		{
			return 0;
		}

		/// <summary>
		/// pspAdhocMatchingCallback : PSP_ADHOC_MATCHING_EVENT_
		/// </summary>
		public enum Event : int
		{
			/// <summary>
			/// Hello event. optdata contains data if optlen > 0.
			/// </summary>
			Hello = 1,

			/// <summary>
			/// Join request. optdata contains data if optlen > 0.
			/// </summary>
			Join = 2,
			
			/// <summary>
			/// Target left matching.
			/// </summary>
			Left = 3,
			
			/// <summary>
			/// Join request rejected.
			/// </summary>
			Reject = 4,
			
			/// <summary>
			/// Join request cancelled.
			/// </summary>
			Cancel = 5,
			
			/// <summary>
			/// Join request accepted. optdata contains data if optlen > 0.
			/// </summary>
			Accept = 6,
			
			/// <summary>
			/// Matching is complete.
			/// </summary>
			Complete = 7,
			
			/// <summary>
			/// Ping timeout event.
			/// </summary>
			Timeout = 8,
			
			/// <summary>
			/// Error event.
			/// </summary>
			Error = 9,
			
			/// <summary>
			/// Peer disconnect event.
			/// </summary>
			Disconnect = 10,

			/// <summary>
			/// Data received event. optdata contains data if optlen > 0.
			/// </summary>
			Data = 11,
			
			/// <summary>
			/// Data acknowledged event.
			/// </summary>
			DataConfirm = 12,
			
			/// <summary>
			/// Data timeout event.
			/// </summary>
			DataTimeout = 13,

			/// <summary>
			/// Internal ping message.
			/// </summary>
			InternalPing = 100,
		}

		public enum Mode : int
		{
			Host = 1,
			Client = 2,
			PeerToPeer = 3
		};

		public class Matching : IHleUidPoolClass
		{
			[Inject]
			public HleInterop HleInterop;

			[Inject]
			public sceNetAdhocctl sceNetAdhocctl;

			[Inject]
			public HleMemoryManager HleMemoryManager;

			[Inject]
			public sceNet sceNet;

			[Inject]
			public InjectContext InjectContext;

			public Mode Mode;
			public int MaxPeers;
			public int Port;
			public int BufSize;
			public int HelloDelay;
			public int PingDelay;
			public int InitCount;
			public int MsgDelay;

			public string ConnectionName { get { return sceNetAdhocctl.ConnectionName; } }

			/// <summary>
			/// typedef void (*pspAdhocMatchingCallback)(int matchingid, Event event, unsigned char *mac, int optlen, void *optdata);
			/// </summary>
			public uint Callback;

			public Matching(InjectContext InjectContext)
			{
				InjectContext.InjectDependencesTo(this);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="Event"></param>
			/// <param name="Mac"></param>
			/// <param name="Data"></param>
			public void NotifyEvent(Event Event, MacAddress Mac, byte[] Data)
			{
				var MacPartition = HleMemoryManager.GetPartition(MemoryPartitions.User).Allocate(8);
				PointerUtils.Memcpy((byte*)MacPartition.LowPointer, new ArraySegment<byte>(Mac.GetAddressBytes()));

				{
					var DataPartition = HleMemoryManager.GetPartition(MemoryPartitions.User).Allocate(Data.Length);
					PointerUtils.Memcpy((byte*)DataPartition.LowPointer, new ArraySegment<byte>(Data));

					Console.WriteLine("Executing callback. Matching.NotifyEvent: 0x{0:X8}, {1}, {2}, 0x{3:X8}, {4}, 0x{5:X8}", this.Callback, this.GetUidIndex(InjectContext), Event, MacPartition.Low, DataPartition.Size, DataPartition.Low);
					HleInterop.ExecuteFunctionLater(
						this.Callback,
						this.GetUidIndex(InjectContext),
						(int)Event,
						MacPartition.Low,
						DataPartition.Size,
						(DataPartition.Size != 0) ? DataPartition.Low : 0
					);

					DataPartition.DeallocateFromParent();
				}
				MacPartition.DeallocateFromParent();
			}

			public void Start()
			{
				(this.MainThread = new Thread(ThreadMain)
				{
					Name = "sceNetAdhocMatching",
					IsBackground = true,
				}).Start();
			}

			Thread MainThread;
			Thread HelloThread;
			Thread PingThread;

			public void ThreadMain()
			{
				(HelloThread = new Thread(() => { while (true) { SendHello(); Thread.Sleep(HelloDelay / 1000); } }) { Name = "sceNetAdhocMatching.HelloThread", IsBackground = true }).Start();
				(PingThread = new Thread(() => { while (true) { SendPing(); Thread.Sleep(PingDelay / 1000); } }) { Name = "sceNetAdhocMatching.PingThread", IsBackground = true }).Start();

				var Socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
				Socket.ExclusiveAddressUse = false;
				Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				Socket.Bind(GetBindEndPoint());

				var Buffer = new byte[0x1000];

				while (true)
				{
					//Console.WriteLine("Packet Waiting...");
					EndPoint EndPoint = new IPEndPoint(IPAddress.Any, 0);
					var BufferLength = Socket.ReceiveFrom(Buffer, SocketFlags.None, ref EndPoint);
					//Console.WriteLine("Packet Received...");
					var Packet = new MemoryStream(Buffer, 0, BufferLength);
					var PacketHeader = Packet.ReadStruct<Header>();
					var PacketData = Packet.ReadBytes(PacketHeader.DataLength);
					if (sceNet.SelfMacAddress != PacketHeader.FromMacAddress)
					{
						MessageReceived(PacketHeader, PacketData);
					}
					else
					{
						//Console.WriteLine("Packet from myself");
					}
				}
			}

			bool ReceivedJoinMacAddressPending;
			MacAddress ReceivedJoinMacAddress;

			private void MessageReceived(Header PacketHeader, byte[] PacketData)
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Blue, () =>
				{
					Console.WriteLine("> Matching.MessageReceived(): {0}", PacketHeader);
					ArrayUtils.HexDump(PacketData);
				});
				if (PacketHeader.Event == Event.Join)
				{
					ReceivedJoinMacAddress = PacketHeader.FromMacAddress;
					ReceivedJoinMacAddressPending = true;
				}
				if (PacketHeader.Event != Event.InternalPing)
				{
					NotifyEvent(PacketHeader.Event, PacketHeader.FromMacAddress, PacketData);
				}
			}

			public void MessageSend(Event Event, MacAddress ToMacAddress, byte[] Data = null)
			{
				if (Data == null) Data = new byte[0];
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Green, () =>
				{
					Console.WriteLine(
						"< Matching.SendMessage(Event={0}, From={1}, To={2}, Data={3})",
						Event,
						sceNet.SelfMacAddress.ToString(),
						ToMacAddress.ToString(),
						BitConverter.ToString(Data)
					);
					ArrayUtils.HexDump(Data);
				});
				var Out = new MemoryStream();
				Out.WriteStruct(new Header() { Event = Event, FromMacAddress = sceNet.SelfMacAddress, ToMacAddress = ToMacAddress, DataLength = Data.Length });
				Out.WriteBytes(Data);
				SendRaw(Out.ToArray());
			}

			public int GetBindedPort()
			{
				return 1234 + this.Port;
			}

			public IPEndPoint GetBindEndPoint()
			{
				return new IPEndPoint(IPAddress.Any, GetBindedPort());
				//return new IPEndPoint(IPAddress.Parse("192.168.1.39"), GetBindedPort());
				//return new IPEndPoint(IPAddress.Parse("127.0.0.1"), GetBindedPort());
			}

			/*
			public IPEndPoint GetSendEndPoint()
			{
				return new IPEndPoint(IPAddress.Parse("192.168.1.255"), GetBindedPort());
				//return new IPEndPoint(IPAddress.Broadcast, GetBindedPort());
			}
			*/

			public void Dispose()
			{
				HelloThread.Abort();
				PingThread.Abort();
				MainThread.Abort();
			}

			public void Stop()
			{
			}

			public struct Header
			{
				public Event Event;
				public MacAddress FromMacAddress;
				public MacAddress ToMacAddress;
				public int DataLength;

				public override string ToString()
				{
					return String.Format("Matching.Header({0}, {1}, {2}, {3})", Event, FromMacAddress, ToMacAddress, DataLength);
				}
			}

			public void SendRaw(byte[] Data)
			{
				//var EndPoint1 = GetSendEndPoint();
				var EndPoint2 = new IPEndPoint(IPAddress.Broadcast, GetBindedPort());

				var Socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
				Socket.ExclusiveAddressUse = false;
				Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				//Socket.SendTo(Data, EndPoint1);
				Socket.SendTo(Data, EndPoint2);
			}

			public void SendPing()
			{
				MessageSend(Event.InternalPing, MacAddress.All);
			}

			public void SendHello()
			{
				if (HelloData != null) MessageSend(Event.Hello, MacAddress.All, HelloData);
			}

			public void SendData(MacAddress MacAddress, byte[] Data)
			{
				MessageSend(Event.Data, MacAddress, Data);
			}

			public void SelectTarget(MacAddress MacAddress, byte[] Data)
			{
				if (ReceivedJoinMacAddressPending && MacAddress == ReceivedJoinMacAddress)
				{
					ReceivedJoinMacAddressPending = false;
					MessageSend(Event.Accept, MacAddress, Data);
				}
				else
				{
					MessageSend(Event.Join, MacAddress, Data);
				}
			}

			byte[] HelloData;

			public void SetHelloOpt(byte[] Data)
			{
				HelloData = Data;
			}

			public byte[] GetHelloOpt()
			{
				return HelloData;
			}

			public void CancelTarget(MacAddress MacAddress, byte[] Data = null)
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Create an Adhoc matching object
		/// </summary>
		/// <param name="Mode">One of pspAdhocMatchingModes</param>
		/// <param name="MaxPeers">Maximum number of peers to match (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST)</param>
		/// <param name="Port">Port. Lumines uses 0x22B</param>
		/// <param name="BufSize">Receiving buffer size</param>
		/// <param name="HelloDelay">Hello message send delay in microseconds (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST or PSP_ADHOC_MATCHING_MODE_PTP)</param>
		/// <param name="PingDelay">Ping send delay in microseconds. Lumines uses 0x5B8D80 (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST or PSP_ADHOC_MATCHING_MODE_PTP)</param>
		/// <param name="InitCount">Initial count of the of the resend counter. Lumines uses 3</param>
		/// <param name="MsgDelay">Message send delay in microseconds</param>
		/// <param name="Callback">Callback to be called for matching</param>
		/// <returns>ID of object on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xCA5EDA6F, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public Matching sceNetAdhocMatchingCreate(Mode Mode, int MaxPeers, int Port, int BufSize, int HelloDelay, int PingDelay, int InitCount, int MsgDelay, uint Callback)
		{
			//throw (new NotImplementedException());
			return new Matching(InjectContext)
			{
				Mode = Mode,
				MaxPeers = MaxPeers,
				Port = Port,
				BufSize = BufSize,
				HelloDelay = HelloDelay,
				PingDelay = PingDelay,
				InitCount = InitCount,
				MsgDelay = MsgDelay,
				Callback = Callback,
			};
		}

		/// <summary>
		/// Start a matching object
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="evthPri">Priority of the event handler thread. Lumines uses 0x10</param>
		/// <param name="evthStack">Stack size of the event handler thread. Lumines uses 0x2000</param>
		/// <param name="inthPri">Priority of the input handler thread. Lumines uses 0x10</param>
		/// <param name="inthStack">Stack size of the input handler thread. Lumines uses 0x2000</param>
		/// <param name="optLen">Size of hellodata</param>
		/// <param name="optData">Pointer to block of data passed to callback</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x93EF3843, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingStart(Matching Matching, int evthPri, int evthStack, int inthPri, int inthStack, int optLen, byte* optData)
		{
			sceNetAdhocMatchingSetHelloOpt(Matching, optLen, optData);
			Matching.Start();
			return 0;
		}

		/// <summary>
		/// Stop a matching object
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x32B156B3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingStop(Matching Matching)
		{
			Matching.Stop();
			return 0;
		}

		/// <summary>
		/// Delete an Adhoc matching object
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xF16EAF4F, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingDelete(Matching Matching)
		{
			Matching.RemoveUid(InjectContext);
			return 0;
		}

		/// <summary>
		/// Send data to a matching target
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="MacAddress">The MAC address to send the data to</param>
		/// <param name="DataLength">Length of the data</param>
		/// <param name="DataPointer">Pointer to the data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xF79472D7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingSendData(Matching Matching, ref MacAddress MacAddress, int DataLength, byte* DataPointer)
		{
			Matching.SendData(MacAddress, PointerUtils.PointerToByteArray(DataPointer, DataLength));
			return 0;
		}

		/// <summary>
		/// Abort a data send to a matching target
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="macAddr">The MAC address to send the data to</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xEC19337D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingAbortSendData(Matching Matching, ref MacAddress MacAddress)
		{
			return 0;
		}

		/// <summary>
		/// Select a matching target
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="macAddr">MAC address to select</param>
		/// <param name="DataLength">Optional data length</param>
		/// <param name="DataPointer">Pointer to the optional data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x5E3D4B79, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingSelectTarget(Matching Matching, ref MacAddress MacAddress, int DataLength, byte* DataPointer)
		{
			Matching.SelectTarget(MacAddress, PointerUtils.PointerToByteArray(DataPointer, DataLength));
			return 0;
		}

		/// <summary>
		/// Cancel a matching target
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="macAddr">The MAC address to cancel</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xEA3C6108, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingCancelTarget(Matching Matching, ref MacAddress MacAddress)
		{
			Matching.CancelTarget(MacAddress);
			return 0;
		}

		/// <summary>
		/// Cancel a matching target (with optional data)
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="macAddr">The MAC address to cancel</param>
		/// <param name="optLen">Optional data length</param>
		/// <param name="optData">Pointer to the optional data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x8F58BEDF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingCancelTargetWithOpt(Matching Matching, ref MacAddress MacAddress, int DataLength, byte* DataPointer)
		{
			Matching.CancelTarget(MacAddress, PointerUtils.PointerToByteArray(DataPointer, DataLength));
			return 0;
		}

		/// <summary>
		/// Get the optional hello message
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="DataLength">Length of the hello data (input/output)</param>
		/// <param name="DataPointer">Pointer to the hello data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB5D96C2A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetHelloOpt(Matching Matching, ref int DataLength, byte* DataPointer)
		{
			var Data = Matching.GetHelloOpt();
			DataLength = Math.Min(Data.Length, DataLength);
			PointerUtils.Memcpy(DataPointer, Data, DataLength);
			return 0;
		}

		/// <summary>
		/// Set the optional hello message
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="DataLength">Length of the hello data</param>
		/// <param name="DataPointer">Pointer to the hello data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB58E61B7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingSetHelloOpt(Matching Matching, int DataLength, byte* DataPointer)
		{
			var Data = PointerUtils.PointerToByteArray(DataPointer, DataLength);
			Matching.SetHelloOpt(Data);
			return 0;
		}

		public struct MemberInfo
		{
			public PspPointer MemberInfoNext;
			public fixed byte Mac[8];
		}

		/// <summary>
		/// Get a list of matching members
		/// </summary>
		/// <param name="Matching">The ID returned from <see cref="sceNetAdhocMatchingCreate"/></param>
		/// <param name="sizeAddr">The length of the list.</param>
		/// <param name="buf">An allocated area of size length.</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xC58BCD9E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetMembers(Matching Matching, uint* sizeAddr, void* buf)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get the status of the memory pool used by the matching library
		/// </summary>
		/// <param name="PoolStat">A ::pspAdhocPoolStat.</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x9C5CFB7D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetPoolStat(out pspAdhocPoolStat PoolStat)
		{
			PoolStat = this.PoolStat;
			return 0;
		}

		/// <summary>
		/// Get the maximum memory usage by the matching library
		/// </summary>
		/// <returns>The memory usage on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x40F8F435, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetPoolMaxAlloc()
		{
			throw (new NotImplementedException());
		}
	}

	unsafe public struct MacAddress
	{
		public static readonly MacAddress All = new MacAddress();

		private fixed byte Data[8];

		public byte[] GetAddressBytes()
		{
			fixed (byte* DataPointer = Data) return PointerUtils.PointerToByteArray(DataPointer, 8);
		}

		public override string ToString()
		{
			return BitConverter.ToString(GetAddressBytes());
		}

		public static MacAddress GenerateRandom()
		{
			var MacAddress = new MacAddress();
			*(uint*)MacAddress.Data = (uint)DateTime.UtcNow.GetTotalMicroseconds();
			return MacAddress;
		}

		public static MacAddress GetNative()
		{
			var MacAddress = new MacAddress();
			var PhysicalAddress = NetworkInterface.GetAllNetworkInterfaces().First(nic => nic.OperationalStatus == OperationalStatus.Up).GetPhysicalAddress();
			var Bytes = PhysicalAddress.GetAddressBytes();
			for (int n = 0; n < 8; n++) MacAddress.Data[n] = (n < Bytes.Length) ? Bytes[n] : (byte)0;
			return MacAddress;
		}

		static public bool operator ==(MacAddress a, MacAddress b)
		{
			return a.GetAddressBytes().SequenceEqual(b.GetAddressBytes());
		}

		static public bool operator !=(MacAddress a, MacAddress b)
		{
			return !a.GetAddressBytes().SequenceEqual(b.GetAddressBytes());
		}
	}
}
