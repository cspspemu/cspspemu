using System.Runtime.InteropServices;

namespace CSharpUtils.Process.Impl
{
	//delegate void VoidDelegate();
	/// <summary>
	/// Untested
	/// </summary>
	/// <see>
	/// <ul>
	///		<li>http://www.mono-project.com/Interop_with_Native_Libraries</li>
	///		<li>http://www.gamedev.net/topic/581545-coroutines-under-windows-linux-and-mac/</li>
	///		<li>http://en.wikipedia.org/wiki/Setcontext</li>
	///	</ul>
	/// </see>
	public unsafe class ProcessBaseImplUcontext
	{
		struct ucontext_t {
			fixed int ContextSize[100];
		}

		/// <summary>
		/// Get user context and store it in variable pointed to by UCP.
		/// </summary>
		[DllImport("libc")]
		static extern int getcontext(ucontext_t *__ucp);

		/// <summary>
		/// Set user context from information of variable pointed to by UCP.
		/// </summary>
		[DllImport("libc")]
		static extern int setcontext(ucontext_t* __ucp);

		/// <summary>
		/// Save current context in context variable pointed to by OUCP
		/// and set context from variable pointed to by UCP.  */
		/// </summary>
		[DllImport("libc")]
		static extern int swapcontext(ucontext_t* __oucp, ucontext_t* __ucp);

		/// <summary>
		/// Manipulate user context UCP to continue with calling functions FUNC
		/// and the ARGC-1 parameters following ARGC when the context is used
		/// the next time in `setcontext' or `swapcontext'.
		/// 
		/// We cannot say anything about the parameters FUNC takes; `void'
		/// is as good as any other choice.
		/// </summary>
		[DllImport("libc")]
		//extern void makecontext(IntPtr* __ucp, RunDelegate __func, int __argc, params IntPtr __rest);
		static extern void makecontext(ucontext_t* __ucp, RunDelegate __func, int __argc);

		bool mainFiberSetted = false;
		private static ucontext_t mainFiberHandle;
		private ucontext_t fiberHandle;

		public void Init(RunDelegate Delegate)
		{
			if (!mainFiberSetted)
			{
				mainFiberSetted = true;
				fixed (ucontext_t *mainFiberHandlePtr = &mainFiberHandle) 
				{
					getcontext(mainFiberHandlePtr);
				}
			}
			fixed (ucontext_t *fiberHandlePtr = &fiberHandle) 
			{
				makecontext(fiberHandlePtr, Delegate, 0);
			}
		}

		public void SwitchTo()
		{
			fixed (ucontext_t* fiberHandlePtr = &fiberHandle)
			{
				setcontext(fiberHandlePtr);
			}
		}

		public void Remove()
		{
		}

		public void Yield()
		{
			fixed (ucontext_t* mainFiberHandlePtr = &mainFiberHandle)
			{
				setcontext(mainFiberHandlePtr);
			}
		}
	}
}
