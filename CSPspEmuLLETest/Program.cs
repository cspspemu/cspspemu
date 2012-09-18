using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
				//return @"..\..\..\deploy\cspspemu\nand-dump-420.bin";
			}
		}

		static public string PreIplPath
		{
			get
			{
				return @"..\..\..\deploy\cspspemu\psp_bios.bin";
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
			var Dma = new Dma(CpuThreadState);
			DebugPspMemory.CpuThreadState = CpuThreadState;
			DebugPspMemory.Dma = Dma;

			PspConfig.TraceJal = true;
			//PspConfig.TraceJIT = true;

			Console.SetWindowSize(120, 60);
			Console.SetBufferSize(120, 8000);

			PspConfig.MustLogWrites = true;

			var NandStream = File.OpenRead(NandPath);
			//var IplReader = new IplReader(new NandReader(NandStream));
			//var Info = IplReader.LoadIplToMemory(new PspMemoryStream(DebugPspMemory));

			//DebugPspMemory.Write4(0xBFC00FFC, 0x20040420);

			// It doesn't start the ME
			//DebugPspMemory.Write4(0xBFC00FFC, 0xFFFFFFFF);

			//uint StartPC = Info.EntryFunction;

			// PRE-IPL
			uint StartPC = 0x1FC00000;
			DebugPspMemory.WriteBytes(StartPC, File.ReadAllBytes(PreIplPath));
			// PRE-IPL
			

			/*
			ME:
			li      $t0, 0x40EC19C
			jr      $t0
			nop
			*/

			//IplReader.WriteIplToFile(File.Open(NandPath + ".ipl.bin", FileMode.Create, FileAccess.Write));

			var LLEState = new LLEState();

			Dma.LLEState = LLEState;
			LLEState.GPIO = new LleGPIO();
			LLEState.NAND = new LleNAND(NandStream);
			LLEState.Cpu = new LlePspCpu("CPU", PspEmulatorContext, CpuProcessor, StartPC);
			LLEState.Me = new LlePspCpu("ME", PspEmulatorContext, CpuProcessor, StartPC);
			LLEState.LleKirk = new LleKirk(DebugPspMemory);
			LLEState.Memory = DebugPspMemory;

			LLEState.Cpu.Start();

			while (true) Thread.Sleep(int.MaxValue);
		}

		
	}
}
