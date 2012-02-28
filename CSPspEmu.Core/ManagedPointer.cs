using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core
{
	unsafe public class ManagedPointer<TType>
	{
		public byte* InternalPointer;
		static public readonly Type Type = typeof(TType);
		static public readonly int EntrySize = Marshal.SizeOf(typeof(TType));

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
}
