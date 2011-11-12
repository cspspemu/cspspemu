using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.Run
{
	unsafe sealed public partial class GpuDisplayListRunner
	{
		/**
		 * Set current primitive color
		 *
		 * @param color - Which color to use (overriden by vertex-colors)
		 **/
		// void sceGuColor(unsigned int color); // sceGuMaterial(7, color); // OP_AMC + OP_AMA + OP_DMC + OP_SMC

		// void sceGuMaterial(int mode, int color); // if (mode & 1) { OP_AMC + OP_AMA } if (mode & 2) { OP_DMC } if (mode & 4) { OP_SMC }
		// void sceGuModelColor(unsigned int emissive, unsigned int ambient, unsigned int diffuse, unsigned int specular); // OP_EMC + OP_DMC + OP_AMC + OP_SMC
		// void sceGuAmbientColor(unsigned int color); // OP_AMC + OP_AMA
		// void sceGuAmbient(unsigned int color); // OP_ALC + OP_ALA

		// Diffuse Model Color
		[GpuOpCodesNotImplemented]
		public void OP_DMC()
		{
			//gpu.state.diffuseModelColor.rgba[] = command.float4[];
		}

		// Specular Model Color
		[GpuOpCodesNotImplemented]
		public void OP_SMC()
		{
			//gpu.state.specularModelColor.rgba[] = command.float4[];
		}

		// Emissive Model Color
		[GpuOpCodesNotImplemented]
		public void OP_EMC()
		{
			//gpu.state.emissiveModelColor.rgba[] = command.float4[];
		}

		// Ambient Model Color/Alpha
		// When lighting is off, this is like glColor*
		[GpuOpCodesNotImplemented]
		public void OP_AMC()
		{
			//gpu.state.ambientModelColor.rgb[] = command.float3[];
		}
		[GpuOpCodesNotImplemented]
		public void OP_AMA()
		{
			//gpu.state.ambientModelColor.alpha = command.float4[0];
		}

		/**
		 * Set which color components that the material will receive
		 *
		 * The components are ORed together from the following values:
		 *   - GU_AMBIENT
		 *   - GU_DIFFUSE
		 *   - GU_SPECULAR
		 *
		 * @param components - Which components to receive
		 **/
		// void sceGuColorMaterial(int components); // OP_CMAT
		// Material Color
		[GpuOpCodesNotImplemented]
		public void OP_CMAT()
		{
			//gpu.state.materialColorComponents = command.extractSet!(LightComponents);
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
		// Texture Environment Color
		[GpuOpCodesNotImplemented]
		public void OP_TEC()
		{
			//gpu.state.textureEnviromentColor.rgb[] = command.float3[]; gpu.state.textureEnviromentColor.a = 1.0;
		}

		// Alpha Blend Enable (GU_BLEND)
		[GpuOpCodesNotImplemented]
		public void OP_ABE()
		{
			//gpu.state.blend.enabled = command.bool1;
		}

		/**
		 * Set the blending-mode
		 *
		 * Keys for the blending operations:
		 *   - Cs - Source color
		 *   - Cd - Destination color
		 *   - Bs - Blend function for source fragment
		 *   - Bd - Blend function for destination fragment
		 *
		 * Available blending-operations are:
		 *   - GU_ADD              - (Cs*Bs) + (Cd*Bd)
		 *   - GU_SUBTRACT         - (Cs*Bs) - (Cd*Bd)
		 *   - GU_REVERSE_SUBTRACT - (Cd*Bd) - (Cs*Bs)
		 *   - GU_MIN              - Cs < Cd ? Cs : Cd
		 *   - GU_MAX              - Cs < Cd ? Cd : Cs
		 *   - GU_ABS              - |Cs-Cd|
		 *
		 * Available blending-functions are:
		 *   - GU_SRC_COLOR
		 *   - GU_ONE_MINUS_SRC_COLOR
		 *   - GU_SRC_ALPHA
		 *   - GU_ONE_MINUS_SRC_ALPHA
		 *   - GU_DST_ALPHA
		 *   - GU_ONE_MINUS_DST_ALPHA
		 *   - GU_DST_COLOR
		 *   - GU_ONE_MINUS_DST_COLOR
		 *   - GU_FIX
		 *
		 * @param op      - Blending Operation
		 * @param src     - Blending function for source operand
		 * @param dest    - Blending function for dest operand
		 * @param srcfix  - Fix value for GU_FIX (source operand)
		 * @param destfix - Fix value for GU_FIX (dest operand)
		 **/
		// void sceGuBlendFunc(int op, int src, int dest, unsigned int srcfix, unsigned int destfix);

		// Blend Equation and Functions
		[GpuOpCodesNotImplemented]
		public void OP_ALPHA()
		{
			/*
			with (gpu.state) {
				blend.funcSrc  = command.extractEnum!(BlendingFactor, 0);
				blend.funcDst  = command.extractEnum!(BlendingFactor, 4);
				blend.equation = command.extractEnum!(BlendingOp    , 8);
			}
			*/
		}

		// source fix color
		[GpuOpCodesNotImplemented]
		public void OP_SFIX()
		{
			//gpu.state.blend.fixColorSrc.rgb[] = command.float3[]; gpu.state.blend.fixColorSrc.a = 1.0;
		}

		// destination fix color
		[GpuOpCodesNotImplemented]
		public void OP_DFIX()
		{
			//gpu.state.blend.fixColorDst.rgb[] = command.float3[]; gpu.state.blend.fixColorDst.a = 1.0;
		}

		/**
		 * Set mask for which bits of the pixels to write
		 *
		 * @param mask - Which bits to filter against writes
		 **/
		// void sceGuPixelMask(unsigned int mask);

		// Pixel MasK Color
		[GpuOpCodesNotImplemented]
		public void OP_PMSKC()
		{
			/*
			gpu.state.colorMask[0] = command.extract!(ubyte,  0, 8);
			gpu.state.colorMask[1] = command.extract!(ubyte,  8, 8);
			gpu.state.colorMask[2] = command.extract!(ubyte, 16, 8);
			*/
		}
		// Pixel MasK Alpha
		[GpuOpCodesNotImplemented]
		public void OP_PMSKA()
		{
			//gpu.state.colorMask[3] = command.extract!(ubyte, 0, 8);
		}

	}
}
