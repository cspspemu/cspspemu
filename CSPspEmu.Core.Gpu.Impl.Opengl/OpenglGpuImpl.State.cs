using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.State.SubStates;
using OpenTK.Graphics.OpenGL;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public partial class OpenglGpuImpl
	{
		private void PrepareState(GpuStateStruct* GpuState)
		{
			PrepareState_CullFace(GpuState);
			PrepareState_Texture(GpuState);
			PrepareState_Colors(GpuState);
			PrepareState_Lighting(GpuState);
			PrepareState_Blend(GpuState);
			PrepareState_Depth(GpuState);
			PrepareState_DepthTest(GpuState);

			GL.ShadeModel((GpuState[0].ShadeModel == ShadingModelEnum.Flat) ? ShadingModel.Flat : ShadingModel.Smooth);
		}

		private void PrepareState_CullFace(GpuStateStruct* GpuState)
		{
			if (!GlEnableDisable(EnableCap.CullFace, GpuState[0].BackfaceCullingState.Enabled))
			{
				return;
			}

			GL.CullFace((GpuState[0].BackfaceCullingState.FrontFaceDirection == State.SubStates.FrontFaceDirectionEnum.ClockWise) ? CullFaceMode.Front : CullFaceMode.Back);
		}

		private void PrepareState_Depth(GpuStateStruct* GpuState)
		{
			GL.DepthRange(GpuState[0].DepthTestState.RangeFar, GpuState[0].DepthTestState.RangeNear);
		}

		static readonly DepthFunction[] TestTranslate = new DepthFunction[] {
			DepthFunction.Never,
			DepthFunction.Always,
			DepthFunction.Equal,
			DepthFunction.Notequal,
			DepthFunction.Less,
			DepthFunction.Lequal,
			DepthFunction.Greater, 
			DepthFunction.Gequal
		};

		private void PrepareState_DepthTest(GpuStateStruct* GpuState)
		{
			GL.DepthMask(GpuState[0].DepthTestState.Mask == 0);
			if (!GlEnableDisable(EnableCap.DepthTest, GpuState[0].DepthTestState.Enabled))
			{
				return;
			}
			//GL.DepthFunc(DepthFunction.Greater);
			GL.DepthFunc(TestTranslate[(int)GpuState[0].DepthTestState.Function]);
		}

		private void PrepareState_Colors(GpuStateStruct* GpuState)
		{
			GlEnableDisable(EnableCap.ColorMaterial, VertexType.Color != VertexTypeStruct.ColorEnum.Void);
	
			var Color = GpuState[0].LightingState.AmbientModelColor;
			GL.Color4(&Color.Red);
		
			if (VertexType.Color != VertexTypeStruct.ColorEnum.Void && GpuState[0].LightingState.Enabled)
			{
				ColorMaterialParameter flags = (ColorMaterialParameter)0;
				/*
				glMaterialfv(faces, GL_AMBIENT , [0.0f, 0.0f, 0.0f, 0.0f].ptr);
				glMaterialfv(faces, GL_DIFFUSE , [0.0f, 0.0f, 0.0f, 0.0f].ptr);
				glMaterialfv(faces, GL_SPECULAR, [0.0f, 0.0f, 0.0f, 0.0f].ptr);
				*/

				var MaterialColorComponents = GpuState[0].LightingState.MaterialColorComponents;

				if (MaterialColorComponents.HasFlag(LightComponentsSet.Ambient))
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, &GpuState[0].LightingState.AmbientModelColor.Red);
				}
				if (MaterialColorComponents.HasFlag(LightComponentsSet.Diffuse))
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, &GpuState[0].LightingState.DiffuseModelColor.Red);
				}
				if (MaterialColorComponents.HasFlag(LightComponentsSet.Specular))
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, &GpuState[0].LightingState.SpecularModelColor.Red);
				}

				if (MaterialColorComponents.HasFlag(LightComponentsSet.AmbientAndDiffuse))
				{
					flags = ColorMaterialParameter.AmbientAndDiffuse;
				}
				else if (MaterialColorComponents.HasFlag(LightComponentsSet.Ambient))
				{
					flags = ColorMaterialParameter.Ambient;
				}
				else if (MaterialColorComponents.HasFlag(LightComponentsSet.Diffuse))
				{
					flags = ColorMaterialParameter.Diffuse;
				}
				else if (MaterialColorComponents.HasFlag(LightComponentsSet.Specular))
				{
					flags = ColorMaterialParameter.Specular;
				}
				else
				{
					//throw (new NotImplementedException("Error! : " + MaterialColorComponents));
				}
				//flags = GL_SPECULAR;
				if (flags != 0)
				{
					GL.ColorMaterial(MaterialFace.FrontAndBack, flags);
				}
				//glEnable(GL_COLOR_MATERIAL);
			}

			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, &GpuState[0].LightingState.EmissiveModelColor.Red);
		}

		private void PrepareState_Lighting(GpuStateStruct* GpuState)
		{
			//Console.WriteLine(GpuState[0].LightingState.AmbientModelColor);
			var LightingState = &GpuState[0].LightingState;

			// if (!glEnableDisable(GL_LIGHTING, state.lighting.enabled) && (state.texture.mapMode != TextureMapMode.GU_ENVIRONMENT_MAP)) {
			if (!GlEnableDisable(EnableCap.Lighting, LightingState[0].Enabled))
			{
				return;
			}

			GL.LightModel(
				LightModelParameter.LightModelColorControl,
				(int)((LightingState[0].LightModel == LightModelEnum.SeparateSpecularColor) ? LightModelColorControl.SeparateSpecularColor : LightModelColorControl.SingleColor)
			);
			GL.LightModel(LightModelParameter.LightModelAmbient, &LightingState[0].AmbientLightColor.Red);

			for (int n = 0; n < 4; n++)
			{
				var LightState = &(&LightingState[0].Light0)[n];
				LightName LightName = (LightName)(LightName.Light0 + n);

				if (!GlEnableDisable((EnableCap)(EnableCap.Light0 + n), LightState[0].Enabled))
				{
					continue;
				}

				GL.Light(LightName, LightParameter.Specular, &LightState[0].SpecularColor.Red);
				GL.Light(LightName, LightParameter.Ambient, &LightState[0].AmbientColor.Red);
				GL.Light(LightName, LightParameter.Diffuse, &LightState[0].DiffuseColor.Red);

				LightState[0].Position.W = 1.0f;
				GL.Light(LightName, LightParameter.Position, &LightState[0].Position.X);

				GL.Light(LightName, LightParameter.ConstantAttenuation, &LightState[0].Attenuation.Constant);
				GL.Light(LightName, LightParameter.LinearAttenuation, &LightState[0].Attenuation.Linear);
				GL.Light(LightName, LightParameter.QuadraticAttenuation, &LightState[0].Attenuation.Quadratic);

				if (LightState[0].Type == LightTypeEnum.SpotLight)
				{
					GL.Light(LightName, LightParameter.SpotDirection, &LightState[0].SpotDirection.X);
					GL.Light(LightName, LightParameter.SpotExponent, &LightState[0].SpotExponent);
					GL.Light(LightName, LightParameter.SpotCutoff, &LightState[0].SpotCutoff);
				}
				else
				{
					GL.Light(LightName, LightParameter.SpotExponent, 0);
					GL.Light(LightName, LightParameter.SpotCutoff, 180);
				}
			}
		}

		static readonly BlendEquationMode[] BlendEquationTranslate = new BlendEquationMode[]
		{
			BlendEquationMode.FuncAdd,
			BlendEquationMode.FuncSubtract,
			BlendEquationMode.FuncReverseSubtract,
			BlendEquationMode.Min,
			BlendEquationMode.Max,
			BlendEquationMode.FuncAdd, /* ABS */
		};

		const int GL_ZERO = 0x0;
		const int GL_ONE                                 = 0x1;
		const int GL_SRC_COLOR                           = 0x0300;
		const int GL_ONE_MINUS_SRC_COLOR                 = 0x0301;
		const int GL_SRC_ALPHA                           = 0x0302;
		const int GL_ONE_MINUS_SRC_ALPHA                 = 0x0303;
		const int GL_DST_ALPHA                           = 0x0304;
		const int GL_ONE_MINUS_DST_ALPHA                 = 0x0305;
		const int GL_DST_COLOR                           = 0x0306;
		const int GL_ONE_MINUS_DST_COLOR                 = 0x0307;
		const int GL_SRC_ALPHA_SATURATE                  = 0x0308;
		const int GL_CONSTANT_COLOR = 0x8001;
		const int GL_ONE_MINUS_CONSTANT_COLOR = 0x8002;
		
		static readonly BlendingFactorSrc[] BlendFuncSrcTranslate = new BlendingFactorSrc[] {
			/// 0 GL_SRC_COLOR,
			//BlendingFactorSrc.DstColor,
			(BlendingFactorSrc)GL_SRC_COLOR,
			/// 1 GL_ONE_MINUS_SRC_COLOR,
			//BlendingFactorSrc.OneMinusDstColor,
			(BlendingFactorSrc)GL_ONE_MINUS_SRC_COLOR,
			/// 2 GL_SRC_ALPHA,
			//BlendingFactorSrc.DstAlpha,
			(BlendingFactorSrc)GL_SRC_ALPHA,
			/// 3 GL_ONE_MINUS_SRC_ALPHA,
			//BlendingFactorSrc.OneMinusDstAlpha,
			(BlendingFactorSrc)GL_ONE_MINUS_SRC_ALPHA,
			/// 4 GL_DST_ALPHA,
			//BlendingFactorSrc.SrcAlpha,
			(BlendingFactorSrc)GL_DST_ALPHA,
			/// 5 GL_ONE_MINUS_DST_ALPHA,
			//BlendingFactorSrc.OneMinusSrcAlpha,
			(BlendingFactorSrc)GL_ONE_MINUS_DST_ALPHA,
			/// 6 GL_SRC_ALPHA,
			//BlendingFactorSrc.DstAlpha,
			(BlendingFactorSrc)GL_SRC_ALPHA,
			/// 7 GL_ONE_MINUS_SRC_ALPHA,
			//BlendingFactorSrc.OneMinusDstAlpha,
			(BlendingFactorSrc)GL_ONE_MINUS_SRC_ALPHA,
			/// 8 GL_DST_ALPHA,
			//BlendingFactorSrc.SrcAlpha,
			(BlendingFactorSrc)GL_DST_ALPHA,
			/// 9 GL_ONE_MINUS_DST_ALPHA,
			//BlendingFactorSrc.OneMinusSrcAlpha,
			(BlendingFactorSrc)GL_ONE_MINUS_DST_ALPHA,
			/// 10 GL_SRC_ALPHA
			//BlendingFactorSrc.DstAlpha,
			(BlendingFactorSrc)GL_SRC_ALPHA,
		};

		static readonly BlendingFactorDest[] BlendFuncDstTranslate = new BlendingFactorDest[] {
			/// 0 GL_DST_COLOR
			//BlendingFactorDest.DstColor,
			(BlendingFactorDest)GL_DST_COLOR,
			
			/// 1 GL_ONE_MINUS_DST_COLOR
			//BlendingFactorDest.OneMinusDstColor,
			(BlendingFactorDest)GL_ONE_MINUS_DST_COLOR,
			
			/// 2 GL_SRC_ALPHA
			//BlendingFactorDest.SrcAlpha,
			(BlendingFactorDest)GL_SRC_ALPHA,
			
			/// 3 GL_ONE_MINUS_SRC_ALPHA
			//BlendingFactorDest.OneMinusSrcAlpha,
			(BlendingFactorDest)GL_ONE_MINUS_SRC_ALPHA,
			
			/// 4 GL_DST_ALPHA
			//BlendingFactorDest.DstAlpha,
			(BlendingFactorDest)GL_DST_ALPHA,
			
			/// 5 GL_ONE_MINUS_DST_ALPHA
			//BlendingFactorDest.OneMinusDstAlpha,
			(BlendingFactorDest)GL_ONE_MINUS_DST_ALPHA,
			
			/// 6 GL_SRC_ALPHA
			//BlendingFactorDest.SrcAlpha, 
			(BlendingFactorDest)GL_SRC_ALPHA,
			
			/// 7 GL_ONE_MINUS_SRC_ALPHA
			//BlendingFactorDest.OneMinusSrcAlpha,
			(BlendingFactorDest)GL_ONE_MINUS_SRC_ALPHA,
			
			/// 8 GL_DST_ALPHA
			//BlendingFactorDest.DstAlpha,
			(BlendingFactorDest)GL_DST_ALPHA,
			
			/// 9 GL_ONE_MINUS_DST_ALPHA
			//BlendingFactorDest.OneMinusDstAlpha,
			(BlendingFactorDest)GL_ONE_MINUS_DST_ALPHA,
			
			/// 10 GL_ONE_MINUS_SRC_ALPHA
			//BlendingFactorDest.OneMinusSrcAlpha
			(BlendingFactorDest)GL_ONE_MINUS_SRC_ALPHA,
		};

		private void PrepareState_Blend(GpuStateStruct* GpuState)
		{
			var BlendingState = &GpuState[0].BlendingState;
			if (!GlEnableDisable(EnableCap.Blend, BlendingState[0].Enabled))
			{
				return;
			}

			var OpenglFunctionSource = BlendFuncSrcTranslate[(int)BlendingState[0].FunctionSource];
			var OpenglFunctionDestination = BlendFuncDstTranslate[(int)BlendingState[0].FunctionDestination];

			Func<ColorfStruct, int> getBlendFix = (Color) =>
			{
				if (Color.IsColorf(0, 0, 0)) return GL_ZERO;
				if (Color.IsColorf(1, 1, 1)) return GL_ONE;
				return GL_CONSTANT_COLOR;
			};

			if (BlendingState[0].FunctionSource == BlendingFactor.GU_FIX)
			{
				OpenglFunctionSource = (BlendingFactorSrc)getBlendFix(BlendingState[0].FixColorSource);
			}

			if (BlendingState[0].FunctionDestination == BlendingFactor.GU_FIX)
			{
				if (((int)OpenglFunctionSource == GL_CONSTANT_COLOR) && (BlendingState[0].FixColorSource + BlendingState[0].FixColorDestination).IsColorf(1, 1, 1))
				{
					OpenglFunctionDestination = (BlendingFactorDest)GL_ONE_MINUS_CONSTANT_COLOR;
				}
				else
				{
					OpenglFunctionDestination = (BlendingFactorDest)getBlendFix(BlendingState[0].FixColorDestination);
				}
			}
			//Console.WriteLine("{0}, {1}", OpenglFunctionSource, OpenglFunctionDestination);

			GL.BlendEquation(BlendEquationTranslate[(int)BlendingState[0].Equation]);
			GL.BlendFunc(OpenglFunctionSource, OpenglFunctionDestination);

			GL.BlendColor(
				BlendingState[0].FixColorDestination.Red,
				BlendingState[0].FixColorDestination.Green,
				BlendingState[0].FixColorDestination.Blue,
				BlendingState[0].FixColorDestination.Alpha
			);
			/*
			int getBlendFix(Colorf color) {
				if (color.isColorf(0, 0, 0)) return GL_ZERO;
				if (color.isColorf(1, 1, 1)) return GL_ONE;
				return GL_CONSTANT_COLOR;
			}



			
			// @CHECK @FIX
			//glBlendEquationEXT(BlendEquationTranslate[state.blend.equation]);
			//glBlendFunc(glFuncSrc, glFuncDst);
			
			// @TODO Must mix colors. 
			glBlendColor(
				state.blend.fixColorDst.r,
				state.blend.fixColorDst.g,
				state.blend.fixColorDst.b,
				state.blend.fixColorDst.a
			);
			*/
			//GL.BlendColor(BlendingState[0].FixColorDst);
		}

		private void PrepareState_Texture(GpuStateStruct* GpuState)
		{
			var TextureMappingState = &GpuState[0].TextureMappingState;
			var ClutState = &TextureMappingState[0].ClutState;
			var TextureState = &TextureMappingState[0].TextureState;
			var Mipmap0 = &TextureMappingState[0].TextureState.Mipmap0;

			if (!GlEnableDisable(EnableCap.Texture2D, TextureMappingState[0].Enabled))
			{
				return;
			}

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.MatrixMode(MatrixMode.Texture); 
			if (VertexType.Transform2D)
			{
				GL.LoadIdentity();
				GL.Scale(
					1.0f / Mipmap0[0].Width,
					1.0f / Mipmap0[0].Height,
					1.0f
				);
			}
			else 
			{
				switch (TextureMappingState[0].TextureMapMode)
				{
					case TextureMapMode.GU_TEXTURE_COORDS:
						GL.LoadIdentity();
						GL.Translate(TextureState[0].OffsetU, TextureState[0].OffsetV, 0);
						GL.Scale(TextureState[0].ScaleU, TextureState[0].ScaleV, 1);
					break;
					case TextureMapMode.GU_TEXTURE_MATRIX:
						//glLoadMatrixf(state.texture.matrix.pointer);
						throw(new NotImplementedException());
					case TextureMapMode.GU_ENVIRONMENT_MAP:
						throw(new NotImplementedException());
						/*
						Matrix envmapMatrix;
						envmapMatrix.setIdentity();
						
						for (int i = 0; i < 3; i++) {
							envmapMatrix.rows[0][i] = state.lighting.lights[state.texture.texShade[0]].position[i];
							envmapMatrix.rows[1][i] = state.lighting.lights[state.texture.texShade[1]].position[i];
						}
						
						glLoadMatrixf(envmapMatrix.pointer);
						Logger.log(Logger.Level.WARNING, "GPU", "Not implemented! texture for transform3D!");
						*/
				}
			}

			var MipmapAddress = Mipmap0[0].Address;
			var MipmapPointer = Memory.PspAddressToPointer(MipmapAddress);

			var Texture = TextureCache.Get(TextureState, ClutState);
			Texture.Bind();
		}
	}
}
