using Microsoft.VisualStudio.TestTools.UnitTesting;
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
		protected CpuProcessor Processor;
		protected MipsAssembler MipsAssembler;

		[TestInitialize]
		public void SetUp()
		{
			PspConfig = new PspConfig();
			PspEmulatorContext = new PspEmulatorContext(PspConfig);
			PspEmulatorContext.SetInstanceType<PspMemory, LazyPspMemory>();

			Processor = PspEmulatorContext.GetInstance<CpuProcessor>();
			MipsAssembler = new MipsAssembler(new PspMemoryStream(PspEmulatorContext.GetInstance<PspMemory>()));
		}

		[TestMethod]
		public void ManagerTest()
		{
		}
	}
}
