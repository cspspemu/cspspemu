using System;
using CSPspEmu.Rasterizer;
using Xunit;
using Xunit.Abstractions;

namespace CSPspEmu.Tests.Gpu
{
    public class RasterizerTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RasterizerTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void Test()
        {
            Rasterizer.Rasterizer.RasterizeTriangle(new RasterizerPoint(50, 0), new RasterizerPoint(0, 100), new RasterizerPoint(100, 100), 0,
                (int y, ref RasterizerResult left, ref RasterizerResult right, ref int context) =>
                {
                    _testOutputHelper.WriteLine($"{y} {left} {right} {context}");
                });
        }
    }
}