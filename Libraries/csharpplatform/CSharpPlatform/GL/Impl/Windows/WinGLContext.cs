using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using CSharpPlatform.Library;
using CSPspEmu.Core;

namespace CSharpPlatform.GL.Impl.Windows
{
    public unsafe class WinGlContext : IGlContext
    {
        IntPtr _dc;
        IntPtr _context;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseDC(IntPtr hwnd, IntPtr dc);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern ushort RegisterClassEx(ref ExtendedWindowClass windowClass);

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
            ExtendedWindowStyle exStyle,
            IntPtr classAtom,
            IntPtr windowName,
            WindowStyle style,
            int x, int y,
            int width, int height,
            IntPtr handleToParentWindow,
            IntPtr menu,
            IntPtr instance,
            IntPtr param
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

        private static bool _classRegistered;

        static readonly IntPtr Instance = Marshal.GetHINSTANCE(typeof(WinGlContext).Module);
        static readonly IntPtr ClassName = Marshal.StringToHGlobalAuto(Guid.NewGuid().ToString());

        const ExtendedWindowStyle ParentStyleEx = ExtendedWindowStyle.WindowEdge | ExtendedWindowStyle.ApplicationWindow
            ;

        private static void RegisterClassOnce()
        {
            if (!_classRegistered)
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
                    throw new Exception(string.Format("Failed to register window class. Error: {0}",
                        Marshal.GetLastWin32Error()));

                _classRegistered = true;
            }
        }

        static IntPtr WindowProcedure(IntPtr handle, WindowMessage message, IntPtr wParam, IntPtr lParam)
        {
            return DefWindowProc(handle, message, wParam, lParam);
        }

        [SuppressUnmanagedCodeSecurity, DllImport("GDI32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int ChoosePixelFormat(IntPtr hDc, PixelFormatDescriptor* pPfd);

        [SuppressUnmanagedCodeSecurity, DllImport("GDI32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool SetPixelFormat(IntPtr hdc, int ipfd, PixelFormatDescriptor* ppfd);

        static IntPtr _sharedContext;

        public static WinGlContext FromWindowHandle(IntPtr windowHandle)
        {
            return new WinGlContext(windowHandle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool AdjustWindowRectEx(ref Rect lpRect, int dwStyle, bool bMenu, int dwExStyle);

        IntPtr _hWnd;

        private WinGlContext(IntPtr winHandle)
        {
            _hWnd = winHandle;
            RegisterClassOnce();
            var width = 512;
            var height = 512;

            if (winHandle == IntPtr.Zero)
            {
                var style = WindowStyle.OverlappedWindow | WindowStyle.ClipChildren | WindowStyle.ClipSiblings;
                var exStyle = ParentStyleEx;

                var rect = new Rect
                {
                    Left = 0,
                    Top = 0,
                    Right = width,
                    Bottom = height
                };
                AdjustWindowRectEx(ref rect, style, false, exStyle);

                var windowName = Marshal.StringToHGlobalAuto("Title");
                _hWnd = CreateWindowEx(
                    exStyle, ClassName, windowName, style,
                    rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top,
                    IntPtr.Zero, IntPtr.Zero, Instance, IntPtr.Zero
                );

                if (_hWnd == IntPtr.Zero)
                    throw new Exception($"Failed to create window. Error: {Marshal.GetLastWin32Error()}");
            }

            _dc = GetDC(_hWnd);

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

            var pf = ChoosePixelFormat(_dc, &pfd);
            //pf = 10;

            if (!SetPixelFormat(_dc, pf, &pfd))
            {
                Console.WriteLine("Error SetPixelFormat failed.");
            }

            _context = Wgl.wglCreateContext(_dc);
            if (_sharedContext != IntPtr.Zero)
            {
                RetryShareLists:
                //Console.WriteLine("SharedContext!"); Console.ReadKey();
                if (!Wgl.wglShareLists(_sharedContext, _context))
                {
                    var lastError = Platform.InternalWindows.GetLastError();
                    Console.WriteLine($"Can't share lists {lastError}");
                    if (lastError == 170) // BUSY
                    {
                        Console.WriteLine($"-- Due to Busy. RETRY.");
                        Thread.Sleep(10);
                        goto RetryShareLists;
                    }
                    Debugger.Break();
                    throw new InvalidOperationException($"Can't share lists {lastError}");
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

            if (_sharedContext == IntPtr.Zero)
            {
                _sharedContext = _context;
            }

            if (Extension.WglSwapIntervalExt != null)
            {
                Extension.WglSwapIntervalExt(0);
            }

            //RECT clientRect;
            //GetClientRect(hWnd, &clientRect);
        }

        public GlContextSize Size
        {
            get
            {
                var bitmapHeader = default(Bitmap);
                var hBitmap = GetCurrentObject(_dc, 7);
                GetObject(hBitmap, sizeof(Bitmap), &bitmapHeader);
                return new GlContextSize {Width = (int) bitmapHeader.BmWidth, Height = (int) bitmapHeader.BmHeight};
            }
        }

        public override string ToString() => $"WinOpenglContext({_dc}, {_context}, {_sharedContext}, {Size})";

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
            public static WglCreateContextAttribsArb WglCreateContextAttribsArb;
            public static WglSwapIntervalExt WglSwapIntervalExt;
            public static WglGetSwapIntervalExt WglGetSwapIntervalExt;
        }

        public delegate IntPtr WglCreateContextAttribsArb(IntPtr hDc, IntPtr hShareContext, int* attribList);

        public delegate bool WglSwapIntervalExt(int interval);

        public delegate int WglGetSwapIntervalExt();

        public IGlContext MakeCurrent()
        {
            if (GlContextFactory.Current != this)
            {
                if (!Wgl.wglMakeCurrent(_dc, _context))
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
                if (!Wgl.wglMakeCurrent(_dc, IntPtr.Zero))
                {
                    throw (new Exception("Can't MakeCurrent"));
                }
                GlContextFactory.Current = null;
            }
            return this;
        }

        public IGlContext SwapBuffers()
        {
            Wgl.wglSwapBuffers(_dc);
            return this;
        }

        public void Dispose()
        {
            Wgl.wglDeleteContext(_context);
            _context = IntPtr.Zero;
            //throw new NotImplementedException();
        }
    }
}