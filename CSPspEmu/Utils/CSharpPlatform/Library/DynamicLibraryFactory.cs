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

            string name;
            switch (Platform.OS) {
                case OS.Windows:
                    name = nameWindows;
                    break;
                case OS.Mac:
                    name = nameMac;
                    break;
                case OS.Android:
                    name = nameAndroid;
                    break;
                case OS.IOS:
                    name = nameWindows;
                    break;
                case OS.Linux:
                    name = nameLinux;
                    break;
                default:
                    name = nameLinux;
                    break;
            }


            switch (Platform.OS) 
            {
                case OS.Windows:
                    return (IDynamicLibrary) new DynamicLibraryWindows(name);
                case OS.Mac:
                    return (IDynamicLibrary) new DynamicLibraryMac(name);
                default: return (IDynamicLibrary) new DynamicLibraryPosix(name);
            }
        }
        
        public static void MapLibraryToType<TType>(IDynamicLibrary dynamicLibrary)
        {
            var type = typeof(TType);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                if (!field.FieldType.IsSubclassOf(typeof(Delegate))) continue;
                if (field.GetValue(null) != null) continue;
                var method = dynamicLibrary.GetMethod(field.Name);
                //Console.WriteLine($"{field.Name} : {method}");
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