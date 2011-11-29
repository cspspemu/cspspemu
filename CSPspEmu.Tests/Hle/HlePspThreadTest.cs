using CSPspEmu.Hle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Assembler;

namespace CSPspEmu.Core.Tests
{
	[TestClass()]
	public class HlePspThreadTest
	{
		protected PspEmulatorContext PspEmulatorContext;
		protected PspConfig PspConfig;
		protected PspMemory Memory;
		protected CpuProcessor Processor;
		protected MipsAssembler MipsAssembler;

		[TestInitialize()]
		public void SetUp()
		{
			PspConfig = new PspConfig();
			PspEmulatorContext = new PspEmulatorContext(PspConfig);
			PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();
			Memory = PspEmulatorContext.GetInstance<PspMemory>();

			Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
			MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));
		}

		[TestMethod()]
		public void CpuThreadStateTest()
		{
			var HlePspThread = new HleThread(new CpuThreadState(Processor));

			MipsAssembler.Assemble(@"
			.code 0x08000000
				li r31, 0x08000000
				jal end
				nop
			end:
				addi r1, r1, 1
				jr r31
				nop
			");

			HlePspThread.CpuThreadState.PC = 0x08000000;
			HlePspThread.Step();
		}

		[TestMethod()]
		public void CpuThreadStateBugTest()
		{
			var HlePspThread = new HleThread(new CpuThreadState(Processor));

			MipsAssembler.Assemble(@"
			.code 0x08000000
				li r31, 0x08000000
				jal end
				nop
			end:
				jr r31
				nop
			");

			HlePspThread.CpuThreadState.PC = 0x08000000;

			Assert.Inconclusive();

			Console.WriteLine("1");
			HlePspThread.Step();
			Console.WriteLine("2");
		}
	}
}
