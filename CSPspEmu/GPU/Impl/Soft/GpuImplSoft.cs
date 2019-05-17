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
                        //Parallel.For(0, vertexCount / 3, (it) =>
                        //{
                        //    var n = it * 3;
                        //    DrawTriangleFast((verticesP[n + 0]), (verticesP[n + 1]), (verticesP[n + 2]));
                        //});
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
            TriangleRasterizer.RasterizeTriangle(triangle.P0.P, triangle.P1.P, triangle.P2.P, triangle);
        }

        //static public Vector4 LerpRatios(Vector4 a, Vector4 b, Vector4 c, float ra, float rb, float rc) => (a * ra) + (b * rb) * (c * rc);
        static public Vector4 LerpRatios(Vector4 a, Vector4 b, Vector4 c, float ra, float rb, float rc) => new Vector4(
            ((a.X * ra) + (b.X * rb) + (c.X * rc)).Clamp(0, 1),
            ((a.Y * ra) + (b.Y * rb) + (c.Y * rc)).Clamp(0, 1),
            ((a.Z * ra) + (b.Z * rb) + (c.Z * rc)).Clamp(0, 1),
            ((a.W * ra) + (b.W * rb) + (c.W * rc)).Clamp(0, 1)
            );

        private RasterizeDelegate<Triangle> DrawTriangleFastTest;
        private TriangleRasterizer<Triangle> TriangleRasterizer;

        public GpuImplSoft()
        {
            DrawTriangleFastTest = (int y, ref RasterizerResult a, ref RasterizerResult b, ref Triangle triangle) =>
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
            };
            TriangleRasterizer = new TriangleRasterizer<Triangle>(DrawTriangleFastTest, 0, 271, 0, 511);
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        private void DrawSprite(VPoint a, VPoint b)
        {
            var minX = Math.Min(a.P.X, b.P.X).Clamp(0, 511);
            var maxX = Math.Max(a.P.X, b.P.X).Clamp(0, 511);
            var minY = Math.Min(a.P.Y, b.P.Y).Clamp(0, 271);
            var maxY = Math.Max(a.P.Y, b.P.Y).Clamp(0, 271);
            //Console.WriteLine($"Sprite {a}, {b}");
            for (var y = minY; y <= maxY; y++) DrawPixelsFast(y, minX, maxX, a.IColor);
        }

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