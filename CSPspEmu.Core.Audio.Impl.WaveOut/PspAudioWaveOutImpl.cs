using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmu.Core.Audio.Impl.WaveOut
{
	public class PspAudioWaveOutImpl : PspAudioImpl
	{
		public override void Update(Func<int, short[]> ReadStream)
		{
			throw new NotImplementedException();
		}

		public override void StopSynchronized()
		{
			throw new NotImplementedException();
		}

		public override void InitializeComponent()
		{
			throw new NotImplementedException();
		}
	}
}
