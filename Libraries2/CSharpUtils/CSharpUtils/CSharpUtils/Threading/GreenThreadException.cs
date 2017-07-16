using System;

namespace CSharpUtils.Threading
{
	public class GreenThreadException : Exception
	{
		public GreenThreadException(string Name, Exception InnerException) : base (Name, InnerException)
		{
		}
	}
}
