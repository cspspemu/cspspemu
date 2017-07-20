using System;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast.Nodes;
using SafeILGenerator.Ast;
using SafeILGenerator.Ast.Utils;
using NUnit.Framework;

namespace SafeILGenerator.Tests.Ast.Generators
{
    [TestFixture]
    public unsafe class GeneratorILTest
    {
        private static AstGenerator ast = AstGenerator.Instance;
        private GeneratorIL GeneratorIL = new GeneratorIL();

        public static void TestAstSetGetLValue_Set(int Index, int Value)
        {
            Console.WriteLine("Set: {0}, {1}", Index, Value);
        }

        public static int TestAstSetGetLValue_Get(int Index)
        {
            Console.WriteLine("Get: {0}", Index);
            return 999;
        }

        [Test]
        public void TestAstSetGetLValue()
        {
            var AstIndex = ast.Immediate(777);
            var AstSetGet = ast.SetGetLValue(
                ast.CallStatic((Action<int, int>) TestAstSetGetLValue_Set, AstIndex,
                    ast.SetGetLValuePlaceholder<int>()),
                ast.CallStatic((Func<int, int>) TestAstSetGetLValue_Get, AstIndex)
            );
            var AstFunc = ast.Statements(
                ast.Assign(AstSetGet, 11),
                ast.Assign(AstSetGet, 12),
                ast.Assign(AstSetGet, AstSetGet + 3),
                ast.Return()
            );

            //Console.WriteLine(GeneratorIL.GenerateToString<Action>(AstFunc));

            var RealOutput = TestUtils.CaptureOutput(() =>
            {
                GeneratorIL.GenerateDelegate<Action>("Test", AstFunc)();
            });

            var ExpectedOutput = string.Join("\r\n", new[]
            {
                "Set: 777, 11",
                "Set: 777, 12",
                "Get: 777",
                "Set: 777, 1002",
            });

            Assert.AreEqual(ExpectedOutput.Trim(), RealOutput.Trim());
        }

        [Test]
        public void TestSimpleReturn()
        {
            var Func = GeneratorIL.GenerateDelegate<Func<int>>("Test", ast.Return(777));

            Assert.AreEqual(777, Func());
        }

        [Test]
        public void TestSimpleCall()
        {
            var Func = GeneratorIL.GenerateDelegate<Func<int>>("Test",
                ast.Return(ast.CallStatic((Func<int, int>) GetTestValue, 10)));

            Assert.AreEqual(3330, Func());
        }

        [Test]
        public void TestSimpleLocal()
        {
            var TestLocal = AstLocal.Create<int>("TestLocal");
            var Func = GeneratorIL.GenerateDelegate<Func<int>>("Test", new AstNodeStmContainer(
                new AstNodeStmAssign(
                    new AstNodeExprLocal(TestLocal),
                    new AstNodeExprImm(123)
                ),
                new AstNodeStmReturn(
                    new AstNodeExprLocal(TestLocal)
                )
            ));

            Assert.AreEqual(123, Func());
        }

        [Test]
        public void TestImmediateType()
        {
            var Func = GeneratorIL.GenerateDelegate<Func<Type>>("Test",
                ast.Statements(ast.Return(ast.Immediate(typeof(int)))));
            Assert.AreEqual(typeof(int).ToString(), Func().ToString());
        }

        [Test]
        public void TestReinterpret()
        {
            var TestArgument = ast.Argument<float>(0, "Input");
            var Ast = ast.Statements(
                ast.Return(
                    ast.Reinterpret<int>(TestArgument)
                )
            );

            var Func = GeneratorIL.GenerateDelegate<Func<float, int>>("Test", Ast);
            int b = 1234567;
            float a = 0;
            *(int*) &a = b;
            Assert.AreEqual(b, Func(a));
        }

        public static Type testReturnType()
        {
            return typeof(int);
        }

