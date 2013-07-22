//#define DEBUG_MSG_PIPES

using System;
using System.Collections.Generic;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.threadman
{
	public unsafe partial class ThreadManForUser
	{
		[Flags]
		public enum MsgPipeAttributes : uint
		{
			/// <summary>
			/// Allocate the pipe in a high address.
			/// </summary>
			UseHighAddress = 0x4000,
		}

		[Flags]
		public enum MsgPipeReceiveAttributes : uint
		{
			PSP_MPP_ATTR_RECEIVE_FIFO = 0,
			PSP_MPP_ATTR_RECEIVE_PRIORITY = 0x1000,

			PSP_MPP_ATTR_RECEIVE = PSP_MPP_ATTR_RECEIVE_FIFO | PSP_MPP_ATTR_RECEIVE_PRIORITY,
		}

		[Flags]
		public enum MsgPipeSendAttributes : uint
		{
			PSP_MPP_ATTR_SEND_FIFO = 0,
			PSP_MPP_ATTR_SEND_PRIORITY = 0x100,
			PSP_MPP_ATTR_SEND = PSP_MPP_ATTR_SEND_FIFO | PSP_MPP_ATTR_SEND_PRIORITY,
		}

		[HleUidPoolClass(NotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE)]
		public class MsgPipe : IHleUidPoolClass, IDisposable
		{
			public string Name;
			public MsgPipeAttributes Attributes;
			public MemoryPartitions PartitionId;
			public int Size;
			protected PspMemory PspMemory;
			protected MemoryPartition PoolPartition;
			public Queue<MemoryPartition> Messages = new Queue<MemoryPartition>();
			public Queue<Action> OnAvailableForSend = new Queue<Action>();
			public Queue<Action> OnAvailableForRecv = new Queue<Action>();
			private HleThreadManager ThreadManager;

			public void NoticeAvailableForSend()
			{
				while (OnAvailableForSend.Count > 0)
				{
#if DEBUG_MSG_PIPES
					Console.Error.WriteLine("MsgPipe.NoticeAvailableForSend");
#endif
					//ThreadManager.Reschedule();
					OnAvailableForSend.Dequeue()();
					//ThreadManager.Current.CpuThreadState.Yield();
				}
			}

			public void NoticeAvailableForRecv()
			{
				while (OnAvailableForRecv.Count > 0)
				{
#if DEBUG_MSG_PIPES
					Console.Error.WriteLine("MsgPipe.NoticeAvailableForRecv");
#endif
					//ThreadManager.Reschedule();
					//ThreadManager.Reschedule();
					OnAvailableForRecv.Dequeue()();
					//ThreadManager.Current.CpuThreadState.Yield();
				}
			}

			public void Enqueue(byte* MessageIn, int MessageSize)
			{
				try
				{
					var MessagePartition = PoolPartition.Allocate(MessageSize);
					Messages.Enqueue(MessagePartition);
					PspMemory.WriteBytes(MessagePartition.Low, MessageIn, MessageSize);
#if DEBUG_MSG_PIPES
					Console.Error.WriteLine("MsgPipe.Enqueue (Ok)");
#endif
				}
				catch
				{
#if DEBUG_MSG_PIPES
					Console.Error.WriteLine("MsgPipe.Enqueue (Failed)");
#endif
					throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_MESSAGE_PIPE_FULL));
				}

				// 0 -> 1
				NoticeAvailableForRecv();
			}

			public void Dequeue(byte* MessageOut, int MessageMaxSize, int* MessageSizeOut)
			{
				if (Messages.Count <= 0)
				{
					//Console.Error.WriteLine("MsgPipe.Dequeue (Failed)");
					throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_MESSAGE_PIPE_EMPTY));
				}
				else 
				{
					//Console.Error.WriteLine("MsgPipe.Dequeue (Ok)");
					var MessagePartition = Messages.Dequeue();
					var ReadSize = Math.Min(MessagePartition.Size, MessageMaxSize);
					PspMemory.ReadBytes(MessagePartition.Low, MessageOut, ReadSize);
					MessagePartition.DeallocateFromParent();
					if (MessageSizeOut != null)
					{
						*MessageSizeOut = ReadSize;
					}

					NoticeAvailableForSend();
				}
			}

			public void Init(HleThreadManager ThreadManager, PspMemory PspMemory, HleMemoryManager MemoryManager)
			{
				var BlockType = Attributes.HasFlag(MsgPipeAttributes.UseHighAddress)
					? MemoryPartition.Anchor.High
					: MemoryPartition.Anchor.Low
				;

				this.ThreadManager = ThreadManager;
				this.PspMemory = PspMemory;
				this.PoolPartition = MemoryManager.GetPartition(PartitionId).Allocate(
					Size,
					BlockType,
					Alignment: 16,
					Name: "<MsgPipe> : " + Name
				);
			}

			public void Delete()
			{
				PoolPartition.DeallocateFromParent();	
			}

			void IDisposable.Dispose()
			{
				// TODO
			}
		}

		/// <summary>
		/// Create a message pipe
		/// </summary>
		/// <param name="Name">Name of the pipe</param>
		/// <param name="PartitionId">ID of the memory partition</param>
		/// <param name="Attributes">One of <see cref="MsgPipeAttributes"/></param>
		/// <param name="Size">Size of the message pipe</param>
		/// <param name="Options">Message pipe options (set to NULL)</param>
		/// <returns>The UID of the created pipe, less than 0 on error</returns>
		[HlePspFunction(NID = 0x7C0DC2A0, FirmwareVersion = 150)]
		public MsgPipe sceKernelCreateMsgPipe(string Name, MemoryPartitions PartitionId, MsgPipeAttributes Attributes, int Size, void* Options)
		{
			if (Options != null) throw(new NotImplementedException());

			var MsgPipe = new MsgPipe()
			{
				Name = Name,
				PartitionId = PartitionId,
				Size = Size,
				Attributes = Attributes,
			};

			MsgPipe.Init(ThreadManager, MemoryManager.Memory, MemoryManager);

			return MsgPipe;
		}

		/// <summary>
		/// Delete a message pipe
		/// </summary>
		/// <param name="MsgPipe">The UID of the pipe</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xF0B7DA1C, FirmwareVersion = 150)]
		public int sceKernelDeleteMsgPipe(MsgPipe MsgPipe)
		{
			MsgPipe.Delete();
			MsgPipe.RemoveUid(InjectContext);
			return 0;
		}

		/// <summary>
		/// Send a message to a pipe
		/// </summary>
		/// <param name="MsgPipe">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="WaitMode">Unknown</param>
		/// <param name="ResultSizeAddr">Unknown</param>
		/// <param name="Timeout">Timeout for send</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x876DBFAD, FirmwareVersion = 150)]
		public int sceKernelSendMsgPipe(MsgPipe MsgPipe, byte* Message, int Size, int WaitMode, int* ResultSizeAddr, uint* Timeout)
		{
			if (Timeout != null)
			{
				Logger.Unimplemented("sceKernelSendMsgPipe.Timeout != null");
			}

#if DEBUG_MSG_PIPES
			Console.Error.WriteLine("sceKernelSendMsgPipe");
#endif

			bool Transferred = false;
			while (!Transferred)
			{
				try
				{
					//bool WaitiMsgPipe.OnAvailableForRecv.Count
					MsgPipe.Enqueue(Message, Size);
					ThreadManager.Current.CpuThreadState.Yield();
					Transferred = true;
				}
				catch (SceKernelException)
				{
					//throw(new NotImplementedException());
					ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelSendMsgPipe", MsgPipe, WakeUpCallback =>
					{
#if DEBUG_MSG_PIPES
						Console.Error.WriteLine("sceKernelSendMsgPipe.wait");
#endif
						MsgPipe.OnAvailableForSend.Enqueue(() =>
						{
#if DEBUG_MSG_PIPES
							Console.Error.WriteLine("sceKernelSendMsgPipe.awake");
#endif
							WakeUpCallback();
						});
					}, HandleCallbacks: false);
				}
			}

			return 0;
		}

		/// <summary>
		/// Try to send a message to a pipe
		/// </summary>
		/// <param name="MsgPipe">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="WaitMode">Unknown</param>
		/// <param name="ResultSizeAddr">Unknown</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x884C9F90, FirmwareVersion = 150)]
		public int sceKernelTrySendMsgPipe(MsgPipe MsgPipe, byte* Message, int Size, int WaitMode, int* ResultSizeAddr)
		{
			MsgPipe.Enqueue(Message, Size);
			if (ResultSizeAddr != null)
			{
				*ResultSizeAddr = Size;
			}
			return 0;
		}

		/// <summary>
		/// Receive a message from a pipe
		/// </summary>
		/// <param name="MsgPipe">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="WaitMode">Unknown</param>
		/// <param name="ResultSizeAddr">Unknown</param>
		/// <param name="Timeout">Timeout for receive</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x74829B76, FirmwareVersion = 150)]
		public int sceKernelReceiveMsgPipe(MsgPipe MsgPipe, byte* Message, int Size, int WaitMode, int* ResultSizeAddr, uint* Timeout)
		{
			if (Timeout != null) throw(new NotImplementedException());

#if DEBUG_MSG_PIPES
			Console.Error.WriteLine("sceKernelReceiveMsgPipe");
#endif

			bool Transferred = false;
			while (!Transferred)
			{
				try
				{
					MsgPipe.Dequeue(Message, Size, ResultSizeAddr);
					Transferred = true;
				}
				catch (SceKernelException)
				{
					ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelReceiveMsgPipe", MsgPipe, WakeUpCallback =>
					{
#if DEBUG_MSG_PIPES
						Console.Error.WriteLine("sceKernelReceiveMsgPipe.wait");
#endif

						MsgPipe.OnAvailableForRecv.Enqueue(() =>
						{
#if DEBUG_MSG_PIPES
							Console.Error.WriteLine("sceKernelReceiveMsgPipe.awake");
#endif
							WakeUpCallback();
						});

					}, HandleCallbacks: false);
				}
			}
			return 0;
		}

		/// <summary>
		/// Receive a message from a pipe
		/// </summary>
		/// <param name="MsgPipe">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="WaitMode">Unknown</param>
		/// <param name="ResultSizeAddr">Unknown</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xDF52098F, FirmwareVersion = 150)]
		public int sceKernelTryReceiveMsgPipe(MsgPipe MsgPipe, byte* Message, int Size, int WaitMode, int* ResultSizeAddr)
		{
			MsgPipe.Dequeue(Message, Size, ResultSizeAddr);
			return 0;
		}

		/// <summary>
		/// Get the status of a Message Pipe
		/// </summary>
		/// <param name="MsgPipe">The uid of the Message Pipe</param>
		/// <param name="SceKernelMppInfo">Pointer to a ::SceKernelMppInfo structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x33BE4024, FirmwareVersion = 150)]
		public int sceKernelReferMsgPipeStatus(MsgPipe MsgPipe, /*SceKernelMppInfo*/void* SceKernelMppInfo)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Cancel a message pipe
		/// </summary>
		/// <param name="MsgPipe">UID of the pipe to cancel</param>
		/// <param name="psend">Receive number of sending threads?</param>
		/// <param name="precv">Receive number of receiving threads?</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x349B864D, FirmwareVersion = 150)]
		public int sceKernelCancelMsgPipe(MsgPipe MsgPipe, int* psend, int* precv)
		{
			throw (new NotImplementedException());
		}
	}
}
