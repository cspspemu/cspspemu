using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cscodec
{
	public class MathUtils
	{
		static public int Clamp(int Value, int Min, int Max)
		{
			if (Value < Min) return Min;
			if (Value > Max) return Max;
			return Value;
		}
	}
}
