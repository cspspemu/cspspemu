using CSPspEmu.Core.Cpu.Table;
using NUnit.Framework;
using System;
using System.Collections.Generic;

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