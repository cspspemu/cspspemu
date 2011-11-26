using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio.Imple.Openal;

namespace CSPspEmu.Runner
{
	sealed public class AudioComponentThread : ComponentThread
	{
		PspAudioOpenalImpl PspAudioOpenalImpl;

		public AudioComponentThread(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			PspAudioOpenalImpl = PspEmulatorContext.GetInstance<PspAudioOpenalImpl>();
		}

		protected override string ThreadName { get { return "AudioThread"; } }

		protected override void Main()
		{
			Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
			while (true)
			{
				ThreadTaskQueue.HandleEnqueued();
				if (!Running)
				{
					return;
				}

				PspAudioOpenalImpl.Update();
				Thread.Sleep(1);
			}
		}
	}
}
