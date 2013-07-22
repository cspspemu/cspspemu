using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public class SysclibForKernel : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="str1"></param>
		/// <param name="str2"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xC0AB8932, FirmwareVersion = 150)]
		public int strcmp(string str1, string str2)
		{
			if (str1.Length > str2.Length) return -1;
			if (str1.Length < str2.Length) return +1;
			int len = str1.Length;
			for (int n = 0; n < len; n++)
			{
				int dif = (int)str1[n] - (int)str2[n];
				if (dif != 0) return dif;
			}
			return 0;
		}
	}
}
