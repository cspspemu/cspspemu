using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace CSharpUtils
{
	static public class TimeSpanUtils
	{
		static public TimeSpan FromMicroseconds(long Microseconds)
		{
			return TimeSpan.FromMilliseconds((double)Microseconds / (double)1000.0);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Action"></param>
		static public void InfiniteLoopDetector(Action Action)
		{
			using (var Timer = new Timer(4.0 * 1000))
			{
				bool Cancel = false;
				Timer.Elapsed += (sender, e) =>
				{
					if (!Cancel)
					{
						Console.WriteLine("InfiniteLoop Detected! : {0}", e.SignalTime);
					}
				};
				Timer.AutoReset = false;
				Timer.Start();
				try
				{
					Action();
				}
				finally
				{
					Cancel = true;
					Timer.Enabled = false;
					Timer.Stop();
				}
			}
		}
	}
}
