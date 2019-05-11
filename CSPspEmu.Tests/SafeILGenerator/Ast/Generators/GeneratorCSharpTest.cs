using System;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using System.Text.RegularExpressions;
using Xunit;


namespace SafeILGenerator.Tests.Ast.Generators
{
    public class GeneratorCSharpTest
    {
        GeneratorCSharp GeneratorCSharp;
        private static readonly AstGenerator Ast = AstGenerator.Instance;

        public GeneratorCSharpTest()
        {
            GeneratorCSharp = new GeneratorCSharp();
        }

        public static void TestAstSetGetLValue_Set(int index, int value)
        {
        }

        public static int TestAstSetGetLValue_Get(int index)
        {
            return 0;
        }

        [Fact]
        public void TestAstSetGetLValue()
        {
            var astIndex = Ast.Immediate(777);
            var astSetGet = Ast.SetGetLValue(
                Ast.CallStatic((Action<int, int>) TestAstSetGetLValue_Set, astIndex,
                    Ast.SetGetLValuePlaceholder<int>()),
                Ast.CallStatic((Func<int, int>) TestAstSetGetLValue_Get, astIndex)
            );
            Assert.Equal("GeneratorCSharpTest.TestAstSetGetLValue_Set(777, 11);",
                GeneratorCSharp.Reset().GenerateRoot(Ast.Assign(astSetGet, 11)).ToString());
            Assert.Equal("GeneratorCSharpTest.TestAstSetGetLValue_Set(777, 12);",
                GeneratorCSharp.Reset().GenerateRoot(Ast.Assign(astSetGet, 12)).ToString());
            Assert.Equal(
                "GeneratorCSharpTest.TestAstSetGetLValue_Set(777, (GeneratorCSharpTest.TestAstSetGetLValue_Get(777) + 1));",
                GeneratorCSharp.Reset().GenerateRoot(Ast.Assign(astSetGet, astSetGet + 1)).ToString());
        }

        [Fact]
        public void TestAstExpression()
        {
            Assert.Equal("(3 + 5)", GeneratorCSharp.GenerateRoot(Ast.Binary(3, "+", 5)).ToString());
        }

        [Fact]
        public void TestAstIf()
        {
            GeneratorCSharp.GenerateRoot(new AstNodeStmIfElse(new AstNodeExprImm(true), new AstNodeStmReturn(),
                new AstNodeStmReturn()));
            Assert.Equal("if (true) return; else return;", GeneratorCSharp.ToString());
        }

        [Fact]
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

            Assert.Equal("return GeneratorCSharpTest.GetTestValue(10);", GeneratorCSharp.ToString());
        }

        [Fact]
        public void TestAstSwitch()
        {
            var local = AstLocal.Create<int>("Local");
            var ast = Ast.Statements(
                Ast.Switch(
                    Ast.Local(local),
                    Ast.Default(Ast.Return("Nor One, nor Three")),
                    Ast.Case(1, Ast.Return("One")),
                    Ast.Case(3, Ast.Return("Three"))
                ),
                Ast.Return("Invalid!")
            );

            var actual = GeneratorCSharp.GenerateString<GeneratorCSharp>(ast);
            var expected = @"
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
            actual = new Regex(@"\s+").Replace(actual, " ").Trim();
            expected = new Regex(@"\s+").Replace(expected, " ").Trim();

            Assert.Equal(expected, actual);
        }

        public static int GetTestValue(int value)
        {
            return 333 * value;
        }
    }
}