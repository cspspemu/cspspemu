using CSharpPlatform.GL.Impl;
using CSharpPlatform.GL.Impl.Android;
using CSharpPlatform.GL.Impl.Linux;
using CSPspEmu.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.GL
{
    public class GLContextFactory
    {
        [ThreadStatic] public static IGLContext Current;

        static public IGLContext CreateWindowless()
        {
            return CreateFromWindowHandle(IntPtr.Zero);
        }

        static public IGLContext CreateFromWindowHandle(IntPtr WindowHandle)
        {
            switch (Platform.OS)
            {
                case OS.Windows: return WinGLContext.FromWindowHandle(WindowHandle);
                case OS.Linux: return LinuxGLContext.FromWindowHandle(WindowHandle);
                case OS.Android: return AndroidGLContext.FromWindowHandle(WindowHandle);
                default: throw (new NotImplementedException(String.Format("Not implemented OS: {0}", Platform.OS)));
            }
        }
    }
}