using CSharpUtils;
using CSPspEmu.Core.Display;

namespace CSPspEmu.Core.Gpu.Run
{
	/// <summary>
	/// CLUT (Color LookUp Table) opcodes.
	/// </summary>
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		 * Upload CLUT (Color Lookup Table)
		 *
		 * @note Data must be aligned to 1 quad word (16 bytes)
		 *
		 * @param num_blocks - How many blocks of 8 entries to upload (32*8 is 256 colors)
		 * @param cbp        - Pointer to palette (16 byte aligned)
		 **/
		///void sceGuClutLoad(int num_blocks, const void* cbp); // OP_CBP + OP_CBPH + OP_CLOAD

		// Clut Buffer Pointer (High)
		// Clut LOAD
		public void OP_CBP()
		{
			GpuState[0].TextureMappingState.ClutState.Address
				= (GpuState[0].TextureMappingState.ClutState.Address & 0xFF000000) | ((Params24 << 0) & 0x00FFFFFF);
			;
		}
		public void OP_CBPH()
		{
			GpuState[0].TextureMappingState.ClutState.Address
				= (GpuState[0].TextureMappingState.ClutState.Address & 0x00FFFFFF) | ((Params24 << 24) & 0xFF000000);
			;
		}
		[GpuOpCodesNotImplemented]
		public void OP_CLOAD()
		{
			//ubyte num_entries = command.extract!(ubyte);
		
			/*
			gpu.state.uploadedClut = gpu.state.clut;
			int size = gpu.state.clut.blocksSize(num_entries);
			gpu.state.uploadedClut.data = gpu.state.clut.address ? gpu.memory[gpu.state.clut.address..gpu.state.clut.address + size].dup : [];
			*/
			//int size = gpu.state.clut.blocksSize(num_entries);

			//writefln("%d, %d", num_entries, size);
		
			//gpu.state.clut.data = gpu.state.clut.address ? cast(ubyte*)gpu.memory.getPointer(gpu.state.clut.address) : null;
		}

		/**
		 * Set current CLUT mode
		 *
		 * Available pixel formats for palettes are:
		 *   - GU_PSM_5650
		 *   - GU_PSM_5551
		 *   - GU_PSM_4444
		 *   - GU_PSM_8888
		 *
		 * @param cpsm  - Which pixel format to use for the palette
		 * @param shift - Shifts color index by that many bits to the right
		 * @param mask  - Masks the color index with this bitmask after the shift (0-0xFF)
		 * @param start - Unknown, set to 0
		 **/
		///void sceGuClutMode(uint cpsm, uint shift, uint mask, uint a3); // OP_CMODE

		// Clut MODE
		public void OP_CMODE()
		{
			GpuState[0].TextureMappingState.ClutState.Format = (PixelFormats)BitUtils.Extract(Params24, 0, 2);
			GpuState[0].TextureMappingState.ClutState.Shift = (uint)BitUtils.Extract(Params24, 2, 5);
			GpuState[0].TextureMappingState.ClutState.Mask = (uint)BitUtils.Extract(Params24, 8, 8);
			GpuState[0].TextureMappingState.ClutState.Start = (uint)BitUtils.Extract(Params24, 16, 5);
		}
	}
}
