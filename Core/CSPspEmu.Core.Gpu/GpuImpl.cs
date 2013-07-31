using System;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu
{
	public unsafe abstract class GpuImpl : PspPluginImpl
	{
		[Inject]
		protected PspStoredConfig PspStoredConfig;

		protected int _ScaleViewport = 1;

		protected event Action<int> OnScaleViewport;

		public int ScaleViewport
		{
			set
			{
				if (OnScaleViewport != null) OnScaleViewport(value);
				this._ScaleViewport = value;
			}
			get
			{
				return _ScaleViewport;
			}
		}

		public abstract void InitSynchronizedOnce();
		public abstract void StopSynchronized();

		public virtual void PrimStart(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState) { }
		public virtual void PrimEnd(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState) { }
		public abstract void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount);
		public abstract void Finish(GpuStateStruct* GpuState);
		public abstract void End(GpuStateStruct* GpuState);
		public abstract void Sync(GpuStateStruct* LastGpuState);

		public abstract void BeforeDraw(GpuStateStruct* GpuState);

		public virtual void InvalidateCache(uint Address, int Size)
		{
		}

		public virtual void TextureFlush(GpuStateStruct* GpuState)
		{
		}
		public virtual void TextureSync(GpuStateStruct* GpuState)
		{
		}

		public abstract void AddedDisplayList();

		public virtual void StartCapture()
		{
		}

		public virtual void EndCapture()
		{
		}

		public virtual void Transfer(GpuStateStruct* GpuState)
		{
			Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}", GpuState->TextureTransferState.ToStringDefault());
		}

		public abstract void SetCurrent();
		public abstract void UnsetCurrent();

		public virtual void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuStateStruct, VertexInfo[,] Patch, int UCount, int VCount)
		{
			Console.Error.WriteLine("GpuImpl.DrawCurvedSurface Not Implemented!!");
		}

		public virtual void DrawVideo(uint FrameBufferAddress, OutputPixel* OutputPixel, int Width, int Height)
		{
		}
	}
}
