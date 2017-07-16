using System;
using System.Runtime.InteropServices;
using System.Security;

namespace CSharpPlatform.GL.Impl
{
    public class WGL
    {
        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr wglCreateContext(IntPtr hDc);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern Boolean wglDeleteContext(IntPtr oldContext);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr wglGetCurrentContext();

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern Boolean wglMakeCurrent(IntPtr hDc, IntPtr newContext);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern Boolean wglCopyContext(IntPtr hglrcSrc, IntPtr hglrcDst, UInt32 mask);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern unsafe int wglChoosePixelFormat(IntPtr hDc, PixelFormatDescriptor* pPfd);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern unsafe int wglDescribePixelFormat(IntPtr hdc, int ipfd, UInt32 cjpfd,
            PixelFormatDescriptor* ppfd);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr wglGetCurrentDC();

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr wglGetDefaultProcAddress(String lpszProc);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr wglGetProcAddress(String lpszProc);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern int wglGetPixelFormat(IntPtr hdc);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern unsafe Boolean wglSetPixelFormat(IntPtr hdc, int ipfd, PixelFormatDescriptor* ppfd);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern Boolean wglSwapBuffers(IntPtr hdc);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern Boolean wglShareLists(IntPtr hrcSrvShare, IntPtr hrcSrvSource);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr wglCreateLayerContext(IntPtr hDc, int level);

