using System;

namespace CSPspEmu.Hle.Attributes
{
	[Flags]
	public enum ModuleFlags : uint
	{
		Flags0x00010011 = 0x00010011,
		Flags0x00000011 = 0x00000011,
		Flags0x00010000 = 0x00010000,

		// Partition
		KernelMode = 0x00000000,
		UserMode = 0x40000000,
	}

	public class HlePspModuleAttribute : Attribute
	{
		public ModuleFlags ModuleFlags;
	}
}
