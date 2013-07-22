using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Loader
{
	public class ElfConfig
	{
		public bool InfoExeHasRelocation;
		public uint RelocatedBaseAddress;
		public string GameTitle;
	}
}
