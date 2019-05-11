using System;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.sysmem
{
    [HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
    public unsafe class sceSysEventForKernel : HleModuleHost
    {
        //int (*handler)(int ev_id, char* ev_name, void* param, int* result);
        public enum PspSysEventHandlerFunc : uint
        {
        }

        public struct PspSysEventHandler
        {
            public int Size;
            public char* Name;
            public int TypeMask;

            public PspSysEventHandlerFunc Handler;
            public int R28;
            public int Busy;
            public PspSysEventHandler* Next;
            public fixed int Reserved[9];
        }

        /// <summary>
        /// Register a SysEvent handler.
        /// </summary>
        /// <param name="PspSysEventHandler">The handler to register</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0xCD9E4BB5, FirmwareVersion = 150)]
        public int sceKernelRegisterSysEventHandler(PspSysEventHandler* PspSysEventHandler)
        {
            /*
            logInfo("sceKernelRegisterSysEventHandler");
            pspSysEventHandler = *PspSysEventHandler;
            return 0;
            */
            throw(new NotImplementedException());
        }

        /// <summary>
        /// Dispatch a SysEvent event.
        /// </summary>
        /// <param name="EventTypeMask">The event type mask</param>
        /// <param name="EventId">The event ID</param>
        /// <param name="EventNamePointer">The event name</param>
        /// <param name="Parameter">The pointer to the custom parameters</param>
        /// <param name="Result">The pointer to the result</param>
        /// <param name="BreakNonZero">Set to 1 to interrupt the calling chain after the first non-zero return</param>
        /// <param name="BreakHandler">The pointer to the event handler having interrupted</param>
        /// <returns>0 on success, less than 0 on error</returns>
        [HlePspFunction(NID = 0x36331294, FirmwareVersion = 150)]
        public int sceKernelSysEventDispatch(int EventTypeMask, int EventId, uint EventNamePointer, uint Parameter,
            uint Result, int BreakNonZero, PspSysEventHandler* BreakHandler)
        {
            /*
            logWarning("Not fully implemented sceKernelSysEventDispatch");
            hleEmulatorState.callbacksHandler.addToExecuteQueue(
                pspSysEventHandler.handler,
                [EventId, EventNamePointer, Parameter, Result]
            );
            return 0;
            */
            throw (new NotImplementedException());
        }
    }
}