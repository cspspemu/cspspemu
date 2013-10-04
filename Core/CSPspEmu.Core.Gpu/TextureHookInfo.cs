using CSPspEmu.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Gpu
{
	unsafe public class TextureHookInfo
	{
		public int Width;
		public int Height;
		public OutputPixel[] Data;
		public TextureCacheKey TextureCacheKey;
	}
}
