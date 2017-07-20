using System;

namespace CSharpPlatform.Library
{
    public interface IDynamicLibrary : IDisposable
    {
        IntPtr GetMethod(string Name);
    }
}