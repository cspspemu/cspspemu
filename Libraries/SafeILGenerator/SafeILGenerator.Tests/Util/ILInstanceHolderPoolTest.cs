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
			var pool = new IlInstanceHolderPool(typeof(int), 16);
			var item = pool.Alloc();
			item.Value = 10;
			Assert.AreEqual(10, item.Value);
			Assert.AreEqual(15, pool.FreeCount);
			item.Free();
			Assert.AreEqual(16, pool.FreeCount);
		}

		[TestMethod]
		public void TestMethod2()
		{
			var pool = new IlInstanceHolderPool(typeof(int), 16);
			var item = pool.Alloc();
			var astNode = ast.Statements(
				ast.Assign(ast.StaticFieldAccess(item.FieldInfo), ast.Argument<int>(0, "Value")),
				ast.Return()
			);
			Console.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(astNode));
			var generatorIl = new GeneratorIL();
			var itemSet = generatorIl.GenerateDelegate<Action<int>>("ItemSet", astNode);
			itemSet(10);
			Assert.AreEqual(10, item.Value);
		}

		[TestMethod]
		public void TestMethod3()
		{
			var pool1 = new IlInstanceHolderPool(typeof(int), 16);
			var pool2 = new IlInstanceHolderPool(typeof(int), 16);
			var item1 = pool1.Alloc();
			var item2 = pool2.Alloc();
			item1.Value = 11;
			item2.Value = 22;
			Assert.AreEqual(11, item1.Value);
			Assert.AreEqual(22, item2.Value);
		}

		[TestMethod]
		public void TestGlobalAlloc()
		{
			Assert.AreEqual(0, IlInstanceHolder.CapacityCount);
			Assert.AreEqual(0, IlInstanceHolder.FreeCount);
			
			var globalKey = IlInstanceHolder.TAlloc<int>();

			Assert.AreEqual(4, IlInstanceHolder.CapacityCount);
			Assert.AreEqual(3, IlInstanceHolder.FreeCount);

			globalKey.Value = 10;
			globalKey.Free();

			Assert.AreEqual(4, IlInstanceHolder.CapacityCount);
			Assert.AreEqual(4, IlInstanceHolder.FreeCount);
		}

	}
}
