using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CSharpUtils.Extensions;
using CSPspEmu.Utils;

namespace CSPspEmu.Rasterizer
{
    public delegate void RasterizeDelegate<T>(int y, ref RasterizerResult left, ref RasterizerResult right, ref T context);
        
    public class Rasterizer
    {
        public static void RasterizeTriangle<T>(RasterizerPoint p0, RasterizerPoint p1, RasterizerPoint p2, T param,
            RasterizeDelegate<T> rowHandler, int clampY0 = int.MinValue, int clampY1 = int.MaxValue)
        {
            // ReSharper disable InvocationIsSkipped
            Debug.Assert(p0.Y <= p1.Y);
            Debug.Assert(p1.Y <= p2.Y);

            var y0 = p0.Y.Clamp(clampY0, clampY1);
            var y1 = p1.Y;
            var y2 = p2.Y.Clamp(clampY0, clampY1);

            RasterizerResult xa, xb;
            
            for (var y = y0; y <= y1; y++)
            {
                InterpolateX(y, p0, p2, 1, out xa);
                InterpolateX(y, p0, p1, 2, out xb);
                if (xa.X > xb.X) Swap(ref xa, ref xb);
                rowHandler(y, ref xa, ref xb, ref param);
            }
            
            for (var y = y1 + 1; y <= y2; y++)
            {
                InterpolateX(y, p0, p2, 1, out xa);
                InterpolateX(y, p1, p2, 0, out xb);
                if (xa.X > xb.X) Swap(ref xa, ref xb);
                rowHandler(y, ref xa, ref xb, ref param);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InterpolateX(int y, RasterizerPoint a, RasterizerPoint b, int notUsedIndex,
            out RasterizerResult result)
        {
            var ratio = (y - a.Y) / (float) (b.Y - a.Y);
            var iratio = 1 - ratio;
            var res = ratio.Interpolate(a.X, b.X);
            switch (notUsedIndex)
            {
                case 0:
                    result = new RasterizerResult(res, 0, iratio, ratio);
                    return;
                case 1:
                    result = new RasterizerResult(res, iratio, 0, ratio);
                    return;
                case 2:
                    result = new RasterizerResult(res, iratio, ratio, 0);
                    return;
                default:
                    throw new Exception();
            }
        }
        
        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
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
        public readonly float Ratio0;
        public readonly float Ratio1;
        public readonly float Ratio2;

        public RasterizerResult(int x, float ratio0, float ratio1, float ratio2)
        {
            X = x;
            Ratio0 = ratio0;
            Ratio1 = ratio1;
            Ratio2 = ratio2;
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