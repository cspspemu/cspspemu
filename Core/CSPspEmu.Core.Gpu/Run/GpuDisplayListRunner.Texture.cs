using System;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSharpUtils;
using CSPspEmu.Core.Types;

namespace CSPspEmu.Core.Gpu.Run
{
    public unsafe sealed partial class GpuDisplayListRunner
    {
        //static pure string TextureArrayOperation(string type, string code) { return ArrayOperation(type, 0, 7, code); }

        private TextureStateStruct* TextureState
        {
            get { return &GpuState->TextureMappingState.TextureState; }
        }

        /// <summary>
        /// Texture Mapping Enable (GL_TEXTURE_2D)
        /// </summary>
        public void OP_TME()
        {
            GpuState->TextureMappingState.Enabled = Bool1;
        }

        public void OP_TMS()
        {
            GpuState->TextureMappingState.Matrix.Reset();
        }

        public void OP_TMATRIX()
        {
            GpuState->TextureMappingState.Matrix.Write(Float1);
        }

        /**
         * Set texture-mode parameters
         *
         * Available texture-formats are:
         *   - GU_PSM_5650 - Hicolor, 16-bit
         *   - GU_PSM_5551 - Hicolor, 16-bit
         *   - GU_PSM_4444 - Hicolor, 16-bit
         *   - GU_PSM_8888 - Truecolor, 32-bit
         *   - GU_PSM_T4   - Indexed, 4-bit (2 pixels per byte)
         *   - GU_PSM_T8   - Indexed, 8-bit
         *   - GU_PSM_T16  - Indexed, 16-bit
         *   - GU_PSM_T32  - Indexed, 32-bit
         *   - GU_PSM_DXT1 - 
         *   - GU_PSM_DXT3 -
         *   - GU_PSM_DXT5 -
         *
         * @param tpsm    - Which texture format to use
         * @param maxmips - Number of mipmaps to use (0-8)
         * @param a2      - Unknown, set to 0
         * @param swizzle - GU_TRUE(1) to swizzle texture-reads
         **/
        // void sceGuTexMode(int tpsm, int maxmips, int a2, int swizzle);

        /// <summary>
        /// Texture Mode
        /// </summary>
        public void OP_TMODE()
        {
            TextureState->Swizzled = (Param8(0) != 0);
            TextureState->MipmapShareClut = (Param8(8) != 0);
            TextureState->MipmapMaxLevel = (int) Param8(16);
        }

        // Texture Pixel Storage Mode
        public void OP_TPSM()
        {
            TextureState->PixelFormat = (GuPixelFormats) Extract(0, 4);
        }

        /**
         * Set current texturemap
         *
         * Textures may reside in main RAM, but it has a huge speed-penalty. Swizzle textures
         * to get maximum speed.
         *
         * @note Data must be aligned to 1 quad word (16 bytes)
         *
         * @param mipmap - Mipmap level
         * @param width  - Width of texture (must be a power of 2)
         * @param height - Height of texture (must be a power of 2)
         * @param tbw    - Texture Buffer Width (block-aligned)
         * @param tbp    - Texture buffer pointer (16 byte aligned)
         **/
        // void sceGuTexImage(int mipmap, int width, int height, int tbw, const void* tbp); // OP_TBP_n, OP_TBW_n, OP_TSIZE_n, OP_TFLUSH

        // TextureMipmap Base Pointer

        private TextureStateStruct.MipmapState* MipMapState(int Index)
        {
            return &(&TextureState->Mipmap0)[Index];
        }

        /// <summary>
        /// TextureMipmap Buffer Pointer.
        /// </summary>
        private void _OP_TBP(int Index)
        {
            var MipMap = MipMapState(Index);
            MipMap->Address = (MipMap->Address & 0xFF000000) | (Params24 & 0x00FFFFFF);
        }

        /// <summary>
        /// TextureMipmap Buffer Width.
        /// </summary>
        private void _OP_TBW(int Index)
        {
            var MipMap = MipMapState(Index);
            MipMap->BufferWidth = Param16(0);
            MipMap->Address = (MipMap->Address & 0x00FFFFFF) | ((uint) (Param8(16) << 24) & 0xFF000000);
        }

        public void OP_TBP0()
        {
            _OP_TBP(0);
        }

        public void OP_TBP1()
        {
            _OP_TBP(1);
        }

        public void OP_TBP2()
        {
            _OP_TBP(2);
        }

        public void OP_TBP3()
        {
            _OP_TBP(3);
        }

        public void OP_TBP4()
        {
            _OP_TBP(4);
        }

        public void OP_TBP5()
        {
            _OP_TBP(5);
        }

        public void OP_TBP6()
        {
            _OP_TBP(6);
        }

        public void OP_TBP7()
        {
            _OP_TBP(7);
        }

        public void OP_TBW0()
        {
            _OP_TBW(0);
        }

        public void OP_TBW1()
        {
            _OP_TBW(1);
        }

        public void OP_TBW2()
        {
            _OP_TBW(2);
        }

