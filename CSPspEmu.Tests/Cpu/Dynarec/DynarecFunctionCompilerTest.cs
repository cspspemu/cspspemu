using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Tests.Cpu.Dynarec
{
	[TestClass]
	public class DynarecFunctionCompilerTest
	{
		[TestMethod]
		public void TestMethod1()
		{
			var Config = new PspConfig();
			var PspEmulatorContext = new PspEmulatorContext(Config);
			PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();

			var DynarecFunctionCompiler = PspEmulatorContext.GetInstance<DynarecFunctionCompiler>();
			var CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			var CpuThreadState = new CpuThreadState(CpuProcessor);
			
			var DynarecFunction = DynarecFunctionCompiler.CreateFunction(
				new InstructionArrayReader(MipsAssembler.StaticAssembleInstructions(@"
					addi r1, r1, 1
					jr r31
					nop
				")),
				 0
			);

			Assert.AreEqual(0, CpuThreadState.GPR[1]);
			DynarecFunction.Delegate(CpuThreadState);
			Assert.AreEqual(1, CpuThreadState.GPR[1]);
		}
	}
}
