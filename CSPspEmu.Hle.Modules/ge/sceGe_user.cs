using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.ge
{
	unsafe public partial class sceGe_user : HleModuleHost
	{
		/// <summary>
		/// Get the address of VRAM.
		/// </summary>
		/// <returns>A pointer to the base of VRAM.</returns>
		[HlePspFunction(NID = 0xE47E40E4, FirmwareVersion = 150)]
		public uint sceGeEdramGetAddr()
		{
			return HleState.Processor.Memory.FrameBufferSegment.Low;
		}

		/// <summary>
		/// Get the size of VRAM.
		/// </summary>
		/// <returns>The size of VRAM (in bytes).</returns>
		[HlePspFunction(NID = 0x1F6752AD, FirmwareVersion = 150)]
		public int sceGeEdramGetSize()
		{
			return HleState.Processor.Memory.FrameBufferSegment.Size;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB77905EA, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeEdramSetAddrTranslation()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="brk_addr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB448EC0D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceGeBreak(int mode, void* brk_addr)
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
