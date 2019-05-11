namespace CSPspEmu.Core.Cpu
{
    public interface IInterruptManager
    {
        void Interrupt(CpuThreadState cpuThreadState);
    }
}