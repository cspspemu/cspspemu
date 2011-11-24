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
//using Cloo;
//using Cloo.Bindings;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	sealed unsafe public class OpenglGpuImpl : IGpuImpl
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
		IGraphicsContext GraphicsContext;

		/// <summary>
		/// 
		/// </summary>
		INativeWindow NativeWindow;

		/// <summary>
		/// 
		/// </summary>
		int VertexShader;

		/// <summary>
		/// 
		/// </summary>
		int FragmentShader;

		/// <summary>
		/// 
		/// </summary>
		int GeometryShader;

		/// <summary>
		/// 
		/// </summary>
		int ProgramShaderProgram;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Config"></param>
		/// <param name="Memory"></param>
		public OpenglGpuImpl(PspConfig Config, PspMemory Memory)
		{
			this.Config = Config;
			this.Memory = Memory;
		}

		private void PrepareShaders()
		{
			/*
			var ComputeContext = new ComputeContext(ComputeDeviceTypes.Accelerator, new ComputeContextPropertyList(ComputePlatform.Platforms[0]), null, IntPtr.Zero);
			ComputeProgram ComputeProgram = new ComputeProgram(ComputeContext, @"

				}
			");
			ComputeProgram.Build(ComputeContext.Devices, "", null, IntPtr.Zero);

			var Kernel = ComputeProgram.CreateKernel("interopTeste");

			var ComputeCommandQueue = new ComputeCommandQueue(ComputeContext, ComputeContext.Devices[0], ComputeCommandQueueFlags.None);
			//ComputeCommandQueue.Write(
			ComputeCommandQueue.Execute(Kernel);
			*/


			//new ComputeBuffer<byte>(
			//ComputeBuffer<byte>.CreateFromGLBuffer<byte>();
			/*
			ProgramShaderProgram = GL.CreateProgram();
	
			VertexShader = GL.CreateShader(ShaderType.VertexShader);
			GL.ShaderSource(VertexShader, "void main() { gl_Position = ftransform();}");
			GL.CompileShader(VertexShader);
			var VertexShaderInfo = GL.GetShaderInfoLog(VertexShader);
			Console.WriteLine("VertexShaderInfo: {0}", VertexShaderInfo);
			GL.AttachShader(ProgramShaderProgram, VertexShader);

			FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
			GL.ShaderSource(FragmentShader, "void main() { gl_Position = ftransform();}");
			GL.CompileShader(FragmentShader);
			var FragmentShaderInfo = GL.GetShaderInfoLog(VertexShader);
			Console.WriteLine("FragmentShaderInfo: {0}", FragmentShaderInfo);
			GL.AttachShader(ProgramShaderProgram, FragmentShader);

			GeometryShader = GL.CreateShader(ShaderType.GeometryShader);
			GL.ShaderSource(GeometryShader, "void main() { gl_Position = ftransform();}");
			GL.CompileShader(GeometryShader);
			var GeometryShaderInfo = GL.GetShaderInfoLog(VertexShader);
			Console.WriteLine("GeometryShaderInfo: {0}", GeometryShaderInfo);
			GL.AttachShader(ProgramShaderProgram, GeometryShader);

			GL.LinkProgram(ProgramShaderProgram);
			GL.UseProgram(ProgramShaderProgram);
			*/
		}

		void Initialize()
		{
			///GL.Enable(EnableCap.Blend);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GpuState"></param>
		public unsafe void Prim(GpuStateStruct* GpuState, PrimitiveType PrimitiveType, ushort VertexCount)
		{
			//return;
			PrepareRead(GpuState);

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

				uint VertexSize = GpuState[0].VertexState.Type.GetVertexSize();

				byte* VertexPtr = (byte *)Memory.PspAddressToPointerSafe(GpuState[0].VertexAddress);

				//Console.WriteLine(VertexSize);

				GL.Begin(BeginMode.TriangleStrip);
				for (int n = 0; n < VertexCount; n++)
				{
					byte* CurrentVertexPtr = VertexPtr + VertexSize * n;
					float x = *(float*)(CurrentVertexPtr + 4);
					float y = *(float*)(CurrentVertexPtr + 8);
					float z = *(float*)(CurrentVertexPtr + 12);
					GL.Vertex3(x, y, z);
					GL.Color4(CurrentVertexPtr[3], CurrentVertexPtr[2], CurrentVertexPtr[1], CurrentVertexPtr[0]);
					//Console.WriteLine("{0}, {1}, {2}", x, y, z);
				}
				GL.End();
				GL.Flush();

				//Console.WriteLine(VertexCount);
			}

			PrepareWrite(GpuState);
		}

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareRead(GpuStateStruct* GpuState)
		{
			var Address = GpuState[0].DrawBufferState.Address;
			//Console.WriteLine("PrepareRead: {0:X}", Address);
			try
			{
				GL.DrawPixels(512, 272, PixelFormat.Rgba, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
		}

		[HandleProcessCorruptedStateExceptions()]
		private void PrepareWrite(GpuStateStruct* GpuState)
		{
			try
			{
				var Address = GpuState[0].DrawBufferState.Address;
				GL.ReadPixels(0, 0, 512, 272, PixelFormat.Rgba, PixelType.UnsignedInt8888, new IntPtr(Memory.PspAddressToPointerSafe(Address)));
			}
			catch (Exception Exception)
			{
				Console.WriteLine(Exception);
			}
		}

		[HandleProcessCorruptedStateExceptions()]
		public void Finish(GpuStateStruct* GpuState)
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

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://www.opentk.com/doc/graphics/graphicscontext"/>
		public void Init()
		{
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
					PrepareShaders();
					Initialize();
				}
				GraphicsContext.MakeCurrent(null);
				CompletedEvent.Set();
				while (true)
				{
					NativeWindow.ProcessEvents();
					Thread.Sleep(1);
				}
			});
			CThread.IsBackground = true;
			CThread.Start();
			CompletedEvent.WaitOne();
		}
	}
}
