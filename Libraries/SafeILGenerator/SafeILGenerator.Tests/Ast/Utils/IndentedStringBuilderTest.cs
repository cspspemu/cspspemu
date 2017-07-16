using System;
using SafeILGenerator.Ast.Utils;
using NUnit.Framework;

namespace SafeILGenerator.Tests.Ast.Utils
{
    [TestFixture]
    public class IndentedStringBuilderTest
    {
        [Test]
        public void TestIndentation()
        {
            var Output = new IndentedStringBuilder();
            Output.Write("{\n");
            Output.Indent(() =>
            {
                Output.Write("Hello World!\n");
                Output.Write("Goodbye World!\n");
            });
            Output.Write("}\n");
            Assert.AreEqual(
                @"{\n" +
                @"    Hello World!\n" +
                @"    Goodbye World!\n" +
                @"}\n",
                AstStringUtils.ToLiteralRaw(Output.ToString())
            );
        }
    }
}