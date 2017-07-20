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
        private static readonly AstGenerator ast = AstGenerator.Instance;

        public GeneratorCSharpTest()
        {
            GeneratorCSharp = new GeneratorCSharp();
        }

        public static void TestAstSetGetLValue_Set(int Index, int Value)
        {
        }

        public static int TestAstSetGetLValue_Get(int Index)
        {
            return 0;
        }

        [Fact]
        public void TestAstSetGetLValue()
        {
            var AstIndex = ast.Immediate(777);
            var AstSetGet = ast.SetGetLValue(
                ast.CallStatic((Action<int, int>) TestAstSetGetLValue_Set, AstIndex,
                    ast.SetGetLValuePlaceholder<int>()),
                ast.CallStatic((Func<int, int>) TestAstSetGetLValue_Get, AstIndex)
            );
            Assert.Equal("GeneratorCSharpTest.TestAstSetGetLValue_Set(777, 11);",
                GeneratorCSharp.Reset().GenerateRoot(ast.Assign(AstSetGet, 11)).ToString());
            Assert.Equal("GeneratorCSharpTest.TestAstSetGetLValue_Set(777, 12);",
                GeneratorCSharp.Reset().GenerateRoot(ast.Assign(AstSetGet, 12)).ToString());
            Assert.Equal(
                "GeneratorCSharpTest.TestAstSetGetLValue_Set(777, (GeneratorCSharpTest.TestAstSetGetLValue_Get(777) + 1));",
                GeneratorCSharp.Reset().GenerateRoot(ast.Assign(AstSetGet, AstSetGet + 1)).ToString());
        }

        [Fact]
        public void TestAstExpression()
        {
            ;
            Assert.Equal("(3 + 5)", GeneratorCSharp.GenerateRoot(ast.Binary(3, "+", 5)).ToString());
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

            Assert.Equal(Expected, Actual);
        }

        public static int GetTestValue(int Value)
        {
            return 333 * Value;
        }
    }
}