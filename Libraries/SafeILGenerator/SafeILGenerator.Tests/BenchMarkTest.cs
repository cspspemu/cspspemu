using NUnit.Framework;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;

namespace SafeILGenerator.Tests
{
    [TestFixture]
    public class BenchMarkTest
    {
        private static AstGenerator ast = AstGenerator.Instance;

        [Test]
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