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
            var output = new IndentedStringBuilder();
            output.Write("{");
            output.WriteNewLine();
            output.Indent(() =>
            {
                output.Write("Hello World!");
                output.WriteNewLine();
                output.Write("Goodbye World!");
                output.WriteNewLine();
            });
            output.Write("}");
            output.WriteNewLine();
            Assert.AreEqual(
                @"{\n" +
                @"    Hello World!\n" +
                @"    Goodbye World!\n" +
                @"}\n",
                AstStringUtils.ToLiteralRaw(output.ToString())
            );
        }
    }
}