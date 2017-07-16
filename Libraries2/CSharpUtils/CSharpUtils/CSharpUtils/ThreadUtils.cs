using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpUtils
{
	public class ThreadUtils
	{
		static public void SleepUntilUtc(DateTime Until)
		{
			var Duration = Until - DateTime.UtcNow;
			if (Duration.TotalSeconds < 0) return;
			Thread.Sleep(Duration);
		}
	}
}