        //[SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true)] public extern static unsafe Boolean wglDescribeLayerPlane(IntPtr hDc, int pixelFormat, int layerPlane, UInt32 nBytes, LayerPlaneDescriptor* plpd);
        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true)]
        public static extern unsafe int wglSetLayerPaletteEntries(IntPtr hdc, int iLayerPlane, int iStart, int cEntries,
            Int32* pcr);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true)]
        public static extern unsafe int wglGetLayerPaletteEntries(IntPtr hdc, int iLayerPlane, int iStart, int cEntries,
            Int32* pcr);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true)]
        public static extern Boolean wglRealizeLayerPalette(IntPtr hdc, int iLayerPlane, Boolean bRealize);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true)]
        public static extern Boolean wglSwapLayerBuffers(IntPtr hdc, UInt32 fuFlags);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, CharSet = CharSet.Auto)]
        public static extern Boolean wglUseFontBitmapsA(IntPtr hDC, Int32 first, Int32 count, Int32 listBase);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, CharSet = CharSet.Auto)]
        public static extern Boolean wglUseFontBitmapsW(IntPtr hDC, Int32 first, Int32 count, Int32 listBase);

        //[SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, CharSet = CharSet.Auto)] public extern static unsafe Boolean wglUseFontOutlinesA(IntPtr hDC, Int32 first, Int32 count, Int32 listBase, float thickness, float deviation, Int32 fontMode, GlyphMetricsFloat* glyphMetrics);
        //[SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, CharSet = CharSet.Auto)] public extern static unsafe Boolean wglUseFontOutlinesW(IntPtr hDC, Int32 first, Int32 count, Int32 listBase, float thickness, float deviation, Int32 fontMode, GlyphMetricsFloat* glyphMetrics);
        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern Boolean wglMakeContextCurrentEXT(IntPtr hDrawDC, IntPtr hReadDC, IntPtr hglrc);

        [SuppressUnmanagedCodeSecurity, DllImport(GL.DllWindows, ExactSpelling = true, SetLastError = true)]
        public static extern unsafe Boolean wglChoosePixelFormatEXT(IntPtr hdc, int* piAttribIList,
            Single* pfAttribFList, UInt32 nMaxFormats, [Out] int* piFormats, [Out] UInt32* nNumFormats);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PixelFormatDescriptor
    {
        public short Size;
        public short Version;
        public PixelFormatDescriptorFlags Flags;
        public PixelType PixelType;
        public byte ColorBits;
        public byte RedBits;
        public byte RedShift;
        public byte GreenBits;
        public byte GreenShift;
        public byte BlueBits;
        public byte BlueShift;
        public byte AlphaBits;
        public byte AlphaShift;
        public byte AccumBits;
        public byte AccumRedBits;
        public byte AccumGreenBits;
        public byte AccumBlueBits;
        public byte AccumAlphaBits;
        public byte DepthBits;
        public byte StencilBits;
        public byte AuxBuffers;
        public byte LayerType;
        private byte Reserved;
        public int LayerMask;
        public int VisibleMask;
        public int DamageMask;
    }

    public enum PixelType : byte
    {
        RGBA = 0,
        INDEXED = 1
    }

    [Flags]
    public enum PixelFormatDescriptorFlags
    {
        // PixelFormatDescriptor flags
        DOUBLEBUFFER = 0x01,
        STEREO = 0x02,
        DRAW_TO_WINDOW = 0x04,
        DRAW_TO_BITMAP = 0x08,
        SUPPORT_GDI = 0x10,
        SUPPORT_OPENGL = 0x20,
        GENERIC_FORMAT = 0x40,
        NEED_PALETTE = 0x80,
        NEED_SYSTEM_PALETTE = 0x100,
        SWAP_EXCHANGE = 0x200,
        SWAP_COPY = 0x400,
        SWAP_LAYER_BUFFERS = 0x800,
        GENERIC_ACCELERATED = 0x1000,
        SUPPORT_DIRECTDRAW = 0x2000,
        SUPPORT_COMPOSITION = 0x8000,

        // PixelFormatDescriptor flags for use in ChoosePixelFormat only
        DEPTH_DONTCARE = 0x20000000,
        DOUBLEBUFFER_DONTCARE = 0x40000000,
        STEREO_DONTCARE = unchecked((int) 0x80000000)
    }

    //[DebuggerDisplay("{Width}x{Height}")]
    public struct GLContextSize
    {
        public int Width;
        public int Height;

        public override string ToString()
        {
            return String.Format("GLContextSize({0}x{1})", Width, Height);
        }
    }

    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    public unsafe struct BITMAP
    {
        public uint bmType;
        public uint bmWidth;
        public uint bmHeight;
        public uint bmWidthBytes;
        public uint bmPlanes;
        public uint bmBitsPixel;
        public void* bmBits;
    }

    [Flags]
    public enum WindowStyle : uint
    {
        Overlapped = 0x00000000,
        Popup = 0x80000000,
        Child = 0x40000000,
        Minimize = 0x20000000,
        Visible = 0x10000000,
        Disabled = 0x08000000,
        ClipSiblings = 0x04000000,
        ClipChildren = 0x02000000,
        Maximize = 0x01000000,
        Caption = 0x00C00000, // Border | DialogFrame
        Border = 0x00800000,
        DialogFrame = 0x00400000,
        VScroll = 0x00200000,
        HScreen = 0x00100000,
        SystemMenu = 0x00080000,
        ThickFrame = 0x00040000,
        Group = 0x00020000,
        TabStop = 0x00010000,

        MinimizeBox = 0x00020000,
        MaximizeBox = 0x00010000,

        Tiled = Overlapped,
        Iconic = Minimize,
        SizeBox = ThickFrame,
        TiledWindow = OverlappedWindow,

        // Common window styles:
        OverlappedWindow = Overlapped | Caption | SystemMenu | ThickFrame | MinimizeBox | MaximizeBox,
        PopupWindow = Popup | Border | SystemMenu,
        ChildWindow = Child
    }

    [Flags]
    public enum ExtendedWindowStyle : uint
    {
        DialogModalFrame = 0x00000001,
        NoParentNotify = 0x00000004,
        Topmost = 0x00000008,
        AcceptFiles = 0x00000010,
        Transparent = 0x00000020,

        // #if(WINVER >= 0x0400)
        MdiChild = 0x00000040,
        ToolWindow = 0x00000080,
        WindowEdge = 0x00000100,
        ClientEdge = 0x00000200,
        ContextHelp = 0x00000400,
        // #endif

        // #if(WINVER >= 0x0400)
        Right = 0x00001000,
        Left = 0x00000000,
        RightToLeftReading = 0x00002000,
        LeftToRightReading = 0x00000000,
        LeftScrollbar = 0x00004000,
        RightScrollbar = 0x00000000,

        ControlParent = 0x00010000,
        StaticEdge = 0x00020000,
        ApplicationWindow = 0x00040000,

        OverlappedWindow = WindowEdge | ClientEdge,
        PaletteWindow = WindowEdge | ToolWindow | Topmost,
        // #endif

        // #if(_WIN32_WINNT >= 0x0500)
        Layered = 0x00080000,
        // #endif

        // #if(WINVER >= 0x0500)
        NoInheritLayout = 0x00100000, // Disable inheritence of mirroring by children
        RightToLeftLayout = 0x00400000, // Right to left mirroring
        // #endif /* WINVER >= 0x0500 */

        // #if(_WIN32_WINNT >= 0x0501)
        Composited = 0x02000000,
        // #endif /* _WIN32_WINNT >= 0x0501 */

        // #if(_WIN32_WINNT >= 0x0500)
        NoActivate = 0x08000000

        // #endif /* _WIN32_WINNT >= 0x0500 */
    }

    public enum WindowMessage : uint
    {
    }

    public enum CursorName
    {
        Arrow = 32512
    }

    [Flags]
    public enum ClassStyle
    {
        //None            = 0x0000,
        VRedraw = 0x0001,
        HRedraw = 0x0002,
        DoubleClicks = 0x0008,
        OwnDC = 0x0020,
        ClassDC = 0x0040,
        ParentDC = 0x0080,
        NoClose = 0x0200,
        SaveBits = 0x0800,
        ByteAlignClient = 0x1000,
        ByteAlignWindow = 0x2000,
        GlobalClass = 0x4000,

        Ime = 0x00010000,

        // #if(_WIN32_WINNT >= 0x0501)
        DropShadow = 0x00020000

        // #endif /* _WIN32_WINNT >= 0x0501 */
    }

    public delegate IntPtr WindowProcedure(IntPtr handle, WindowMessage message, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct ExtendedWindowClass
    {
        public uint Size;

        public ClassStyle Style;

        //public WNDPROC WndProc;
        [MarshalAs(UnmanagedType.FunctionPtr)] public WindowProcedure WndProc;

        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr Instance;
        public IntPtr Icon;
        public IntPtr Cursor;
        public IntPtr Background;
        public IntPtr MenuName;
        public IntPtr ClassName;
        public IntPtr IconSm;

        public static uint SizeInBytes = (uint) Marshal.SizeOf(default(ExtendedWindowClass));
    }

    public enum WindowClass : uint
    {
        Alert = 1, /* "I need your attention now."*/

        MovableAlert =
            2, /* "I need your attention now, but I'm kind enough to let you switch out of this app to do other things."*/
        Modal = 3, /* system modal, not draggable*/
        MovableModal = 4, /* application modal, draggable*/
        Floating = 5, /* floats above all other application windows*/
        Document = 6, /* document windows*/
        Desktop = 7, /* desktop window (usually only one of these exists) - OS X only in CarbonLib 1.0*/
        Utility = 8, /* Available in CarbonLib 1.1 and later, and in Mac OS X*/
        Help = 10, /* Available in CarbonLib 1.1 and later, and in Mac OS X*/
        Sheet = 11, /* Available in CarbonLib 1.3 and later, and in Mac OS X*/
        Toolbar = 12, /* Available in CarbonLib 1.1 and later, and in Mac OS X*/
        Plain = 13, /* Available in CarbonLib 1.2.5 and later, and Mac OS X*/
        Overlay = 14, /* Available in Mac OS X*/
        SheetAlert = 15, /* Available in CarbonLib 1.3 and later, and in Mac OS X 10.1 and later*/
        AltPlain = 16, /* Available in CarbonLib 1.3 and later, and in Mac OS X 10.1 and later*/
        Drawer = 20, /* Available in Mac OS X 10.2 or later*/
        All = 0xFFFFFFFFu /* for use with GetFrontWindowOfClass, FindWindowOfClass, GetNextWindowOfClass*/
    }
}