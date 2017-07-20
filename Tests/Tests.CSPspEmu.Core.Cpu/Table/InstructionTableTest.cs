using CSPspEmu.Core.Cpu.Table;
using Xunit;


namespace CSPspEmu.Core.Tests
{
    
    public class InstructionTableTest
    {
        [Fact]
        public void PspInstructionsTest()
        {
            var Instructions = InstructionTable.All;
        }
    }
}