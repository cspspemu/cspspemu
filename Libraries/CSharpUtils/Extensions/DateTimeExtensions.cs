using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static public class DateTimeExtensions
{
	static public long GetTotalNanoseconds(this DateTime DateTime)
	{
		return DateTime.Ticks * 10;
	}

	static public long GetTotalMicroseconds(this DateTime DateTime)
	{
		return DateTime.GetTotalNanoseconds() * 1000;
	}
}
