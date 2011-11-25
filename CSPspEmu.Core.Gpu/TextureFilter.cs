using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu.State
{
	public enum TextureFilter
	{
		Nearest = 0,
		Linear = 1,

		NearestMipmapNearest = 4,
		LinearMipmapNearest = 5,
		NearestMipmapLinear = 6,
		LinearMipmapLinear = 7,
	}
}
