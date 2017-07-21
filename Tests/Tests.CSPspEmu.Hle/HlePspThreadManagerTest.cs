using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Cpu.Assembler;
using Xunit;


namespace CSPspEmu.Core.Tests
{
    
    public class HlePspThreadManagerTest
    {
        [Inject] InjectContext InjectContext;

        [Inject] CpuProcessor Processor;

        MipsAssembler MipsAssembler;

        public HlePspThreadManagerTest()
        {
            TestHleUtils.CreateInjectContext(this);
            MipsAssembler = new MipsAssembler(new PspMemoryStream(InjectContext.GetInstance<PspMemory>()));
        }

        [Fact]
        public void ManagerTest()
        {
        }
    }
}