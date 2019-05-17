using BenchmarkDotNet.Attributes;
using CSPspEmu.Rasterizer;

namespace CSPspEmu.Benchmark
{
    [MemoryDiagnoser]
    public class RasterizerBenchmark
    {
        RasterizeDelegate<int> mydelegate = (a, b, c, d) =>
        {
        };
    
        [Benchmark]
        public void Rasterize() => Rasterizer.Rasterizer.RasterizeTriangle(new RasterizerPoint(100, 0), new RasterizerPoint(50, 100), new RasterizerPoint(200, 80), 0, mydelegate, 0, 272);
    }
}
