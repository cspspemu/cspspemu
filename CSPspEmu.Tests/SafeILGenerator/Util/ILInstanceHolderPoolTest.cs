using System;
using SafeILGenerator.Utils;
using SafeILGenerator.Ast.Generators;
using SafeILGenerator.Ast;
using Xunit;
using Xunit.Abstractions;


namespace SafeILGenerator.Tests.Util
{
    
    public class IlInstanceHolderPoolTest
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private static AstGenerator ast = AstGenerator.Instance;

        public IlInstanceHolderPoolTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void TestAllocAssignGetAndRelease()
        {
            var pool = new IlInstanceHolderPool(typeof(int), 16);
            var item = pool.Alloc();
            item.Value = 10;
            Assert.Equal(10, item.Value);
            Assert.Equal(15, pool.FreeCount);
            item.Free();
            Assert.Equal(16, pool.FreeCount);
        }

        [Fact]
        public void TestMethod2()
        {
            var pool = new IlInstanceHolderPool(typeof(int), 16);
            var item = pool.Alloc();
            var astNode = ast.Statements(
                ast.Assign(ast.StaticFieldAccess(item.FieldInfo), ast.Argument<int>(0, "Value")),
                ast.Return()
            );
            _testOutputHelper.WriteLine(GeneratorCSharp.GenerateString<GeneratorCSharp>(astNode));
            var generatorIl = new GeneratorIl();
            var itemSet = generatorIl.GenerateDelegate<Action<int>>("ItemSet", astNode);
            itemSet(10);
            Assert.Equal(10, item.Value);
        }

        [Fact]
        public void TestMethod3()
        {
            var pool1 = new IlInstanceHolderPool(typeof(int), 16);
            var pool2 = new IlInstanceHolderPool(typeof(int), 16);
            var item1 = pool1.Alloc();
            var item2 = pool2.Alloc();
            item1.Value = 11;
            item2.Value = 22;
            Assert.Equal(11, item1.Value);
            Assert.Equal(22, item2.Value);
        }

        [Fact(Skip = "Not working")]
        public void TestGlobalAlloc()
        {
            Assert.Equal(0, IlInstanceHolder.CapacityCount);
            Assert.Equal(0, IlInstanceHolder.FreeCount);

            var globalKey = IlInstanceHolder.TaAlloc<int>();

            Assert.Equal(4, IlInstanceHolder.CapacityCount);
            Assert.Equal(3, IlInstanceHolder.FreeCount);

            globalKey.Value = 10;
            globalKey.Free();

            Assert.Equal(4, IlInstanceHolder.CapacityCount);
            Assert.Equal(4, IlInstanceHolder.FreeCount);
        }
    }
}