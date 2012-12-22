using CSPspEmu.Core;
using CSPspEmu.Hle.Threading.Semaphores;

namespace CSPspEmu.Hle.Managers
{
	public class HleSemaphoreManager
	{
		public HleUidPool<HleSemaphore> Semaphores = new HleUidPool<HleSemaphore>();

		public HleSemaphore Create()
		{
			return new HleSemaphore();
		}
	}
}
