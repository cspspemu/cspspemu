//#define DISABLE_SKINNING
//#define SLOW_SIMPLE_RENDER_TARGET

//#define DEBUG_PRIM

#if !RELEASE
	//#define DEBUG_VERTEX_TYPE
#endif

using System;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Memory;
//using Cloo;
//using Cloo.Bindings;

using CSPspEmu.Core.Gpu.Formats;
using CSPspEmu.Core.Utils;
using CSPspEmu.Core.Types;
using CSharpPlatform;
using CSPspEmu.Core.Gpu.State.SubStates;
using System.Collections.Generic;
using CSPspEmu.Core.Cpu;
using System.Threading;
using CSharpPlatform.GL;
using CSharpPlatform.GL.Utils;
using System.Runtime.InteropServices;
using CSharpPlatform.GL.Impl;
using System.IO;
using CSPspEmu.Core.Gpu.Impl.Opengl.Utils;
using CSPspEmu.Core.Gpu.Impl.Opengl.Modules;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	public sealed unsafe partial class OpenglGpuImpl : GpuImpl, IInjectInitialize
	{
		/// <summary>
		/// 
		/// </summary>
		[Inject]
		private PspMemory Memory;

		/// <summary>
		/// 
		/// </summary>
		private VertexReader VertexReader;

		/// <summary>
		/// 
		/// </summary>
		public TextureCacheOpengl TextureCache;

		/// <summary>
		/// 
		/// </summary>
		private GpuStateStruct* GpuState;

		private VertexTypeStruct VertexType;
		private byte* IndexListByte;
		private ushort* IndexListShort;
		private VertexInfo[] Vertices = new VertexInfo[ushort.MaxValue];


		/// <summary>
		/// 
		/// </summary>
		private OpenglGpuImpl()
		{
		}

		override public void InvalidateCache(uint Address, int Size)
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

			public void Reset()
			{
				Length = 0;
			}

			public void Add(T Item)
			{
				if (Length >= Buffer.Length) Buffer = Buffer.ResizedCopy(Buffer.Length * 2);
				Buffer[Length++] = Item;
			}
		}

		private FastList<VertexInfoVector3f> VerticesPosition = new FastList<VertexInfoVector3f>();
		private FastList<VertexInfoVector3f> VerticesNormal = new FastList<VertexInfoVector3f>();
		private FastList<VertexInfoVector3f> VerticesTexcoords = new FastList<VertexInfoVector3f>();
		private FastList<VertexInfoColor> VerticesColors = new FastList<VertexInfoColor>();
		private FastList<VertexInfoWeights> VerticesWeights = new FastList<VertexInfoWeights>();

		private GLBuffer VerticesPositionBuffer;
		private GLBuffer VerticesNormalBuffer;
		private GLBuffer VerticesTexcoordsBuffer;
		private GLBuffer VerticesColorsBuffer;
		private GLBuffer VerticesWeightsBuffer;

		private FastList<uint> IndicesList = new FastList<uint>();

		private Matrix4f WorldViewProjectionMatrix = default(Matrix4f);
		private Matrix4f TextureMatrix = default(Matrix4f);

		public RenderbufferManager RenderbufferManager { get; private set; }
		private GLShader Shader;

		public class ShaderInfoClass
		{
			public GLUniform matrixWorldViewProjection;
			public GLUniform matrixTexture;
			public GLUniform matrixBones;

			public GLUniform hasPerVertexColor;
			public GLUniform hasTexture;
			public GLUniform hasReversedNormal;
			public GLUniform clearingMode;

			public GLUniform texture0;
			public GLUniform uniformColor;

			public GLUniform colorTest;

			public GLUniform alphaTest;
			public GLUniform alphaFunction;
			public GLUniform alphaValue;
			public GLUniform alphaMask;

			public GLUniform weightCount;

			public GLUniform tfx;
			public GLUniform tcc;

			public GLAttribute vertexPosition;
			public GLAttribute vertexTexCoords;
			public GLAttribute vertexColor;
			public GLAttribute vertexNormal;

			public GLAttribute vertexWeight0;
			public GLAttribute vertexWeight1;
			public GLAttribute vertexWeight2;
			public GLAttribute vertexWeight3;
			public GLAttribute vertexWeight4;
			public GLAttribute vertexWeight5;
			public GLAttribute vertexWeight6;
			public GLAttribute vertexWeight7;
		}

		ShaderInfoClass ShaderInfo = new ShaderInfoClass();

		[Inject]
		InjectContext InjectContext;

		void IInjectInitialize.Initialize()
		{
			this.RenderbufferManager = new RenderbufferManager(this);
			this.TextureCache = new TextureCacheOpengl(this.Memory, this, InjectContext);
			this.VertexReader = new VertexReader();
		}

		private void DrawInitVertices()
		{
			//Console.WriteLine(WGL.wglGetCurrentContext());
			VerticesPositionBuffer = GLBuffer.Create();
			VerticesNormalBuffer = GLBuffer.Create();
			VerticesTexcoordsBuffer = GLBuffer.Create();
			VerticesColorsBuffer = GLBuffer.Create();
			VerticesWeightsBuffer = GLBuffer.Create();
			Shader = new GLShader(
				typeof(OpenglGpuImpl).Assembly.GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.Opengl.shader.vert").ReadAllContentsAsString(),
				typeof(OpenglGpuImpl).Assembly.GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.Opengl.shader.frag").ReadAllContentsAsString()
			);
			Console.WriteLine("###################################");
			foreach (var Uniform in Shader.Uniforms) Console.WriteLine(Uniform);
			foreach (var Attribute in Shader.Attributes) Console.WriteLine(Attribute);
			Console.WriteLine("###################################");

			Shader.BindUniformsAndAttributes(ShaderInfo);
		}

		private void PrepareDrawStateFirst()
		{
			if (Shader == null) this.DrawInitVertices();

			ShaderInfo.matrixWorldViewProjection.Set(WorldViewProjectionMatrix);
			ShaderInfo.matrixTexture.Set(TextureMatrix);
			ShaderInfo.uniformColor.Set(GpuState->LightingState.AmbientModelColor.ToVector4f());
			ShaderInfo.hasPerVertexColor.Set(VertexType.HasColor);
			ShaderInfo.clearingMode.Set(GpuState->ClearingMode);
			ShaderInfo.hasTexture.Set(GpuState->TextureMappingState.Enabled);

			ShaderInfo.weightCount.Set(VertexType.RealSkinningWeightCount);
			//ShaderInfo.weightCount.Set(0);
			if (VertexType.HasWeight)
			{
				ShaderInfo.matrixBones.Set(new Matrix4f[]
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
				var TextureState = &GpuState->TextureMappingState.TextureState;

				ShaderInfo.tfx.Set((int)TextureState->Effect);
				ShaderInfo.tcc.Set((int)TextureState->ColorComponent);
				ShaderInfo.colorTest.NoWarning().Set(GpuState->ColorTestState.Enabled);

				ShaderInfo.alphaTest.Set(GpuState->AlphaTestState.Enabled);
				ShaderInfo.alphaFunction.Set((int)GpuState->AlphaTestState.Function);
				ShaderInfo.alphaMask.NoWarning().Set(GpuState->AlphaTestState.Mask);
				ShaderInfo.alphaValue.Set(GpuState->AlphaTestState.Value);

				//Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", TextureState->Effect, TextureState->ColorComponent, GpuState->BlendingState.Enabled, GpuState->BlendingState.FunctionSource, GpuState->BlendingState.FunctionDestination, GpuState->ColorTestState.Enabled);

				ShaderInfo.texture0.Set(GLTextureUnit.CreateAtIndex(0)
					.SetWrap(
						(GLWrap)((TextureState->WrapU == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE),
						(GLWrap)((TextureState->WrapV == WrapMode.Repeat) ? GL.GL_REPEAT : GL.GL_CLAMP_TO_EDGE)
					)
					.SetFiltering(
						(GLScaleFilter)((TextureState->FilterMinification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST),
						(GLScaleFilter)((TextureState->FilterMagnification == TextureFilter.Linear) ? GL.GL_LINEAR : GL.GL_NEAREST)
					)
					.SetTexture(RenderbufferManager.TextureCacheGetAndBind(GpuState))
				);
			}
		}

		private void DrawVertices(GLGeometry Type)
		{
			ShaderInfo.hasReversedNormal.NoWarning().Set(VertexType.ReversedNormal);

			Shader.Draw(Type, IndicesList.Buffer, IndicesList.Length, () =>
			{
				if (VertexType.HasPosition)
				{
					VerticesPositionBuffer.SetData(this.VerticesPosition.Buffer, 0, this.VerticesPosition.Length);
					ShaderInfo.vertexPosition.SetData<float>(VerticesPositionBuffer, 3, 0, sizeof(VertexInfoVector3f), false);
				}

				if (VertexType.HasTexture)
				{
					VerticesTexcoordsBuffer.SetData(this.VerticesTexcoords.Buffer, 0, this.VerticesTexcoords.Length);
					ShaderInfo.vertexTexCoords.SetData<float>(VerticesTexcoordsBuffer, 3, 0, sizeof(VertexInfoVector3f), false);
				}

				if (VertexType.HasColor)
				{
					VerticesColorsBuffer.SetData(this.VerticesColors.Buffer, 0, this.VerticesColors.Length);
					ShaderInfo.vertexColor.SetData<float>(VerticesColorsBuffer, 4, 0, sizeof(VertexInfoColor), false);
				}

				if (VertexType.HasNormal)
				{
					VerticesNormalBuffer.SetData(this.VerticesNormal.Buffer, 0, this.VerticesNormal.Length);
					ShaderInfo.vertexNormal.NoWarning().SetData<float>(VerticesNormalBuffer, 4, 0, sizeof(VertexInfoVector3f), false);
				}

				if (VertexType.HasWeight)
				{
					VerticesWeightsBuffer.SetData(this.VerticesWeights.Buffer, 0, this.VerticesWeights.Length);
					var vertexWeights = new[] { ShaderInfo.vertexWeight0, ShaderInfo.vertexWeight1, ShaderInfo.vertexWeight2, ShaderInfo.vertexWeight3, ShaderInfo.vertexWeight4, ShaderInfo.vertexWeight5, ShaderInfo.vertexWeight6, ShaderInfo.vertexWeight7 };
					for (int n = 0; n < VertexType.RealSkinningWeightCount; n++)
					{
						vertexWeights[n].SetData<float>(VerticesWeightsBuffer, 1, n * sizeof(float), sizeof(VertexInfoWeights), false);
					}
				}
			});
		}

		private void ResetVertex()
		{
			VerticesPosition.Reset();
			VerticesNormal.Reset();
			VerticesWeights.Reset();
			VerticesTexcoords.Reset();
			VerticesColors.Reset();

			IndicesList.Reset();
		}

		private void PutVertices(params VertexInfo[] _VertexInfoList)
		{
			foreach (var VertexInfo in _VertexInfoList) PutVertex(VertexInfo);
		}

		private void PutVertexIndexRelative(int Offset)
		{
			PutVertexIndex(VerticesPosition.Length + Offset);
		}

		private void PutVertexIndex(int VertexIndex)
		{
			IndicesList.Add((uint)VertexIndex);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_VertexInfo"></param>
		/// <param name="VertexType"></param>
		private void PutVertex(VertexInfo VertexInfo)
		{
			_CapturePutVertex(ref VertexInfo);

			PutVertexIndex(VerticesPosition.Length);

			VerticesPosition.Add(new VertexInfoVector3f(VertexInfo.Position));
			VerticesNormal.Add(new VertexInfoVector3f(VertexInfo.Normal));
			VerticesTexcoords.Add(new VertexInfoVector3f(VertexInfo.Texture));
			VerticesColors.Add(new VertexInfoColor(VertexInfo.Color));
			VerticesWeights.Add(new VertexInfoWeights(VertexInfo));
		}

		private void ReadVertex_Void(int Index, out VertexInfo VertexInfo)
		{
			VertexInfo = Vertices[Index];
		}

		private void ReadVertex_Byte(int Index, out VertexInfo VertexInfo)
		{
			VertexInfo = Vertices[IndexListByte[Index]];
		}

		private void ReadVertex_Short(int Index, out VertexInfo VertexInfo)
		{
			VertexInfo = Vertices[IndexListShort[Index]];
		}

		private delegate void ReadVertexDelegate(int Index, out VertexInfo VertexInfo);

		private object PspWavefrontObjWriterLock = new object();
		private PspWavefrontObjWriter PspWavefrontObjWriter = null;

		public override void StartCapture()
		{
			lock (PspWavefrontObjWriterLock)
			{
				PspWavefrontObjWriter = new PspWavefrontObjWriter(new WavefrontObjWriter(ApplicationPaths.MemoryStickRootFolder + "/gpu_frame.obj"));
			}
		}

		public override void EndCapture()
		{
			lock (PspWavefrontObjWriterLock)
			{
				PspWavefrontObjWriter.End();
				PspWavefrontObjWriter = null;
			}
		}

		private void _CapturePrimitive(GuPrimitiveType PrimitiveType, uint VertexAddress, int VetexCount, ref VertexTypeStruct VertexType, Action Action)
		{
			if (PspWavefrontObjWriter != null)
			{
				lock (PspWavefrontObjWriterLock) PspWavefrontObjWriter.StartPrimitive(GpuState, PrimitiveType, VertexAddress, VetexCount, ref VertexType);
				try
				{
					Action();
				}
				finally
				{
					lock (PspWavefrontObjWriterLock) PspWavefrontObjWriter.EndPrimitive();
				}
			}
			else
			{
				Action();
			}
		}

		private void _CapturePutVertex(ref VertexInfo VertexInfo)
		{
			if (PspWavefrontObjWriter != null)
			{
				lock (this) PspWavefrontObjWriter.PutVertex(ref VertexInfo);
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

		bool DoPrimStart = false;
		VertexTypeStruct CachedVertexType;
		GuPrimitiveType PrimitiveType;
		GLRenderTarget LogicOpsRenderTarget;

		public override unsafe void PrimStart(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType)
		{
			this.GpuState = GpuState;
			this.PrimitiveType = PrimitiveType;
			DoPrimStart = true;
			ResetVertex();


			if (Shader != null)
			{
				Shader.GetUniform("lopEnabled").Set(GpuState->LogicalOperationState.Enabled);

				if (GpuState->LogicalOperationState.Enabled)
				{
					if (LogicOpsRenderTarget == null)
					{
						LogicOpsRenderTarget = GLRenderTarget.Create(512, 272, RenderTargetLayers.Color);
					}
					GLRenderTarget.CopyFromTo(GLRenderTarget.Current, LogicOpsRenderTarget);
					Shader.GetUniform("backtex").Set(GLTextureUnit.CreateAtIndex(1).SetFiltering(GLScaleFilter.Linear).SetWrap(GLWrap.ClampToEdge).SetTexture(LogicOpsRenderTarget.TextureColor));

					Shader.GetUniform("lop").Set((int)GpuState->LogicalOperationState.Operation);

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
			DrawVertices(ConvertGLGeometry(PrimitiveType));
			ResetVertex();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GlobalGpuState"></param>
		/// <param name="GpuState"></param>
		/// <param name="PrimitiveType"></param>
		/// <param name="VertexCount"></param>
		public override unsafe void Prim(ushort VertexCount)
		{
			VertexType = GpuState->VertexState.Type;

			if (DoPrimStart || (VertexType != CachedVertexType))
			{
				CachedVertexType = VertexType;
				DoPrimStart = false;

				OpenglGpuImplCommon.PrepareStateCommon(GpuState, ScaleViewport);

				if (GpuState->ClearingMode)
				{
					OpenglGpuImplClear.PrepareStateClear(GpuState);
				}
				else
				{
					PrepareStateDraw(GpuState);
				}

				OpenglGpuImplMatrix.PrepareStateMatrix(GpuState, ref WorldViewProjectionMatrix);
				PrepareDrawStateFirst();
			}

			int MorpingVertexCount = (int)VertexType.MorphingVertexCount + 1;

			//if (PrimitiveType == GuPrimitiveType.TriangleStrip) VertexCount++;

			ReadVertexDelegate ReadVertex = ReadVertex_Void;
			VertexReader.SetVertexTypeStruct(
				VertexType,
				(byte*)Memory.PspAddressToPointerSafe(GpuState->GetAddressRelativeToBaseOffset(GpuState->VertexAddress), 0)
			);

			// Fix missing geometry! At least!
			if (VertexType.Index == VertexTypeStruct.IndexEnum.Void)
			{
				GpuState->VertexAddress += (uint)(VertexReader.VertexSize * VertexCount * MorpingVertexCount);
				//GpuState->VertexAddress += (uint)(VertexReader.VertexSize * VertexCount);
			}

			if (MorpingVertexCount != 1 || VertexType.RealSkinningWeightCount != 0)
			{
				//Console.WriteLine("PRIM: {0}, {1}, Morphing:{2}, Skinning:{3}", PrimitiveType, VertexCount, MorpingVertexCount, VertexType.RealSkinningWeightCount);
			}

			uint TotalVerticesWithoutMorphing = VertexCount;

			void* IndexPointer = null;
			if (VertexType.Index != VertexTypeStruct.IndexEnum.Void)
			{
				IndexPointer = Memory.PspAddressToPointerSafe(GpuState->GetAddressRelativeToBaseOffset(GpuState->IndexAddress), 0);
			}

			//Console.Error.WriteLine(VertexType.Index);
			switch (VertexType.Index)
			{
				case VertexTypeStruct.IndexEnum.Void:
					break;
				case VertexTypeStruct.IndexEnum.Byte:
					ReadVertex = ReadVertex_Byte;
					IndexListByte = (byte*)IndexPointer;
					TotalVerticesWithoutMorphing = 0;
					for (int n = 0; n < VertexCount; n++)
					{
						if (TotalVerticesWithoutMorphing < IndexListByte[n]) TotalVerticesWithoutMorphing = IndexListByte[n];
					}
					break;
				case VertexTypeStruct.IndexEnum.Short:
					ReadVertex = ReadVertex_Short;
					IndexListShort = (ushort*)IndexPointer;
					TotalVerticesWithoutMorphing = 0;
					//VertexCount--;
					for (int n = 0; n < VertexCount; n++)
					{
						//Console.Error.WriteLine(IndexListShort[n]);
						if (TotalVerticesWithoutMorphing < IndexListShort[n]) TotalVerticesWithoutMorphing = IndexListShort[n];
					}
					break;
				default:
					throw (new NotImplementedException("VertexType.Index: " + VertexType.Index));
			}
			TotalVerticesWithoutMorphing++;


			//Console.WriteLine(TotalVerticesWithoutMorphing);

			int z = 0;
			VertexInfo TempVertexInfo;

			float* Morphs = &GpuState->MorphingState.MorphWeight0;

			//for (int n = 0; n < MorpingVertexCount; n++) Console.Write("{0}, ", Morphs[n]); Console.WriteLine("");

			//int VertexInfoFloatCount = (sizeof(Color4F) + sizeof(Vector3F) * 3) / sizeof(float);
			int VertexInfoFloatCount = (sizeof(VertexInfo)) / sizeof(float);
			fixed (VertexInfo* VerticesPtr = Vertices)
			{
				if (MorpingVertexCount == 1)
				{
					VertexReader.ReadVertices(0, VerticesPtr, (int)TotalVerticesWithoutMorphing);
				}
				else
				{
					var ComponentsIn = (float*)&TempVertexInfo;
					for (int n = 0; n < TotalVerticesWithoutMorphing; n++)
					{
						var ComponentsOut = (float*)&VerticesPtr[n];
						for (int cc = 0; cc < VertexInfoFloatCount; cc++) ComponentsOut[cc] = 0;
						for (int m = 0; m < MorpingVertexCount; m++)
						{
							VertexReader.ReadVertex(z++, &TempVertexInfo);
							for (int cc = 0; cc < VertexInfoFloatCount; cc++) ComponentsOut[cc] += ComponentsIn[cc] * Morphs[m];
						}
						VerticesPtr[n].Normal = VerticesPtr[n].Normal.Normalize();
					}
				}
			}

			_CapturePrimitive(PrimitiveType, GpuState->GetAddressRelativeToBaseOffset(GpuState->VertexAddress), VertexCount, ref VertexType, () =>
			{
				// Continuation
				if (IndicesList.Length > 0)
				{
					switch (PrimitiveType)
					{
						// Degenerate.
						case GuPrimitiveType.TriangleStrip:
						case GuPrimitiveType.Sprites:
							if (VertexCount > 0)
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

				if (PrimitiveType == GuPrimitiveType.Sprites)
				{
					GL.glDisable(GL.GL_CULL_FACE);
					for (int n = 0; n < VertexCount; n += 2)
					{
						VertexInfo V0, V1, V2, V3;

						ReadVertex(n + 0, out V0);
						ReadVertex(n + 1, out V3);

						VertexUtils.GenerateTriangleStripFromSpriteVertices(ref V0, out V1, out V2, ref V3);

						if (n > 0)
						{
							PutVertexIndexRelative(-1);
							PutVertexIndexRelative(0);
						}

						PutVertices(V0, V1, V2, V3);
					}
				}
				else
				{
					VertexInfo VertexInfo;
					//Console.Error.WriteLine("{0} : {1} : {2}", BeginMode, VertexCount, VertexType.Index);
					for (int n = 0; n < VertexCount; n++)
					{
						ReadVertex(n, out VertexInfo);
						PutVertex(VertexInfo);
					}
				}
			});
		}

		private GLGeometry ConvertGLGeometry(GuPrimitiveType PrimitiveType)
		{
			switch (PrimitiveType)
			{
				case GuPrimitiveType.Lines: return GLGeometry.GL_LINES;
				case GuPrimitiveType.LineStrip: return GLGeometry.GL_LINE_STRIP;
				case GuPrimitiveType.Triangles: return GLGeometry.GL_TRIANGLES;
				case GuPrimitiveType.Points: return GLGeometry.GL_POINTS;
				case GuPrimitiveType.TriangleFan: return GLGeometry.GL_TRIANGLE_FAN;
				case GuPrimitiveType.TriangleStrip: return GLGeometry.GL_TRIANGLE_STRIP;
				case GuPrimitiveType.Sprites: return GLGeometry.GL_TRIANGLE_STRIP;
				default: throw (new NotImplementedException("Not implemented PrimitiveType:'" + PrimitiveType + "'"));
			}
		}

		public override void BeforeDraw(GpuStateStruct* GpuState)
		{
			RenderbufferManager.BindCurrentDrawBufferTexture(GpuState);
		}

		public override void DrawVideo(uint FrameBufferAddress, OutputPixel* OutputPixel, int Width, int Height)
		{
			RenderbufferManager.DrawVideo(FrameBufferAddress, OutputPixel, Width, Height);
		}


		[HandleProcessCorruptedStateExceptions]
		public override void Finish(GpuStateStruct* GpuState)
		{
		}

		public override void End(GpuStateStruct* GpuState)
		{
			//PrepareWrite(GpuState);
		}

		public override void Sync(GpuStateStruct* GpuState)
		{
		}

		public override void TextureFlush(GpuStateStruct* GpuState)
		{
			TextureCache.RecheckAll();
		}

		public override void TextureSync(GpuStateStruct* GpuState)
		{
		}

		public override void AddedDisplayList()
		{
		}

		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "OpenGl 2.0 (|ES)",
					Version = "0.1",
				};
			}
		}

		public override bool IsWorking
		{
			get { return true; }
		}
	}
}
