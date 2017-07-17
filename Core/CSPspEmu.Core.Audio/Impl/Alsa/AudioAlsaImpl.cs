using System;

namespace CSPspEmu.Core.Audio.Impl.Alsa
{
    /// <summary>
    /// ALSA Implementation of PSP audio playback
    /// </summary>
    public unsafe class AudioAlsaImpl : PspAudioImpl
    {
        private const string Device = "default";

        //public static string Device = "plughw:0,0";
        //public static string Device = "hw:0,0";
        private static IntPtr _playbackHandle = IntPtr.Zero;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static IntPtr _hwParams = IntPtr.Zero;

        private const int Channels = 2;

        private const int Periods = 2; /* Number of periods */
        //private const int periodsize = 8192; /* Periodsize (bytes) */
        //private const int periodsize = 2048;

        const int SampleRate = 44100;

        private const int Periodsize = SampleRate / 10 / 2;


        //const int NumBuffers = periods;

        //protected short[] Buffer = new short[periodsize / channels / periods];
        protected short[] Buffer = new short[Periodsize * Channels];

        //const int SndOutPacketSize = 512;
        //short[] Buffer = new short[2 * 1024];

        // Frame = 2 * sizeof(ushort)
        // Period = is the number of frames in between each hardware interrupt. The poll() will return once a period.
        // PeriodsPerBuffer
        public AudioAlsaImpl()
        {
            if (_playbackHandle != IntPtr.Zero) return;

            try
            {
                fixed (IntPtr* playbackHandlePtr = &_playbackHandle)
                fixed (IntPtr* hwParamsPtr = &_hwParams)
                {
                    var rate = SampleRate;

                    //int period_time = (SndOutPacketSize * 1000) / (SampleRate / 1000);
                    //int buffer_time = period_time * NumBuffers;

                    Assert("snd_pcm_open",
                        Alsa.snd_pcm_open(playbackHandlePtr, Device,
                            Alsa.snd_pcm_stream_t.SND_PCM_LB_OPEN_PLAYBACK, 0));
                    Assert("snd_pcm_hw_params_malloc", Alsa.snd_pcm_hw_params_malloc(hwParamsPtr));
                    Assert("snd_pcm_hw_params_any", Alsa.snd_pcm_hw_params_any(_playbackHandle, _hwParams));
                    Assert("snd_pcm_hw_params_set_access",
                        Alsa.snd_pcm_hw_params_set_access(_playbackHandle, _hwParams,
                            Alsa.snd_pcm_access.SND_PCM_ACCESS_RW_INTERLEAVED));
                    Assert("snd_pcm_hw_params_set_format",
                        Alsa.snd_pcm_hw_params_set_format(_playbackHandle, _hwParams,
                            Alsa.snd_pcm_format.SND_PCM_FORMAT_S16_LE));
                    Assert("snd_pcm_hw_params_set_rate_near",
                        Alsa.snd_pcm_hw_params_set_rate_near(_playbackHandle, _hwParams, &rate, null));

                    //Assert("snd_pcm_hw_params_set_buffer_time_near", Alsa.snd_pcm_hw_params_set_buffer_time_near(playback_handle, hw_params, &buffer_time, null));
                    //Assert("snd_pcm_hw_params_set_period_time_near", Alsa.snd_pcm_hw_params_set_period_time_near(playback_handle, hw_params, &period_time, null));

                    Assert("snd_pcm_hw_params_set_channels",
                        Alsa.snd_pcm_hw_params_set_channels(_playbackHandle, _hwParams, Channels));

                    Assert("snd_pcm_hw_params_set_periods",
                        Alsa.snd_pcm_hw_params_set_periods(_playbackHandle, _hwParams, Periods, 0));
                    Assert("snd_pcm_hw_params_set_period_size",
                        Alsa.snd_pcm_hw_params_set_period_size(_playbackHandle, _hwParams, Periodsize, null));
                    Assert("snd_pcm_hw_params_set_buffer_size",
                        Alsa.snd_pcm_hw_params_set_buffer_size(_playbackHandle, _hwParams, 4 * Periodsize));

                    Assert("snd_pcm_hw_params", Alsa.snd_pcm_hw_params(_playbackHandle, _hwParams));
                    Assert("snd_pcm_hw_params_free", Alsa.snd_pcm_hw_params_free(_hwParams));
                }
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception);
            }

            //available_start = Alsa.snd_pcm_avail_update(playback_handle);
        }

        private static void Assert(string function, int value)
        {
            Console.WriteLine("Alsa.{0} : {1}", function, value);
            //if (Value < 0) throw(new Exception(String.Format("Alsa error({0}) calling function '{1}'", Value, Function)));
        }

        //public int available_start;

        public override void Update(Action<short[]> readStream)
        {
            readStream(Buffer);
            fixed (short* bufferPtr = &Buffer[0])
            {
                if (Alsa.snd_pcm_state(_playbackHandle) == Alsa._snd_pcm_state.SND_PCM_STATE_XRUN)
                {
                    Alsa.snd_pcm_prepare(_playbackHandle);
                }
                Alsa.snd_pcm_writei(_playbackHandle, bufferPtr, Buffer.Length / Channels);
            }
        }

        public override void StopSynchronized()
        {
            /*
            if (playback_handle != IntPtr.Zero)
            {
                Alsa.snd_pcm_close(playback_handle);
                playback_handle = IntPtr.Zero;
            }
            */
        }

        public override PluginInfo PluginInfo => new PluginInfo
        {
            Name = "Alsa",
            Version = "0.1",
        };

        // snd_ctl_open 

        public override bool IsWorking
        {
            get
            {
                try
                {
                    if (Platform.IsPosix)
                    {
                        var tempPlaybackHandle = IntPtr.Zero;
                        var result = Alsa.snd_pcm_open(&tempPlaybackHandle, Device,
                            Alsa.snd_pcm_stream_t.SND_PCM_LB_OPEN_PLAYBACK, 0);
                        if (result >= 0)
                        {
                            Alsa.snd_pcm_close(tempPlaybackHandle);
                            return true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine(exception);
                }
                return false;
            }
        }
    }
}