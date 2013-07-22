using CSPspEmu.Core;
using CSPspEmu.Hle.Threading.Semaphores;

namespace CSPspEmu.Hle.Managers
{
	public class HleSemaphoreManager
	{
		public HleUidPool<HleSemaphore> Semaphores = new HleUidPool<HleSemaphore>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_SEMAPHORE,
		};

		public HleSemaphore Create()
		{
			return new HleSemaphore();
		}
	}
}
