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
		public uint sceKernelCreateThread(CpuThreadState CpuThreadState, string Name, uint EntryPoint, int InitPriority, uint StackSize, uint Attribute, SceKernelThreadOptParam* Option)
		{
			Console.Write("PC(0x{0:X}) : ", CpuThreadState.RA);
			Console.WriteLine("ThreadManForUser.sceKernelCreateThread('{0}', {1:X}, {2:X}, {3:X}, {4:X}, {5:X})", Name, EntryPoint, InitPriority, StackSize, Attribute, (uint)Option);

			var Thread = HleState.HleThreadManager.Create();
			Thread.Stack = HleState.HleMemoryManager.RootPartition.Allocate(StackSize, MemoryPartition.Anchor.High);
			Thread.CpuThreadState.PC = (uint)EntryPoint;
			Thread.CpuThreadState.GP = (uint)CpuThreadState.GP;
			Thread.CpuThreadState.SP = (uint)(Thread.Stack.High);
			Thread.CpuThreadState.RA = (uint)0;

			Console.WriteLine("    : {0}", Thread.Id);

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
		public int sceKernelStartThread(CpuThreadState CpuThreadState, uint ThreadId, uint ArgumentsLength, void* ArgumentsPointer)
		{
			Console.Write("PC(0x{0:X}) : ", CpuThreadState.RA);

			Console.WriteLine("ThreadManForUser.sceKernelStartThread({0:X}, {1:X}, {2:X})", ThreadId, ArgumentsLength, (uint)ArgumentsPointer);

			HleState.HleThreadManager.GetThreadById((int)ThreadId).CurrentStatus = HleThread.Status.Ready;

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
			Console.WriteLine("ThreadManForUser.sceKernelExitThread({0:X})", Status);
			HleState.HleThreadManager.Exit(HleState.HleThreadManager.Current);
			return 0;
		}

		/// <summary>
		/// Exit a thread
		/// </summary>
		/// <param name="status">Exit status.</param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xAA73C935, FirmwareVersion = 150)]
		public int sceKernelExitThread(int status)
		{
			throw (new NotImplementedException());
		}
	}
}
