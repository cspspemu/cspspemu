#define USE_GL_CONTROL
//#define DO_NOT_USE_STENCIL
//#define SHOW_WINDOW

using System;
using System.Globalization;
using System.Threading;
using CSharpPlatform.GL;
using System.Runtime.InteropServices;
using CSPspEmu.Utils;
using OpenTK;
using OpenTK.Graphics;

namespace CSPspEmu.Core.Gpu.Impl.Opengl
{
    public unsafe partial class OpenglGpuImpl
    {
        static public GraphicsContext MyContext;
        
        //Thread CThread;
        AutoResetEvent StopEvent = new AutoResetEvent(false);
        
        

        bool Running = true;

        /// <summary>
        /// 
        /// </summary>
        public static IGlContext OpenglContext;

        /// <summary>
        /// 
        /// </summary>
        public static bool AlreadyInitialized;

        public bool IsCurrentWindow;

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

        public static string GlGetString(int name) => GL.GetString(name);

        /// <summary>
        /// 
        /// </summary>
        /// <see cref="http://www.opentk.com/doc/graphics/graphicscontext"/>
        //[HandleProcessCorruptedStateExceptions]
        public override void InitSynchronizedOnce()
        {
            //Memory.WriteBytesHook += OnMemoryWrite;
            ScaleViewport = PspStoredConfig.RenderScale;

            if (!AlreadyInitialized)
            {
                AlreadyInitialized = true;
                var completedEvent = new AutoResetEvent(false);

                new Thread(() =>
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(GlobalConfig.ThreadCultureName);

                    OpenglContext = GlContextFactory.CreateWindowless();
                    OpenglContext.MakeCurrent();

                    try
                    {
                        Console.Out.WriteLineColored(ConsoleColor.White, "## OpenGL Context Version: {0}, {1}",
                            GlGetString(GL.GL_VERSION), GlGetString(GL.GL_RENDERER));
                        Console.Out.WriteLineColored(ConsoleColor.White, "## Depth Bits: {0}",
                            GL.glGetInteger(GL.GL_DEPTH_BITS));
                        Console.Out.WriteLineColored(ConsoleColor.White, "## Stencil Bits: {0}",
                            GL.glGetInteger(GL.GL_STENCIL_BITS));
                        Console.Out.WriteLineColored(ConsoleColor.White, "## Color Bits: {0},{1},{2},{3}",
                            GL.glGetInteger(GL.GL_RED_BITS), GL.glGetInteger(GL.GL_GREEN_BITS),
                            GL.glGetInteger(GL.GL_BLUE_BITS), GL.glGetInteger(GL.GL_ALPHA_BITS));

                        if (GL.glGetInteger(GL.GL_STENCIL_BITS) <= 0)
                        {
                            Console.Error.WriteLineColored(ConsoleColor.Red, "No stencil bits available!");
                            //throw new Exception("Couldn't initialize opengl");
                        }

                        OpenglContext.ReleaseCurrent();

                        completedEvent.Set();
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
                    } catch (Exception e) {
                        Console.WriteLine("OpenglGpuImpl.Init.Error: {0}", e);
                    }
                })
                {
                    Name = "GpuImplEventHandling",
                    IsBackground = true
                }.Start();

                completedEvent.WaitOne();
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