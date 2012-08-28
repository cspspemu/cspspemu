using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
	class Program
	{
		static public string Flash0Directory
		{
			get
			{
				return @"..\..\..\deploy\cspspemu\flash0";
			}
		}

		static public string NandPath
		{
			get
			{
				return @"..\..\..\deploy\cspspemu\nand-dump.bin";
			}
		}

		static public string IplPath
		{
			get
			{
				return @"..\..\..\deploy\cspspemu\ipl.bin";
			}
		}

		static void Main(string[] args)
		{
			var IplReader = new IplReader(new NandReader(File.OpenRead(NandPath)));
			var IplData = IplReader.GetIplData().ToArray();

			var PspConfig = new PspConfig();
			var PspEmulatorContext = new PspEmulatorContext(PspConfig);
			var DebugPspMemory = PspEmulatorContext.GetInstance<DebugPspMemory>();
			PspEmulatorContext.SetInstance<PspMemory>(DebugPspMemory);
			var CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			var CpuThreadState = new CpuThreadState(CpuProcessor);
			CpuProcessor.Memory.WriteBytes(0x1FD00000, IplData);
			var CachedGetMethodCache = PspEmulatorContext.GetInstance<CachedGetMethodCache>();
			DebugPspMemory.CpuThreadState = CpuThreadState;

			Console.SetWindowSize(120, 60);

			PspConfig.MustLogWrites = true;

			//for (int n = 0; n < 32; n++) CpuThreadState.GPR[n] = (int)(uint)(0xFFF00000 + n);
			//CpuThreadState.GPR[4] = unchecked((int)0x88500000);
			//CpuThreadState.GPR[5] = unchecked((int)0x89000000);
			CpuThreadState.C0R[12] = 0xFFFFFFFF;
			CpuThreadState.PC = 0x1FD00000 | 0x100;
			try
			{
				while (true)
				{
					var PC = CpuThreadState.PC & PspMemory.MemoryMask;
					Console.WriteLine("PC:{0:X8} - {1:X8}", PC, CpuThreadState.PC);
					var Func = CachedGetMethodCache.GetDelegateAt(PC);
					Func.Delegate(CpuThreadState);
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
				Console.ReadKey();
			}
		}
	}
}
