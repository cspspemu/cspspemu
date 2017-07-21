using System;
using SafeILGenerator.Ast.Serializers;
using SafeILGenerator.Ast;
using Xunit;


namespace SafeILGenerator.Tests.Ast.Serializers
{
    
    public class UnitTest1
    {
        private static AstGenerator ast = AstGenerator.Instance;

        [Fact]
        public void TestMethod1()
        {
            var Ast = ast.Statements(
                ast.Assign(ast.Argument<int>(0, "Argument0"), 777),
                ast.Return()
            );
            Console.WriteLine(AstSerializer.SerializeAsXml(Ast));
        }
    }
}