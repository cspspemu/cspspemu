using CSharpUtils;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.loadexec
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public unsafe class sceDmac : HleModuleHost
	{
		/// <summary>
		/// Copies data using the internal DMAC. Should be faster than a memcpy,
		/// but requires that data to be copied is no more in the cache, so usually
		/// you should issue a oslUncacheData on the source and destination addresses
		/// else very strange bugs may happen.
		/// </summary>
		/// <param name="Destination"></param>
		/// <param name="Source"></param>
		/// <param name="Size"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x617F3FE6, FirmwareVersion = 150)]
		public int sceDmacMemcpy(byte* Destination, byte* Source, int Size)
		{
			PointerUtils.Memcpy(Destination, Source, Size);
			return 0;
		}

		//mixin(registerFunction!(0xD97F94D8, sceDmacTryMemcpy));
	}
}
