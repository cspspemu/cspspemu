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

#if OPENTK
using OpenTK.Graphics.OpenGL;
using CSPspEmu.Core.Gpu.Formats;
using Mono.Simd;
using CSPspEmu.Core.Utils;
using CSPspEmu.Core.Types;
using CSharpPlatform;
using CSPspEmu.Core.Gpu.State.SubStates;
using System.Collections.Generic;
using CSPspEmu.Core.Cpu;
#else
using MiniGL;
#endif

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
		private TextureCacheOpengl TextureCache;

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

		void IInjectInitialize.Initialize()
		{
			this.TextureCache = new TextureCacheOpengl(this.Memory, this);
			this.VertexReader = new VertexReader();
		}

		//public static object GpuLock = new object();

		static public int FrameBufferTexture = -1;

		/// <summary>
		/// 
		/// </summary>
		static private void Initialize()
		{
			FrameBufferTexture = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexture);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new uint[] { 0xFF000000 });
			
			GL.BindTexture(TextureTarget.Texture2D, 0);

			GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, Color.FromArgb(1, 0, 0, 0));
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="VertexInfo"></param>
		/// <returns></returns>
		private VertexInfo PerformSkinning(VertexInfo VertexInfo)
		{
			int SkinningWeightCount = VertexType.RealSkinningWeightCount;
			if (SkinningWeightCount == 0) return VertexInfo;

			var OutputPosition = default(Vector4fRaw);
			var OutputNormal = default(Vector4fRaw);
			var InputPosition = VertexInfo.Position;
			var InputNormal = VertexInfo.Normal;

			var BoneMatrices = &GpuState->SkinningState.BoneMatrix0;

			for (int m = 0; m < SkinningWeightCount; m++)
			{
				var BoneMatrix = BoneMatrices[m];
				var BoneMatrixValues = BoneMatrix.Values;
				float Weight = VertexInfo.Weights[m];

				BoneMatrix.SetLastColumn();

				if (Weight != 0)
				{
					OutputPosition.X += (InputPosition.X * BoneMatrixValues[0] + InputPosition.Y * BoneMatrixValues[4] + InputPosition.Z * BoneMatrixValues[8] + 1 * BoneMatrixValues[12]) * Weight;
					OutputPosition.Y += (InputPosition.X * BoneMatrixValues[1] + InputPosition.Y * BoneMatrixValues[5] + InputPosition.Z * BoneMatrixValues[9] + 1 * BoneMatrixValues[13]) * Weight;
					OutputPosition.Z += (InputPosition.X * BoneMatrixValues[2] + InputPosition.Y * BoneMatrixValues[6] + InputPosition.Z * BoneMatrixValues[10] + 1 * BoneMatrixValues[14]) * Weight;

					OutputNormal.X += (InputNormal.X * BoneMatrixValues[0] + InputNormal.Y * BoneMatrixValues[4] + InputNormal.Z * BoneMatrixValues[8] + 0 * BoneMatrixValues[12]) * Weight;
					OutputNormal.Y += (InputNormal.X * BoneMatrixValues[1] + InputNormal.Y * BoneMatrixValues[5] + InputNormal.Z * BoneMatrixValues[9] + 0 * BoneMatrixValues[13]) * Weight;
					OutputNormal.Z += (InputNormal.X * BoneMatrixValues[2] + InputNormal.Y * BoneMatrixValues[6] + InputNormal.Z * BoneMatrixValues[10] + 0 * BoneMatrixValues[14]) * Weight;
				}
			}

			VertexInfo.Position = OutputPosition;
			VertexInfo.Normal = OutputNormal;

			return VertexInfo;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_VertexInfo"></param>
		/// <param name="VertexType"></param>
		private void PutVertex(ref VertexInfo _VertexInfo, ref VertexTypeStruct VertexType)
		{
			VertexInfo VertexInfo = PerformSkinning(_VertexInfo);

			_CapturePutVertex(ref VertexInfo);

#if DEBUG_VERTEX_TYPE
			if (OutputVertexInfoStream != null) OutputVertexInfoStream.WriteBytes(Encoding.UTF8.GetBytes(String.Format("{0}\n", VertexInfo)));
#endif

			if (VertexType.Color != VertexTypeStruct.ColorEnum.Void)
			{
				GL.Color4(VertexInfo.Color.X, VertexInfo.Color.Y, VertexInfo.Color.Z, VertexInfo.Color.W);
			}
			if (VertexType.Texture != VertexTypeStruct.NumericEnum.Void)
			{
				GL.TexCoord3(VertexInfo.Texture.X, VertexInfo.Texture.Y, VertexInfo.Texture.Z);
			}
			//Console.Write(",{0}", VertexInfo.PZ);
			if (VertexType.Normal != VertexTypeStruct.NumericEnum.Void)
			{
				if (VertexType.ReversedNormal)
				{
					GL.Normal3(-VertexInfo.Normal.X, -VertexInfo.Normal.Y, -VertexInfo.Normal.Z);
				}
				else
				{
					GL.Normal3(VertexInfo.Normal.X, VertexInfo.Normal.Y, VertexInfo.Normal.Z);
				}
			}
			if (VertexType.Position != VertexTypeStruct.NumericEnum.Void)
			{
				GL.Vertex3(VertexInfo.Position.X, VertexInfo.Position.Y, VertexInfo.Position.Z);
			}
		}

		private void ReadVertex_Byte(int Index, VertexInfo* VertexInfo)
		{
			*VertexInfo = Vertices[IndexListByte[Index]];
		}

		private void ReadVertex_Short(int Index, VertexInfo* VertexInfo)
		{
			*VertexInfo = Vertices[IndexListShort[Index]];
		}

		private void ReadVertex_Void(int Index, VertexInfo* VertexInfo)
		{
			*VertexInfo = Vertices[Index];
		}

		private delegate void ReadVertexDelegate(int Index, VertexInfo* VertexInfo);

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

		private void ResetState()
		{
			GL.Viewport(0, 0, 512, 272);
			GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
			GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Fastest);
			GL.Hint(HintTarget.PointSmoothHint, HintMode.Fastest);
			foreach (var Item in Enum.GetValues(typeof(EnableCap)).Cast<EnableCap>())
			{
				GL.Disable(Item);
			}

			GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
			GL.Ortho(0, 480, 272, 0, 0, -0xFFFF);

			GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();
			GL.MatrixMode(MatrixMode.Color); GL.LoadIdentity();
			GL.Color3(Color.White);
		}

		private static readonly GuPrimitiveType[] patch_prim_types = { GuPrimitiveType.TriangleStrip, GuPrimitiveType.LineStrip, GuPrimitiveType.Points };

		public override void DrawCurvedSurface(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuStateStruct, VertexInfo[,] Patch, int UCount, int VCount)
		{
			//GpuState->TextureMappingState.Enabled = true;

			//ResetState();
			PrepareStateCommon(GpuState);
			PrepareStateDraw(GpuState);
			PrepareStateMatrix(GpuState);

#if true
			PrepareState_Texture_Common(GpuState);
			PrepareState_Texture_3D(GpuState);

			GL.ActiveTexture(TextureUnit.Texture0);
#endif

			//GL.ActiveTexture(TextureUnit.Texture0);
			//GL.Disable(EnableCap.Texture2D);

			var VertexType = GpuStateStruct->VertexState.Type;

			//GL.Color3(Color.White);

			int s_len = Patch.GetLength(0);
			int t_len = Patch.GetLength(1);

			float s_len_float = s_len;
			float t_len_float = t_len;

			var Mipmap0 = &GpuStateStruct->TextureMappingState.TextureState.Mipmap0;

			float MipmapWidth = Mipmap0->TextureWidth;
			float MipmapHeight = Mipmap0->TextureHeight;

			//float MipmapWidth = 1f;
			//float MipmapHeight = 1f;

			GL.Begin(BeginMode.Triangles);
			{
				for (int t = 0; t < t_len - 1; t++)
				{
					for (int s = 0; s < s_len - 1; s++)
					{
						var VertexInfo1 = Patch[s + 0, t + 0];
						var VertexInfo2 = Patch[s + 0, t + 1];
						var VertexInfo3 = Patch[s + 1, t + 1];
						var VertexInfo4 = Patch[s + 1, t + 0];

						if (VertexType.Texture != VertexTypeStruct.NumericEnum.Void)
						{
							VertexInfo1.Texture.X = ((float)s + 0) * MipmapWidth / s_len_float;
							VertexInfo1.Texture.Y = ((float)t + 0) * MipmapHeight / t_len_float;

							VertexInfo2.Texture.X = ((float)s + 0) * MipmapWidth / s_len_float;
							VertexInfo2.Texture.Y = ((float)t + 1) * MipmapHeight / t_len_float;

							VertexInfo3.Texture.X = ((float)s + 1) * MipmapWidth / s_len_float;
							VertexInfo3.Texture.Y = ((float)t + 1) * MipmapHeight / t_len_float;

							VertexInfo4.Texture.X = ((float)s + 1) * MipmapWidth / s_len_float;
							VertexInfo4.Texture.Y = ((float)t + 0) * MipmapHeight / t_len_float;
						}

						PutVertex(ref VertexInfo1, ref VertexType);
						PutVertex(ref VertexInfo2, ref VertexType);
						PutVertex(ref VertexInfo3, ref VertexType);

						PutVertex(ref VertexInfo1, ref VertexType);
						PutVertex(ref VertexInfo3, ref VertexType);
						PutVertex(ref VertexInfo4, ref VertexType);

						//GL.Color3(Color.White);
						//Console.WriteLine("{0}, {1} : {2}", s, t, VertexInfo1);
					}
				}
			}
			GL.End();

		}

		public override unsafe void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
