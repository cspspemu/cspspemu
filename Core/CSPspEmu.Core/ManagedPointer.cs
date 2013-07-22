using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core
{
	/*
	public unsafe class ManagedPointer<TType>
	{
		public byte* InternalPointer;
		public static readonly Type Type = typeof(TType);
		public static readonly int EntrySize = Marshal.SizeOf(typeof(TType));

		public ManagedPointer(byte* InternalPointer)
		{
			this.InternalPointer = InternalPointer;
		}

		public TType this[int Offset]
		{
			get
			{
				return (TType)Marshal.PtrToStructure(new IntPtr(InternalPointer + EntrySize * Offset), Type);
			}
			set
			{
				Marshal.StructureToPtr(value, new IntPtr(InternalPointer + EntrySize * Offset), false);
			}
		}
	}
	*/
}
