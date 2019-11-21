//#define DISABLE_SKINNING
//#define SLOW_SIMPLE_RENDER_TARGET

//#define DEBUG_PRIM

#if !RELEASE
//#define DEBUG_VERTEX_TYPE
#endif

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Threading;
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
using CSharpUtils;
using CSharpUtils.Drawing;
using CSharpUtils.Extensions;
using CSPspEmu.Core.Gpu.Impl.Opengl.Utils;
using CSPspEmu.Core.Gpu.Impl.Opengl.Modules;
using CSPspEmu.Core.Gpu.VertexReading;
using CSPspEmu.Utils;
using OpenTK.Graphics;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public unsafe class OpenglGpuImpl : GpuImpl, IInjectInitialize
    {

        /// <summary>
        /// 
        /// </summary>
        public TextureCacheOpengl TextureCache;

        /// <summary>
        /// 
        /// </summary>
        private GpuStateStruct GpuState;

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

        private readonly FastList<Vector3> _verticesPosition = new FastList<Vector3>();
        private readonly FastList<Vector3> _verticesNormal = new FastList<Vector3>();
        private readonly FastList<Vector3> _verticesTexcoords = new FastList<Vector3>();
        private readonly FastList<RgbaFloat> _verticesColors = new FastList<RgbaFloat>();
        private readonly FastList<VertexInfoWeights> _verticesWeights = new FastList<VertexInfoWeights>();

        private GLBuffer _verticesPositionBuffer;
        private GLBuffer _verticesNormalBuffer;
        private GLBuffer _verticesTexcoordsBuffer;
        private GLBuffer _verticesColorsBuffer;
        private GLBuffer _verticesWeightsBuffer;

        private FastList<uint> _indicesList = new FastList<uint>();

        private Matrix4x4 _worldViewProjectionMatrix = Matrix4x4.Identity;
        private Matrix4x4 _textureMatrix = Matrix4x4.Identity;

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
            //Console.WriteLine(typeof(OpenglGpuImpl).Assembly.GetManifestResourceNames().ToStringList());
            _shader = new GLShader(
                typeof(OpenglGpuImpl).Assembly.GetManifestResourceStream("CSPspEmu.GPU.Impl.Opengl.shader.vert")
                    .ReadAllContentsAsString(),
                typeof(OpenglGpuImpl).Assembly.GetManifestResourceStream("CSPspEmu.GPU.Impl.Opengl.shader.frag")
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
            ShaderInfo.uniformColor.Set(GpuState.LightingState.AmbientModelColor.ToVector4());
            ShaderInfo.hasPerVertexColor.Set(VertexType.HasColor);
            ShaderInfo.clearingMode.Set(GpuState.ClearingMode);
            ShaderInfo.hasTexture.Set(GpuState.TextureMappingState.Enabled);

            ShaderInfo.weightCount.Set(VertexType.RealSkinningWeightCount);
            //ShaderInfo.weightCount.Set(0);
            if (VertexType.HasWeight)
            {
                ShaderInfo.matrixBones.Set(new[]
                {
                    GpuState.SkinningState.BoneMatrix0,
                    GpuState.SkinningState.BoneMatrix1,
                    GpuState.SkinningState.BoneMatrix2,
                    GpuState.SkinningState.BoneMatrix3,
                    GpuState.SkinningState.BoneMatrix4,
                    GpuState.SkinningState.BoneMatrix5,
                    GpuState.SkinningState.BoneMatrix6,
                    GpuState.SkinningState.BoneMatrix7,
                });
            }

            if (VertexType.HasTexture && GpuState.TextureMappingState.Enabled)
            {
                var textureState = GpuState.TextureMappingState.TextureState;

                ShaderInfo.tfx.Set((int) textureState.Effect);
                ShaderInfo.tcc.Set((int) textureState.ColorComponent);
                ShaderInfo.colorTest.NoWarning().Set(GpuState.ColorTestState.Enabled);

                ShaderInfo.alphaTest.Set(GpuState.AlphaTestState.Enabled);
                ShaderInfo.alphaFunction.Set((int) GpuState.AlphaTestState.Function);
                ShaderInfo.alphaMask.NoWarning().Set(GpuState.AlphaTestState.Mask);
                ShaderInfo.alphaValue.Set(GpuState.AlphaTestState.Value);

                //Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", TextureState->Effect, TextureState->ColorComponent, GpuState->BlendingState.Enabled, GpuState->BlendingState.FunctionSource, GpuState->BlendingState.FunctionDestination, GpuState->ColorTestState.Enabled);

                ShaderInfo.texture0.Set(GLTextureUnit.CreateAtIndex(0)
                    .SetWrap(
                        (GLWrap) (textureState.WrapU == WrapMode.Repeat ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE),
                        (GLWrap) (textureState.WrapV == WrapMode.Repeat ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE)
                    )
                    .SetFiltering(
                        (GLScaleFilter) (textureState.FilterMinification == TextureFilter.Linear
                            ? GL.GL_LINEAR
                            : GL.GL_NEAREST),
                        (GLScaleFilter) (textureState.FilterMagnification == TextureFilter.Linear
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
                    ShaderInfo.vertexPosition.SetData<float>(_verticesPositionBuffer, 3, 0, sizeof(Vector3),
                        false);
                }

                if (VertexType.HasTexture)
                {
                    _verticesTexcoordsBuffer.SetData(_verticesTexcoords.Buffer, 0, _verticesTexcoords.Length);
                    ShaderInfo.vertexTexCoords.SetData<float>(_verticesTexcoordsBuffer, 3, 0,
                        sizeof(Vector3),
                        false);
                }

                if (VertexType.HasColor)
                {
                    _verticesColorsBuffer.SetData(_verticesColors.Buffer, 0, _verticesColors.Length);
                    ShaderInfo.vertexColor.SetData<float>(_verticesColorsBuffer, 4, 0, sizeof(RgbaFloat), false);
                }

                if (VertexType.HasNormal)
                {
                    _verticesNormalBuffer.SetData(_verticesNormal.Buffer, 0, _verticesNormal.Length);
                    ShaderInfo.vertexNormal.NoWarning()
                        .SetData<float>(_verticesNormalBuffer, 4, 0, sizeof(Vector3), false);
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

            _verticesPosition.Add(vertexInfo.Position.ToRVector3());
            _verticesNormal.Add(vertexInfo.Normal.ToRVector3());
            _verticesTexcoords.Add(vertexInfo.Texture.ToRVector3());
            _verticesColors.Add(new RgbaFloat(vertexInfo.Color));
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

        public override void PrimStart(GlobalGpuState globalGpuState, GpuStateStruct gpuState,
            GuPrimitiveType primitiveType)
        {
            GpuState = gpuState;
            _primitiveType = primitiveType;
            _doPrimStart = true;
            ResetVertex();


            if (_shader != null)
            {
                _shader.GetUniform("lopEnabled").Set(gpuState.LogicalOperationState.Enabled);

                if (gpuState.LogicalOperationState.Enabled)
                {
                    if (_logicOpsRenderTarget == null)
                    {
                        _logicOpsRenderTarget = GLRenderTarget.Create(512, 272, RenderTargetLayers.Color);
                    }
                    GLRenderTarget.CopyFromTo(GLRenderTarget.Current, _logicOpsRenderTarget);
                    _shader.GetUniform("backtex").Set(GLTextureUnit.CreateAtIndex(1).SetFiltering(GLScaleFilter.Linear)
                        .SetWrap(GLWrap.ClampToEdge).SetTexture(_logicOpsRenderTarget.TextureColor));

                    _shader.GetUniform("lop").Set((int) gpuState.LogicalOperationState.Operation);

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
            VertexType = GpuState.VertexState.Type;

            if (_doPrimStart || VertexType != _cachedVertexType)
            {
                _cachedVertexType = VertexType;
                _doPrimStart = false;

                OpenglGpuImplCommon.PrepareStateCommon(GpuState, ScaleViewport);

                if (GpuState.ClearingMode)
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
            var vertexInfoFloatCount = sizeof(VertexInfo) / sizeof(float);
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
                            for (var cc = 0; cc < vertexInfoFloatCount; cc++)
                                componentsOut[cc] += componentsIn[cc] * GpuState.MorphingState.MorphWeight(m);
                        }
                        verticesPtr[n].Normal = verticesPtr[n].Normal.Normalize();
                    }
                }
            }

            _CapturePrimitive(_primitiveType, GpuState.GetAddressRelativeToBaseOffset(GpuState.VertexAddress),
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
                        for (var n = 0; n < vertexCount; n += 2)
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
                        for (var n = 0; n < vertexCount; n++)
                        {
                            readVertex(n, out VertexInfo);
                            PutVertex(VertexInfo);
                        }
                    }
                });
        }

        private static GLGeometry ConvertGLGeometry(GuPrimitiveType primitiveType) =>
            primitiveType switch
            {
                GuPrimitiveType.Lines => GLGeometry.GL_LINES,
                GuPrimitiveType.LineStrip => GLGeometry.GL_LINE_STRIP,
                GuPrimitiveType.Triangles => GLGeometry.GL_TRIANGLES,
                GuPrimitiveType.Points => GLGeometry.GL_POINTS,
                GuPrimitiveType.TriangleFan => GLGeometry.GL_TRIANGLE_FAN,
                GuPrimitiveType.TriangleStrip => GLGeometry.GL_TRIANGLE_STRIP,
                GuPrimitiveType.Sprites => GLGeometry.GL_TRIANGLE_STRIP,
                _ => throw new NotImplementedException("Not implemented PrimitiveType:'" + primitiveType + "'")
            };

        public override void BeforeDraw(GpuStateStruct gpuState)
        {
            RenderbufferManager.BindCurrentDrawBufferTexture(gpuState);
        }

        public override void DrawVideo(uint frameBufferAddress, OutputPixel* outputPixel, int width, int height)
        {
            RenderbufferManager.DrawVideo(frameBufferAddress, outputPixel, width, height);
        }


        [HandleProcessCorruptedStateExceptions]
        public override void Finish(GpuStateStruct gpuState)
        {
        }

        public override void End(GpuStateStruct gpuState)
        {
            //PrepareWrite(GpuState);
        }

        public override void Sync(GpuStateStruct gpuState)
        {
        }

        public override void TextureFlush(GpuStateStruct gpuState)
        {
            TextureCache.RecheckAll();
        }

        public override void TextureSync(GpuStateStruct gpuState)
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
        
        static public GraphicsContext MyContext;
        
        //Thread CThread;
        AutoResetEvent StopEvent = new AutoResetEvent(false);
        
        

        bool Running = true;

        /// <summary>
        /// 
        /// </summary>
        public static IGlContext OpenglContext;

        /// <summary>
        /// 
        /// </summary>
        public static bool AlreadyInitialized;

        public bool IsCurrentWindow;

        public override void SetCurrent()
        {
            if (!IsCurrentWindow)
            {
                OpenglContext.MakeCurrent();
                IsCurrentWindow = true;
            }
        }

        public override void UnsetCurrent()
        {
            OpenglContext.ReleaseCurrent();
            IsCurrentWindow = false;
        }

        public static string GlGetString(int name) => GL.GetString(name);

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="http://www.opentk.com/doc/graphics/graphicscontext"/>
        //[HandleProcessCorruptedStateExceptions]
        public override void InitSynchronizedOnce()
        {
            //Memory.WriteBytesHook += OnMemoryWrite;
            ScaleViewport = PspStoredConfig.RenderScale;

            if (!AlreadyInitialized)
            {
                AlreadyInitialized = true;
                var completedEvent = new AutoResetEvent(false);

                new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(GlobalConfig.ThreadCultureName);

                    OpenglContext = GlContextFactory.CreateWindowless();
                    OpenglContext.MakeCurrent();

                    try
                    {
                        Console.Out.WriteLineColored(ConsoleColor.White, "## OpenGL Context Version: {0}, {1}",
                            GlGetString(GL.GL_VERSION), GlGetString(GL.GL_RENDERER));
                        Console.Out.WriteLineColored(ConsoleColor.White, "## Depth Bits: {0}",
                            GL.glGetInteger(GL.GL_DEPTH_BITS));
                        Console.Out.WriteLineColored(ConsoleColor.White, "## Stencil Bits: {0}",
                            GL.glGetInteger(GL.GL_STENCIL_BITS));
                        Console.Out.WriteLineColored(ConsoleColor.White, "## Color Bits: {0},{1},{2},{3}",
                            GL.glGetInteger(GL.GL_RED_BITS), GL.glGetInteger(GL.GL_GREEN_BITS),
                            GL.glGetInteger(GL.GL_BLUE_BITS), GL.glGetInteger(GL.GL_ALPHA_BITS));

                        if (GL.glGetInteger(GL.GL_STENCIL_BITS) <= 0)
                        {
                            Console.Error.WriteLineColored(ConsoleColor.Red, "No stencil bits available!");
                            //throw new Exception("Couldn't initialize opengl");
                        }

                        OpenglContext.ReleaseCurrent();

                        completedEvent.Set();
                        Console.WriteLine("OpenglGpuImpl.Init.Start()");
                        try
                        {
                            while (Running)
                            {
                                Thread.Sleep(10);
                            }
                            StopEvent.Set();
                        }
                        finally
                        {
                            Console.WriteLine("OpenglGpuImpl.Init.End()");
                        }
                    } catch (Exception e) {
                        Console.WriteLine("OpenglGpuImpl.Init.Error: {0}", e);
                    }
                })
                {
                    Name = "GpuImplEventHandling",
                    IsBackground = true
                }.Start();

                completedEvent.WaitOne();
            }
        }

        public override void StopSynchronized()
        {
            //Running = false;
            //StopEvent.WaitOne();

            //GraphicsContext.Dispose();
            //NativeWindow.Dispose();
        }
        
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

            GL.glCullFace(gpuState.BackfaceCullingState.FrontFaceDirection == FrontFaceDirectionEnum.ClockWise
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
                if ((int) openglFunctionSource == GL.GL_CONSTANT_COLOR &&
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
        
        private void TransferToFrameBuffer(GpuStateStruct gpuState)
        {
            Console.WriteLine("TransferToFrameBuffer Not Implemented");
            //var TextureTransferState = GpuState->TextureTransferState;
            //
            //var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
            //
            //GL.PixelZoom(1, -1);
            //GL.WindowPos2(TextureTransferState.DestinationX, 272 - TextureTransferState.DestinationY);
            ////GL.PixelZoom(1, -1);
            ////GL.PixelZoom(1, 1);
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, TextureTransferState.BytesPerPixel);
            //GL.PixelStore(PixelStoreParameter.UnpackRowLength, TextureTransferState.SourceLineWidth);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, TextureTransferState.SourceX);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipRows, TextureTransferState.SourceY);
            //
            //{
            //	GL.DrawPixels(
            //		TextureTransferState.Width,
            //		TextureTransferState.Height,
            //		PixelFormat.Rgba,
            //		GlPixelFormat.OpenglPixelType,
            //		new IntPtr(Memory.PspAddressToPointerSafe(
            //			TextureTransferState.SourceAddress,
            //			TextureTransferState.Width * TextureTransferState.Height * 4
            //		))
            //	);
            //}
            //
            //GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            //GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipPixels, 0);
            //GL.PixelStore(PixelStoreParameter.UnpackSkipRows, 0);
        }

        private void TransferGeneric(GpuStateStruct gpuState)
        {
            Console.WriteLine("TransferGeneric Not Implemented");
            var textureTransferState = gpuState.TextureTransferState;

            var sourceX = textureTransferState.SourceX;
            var sourceY = textureTransferState.SourceY;
            var destinationX = textureTransferState.DestinationX;
            var destinationY = textureTransferState.DestinationY;
            var bytesPerPixel = textureTransferState.BytesPerPixel;

            var sourceTotalBytes = textureTransferState.SourceLineWidth * textureTransferState.Height * bytesPerPixel;
            var destinationTotalBytes =
                textureTransferState.DestinationLineWidth * textureTransferState.Height * bytesPerPixel;

            var sourcePointer =
                (byte*) Memory.PspAddressToPointerSafe(textureTransferState.SourceAddress.Address, sourceTotalBytes);
            var destinationPointer =
                (byte*) Memory.PspAddressToPointerSafe(textureTransferState.DestinationAddress.Address,
                    destinationTotalBytes);

            for (uint y = 0; y < textureTransferState.Height; y++)
            {
                var rowSourceOffset = (uint) (
                    textureTransferState.SourceLineWidth * (y + sourceY) + sourceX
                );
                var rowDestinationOffset = (uint) (
                    textureTransferState.DestinationLineWidth * (y + destinationY) + destinationX
                );
                PointerUtils.Memcpy(
                    destinationPointer + rowDestinationOffset * bytesPerPixel,
                    sourcePointer + rowSourceOffset * bytesPerPixel,
                    textureTransferState.Width * bytesPerPixel
                );
            }

            /*
            // Generic implementation.
            with (gpu.state.textureTransfer) {
                auto srcAddressHost = cast(ubyte*)gpu.memory.getPointer(srcAddress);
                auto dstAddressHost = cast(ubyte*)gpu.memory.getPointer(dstAddress);

                if (gpu.state.drawBuffer.isAnyAddressInBuffer([srcAddress, dstAddress])) {
                    gpu.performBufferOp(BufferOperation.STORE, BufferType.COLOR);
                }

                for (int n = 0; n < height; n++) {
                    int srcOffset = ((n + srcY) * srcLineWidth + srcX) * bpp;
                    int dstOffset = ((n + dstY) * dstLineWidth + dstX) * bpp;
                    (dstAddressHost + dstOffset)[0.. width * bpp] = (srcAddressHost + srcOffset)[0.. width * bpp];
                    //writefln("%08X <- %08X :: [%d]", dstOffset, srcOffset, width * bpp);
                }
                //std.file.write("buffer", dstAddressHost[0..512 * 272 * 4]);
            
                if (gpu.state.drawBuffer.isAnyAddressInBuffer([dstAddress])) {
                    //gpu.impl.test();
                    //gpu.impl.test("trxkick");
                    gpu.markBufferOp(BufferOperation.LOAD, BufferType.COLOR);
                }
                //gpu.impl.test();
            }
            */
        }

        public override void Transfer(GpuStateStruct gpuState)
        {
            Console.WriteLine("Transfer Not Implemented");
            //return;
            var textureTransferState = gpuState.TextureTransferState;

            if (
                textureTransferState.DestinationAddress.Address == gpuState.DrawBufferState.Address &&
                textureTransferState.DestinationLineWidth == gpuState.DrawBufferState.Width &&
                textureTransferState.BytesPerPixel == gpuState.DrawBufferState.BytesPerPixel
            )
            {
                //Console.Error.WriteLine("Writting to DrawBuffer");
                TransferToFrameBuffer(gpuState);
            }
            else
            {
                Console.Error.WriteLine("NOT Writting to DrawBuffer");
                TransferGeneric(gpuState);
                /*
                base.Transfer(GpuStateStruct);
                PrepareWrite(GpuStateStruct);
                {

                }
                PrepareRead(GpuStateStruct);
                */
            }
        }

        /*
        readonly byte[] TempBuffer = new byte[512 * 512 * 4];

        struct GlPixelFormat {
            PixelFormats pspFormat;
            float size;
            uint  internal;
            uint  external;
            uint  opengl;
            uint  isize() { return cast(uint)size; }
        }

        static const auto GlPixelFormats = [
            GlPixelFormat(PixelFormats.GU_PSM_5650,   2, 3, GL_RGB,  GL_UNSIGNED_SHORT_5_6_5_REV),
            GlPixelFormat(PixelFormats.GU_PSM_5551,   2, 4, GL_RGBA, GL_UNSIGNED_SHORT_1_5_5_5_REV),
            GlPixelFormat(PixelFormats.GU_PSM_4444,   2, 4, GL_RGBA, GL_UNSIGNED_SHORT_4_4_4_4_REV),
            GlPixelFormat(PixelFormats.GU_PSM_8888,   4, 4, GL_RGBA, GL_UNSIGNED_INT_8_8_8_8_REV),
            GlPixelFormat(PixelFormats.GU_PSM_T4  , 0.5, 1, GL_COLOR_INDEX, GL_COLOR_INDEX4_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_T8  ,   1, 1, GL_COLOR_INDEX, GL_COLOR_INDEX8_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_T16 ,   2, 4, GL_COLOR_INDEX, GL_COLOR_INDEX16_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_T32 ,   4, 4, GL_RGBA, GL_UNSIGNED_INT ), // COLOR_INDEX, GL_COLOR_INDEX32_EXT Not defined.
            GlPixelFormat(PixelFormats.GU_PSM_DXT1,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT1_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_DXT3,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT3_EXT),
            GlPixelFormat(PixelFormats.GU_PSM_DXT5,   4, 4, GL_RGBA, GL_COMPRESSED_RGBA_S3TC_DXT5_EXT),
        ];
        */

        //[HandleProcessCorruptedStateExceptions]
        //private void PrepareRead(GpuStateStruct* GpuState)
        //{
        //	if (true)
        //	{
        //		var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
        //		int Width = (int)GpuState->DrawBufferState.Width;
        //		if (Width == 0) Width = 512;
        //		int Height = 272;
        //		int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
        //		int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
        //		//GpuState->DrawBufferState.Format
        //		var Address = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address, 0);
        //		GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
        //		//Console.WriteLine("PrepareRead: {0:X}", Address);
        //
        //		try
        //		{
        //			GL.WindowPos2(0, 272);
        //			GL.PixelZoom(1, -1);
        //
        //			GL.DrawPixels(Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(Address));
        //			//GL.DrawPixels(512, 272, PixelFormat.AbgrExt, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));
        //
        //			//GL.WindowPos2(0, 0);
        //			//GL.PixelZoom(1, 1);
        //		}
        //		catch (Exception Exception)
        //		{
        //			Console.WriteLine(Exception);
        //		}
        //	}
        //}

        //int[] pboIds = { -1 };
        //
        //static bool UsePbo = false;
        //
        //private void PreParePbos()
        //{
        //	if (UsePbo)
        //	{
        //		if (pboIds[0] == -1)
        //		{
        //			GL.GenBuffers(1, pboIds);
        //			GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pboIds[0]);
        //			GL.BufferData(BufferTarget.PixelUnpackBuffer, new IntPtr(512 * 272 * 4), IntPtr.Zero, BufferUsageHint.StreamRead);
        //			GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        //		}
        //		GL.BindBuffer(BufferTarget.PixelPackBuffer, pboIds[0]);
        //	}
        //}
        //
        //private void UnPreParePbos()
        //{
        //	if (UsePbo)
        //	{
        //		GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
        //	}
        //}

        //private void SaveFrameBuffer(GpuStateStruct* GpuState, string FileName)
        //{
        //	var GlPixelFormat = GlPixelFormatList[(int)GuPixelFormats.RGBA_8888];
        //	int Width = (int)GpuState->DrawBufferState.Width;
        //	if (Width == 0) Width = 512;
        //	int Height = 272;
        //	int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
        //	int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
        //
        //	if (Width == 0) Width = 512;
        //
        //	GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
        //
        //	var FB = new Bitmap(Width, Height);
        //	var Data = new byte[Width * Height * 4];
        //
        //	fixed (byte* DataPtr = Data)
        //	{
        //		//glBindBufferARB(GL_PIXEL_PACK_BUFFER_ARB, pboIds[index]);
        //		GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(DataPtr));
        //
        //		BitmapUtils.TransferChannelsDataInterleaved(
        //			FB.GetFullRectangle(),
        //			FB,
        //			DataPtr,
        //			BitmapUtils.Direction.FromDataToBitmap,
        //			BitmapChannel.Red,
        //			BitmapChannel.Green,
        //			BitmapChannel.Blue,
        //			BitmapChannel.Alpha
        //		);
        //	}
        //
        //	FB.Save(FileName);
        //}

        //[HandleProcessCorruptedStateExceptions]
        //private void PrepareWrite(GpuStateStruct* GpuState)
        //{
        //	//GL.Flush();
        //	//return;
        //
        //#if true
        //	//if (SwapBuffers)
        //	//{
        //	//	RenderGraphicsContext.SwapBuffers();
        //	//}
        //	//
        //	//GL.PushAttrib(AttribMask.EnableBit);
        //	//GL.PushAttrib(AttribMask.TextureBit);
        //	//{
        //	//	GL.Enable(EnableCap.Texture2D);
        //	//	GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexture);
        //	//	{
        //	//		//GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, 512, 272);
        //	//		GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, 512, 272, 0);
        //	//		//GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new uint[] { 0xFFFF00FF });
        //	//	}
        //	//	GL.BindTexture(TextureTarget.Texture2D, 0);
        //	//}
        //	//GL.PopAttrib();
        //	//GL.PopAttrib();
        //#else
        //
        //	//Console.WriteLine("PrepareWrite");
        //	try
        //	{
        //		var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
        //		int Width = (int)GpuState->DrawBufferState.Width;
        //		if (Width == 0) Width = 512;
        //		int Height = 272;
        //		int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
        //		int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
        //		//GpuState->DrawBufferState.Format
        //		var Address = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address);
        //
        //		//Console.WriteLine("{0}", GlPixelFormat.GuPixelFormat);
        //
        //		//Console.WriteLine("{0:X}", GpuState->DrawBufferState.Address);
        //		GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
        //
        //		fixed (void* _TempBufferPtr = &TempBuffer[0])
        //		{
        //			var Input = (byte*)_TempBufferPtr;
        //			var Output = (byte*)Address;
        //
        //			PreParePbos();
        //			if (this.pboIds[0] > 0)
        //			{
        //				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, IntPtr.Zero);
        //				Input = (byte*)GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly).ToPointer();
        //				GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
        //				if (Input == null)
        //				{
        //					Console.WriteLine("PBO ERROR!");
        //				}
        //			}
        //			else
        //			{
        //				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(_TempBufferPtr));
        //			}
        //			UnPreParePbos();
        //
        //			for (int Row = 0; Row < Height; Row++)
        //			{
        //				var ScanIn = (byte*)&Input[ScanWidth * Row];
        //				var ScanOut = (byte*)&Output[ScanWidth * (Height - Row - 1)];
        //				//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
        //				PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
        //			}
        //		}
        //	}
        //	catch (Exception Exception)
        //	{
        //		Console.WriteLine(Exception);
        //	}
        //
        //	if (SwapBuffers)
        //	{
        //		RenderGraphicsContext.SwapBuffers();
        //	}
        // #endif
        //}
    }
}