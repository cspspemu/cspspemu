using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Audio.Impl.Openal;
using CSPspEmu.Core.Audio.Impl.WaveOut;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Modules;
using System;
using System.Collections.Generic;

namespace CSPspEmu.Runner
{
    public class PspInjectContext
    {
        public static InjectContext CreateInjectContext(PspStoredConfig storedConfig, bool test)
        {
            var injectContext = new InjectContext();
            injectContext.SetInstance<PspStoredConfig>(storedConfig);
            injectContext.GetInstance<HleConfig>().HleModulesDll = typeof(HleModulesRoot).Assembly;
            injectContext.SetInstanceType<ICpuConnector, HleThreadManager>();
            injectContext.SetInstanceType<IGpuConnector, HleThreadManager>();
            injectContext.SetInstanceType<IInterruptManager, HleInterruptManager>();

            // Memory
#if true // Disabled because crashes on x86
            if (storedConfig.UseFastMemory)
            {
                injectContext.SetInstanceType<PspMemory, FastPspMemory>();
            }
            else
#endif
            {
                injectContext.SetInstanceType<PspMemory, NormalPspMemory>();
            }

            if (!test)
            {
                // GPU
                PspPluginImpl.SelectWorkingPlugin<GpuImpl>(injectContext,
#if false
					typeof(GpuImplNull)
#else
                    typeof(OpenglGpuImpl),
                    //typeof(GpuImplOpenglEs),
                    //typeof(GpuImplSoft),
                    typeof(GpuImplNull)
#endif
                );

                // AUDIO

                var audioPlugins = new List<Type>
                {
                    typeof(PspAudioOpenalImpl)
                };


                if (Platform.OS == OS.Windows) audioPlugins.Add(typeof(PspAudioWaveOutImpl));
                if (Platform.OS == OS.Linux) audioPlugins.Add(typeof(AudioAlsaImpl));
                audioPlugins.Add(typeof(AudioImplNull));

                PspPluginImpl.SelectWorkingPlugin<PspAudioImpl>(injectContext, audioPlugins.ToArray());
            }
            else
            {
                injectContext.SetInstanceType<GpuImpl, OpenglGpuImpl>();
                injectContext.SetInstanceType<PspAudioImpl, AudioImplNull>();
            }

            return injectContext;
        }
    }
}