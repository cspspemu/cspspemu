using System;
using CSharpUtils;
using CSPspEmu.Core.Display;
using CSPspEmu.Core.Gpu.State.SubStates;

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
		///
		ClutStateStruct* ClutState
		{
			get
			{
				return &GpuState[0].TextureMappingState.ClutState;
			}
		}

		// Clut Buffer Pointer (High)
		// Clut LOAD
		public void OP_CBP()
		{
			ClutState[0].Address
				= (ClutState[0].Address & 0xFF000000) | ((Params24 << 0) & 0x00FFFFFF);
			;
			//Console.WriteLine("OP_CBP:{0:X}", Params24);
		}
		public void OP_CBPH()
		{
			ClutState[0].Address
				= (ClutState[0].Address & 0x00FFFFFF) | ((Params24 << 8) & 0xFF000000);
			;
			//Console.WriteLine("OP_CBPH:{0:X}", Params24);
		}
		public void OP_CLOAD()
		{
			ClutState[0].NumberOfColors = Param8(0) * 8;
			//Console.WriteLine("{0:X}", GpuState[0].TextureMappingState.ClutState.Address);
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
			ClutState[0].PixelFormat = Extract<GuPixelFormats>(0, 2);
			ClutState[0].Shift = (int)Extract(2, 5);
			ClutState[0].Mask = (int)Extract(8, 8);
			ClutState[0].Start = (int)Extract(16, 5);
		}
	}
}
