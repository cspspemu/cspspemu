using CSPspEmu.Core;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using CSharpPlatform.Library.Impl;

namespace CSharpPlatform.Library
{
    public static class DynamicLibraryFactory
    {
        public static IDynamicLibrary CreateForLibrary(string nameWindows, string nameLinux = null,
            string nameMac = null, string nameAndroid = null)
        {
            if (nameLinux == null) nameLinux = nameWindows;
            if (nameMac == null) nameMac = nameLinux;
            if (nameAndroid == null) nameAndroid = nameLinux;

            var name = Platform.OS switch {
                OS.Windows => nameWindows,
                OS.Mac => nameMac,
                OS.Android => nameAndroid,
                OS.IOS => nameWindows,
                OS.Linux => nameLinux,
                _ => nameLinux
                };


            return Platform.OS switch
                {
                OS.Windows => (IDynamicLibrary)new DynamicLibraryWindows(name),
                OS.Mac => (IDynamicLibrary)new DynamicLibraryMac(name),
                _ => (IDynamicLibrary)new DynamicLibraryPosix(name)
                };
        }

        public static void MapLibraryToType<TType>(IDynamicLibrary dynamicLibrary)
        {
            var type = typeof(TType);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.FieldType.IsSubclassOf(typeof(Delegate))) continue;
                if (field.GetValue(null) != null) continue;
                var method = dynamicLibrary.GetMethod(field.Name);
                if (method != IntPtr.Zero)
                {
                    field.SetValue(
                        null,
                        Marshal.GetDelegateForFunctionPointer(method, field.FieldType)
                    );
                }
                else
                {
                    //Console.WriteLine(Field.Name);
                }
            }
        }
    }
}