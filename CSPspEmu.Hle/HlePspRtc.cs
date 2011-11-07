using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle
{
	public class HlePspRtc
	{
		public DateTime StepDateTime;

		public void Update()
		{
			this.StepDateTime = DateTime.UtcNow;
		}
	}
}
