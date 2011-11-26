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
		public class WaitingThread
		{
			public HleThread HleThread;
			public int ExpectedMinimumCount;
			public WakeUpCallbackDelegate WakeUpCallback;
		}

		public SceKernelSemaInfo SceKernelSemaInfo;
		protected List<WaitingThread> WaitingThreads = new List<WaitingThread>();
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
			WaitingThreads.Add(
				new WaitingThread()
				{
					HleThread = HleThread,
					ExpectedMinimumCount = ExpectedMinimumCount,
					WakeUpCallback = WakeUpCallback,
				}
			);

			SceKernelSemaInfo.NumberOfWaitingThreads = WaitingThreads.Count;

			UpdatedCurrentCount();
		}

		protected void UpdatedCurrentCount()
		{
			foreach (var WaitingThread in WaitingThreads.Where(WaitingThread => (CurrentCount >= WaitingThread.ExpectedMinimumCount)).ToArray())
			{
				WaitingThreads.Remove(WaitingThread);
				WaitingThread.WakeUpCallback();
			}
		}
	}
}
