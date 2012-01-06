using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils.Extensions;

namespace CSPspEmu.Core.Utils
{
	public class NumberUtils
	{
		static public int ParseIntegerConstant(String Value, int DefaultBase = 10)
		{
			try
			{
				Value = Value.Replace("_", "");
				if (Value.Substr(0, 1) == "-") return -ParseIntegerConstant(Value.Substr(1));
				if (Value.Substr(0, 1) == "+") return +ParseIntegerConstant(Value.Substr(1));
				if (Value.Substr(0, 2) == "0x") return Convert.ToInt32(Value.Substr(2), 16);
				if (Value.Substr(0, 2) == "0b") return Convert.ToInt32(Value.Substr(2), 2);
				return Convert.ToInt32(Value, DefaultBase);
			}
			catch (FormatException FormatException)
			{
				throw (new FormatException("Can't parse the string '" + Value + "'", FormatException));
			}
		}
	}
}
