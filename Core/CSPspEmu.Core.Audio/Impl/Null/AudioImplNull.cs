using System;

namespace CSPspEmu.Core.Audio
{
    public class AudioImplNull : PspAudioImpl
	{
		public AudioImplNull()
		{
		}

		public override void Update(Action<short[]> ReadStream)
		{
			//throw new NotImplementedException();
		}

		public override void StopSynchronized()
		{
			//throw new NotImplementedException();
		}

		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "Null",
					Version = "1.0",
				};
			}
		}

		public override bool IsWorking
		{
			get { return true; }
		}
	}
}
