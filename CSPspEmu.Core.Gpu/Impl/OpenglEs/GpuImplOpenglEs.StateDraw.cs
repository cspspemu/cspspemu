#define ENABLE_TEXTURES

using CSharpPlatform;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Gpu.State.SubStates;
using GLES;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public unsafe partial class GpuImplOpenglEs
	{
		private void PrepareStateDraw(GpuStateStruct* GpuState)
		{
			GL.glColorMask(true, true, true, true);

#if ENABLE_TEXTURES
			PrepareState_Texture_Common(GpuState);
#endif
			PrepareState_Blend(GpuState);

			if (GpuState->VertexState.Type.Transform2D)
			{
				PrepareState_Colors_2D(GpuState);
				GL.glDisable(GL.GL_STENCIL_TEST);
				GL.glDisable(GL.GL_CULL_FACE);
				GL.glDepthRangef(0, 1);
				GL.glDisable(GL.GL_DEPTH_TEST);
				//GL.glDisable(GL.GL_LIGHTNING_TEST);
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

		private void PrepareState_AlphaTest(GpuStateStruct* GpuState)
		{
			/*
			if (GlEnableDisable(GL.GL_ALPHA_TEST, GpuState->AlphaTestState.Enabled))
			{
				return;
			}

			GL.AlphaFunc(
				(AlphaFunction)DepthFunctionTranslate[(int)GpuState->AlphaTestState.Function],
				GpuState->AlphaTestState.Value
			);
			*/
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
			//GL.CullFace((GpuState->BackfaceCullingState.FrontFaceDirection == FrontFaceDirectionEnum.ClockWise) ? CullFaceMode.Back : CullFaceMode.Front);
		}

		private void PrepareState_Depth(GpuStateStruct* GpuState)
		{
			//GL.DepthRange((double)GpuState->DepthTestState.RangeFar, (double)GpuState->DepthTestState.RangeNear);
			GL.glDepthRangef(GpuState->DepthTestState.RangeNear, GpuState->DepthTestState.RangeFar);
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
			//GL.DepthFunc(DepthFunction.Greater);
			GL.glDepthFunc(DepthFunctionTranslate[(int)GpuState->DepthTestState.Function]);
			//GL.DepthRange
		}

		private void PrepareState_Colors_2D(GpuStateStruct* GpuState)
		{
			PrepareState_Colors_3D(GpuState);
		}

		private void PrepareState_Colors_3D(GpuStateStruct* GpuState)
		{
			/*
			//GlEnableDisable(EnableCap.ColorMaterial, VertexType.Color != VertexTypeStruct.ColorEnum.Void);

			var Color = GpuState->LightingState.AmbientModelColor;
			GL.glColor4(&Color.Red);

			if (VertexType.Color != VertexTypeStruct.ColorEnum.Void && GpuState->LightingState.Enabled)
			{
				ColorMaterialParameter flags = (ColorMaterialParameter)0;
				//glMaterialfv(faces, GL_AMBIENT , [0.0f, 0.0f, 0.0f, 0.0f].ptr);
				//glMaterialfv(faces, GL_DIFFUSE , [0.0f, 0.0f, 0.0f, 0.0f].ptr);
				//glMaterialfv(faces, GL_SPECULAR, [0.0f, 0.0f, 0.0f, 0.0f].ptr);

				var MaterialColorComponents = GpuState->LightingState.MaterialColorComponents;

				if (MaterialColorComponents.HasFlag(LightComponentsSet.Ambient))
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, &GpuState->LightingState.AmbientModelColor.Red);
				}

				if (MaterialColorComponents.HasFlag(LightComponentsSet.Diffuse))
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, &GpuState->LightingState.DiffuseModelColor.Red);
				}

				if (MaterialColorComponents.HasFlag(LightComponentsSet.Specular))
				{
					GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, &GpuState->LightingState.SpecularModelColor.Red);
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

			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, &GpuState->LightingState.EmissiveModelColor.Red);
			*/
		}

		private void PrepareState_Lighting(GpuStateStruct* GpuState)
		{
			/*
			//Console.WriteLine(GpuState->LightingState.AmbientModelColor);
			var LightingState = &GpuState->LightingState;

			// if (!glEnableDisable(GL_LIGHTING, state.lighting.enabled) && (state.texture.mapMode != TextureMapMode.GU_ENVIRONMENT_MAP)) {
			if (!GlEnableDisable(EnableCap.Lighting, LightingState->Enabled))
			{
				return;
			}

			GL.LightModel(
				LightModelParameter.LightModelColorControl,
				(int)((LightingState->LightModel == LightModelEnum.SeparateSpecularColor) ? LightModelColorControl.SeparateSpecularColor : LightModelColorControl.SingleColor)
			);
			GL.LightModel(LightModelParameter.LightModelAmbient, &LightingState->AmbientLightColor.Red);

			for (int n = 0; n < 4; n++)
			{
				var LightState = &(&LightingState->Light0)[n];
				LightName LightName = (LightName)(LightName.Light0 + n);

				if (!GlEnableDisable((EnableCap)(EnableCap.Light0 + n), LightState->Enabled))
				{
					continue;
				}

				GL.Light(LightName, LightParameter.Specular, &LightState->SpecularColor.Red);
				GL.Light(LightName, LightParameter.Ambient, &LightState->AmbientColor.Red);
				GL.Light(LightName, LightParameter.Diffuse, &LightState->DiffuseColor.Red);

				LightState->Position.W = 1.0f;
				GL.Light(LightName, LightParameter.Position, &LightState->Position.X);

				GL.Light(LightName, LightParameter.ConstantAttenuation, &LightState->Attenuation.Constant);
				GL.Light(LightName, LightParameter.LinearAttenuation, &LightState->Attenuation.Linear);
				GL.Light(LightName, LightParameter.QuadraticAttenuation, &LightState->Attenuation.Quadratic);

				if (LightState->Type == LightTypeEnum.SpotLight)
				{
					GL.Light(LightName, LightParameter.SpotDirection, &LightState->SpotDirection.X);
					GL.Light(LightName, LightParameter.SpotExponent, &LightState->SpotExponent);
					GL.Light(LightName, LightParameter.SpotCutoff, &LightState->SpotCutoff);
				}
				else
				{
					GL.Light(LightName, LightParameter.SpotExponent, 0);
					GL.Light(LightName, LightParameter.SpotCutoff, 180);
				}
			}
			*/
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
				textureMatrix.SetMatrix4(Matrix4.Identity.Scale(1f / Mipmap0->BufferWidth, 1f / Mipmap0->TextureHeight, 1));
				//textureMatrix.SetMatrix4(Matrix4.Identity);

				//GL.glActiveTexture(TextureUnit.Texture0);
				//GL.glMatrixMode(MatrixMode.Texture);
				//GL.glLoadIdentity();
				//
				////GL.LoadIdentity();
				//GL.glScale(
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
				switch (TextureMappingState->TextureMapMode)
				{
					case TextureMapMode.GU_TEXTURE_COORDS:
						//GL.LoadIdentity();
						textureMatrix.SetMatrix4(
							Matrix4.Identity
							.Translate(TextureState->OffsetU, TextureState->OffsetV, 0)
							.Scale(TextureState->ScaleU, TextureState->ScaleV, 1)
						);
						//Console.Error.WriteLine("NotImplemented: GU_TEXTURE_COORDS");
						break;
					case TextureMapMode.GU_TEXTURE_MATRIX:
						//glLoadMatrixf(state.texture.matrix.pointer);
						//throw(new NotImplementedException());
						switch (GpuState->TextureMappingState.TextureProjectionMapMode)
						{
							//case TextureProjectionMapMode.GU_UV:
							default:
								Console.Error.WriteLine("NotImplemented: GU_TEXTURE_MATRIX: {0}", GpuState->TextureMappingState.TextureProjectionMapMode);
								break;
						}
						break;
					case TextureMapMode.GU_ENVIRONMENT_MAP:
						Console.Error.WriteLine("NotImplemented: GU_ENVIRONMENT_MAP");

						//throw(new NotImplementedException());
						//GpuMatrix4x4Struct EnviromentMapMatrix;

						//EnviromentMapMatrix.SetIdentity();

						/*
						for (int i = 0; i < 3; i++) {
							EnviromentMapMatrix.Set(0, i);
							EnviromentMapMatrix.rows[0][i] = state.lighting.lights[state.texture.texShade[0]].position[i];
							EnviromentMapMatrix.rows[1][i] = state.lighting.lights[state.texture.texShade[1]].position[i];
						}
						
						glLoadMatrixf(EnviromentMapMatrix.pointer);
						Logger.log(Logger.Level.WARNING, "GPU", "Not implemented! texture for transform3D!");
						*/
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

			GL.glActiveTexture(GL.GL_TEXTURE0);
			CurrentTexture = TextureCache.Get(GpuState);
			CurrentTexture.Bind();
			//CurrentTexture.Save("c:/temp/" + CurrentTexture.TextureHash + ".png");

			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (TextureState->FilterMinification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST);
			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (TextureState->FilterMagnification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST);

			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (TextureState->WrapU == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE);
			GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (TextureState->WrapV == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE);

			//Console.WriteLine("{0}", TextureState->Effect);
			//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvModeTranslate[(int)TextureState->Effect]);

			//CurrentTexture = TextureCache.Get(GpuState);
			//CurrentTexture.Bind();
			////CurrentTexture.Save("test.png");
			//
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)((TextureState->FilterMinification == TextureFilter.Linear) ? TextureMinFilter.Linear : TextureMinFilter.Nearest));
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)((TextureState->FilterMagnification == TextureFilter.Linear) ? TextureMagFilter.Linear : TextureMagFilter.Nearest));
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)((TextureState->WrapU == WrapMode.Repeat) ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge));
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)((TextureState->WrapV == WrapMode.Repeat) ? TextureWrapMode.Repeat : TextureWrapMode.ClampToEdge));
			//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvModeTranslate[(int)TextureState->Effect]);
		}

