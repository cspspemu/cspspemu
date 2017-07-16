#define USE_GL_CONTROL
//#define DO_NOT_USE_STENCIL
//#define SHOW_WINDOW

using System;
using System.Globalization;
using System.Threading;

using CSharpUtils;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using CSharpPlatform.GL;
using System.Runtime.InteropServices;
using CSharpPlatform.GL.Impl;

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
		public static IGLContext OpenglContext;

		/// <summary>
		/// 
		/// </summary>
		public static bool AlreadyInitialized = false;

		public bool IsCurrentWindow = false;

		public override void SetCurrent()
		{
			if (!IsCurrentWindow)
			{
				OpenglContext.MakeCurrent();
				IsCurrentWindow = true;
			}
		}

		public override void UnsetCurrent()
		{
			OpenglContext.ReleaseCurrent();
			IsCurrentWindow = false;
		}

		public static string GlGetString(int Name)
		{
			return Marshal.PtrToStringAnsi(new IntPtr(GL.glGetString(Name)));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <see cref="http://www.opentk.com/doc/graphics/graphicscontext"/>
		//[HandleProcessCorruptedStateExceptions]
		public override void InitSynchronizedOnce()
		{
			//Memory.WriteBytesHook += OnMemoryWrite;
			this.ScaleViewport = PspStoredConfig.RenderScale;

			if (!AlreadyInitialized)
			{
				AlreadyInitialized = true;
				AutoResetEvent CompletedEvent = new AutoResetEvent(false);
				var CThread = new Thread(() =>
				{
					Thread.CurrentThread.CurrentCulture = new CultureInfo(GlobalConfig.ThreadCultureName);

					OpenglContext = GLContextFactory.CreateWindowless();
					OpenglContext.MakeCurrent();

					Console.Out.WriteLineColored(ConsoleColor.White, "## OpenGL Context Version: {0}", GlGetString(GL.GL_VERSION));
					Console.Out.WriteLineColored(ConsoleColor.White, "## Depth Bits: {0}", GL.glGetInteger(GL.GL_DEPTH_BITS));
					Console.Out.WriteLineColored(ConsoleColor.White, "## Stencil Bits: {0}", GL.glGetInteger(GL.GL_STENCIL_BITS));
					Console.Out.WriteLineColored(ConsoleColor.White, "## Color Bits: {0},{1},{2},{3}", GL.glGetInteger(GL.GL_RED_BITS),
						GL.glGetInteger(GL.GL_GREEN_BITS), GL.glGetInteger(GL.GL_BLUE_BITS), GL.glGetInteger(GL.GL_ALPHA_BITS));

					if (GL.glGetInteger(GL.GL_STENCIL_BITS) <= 0)
					{
						Console.Error.WriteLineColored(ConsoleColor.Red, "No stencil bits available!");
					}

					OpenglContext.ReleaseCurrent();

					CompletedEvent.Set();
					Console.WriteLine("OpenglGpuImpl.Init.Start()");
					try
					{
						while (Running)
						{
							Thread.Sleep(10);
						}
						StopEvent.Set();
					}
					finally
					{
						Console.WriteLine("OpenglGpuImpl.Init.End()");
					}
				})
				{
					Name = "GpuImplEventHandling",
					IsBackground = true,
				};
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
