using System;
using System.Runtime.InteropServices;
using CSharpPlatform.GL.Impl.Windows;

namespace CSharpPlatform.GL.Impl.Mac
{
    public class MacGLContext : IGlContext
    {
        public MacGLContext(IntPtr windowHandle)
        {
        }

        public static MacGLContext FromWindowHandle(IntPtr windowHandle)
        {
            //System.Diagnostics.Debugger.Launch();
            return new MacGLContext(windowHandle);
        }

        public GlContextSize Size => throw new NotImplementedException();

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public IGlContext MakeCurrent()
        {
            //throw new NotImplementedException();
            return this;
        }

        public IGlContext ReleaseCurrent()
        {
            //throw new NotImplementedException();
            return this;
        }

        public IGlContext SwapBuffers()
        {
            //throw new NotImplementedException();
            return this;
        }


        //private static readonly IntPtr NSOpenGLContext = Class.Get("NSOpenGLContext");
        //private static readonly IntPtr selCurrentContext = Selector.Get("currentContext");
        //private static readonly IntPtr selFlushBuffer = Selector.Get("flushBuffer");
        //private static readonly IntPtr selMakeCurrentContext = Selector.Get("makeCurrentContext");
        //private static readonly IntPtr selUpdate = Selector.Get("update");
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

        [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
        public extern static IntPtr Get(string name);
    }

    internal static class Class
    {
        internal const string LibObjC = "/usr/lib/libobjc.dylib";

        public static readonly IntPtr NSAutoreleasePool = Get("NSAutoreleasePool");
        public static readonly IntPtr NSDictionary = Get("NSDictionary");
        public static readonly IntPtr NSNumber = Get("NSNumber");
        public static readonly IntPtr NSUserDefaults = Get("NSUserDefaults");

        [DllImport(LibObjC)]
        private extern static IntPtr class_getName(IntPtr handle);

        [DllImport(LibObjC)]
        private extern static bool class_addMethod(IntPtr classHandle, IntPtr selector, IntPtr method, string types);

        [DllImport(LibObjC)]
        private extern static IntPtr objc_getClass(string name);

        [DllImport(LibObjC)]
        private extern static IntPtr objc_allocateClassPair(IntPtr parentClass, string name, int extraBytes);

        [DllImport(LibObjC)]
        private extern static void objc_registerClassPair(IntPtr classToRegister);

        [DllImport(LibObjC)]
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
}
