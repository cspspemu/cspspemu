using System;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.VertexReading;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Types;

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
            set
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

        public virtual void PrimStart(GlobalGpuState globalGpuState, GpuStateStruct* gpuState,
            GuPrimitiveType primitiveType)
        {
        }

        public virtual void PrimEnd()
        {
        }

        public virtual void Prim(ushort vertexCount)
        {
        }

        public virtual void Finish(GpuStateStruct* gpuState)
        {
        }

        public virtual void End(GpuStateStruct* gpuState)
        {
        }

        public virtual void Sync(GpuStateStruct* lastGpuState)
        {
        }

        public virtual void BeforeDraw(GpuStateStruct* gpuState)
        {
        }

        public virtual void InvalidateCache(uint address, int size)
        {
        }

        public virtual void TextureFlush(GpuStateStruct* gpuState)
        {
        }

        public virtual void TextureSync(GpuStateStruct* gpuState)
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

        public virtual void Transfer(GpuStateStruct* gpuState)
        {
            Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}",
                gpuState->TextureTransferState.ToStringDefault());
        }

        public virtual void SetCurrent()
        {
        }

        public virtual void UnsetCurrent()
        {
        }

        public virtual void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuStateStruct,
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
        
        protected void PreparePrim(GpuStateStruct* GpuState, out uint totalVerticesWithoutMorphing, uint vertexCount, out uint morpingVertexCount)
        {
            totalVerticesWithoutMorphing = vertexCount;
            morpingVertexCount = (uint)(VertexType.MorphingVertexCount + 1);
            readVertex = ReadVertex_Void_delegate;
            VertexReader.SetVertexTypeStruct(
                VertexType,
                (byte*) Memory.PspAddressToPointerSafe(GpuState->GetAddressRelativeToBaseOffset(GpuState->VertexAddress), 0)
            );
            
            void* indexPointer = null;
            if (VertexType.Index != VertexTypeStruct.IndexEnum.Void)
            {
                indexPointer =
                    Memory.PspAddressToPointerSafe(GpuState->GetAddressRelativeToBaseOffset(GpuState->IndexAddress), 0);
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
                    throw (new NotImplementedException("VertexType.Index: " + VertexType.Index));
            }
            totalVerticesWithoutMorphing++;
         
            
            // Fix missing geometry! At least!
            if (VertexType.Index == VertexTypeStruct.IndexEnum.Void)
            {
                GpuState->VertexAddress += (uint) (VertexReader.VertexSize * vertexCount * morpingVertexCount);
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