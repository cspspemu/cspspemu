using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Assembler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class HlePspThreadManagerTest
	{
		[Inject]
		InjectContext InjectContext;

		[Inject]
		CpuProcessor Processor;
	
		MipsAssembler MipsAssembler;

		[TestInitialize]
		public void SetUp()
		{
			TestHleUtils.CreateInjectContext(this);
			MipsAssembler = new MipsAssembler(new PspMemoryStream(InjectContext.GetInstance<PspMemory>()));
		}

		[TestMethod]
		public void ManagerTest()
		{
		}
	}
}
