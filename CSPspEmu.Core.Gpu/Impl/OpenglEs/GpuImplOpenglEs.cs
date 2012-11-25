using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLES;
using System.Runtime.ExceptionServices;
using CSPspEmu.Core.Gpu.State;
using CSPspEmu.Core.Utils;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using System.Runtime.InteropServices;
using System.Threading;
using System.Reflection;
using Mono.Simd;
using CSPspEmu.Core.Gpu.State.SubStates;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
	public sealed unsafe partial class GpuImplOpenglEs : GpuImpl
    {
		[Inject]
		private PspMemory Memory;

		OffscreenContext GraphicsContext;
		ShaderProgram ShaderProgram;

		Uniform projectionMatrix;
		Uniform viewMatrix;
		Uniform worldMatrix;
		Uniform textureMatrix;

		Uniform fColor;
		Uniform u_has_vertex_color;

		Uniform u_has_texture;
		Uniform u_texture;
		Uniform u_texture_effect;

		VertexAttribLocation aColorLocation;
		VertexAttribLocation aPositionLocation;
		VertexAttribLocation aTexCoord;

		TextureCacheOpengles TextureCache;
		TextureOpengles CurrentTexture;

		public override void InitializeComponent()
		{
			base.InitializeComponent();

			this.TextureCache = new TextureCacheOpengles(Memory, this);
		}

		private string glVENDOR;
		private string glRENDERER;
		private string glVERSION;
		private string glSLVERSION;
		private string glEXTENSIONS;

		public override void InitSynchronizedOnce()
		{
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
			{
				Console.WriteLine("Gpu.InitSynchronizedOnce::Thread({0})", Thread.CurrentThread.ManagedThreadId);
			});

			var FragmentProgram = Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.OpenglEs.shader.fragment").ReadAllContentsAsString(Encoding.UTF8);
			var VertexProgram = Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.OpenglEs.shader.vertex").ReadAllContentsAsString(Encoding.UTF8);

			this.GraphicsContext = new OffscreenContext(512, 272);

			this.glVENDOR = GL.glGetString2(GL.GL_VENDOR);
			this.glRENDERER = GL.glGetString2(GL.GL_RENDERER);
			this.glVERSION = GL.glGetString2(GL.GL_VERSION);
			this.glSLVERSION = GL.glGetString2(GL.GL_SHADING_LANGUAGE_VERSION);
			this.glEXTENSIONS = GL.glGetString2(GL.GL_EXTENSIONS);

			this.GraphicsContext.SetCurrent();
			GL.glViewport(0, 0, 512, 272);
			{
				this.ShaderProgram = ShaderProgram.CreateProgram(VertexProgram, FragmentProgram);
				{
					this.aColorLocation = this.ShaderProgram.GetVertexAttribLocation("a_color0");
					this.aPositionLocation = this.ShaderProgram.GetVertexAttribLocation("a_position");
					this.aTexCoord = this.ShaderProgram.GetVertexAttribLocation("a_texCoord");
				}
				this.ShaderProgram.Link();
				{
					// Matrices
					this.projectionMatrix = this.ShaderProgram.GetUniformLocation("projectionMatrix");
					this.viewMatrix = this.ShaderProgram.GetUniformLocation("viewMatrix");
					this.worldMatrix = this.ShaderProgram.GetUniformLocation("worldMatrix");
					this.textureMatrix = this.ShaderProgram.GetUniformLocation("textureMatrix");

					// Colors
					this.fColor = this.ShaderProgram.GetUniformLocation("u_color");
					this.u_has_vertex_color = this.ShaderProgram.GetUniformLocation("u_has_vertex_color");

					// Textures
					this.u_has_texture = this.ShaderProgram.GetUniformLocation("u_has_texture");
					this.u_texture = this.ShaderProgram.GetUniformLocation("u_texture");
					this.u_texture_effect = this.ShaderProgram.GetUniformLocation("u_texture_effect");
				}
			}

			//TestingRender();
		}

		public override void StopSynchronized()
		{
		}

		VertexInfo[] VertexInfoBuffer = new VertexInfo[1024 * 1024];

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

		public override void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
			var VertexType = GpuState->VertexState.Type;
			var TextureState = &GpuState->TextureMappingState.TextureState;

			var VertexReader = new VertexReader();

			VertexReader.SetVertexTypeStruct(
				VertexType,
				(byte*)Memory.PspAddressToPointerSafe(GlobalGpuState.GetAddressRelativeToBaseOffset(GpuState->VertexAddress), 0)
			);

			// Set Matrices
			this.ShaderProgram.Use();

			if (VertexType.Transform2D)
			{
				projectionMatrix.SetMatrix4(Matrix4.Ortho(0, 480, 272, 0, 0, -0xFFFF));
				worldMatrix.SetMatrix4(Matrix4.Identity);
				viewMatrix.SetMatrix4(Matrix4.Identity);
				textureMatrix.SetMatrix4(Matrix4.Identity);
				GL.glDepthRangef(0f, 1f);
			}
			else
			{
				GL.glDepthRangef(GpuState->DepthTestState.RangeNear, GpuState->DepthTestState.RangeFar);
				PrepareState_DepthTest(GpuState);
				projectionMatrix.SetMatrix4(GpuState->VertexState.ProjectionMatrix.Values);
				worldMatrix.SetMatrix4(GpuState->VertexState.WorldMatrix.Values);
				viewMatrix.SetMatrix4(GpuState->VertexState.ViewMatrix.Values);

				textureMatrix.SetMatrix4(Matrix4.Identity);
				/*
				textureMatrix.SetMatrix4(Matrix4.Identity
					.Translate(TextureState->OffsetU, TextureState->OffsetV, 0)
					.Scale(TextureState->ScaleU, TextureState->ScaleV, 1)
				);
				*/
			}

			if (GpuState->ClearingMode)
			{
				PrepareStateClear(GpuState);
			}
			else
			{
				if (PrimitiveType == GuPrimitiveType.Sprites)
				{
					GL.glDisable(GL.GL_CULL_FACE);
				}
				else
				{
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

			fixed (VertexInfo* VertexInfoBufferPtr = VertexInfoBuffer)
			{
				switch (PrimitiveType)
				{
					case GuPrimitiveType.Sprites:
						int m = 0;
						for (int n = 0; n < VertexCount; n += 2)
						{
							VertexInfo TopLeft, _TopRight, BottomRight, _BottomLeft;

							VertexReader.ReadVertex(n + 0, &TopLeft);
							VertexReader.ReadVertex(n + 1, &BottomRight);
							TopLeft.Color.W = 1.0f;
							BottomRight.Color.W = 1.0f;

							{
								//if (GpuState->ClearingMode) Console.WriteLine("{0} - {1}", VertexInfoTopLeft, VertexInfoBottomRight);

								var Color = BottomRight.Color;
								var TZ = TopLeft.Texture.Z;
								var PZ = TopLeft.Position.Z;
								var NZ = TopLeft.Normal.Z;

								_TopRight = new VertexInfo()
								{
									Texture = new Vector4f(BottomRight.Texture.X, TopLeft.Texture.Y, TZ, 0),
									Position = new Vector4f(BottomRight.Position.X, TopLeft.Position.Y, PZ, 0),
									Normal = new Vector4f(BottomRight.Normal.X, TopLeft.Normal.Y, NZ, 0),
								};

								_BottomLeft = new VertexInfo()
								{
									Texture = new Vector4f(TopLeft.Texture.X, BottomRight.Texture.Y, TZ, 0),
									Position = new Vector4f(TopLeft.Position.X, BottomRight.Position.Y, PZ, 0),
									Normal = new Vector4f(TopLeft.Normal.X, BottomRight.Normal.Y, NZ, 0),
								};

								_BottomLeft.Color = BottomRight.Color = _TopRight.Color = TopLeft.Color = Color;
								_BottomLeft.Position.Z = BottomRight.Position.Z = _TopRight.Position.Z = TopLeft.Position.Z = PZ;
								_BottomLeft.Normal.Z = BottomRight.Normal.Z = _TopRight.Normal.Z = TopLeft.Normal.Z = NZ;
								_BottomLeft.Texture.Z = BottomRight.Texture.Z = _TopRight.Texture.Z = TopLeft.Texture.Z = NZ;
							}

							//VertexInfoBufferPtr[m++] = _TopRight;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = TopLeft;
							//VertexInfoBufferPtr[m++] = _BottomLeft;

							VertexInfoBufferPtr[m++] = _TopRight;
							VertexInfoBufferPtr[m++] = TopLeft;
							VertexInfoBufferPtr[m++] = BottomRight;
							VertexInfoBufferPtr[m++] = _BottomLeft;

							//VertexInfoBufferPtr[m++] = _BottomLeft;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = _TopRight;
							//VertexInfoBufferPtr[m++] = TopLeft;

							//VertexInfoBufferPtr[m++] = _BottomLeft;
							//VertexInfoBufferPtr[m++] = TopLeft;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = _TopRight;

							//VertexInfoBufferPtr[m++] = _BottomLeft;
							//VertexInfoBufferPtr[m++] = TopLeft;
							//VertexInfoBufferPtr[m++] = BottomRight;
							//VertexInfoBufferPtr[m++] = _TopRight;
						}
						VertexCount = (ushort)m;
						break;
					default:
						VertexReader.ReadVertices(0, VertexInfoBufferPtr, VertexCount);
						break;
				}

				//if (PrimitiveType == GuPrimitiveType.LineStrip)
				{
					aPositionLocation.Pointer(3, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Position"));
					aPositionLocation.Enable();

					if (VertexType.ColorSize != 0)
					{
						aColorLocation.Pointer(4, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Color"));
						aColorLocation.Enable();
						u_has_vertex_color.SetBool(true);
					}
					else
					{
						aColorLocation.Disable();
						u_has_vertex_color.SetBool(false);
					}


					if (GpuState->TextureMappingState.Enabled)
					{
						aTexCoord.Pointer(3, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Texture"));
						aTexCoord.Enable();
						u_has_texture.SetBool(true);
						u_texture.SetInt(0);
						u_texture_effect.SetInt((int)TextureState->Effect);
					}
					else
					{
						aTexCoord.Disable();
						u_has_texture.SetBool(false);
					}

					GL.glDrawArrays(PrimitiveTypeTranslate[(int)PrimitiveType], 0, VertexCount);
				}

				//TestingRender();
			}
		}

		public override void Finish(GpuStateStruct* GpuState)
		{
		}

		public override void End(GpuStateStruct* GpuState)
		{
			/*
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
			{
				Console.WriteLine("Gpu.End::Thread({0})", Thread.CurrentThread.ManagedThreadId);
			});

			TestingRender();
			*/
			
			PrepareWrite(GpuState);
		}

		public override void AddedDisplayList()
		{
		}

		public override void SetCurrent()
		{
			this.GraphicsContext.SetCurrent();
		}

		public override void UnsetCurrent()
		{
			this.GraphicsContext.UnsetCurrent();
		}

		readonly byte[] TempBuffer = new byte[512 * 512 * 4];

		private void PrepareWrite(GpuStateStruct* GpuState)
		{
			//Console.WriteLine("PrepareWrite");
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
			GL.glPixelStorei(GL.GL_PACK_ALIGNMENT, PixelSize);

			fixed (void* _TempBufferPtr = &TempBuffer[0])
			{
				GL.glReadPixels(0, 0, Width, Height, GL.GL_RGBA, GlPixelFormat.OpenglPixelType, _TempBufferPtr);
				/*
				for (int n = 0; n < 512 * 272 * 4; n++)
				{
					if (TempBuffer[n] != 0)
					{
						Console.WriteLine(TempBuffer[n]);
					}
				}
				*/

				var Input = (byte*)_TempBufferPtr;
				var Output = (byte*)Address;

				for (int Row = 0; Row < Height; Row++)
				{
					var ScanIn = (byte*)&Input[ScanWidth * Row];
					var ScanOut = (byte*)&Output[ScanWidth * (Height - Row - 1)];
					//Console.WriteLine("{0}:{1},{2},{3}", Row, PixelSize, Width, ScanWidth);
					PointerUtils.Memcpy(ScanOut, ScanIn, ScanWidth);
				}
			}
		}

		public bool SwapBuffers = false;

		public override PluginInfo PluginInfo
		{
			get {
				return new PluginInfo()
				{
					Name = "OpenglEs - " + this.glRENDERER + " - " + this.glVERSION + " - " + this.glSLVERSION + " - " + this.glVENDOR,
					Version = "0.1",
				};
			}
		}

		public override bool IsWorking
		{
			get
			{
				return OffscreenContext.IsWorking;
			}
		}
	}
}
