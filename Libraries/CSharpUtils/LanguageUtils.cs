using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils
{
	public class LanguageUtils
	{
		static public void Swap<T>(ref T a, ref T b)
		{
			T c = a;
			a = b;
			b = c;
		}
	}
}
