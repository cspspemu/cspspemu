
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Managers
{
	public enum RegHandle : uint { }

	public class HleRegistryManager
	{
		HleUidPool<RegHandle> RegHandles = new HleUidPool<RegHandle>();
	}
}
