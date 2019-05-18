#define ENABLE_TEXTURES

using System;
using System.Numerics;
using CSPspEmu.Core.Gpu.State;
using CSharpPlatform.GL;
using CSPspEmu.Core.Gpu.Impl.Opengl.Utils;
using CSharpPlatform;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public sealed unsafe partial class OpenglGpuImpl
    {
        private void PrepareStateDraw(GpuStateStruct gpuState)
        {
            GL.glColorMask(true, true, true, true);

#if ENABLE_TEXTURES
            PrepareState_Texture_Common(gpuState);
#endif
            PrepareState_Blend(gpuState);
            PrepareState_Clip(gpuState);

            if (gpuState.VertexState.Type.Transform2D)
            {
                PrepareState_Colors_2D(gpuState);
                GL.glDisable(GL.GL_STENCIL_TEST);
                GL.glDisable(GL.GL_CULL_FACE);
                GL.DepthRange(0, 1);
                GL.glDisable(GL.GL_DEPTH_TEST);
                //GL.glDisable(EnableCap.Lighting);
            }
            else
            {
                PrepareState_Colors_3D(gpuState);
                PrepareState_CullFace(gpuState);
                PrepareState_Lighting(gpuState);
                PrepareState_Depth(gpuState);
                PrepareState_DepthTest(gpuState);
                PrepareState_Stencil(gpuState);
            }
            //GL.ShadeModel((GpuState->ShadeModel == ShadingModelEnum.Flat) ? ShadingModel.Flat : ShadingModel.Smooth);
            PrepareState_AlphaTest(gpuState);
        }

        private void PrepareState_Clip(GpuStateStruct gpuState)
        {
            if (!GL.EnableDisable(GL.GL_SCISSOR_TEST, gpuState.ClipPlaneState.Enabled))
            {
                return;
            }
            var scissor = gpuState.ClipPlaneState.Scissor;
            GL.glScissor(
                scissor.Left * ScaleViewport,
                scissor.Top * ScaleViewport,
                scissor.Width * ScaleViewport,
                scissor.Height * ScaleViewport
            );
        }

        private void PrepareState_AlphaTest(GpuStateStruct gpuState)
        {
            //if (!GL.EnableDisable(EnableCap.AlphaTest, GpuState->AlphaTestState.Enabled))
            //{
            //	return;
            //}
            //
            //GL.glAlphaFunc(
            //	(AlphaFunction)DepthFunctionTranslate[(int)GpuState->AlphaTestState.Function],
            //	GpuState->AlphaTestState.Value
            //);
        }

        private void PrepareState_Stencil(GpuStateStruct gpuState)
        {
            if (!GL.EnableDisable(GL.GL_STENCIL_TEST, gpuState.StencilState.Enabled))
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
                OpenglGpuImplConversionTables.StencilFunctionTranslate[(int) gpuState.StencilState.Function],
                gpuState.StencilState.FunctionRef,
                gpuState.StencilState.FunctionMask
            );

            GL.glStencilOp(
                OpenglGpuImplConversionTables.StencilOperationTranslate[(int) gpuState.StencilState.OperationFail],
                OpenglGpuImplConversionTables.StencilOperationTranslate[(int) gpuState.StencilState.OperationZFail],
                OpenglGpuImplConversionTables.StencilOperationTranslate[(int) gpuState.StencilState.OperationZPass]
            );
        }

        private void PrepareState_CullFace(GpuStateStruct gpuState)
        {
            if (!GL.EnableDisable(GL.GL_CULL_FACE, gpuState.BackfaceCullingState.Enabled))
            {
                return;
            }

            //GL.EnableDisable(EnableCap.CullFace, false);

            GL.glCullFace((gpuState.BackfaceCullingState.FrontFaceDirection == FrontFaceDirectionEnum.ClockWise)
                ? GL.GL_FRONT
                : GL.GL_BACK);
        }

        private void PrepareState_Depth(GpuStateStruct gpuState)
        {
            GL.DepthRange(gpuState.DepthTestState.RangeNear, gpuState.DepthTestState.RangeFar);
        }

        private void PrepareState_DepthTest(GpuStateStruct gpuState)
        {
            if (gpuState.DepthTestState.Mask != 0 && gpuState.DepthTestState.Mask != 1)
            {
                Console.Error.WriteLine("WARNING! DepthTestState.Mask: {0}", gpuState.DepthTestState.Mask);
            }
            GL.glDepthMask(gpuState.DepthTestState.Mask == 0);
            if (!GL.EnableDisable(GL.GL_DEPTH_TEST, gpuState.DepthTestState.Enabled))
            {
                return;
            }
            GL.glDepthFunc(
                OpenglGpuImplConversionTables.DepthFunctionTranslate[(int) gpuState.DepthTestState.Function]);
        }

        private void PrepareState_Colors_2D(GpuStateStruct gpuState)
        {
            PrepareState_Colors_3D(gpuState);
        }

        private void PrepareState_Colors_3D(GpuStateStruct gpuState)
        {
            //GL.EnableDisable(EnableCap.ColorMaterial, VertexType.Color != VertexTypeStruct.ColorEnum.Void);
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

        private void PrepareState_Lighting(GpuStateStruct gpuState)
        {
            //var LightingState = &GpuState->LightingState;
            //
            //if (!GL.EnableDisable(EnableCap.Lighting, LightingState->Enabled))
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
            //	if (!GL.EnableDisable((EnableCap)(EnableCap.Light0 + n), LightState->Enabled))
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

        private void PrepareState_Blend(GpuStateStruct gpuState)
        {
            var blendingState = gpuState.BlendingState;
            if (!GL.EnableDisable(GL.GL_BLEND, blendingState.Enabled))
            {
                return;
            }

            //Console.WriteLine("Blend!");

            var openglFunctionSource =
                OpenglGpuImplConversionTables.BlendFuncSrcTranslate[(int) blendingState.FunctionSource];
            //var OpenglFunctionDestination = BlendFuncDstTranslate[(int)BlendingState->FunctionDestination];
            var openglFunctionDestination =
                OpenglGpuImplConversionTables.BlendFuncSrcTranslate[(int) blendingState.FunctionDestination];

            Func<ColorfStruct, int> getBlendFix = (color) =>
            {
                if (color.IsColorf(0, 0, 0)) return GL.GL_ZERO;
                if (color.IsColorf(1, 1, 1)) return GL.GL_ONE;
                return GL.GL_CONSTANT_COLOR;
            };

            if (blendingState.FunctionSource == GuBlendingFactorSource.GuFix)
            {
                openglFunctionSource = getBlendFix(blendingState.FixColorSource);
            }

            if (blendingState.FunctionDestination == GuBlendingFactorDestination.GuFix)
            {
                if (((int) openglFunctionSource == GL.GL_CONSTANT_COLOR) &&
                    (blendingState.FixColorSource + blendingState.FixColorDestination).IsColorf(1, 1, 1))
                {
                    openglFunctionDestination = GL.GL_ONE_MINUS_CONSTANT_COLOR;
                }
                else
                {
                    openglFunctionDestination = getBlendFix(blendingState.FixColorDestination);
                }
            }
            //Console.WriteLine("{0}, {1}", OpenglFunctionSource, OpenglFunctionDestination);

            var openglBlendEquation =
                OpenglGpuImplConversionTables.BlendEquationTranslate[(int) blendingState.Equation];

            /*
            Console.WriteLine(
                "{0} : {1} -> {2}",
                OpenglBlendEquation, OpenglFunctionSource, OpenglFunctionDestination
            );
            */

            GL.glBlendEquation(openglBlendEquation);
            GL.glBlendFunc(openglFunctionSource, openglFunctionDestination);

            GL.glBlendColor(
                blendingState.FixColorDestination.Red,
                blendingState.FixColorDestination.Green,
                blendingState.FixColorDestination.Blue,
                blendingState.FixColorDestination.Alpha
            );
        }

        private void PrepareState_Texture_2D(GpuStateStruct gpuState)
        {
            var textureMappingState = gpuState.TextureMappingState;
            var mipmap0 = textureMappingState.TextureState.Mipmap0;

            if (textureMappingState.Enabled)
            {
                _textureMatrix = Matrix4x4.CreateScale(
                        1.0f / mipmap0.BufferWidth,
                        1.0f / mipmap0.TextureHeight,
                        1.0f
                )
                    ;
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

        private void PrepareState_Texture_3D(GpuStateStruct gpuState)
        {
            var textureMappingState = gpuState.TextureMappingState;
            var textureState = textureMappingState.TextureState;

            if (textureMappingState.Enabled)
            {
                _textureMatrix = Matrix4x4.Identity;

                switch (textureMappingState.TextureMapMode)
                {
                    case TextureMapMode.GuTextureCoords:

                        _textureMatrix = _textureMatrix *
                                         Matrix4x4.CreateTranslation(textureState.OffsetU, textureState.OffsetV, 0) *
                                         Matrix4x4.CreateScale(textureState.ScaleU, textureState.ScaleV, 1);
                        break;
                    case TextureMapMode.GuTextureMatrix:
                        switch (gpuState.TextureMappingState.TextureProjectionMapMode)
                        {
                            default:
                                Console.Error.WriteLine("NotImplemented: GU_TEXTURE_MATRIX: {0}",
                                    gpuState.TextureMappingState.TextureProjectionMapMode);
                                break;
                        }
                        break;
                    case TextureMapMode.GuEnvironmentMap:
                        Console.Error.WriteLine("NotImplemented: GU_ENVIRONMENT_MAP");
                        break;
                    default:
                        Console.Error.WriteLine("NotImplemented TextureMappingState->TextureMapMode: " +
                                                textureMappingState.TextureMapMode);
                        break;
                }
            }
        }

        private void PrepareState_Texture_Common(GpuStateStruct gpuState)
        {
            var textureMappingState = gpuState.TextureMappingState;
            //var ClutState = &TextureMappingState->ClutState;
            var textureState = textureMappingState.TextureState;

            if (!GL.EnableDisable(GL.GL_TEXTURE_2D, textureMappingState.Enabled)) return;

            if (VertexType.Transform2D)
            {
                PrepareState_Texture_2D(gpuState);
            }
            else
            {
                PrepareState_Texture_3D(gpuState);
            }

            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            //glPixelStorei(GL_UNPACK_ALIGNMENT, 1);

            RenderbufferManager.TextureCacheGetAndBind(gpuState);
            //CurrentTexture.Save("test.png");

            //GL.glTexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvModeTranslate[(int)TextureState->Effect]);
        }
    }
}