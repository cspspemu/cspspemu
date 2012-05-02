using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	public class ColorFormats
	{
		static public readonly ColorFormat RGBA_5650 = new ColorFormat()
		{
			TotalBytes = 2,
			Offsets = new int[] { 0, 5, 11, 0 },
			Sizes = new int[] { 5, 6, 5, 0 },
		};

		static public readonly ColorFormat RGBA_4444 = new ColorFormat()
		{
			TotalBytes = 2,
			Offsets = new int[] { 0, 4, 8, 12 },
			Sizes = new int[] { 4, 4, 4, 4 },
		};

		static public readonly ColorFormat RGBA_5551 = new ColorFormat()
		{
			TotalBytes = 2,
			Offsets = new int[] { 0, 5, 10, 15 },
			Sizes = new int[] { 5, 5, 5, 1 },
		};

		static public readonly ColorFormat RGBA_8888 = new ColorFormat()
		{
			TotalBytes = 4,
			Offsets = new int[] { 0, 8, 16, 24 },
			Sizes = new int[] { 8, 8, 8, 8 },
		};
	}
}
