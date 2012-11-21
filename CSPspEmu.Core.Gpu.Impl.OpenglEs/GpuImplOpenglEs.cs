using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLES;

namespace CSPspEmu.Core.Gpu.Impl.OpenglEs
{
    unsafe public class GpuImplOpenglEs : GpuImpl
    {
		OffscreenContext OffscreenContext;

		public override void InitSynchronizedOnce()
		{
			this.OffscreenContext = new OffscreenContext(512, 272);
		}

		public override void StopSynchronized()
		{
		}

		public override void Prim(State.GlobalGpuState GlobalGpuState, State.GpuStateStruct* GpuState, State.GuPrimitiveType PrimitiveType, ushort VertexCount)
		{
			//throw new NotImplementedException();
		}

		public override void Finish(State.GpuStateStruct* GpuState)
		{
		}

		public override void End(State.GpuStateStruct* GpuState)
		{
		}

		public override void AddedDisplayList()
		{
		}

		public override void SetCurrent()
		{
			this.OffscreenContext.SetCurrent();
		}

		public override void UnsetCurrent()
		{
		}

		public override PluginInfo PluginInfo
		{
			get {
				return new PluginInfo()
				{
					Name = "OpenglEs",
					Version = "0.1",
				};
			}
		}

		public override bool IsWorking
		{
			get
			{
				return OffscreenContext.IsWorking;
			}
		}
	}
}
