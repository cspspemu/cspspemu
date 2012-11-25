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
				GL.glEnableDisable(GL.GL_CULL_FACE, GpuState->BackfaceCullingState.Enabled);
				GL.glCullFace((GpuState->BackfaceCullingState.FrontFaceDirection == FrontFaceDirectionEnum.ClockWise) ? GL.GL_CW : GL.GL_CCW);

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

			GL.glDepthMask(GpuState->DepthTestState.Mask == 0);

			if (!GL.glEnableDisable(GL.GL_DEPTH_TEST, GpuState->DepthTestState.Enabled))
			{
				return;
			}
			//GL.DepthFunc(DepthFunction.Greater);
			GL.glDepthFunc(DepthFunctionTranslate[(int)GpuState->DepthTestState.Function]);
			//GL.DepthRange
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
	}
}
