using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Name"></param>
		/// <param name="Attributes"></param>
		/// <param name="Options"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB7D098C6, FirmwareVersion = 150)]
		public int sceKernelCreateMutex(string Name, uint Attributes, int Options)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="MutexId"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF8170FBE, FirmwareVersion = 150)]
		public int sceKernelDeleteMutex(int MutexId)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="MutexId"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x6B30100F, FirmwareVersion = 150)]
		public int sceKernelUnlockMutex(int MutexId)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="MutexId"></param>
		/// <param name="Count"></param>
		/// <param name="Timeout"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xB011B11F, FirmwareVersion = 150)]
		public int sceKernelLockMutex(int MutexId, int Count, uint* Timeout)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="MutexId"></param>
		/// <param name="Count"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x0DDCD2C9, FirmwareVersion = 150)]
		public int sceKernelTryLockMutex(int MutexId, int Count)
		{
			throw (new NotImplementedException());
		}
	}
}
