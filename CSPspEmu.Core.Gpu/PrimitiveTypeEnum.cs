
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Gpu
{
	public enum PrimitiveType : byte
	{
		Points = 0,
		Lines = 1,
		LineStrip = 2,
		Triangles = 3,
		TriangleStrip = 4,
		TriangleFan = 5,
		Sprites = 6,
	}

}
