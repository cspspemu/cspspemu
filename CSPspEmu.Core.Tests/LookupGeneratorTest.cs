using CSPspEmu.Core.Cpu.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class LookupGeneratorTest
	{
		[TestMethod]
		public void GenerateSwitchCodeTest()
		{
			var LookupGenerator = new RoslynLookupGenerator();
			LookupGenerator.GenerateSwitchCode(new List<InstructionInfo>() { });
		}
	}
}
