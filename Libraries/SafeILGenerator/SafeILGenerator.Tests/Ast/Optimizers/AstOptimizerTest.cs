using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Optimizers;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast.Serializers;
using Xunit;


namespace SafeILGenerator.Tests.Ast.Optimizers
{
    
    public class AstOptimizerTest
    {
        private static AstGenerator ast = AstGenerator.Instance;

        [Fact]
        public void TestCalculateImmediates()
        {
            var Node = (AstNode) ((ast.Immediate(0) + ast.Immediate(2)) * ast.Immediate(3));
            Assert.Equal("((0 + 2) * 3)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("6", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestAdd0()
        {
            var Node = (AstNode) ((ast.Argument<int>(0, "Arg") + ast.Immediate(0)) * ast.Immediate(3));
            Assert.Equal("((Arg + 0) * 3)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("(Arg * 3)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestTripleCast()
        {
            var Node = (AstNode) ast.Cast<int>(ast.Cast<uint>(ast.Immediate((int) 7)));
            Assert.Equal("((Int32)((UInt32)7))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("7", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestCastToImmediate()
        {
            var Node = (AstNode) ast.Cast<uint>(ast.Immediate((int) 7));
            Assert.Equal("((UInt32)7)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("7", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestCastSignExtend()
        {
            var Node = (AstNode) ast.Cast<uint>(ast.Cast<sbyte>(ast.Argument<int>(0, "Arg")));
            Assert.Equal("((UInt32)((SByte)Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("((UInt32)((SByte)Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestCastSignExtend2()
        {
            var Node = (AstNode) ast.Cast<sbyte>(ast.Cast<uint>(ast.Argument<int>(0, "Arg")));
            Assert.Equal("((SByte)((UInt32)Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("((SByte)Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestZeroMinusNumber()
        {
            var Node = (AstNode) ast.Binary(ast.Immediate(0), "-", ast.Argument<int>(0, "Arg"));
            Assert.Equal("(0 - Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("(-Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestAddNegated()
        {
            var Node = (AstNode) ast.Binary(ast.Immediate(1), "+", -ast.Argument<int>(0, "Arg"));
            Assert.Equal("(1 + (-Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal("(1 - Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Fact]
        public void TestCompactStmContainer()
        {
            var Node = (AstNode) ast.Statements(
                ast.Return(),
                ast.Statements(
                    ast.Statements(
                        ast.Return()
                    ),
                    ast.Return(),
                    ast.Statements(ast.Statements(ast.Statements())),
                    ast.Return(),
                    ast.Statements(
                        ast.Return()
                    )
                ),
                ast.Return()
            );
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal(
                "AstNodeStmContainer(AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn())",
                AstSerializer.Serialize(Node)
            );
        }

        [Fact]
        public void TestCompactStmContainer2()
        {
            var Node = (AstNode) ast.Statements(
                ast.Return()
            );
            Node = new AstOptimizer().Optimize(Node);
            Assert.Equal(
                "<AstNodeStmReturn />",
                AstSerializer.SerializeAsXml(Node, false)
            );
        }
    }
}