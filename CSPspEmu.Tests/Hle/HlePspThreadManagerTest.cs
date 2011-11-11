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
		protected PspConfig PspConfig;
		protected LazyPspMemory Memory;
		protected CpuProcessor Processor;
		protected MipsAssembler MipsAssembler;

		[TestInitialize()]
		public void SetUp()
		{
			Memory = new LazyPspMemory();
			PspConfig = new PspConfig();
			Processor = new CpuProcessor(PspConfig, Memory);
			MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));
		}

		[TestMethod]
		public void ManagerTest()
		{
		}
	}
}
