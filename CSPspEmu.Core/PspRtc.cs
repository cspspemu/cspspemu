using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Core
{
	public class PspRtc
	{
		public DateTime StartDateTime;
		public DateTime CurrentDateTime;
		public TimeSpan Elapsed
		{
			get {
				return CurrentDateTime - StartDateTime;
			}
		}

		public uint UnixTimeStamp
		{
			get
			{
				return (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			}
		}

		public PspRtc()
		{
			Start();
		}

		public void Start()
		{
			this.StartDateTime = DateTime.UtcNow;
		}

		public void Update()
		{
			this.CurrentDateTime = DateTime.UtcNow;
		}
	}
}
