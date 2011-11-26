using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Threading.Semaphores;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		HleSemaphoreManager SemaphoreManager { get { return HleState.SemaphoreManager; } }

		/// <summary>
		/// Creates a new semaphore
		/// </summary>
		/// <example>
		///		int semaid;
		///		semaid = sceKernelCreateSema("MyMutex", 0, 1, 1, 0);
		/// </example>
		/// <param name="Name">Specifies the name of the sema</param>
		/// <param name="SemaphoreAttribute">Sema attribute flags (normally set to 0)</param>
		/// <param name="InitialCount">Sema initial value </param>
		/// <param name="MaximumCount">Sema maximum value</param>
		/// <param name="Options">Sema options (normally set to 0)</param>
		/// <returns>A semaphore id</returns>
		[HlePspFunction(NID = 0xD6DA4BA1, FirmwareVersion = 150)]
		public SemaphoreId sceKernelCreateSema(string Name, SemaphoreAttribute SemaphoreAttribute, int InitialCount, int MaximumCount, SceKernelSemaOptParam* Options)
		{
			var HleSemaphore = SemaphoreManager.Create();
			{
				HleSemaphore.Name = Name;
				HleSemaphore.SceKernelSemaInfo.Attributes = SemaphoreAttribute;
				HleSemaphore.SceKernelSemaInfo.InitialCount = InitialCount;
				HleSemaphore.SceKernelSemaInfo.CurrentCount = InitialCount;
				HleSemaphore.SceKernelSemaInfo.MaximumCount = MaximumCount;
			}
			return (SemaphoreId)SemaphoreManager.Semaphores.Create(HleSemaphore);
		}

		/// <summary>
		/// Send a signal to a semaphore
		/// </summary>
		/// <example>
		/// // Signal the sema
		/// sceKernelSignalSema(semaid, 1);
		/// </example>
		/// <param name="SemaphoreId">The sema id returned from sceKernelCreateSema</param>
		/// <param name="Signal">The amount to signal the sema (i.e. if 2 then increment the sema by 2)</param>
		/// <returns>less than 0 On error.</returns>
		[HlePspFunction(NID = 0x3F53E640, FirmwareVersion = 150)]
		public int sceKernelSignalSema(SemaphoreId SemaphoreId, int Signal)
		{
			var HleSemaphore = SemaphoreManager.Semaphores.Get((int)SemaphoreId);
			HleSemaphore.IncrementCount(Signal);
			return 0;
		}

		/// <summary>
		/// Destroy a semaphore
		/// </summary>
		/// <param name="SemaphoreId">The semaid returned from a previous create call.</param>
		/// <returns>Returns the value 0 if its succesful otherwise -1</returns>
		[HlePspFunction(NID = 0x28B6489C, FirmwareVersion = 150)]
		public int sceKernelDeleteSema(SemaphoreId SemaphoreId)
		{
			var HleSemaphore = SemaphoreManager.Semaphores.Get((int)SemaphoreId);
			SemaphoreManager.Semaphores.Remove((int)SemaphoreId);
			HleSemaphore.IncrementCount(HleSemaphore.SceKernelSemaInfo.MaximumCount);

			return 0;
		}

		/// <summary>
		/// Lock a semaphore
		/// </summary>
		/// <example>
		/// sceKernelWaitSema(semaid, 1, 0);
		/// </example>
		/// <param name="SemaphoreId">The sema id returned from sceKernelCreateSema</param>
		/// <param name="Signal">The value to wait for (i.e. if 1 then wait till reaches a signal state of 1 or greater)</param>
		/// <param name="Timeout">Timeout in microseconds (assumed).</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x4E3A1105, FirmwareVersion = 150)]
		public int sceKernelWaitSema(SemaphoreId SemaphoreId, int Signal, uint* Timeout)
		{
			var CurrentThread = HleState.ThreadManager.Current;
			var Semaphore = SemaphoreManager.Semaphores.Get((int)SemaphoreId);

			CurrentThread.SetWaitAndPrepareWakeUp(HleThread.WaitType.Semaphore, "sceKernelWaitSema", (WakeUpCallback) =>
			{
				Semaphore.WaitThread(CurrentThread, WakeUpCallback, Signal);
			});

			return 0;
		}

		/// <summary>
		/// Lock a semaphore a handle callbacks if necessary.
		/// </summary>
		/// <example>
		/// sceKernelWaitSemaCB(semaid, 1, 0);
		/// </example>
		/// <param name="SemaphoreId">The sema id returned from sceKernelCreateSema</param>
		/// <param name="Signal">The value to wait for (i.e. if 1 then wait till reaches a signal state of 1)</param>
		/// <param name="Timeout">Timeout in microseconds (assumed).</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x6D212BAC, FirmwareVersion = 150)]
		public int sceKernelWaitSemaCB(SemaphoreId SemaphoreId, int Signal, uint* Timeout)
		{
			throw (new NotImplementedException());
			//return _sceKernelWaitSemaCB(semaid, signal, timeout, /* callback = */ true);
		}

		/// <summary>
		/// Poll a sempahore.
		/// </summary>
		/// <param name="SemaphoreId">UID of the semaphore to poll.</param>
		/// <param name="Signal">The value to test for.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x58B1F937, FirmwareVersion = 150)]
		public int sceKernelPollSema(SemaphoreId SemaphoreId, int Signal)
		{
			throw(new NotImplementedException());
			/*
			if (signal <= 0) return SceKernelErrors.ERROR_KERNEL_ILLEGAL_COUNT;

			try {
				PspSemaphore pspSemaphore = uniqueIdFactory.get!PspSemaphore(semaid);
			
				if (pspSemaphore.info.currentCount - signal < 0) return SceKernelErrors.ERROR_KERNEL_SEMA_ZERO;

				pspSemaphore.info.currentCount -= signal;
				return 0;
			} catch (UniqueIdNotFoundException) {
				return SceKernelErrors.ERROR_KERNEL_NOT_FOUND_SEMAPHORE;
			}
			*/
		}

		/// <summary>
		/// Retrieve information about a semaphore.
		/// </summary>
		/// <param name="SemaphoreId">UID of the semaphore to retrieve info for.</param>
		/// <param name="SceKernelSemaInfo">Pointer to a ::SceKernelSemaInfo struct to receive the info.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xBC6FEBC5, FirmwareVersion = 150)]
		public int sceKernelReferSemaStatus(SemaphoreId SemaphoreId, SceKernelSemaInfo* SceKernelSemaInfo)
		{
			var HleSemaphore = SemaphoreManager.Semaphores.Get((int)SemaphoreId);
			*SceKernelSemaInfo = HleSemaphore.SceKernelSemaInfo;
			return 0;
		}

		[HlePspFunction(NID = 0x8FFDF9A2, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		[HlePspUnknownDefinition]
		public void sceKernelCancelSema()
		{
			throw (new NotImplementedException());
		}
	}
}
