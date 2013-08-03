using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using System;

namespace CSPspEmu.Runner.Components.Audio
{
	public sealed class AudioComponentThread : ComponentThread
	{
		[Inject]
		private PspAudio PspAudio;

		protected override string ThreadName { get { return "AudioThread"; } }

		protected override void Main()
		{
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			//Thread.CurrentThread.Priority = ThreadPriority.Normal;
			Console.WriteLine("AudioComponentThread.Start()");
			try
			{
				while (true)
				{
					ThreadTaskQueue.HandleEnqueued();
					if (!Running) return;

					PspAudio.Update();
					Thread.Sleep(1);
				}
			}
			finally
			{
				Console.WriteLine("AudioComponentThread.End()");
			}
		}
	}
}
