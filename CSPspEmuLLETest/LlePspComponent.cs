using System.Threading;

namespace CSPspEmuLLETest
{
	public abstract class LlePspComponent
	{
		protected AutoResetEvent StartEvent = new AutoResetEvent(false);
		protected bool Running = false;
		protected Thread Thread;

		public LlePspComponent()
		{
			Thread = new Thread(Main)
			{
				Name = "LlePspComponent",
				IsBackground = true,
			};
			Thread.Start();
		}

		public abstract void Main();

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
