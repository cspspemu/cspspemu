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
    public class PspRunner : IRunnableComponent, IInjectInitialize
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

        private PspRunner()
        {
        }

        void IInjectInitialize.Initialize()
        {
            RunnableComponentList.Add(CpuComponentThread);
            RunnableComponentList.Add(GpuComponentThread);
            RunnableComponentList.Add(AudioComponentThread);
            RunnableComponentList.Add(DisplayComponentThread);
        }

        public void StartSynchronized()
        {
            RunnableComponentList.ForEach((runnableComponent) =>
                runnableComponent.StartSynchronized()
            );
        }

        public void StopSynchronized()
        {
            Console.WriteLine("Stopping!");
            RunnableComponentList.ForEach((runnableComponent) =>
                runnableComponent.StopSynchronized()
            );
            Console.WriteLine("Stopped!");
        }

        public void PauseSynchronized()
        {
            RunnableComponentList.ForEach((runnableComponent) =>
            {
                Console.Write("Pausing {0}...", runnableComponent);
                runnableComponent.PauseSynchronized();
                Console.WriteLine("Ok");
            });
            Paused = true;
        }

        public void ResumeSynchronized()
        {
            RunnableComponentList.ForEach((runnableComponent) =>
            {
                Console.Write("Resuming {0}...", runnableComponent);
                runnableComponent.ResumeSynchronized();
                Console.WriteLine("Ok");
            });
            Paused = false;
        }
    }
}