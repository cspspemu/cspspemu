using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		 * Set current Fog
		 *
		 * @param near  - 
		 * @param far   - 
		 * @param color - 0x00RRGGBB
		 **/
		// void sceGuFog(float near, float far, unsigned int color); // OP_FCOL + OP_FFAR + OP_FDIST

		// Fog enable (GU_FOG)
		[GpuOpCodesNotImplemented]
		public void OP_FGE()
		{
			//gpu.state.fog.enabled = command.bool1;
		}

		// Fog COLor
		[GpuOpCodesNotImplemented]
		public void OP_FCOL()
		{
			//gpu.state.fog.color.rgb[] = command.float3[];
			//gpu.state.fog.color.a = 1.0;
		}

		// Fog FAR
		[GpuOpCodesNotImplemented]
		public void OP_FFAR()
		{
			//gpu.state.fog.end = command.float1;
			//writefln("OP_FFAR: %f", gpu.state.fog.end);
		}

		// Fog DISTance
		[GpuOpCodesNotImplemented]
		public void OP_FDIST()
		{
			//gpu.state.fog.dist = command.float1;
			//writefln("OP_FDIST: %f", gpu.state.fog.end);
		}
	}
}
