//#define SHOW_WINDOW

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	unsafe public partial class OpenglGpuImpl
	{
		//Thread CThread;
		AutoResetEvent StopEvent = new AutoResetEvent(false);

		bool Running = true;

		/// <summary>
		/// 
		/// </summary>
		static public IGraphicsContext GraphicsContext;

		/// <summary>
		/// 
		/// </summary>
		static public INativeWindow NativeWindow;

		/// <summary>
		/// 
		/// </summary>
		static public bool AlreadyInitialized = false;

		public bool IsCurrentWindow = false;

		public override void SetCurrent()
		{
			if (!IsCurrentWindow)
			{
				GraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
				IsCurrentWindow = true;
			}
		}

		public override void UnsetCurrent()
		{
			GraphicsContext.MakeCurrent(null);
			IsCurrentWindow = false;
		}

#if SHOW_WINDOW
		public bool SwapBuffers = true;
#else
		public bool SwapBuffers = false;
#endif

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://www.opentk.com/doc/graphics/graphicscontext"/>
		public override void InitSynchronizedOnce()
		{
			if (!AlreadyInitialized)
			{
				AlreadyInitialized = true;
				AutoResetEvent CompletedEvent = new AutoResetEvent(false);
				var CThread = new Thread(() =>
				{
					Thread.CurrentThread.CurrentCulture = new CultureInfo(PspConfig.CultureName);
					// Index: 8, Color: 32 (8888), Depth: 24, Stencil: 0, Samples: 0, Accum: 64 (16161616), Buffers: 2, Stereo: False
					//var UsedGraphicsMode = GraphicsMode.Default;
					var UsedGraphicsMode = new GraphicsMode(
						color: new ColorFormat(8, 8, 8, 8),
						depth: 16,
						stencil: 8,
						samples: 0,
						accum: new ColorFormat(16, 16, 16, 16),
						buffers: 2,
						stereo: false
					);
					//var UsedGraphicsMode = new GraphicsMode(color: new ColorFormat(32), depth: 24, stencil: 8, samples: 0);
					var UsedGameWindowFlags = GameWindowFlags.Default;
					//Console.Error.WriteLine(UsedGraphicsMode);
					//Console.ReadKey();
					NativeWindow = new NativeWindow(512, 272, "PspGraphicEngine", UsedGameWindowFlags, UsedGraphicsMode, DisplayDevice.Default);
#if SHOW_WINDOW
					NativeWindow.Visible = true;
#endif
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
				CThread.Name = "GpuImplEventHandling";
				CThread.IsBackground = true;
				CThread.Start();
				CompletedEvent.WaitOne();
			}
		}

		public override void StopSynchronized()
		{
			//Running = false;
			//StopEvent.WaitOne();

			//GraphicsContext.Dispose();
			//NativeWindow.Dispose();
		}
	}
}
