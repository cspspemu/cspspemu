
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Generators;
using Xunit;

namespace SafeILGenerator.Tests
{
    
    public class BenchMarkTest
    {
        private static AstGenerator ast = AstGenerator.Instance;

        [Fact]
        public void TestBenchmark()
        {
            var GeneratorCSharp = new GeneratorCSharp();
            var AstNode = ast.Ternary(ast.Unary("-", ast.Binary(ast.Binary(10, "+", 11), "*", 2)), 1, 2);
            for (int n = 0; n < 20000; n++)
            {
                GeneratorCSharp.Reset().GenerateRoot(AstNode);
            }
            Assert.Equal(GeneratorCSharp.ToString(), "((-((10 + 11) * 2)) ? 1 : 2)");
        }
    }
}