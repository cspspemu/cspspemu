using System;
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
            System.Diagnostics.Debugger.Launch();
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
    }
}
