//#define DISABLE_SKINNING
//#define SLOW_SIMPLE_RENDER_TARGET

//#define DEBUG_PRIM

#if !RELEASE
//#define DEBUG_VERTEX_TYPE
#endif

using System;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Transactions;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
//using Cloo;
//using Cloo.Bindings;
using CSPspEmu.Core.Gpu.Formats;
using CSPspEmu.Core.Types;
using CSharpPlatform;
using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.Impl.Opengl.Utils;
using CSPspEmu.Core.Gpu.Impl.Opengl.Modules;
using CSPspEmu.Core.Gpu.VertexReading;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public sealed unsafe partial class OpenglGpuImpl : GpuImpl, IInjectInitialize
    {

        /// <summary>
        /// 
        /// </summary>
        public TextureCacheOpengl TextureCache;

        /// <summary>
        /// 
        /// </summary>
        private GpuStateStruct* GpuState;

        public override void InvalidateCache(uint address, int size)
        {
            //ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.White, () =>
            //{
            //	//foreach ()
            //	//Console.WriteLine("OnMemoryWrite: {0:X8}, {1}", Address, Size);
            //	//foreach (var DrawBufferTexture in DrawBufferTextures)
            //	//{
            //	//	Console.WriteLine("::{0:X8}", DrawBufferTexture.Key.Address);
            //	//}
            //});
        }

        //public static object GpuLock = new object();

        public class FastList<T>
        {
            public int Length = 0;
            public T[] Buffer = new T[1024];

            public void Reset() => Length = 0;

            public void Add(T item)
            {
                if (Length >= Buffer.Length) Buffer = Buffer.ResizedCopy(Buffer.Length * 2);
                Buffer[Length++] = item;
            }
        }

        private readonly FastList<VertexInfoVector3F> _verticesPosition = new FastList<VertexInfoVector3F>();
        private readonly FastList<VertexInfoVector3F> _verticesNormal = new FastList<VertexInfoVector3F>();
        private readonly FastList<VertexInfoVector3F> _verticesTexcoords = new FastList<VertexInfoVector3F>();
        private readonly FastList<VertexInfoColor> _verticesColors = new FastList<VertexInfoColor>();
        private readonly FastList<VertexInfoWeights> _verticesWeights = new FastList<VertexInfoWeights>();

        private GLBuffer _verticesPositionBuffer;
        private GLBuffer _verticesNormalBuffer;
        private GLBuffer _verticesTexcoordsBuffer;
        private GLBuffer _verticesColorsBuffer;
        private GLBuffer _verticesWeightsBuffer;

        private FastList<uint> _indicesList = new FastList<uint>();

        private Matrix4F _worldViewProjectionMatrix = default(Matrix4F);
        private Matrix4F _textureMatrix = default(Matrix4F);

        public RenderbufferManager RenderbufferManager { get; private set; }
        private GLShader _shader;

        // ReSharper disable FieldCanBeMadeReadOnly.Global
        // ReSharper disable UnassignedField.Global
        // ReSharper disable InconsistentNaming
        public class ShaderInfoClass
        {
            public GlUniform matrixWorldViewProjection;
            public GlUniform matrixTexture;
            public GlUniform matrixBones;

            public GlUniform hasPerVertexColor;
            public GlUniform hasTexture;
            public GlUniform hasReversedNormal;
            public GlUniform clearingMode;

            public GlUniform texture0;
            public GlUniform uniformColor;

            public GlUniform colorTest;

            public GlUniform alphaTest;
            public GlUniform alphaFunction;
            public GlUniform alphaValue;
            public GlUniform alphaMask;

            public GlUniform weightCount;

            public GlUniform tfx;
            public GlUniform tcc;

            public GlAttribute vertexPosition;
            public GlAttribute vertexTexCoords;
            public GlAttribute vertexColor;
            public GlAttribute vertexNormal;

            public GlAttribute vertexWeight0;
            public GlAttribute vertexWeight1;
            public GlAttribute vertexWeight2;
            public GlAttribute vertexWeight3;
            public GlAttribute vertexWeight4;
            public GlAttribute vertexWeight5;
            public GlAttribute vertexWeight6;
            public GlAttribute vertexWeight7;
        }

        ShaderInfoClass ShaderInfo = new ShaderInfoClass();

        [Inject] InjectContext InjectContext;

        void IInjectInitialize.Initialize()
        {
            RenderbufferManager = new RenderbufferManager(this);
            TextureCache = new TextureCacheOpengl(Memory, this, InjectContext);
            VertexReader = new VertexReader();
        }

        private void DrawInitVertices()
        {
            //Console.WriteLine(WGL.wglGetCurrentContext());
            _verticesPositionBuffer = GLBuffer.Create();
            _verticesNormalBuffer = GLBuffer.Create();
            _verticesTexcoordsBuffer = GLBuffer.Create();
            _verticesColorsBuffer = GLBuffer.Create();
            _verticesWeightsBuffer = GLBuffer.Create();
            _shader = new GLShader(
                typeof(OpenglGpuImpl).Assembly.GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.Opengl.shader.vert")
                    .ReadAllContentsAsString(),
                typeof(OpenglGpuImpl).Assembly.GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.Opengl.shader.frag")
                    .ReadAllContentsAsString()
            );
            Console.WriteLine("###################################");
            foreach (var uniform in _shader.Uniforms) Console.WriteLine(uniform);
            foreach (var attribute in _shader.Attributes) Console.WriteLine(attribute);
            Console.WriteLine("###################################");

            _shader.BindUniformsAndAttributes(ShaderInfo);
        }

        private void PrepareDrawStateFirst()
        {
            if (_shader == null) DrawInitVertices();

            ShaderInfo.matrixWorldViewProjection.Set(_worldViewProjectionMatrix);
            ShaderInfo.matrixTexture.Set(_textureMatrix);
            ShaderInfo.uniformColor.Set(GpuState->LightingState.AmbientModelColor.ToVector4());
            ShaderInfo.hasPerVertexColor.Set(VertexType.HasColor);
            ShaderInfo.clearingMode.Set(GpuState->ClearingMode);
            ShaderInfo.hasTexture.Set(GpuState->TextureMappingState.Enabled);

            ShaderInfo.weightCount.Set(VertexType.RealSkinningWeightCount);
            //ShaderInfo.weightCount.Set(0);
            if (VertexType.HasWeight)
            {
                ShaderInfo.matrixBones.Set(new[]
                {
                    GpuState->SkinningState.BoneMatrix0.Matrix4,
                    GpuState->SkinningState.BoneMatrix1.Matrix4,
                    GpuState->SkinningState.BoneMatrix2.Matrix4,
                    GpuState->SkinningState.BoneMatrix3.Matrix4,
                    GpuState->SkinningState.BoneMatrix4.Matrix4,
                    GpuState->SkinningState.BoneMatrix5.Matrix4,
                    GpuState->SkinningState.BoneMatrix6.Matrix4,
                    GpuState->SkinningState.BoneMatrix7.Matrix4,
                });
            }

            if (VertexType.HasTexture && GpuState->TextureMappingState.Enabled)
            {
                var textureState = &GpuState->TextureMappingState.TextureState;

                ShaderInfo.tfx.Set((int) textureState->Effect);
                ShaderInfo.tcc.Set((int) textureState->ColorComponent);
                ShaderInfo.colorTest.NoWarning().Set(GpuState->ColorTestState.Enabled);

                ShaderInfo.alphaTest.Set(GpuState->AlphaTestState.Enabled);
                ShaderInfo.alphaFunction.Set((int) GpuState->AlphaTestState.Function);
                ShaderInfo.alphaMask.NoWarning().Set(GpuState->AlphaTestState.Mask);
                ShaderInfo.alphaValue.Set(GpuState->AlphaTestState.Value);

                //Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", TextureState->Effect, TextureState->ColorComponent, GpuState->BlendingState.Enabled, GpuState->BlendingState.FunctionSource, GpuState->BlendingState.FunctionDestination, GpuState->ColorTestState.Enabled);

                ShaderInfo.texture0.Set(GLTextureUnit.CreateAtIndex(0)
                    .SetWrap(
                        (GLWrap) ((textureState->WrapU == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE),
                        (GLWrap) ((textureState->WrapV == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE)
                    )
                    .SetFiltering(
                        (GLScaleFilter) ((textureState->FilterMinification == TextureFilter.Linear)
                            ? GL.GL_LINEAR
                            : GL.GL_NEAREST),
                        (GLScaleFilter) ((textureState->FilterMagnification == TextureFilter.Linear)
                            ? GL.GL_LINEAR
                            : GL.GL_NEAREST)
                    )
                    .SetTexture(RenderbufferManager.TextureCacheGetAndBind(GpuState))
                );
            }
        }

        private void DrawVertices(GLGeometry type)
        {
            ShaderInfo.hasReversedNormal.NoWarning().Set(VertexType.ReversedNormal);

            _shader.Draw(type, _indicesList.Buffer, _indicesList.Length, () =>
            {
                if (VertexType.HasPosition)
                {
                    _verticesPositionBuffer.SetData(_verticesPosition.Buffer, 0, _verticesPosition.Length);
                    ShaderInfo.vertexPosition.SetData<float>(_verticesPositionBuffer, 3, 0, sizeof(VertexInfoVector3F),
                        false);
                }

                if (VertexType.HasTexture)
                {
                    _verticesTexcoordsBuffer.SetData(_verticesTexcoords.Buffer, 0, _verticesTexcoords.Length);
                    ShaderInfo.vertexTexCoords.SetData<float>(_verticesTexcoordsBuffer, 3, 0,
                        sizeof(VertexInfoVector3F),
                        false);
                }

                if (VertexType.HasColor)
                {
                    _verticesColorsBuffer.SetData(_verticesColors.Buffer, 0, _verticesColors.Length);
                    ShaderInfo.vertexColor.SetData<float>(_verticesColorsBuffer, 4, 0, sizeof(VertexInfoColor), false);
                }

                if (VertexType.HasNormal)
                {
                    _verticesNormalBuffer.SetData(_verticesNormal.Buffer, 0, _verticesNormal.Length);
                    ShaderInfo.vertexNormal.NoWarning()
                        .SetData<float>(_verticesNormalBuffer, 4, 0, sizeof(VertexInfoVector3F), false);
                }

                if (VertexType.HasWeight)
                {
                    _verticesWeightsBuffer.SetData(_verticesWeights.Buffer, 0, _verticesWeights.Length);
                    var vertexWeights = new[]
                    {
                        ShaderInfo.vertexWeight0, ShaderInfo.vertexWeight1, ShaderInfo.vertexWeight2,
                        ShaderInfo.vertexWeight3, ShaderInfo.vertexWeight4, ShaderInfo.vertexWeight5,
                        ShaderInfo.vertexWeight6, ShaderInfo.vertexWeight7
                    };
                    for (var n = 0; n < VertexType.RealSkinningWeightCount; n++)
                    {
                        vertexWeights[n].SetData<float>(_verticesWeightsBuffer, 1, n * sizeof(float),
                            sizeof(VertexInfoWeights), false);
                    }
                }
            });
        }

        private void ResetVertex()
        {
            _verticesPosition.Reset();
            _verticesNormal.Reset();
            _verticesWeights.Reset();
            _verticesTexcoords.Reset();
            _verticesColors.Reset();

            _indicesList.Reset();
        }

        private void PutVertices(params VertexInfo[] vertexInfoList)
        {
            foreach (var vertexInfo in vertexInfoList) PutVertex(vertexInfo);
        }

        private void PutVertexIndexRelative(int offset)
        {
            PutVertexIndex(_verticesPosition.Length + offset);
        }

        private void PutVertexIndex(int vertexIndex)
        {
            _indicesList.Add((uint) vertexIndex);
        }

        /// <summary>
        /// </summary>
        /// <param name="vertexInfo"></param>
        private void PutVertex(VertexInfo vertexInfo)
        {
            _CapturePutVertex(ref vertexInfo);

            PutVertexIndex(_verticesPosition.Length);

            _verticesPosition.Add(new VertexInfoVector3F(vertexInfo.Position));
            _verticesNormal.Add(new VertexInfoVector3F(vertexInfo.Normal));
            _verticesTexcoords.Add(new VertexInfoVector3F(vertexInfo.Texture));
            _verticesColors.Add(new VertexInfoColor(vertexInfo.Color));
            _verticesWeights.Add(new VertexInfoWeights(vertexInfo));
        }

        private object PspWavefrontObjWriterLock = new object();
        private PspWavefrontObjWriter _pspWavefrontObjWriter = null;

        public override void StartCapture()
        {
            lock (PspWavefrontObjWriterLock)
            {
                _pspWavefrontObjWriter =
                    new PspWavefrontObjWriter(
                        new WavefrontObjWriter(ApplicationPaths.MemoryStickRootFolder + "/gpu_frame.obj"));
            }
        }

        public override void EndCapture()
        {
            lock (PspWavefrontObjWriterLock)
            {
                _pspWavefrontObjWriter.End();
                _pspWavefrontObjWriter = null;
            }
        }

        private void _CapturePrimitive(GuPrimitiveType primitiveType, uint vertexAddress, int vetexCount,
            ref VertexTypeStruct vertexType, Action action)
        {
            if (_pspWavefrontObjWriter != null)
            {
                lock (PspWavefrontObjWriterLock)
                    _pspWavefrontObjWriter.StartPrimitive(GpuState, primitiveType, vertexAddress, vetexCount,
                        ref vertexType);
                try
                {
                    action();
                }
                finally
                {
                    lock (PspWavefrontObjWriterLock) _pspWavefrontObjWriter.EndPrimitive();
                }
            }
            else
            {
                action();
            }
        }

        private void _CapturePutVertex(ref VertexInfo vertexInfo)
        {
            if (_pspWavefrontObjWriter != null)
            {
                lock (this) _pspWavefrontObjWriter.PutVertex(ref vertexInfo);
            }
        }

        //private static readonly GuPrimitiveType[] patch_prim_types = { GuPrimitiveType.TriangleStrip, GuPrimitiveType.LineStrip, GuPrimitiveType.Points };
        //public override void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuStateStruct, VertexInfo[,] Patch, int UCount, int VCount)
        //{
        //	//GpuState->TextureMappingState.Enabled = true;
        //
        //	//ResetState();
        //	OpenglGpuImplCommon.PrepareStateCommon(GpuState, ScaleViewport);
        //	PrepareStateDraw(GpuState);
        //	OpenglGpuImplMatrix.PrepareStateMatrix(GpuState, ref ModelViewProjectionMatrix);
        //
        //#if true
        //	PrepareState_Texture_Common(GpuState);
        //	PrepareState_Texture_3D(GpuState);
        //#endif
        //
        //	//GL.ActiveTexture(TextureUnit.Texture0);
        //	//GL.Disable(EnableCap.Texture2D);
        //
        //	var VertexType = GpuStateStruct->VertexState.Type;
        //
        //	//GL.Color3(Color.White);
        //
        //	int s_len = Patch.GetLength(0);
        //	int t_len = Patch.GetLength(1);
        //
        //	float s_len_float = s_len;
        //	float t_len_float = t_len;
        //
        //	var Mipmap0 = &GpuStateStruct->TextureMappingState.TextureState.Mipmap0;
        //
        //	float MipmapWidth = Mipmap0->TextureWidth;
        //	float MipmapHeight = Mipmap0->TextureHeight;
        //
        //	//float MipmapWidth = 1f;
        //	//float MipmapHeight = 1f;
        //
        //	ResetVertex();
        //	for (int t = 0; t < t_len - 1; t++)
        //	{
        //		for (int s = 0; s < s_len - 1; s++)
        //		{
        //			var VertexInfo1 = Patch[s + 0, t + 0];
        //			var VertexInfo2 = Patch[s + 0, t + 1];
        //			var VertexInfo3 = Patch[s + 1, t + 1];
        //			var VertexInfo4 = Patch[s + 1, t + 0];
        //
        //			if (VertexType.Texture != VertexTypeStruct.NumericEnum.Void)
        //			{
        //				VertexInfo1.Texture.X = ((float)s + 0) * MipmapWidth / s_len_float;
        //				VertexInfo1.Texture.Y = ((float)t + 0) * MipmapHeight / t_len_float;
        //
        //				VertexInfo2.Texture.X = ((float)s + 0) * MipmapWidth / s_len_float;
        //				VertexInfo2.Texture.Y = ((float)t + 1) * MipmapHeight / t_len_float;
        //
        //				VertexInfo3.Texture.X = ((float)s + 1) * MipmapWidth / s_len_float;
        //				VertexInfo3.Texture.Y = ((float)t + 1) * MipmapHeight / t_len_float;
        //
        //				VertexInfo4.Texture.X = ((float)s + 1) * MipmapWidth / s_len_float;
        //				VertexInfo4.Texture.Y = ((float)t + 0) * MipmapHeight / t_len_float;
        //			}
        //
        //			PutVertex(ref VertexType, VertexInfo1);
        //			PutVertex(ref VertexType, VertexInfo2);
        //			PutVertex(ref VertexType, VertexInfo3);
        //
        //			PutVertex(ref VertexType, VertexInfo1);
        //			PutVertex(ref VertexType, VertexInfo3);
        //			PutVertex(ref VertexType, VertexInfo4);
        //
        //			//GL.Color3(Color.White);
        //			//Console.WriteLine("{0}, {1} : {2}", s, t, VertexInfo1);
        //		}
        //	}
        //	DrawVertices(GLGeometry.GL_TRIANGLES);
        //}

        bool _doPrimStart;
        VertexTypeStruct _cachedVertexType;
        GuPrimitiveType _primitiveType;
        GLRenderTarget _logicOpsRenderTarget;

        public override void PrimStart(GlobalGpuState globalGpuState, GpuStateStruct* gpuState,
            GuPrimitiveType primitiveType)
        {
            GpuState = gpuState;
            _primitiveType = primitiveType;
            _doPrimStart = true;
            ResetVertex();


            if (_shader != null)
            {
                _shader.GetUniform("lopEnabled").Set(gpuState->LogicalOperationState.Enabled);

                if (gpuState->LogicalOperationState.Enabled)
                {
                    if (_logicOpsRenderTarget == null)
                    {
                        _logicOpsRenderTarget = GLRenderTarget.Create(512, 272, RenderTargetLayers.Color);
                    }
                    GLRenderTarget.CopyFromTo(GLRenderTarget.Current, _logicOpsRenderTarget);
                    _shader.GetUniform("backtex").Set(GLTextureUnit.CreateAtIndex(1).SetFiltering(GLScaleFilter.Linear)
                        .SetWrap(GLWrap.ClampToEdge).SetTexture(_logicOpsRenderTarget.TextureColor));

                    _shader.GetUniform("lop").Set((int) gpuState->LogicalOperationState.Operation);

                    //new Bitmap(512, 272).SetChannelsDataInterleaved(LogicOpsRenderTarget.ReadPixels(), BitmapChannelList.RGBA).Save(@"c:\temp\test.png");
                }
            }
        }

        public override void PrimEnd()
        {
            EndVertex();
        }

        private void EndVertex()
        {
            DrawVertices(ConvertGLGeometry(_primitiveType));
            ResetVertex();
        }

        /// <summary>
        /// </summary>
        /// <param name="vertexCount"></param>
        public override void Prim(ushort vertexCount)
        {
            VertexType = GpuState->VertexState.Type;

            if (_doPrimStart || (VertexType != _cachedVertexType))
            {
                _cachedVertexType = VertexType;
                _doPrimStart = false;

                OpenglGpuImplCommon.PrepareStateCommon(GpuState, ScaleViewport);

                if (GpuState->ClearingMode)
                {
                    OpenglGpuImplClear.PrepareStateClear(GpuState);
                }
                else
                {
                    PrepareStateDraw(GpuState);
                }

                OpenglGpuImplMatrix.PrepareStateMatrix(GpuState, out _worldViewProjectionMatrix);
                PrepareDrawStateFirst();
            }

            //if (PrimitiveType == GuPrimitiveType.TriangleStrip) VertexCount++;

            uint morpingVertexCount, totalVerticesWithoutMorphing;
            PreparePrim(GpuState, out totalVerticesWithoutMorphing, vertexCount, out morpingVertexCount);

            var z = 0;

            //for (int n = 0; n < MorpingVertexCount; n++) Console.Write("{0}, ", Morphs[n]); Console.WriteLine("");

            //int VertexInfoFloatCount = (sizeof(Color4F) + sizeof(Vector3F) * 3) / sizeof(float);
            var vertexInfoFloatCount = (sizeof(VertexInfo)) / sizeof(float);
            fixed (VertexInfo* verticesPtr = Vertices)
            {
                if (morpingVertexCount == 1)
                {
                    VertexReader.ReadVertices(0, verticesPtr, (int) totalVerticesWithoutMorphing);
                }
                else
                {
                    VertexInfo tempVertexInfo;
                    var componentsIn = (float*) &tempVertexInfo;
                    for (var n = 0; n < totalVerticesWithoutMorphing; n++)
                    {
                        var componentsOut = (float*) &verticesPtr[n];
                        for (var cc = 0; cc < vertexInfoFloatCount; cc++) componentsOut[cc] = 0;
                        for (var m = 0; m < morpingVertexCount; m++)
                        {
                            VertexReader.ReadVertex(z++, &tempVertexInfo);
                            for (int cc = 0; cc < vertexInfoFloatCount; cc++)
                                componentsOut[cc] += componentsIn[cc] * GpuState->MorphingState.MorphWeight[m];
                        }
                        verticesPtr[n].Normal = verticesPtr[n].Normal.Normalize();
                    }
                }
            }

            _CapturePrimitive(_primitiveType, GpuState->GetAddressRelativeToBaseOffset(GpuState->VertexAddress),
                vertexCount, ref VertexType, () =>
                {
                    // Continuation
                    if (_indicesList.Length > 0)
                    {
                        switch (_primitiveType)
                        {
                            // Degenerate.
                            case GuPrimitiveType.TriangleStrip:
                            case GuPrimitiveType.Sprites:
                                if (vertexCount > 0)
                                {
                                    PutVertexIndexRelative(-1);
                                    PutVertexIndexRelative(0);
                                }
                                break;
                            // Can't degenerate, flush.
                            default:
                                EndVertex();
                                break;
                        }
                    }

                    if (_primitiveType == GuPrimitiveType.Sprites)
                    {
                        GL.glDisable(GL.GL_CULL_FACE);
                        for (int n = 0; n < vertexCount; n += 2)
                        {
                            VertexInfo v0, v1, v2, v3;

                            readVertex(n + 0, out v0);
                            readVertex(n + 1, out v3);

                            VertexUtils.GenerateTriangleStripFromSpriteVertices(ref v0, out v1, out v2, ref v3);

                            if (n > 0)
                            {
                                PutVertexIndexRelative(-1);
                                PutVertexIndexRelative(0);
                            }

                            PutVertices(v0, v1, v2, v3);
                        }
                    }
                    else
                    {
                        VertexInfo VertexInfo;
                        //Console.Error.WriteLine("{0} : {1} : {2}", BeginMode, VertexCount, VertexType.Index);
                        for (int n = 0; n < vertexCount; n++)
                        {
                            readVertex(n, out VertexInfo);
                            PutVertex(VertexInfo);
                        }
                    }
                });
        }

        private GLGeometry ConvertGLGeometry(GuPrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case GuPrimitiveType.Lines: return GLGeometry.GL_LINES;
                case GuPrimitiveType.LineStrip: return GLGeometry.GL_LINE_STRIP;
                case GuPrimitiveType.Triangles: return GLGeometry.GL_TRIANGLES;
                case GuPrimitiveType.Points: return GLGeometry.GL_POINTS;
                case GuPrimitiveType.TriangleFan: return GLGeometry.GL_TRIANGLE_FAN;
                case GuPrimitiveType.TriangleStrip: return GLGeometry.GL_TRIANGLE_STRIP;
                case GuPrimitiveType.Sprites: return GLGeometry.GL_TRIANGLE_STRIP;
                default: throw (new NotImplementedException("Not implemented PrimitiveType:'" + primitiveType + "'"));
            }
        }

        public override void BeforeDraw(GpuStateStruct* gpuState)
        {
            RenderbufferManager.BindCurrentDrawBufferTexture(gpuState);
        }

        public override void DrawVideo(uint frameBufferAddress, OutputPixel* outputPixel, int width, int height)
        {
            RenderbufferManager.DrawVideo(frameBufferAddress, outputPixel, width, height);
        }


        [HandleProcessCorruptedStateExceptions]
        public override void Finish(GpuStateStruct* gpuState)
        {
        }

        public override void End(GpuStateStruct* gpuState)
        {
            //PrepareWrite(GpuState);
        }

        public override void Sync(GpuStateStruct* gpuState)
        {
        }

        public override void TextureFlush(GpuStateStruct* gpuState)
        {
            TextureCache.RecheckAll();
        }

        public override void TextureSync(GpuStateStruct* gpuState)
        {
        }

        public override void AddedDisplayList()
        {
        }

        public override PluginInfo PluginInfo => new PluginInfo
        {
            Name = "OpenGl 2.0 (|ES)",
            Version = "0.1",
        };

        public override bool IsWorking => true;
    }
}