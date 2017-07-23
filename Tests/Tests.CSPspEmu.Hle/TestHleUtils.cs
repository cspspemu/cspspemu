using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Audio.Impl.Null;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Null;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle.Managers;

namespace Tests.CSPspEmu.Hle
{
    [InjectMap(typeof(PspMemory), typeof(LazyPspMemory))]
    [InjectMap(typeof(GpuImpl), typeof(GpuImplNull))]
    [InjectMap(typeof(PspAudioImpl), typeof(AudioImplNull))]
    [InjectMap(typeof(ICpuConnector), typeof(CpuConnector))]
    [InjectMap(typeof(IInterruptManager), typeof(HleInterruptManager))]
    public class TestHleUtils
    {
        class CpuConnector : ICpuConnector
        {
            public void Yield(CpuThreadState cpuThreadState)
            {
            }
        }

        public static InjectContext CreateInjectContext(object bootstrap)
        {
            var injectContext = InjectContext.Bootstrap(new TestHleUtils());
            injectContext.InjectDependencesTo(bootstrap);
            return injectContext;
        }
    }
}