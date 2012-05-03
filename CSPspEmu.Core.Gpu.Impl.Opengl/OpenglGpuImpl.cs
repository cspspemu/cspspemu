#if !RELEASE
	//#define DEBUG_VERTEX_TYPE
#endif

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpUtils;
using CSPspEmu.Core.Gpu.State;
using System.Globalization;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Utils;
using System.Diagnostics;
using System.IO;
//using Cloo;
//using Cloo.Bindings;

#if OPENTK
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using CSPspEmu.Core.Gpu.Formats;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public partial class OpenglGpuImpl : GpuImpl
	{
		/// <summary>
		/// 
		/// </summary>
		private PspConfig Config;

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
		[Inject]
		private TextureCache TextureCache;

		/// <summary>
		/// 
		/// </summary>
		private Texture CurrentTexture;

		/// <summary>
		/// 
		/// </summary>
		public override void InitializeComponent()
		{
			this.Config = PspEmulatorContext.PspConfig;
			this.TextureCache.OpenglGpuImpl = this;
			this.VertexReader = new VertexReader();
		}

		//static public object GpuLock = new object();

		/// <summary>
		/// 
		/// </summary>
		void Initialize()
		{
			///GL.Enable(EnableCap.Blend);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			/*
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineRgb, (int)TextureEnvModeCombine.Modulate);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Source0Rgb, (int)TextureEnvModeSource.Texture);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src1Rgb, (int)TextureEnvModeSource.Previous);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand1Rgb, (int)TextureEnvModeOperandRgb.SrcColor);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.CombineAlpha, (int)TextureEnvModeCombine.Replace);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Src0Alpha, (int)TextureEnvModeSource.Previous);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.Operand0Alpha, (int)TextureEnvModeOperandAlpha.SrcAlpha);
			*/
			//GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Combine);

			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, Color.FromArgb(1, 0, 0, 0));
			//glAlphaFunc(GL_GREATER,0.000000)


			//glTexEnvi(GL_TEXTURE_ENV,GL_TEXTURE_ENV_MODE,GL_MODULATE)


			//glTexEnvfv(GL_TEXTURE_ENV,GL_TEXTURE_ENV_COLOR, { 0.000000, 0.000000, 0.000000, 1.000000 } )

			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

			//drawBeginClear(GpuState);
			/*
			GL.ClearColor(0, 0, 0, 1);
			GL.ClearDepth(0);
			GL.ClearStencil(0);
			GL.ClearAccum(0, 0, 0, 0);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit | ClearBufferMask.AccumBufferBit);
			*/
		}

		GpuStateStruct* GpuState;

#if true
		VertexInfo PerformSkinning(VertexInfo VertexInfo)
		{
			int SkinningWeightCount = VertexType.RealSkinningWeightCount;
			if (SkinningWeightCount == 0) return VertexInfo;

			var OutputPosition = default(Vector3F);
			var OutputNormal = default(Vector3F);
			var InputPosition = VertexInfo.Position;
			var InputNormal = VertexInfo.Normal;

			var BoneMatrices = &GpuState->SkinningState.BoneMatrix0;

			for (int m = 0; m < SkinningWeightCount; m++)
			{
				var BoneMatrix = BoneMatrices[m];
				float Weight = VertexInfo.Weights[m];

				if (Weight != 0)
				{
					OutputPosition.X += (InputPosition.X * BoneMatrix.Values[0] + InputPosition.Y * BoneMatrix.Values[4] + InputPosition.Z * BoneMatrix.Values[8] + 1 * BoneMatrix.Values[12]) * Weight;
					OutputPosition.Y += (InputPosition.X * BoneMatrix.Values[1] + InputPosition.Y * BoneMatrix.Values[5] + InputPosition.Z * BoneMatrix.Values[9] + 1 * BoneMatrix.Values[13]) * Weight;
					OutputPosition.Z += (InputPosition.X * BoneMatrix.Values[2] + InputPosition.Y * BoneMatrix.Values[6] + InputPosition.Z * BoneMatrix.Values[10] + 1 * BoneMatrix.Values[14]) * Weight;

					OutputNormal.X += (InputNormal.X * BoneMatrix.Values[0] + InputNormal.Y * BoneMatrix.Values[4] + InputNormal.Z * BoneMatrix.Values[8] + 0 * BoneMatrix.Values[12]) * Weight;
					OutputNormal.Y += (InputNormal.X * BoneMatrix.Values[1] + InputNormal.Y * BoneMatrix.Values[5] + InputNormal.Z * BoneMatrix.Values[9] + 0 * BoneMatrix.Values[13]) * Weight;
					OutputNormal.Z += (InputNormal.X * BoneMatrix.Values[2] + InputNormal.Y * BoneMatrix.Values[6] + InputNormal.Z * BoneMatrix.Values[10] + 0 * BoneMatrix.Values[14]) * Weight;
				}
			}

			VertexInfo.Position = OutputPosition;
			VertexInfo.Normal = OutputNormal;

			return VertexInfo;
		}
