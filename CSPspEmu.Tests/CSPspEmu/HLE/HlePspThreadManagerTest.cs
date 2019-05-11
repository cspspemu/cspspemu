using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Memory;
using Xunit;

namespace Tests.CSPspEmu.Hle
{
    
    public class HlePspThreadManagerTest
    {
        [Inject] protected InjectContext InjectContext;
        [Inject] protected CpuProcessor Processor;

        MipsAssembler _mipsAssembler;

        public HlePspThreadManagerTest()
        {
            TestHleUtils.CreateInjectContext(this);
            _mipsAssembler = new MipsAssembler(new PspMemoryStream(InjectContext.GetInstance<PspMemory>()));
        }

        [Fact]
        public void ManagerTest()
        {
        }
    }
}