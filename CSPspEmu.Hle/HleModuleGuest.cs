using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;
using System.Reflection;
using CSPspEmu.Hle.Formats;
using CSPspEmu.Hle.Loader;
using CSPspEmu.Core.Memory;
using CSharpUtils;

namespace CSPspEmu.Hle
{
	unsafe public struct SceModule
	{
		/// <summary>
		/// 
		/// </summary>
		public PspPointer Next; // SceModule*

		/// <summary>
		/// 
		/// </summary>
		public ElfPsp.ModuleInfo.AtributesEnum Attributes;

		/// <summary>
		/// 
		/// </summary>
		public ushort Version;

		/// <summary>
		/// 
		/// </summary>
		public fixed byte ModuleNameRaw[28];

		/// <summary>
		/// 
		/// </summary>
		public string ModuleName
		{
			get
			{
				fixed (byte* Ptr = ModuleNameRaw) return PointerUtils.PtrToStringUtf8(Ptr);
			}
			set
			{
				fixed (byte* Ptr = ModuleNameRaw) PointerUtils.StoreStringOnPtr(value, Encoding.UTF8, Ptr, 28);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public uint Unknown1;

		/// <summary>
		/// 
		/// </summary>
		public uint Unknown2;

		/// <summary>
		/// 
		/// </summary>
		public SceUID ModuleId;

		/// <summary>
		/// 
		/// </summary>
		public fixed uint Unknown3[4];

		/// <summary>
		/// 
		/// </summary>
		public PspPointer ent_top; // void*

		/// <summary>
		/// 
		/// </summary>
		public uint ent_size;

		/// <summary>
		/// 
		/// </summary>
		public PspPointer stub_top; // void*

		/// <summary>
		/// 
		/// </summary>
		public uint stub_size;

		/// <summary>
		/// 
		/// </summary>
		public fixed uint unknown4[4];

		/// <summary>
		/// 
		/// </summary>
		public uint entry_addr;

		/// <summary>
		/// 
		/// </summary>
		public uint gp_value;

		/// <summary>
		/// 
		/// </summary>
		public uint text_addr;

		/// <summary>
		/// 
		/// </summary>
		public uint text_size;

		/// <summary>
		/// 
		/// </summary>
		public uint data_size;

		/// <summary>
		/// 
		/// </summary>
		public uint bss_size;

		/// <summary>
		/// 
		/// </summary>
		public uint nsegment;

		/// <summary>
		/// 
		/// </summary>
		public fixed uint segmentaddr[4];

		/// <summary>
		/// 
		/// </summary>
		public fixed uint segmentsize[4];
	}

	unsafe public class HleModuleGuest : HleModule
	{
		public bool Loaded;
		public ElfPsp.ModuleInfo ModuleInfo;
		public ElfPspLoader.InitInfoStruct InitInfo;
		public MemoryPartition SceModuleStructPartition;
	}
}
