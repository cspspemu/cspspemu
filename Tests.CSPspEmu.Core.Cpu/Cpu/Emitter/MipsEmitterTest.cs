using CSPspEmu.Core.Cpu.Emitter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu;
using System.Reflection.Emit;
using CSPspEmu.Core.Memory;
using SafeILGenerator;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public unsafe class MipsEmitterTest
	{
		[TestMethod]
		public void CreateDelegateTest()
		{
			Assert.Inconclusive();
			
			//var PspConfig = new PspConfig();
			//var PspEmulatorContext = new PspEmulatorContext(PspConfig);
			//PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();
			//var Memory = PspEmulatorContext.GetInstance<PspMemory>();
			//var Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
			//var CpuThreadState = new CpuThreadState(Processor);
			//var MipsEmiter = new MipsMethodEmitter(Processor, 0);
			//CpuThreadState.GPR[1] = 1;
			//CpuThreadState.GPR[2] = 2;
			//CpuThreadState.GPR[3] = 3;
			//MipsEmiter.OP_3REG_Unsigned(1, 2, 2, () => { MipsEmiter.SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); });
			//MipsEmiter.OP_3REG_Unsigned(0, 2, 2, () => { MipsEmiter.SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); });
			//MipsEmiter.OP_2REG_IMM_Signed(10, 0, 1000, () => { MipsEmiter.SafeILGenerator.BinaryOperation(SafeBinaryOperator.AdditionSigned); });
			//MipsEmiter.CreateDelegate()(CpuThreadState);
			//Assert.AreEqual(4, CpuThreadState.GPR[1]);
			//Assert.AreEqual(0, CpuThreadState.GPR[0]);
			//Assert.AreEqual(1000, CpuThreadState.GPR[10]);
		}
	}
}
