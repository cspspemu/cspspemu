#define USE_GL_CONTROL
//#define DO_NOT_USE_STENCIL
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
using System.Diagnostics;
using System.Runtime.ExceptionServices;
#else
using MiniGL;
#endif

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
	unsafe public sealed partial class OpenglGpuImpl
	{
		//Thread CThread;
		AutoResetEvent StopEvent = new AutoResetEvent(false);

		bool Running = true;

		/// <summary>
		/// 
		/// </summary>
		public static IGraphicsContext RenderGraphicsContext;

		/// <summary>
		/// 
		/// </summary>
		public static bool AlreadyInitialized = false;

		public bool IsCurrentWindow = false;

		public static IWindowInfo WindowInfo
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
				RenderGraphicsContext.MakeCurrent(WindowInfo);
				IsCurrentWindow = true;
			}
		}

		public override void UnsetCurrent()
		{
			RenderGraphicsContext.MakeCurrent(null);
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
		private static GLControl GLControl;
#else
		/// <summary>
		/// 
		/// </summary>
		private static INativeWindow NativeWindow;
#endif

		public static int GlGetInteger(GetPName Name)
		{
			int Value;
			GL.GetInteger(Name, out Value);
			return Value;
		}

		public static GraphicsMode UsedGraphicsMode = new GraphicsMode(
			color: new OpenTK.Graphics.ColorFormat(8, 8, 8, 8),
			depth: 16,
#if DO_NOT_USE_STENCIL
			stencil: 0,
#else
			stencil: 8,
#endif
			samples: 0,
			accum: new OpenTK.Graphics.ColorFormat(16, 16, 16, 16),
			//accum: new OpenTK.Graphics.ColorFormat(0, 0, 0, 0),
			buffers: 2,
			stereo: false
		);

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://www.opentk.com/doc/graphics/graphicscontext"/>
		//[HandleProcessCorruptedStateExceptions]
		public override void InitSynchronizedOnce()
		{
			//Memory.WriteBytesHook += OnMemoryWrite;

			if (!AlreadyInitialized)
			{
				AlreadyInitialized = true;
				AutoResetEvent CompletedEvent = new AutoResetEvent(false);
				var CThread = new Thread(() =>
				{
					//CompletedEvent.Set(); return;

					Thread.CurrentThread.CurrentCulture = new CultureInfo(GlobalConfig.ThreadCultureName);

					//GraphicsContext.DirectRendering = true;

					//Console.Error.WriteLine(UsedGraphicsMode);
					//Console.ReadKey();

#if USE_GL_CONTROL
					//try
					//{
						GLControl = new GLControl(UsedGraphicsMode, 3, 0, GraphicsContextFlags.Default);
						GLControl.Size = new System.Drawing.Size(512, 272);
						RenderGraphicsContext = GLControl.Context;
					//}
					//catch (AccessViolationException)
					//{
					//	UsedGraphicsMode = GraphicsMode.Default;
					//	GLControl = new GLControl(GraphicsMode.Default, 3, 0, GraphicsContextFlags.Default);
					//	GLControl.Size = new System.Drawing.Size(512, 272);
					//	RenderGraphicsContext = GLControl.Context;
					//}
#else
					NativeWindow = new NativeWindow(512, 272, "PspGraphicEngine", GameWindowFlags.Default, UsedGraphicsMode, DisplayDevice.GetDisplay(DisplayIndex.Default));
					RenderGraphicsContext = new GraphicsContext(UsedGraphicsMode, WindowInfo);
#endif

					RenderGraphicsContext.MakeCurrent(WindowInfo);
					{
						RenderGraphicsContext.LoadAll();
						Initialize();
					}
					RenderGraphicsContext.SwapInterval = 0;

#if !USE_GL_CONTROL
#if SHOW_WINDOW
					NativeWindow.Visible = true;
#endif
#endif
					//Utilities.CreateWindowsWindowInfo(handle);

					//Console.WriteLine("## {0}", UsedGraphicsMode);
					Console.WriteLine("## UsedGraphicsMode: {0}", UsedGraphicsMode);
					Console.WriteLine("## GraphicsContext.GraphicsMode: {0}", RenderGraphicsContext.GraphicsMode);

					Console.WriteLine("## OpenGL Context Version: {0}.{1}", GlGetInteger(GetPName.MajorVersion), GlGetInteger(GetPName.MinorVersion));

					Console.WriteLine("## Depth Bits: {0}", GlGetInteger(GetPName.DepthBits));
					Console.WriteLine("## Stencil Bits: {0}", GlGetInteger(GetPName.StencilBits));
					Console.WriteLine("## Accum Bits: {0},{1},{2},{3}", GlGetInteger(GetPName.AccumRedBits), GlGetInteger(GetPName.AccumGreenBits), GlGetInteger(GetPName.AccumBlueBits), GlGetInteger(GetPName.AccumAlphaBits));

					if (GlGetInteger(GetPName.StencilBits) <= 0)
					{
						ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
						{
							Console.Error.WriteLine("No stencil bits available!");
						});
					}

					RenderGraphicsContext.MakeCurrent(null);
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
