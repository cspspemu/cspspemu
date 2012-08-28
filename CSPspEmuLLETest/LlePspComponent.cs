using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSPspEmuLLETest
{
	abstract public class LlePspComponent
	{
		protected AutoResetEvent StartEvent = new AutoResetEvent(false);
		protected bool Running = false;
		protected Thread Thread;

		public LlePspComponent()
		{
			Thread = new Thread(Main);
			Thread.IsBackground = true;
			Thread.Start();
		}

		abstract public void Main();

		public void Start()
		{
			Running = true;
			StartEvent.Set();
		}

		public void Stop()
		{
			Running = false;
		}

		public void Reset()
		{
			Stop();
			Start();
		}
	}
}
