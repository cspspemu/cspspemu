using CSharpPlatform.AL;
using System;

namespace CSPspEmu.Core.Audio.Impl.Openal
{
    unsafe public class PspAudioOpenalImpl : PspAudioImpl
	{
		protected static IntPtr* device;
		protected static IntPtr* context;
		//protected static XRamExtension XRam;
		internal static AudioStream AudioStream;

		public PspAudioOpenalImpl()
		{
		}

		private void InitOnce()
		{
			if (context == null)
			{
				//AudioContext = new AudioContext(AudioContext.DefaultDevice, 44100, 4410);
				//AudioContext = new AudioContext();
				//XRam = new XRamExtension();

				device = AL.alcOpenDevice(AL.alcGetString(null, AL.ALC_DEFAULT_DEVICE_SPECIFIER));
				context = AL.alcCreateContext(device, null);
				AL.alcMakeContextCurrent(context);

				AL.alListener3f(AL.AL_POSITION, 0, 0, 0);
				AL.alListener3f(AL.AL_VELOCITY, 0, 0, 0);

				AudioStream = new AudioStream();
			}
		}

		public override void Update(Action<short[]> ReadStream)
		{
			InitOnce();

			if (context != null)
			{
				AL.alcProcessContext(context);
			}

			AudioStream.Update(ReadStream);
		}

		public override void StopSynchronized()
		{
		}

		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "OpenAl",
					Version = "1.0",
				};
			}
		}
	
		public override bool IsWorking
		{
			get
			{
				try
				{
					AL.alGetError();
					return true;
				}
				catch
				{
					return false;
				}
			}
		}
	}
}
