namespace CSPspEmuLLETest
{
	public interface ILleDma
	{
		void Transfer(Dma.Direction Direction, int Size, DmaEnum Address, ref uint Value);
	}
}
