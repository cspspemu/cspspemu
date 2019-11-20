using System;
using System.Numerics;
using CSharpUtils.Drawing;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.VertexReading;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;
using CSPspEmu.Rasterizer;
using CSPspEmu.Utils;

namespace CSPspEmu.Core.Gpu
{
    public abstract unsafe class GpuImpl : PspPluginImpl
    {
        [Inject] protected PspMemory Memory;
        [Inject] protected PspStoredConfig PspStoredConfig;
        

        protected int _ScaleViewport = 2;

        internal event Action<int> OnScaleViewport;

        public int ScaleViewport
        {
            protected set
            {
                OnScaleViewport?.Invoke(value);
                _ScaleViewport = value;
            }
            get => _ScaleViewport;
        }

        public virtual void InitSynchronizedOnce()
        {
        }

        public virtual void StopSynchronized()
        {
        }

        protected Matrix4x4 modelView = Matrix4x4.Identity;
        protected Matrix4x4 worldViewProjection3D = Matrix4x4.Identity;
        protected Matrix4x4 worldViewProjection2D = Matrix4x4.Identity;
        protected Matrix4x4 transform3d = Matrix4x4.Identity;

        protected Matrix4x4 unitToScreenCoords = Matrix4x4.CreateScale(1f, -1f, 1f) *
                                               Matrix4x4.CreateTranslation(+1f, +1f, 0f) *
                                               Matrix4x4.CreateScale(.5f, .5f, 1f) *
                                               Matrix4x4.CreateScale(480, 272, 1f);

        protected GpuStateStruct GpuState;
        protected uint DrawAddress;
        protected GuPrimitiveType PrimitiveType;


