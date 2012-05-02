using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codegen.Tests
{
	[TestClass]
	public class SafeILGeneratorTest
	{
		[TestMethod]
		public void TestGenerate()
		{
			var Adder = SafeILGenerator.Generate<Func<int, int, int>>("TestGenerate", (Generator) =>
			{
				Generator.LoadArgument<int>(0);
				Generator.LoadArgument<int>(1);
				Generator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				Generator.Return();
			});
			Assert.AreEqual(3, Adder(1, 2));
		}

		[TestMethod]
		public void TestLog()
		{
			SafeILGenerator SafeILGenerator = null;

			var AdderPlus16 = SafeILGenerator.Generate<Func<int, int, int>>("TestGenerate", (Generator) =>
			{
				SafeILGenerator = Generator;
				Generator.LoadArgument<int>(0);
				Generator.LoadArgument<int>(1);
				Generator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				Generator.Push((int)16);
				Generator.BinaryOperation(SafeBinaryOperator.AdditionSigned);
				Generator.Return();
			}, DoLog: true);

			Assert.AreEqual(
				String.Join("\r\n", new string[] {
					"Op.ldarg.0",
					"Op.ldarg.1",
					"Op.add",
					"Op.ldc.i4.s 16",
					"Op.add",
					"Op.ret",
				}),
				String.Join("\r\n", SafeILGenerator.EmittedInstructions)
			);
		}

		[TestMethod]
		public void TestSwitch()
		{
			var Switcher = SafeILGenerator.Generate<Func<int, int>>("TestSwitch", (Generator) =>
			{
				var Local = Generator.DeclareLocal<int>("Value");
				Generator.Push((int)-33);
				Generator.StoreLocal(Local);

				Generator.LoadArgument<int>(0);
				Generator.Switch(
					// List
					new int[] { 0, 2, 3 },
					// Integer Selector
					(Value) => Value,
					// Case
					(Value) =>
					{
						Generator.Push(Value);
						Generator.StoreLocal(Local);
					},
					// Default
					() =>
					{
						Generator.Push(-99);
						Generator.StoreLocal(Local);
					}
				);
				Generator.LoadLocal(Local);
				Generator.Return();
			});

			var ExpectedItems = new int[] { -99, 0, -99, 2, 3, -99 };
			var GeneratedItems = new int[] { -1, 0, 1, 2, 3, 4 }.Select(Item => Switcher(Item));
			CollectionAssert.AreEquivalent(ExpectedItems.ToArray(), GeneratedItems.ToArray());
		}
	}
}
