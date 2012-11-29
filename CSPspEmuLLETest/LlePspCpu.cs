using System;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
	public class LlePspCpu : LlePspComponent
	{
		string Name;
		//CachedGetMethodCache CachedGetMethodCache;
		CpuThreadState CpuThreadState;
		uint EntryPoint;

		public LlePspCpu(string Name, PspEmulatorContext PspEmulatorContext, CpuProcessor CpuProcessor, uint EntryPoint = 0x1fc00000)
		{
			this.Name = Name;
			//this.CachedGetMethodCache = PspEmulatorContext.GetInstance<CachedGetMethodCache>();
			this.CpuThreadState = new CpuThreadState(CpuProcessor);
			this.EntryPoint = EntryPoint;
		}

		public override void Main()
		{
			while (true)
			{
				StartEvent.WaitOne();

				CpuThreadState.PC = EntryPoint;
				try
				{
					while (Running)
					{
						var PC = CpuThreadState.PC & PspMemory.MemoryMask;
						//Console.WriteLine("PC:{0:X8} - {1:X8}", PC, CpuThreadState.PC);
						
						//var Func = CachedGetMethodCache.GetDelegateAt(PC);
						throw(new NotImplementedException());
		
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
				catch (Exception Exception)
				{
					CpuThreadState.DumpRegisters();
					Console.WriteLine("----------------------------------------------------");
					Console.Error.WriteLine(Exception.Message);
					Console.WriteLine("----------------------------------------------------");
					Console.WriteLine("at {0:X8}", CpuThreadState.PC);
					Console.WriteLine("----------------------------------------------------");
					Console.Error.WriteLine(Exception);
					Console.WriteLine("----------------------------------------------------");
					Console.WriteLine("at {0:X8}", CpuThreadState.PC);
					Console.ReadKey();
				}
			}
		}
	}
}
