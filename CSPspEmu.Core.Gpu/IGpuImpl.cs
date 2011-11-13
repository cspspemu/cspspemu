using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu
{
	unsafe public interface IGpuImpl
	{
		void Prim(GpuStateStruct* GpuState, PrimitiveType PrimitiveType, ushort VertexCount);
		void Finish(GpuStateStruct* GpuState);
	}
}
