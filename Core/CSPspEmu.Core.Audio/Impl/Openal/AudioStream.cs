using OpenTK.Audio.OpenAL;
using System;

namespace CSPspEmu.Core.Audio.Impl.Openal
{
	/// <summary>
	/// 48 Samples 1 ms
	/// </summary>
	unsafe internal sealed class AudioStream
	{
		public const int Frequency = 44100;
		//public const int Frequency = 48000;

		//public const double SamplesPerMillisecond = ((double)Frequency) / 1000;
		public const double SamplesPerMillisecond = ((double)Frequency) / 500;
		public const int NumberOfBuffers = 4;
		public const int NumberOfChannels = 2;
		public const int BufferMilliseconds = 10;
		public const int SamplesPerBuffer = (int)(SamplesPerMillisecond * BufferMilliseconds * NumberOfChannels);
		public uint[] BufferIds;
		public uint SourceId;

		private static void ALEnforce(string AT = "Unknown")
		{
			var Error = AL.GetError();
			if (Error != ALError.NoError)
			{
				Console.Error.WriteLine("ALEnforce: " + AL.GetErrorString(Error) + "(" + Error + ") : " + AT);
				//throw (new Exception("Error: " + AL.GetErrorString(Error)));
			}
		}

		private static T ALEnforce<T>(T Input)
		{
			ALEnforce();
			return Input;
		}

		public AudioStream()
		{
			this.BufferIds = new uint[NumberOfBuffers];

			fixed (uint* BufferIdsPtr = this.BufferIds)
			{
				AL.GenBuffers(NumberOfBuffers, BufferIdsPtr); ALEnforce();
				AL.GenSources(1, out this.SourceId); ALEnforce();
			}

			AL.Source(SourceId, ALSourcef.Pitch, 1.0f); ALEnforce();
			AL.Source(SourceId, ALSourcef.Gain, 1.0f); ALEnforce();
			AL.Source(SourceId, ALSourcef.RolloffFactor, 0.0f); ALEnforce();
			AL.Source(SourceId, ALSource3f.Velocity, 0f, 0f, 0f); ALEnforce();
			AL.Source(SourceId, ALSource3f.Position, 0f, 0f, 0f); ALEnforce();
			//AL.Source(SourceId, ALSourceb.Looping, true); ALEnforce();
		}

		~AudioStream()
		{
			fixed (uint* SourceIdPtr = &this.SourceId)
			fixed (uint* BufferIdsPtr = this.BufferIds)
			{
				AL.DeleteSources(1, SourceIdPtr);
				AL.DeleteBuffers(this.BufferIds.Length, BufferIdsPtr);
			}
		}

		private void Start()
		{
			foreach (var _BufferId in BufferIds)
			{
				uint BufferId = _BufferId;
				ReadStream(BufferId);
				AL.SourceQueueBuffers(SourceId, 1, &BufferId);
				ALEnforce();
			}
			AL.SourcePlay(SourceId); ALEnforce();
		}

		public bool IsPlaying
		{
			get
			{
				return AL.GetSourceState(SourceId) == ALSourceState.Playing;
			}
		}

		public void Update(Action<short[]> ReadStreamCallback)
		{
			int Processed = -1;

			if (!IsPlaying)
			{
				AL.DeleteSources(1, ref this.SourceId); ALEnforce();
				AL.GenSources(1, out this.SourceId); ALEnforce();
				//AL.SourceStop(SourceId);
				Start();
				//AL.SourcePlay(SourceId); ALEnforce();
			}

			AL.GetSource(SourceId, ALGetSourcei.BuffersProcessed, out Processed);
			ALEnforce();

			while (Processed-- > 0)
			{
				uint DequeuedBufferId = 0;
				AL.SourceUnqueueBuffers(SourceId, 1, &DequeuedBufferId);
				{
					ReadStream(DequeuedBufferId, ReadStreamCallback);
				}
				AL.SourceQueueBuffers(SourceId, 1, &DequeuedBufferId); ALEnforce();
			}
		}

		private short[] BufferData = null;
		private void ReadStream(uint BufferId, Action<short[]> ReadStreamCallback = null)
		{
			//short[] BufferData;

			const int ReadSamples = SamplesPerBuffer;
			if (BufferData == null || BufferData.Length != ReadSamples)
			{
				BufferData = new short[ReadSamples];
				//Console.WriteLine("Created buffer");
			}

			if (ReadStreamCallback != null)
			{
				ReadStreamCallback(BufferData);
				//if (BufferData.Any(Item => Item != 0)) foreach (var C in BufferData) Console.Write("{0},", C);
			}
			else
			{
				//BufferData = new short[ReadSamples];
			}

			fixed (short* BufferDataPtr = BufferData)
			{
				AL.BufferData(BufferId, ALFormat.Stereo16, new IntPtr(BufferDataPtr), BufferData.Length * sizeof(short), Frequency);
				ALEnforce("ReadStream");
			}
		}
	}
}
