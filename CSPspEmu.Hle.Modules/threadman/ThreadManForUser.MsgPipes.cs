using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public enum MsgPipeAttributes : uint
		{
			PSP_MPP_ATTR_SEND_FIFO = 0,
			PSP_MPP_ATTR_SEND_PRIORITY = 0x100,

			PSP_MPP_ATTR_RECEIVE_FIFO = 0,
			PSP_MPP_ATTR_RECEIVE_PRIORITY = 0x1000,

			PSP_MPP_ATTR_SEND = PSP_MPP_ATTR_SEND_FIFO | PSP_MPP_ATTR_SEND_PRIORITY,
			PSP_MPP_ATTR_RECEIVE = PSP_MPP_ATTR_RECEIVE_FIFO | PSP_MPP_ATTR_RECEIVE_PRIORITY,

			PSP_MPP_ATTR_ADDR_HIGH = 0x4000,
		}

		public enum MsgPipeId : int { }

		public class MsgPipe
		{
			public string Name;
			public MsgPipeAttributes Attributes;
			public HleMemoryManager.Partitions PartitionId;
			public uint Size;

			public void Init()
			{
			}
		}

		HleUidPoolSpecial<MsgPipe, MsgPipeId> MessagePipeList = new HleUidPoolSpecial<MsgPipe, MsgPipeId>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_MESSAGE_PIPE,
		};

		/// <summary>
		/// Create a message pipe
		/// </summary>
		/// <param name="Name">Name of the pipe</param>
		/// <param name="PartitionId">ID of the memory partition</param>
		/// <param name="Attributes">One of ::MsgPipeAttributes</param>
		/// <param name="Size">Size of the message pipe</param>
		/// <param name="Options">Message pipe options (set to NULL)</param>
		/// <returns>The UID of the created pipe, less than 0 on error</returns>
		[HlePspFunction(NID = 0x7C0DC2A0, FirmwareVersion = 150)]
		public MsgPipeId sceKernelCreateMsgPipe(string Name, HleMemoryManager.Partitions PartitionId, MsgPipeAttributes Attributes, uint Size, void* Options)
		{
			if (Options != null) throw(new NotImplementedException());

			var MsgPipe = new MsgPipe()
			{
				Name = Name,
				PartitionId = PartitionId,
				Size = Size,
				Attributes = Attributes,
			};
			MsgPipe.Init();

			return MessagePipeList.Create(MsgPipe);
		}

		/// <summary>
		/// Delete a message pipe
		/// </summary>
		/// <param name="PipeId">The UID of the pipe</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xF0B7DA1C, FirmwareVersion = 150)]
		public int sceKernelDeleteMsgPipe(MsgPipeId PipeId)
		{
			var MsgPipe = MessagePipeList.Get(PipeId);
			MessagePipeList.Remove(PipeId);
			return 0;
		}

		/// <summary>
		/// Send a message to a pipe
		/// </summary>
		/// <param name="PipeId">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="WaitMode">Unknown</param>
		/// <param name="ResultSizeAddr">Unknown</param>
		/// <param name="Timeout">Timeout for send</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x876DBFAD, FirmwareVersion = 150)]
		public int sceKernelSendMsgPipe(MsgPipeId PipeId, void* Message, uint Size, int WaitMode, uint* ResultSizeAddr, uint* Timeout)
		{
			//sceKernelSendMsgPipe(int uid, int msg_addr, int size, int waitMode, int resultSize_addr, int timeout_addr) {
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Try to send a message to a pipe
		/// </summary>
		/// <param name="PipeId">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="Unknown1">Unknown</param>
		/// <param name="Unknown2">Unknown</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x884C9F90, FirmwareVersion = 150)]
		public int sceKernelTrySendMsgPipe(MsgPipeId PipeId, void* Message, uint Size, int Unknown1, void* Unknown2)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Receive a message from a pipe
		/// </summary>
		/// <param name="PipeId">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="Unknown1">Unknown</param>
		/// <param name="Unknown2">Unknown</param>
		/// <param name="Timeout">Timeout for receive</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x74829B76, FirmwareVersion = 150)]
		public int sceKernelReceiveMsgPipe(MsgPipeId PipeId, void* Message, uint Size, int Unknown1, void* Unknown2, uint* Timeout)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Receive a message from a pipe
		/// </summary>
		/// <param name="PipeId">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="Unknown1">Unknown</param>
		/// <param name="Unknown2">Unknown</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xDF52098F, FirmwareVersion = 150)]
		public int sceKernelTryReceiveMsgPipe(MsgPipeId PipeId, void* Message, uint Size, int Unknown1, void* Unknown2)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get the status of a Message Pipe
		/// </summary>
		/// <param name="PipeId">The uid of the Message Pipe</param>
		/// <param name="SceKernelMppInfo">Pointer to a ::SceKernelMppInfo structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x33BE4024, FirmwareVersion = 150)]
		public int sceKernelReferMsgPipeStatus(MsgPipeId PipeId, /*SceKernelMppInfo*/void* SceKernelMppInfo)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Cancel a message pipe
		/// </summary>
		/// <param name="uid">UID of the pipe to cancel</param>
		/// <param name="psend">Receive number of sending threads?</param>
		/// <param name="precv">Receive number of receiving threads?</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x349B864D, FirmwareVersion = 150)]
		public int sceKernelCancelMsgPipe(MsgPipeId uid, int *psend, int *precv)
		{
			throw (new NotImplementedException());
		}
	}
}
