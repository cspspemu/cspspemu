using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu
{
	public unsafe class GpuImplNull : GpuImpl
	{
		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "Null",
					Version = "1.0",
				};
			}
		}

		public override bool IsWorking
		{
			get { return true; }
		}
	}
}
