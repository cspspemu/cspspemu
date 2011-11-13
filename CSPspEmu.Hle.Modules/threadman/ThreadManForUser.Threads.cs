using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core.Cpu;

namespace CSPspEmu.Hle.Modules.threadman
{
	unsafe public partial class ThreadManForUser
	{
		public struct SceKernelThreadOptParam
		{
		}

		/// <summary>
		/// Create a thread
		/// </summary>
		/// <example>
		/// SceUID thid;
		/// thid = sceKernelCreateThread("my_thread", threadFunc, 0x18, 0x10000, 0, NULL);
		/// </example>
		/// <param name="Name">An arbitrary thread name.</param>
		/// <param name="EntryPoint">The thread function to run when started.</param>
		/// <param name="InitPriority">The initial priority of the thread. Less if higher priority.</param>
		/// <param name="StackSize">The size of the initial stack.</param>
		/// <param name="Attribute">The thread attributes, zero or more of ::PspThreadAttributes.</param>
		/// <param name="Option">Additional options specified by ::SceKernelThreadOptParam.</param>
		/// <returns>UID of the created thread, or an error code.</returns>
		[HlePspFunction(NID = 0x446D8DE6, FirmwareVersion = 150)]
		public uint sceKernelCreateThread(CpuThreadState CpuThreadState, string Name, uint EntryPoint, int InitPriority, int StackSize, uint Attribute, SceKernelThreadOptParam* Option)
		{
			var Thread = HleState.ThreadManager.Create();
			Thread.Name = Name;
			Thread.EntryPoint = EntryPoint;
			Thread.InitPriority = InitPriority;
			Thread.Attribute = Attribute;
			Thread.Stack = HleState.MemoryManager.RootPartition.Allocate(StackSize, MemoryPartition.Anchor.High);
			Thread.CpuThreadState.PC = (uint)EntryPoint;
			Thread.CpuThreadState.GP = (uint)CpuThreadState.GP;
			Thread.CpuThreadState.SP = (uint)(Thread.Stack.High & ~(uint)0xFF);
			Thread.CpuThreadState.RA = (uint)0x08000000;
			//Thread.CpuThreadState.RA = (uint)0;

			//Console.WriteLine("STACK: {0:X}", Thread.CpuThreadState.SP);

			return (uint)Thread.Id;
		}

		/// <summary>
		/// Start a created thread
		/// </summary>
		/// <param name="ThreadId">Thread id from sceKernelCreateThread</param>
		/// <param name="ArgumentsLength">Length of the data pointed to by argp, in bytes</param>
		/// <param name="ArgumentsPointer">Pointer to the arguments.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xF475845D, FirmwareVersion = 150)]
		public int sceKernelStartThread(CpuThreadState CpuThreadState, uint ThreadId, uint ArgumentsLength, uint ArgumentsPointer)
		{
			var Thread = HleState.ThreadManager.GetThreadById((int)ThreadId);
			Thread.CpuThreadState.GPR[4] = (int)ArgumentsLength;
			Thread.CpuThreadState.GPR[5] = (int)ArgumentsPointer;
			Thread.CurrentStatus = HleThread.Status.Ready;

			// Schedule new thread?
			CpuThreadState.Yield();
			return 0;
		}

		/// <summary>
		/// Exit a thread and delete itself.
		/// </summary>
		/// <param name="Status">Exit status</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x809CE29B, FirmwareVersion = 150)]
		public int sceKernelExitDeleteThread(int Status)
		{
			HleState.ThreadManager.Exit(HleState.ThreadManager.Current);
			return 0;
		}

		/// <summary>
		/// Exit a thread
		/// </summary>
		/// <param name="status">Exit status.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xAA73C935, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelExitThread(int status)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Sleep thread but service any callbacks as necessary
		/// </summary>
		/// <example>
		///		// Once all callbacks have been setup call this function
		///		sceKernelSleepThreadCB();
		/// </example>
		/// <returns></returns>
		[HlePspFunction(NID = 0x82826F70, FirmwareVersion = 150)]
		public int sceKernelSleepThreadCB(CpuThreadState CpuThreadState)
		{
			HleState.ThreadManager.Current.CurrentStatus = HleThread.Status.Waiting;
			HleState.ThreadManager.Current.CurrentWaitType = HleThread.WaitType.None;
			CpuThreadState.Yield();
			return 0;
		}
	}
}
