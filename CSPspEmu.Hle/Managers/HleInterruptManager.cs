using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Managers
{
	public class HleSubinterruptHandler
	{
		public bool Enabled;
		public int Index;
		public uint Address;
		public uint Argument;

		public HleSubinterruptHandler(int Index)
		{
			this.Index = Index;
			this.Enabled = false;
		}
	}

	sealed public class HleInterruptHandler
	{
		public PspInterrupts PspInterrupt;
		public HleSubinterruptHandler[] SubinterruptHandlers;
		private HleCallbackManager HleCallbackManager;
		private HleInterruptManager HleInterruptManager;

		internal HleInterruptHandler(HleInterruptManager HleInterruptManager, PspInterrupts PspInterrupt, HleCallbackManager HleCallbackManager)
		{
			this.HleInterruptManager = HleInterruptManager;
			this.PspInterrupt = PspInterrupt;
			this.HleCallbackManager = HleCallbackManager;
			this.SubinterruptHandlers = new HleSubinterruptHandler[16];
			for (int Index = 0; Index < this.SubinterruptHandlers.Length; Index++)
			{
				this.SubinterruptHandlers[Index] = new HleSubinterruptHandler(Index);
			}
		}

		public void Trigger()
		{
			if (HleInterruptManager.Enabled)
			{
				foreach (var Handler in SubinterruptHandlers.Where(Handler => Handler.Enabled))
				{
					HleInterruptManager.Queue(HleCallback.Create("InterruptTrigger", Handler.Address, Handler.Index, Handler.Argument));
				}
			}
			//Console.Error.WriteLine("Trigger: " + PspInterrupt);
		}
	}

	sealed public class HleInterruptManager : PspEmulatorComponent
	{
		/// <summary>
		/// Global Interrupt Enable
		/// </summary>
		public bool Enabled = true;

		private HleInterruptHandler[] InterruptHandlers;
		private HleCallbackManager HleCallbackManager;
		private CpuProcessor CpuProcessor;

		public HleInterruptHandler GetInterruptHandler(PspInterrupts PspInterrupt)
		{
			return InterruptHandlers[(int)PspInterrupt];
		}

		public override void InitializeComponent()
		{
			this.HleCallbackManager = PspEmulatorContext.GetInstance<HleCallbackManager>();
			this.CpuProcessor = PspEmulatorContext.GetInstance<CpuProcessor>();
			//uint MaxHandlers = Enum.GetValues(typeof(PspInterrupts)).OfType<uint>().Max() + 1;
			InterruptHandlers = new HleInterruptHandler[(int)PspInterrupts._MAX];
			for (int n = 0; n < InterruptHandlers.Length; n++)
			{
				InterruptHandlers[n] = new HleInterruptHandler(
					this,
					(PspInterrupts)n,
					HleCallbackManager
				);
			}
		}

		List<HleCallback> HleCallbackList = new List<HleCallback>();

		public void Queue(HleCallback HleCallback)
		{
			lock (HleCallbackList)
			{
				HleCallbackList.Add(HleCallback);
			}
		}

		public void ExecuteQueued(CpuThreadState BaseCpuThreadState)
		{
			if (Enabled)
			{
				HleCallback[] HleCallbackListCopy;
				lock (HleCallbackList)
				{
					HleCallbackListCopy = HleCallbackList.ToArray();
					HleCallbackList.Clear();
				}

				foreach (var HleCallback in HleCallbackListCopy)
				{
					var FakeCpuThreadState = new CpuThreadState(CpuProcessor);
					FakeCpuThreadState.CopyRegistersFrom(BaseCpuThreadState);
					HleCallback.SetArgumentsToCpuThreadState(FakeCpuThreadState);

					HleInterop.Execute(FakeCpuThreadState);
					//Console.Error.WriteLine("Execute queued");
				}
			}
		}

		public uint sceKernelCpuSuspendIntr()
		{
			try
			{
				return (uint)(Enabled ? 1 : 0);
			}
			finally
			{
				Enabled = false;
			}
		}

		public void sceKernelCpuResumeIntr(uint Flags)
		{
			//if (set != true) throw new NotImplementedException();
			Enabled = (Flags != 0);
		}
	}

	public enum PspInterrupts : uint
	{
		/// <summary>
		/// 
		/// </summary>
		PSP_GPIO_INT = 4,

		/// <summary>
		/// 
		/// </summary>
		PSP_ATA_INT = 5,

		/// <summary>
		/// 
		/// </summary>
		PSP_UMD_INT = 6,

		/// <summary>
		/// 
		/// </summary>
		PSP_MSCM0_INT = 7,

		/// <summary>
		/// 
		/// </summary>
		PSP_WLAN_INT = 8,

		/// <summary>
		/// 
		/// </summary>
		PSP_AUDIO_INT = 10,

		/// <summary>
		/// 
		/// </summary>
		PSP_I2C_INT = 12,

		/// <summary>
		/// 
		/// </summary>
		PSP_SIRCS_INT = 14,

		/// <summary>
		/// Calls to register or enable on these interrupts always yield 0x80020065 (illegal intr code),
		/// which seems plausible if they are system interrupts. QueryIntrHandlerInfo yields the following interesting information: 
		/// </summary>
		PSP_SYSTIMER0_INT = 15,

		/// <summary>
		/// 
		/// </summary>
		PSP_SYSTIMER1_INT = 16,

		/// <summary>
		/// 
		/// </summary>
		PSP_SYSTIMER2_INT = 17,

		/// <summary>
		/// 
		/// </summary>
		PSP_SYSTIMER3_INT = 18,

		/// <summary>
		/// 
		/// </summary>
		PSP_THREAD0_INT = 19,

		/// <summary>
		/// 
		/// </summary>
		PSP_NAND_INT = 20,

		/// <summary>
		/// 
		/// </summary>
		PSP_DMACPLUS_INT = 21,

		/// <summary>
		/// 
		/// </summary>
		PSP_DMA0_INT = 22,

		/// <summary>
		/// 
		/// </summary>
		PSP_DMA1_INT = 23,

		/// <summary>
		/// 
		/// </summary>
		PSP_MEMLMD_INT = 24,

		/// <summary>
		/// 
		/// </summary>
		PSP_GE_INT = 25,

		/// <summary>
		/// The vblank interrupt triggers every 1/60 second. Using the following function: 
		/// 
		///		int sceKernelRegisterSubIntrHandler(int intno, int no, void* handler, void* arg);
		///		
		/// up to 16 individual subinterrupt handlers may be installed for the vblank interrupt (intno = PSP_VBLANK_INT, no = 0 - 15).
		/// The prototype for vblank handler functions is: 
		/// 
		///		void vblank_handler(int no, void* arg);
		/// </summary>
		PSP_VBLANK_INT = 30,

		/// <summary>
		/// 
		/// </summary>
		PSP_MECODEC_INT = 31,

		/// <summary>
		/// 
		/// </summary>
		PSP_HPREMOTE_INT = 36,

		/// <summary>
		/// 
		/// </summary>
		PSP_MSCM1_INT = 60,

		/// <summary>
		/// 
		/// </summary>
		PSP_MSCM2_INT = 61,

		/// <summary>
		/// 
		/// </summary>
		PSP_THREAD1_INT = 65,

		/// <summary>
		/// 
		/// </summary>
		PSP_INTERRUPT_INT = 66,

		/// <summary>
		/// 
		/// </summary>
		_MAX = 67,
	}

	/*
	public enum PspSubInterrupts : uint
	{
		PSP_GPIO_SUBINT = PspInterrupts.PSP_GPIO_INT,
		PSP_ATA_SUBINT = PspInterrupts.PSP_ATA_INT,
		PSP_UMD_SUBINT = PspInterrupts.PSP_UMD_INT,
		PSP_DMACPLUS_SUBINT = PspInterrupts.PSP_DMACPLUS_INT,
		PSP_GE_SUBINT = PspInterrupts.PSP_GE_INT,
		PSP_DISPLAY_SUBINT = PspInterrupts.PSP_VBLANK_INT,
	}
	*/

	public struct PspIntrHandlerOptionParam
	{
		public int size;
		public uint entry;
		public uint common;
		public uint gp;
		public ushort intr_code;
		public ushort sub_count;
		public ushort intr_level;
		public ushort enabled;
		public uint calls;
		public uint field_1C;
		public uint total_clock_lo;
		public uint total_clock_hi;
		public uint min_clock_lo;
		public uint min_clock_hi;
		public uint max_clock_lo;
		public uint max_clock_hi;
	}
}
