using System;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu
{
    public abstract unsafe class GpuImpl : PspPluginImpl
    {
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
    }
}