#else
		VertexInfo PerformSkinning(VertexInfo VertexInfo)
		{
			if (VertexType.RealSkinningWeightCount == 0) return VertexInfo;
			//return VertexInfo;

			var SkinnedVertexState = VertexInfo;

			SkinnedVertexState.Position = Vector3.Zero;
			SkinnedVertexState.Normal = Vector3.Zero;

			var BoneMatrices = &GpuState->SkinningState.BoneMatrix0;
			//Console.WriteLine(VertexType.SkinningWeightCount);

			int SkinningWeightCount = VertexType.RealSkinningWeightCount;

			for (int m = 0; m < SkinningWeightCount; m++)
			{
				var BoneMatrix = BoneMatrices[m].Matrix4;
				//BoneMatrix.Transpose();
				//Console.WriteLine("{0}", VertexInfo.Weights[m]);
				SkinnedVertexState.Position += Vector3.Transform(VertexInfo.Position, BoneMatrix) * VertexInfo.Weights[m];
				SkinnedVertexState.Normal += Vector3.Transform(VertexInfo.Normal, BoneMatrix) * VertexInfo.Weights[m];
			}

			return SkinnedVertexState;

			/*
			if (!shouldPerformSkin) return vertexState;
			
			//writefln("%s", gpu.state.boneMatrix[0]);
			VertexState skinnedVertexState = vertexState;
			(cast(float *)&skinnedVertexState.px)[0..3] = 0.0;
			(cast(float *)&skinnedVertexState.nx)[0..3] = 0.0;
			*/
			
			/*
			float[3] p, n;

			for (int m = 0; m < vertexType.skinningWeightCount; m++) {
				multiplyVectorPerMatrix!(true)(
					p,
					(cast(float *)&vertexState.px)[0..3],
					boneMatrix[m],
					vertexState.weights[m]
				);

				multiplyVectorPerMatrix!(false)(
					n,
					(cast(float *)&vertexState.nx)[0..3],
					boneMatrix[m],
					vertexState.weights[m]
				);
				
				//writefln("%s", p);
				
				(cast(float *)&skinnedVertexState.px)[0..3] += p[];
				(cast(float *)&skinnedVertexState.nx)[0..3] += n[];
			}
			
			return skinnedVertexState;
			*/
		}
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="VertexInfo"></param>
		/// <param name="VertexType"></param>
		private void PutVertex(ref VertexInfo _VertexInfo, ref VertexTypeStruct VertexType)
		{
			VertexInfo VertexInfo = PerformSkinning(_VertexInfo);

			_CapturePutVertex(ref VertexInfo);

			//Console.WriteLine(VertexType);
			//Console.WriteLine(VertexInfo);
			/*
			if (GpuState->ClearingMode)
			{
				Console.WriteLine(VertexInfo);
			}
			*/
			//Console.WriteLine(VertexInfo);

			//Console.WriteLine(VertexInfo);
			//Console.WriteLine(VertexInfo);

#if DEBUG_VERTEX_TYPE
			if (OutputVertexInfoStream != null) OutputVertexInfoStream.WriteBytes(Encoding.UTF8.GetBytes(String.Format("{0}\n", VertexInfo)));
#endif

#if false
			GL.Color4(1.0f, 1.0f, 1.0f, 1.0f);
			GL.Vertex3(VertexInfo.PX, VertexInfo.PY, 0.5f);
#else

			if (VertexType.Color != VertexTypeStruct.ColorEnum.Void)
			{
				GL.Color4(VertexInfo.Color.R, VertexInfo.Color.G, VertexInfo.Color.B, VertexInfo.Color.A);
			}
			if (VertexType.Texture != VertexTypeStruct.NumericEnum.Void)
			{
				/*
				if (VertexType.Texture == VertexTypeStruct.NumericEnum.Short)
				{
					var Texture = CurrentTexture;

					Console.WriteLine(
						"U={0}, V={1}, OFFSET({2}, {3}), SCALE({4}, {5}) : SIZE({6},{7}) : {8}",
						VertexInfo.U, VertexInfo.V,
						GpuState->TextureMappingState.TextureState.OffsetU, GpuState->TextureMappingState.TextureState.OffsetV,
						GpuState->TextureMappingState.TextureState.ScaleU, GpuState->TextureMappingState.TextureState.ScaleV,
						Texture.Width, Texture.Height,
						GpuState->TextureMappingState.TextureState
					);
					
					//return;
				}
				*/

				//Console.WriteLine("{0}, {1}", VertexInfo.U, VertexInfo.V);
				//GL.TexCoord2(VertexInfo.TX, VertexInfo.TY);
				GL.TexCoord3(VertexInfo.Texture.X, VertexInfo.Texture.Y, VertexInfo.Texture.Z);
			}
			//Console.Write(",{0}", VertexInfo.PZ);
			if (VertexType.Normal != VertexTypeStruct.NumericEnum.Void)
			{
#if false
				if (VertexType.ReversedNormal)
				{
					GL.Normal3(-VertexInfo.Normal.X, -VertexInfo.Normal.Y, -VertexInfo.Normal.Z);
				}
				else
#endif
				{
					GL.Normal3(VertexInfo.Normal.X, VertexInfo.Normal.Y, VertexInfo.Normal.Z);
				}
			}
			if (VertexType.Position != VertexTypeStruct.NumericEnum.Void)
			{
				GL.Vertex3(VertexInfo.Position.X, VertexInfo.Position.Y, VertexInfo.Position.Z);
			}
#endif
		}

		VertexTypeStruct VertexType;
		byte* IndexListByte;
		ushort* IndexListShort;
		VertexInfo[] Vertices = new VertexInfo[ushort.MaxValue];

		/*
		void ReadVertex(int Index, VertexInfo* VertexInfo)
		{
			if (VertexType.Index == VertexTypeStruct.IndexEnum.Byte)
			{
				*VertexInfo = Vertices[IndexListByte[Index]];
			}
			else if (VertexType.Index == VertexTypeStruct.IndexEnum.Short)
			{
				*VertexInfo = Vertices[IndexListShort[Index]];
			}
			else *VertexInfo = Vertices[Index];
		}
		*/

		void ReadVertex_Byte(int Index, VertexInfo* VertexInfo)
		{
			*VertexInfo = Vertices[IndexListByte[Index]];
		}

		void ReadVertex_Short(int Index, VertexInfo* VertexInfo)
		{
			*VertexInfo = Vertices[IndexListShort[Index]];
		}

		void ReadVertex_Void(int Index, VertexInfo* VertexInfo)
		{
			*VertexInfo = Vertices[Index];
		}

		delegate void ReadVertexDelegate(int Index, VertexInfo* VertexInfo);

		object PspWavefrontObjWriterLock = new object();
		PspWavefrontObjWriter PspWavefrontObjWriter = null;

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

		private void _CaptureStartPrimitive(GuPrimitiveType PrimitiveType, uint VertexAddress, int VetexCount, ref VertexTypeStruct VertexType)
		{
			if (PspWavefrontObjWriter != null)
			{
				lock (PspWavefrontObjWriterLock)
				{
					if (PspWavefrontObjWriter != null) PspWavefrontObjWriter.StartPrimitive(GpuState, PrimitiveType, VertexAddress, VetexCount, ref VertexType);
				}
			}
		}

		private void _CaptureEndPrimitive()
		{
			if (PspWavefrontObjWriter != null)
			{
				lock (PspWavefrontObjWriterLock)
				{
					if (PspWavefrontObjWriter != null) PspWavefrontObjWriter.EndPrimitive();
				}
			}
		}

		private void _CapturePutVertex(ref VertexInfo VertexInfo)
		{
			if (PspWavefrontObjWriter != null)
			{
				lock (this)
				{
					if (PspWavefrontObjWriter != null) PspWavefrontObjWriter.PutVertex(ref VertexInfo);
				}
			}
		}


		override public unsafe void Prim(GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
			//Console.WriteLine("VertexCount: {0}", VertexCount);
			var Start = DateTime.UtcNow;
			{
				_Prim(GpuState, PrimitiveType, VertexCount);
			}
			var End = DateTime.UtcNow;
			//Console.Error.WriteLine("Prim: {0}", End - Start);
		}

