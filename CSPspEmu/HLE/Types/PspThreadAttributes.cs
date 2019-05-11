using System;

namespace CSPspEmu.Hle
{
    [Flags]
    public enum PspThreadAttributes : uint
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// Enable VFPU access for the thread.
        /// </summary>
        Vfpu = 0x00004000,

        /// <summary>
        /// Start the thread in user mode (done automatically if the thread creating it is in user mode).
        /// </summary>
        User = 0x80000000,

        /// <summary>
        /// Thread is part of the USB/WLAN API.
        /// </summary>
        UsbWlan = 0xa0000000,

        /// <summary>
        /// Thread is part of the VSH API.
        /// </summary>
        Vsh = 0xc0000000,

        /// <summary>
        /// Allow using scratchpad memory for a thread, NOT USABLE ON V1.0
        /// </summary>
        ScratchRamEnable = 0x00008000,

        /// <summary>
        /// Disables filling the stack with 0xFF on creation
        /// </summary>
        NoFillStack = 0x00100000,

        /// <summary>
        /// Clear the stack when the thread is deleted
        /// </summary>
        ClearStack = 0x00200000,
    }
}