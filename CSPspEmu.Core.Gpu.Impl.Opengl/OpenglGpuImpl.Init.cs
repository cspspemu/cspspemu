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
					NativeWindow = new NativeWindow(512, 272, "PspGraphicEngine", GameWindowFlags.Default, GraphicsMode.Default, DisplayDevice.Default);
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
