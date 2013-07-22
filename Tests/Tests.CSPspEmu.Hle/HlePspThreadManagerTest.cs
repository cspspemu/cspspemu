using NUnit.Framework;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Assembler;

namespace CSPspEmu.Core.Tests
{
	[TestFixture]
	public class HlePspThreadManagerTest
	{
		[Inject]
		InjectContext InjectContext;

		[Inject]
		CpuProcessor Processor;
	
		MipsAssembler MipsAssembler;

		[SetUp]
		public void SetUp()
		{
			TestHleUtils.CreateInjectContext(this);
			MipsAssembler = new MipsAssembler(new PspMemoryStream(InjectContext.GetInstance<PspMemory>()));
		}

		[Test]
		public void ManagerTest()
		{
		}
	}
}
