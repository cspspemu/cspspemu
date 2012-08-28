using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPspEmuLLETest
{
	public interface ILleDma
	{
		void Transfer(Dma.Direction Direction, int Size, uint Address, ref uint Value);
	}
}
