using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.State.SubStates;

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
		public void OP_DMC()
		{
			GpuState[0].LightingState.DiffuseModelColor.SetRGB_A1(Params24);
		}

		// Specular Model Color
		public void OP_SMC()
		{
			GpuState[0].LightingState.SpecularModelColor.SetRGB_A1(Params24);
		}

		// Emissive Model Color
		public void OP_EMC()
		{
			GpuState[0].LightingState.EmissiveModelColor.SetRGB_A1(Params24);
		}

		// Ambient Model Color/Alpha
		// When lighting is off, this is like glColor*
		public void OP_AMC()
		{
			GpuState[0].LightingState.AmbientModelColor.SetRGB(Params24);
		}
		public void OP_AMA()
		{
			GpuState[0].LightingState.AmbientModelColor.SetA(Params24);
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
		public void OP_CMAT()
		{
			GpuState[0].LightingState.MaterialColorComponents = (LightComponentsSet)BitUtils.Extract(Params24, 0, 8);
		}

		// Alpha Blend Enable (GU_BLEND)
		public void OP_ABE()
		{
			GpuState[0].BlendingState.Enabled = Bool1;
			//Console.WriteLine("BLEND! : " + Bool1 + ", " + Params24);
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
		public void OP_ALPHA()
		{
			GpuState[0].BlendingState.FunctionSource = (GuBlendingFactorSource)((Params24 >> 0) & 0xF);
			GpuState[0].BlendingState.FunctionDestination = (GuBlendingFactorDestination)((Params24 >> 4) & 0xF);
			GpuState[0].BlendingState.Equation = (BlendingOpEnum)((Params24 >> 8) & 0xF);
			/*
			Console.WriteLine(
				"Alpha! : {0}, {1}, {2}",
				GpuState[0].BlendingState.FunctionSource,
				GpuState[0].BlendingState.FunctionDestination,
				GpuState[0].BlendingState.Equation
			);
			*/
		}

		// source fix color
		public void OP_SFIX()
		{
			GpuState[0].BlendingState.FixColorSource.SetRGB_A1(Params24);
		}

		// destination fix color
		public void OP_DFIX()
		{
			GpuState[0].BlendingState.FixColorDestination.SetRGB_A1(Params24);
		}

		/**
		 * Set mask for which bits of the pixels to write
		 *
		 * @param mask - Which bits to filter against writes
		 **/
		// void sceGuPixelMask(unsigned int mask);

		// Pixel MasK Color
		public void OP_PMSKC()
		{
			GpuState[0].BlendingState.ColorMaskR = Param8(0);
			GpuState[0].BlendingState.ColorMaskG = Param8(8);
			GpuState[0].BlendingState.ColorMaskB = Param8(16);
		}
		// Pixel MasK Alpha
		public void OP_PMSKA()
		{
			GpuState[0].BlendingState.ColorMaskA = Param8(0);
		}

	}
}
