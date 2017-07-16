using CSharpPlatform.AL;
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
        public const double SamplesPerMillisecond = ((double) Frequency) / 500;

        public const int NumberOfBuffers = 4;
        public const int NumberOfChannels = 2;
        public const int BufferMilliseconds = 10;
        public const int SamplesPerBuffer = (int) (SamplesPerMillisecond * BufferMilliseconds * NumberOfChannels);
        public uint[] BufferIds;
        public uint SourceId;

        private static void ALEnforce(string AT = "Unknown")
        {
            var Error = AL.alGetError();
            if (Error != AL.AL_NO_ERROR)
            {
                Console.Error.WriteLine("ALEnforce: " + AL.alGetErrorString(Error) + "(" + Error + ") : " + AT);
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
            fixed (uint* SourceIdPtr = &this.SourceId)
            {
                AL.alGenBuffers(NumberOfBuffers, BufferIdsPtr);
                ALEnforce();
                AL.alGenSources(1, SourceIdPtr);
                ALEnforce();
            }

            AL.alSourcef(SourceId, AL.AL_PITCH, 1f);
            ALEnforce();
            AL.alSourcef(SourceId, AL.AL_GAIN, 1f);
            ALEnforce();
            AL.alSourcef(SourceId, AL.AL_ROLLOFF_FACTOR, 0f);
            ALEnforce();
            AL.alSource3f(SourceId, AL.AL_VELOCITY, 0f, 0f, 0f);
            ALEnforce();
            AL.alSource3f(SourceId, AL.AL_POSITION, 0f, 0f, 0f);
            ALEnforce();
            //AL.Source(SourceId, ALSourceb.Looping, true); ALEnforce();
        }

        ~AudioStream()
        {
            fixed (uint* SourceIdPtr = &this.SourceId)
            fixed (uint* BufferIdsPtr = this.BufferIds)
            {
                AL.alDeleteSources(1, SourceIdPtr);
                AL.alDeleteBuffers(this.BufferIds.Length, BufferIdsPtr);
            }
        }

        private void Start()
        {
            foreach (var _BufferId in BufferIds)
            {
                uint BufferId = _BufferId;
                ReadStream(BufferId);
                AL.alSourceQueueBuffers(SourceId, 1, &BufferId);
                ALEnforce();
            }
            AL.alSourcePlay(SourceId);
            ALEnforce();
        }

        public bool IsPlaying
        {
            get
            {
                int SourceState;
                AL.alGetSourcei(SourceId, AL.AL_SOURCE_STATE, &SourceState);
                return SourceState == AL.AL_PLAYING;
            }
        }

        public void Update(Action<short[]> ReadStreamCallback)
        {
            int Processed = -1;

            fixed (uint* SourceIdPointer = &this.SourceId)
            {
                if (!IsPlaying)
                {
                    AL.alDeleteSources(1, SourceIdPointer);
                    ALEnforce();
                    AL.alGenSources(1, SourceIdPointer);
                    ALEnforce();
                    //AL.SourceStop(SourceId);
                    Start();
                    //AL.SourcePlay(SourceId); ALEnforce();
                }

                AL.alGetSourcei(SourceId, AL.AL_BUFFERS_PROCESSED, &Processed);
                ALEnforce();

                while (Processed-- > 0)
                {
                    uint DequeuedBufferId = 0;
                    AL.alSourceUnqueueBuffers(SourceId, 1, &DequeuedBufferId);
                    {
                        ReadStream(DequeuedBufferId, ReadStreamCallback);
                    }
                    AL.alSourceQueueBuffers(SourceId, 1, &DequeuedBufferId);
                    ALEnforce();
                }
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
                AL.alBufferData(BufferId, AL.AL_FORMAT_STEREO16, BufferDataPtr, BufferData.Length * sizeof(short),
                    Frequency);
                ALEnforce("ReadStream");
            }
        }

        public void StopSynchronized()
        {
        }
    }
}