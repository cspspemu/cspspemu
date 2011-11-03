using CSPspEmu.Core.Cpu.Assembler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils.Streams;
using CSharpUtils.Extensions;

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
			var Parts = MipsAssembler.MatchFormat("%d, %s, %t", "  r1,  r2,   r3  ");
			Assert.AreEqual(3, Parts.Count);
			Assert.AreEqual("(%d, r1)", Parts[0].ToString());
			Assert.AreEqual("(%s, r2)", Parts[1].ToString());
			Assert.AreEqual("(%t, r3)", Parts[2].ToString());
		}

		[TestMethod]
		public void TokenizeTest()
		{
			CollectionAssert.AreEquivalent(
				new List<String>(new String[] { "add", "r1", ",", "r2", ",", "r3" }),
				new List<String>(MipsAssembler.Tokenize("  add r1, r2,   r3   "))
			);

			CollectionAssert.AreEquivalent(
				new List<String>(new String[] { ",", ",", ",", ",", ".", ".", ".", "[", "]" }),
				new List<String>(MipsAssembler.Tokenize("  ,, , , .. .[]\t"))
			);
		}
	}
}
