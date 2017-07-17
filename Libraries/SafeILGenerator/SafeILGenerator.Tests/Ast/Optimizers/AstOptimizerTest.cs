using System;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Optimizers;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text;
using System.IO;
using SafeILGenerator.Ast.Serializers;
using NUnit.Framework;

namespace SafeILGenerator.Tests.Ast.Optimizers
{
    [TestFixture]
    public class AstOptimizerTest
    {
        private static AstGenerator ast = AstGenerator.Instance;

        [Test]
        public void TestCalculateImmediates()
        {
            var Node = (AstNode) ((ast.Immediate(0) + ast.Immediate(2)) * ast.Immediate(3));
            Assert.AreEqual("((0 + 2) * 3)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("6", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
        public void TestAdd0()
        {
            var Node = (AstNode) ((ast.Argument<int>(0, "Arg") + ast.Immediate(0)) * ast.Immediate(3));
            Assert.AreEqual("((Arg + 0) * 3)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("(Arg * 3)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
        public void TestTripleCast()
        {
            var Node = (AstNode) ast.Cast<int>(ast.Cast<uint>(ast.Immediate((int) 7)));
            Assert.AreEqual("((Int32)((UInt32)7))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("7", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
        public void TestCastToImmediate()
        {
            var Node = (AstNode) ast.Cast<uint>(ast.Immediate((int) 7));
            Assert.AreEqual("((UInt32)7)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("7", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
        public void TestCastSignExtend()
        {
            var Node = (AstNode) ast.Cast<uint>(ast.Cast<sbyte>(ast.Argument<int>(0, "Arg")));
            Assert.AreEqual("((UInt32)((SByte)Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("((UInt32)((SByte)Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
        public void TestCastSignExtend2()
        {
            var Node = (AstNode) ast.Cast<sbyte>(ast.Cast<uint>(ast.Argument<int>(0, "Arg")));
            Assert.AreEqual("((SByte)((UInt32)Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("((SByte)Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
        public void TestZeroMinusNumber()
        {
            var Node = (AstNode) ast.Binary(ast.Immediate(0), "-", ast.Argument<int>(0, "Arg"));
            Assert.AreEqual("(0 - Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("(-Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
        public void TestAddNegated()
        {
            var Node = (AstNode) ast.Binary(ast.Immediate(1), "+", -ast.Argument<int>(0, "Arg"));
            Assert.AreEqual("(1 + (-Arg))", new GeneratorCSharp().GenerateRoot(Node).ToString());
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual("(1 - Arg)", new GeneratorCSharp().GenerateRoot(Node).ToString());
        }

        [Test]
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
            Assert.AreEqual(
                "AstNodeStmContainer(AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn(), AstNodeStmReturn())",
                AstSerializer.Serialize(Node)
            );
        }

        [Test]
        public void TestCompactStmContainer2()
        {
            var Node = (AstNode) ast.Statements(
                ast.Return()
            );
            Node = new AstOptimizer().Optimize(Node);
            Assert.AreEqual(
                "<AstNodeStmReturn />",
                AstSerializer.SerializeAsXml(Node, false)
            );
        }
    }
}