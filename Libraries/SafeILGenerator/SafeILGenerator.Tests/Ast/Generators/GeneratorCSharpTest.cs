using System;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeILGenerator.Tests.Ast.Generators
{
    [TestClass]
    public class GeneratorCSharpTest
    {
        GeneratorCSharp GeneratorCSharp;
        static private readonly AstGenerator ast = AstGenerator.Instance;

        [TestInitialize]
        public void SetUp()
        {
            GeneratorCSharp = new GeneratorCSharp();
        }

        static public void TestAstSetGetLValue_Set(int Index, int Value)
        {
        }

        static public int TestAstSetGetLValue_Get(int Index)
        {
            return 0;
        }

        [TestMethod]
        public void TestAstSetGetLValue()
        {
            var AstIndex = ast.Immediate(777);
            var AstSetGet = ast.SetGetLValue(
                ast.CallStatic((Action<int, int>) TestAstSetGetLValue_Set, AstIndex,
                    ast.SetGetLValuePlaceholder<int>()),
                ast.CallStatic((Func<int, int>) TestAstSetGetLValue_Get, AstIndex)
            );
            Assert.AreEqual("GeneratorCSharpTest.TestAstSetGetLValue_Set(777, 11);",
                GeneratorCSharp.Reset().GenerateRoot(ast.Assign(AstSetGet, 11)).ToString());
            Assert.AreEqual("GeneratorCSharpTest.TestAstSetGetLValue_Set(777, 12);",
                GeneratorCSharp.Reset().GenerateRoot(ast.Assign(AstSetGet, 12)).ToString());
            Assert.AreEqual(
                "GeneratorCSharpTest.TestAstSetGetLValue_Set(777, (GeneratorCSharpTest.TestAstSetGetLValue_Get(777) + 1));",
                GeneratorCSharp.Reset().GenerateRoot(ast.Assign(AstSetGet, AstSetGet + 1)).ToString());
        }

        [TestMethod]
        public void TestAstExpression()
        {
            ;
            Assert.AreEqual("(3 + 5)", GeneratorCSharp.GenerateRoot(ast.Binary(3, "+", 5)).ToString());
        }

        [TestMethod]
        public void TestAstIf()
        {
            GeneratorCSharp.GenerateRoot(new AstNodeStmIfElse(new AstNodeExprImm(true), new AstNodeStmReturn(),
                new AstNodeStmReturn()));
            Assert.AreEqual("if (true) return; else return;", GeneratorCSharp.ToString());
        }

        [TestMethod]
        public void TestSimpleCall()
        {
            GeneratorCSharp.GenerateRoot(
                new AstNodeStmReturn(
                    new AstNodeExprCallStatic(
                        (Func<int, int>) GetTestValue,
                        new AstNodeExprImm(10)
                    )
                )
            );

            Assert.AreEqual("return GeneratorCSharpTest.GetTestValue(10);", GeneratorCSharp.ToString());
        }

        [TestMethod]
        public void TestAstSwitch()
        {
            var Local = AstLocal.Create<int>("Local");
            var Ast = ast.Statements(
                ast.Switch(
                    ast.Local(Local),
                    ast.Default(ast.Return("Nor One, nor Three")),
                    ast.Case(1, ast.Return("One")),
                    ast.Case(3, ast.Return("Three"))
                ),
                ast.Return("Invalid!")
            );

            var Actual = GeneratorCSharp.GenerateString<GeneratorCSharp>(Ast);
            var Expected = @"
				{
					switch (Local) {
						case 1:
							return ""One"";
						break;
						case 3:
							return ""Three"";
						break;
						default:
							return ""Nor One, nor Three"";
						break;
					}
					return ""Invalid!"";
				}
			";
            Actual = new Regex(@"\s+").Replace(Actual, " ").Trim();
            Expected = new Regex(@"\s+").Replace(Expected, " ").Trim();

            Assert.AreEqual(Expected, Actual);
        }

        static public int GetTestValue(int Value)
        {
            return 333 * Value;
        }
    }
}