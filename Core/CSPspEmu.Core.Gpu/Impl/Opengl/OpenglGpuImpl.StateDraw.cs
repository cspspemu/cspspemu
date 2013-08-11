#define ENABLE_TEXTURES

using System;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.State.SubStates;
using CSharpPlatform.GL;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public sealed unsafe partial class OpenglGpuImpl
	{
		private void PrepareStateDraw(GpuStateStruct* GpuState)
		{
			GL.glColorMask(true, true, true, true);

#if ENABLE_TEXTURES
			PrepareState_Texture_Common(GpuState);
#endif
			PrepareState_Blend(GpuState);
			PrepareState_Clip(GpuState);

			if (GpuState->VertexState.Type.Transform2D)
			{
				PrepareState_Colors_2D(GpuState);
				GL.glDisable(GL.GL_STENCIL_TEST);
				GL.glDisable(GL.GL_CULL_FACE);
				GL.glDepthRangef(0, 1);
				GL.glDisable(GL.GL_DEPTH_TEST);
				//GL.glDisable(EnableCap.Lighting);
			}
			else
			{
				PrepareState_Colors_3D(GpuState);
				PrepareState_CullFace(GpuState);
				PrepareState_Lighting(GpuState);
				PrepareState_Depth(GpuState);
				PrepareState_DepthTest(GpuState);
				PrepareState_Stencil(GpuState);
			}
			//GL.ShadeModel((GpuState->ShadeModel == ShadingModelEnum.Flat) ? ShadingModel.Flat : ShadingModel.Smooth);
			PrepareState_AlphaTest(GpuState);
		}

		private void PrepareState_Clip(GpuStateStruct* GpuState)
		{
			if (!GlEnableDisable(GL.GL_SCISSOR_TEST, GpuState->ClipPlaneState.Enabled))
			{
				return;
			}
			var Scissor = &GpuState->ClipPlaneState.Scissor;
			GL.glScissor(
				Scissor->Left * ScaleViewport,
				Scissor->Top * ScaleViewport,
				(Scissor->Right - Scissor->Left) * ScaleViewport,
				(Scissor->Bottom - Scissor->Top) * ScaleViewport
			);
		}

		private void PrepareState_AlphaTest(GpuStateStruct* GpuState)
		{
			//if (!GlEnableDisable(EnableCap.AlphaTest, GpuState->AlphaTestState.Enabled))
			//{
			//	return;
			//}
			//
			//GL.glAlphaFunc(
			//	(AlphaFunction)DepthFunctionTranslate[(int)GpuState->AlphaTestState.Function],
			//	GpuState->AlphaTestState.Value
			//);
		}

		private void PrepareState_Stencil(GpuStateStruct* GpuState)
		{
			if (!GlEnableDisable(GL.GL_STENCIL_TEST, GpuState->StencilState.Enabled))
			{
				return;
			}

			//Console.Error.WriteLine("aaaaaa!");

			//if (state.stencilFuncFunc == 2) { outputDepthAndStencil(); assert(0); }

#if false
											Console.Error.WriteLine(
			"{0}:{1}:{2} - {3}, {4}, {5}",
			StencilFunctionTranslate[(int)GpuState->StencilState.Function],
			GpuState->StencilState.FunctionRef,
			GpuState->StencilState.FunctionMask,
			StencilOperationTranslate[(int)GpuState->StencilState.OperationFail],
			StencilOperationTranslate[(int)GpuState->StencilState.OperationZFail],
			StencilOperationTranslate[(int)GpuState->StencilState.OperationZPass]
		);
#endif

			GL.glStencilFunc(
				StencilFunctionTranslate[(int)GpuState->StencilState.Function],
				GpuState->StencilState.FunctionRef,
				GpuState->StencilState.FunctionMask
			);

			GL.glStencilOp(
				StencilOperationTranslate[(int)GpuState->StencilState.OperationFail],
				StencilOperationTranslate[(int)GpuState->StencilState.OperationZFail],
				StencilOperationTranslate[(int)GpuState->StencilState.OperationZPass]
			);
		}

		private void PrepareState_CullFace(GpuStateStruct* GpuState)
		{
			if (!GlEnableDisable(GL.GL_CULL_FACE, GpuState->BackfaceCullingState.Enabled))
			{
				return;
			}

			//GlEnableDisable(EnableCap.CullFace, false);

			GL.glCullFace((GpuState->BackfaceCullingState.FrontFaceDirection == FrontFaceDirectionEnum.ClockWise) ? GL.GL_FRONT : GL.GL_BACK);
		}

		private void PrepareState_Depth(GpuStateStruct* GpuState)
		{
			GL.glDepthRangef(GpuState->DepthTestState.RangeNear, GpuState->DepthTestState.RangeFar);
			//GL.DepthRange(GpuState->DepthTestState.RangeNear, GpuState->DepthTestState.RangeFar);
		}

		private void PrepareState_DepthTest(GpuStateStruct* GpuState)
		{
			if (GpuState->DepthTestState.Mask != 0 && GpuState->DepthTestState.Mask != 1)
			{
				Console.Error.WriteLine("WARNING! DepthTestState.Mask: {0}", GpuState->DepthTestState.Mask);
			}
			GL.glDepthMask(GpuState->DepthTestState.Mask == 0);
			if (!GlEnableDisable(GL.GL_DEPTH_TEST, GpuState->DepthTestState.Enabled))
			{
				return;
			}
			GL.glDepthFunc(DepthFunctionTranslate[(int)GpuState->DepthTestState.Function]);
		}

		private void PrepareState_Colors_2D(GpuStateStruct* GpuState)
		{
			PrepareState_Colors_3D(GpuState);
		}

		private void PrepareState_Colors_3D(GpuStateStruct* GpuState)
		{
			//GlEnableDisable(EnableCap.ColorMaterial, VertexType.Color != VertexTypeStruct.ColorEnum.Void);
			//
			//var Color = GpuState->LightingState.AmbientModelColor;
			//var LightingState = &GpuState->LightingState;
			//GL.Color4(&Color.Red);
			//
			//if (VertexType.Color != VertexTypeStruct.ColorEnum.Void && LightingState->Enabled)
			//{
			//	var Flags = (ColorMaterialParameter)0;
			//	/*
			//	glMaterialfv(faces, GL_AMBIENT , [0.0f, 0.0f, 0.0f, 0.0f].ptr);
			//	glMaterialfv(faces, GL_DIFFUSE , [0.0f, 0.0f, 0.0f, 0.0f].ptr);
			//	glMaterialfv(faces, GL_SPECULAR, [0.0f, 0.0f, 0.0f, 0.0f].ptr);
			//	*/
			//
			//	var MaterialColorComponents = LightingState->MaterialColorComponents;
			//
			//	if (MaterialColorComponents.HasFlag(LightComponentsSet.Ambient))
			//	{
			//		GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, &LightingState->AmbientModelColor.Red);
			//	}
			//
			//	if (MaterialColorComponents.HasFlag(LightComponentsSet.Diffuse))
			//	{
			//		GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, &LightingState->DiffuseModelColor.Red);
			//	}
			//
			//	if (MaterialColorComponents.HasFlag(LightComponentsSet.Specular))
			//	{
			//		GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, &LightingState->SpecularModelColor.Red);
			//	}
			//
			//	if (MaterialColorComponents.HasFlag(LightComponentsSet.AmbientAndDiffuse))
			//	{
			//		Flags = ColorMaterialParameter.AmbientAndDiffuse;
			//	}
			//	else if (MaterialColorComponents.HasFlag(LightComponentsSet.Ambient))
			//	{
			//		Flags = ColorMaterialParameter.Ambient;
			//	}
			//	else if (MaterialColorComponents.HasFlag(LightComponentsSet.Diffuse))
			//	{
			//		Flags = ColorMaterialParameter.Diffuse;
			//	}
			//	else if (MaterialColorComponents.HasFlag(LightComponentsSet.Specular))
			//	{
			//		Flags = ColorMaterialParameter.Specular;
			//	}
			//	else
			//	{
			//		//throw (new NotImplementedException("Error! : " + MaterialColorComponents));
			//	}
			//	//flags = GL_SPECULAR;
			//	if (Flags != 0)
			//	{
			//		GL.ColorMaterial(MaterialFace.FrontAndBack, Flags);
			//	}
			//	//glEnable(GL_COLOR_MATERIAL);
			//}
			//
			//GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, &GpuState->LightingState.EmissiveModelColor.Red);
		}

		private void PrepareState_Lighting(GpuStateStruct* GpuState)
		{
			//var LightingState = &GpuState->LightingState;
			//
			//if (!GlEnableDisable(EnableCap.Lighting, LightingState->Enabled))
			//{
			//	return;
			//}
			//
			//GL.LightModel(
			//	LightModelParameter.LightModelColorControl,
			//	(int)((LightingState->LightModel == LightModelEnum.SeparateSpecularColor) ? LightModelColorControl.SeparateSpecularColor : LightModelColorControl.SingleColor)
			//);
			//GL.LightModel(LightModelParameter.LightModelAmbient, &LightingState->AmbientLightColor.Red);
			//
			//for (int n = 0; n < 4; n++)
			//{
			//	var LightState = &(&LightingState->Light0)[n];
			//	LightName LightName = (LightName)(LightName.Light0 + n);
			//
			//	if (!GlEnableDisable((EnableCap)(EnableCap.Light0 + n), LightState->Enabled))
			//	{
			//		continue;
			//	}
			//
			//	GL.Light(LightName, LightParameter.Specular, &LightState->SpecularColor.Red);
			//	GL.Light(LightName, LightParameter.Ambient, &LightState->AmbientColor.Red);
			//	GL.Light(LightName, LightParameter.Diffuse, &LightState->DiffuseColor.Red);
			//
			//	LightState->Position.W = 1.0f;
			//	GL.Light(LightName, LightParameter.Position, &LightState->Position.X);
			//
			//	GL.Light(LightName, LightParameter.ConstantAttenuation, &LightState->Attenuation.Constant);
			//	GL.Light(LightName, LightParameter.LinearAttenuation, &LightState->Attenuation.Linear);
			//	GL.Light(LightName, LightParameter.QuadraticAttenuation, &LightState->Attenuation.Quadratic);
			//
			//	if (LightState->Type == LightTypeEnum.SpotLight)
			//	{
			//		GL.Light(LightName, LightParameter.SpotDirection, &LightState->SpotDirection.X);
			//		GL.Light(LightName, LightParameter.SpotExponent, &LightState->SpotExponent);
			//		GL.Light(LightName, LightParameter.SpotCutoff, &LightState->SpotCutoff);
			//	}
			//	else
			//	{
			//		GL.Light(LightName, LightParameter.SpotExponent, 0);
			//		GL.Light(LightName, LightParameter.SpotCutoff, 180);
			//	}
			//}
		}

		private void PrepareState_Blend(GpuStateStruct* GpuState)
		{
			if (GpuState->ColorTestState.Enabled)
			{
				GlEnableDisable(GL.GL_BLEND, true);
				GL.glBlendEquation(GL.GL_FUNC_ADD);
				GL.glBlendFunc(GL.GL_SRC_ALPHA, GL.GL_ONE_MINUS_SRC_ALPHA);
				GL.glBlendColor(0, 0, 0, 0);

				return;
			}
			else
			{
				GlEnableDisable(GL.GL_BLEND, false);
			}

			var BlendingState = &GpuState->BlendingState;
			if (!GlEnableDisable(GL.GL_BLEND, BlendingState->Enabled))
			{
				return;
			}

			//Console.WriteLine("Blend!");

			var OpenglFunctionSource = BlendFuncSrcTranslate[(int)BlendingState->FunctionSource];
			//var OpenglFunctionDestination = BlendFuncDstTranslate[(int)BlendingState->FunctionDestination];
			var OpenglFunctionDestination = BlendFuncSrcTranslate[(int)BlendingState->FunctionDestination];

			Func<ColorfStruct, int> getBlendFix = (Color) =>
			{
				if (Color.IsColorf(0, 0, 0)) return GL.GL_ZERO;
				if (Color.IsColorf(1, 1, 1)) return GL.GL_ONE;
				return GL.GL_CONSTANT_COLOR;
			};

			if (BlendingState->FunctionSource == GuBlendingFactorSource.GU_FIX)
			{
				OpenglFunctionSource = getBlendFix(BlendingState->FixColorSource);
			}

			if (BlendingState->FunctionDestination == GuBlendingFactorDestination.GU_FIX)
			{
				if (((int)OpenglFunctionSource == GL.GL_CONSTANT_COLOR) && (BlendingState->FixColorSource + BlendingState->FixColorDestination).IsColorf(1, 1, 1))
				{
					OpenglFunctionDestination = GL.GL_ONE_MINUS_CONSTANT_COLOR;
				}
				else
				{
					OpenglFunctionDestination = getBlendFix(BlendingState->FixColorDestination);
				}
			}
			//Console.WriteLine("{0}, {1}", OpenglFunctionSource, OpenglFunctionDestination);

			var OpenglBlendEquation = BlendEquationTranslate[(int)BlendingState->Equation];

			/*
			Console.WriteLine(
				"{0} : {1} -> {2}",
				OpenglBlendEquation, OpenglFunctionSource, OpenglFunctionDestination
			);
			*/

			GL.glBlendEquation(OpenglBlendEquation);
			GL.glBlendFunc(OpenglFunctionSource, OpenglFunctionDestination);

			GL.glBlendColor(
				BlendingState->FixColorDestination.Red,
				BlendingState->FixColorDestination.Green,
				BlendingState->FixColorDestination.Blue,
				BlendingState->FixColorDestination.Alpha
			);
		}

		private void PrepareState_Texture_2D(GpuStateStruct* GpuState)
		{
			var TextureMappingState = &GpuState->TextureMappingState;
			var Mipmap0 = &TextureMappingState->TextureState.Mipmap0;

			if (TextureMappingState->Enabled)
			{
				//GL.ActiveTexture(TextureUnit.Texture0);
				//GL.MatrixMode(MatrixMode.Texture);
				//GL.LoadIdentity();
				//
				//GL.Scale(
				//	1.0f / Mipmap0->BufferWidth,
				//	1.0f / Mipmap0->TextureHeight,
				//	1.0f
				//);
			}
		}

		private void PrepareState_Texture_3D(GpuStateStruct* GpuState)
		{
			var TextureMappingState = &GpuState->TextureMappingState;
			var TextureState = &TextureMappingState->TextureState;

			if (TextureMappingState->Enabled)
			{
				//GL.ActiveTexture(TextureUnit.Texture0);
				//GL.MatrixMode(MatrixMode.Texture);
				//GL.LoadIdentity();

				switch (TextureMappingState->TextureMapMode)
				{
					case TextureMapMode.GU_TEXTURE_COORDS:
						//GL.Translate(TextureState->OffsetU, TextureState->OffsetV, 0);
						//GL.Scale(TextureState->ScaleU, TextureState->ScaleV, 1);
						break;
					case TextureMapMode.GU_TEXTURE_MATRIX:
						switch (GpuState->TextureMappingState.TextureProjectionMapMode)
						{
							default:
								Console.Error.WriteLine("NotImplemented: GU_TEXTURE_MATRIX: {0}", GpuState->TextureMappingState.TextureProjectionMapMode);
								break;
						}
						break;
					case TextureMapMode.GU_ENVIRONMENT_MAP:
						Console.Error.WriteLine("NotImplemented: GU_ENVIRONMENT_MAP");
						break;
					default:
						Console.Error.WriteLine("NotImplemented TextureMappingState->TextureMapMode: " + TextureMappingState->TextureMapMode);
						break;
				}
			}
		}

		private void PrepareState_Texture_Common(GpuStateStruct* GpuState)
		{
			var TextureMappingState = &GpuState->TextureMappingState;
			//var ClutState = &TextureMappingState->ClutState;
			var TextureState = &TextureMappingState->TextureState;

			if (!GlEnableDisable(GL.GL_TEXTURE_2D, TextureMappingState->Enabled))
			{
				return;
			}

			if (VertexType.Transform2D)
			{
				PrepareState_Texture_2D(GpuState);
			}
			else 
			{
				PrepareState_Texture_3D(GpuState);
			}

			//GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			//glPixelStorei(GL_UNPACK_ALIGNMENT, 1);

			TextureCacheGetAndBind(GpuState);
			//CurrentTexture.Save("test.png");

			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)((TextureState->FilterMinification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST));
			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)((TextureState->FilterMagnification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST));
			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int)((TextureState->WrapU == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE));
			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int)((TextureState->WrapV == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE));

			//GL.glTexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvModeTranslate[(int)TextureState->Effect]);
		}
	}
}
