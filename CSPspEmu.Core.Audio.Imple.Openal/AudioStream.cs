using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using OpenTK;
using OpenTK.Audio.OpenAL;

namespace CSPspEmu.Core.Audio.Imple.Openal
{
	/// <summary>
	/// 48 Samples 1 ms
	/// </summary>
	sealed internal class AudioStream
	{
		public const int SamplesPerMillisecond = 48;
		public const int Frequency = 48000;
		public const int NumberOfBuffers = 2;
		public const int NumberOfChannels = 2;
		public const int SamplesPerBuffer = SamplesPerMillisecond * 20 * NumberOfChannels;
		public int[] BufferIds;
		public int SourceId;
		public ProduceConsumeBuffer<short> ProduceConsumeBuffer = new ProduceConsumeBuffer<short>();
		List<Tuple<long, Action>> Callbacks = new List<Tuple<long, Action>>();

		static void ALEnforce()
		{
			var Error = AL.GetError();
			if (Error != ALError.NoError)
			{
				Console.Error.WriteLine("ALEnforce: " + AL.GetErrorString(Error));
				//throw (new Exception("Error: " + AL.GetErrorString(Error)));
			}
		}

		static T ALEnforce<T>(T Input)
		{
			ALEnforce();
			return Input;
		}

		public AudioStream()
		{
			this.BufferIds = ALEnforce(AL.GenBuffers(NumberOfBuffers));
			this.SourceId = ALEnforce(AL.GenSource());

			var Position = new Vector3(0, 0, 0);
			var Velocity = new Vector3(0, 0, 0);

			AL.Source(SourceId, ALSourcef.Pitch, 1.0f); ALEnforce();
			AL.Source(SourceId, ALSourcef.Gain, 1.0f); ALEnforce();
			AL.Source(SourceId, ALSourcef.RolloffFactor, 0.0f); ALEnforce();
			//AL.Source(SourceId, ALSourceb.Looping, true); ALEnforce();
			AL.Source(SourceId, ALSource3f.Velocity, ref Velocity); ALEnforce();
			AL.Source(SourceId, ALSource3f.Position, ref Position); ALEnforce();
		}

		~AudioStream()
		{
			AL.DeleteSource(this.SourceId); ALEnforce();
			AL.DeleteBuffers(this.BufferIds); ALEnforce();
		}

		public void Start()
		{
			foreach (var BufferId in BufferIds)
			{
				ReadStream(BufferId);
				AL.SourceQueueBuffer(SourceId, BufferId); ALEnforce();
			}
			AL.SourcePlay(SourceId); ALEnforce();
		}

		public void Update()
		{
			int Processed = -1;

			AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out Processed);
			ALEnforce();

			while (Processed-- > 0)
			{
				int BufferId = ALEnforce(AL.SourceUnqueueBuffer(SourceId));
				{
					ReadStream(BufferId);
				}
				AL.SourceQueueBuffer(SourceId, BufferId); ALEnforce();
			}
		}

		private void ReadStream(int BufferId)
		{
			short[] Buffer;
			lock (ProduceConsumeBuffer)
			{
				foreach (var Tuple in Callbacks.Where(Tuple => ProduceConsumeBuffer.TotalConsumed >= Tuple.Item1).ToArray())
				{
					Tuple.Item2();
					Callbacks.Remove(Tuple);
				}
				Buffer = ProduceConsumeBuffer.Consume(SamplesPerBuffer);
			}
			AL.BufferData(BufferId, ALFormat.Stereo16, Buffer, SamplesPerBuffer, Frequency); ALEnforce();
		}

		public void Output(short[] Samples, Action Callback)
		{
			lock (ProduceConsumeBuffer)
			{
				Callbacks.Add(new Tuple<long, Action>(ProduceConsumeBuffer.TotalConsumed + Samples.Length, Callback));
				ProduceConsumeBuffer.Produce(Samples);
			}
		}
	}
}
