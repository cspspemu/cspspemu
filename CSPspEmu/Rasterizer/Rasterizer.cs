using System.Diagnostics;
using CSPspEmu.Utils;

namespace CSPspEmu.Rasterizer
{
    public class Rasterizer
    {
        public static void RasterizeTriangle<T>(RasterizerPoint p0, RasterizerPoint p1, RasterizerPoint p2, T param,
            RasterizeDelegate<T> rowHandler, int clampY0 = int.MinValue, int clampY1 = int.MaxValue)
        {
            // ReSharper disable InvocationIsSkipped
            Debug.Assert(p0.Y <= p1.Y);
            Debug.Assert(p1.Y <= p2.Y);

            var y0 = p0.Y.Clamp(clampY0, clampY1);
            var y2 = p2.Y.Clamp(clampY0, clampY1);

            for (var y = y0; y <= y2; y++)
            {
                var xa = InterpolateX(y, p0, p2, 1);
                var xb = (y <= p1.Y) ? InterpolateX(y, p0, p1, 2) : InterpolateX(y, p1, p2, 0);
                if (xa.X < xb.X)
                {
                    rowHandler(y, xa, xb, param);
                }
                else
                {
                    rowHandler(y, xb, xa, param);
                }

                //Console.WriteLine($"{y}: {xa} - {xb}");
                //for (var x = xmin; x <= xmax; x++) DrawPixel(x, y, 0xFFFFFFFF);
            }
        }

        private static RasterizerResult InterpolateX(int y, RasterizerPoint a, RasterizerPoint b, int notUsedIndex)
        {
            var ratio = (y - a.Y) / (float) (b.Y - a.Y);
            var res = ratio.Interpolate(a.X, b.X);
            return new RasterizerResult
            {
                X = res,
                Ratio = ratio,
                NotUsedIndex = notUsedIndex,
            };
        }
    }

    public delegate void RasterizeDelegate<T>(int a, RasterizerResult b, RasterizerResult c, T d);

    // Using ref
    //|    Method |     Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|---------- |---------:|----------:|----------:|------:|------:|------:|----------:|
    //| Rasterize | 3.538 us | 0.0435 us | 0.0386 us |     - |     - |     - |         - |

    //public ref struct Result
    public struct RasterizerResult
    {
        public int X;
        public int NotUsedIndex;
        public float Ratio;
    }

    //public ref struct PointIS
    public struct RasterizerPoint
    {
        public readonly int X;
        public readonly int Y;

        public RasterizerPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}