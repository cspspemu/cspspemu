using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpUtils;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace CSPspEmu.Core.Audio.Imple.Openal
{
	unsafe public class PspAudioOpenalImpl : IPspAudioImpl
	{
		protected AudioContext AudioContext;
		protected XRamExtension XRam;
		protected Channel[] Channels;
		protected Thread AudioThread;

		public const int NumberOfChannels = 8;
		//public const int NumberOfChannels = 1;

		static void ALEnforce()
		{
			var Error = AL.GetError();
			if (Error != ALError.NoError)
			{
				Console.Error.WriteLine(AL.GetErrorString(Error));
				//throw (new Exception("Error: " + AL.GetErrorString(Error)));
			}
		}

		static T ALEnforce<T>(T Input)
		{
			ALEnforce();
			return Input;
		}

		protected class Channel
		{
			public const int NumberOfBuffers = 10;
			public int Index;
			public int[] BufferIds;
			public int SourceId;

			public Channel(int Index)
			{
				this.Index = Index;
				this.BufferIds = ALEnforce(AL.GenBuffers(NumberOfBuffers));
				this.SourceId = ALEnforce(AL.GenSource());
			}

			~Channel()
			{
				AL.DeleteSource(this.SourceId); ALEnforce();
				AL.DeleteBuffers(this.BufferIds); ALEnforce();
			}

			public void Start()
			{
				var Position = new Vector3(0, 0, 0);
				var Velocity = new Vector3(0, 0, 0);

				AL.Source(SourceId, ALSourcef.Pitch, 1.0f); ALEnforce();
				AL.Source(SourceId, ALSourcef.Gain, 1.0f); ALEnforce();
				AL.Source(SourceId, ALSourcef.RolloffFactor, 0.0f); ALEnforce();
				//AL.Source(SourceId, ALSourceb.Looping, true); ALEnforce();
				AL.Source(SourceId, ALSource3f.Velocity, ref Velocity); ALEnforce();
				AL.Source(SourceId, ALSource3f.Position, ref Position); ALEnforce();

				Play();
			}

			protected void Play()
			{
				foreach (var BufferId in BufferIds)
				{
					ReadStream(BufferId);
					AL.SourceQueueBuffer(SourceId, BufferId);
					ALEnforce();
				}
				//AL.SourceQueueBuffers(SourceId, BufferIds.Length, BufferIds);
				//AL.SourceQueueBuffers(SourceId, BufferIds.Length, BufferIds);
				AL.SourcePlay(SourceId);
				ALEnforce();
			}

			public bool IsPlaying
			{
				get
				{
					int state = -1;
					AL.GetSource(SourceId, ALGetSourcei.SourceState, out state);
					ALEnforce();
					return (state == (int)ALSourceState.Playing);
				}
			}

			public bool Update()
			{
				int Processed = -1;
				bool Active = true;

				AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out Processed);
				ALEnforce();

				while (Processed-- > 0)
				{
					int BufferId = ALEnforce(AL.SourceUnqueueBuffer(SourceId));
					{
						Active = ReadStream(BufferId);
					}
					AL.SourceQueueBuffer(SourceId, BufferId); ALEnforce();
				}

				return Active;
			}

			Queue<Tuple<short[], Action>> SamplesBuffer = new Queue<Tuple<short[], Action>>();

			public bool ReadStream(int BufferId)
			{
				if (SamplesBuffer.Count > 0)
				{
					//Console.WriteLine("#########################################");
					var BufferInfo = SamplesBuffer.Dequeue();
					var Buffer = BufferInfo.Item1;
					var Callback = BufferInfo.Item2;
					AL.BufferData(BufferId, ALFormat.Stereo16, Buffer, Buffer.Length / sizeof(short), 48000); ALEnforce();
					Callback();
				}
				else
				{
					var Buffer = new short[10240 * 2];
					AL.BufferData(BufferId, ALFormat.Stereo16, Buffer, Buffer.Length / sizeof(short), 48000); ALEnforce();
				}

				//Console.WriteLine("Stream");
				/*
				for (int n = 0; n < Buffer.Length; n++)
				{
					Buffer[n] = (short)(Math.Cos((double)n / (double)100) * short.MaxValue);
				}
				*/
				return true;
			}

			public override string ToString()
			{
				return String.Format("Audio.Channel[{0}]: IsPlaying:{1}", Index, IsPlaying);
			}

			internal void Output(short[] Samples, Action Callback)
			{
				//Console.WriteLine("//////////////////////////////////////////////////////");
				SamplesBuffer.Enqueue(new Tuple<short[], Action>(Samples, Callback));
			}
		}

		public PspAudioOpenalImpl()
		{
			AudioContext = new AudioContext();
			XRam = new XRamExtension();

			var Position = new Vector3(0, 0, 0);
			var Velocity = new Vector3(0, 0, 0);
			//var Orientation = new float[] { 0, 0, 1 };
			AL.Listener(ALListener3f.Position, ref Position); ALEnforce();
			AL.Listener(ALListener3f.Velocity, ref Velocity); ALEnforce();
			//AL.Listener(ALListenerfv.Orientation, ref Orientation);

			Channels = new Channel[NumberOfChannels];
			for (int Index = 0; Index < Channels.Length; Index++)
			{
				Channels[Index] = new Channel(Index);
				Channels[Index].Start();
			}

			var ParentThread = Thread.CurrentThread;
			AudioThread = new Thread(() =>
			{
				AudioThreadMain(ParentThread);
			});
			AudioThread.Start();
		}

		protected void AudioThreadMain(Thread ParentThread)
		{
			AudioContext.MakeCurrent();

			while (ParentThread.IsAlive)
			{
				foreach (var Channel in Channels)
				{
					//Console.WriteLine(Channel);
					Channel.Update();
				}
				AudioContext.Process();
				//Thread.Sleep(1);
				Thread.Sleep(20);
			}
		}

		public void OutputStereo16_48000(int ChannelId, short[] Samples, Action Callback)
		{
			//Console.WriteLine("*******************************************************************");
			Channels[ChannelId].Output(Samples, Callback);
		}
	}
}
