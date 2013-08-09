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
using System.Linq;
using System.Text;

namespace CSPspEmu.Runner
{
	public class PspInjectContext
	{
		static public InjectContext CreateInjectContext(PspStoredConfig StoredConfig, bool Test)
		{
			var _InjectContext = new InjectContext();
			_InjectContext.SetInstance<PspStoredConfig>(StoredConfig);
			_InjectContext.GetInstance<HleConfig>().HleModulesDll = typeof(HleModulesRoot).Assembly;
			_InjectContext.SetInstanceType<ICpuConnector, HleThreadManager>();
			_InjectContext.SetInstanceType<IGpuConnector, HleThreadManager>();
			_InjectContext.SetInstanceType<IInterruptManager, HleInterruptManager>();

			// Memory
#if true // Disabled because crashes on x86
			if (StoredConfig.UseFastMemory)
			{
				_InjectContext.SetInstanceType<PspMemory, FastPspMemory>();
			}
			else
#endif
			{
				_InjectContext.SetInstanceType<PspMemory, NormalPspMemory>();
			}

			if (!Test)
			{
				// GPU
				PspPluginImpl.SelectWorkingPlugin<GpuImpl>(_InjectContext,
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

				var AudioPlugins = new List<Type>();

				AudioPlugins.Add(typeof(PspAudioOpenalImpl));

				if (Platform.OS == OS.Windows)
				{
					AudioPlugins.Add(typeof(PspAudioWaveOutImpl));
				}
				if (Platform.OS == OS.Linux)
				{
					AudioPlugins.Add(typeof(AudioAlsaImpl));
				}
				AudioPlugins.Add(typeof(AudioImplNull));

				PspPluginImpl.SelectWorkingPlugin<PspAudioImpl>(_InjectContext, AudioPlugins.ToArray());
			}
			else
			{
				_InjectContext.SetInstanceType<GpuImpl, OpenglGpuImpl>();
				_InjectContext.SetInstanceType<PspAudioImpl, AudioImplNull>();
			}

			return _InjectContext;
		}
	}
}
