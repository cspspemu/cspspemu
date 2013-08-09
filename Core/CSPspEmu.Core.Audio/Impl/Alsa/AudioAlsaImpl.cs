using System;
using CSPspEmu.Core.Audio.Impl.Alsa;

namespace CSPspEmu.Core.Audio
{
	/// <summary>
	/// ALSA Implementation of PSP audio playback
	/// </summary>
	public unsafe class AudioAlsaImpl : PspAudioImpl
	{
		private const string Device = "default";
		//public static string Device = "plughw:0,0";
		//public static string Device = "hw:0,0";
		private static IntPtr playback_handle = IntPtr.Zero;
		private static IntPtr hw_params = IntPtr.Zero;
		private const int channels = 2;
		private const int periods = 2;       /* Number of periods */
		//private const int periodsize = 8192; /* Periodsize (bytes) */
		//private const int periodsize = 2048;

		const int SampleRate = 44100;

		private const int periodsize = SampleRate / 10 / 2;


		//const int NumBuffers = periods;

		//protected short[] Buffer = new short[periodsize / channels / periods];
		protected short[] Buffer = new short[periodsize * channels];
		const int SndOutPacketSize = 512;
		//short[] Buffer = new short[2 * 1024];

		// Frame = 2 * sizeof(ushort)
		// Period = is the number of frames in between each hardware interrupt. The poll() will return once a period.
		// PeriodsPerBuffer
		public AudioAlsaImpl()
		{
			if (playback_handle == IntPtr.Zero)
			{
				try
				{
					fixed (IntPtr* playback_handle_ptr = &playback_handle)
					fixed (IntPtr* hw_params_ptr = &hw_params)
					{
						int rate = SampleRate;
						
						//int period_time = (SndOutPacketSize * 1000) / (SampleRate / 1000);
						//int buffer_time = period_time * NumBuffers;

						Assert("snd_pcm_open", Alsa.snd_pcm_open(playback_handle_ptr, Device, Alsa.snd_pcm_stream_t.SND_PCM_LB_OPEN_PLAYBACK, 0));
						Assert("snd_pcm_hw_params_malloc", Alsa.snd_pcm_hw_params_malloc(hw_params_ptr));
						Assert("snd_pcm_hw_params_any", Alsa.snd_pcm_hw_params_any(playback_handle, hw_params));
						Assert("snd_pcm_hw_params_set_access", Alsa.snd_pcm_hw_params_set_access(playback_handle, hw_params, Alsa.snd_pcm_access.SND_PCM_ACCESS_RW_INTERLEAVED));
						Assert("snd_pcm_hw_params_set_format", Alsa.snd_pcm_hw_params_set_format(playback_handle, hw_params, Alsa.snd_pcm_format.SND_PCM_FORMAT_S16_LE));
						Assert("snd_pcm_hw_params_set_rate_near", Alsa.snd_pcm_hw_params_set_rate_near(playback_handle, hw_params, &rate, null));

						//Assert("snd_pcm_hw_params_set_buffer_time_near", Alsa.snd_pcm_hw_params_set_buffer_time_near(playback_handle, hw_params, &buffer_time, null));
						//Assert("snd_pcm_hw_params_set_period_time_near", Alsa.snd_pcm_hw_params_set_period_time_near(playback_handle, hw_params, &period_time, null));
						 
						Assert("snd_pcm_hw_params_set_channels", Alsa.snd_pcm_hw_params_set_channels(playback_handle, hw_params, channels));

						Assert("snd_pcm_hw_params_set_periods", Alsa.snd_pcm_hw_params_set_periods(playback_handle, hw_params, periods, 0));
						Assert("snd_pcm_hw_params_set_period_size", Alsa.snd_pcm_hw_params_set_period_size(playback_handle, hw_params, periodsize, null));
						Assert("snd_pcm_hw_params_set_buffer_size", Alsa.snd_pcm_hw_params_set_buffer_size(playback_handle, hw_params, 4 * periodsize));

						Assert("snd_pcm_hw_params", Alsa.snd_pcm_hw_params(playback_handle, hw_params));
						Assert("snd_pcm_hw_params_free", Alsa.snd_pcm_hw_params_free(hw_params));
					}
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
			}

			//available_start = Alsa.snd_pcm_avail_update(playback_handle);
		}

		private static void Assert(string Function, int Value)
		{
			Console.WriteLine("Alsa.{0} : {1}", Function, Value);
			//if (Value < 0) throw(new Exception(String.Format("Alsa error({0}) calling function '{1}'", Value, Function)));
		}

		//public int available_start;

		public override void Update(Action<short[]> ReadStream)
		{
			ReadStream(Buffer);
			fixed (short* BufferPtr = &Buffer[0])
			{
				if (Alsa.snd_pcm_state(playback_handle) == Alsa._snd_pcm_state.SND_PCM_STATE_XRUN)
				{
					Alsa.snd_pcm_prepare(playback_handle);
				}
				Alsa.snd_pcm_writei(playback_handle, BufferPtr, Buffer.Length / channels);
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

		public override PluginInfo PluginInfo
		{
			get
			{
				return new PluginInfo()
				{
					Name = "Alsa",
					Version = "0.1",
				};
			}
		}

		// snd_ctl_open 

		public override bool IsWorking
		{
			get
			{
				try
				{
					if (Platform.IsPosix)
					{
						IntPtr temp_playback_handle = IntPtr.Zero;
						var Result = Alsa.snd_pcm_open(&temp_playback_handle, Device, Alsa.snd_pcm_stream_t.SND_PCM_LB_OPEN_PLAYBACK, 0);
						if (Result >= 0)
						{
							Alsa.snd_pcm_close(temp_playback_handle);
							return true;
						}
					}
				}
				catch (Exception Exception)
				{
					Console.Error.WriteLine(Exception);
				}
				return false;
			}
		}
	}
}
