
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
            var generatorCSharp = new GeneratorCSharp();
            var astNode = ast.Ternary(ast.Unary("-", ast.Binary(ast.Binary(10, "+", 11), "*", 2)), 1, 2);
            for (var n = 0; n < 20000; n++)
            {
                generatorCSharp.Reset().GenerateRoot(astNode);
            }
            Assert.Equal(generatorCSharp.ToString(), "((-((10 + 11) * 2)) ? 1 : 2)");
        }
    }
}