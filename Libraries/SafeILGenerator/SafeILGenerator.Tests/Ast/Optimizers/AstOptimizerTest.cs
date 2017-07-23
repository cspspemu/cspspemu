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
        private static AstGenerator _ast = AstGenerator.Instance;

        [Fact]
        public void TestCalculateImmediates()
        {
            var node = (AstNode) ((_ast.Immediate(0) + _ast.Immediate(2)) * _ast.Immediate(3));
            Assert.Equal("((0 + 2) * 3)", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("6", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestAdd0()
        {
            var node = (AstNode) ((_ast.Argument<int>(0, "Arg") + _ast.Immediate(0)) * _ast.Immediate(3));
            Assert.Equal("((Arg + 0) * 3)", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("(Arg * 3)", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestTripleCast()
        {
            var node = (AstNode) _ast.Cast<int>(_ast.Cast<uint>(_ast.Immediate(7)));
            Assert.Equal("((Int32)((UInt32)7))", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("7", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestCastToImmediate()
        {
            var node = (AstNode) _ast.Cast<uint>(_ast.Immediate(7));
            Assert.Equal("((UInt32)7)", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("7", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestCastSignExtend()
        {
            var node = (AstNode) _ast.Cast<uint>(_ast.Cast<sbyte>(_ast.Argument<int>(0, "Arg")));
            Assert.Equal("((UInt32)((SByte)Arg))", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("((UInt32)((SByte)Arg))", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestCastSignExtend2()
        {
            var node = (AstNode) _ast.Cast<sbyte>(_ast.Cast<uint>(_ast.Argument<int>(0, "Arg")));
            Assert.Equal("((SByte)((UInt32)Arg))", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("((SByte)Arg)", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestZeroMinusNumber()
        {
            var node = (AstNode) _ast.Binary(_ast.Immediate(0), "-", _ast.Argument<int>(0, "Arg"));
            Assert.Equal("(0 - Arg)", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("(-Arg)", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestAddNegated()
        {
            var node = (AstNode) _ast.Binary(_ast.Immediate(1), "+", -_ast.Argument<int>(0, "Arg"));
            Assert.Equal("(1 + (-Arg))", new GeneratorCSharp().GenerateRoot(node).ToString());
            node = new AstOptimizer().Optimize(node);
            Assert.Equal("(1 - Arg)", new GeneratorCSharp().GenerateRoot(node).ToString());
        }

        [Fact]
        public void TestCompactStmContainer()
        {
            var node = (AstNode) _ast.Statements(
                _ast.Return(),
                _ast.Statements(
                    _ast.Statements(
                        _ast.Return()
                    ),
                    _ast.Return(),
                    _ast.Statements(_ast.Statements(_ast.Statements())),
                    _ast.Return(),
                    _ast.Statements(
                        _ast.Return()
                    )
                ),
                _ast.Return()
            );
            node = new AstOptimizer().Optimize(node);
            Assert.Equal(
                "AstNodeStmContainer(AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn())",
                AstSerializer.Serialize(node)
            );
        }

        [Fact]
        public void TestCompactStmContainer2()
        {
            var node = (AstNode) _ast.Statements(
                _ast.Return()
            );
            node = new AstOptimizer().Optimize(node);
            Assert.Equal(
                "<AstNodeStmReturn />",
                AstSerializer.SerializeAsXml(node, false)
            );
        }
    }
}