using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu.Run
{
    /// <summary>
    /// CLUT (Color LookUp Table) opcodes.
    /// </summary>
    public sealed unsafe partial class GpuDisplayListRunner
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
        ClutStateStruct* ClutState => &GpuState->TextureMappingState.ClutState;

        // Clut Buffer Pointer (High)
        // Clut LOAD
        public void OP_CBP() => ClutState->Address = (ClutState->Address & 0xFF000000) | ((Params24 << 0) & 0x00FFFFFF);

        public void OP_CBPH() => ClutState->Address = (ClutState->Address & 0x00FFFFFF) | ((Params24 << 8) & 0xFF000000);

        public void OP_CLOAD() => ClutState->NumberOfColors = Param8(0) * 8;

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
            ClutState->PixelFormat = (GuPixelFormats) Extract(0, 2);
            ClutState->Shift = (int) Extract(2, 5);
            ClutState->Mask = (int) Extract(8, 8);
            ClutState->Start = (int) Extract(16, 5);
        }
    }
}