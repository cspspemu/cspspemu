using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace cscodec
{
	public class MathUtils
	{
		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static int Clamp(int Value, int Min, int Max)
		{
			if (Value < Min) return Min;
			if (Value > Max) return Max;
			return Value;
		}
	}
}
