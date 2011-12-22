using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		/// <summary>
		/// Creates a new messagebox
		/// </summary>
		/// <example>
		/// int mbxid;
		/// mbxid = sceKernelCreateMbx("MyMessagebox", 0, NULL);
		/// </example>
		/// <param name="name">Specifies the name of the mbx</param>
		/// <param name="attr">Mbx attribute flags (normally set to 0)</param>
		/// <param name="option">Mbx options (normally set to NULL)</param>
		/// <returns>A messagebox id</returns>
		[HlePspFunction(NID = 0x8125221D, FirmwareVersion = 150)]
		public int sceKernelCreateMbx(string name, uint attr, SceKernelMbxOptParam* option)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Destroy a messagebox
		/// </summary>
		/// <param name="mbxid">The mbxid returned from a previous create call.</param>
		/// <returns>Returns the value 0 if its succesful otherwise an error code</returns>
		[HlePspFunction(NID = 0x86255ADA, FirmwareVersion = 150)]
		public int sceKernelDeleteMbx(int mbxid)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Send a message to a messagebox
		/// </summary>
		/// <example>
		/// struct MyMessage {
		/// 	SceKernelMsgPacket header;
		/// 	char text[8];
		/// };
		/// 
		/// struct MyMessage msg = { {0}, "Hello" };
		/// // Send the message
		/// sceKernelSendMbx(mbxid, (void*) &msg);
		/// </example>
		/// <param name="mbxid">The mbx id returned from sceKernelCreateMbx</param>
		/// <param name="message">
		/// A message to be forwarded to the receiver.
		/// The start of the message should be the 
		/// ::SceKernelMsgPacket structure, the rest
		/// </param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xE9B3061E, FirmwareVersion = 150)]
		public int sceKernelSendMbx(int mbxid, void* message)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Wait for a message to arrive in a messagebox
		/// </summary>
		/// <example>
		/// void *msg;
		/// sceKernelReceiveMbx(mbxid, &msg, NULL);
		/// </example>
		/// <param name="mbxid">The mbx id returned from sceKernelCreateMbx</param>
		/// <param name="pmessage">
		///		A pointer to where a pointer to the
		///		received message should be stored
		/// </param>
		/// <param name="timeout">Timeout in microseconds</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x18260574, FirmwareVersion = 150)]
		public int sceKernelReceiveMbx(int mbxid, void** pmessage, uint* timeout)
		{
			throw(new NotImplementedException());
		}

		public struct SceKernelMbxOptParam
		{
		}
	}
}
