using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
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

        public override void InitSynchronizedOnce()
        {
            base.InitSynchronizedOnce();
        }

        public override void StopSynchronized()
        {
            base.StopSynchronized();
        }

        private Matrix4x4 modelView = Matrix4x4.Identity;
        private Matrix4x4 worldViewProjection3D = Matrix4x4.Identity;
        private Matrix4x4 worldViewProjection2D = Matrix4x4.Identity;
        private Matrix4x4 transform3d = Matrix4x4.Identity;

        private Matrix4x4 unitToScreenCoords = Matrix4x4.CreateScale(1f, -1f, 1f) *
                                               Matrix4x4.CreateTranslation(+1f, +1f, 0f) *
                                               Matrix4x4.CreateScale(.5f, .5f, 1f) *
                                               Matrix4x4.CreateScale(480, 272, 1f);

        private GpuStateStruct* GpuState;
        private uint DrawAddress;
        private GuPrimitiveType PrimitiveType;

        public override void PrimStart(GlobalGpuState globalGpuState, GpuStateStruct* gpuState,
            GuPrimitiveType primitiveType)
        {
            GpuState = gpuState;
            VertexType = gpuState->VertexState.Type;
            PrimitiveType = primitiveType;
            var model = gpuState->VertexState.WorldMatrix.Matrix4x4;
            var view = gpuState->VertexState.ViewMatrix.Matrix4x4;
            var projection3D = gpuState->VertexState.ProjectionMatrix.Matrix4x4;
            var projection2D = Matrix4x4.CreateOrthographic(512, 272, -1f, +1f);
            //Matrix4x4.Invert(projection2D, out unitToScreenCoords);
            modelView = model * view;
            worldViewProjection3D = modelView * projection3D;
            worldViewProjection2D = modelView * projection2D;
            DrawAddress = GpuState->DrawBufferState.Address;
            //if (primitiveType == GuPrimitiveType.Triangles)
            //{
            //    Console.WriteLine($"primitiveType {primitiveType}");
            //    Console.WriteLine($"Model {model}");
            //    Console.WriteLine($"View {view}");
            //    Console.WriteLine($"Projection3D {projection3D}");
            //    //Console.WriteLine($"Projection2D {projection2D}");
            //    Console.WriteLine($"worldView {modelView}");
            //    Console.WriteLine($"worldViewProjection3D {worldViewProjection3D}");
            //    //Console.WriteLine($"worldViewProjection2D {worldViewProjection2D}");
            //    Console.WriteLine($"PrimStart: {primitiveType}");
            //}
        }

        public override void PrimEnd()
        {
            //Console.WriteLine($"PrimEnd");
        }

        public override void Prim(ushort vertexCount)
        {
            uint morpingVertexCount, totalVerticesWithoutMorphing;
            PreparePrim(GpuState, out totalVerticesWithoutMorphing, vertexCount, out morpingVertexCount);
            var vertices = new Vector4[vertexCount];
            var verticesP = new VPoint[vertexCount];
            var vertexTransform = this.worldViewProjection3D;
            {
                var transform = VertexType.Transform2D ? Matrix4x4.Identity : worldViewProjection3D;
                var rtransform = VertexType.Transform2D ? transform : transform * unitToScreenCoords;

                for (var n = 0; n < vertexCount; n++)
                {
                    var vinfo = VertexReader.ReadVertex(n);
                    var vector = vinfo.Position.ToVector4();
                    var tvertex = VertexType.Transform2D ? vector : Vector4.Transform(vector, rtransform);

                    //Console.WriteLine($"VertexType.Transform2D: {VertexType.Transform2D} : {PrimitiveType} : {vinfo}");
                    vertices[n] = tvertex;
                    verticesP[n] = Vector4ToPoint(tvertex, vinfo.Normal.ToVector4(), vinfo.Color.ToVector4());
                }
            }
            {
                switch (PrimitiveType)
                {
                    case GuPrimitiveType.Triangles:
                    {
                        //{ for (var n = 0; n < vertexCount; n++) Console.WriteLine($"Triangle {VertexType.Transform2D}:{VertexReader.ReadVertex(n)} : {vertices[n]} : {Vector4ToPoint(vertices[n])}"); }
                        var m = 0;
                        for (var n = 0; n < vertexCount; n += 3)
                        {
                            DrawTriangleFast((verticesP[n + 0]), (verticesP[n + 1]), (verticesP[n + 2]));
                        }

                        //DrawTriangleFast(new PointIS(100, 0), new PointIS(50, 100), new PointIS(150, 50), 0xFF00FFFF);

                        break;
                    }

                    case GuPrimitiveType.Sprites:
                    {
                        //{ for (var n = 0; n < vertexCount; n++) Console.WriteLine($"Sprite {VertexType.Transform2D}:" + VertexReader.ReadVertex(n) + " : " + Vector4ToPoint(vertices[n])); }
                        for (var n = 0; n < vertexCount; n += 2)
                        {
                            DrawSprite((verticesP[n + 0]), (verticesP[n + 1]));
                        }

                        break;
                    }

                    default:
                        Console.WriteLine($"Unsupported {PrimitiveType}");
                        break;
                }
            }

            //DrawPixel(52, 52, 0xFFFFFFFF);
            //Console.WriteLine($"Prim {vertexCount}");
        }

        private VPoint Vector4ToPoint(Vector4 v, Vector4 n, Vector4 color) =>
            new VPoint(new RasterizerPoint((int) v.X, (int) v.Y), n, color);

        private uint[] colors = {0xFF0077FF, 0xFF00FFFF, 0xFF0000FF};

        public struct VPoint
        {
            public RasterizerPoint P;
            public Vector4 N;
            public Vector4 Color;

            public uint IColor => new RgbaFloat(Color).Int;

            public VPoint(RasterizerPoint p, Vector4 n, Vector4 color)
            {
                P = p;
                N = n;
                Color = color;
            }
        }

        public struct Triangle
        {
            public VPoint P0;
            public VPoint P1;
            public VPoint P2;

            public Triangle(VPoint p0, VPoint p1, VPoint p2)
            {
                // Sort the points so that y0 <= y1 <= y2
                if (p1.P.Y < p0.P.Y) Swap(ref p1, ref p0);
                if (p2.P.Y < p0.P.Y) Swap(ref p2, ref p0);
                if (p2.P.Y < p1.P.Y) Swap(ref p2, ref p1);


                P0 = p0;
                P1 = p1;
                P2 = p2;
            }
        }

        private void DrawTriangleFast(VPoint p0, VPoint p1, VPoint p2)
        {
            var triangle = new Triangle(p0, p1, p2);
            Rasterizer.Rasterizer.RasterizeTriangle(triangle.P0.P, triangle.P1.P, triangle.P2.P, triangle,
                DrawTriangleFastTest, 0, 272);
        }

        static public Vector4 LerpNotUsedIndex(Vector4 a, Vector4 b, Vector4 c, int notUsedIndex, float ratio)
        {
            switch (notUsedIndex)
            {
                case 0: return Vector4.Lerp(b, c, ratio);
                case 1: return Vector4.Lerp(a, c, ratio);
                case 2: return Vector4.Lerp(a, b, ratio);
            }

            return a;
        }

        private RasterizeDelegate<Triangle> DrawTriangleFastTest;

        public GpuImplSoft()
        {
            DrawTriangleFastTest = (y, a, b, triangle) =>
            {
                //Console.WriteLine($"{a.Ratio} {b.Ratio} {triangle.P0.Color} : {triangle.P1.IColor}");
                if (triangle.P0.Color == triangle.P1.Color && triangle.P0.Color == triangle.P2.Color)
                {
                    DrawPixelsFast(y, a.X, b.X, triangle.P0.IColor);
                }
                else
                {
                    var colorA = LerpNotUsedIndex(triangle.P0.Color, triangle.P1.Color, triangle.P2.Color, a.NotUsedIndex,
                        a.Ratio);
                    var colorB = LerpNotUsedIndex(triangle.P0.Color, triangle.P1.Color, triangle.P2.Color, b.NotUsedIndex,
                        b.Ratio);
                    DrawPixelsFast(y, a.X, b.X, colorA, colorB);
                }
            };
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        private void DrawSprite(VPoint a, VPoint b)
        {
            var minX = Math.Min(a.P.X, b.P.X).Clamp(0, 512);
            var maxX = Math.Max(a.P.X, b.P.X).Clamp(0, 512);
            var minY = Math.Min(a.P.Y, b.P.Y).Clamp(0, 272);
            var maxY = Math.Max(a.P.Y, b.P.Y).Clamp(0, 272);
            //Console.WriteLine($"Sprite {a}, {b}");
            for (var y = minY; y <= maxY; y++)
            {
                DrawPixelsFast(y, minX, maxX, a.IColor);
            }
        }

        private void DrawPixelsFast(int y, int x0, int x1, uint color)
        {
            if (y < 0 || y > 272) return;
            var startX = x0.Clamp(0, 512);
            var endX = x1.Clamp(0, 512);
            var ptr = (uint*) Memory.PspAddressToPointerSafe((uint) (DrawAddress + (y * 512 + startX) * 4));
            var count = endX - startX;
            for (var n = 0; n < count; n++) ptr[n] = color;
        }

        private void DrawPixelsFast(int y, int x0, int x1, Vector4 c1, Vector4 c2)
        {
            if (y < 0 || y >= 272) return;
            var startX = x0.Clamp(0, 512);
            var endX = x1.Clamp(0, 512);
            var ptr = (uint*) Memory.PspAddressToPointerSafe((uint) (DrawAddress + (y * 512 + startX) * 4));
            var count = endX - startX;
            for (int n = 0; n < count; n++)
            {
                var ratio = (startX + n).RatioInRange(x0, x1);
                ptr[n] = new RgbaFloat(Vector4.Lerp(c1, c2, ratio)).Int;
            }
        }

        public override void Finish(GpuStateStruct* gpuState)
        {
            base.Finish(gpuState);
        }

        public override void End(GpuStateStruct* gpuState)
        {
            base.End(gpuState);
        }

        public override void Sync(GpuStateStruct* lastGpuState)
        {
            base.Sync(lastGpuState);
        }

        public override void BeforeDraw(GpuStateStruct* gpuState)
        {
            base.BeforeDraw(gpuState);
        }

        public override void InvalidateCache(uint address, int size)
        {
            base.InvalidateCache(address, size);
        }

        public override void TextureFlush(GpuStateStruct* gpuState)
        {
            base.TextureFlush(gpuState);
        }

        public override void TextureSync(GpuStateStruct* gpuState)
        {
            base.TextureSync(gpuState);
        }

        public override void AddedDisplayList()
        {
            base.AddedDisplayList();
        }

        public override void StartCapture()
        {
            base.StartCapture();
        }

        public override void EndCapture()
        {
            base.EndCapture();
        }

        public override void Transfer(GpuStateStruct* gpuState)
        {
            base.Transfer(gpuState);
        }

        public override void SetCurrent()
        {
            base.SetCurrent();
        }

        public override void UnsetCurrent()
        {
            base.UnsetCurrent();
        }

        public override void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuStateStruct,
            VertexInfo[,] Patch, int UCount,
            int VCount)
        {
            base.DrawCurvedSurface(GlobalGpuState, GpuStateStruct, Patch, UCount, VCount);
        }

        public override void DrawVideo(uint FrameBufferAddress, OutputPixel* OutputPixel, int Width, int Height)
        {
            base.DrawVideo(FrameBufferAddress, OutputPixel, Width, Height);
        }
    }
}