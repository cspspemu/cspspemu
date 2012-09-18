using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSPspEmu.Core.Crypto;

namespace CSPspEmuLLETest
{
	unsafe public class LleKirk : ILleDma
	{
		DebugPspMemory Memory;
		Kirk Kirk;
		uint KirkSource;
		uint KirkDestination;
		uint KirkCommand;
		uint KirkResult;

		public LleKirk(DebugPspMemory Memory)
		{
			this.Memory = Memory;
			Kirk = new Kirk();
			Kirk.kirk_init();
		}

		public void Transfer(Dma.Direction Direction, int Size, DmaEnum Address, ref uint Value)
		{
			switch (Address)
			{
				case DmaEnum.KIRK_PATTERN:
					Value = 1;
					break;
				case DmaEnum.KIRK_COMMAND:
					TransferUtils.Transfer(Direction, ref KirkCommand, ref Value);
					break;
				case DmaEnum.KIRK_RESULT:
					TransferUtils.Transfer(Direction, ref KirkResult, ref Value);
					break;
				case DmaEnum.KIRK_SOURCE_ADDRESS:
					TransferUtils.Transfer(Direction, ref KirkSource, ref Value);
					break;
				case DmaEnum.KIRK_DESTINATION_ADDRESS:
					TransferUtils.Transfer(Direction, ref KirkDestination, ref Value);
					break;
				case DmaEnum.KIRK_START:
					if (KirkCommand != 1) throw(new NotImplementedException());

					var SourcePtr = (byte *)Memory.PspAddressToPointerSafe(KirkSource);
					var DestinationPtr = (byte*)Memory.PspAddressToPointerSafe(KirkDestination);

					try
					{
						Kirk.kirk_CMD1(DestinationPtr, SourcePtr, -1, true);
						this.KirkResult = (uint)Kirk.ResultEnum.OK;
					}
					catch (Kirk.KirkException KirkException)
					{
						this.KirkResult = (uint)KirkException.Result;
						Console.Error.WriteLine("Kirk.KirkException : {0}", KirkException);
					}

					//Thread.Sleep(4);
					break;
			}
		}
	}
}
