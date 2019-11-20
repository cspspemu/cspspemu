using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Cpu;
using CSPspEmu.Core.Gpu;
using CSPspEmu.Core.Gpu.Impl.Opengl;
using CSPspEmu.Core.Memory;
using CSPspEmu.Hle;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Modules;
using System;
using System.Collections.Generic;
using CSPspEmu.Core.Audio.Impl.Alsa;
using CSPspEmu.Core.Audio.Impl.Null;
using CSPspEmu.Core.Audio.Impl.Openal;
using CSPspEmu.Core.Audio.Impl.WaveOut;
using CSPspEmu.Core.Gpu.Impl.Null;
using CSPspEmu.Core.Gpu.Impl.Soft;

namespace CSPspEmu.Runner
{
    public class PspInjectContext
    {
        public static InjectContext CreateInjectContext(PspStoredConfig storedConfig, bool test, Action<InjectContext>? configure = null)
        {
            var injectContext = new InjectContext();
            configure?.Invoke(injectContext);
            injectContext.SetInstance<PspStoredConfig>(storedConfig);
            injectContext.GetInstance<HleConfig>().HleModulesDll = typeof(HleModulesRoot).Assembly;
            injectContext.SetInstanceType<ICpuConnector, HleThreadManager>();
            injectContext.SetInstanceType<IGpuConnector, HleThreadManager>();
            injectContext.SetInstanceType<IInterruptManager, HleInterruptManager>();
            injectContext.SetInstanceType<PspMemory, FastPspMemory>();

            if (!test)
            {
                // GPU
                PspPluginImpl.SelectWorkingPlugin<GpuImpl>(injectContext,
                    typeof(OpenglGpuImpl),
                    typeof(GpuImplSoft),
                    typeof(GpuImplNull)
                );

                // AUDIO

                var audioPlugins = new List<Type>();

                //if (Platform.OS == OS.Windows) audioPlugins.Add(typeof(PspAudioWaveOutImpl));
                //if (Platform.OS == OS.Linux) audioPlugins.Add(typeof(AudioAlsaImpl));
                //audioPlugins.Add(typeof(PspAudioOpenalImpl));
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