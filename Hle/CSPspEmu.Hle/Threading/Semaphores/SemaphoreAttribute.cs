using System;

namespace CSPspEmu.Hle.Threading.Semaphores
{
    [Flags]
    public enum SemaphoreAttribute : uint
    {
        /// <summary>
        /// Signal waiting threads with a FIFO iterator.
        /// </summary>
        FirstInFirstOut = 0x000,

        /// <summary>
        /// Signal waiting threads with a priority based iterator.
        /// </summary>
        Priority = 0x100,
    }
}