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
		public CpuThreadState CpuThreadState;
		//0xBC100004

		public Dma(CpuThreadState CpuThreadState)
		{
			this.CpuThreadState = CpuThreadState;
		}

		bool LogDMAReads = true;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Size"></param>
		/// <param name="Address"></param>
		/// <returns></returns>
		public uint ReadDMA(int Size, uint Address)
		{
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
				case 0xBE240000:
					Console.WriteLine("Read: GPIO");
					break;
				case 0xBC10007C:
					Console.WriteLine("Read: SystemConfig.GPIO_IO_ENABLE");
					break;
				default:
					if (LogDMAReads) Console.WriteLine("ReadDMA(0x{0:X8}) at 0x{1:X8}", Address, CpuThreadState.PC);
					break;
			}
			return 0;
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
					Console.WriteLine("{0:X8}: Write: SystemConfig.ClearAllInterrupts : 0x{1:X8}", CpuThreadState.PC, Value);
					break;
				case 0xBC10004C:
					Console.WriteLine("{0:X8}: Write: SystemConfig.RESET_ENABLE : 0x{1:X8}", CpuThreadState.PC, Value);
					break;
				case 0xbe240000:
					Console.WriteLine("Write: GPIO");
					break;
				case 0xbe240004:
					Console.WriteLine("Write: GPIO.PortRead");
					break;
				case 0xbe240008:
					Console.WriteLine("Write: GPIO.PortWrite");
					break;
				case 0xbe24000C:
					Console.WriteLine("Write: GPIO.PortClear");
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
					if (LogDMAReads) Console.WriteLine("WriteDMA(0x{0:X8}) <- 0x{1:X8} at 0x{2:X8}", Address, Value, CpuThreadState.PC);
					break;
			}
		}
	}
}
