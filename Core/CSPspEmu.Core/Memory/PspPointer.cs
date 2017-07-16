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
            set { Address = (Address & 0xFF000000) | (value & 0x00FFFFFF); }
        }

        public uint High8
        {
            set { Address = (Address & 0x00FFFFFF) | (value & 0xFF000000); }
        }

        public PspPointer(uint Address)
        {
            this.Address = Address;
        }

        public static implicit operator uint(PspPointer<TType> that)
        {
            return that.Address;
        }

        public static implicit operator PspPointer<TType>(uint that)
        {
            return new PspPointer()
            {
                Address = that,
            };
        }

        public override string ToString()
        {
            return String.Format("PspPointer(0x{0:X})", Address);
        }

        public bool IsNull
        {
            get { return Address == 0; }
        }

        public unsafe void* GetPointer(PspMemory pspMemory)
        {
            return pspMemory.PspPointerToPointerSafe(this, Marshal.SizeOf(typeof(TType)));
        }

        public unsafe void* GetPointerNotNull(PspMemory pspMemory)
        {
            var Pointer = this.GetPointer(pspMemory);
            if (Pointer == null)
                throw (new NullReferenceException(String.Format("Pointer for {0} can't be null", typeof(TType))));
            return Pointer;
        }

        public static implicit operator PspPointer<TType>(PspPointer that)
        {
            return new PspPointer<TType>(that.Address);
        }

        public static implicit operator PspPointer(PspPointer<TType> that)
        {
            return new PspPointer(that.Address);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 4)]
    public unsafe struct PspPointer
    {
        public uint Address;

        public uint Low24
        {
            set { Address = (Address & 0xFF000000) | (value & 0x00FFFFFF); }
        }

        public uint High8
        {
            set { Address = (Address & 0x00FFFFFF) | (value & 0xFF000000); }
        }

        public PspPointer(uint Address)
        {
            this.Address = Address;
        }

        public static implicit operator uint(PspPointer that)
        {
            return that.Address;
        }

        public static implicit operator PspPointer(uint that)
        {
            return new PspPointer()
            {
                Address = that,
            };
        }

        public override string ToString()
        {
            return String.Format("PspPointer(0x{0:X})", Address);
        }

        public bool IsNull
        {
            get { return Address == 0; }
        }

        public unsafe void* GetPointer(PspMemory PspMemory, int Size)
        {
            return PspMemory.PspPointerToPointerSafe(this, Size);
        }

        public unsafe void* GetPointer<TType>(PspMemory PspMemory)
        {
            return PspMemory.PspPointerToPointerSafe(this, Marshal.SizeOf(typeof(TType)));
        }

        public unsafe void* GetPointerNotNull<TType>(PspMemory PspMemory)
        {
            var Pointer = this.GetPointer<TType>(PspMemory);
            if (Pointer == null)
                throw(new NullReferenceException(String.Format("Pointer for {0} can't be null", typeof(TType))));
            return Pointer;
        }

        /*
        public unsafe PspMemoryPointer<TType> GetPspMemoryPointer<TType>(PspMemory pspMemory)
        {
            return new PspMemoryPointer<TType>(pspMemory, Address);
        }
        */
    }

    /*
    public class Reference<TType>
    {
        public TType Value;
    }

    public class PspMemoryPointer<TType>
    {
        public PspMemory PspMemory;
        public uint Address;

        public PspMemoryPointer(PspMemory PspMemory, uint Address)
        {
            this.PspMemory = PspMemory;
            this.Address = Address;
        }

        public void UpdateIndex(Action<Reference<TType>> Action)
        {
        }
    }
    */

    /*
    public struct PspPointer<TType>
    {
        public uint Address;

        public PspPointer(uint Address)
        {
            this.Address = Address;
        }
    }
    */
}