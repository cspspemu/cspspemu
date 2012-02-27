using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public enum MbxAttributesEnum : uint
		{
			/// <summary>
			/// PSP_MBX_ATTR_FIFO
			/// </summary>
			RecvFifo = 0,

			/// <summary>
			/// PSP_MBX_ATTR_PRIORITY
			/// </summary>
			RecvPriority = 0x100,

			/// <summary>
			/// Add new messages by FIFO.
			/// PSP_MBX_ATTR_MSG_FIFO
			/// </summary>
			SendFifo = 0,

			/// <summary>
			/// Add new messages by MsgPacket priority.
			/// PSP_MBX_ATTR_MSG_PRIORITY
			/// </summary>
			SendPriority = 0x400,
		}

		public class MessageBox
		{
			public string Name;
			public MbxAttributesEnum Attributes;
			public SceKernelMbxOptParam Options;

			LinkedList<PspPointer> Messages = new LinkedList<PspPointer>();
			Queue<Action> ExtractQueue = new Queue<Action>();

			protected void CheckQueue()
			{
				while (Messages.Any() && ExtractQueue.Any())
				{
					var Extract = ExtractQueue.Dequeue();
					Extract();
				}
			}

			public int Send(PspPointer Message)
			{
				lock (this)
				{
					Messages.AddLast(Message);
					CheckQueue();
					return 0;
				}
			}

			public void Receive(PspPointer* PointerToMessage, WakeUpCallbackDelegate WakeUpCallback)
			{
				lock (this)
				{
					Action Extract = () =>
					{
						*PointerToMessage = Messages.Last.Value;
						Messages.RemoveLast();
						if (WakeUpCallback != null) WakeUpCallback();
					};
					ExtractQueue.Enqueue(Extract);
					CheckQueue();
				}
			}

			public bool Poll(PspPointer* PointerToMessage)
			{
				lock (this)
				{
					if (Messages.Any())
					{
						Receive(PointerToMessage, null);
						return true;
					}
					else
					{
						return false;
					}
				}
			}
		}

		public HleUidPoolSpecial<MessageBox, int> MessageBoxList = new HleUidPoolSpecial<MessageBox, int>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_MESSAGE_BOX,
		};

		/// <summary>
		/// Creates a new messagebox
		/// </summary>
		/// <example>
		/// int mbxid;
		/// mbxid = sceKernelCreateMbx("MyMessagebox", 0, NULL);
		/// </example>
		/// <param name="Name">Specifies the name of the mbx</param>
		/// <param name="Attributes">Mbx attribute flags (normally set to 0)</param>
		/// <param name="Options">Mbx options (normally set to NULL)</param>
		/// <returns>A messagebox id</returns>
		[HlePspFunction(NID = 0x8125221D, FirmwareVersion = 150)]
		public int sceKernelCreateMbx(string Name, MbxAttributesEnum Attributes, SceKernelMbxOptParam* Options)
		{
			if (Options != null) throw(new NotImplementedException());
			if (Attributes.HasFlag(MbxAttributesEnum.RecvPriority)) throw(new NotImplementedException());
			if (Attributes.HasFlag(MbxAttributesEnum.SendPriority)) throw (new NotImplementedException());

			var Mbx = new MessageBox()
			{
				Name = Name,
				Attributes = Attributes,
			};
			if (Options != null) Mbx.Options = *Options;
			return MessageBoxList.Create(Mbx);
		}

		/// <summary>
		/// Destroy a messagebox
		/// </summary>
		/// <param name="MessageBoxId">The mbxid returned from a previous create call.</param>
		/// <returns>Returns the value 0 if its succesful otherwise an error code</returns>
		[HlePspFunction(NID = 0x86255ADA, FirmwareVersion = 150)]
		public int sceKernelDeleteMbx(int MessageBoxId)
		{
			MessageBoxList.Remove(MessageBoxId);
			return 0;
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
		public int sceKernelSendMbx(int MessageBoxId, PspPointer Message)
		{
			var MessageBox = MessageBoxList.Get(MessageBoxId);
			return MessageBox.Send(Message);
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
		public int sceKernelReceiveMbx(int MessageBoxId, PspPointer* PointerToMessage, uint* Timeout)
		{
			var CurrentThread = HleState.ThreadManager.Current;
			var MessageBox = MessageBoxList.Get(MessageBoxId);
			bool TimedOut = false;
			CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelReceiveMbx", MessageBox, WakeUpCallback =>
			{
				if (Timeout != null)
				{
					HleState.PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*Timeout), () =>
					{
						TimedOut = true;
						WakeUpCallback();
					});
				}
				MessageBox.Receive(PointerToMessage, WakeUpCallback);
			}, HandleCallbacks: false);

			if (TimedOut)
			{
				return (int)SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
			}
			else
			{
				//if (Timeout)
				//return MessageBox.Receive(Message);
				return 0;
			}
		}

		/// <summary>
		/// Check if a message has arrived in a messagebox
		/// </summary>
		/// <example>
		/// void *msg;
		/// sceKernelPollMbx(mbxid, &msg);
		/// </example>
		/// <param name="MessageBoxId">The mbx id returned from sceKernelCreateMbx</param>
		/// <param name="PointerToMessage">A pointer to where a pointer to the received message should be stored</param>
		/// <returns>Less than 0 on error (SCE_KERNEL_ERROR_MBOX_NOMSG if the mbx is empty)</returns>
		[HlePspFunction(NID = 0x0D81716A, FirmwareVersion = 150)]
		public int sceKernelPollMbx(int MessageBoxId, PspPointer* PointerToMessage)
		{
			var MessageBox = MessageBoxList.Get(MessageBoxId);
			if (!MessageBox.Poll(PointerToMessage))
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_MESSAGEBOX_NO_MESSAGE));
			}

			return 0;
		}

		public struct SceKernelMbxOptParam
		{
		}
	}
}
