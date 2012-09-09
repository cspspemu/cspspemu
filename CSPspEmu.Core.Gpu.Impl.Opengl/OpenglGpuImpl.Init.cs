//#define USE_GL_CONTROL
//#define SHOW_WINDOW

using System;
using System.Globalization;
using System.Threading;

#if OPENTK
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using CSharpUtils;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public partial class OpenglGpuImpl
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
		static private INativeWindow NativeWindow;
#endif

		static public int GlGetInteger(GetPName Name)
		{
			int Value;
			GL.GetInteger(Name, out Value);
			return Value;
		}

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

					var UsedGraphicsMode = new GraphicsMode(
						color: new OpenTK.Graphics.ColorFormat(8, 8, 8, 8),
						depth: 16,
						stencil: 8,
						samples: 0,
						accum: new OpenTK.Graphics.ColorFormat(16, 16, 16, 16),
						//accum: new OpenTK.Graphics.ColorFormat(0, 0, 0, 0),
						buffers: 2,
						stereo: false
					);

					var UsedGameWindowFlags = GameWindowFlags.Default;

					//Console.Error.WriteLine(UsedGraphicsMode);
					//Console.ReadKey();
#if USE_GL_CONTROL
					GLControl = new GLControl(UsedGraphicsMode, 3, 0, GraphicsContextFlags.Default);
#else
					NativeWindow = new NativeWindow(512, 272, "PspGraphicEngine", UsedGameWindowFlags, UsedGraphicsMode, DisplayDevice.GetDisplay(DisplayIndex.Default));
#endif
					
#if SHOW_WINDOW
					NativeWindow.Visible = true;
#endif
					//Utilities.CreateWindowsWindowInfo(handle);

					GraphicsContext = new GraphicsContext(UsedGraphicsMode, WindowInfo);
					GraphicsContext.MakeCurrent(WindowInfo);
					{
						GraphicsContext.LoadAll();
						Initialize();
					}
					GraphicsContext.SwapInterval = 0;

#if true
					//Console.WriteLine("## {0}", UsedGraphicsMode);
					Console.WriteLine("## UsedGraphicsMode: {0}", UsedGraphicsMode);
					Console.WriteLine("## GraphicsContext.GraphicsMode: {0}", GraphicsContext.GraphicsMode);

					Console.WriteLine("## OpenGL Context Version: {0}.{1}", GlGetInteger(GetPName.MajorVersion), GlGetInteger(GetPName.MinorVersion));

					Console.WriteLine("## Depth Bits: {0}", GlGetInteger(GetPName.DepthBits));
					Console.WriteLine("## Stencil Bits: {0}", GlGetInteger(GetPName.StencilBits));
					Console.WriteLine("## Accum Bits: {0},{1},{2},{3}", GlGetInteger(GetPName.AccumRedBits), GlGetInteger(GetPName.AccumGreenBits), GlGetInteger(GetPName.AccumBlueBits), GlGetInteger(GetPName.AccumAlphaBits));

					if (GlGetInteger(GetPName.StencilBits) <= 0)
					{
						ConsoleUtils.SaveRestoreConsoleState(() =>
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Error.WriteLine("No stencil bits available!");
						});
					}

					/*
					GL.Enable(EnableCap.StencilTest);
					GL.StencilMask(0xFF);
					GL.ClearColor(new Color4(Color.FromArgb(0x11, 0x22, 0x33, 0x44)));
					GL.ClearStencil(0x7F);
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.StencilBufferBit);

					var TestData = new uint[16 * 16];
					TestData[0] = 0x12345678;
					GL.ReadPixels(0, 0, 16, 16, PixelFormat.Rgba, PixelType.UnsignedInt8888Reversed, TestData);
					Console.WriteLine(GL.GetError());
					for (int n = 0; n < TestData.Length; n++) Console.Write("{0:X}", TestData[n]);
					*/
#endif

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
