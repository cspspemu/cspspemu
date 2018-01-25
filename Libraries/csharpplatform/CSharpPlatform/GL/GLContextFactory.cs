using System;
using CSharpPlatform.GL.Impl.Android;
using CSharpPlatform.GL.Impl.Linux;
using CSharpPlatform.GL.Impl.Mac;
using CSharpPlatform.GL.Impl.Windows;
using CSPspEmu.Core;

namespace CSharpPlatform.GL
{
    public class GlContextFactory
    {
        [ThreadStatic] public static IGlContext Current;

        public static IGlContext CreateWindowless()
        {
            return CreateFromWindowHandle(IntPtr.Zero);
        }

        public static IGlContext CreateFromWindowHandle(IntPtr windowHandle)
        {
            switch (Platform.OS)
            {
                case OS.Windows: return WinGlContext.FromWindowHandle(windowHandle);
                case OS.Mac: return MacGLContext.FromWindowHandle(windowHandle);
                case OS.Linux: return LinuxGlContext.FromWindowHandle(windowHandle);
                case OS.Android: return AndroidGLContext.FromWindowHandle(windowHandle);
                default: throw (new NotImplementedException($"Not implemented OS: {Platform.OS}"));
            }
        }
    }
}