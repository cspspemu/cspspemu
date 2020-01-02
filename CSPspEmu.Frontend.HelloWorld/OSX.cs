using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace CSPspEmu.Frontend.HelloWorld
{
    /*
    internal static class Cocoa
    {
        private static readonly IntPtr selUTF8String = Selector.Get("UTF8String");

        internal const string LibObjC = "/usr/lib/libobjc.dylib";

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, ulong ulong1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, NSSize size);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr intPtr1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr intPtr1, int int1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr intPtr1, IntPtr intPtr2);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr intPtr1, IntPtr intPtr2, IntPtr intPtr3);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr intPtr1, IntPtr intPtr2, IntPtr intPtr3, IntPtr intPtr4, IntPtr intPtr5);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr p1, NSPoint p2);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, bool p1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, NSPoint p1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, NSRect rectangle1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, NSRect rectangle1, int int1, int int2, bool bool1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, uint uint1, IntPtr intPtr1, IntPtr intPtr2, bool bool1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, NSRect rectangle1, int int1, IntPtr intPtr1, IntPtr intPtr2);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static IntPtr SendIntPtr(IntPtr receiver, IntPtr selector, IntPtr p1, int p2, int p3, int p4, int p5, bool p6, bool p7, IntPtr p8, NSBitmapFormat p9, int p10, int p11);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static bool SendBool(IntPtr receiver, IntPtr selector);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static bool SendBool(IntPtr receiver, IntPtr selector, int int1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, uint uint1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, uint uint1, IntPtr intPtr1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, IntPtr intPtr1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, IntPtr intPtr1, int int1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, IntPtr intPtr1, IntPtr intPtr2);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, int int1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, bool bool1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, NSPoint point1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, NSRect rect1, bool bool1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static void SendVoid(IntPtr receiver, IntPtr selector, NSRect rect1, IntPtr intPtr1);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static int SendInt(IntPtr receiver, IntPtr selector);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static uint SendUint(IntPtr receiver, IntPtr selector);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        public extern static ushort SendUshort(IntPtr receiver, IntPtr selector);

        [DllImport(LibObjC, EntryPoint="objc_msgSend_fpret")]
        private extern static float SendFloat_i386(IntPtr receiver, IntPtr selector);

        // On x64 using selector that return CGFloat give you 64 bit == double
        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        private extern static double SendFloat_x64(IntPtr receiver, IntPtr selector);

        [DllImport(LibObjC, EntryPoint="objc_msgSend")]
        private extern static float SendFloat_ios(IntPtr receiver, IntPtr selector);

        public static float SendFloat(IntPtr receiver, IntPtr selector)
        {
            #if IOS
            return SendFloat_ios(receiver, selector);
            #else
            if (IntPtr.Size == 4)
            {
                return SendFloat_i386(receiver, selector);
            }
            else
            {
                return (float)SendFloat_x64(receiver, selector);
            }
            #endif
        }

        // Not the _stret version, perhaps because a NSPoint fits in one register?
        // thefiddler: gcc is indeed using objc_msgSend for NSPoint on i386
        [DllImport (LibObjC, EntryPoint="objc_msgSend")]
        public extern static NSPointF SendPointF(IntPtr receiver, IntPtr selector);
        [DllImport (LibObjC, EntryPoint="objc_msgSend")]
        public extern static NSPointD SendPointD(IntPtr receiver, IntPtr selector);

        public static NSPoint SendPoint(IntPtr receiver, IntPtr selector)
        {
            NSPoint r = new NSPoint();

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    NSPointF pf = SendPointF(receiver, selector);
                    r.X.Value = *(IntPtr *)&pf.X;
                    r.Y.Value = *(IntPtr *)&pf.Y;
                }
                else
                {
                    NSPointD pd = SendPointD(receiver, selector);
                    r.X.Value = *(IntPtr *)&pd.X;
                    r.Y.Value = *(IntPtr *)&pd.Y;
                }
            }

            return r;
        }

        [DllImport (LibObjC, EntryPoint="objc_msgSend_stret")]
        private extern static void SendRect(out NSRect retval, IntPtr receiver, IntPtr selector);

        [DllImport (LibObjC, EntryPoint="objc_msgSend_stret")]
        private extern static void SendRect(out NSRect retval, IntPtr receiver, IntPtr selector, NSRect rect1);

        public static NSRect SendRect(IntPtr receiver, IntPtr selector)
        {
            NSRect r;
            SendRect(out r, receiver, selector);
            return r;
        }

        public static NSRect SendRect(IntPtr receiver, IntPtr selector, NSRect rect1)
        {
            NSRect r;
            SendRect(out r, receiver, selector, rect1);
            return r;
        }

        public static IntPtr ToNSString(string str)
        {
            if (str == null)
            {
                return IntPtr.Zero;
            }

            unsafe
            {
                fixed (char* ptrFirstChar = str)
                {
                    var handle = Cocoa.SendIntPtr(Class.Get("NSString"), Selector.Alloc);
                    handle = Cocoa.SendIntPtr(handle, Selector.Get("initWithCharacters:length:"), (IntPtr)ptrFirstChar, str.Length);
                    return handle;
                }
            }
        }

        public static string FromNSString(IntPtr handle)
        {
            return Marshal.PtrToStringAuto(SendIntPtr(handle, selUTF8String));
        }

        public static unsafe IntPtr ToNSImage(Image img)
        {
            using (System.IO.MemoryStream s = new System.IO.MemoryStream())
            {
                img.Save(s, ImageFormat.Png);
                byte[] b = s.ToArray();

                fixed (byte* pBytes = b)
                {
                    IntPtr nsData = Cocoa.SendIntPtr(Cocoa.SendIntPtr(Class.Get("NSData"), Selector.Alloc),
                        Selector.Get("initWithBytes:length:"), (IntPtr)pBytes, b.Length);

                    IntPtr nsImage = Cocoa.SendIntPtr(Cocoa.SendIntPtr(Class.Get("NSImage"), Selector.Alloc),
                        Selector.Get("initWithData:"), nsData);

                    Cocoa.SendVoid(nsData, Selector.Release);
                    return nsImage;
                }
            }
        }

        public static IntPtr GetStringConstant(IntPtr handle, string symbol)
        {
            var indirect = NS.GetSymbol(handle, symbol);
            if (indirect == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var actual = Marshal.ReadIntPtr(indirect);
            if (actual == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            return actual;
        }

        public static IntPtr AppKitLibrary;
        public static IntPtr FoundationLibrary;

        public static void Initialize()
        {
            if (AppKitLibrary != IntPtr.Zero)
            {
                return;
            }

            AppKitLibrary = NS.LoadLibrary("/System/Library/Frameworks/AppKit.framework/AppKit");
            FoundationLibrary = NS.LoadLibrary("/System/Library/Frameworks/Foundation.framework/Foundation");
        }
    }
    
    internal static class Selector
    {
        // Frequently used selectors
        public static readonly IntPtr Init = Selector.Get("init");
        public static readonly IntPtr InitWithCoder = Selector.Get("initWithCoder:");
        public static readonly IntPtr Alloc = Selector.Get("alloc");
        public static readonly IntPtr Retain = Selector.Get("retain");
        public static readonly IntPtr Release = Selector.Get("release");
        public static readonly IntPtr Autorelease = Selector.Get("autorelease");

        [DllImport ("/usr/lib/libobjc.dylib", EntryPoint="sel_registerName")]
        public extern static IntPtr Get(string name);
    }
    
    internal struct NSFloat
    {
        private IntPtr _value;

        public IntPtr Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public static implicit operator NSFloat(float v)
        {
            NSFloat f = new NSFloat();
            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    f.Value = *(IntPtr*)&v;
                }
                else
                {
                    double d = v;
                    f.Value = *(IntPtr*)&d;
                }
            }
            return f;
        }

        public static implicit operator NSFloat(double v)
        {
            NSFloat f = new NSFloat();
            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    float fv = (float)v;
                    f.Value = *(IntPtr*)&fv;
                }
                else
                {
                    f.Value = *(IntPtr*)&v;
                }
            }
            return f;
        }

        public static implicit operator float(NSFloat f)
        {
            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    return *(float*)&f._value;
                }
                else
                {
                    return (float)*(double*)&f._value;
                }
            }
        }

        public static implicit operator double(NSFloat f)
        {
            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    return (double)*(float*)&f._value;
                }
                else
                {
                    return *(double*)&f._value;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NSPoint
    {
        public NSFloat X;
        public NSFloat Y;

        public static implicit operator NSPoint(PointF p)
        {
            return new NSPoint
            {
                X = p.X,
                Y = p.Y
            };
        }

        public static implicit operator PointF(NSPoint s)
        {
            return new PointF(s.X, s.Y);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NSSize
    {
        public NSFloat Width;
        public NSFloat Height;

        public static implicit operator NSSize(SizeF s)
        {
            return new NSSize
            {
                Width = s.Width,
                Height = s.Height
            };
        }

        public static implicit operator SizeF(NSSize s)
        {
            return new SizeF(s.Width, s.Height);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NSRect
    {
        public NSPoint Location;
        public NSSize Size;

        public NSFloat Width { get { return Size.Width; } }
        public NSFloat Height { get { return Size.Height; } }
        public NSFloat X { get { return Location.X; } }
        public NSFloat Y { get { return Location.Y; } }

        public static implicit operator NSRect(RectangleF s)
        {
            return new NSRect
            {
                Location = s.Location,
                Size = s.Size
            };
        }

        public static implicit operator RectangleF(NSRect s)
        {
            return new RectangleF(s.Location, s.Size);
        }
    }

    // Using IntPtr in NSFloat cause that if imported function
    // return struct that consist of them you will get wrong data
    // This types are used for such function.
    [StructLayout(LayoutKind.Sequential)]
    internal struct NSPointF
    {
        public float X;
        public float Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NSPointD
    {
        public double X;
        public double Y;
    }
    [Flags]
    internal enum NSBitmapFormat
    {
        AlphaFirst = 1 << 0,
        AlphaNonpremultiplied = 1 << 1,
        FloatingPointSamples  = 1 << 2
    }
    
    internal static class Class
    {
        public static readonly IntPtr NSAutoreleasePool = Get("NSAutoreleasePool");
        public static readonly IntPtr NSDictionary = Get("NSDictionary");
        public static readonly IntPtr NSNumber = Get("NSNumber");
        public static readonly IntPtr NSUserDefaults = Get("NSUserDefaults");

        [DllImport (Cocoa.LibObjC)]
        private extern static IntPtr class_getName(IntPtr handle);

        [DllImport (Cocoa.LibObjC)]
        private extern static bool class_addMethod(IntPtr classHandle, IntPtr selector, IntPtr method, string types);

        [DllImport (Cocoa.LibObjC)]
        private extern static IntPtr objc_getClass(string name);

        [DllImport (Cocoa.LibObjC)]
        private extern static IntPtr objc_allocateClassPair(IntPtr parentClass, string name, int extraBytes);

        [DllImport (Cocoa.LibObjC)]
        private extern static void objc_registerClassPair(IntPtr classToRegister);

        [DllImport (Cocoa.LibObjC)]
        private extern static void objc_disposeClassPair(IntPtr cls);

        public static IntPtr Get(string name)
        {
            var id = objc_getClass(name);
            if (id == IntPtr.Zero)
            {
                throw new ArgumentException("Unknown class: " + name);
            }
            return id;
        }

        public static IntPtr AllocateClass(string className, string parentClass)
        {
            return objc_allocateClassPair(Get(parentClass), className, 0);
        }

        public static void RegisterClass(IntPtr handle)
        {
            objc_registerClassPair(handle);
        }

        public static void DisposeClass(IntPtr handle)
        {
            objc_disposeClassPair(handle);
        }

        public static void RegisterMethod(IntPtr handle, Delegate d, string selector, string typeString)
        {
            // TypeString info:
            // https://developer.apple.com/library/mac/documentation/Cocoa/Conceptual/ObjCRuntimeGuide/Articles/ocrtTypeEncodings.html

            IntPtr p = Marshal.GetFunctionPointerForDelegate(d);
            bool r = class_addMethod(handle, Selector.Get(selector), p, typeString);

            if (!r)
            {
                throw new ArgumentException("Could not register method " + d + " in class + " + class_getName(handle));
            }
        }
    }
    
    [Flags]
    internal enum AddImageFlags
    {
        ReturnOnError = 1,
        WithSearching = 2,
        ReturnOnlyIfLoaded = 4
    }

    [Flags]
    internal enum SymbolLookupFlags
    {
        Bind = 0,
        BindNow = 1,
        BindFully = 2,
        ReturnOnError = 4
    }

    internal class NS
    {
        private const string Library = "libdl.dylib";

        [DllImport(Library, EntryPoint = "NSAddImage")]
        internal static extern IntPtr AddImage(string s, AddImageFlags flags);
        [DllImport(Library, EntryPoint = "NSAddressOfSymbol")]
        internal static extern IntPtr AddressOfSymbol(IntPtr symbol);
        [DllImport(Library, EntryPoint = "NSIsSymbolNameDefined")]
        internal static extern bool IsSymbolNameDefined(string s);
        [DllImport(Library, EntryPoint = "NSIsSymbolNameDefined")]
        internal static extern bool IsSymbolNameDefined(IntPtr s);
        [DllImport(Library, EntryPoint = "NSLookupAndBindSymbol")]
        internal static extern IntPtr LookupAndBindSymbol(string s);
        [DllImport(Library, EntryPoint = "NSLookupAndBindSymbol")]
        internal static extern IntPtr LookupAndBindSymbol(IntPtr s);
        [DllImport(Library, EntryPoint = "NSLookupSymbolInImage")]
        internal static extern IntPtr LookupSymbolInImage(IntPtr image, IntPtr symbolName, SymbolLookupFlags options);

        // Unfortunately, these are slower even if they are more
        // portable and simpler to use.
        [DllImport(Library)]
        internal static extern IntPtr dlopen(String fileName, int flags);
        [DllImport(Library)]
        internal static extern int dlclose(IntPtr handle);
        [DllImport (Library)]
        internal static extern IntPtr dlsym (IntPtr handle, string symbol);
        [DllImport (Library)]
        internal static extern IntPtr dlsym (IntPtr handle, IntPtr symbol);

        public static IntPtr GetAddress(string function)
        {
            // Instead of allocating and combining strings in managed memory
            // we do that directly in unmanaged memory. This way, we avoid
            // 2 string allocations every time this function is called.

            // must add a '_' prefix and null-terminate the function name,
            // hence we allocate +2 bytes
            IntPtr ptr = Marshal.AllocHGlobal(function.Length + 2);
            try
            {
                Marshal.WriteByte(ptr, (byte)'_');
                for (int i = 0; i < function.Length; i++)
                {
                    Marshal.WriteByte(ptr, i + 1, (byte)function[i]);
                }
                Marshal.WriteByte(ptr, function.Length + 1, 0); // null-terminate

                IntPtr symbol = GetAddressInternal(ptr);
                return symbol;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static IntPtr GetAddress(IntPtr function)
        {
            unsafe
            {
                const int max = 64;
                byte* symbol = stackalloc byte[max];
                byte* ptr = symbol;
                byte* cur = (byte*)function.ToPointer();
                int i = 0;

                *ptr++ = (byte)'_';
                while (*cur != 0 && ++i < max)
                {
                    *ptr++ = *cur++;
                }

                if (i >= max - 1)
                {
                    throw new NotSupportedException(String.Format(
                        "Function {0} is too long. Please report a bug at https://github.com/opentk/issues/issues",
                        Marshal.PtrToStringAnsi(function)));
                }

                return GetAddressInternal(new IntPtr(symbol));
            }
        }

        private static IntPtr GetAddressInternal(IntPtr function)
        {
            IntPtr symbol = IntPtr.Zero;
            if (IsSymbolNameDefined(function))
            {
                symbol = LookupAndBindSymbol(function);
                if (symbol != IntPtr.Zero)
                {
                    symbol = AddressOfSymbol(symbol);
                }
            }
            return symbol;
        }

        public static IntPtr GetSymbol(IntPtr handle, string symbol)
        {
            return dlsym(handle, symbol);
        }

        public static IntPtr GetSymbol(IntPtr handle, IntPtr symbol)
        {
            return dlsym(handle, symbol);
        }

        public static IntPtr LoadLibrary(string fileName)
        {
            const int RTLD_NOW = 2;
            return dlopen(fileName, RTLD_NOW);
        }

        public static void FreeLibrary(IntPtr handle)
        {
            dlclose(handle);
        }
    }
    
    internal static class NSApplication
    {
        internal static IntPtr Handle;

        private static readonly IntPtr selQuit = Selector.Get("quit");

        private static readonly int ThreadId =
            System.Threading.Thread.CurrentThread.ManagedThreadId;

        internal static void Initialize() { }

        static NSApplication()
        {
            Cocoa.Initialize();

            // Register a Quit method to be called on cmd-q
            IntPtr nsapp = Class.Get("NSApplication");
            Class.RegisterMethod(nsapp, OnQuitHandler, "quit", "v@:");

            // Fetch the application handle
            Handle = Cocoa.SendIntPtr(nsapp, Selector.Get("sharedApplication"));

            // Setup the application
            Cocoa.SendBool(Handle, Selector.Get("setActivationPolicy:"), (int)NSApplicationActivationPolicy.Regular);
            Cocoa.SendVoid(Handle, Selector.Get("discardEventsMatchingMask:beforeEvent:"), uint.MaxValue, IntPtr.Zero);
            Cocoa.SendVoid(Handle, Selector.Get("activateIgnoringOtherApps:"), true);

            if (Cocoa.SendIntPtr(Handle, Selector.Get("mainMenu")) == IntPtr.Zero)
            {
                // Create the menu bar
                var menubar = Cocoa.SendIntPtr(Class.Get("NSMenu"), Selector.Alloc);
                var menuItem = Cocoa.SendIntPtr(Class.Get("NSMenuItem"), Selector.Alloc);

                // Add menu item to bar, and bar to application
                Cocoa.SendIntPtr(menubar, Selector.Get("addItem:"), menuItem);
                Cocoa.SendIntPtr(Handle, Selector.Get("setMainMenu:"), menubar);

                // Add a "Quit" menu item and bind the button.
                var appMenu = Cocoa.SendIntPtr(Class.Get("NSMenu"), Selector.Alloc);
                var quitMenuItem = Cocoa.SendIntPtr(Cocoa.SendIntPtr(Class.Get("NSMenuItem"), Selector.Alloc),
                                   Selector.Get("initWithTitle:action:keyEquivalent:"), Cocoa.ToNSString("Quit"), selQuit, Cocoa.ToNSString("q"));

                Cocoa.SendIntPtr(appMenu, Selector.Get("addItem:"), quitMenuItem);
                Cocoa.SendIntPtr(menuItem, Selector.Get("setSubmenu:"), appMenu);

                // Tell cocoa we're ready to run the application (usually called by [NSApp run]).
                // Note: if a main menu exists, then this method has already been called and
                // calling it again will result in a crash. For this reason, we only call it
                // when we create our own main menu.
                Cocoa.SendVoid(Handle, Selector.Get("finishLaunching"));
            }

            // Disable momentum scrolling and long-press key pop-ups
            IntPtr settings = Cocoa.SendIntPtr(Class.NSDictionary, Selector.Alloc);
            //IntPtr momentum_scrolling = Cocoa.SendIntPtr(Class.NSNumber, Selector.Get("numberWithBool:"), false);
            IntPtr press_and_hold = Cocoa.SendIntPtr(Class.NSNumber, Selector.Get("numberWithBool:"), false);

            // Initialize and register the settings dictionary
            settings =
                Cocoa.SendIntPtr(settings, Selector.Get("initWithObjectsAndKeys:"),
                    //momentum_scrolling, Cocoa.ToNSString("AppleMomentumScrollSupported"),
                    press_and_hold, Cocoa.ToNSString("ApplePressAndHoldEnabled"),
                    IntPtr.Zero);
            Cocoa.SendVoid(
                Cocoa.SendIntPtr(Class.NSUserDefaults, Selector.Get("standardUserDefaults")),
                Selector.Get("registerDefaults:"),
                settings);
            Cocoa.SendVoid(settings, Selector.Release);
        }

        internal static bool IsUIThread
        {
            get
            {
                int thread_id = Thread.CurrentThread.ManagedThreadId;
                bool is_ui_thread = thread_id == NSApplication.ThreadId;
                if (!is_ui_thread)
                {
                    Debug.Print("[Warning] UI resources must be disposed in the UI thread #{0}, not #{1}.",
                        NSApplication.ThreadId, thread_id);
                }
                return is_ui_thread;
            }
        }

        internal static event EventHandler<CancelEventArgs> Quit = delegate { };

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate void OnQuitDelegate(IntPtr self, IntPtr cmd);

        private static OnQuitDelegate OnQuitHandler = OnQuit;

        private static void OnQuit(IntPtr self, IntPtr cmd)
        {
            var e = new CancelEventArgs();
            Quit(null, e);
            if (!e.Cancel)
            {
                Cocoa.SendVoid(Handle, Selector.Get("terminate:"), Handle);
            }
        }
    }
    
    internal enum NSApplicationActivationPolicy
    {
        Regular,
        Accessory,
        Prohibited,
    }
    
    [Flags]
    internal enum NSWindowStyle
    {
        Borderless = 0,
        Titled = 1,
        Closable = 2,
        Miniaturizable = 4,
        Resizable = 8,
        Utility = 16,
        DocModal = 64,
        NonactivatingPanel = 128,
        TexturedBackground = 256,
        Unscaled = 2048,
        UnifiedTitleAndToolbar = 4096,
        Hud = 8192,
        FullScreenWindow = 16384,
    }
    */
}