using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using CSharpUtils.Extensions;
using CSPspEmu.Utils;

namespace CSPspEmu.Rasterizer
{
    public delegate void RasterizeDelegate<T>(int y, ref RasterizerResult left, ref RasterizerResult right,
        ref T context);

    public struct TriangleRasterizer<T>
    {
        public RasterizeDelegate<T> rowHandler;
        public int ymin;
        public int ymax;
        public int xmin;
        public int xmax;

        public TriangleRasterizer(RasterizeDelegate<T> rowHandler, int ymin = int.MinValue, int ymax = int.MaxValue, int xmin = int.MinValue, int xmax = int.MaxValue)
        {
            this.rowHandler = rowHandler;
            this.ymin = ymin;
            this.ymax = ymax;
            this.xmin = xmin;
            this.xmax = xmax;
        }

        public void RasterizeTriangle(
            RasterizerPoint p0, RasterizerPoint p1, RasterizerPoint p2, T param
        )
        {
            // ReSharper disable InvocationIsSkipped
            Debug.Assert(p0.Y <= p1.Y);
            Debug.Assert(p1.Y <= p2.Y);

            if (p0.Y > ymax) return;

            var y0 = p0.Y.Clamp(ymin, ymax);
            var y1 = p1.Y;
            var y2 = p2.Y.Clamp(ymin, ymax);
            
            RasterizerResult r0, r1;
            
            for (var y = y0; y <= y2; y++)
            {
                InterpolateX(y, p0, p2, 1, out r0);
                if (y <= y1) InterpolateX(y, p0, p1, 2, out r1); else InterpolateX(y, p1, p2, 0, out r1);

                AdjustX(ref r0, ref r1);
                rowHandler(y, ref r0, ref r1, ref param);
            }
        }

        public void RasterizeLine(
            RasterizerPoint p0, RasterizerPoint p1, T param
        )
        {
            // ReSharper disable InvocationIsSkipped
            Debug.Assert(p0.Y <= p1.Y);

            var y0 = p0.Y.Clamp(ymin, ymax);
            var y1 = p1.Y.Clamp(ymin, ymax);

            for (var y = y0; y < y1; y++)
            {
                var ratio0 = y.RatioInRange(y0, y1);
                var ratio1 = (y + 1).RatioInRange(y0, y1);
                var vx0 = ratio0.Interpolate(p0.X, p1.X);
                var vx1 = ratio1.Interpolate(p0.X, p1.X) + 1;
                var r0 = new RasterizerResult(vx0, new Vector3(ratio0));
                var r1 = new RasterizerResult(vx1, new Vector3(ratio1));
                AdjustX(ref r0, ref r1);
                rowHandler(y, ref r0, ref r1, ref param);
            }
        }

        
        private static void InterpolateX(
            int y,
            RasterizerPoint a, RasterizerPoint b,
            int notUsedIndex,
            out RasterizerResult result
        )
        {
            int dY = b.Y - a.Y;
            var ratio = (y - a.Y) / (float) (dY != 0 ? dY : 1);
            var iratio = 1 - ratio;
            var x = ratio.Interpolate(a.X, b.X);
            switch (notUsedIndex)
            {
                case 0:
                    result = new RasterizerResult(x, new Vector3(0, iratio, ratio));
                    return;
                case 1:
                    result = new RasterizerResult(x, new Vector3(iratio, 0, ratio));
                    return;
                case 2:
                    result = new RasterizerResult(x, new Vector3(iratio, ratio, 0));
                    return;
                default:
                    throw new Exception();
            }
        }
        
        private void AdjustX(ref RasterizerResult r0, ref RasterizerResult r1)
        {
            if (r0.X > r1.X) Swap(ref r0, ref r1);

            var rx0 = r0.X;
            var rx1 = r1.X;

            if (rx0 < xmin || rx0 > xmax || rx1 < xmin || rx1 > xmax)
            {
                var x0 = rx0.Clamp(xmin, xmax);
                var x1 = rx1.Clamp(xmin, xmax);

                r0 = new RasterizerResult(x0, Vector3.Lerp(r0.Ratios, r1.Ratios, x0.RatioInRange(rx0, rx1)));
                r1 = new RasterizerResult(x1, Vector3.Lerp(r0.Ratios, r1.Ratios, x1.RatioInRange(rx0, rx1)));
            }
            
        }

        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }

    public class Rasterizer
    {
        public static void RasterizeTriangle<T>(
            RasterizerPoint p0, RasterizerPoint p1, RasterizerPoint p2,
            T param, RasterizeDelegate<T> rowHandler,
            int ymin = int.MinValue, int ymax = int.MaxValue, int xmin = int.MinValue, int xmax = int.MaxValue
        )
        {
            new TriangleRasterizer<T>(rowHandler, ymin, ymax, xmin, xmax).RasterizeTriangle(p0, p1, p2, param);
        }
    }

    // Using ref
    //|    Method |     Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
    //|---------- |---------:|----------:|----------:|------:|------:|------:|----------:|
    //| Rasterize | 3.538 us | 0.0435 us | 0.0386 us |     - |     - |     - |         - |

    //public ref struct Result
    public struct RasterizerResult
    {
        public readonly int X;
        public readonly Vector3 Ratios;

        public RasterizerResult(int x, Vector3 ratios)
        {
            X = x;
            Ratios = ratios;
        }

        public override string ToString() => this.ToStringDefault();
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

        public override string ToString() => this.ToStringDefault();
    }
}