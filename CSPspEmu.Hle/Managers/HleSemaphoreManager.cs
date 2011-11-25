using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Hle.Threading.Semaphores;

namespace CSPspEmu.Hle.Managers
{
	public class HleSemaphoreManager : PspEmulatorComponent
	{
		public HleUidPool<HleSemaphore> Semaphores = new HleUidPool<HleSemaphore>();

		public HleSemaphoreManager(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
		}

		public HleSemaphore Create()
		{
			return new HleSemaphore();
		}
	}
}
