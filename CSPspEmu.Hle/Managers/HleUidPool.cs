using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Managers
{
	public class HleUidPool<TType> : HleUidPoolSpecial<TType, int> where TType : IDisposable
	{
	}
}
