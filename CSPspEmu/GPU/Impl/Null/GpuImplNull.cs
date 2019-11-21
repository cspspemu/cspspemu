namespace CSPspEmu.Core.Gpu.Impl.Null
{
    public class GpuImplNull : GpuImpl
    {
        public override PluginInfo PluginInfo => new PluginInfo
        {
            Name = "Null",
            Version = "1.0",
        };

        public override bool IsWorking => true;
    }
}