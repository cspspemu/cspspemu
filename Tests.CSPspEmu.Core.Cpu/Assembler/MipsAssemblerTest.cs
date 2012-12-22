using CSPspEmu.Core.Cpu.Assembler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils.Streams;
using CSharpUtils;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class MipsAssemblerTest
	{
		[TestMethod]
		public void AssembleLineTest()
		{
			var MipsAssembler = new MipsAssembler(new MemoryStream());
			Assert.AreEqual((uint)0x00020820, (uint)MipsAssembler.AssembleInstruction("add r1, r0, r2").Value);
		}

		[TestMethod]
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

			Assert.AreEqual(8, MemoryStream.Length);
			Assert.AreEqual((uint)0x00020820, BinaryReader.ReadUInt32());
			Assert.AreEqual((uint)0x03E71822, BinaryReader.ReadUInt32());
		}

		[TestMethod]
		public void MatchFormatTest()
		{
			var Parts = MipsAssembler.Matcher("%d, %s, %t", "  r1,  r2,   r3  ");
			Assert.AreEqual(3, Parts.Count);
			Assert.AreEqual("r1", Parts["%d"]);
			Assert.AreEqual("r2", Parts["%s"]);
			Assert.AreEqual("r3", Parts["%t"]);
		}

		[TestMethod]
		[ExpectedException(typeof(Exception))]
		public void MatcherNoMatchTest()
		{
			MipsAssembler.Matcher("add %s, %t", "add 1, 2, 3");
		}
	}
}
