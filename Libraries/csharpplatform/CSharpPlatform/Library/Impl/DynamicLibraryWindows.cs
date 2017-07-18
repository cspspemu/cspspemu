using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.Library
{
    public class DynamicLibraryWindows : IDynamicLibrary
    {
        IntPtr LibraryHandle;
        string LibraryName;

        public DynamicLibraryWindows(string LibraryName)
        {
            this.LibraryName = LibraryName;
            this.LibraryHandle = LoadLibrary(LibraryName);
            if (this.LibraryHandle == IntPtr.Zero)
            {
                throw (new InvalidOperationException(string.Format("Can't find library '{0}'", LibraryName)));
            }
        }

        public IntPtr GetMethod(string Name)
        {
            return GetProcAddress(this.LibraryHandle, Name);
        }

        public void Dispose()
        {
            //if (this.hModule != IntPtr.Zero)
            //{
            //	FreeLibrary(this.hModule);
            //	this.hModule = IntPtr.Zero;
            //}
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);
    }
}