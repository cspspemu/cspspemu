using CSPspEmu.Core.Gpu.State;

namespace CSPspEmu.Core.Gpu
{
	public unsafe class GpuImplNull : GpuImpl
	{
		public override void InitSynchronizedOnce()
		{
		}

		public override void StopSynchronized()
		{
		}

		public override void Prim(GlobalGpuState GlobalGpuState, GpuStateStruct* GpuState, GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
		}

		public override void Finish(GpuStateStruct* GpuState)
		{
		}

		public override void End(GpuStateStruct* GpuState)
		{
		}

		public override void AddedDisplayList()
		{
		}

		public override void SetCurrent()
		{
		}

		public override void UnsetCurrent()
		{
		}

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
