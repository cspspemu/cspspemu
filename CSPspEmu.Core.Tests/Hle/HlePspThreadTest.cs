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
		protected LazyPspMemory Memory;
		protected Processor Processor;
		protected MipsAssembler MipsAssembler;

		[TestInitialize()]
		public void SetUp()
		{
			Memory = new LazyPspMemory();
			Processor = new Processor(Memory);
			MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));
		}

		[TestMethod()]
		public void CpuThreadStateTest()
		{
			var HlePspThread = new HlePspThread(new CpuThreadState(Processor));

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
			var HlePspThread = new HlePspThread(new CpuThreadState(Processor));

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
