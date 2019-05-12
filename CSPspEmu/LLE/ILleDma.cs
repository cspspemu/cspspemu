namespace CSPspEmuLLETest
{
    public interface ILleDma
    {
        void Transfer(Dma.Direction direction, int size, DmaEnum address, ref uint value);
    }
}