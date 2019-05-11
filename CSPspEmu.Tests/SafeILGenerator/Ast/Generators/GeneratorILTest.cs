using System;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Utils;
using Xunit;


namespace SafeILGenerator.Tests.Ast.Generators
{
    public unsafe class GeneratorIlTest
    {
        private static readonly AstGenerator Ast = AstGenerator.Instance;
        private readonly GeneratorIl _generatorIl = new GeneratorIl();

        public static void TestAstSetGetLValue_Set(int index, int value)
        {
            Console.WriteLine("Set: {0}, {1}", index, value);
        }

        public static int TestAstSetGetLValue_Get(int index)
        {
            Console.WriteLine("Get: {0}", index);
            return 999;
        }

        [Fact(Skip = "Check")]
        public void TestAstSetGetLValue()
        {
            var astIndex = Ast.Immediate(777);
            var astSetGet = Ast.SetGetLValue(
                Ast.CallStatic((Action<int, int>) TestAstSetGetLValue_Set, astIndex,
                    Ast.SetGetLValuePlaceholder<int>()),
                Ast.CallStatic((Func<int, int>) TestAstSetGetLValue_Get, astIndex)
            );
            var astFunc = Ast.Statements(
                Ast.Assign(astSetGet, 11),
                Ast.Assign(astSetGet, 12),
                Ast.Assign(astSetGet, astSetGet + 3),
                Ast.Return()
            );

            //Console.WriteLine(GeneratorIL.GenerateToString<Action>(AstFunc));

            var realOutput = TestUtils.CaptureOutput(() =>
            {
                _generatorIl.GenerateDelegate<Action>("Test", astFunc)();
            });

            var expectedOutput = string.Join("\r\n", new[]
            {
                "Set: 777, 11",
                "Set: 777, 12",
                "Get: 777",
                "Set: 777, 1002",
            });

            Assert.Equal(expectedOutput.Trim(), realOutput.Trim());
        }

        [Fact]
        public void TestSimpleReturn()
        {
            var func = _generatorIl.GenerateDelegate<Func<int>>("Test", Ast.Return(777));

            Assert.Equal(777, func());
        }

        [Fact]
        public void TestSimpleCall()
        {
            var func = _generatorIl.GenerateDelegate<Func<int>>("Test",
                Ast.Return(Ast.CallStatic((Func<int, int>) GetTestValue, 10)));

            Assert.Equal(3330, func());
        }

        [Fact]
        public void TestSimpleLocal()
        {
            var testLocal = AstLocal.Create<int>("TestLocal");
            var func = _generatorIl.GenerateDelegate<Func<int>>("Test", new AstNodeStmContainer(
                new AstNodeStmAssign(
                    new AstNodeExprLocal(testLocal),
                    new AstNodeExprImm(123)
                ),
                new AstNodeStmReturn(
                    new AstNodeExprLocal(testLocal)
                )
            ));

            Assert.Equal(123, func());
        }

        [Fact]
        public void TestImmediateType()
        {
            var func = _generatorIl.GenerateDelegate<Func<Type>>("Test",
                Ast.Statements(Ast.Return(Ast.Immediate(typeof(int)))));
            Assert.Equal(typeof(int).ToString(), func().ToString());
        }

        [Fact]
        public void TestReinterpret()
        {
            var testArgument = Ast.Argument<float>(0, "Input");
            var ast = Ast.Statements(
                Ast.Return(
                    Ast.Reinterpret<int>(testArgument)
                )
            );

            var func = _generatorIl.GenerateDelegate<Func<float, int>>("Test", ast);
            int b = 1234567;
            float a = 0;
            *(int*) &a = b;
            Assert.Equal(b, func(a));
        }

        public static Type TestReturnType()
        {
            return typeof(int);
        }

        [Fact]
        public void TestFieldAccess()
        {
            var testArgument = Ast.Argument<TestClass>(0, "Test");
            var func = _generatorIl.GenerateDelegate<Func<TestClass, int>>("Test", Ast.Statements(
                Ast.Assign(
                    Ast.FieldAccess(testArgument, "Test"),
                    Ast.Immediate(456)
                ),
                Ast.Return(
                    Ast.FieldAccess(testArgument, "Test")
                )
            ));

            Assert.Equal(456, func(new TestClass()));
        }

        delegate void ActionPointerDelegate(void* pointer);

        [Fact]
        public void TestPointerWrite()
        {
            var func = _generatorIl.GenerateDelegate<ActionPointerDelegate>("Test", Ast.Statements(
                Ast.Assign(
                    Ast.Indirect(Ast.Cast(typeof(int*), Ast.Argument(typeof(int*), 0, "Ptr"))),
                    Ast.Immediate(456)
                ),
                Ast.Return()
            ));

            var data = new int[1];
            fixed (int* dataPtr = data)
            {
                func(dataPtr);
            }

            Assert.Equal(456, data[0]);
        }

        [Fact]
        public void TestPointerWrite_bool()
        {
            var func = _generatorIl.GenerateDelegate<ActionPointerDelegate>("Test", Ast.Statements(
                Ast.Assign(
                    Ast.Indirect(Ast.Cast(typeof(bool*), Ast.Argument(typeof(bool*), 0, "Ptr"))),
                    Ast.Immediate(true)
                ),
                Ast.Return()
            ));

            foreach (var fillValue in new[] {false, true})
            {
                var data = new bool[8];
                for (int n = 0; n < data.Length; n++) data[n] = fillValue;
                data[0] = false;

                fixed (bool* dataPtr = data)
                {
                    func(dataPtr);
                }

                Assert.Equal(true, data[0]);
                for (int n = 1; n < 8; n++) Assert.Equal(fillValue, data[n]);
            }
        }

        [Fact]
        public void TestWriteLineLoadString()
        {
            var ast = Ast.Statements(
                Ast.Statement(Ast.CallStatic((Action<string>) Console.WriteLine, Ast.Argument<string>(0))),
                Ast.Statement(Ast.CallStatic((Action<string>) Console.WriteLine, "Goodbye World!")),
                Ast.Return()
            );

            var generatorIl = new GeneratorIl();
            var method = generatorIl.GenerateDelegate<Action<string>>("TestWriteLine", ast);

            Console.WriteLine(new GeneratorCSharp().GenerateRoot(ast).ToString());
            Console.WriteLine("{0}", new GeneratorIl().GenerateToString(method.Method, ast));

            var output = AstStringUtils.CaptureOutput(() => { method("Hello World!"); });

            Assert.Equal("Hello World!" + Environment.NewLine + "Goodbye World!" + Environment.NewLine, output);
        }

        [Fact]
        public void TestAstSwitch()
        {
            var argument = AstArgument.Create<int>(0, "Value");
            var ast = Ast.Statements(
                Ast.Switch(
                    Ast.Argument(argument),
                    Ast.Default(Ast.Return("-")),
                    Ast.Case(1, Ast.Return("One")),
                    Ast.Case(3, Ast.Return("Three"))
                ),
                Ast.Return("Invalid!")
            );
            var generatorIl = new GeneratorIl();
            var method = generatorIl.GenerateDelegate<Func<int, string>>("TestSwitch", ast);
            Assert.Equal("-", method(0));
            Assert.Equal("One", method(1));
            Assert.Equal("-", method(2));
            Assert.Equal("Three", method(3));
            Assert.Equal("-", method(4));
        }

        public class TestClass
        {
            public int Test;
        }

        public static int GetTestValue(int value)
        {
            return 333 * value;
        }
    }
}