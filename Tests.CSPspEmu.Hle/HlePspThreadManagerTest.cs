using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Assembler;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class HlePspThreadManagerTest
	{
		protected InjectContext InjectContext;
		protected CpuProcessor Processor;
		protected MipsAssembler MipsAssembler;

		[TestInitialize]
		public void SetUp()
		{
			InjectContext = new InjectContext();
			InjectContext.SetInstanceType<PspMemory, LazyPspMemory>();

			Processor = InjectContext.GetInstance<CpuProcessor>();
			MipsAssembler = new MipsAssembler(new PspMemoryStream(InjectContext.GetInstance<PspMemory>()));
		}

		[TestMethod]
		public void ManagerTest()
		{
		}
	}
}
