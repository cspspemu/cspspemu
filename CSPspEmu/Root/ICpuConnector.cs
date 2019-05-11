namespace CSPspEmu.Core.Cpu
{
    public interface ICpuConnector
    {
        void Yield(CpuThreadState cpuThreadState);
    }
}