using System;
using CSPspEmu.Hle.Attributes;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core;
using CSPspEmu.Core.Memory;

namespace CSPspEmu.Hle.Modules.ge
{
	[HlePspModule(ModuleFlags = ModuleFlags.UserMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceGe_user : HleModuleHost
	{
		[Inject]
		CpuProcessor CpuProcessor;

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
			return PspMemory.FrameBufferSegment.Size;
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
		/// 
		/// </summary>
		/// <param name="Mode"></param>
		/// <param name="BreakAddress"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB448EC0D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeBreak(int Mode, void* BreakAddress)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x4C06E472, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeContinue()
		{
			throw (new NotImplementedException());
		}
	}
}
