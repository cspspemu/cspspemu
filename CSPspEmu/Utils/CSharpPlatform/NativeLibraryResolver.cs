using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CSPspEmu.Utils.CSharpPlatform
{
    public class NativeLibraryResolver
    {
        /*
        static NativeLibraryResolver()
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetCallingAssembly(), customDllImportResolver);
        }

        private static IntPtr customDllImportResolver(
            string libraryName,
            Assembly assembly,
            DllImportSearchPath? searchPath)
        {
            Console.WriteLine($"customDllImportResolver({libraryName}, {assembly}, {searchPath})");
            return IntPtr.Zero;
        }
        */
    }
}