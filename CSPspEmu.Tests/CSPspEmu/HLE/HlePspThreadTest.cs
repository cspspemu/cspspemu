using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Cpu.Assembler;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;
using Xunit;

namespace Tests.CSPspEmu.Hle
{
    
    public class HlePspThreadTest
    {
        [Inject] protected InjectContext InjectContext;

        [Inject] protected PspMemory Memory;

        [Inject] protected CpuProcessor Processor;

        [Inject] protected HleThreadManager ThreadManager;

        protected MipsAssembler MipsAssembler;

        public HlePspThreadTest()
        {
            TestHleUtils.CreateInjectContext(this);

            MipsAssembler = new MipsAssembler(new PspMemoryStream(Memory));
        }

        [Fact(Skip = "Inconclusive")]
        public void CpuThreadStateTest()
        {
            //Assert.Inconclusive();
            //var HlePspThread = new HleThread(PspEmulatorContext, new CpuThreadState(Processor));
            //
            //MipsAssembler.Assemble(@"
            //.code 0x08000000
            //	li r31, 0x08000000
            //	jal end
            //	nop
            //end:
            //	addi r1, r1, 1
            //	jr r31
            //	nop
            //");
            //
            //HlePspThread.CpuThreadState.PC = 0x08000000;
            //HlePspThread.Step();
        }
    }
}