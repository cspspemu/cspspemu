using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.GL.Impl
{
    public interface IGLContext : IDisposable
    {
        GLContextSize Size { get; }
        IGLContext MakeCurrent();
        IGLContext ReleaseCurrent();
        IGLContext SwapBuffers();
    }
}