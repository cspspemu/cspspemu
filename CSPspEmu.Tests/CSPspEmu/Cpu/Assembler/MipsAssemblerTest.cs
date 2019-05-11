using System;
using System.IO;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Cpu.Assembler;
using Xunit;

namespace Tests.CSPspEmu.Core.Cpu.Assembler
{
    
    public class MipsAssemblerTest
    {
        [Fact]
        public void AssembleLineTest()
        {
            var mipsAssembler = new MipsAssembler(new MemoryStream());
            Assert.Equal((uint) 0x00020820, mipsAssembler.AssembleInstruction("add r1, r0, r2").Value);
        }

        [Fact]
        public void AssembleTest()
        {
            var memoryStream = new MemoryStream();
            var binaryReader = new BinaryReader(memoryStream);

            memoryStream.PreservePositionAndLock(() =>
            {
                var mipsAssembler = new MipsAssembler(memoryStream);

                mipsAssembler.Assemble(@"
					add r1, r0, r2
					sub r3, r31, r7
				");
            });

            Assert.Equal(8, memoryStream.Length);
            Assert.Equal((uint) 0x00020820, binaryReader.ReadUInt32());
            Assert.Equal((uint) 0x03E71822, binaryReader.ReadUInt32());
        }

        [Fact]
        public void MatchFormatTest()
        {
            var parts = MipsAssembler.Matcher("%d, %s, %t", "  r1,  r2,   r3  ");
            Assert.Equal(3, parts.Count);
            Assert.Equal("r1", parts["%d"]);
            Assert.Equal("r2", parts["%s"]);
            Assert.Equal("r3", parts["%t"]);
        }

        [Fact]
        public void MatcherNoMatchTest()
        {
            Assert.Throws<Exception>(() => { MipsAssembler.Matcher("add %s, %t", "add 1, 2, 3"); });
        }
    }
}