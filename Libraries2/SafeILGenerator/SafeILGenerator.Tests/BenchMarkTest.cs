using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;

namespace SafeILGenerator.Tests
{
	[TestClass]
	public class BenchMarkTest
	{
		static private AstGenerator ast = AstGenerator.Instance;

		[TestMethod]
		public void TestBenchmark()
		{
			var GeneratorCSharp = new GeneratorCSharp();
			var AstNode = ast.Ternary(ast.Unary("-", ast.Binary(ast.Binary(10, "+", 11), "*", 2)), 1, 2);
			for (int n = 0; n < 20000; n++)
			{
				GeneratorCSharp.Reset().GenerateRoot(AstNode);
			}
			Assert.AreEqual(GeneratorCSharp.ToString(), "((-((10 + 11) * 2)) ? 1 : 2)");
		}
	}
}
