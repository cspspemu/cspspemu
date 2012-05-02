using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	static public class TimeSpanUtils
	{
		static public TimeSpan FromMicroseconds(long Microseconds)
		{
			return TimeSpan.FromMilliseconds((double)Microseconds / (double)1000.0);
		}
	}
}
