using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSPspEmu.Core;
using CSPspEmu.Core.Cpu;

namespace CSPspEmuLLETest
{
	public class Dma
	{
		public enum Direction
		{
			Write,
			Read,
		}

		public CpuThreadState CpuThreadState;
		public LLEState LLEState;
		//0xBC100004

		public Dma(CpuThreadState CpuThreadState)
		{
			this.CpuThreadState = CpuThreadState;
		}

		bool LogDMAReads = true;

		public void TransferDMA(Dma.Direction Direction, int Size, uint Address, ref uint Value)
		{
			if (Address >= 0xBE240000 && Address <= 0xbe24000C) { LLEState.GPIO.Transfer(Direction, Size, Address, ref Value); return; }
			else
			{
				if (LogDMAReads) Console.WriteLine("{0}.DMA(0x{1:X8}) at 0x{2:X8}", Direction, Address, CpuThreadState.PC);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <param name="Address"></param>
		/// <returns></returns>
		public uint ReadDMA(int Size, uint Address)
		{
			uint Value = 0;
			switch (Address)
			{
				case 0xBC10004C:
					Console.WriteLine("Read: SystemConfig.RESET_ENABLE");
					break;
				case 0xbc100050:
					Console.WriteLine("Read: SystemConfig.BUS_CLOCK_ENABLE");
					break;
				case 0xbc100078:
					Console.WriteLine("Read: SystemConfig.IO_ENABLE");
					break;
				case 0xBC10007C:
					Console.WriteLine("Read: SystemConfig.GPIO_IO_ENABLE");
					break;
				default:
					TransferDMA(Direction.Read, Size, Address, ref Value);
					break;
			}
			return Value;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <param name="Address"></param>
		/// <param name="Value"></param>
		public void WriteDMA(int Size, uint Address, uint Value)
		{
			switch (Address)
			{
				case 0xbc100004: // Clear All Interrupts
					Console.WriteLine("{0:X8}: Write: SystemConfig.ClearInterrupts : 0x{1:X8}", CpuThreadState.PC, Value);
					break;
				case 0xBC10004C:
					Console.WriteLine("{0:X8}: Write: SystemConfig.RESET_ENABLE : 0x{1:X8}", CpuThreadState.PC, Value);
					if ((Value & 2) != 0) // ME
					{
						LLEState.Me.Reset();
					}
					break;
				case 0xbc100050:
					Console.WriteLine("Write: SystemConfig.BUS_CLOCK_ENABLE");
					break;
				case 0xBC10007C:
					Console.WriteLine("Write: SystemConfig.GPIO_IO_ENABLE(0x{0:X8})", Value);
					break;
				case 0xbc100078:
					Console.WriteLine("Write: SystemConfig.IO_ENABLE(0x{0:X8})", Value);
					break;
				default:
					TransferDMA(Direction.Write, Size, Address, ref Value);
					break;
			}
		}
	}
}
