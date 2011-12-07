using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public enum PipeId : int { }

		/// <summary>
		/// Create a message pipe
		/// </summary>
		/// <param name="PipeName">Name of the pipe</param>
		/// <param name="MemoryPartition">ID of the memory partition</param>
		/// <param name="Attributes">Set to 0?</param>
		/// <param name="Unknown1">Unknown</param>
		/// <param name="Options">Message pipe options (set to NULL)</param>
		/// <returns>The UID of the created pipe, less than 0 on error</returns>
		[HlePspFunction(NID = 0x7C0DC2A0, FirmwareVersion = 150)]
		public PipeId sceKernelCreateMsgPipe(string PipeName, int MemoryPartition, int Attributes, uint Unknown1, void* Options)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Delete a message pipe
		/// </summary>
		/// <param name="PipeId">The UID of the pipe</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xF0B7DA1C, FirmwareVersion = 150)]
		public int sceKernelDeleteMsgPipe(PipeId PipeId)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Send a message to a pipe
		/// </summary>
		/// <param name="PipeId">The UID of the pipe</param>
		/// <param name="Message">Pointer to the message</param>
		/// <param name="Size">Size of the message</param>
		/// <param name="Unknown1">Unknown</param>
		/// <param name="Unknown2">Unknown</param>
		/// <param name="Timeout">Timeout for send</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x876DBFAD, FirmwareVersion = 150)]
		public int sceKernelSendMsgPipe(PipeId PipeId, void* Message, uint Size, int Unknown1, void* Unknown2, uint* Timeout)
		{
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
		public int sceKernelTrySendMsgPipe(PipeId PipeId, void* Message, uint Size, int Unknown1, void* Unknown2)
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
		public int sceKernelReceiveMsgPipe(PipeId PipeId, void* Message, uint Size, int Unknown1, void* Unknown2, uint* Timeout)
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
		public int sceKernelTryReceiveMsgPipe(PipeId PipeId, void* Message, uint Size, int Unknown1, void* Unknown2)
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
		public int sceKernelReferMsgPipeStatus(PipeId PipeId, /*SceKernelMppInfo*/void* SceKernelMppInfo)
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
		public int sceKernelCancelMsgPipe(PipeId uid, int *psend, int *precv)
		{
			throw (new NotImplementedException());
		}
	}
}
