using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.pmfplayer
{
	unsafe public partial class scePsmf : HleModuleHost
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmf">PSMF struct.</param>
		/// <param name="buffer_addr">Actual PMF data</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xC22C8327, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfSetPsmf(int psmf, int buffer_addr)
		{
			return -1;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="psmf"></param>
		/// <param name="videoInfoAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0BA514E5, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfGetVideoInfo(int psmf, int videoInfoAddr)
		{
			return -1;
		}

		[HlePspFunction(NID = 0x68D42328, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int scePsmfGetNumberOfSpecificStreams()
		{
			return 0;
		}
	}
}
