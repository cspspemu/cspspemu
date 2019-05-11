using CSharpUtils.Ext.Streams;
using CSharpUtils.Extensions;
using CSharpUtils.Streams;
using Xunit;


namespace CSharpUtilsTests.Streams
{
    
    public class ProxyStreamReadWriteAnalyzerTest
    {
        [Fact]
        public void TestReadAnalyzing()
        {
            var ZeroStream = new ZeroStream(0x1000000);
            var StreamAnalyzer = new ProxyStreamReadWriteAnalyzer(ZeroStream);
            StreamAnalyzer.Position = 100;
            StreamAnalyzer.ReadBytes(8);
            StreamAnalyzer.Position = 104;
            StreamAnalyzer.ReadBytes(8);

            StreamAnalyzer.Position = 200;
            StreamAnalyzer.ReadBytes(16);

            var Usage = StreamAnalyzer.ReadUsage;

            Assert.Equal(
                "Space(Min=100, Max=112),Space(Min=200, Max=216)",
                Usage.ToStringArray()
            );
        }
    }
}