using System;
using System.Runtime.InteropServices;
using System.Security;
using CSharpPlatform.GL.Impl.Windows;
using CSharpPlatform.Library;

namespace CSharpPlatform.GL.Impl
{
    public unsafe class WinGLContext : IGlContext
    {
        IntPtr DC;
        IntPtr Context;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseDC(IntPtr hwnd, IntPtr DC);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern ushort RegisterClassEx(ref ExtendedWindowClass window_class);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

        [DllImport("user32.dll", EntryPoint = "AdjustWindowRectEx", CallingConvention = CallingConvention.StdCall,
             SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern bool AdjustWindowRectEx(ref Rect lpRect, WindowStyle dwStyle, bool bMenu,
            ExtendedWindowStyle dwExStyle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr CreateWindowEx(
            ExtendedWindowStyle ExStyle,
            IntPtr ClassAtom,
            IntPtr WindowName,
            WindowStyle Style,
            int X, int Y,
            int Width, int Height,
            IntPtr HandleToParentWindow,
            IntPtr Menu,
            IntPtr Instance,
            IntPtr Param
        );

        [DllImport("Gdi32.dll")]
        internal static extern IntPtr GetCurrentObject(
            IntPtr hdc,
            uint uObjectType
        );

        [DllImport("Gdi32.dll")]
        internal static extern int GetObject(
            IntPtr hgdiobj,
            int cbBuffer,
            void* lpvObject
        );

        const ClassStyle DefaultClassStyle = ClassStyle.OwnDc;

        private static bool class_registered;

        static readonly IntPtr Instance = Marshal.GetHINSTANCE(typeof(WinGLContext).Module);
        static readonly IntPtr ClassName = Marshal.StringToHGlobalAuto(Guid.NewGuid().ToString());

        const ExtendedWindowStyle ParentStyleEx = ExtendedWindowStyle.WindowEdge | ExtendedWindowStyle.ApplicationWindow
            ;

        private static void RegisterClassOnce()
        {
            if (!class_registered)
            {
                ExtendedWindowClass wc = new ExtendedWindowClass();
                wc.Size = ExtendedWindowClass.SizeInBytes;
                wc.Style = DefaultClassStyle;
                wc.Instance = Instance;
                wc.WndProc = WindowProcedure;
                wc.ClassName = ClassName;
                wc.Icon = IntPtr.Zero;
                wc.IconSm = IntPtr.Zero;
                wc.Cursor = LoadCursor(IntPtr.Zero, (IntPtr) CursorName.Arrow);
                ushort atom = RegisterClassEx(ref wc);

                if (atom == 0)
                    throw new Exception(String.Format("Failed to register window class. Error: {0}",
                        Marshal.GetLastWin32Error()));

                class_registered = true;
            }
        }

        static IntPtr WindowProcedure(IntPtr handle, WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            return DefWindowProc(handle, message, wParam, lParam);
        }

        [SuppressUnmanagedCodeSecurity, DllImport("GDI32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int ChoosePixelFormat(IntPtr hDc, PixelFormatDescriptor* pPfd);

        [SuppressUnmanagedCodeSecurity, DllImport("GDI32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern Boolean SetPixelFormat(IntPtr hdc, int ipfd, PixelFormatDescriptor* ppfd);

        static IntPtr SharedContext;

        public static WinGLContext FromWindowHandle(IntPtr WindowHandle)
        {
            return new WinGLContext(WindowHandle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool AdjustWindowRectEx(ref Rect lpRect, int dwStyle, bool bMenu, int dwExStyle);

        IntPtr hWnd;

        private WinGLContext(IntPtr WinHandle)
        {
            hWnd = WinHandle;
            RegisterClassOnce();
            int Width = 512;
            int Height = 512;

            if (WinHandle == IntPtr.Zero)
            {
                WindowStyle style = WindowStyle.OverlappedWindow | WindowStyle.ClipChildren | WindowStyle.ClipSiblings;
                ExtendedWindowStyle ex_style = ParentStyleEx;

                var rect = new Rect
                {
                    Left = 0,
                    Top = 0,
                    Right = Width,
                    Bottom = Height
                };
                AdjustWindowRectEx(ref rect, style, false, ex_style);

                IntPtr window_name = Marshal.StringToHGlobalAuto("Title");
                hWnd = CreateWindowEx(
                    ex_style, ClassName, window_name, style,
                    rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top,
                    IntPtr.Zero, IntPtr.Zero, Instance, IntPtr.Zero
                );

                if (hWnd == IntPtr.Zero)
                {
                    throw new Exception(String.Format("Failed to create window. Error: {0}",
                        Marshal.GetLastWin32Error()));
                }
            }

            DC = GetDC(hWnd);

            var pfd = new PixelFormatDescriptor();
            pfd.Size = (short) sizeof(PixelFormatDescriptor);
            pfd.Version = 1;
            pfd.Flags = PixelFormatDescriptorFlags.DrawToWindow | PixelFormatDescriptorFlags.SupportOpengl |
                        PixelFormatDescriptorFlags.Doublebuffer;
            pfd.LayerType = 0;
            pfd.PixelType = PixelType.RGBA; // PFD_TYPE_RGBA
            //pfd.ColorBits = 32;
            pfd.ColorBits = 24;
            pfd.DepthBits = 16;
            pfd.StencilBits = 8;

            var pf = ChoosePixelFormat(DC, &pfd);
            //pf = 10;

            if (!SetPixelFormat(DC, pf, &pfd))
            {
                Console.WriteLine("Error SetPixelFormat failed.");
            }

            Context = Wgl.wglCreateContext(DC);
            if (SharedContext != IntPtr.Zero)
            {
                //Console.WriteLine("SharedContext!"); Console.ReadKey();
                if (!Wgl.wglShareLists(SharedContext, Context))
                {
                    throw(new InvalidOperationException("Can't share lists"));
                }
            }
            MakeCurrent();
            Console.Out.WriteLineColored(ConsoleColor.Yellow, "Version:{0}.{1}", GL.glGetInteger(GL.GL_MAJOR_VERSION),
                GL.glGetInteger(GL.GL_MINOR_VERSION));
            DynamicLibraryFactory.MapLibraryToType<Extension>(new DynamicLibraryGl());
            GL.LoadAllOnce();

#if false
			if (Extension.wglCreateContextAttribsARB != null)
			{
				ReleaseCurrent();
				WGL.wglDeleteContext(this.Context);
				fixed (int* AttribListPtr =
new int[] { (int)ArbCreateContext.MajorVersion, 3, (int)ArbCreateContext.MinorVersion, 1, 0, 0 })
				{
					this.Context = Extension.wglCreateContextAttribsARB(DC, SharedContext, AttribListPtr);
				}
				if (this.Context == IntPtr.Zero) throw(new Exception("Error creating context"));
				MakeCurrent();

				Console.WriteLine("OpenGL Version: {0}", Marshal.PtrToStringAnsi(new IntPtr(GL.glGetString(GL.GL_VERSION))));
				//Console.ReadKey();
			}
#endif

            if (SharedContext == IntPtr.Zero)
            {
                SharedContext = Context;
            }

            if (Extension.wglSwapIntervalEXT != null)
            {
                Extension.wglSwapIntervalEXT(0);
            }

            //RECT clientRect;
            //GetClientRect(hWnd, &clientRect);
        }

        public GlContextSize Size
        {
            get
            {
                var bitmapHeader = default(Bitmap);
                var hBitmap = GetCurrentObject(DC, 7);
                GetObject(hBitmap, sizeof(Bitmap), &bitmapHeader);
                return new GlContextSize {Width = (int) bitmapHeader.BmWidth, Height = (int) bitmapHeader.BmHeight};
            }
        }

        public override string ToString()
        {
            return String.Format("WinOpenglContext({0}, {1}, {2}, {3})", DC, Context, SharedContext, Size);
        }

        public enum ArbCreateContext
        {
            DebugBit = 0x0001,
            ForwardCompatibleBit = 0x0002,
            MajorVersion = 0x2091,
            MinorVersion = 0x2092,
            LayerPlane = 0x2093,
            Flags = 0x2094,
            ErrorInvalidVersion = 0x2095
        }


        public class Extension
        {
            public static readonly wglCreateContextAttribsARB wglCreateContextAttribsARB;
            public static readonly wglSwapIntervalEXT wglSwapIntervalEXT;
            public static readonly wglGetSwapIntervalEXT wglGetSwapIntervalEXT;
        }

        public delegate IntPtr wglCreateContextAttribsARB(IntPtr hDC, IntPtr hShareContext, int* attribList);

        public delegate Boolean wglSwapIntervalEXT(int interval);

        public delegate int wglGetSwapIntervalEXT();

        public IGlContext MakeCurrent()
        {
            if (GlContextFactory.Current != this)
            {
                if (!Wgl.wglMakeCurrent(DC, Context))
                {
                    throw (new Exception("Can't MakeCurrent"));
                }
                GlContextFactory.Current = this;
            }
            return this;
        }

        public IGlContext ReleaseCurrent()
        {
            if (GlContextFactory.Current != null)
            {
                if (!Wgl.wglMakeCurrent(DC, IntPtr.Zero))
                {
                    throw (new Exception("Can't MakeCurrent"));
                }
                GlContextFactory.Current = null;
            }
            return this;
        }

        public IGlContext SwapBuffers()
        {
            Wgl.wglSwapBuffers(DC);
            return this;
        }

        public void Dispose()
        {
            Wgl.wglDeleteContext(Context);
            Context = IntPtr.Zero;
            //throw new NotImplementedException();
        }
    }
}