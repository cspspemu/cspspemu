using System;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Hle.Modules.sysmem;

namespace CSPspEmu.Hle.Modules.ge
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceGe_user : HleModuleHost
	{
		[Inject]
		public GpuProcessor GpuProcessor;

		[Inject]
		public CpuProcessor CpuProcessor;

		[Inject]
		public SysMemUserForUser SysMemUserForUser;

		/// <summary>
		/// Get the address of VRAM.
		/// </summary>
		/// <returns>A pointer to the base of VRAM.</returns>
		[HlePspFunction(NID = 0xE47E40E4, FirmwareVersion = 150)]
		public uint sceGeEdramGetAddr()
		{
			return PspMemory.FrameBufferSegment.Low;
		}

		/// <summary>
		/// Get the size of VRAM.
		/// </summary>
		/// <returns>The size of VRAM (in bytes).</returns>
		[HlePspFunction(NID = 0x1F6752AD, FirmwareVersion = 150)]
		public int sceGeEdramGetSize()
		{
			return (int)PspMemory.FrameBufferSegment.Size;
		}

		private int eDRAMMemoryWidth;

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB77905EA, FirmwareVersion = 150)]
		public int sceGeEdramSetAddrTranslation(int Size)
		{
			try
			{
				return eDRAMMemoryWidth;
			}
			finally
			{
				eDRAMMemoryWidth = Size;
			}
		}

		/// <summary>
		/// Interrupt drawing queue.
		/// </summary>
		/// <param name="Mode">If set to 1, reset all the queues.</param>
		/// <param name="BreakAddress">Unused (just K1-checked).</param>
		/// <returns>The stopped queue ID if mode isn't set to 0, otherwise 0, and &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0xB448EC0D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeBreak(int Mode, void* BreakAddress)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Restart drawing queue.
		/// </summary>
		/// <returns>&lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x4C06E472, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeContinue()
		{
			var currentList = GpuProcessor.GetCurrentGpuDisplayList();
			if (currentList == null)
			{
				return 0;
			}

			if (currentList.Status.Value == DisplayListStatusEnum.Paused)
			{
				if (!GpuProcessor.IsBreak)
				{
					if (currentList.Signal == SignalBehavior.PSP_GE_SIGNAL_HANDLER_PAUSE)
					{
						return unchecked((int)SceKernelErrors.ERROR_BUSY);
					}

					currentList.Status.SetValue(DisplayListStatusEnum.Drawing);
					currentList.Signal = SignalBehavior.PSP_GE_SIGNAL_NONE;

					// TODO Restore context of DL is necessary
					// TODO Restore BASE

					// We have a list now, so it's not complete.
					//drawCompleteTicks = (u64) - 1;
				}
				else
				{
					currentList.Status.SetValue(DisplayListStatusEnum.Queued);
				}
			}
			else if (currentList.Status.Value == DisplayListStatusEnum.Drawing)
			{
				if (SysMemUserForUser.sceKernelGetCompiledSdkVersion() >= 0x02000000)
				{
					return unchecked((int)SceKernelErrors.ERROR_ALREADY);
				}
				return -1;
			}
			else
			{
				if (SysMemUserForUser.sceKernelGetCompiledSdkVersion() >= 0x02000000)
				{
					return unchecked((int)0x80000004);
				}
				return -1;
			}

			//ProcessDLQueue();
			return 0;
		}
	}
}
