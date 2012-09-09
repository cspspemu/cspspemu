using System;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Rtc;
using CSPspEmu.Hle.Managers;
using CSharpUtils;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.threadman
{
	public unsafe partial class ThreadManForUser
	{
		public class VirtualTimer : IDisposable
		{
			[Inject]
			PspRtc PspRtc;

			[Inject]
			HleMemoryManager MemoryManager;

			[Inject]
			CpuProcessor CpuProcessor;

			[Inject]
			PspMemory Memory;

			[Inject]
			HleInterop HleInterop;

			protected PspVirtualTimer Timer;
			public int Id;
			public string Name;
			public SceKernelVTimerOptParam SceKernelVTimerOptParam;
			protected long PreviousUpdatedTime;
			protected long CurrentUpdatedTime;
			protected long ElapsedAccumulatedTime;

			protected bool HandlerEnabled;
			protected long HandlerTime;
			protected bool HandlerIsWide;
			protected PspPointer HandlerCallback;
			protected PspPointer HandlerArgument;

			public struct PspSharedInfoStruct
			{
				public SceKernelSysClock ElapsedScheduled;
				public SceKernelSysClock ElapsedReal;
			}

			MemoryPartition PspSharedInfoMemoryPartition;
			PspSharedInfoStruct* PspSharedInfo;

			public VirtualTimer(PspEmulatorContext PspEmulatorContext, string Name)
			{
				PspEmulatorContext.InjectDependencesTo(this);

				this.Timer = PspRtc.CreateVirtualTimer(Handler);
				this.Name = Name;
				this.Timer.Enabled = false;
				this.PspSharedInfoMemoryPartition = MemoryManager.GetPartition(HleMemoryManager.Partitions.Kernel0).Allocate(
					sizeof(PspSharedInfoStruct),
					Name: "VTimer.PspSharedInfoStruct"
				);
				this.PspSharedInfo = (PspSharedInfoStruct*)CpuProcessor.Memory.PspAddressToPointerSafe(this.PspSharedInfoMemoryPartition.Low);
			}

			public void Dispose()
			{
				this.PspSharedInfoMemoryPartition.DeallocateFromParent();
			}

			public void Handler()
			{
				Console.Error.WriteLine("Handler!!");
				if (HandlerEnabled)
				{
					UpdateElapsedTime(true);
					PspSharedInfo->ElapsedScheduled.MicroSeconds = HandlerTime;
					PspSharedInfo->ElapsedReal.MicroSeconds = ElapsedAccumulatedTime;

					uint Result = HleInterop.ExecuteFunctionNow(
						HandlerCallback,
						Id,
						Memory.PointerToPspAddressSafe(&PspSharedInfo->ElapsedScheduled),
						Memory.PointerToPspAddressSafe(&PspSharedInfo->ElapsedReal),
						HandlerArgument
					); 
					Console.Error.WriteLine("Handler ENABLED!! {0}", Result);
				}
			}

			public void UpdateElapsedTime(bool Increment)
			{
				PspRtc.Update();
				this.CurrentUpdatedTime = PspRtc.Elapsed.GetTotalMicroseconds();
				{
					if (Increment)
					{
						this.ElapsedAccumulatedTime += (this.CurrentUpdatedTime - this.PreviousUpdatedTime);
					}
				}
				this.PreviousUpdatedTime = this.CurrentUpdatedTime;
			}

			public long ElapsedMicroseconds
			{
				get
				{
					lock (Timer)
					{
						UpdateElapsedTime(this.Timer.Enabled);
						return this.ElapsedAccumulatedTime;
					}
				}
			}

			public void CancelHandler()
			{
				lock (Timer)
				{
					this.HandlerEnabled = false;
				}
			}

			protected void UpdateHandlerTime()
			{
				PspRtc.Update();
				Console.Error.WriteLine("UpdateHandlerTime: {0}", this.HandlerTime - ElapsedAccumulatedTime);
				this.Timer.DateTime = PspRtc.CurrentDateTime + TimeSpanUtils.FromMicroseconds(this.HandlerTime - ElapsedAccumulatedTime);
			}

			public void SetHandler(long Time, PspPointer HandlerCallback, PspPointer HandlerArgument, bool HandlerIsWide)
			{
				lock (Timer)
				{
					this.HandlerTime = Time;
					this.HandlerCallback = HandlerCallback;
					this.HandlerArgument = HandlerArgument;
					this.HandlerIsWide = HandlerIsWide;
					this.HandlerEnabled = true;
					UpdateHandlerTime();
				}
			}

			public void Start()
			{
				lock (Timer)
				{
					if (!this.Timer.Enabled)
					{
						UpdateElapsedTime(false);
						UpdateHandlerTime();
						this.Timer.Enabled = true;
					}
				}
			}

			public void Stop()
			{
				lock (Timer)
				{
					if (this.Timer.Enabled)
					{
						this.Timer.Enabled = false;
						UpdateElapsedTime(false);
					}
				}
			}
		}

		HleUidPoolSpecial<VirtualTimer, int> VirtualTimerPool = new HleUidPoolSpecial<VirtualTimer, int>()
		{
			OnKeyNotFoundError = SceKernelErrors.ERROR_KERNEL_NOT_FOUND_VTIMER,
		};

		public struct SceKernelVTimerOptParam
		{
			public uint StructSize;
		}

		/// <summary>
		/// Create a virtual timer
		/// </summary>
		/// <param name="Name">Name for the timer.</param>
		/// <param name="SceKernelVTimerOptParam">Pointer to an ::SceKernelVTimerOptParam (pass NULL)</param>
		/// <returns>The VTimer's UID or less than 0 on error.</returns>
		[HlePspFunction(NID = 0x20FFF560, FirmwareVersion = 150)]
		public int sceKernelCreateVTimer(string Name, SceKernelVTimerOptParam *SceKernelVTimerOptParam)
		{
			var VirtualTimer = new VirtualTimer(PspEmulatorContext, Name);
			if (SceKernelVTimerOptParam != null) VirtualTimer.SceKernelVTimerOptParam = *SceKernelVTimerOptParam;
			var VirtualTimerId = VirtualTimerPool.Create(VirtualTimer);
			VirtualTimer.Id = VirtualTimerId;
			return VirtualTimerId;
		}

		/// <summary>
		/// Start a virtual timer
		/// </summary>
		/// <param name="VirtualTimerId">The UID of the timer</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xC68D9437, FirmwareVersion = 150)]
		public int sceKernelStartVTimer(int VirtualTimerId)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			VirtualTimer.Start();
			return 0;
		}

		/// <summary>
		/// Stop a virtual timer
		/// </summary>
		/// <param name="VirtualTimerId">The UID of the timer</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xD0AEEE87, FirmwareVersion = 150)]
		public int sceKernelStopVTimer(int VirtualTimerId)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			VirtualTimer.Stop();
			return 0;
		}

		/// <summary>
		/// Set the timer time (wide format)
		/// </summary>
		/// <param name="VirtualTimerId">UID of the vtimer</param>
		/// <param name="Time">A ::SceKernelSysClock structure</param>
		/// <returns>Possibly the last time</returns>
		[HlePspFunction(NID = 0xFB6425C3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		[PspUntested]
		public int sceKernelSetVTimerTimeWide(int VirtualTimerId, long Time)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			//VirtualTimer.ElapsedMicroseconds = Time;
			return 0;
		}

		/// <summary>
		/// Get the timer time
		/// </summary>
		/// <param name="VirtualTimerId">UID of the vtimer</param>
		/// <param name="Time">Pointer to a ::SceKernelSysClock structure</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x034A921F, FirmwareVersion = 150)]
		public int sceKernelGetVTimerTime(int VirtualTimerId, SceKernelSysClock* Time)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			return (int)VirtualTimer.ElapsedMicroseconds;
		}

		/// <summary>
		/// Get the timer time (wide format)
		/// </summary>
		/// <param name="VirtualTimerId">UID of the vtimer</param>
		/// <returns>The 64bit timer time</returns>
		[HlePspFunction(NID = 0xC0B3FFD2, FirmwareVersion = 150)]
		public long sceKernelGetVTimerTimeWide(int VirtualTimerId)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			return VirtualTimer.ElapsedMicroseconds;
		}

		/// <summary>
		/// Set the timer handler.
		/// 
		/// Timer handler will be executed once after
		/// </summary>
		/// <param name="VirtualTimerId">UID of the vtimer</param>
		/// <param name="Time">Time to call the handler?</param>
		/// <param name="HandlerCallback">The timer handler</param>
		/// <param name="HandlerArgument">Common pointer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD8B299AE, FirmwareVersion = 150)]
		public int sceKernelSetVTimerHandler(int VirtualTimerId, SceKernelSysClock* Time, PspPointer HandlerCallback, PspPointer HandlerArgument)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			VirtualTimer.SetHandler(Time: Time->MicroSeconds, HandlerCallback: HandlerCallback, HandlerArgument: HandlerArgument, HandlerIsWide: false);
			return 0;
		}

		/// <summary>
		/// Set the timer handler (wide mode)
		/// </summary>
		/// <param name="VirtualTimerId">UID of the vtimer</param>
		/// <param name="Time">Time to call the handler?</param>
		/// <param name="HandlerCallback">The timer handler</param>
		/// <param name="HandlerArgument">Common pointer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x53B00E9A, FirmwareVersion = 150)]
		public int sceKernelSetVTimerHandlerWide(int VirtualTimerId, long Time, PspPointer HandlerCallback, PspPointer HandlerArgument)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			VirtualTimer.SetHandler(Time: Time, HandlerCallback: HandlerCallback, HandlerArgument: HandlerArgument, HandlerIsWide: true);
			return 0;
		}

		/// <summary>
		/// Cancel the timer handler
		/// </summary>
		/// <param name="VirtualTimerId">The UID of the vtimer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0xD2D615EF, FirmwareVersion = 150)]
		public int sceKernelCancelVTimerHandler(int VirtualTimerId)
		{
			var VirtualTimer = VirtualTimerPool.Get(VirtualTimerId);
			VirtualTimer.CancelHandler();
			return 0;
		}

		/// <summary>
		/// Deletes the timer handler
		/// </summary>
		/// <param name="VirtualTimerId">The UID of the vtimer</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x328F9E52, FirmwareVersion = 150)]
		public int sceKernelDeleteVTimer(int VirtualTimerId)
		{
			VirtualTimerPool.Remove(VirtualTimerId);
			return 0;
		}
	}
}
