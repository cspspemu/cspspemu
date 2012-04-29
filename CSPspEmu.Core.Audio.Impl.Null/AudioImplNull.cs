using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;

namespace CSPspEmu.Core.Audio
{
	unsafe public class AudioImplNull : PspAudioImpl
	{
		public override void InitializeComponent()
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
