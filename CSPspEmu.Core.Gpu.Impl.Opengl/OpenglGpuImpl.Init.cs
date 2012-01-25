//#define USE_GL_CONTROL
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
using OpenTK.Platform;

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
		static public bool AlreadyInitialized = false;

		public bool IsCurrentWindow = false;

		public IWindowInfo WindowInfo
		{
			get
			{
#if USE_GL_CONTROL
				if (GLControl == null) throw (new Exception("GLControl NULL!"));
				if (GLControl.WindowInfo == null) throw (new Exception("GLControl not prepared!"));
				return GLControl.WindowInfo;
#else
				return NativeWindow.WindowInfo;
#endif
			}
		}

		public override void SetCurrent()
		{
			if (!IsCurrentWindow)
			{
				GraphicsContext.MakeCurrent(WindowInfo);
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

#if USE_GL_CONTROL
		/// <summary>
		/// 
		/// </summary>
		static protected GLControl GLControl;
#else
		/// <summary>
		/// 
		/// </summary>
		static protected INativeWindow NativeWindow;
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
					/*
					var UsedGraphicsMode = new GraphicsMode(
						color: new ColorFormat(8, 8, 8, 8),
						depth: 16,
						stencil: 8,
						samples: 0,
						accum: new ColorFormat(16, 16, 16, 16),
						buffers: 2,
						stereo: false
					);
					*/

					GraphicsMode gm = GraphicsMode.Default;
					var UsedGraphicsMode = new GraphicsMode(
						gm.ColorFormat,
						gm.Depth,
						8, //gm.Stencil,
						gm.Samples, // 4 // anti-alias
						gm.AccumulatorFormat,
						gm.Buffers,
						gm.Stereo
					);

					//var UsedGraphicsMode = new GraphicsMode(color: new ColorFormat(32), depth: 24, stencil: 8, samples: 0);
					var UsedGameWindowFlags = GameWindowFlags.Default;
					//Console.Error.WriteLine(UsedGraphicsMode);
					//Console.ReadKey();
#if USE_GL_CONTROL
					GLControl = new GLControl(UsedGraphicsMode, 3, 0, GraphicsContextFlags.Default);
#else
					NativeWindow = new NativeWindow(512, 272, "PspGraphicEngine", UsedGameWindowFlags, UsedGraphicsMode, DisplayDevice.Default);
#endif

#if SHOW_WINDOW
					NativeWindow.Visible = true;
#endif

					GraphicsContext = new GraphicsContext(GraphicsMode.Default, WindowInfo);
					GraphicsContext.MakeCurrent(WindowInfo);
					{
						(GraphicsContext as IGraphicsContextInternal).LoadAll();
						Initialize();
					}
					GraphicsContext.MakeCurrent(null);
					CompletedEvent.Set();
					while (Running)
					{
#if !USE_GL_CONTROL
						NativeWindow.ProcessEvents();
#endif
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
