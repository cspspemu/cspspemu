using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.emulator
{
    [HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
    public unsafe class Emulator : HleModuleHost
    {
        [Inject] HleThreadManager ThreadManager;

        [HlePspFunction(NID = 0x00000000, FirmwareVersion = 150)]
        public void emitInt(int value) => Console.WriteLine("emitInt: {0}", value);

        [HlePspFunction(NID = 0x00000001, FirmwareVersion = 150)]
        public void emitFloat(float value) => Console.WriteLine("emitFloat: {0:0.00000}", value);

        [HlePspFunction(NID = 0x00000002, FirmwareVersion = 150)]
        public void emitString(string value) => Console.WriteLine("emitString: '{0}'", value);

        [HlePspFunction(NID = 0x00000003, FirmwareVersion = 150)]
        public void emitMemoryBlock(byte* value, uint size) => throw new NotImplementedException();

        [HlePspFunction(NID = 0x00000004, FirmwareVersion = 150)]
        public void emitHex(byte* value, uint size) => throw new NotImplementedException();

        [HlePspFunction(NID = 0x00000005, FirmwareVersion = 150)]
        public void emitUInt(uint value) => Console.WriteLine("emitUInt: {0}", "0x%08X".Sprintf(value));

        [HlePspFunction(NID = 0x00000006, FirmwareVersion = 150)]
        public void emitLong(long value) => Console.WriteLine("emitLong: {0}", "0x%016X".Sprintf(value));

        [HlePspFunction(NID = 0x10000010, FirmwareVersion = 150)]
        public long testArguments(int arg1, long arg2, float arg3) => (long) arg1 + (long) arg2 + (long) arg3;

        [HlePspFunction(NID = 0x10000000, FirmwareVersion = 150)]
        public void waitThreadForever(CpuThreadState cpuThreadState)
        {
            var sleepThread = ThreadManager.Current;
            sleepThread.SetStatus(HleThread.Status.Waiting);
            sleepThread.CurrentWaitType = HleThread.WaitType.None;
            ThreadManager.Yield();
        }

        [HlePspFunction(NID = 0x10000001, FirmwareVersion = 150)]
        public void finalizeCallback(CpuThreadState cpuThreadState)
        {
            //CpuThreadState.CpuProcessor.RunningCallback = false;
            cpuThreadState.Yield();
            //throw (new HleEmulatorFinalizeCallbackException());
        }
    }
}