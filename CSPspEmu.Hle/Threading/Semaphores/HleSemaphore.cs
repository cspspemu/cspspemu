using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Threading.Semaphores;
using CSharpUtils;

namespace CSPspEmu.Hle.Threading.Semaphores
{
	unsafe public class HleSemaphore
	{
		public class WaitingSemaphoreThread
		{
			public HleThread HleThread;
			public int ExpectedMinimumCount;
			public WakeUpCallbackDelegate WakeUpCallback;
		}

		public SceKernelSemaInfo SceKernelSemaInfo;
		protected List<WaitingSemaphoreThread> WaitingSemaphoreThreadList = new List<WaitingSemaphoreThread>();
		//public SortedSet<>

		protected int CurrentCount { get { return SceKernelSemaInfo.CurrentCount; } set { SceKernelSemaInfo.CurrentCount = value; } }

		public String Name
		{
			get
			{
				fixed (byte* NamePtr = SceKernelSemaInfo.Name) return PointerUtils.PtrToString(NamePtr, Encoding.ASCII);
			}
			set
			{
				fixed (byte* NamePtr = SceKernelSemaInfo.Name) PointerUtils.StoreStringOnPtr(value, Encoding.ASCII, NamePtr);
			}
		}

		public void IncrementCount(int IncrementCount)
		{
			CurrentCount += IncrementCount;

			UpdatedCurrentCount();
		}

		public void WaitThread(HleThread HleThread, WakeUpCallbackDelegate WakeUpCallback, int ExpectedMinimumCount)
		{
			WaitingSemaphoreThreadList.Add(
				new WaitingSemaphoreThread()
				{
					HleThread = HleThread,
					ExpectedMinimumCount = ExpectedMinimumCount,
					WakeUpCallback = WakeUpCallback,
				}
			);

			UpdatedCurrentCount();
		}

		protected void UpdatedCurrentCount()
		{
			// Selects all the waiting semaphores, that fit the count condition, in a FIFO order.
			var WaitingSemaphoreThreadIterator = WaitingSemaphoreThreadList
				.Where(WaitingThread => (CurrentCount >= WaitingThread.ExpectedMinimumCount))
			;

			// Reorders the waiting semaphores in a Thread priority order (descending).
			if (SceKernelSemaInfo.Attributes == SemaphoreAttribute.Priority)
			{
				WaitingSemaphoreThreadIterator = WaitingSemaphoreThreadIterator
					.OrderByDescending(WaitingThread => WaitingThread.HleThread.PriorityValue)
				;
			}

			// Iterates all the waiting semaphores in order removing them from list.
			foreach (var WaitingSemaphoreThread in WaitingSemaphoreThreadIterator.ToArray())
			{
				CurrentCount -= WaitingSemaphoreThread.ExpectedMinimumCount;
				WaitingSemaphoreThreadList.Remove(WaitingSemaphoreThread);
				WaitingSemaphoreThread.WakeUpCallback();
			}

			// Updates the statistic about waiting threads.
			SceKernelSemaInfo.NumberOfWaitingThreads = WaitingSemaphoreThreadList.Count;
		}
	}
}
