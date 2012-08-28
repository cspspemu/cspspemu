using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Crypto;
using CSPspEmu.Core.Memory;

namespace CSPspEmuLLETest
{
	unsafe class Program
	{
		static public string NandPath
		{
			get
			{
				return @"..\..\..\deploy\cspspemu\nand-dump.bin";
			}
		}

		static void Main(string[] args)
		{
			var PspConfig = new PspConfig();
			var PspEmulatorContext = new PspEmulatorContext(PspConfig);
			var DebugPspMemory = PspEmulatorContext.GetInstance<DebugPspMemory>();
			PspEmulatorContext.SetInstance<PspMemory>(DebugPspMemory);
			var CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			var CpuThreadState = new CpuThreadState(CpuProcessor);
			var CachedGetMethodCache = PspEmulatorContext.GetInstance<CachedGetMethodCache>();
			var Dma = new Dma(CpuThreadState);
			DebugPspMemory.CpuThreadState = CpuThreadState;
			DebugPspMemory.Dma = Dma;

			Console.SetWindowSize(120, 60);

			PspConfig.MustLogWrites = true;

			var IplReader = new IplReader(new NandReader(File.OpenRead(NandPath)));
			var Info = IplReader.LoadIplToMemory(new PspMemoryStream(DebugPspMemory));

			/*
			ME:
			li      $t0, 0x40EC19C
			jr      $t0
			nop
			*/

			//IplReader.WriteIplToFile(File.Open(NandPath + ".ipl.bin", FileMode.Create, FileAccess.Write));

			CpuThreadState.PC = Info.EntryFunction;
			try
			{
				while (true)
				{
					var PC = CpuThreadState.PC & PspMemory.MemoryMask;
					//Console.WriteLine("PC:{0:X8} - {1:X8}", PC, CpuThreadState.PC);
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
				Console.Error.WriteLine(Exception);
				Console.WriteLine("----------------------------------------------------");
				Console.WriteLine("at {0:X8}", CpuThreadState.PC);
				Console.ReadKey();
			}
		}
	}
}
