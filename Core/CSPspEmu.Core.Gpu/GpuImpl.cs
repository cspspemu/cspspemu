using System;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu
{
	public unsafe abstract class GpuImpl : PspPluginImpl
	{
		[Inject]
		protected PspStoredConfig PspStoredConfig;

		protected int _scaleViewport = 2;

		internal event Action<int> OnScaleViewport;

		public int ScaleViewport
		{
			set
			{
				if (OnScaleViewport != null) OnScaleViewport(value);
				_scaleViewport = value;
			}
			get
			{
				return _scaleViewport;
			}
		}

		public virtual void InitSynchronizedOnce() { }
		public virtual void StopSynchronized() { }

		public virtual void PrimStart(GlobalGpuState globalGpuState, GpuStateStruct* gpuState, GuPrimitiveType primitiveType) { }
		public virtual void PrimEnd() { }
		public virtual void Prim(ushort vertexCount) { }
		public virtual void Finish(GpuStateStruct* gpuState) { }
		public virtual void End(GpuStateStruct* gpuState) { }
		public virtual void Sync(GpuStateStruct* lastGpuState) { }

		public virtual void BeforeDraw(GpuStateStruct* gpuState) { }

		public virtual void InvalidateCache(uint address, int size)
		{
		}

		public virtual void TextureFlush(GpuStateStruct* gpuState)
		{
		}
		public virtual void TextureSync(GpuStateStruct* gpuState)
		{
		}

		public virtual void AddedDisplayList() { }

		public virtual void StartCapture()
		{
		}

		public virtual void EndCapture()
		{
		}

		public virtual void Transfer(GpuStateStruct* gpuState)
		{
			Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}", gpuState->TextureTransferState.ToStringDefault());
		}

		public virtual void SetCurrent() { }
		public virtual void UnsetCurrent() { }

		public virtual void DrawCurvedSurface(GlobalGpuState globalGpuState, GpuStateStruct* gpuStateStruct, VertexInfo[,] patch, int uCount, int vCount)
		{
			Console.Error.WriteLine("GpuImpl.DrawCurvedSurface Not Implemented!!");
		}

		public virtual void DrawVideo(uint frameBufferAddress, OutputPixel* outputPixel, int width, int height)
		{
		}
	}
}
