using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Gpu
{
	unsafe abstract public class GpuImpl : PspEmulatorComponent
	{
		public override void InitializeComponent()
		{
		}

		abstract public void InitSynchronizedOnce();
		abstract public void StopSynchronized();

		abstract public void Prim(GpuStateStruct* GpuState, PrimitiveType PrimitiveType, ushort VertexCount);
		abstract public void Finish(GpuStateStruct* GpuState);
		abstract public void End(GpuStateStruct* GpuState);

		virtual public void TextureFlush(GpuStateStruct* GpuState)
		{
		}
		virtual public void TextureSync(GpuStateStruct* GpuState)
		{
		}

		abstract public void AddedDisplayList();

		virtual public void Transfer(GpuStateStruct* GpuStateStruct)
		{
			Console.Error.WriteLine("GpuImpl.Transfer Not Implemented!! : {0}", GpuStateStruct->TextureTransferState.ToStringDefault());
		}
	}
}
