using System;
using SafeILGenerator.Ast.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SafeILGenerator.Tests.Ast.Utils
{
    [TestClass]
    public class IndentedStringBuilderTest
    {
        [TestMethod]
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