using System;
using CSharpPlatform.GL.Impl;

namespace CSharpPlatform.GL
{
    public interface IGlContext : IDisposable
    {
        GLContextSize Size { get; }
        IGlContext MakeCurrent();
        IGlContext ReleaseCurrent();
        IGlContext SwapBuffers();
    }
}