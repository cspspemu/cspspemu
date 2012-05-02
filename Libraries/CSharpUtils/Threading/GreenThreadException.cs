using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpUtils.Threading
{
	public class GreenThreadException : Exception
	{
		public GreenThreadException(string Name, Exception InnerException) : base (Name, InnerException)
		{
		}
	}
}
