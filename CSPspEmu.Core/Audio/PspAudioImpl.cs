using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core.Audio
{
	abstract public class PspAudioImpl : PspEmulatorComponent
	{
		/// <summary>
		/// Called periodically on a thread.
		/// </summary>
		abstract public void Update(Func<int, short[]> ReadStream);

		/// <summary>
		/// 
		/// </summary>
		abstract public void StopSynchronized();
	}
}
