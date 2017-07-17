using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu
{
    public enum SyncTypeEnum : byte
    {
        WaitForCompletion = 0,
        Peek = 1,
    }

    public enum SignalBehavior : byte
    {
        PSP_GE_SIGNAL_NONE = 0x00,
        PSP_GE_SIGNAL_HANDLER_SUSPEND = 0x01,
        PSP_GE_SIGNAL_HANDLER_CONTINUE = 0x02,
        PSP_GE_SIGNAL_HANDLER_PAUSE = 0x03,
        PSP_GE_SIGNAL_SYNC = 0x08,
        PSP_GE_SIGNAL_JUMP = 0x10,
        PSP_GE_SIGNAL_CALL = 0x11,
        PSP_GE_SIGNAL_RET = 0x12,
        PSP_GE_SIGNAL_RJUMP = 0x13,
        PSP_GE_SIGNAL_RCALL = 0x14,
        PSP_GE_SIGNAL_OJUMP = 0x15,
        PSP_GE_SIGNAL_OCALL = 0x16,

        PSP_GE_SIGNAL_RTBP0 = 0x20,
        PSP_GE_SIGNAL_RTBP1 = 0x21,
        PSP_GE_SIGNAL_RTBP2 = 0x22,
        PSP_GE_SIGNAL_RTBP3 = 0x23,
        PSP_GE_SIGNAL_RTBP4 = 0x24,
        PSP_GE_SIGNAL_RTBP5 = 0x25,
        PSP_GE_SIGNAL_RTBP6 = 0x26,
        PSP_GE_SIGNAL_RTBP7 = 0x27,
        PSP_GE_SIGNAL_OTBP0 = 0x28,
        PSP_GE_SIGNAL_OTBP1 = 0x29,
        PSP_GE_SIGNAL_OTBP2 = 0x2A,
        PSP_GE_SIGNAL_OTBP3 = 0x2B,
        PSP_GE_SIGNAL_OTBP4 = 0x2C,
        PSP_GE_SIGNAL_OTBP5 = 0x2D,
        PSP_GE_SIGNAL_OTBP6 = 0x2E,
        PSP_GE_SIGNAL_OTBP7 = 0x2F,
        PSP_GE_SIGNAL_RCBP = 0x30,
        PSP_GE_SIGNAL_OCBP = 0x38,
        PSP_GE_SIGNAL_BREAK1 = 0xF0,
        PSP_GE_SIGNAL_BREAK2 = 0xFF,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DisplayListStatusEnum
    {
        /// <summary>
        /// The list has been completed (PSP_GE_LIST_COMPLETED)
        /// </summary>
        Completed = 0,

        /// <summary>
        ///  list is queued but not executed yet (PSP_GE_LIST_QUEUED)
        /// </summary>
        Queued = 1,

        /// <summary>
        /// The list is currently being executed (PSP_GE_LIST_DRAWING)
        /// </summary>
        Drawing = 2,

        /// <summary>
        /// The list was stopped because it encountered stall address (PSP_GE_LIST_STALLING)
        /// </summary>
        Stalling = 3,

        /// <summary>
        /// The list is paused because of a signal or sceGeBreak (PSP_GE_LIST_PAUSED)
        /// </summary>
        Paused = 4,
    }

    public unsafe struct PspGeStack
    {
        public fixed uint Stack[8];
    }

    /// <summary>
    /// 
    /// </summary>
    public unsafe struct PspGeListArgs
    {
        /// <summary>
        /// Size of the structure
        /// </summary>
        public uint Size;

        /// <summary>
        /// Pointer to a GpuStateStruct
        /// </summary>
        public uint GpuStateStructAddress;

        /// <summary>
        /// 
        /// </summary>
        public uint NumberOfStacks;

        /// <summary>
        /// 
        /// </summary>
        public uint StacksAddress;
    }
}