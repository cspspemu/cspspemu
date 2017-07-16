using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Hle
{
    [Flags]
    public enum PspThreadStatus : uint
    {
        /// <summary>
        /// 0x01 - Running.
        /// </summary>
        PSP_THREAD_RUNNING = 1,

        /// <summary>
        /// 0x02 - Ready.
        /// </summary>
        PSP_THREAD_READY = 2,

        /// <summary>
        /// 0x04 - Waiting.
        /// </summary>
        PSP_THREAD_WAITING = 4,

        /// <summary>
        /// 0x08 - Suspended.
        /// </summary>
        PSP_THREAD_SUSPEND = 8,

        /// <summary>
        /// 0x10 - Stopped. (Before startThread)
        /// </summary>
        PSP_THREAD_STOPPED = 16,

        /// <summary>
        /// 0x20 - Thread manager has killed the thread (stack overflow)
        /// </summary>
        PSP_THREAD_KILLED = 32,
    }
}