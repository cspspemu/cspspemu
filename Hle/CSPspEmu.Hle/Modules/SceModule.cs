using CSharpUtils;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules
{
	public unsafe struct SceModule
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
		public uint EntryAddress;

		/// <summary>
		/// 
		/// </summary>
		public uint GP;

		/// <summary>
		/// 
		/// </summary>
		public uint TextAddress;

		/// <summary>
		/// 
		/// </summary>
		public uint TextSize;

		/// <summary>
		/// 
		/// </summary>
		public uint DataSize;

		/// <summary>
		/// 
		/// </summary>
		public uint BssSize;

		/// <summary>
		/// 
		/// </summary>
		public uint NumberOfSegments;

		/// <summary>
		/// 
		/// </summary>
		public fixed uint SegmentAddress[4];

		/// <summary>
		/// 
		/// </summary>
		public fixed uint SegmentSize[4];
	}
}
