using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;
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

        private Matrix4x4 vertexTransform = new Matrix4x4();

        private GpuStateStruct* GpuState;
        private uint DrawAddress;
        private GuPrimitiveType PrimitiveType;

        public override void PrimStart(GlobalGpuState globalGpuState, GpuStateStruct* gpuState, GuPrimitiveType primitiveType)
        {
            GpuState = gpuState;
            VertexType = gpuState->VertexState.Type;
            PrimitiveType = primitiveType;
            var world = gpuState->VertexState.WorldMatrix.Matrix4x4;
            var view = gpuState->VertexState.ViewMatrix.Matrix4x4;
            var projection = gpuState->VertexState.ProjectionMatrix.Matrix4x4;
            vertexTransform = world * view * projection * Matrix4x4.CreateTranslation(-1, -1, 0) * Matrix4x4.CreateScale(512 / 2, 272 / 2, 0);
            DrawAddress = GpuState->DrawBufferState.Address;
            Console.WriteLine($"PrimStart: {primitiveType}");
        }

        public override void PrimEnd()
        {
            Console.WriteLine($"PrimEnd");
        }

        public override void Prim(ushort vertexCount)
        {
            uint morpingVertexCount, totalVerticesWithoutMorphing;
            PreparePrim(GpuState, out totalVerticesWithoutMorphing, vertexCount, out morpingVertexCount);
            var vertices = new Vector4[vertexCount];
            var vertexTransform = this.vertexTransform;
            {
                for (var n = 0; n < vertexCount; n++)
                {
                    var vinfo = VertexReader.ReadVertex(n);
                    var vector = vinfo.Position.ToVector4();
                    
                    vertices[n] = VertexType.Transform2D ? vector : Vector4.Transform(vector, vertexTransform);
                }
            }
            {
                switch (PrimitiveType)
                {
                    case GuPrimitiveType.Triangles:
                    {
                        var m = 0;
                        for (var n = 0; n < vertexCount; n += 3)
                        {
                            //DrawTriangleFast(Vector4ToPoint(vertices[n + 0]), Vector4ToPoint(vertices[n + 1]), Vector4ToPoint(vertices[n + 2]), colors[m++ % colors.Length]);
                        }

                        DrawTriangleFast(new PointIS(100, 0), new PointIS(50, 100), new PointIS(150, 50), 0xFF00FFFF);

                        break;
                    }

                    case GuPrimitiveType.Sprites:
                    {
                        {
                            //for (var n = 0; n < vertexCount; n++) Console.WriteLine("Sprite:" + VertexReader.ReadVertex(n));

                        }
                        for (var n = 0; n < vertexCount; n += 2)
                        {
                            DrawSprite(Vector4ToPoint(vertices[n + 0]), Vector4ToPoint(vertices[n + 1]));
                        }
                        break;
                    }
                    default:
                        Console.WriteLine($"Unsupported {PrimitiveType}");
                        break;
                }
            }

            //DrawPixel(52, 52, 0xFFFFFFFF);
            Console.WriteLine($"Prim {vertexCount}");
        }

        private PointIS Vector4ToPoint(Vector4 v) => new PointIS(
            (int)v.X, (int)v.Y
            //(int) (((v.X + 1.0) / 2.0) * 512),
            //(int) (((v.Y + 1.0) / 2.0) * 272)
        );

        private void DrawTriangleSlow(PointIS a, PointIS b, PointIS c)
        {
            //Console.WriteLine($"Triangle {a}, {b}, {c}");
            var minX = Math.Min(Math.Min(a.X, b.X), c.X).Clamp(0, 512);
            var maxX = Math.Max(Math.Max(a.X, b.X), c.X).Clamp(0, 512);
            var minY = Math.Min(Math.Min(a.Y, b.Y), c.Y).Clamp(0, 272);
            var maxY = Math.Max(Math.Max(a.Y, b.Y), c.Y).Clamp(0, 272);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (PointInTriangle(new PointIS(x, y), a, b, c))
                    {
                        DrawPixel(x, y, 0xFFFFFFFF);
                    }
                }
            }
        }

        private uint[] colors = {0xFF0077FF, 0xFF00FFFF, 0xFF0000FF};

        private void DrawTriangleFast(PointIS P0, PointIS P1, PointIS P2, uint color)
        {
            // Sort the points so that y0 <= y1 <= y2
            if (P1.y < P0.y) Swap(ref P1, ref P0);
            if (P2.y < P0.y) Swap(ref P2, ref P0);
            if (P2.y < P1.y) Swap(ref P2, ref P1);
            
            Console.WriteLine($"{P0}, {P1}, {P2}");

            var y0 = P0.y.Clamp(0, 272);
            var y2 = P2.y.Clamp(0, 272);
            
            for (var y = y0; y <= y2; y++)
            {
                var xa = InterpolateX(P0, P2, y);
                var xb = (y <= P1.y) ? InterpolateX(P0, P1, y) : InterpolateX(P1, P2, y);
                var xmin = Math.Min(xa, xb).Clamp(0, 512);
                var xmax = Math.Max(xa, xb).Clamp(0, 512);
                DrawPixelsFast(xmin, y, xmax - xmin, color);
                //for (var x = xmin; x <= xmax; x++) DrawPixel(x, y, 0xFFFFFFFF);
            }
        }

        private int InterpolateX(PointIS a, PointIS b, int y)
        {
            var ratio = (double)(y - b.Y) / (double)(b.Y - a.Y);
            return ratio.Interpolate(a.X, b.X);
        }
        
        public static void Swap<T> (ref T lhs, ref T rhs) {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        
        private void DrawSprite(PointIS a, PointIS b)
        {
            var minX = Math.Min(a.X, b.X).Clamp(0, 512);
            var maxX = Math.Max(a.X, b.X).Clamp(0, 512);
            var minY = Math.Min(a.Y, b.Y).Clamp(0, 272);
            var maxY = Math.Max(a.Y, b.Y).Clamp(0, 272);
            //Console.WriteLine($"Sprite {a}, {b}");
            for (int y = minY; y <= maxY; y++)
            {
                DrawPixelsFast(minX, y, maxX - minX, 0x777777FF);
            }
        }
        
        float sign (PointIS p1, PointIS p2, PointIS p3)
        {
            return (p1.X - p3.X) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        bool PointInTriangle (PointIS pt, PointIS v1, PointIS v2, PointIS v3)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, v1, v2);
            d2 = sign(pt, v2, v3);
            d3 = sign(pt, v3, v1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        private void DrawPixel(int x, int y, uint color)
        {
            if (x < 0 || y < 0 || x > 512 || y > 272) return;
            Memory.Write4((uint)(DrawAddress + (y * 512 + x) * 4), color);
        }

        private void DrawPixelsFast(int x, int y, int count, uint color)
        {
            if (x < 0 || y < 0 || x > 512 || y > 272) return;
            var ptr = (uint *)Memory.PspAddressToPointerSafe((uint) (DrawAddress + (y * 512 + x) * 4));
            for (int n = 0; n < count; n++) ptr[n] = color; 
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

        public override void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuStateStruct, VertexInfo[,] Patch, int UCount,
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