using System;
using System.Collections.Generic;
using CSPspEmu.Core;
using CSPspEmu.Runner.Components;
using CSPspEmu.Runner.Components.Audio;
using CSPspEmu.Runner.Components.Cpu;
using CSPspEmu.Runner.Components.Display;
using CSPspEmu.Runner.Components.Gpu;

namespace CSPspEmu.Runner
{
	public unsafe class PspRunner : PspEmulatorComponent, IRunnableComponent
	{
		[Inject]
		public CpuComponentThread CpuComponentThread { get; protected set; }
		
		[Inject]
		public GpuComponentThread GpuComponentThread { get; protected set; }

		[Inject]
		public AudioComponentThread AudioComponentThread { get; protected set; }
		
		[Inject]
		public DisplayComponentThread DisplayComponentThread { get; protected set; }

		protected List<IRunnableComponent> RunnableComponentList = new List<IRunnableComponent>();

		public bool Paused { get; protected set; }

		public override void InitializeComponent()
		{
			RunnableComponentList.Add(CpuComponentThread);
			RunnableComponentList.Add(GpuComponentThread);
			RunnableComponentList.Add(AudioComponentThread);
			RunnableComponentList.Add(DisplayComponentThread);
		}

		public void StartSynchronized()
		{
			RunnableComponentList.ForEach((RunnableComponent) =>
				RunnableComponent.StartSynchronized()
			);
		}

		public void StopSynchronized()
		{
			Console.WriteLine("Stopping!");
			RunnableComponentList.ForEach((RunnableComponent) =>
				RunnableComponent.StopSynchronized()
			);
			Console.WriteLine("Stopped!");
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
