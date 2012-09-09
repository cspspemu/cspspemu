using System;

namespace CSPspEmuLLETest
{
	public class LleGPIO : ILleDma
	{
		public void Transfer(Dma.Direction Direction, int Size, uint Address, ref uint Value)
		{
			switch (Address)
			{
				case 0xbe240000:
					Console.WriteLine("{0}: GPIO", Direction);
					break;
				case 0xbe240004:
					Console.WriteLine("{0}: GPIO.PortRead", Direction);
					break;
				case 0xbe240008:
					Console.WriteLine("{0}: GPIO.PortWrite", Direction);
					break;
				case 0xbe24000C:
					Console.WriteLine("{0}: GPIO.PortClear", Direction);
					break;
				default:
					throw(new NotImplementedException());
			}
		}
	}
}
