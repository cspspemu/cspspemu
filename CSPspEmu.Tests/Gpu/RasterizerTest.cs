using System;
using System.Collections.Generic;
using System.Numerics;
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

        public struct RasterInfo
        {
            public int Y;
            public RasterizerResult Left;
            public RasterizerResult Right;
            public int Context;
        }

        public List<RasterInfo> RasterizeTriangle(RasterizerPoint p0, RasterizerPoint p1, RasterizerPoint p2)
        {
            var list = new List<RasterInfo>();

            Rasterizer.Rasterizer.RasterizeTriangle(p0, p1, p2, 7,
                (int y, ref RasterizerResult left, ref RasterizerResult right, ref int context) =>
                {
                    list.Add(new RasterInfo {Y = y, Left = left, Right = right, Context = context});
                });

            return list;
        }

        [Fact]
        public void Test()
        {
            var list = RasterizeTriangle(
                new RasterizerPoint(50, 0),
                new RasterizerPoint(0, 100),
                new RasterizerPoint(100, 100)
            );
            Assert.Equal(101, list.Count);
            Assert.Equal(0, list[0].Y);
            Assert.Equal(100, list[100].Y);

            Assert.Equal(7, list[0].Context);
            Assert.Equal(7, list[100].Context);

            Assert.Equal(new Vector3(1, 0, 0), list[0].Left.Ratios);
            Assert.Equal(new Vector3(1, 0, 0), list[0].Right.Ratios);

            Assert.Equal(new Vector3(0, 1, 0), list[100].Left.Ratios);
            Assert.Equal(new Vector3(0, 0, 1), list[100].Right.Ratios);
        }
    }
}