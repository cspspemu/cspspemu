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
		private HleThreadManager ThreadManager;

		[TestInitialize()]
		public void SetUp()
		{
			PspConfig = new PspConfig();
			PspConfig.HleModulesDll = Assembly.GetExecutingAssembly();
			PspEmulatorContext = new PspEmulatorContext(PspConfig);
			PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();
			PspEmulatorContext.SetInstanceType<GpuImpl, GpuImplNull>();
			PspEmulatorContext.SetInstanceType<PspAudioImpl, AudioImplNull>();
			Memory = PspEmulatorContext.GetInstance<PspMemory>();
			ThreadManager = PspEmulatorContext.GetInstance<HleThreadManager>();

			Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
			MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));
		}

		[TestMethod()]
		public void CpuThreadStateTest()
		{
			var HlePspThread = new HleThread(PspEmulatorContext, new CpuThreadState(Processor));

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
			var HlePspThread = new HleThread(PspEmulatorContext, new CpuThreadState(Processor));

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
