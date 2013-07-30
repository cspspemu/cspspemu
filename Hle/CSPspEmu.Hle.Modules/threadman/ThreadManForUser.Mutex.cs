#define MUTEX_USE_WAIT_CALLBACK

using System;
using System.Collections.Generic;
using System.Linq;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.threadman
{
	public unsafe partial class ThreadManForUser
	{
		[Flags]
		public enum MutexAttributesEnum : uint
		{
			/// <summary>
			/// PSP_MUTEX_ATTR_FIFO
			/// </summary>
			Fifo = 0x000,

			/// <summary>
			/// PSP_MUTEX_ATTR_PRIORITY
			/// </summary>
			Priority = 0x100,

			/// <summary>
			/// Allows recursively call lock mutex but only on the same thread.
			/// 
			/// PSP_MUTEX_ATTR_ALLOW_RECURSIVE
			/// </summary>
			AllowRecursive = 0x200,
		}

		[HleUidPoolClass(NotFoundError = SceKernelErrors.ERROR_KERNEL_MUTEX_NOT_FOUND)]
		public class PspMutex : IHleUidPoolClass, IDisposable
		{
			public string Name;
			public MutexAttributesEnum Attributes;
			public uint Options;
			public ThreadManForUser ThreadManForUser;
			public CpuThreadState LockCpuThreadState;
			public int CurrentCountValue = 0;
			public Queue<Action> WakeUpList = new Queue<Action>();

			public PspMutex(ThreadManForUser ThreadManForUser)
			{
				this.ThreadManForUser = ThreadManForUser;
			}

			public void Lock(CpuThreadState CurrentCpuThreadState, int UpdateCountValue, uint* Timeout)
			{
				if (Timeout != null)
				{
					Console.Error.WriteLine("PspMutex.Lock with Timeout not implemented!!");
					//throw (new NotImplementedException());
				}
				if (UpdateCountValue <= 0) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_COUNT));
				//Console.Error.WriteLine("Lock : {0}", ThreadManager.Current.Id);
				if (!TryLock(CurrentCpuThreadState, UpdateCountValue))
				{
					//ThreadManager.Current.
					ThreadManForUser.ThreadManager.Current.SetWaitAndPrepareWakeUp(HleThread.WaitType.Mutex, "sceKernelLockMutex", this, (WakeUp) =>
					{
						WakeUpList.Enqueue(() =>
						{
							WakeUp();
						});
					}, HandleCallbacks: false);
				}
			}

			public void Unlock(CpuThreadState CurrentCpuThreadState, int UpdateCountValue)
			{
				if (UpdateCountValue == 0) throw(new SceKernelException(SceKernelErrors.ERROR_KERNEL_MUTEX_UNLOCKED));
				if (this.CurrentCountValue - UpdateCountValue < 0) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_MUTEX_UNLOCK_UNDERFLOW));
				//Console.Error.WriteLine("Unlock : {0}", ThreadManager.Current.Id);
				//Console.Error.WriteLine(" {0} -> {1}", this.CurrentCountValue, this.CurrentCountValue - UpdateCountValue);
				CurrentCountValue -= UpdateCountValue;
				if (CurrentCountValue == 0)
				{
					//Console.Error.WriteLine("Release!");
					if (WakeUpList.Any())
					{
						var Action = WakeUpList.Dequeue();
						Action();
					}
				}
			}

			public bool TryLock(CpuThreadState CurrentCpuThreadState, int UpdateCountValue)
			{
				if (
					CurrentCountValue == 0 ||
					(Attributes.HasFlag(MutexAttributesEnum.AllowRecursive) && CurrentCpuThreadState == LockCpuThreadState)
				)
				{
					//Console.Error.WriteLine(" {0} -> {1}", this.CurrentCountValue, this.CurrentCountValue + UpdateCountValue);
					this.CurrentCountValue += UpdateCountValue;
					this.LockCpuThreadState = CurrentCpuThreadState;
					return true;
				}
				else
				{
					return false;
				}
			}

			void IDisposable.Dispose()
			{
				// TODO
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="Name"></param>
		/// <param name="Attributes"></param>
		/// <param name="Options"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB7D098C6, FirmwareVersion = 150)]
		public PspMutex sceKernelCreateMutex(CpuThreadState CpuThreadState, string Name, MutexAttributesEnum Attributes, uint Options)
		{
			var PspMutex = new PspMutex(this)
			{
				Name = Name,
				Attributes = Attributes,
				Options = Options,
				LockCpuThreadState = CpuThreadState,
			};
			return PspMutex;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="Mutex"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF8170FBE, FirmwareVersion = 150)]
		public int sceKernelDeleteMutex(CpuThreadState CpuThreadState, PspMutex Mutex)
		{
			Mutex.RemoveUid(InjectContext);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="Mutex"></param>
		/// <param name="Count"></param>
		/// <param name="Timeout"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB011B11F, FirmwareVersion = 150)]
		public int sceKernelLockMutex(CpuThreadState CpuThreadState, PspMutex Mutex, int Count, uint* Timeout)
		{
			Mutex.Lock(CpuThreadState, Count, Timeout);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		[HlePspFunction(NID = 0x5BF4DD27, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelLockMutexCB(CpuThreadState CpuThreadState, PspMutex Mutex, int Count, uint* Timeout)
		{
			return sceKernelLockMutex(CpuThreadState, Mutex, Count, Timeout);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="Mutex"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0DDCD2C9, FirmwareVersion = 150)]
		public int sceKernelTryLockMutex(CpuThreadState CpuThreadState, PspMutex Mutex, int Count)
		{
			Mutex.TryLock(CpuThreadState, Count);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="CpuThreadState"></param>
		/// <param name="Mutex"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x6B30100F, FirmwareVersion = 150)]
		public int sceKernelUnlockMutex(CpuThreadState CpuThreadState, PspMutex Mutex, int Count)
		{
			Mutex.Unlock(CpuThreadState, Count);
			return 0;
		}
	}
}
