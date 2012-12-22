using System;
using System.IO;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;
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

		[Inject]
		DebugPspMemory PspMemory;

		[Inject]
		CpuProcessor CpuProcessor;

		[Inject]
		InjectContext InjectContext;

		public Program()
		{
			var InjectContext = new InjectContext();
			{
				InjectContext.SetInstanceType<PspMemory, DebugPspMemory>();
				InjectContext.SetInstanceType<DebugPspMemory, DebugPspMemory>();
			}
			InjectContext.InjectDependencesTo(this);
		}

		public void Run()
		{
			var CpuThreadState = new CpuThreadState(CpuProcessor);
			var Dma = new Dma(CpuThreadState);
		
			Console.SetWindowSize(120, 60);
			Console.SetBufferSize(120, 8000);

			var NandStream = File.OpenRead(NandPath);
			var IplReader = new IplReader(new NandReader(NandStream));
			var Info = IplReader.LoadIplToMemory(new PspMemoryStream(PspMemory));
			uint StartPC = Info.EntryFunction;

			var LLEState = new LLEState();

			Dma.LLEState = LLEState;
			LLEState.GPIO = new LleGPIO();
			LLEState.NAND = new LleNAND(NandStream);
			LLEState.Cpu = new LlePspCpu("CPU", InjectContext, CpuProcessor, StartPC);
			LLEState.Me = new LlePspCpu("ME", InjectContext, CpuProcessor, StartPC);
			LLEState.LleKirk = new LleKirk(PspMemory);
			LLEState.Memory = PspMemory;

			LLEState.Cpu.Start();

			while (true) Thread.Sleep(int.MaxValue);
		}

		static void Main(string[] args)
		{
			new Program().Run();


			//DebugPspMemory.Write4(0xBFC00FFC, 0x20040420);

			// It doesn't start the ME
			//DebugPspMemory.Write4(0xBFC00FFC, 0xFFFFFFFF);

			/*
			ME:
			li      $t0, 0x40EC19C
			jr      $t0
			nop
			*/

			//IplReader.WriteIplToFile(File.Open(NandPath + ".ipl.bin", FileMode.Create, FileAccess.Write));

			
		}
	}
}
