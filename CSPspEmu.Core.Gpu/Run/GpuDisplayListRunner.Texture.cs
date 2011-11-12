using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		//static pure string TextureArrayOperation(string type, string code) { return ArrayOperation(type, 0, 7, code); }

		// Texture Mapping Enable (GL_TEXTURE_2D)
		[GpuOpCodesNotImplemented]
		public void OP_TME()
		{
			GpuDisplayList.GpuStateStructPointer[0].TextureMappingState.Enabled = Bool1;
		}

		public void OP_TMS()
		{
			GpuDisplayList.GpuStateStructPointer[0].TextureMappingState.Matrix.Reset();
		}
		public void OP_TMATRIX()
		{
			GpuDisplayList.GpuStateStructPointer[0].TextureMappingState.Matrix.Write(Float1);
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

		// Texture Mode
		[GpuOpCodesNotImplemented]
		public void OP_TMODE()
		{
			/*
			with (gpu.state.texture) {
				swizzled        =  command.extract!(bool,  0, 8);
				mipmapShareClut = !command.extract!(bool,  8, 8);
				mipmapMaxLevel  =  command.extract!(ubyte, 16, 8);
			}
			*/
		
			//writefln("%d, %d", gpu.state.texture.mipmapShareClut, gpu.state.texture.mipmapMaxLevel);
		}

		// Texture Pixel Storage Mode
		[GpuOpCodesNotImplemented]
		public void OP_TPSM()
		{
			//gpu.state.texture.format = command.extractEnum!(PixelFormats);
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
		/*
		mixin (TextureArrayOperation("OP_TBP_n", q{
			with (gpu.state.texture.mipmaps[Index]) {
				address &= 0xFF000000;
				address |= command.param24;
			}
		}));
		*/

		[GpuOpCodesNotImplemented]
		public void OP_TBP0() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBP1() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBP2() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBP3() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBP4() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBP5() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBP6() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBP7() { }

		// TextureMipmap Buffer Width.
		[GpuOpCodesNotImplemented]
		public void OP_TBW0() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBW1() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBW2() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBW3() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBW4() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBW5() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBW6() { }
		[GpuOpCodesNotImplemented]
		public void OP_TBW7() { }
		/*
		mixin (TextureArrayOperation("OP_TBW_n", q{
			with (gpu.state.texture.mipmaps[Index]) {
				buffer_width = command.extract!(uint, 0, 16); // ???
				address &= 0x00FFFFFF;
				address |= command.extract!(uint, 16, 8) << 24;
			}
		}));
		*/

		// TextureMipmap Size
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE0() { }
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE1() { }
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE2() { }
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE3() { }
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE4() { }
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE5() { }
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE6() { }
		[GpuOpCodesNotImplemented]
		public void OP_TSIZE7() { }
		/*
		mixin (TextureArrayOperation("OP_TSIZE_n", q{
			with (gpu.state.texture.mipmaps[Index]) {
				width  = 1 << command.extract!(uint, 0, 8);
				height = 1 << command.extract!(uint, 8, 8);
			}
		}));
		*/

		/**
		 * Flush texture page-cache
		 *
		 * Do this if you have copied/rendered into an area currently in the texture-cache
		**/
		// void sceGuTexFlush(void); // OP_TFLUSH

		// Texture Flush. NOTE: 'sceGuTexImage' and 'sceGuTexMode' calls TFLUSH.
		[GpuOpCodesNotImplemented]
		public void OP_TFLUSH()
		{
			//writefln("TFLUSH!");
			//gpu.impl.tflush();
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
		// SceGuTexSync() is needed when you upload a texture to VRAM and part of that memory is still in texture cash
		// (which you won't know until you get some wrong texture artifacts). So just call it after each sceGuCopyImage and you're fine. 

		// Texture Sync
		[GpuOpCodesNotImplemented]
		public void OP_TSYNC()
		{
			//writefln("TSYNC!");
			//gpu.impl.tsync();
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

		// Texture FiLTer
		[GpuOpCodesNotImplemented]
		public void OP_TFLT()
		{
			/*
			with (gpu.state.texture) {
				filterMin = command.extractEnum!(TextureFilter, 0); // only GL_NEAREST, GL_LINEAR (no mipmaps) (& 0b_1)
				filterMag = command.extractEnum!(TextureFilter, 8); // only GL_NEAREST, GL_LINEAR (no mipmaps) (& 0b_1)
			}
			*/
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

		// Texture WRAP
		[GpuOpCodesNotImplemented]
		public void OP_TWRAP()
		{
			/*
			with (gpu.state.texture) {
				wrapU = command.extractEnum!(WrapMode, 0);
				wrapV = command.extractEnum!(WrapMode, 8);
			}
			*/
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

		// Texture enviroment Mode
		[GpuOpCodesNotImplemented]
		public void OP_TFUNC()
		{
			/*
			with (gpu.state.texture) {
				effect         = command.extractEnum!(TextureEffect, 0);
				colorComponent = command.extractEnum!(TextureColorComponent, 8);
				fragment_2x    = command.extract!(bool, 16, 1); // ?
			}
			*/
		}

		/**
		 * Set texture scale
		 *
		 * @note Only used by the 3D T&L pipe, renders ton with GU_TRANSFORM_2D are
		 * not affected by this.
		 *
		 * @param u - Scalar to multiply U coordinate with
		 * @param v - Scalar to multiply V coordinate with
		 **/
		// void sceGuTexScale(float u, float v);

		// UV SCALE
		// gpu.state.texture.scale.u = command.float1; 
		[GpuOpCodesNotImplemented]
		public void OP_USCALE() { }
		// gpu.state.texture.scale.v = command.float1; 
		[GpuOpCodesNotImplemented]
		public void OP_VSCALE() { }

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
		// gpu.state.texture.offset.u = command.float1;
		[GpuOpCodesNotImplemented]
		public void OP_UOFFSET() { }
		// gpu.state.texture.offset.v = command.float1;
		[GpuOpCodesNotImplemented]
		public void OP_VOFFSET() { }

		[GpuOpCodesNotImplemented]
		public void OP_TEXTURE_ENV_MAP_MATRIX()
		{
			//gpu.state.texture.texShade[0] = command.extract!(int, 0, 8) & 3;
			//gpu.state.texture.texShade[1] = command.extract!(int, 8, 8) & 3;
		}

		[GpuOpCodesNotImplemented]
		public void OP_TMAP()
		{
			//gpu.state.texture.mapMode     = command.extractEnum!(TextureMapMode          , 0);
			//gpu.state.texture.projMapMode = command.extractEnum!(TextureProjectionMapMode, 8);
		}

		[GpuOpCodesNotImplemented]
		public void OP_TBIAS()
		{
			//gpu.state.texture.levelMode  = command.extractEnum!(TextureLevelMode, 0);
			//gpu.state.texture.mipmapBias = cast(float)command.extract!(int, 16, 8) / 16.0f;
		}
	}
}
