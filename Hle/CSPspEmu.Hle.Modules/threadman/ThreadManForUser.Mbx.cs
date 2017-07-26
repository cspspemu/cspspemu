using System;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.threadman
{
    public unsafe partial class ThreadManForUser
    {
        [Flags]
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

        [HleUidPoolClass(NotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_MESSAGE_BOX)]
        public class MessageBox : IHleUidPoolClass, IDisposable
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

            public void Receive(PspPointer* PointerToMessage, Action WakeUp)
            {
                lock (this)
                {
                    Action Extract = () =>
                    {
                        *PointerToMessage = Messages.Last.Value;
                        Messages.RemoveLast();
                        WakeUp?.Invoke();
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

            void IDisposable.Dispose()
            {
                // TODO
            }
        }

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
        /// <returns>A messagebox ID</returns>
        [HlePspFunction(NID = 0x8125221D, FirmwareVersion = 150)]
        public MessageBox sceKernelCreateMbx(string Name, MbxAttributesEnum Attributes, SceKernelMbxOptParam* Options)
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
            return Mbx;
        }

        /// <summary>
        /// Destroy a messagebox
        /// </summary>
        /// <param name="MessageBox">The mbxid returned from a previous create call.</param>
        /// <returns>Returns the value 0 if its succesful otherwise an error code</returns>
        [HlePspFunction(NID = 0x86255ADA, FirmwareVersion = 150)]
        public int sceKernelDeleteMbx(MessageBox MessageBox)
        {
            MessageBox.RemoveUid(InjectContext);
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
        /// <param name="MessageBoxId">The mbx id returned from sceKernelCreateMbx</param>
        /// <param name="Message">
        /// A message to be forwarded to the receiver.
        /// The start of the message should be the 
        /// ::SceKernelMsgPacket structure, the rest
        /// </param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0xE9B3061E, FirmwareVersion = 150)]
        public int sceKernelSendMbx(MessageBox MessageBox, PspPointer Message)
        {
            return MessageBox.Send(Message);
        }

        /// <summary>
        /// Wait for a message to arrive in a messagebox
        /// </summary>
        /// <example>
        /// void *msg;
        /// sceKernelReceiveMbx(mbxid, &amp;msg, NULL);
        /// </example>
        /// <param name="MessageBox">The mbx ID returned from <see cref="sceKernelCreateMbx"/></param>
        /// <param name="PointerToMessage">
        ///		A pointer to where a pointer to the
        ///		received message should be stored
        /// </param>
        /// <param name="Timeout">Timeout in microseconds</param>
        /// <returns>Less than 0 on error</returns>
        [HlePspFunction(NID = 0x18260574, FirmwareVersion = 150)]
        public int sceKernelReceiveMbx(MessageBox MessageBox, PspPointer* PointerToMessage, uint* Timeout)
        {
            var CurrentThread = ThreadManager.Current;
            bool TimedOut = false;
            CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.None, "sceKernelReceiveMbx", MessageBox,
                WakeUpCallback =>
                {
                    if (Timeout != null)
                    {
                        PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*Timeout), () =>
                        {
                            TimedOut = true;
                            WakeUpCallback();
                        });
                    }
                    MessageBox.Receive(PointerToMessage, WakeUpCallback);
                }, HandleCallbacks: false);

            if (TimedOut)
            {
                return (int) SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT;
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
        /// <param name="MessageBoxId">The mbx ID returned from <see cref="sceKernelCreateMbx"/></param>
        /// <param name="PointerToMessage">A pointer to where a pointer to the received message should be stored</param>
        /// <returns>Less than 0 on error (SCE_KERNEL_ERROR_MBOX_NOMSG if the mbx is empty)</returns>
        [HlePspFunction(NID = 0x0D81716A, FirmwareVersion = 150)]
        public int sceKernelPollMbx(MessageBox MessageBox, PspPointer* PointerToMessage)
        {
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