        public void OP_TBW3()
        {
            _OP_TBW(3);
        }

        public void OP_TBW4()
        {
            _OP_TBW(4);
        }

        public void OP_TBW5()
        {
            _OP_TBW(5);
        }

        public void OP_TBW6()
        {
            _OP_TBW(6);
        }

        public void OP_TBW7()
        {
            _OP_TBW(7);
        }

        /// <summary>
        /// TextureMipmap Size
        /// </summary>
        /// <param name="Index"></param>
        private void _OP_TSIZE(int Index)
        {
            // Astonishia Story is using normalArgument = 0x1804
            // -> use texture_height = 1 << 0x08 (and not 1 << 0x18)
            //        texture_width  = 1 << 0x04
            // The maximum texture size is 512x512: the exponent value must be [0..9]
            // Maybe a bit flag for something?

            var MipMap = MipMapState(Index);
            var WidthExp = (int) BitUtils.Extract(Params24, 0, 4);
            var HeightExp = (int) BitUtils.Extract(Params24, 8, 4);
            bool UnknownFlag = BitUtils.Extract(Params24, 15, 1) != 0;
            if (UnknownFlag)
            {
                Console.Error.WriteLine("_OP_TSIZE UnknownFlag : 0x{0:X}", Params24);
            }
            WidthExp = Math.Min(WidthExp, 9);
            HeightExp = Math.Min(HeightExp, 9);

            MipMap->TextureWidth = (ushort) (1 << WidthExp);
            MipMap->TextureHeight = (ushort) (1 << HeightExp);
        }

        public void OP_TSIZE0()
        {
            _OP_TSIZE(0);
        }

        public void OP_TSIZE1()
        {
            _OP_TSIZE(1);
        }

        public void OP_TSIZE2()
        {
            _OP_TSIZE(2);
        }

        public void OP_TSIZE3()
        {
            _OP_TSIZE(3);
        }

        public void OP_TSIZE4()
        {
            _OP_TSIZE(4);
        }

        public void OP_TSIZE5()
        {
            _OP_TSIZE(5);
        }

        public void OP_TSIZE6()
        {
            _OP_TSIZE(6);
        }

        public void OP_TSIZE7()
        {
            _OP_TSIZE(7);
        }


        /// <summary>
        /// Texture Flush. Flush texture page-cache
        /// 
        /// Do this if you have copied/rendered into an area currently in the texture-cache
        /// </summary>
        /// <remarks>
        /// NOTE: 'sceGuTexImage' and 'sceGuTexMode' calls TFLUSH.
        /// </remarks>
        /// <see cref="void sceGuTexFlush(void); // OP_TFLUSH"/>
        public void OP_TFLUSH()
        {
            GpuDisplayList.GpuProcessor.GpuImpl.TextureFlush(GpuState);
        }

        /**
         * Synchronize rendering pipeline with image upload.
         *
         * This will stall the rendering pipeline until the current image upload initiated by
         * sceGuCopyImage() has completed.
         **/
        // void sceGuTexSync(); // OP_TSYNC
        //
        // http://forums.ps2dev.org/viewtopic.php?t=6304
        // SceGuTexSync() is needed when you upload a texture to VRAM and part of that memory is still in texture cache
        // (which you won't know until you get some wrong texture artifacts). So just call it after each sceGuCopyImage and you're fine. 

        /// <summary>
        /// Texture Sync
        /// </summary>
        public void OP_TSYNC()
        {
            GpuDisplayList.GpuProcessor.GpuImpl.TextureSync(GpuState);
        }

        /**
         * Set how the texture is filtered
         *
         * Available filters are:
         *   - GU_NEAREST
         *   - GU_LINEAR
         *   - GU_NEAREST_MIPMAP_NEAREST
         *   - GU_LINEAR_MIPMAP_NEAREST
         *   - GU_NEAREST_MIPMAP_LINEAR
         *   - GU_LINEAR_MIPMAP_LINEAR
         *
         * @param min - Minimizing filter
         * @param mag - Magnifying filter
         **/
        // void sceGuTexFilter(int min, int mag); // OP_TFLT

        /// <summary>
        /// Texture FiLTer
        /// </summary>
        public void OP_TFLT()
        {
            TextureState->FilterMinification = (TextureFilter) Param8(0);
            TextureState->FilterMagnification = (TextureFilter) Param8(8);
        }

        /**
         * Set if the texture should repeat or clamp
         *
         * Available modes are:
         *   - GU_REPEAT - The texture repeats after crossing the border
         *   - GU_CLAMP - Texture clamps at the border
         *
         * @param u - Wrap-mode for the U direction
         * @param v - Wrap-mode for the V direction
         **/
        // void sceGuTexWrap(int u, int v); // OP_TWRAP

        /// <summary>
        /// Texture WRAP
        /// </summary>
        public void OP_TWRAP()
        {
            TextureState->WrapU = (WrapMode) Param8(0);
            TextureState->WrapV = (WrapMode) Param8(8);
        }

