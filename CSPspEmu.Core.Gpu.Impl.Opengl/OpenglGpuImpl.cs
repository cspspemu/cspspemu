using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSPspEmu.Core.Gpu.State;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;

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
		/// <param name="Config"></param>
		/// <param name="Memory"></param>
		public OpenglGpuImpl(PspConfig Config, PspMemory Memory)
		{
			this.Config = Config;
			this.Memory = Memory;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="GpuState"></param>
		public unsafe void Prim(GpuStateStruct* GpuState, PrimitiveType PrimitiveType, ushort VertexCount)
		{
			GraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
			//GL.ClearColor(1, 1, 1, 1);
			//GL.Clear(ClearBufferMask.ColorBufferBit);

			//GL.


			/*
			var Data = new byte[512 * 272 * 4];

			fixed (byte* DataPtr = Data)
			{
				GL.ReadPixels(0, 0, 480, 272, PixelFormat.Rgba, PixelType.UnsignedInt8888, new IntPtr(DataPtr));
			}
			Console.WriteLine(String.Join(",", Data));
			*/
			//GL.Enable(EnableCap.Blend);
			//throw new NotImplementedException();
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
				NativeWindow = new OpenTK.NativeWindow(512, 272, "PspGraphicEngine", GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.Default);
				NativeWindow.Visible = false;
				GraphicsContext = new GraphicsContext(GraphicsMode.Default, NativeWindow.WindowInfo);
				GraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
				(GraphicsContext as IGraphicsContextInternal).LoadAll();
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
