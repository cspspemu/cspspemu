using System;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu
{
	public unsafe abstract class GpuImpl : PspPluginImpl
	{
		public abstract void InitSynchronizedOnce();
		public abstract void StopSynchronized();

		public abstract void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount);
		public abstract void Finish(GpuStateStruct* GpuState);
		public abstract void End(GpuStateStruct* GpuState);
		public abstract void Sync(GpuStateStruct* LastGpuState);

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
	}
}
