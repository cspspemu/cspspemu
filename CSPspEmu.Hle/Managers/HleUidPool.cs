using System;

namespace CSPspEmu.Hle.Managers
{
	public class HleUidPool<TType> : HleUidPoolSpecial<TType, int> where TType : IDisposable
	{
	}
}
