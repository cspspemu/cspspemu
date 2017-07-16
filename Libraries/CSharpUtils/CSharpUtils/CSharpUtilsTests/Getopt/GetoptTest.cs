using System.Collections.Generic;
using CSharpUtils.Getopt;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSharpUtilsTests
{
	[TestClass]
	public class GetoptTest
	{
		[TestMethod]
		public void AddRuleTest()
		{
			bool BooleanValue = false;
			int IntegerValue = 0;
			var Getopt = new Getopt(new string[] { "-b", "-i", "50" });
			Getopt.AddRule("-b", ref BooleanValue);
			Getopt.AddRule("-i", ref IntegerValue);
			Getopt.Process();

			Assert.AreEqual(true, BooleanValue);
			Assert.AreEqual(50, IntegerValue);
		}

		[TestMethod]
		public void AddRule2Test()
		{
			bool BooleanValue = false;
			int IntegerValue = 0;
			string StringValue = "";
			var Getopt = new Getopt(new string[] { "-b", "-i", "50", "-s", "hello_world" });
			Getopt.AddRule("-b", (bool _Value) =>
			{
				BooleanValue = _Value;
			});
			Getopt.AddRule("-i", (int _Value) =>
			{
				IntegerValue = _Value;
			});
			Getopt.AddRule("-s", (string _Value) =>
			{
				StringValue = _Value;
			});
			Getopt.Process();

			Assert.AreEqual(true, BooleanValue);
			Assert.AreEqual(50, IntegerValue);
			Assert.AreEqual("hello_world", StringValue);
		}

		[TestMethod]
		public void AddRule3Test()
		{
			var Values = new List<int>();
			var Getopt = new Getopt(new string[] { "-i=50", "-i=25" });
			Getopt.AddRule("-i", (int Value) =>
			{
				Values.Add(Value);
			});
			Getopt.Process();
			Assert.AreEqual("50,25", Values.ToStringArray());
		}

		[TestMethod]
		public void AddRule4Test()
		{
			int ExecutedCount = 0;
			var Getopt = new Getopt(new string[] { "-a", "-a" });
			Getopt.AddRule("-a", () =>
			{
				ExecutedCount++;
			});
			Getopt.Process();
			Assert.AreEqual(2, ExecutedCount);
		}
	}
}
