using System;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu.Run
{
	public unsafe sealed partial class GpuDisplayListRunner
	{
		//string LightArrayOperation(string type, string code, int step = 1) { return ArrayOperation(type, 0, 3, code, step); }
		//string LightArrayOperationStep3(string type, string code) { return LightArrayOperation(type, code, 3); }

		/// <summary>
		/// Lighting Test Enable GL_LIGHTING.
		/// </summary>
		public void OP_LTE() {
			GpuState->LightingState.Enabled = Bool1;
		}
	
		/// <summary>
		/// Ambient Light Color
		/// </summary>
		public void OP_ALC()
		{
			GpuState->LightingState.AmbientLightColor.SetRGB(Params24);
		}

		/// <summary>
		/// Ambient Light Alpha
		/// </summary>
		public void OP_ALA()
		{
			GpuState->LightingState.AmbientLightColor.SetA(Params24);
		}

		/**
		 * Set light mode
		 *
		 * Available light modes are:
		 *   - GU_SINGLE_COLOR
		 *   - GU_SEPARATE_SPECULAR_COLOR
		 *
		 * Separate specular colors are used to interpolate the specular component
		 * independently, so that it can be added to the fragment after the texture color.
		 *
		 * @param mode - Light mode to use
		 **/
		// void sceGuLightMode(int mode);
		
		/// <summary>
		/// Light MODE (global)
		/// </summary>
		public void OP_LMODE()
		{
			GpuState->LightingState.LightModel = (LightModelEnum)Param8(0);
		}

		LightStateStruct *GetLigth(int Index)
		{
			return &((&GpuState->LightingState.Light0)[Index]);
		}

		private void _OP_LTE(int Index)
		{
			GetLigth(Index)->Enabled = Bool1;
		}

		//mixin(LightArrayOperation("OP_LTE_n", q{ gpu.state.lighting.lights[Index].enabled = command.bool1; }));
		public void OP_LTE0() { _OP_LTE(0); }
		public void OP_LTE1() { _OP_LTE(1); }
		public void OP_LTE2() { _OP_LTE(2); }
		public void OP_LTE3() { _OP_LTE(3); }

		// LighT Enable (per light)

		/**
		 * Set light parameters
		 *
		 * Available light types are:
		 *   - GU_DIRECTIONAL - Directional light
		 *   - GU_POINTLIGHT  - Single point of light
		 *   - GU_SPOTLIGHT   - Point-light with a cone
		 *
		 * Available light components are:
		 *   - GU_AMBIENT_AND_DIFFUSE
		 *   - GU_DIFFUSE_AND_SPECULAR
		 *   - GU_UNKNOWN_LIGHT_COMPONENT
		 *
		 * @param light      - Light index
		 * @param type       - Light type
		 * @param components - Light components
		 * @param position   - Light position
		 **/
		// void sceGuLight(int light, int type, int components, const ScePspFVector3* position); // OP_LXP_n + OP_LYP_n + OP_LZP_n + OP_LT_n

		// gpu.state.lighting.lights[Index].position.x = command.float1;

		private void _OP_LXP(int Index) { GetLigth(Index)->Position.X = Float1; }
		private void _OP_LYP(int Index) { GetLigth(Index)->Position.Y = Float1; }
		private void _OP_LZP(int Index) { GetLigth(Index)->Position.Z = Float1; }

		public void OP_LXP0() { _OP_LXP(0); }
		public void OP_LXP1() { _OP_LXP(1); }
		public void OP_LXP2() { _OP_LXP(2); }
		public void OP_LXP3() { _OP_LXP(3); }

		// gpu.state.lighting.lights[Index].position.y = command.float1;
		public void OP_LYP0() { _OP_LYP(0); }
		public void OP_LYP1() { _OP_LYP(1); }
		public void OP_LYP2() { _OP_LYP(2); }
		public void OP_LYP3() { _OP_LYP(3); }

		// gpu.state.lighting.lights[Index].position.z = command.float1;
		public void OP_LZP0() { _OP_LZP(0); }
		public void OP_LZP1() { _OP_LZP(1); }
		public void OP_LZP2() { _OP_LZP(2); }
		public void OP_LZP3() { _OP_LZP(3); }

		// Light Type (per light)
		/*
		mixin(LightArrayOperation("OP_LT_n" , q{
			with (gpu.state.lighting.lights[Index]) {
				type = command.extractEnum!(LightType, 8);
				kind = command.extractEnum!(LightModel, 0);
				switch (type) {
					case LightType.GU_DIRECTIONAL:
						position.t = 0.0;
					break;
					case LightType.GU_POINTLIGHT:
						position.t = 1.0;
						spotCutoff = 180;
					break;
					case LightType.GU_SPOTLIGHT:
						position.t = 1.0;
					break;
					default:
						throw(new Exception("Unexpected LightType"));
				}
			}
		}));
		*/

		private void _OP_LT(int Index)
		{
			GetLigth(Index)->Kind = (LightModelEnum)Param8(0);
			GetLigth(Index)->Type = (LightTypeEnum)Param8(8);
			switch (GetLigth(Index)->Type)
			{
				case LightTypeEnum.Directional:
					GetLigth(Index)->Position.W = 0;
					break;
				case LightTypeEnum.PointLight:
					GetLigth(Index)->Position.W = 1;
					GetLigth(Index)->SpotCutoff = 180;
					break;
				case LightTypeEnum.SpotLight:
					GetLigth(Index)->Position.W = 1;
					break;
				default:
					throw(new NotImplementedException());
			}
		}

		public void OP_LT0() { _OP_LT(0); }
		public void OP_LT1() { _OP_LT(1); }
		public void OP_LT2() { _OP_LT(2); }
		public void OP_LT3() { _OP_LT(3); }


		/**
		 * Set light attenuation
		 *
		 * @param light  - Light index
		 * @param atten0 - Constant attenuation factor
		 * @param atten1 - Linear attenuation factor
		 * @param atten2 - Quadratic attenuation factor
		 **/
		// void sceGuLightAtt(int light, float atten0, float atten1, float atten2); // OP_LCA_n + OP_LLA_n + OP_LQA_n
		// Light Constant/Linear/Quadratic Attenuation (per light)
		//mixin(LightArrayOperationStep3("OP_LCA_n", q{ gpu.state.lighting.lights[Index].attenuation.constant  = command.float1; }));
		//mixin(LightArrayOperationStep3("OP_LLA_n", q{ gpu.state.lighting.lights[Index].attenuation.linear    = command.float1; }));
		//mixin(LightArrayOperationStep3("OP_LQA_n", q{ gpu.state.lighting.lights[Index].attenuation.quadratic = command.float1; }));

		private void _OP_LCA(int Index) { GetLigth(Index)->Attenuation.Constant = Float1; }
		private void _OP_LLA(int Index) { GetLigth(Index)->Attenuation.Linear = Float1; }
		private void _OP_LQA(int Index) { GetLigth(Index)->Attenuation.Quadratic = Float1; }

		public void OP_LCA0() { _OP_LCA(0); }
		public void OP_LCA1() { _OP_LCA(1); }
		public void OP_LCA2() { _OP_LCA(2); }
		public void OP_LCA3() { _OP_LCA(3); }

		public void OP_LLA0() { _OP_LLA(0); }
		public void OP_LLA1() { _OP_LLA(1); }
		public void OP_LLA2() { _OP_LLA(2); }
		public void OP_LLA3() { _OP_LLA(3); }

		public void OP_LQA0() { _OP_LQA(0); }
		public void OP_LQA1() { _OP_LQA(1); }
		public void OP_LQA2() { _OP_LQA(2); }
		public void OP_LQA3() { _OP_LQA(3); }

		/**
		 * Set spotlight parameters
		 *
		 * @param light     - Light index
		 * @param direction - Spotlight direction
		 * @param exponent  - Spotlight exponent
		 * @param cutoff    - Spotlight cutoff angle (in radians)
		 **/
		// void sceGuLightSpot(int light, const ScePspFVector3* direction, float exponent, float cutoff); // OP_SPOTEXP_n + OP_SPOTCUT_n + OP_LXD_n + OP_LYD_n + OP_LZD_n

		// spot Light Direction (X, Y, Z) (per light)
		//mixin(LightArrayOperationStep3("OP_LXD_n", q{ gpu.state.lighting.lights[Index].spotDirection.x = command.float1; }));
		//mixin(LightArrayOperationStep3("OP_LYD_n", q{ gpu.state.lighting.lights[Index].spotDirection.y = command.float1; }));
		//mixin(LightArrayOperationStep3("OP_LZD_n", q{ gpu.state.lighting.lights[Index].spotDirection.z = command.float1; }));

		private void _OP_LXD(int Index) { GetLigth(Index)->SpotDirection.X = Float1; }
		private void _OP_LYD(int Index) { GetLigth(Index)->SpotDirection.Y = Float1; }
		private void _OP_LZD(int Index) { GetLigth(Index)->SpotDirection.Z = Float1; }

		public void OP_LXD0() { _OP_LXD(0); }
		public void OP_LXD1() { _OP_LXD(1); }
		public void OP_LXD2() { _OP_LXD(2); }
		public void OP_LXD3() { _OP_LXD(3); }

		public void OP_LYD0() { _OP_LYD(0); }
		public void OP_LYD1() { _OP_LYD(1); }
		public void OP_LYD2() { _OP_LYD(2); }
		public void OP_LYD3() { _OP_LYD(3); }

		public void OP_LZD0() { _OP_LZD(0); }
		public void OP_LZD1() { _OP_LZD(1); }
		public void OP_LZD2() { _OP_LZD(2); }
		public void OP_LZD3() { _OP_LZD(3); }


		// SPOT light EXPonent/CUToff (per light)
		//mixin(LightArrayOperation("OP_SPOTEXP_n", q{ gpu.state.lighting.lights[Index].spotExponent = command.float1; }));
		//mixin(LightArrayOperation("OP_SPOTCUT_n", q{ gpu.state.lighting.lights[Index].spotCutoff   = command.float1; }));

		private void _OP_SPOTEXP(int Index) { GetLigth(Index)->SpotExponent = Float1; }
		private void _OP_SPOTCUT(int Index) { GetLigth(Index)->SpotCutoff = Float1; }

		public void OP_SPOTEXP0() { _OP_SPOTEXP(0); }
		public void OP_SPOTEXP1() { _OP_SPOTEXP(1); }
		public void OP_SPOTEXP2() { _OP_SPOTEXP(2); }
		public void OP_SPOTEXP3() { _OP_SPOTEXP(3); }

		public void OP_SPOTCUT0() { _OP_SPOTCUT(0); }
		public void OP_SPOTCUT1() { _OP_SPOTCUT(1); }
		public void OP_SPOTCUT2() { _OP_SPOTCUT(2); }
		public void OP_SPOTCUT3() { _OP_SPOTCUT(3); }

		/**
		 * Set light color
		 *
		 * Available light components are:
		 *   - GU_AMBIENT
		 *   - GU_DIFFUSE
		 *   - GU_SPECULAR
		 *   - GU_AMBIENT_AND_DIFFUSE
		 *   - GU_DIFFUSE_AND_SPECULAR
		 *
		 * @param light     - Light index
		 * @param component - Which component to set
		 * @param color     - Which color to use
		 **/
		// void sceGuLightColor(int light, int component, unsigned int color); // OP_ALC_n + OP_DLC_n + OP_SLC_n

		// Ambient/Diffuse/Specular Light Color (per light)
		//mixin(LightArrayOperationStep3("OP_ALC_n", q{ gpu.state.lighting.lights[Index].ambientColor.rgba[]  = command.float4[]; }));
		//mixin(LightArrayOperationStep3("OP_DLC_n", q{ gpu.state.lighting.lights[Index].diffuseColor.rgba[]  = command.float4[]; }));
		//mixin(LightArrayOperationStep3("OP_SLC_n", q{ gpu.state.lighting.lights[Index].specularColor.rgba[] = command.float4[]; }));

		private void _OP_ALC(int Index) { GetLigth(Index)->AmbientColor.SetRGB_A1(Params24); }
		private void _OP_DLC(int Index) { GetLigth(Index)->DiffuseColor.SetRGB_A1(Params24); }
		private void _OP_SLC(int Index) { GetLigth(Index)->SpecularColor.SetRGB_A1(Params24); }

		public void OP_ALC0() { _OP_ALC(0); }
		public void OP_ALC1() { _OP_ALC(1); }
		public void OP_ALC2() { _OP_ALC(2); }
		public void OP_ALC3() { _OP_ALC(3); }

		public void OP_DLC0() { _OP_DLC(0); }
		public void OP_DLC1() { _OP_DLC(1); }
		public void OP_DLC2() { _OP_DLC(2); }
		public void OP_DLC3() { _OP_DLC(3); }

		public void OP_SLC0() { _OP_SLC(0); }
		public void OP_SLC1() { _OP_SLC(1); }
		public void OP_SLC2() { _OP_SLC(2); }
		public void OP_SLC3() { _OP_SLC(3); }

		/**
		 * Set the specular power for the material
		 *
		 * @param power - Specular power
		 **/
		// void sceGuSpecular(float power); // OP_SPOW
		// Specular POWer (global)
		// gpu.state.lighting.specularPower = command.float1;
		public void OP_SPOW() { GpuState->LightingState.SpecularPower = Float1; }
	}
}
