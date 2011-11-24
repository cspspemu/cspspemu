using CSPspEmu.Hle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Assembler;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class HlePspThreadManagerTest
	{
		protected PspEmulatorContext PspEmulatorContext;
		protected PspConfig PspConfig;
		protected LazyPspMemory Memory;
		protected CpuProcessor Processor;
		protected MipsAssembler MipsAssembler;

		[TestInitialize()]
		public void SetUp()
		{
			PspConfig = new PspConfig();
			PspEmulatorContext = new PspEmulatorContext(PspConfig);
			Memory = PspEmulatorContext.GetInstance<LazyPspMemory>();

			Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
			MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));
		}

		[TestMethod]
		public void ManagerTest()
		{
		}
	}
}
