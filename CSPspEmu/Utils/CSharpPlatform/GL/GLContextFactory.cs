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

        public static IGlContext CreateWindowless() => CreateFromWindowHandle(IntPtr.Zero);

        public static IGlContext CreateFromWindowHandle(IntPtr windowHandle) =>
            Platform.OS switch
            {
                OS.Windows => WinGlContext.FromWindowHandle(windowHandle),
                OS.Mac => MacGLContext.FromWindowHandle(windowHandle),
                OS.Linux => LinuxGlContext.FromWindowHandle(windowHandle),
                OS.Android => AndroidGLContext.FromWindowHandle(windowHandle),
                _ => throw (new NotImplementedException($"Not implemented OS: {Platform.OS}"))
            };
    }
}