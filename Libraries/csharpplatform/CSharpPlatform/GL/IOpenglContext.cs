using System;
using CSharpPlatform.GL.Impl;
using CSharpPlatform.GL.Impl.Windows;

namespace CSharpPlatform.GL
{
    public interface IGlContext : IDisposable
    {
        GlContextSize Size { get; }
        IGlContext MakeCurrent();
        IGlContext ReleaseCurrent();
        IGlContext SwapBuffers();
    }
}