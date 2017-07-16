using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests.CSPspEmu.Core.Cpu.Cpu
{
    public class TestConnector : ICpuConnector
    {
        public void Yield(CpuThreadState CpuThreadState)
        {
        }
    }

    public class TestInterruptManager : IInterruptManager
    {
        void IInterruptManager.Interrupt(CpuThreadState CpuThreadState)
        {
        }
    }

    static public class CpuUtils
    {
        static LazyPspMemory LazyPspMemory = new LazyPspMemory();

        static public CpuProcessor CreateCpuProcessor(PspMemory Memory = null)
        {
            if (Memory == null) Memory = LazyPspMemory;
            var InjectContext = new InjectContext();
            InjectContext.SetInstance<PspMemory>(Memory);
            InjectContext.SetInstance<ICpuConnector>(new TestConnector());
            InjectContext.SetInstance<IInterruptManager>(new TestInterruptManager());
            return InjectContext.GetInstance<CpuProcessor>();
        }
    }
}