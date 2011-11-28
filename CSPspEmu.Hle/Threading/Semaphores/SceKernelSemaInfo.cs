using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Threading.Semaphores
{
	unsafe public struct SceKernelSemaInfo
	{
		/// <summary>
		/// Size of the ::SceKernelSemaInfo structure.
		/// </summary>
		public int Size;

		/// <summary>
		/// NUL-terminated name of the semaphore.
		/// </summary>
		public fixed byte Name[32];

		/// <summary>
		/// Attributes.
		/// </summary>
		public SemaphoreAttribute Attributes;

		/// <summary>
		/// The initial count the semaphore was created with.
		/// </summary>
		public int InitialCount;

		/// <summary>
		/// The current count.
		/// </summary>
		public int CurrentCount;

		/// <summary>
		/// The maximum count.
		/// </summary>
		public int MaximumCount;

		/// <summary>
		/// The number of threads waiting on the semaphore.
		/// </summary>
		public int NumberOfWaitingThreads;
	}
}
