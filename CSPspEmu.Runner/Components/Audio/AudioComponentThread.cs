using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;

namespace CSPspEmu.Runner.Components.Audio
{
	public sealed class AudioComponentThread : ComponentThread
	{
		[Inject]
		private PspAudio PspAudio;

		public override void InitializeComponent()
		{
		}

		protected override string ThreadName { get { return "AudioThread"; } }

		protected override void Main()
		{
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			while (true)
			{
				ThreadTaskQueue.HandleEnqueued();
				if (!Running) return;

				PspAudio.Update();
				Thread.Sleep(1);
			}
		}
	}
}
