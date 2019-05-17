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
                var P0 = triangle.P0;
                var P1 = triangle.P1;
                var P2 = triangle.P2;
                var C0 = P0.Color;
                var C1 = P1.Color;
                var C2 = P2.Color;
                //Console.WriteLine(triangle.P0.T);
                if (VertexType.HasTexture)
                {
                    
                    var texA = LerpRatios(P0.T, P1.T, P2.T, a.Ratios);
                    var texB = LerpRatios(P0.T, P1.T, P2.T, b.Ratios);
                    //Console.WriteLine($"{P0.T} {P1.T} {P2.T} {a.Ratios} {b.Ratios} {texA} {texB}");
                    DrawRowFastTextureLookup(y, a.X, b.X, texA, texB);
                }
                else if (C0 == C1 && C0 == C2)
                {
                    DrawRowFastSingleColor(y, a.X, b.X, triangle.P0.RgbaColor);
                }
                else
                {
                    var colorA = LerpRatios(C0, C1, C2, a.Ratios).Clamp(0, 1);
                    var colorB = LerpRatios(C0, C1, C2, b.Ratios).Clamp(0, 1);
                    //Console.WriteLine($"{a.Ratio0}, {a.Ratio1}, {a.Ratio2}");
                    DrawRowFastInterpolateTwoColors(y, a.X, b.X, colorA, colorB);
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

        private uint* ScreenRow(int y)
        {
            return (uint*) Memory.PspAddressToPointerSafe((uint) (DrawAddress + (y * 512) * 4));
        }

        private void DrawRowFastSingleColor(int y, int x0, int x1, Rgba color)
        {
            var ptr = (Rgba*) ScreenRow(y);
            for (var n = x0; n < x1; n++) ptr[n] = color;
        }

        private void DrawRowFastInterpolateTwoColors(int y, int x0, int x1, Vector4 c1, Vector4 c2)
        {
            var ptr = (uint*) ScreenRow(y);
            for (var n = x0; n < x1; n++) ptr[n] = new RgbaFloat(Vector4.Lerp(c1, c2, n.RatioInRange(x0, x1))).Int;
        }
        
        private void DrawRowFastTextureLookup(int y, int x0, int x1, Vector4 uv1, Vector4 uv2)
        {
            var ptr = (uint*) ScreenRow(y);
            for (var n = x0; n < x1; n++) ptr[n] = TextureLookup(Vector4.Lerp(uv1, uv2, n.RatioInRange(x0, x1)));
        }

        public uint TextureLookup(Vector4 pos) => TextureLookup((int) pos.X, (int) pos.Y); 

        public uint TextureLookup(int x, int y) => *TextureLookupAddress(x, y);

        public uint* TextureLookupAddress(Vector4 pos) => TextureLookupAddress((int) pos.X, (int) pos.Y);
        
        public uint* TextureLookupAddress(int x, int y)
        {
            var textureMappingStateStruct = GpuState->TextureMappingState;
            var textureStateStruct = textureMappingStateStruct.TextureState;
            var mipmap = textureStateStruct.Mipmap0;
            return (uint*) Memory.PspAddressToPointerSafe((uint)(mipmap.Address + (((y * mipmap.BufferWidth) + x) * 4)));
        }

    }
}