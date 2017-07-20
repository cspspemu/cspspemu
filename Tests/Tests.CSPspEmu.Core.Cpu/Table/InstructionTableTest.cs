using CSPspEmu.Core.Cpu.Table;
using NUnit.Framework;

namespace CSPspEmu.Core.Tests
{
    [TestFixture]
    public class InstructionTableTest
    {
        [Test]
        public void PspInstructionsTest()
        {
            var Instructions = InstructionTable.All;
        }
    }
}