using CSPspEmu.Core.Cpu.Table;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Core.Tests
{
	[TestClass]
	public class EmitLookupGeneratorTest
	{
		public class HandlerClass
		{
			public List<int> Values = new List<int>();

			public void Test1()
			{
				Values.Add(1);
			}

			public void Test2()
			{
				Values.Add(2);
			}

			public void Test3()
			{
				Values.Add(3);
			}

			public void Default()
			{
				Values.Add(0);
			}
		}

		[TestMethod]
		public void GenerateSwitchDelegateTest()
		{
			var EmitLookupGenerator = new EmitLookupGenerator();
			var Callback = EmitLookupGenerator.GenerateSwitchDelegate<HandlerClass>(new InstructionInfo[] {
				new InstructionInfo() {
					BinaryEncoding = "----------------------------0001",
					Name = "Test1"
				},
				new InstructionInfo() {
					BinaryEncoding = "----------------------------0010",
					Name = "Test2"
				},
				new InstructionInfo() {
					BinaryEncoding = "----------------------------01--",
					Name = "Test3"
				},
			});
			
			var HandlerClass = new HandlerClass();
			Callback(Convert.ToUInt32("0000", 2), HandlerClass);
			Callback(Convert.ToUInt32("0001", 2), HandlerClass);
			Callback(Convert.ToUInt32("0010", 2), HandlerClass);
			Callback(Convert.ToUInt32("0011", 2), HandlerClass);
			Callback(Convert.ToUInt32("0100", 2), HandlerClass);
			Callback(Convert.ToUInt32("0110", 2), HandlerClass);
			Callback(Convert.ToUInt32("1110", 2), HandlerClass);
			Assert.AreEqual("0,1,2,0,3,3,0", String.Join(",", HandlerClass.Values));
		}
	}
}
