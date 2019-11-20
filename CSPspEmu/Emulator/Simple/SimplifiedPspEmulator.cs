using CSPspEmu.Core;
using CSPspEmu.Core.Components.Controller;
using CSPspEmu.Core.Components.Display;
using CSPspEmu.Core.Components.Rtc;
using CSPspEmu.Core.Memory;
using CSPspEmu.Runner;
using CSPspEmu.Runner.Components.Display;

namespace CSPspEmu.Emulator.Simple
{
    public class SimplifiedPspEmulator
    {
        public InjectContext injector;
        public PspEmulator Emulator;
        public PspRtc Rtc;
        public PspDisplay Display;
        public DisplayComponentThread DisplayComponent;
        public PspMemory Memory;
        public PspController Controller;

        public SimplifiedPspEmulator()
        {
            injector = PspInjectContext.CreateInjectContext(PspStoredConfig.Load(), false);
            Emulator = injector.GetInstance<PspEmulator>();
            Emulator.PspRunner = null;
            Emulator.CpuConfig.DebugSyscalls = true;
            Emulator.CpuConfig.TrackCallStack = true;
            
            Rtc = injector.GetInstance<PspRtc>();
            Display = injector.GetInstance<PspDisplay>();
            DisplayComponent = injector.GetInstance<DisplayComponentThread>();
            Memory = injector.GetInstance<PspMemory>();
            Controller = injector.GetInstance<PspController>();
            DisplayComponent.triggerStuff = true;
        }

        public void LoadAndStart(string File)
        {
            Emulator.LoadFile("../../../../deploy/cspspemu/demos/compilerPerf.pbp");
        } 
    }
}