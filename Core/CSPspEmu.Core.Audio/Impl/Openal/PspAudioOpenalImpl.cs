using OpenTK;
using OpenTK.Audio.OpenAL;
using System;

namespace CSPspEmu.Core.Audio.Impl.Openal
{
    unsafe public class PspAudioOpenalImpl : PspAudioImpl
	{
		protected static IntPtr device;
		protected static ContextHandle context;
		//protected static XRamExtension XRam;
		internal static AudioStream AudioStream;

		public PspAudioOpenalImpl()
		{
		}

		private void InitOnce()
		{
			if (AudioStream == null)
			{
				//AudioContext = new AudioContext(AudioContext.DefaultDevice, 44100, 4410);
				//AudioContext = new AudioContext();
				//XRam = new XRamExtension();

				device = Alc.OpenDevice(Alc.GetString(IntPtr.Zero, AlcGetString.DefaultDeviceSpecifier));
				context = Alc.CreateContext(device, new int[] { 0, 0 });
				Alc.MakeContextCurrent(context);

				AL.Listener(ALListener3f.Position, 0f, 0f, 0f);
				AL.Listener(ALListener3f.Velocity, 0f, 0f, 0f);

				AudioStream = new AudioStream();
			}
		}

		public override void Update(Action<short[]> ReadStream)
		{
			InitOnce();
			Alc.ProcessContext(context);
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
					AL.GetError();
					return true;
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
					return false;
				}
			}
		}
	}
}
