using System;
using SafeILGenerator.Utils;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeILGenerator.Tests.Util
{
	[TestClass]
	public class ILInstanceHolderPoolTest
	{
		private static AstGenerator ast = AstGenerator.Instance;

		[TestMethod]
		public void TestAllocAssignGetAndRelease()
		{
			var Pool = new ILInstanceHolderPool(typeof(int), 16);
			var Item = Pool.Alloc();
			Item.Value = 10;
			Assert.AreEqual(10, Item.Value);
			Assert.AreEqual(15, Pool.FreeCount);
			Item.Free();
			Assert.AreEqual(16, Pool.FreeCount);
		}

		[TestMethod]
		public void TestMethod2()
		{
			var Pool = new ILInstanceHolderPool(typeof(int), 16);
			var Item = Pool.Alloc();
			var AstNode = ast.Statements(
				ast.Assign(ast.StaticFieldAccess(Item.FieldInfo), ast.Argument<int>(0, "Value")),
				ast.Return()
			);
			Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(AstNode));
			var GeneratorIL = new GeneratorIL();
			var ItemSet = GeneratorIL.GenerateDelegate<Action<int>>("ItemSet", AstNode);
			ItemSet(10);
			Assert.AreEqual(10, Item.Value);
		}

		[TestMethod]
		public void TestMethod3()
		{
			var Pool1 = new ILInstanceHolderPool(typeof(int), 16);
			var Pool2 = new ILInstanceHolderPool(typeof(int), 16);
			var Item1 = Pool1.Alloc();
			var Item2 = Pool2.Alloc();
			Item1.Value = 11;
			Item2.Value = 22;
			Assert.AreEqual(11, Item1.Value);
			Assert.AreEqual(22, Item2.Value);
		}

		[TestMethod]
		public void TestGlobalAlloc()
		{
			Assert.AreEqual(0, ILInstanceHolder.CapacityCount);
			Assert.AreEqual(0, ILInstanceHolder.FreeCount);
			
			var GlobalKey = ILInstanceHolder.TAlloc<int>();

			Assert.AreEqual(4, ILInstanceHolder.CapacityCount);
			Assert.AreEqual(3, ILInstanceHolder.FreeCount);

			GlobalKey.Value = 10;
			GlobalKey.Free();

			Assert.AreEqual(4, ILInstanceHolder.CapacityCount);
			Assert.AreEqual(4, ILInstanceHolder.FreeCount);
		}

	}
}
