using SafeILGenerator.Ast.Utils;
using Xunit;


namespace SafeILGenerator.Tests.Ast.Utils
{
    
    public class IndentedStringBuilderTest
    {
        [Fact]
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
            Assert.Equal(
                @"{\n" +
                @"    Hello World!\n" +
                @"    Goodbye World!\n" +
                @"}\n",
                AstStringUtils.ToLiteralRaw(output.ToString())
            );
        }
    }
}