        public virtual void PrimStart(GlobalGpuState globalGpuState, GpuStateStruct gpuState,
            GuPrimitiveType primitiveType)
        {
            GpuState = gpuState;
            VertexType = gpuState.VertexState.Type;
            PrimitiveType = primitiveType;
            var model = gpuState.VertexState.WorldMatrix;
            var view = gpuState.VertexState.ViewMatrix;
            var projection3D = gpuState.VertexState.ProjectionMatrix;
            var projection2D = Matrix4x4.CreateOrthographic(512, 272, -1f, +1f);
            //Matrix4x4.Invert(projection2D, out unitToScreenCoords);
            modelView = model * view;
            worldViewProjection3D = modelView * projection3D;
            worldViewProjection2D = modelView * projection2D;
            DrawAddress = GpuState.DrawBufferState.Address;
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

        public virtual void PrimEnd()
        {
        }

        public virtual void Prim(ushort vertexCount)
        {
            //Console.WriteLine($"Prim({vertexCount})");
            uint morpingVertexCount, totalVerticesWithoutMorphing;
            PreparePrim(GpuState, out totalVerticesWithoutMorphing, vertexCount, out morpingVertexCount);
            var vertices = new Vector4[vertexCount];
            var vP = new VPoint[vertexCount];
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
                    var color = VertexType.HasColor ? vinfo.Color : GpuState.LightingState.AmbientModelColor.ToVector4();
                    //Console.WriteLine(GpuState->LightingState.AmbientLightColor);
                    vP[n] = Vector4ToPoint(tvertex, vinfo.Normal, vinfo.Texture, color);
                }
            }
            {
                switch (PrimitiveType)
                {
                    case GuPrimitiveType.Points:
                    {
                        for (var n = 0; n < vertexCount; n++) DrawPoint(vP[n]);
                        break;
                    }
                    case GuPrimitiveType.Lines:
                    {
                        for (var n = 0; n < vertexCount; n += 2) DrawLine(vP[n], vP[n + 1]);
                        break;
                    }
                    case GuPrimitiveType.LineStrip:
                    {
                        for (var n = 1; n < vertexCount; n++) DrawLine(vP[n - 1], vP[n]);
                        break;
                    }
                    case GuPrimitiveType.Triangles:
                    {
                        for (var n = 0; n < vertexCount; n += 3) DrawTriangle(vP[n + 0], vP[n + 1], vP[n + 2]);
                        break;
                    }
                    case GuPrimitiveType.TriangleStrip:
                    {
                        for (var n = 2; n < vertexCount; n++) DrawTriangle(vP[n - 2], vP[n - 1], vP[n]);
                        break;
                    }
                    case GuPrimitiveType.TriangleFan:
                    {
                        for (var n = 2; n < vertexCount; n++) DrawTriangle(vP[0], vP[n - 1], vP[n]);
                        break;
                    }
                    case GuPrimitiveType.Sprites:
                    {
                        for (var n = 0; n < vertexCount; n += 2) DrawSprite(vP[n + 0], vP[n + 1]);
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

        virtual protected void DrawPoint(VPoint a)
        {
            VPoint b = new VPoint(new RasterizerPoint(a.P.X, a.P.Y + 1), a.N, a.T, a.Color);
            DrawLine(a, b);
        }

        virtual protected void DrawLine(VPoint a, VPoint b)
        {
        }

        virtual protected void DrawTriangle(VPoint a, VPoint b, VPoint c)
        {
        }

        virtual protected void DrawSprite(VPoint a, VPoint b)
        {
            VPoint ar = new VPoint(new RasterizerPoint(b.P.X, a.P.Y), a.N, a.T, a.Color);
            VPoint bl = new VPoint(new RasterizerPoint(a.P.X, b.P.Y), b.N, b.T, b.Color);
            DrawTriangle(a, ar, bl);
            DrawTriangle(bl, ar, b);
        }

        //static public Vector4 LerpRatios(Vector4 a, Vector4 b, Vector4 c, float ra, float rb, float rc) => (a * ra) + (b * rb) * (c * rc);
        static public Vector4 LerpRatios(Vector4 a, Vector4 b, Vector4 c, float ra, float rb, float rc) => new Vector4(
            a.X * ra + b.X * rb + c.X * rc,
            a.Y * ra + b.Y * rb + c.Y * rc,
            a.Z * ra + b.Z * rb + c.Z * rc,
            a.W * ra + b.W * rb + c.W * rc
        );

        static public Vector4 LerpRatios(Vector4 a, Vector4 b, Vector4 c, Vector3 ratios) =>
            LerpRatios(a, b, c, ratios.X, ratios.Y, ratios.Z);


        private VPoint Vector4ToPoint(Vector4 v, Vector4 n, Vector4 t, Vector4 color) =>
            new VPoint(new RasterizerPoint((int) v.X, (int) v.Y), n, t, color);

        private uint[] colors = {0xFF0077FF, 0xFF00FFFF, 0xFF0000FF};

        public struct VPoint
        {
            public RasterizerPoint P;
            public Vector4 N;
            public Vector4 T;
            public Vector4 Color;

            public Rgba RgbaColor => new RgbaFloat(Color).Rgba;
            public uint IColor => new RgbaFloat(Color).Int;

            public VPoint(RasterizerPoint p, Vector4 n, Vector4 t, Vector4 color)
            {
                P = p;
                N = n;
                T = t;
                Color = color;
            }

            public override string ToString() => $"VPoint({P}, {N}, {T}, {Color})";
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

        public struct Line
        {
            public VPoint P0;
            public VPoint P1;

            public Line(VPoint p0, VPoint p1)
            {
                // Sort the points so that y0 <= y1 <= y2
                if (p1.P.Y < p0.P.Y) Swap(ref p1, ref p0);
                P0 = p0;
                P1 = p1;
            }
        }

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }


        public virtual void Finish(GpuStateStruct gpuState)
        {
        }

        public virtual void End(GpuStateStruct gpuState)
        {
        }

        public virtual void Sync(GpuStateStruct lastGpuState)
        {
        }

        public virtual void BeforeDraw(GpuStateStruct gpuState)
        {
        }

        public virtual void InvalidateCache(uint address, int size)
        {
        }

        public virtual void TextureFlush(GpuStateStruct gpuState)
        {
        }

        public virtual void TextureSync(GpuStateStruct gpuState)
        {
        }

        public virtual void AddedDisplayList()
        {
        }

        public virtual void StartCapture()
        {
        }

        public virtual void EndCapture()
        {
        }

        public virtual void Transfer(GpuStateStruct gpuState)
        {
            Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!!");
        }

        public virtual void SetCurrent()
        {
        }

        public virtual void UnsetCurrent()
        {
        }

        public virtual void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct GpuStateStruct,
            VertexInfo[,] Patch, int UCount, int VCount)
        {
            Console.Error.WriteLine("GpuImpl.DrawCurvedSurface Not Implemented!!");
        }

        public virtual void DrawVideo(uint FrameBufferAddress, OutputPixel* OutputPixel, int Width, int Height)
        {
        }
        
        protected VertexInfo[] Vertices = new VertexInfo[ushort.MaxValue];
        protected VertexTypeStruct VertexType;
        protected byte* indexListByte;
        protected ushort* indexListShort;
        protected ReadVertexDelegate readVertex;
        protected VertexReader VertexReader = new VertexReader();

        protected ReadVertexDelegate ReadVertex_Void_delegate;
        protected ReadVertexDelegate ReadVertex_Byte_delegate;
        protected ReadVertexDelegate ReadVertex_Short_delegate;

        public GpuImpl()
        {
            // ReSharper disable HeapView.DelegateAllocation
            ReadVertex_Void_delegate = ReadVertex_Void;
            ReadVertex_Byte_delegate = ReadVertex_Byte;
            ReadVertex_Short_delegate = ReadVertex_Short;
        }

        private void ReadVertex_Void(int index, out VertexInfo vertexInfo) => vertexInfo = Vertices[index];

        private void ReadVertex_Byte(int index, out VertexInfo vertexInfo) =>
            vertexInfo = Vertices[indexListByte[index]];

        private void ReadVertex_Short(int index, out VertexInfo vertexInfo) =>
            vertexInfo = Vertices[indexListShort[index]];

        protected delegate void ReadVertexDelegate(int index, out VertexInfo vertexInfo);
        
        protected void PreparePrim(GpuStateStruct GpuState, out uint totalVerticesWithoutMorphing, uint vertexCount, out uint morpingVertexCount)
        {
            totalVerticesWithoutMorphing = vertexCount;
            morpingVertexCount = (uint)(VertexType.MorphingVertexCount + 1);
            readVertex = ReadVertex_Void_delegate;
            VertexReader.SetVertexTypeStruct(
                VertexType,
                (byte*) Memory.PspAddressToPointerSafe(GpuState.GetAddressRelativeToBaseOffset(GpuState.VertexAddress), 0)
            );
            
            void* indexPointer = null;
            if (VertexType.Index != VertexTypeStruct.IndexEnum.Void)
            {
                indexPointer =
                    Memory.PspAddressToPointerSafe(GpuState.GetAddressRelativeToBaseOffset(GpuState.IndexAddress), 0);
            }

            //Console.Error.WriteLine(VertexType.Index);
            switch (VertexType.Index)
            {
                case VertexTypeStruct.IndexEnum.Void:
                    break;
                case VertexTypeStruct.IndexEnum.Byte:
                    readVertex = ReadVertex_Byte_delegate;
                    indexListByte = (byte*) indexPointer;
                    totalVerticesWithoutMorphing = 0;
                    if (indexListByte != null)
                    {
                        for (var n = 0; n < vertexCount; n++)
                        {
                            if (totalVerticesWithoutMorphing < indexListByte[n])
                                totalVerticesWithoutMorphing = indexListByte[n];
                        }
                    }

                    break;
                case VertexTypeStruct.IndexEnum.Short:
                    readVertex = ReadVertex_Short_delegate;
                    indexListShort = (ushort*) indexPointer;
                    totalVerticesWithoutMorphing = 0;
                    //VertexCount--;
                    if (indexListShort != null)
                    {
                        for (var n = 0; n < vertexCount; n++)
                        {
                            //Console.Error.WriteLine(IndexListShort[n]);
                            if (totalVerticesWithoutMorphing < indexListShort[n])
                                totalVerticesWithoutMorphing = indexListShort[n];
                        }
                    }

                    break;
                default:
                    throw new NotImplementedException("VertexType.Index: " + VertexType.Index);
            }
            totalVerticesWithoutMorphing++;
         
            
            // Fix missing geometry! At least!
            if (VertexType.Index == VertexTypeStruct.IndexEnum.Void)
            {
                GpuState.VertexAddress += (uint) (VertexReader.VertexSize * vertexCount * morpingVertexCount);
                //GpuState->VertexAddress += (uint)(VertexReader.VertexSize * VertexCount);
            }

            if (morpingVertexCount != 1 || VertexType.RealSkinningWeightCount != 0)
            {
                //Console.WriteLine("PRIM: {0}, {1}, Morphing:{2}, Skinning:{3}", PrimitiveType, VertexCount, MorpingVertexCount, VertexType.RealSkinningWeightCount);
            }


            //Console.WriteLine(TotalVerticesWithoutMorphing);
        }
    }
}