        [Test]
        public void TestFieldAccess()
        {
            var TestArgument = ast.Argument<TestClass>(0, "Test");
            var Func = GeneratorIL.GenerateDelegate<Func<TestClass, int>>("Test", ast.Statements(
                ast.Assign(
                    ast.FieldAccess(TestArgument, "Test"),
                    ast.Immediate(456)
                ),
                ast.Return(
                    ast.FieldAccess(TestArgument, "Test")
                )
            ));

            Assert.AreEqual(456, Func(new TestClass()));
        }

        delegate void ActionPointerDelegate(void* Pointer);

        [Test]
        public void TestPointerWrite()
        {
            var Func = GeneratorIL.GenerateDelegate<ActionPointerDelegate>("Test", ast.Statements(
                ast.Assign(
                    ast.Indirect(ast.Cast(typeof(int*), ast.Argument(typeof(int*), 0, "Ptr"))),
                    ast.Immediate(456)
                ),
                ast.Return()
            ));

            var Data = new int[1];
            fixed (int* DataPtr = Data)
            {
                Func(DataPtr);
            }

            Assert.AreEqual(456, Data[0]);
        }

        [Test]
        public void TestPointerWrite_bool()
        {
            var Func = GeneratorIL.GenerateDelegate<ActionPointerDelegate>("Test", ast.Statements(
                ast.Assign(
                    ast.Indirect(ast.Cast(typeof(bool*), ast.Argument(typeof(bool*), 0, "Ptr"))),
                    ast.Immediate(true)
                ),
                ast.Return()
            ));

            foreach (var FillValue in new bool[] {false, true})
            {
                var Data = new bool[8];
                for (int n = 0; n < Data.Length; n++) Data[n] = FillValue;
                Data[0] = false;

                fixed (bool* DataPtr = Data)
                {
                    Func(DataPtr);
                }

                Assert.AreEqual(true, Data[0]);
                for (int n = 1; n < 8; n++) Assert.AreEqual(FillValue, Data[n]);
            }
        }

        [Test]
        public void TestWriteLineLoadString()
        {
            var Ast = ast.Statements(
                ast.Statement(ast.CallStatic((Action<string>) Console.WriteLine, ast.Argument<string>(0))),
                ast.Statement(ast.CallStatic((Action<string>) Console.WriteLine, "Goodbye World!")),
                ast.Return()
            );

            var GeneratorIL = new GeneratorIL();
            var Method = GeneratorIL.GenerateDelegate<Action<string>>("TestWriteLine", Ast);

            Console.WriteLine(new GeneratorCSharp().GenerateRoot(Ast).ToString());
            Console.WriteLine("{0}", new GeneratorIL().GenerateToString(Method.Method, Ast));

            var Output = AstStringUtils.CaptureOutput(() => { Method("Hello World!"); });

            Assert.AreEqual("Hello World!" + Environment.NewLine + "Goodbye World!" + Environment.NewLine, Output);
        }

        [Test]
        public void TestAstSwitch()
        {
            var Argument = AstArgument.Create<int>(0, "Value");
            var Ast = ast.Statements(
                ast.Switch(
                    ast.Argument(Argument),
                    ast.Default(ast.Return("-")),
                    ast.Case(1, ast.Return("One")),
                    ast.Case(3, ast.Return("Three"))
                ),
                ast.Return("Invalid!")
            );
            var GeneratorIL = new GeneratorIL();
            var Method = GeneratorIL.GenerateDelegate<Func<int, string>>("TestSwitch", Ast);
            Assert.AreEqual("-", Method(0));
            Assert.AreEqual("One", Method(1));
            Assert.AreEqual("-", Method(2));
            Assert.AreEqual("Three", Method(3));
            Assert.AreEqual("-", Method(4));
        }

        public class TestClass
        {
            public int Test;
        }

        public static int GetTestValue(int Value)
        {
            return 333 * Value;
        }
    }
}