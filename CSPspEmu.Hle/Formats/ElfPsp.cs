using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Formats
{
	unsafe public class ElfPsp
	{
		public enum ModuleFlags : ushort
		{
			User   = 0x0000,
			Kernel = 0x1000,
		}

		public enum LibFlags : ushort
		{
			DirectJump = 0x0001,
			Syscall    = 0x4000,
			SysLib     = 0x8000,
		}

		public struct ModuleExport {
			/// <summary>
			/// Address to a stringz with the module.
			/// Relative to ?
			/// </summary>
			public uint Name;

			/// <summary>
			/// ?
			/// </summary>
			public ushort Version;

			/// <summary>
			/// ?
			/// </summary>
			public ushort Flags;
			
			/// <summary>
			/// ?
			/// </summary>
			public byte EntrySize;

			/// <summary>
			/// ?
			/// </summary>
			public byte VariableCount;

			/// <summary>
			/// ?
			/// </summary>
			public ushort FunctionCount;

			/// <summary>
			/// ?
			/// </summary>
			public uint Exports;
		}

		public struct ModuleImport {
			/// <summary>
			/// Address to a stringz with the module.
			/// </summary>
			public uint Name;
			
			/// <summary>
			/// Version of the module?
			/// </summary>
			public ushort Version;

			/// <summary>
			/// Flags for the module.
			/// </summary>
			public ushort Flags;

			/// <summary>
			/// ?
			/// </summary>
			public byte EntrySize;

			/// <summary>
			/// Number of imported variables.
			/// </summary>
			public byte VariableCount;

			/// <summary>
			/// Number of imported functions.
			/// </summary>
			public ushort FunctionCount;

			/// <summary>
			/// Address to the nid pointer. (Read)
			/// </summary>
			public uint NidAddress;

			/// <summary>
			/// Address to the function table. (Write 16 bits. jump/syscall)
			/// </summary>
			public uint CallAddress;
		}
	
		public struct ModuleInfo {
			/// <summary>
			/// ?
			/// </summary>
			public uint Flags;

			/// <summary>
			/// Name of the module.
			/// </summary>
			public fixed byte Name[28];

			/// <summary>
			/// Global Pointer initial value.
			/// </summary>
			public uint GP;

			/// <summary>
			/// ?
			/// </summary>
			public uint ExportsStart;

			/// <summary>
			/// ?
			/// </summary>
			public uint ExportsEnd;

			/// <summary>
			/// ?
			/// </summary>
			public uint ImportsStart;

			/// <summary>
			/// ?
			/// </summary>
			public uint ImportsEnd;
		}
	}
}