#if SLOW_SIMPLE_RENDER_TARGET
			PrepareRead(GpuState);
#endif

			//Console.WriteLine("VertexCount: {0}", VertexCount);
			var Start = DateTime.UtcNow;
			{
				_Prim(GlobalGpuState, GpuState, PrimitiveType, VertexCount);
			}
			var End = DateTime.UtcNow;

#if SLOW_SIMPLE_RENDER_TARGET
			PrepareWrite(GpuState);
#endif


			//Console.Error.WriteLine("Prim: {0}", End - Start);

			if (!GpuState->ClearingMode)
			{
				//Console.WriteLine("{0}", (*GpuState).ToStringDefault());
			}
		}

#if DEBUG_VERTEX_TYPE
		Stream OutputVertexInfoStream;
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GlobalGpuState"></param>
		/// <param name="GpuState"></param>
		/// <param name="PrimitiveType"></param>
		/// <param name="VertexCount"></param>
		private unsafe void _Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
			//if (PrimitiveType == GuPrimitiveType.TriangleStrip) VertexCount++;

			//Console.WriteLine("Prim: {0}, {1}", PrimitiveType, VertexCount);
			this.GpuState = GpuState;

			//Console.WriteLine("--------------------------------------------------------");
			VertexType = GpuState->VertexState.Type;

			ReadVertexDelegate ReadVertex = ReadVertex_Void;
			VertexReader.SetVertexTypeStruct(
				VertexType,
				(byte*)Memory.PspAddressToPointerSafe(GlobalGpuState.GetAddressRelativeToBaseOffset(GpuState->VertexAddress), 0)
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
				IndexPointer = Memory.PspAddressToPointerSafe(GlobalGpuState.GetAddressRelativeToBaseOffset(GpuState->IndexAddress), 0);
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

			_CaptureStartPrimitive(PrimitiveType, GlobalGpuState.GetAddressRelativeToBaseOffset(GpuState->VertexAddress), VertexCount, ref VertexType);

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

				//var CurrentTexture = new Texture(this).Load("Bezier2.png");
				//CurrentTexture.Bind();

				//lock (GpuLock)
				{
					//Console.Error.WriteLine("GL.Begin : Thread : {0}", Thread.CurrentThread.ManagedThreadId);
					GL.Begin(BeginMode);
					{
						if (PrimitiveType == GuPrimitiveType.Sprites)
						{
#if DEBUG_PRIM
							if (!GpuState->ClearingMode)
							{
								Console.WriteLine("************************");
							}
#endif
							GL.Disable(EnableCap.CullFace);
							for (int n = 0; n < VertexCount; n += 2)
							{
								VertexInfo V1, V2, V3, V4;

								ReadVertex(n + 0, &V1);
								ReadVertex(n + 1, &V3);
								V1.Color.W = 1.0f;
								V3.Color.W = 1.0f;

								{
									//if (GpuState->ClearingMode) Console.WriteLine("{0} - {1}", VertexInfoTopLeft, VertexInfoBottomRight);

									var Color = V3.Color;
									var TZ = V1.Texture.Z;
									var PZ = V1.Position.Z;
									var NZ = V1.Normal.Z;

									V2 = new VertexInfo()
									{
										Texture = new Vector4fRaw(V3.Texture.X, V1.Texture.Y, TZ, 0),
										Position = new Vector4fRaw(V3.Position.X, V1.Position.Y, PZ, 0),
										Normal = new Vector4fRaw(V3.Normal.X, V1.Normal.Y, NZ, 0),
									};

									V4 = new VertexInfo()
									{
										Texture = new Vector4fRaw(V1.Texture.X, V3.Texture.Y, TZ, 0),
										Position = new Vector4fRaw(V1.Position.X, V3.Position.Y, PZ, 0),
										Normal = new Vector4fRaw(V1.Normal.X, V3.Normal.Y, NZ, 0),
									};

									V4.Color = V3.Color = V2.Color = V1.Color = Color;
									V4.Position.Z = V3.Position.Z = V2.Position.Z = V1.Position.Z = PZ;
									V4.Normal.Z = V3.Normal.Z = V2.Normal.Z = V1.Normal.Z = NZ;
									V4.Texture.Z = V3.Texture.Z = V2.Texture.Z = V1.Texture.Z = NZ;
								}
#if DEBUG_PRIM
								if (!GpuState->ClearingMode)
								{
									Console.WriteLine("--------------------");
									Console.WriteLine("{0}", V1);
									Console.WriteLine("{0}", V2);
									Console.WriteLine("{0}", V3);
									Console.WriteLine("{0}", V4);
								}
#endif
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
					var Error = GL.GetError();
					if (Error != ErrorCode.NoError)
					{
						//Console.Error.WriteLine("GL.Error: GL.End: {0}", Error);
					}
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

		[HandleProcessCorruptedStateExceptions]
		private void PrepareRead(GpuStateStruct* GpuState)
		{
#if true
//#else
			var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
			int Width = (int)GpuState->DrawBufferState.Width;
			if (Width == 0) Width = 512;
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

		int[] pboIds = { -1 };

		static bool UsePbo = false;

		private void PreParePbos()
		{
			if (UsePbo)
			{
				if (pboIds[0] == -1)
				{
					GL.GenBuffers(1, pboIds);
					GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pboIds[0]);
					GL.BufferData(BufferTarget.PixelUnpackBuffer, new IntPtr(512 * 272 * 4), IntPtr.Zero, BufferUsageHint.StreamRead);
					GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
				}
				GL.BindBuffer(BufferTarget.PixelPackBuffer, pboIds[0]);
			}
		}

		private void UnPreParePbos()
		{
			if (UsePbo)
			{
				GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
			}
		}
		
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

		public struct DrawBufferKey
		{
			public uint Address;
			//public int Width;
			//public int Height;
		}

		public class DrawBufferValue : IDisposable
		{
			public DrawBufferKey DrawBufferKey;
			public int FBO;
			public int TextureColor;
			public int TextureDepthStencil;

			public DrawBufferValue(DrawBufferKey DrawBufferKey)
			{
				this.DrawBufferKey = DrawBufferKey;

				GL.GenFramebuffers(1, out FBO);
				GL.GenTextures(1, out TextureColor);
				GL.GenTextures(1, out TextureDepthStencil);

				int Width = 512;
				int Height = 272;
				var EmptyDataPtr = stackalloc uint[Width * Height];
				
				GL.BindTexture(TextureTarget.Texture2D, TextureColor);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new IntPtr(EmptyDataPtr));

				GL.BindTexture(TextureTarget.Texture2D, TextureDepthStencil);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthStencil, Width, Height, 0, PixelFormat.DepthStencil, PixelType.UnsignedInt248, new IntPtr(EmptyDataPtr));
			}

			public void Bind()
			{
				//Console.WriteLine("BindCurrentDrawBufferTexture: 0x{0:X8}, {1}, {2}", DrawBufferKey.Address, TextureColor, TextureDepthStencil);
				GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, FBO);
				//Console.WriteLine(GL.GetError());
				GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureColor, 0);
				//Console.WriteLine(GL.GetError());
				GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.DepthStencilAttachment, TextureTarget.Texture2D, TextureDepthStencil, 0);
				//Console.WriteLine(GL.GetError());
				//Console.WriteLine("{0}, {1}, {2}", FBO, TextureColor, TextureDepthStencil);
			}

			public void Dispose()
			{
				GL.DeleteFramebuffers(1, ref FBO);
				GL.DeleteTextures(1, ref TextureColor);
				GL.DeleteTextures(1, ref TextureDepthStencil);
				FBO = 0;
				TextureColor = 0;
				TextureDepthStencil = 0;
			}
		}

		private readonly Dictionary<DrawBufferKey, DrawBufferValue> DrawBufferTextures = new Dictionary<DrawBufferKey, DrawBufferValue>();

		public void TextureCacheGetAndBind(GpuStateStruct* GpuState)
		{
			if (_DynarecConfig.EnableRenderTarget)
			{
				var TextureMappingState = &GpuState->TextureMappingState;
				var ClutState = &TextureMappingState->ClutState;
				var TextureState = &TextureMappingState->TextureState;
				var Key = new DrawBufferKey()
				{
					Address = TextureState->Mipmap0.Address,
				};

				if (DrawBufferTextures.ContainsKey(Key))
				{
					GL.BindTexture(TextureTarget.Texture2D, GetCurrentDrawBufferTexture(Key).TextureColor);
					return;
				}
			}

			var CurrentTexture = TextureCache.Get(GpuState);
			CurrentTexture.Bind();
		}

		public int GetDrawTexture(DrawBufferKey Key)
		{
			//Console.WriteLine("GetDrawTexture: {0}", GetCurrentDrawBufferTexture(Key).TextureColor);
			return GetCurrentDrawBufferTexture(Key).TextureColor;
		}

		public DrawBufferValue GetCurrentDrawBufferTexture(DrawBufferKey Key)
		{
			if (!DrawBufferTextures.ContainsKey(Key)) DrawBufferTextures[Key] = new DrawBufferValue(Key);
			return DrawBufferTextures[Key];
		}

		private uint CachedBindAddress;

		void BindCurrentDrawBufferTexture(GpuStateStruct* GpuState)
		{
			if (CachedBindAddress != GpuState->DrawBufferState.Address)
			{
				CachedBindAddress = GpuState->DrawBufferState.Address;
				var Key = new DrawBufferKey()
				{
					Address = GpuState->DrawBufferState.Address,
					//Width = (int)GpuState->DrawBufferState.Width,
					//Height = (int)272,
				};
				GetCurrentDrawBufferTexture(Key).Bind();
			}
		}

		public override void BeforeDraw(GpuStateStruct* GpuState)
		{
			BindCurrentDrawBufferTexture(GpuState);
		}

		[HandleProcessCorruptedStateExceptions]
		private void PrepareWrite(GpuStateStruct* GpuState)
		{
			GL.Flush();
			//return;

#if true
			//if (SwapBuffers)
			//{
			//	RenderGraphicsContext.SwapBuffers();
			//}
			//
			//GL.PushAttrib(AttribMask.EnableBit);
			//GL.PushAttrib(AttribMask.TextureBit);
			//{
			//	GL.Enable(EnableCap.Texture2D);
			//	GL.BindTexture(TextureTarget.Texture2D, FrameBufferTexture);
			//	{
			//		//GL.CopyTexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 0, 0, 512, 272);
			//		GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, 512, 272, 0);
			//		//GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, new uint[] { 0xFFFF00FF });
			//	}
			//	GL.BindTexture(TextureTarget.Texture2D, 0);
			//}
			//GL.PopAttrib();
			//GL.PopAttrib();
#else

			//Console.WriteLine("PrepareWrite");
			try
			{
				var GlPixelFormat = GlPixelFormatList[(int)GpuState->DrawBufferState.Format];
				int Width = (int)GpuState->DrawBufferState.Width;
				if (Width == 0) Width = 512;
				int Height = 272;
				int ScanWidth = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, Width);
				int PixelSize = PixelFormatDecoder.GetPixelsSize(GlPixelFormat.GuPixelFormat, 1);
				//GpuState->DrawBufferState.Format
				var Address = (void*)Memory.PspAddressToPointerSafe(GpuState->DrawBufferState.Address);

				//Console.WriteLine("{0}", GlPixelFormat.GuPixelFormat);

				//Console.WriteLine("{0:X}", GpuState->DrawBufferState.Address);
				GL.PixelStore(PixelStoreParameter.PackAlignment, PixelSize);

				fixed (void* _TempBufferPtr = &TempBuffer[0])
				{
					var Input = (byte*)_TempBufferPtr;
					var Output = (byte*)Address;

					PreParePbos();
					if (this.pboIds[0] > 0)
					{
						GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, IntPtr.Zero);
						Input = (byte*)GL.MapBuffer(BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly).ToPointer();
						GL.UnmapBuffer(BufferTarget.PixelPackBuffer);
						if (Input == null)
						{
							Console.WriteLine("PBO ERROR!");
						}
					}
					else
					{
						GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, GlPixelFormat.OpenglPixelType, new IntPtr(_TempBufferPtr));
					}
					UnPreParePbos();

					for (int Row = 0; Row < Height; Row++)
					{
						var ScanIn = (byte*)&Input[ScanWidth * Row];
						var ScanOut = (byte*)&Output[ScanWidth * (Height - Row - 1)];
						//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
						PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
					}
				}
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}

			if (SwapBuffers)
			{
				RenderGraphicsContext.SwapBuffers();
			}
#endif
		}

		[HandleProcessCorruptedStateExceptions]
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
		}

		public override void Sync(GpuStateStruct* GpuState)
		{
			//PrepareWrite(GpuState);
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
