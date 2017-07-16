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
			set
			{
				Address = (Address & 0xFF000000) | (value & 0x00FFFFFF);
			}
		}

		public uint High8
		{
			set
			{
				Address = (Address & 0x00FFFFFF) | (value & 0xFF000000);
			}
		}

		public PspPointer(uint address)
		{
			Address = address;
		}

		public static implicit operator uint(PspPointer<TType> that)
		{
			return that.Address;
		}

		public static implicit operator PspPointer<TType>(uint that)
		{
			return new PspPointer
			{
				Address = that,
			};
		}

		public override string ToString()
		{
			return String.Format("PspPointer(0x{0:X})", Address);
		}

		public bool IsNull => Address == 0;

		public void* GetPointer(PspMemory pspMemory)
		{
			return pspMemory.PspPointerToPointerSafe(this, Marshal.SizeOf(typeof(TType)));
		}

		public void* GetPointerNotNull(PspMemory pspMemory)
		{
			var pointer = GetPointer(pspMemory);
			if (pointer == null) throw (new NullReferenceException(String.Format("Pointer for {0} can't be null", typeof(TType))));
			return pointer;
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
			set
			{
				Address = (Address & 0xFF000000) | (value & 0x00FFFFFF);
			}
		}

		public uint High8
		{
			set
			{
				Address = (Address & 0x00FFFFFF) | (value & 0xFF000000);
			}
		}

		public PspPointer(uint address)
		{
			Address = address;
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

		public bool IsNull => Address == 0;

		public void* GetPointer(PspMemory pspMemory, int size)
		{
			return pspMemory.PspPointerToPointerSafe(this, size);
		}

		public void* GetPointer<TType>(PspMemory pspMemory)
		{
			return pspMemory.PspPointerToPointerSafe(this, Marshal.SizeOf(typeof(TType)));
		}

		public void* GetPointerNotNull<TType>(PspMemory pspMemory)
		{
			var pointer = GetPointer<TType>(pspMemory);
			if (pointer == null) throw(new NullReferenceException(String.Format("Pointer for {0} can't be null", typeof(TType))));
			return pointer;
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