#if DEBUG_VERTEX_TYPE
		Stream OutputVertexInfoStream;
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GpuState"></param>
		private unsafe void _Prim(GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
			//if (PrimitiveType == GuPrimitiveType.TriangleStrip) VertexCount++;

			//Console.WriteLine("Prim: {0}, {1}", PrimitiveType, VertexCount);
			this.GpuState = GpuState;

			//Console.WriteLine("--------------------------------------------------------");
			VertexType = GpuState->VertexState.Type;

			ReadVertexDelegate ReadVertex = ReadVertex_Void;
			VertexReader.SetVertexTypeStruct(
				VertexType,
				(byte*)Memory.PspAddressToPointerSafe(GpuState->GetAddressRelativeToBaseOffset(GpuState->VertexAddress), 0)
			);

#if DEBUG_VERTEX_TYPE
			try
			{
				if (!File.Exists("VertexType_" + VertexType.Value))
				{
					File.WriteAllBytes(
						"VertexType_" + VertexType.Value,
						PointerUtils.PointerToByteArray((byte*)Memory.PspAddressToPointerSafe(GpuState->VertexAddress), 16 * 1024)
					);
					File.WriteAllText(
						"VertexType_" + VertexType.Value + "_str",
						VertexCount + "," + PrimitiveType + "\n" +
						VertexType.ToString()
					);
					OutputVertexInfoStream = File.OpenWrite("VertexType_" + VertexType.Value + "_list");
				}
			}
			catch
			{
			}
#endif
			//IndexReader.SetVertexTypeStruct(VertexType, VertexCount, (byte*)Memory.PspAddressToPointerSafe(GpuState->IndexAddress));

			uint TotalVerticesWithoutMorphing = VertexCount;

			//Console.Error.WriteLine("GpuState->IndexAddress: {0:X}", GpuState->IndexAddress);

			// Invalid
			/*
			if (GpuState->IndexAddress == 0xFFFFFFFF)
			{
				//Debug.Fail("Invalid IndexAddress");
				throw (new Exception("Invalid IndexAddress == 0xFFFFFFFF"));
			}
			*/

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
					IndexListByte = (byte *)IndexPointer;
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
					throw (new NotImplementedException());
			}
			TotalVerticesWithoutMorphing++;

			//Console.WriteLine(TotalVerticesWithoutMorphing);

			int MorpingVertexCount = (int)VertexType.MorphingVertexCount + 1;
			int z = 0;
			VertexInfo TempVertexInfo;

			float* Morphs = &GpuState->MorphingState.MorphWeight0;

			//for (int n = 0; n < MorpingVertexCount; n++) Console.Write("{0}, ", Morphs[n]); Console.WriteLine("");

			//int VertexInfoFloatCount = (sizeof(Color4F) + sizeof(Vector3F) * 3) / sizeof(float);
			int VertexInfoFloatCount = (sizeof(VertexInfo)) / sizeof(float);
			fixed (VertexInfo* VerticesPtr = Vertices)
			{
#if true
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
#else
				var ComponentsIn = (float*)&TempVertexInfo;
				for (int n = 0; n < TotalVerticesWithoutMorphing; n++)
				{
					if (MorpingVertexCount == 1)
					{
						VertexReader.ReadVertex(z++, &TempVertexInfo);
						VerticesPtr[n] = TempVertexInfo;
						//VertexReader.ReadVertices(0, VerticesPtr, TotalVerticesWithoutMorphing);
					}
					else
					{
						var ComponentsOut = (float*)&VerticesPtr[n];
						for (int cc = 0; cc < VertexInfoFloatCount; cc++) ComponentsOut[cc] = 0;
						for (int m = 0; m < MorpingVertexCount; m++)
						{
							VertexReader.ReadVertex(z++, &TempVertexInfo);
							for (int cc = 0; cc < VertexInfoFloatCount; cc++) ComponentsOut[cc] += ComponentsIn[cc] * Morphs[m];
						}
					}
				}
#endif
			}

			//VertexType.Texture == VertexTypeStruct.TextureEnum.Byte
			//return;
			//PrepareRead(GpuState);

			PrepareStateCommon(GpuState);

			if (GpuState->ClearingMode)
			{
				PrepareStateClear(GpuState);
			}
			else
			{
				PrepareStateDraw(GpuState);
			}

			PrepareStateMatrix(GpuState);

			//GL.Enable(EnableCap.Blend);

			/*
			if (CurrentTexture != null)
			{
				if (CurrentTexture.TextureHash == 0x2202293873)
				{
					Console.Error.WriteLine(CurrentTexture);
					Console.Error.WriteLine(VertexCount);
				}
			}
			*/

			_CaptureStartPrimitive(PrimitiveType, GpuState->GetAddressRelativeToBaseOffset(GpuState->VertexAddress), VertexCount, ref VertexType);

			// DRAW ACTUALLY
			{
				//uint VertexSize = GpuState->VertexState.Type.GetVertexSize();

				//byte* VertexPtr = (byte*)Memory.PspAddressToPointerSafe(GpuState->VertexAddress);

				//Console.WriteLine(VertexSize);

				var BeginMode = default(BeginMode);

				switch (PrimitiveType)
				{
					case GuPrimitiveType.Lines: BeginMode = BeginMode.Lines; break;
					case GuPrimitiveType.LineStrip: BeginMode = BeginMode.LineStrip; break;
					case GuPrimitiveType.Triangles: BeginMode = BeginMode.Triangles; break;
					case GuPrimitiveType.Points: BeginMode = BeginMode.Points; break;
					case GuPrimitiveType.TriangleFan: BeginMode = BeginMode.TriangleFan; break;
					case GuPrimitiveType.TriangleStrip: BeginMode = BeginMode.TriangleStrip; break;
					case GuPrimitiveType.Sprites: BeginMode = BeginMode.Quads; break;
					default: throw (new NotImplementedException("Not implemented PrimitiveType:'" + PrimitiveType + "'"));
				}

				if (PrimitiveType == GuPrimitiveType.Sprites)
				{
					GL.Disable(EnableCap.CullFace);
				}

				//Console.WriteLine(BeginMode);

				//lock (GpuLock)
				{
					//Console.Error.WriteLine("GL.Begin : Thread : {0}", Thread.CurrentThread.ManagedThreadId);
					GL.Begin(BeginMode);
					{
						if (PrimitiveType == GuPrimitiveType.Sprites)
						{
							GL.Disable(EnableCap.CullFace);
							for (int n = 0; n < VertexCount; n += 2)
							{
								VertexInfo V1, V2, V3, V4;

								ReadVertex(n + 0, &V1);
								ReadVertex(n + 1, &V3);
								{
									//if (GpuState->ClearingMode) Console.WriteLine("{0} - {1}", VertexInfoTopLeft, VertexInfoBottomRight);

									var Color = V3.Color;
									var TZ = V1.Texture.Z;
									var PZ = V1.Position.Z;
									var NZ = V1.Normal.Z;

									V2 = new VertexInfo()
									{
										Texture = new Vector3F(V3.Texture.X, V1.Texture.Y, TZ),
										Position = new Vector3F(V3.Position.X, V1.Position.Y, PZ),
										Normal = new Vector3F(V3.Normal.X, V1.Normal.Y, NZ),
									};

									V4 = new VertexInfo()
									{
										Texture = new Vector3F(V1.Texture.X, V3.Texture.Y, TZ),
										Position = new Vector3(V1.Position.X, V3.Position.Y, PZ),
										Normal = new Vector3F(V1.Normal.X, V3.Normal.Y, NZ),
									};

									V4.Color = V3.Color = V2.Color = V1.Color = Color;
									V4.Position.Z = V3.Position.Z = V2.Position.Z = V1.Position.Z = PZ;
									V4.Normal.Z = V3.Normal.Z = V2.Normal.Z = V1.Normal.Z = NZ;
									V4.Texture.Z = V3.Texture.Z = V2.Texture.Z = V1.Texture.Z = NZ;
								}
								PutVertex(ref V1, ref VertexType);
								PutVertex(ref V2, ref VertexType);
								PutVertex(ref V3, ref VertexType);
								PutVertex(ref V4, ref VertexType);
							}
						}
						else
						{
							VertexInfo VertexInfo;
							//Console.Error.WriteLine("{0} : {1} : {2}", BeginMode, VertexCount, VertexType.Index);
							for (int n = 0; n < VertexCount; n++)
							{
								ReadVertex(n, &VertexInfo);
								PutVertex(ref VertexInfo, ref VertexType);
							}
						}
					}
					GL.End();
				}
			}

			_CaptureEndPrimitive();

			//Console.WriteLine(VertexCount);

			//PrepareWrite(GpuState);

