using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CSPspEmu.Core;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Runner.Components;
using CSPspEmu.Runner.Components.Audio;
using CSPspEmu.Runner.Components.Cpu;
using CSPspEmu.Runner.Components.Display;
using CSPspEmu.Runner.Components.Gpu;

namespace CSPspEmu.Runner
{
	unsafe public class PspRunner : PspEmulatorComponent, IRunnableComponent
	{
		public CpuComponentThread CpuComponentThread { get; protected set; }
		public GpuComponentThread GpuComponentThread { get; protected set; }
		public AudioComponentThread AudioComponentThread { get; protected set; }
		public DisplayComponentThread DisplayComponentThread { get; protected set; }
		protected List<IRunnableComponent> RunnableComponentList = new List<IRunnableComponent>();

		public bool Paused { get; protected set; }

		public override void InitializeComponent()
		{
			if (sizeof(uint*) != 4)
			{
				throw(new NotImplementedException("At the moment the only supported target is 32-bits"));
			}

			RunnableComponentList.Add(CpuComponentThread = PspEmulatorContext.GetInstance<CpuComponentThread>());
			RunnableComponentList.Add(GpuComponentThread = PspEmulatorContext.GetInstance<GpuComponentThread>());
			RunnableComponentList.Add(AudioComponentThread = PspEmulatorContext.GetInstance<AudioComponentThread>());
			RunnableComponentList.Add(DisplayComponentThread = PspEmulatorContext.GetInstance<DisplayComponentThread>());
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
