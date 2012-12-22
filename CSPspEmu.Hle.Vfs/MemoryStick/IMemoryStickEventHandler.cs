using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Vfs.MemoryStick
{
	public interface IMemoryStickEventHandler
	{
		void ScheduleCallback(int CallbackId);
	}
}
