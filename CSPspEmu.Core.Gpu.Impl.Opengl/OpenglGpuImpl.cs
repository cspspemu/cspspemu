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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System.Globalization;
using CSPspEmu.Core.Memory;
//using Cloo;
//using Cloo.Bindings;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public class OpenglGpuImpl : GpuImpl
	{
		/// <summary>
		/// 
		/// </summary>
		private PspConfig Config;

		/// <summary>
		/// 
		/// </summary>
		private PspMemory Memory;

		/// <summary>
		/// 
		/// </summary>
		VertexReader VertexReader;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Config"></param>
		/// <param name="Memory"></param>
		public OpenglGpuImpl(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			this.Config = PspEmulatorContext.PspConfig;
			this.Memory = PspEmulatorContext.GetInstance<PspMemory>();
			this.VertexReader = new VertexReader();
		}

		void Initialize()
		{
			///GL.Enable(EnableCap.Blend);
		}

		private void PrepareState_CullFace(GpuStateStruct* GpuState)
		{
			if (GlEnableDisable(EnableCap.CullFace, GpuState[0].BackfaceCullingState.Enabled))
			{
				GL.CullFace((GpuState[0].BackfaceCullingState.FrontFaceDirection == State.SubStates.FrontFaceDirectionEnum.ClockWise) ? CullFaceMode.Front : CullFaceMode.Back);
			}
		}

		private void PrepareState_Texture(GpuStateStruct* GpuState)
		{
			//if (GlEnableDisable(EnableCap.Texture2D, GpuState[0].TextureMappingState.Enabled))
			{
				//GL
				//Console.WriteLine("aaaaaaaaaaaaa");
			}
		}

		private void PrepareState_Lighting(GpuStateStruct* GpuState)
		{
			//Console.WriteLine(GpuState[0].LightingState.AmbientModelColor);
			
			var Color = GpuState[0].LightingState.AmbientModelColor;
			GL.Color4(&Color.Red);

			//if (GlEnableDisable(EnableCap.Lighting, GpuState[0].LightingState.Enabled))
			{
			}
		}

		private void PrepareState(GpuStateStruct* GpuState)
		{
			PrepareState_CullFace(GpuState);
			PrepareState_Texture(GpuState);
			PrepareState_Lighting(GpuState);

			//GL.Disable(EnableCap.Blend);

			GL.ShadeModel((GpuState[0].ShadeModel == ShadingModelEnum.Flat) ? ShadingModel.Flat : ShadingModel.Smooth);
		}

		private void PutVertex(ref VertexInfo VertexInfo, ref VertexTypeStruct VertexType)
		{
			//Console.WriteLine(VertexInfo);
			if (VertexType.Color != VertexTypeStruct.ColorEnum.Void)
			{
				GL.Color4((byte)VertexInfo.R, (byte)VertexInfo.G, (byte)VertexInfo.B, (byte)VertexInfo.A);
			}
			if (VertexType.Texture != VertexTypeStruct.NumericEnum.Void)
			{
				GL.TexCoord2(VertexInfo.U, VertexInfo.V);
			}
			if (VertexType.Normal != VertexTypeStruct.NumericEnum.Void)
			{
				GL.Normal3(VertexInfo.NX, VertexInfo.NY, VertexInfo.NZ);
			}
			if (VertexType.Position != VertexTypeStruct.NumericEnum.Void)
			{
				GL.Vertex3(VertexInfo.PX, VertexInfo.PY, VertexInfo.PZ);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GpuState"></param>
		override public unsafe void Prim(GpuStateStruct* GpuState, PrimitiveType PrimitiveType, ushort VertexCount)
		{
			//Console.WriteLine("--------------------------------------------------------");
			var VertexType = GpuState[0].VertexState.Type;

			VertexReader.SetVertexTypeStruct(VertexType, (byte*)Memory.PspAddressToPointerSafe(GpuState[0].VertexAddress));
			
			//VertexType.Texture == VertexTypeStruct.TextureEnum.Byte
			//return;
			//PrepareRead(GpuState);

			if (GpuState[0].ClearingMode)
			{
				GraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
				//GL.ClearColor(1, 1, 0, 0);

				// @TODO: Fake
				GL.ClearColor(0, 0, 0, 0);
				GL.Clear(ClearBufferMask.ColorBufferBit);
			}
			else
			{
				if (GpuState[0].VertexState.Type.Transform2D)
				{
					GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
					GL.Ortho(0, 512, 272, 0, -0x7FFF, +0x7FFF);
					GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();
				}
				else
				{
					GL.MatrixMode(MatrixMode.Projection); GL.LoadIdentity();
					{
						GL.MultMatrix(GpuState[0].VertexState.ProjectionMatrix.Values);
					}

					GL.MatrixMode(MatrixMode.Modelview); GL.LoadIdentity();
					GL.MultMatrix(GpuState[0].VertexState.ViewMatrix.Values);
					GL.MultMatrix(GpuState[0].VertexState.WorldMatrix.Values);

					if (GpuState[0].VertexState.WorldMatrix.Values[0] == float.NaN)
					{
						throw (new Exception("Invalid WorldMatrix"));
					}

					//GpuState[0].VertexState.ViewMatrix.Dump();
					//GpuState[0].VertexState.WorldMatrix.Dump();

					//Console.WriteLine("NO Transform2D");
				}

				PrepareState(GpuState);

				uint VertexSize = GpuState[0].VertexState.Type.GetVertexSize();

				byte* VertexPtr = (byte*)Memory.PspAddressToPointerSafe(GpuState[0].VertexAddress);

				//Console.WriteLine(VertexSize);

				BeginMode BeginMode = default(BeginMode);

				switch (PrimitiveType)
				{
					case PrimitiveType.Lines: BeginMode = BeginMode.Lines; break;
					case PrimitiveType.LineStrip: BeginMode = BeginMode.LineStrip; break;
					case PrimitiveType.Triangles: BeginMode = BeginMode.Triangles; break;
					case PrimitiveType.Points: BeginMode = BeginMode.Points; break;
					case PrimitiveType.TriangleFan: BeginMode = BeginMode.TriangleFan; break;
					case PrimitiveType.TriangleStrip: BeginMode = BeginMode.TriangleStrip; break;
					case PrimitiveType.Sprites: BeginMode = BeginMode.Quads; break;
					default: throw(new NotImplementedException("Not implemented PrimitiveType:'" + PrimitiveType + "'"));
				}

				//Console.WriteLine(BeginMode);

				GL.Begin(BeginMode);
				{
					if (PrimitiveType == PrimitiveType.Sprites)
					{
						for (int n = 0; n < VertexCount; n += 2)
						{
							VertexInfo VertexInfoTopLeft;
							VertexInfo VertexInfoTopRight;
							VertexInfo VertexInfoBottomRight;
							VertexInfo VertexInfoBottomLeft;
							VertexReader.ReadVertex(n + 0, &VertexInfoTopLeft);
							VertexReader.ReadVertex(n + 1, &VertexInfoBottomRight);

							VertexInfoTopRight = new VertexInfo()
							{
								U = VertexInfoBottomRight.U,
								V = VertexInfoTopLeft.V,
								PX = VertexInfoBottomRight.PX,
								PY = VertexInfoTopLeft.PY,
								PZ = (VertexInfoTopLeft.PZ + VertexInfoBottomRight.PZ) / 2,
								NX = VertexInfoBottomRight.NX,
								NY = VertexInfoTopLeft.NY,
								NZ = (VertexInfoTopLeft.NZ + VertexInfoBottomRight.NZ) / 2,
								R = (byte)((VertexInfoTopLeft.R + VertexInfoBottomRight.R) / 2),
								G = (byte)((VertexInfoTopLeft.G + VertexInfoBottomRight.G) / 2),
								B = (byte)((VertexInfoTopLeft.B + VertexInfoBottomRight.B) / 2),
								A = (byte)((VertexInfoTopLeft.A + VertexInfoBottomRight.A) / 2),
							};

							VertexInfoBottomLeft = new VertexInfo()
							{
								U = VertexInfoTopLeft.U,
								V = VertexInfoBottomRight.V,
								PX = VertexInfoTopLeft.PX,
								PY = VertexInfoBottomRight.PY,
								PZ = (VertexInfoTopLeft.PZ + VertexInfoBottomRight.PZ) / 2,
								NX = VertexInfoTopLeft.NX,
								NY = VertexInfoBottomRight.NY,
								NZ = (VertexInfoTopLeft.NZ + VertexInfoBottomRight.NZ) / 2,
								R = (byte)((VertexInfoTopLeft.R + VertexInfoBottomRight.R) / 2),
								G = (byte)((VertexInfoTopLeft.G + VertexInfoBottomRight.G) / 2),
								B = (byte)((VertexInfoTopLeft.B + VertexInfoBottomRight.B) / 2),
								A = (byte)((VertexInfoTopLeft.A + VertexInfoBottomRight.A) / 2),
							};

							PutVertex(ref VertexInfoTopLeft, ref VertexType);
							PutVertex(ref VertexInfoTopRight, ref VertexType);
							PutVertex(ref VertexInfoBottomRight, ref VertexType);
							PutVertex(ref VertexInfoBottomLeft, ref VertexType);

						}
					}
					else
					{
						for (int n = 0; n < VertexCount; n++)
						{
							VertexInfo VertexInfo;
							VertexReader.ReadVertex(n, &VertexInfo);
							PutVertex(ref VertexInfo, ref VertexType);

						}
					}
				}
				GL.End();
				GL.Flush();

				//Console.WriteLine(VertexCount);
			}

			//PrepareWrite(GpuState);
		}

		static private bool GlEnableDisable(EnableCap EnableCap, bool EnableDisable)
		{
			if (EnableDisable)
			{
				GL.Enable(EnableCap);
			}
			else
			{
				GL.Disable(EnableCap);
			}
			return EnableDisable;
		}

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareRead(GpuStateStruct* GpuState)
		{
			/*
			var Address = GpuState[0].DrawBufferState.Address;
			//Console.WriteLine("PrepareRead: {0:X}", Address);

			try
			{
				//GL.WindowPos2(0, 272);
				//GL.PixelZoom(1, -1);

				GL.DrawPixels(512, 272, PixelFormat.Rgba, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));
				//GL.DrawPixels(512, 272, PixelFormat.AbgrExt, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));

				//GL.WindowPos2(0, 0);
				//GL.PixelZoom(1, 1);
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
			*/
		}

		readonly byte[] TempBuffer = new byte[512 * 272 * 4];

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareWrite(GpuStateStruct* GpuState)
		{
			try
			{
				int Width = 512;
				int Height = 272;
				int Width4 = Width * 4;
				var Address = (void *)Memory.PspAddressToPointerSafe(GpuState[0].DrawBufferState.Address);
				GL.ReadPixels(0, 0, Width, Height, PixelFormat.Rgba, PixelType.UnsignedInt8888, TempBuffer);
				fixed (void* _TempBufferPtr = &TempBuffer[0])
				{
					var Input = (byte*)_TempBufferPtr;
					var Output = (byte*)Address;

					for (int Row = 0; Row < Height; Row++)
					{
						var ScanIn = &Input[Width4 * Row];
						var ScanOut = &Output[Width4 * (Height - Row - 1)];
						//var ScanOut = &Output[Width4 * Row];
						for (int n = 0; n < Width4; n += 4)
						{
							ScanOut[n + 0] = ScanIn[n + 1];
							ScanOut[n + 1] = ScanIn[n + 2];
							ScanOut[n + 2] = ScanIn[n + 3];
							ScanOut[n + 3] = ScanIn[n + 0];
						}
					}
				}
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
		}

		[HandleProcessCorruptedStateExceptions()]
		public override void Finish(GpuStateStruct* GpuState)
		{
			//return;
			/*
			if (GpuState[0].DrawBufferState.LowAddress != 0)
			{
				//var Address = PspMemory.FrameBufferOffset | GpuState[0].DrawBufferState.LowAddress;
				var Address = GpuState[0].DrawBufferState.Address;
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

		//Thread CThread;
		AutoResetEvent StopEvent = new AutoResetEvent(false);
		bool Running = true;

		/// <summary>
		/// 
		/// </summary>
		static IGraphicsContext GraphicsContext;

		/// <summary>
		/// 
		/// </summary>
		static INativeWindow NativeWindow;

		/// <summary>
		/// 
		/// </summary>
		static bool AlreadySynchronized = false;

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://www.opentk.com/doc/graphics/graphicscontext"/>
		public override void InitSynchronizedOnce()
		{
			if (!AlreadySynchronized)
			{
				AlreadySynchronized = true;
				AutoResetEvent CompletedEvent = new AutoResetEvent(false);
				var CThread = new Thread(() =>
				{
					Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.CultureName);
					NativeWindow = new OpenTK.NativeWindow(512, 272, "PspGraphicEngine", GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.Default);
					NativeWindow.Visible = false;
					GraphicsContext = new GraphicsContext(GraphicsMode.Default, NativeWindow.WindowInfo);
					GraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
					{
						(GraphicsContext as IGraphicsContextInternal).LoadAll();
						Initialize();
					}
					GraphicsContext.MakeCurrent(null);
					CompletedEvent.Set();
					while (Running)
					{
						NativeWindow.ProcessEvents();
						Thread.Sleep(1);
					}
					StopEvent.Set();
				});
				CThread.IsBackground = true;
				CThread.Start();
				CompletedEvent.WaitOne();
			}
		}

		public override void StopSynchronized()
		{
			//Running = false;
			//StopEvent.WaitOne();
		}

		public override void End(GpuStateStruct* GpuState)
		{
			PrepareWrite(GpuState);
			//throw new NotImplementedException();
		}
	}
}