        /**
         * Set how textures are applied
         *
         * Key for the apply-modes:
         *   - Cv - Color value result
         *   - Ct - Texture color
         *   - Cf - Fragment color
         *   - Cc - Constant color (specified by sceGuTexEnvColor())
         *
         * Available apply-modes are: (TFX)
         *   - GU_TFX_MODULATE - Cv=Ct*Cf TCC_RGB: Av=Af TCC_RGBA: Av=At*Af
         *   - GU_TFX_DECAL    - TCC_RGB: Cv=Ct,Av=Af TCC_RGBA: Cv=Cf*(1-At)+Ct*At Av=Af
         *   - GU_TFX_BLEND    - Cv=(Cf*(1-Ct))+(Cc*Ct) TCC_RGB: Av=Af TCC_RGBA: Av=At*Af
         *   - GU_TFX_REPLACE  - Cv=Ct TCC_RGB: Av=Af TCC_RGBA: Av=At
         *   - GU_TFX_ADD      - Cv=Cf+Ct TCC_RGB: Av=Af TCC_RGBA: Av=At*Af
         *
         * The fields TCC_RGB and TCC_RGBA specify components that differ between
         * the two different component modes.
         *
         *   - GU_TFX_MODULATE - The texture is multiplied with the current diffuse fragment
         *   - GU_TFX_REPLACE  - The texture replaces the fragment
         *   - GU_TFX_ADD      - The texture is added on-top of the diffuse fragment
         *   
         * Available component-modes are: (TCC)
         *   - GU_TCC_RGB  - The texture alpha does not have any effect
         *   - GU_TCC_RGBA - The texture alpha is taken into account
         *
         * @param tfx - Which apply-mode to use
         * @param tcc - Which component-mode to use
        **/
        // void sceGuTexFunc(int tfx, int tcc); // OP_TFUNC

        /// <summary>
        /// Texture enviroment Mode
        /// </summary>
        public void OP_TFUNC()
        {
            TextureState->Effect = (TextureEffect) Param8(0);
            TextureState->ColorComponent = (TextureColorComponent) Param8(8);
            TextureState->Fragment2X = (Param8(16) != 0);

            //Console.WriteLine(TextureState->Effect);
        }

        /**
         * Set texture scale
         *
         * @note Only used by the 3D T&L pipe, renders done with GU_TRANSFORM_2D are
         * not affected by this.
         *
         * @param u - Scalar to multiply U coordinate with
         * @param v - Scalar to multiply V coordinate with
         **/
        // void sceGuTexScale(float u, float v);

        /// <summary>
        /// UV SCALE
        /// </summary>
        public void OP_USCALE()
        {
            GpuState->TextureMappingState.TextureState.ScaleU = Float1;
        }

        public void OP_VSCALE()
        {
            GpuState->TextureMappingState.TextureState.ScaleV = Float1;
        }

        /**
         * Set texture offset
         *
         * @note Only used by the 3D T&L pipe, renders done with GU_TRANSFORM_2D are
         * not affected by this.
         *
         * @param u - Offset to add to the U coordinate
         * @param v - Offset to add to the V coordinate
         **/
        // void sceGuTexOffset(float u, float v);

        // UV OFFSET
        public void OP_UOFFSET()
        {
            GpuState->TextureMappingState.TextureState.OffsetU = Float1;
        }

        public void OP_VOFFSET()
        {
            GpuState->TextureMappingState.TextureState.OffsetV = Float1;
        }

        /**
         * Specify the texture environment color
         *
         * This is used in the texture function when a constant color is needed.
         *
         * See sceGuTexFunc() for more information.
         *
         * @param color - Constant color (0x00BBGGRR)
         **/
        // void sceGuTexEnvColor(unsigned int color); // OP_TEC

        /// <summary>
        /// Texture Environment Color
        /// </summary>
        public void OP_TEC()
        {
            GpuState->TextureMappingState.TextureEnviromentColor.SetRGB_A1(Params24);
        }

        public void OP_TEXTURE_ENV_MAP_MATRIX()
        {
            GpuState->TextureMappingState.ShadeU = (short) BitUtils.Extract(Params24, 0, 2);
            GpuState->TextureMappingState.ShadeV = (short) BitUtils.Extract(Params24, 8, 2);
        }

        public void OP_TMAP()
        {
            GpuState->TextureMappingState.TextureMapMode = (TextureMapMode) Param8(0);
            GpuState->TextureMappingState.TextureProjectionMapMode = (TextureProjectionMapMode) Param8(8);
            GpuState->VertexState.Type.NormalCount = GpuState->TextureMappingState.GetTextureComponentsCount();
        }

        public void OP_TBIAS()
        {
            GpuState->TextureMappingState.LevelMode = (TextureLevelMode) Param8(0);
            GpuState->TextureMappingState.MipmapBias = ((float) Param8(16)) / 16.0f;
        }

        public void OP_TSLOPE()
        {
            GpuState->TextureMappingState.SlopeLevel = Float1;
        }
    }
}