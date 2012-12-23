using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu.Dynarec;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using Tests.CSPspEmu.Core.Cpu.Cpu;

namespace CSPspEmu.Tests.Cpu.Dynarec
{
	[TestFixture]
	public class DynarecFunctionCompilerTest
	{
		[Test]
		public void TestMethod1()
		{
			var CpuProcessor = CpuUtils.CreateCpuProcessor();

			var DynarecFunction = CpuProcessor.DynarecFunctionCompiler.CreateFunction(
				new InstructionArrayReader(MipsAssembler.StaticAssembleInstructions(@"
					addi r1, r1, 1
					jr r31
					nop
				").Instructions),
				 0
			);

			var CpuThreadState = new CpuThreadState(CpuProcessor);
			Assert.AreEqual(0, CpuThreadState.GPR[1]);
			DynarecFunction.Delegate(CpuThreadState);
			Assert.AreEqual(1, CpuThreadState.GPR[1]);
		}
	}
}
