namespace CSPspEmu.Core.Gpu.Impl.Soft
{
    public class GpuImplSoft : GpuImpl
    {
        public override PluginInfo PluginInfo => new PluginInfo
        {
            Name = "Soft",
            Version = "1.0",
        };

        public override bool IsWorking => true;
    }
}