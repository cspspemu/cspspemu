using CSPspEmu.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlatform
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    unsafe public delegate uint ByteSwap(uint Value);

    sealed unsafe public class Intrinsics
    {
        static public readonly ByteSwap ByteSwap;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static ushort Portable_ByteSwap_ushort(ushort Value)
        {
            return (ushort) ((Value >> 8) | (Value << 8));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static uint Portable_ByteSwap_uint(uint Value)
        {
            return (
                ((uint) Portable_ByteSwap_ushort((ushort) (Value >> 0)) << 16) |
                ((uint) Portable_ByteSwap_ushort((ushort) (Value >> 16)) << 0)
            );
        }

        [DllImport("Kernel32.dll")]
        private static extern IntPtr HeapCreate(uint flOptions, int dwInitialSize, int dwMaximumSize);

        [DllImport("Kernel32.dll")]
        private static extern void* HeapAlloc(IntPtr hHeap, uint dwFlags, int dwBytes);

        static private void SetMethodBody<T>(ref T Func, byte[] FuncBody)
        {
            fixed (byte* FuncBodyPtr = FuncBody)
            {
                var Heap = HeapCreate(0x00040000, FuncBody.Length, FuncBody.Length);
                var Ptr = (byte*) HeapAlloc(Heap, 0x00000008, FuncBody.Length);
                Marshal.Copy(FuncBody, 0, new IntPtr(Ptr), FuncBody.Length);
                Func = (T) (object) Marshal.GetDelegateForFunctionPointer(new IntPtr(Ptr), typeof(T));
            }
        }

        static private void SetMethodBody<T>(ref T Func, T Func2)
        {
            Func = Func2;
        }

        static Intrinsics()
        {
            SetMethodBody(ref ByteSwap, Portable_ByteSwap_uint);
            //switch (Platform.Architecture)
            //{
            //	case Architecture.x64:
            //		SetMethodBody(ref ByteSwap, new byte[] { 0x0F, 0xC9, 0x8B, 0xC1, 0xC3 });
            //		break;
            //}
        }
    }
}