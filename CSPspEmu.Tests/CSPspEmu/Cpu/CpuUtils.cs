using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;

namespace Tests.CSPspEmu.Core.Cpu
{
    public class TestConnector : ICpuConnector
    {
        public void Yield(CpuThreadState cpuThreadState)
        {
        }
    }

    public class TestInterruptManager : IInterruptManager
    {
        void IInterruptManager.Interrupt(CpuThreadState cpuThreadState)
        {
        }
    }

    public static class CpuUtils
    {
        static readonly LazyPspMemory _lazyPspMemory = new LazyPspMemory();

        public static CpuProcessor CreateCpuProcessor(PspMemory memory = null)
        {
            if (memory == null) memory = _lazyPspMemory;
            var injectContext = new InjectContext();
            injectContext.SetInstance<PspMemory>(memory);
            injectContext.SetInstance<ICpuConnector>(new TestConnector());
            injectContext.SetInstance<IInterruptManager>(new TestInterruptManager());
            return injectContext.GetInstance<CpuProcessor>();
        }
    }
}