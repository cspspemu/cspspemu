using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.Library
{
    public interface IDynamicLibrary : IDisposable
    {
        IntPtr GetMethod(string Name);
    }
}