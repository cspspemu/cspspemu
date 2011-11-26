using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;

namespace CSPspEmu.Runner
{
	public class PspRunner : PspEmulatorComponent, IRunnableComponent
	{
		readonly public CpuComponentThread CpuThread;
		readonly public GpuComponentThread GpuThread;
		protected List<IRunnableComponent> RunnableComponentList = new List<IRunnableComponent>();

		public bool Paused { get; protected set; }

		public PspRunner(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			CpuThread = PspEmulatorContext.GetInstance<CpuComponentThread>();
			GpuThread = PspEmulatorContext.GetInstance<GpuComponentThread>();

			RunnableComponentList.Add(CpuThread);
			RunnableComponentList.Add(GpuThread);
		}

		public void StartSynchronized()
		{
			RunnableComponentList.ForEach((RunnableComponent) =>
				RunnableComponent.StartSynchronized()
			);
		}

		public void StopSynchronized()
		{
			RunnableComponentList.ForEach((RunnableComponent) =>
				RunnableComponent.StopSynchronized()
			);
		}

		public void PauseSynchronized()
		{
			RunnableComponentList.ForEach((RunnableComponent) =>
				RunnableComponent.PauseSynchronized()
			);
			Paused = true;
		}

		public void ResumeSynchronized()
		{
			RunnableComponentList.ForEach((RunnableComponent) =>
				RunnableComponent.ResumeSynchronized()
			);
			Paused = false;
		}
	}
}
