using System;
using System.Runtime.InteropServices;

namespace CSharpUtils.Process.Impl
{
	public class ProcessBaseImplFibers : IProcessBaseImpl
	{
		[DllImport("kernel32.dll")]
		private static extern IntPtr ConvertThreadToFiber(int fiberData);

		[DllImport("kernel32.dll")]
		private static extern IntPtr CreateFiber(int size, System.Delegate function, int handle);

		[DllImport("kernel32.dll")]
		private static extern IntPtr SwitchToFiber(IntPtr fiberAddress);

		[DllImport("kernel32.dll")]
		private static extern void DeleteFiber(IntPtr fiberAddress);

		[DllImport("kernel32.dll")]
		private static extern int GetLastError();

		private static IntPtr mainFiberHandle = IntPtr.Zero;
		private IntPtr fiberHandle;

		public void Init(RunDelegate Delegate)
		{
			if (mainFiberHandle == IntPtr.Zero)
			{
				mainFiberHandle = ConvertThreadToFiber(0);
			}
			fiberHandle = CreateFiber(500, Delegate, 0);
		}

		public void SwitchTo()
		{
			SwitchToFiber(fiberHandle);
		}

		public void Remove()
		{
			if (fiberHandle != IntPtr.Zero)
			{
				DeleteFiber(fiberHandle);
				fiberHandle = IntPtr.Zero;
			}
		}

		public void Yield()
		{
			SwitchToFiber(mainFiberHandle);
		}
	}
}
