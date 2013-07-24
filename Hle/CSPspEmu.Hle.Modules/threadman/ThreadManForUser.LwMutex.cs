namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public struct SceLwMutexWorkarea
	{
		public int lockLevel;
		public int lockThread;
		public ThreadManForUser.MutexAttributesEnum attr;
		public int numWaitThreads;
		public int uid;
		public fixed int pad[3];
	}

	unsafe public struct SceKernelLwMutexInfo
	{
		public SceSize size;
		public fixed byte _name[32];
		public ThreadManForUser.MutexAttributesEnum attr;
		public SceUID uid;
		public void* workarea;
		public int initCount;
		public int currentCount;
		public int lockThread;
		public int numWaitThreads;
	}

	public unsafe partial class ThreadManForUser
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="workAreaAddr"></param>
		/// <param name="Name"></param>
		/// <param name="Attributes"></param>
		/// <param name="InitialCount"></param>
		/// <param name="OptionAddress"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x19CFF145, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelCreateLwMutex(SceLwMutexWorkarea* WorkAreaPointer, string Name, ThreadManForUser.MutexAttributesEnum Attributes, int InitialCount, int OptionAddress)
		{
			WorkAreaPointer->attr = Attributes;
			WorkAreaPointer->lockLevel = 0;
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="workAreaAddr"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x60107536, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelDeleteLwMutex(SceLwMutexWorkarea* WorkAreaPointer)
		{
			//throw (new NotImplementedException());
			return 0;
		}
	}
}
