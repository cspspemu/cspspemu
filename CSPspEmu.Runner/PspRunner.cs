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
		readonly public CpuComponentThread CpuComponentThread;
		readonly public GpuComponentThread GpuComponentThread;
		readonly public AudioComponentThread AudioComponentThread;
		protected List<IRunnableComponent> RunnableComponentList = new List<IRunnableComponent>();

		public bool Paused { get; protected set; }

		public PspRunner(PspEmulatorContext PspEmulatorContext) : base(PspEmulatorContext)
		{
			RunnableComponentList.Add(CpuComponentThread = PspEmulatorContext.GetInstance<CpuComponentThread>());
			RunnableComponentList.Add(GpuComponentThread = PspEmulatorContext.GetInstance<GpuComponentThread>());
			RunnableComponentList.Add(AudioComponentThread = PspEmulatorContext.GetInstance<AudioComponentThread>());
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
