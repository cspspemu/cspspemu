using CSharpPlatform.Library;
using CSPspEmu.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform.GL
{
    class DynamicLibraryGL : IDynamicLibrary
    {
        public IntPtr GetMethod(string Name)
        {
            IntPtr Return;
            switch (Platform.OS)
            {
                case OS.Windows:
                    Return = wglGetProcAddress(Name);
                    break;
                default:
                    Return = glxGetProcAddress(Name);
                    break;
            }
            if (Return == IntPtr.Zero)
            {
                Console.WriteLine("Can't find '{0}' : {1:X8}", Name, Marshal.GetLastWin32Error());
            }
            return Return;
        }

        public void Dispose()
        {
        }

        [DllImport(GL.DllWindows, EntryPoint = "wglGetProcAddress", ExactSpelling = true)]
        private static extern IntPtr wglGetProcAddress(String lpszProc);

        [DllImport(GL.DllLinux, EntryPoint = "glXGetProcAddress")]
        private static extern IntPtr glxGetProcAddress([MarshalAs(UnmanagedType.LPTStr)] string procName);
    }
}