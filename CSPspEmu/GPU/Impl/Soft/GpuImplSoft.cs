using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using CSharpUtils.Drawing;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;
using CSPspEmu.Rasterizer;
using CSPspEmu.Utils;

namespace CSPspEmu.Core.Gpu.Impl.Soft
{
    unsafe public class GpuImplSoft : GpuImpl
    {
        public override PluginInfo PluginInfo => new PluginInfo
        {
            Name = "Soft",
            Version = "1.0",
        };

        public override bool IsWorking => true;

        private TriangleRasterizer<Triangle> TriangleRasterizer;

        public GpuImplSoft()
        {
            TriangleRasterizer = new TriangleRasterizer<Triangle>((int y, ref RasterizerResult a, ref RasterizerResult b, ref Triangle triangle) =>
            {
                //Console.WriteLine($"{a.Ratio} {b.Ratio} {triangle.P0.Color} : {triangle.P1.IColor}");
                var C0 = triangle.P0.Color;
                var C1 = triangle.P1.Color;
                var C2 = triangle.P2.Color;
                if (C0 == C1 && C0 == C2)
                {
                    DrawPixelsFast(y, a.X, b.X, triangle.P0.IColor);
                }
                else
                {
                    var colorA = LerpRatios(C0, C1, C2, a.Ratios.X, a.Ratios.Y, a.Ratios.Z);
                    var colorB = LerpRatios(C0, C1, C2, b.Ratios.X, b.Ratios.Y, b.Ratios.Z);
                    //Console.WriteLine($"{a.Ratio0}, {a.Ratio1}, {a.Ratio2}");
                    DrawPixelsFast(y, a.X, b.X, colorA, colorB);
                }
            }, 0, 271, 0, 511);
        }

        protected override void DrawTriangle(VPoint a, VPoint b, VPoint c)
        {
            var triangle = new Triangle(a, b, c);
            TriangleRasterizer.RasterizeTriangle(triangle.P0.P, triangle.P1.P, triangle.P2.P, triangle);
        }

        protected override void DrawLine(VPoint a, VPoint b)
        {
            var triangle = new Triangle(a, b, b);
            //Console.WriteLine($"DrawLine({a}, {b})");
            TriangleRasterizer.RasterizeLine(a.P, b.P, triangle);
        }

        //protected override void DrawSprite(VPoint a, VPoint b)
        //{
        //    var minX = Math.Min(a.P.X, b.P.X).Clamp(0, 511);
        //    var maxX = Math.Max(a.P.X, b.P.X).Clamp(0, 511);
        //    var minY = Math.Min(a.P.Y, b.P.Y).Clamp(0, 271);
        //    var maxY = Math.Max(a.P.Y, b.P.Y).Clamp(0, 271);
        //    //Console.WriteLine($"Sprite {a}, {b}");
        //    for (var y = minY; y <= maxY; y++) DrawPixelsFast(y, minX, maxX, a.IColor);
        //}

        private void DrawPixelsFast(int y, int x0, int x1, uint color)
        {
            var ptr = (uint*) Memory.PspAddressToPointerSafe((uint) (DrawAddress + (y * 512) * 4));
            for (var n = x0; n < x1; n++) ptr[n] = color;
        }

        private void DrawPixelsFast(int y, int x0, int x1, Vector4 c1, Vector4 c2)
        {
            var ptr = (uint*) Memory.PspAddressToPointerSafe((uint) (DrawAddress + (y * 512) * 4));
            for (var n = x0; n < x1; n++) ptr[n] = new RgbaFloat(Vector4.Lerp(c1, c2, n.RatioInRange(x0, x1))).Int;
        }
    }
}