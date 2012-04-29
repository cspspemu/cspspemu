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
using CSharpUtils;

namespace CSPspEmu.Runner
{
	unsafe public class PspRunner : PspEmulatorComponent, IRunnableComponent
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
			if (sizeof(uint*) != 4)
			{
				ConsoleUtils.SaveRestoreConsoleColor(ConsoleColor.Red, () =>
				{
					Console.Error.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!");
					Console.Error.WriteLine("WARNING: At the moment the only supported target is 32-bits : " + sizeof(uint*));
					Console.Error.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				});
				//throw(new NotImplementedException("At the moment the only supported target is 32-bits"));
			}

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