#if false
		private void PrepareStateDraw(GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType)
		{
			var VertexType = GpuState->VertexState.Type;
			var TextureState = &GpuState->TextureMappingState.TextureState;

			if (PrimitiveType == GuPrimitiveType.Sprites)
			{
				GL.glDisable(GL.GL_CULL_FACE);
			}
			else
			{
				PrepareState_Blend(GpuState);
				GL.glDisable(GL.GL_CULL_FACE);
				//if (GL.glEnableDisable(GL.GL_CULL_FACE, GpuState->BackfaceCullingState.Enabled))
				//{
				//	GL.glCullFace((GpuState->BackfaceCullingState.FrontFaceDirection == FrontFaceDirectionEnum.ClockWise) ? GL.GL_CW : GL.GL_CCW);
				//}

				//GL.glEnableDisable(GL.GL_CULL_FACE, true);
				//GL.glCullFace(GL.GL_CCW);

				//this.fColor.SetVec4(1, 0, 0, 1);
				if (VertexType.Color == VertexTypeStruct.ColorEnum.Void)
				{
					this.fColor.SetVec4(&GpuState->LightingState.AmbientModelColor.Red);
					//this.fColor.SetVec4(VertexInfo.Color.X, VertexInfo.Color.Y, VertexInfo.Color.Z, VertexInfo.Color.W);
				}
			}

			if (GlEnableDisable(GL.GL_TEXTURE_2D, GpuState->TextureMappingState.Enabled))
			{
				GL.glActiveTexture(GL.GL_TEXTURE0);
				CurrentTexture = TextureCache.Get(GpuState);
				CurrentTexture.Bind();
				//CurrentTexture.Save("c:/temp/" + CurrentTexture.TextureHash + ".png");

				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (TextureState->FilterMinification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST);
				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (TextureState->FilterMagnification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST);

				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (TextureState->WrapU == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE);
				GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (TextureState->WrapV == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE);

				//Console.WriteLine("{0}", TextureState->Effect);
				//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvModeTranslate[(int)TextureState->Effect]);
			}
		}

		private static void PrepareState_DepthTest(GpuStateStruct* GpuState)
		{
			if (GpuState->DepthTestState.Mask != 0 && GpuState->DepthTestState.Mask != 1)
			{
				Console.Error.WriteLine("WARNING! DepthTestState.Mask: {0}", GpuState->DepthTestState.Mask);
			}

			//Console.WriteLine("near: {0}, far: {1}", GpuState->DepthTestState.RangeNear, GpuState->DepthTestState.RangeFar);

			if (GL.glEnableDisable(GL.GL_DEPTH_TEST, GpuState->DepthTestState.Enabled))
			{
				GL.glDepthRangef(GpuState->DepthTestState.RangeFar, GpuState->DepthTestState.RangeNear);
				GL.glDepthMask(GpuState->DepthTestState.Mask == 0);
				GL.glDepthFunc(DepthFunctionTranslate[(int)GpuState->DepthTestState.Function]);
			}
		}

		private static void PrepareState_Blend(GpuStateStruct* GpuState)
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
#endif
	}
}
