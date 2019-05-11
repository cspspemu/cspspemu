using System;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Vfs.MemoryStick
{
    public unsafe class HleIoDriverMemoryStick : ProxyHleIoDriver
    {
        PspMemory Memory;
        IMemoryStickEventHandler MemoryStickEventHandler;

        public HleIoDriverMemoryStick(PspMemory Memory, IMemoryStickEventHandler MemoryStickEventHandler,
            IHleIoDriver HleIoDriver)
            : base(HleIoDriver)
        {
            this.Memory = Memory;
            this.MemoryStickEventHandler = MemoryStickEventHandler;
        }

        public enum CommandType : uint
        {
            CheckInserted = 0x02425823,
            MScmRegisterMSInsertEjectCallback = 0x02415821,
            MScmUnregisterMSInsertEjectCallback = 0x02415822,
            GetMemoryStickCapacity = 0x02425818,
            CheckMemoryStickIsInserted = 0x02025806,
            CheckMemoryStickStatus = 0x02025801,
        }

        public struct SizeInfoStruct
        {
            public uint MaxClusters;
            public uint FreeClusters;
            public uint MaxSectors;
            public uint SectorSize;
            public uint SectorCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="HleIoDrvFileArg"></param>
        /// <param name="DeviceName"></param>
        /// <param name="Command"></param>
        /// <param name="InputPointer"></param>
        /// <param name="InputLength"></param>
        /// <param name="OutputPointer"></param>
        /// <param name="OutputLength"></param>
        /// <returns></returns>
        public override int IoDevctl(HleIoDrvFileArg HleIoDrvFileArg, string DeviceName, uint Command,
            byte* InputPointer, int InputLength, byte* OutputPointer, int OutputLength)
        {
            //Console.Error.WriteLine("MemoryStick.IoDevctl: ({0}, 0x{1:X})", DeviceName, Command);

            switch ((CommandType) Command)
            {
                case CommandType.CheckInserted:
                {
                    if (OutputPointer == null || OutputLength < 4)
                        return (int) SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
                    // 0 - Device is not assigned (callback not registered).
                    // 1 - Device is assigned (callback registered).
                    *(uint*) OutputPointer = 1;
                    return 0;
                }
                case CommandType.MScmRegisterMSInsertEjectCallback:
                {
                    if (InputPointer == null || InputLength < 4)
                        return (int) SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
                    int CallbackId = *(int*) InputPointer;
                    MemoryStickEventHandler.ScheduleCallback(CallbackId, 1, 1);
                    //var Callback = CallbackManager.Callbacks.Get(CallbackId);
                    //CallbackManager.ScheduleCallback(
                    //	HleCallback.Create(
                    //		"RegisterInjectEjectCallback",
                    //		Callback.Function,
                    //		new object[] {
                    //			1, // a0
                    //			1, // a1
                    //			Callback.Arguments[0] // a2
                    //		}
                    //	)
                    //);

                    return 0;
                }
                case CommandType.GetMemoryStickCapacity:
                {
                    if (InputPointer == null || InputLength < 4)
                        return (int) SceKernelErrors.ERROR_ERRNO_INVALID_ARGUMENT;
                    var SizeInfo = (SizeInfoStruct*) Memory.PspAddressToPointerSafe(*(uint*) InputPointer);
                    var MemoryStickSectorSize = (32 * 1024);
                    //var TotalSpaceInBytes = 2L * 1024 * 1024 * 1024;
                    var FreeSpaceInBytes = 1L * 1024 * 1024 * 1024;
                    SizeInfo->SectorSize = 0x200;
                    SizeInfo->SectorCount = (uint) (MemoryStickSectorSize / SizeInfo->SectorSize);
                    SizeInfo->MaxClusters = (uint) (FreeSpaceInBytes * 95 / 100) /
                                            (SizeInfo->SectorSize * SizeInfo->SectorCount);
                    SizeInfo->FreeClusters = SizeInfo->MaxClusters;
                    SizeInfo->MaxSectors = SizeInfo->MaxClusters;

                    return 0;
                }
                case CommandType.MScmUnregisterMSInsertEjectCallback:
                    // Ignore.
                    return 0;
                case CommandType.CheckMemoryStickIsInserted:
                    *((uint*) OutputPointer) = 1;
                    return 0;
                case CommandType.CheckMemoryStickStatus:
                    // 0 <- Busy
                    // 1 <- Ready
                    *((uint*) OutputPointer) = 4;
                    break;
                default:
                    Console.Error.WriteLine("MemoryStick.IoDevctl Not Implemented! ({0}, 0x{1:X})", DeviceName,
                        Command);
                    break;
            }

            return 0;
        }
    }
}