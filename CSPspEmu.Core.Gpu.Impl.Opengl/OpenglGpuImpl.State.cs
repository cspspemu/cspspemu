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
			PrepareState_Texture(GpuState);
			PrepareState_CullFace(GpuState);
			//PrepareState_Colors(GpuState);
			PrepareState_Lighting(GpuState);
			PrepareState_Blend(GpuState);
			PrepareState_Depth(GpuState);
			PrepareState_DepthTest(GpuState);
			PrepareState_Stencil(GpuState);

			GL.ShadeModel((GpuState[0].ShadeModel == ShadingModelEnum.Flat) ? ShadingModel.Flat : ShadingModel.Smooth);
		}

		private void PrepareState_Stencil(GpuStateStruct* GpuState)
		{
			
			if (!GlEnableDisable(EnableCap.StencilTest, GpuState[0].StencilState.Enabled))
			{
				return;
			}

			//if (state.stencilFuncFunc == 2) { outputDepthAndStencil(); assert(0); }
			
			GL.StencilFunc(
				(StencilFunction)TestTranslate[(int)GpuState[0].StencilState.Function],
				GpuState[0].StencilState.FunctionRef,
				GpuState[0].StencilState.FunctionMask
			);

			GL.StencilOp(
				StencilOperationTranslate[(int)GpuState[0].StencilState.OperationSFail],
				StencilOperationTranslate[(int)GpuState[0].StencilState.OperationDpFail],
				StencilOperationTranslate[(int)GpuState[0].StencilState.OperationDpPass]
			);
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

		private void PrepareState_Blend(GpuStateStruct* GpuState)
		{
			/*
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			//glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

			//Console.WriteLine("{0}, {1}", GpuState[0].BlendingState.FunctionSource, GpuState[0].BlendingState.FunctionDestination);

			return;
			*/

			var BlendingState = &GpuState[0].BlendingState;
			if (!GlEnableDisable(EnableCap.Blend, BlendingState[0].Enabled))
			{
				return;
			}

			//Console.WriteLine("Blend!");

			var OpenglFunctionSource = BlendFuncSrcTranslate[(int)BlendingState[0].FunctionSource];
			var OpenglFunctionDestination = BlendFuncDstTranslate[(int)BlendingState[0].FunctionDestination];

			Func<ColorfStruct, int> getBlendFix = (Color) =>
			{
				if (Color.IsColorf(0, 0, 0)) return GL_ZERO;
				if (Color.IsColorf(1, 1, 1)) return GL_ONE;
				return GL_CONSTANT_COLOR;
			};

			if (BlendingState[0].FunctionSource == GuBlendingFactorSource.GU_FIX)
			{
				OpenglFunctionSource = (BlendingFactorSrc)getBlendFix(BlendingState[0].FixColorSource);
			}

			if (BlendingState[0].FunctionDestination == GuBlendingFactorDestination.GU_FIX)
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

			var Texture = TextureCache.Get(TextureState, ClutState);
			Texture.Bind();

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)((TextureState[0].FilterMinification == TextureFilter.Linear) ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)((TextureState[0].FilterMagnification == TextureFilter.Linear) ? TextureMagFilter.Linear : TextureMagFilter.Nearest));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)(TextureState[0].WrapU == WrapMode.Repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp));
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)(TextureState[0].WrapV == WrapMode.Repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp));

		}
	}
}
