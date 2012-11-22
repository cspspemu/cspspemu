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
		Uniform fColor;
		Uniform u_has_vertex_color;

		VertexAttribLocation vColorLocation;
		VertexAttribLocation vPositionLocation;

		public override void InitSynchronizedOnce()
		{
			ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Magenta, () =>
			{
				Console.WriteLine("Gpu.InitSynchronizedOnce::Thread({0})", Thread.CurrentThread.ManagedThreadId);
			});

			var FragmentProgram = Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.OpenglEs.shader.fragment").ReadAllContentsAsString(Encoding.UTF8);
			var VertexProgram = Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Core.Gpu.Impl.OpenglEs.shader.vertex").ReadAllContentsAsString(Encoding.UTF8);

			this.GraphicsContext = new OffscreenContext(512, 272);
			this.GraphicsContext.SetCurrent();
			GL.glViewport(0, 0, 512, 272);
			{
				this.ShaderProgram = ShaderProgram.CreateProgram(VertexProgram, FragmentProgram);
				{
					this.vColorLocation = this.ShaderProgram.GetVertexAttribLocation("a_color0");
					this.vPositionLocation = this.ShaderProgram.GetVertexAttribLocation("a_position");
				}
				this.ShaderProgram.Link();
				{
					this.projectionMatrix = this.ShaderProgram.GetUniformLocation("projectionMatrix");
					this.viewMatrix = this.ShaderProgram.GetUniformLocation("viewMatrix");
					this.worldMatrix = this.ShaderProgram.GetUniformLocation("worldMatrix");
					this.fColor = this.ShaderProgram.GetUniformLocation("u_color");
					this.u_has_vertex_color = this.ShaderProgram.GetUniformLocation("u_has_vertex_color");
				}
			}

			//TestingRender();
		}

		public override void StopSynchronized()
		{
		}

		VertexInfo[] VertexInfoBuffer = new VertexInfo[1024 * 1024];

		public override void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
			var VertexType = GpuState->VertexState.Type;

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
			}
			else
			{
				projectionMatrix.SetMatrix4(GpuState->VertexState.ProjectionMatrix.Values);
				worldMatrix.SetMatrix4(GpuState->VertexState.WorldMatrix.Values);
				viewMatrix.SetMatrix4(GpuState->VertexState.ViewMatrix.Values);
			}

			if (GpuState->ClearingMode)
			{
				GL.glDisable(GL.GL_CULL_FACE);
				this.fColor.SetVec4(0, 0, 0, 1);
			}
			else
			{
				if (PrimitiveType == GuPrimitiveType.Sprites)
				{
					GL.glDisable(GL.GL_CULL_FACE);
				}
				else
				{
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
					vPositionLocation.Pointer(3, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Position"));
					vPositionLocation.Enable();
					if (VertexType.ColorSize != 0)
					{
						vColorLocation.Pointer(4, GL.GL_FLOAT, false, sizeof(VertexInfo), ((byte*)VertexInfoBufferPtr) + (int)Marshal.OffsetOf(typeof(VertexInfo), "Color"));
						vColorLocation.Enable();
						u_has_vertex_color.SetBool(true);
					}
					else
					{
						u_has_vertex_color.SetBool(false);
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
					Name = "OpenglEs",
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
