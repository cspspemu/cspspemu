using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using CSharpUtils;
using CSPspEmu.Core.Audio.Impl.WaveOut.WaveLib;

namespace CSPspEmu.Core.Audio.Impl.WaveOut
{
    public unsafe class PspAudioWaveOutImpl : PspAudioImpl, IInjectInitialize
    {
        public const int Frequency = 44100;
        //public const int Frequency = 48000;

        //public const double SamplesPerMillisecond = ((double)Frequency) / 1000;
        public const double SamplesPerMillisecond = ((double) Frequency) / 500;

        //public const double SamplesPerMillisecond = ((double)Frequency) / 100;
        //public const int BufferSize = 16384;
        public const int NumberOfBuffers = 4;

        public const int NumberOfChannels = 2;

        //public const int BufferMilliseconds = 10;
        public const int BufferMilliseconds = 20;

        public const int SamplesPerBuffer = (int) (SamplesPerMillisecond * BufferMilliseconds * NumberOfChannels);
        public const int BufferSize = (int) SamplesPerBuffer;

        private bool _initialized = false;
        private static IntPtr _waveOutHandle = IntPtr.Zero;

        private ConcurrentQueue<short[]> Queue = new ConcurrentQueue<short[]>();

        private WaveOutPlayer _mPlayer;


        private void BufferFillEventHandler(IntPtr data, int size)
        {
            if (Queue.Count > 0)
            {
                short[] Result;
                while (!Queue.TryDequeue(out Result))
                {
                    if (_mPlayer.Disposing) return;
                }
                Marshal.Copy(Result, 0, data, size / 2);
            }
            else
            {
                PointerUtils.Memset((byte*) data.ToPointer(), 0, size);
            }
        }

        public override void Update(Action<short[]> readStream)
        {
            if (_initialized)
            {
                while (Queue.Count < 2)
                {
                    var data = new short[BufferSize / 2];
                    readStream(data);
                    //for (int n = 0; n < Data.Length; n++) Console.Write(Data[n]);
                    Queue.Enqueue(data);
                }
            }
        }

        public override void StopSynchronized()
        {
            //Console.ReadKey();
#if false
			Initialized = false;
			if (m_Player != null)
			{
				m_Player.Stop();
				m_Player = null;
			}
#endif
        }

        void IInjectInitialize.Initialize()
        {
            Console.WriteLine("PspAudioWaveOutImpl.Initialize()!");
            if (_mPlayer == null)
            {
                _mPlayer = new WaveOutPlayer(-1, new WaveFormat(rate: Frequency, bits: 16, channels: NumberOfChannels),
                    BufferSize, NumberOfBuffers, BufferFillEventHandler);
            }
            _initialized = true;
        }

        public override PluginInfo PluginInfo => new PluginInfo
        {
            Name = "WaveOut",
            Version = "1.0",
        };

        public override bool IsWorking
        {
            get
            {
                if (Platform.OS != OS.Windows) return false;
                if (WaveOutPlayer.DeviceCount == 0) return false;
                return true;
            }
        }
    }
}