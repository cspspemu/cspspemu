using System.Threading;

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
