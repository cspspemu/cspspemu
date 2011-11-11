using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core.Threading.Synchronization;

namespace CSPspEmu.Core.Gpu
{
	sealed unsafe public class GpuDisplayList
	{
		public enum StatusEnum
		{
			Done = 0,
			Queued = 1,
			DrawingDone = 2,
			StallReached = 3,
			CancelDone = 4,
		}

		readonly public WaitableStateMachine<StatusEnum> Status = new WaitableStateMachine<StatusEnum>();

		public uint* InstructionAddressCurrent;
		public uint* InstructionAddressStall;

		internal GpuDisplayList()
		{
		}

		internal void Process()
		{
			for (;  InstructionAddressCurrent < InstructionAddressStall; InstructionAddressCurrent++)
			{
				uint CurrentInstruction = *InstructionAddressCurrent;
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
	}
}
