using CSharpPlatform.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.GL.Impl
{
    unsafe public class WinGLContext : IGLContext
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
        public extern static IntPtr DefWindowProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

        [DllImport("user32.dll", EntryPoint = "AdjustWindowRectEx", CallingConvention = CallingConvention.StdCall,
             SetLastError = true), SuppressUnmanagedCodeSecurity]
        internal static extern bool AdjustWindowRectEx(ref RECT lpRect, WindowStyle dwStyle, bool bMenu,
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

        const ClassStyle DefaultClassStyle = ClassStyle.OwnDC;

        private static bool class_registered = false;

        static readonly IntPtr Instance = Marshal.GetHINSTANCE(typeof(WinGLContext).Module);
        static readonly IntPtr ClassName = Marshal.StringToHGlobalAuto(Guid.NewGuid().ToString());

        const ExtendedWindowStyle ParentStyleEx = ExtendedWindowStyle.WindowEdge | ExtendedWindowStyle.ApplicationWindow
            ;

        static private void RegisterClassOnce()
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
        public extern static unsafe int ChoosePixelFormat(IntPtr hDc, PixelFormatDescriptor* pPfd);

        [SuppressUnmanagedCodeSecurity, DllImport("GDI32.dll", ExactSpelling = true, SetLastError = true)]
        public extern static unsafe Boolean SetPixelFormat(IntPtr hdc, int ipfd, PixelFormatDescriptor* ppfd);

        static IntPtr SharedContext;

        static public WinGLContext FromWindowHandle(IntPtr WindowHandle)
        {
            return new WinGLContext(WindowHandle);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

        IntPtr hWnd;

        private WinGLContext(IntPtr WinHandle)
        {
            this.hWnd = WinHandle;
            RegisterClassOnce();
            int Width = 512;
            int Height = 512;

            if (WinHandle == IntPtr.Zero)
            {
                WindowStyle style = WindowStyle.OverlappedWindow | WindowStyle.ClipChildren | WindowStyle.ClipSiblings;
                ExtendedWindowStyle ex_style = ParentStyleEx;

                var rect = new RECT()
                {
                    left = 0,
                    top = 0,
                    right = Width,
                    bottom = Height,
                };
                AdjustWindowRectEx(ref rect, style, false, ex_style);

                IntPtr window_name = Marshal.StringToHGlobalAuto("Title");
                hWnd = CreateWindowEx(
                    ex_style, ClassName, window_name, style,
                    rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top,
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
            pfd.Flags = PixelFormatDescriptorFlags.DRAW_TO_WINDOW | PixelFormatDescriptorFlags.SUPPORT_OPENGL |
                        PixelFormatDescriptorFlags.DOUBLEBUFFER;
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

            this.Context = WGL.wglCreateContext(DC);
            if (SharedContext != IntPtr.Zero)
            {
                //Console.WriteLine("SharedContext!"); Console.ReadKey();
                if (!WGL.wglShareLists(SharedContext, this.Context))
                {
                    throw(new InvalidOperationException("Can't share lists"));
                }
            }
            MakeCurrent();
            Console.Out.WriteLineColored(ConsoleColor.Yellow, "Version:{0}.{1}", GL.glGetInteger(GL.GL_MAJOR_VERSION),
                GL.glGetInteger(GL.GL_MINOR_VERSION));
            DynamicLibraryFactory.MapLibraryToType<Extension>(new DynamicLibraryGL());
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
                SharedContext = this.Context;
            }

            if (Extension.wglSwapIntervalEXT != null)
            {
                Extension.wglSwapIntervalEXT(0);
            }

            //RECT clientRect;
            //GetClientRect(hWnd, &clientRect);
        }

        public GLContextSize Size
        {
            get
            {
                var bitmapHeader = default(BITMAP);
                var hBitmap = GetCurrentObject(DC, 7);
                GetObject(hBitmap, sizeof(BITMAP), &bitmapHeader);
                return new GLContextSize() {Width = (int) bitmapHeader.bmWidth, Height = (int) bitmapHeader.bmHeight};
            }
        }

        public override string ToString()
        {
            return String.Format("WinOpenglContext({0}, {1}, {2}, {3})", this.DC, this.Context, SharedContext, Size);
        }

        public enum ArbCreateContext : int
        {
            DebugBit = 0x0001,
            ForwardCompatibleBit = 0x0002,
            MajorVersion = 0x2091,
            MinorVersion = 0x2092,
            LayerPlane = 0x2093,
            Flags = 0x2094,
            ErrorInvalidVersion = 0x2095,
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

        public IGLContext MakeCurrent()
        {
            if (GLContextFactory.Current != this)
            {
                if (!WGL.wglMakeCurrent(DC, Context))
                {
                    throw (new Exception("Can't MakeCurrent"));
                }
                GLContextFactory.Current = this;
            }
            return this;
        }

        public IGLContext ReleaseCurrent()
        {
            if (GLContextFactory.Current != null)
            {
                if (!WGL.wglMakeCurrent(DC, IntPtr.Zero))
                {
                    throw (new Exception("Can't MakeCurrent"));
                }
                GLContextFactory.Current = null;
            }
            return this;
        }

        public IGLContext SwapBuffers()
        {
            WGL.wglSwapBuffers(DC);
            return this;
        }

        public void Dispose()
        {
            WGL.wglDeleteContext(this.Context);
            this.Context = IntPtr.Zero;
            //throw new NotImplementedException();
        }
    }
}