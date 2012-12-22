using CSPspEmu.Hle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Audio;
using System.Reflection;
using System.Collections.Generic;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class HlePspThreadTest
	{
		[Inject]
		protected InjectContext InjectContext;
		
		[Inject]
		protected PspMemory Memory;

		[Inject]
		protected CpuProcessor Processor;

		[Inject]
		private HleThreadManager ThreadManager;

		protected MipsAssembler MipsAssembler;

		[TestInitialize()]
		public void SetUp()
		{
			TestHleUtils.CreateInjectContext(this);

			MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));
		}

		[TestMethod()]
		public void CpuThreadStateTest()
		{
			Assert.Inconclusive();
			//var HlePspThread = new HleThread(PspEmulatorContext, new CpuThreadState(Processor));
			//
			//MipsAssembler.Assemble(@"
			//.code 0x08000000
			//	li r31, 0x08000000
			//	jal end
			//	nop
			//end:
			//	addi r1, r1, 1
			//	jr r31
			//	nop
			//");
			//
			//HlePspThread.CpuThreadState.PC = 0x08000000;
			//HlePspThread.Step();
		}
	}
}
