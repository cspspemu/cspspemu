using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Memory
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public unsafe struct PspPointer<TType>
    {
        public uint Address;

        public uint Low24
        {
            set => Address = (Address & 0xFF000000) | (value & 0x00FFFFFF);
        }

        public uint High8
        {
            set => Address = (Address & 0x00FFFFFF) | (value & 0xFF000000);
        }

        public PspPointer(uint Address)
        {
            this.Address = Address;
        }

        public static implicit operator uint(PspPointer<TType> that) => that.Address;
        public static implicit operator PspPointer<TType>(uint that) => new PspPointer(that);
        public override string ToString() => $"PspPointer(0x{Address:X})";
        public bool IsNull => Address == 0;
        public unsafe void* GetPointer(PspMemory pspMemory) => pspMemory.PspPointerToPointerSafe(this, Marshal.SizeOf(typeof(TType)));
        public unsafe void* GetPointerNotNull(PspMemory pspMemory)
        {
            var Pointer = this.GetPointer(pspMemory);
            if (Pointer == null)
                throw (new NullReferenceException($"Pointer for {typeof(TType)} can't be null"));
            return Pointer;
        }

        public static implicit operator PspPointer<TType>(PspPointer that) => new PspPointer<TType>(that.Address);
        public static implicit operator PspPointer(PspPointer<TType> that) => new PspPointer(that.Address);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public unsafe struct PspPointer
    {
        public uint Address;
        public uint Low24 { set => Address = (Address & 0xFF000000) | (value & 0x00FFFFFF); }
        public uint High8 { set => Address = (Address & 0x00FFFFFF) | (value & 0xFF000000); }

        public PspPointer(uint Address) => this.Address = Address;
        public static implicit operator uint(PspPointer that) => that.Address;
        public static implicit operator PspPointer(uint that) => new PspPointer(that);
        public override string ToString() => $"PspPointer(0x{Address:X})";
        public bool IsNull => Address == 0;
        public unsafe void* GetPointer(PspMemory PspMemory, int Size) => PspMemory.PspPointerToPointerSafe(this, Size);
        public unsafe void* GetPointer<TType>(PspMemory PspMemory) => PspMemory.PspPointerToPointerSafe(this, Marshal.SizeOf(typeof(TType)));
        public unsafe void* GetPointerNotNull<TType>(PspMemory PspMemory)
        {
            var Pointer = this.GetPointer<TType>(PspMemory);
            if (Pointer == null)
                throw(new NullReferenceException($"Pointer for {typeof(TType)} can't be null"));
            return Pointer;
        }
    }
}