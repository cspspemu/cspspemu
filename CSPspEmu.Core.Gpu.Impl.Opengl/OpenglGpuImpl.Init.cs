using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;

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
		public static IGraphicsContext GraphicsContext;

		/// <summary>
		/// 
		/// </summary>
		public static INativeWindow NativeWindow;

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
	}
}
