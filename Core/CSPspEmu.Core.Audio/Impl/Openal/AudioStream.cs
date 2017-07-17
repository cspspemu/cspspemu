using CSharpPlatform.AL;
using System;

namespace CSPspEmu.Core.Audio.Impl.Openal
{
    /// <summary>
    /// 48 Samples 1 ms
    /// </summary>
    internal sealed unsafe class AudioStream
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

        private static void AlEnforce(string at = "Unknown")
        {
            var error = AL.alGetError();
            if (error != AL.AL_NO_ERROR)
            {
                Console.Error.WriteLine("ALEnforce: " + AL.alGetErrorString(error) + "(" + error + ") : " + at);
                //throw (new Exception("Error: " + AL.GetErrorString(Error)));
            }
        }

        private static T AlEnforce<T>(T input)
        {
            AlEnforce();
            return input;
        }

        public AudioStream()
        {
            BufferIds = new uint[NumberOfBuffers];

            fixed (uint* bufferIdsPtr = BufferIds)
            fixed (uint* sourceIdPtr = &SourceId)
            {
                AL.alGenBuffers(NumberOfBuffers, bufferIdsPtr);
                AlEnforce();
                AL.alGenSources(1, sourceIdPtr);
                AlEnforce();
            }

            AL.alSourcef(SourceId, AL.AL_PITCH, 1f);
            AlEnforce();
            AL.alSourcef(SourceId, AL.AL_GAIN, 1f);
            AlEnforce();
            AL.alSourcef(SourceId, AL.AL_ROLLOFF_FACTOR, 0f);
            AlEnforce();
            AL.alSource3f(SourceId, AL.AL_VELOCITY, 0f, 0f, 0f);
            AlEnforce();
            AL.alSource3f(SourceId, AL.AL_POSITION, 0f, 0f, 0f);
            AlEnforce();
            //AL.Source(SourceId, ALSourceb.Looping, true); ALEnforce();
        }

        ~AudioStream()
        {
            fixed (uint* sourceIdPtr = &SourceId)
            fixed (uint* bufferIdsPtr = BufferIds)
            {
                AL.alDeleteSources(1, sourceIdPtr);
                AL.alDeleteBuffers(this.BufferIds.Length, bufferIdsPtr);
            }
        }

        private void Start()
        {
            foreach (var bufferId2 in BufferIds)
            {
                var bufferId = bufferId2;
                ReadStream(bufferId);
                AL.alSourceQueueBuffers(SourceId, 1, &bufferId);
                AlEnforce();
            }
            AL.alSourcePlay(SourceId);
            AlEnforce();
        }

        public bool IsPlaying
        {
            get
            {
                int sourceState;
                AL.alGetSourcei(SourceId, AL.AL_SOURCE_STATE, &sourceState);
                return sourceState == AL.AL_PLAYING;
            }
        }

        public void Update(Action<short[]> readStreamCallback)
        {
            var processed = -1;

            fixed (uint* sourceIdPointer = &SourceId)
            {
                if (!IsPlaying)
                {
                    AL.alDeleteSources(1, sourceIdPointer);
                    AlEnforce();
                    AL.alGenSources(1, sourceIdPointer);
                    AlEnforce();
                    //AL.SourceStop(SourceId);
                    Start();
                    //AL.SourcePlay(SourceId); ALEnforce();
                }

                AL.alGetSourcei(SourceId, AL.AL_BUFFERS_PROCESSED, &processed);
                AlEnforce();

                while (processed-- > 0)
                {
                    uint dequeuedBufferId = 0;
                    AL.alSourceUnqueueBuffers(SourceId, 1, &dequeuedBufferId);
                    {
                        ReadStream(dequeuedBufferId, readStreamCallback);
                    }
                    AL.alSourceQueueBuffers(SourceId, 1, &dequeuedBufferId);
                    AlEnforce();
                }
            }
        }

        private short[] _bufferData;

        private void ReadStream(uint bufferId, Action<short[]> readStreamCallback = null)
        {
            //short[] BufferData;

            const int readSamples = SamplesPerBuffer;
            if (_bufferData == null || _bufferData.Length != readSamples)
            {
                _bufferData = new short[readSamples];
                //Console.WriteLine("Created buffer");
            }

            readStreamCallback?.Invoke(_bufferData);
            //if (BufferData.Any(Item => Item != 0)) foreach (var C in BufferData) Console.Write("{0},", C);

            fixed (short* bufferDataPtr = _bufferData)
            {
                AL.alBufferData(bufferId, AL.AL_FORMAT_STEREO16, bufferDataPtr, _bufferData.Length * sizeof(short),
                    Frequency);
                AlEnforce("ReadStream");
            }
        }

        public void StopSynchronized()
        {
        }
    }
}