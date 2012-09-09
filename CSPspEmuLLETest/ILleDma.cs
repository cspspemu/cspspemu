namespace CSPspEmuLLETest
{
	public interface ILleDma
	{
		void Transfer(Dma.Direction Direction, int Size, uint Address, ref uint Value);
	}
}
