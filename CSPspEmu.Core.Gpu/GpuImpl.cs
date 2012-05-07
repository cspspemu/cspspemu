using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu
{
	unsafe abstract public class GpuImpl : PspPluginImpl
	{
		public override void InitializeComponent()
		{
		}

		abstract public void InitSynchronizedOnce();
		abstract public void StopSynchronized();

		abstract public void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount);
		abstract public void Finish(GpuStateStruct* GpuState);
		abstract public void End(GpuStateStruct* GpuState);

		virtual public void TextureFlush(GpuStateStruct* GpuState)
		{
		}
		virtual public void TextureSync(GpuStateStruct* GpuState)
		{
		}

		abstract public void AddedDisplayList();

		virtual public void StartCapture()
		{
		}

		virtual public void EndCapture()
		{
		}

		virtual public void Transfer(GpuStateStruct* GpuState)
		{
			Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}", GpuState->TextureTransferState.ToStringDefault());
		}

		abstract public void SetCurrent();
		abstract public void UnsetCurrent();

		virtual public void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuStateStruct, VertexInfo[,] Patch, int UCount, int VCount)
		{
			Console.Error.WriteLine("GpuImpl.DrawCurvedSurface Not Implemented!!");
		}
	}
}
