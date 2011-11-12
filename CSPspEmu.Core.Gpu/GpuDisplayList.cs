using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core.Threading.Synchronization;
using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu
{
	sealed unsafe public class GpuDisplayList
	{
		public struct OptionalParams
		{
			public int ContextAddress;
			public int StackDepth;
			public int StackAddress;
		}

		public enum StatusEnum
		{
			Done = 0,
			Queued = 1,
			DrawingDone = 2,
			StallReached = 3,
			CancelDone = 4,
		}

		/// <summary>
		/// A value between 0 and 63 inclusive.
		/// </summary>
		public int Id;

		public uint* InstructionAddressStart;
		public uint* InstructionAddressCurrent;
		public uint* InstructionAddressStall;
		public GpuStateStruct* GpuStateStructPointer;

		/// <summary>
		/// Stack with the InstructionAddressCurrent for the CALL/RET opcodes.
		/// </summary>
		readonly private Stack<IntPtr> ExecutionStack = new Stack<IntPtr>();

		/*
		private bool Finished;
		private bool Paused;
		private bool Ended;
		private bool Reset;
		private bool Restarted;
		*/

		/// <summary>
		/// Current status of the DisplayList.
		/// </summary>
		readonly public WaitableStateMachine<StatusEnum> Status = new WaitableStateMachine<StatusEnum>();

		/// <summary>
		/// Indicates if the list can be used.
		/// </summary>
		public bool Available { set; get; }

		/// <summary>
		/// 
		/// </summary>
		public OptionalParams pspGeListOptParam;

		/// <summary>
		/// Constructor
		/// </summary>
		internal GpuDisplayList()
		{
		}

		bool Done;

		/// <summary>
		/// Executes this Display List.
		/// </summary>
		internal void Process()
		{

			for (Done = false; !Done ; InstructionAddressCurrent++)
			{
				if ((InstructionAddressStall != null) && (InstructionAddressCurrent >= InstructionAddressStall)) break;
				ProcessInstruction(*InstructionAddressCurrent);
			}

			if (InstructionAddressStall == null)
			{
				Status.Value = StatusEnum.Done;
				return;
			}

			if (InstructionAddressCurrent == InstructionAddressStall)
			{
				Status.Value = StatusEnum.StallReached;
				return;
			}
		}

		private void ProcessInstruction(uint Instruction)
		{
			var OpCode = (GpuOpCodes)(Instruction & 0xFF);

			Console.WriteLine(OpCode);

			switch (OpCode)
			{
				case GpuOpCodes.END:
					Console.WriteLine("END!");
					Done = true;
					break;
			}
		}
	}
}
