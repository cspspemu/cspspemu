using CSPspEmu.Core.Cpu.Assembler;
using System;
using System.IO;
using CSharpUtils.Extensions;
using Xunit;


namespace CSPspEmu.Core.Tests
{
    
    public class MipsAssemblerTest
    {
        [Fact]
        public void AssembleLineTest()
        {
            var MipsAssembler = new MipsAssembler(new MemoryStream());
            Assert.Equal((uint) 0x00020820, (uint) MipsAssembler.AssembleInstruction("add r1, r0, r2").Value);
        }

        [Fact]
        public void AssembleTest()
        {
            var MemoryStream = new MemoryStream();
            var BinaryReader = new BinaryReader(MemoryStream);

            MemoryStream.PreservePositionAndLock(() =>
            {
                var MipsAssembler = new MipsAssembler(MemoryStream);

                MipsAssembler.Assemble(@"
					add r1, r0, r2
					sub r3, r31, r7
				");
            });

            Assert.Equal(8, MemoryStream.Length);
            Assert.Equal((uint) 0x00020820, BinaryReader.ReadUInt32());
            Assert.Equal((uint) 0x03E71822, BinaryReader.ReadUInt32());
        }

        [Fact]
        public void MatchFormatTest()
        {
            var Parts = MipsAssembler.Matcher("%d, %s, %t", "  r1,  r2,   r3  ");
            Assert.Equal(3, Parts.Count);
            Assert.Equal("r1", Parts["%d"]);
            Assert.Equal("r2", Parts["%s"]);
            Assert.Equal("r3", Parts["%t"]);
        }

        [Fact]
        public void MatcherNoMatchTest()
        {
            Assert.Throws<Exception>(() => { MipsAssembler.Matcher("add %s, %t", "add 1, 2, 3"); });
        }
    }
}