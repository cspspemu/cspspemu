using System;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace CSPspEmu.Core.Audio.Impl.Openal
{
    public class PspAudioOpenalImpl : PspAudioImpl
	{
		protected static AudioContext AudioContext;
		//protected static XRamExtension XRam;
		static internal AudioStream AudioStream;

		public override void InitializeComponent()
		{
			if (AudioContext == null)
			{
				//AudioContext = new AudioContext(AudioContext.DefaultDevice, 44100, 4410);
				AudioContext = new AudioContext();
				//XRam = new XRamExtension();

				var Position = new Vector3(0, 0, 0);
				var Velocity = new Vector3(0, 0, 0);
				//var Orientation = new float[] { 0, 0, 1 };
				AL.Listener(ALListener3f.Position, ref Position);
				AL.Listener(ALListener3f.Velocity, ref Velocity);
				//AL.Listener(ALListenerfv.Orientation, ref Orientation);

				AudioStream = new AudioStream();
			}
		}

		public override void Update(Action<short[]> ReadStream)
		{
			AudioContext.Process();
			AudioStream.Update(ReadStream);
		}

		public override void StopSynchronized()
		{
		}

		public override PluginInfo PluginInfo
		{
			get {
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
					return AudioContext.AvailableDevices.Count > 0;
				}
				catch
				{
					return false;
				}
			}
		}
	}
}
