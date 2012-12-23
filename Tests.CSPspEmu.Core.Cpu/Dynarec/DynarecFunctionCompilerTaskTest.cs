using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;

namespace CSPspEmu.Tests.Cpu.Dynarec
{
	[TestFixture]
	public class DynarecFunctionCompilerTaskTest
	{
		[Test]
		public void TestMethod1()
		{
			Assert.Inconclusive();
			//var Config = new PspConfig();
			//var PspEmulatorContext = new PspEmulatorContext(Config);
			//PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();
			//
			//var PspMemory = PspEmulatorContext.GetInstance<PspMemory>();
			//var PspMemoryStream = new PspMemoryStream(PspMemory);
			//var MipsAssembler = new MipsAssembler(PspMemoryStream);
			////var DynarecFunctionCompilerTask = PspEmulatorContext.GetInstance<DynarecFunctionCompilerTask>();
			//var CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			//var CpuThreadState = new CpuThreadState(CpuProcessor);
			//
			//MipsAssembler.Assemble(@"
			//	.code 0x08000000
			//	addi r1, r1, 1
			//	jr r31
			//	nop
			//");
			//
			////var DynarecFunction = DynarecFunctionCompilerTask.GetFunctionForAddress(PspMemory.MainSegment.Low);
			//
			//Assert.AreEqual(0, CpuThreadState.GPR[1]);
			//DynarecFunction.Delegate(CpuThreadState);
			//Assert.AreEqual(1, CpuThreadState.GPR[1]);
		}
	}
}
