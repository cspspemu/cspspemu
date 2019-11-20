using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
    public class LlePspCpu : LlePspComponent
    {
        string _name;

        //CachedGetMethodCache CachedGetMethodCache;
        CpuThreadState CpuThreadState;

        uint EntryPoint;

        public LlePspCpu(string name, InjectContext injectContext, CpuProcessor cpuProcessor,
            uint entryPoint = 0x1fc00000)
        {
            _name = name;
            //this.CachedGetMethodCache = PspEmulatorContext.GetInstance<CachedGetMethodCache>();
            CpuThreadState = new CpuThreadState(cpuProcessor);
            EntryPoint = entryPoint;
        }

        public override void Main()
        {
            while (true)
            {
                StartEvent.WaitOne();

                CpuThreadState.Pc = EntryPoint;
                try
                {
                    while (Running)
                    {
                        var pc = CpuThreadState.Pc & PspMemory.MemoryMask;
                        //Console.WriteLine("PC:{0:X8} - {1:X8}", PC, CpuThreadState.PC);

                        //var Func = CachedGetMethodCache.GetDelegateAt(PC);
                        throw new NotImplementedException();

                        //if (Name == "ME")
                        //{
                        //	Console.WriteLine("{0}: {1:X8}", Name, PC);
                        //}
                        //
                        //if (PC == 0x040EC228)
                        //{
                        //	if (((int)CpuThreadState.V0) < 0)
                        //	{
                        //		Console.WriteLine("!!ERROR: 0x{0:X8}", CpuThreadState.V0);
                        //		//(SceKernelErrors)
                        //	}
                        //	//CpuThreadState.DumpRegisters();
                        //}
                        //
                        //Func.Delegate(CpuThreadState);
                        //throw(new PspMemory.InvalidAddressException(""));
                    }
                }
                catch (Exception e)
                {
                    CpuThreadState.DumpRegisters();
                    Console.WriteLine("----------------------------------------------------");
                    Console.Error.WriteLine(e.Message);
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("at {0:X8}", CpuThreadState.Pc);
                    Console.WriteLine("----------------------------------------------------");
                    Console.Error.WriteLine(e);
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("at {0:X8}", CpuThreadState.Pc);
                    Console.ReadKey();
                }
            }
        }
    }
}