#if DEBUG_VERTEX_TYPE
			if (OutputVertexInfoStream != null)
			{
				OutputVertexInfoStream.Close();
				OutputVertexInfoStream = null;
			}
#endif
		}

		readonly byte[] TempBuffer = new byte[512 * 512 * 4];

		/*
		 * 
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

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareRead(GpuStateStruct* GpuState)
		{
#if true
//#else
			var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
			int Width = (int)GpuState->DrawBufferState.Width;
			int Height = 272;
			int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
			int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
			//GpuState->DrawBufferState.Format
			var Address = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address, 0);
			GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);
			//Console.WriteLine("PrepareRead: {0:X}", Address);

			try
			{
				GL.WindowPos2(0, 272);
				GL.PixelZoom(1, -1);

				GL.DrawPixels(Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(Address));
				//GL.DrawPixels(512, 272, PixelFormat.AbgrExt, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));

				//GL.WindowPos2(0, 0);
				//GL.PixelZoom(1, 1);
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
#endif
		}

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareWrite(GpuStateStruct* GpuState)
		{
			//Console.WriteLine("PrepareWrite");
			try
			{
				var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
				int Width = (int)GpuState->DrawBufferState.Width;
				int Height = 272;
				int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
				int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
				//GpuState->DrawBufferState.Format
				var Address = (void *)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address);

				//Console.WriteLine("{0}", GlPixelFormat.GuPixelFormat);

				//Console.WriteLine("{0:X}", GpuState->DrawBufferState.Address);
				GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);

#if false
				//GL.WindowPos2(0, 272);
				//GL.PixelZoom(1, -1);

				GL.WindowPos2(0, 0);
				GL.PixelZoom(1, 1);

				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(Address));
#else
				fixed (void* _TempBufferPtr = &TempBuffer[0])
				{
					GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(_TempBufferPtr));

					var Input = (byte*)_TempBufferPtr;
					var Output = (byte*)Address;

					for (int Row = 0; Row < Height; Row++)
					{
						var ScanIn = (byte *)&Input[ScanWidth * Row];
						var ScanOut = (byte*)&Output[ScanWidth * (Height - Row - 1)];
						//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
						PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
					}
				}
#endif
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}

			if (SwapBuffers)
			{
				GraphicsContext.SwapBuffers();
			}
		}

		[HandleProcessCorruptedStateExceptions()]
		public override void Finish(GpuStateStruct* GpuState)
		{
			//PrepareWrite(GpuState);
			//return;
			/*
			if (GpuState->DrawBufferState.LowAddress != 0)
			{
				//var Address = PspMemory.FrameBufferOffset | GpuState->DrawBufferState.LowAddress;
				var Address = GpuState->DrawBufferState.Address;
				try
				{
					Console.WriteLine("{0:X}", Address);
					Memory.CheckAndEnforceAddressValid(Address);
					GL.ReadPixels(0, 0, 512, 272, PixelFormat.Rgba, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));
				}
				catch (Exception Exception)
				{
					// 0x04000000
					Console.WriteLine("Address: {0:X}", Address);
					Console.WriteLine(Exception);
					//throw(Exception);
				}
			}
			*/
		}

		public override void End(GpuStateStruct* GpuState)
		{
			PrepareWrite(GpuState);
			//throw new NotImplementedException();
		}

		public override void TextureFlush(GpuStateStruct* GpuState)
		{
			TextureCache.RecheckAll();
			//Console.WriteLine("TextureFlush!");
			//base.TextureFlush(GpuState);
		}

		public override void TextureSync(GpuStateStruct* GpuState)
		{
			//Console.WriteLine("TextureSync!");
			//base.TextureSync(GpuState);
		}

		public override void AddedDisplayList()
		{
			//TextureCache.RecheckAll();
			//throw new NotImplementedException();
		}

		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "OpenGl",
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
