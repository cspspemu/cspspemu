namespace CSPspEmu.Core.Gpu.Run
{
	public unsafe sealed partial class GpuDisplayListRunner
	{
		/**
		 * Set current Fog
		 *
		 * @param near  - 
		 * @param far   - 
		 * @param color - 0x00RRGGBB
		 **/
		// void sceGuFog(float near, float far, unsigned int color); // OP_FCOL + OP_FFAR + OP_FDIST

		/// <summary>
		/// Fog enable (GU_FOG)
		/// </summary>
		public void OP_FGE()
		{
			GpuState->FogState.Enabled = Bool1;
			//gpu.state.fog.enabled = command.bool1;
		}

		/// <summary>
		/// Fog COLor
		/// </summary>
		public void OP_FCOL()
		{
			GpuState->FogState.Color.SetRGB_A1(Params24);
		}

		/// <summary>
		/// Fog FAR
		/// </summary>
		//[GpuOpCodesNotImplemented]
		public void OP_FFAR()
		{
			GpuState->FogState.End = Float1;
			//gpu.state.fog.end = command.float1;
			//writefln("OP_FFAR: %f", gpu.state.fog.end);
		}

		/// <summary>
		/// Fog DISTance
		/// </summary>
		//[GpuOpCodesNotImplemented]
		public void OP_FDIST()
		{
			GpuState->FogState.Dist = Float1;
			//gpu.state.fog.dist = command.float1;
			//writefln("OP_FDIST: %f", gpu.state.fog.end);
		}
	}
}
