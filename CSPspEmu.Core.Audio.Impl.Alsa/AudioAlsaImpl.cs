using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Core;
using CSPspEmu.Core.Audio;
using CSPspEmu.Core.Audio.Impl.Alsa;
using System.Threading;

namespace CSPspEmu.Core.Audio
{
	unsafe public class AudioAlsaImpl : PspAudioImpl
	{
		static IntPtr playback_handle = IntPtr.Zero;
		static IntPtr hw_params = IntPtr.Zero;
		int periods = 2;       /* Number of periods */
		int periodsize = 8192; /* Periodsize (bytes) */

		public override void InitializeComponent()
		{
			if (playback_handle == IntPtr.Zero)
			{
				fixed (IntPtr* playback_handle_ptr = &playback_handle)
				fixed (IntPtr* hw_params_ptr = &hw_params)
				{
					int rate = 44100;
					Assert(Alsa.snd_pcm_open(playback_handle_ptr, "default", Alsa.snd_pcm_stream_t.SND_PCM_LB_OPEN_PLAYBACK, 0));
					Assert(Alsa.snd_pcm_hw_params_malloc(hw_params_ptr));
					Assert(Alsa.snd_pcm_hw_params_any(playback_handle, hw_params));

					Assert(Alsa.snd_pcm_hw_params_set_access(playback_handle, hw_params, Alsa.snd_pcm_access.SND_PCM_ACCESS_RW_INTERLEAVED));
					Assert(Alsa.snd_pcm_hw_params_set_format(playback_handle, hw_params, Alsa.snd_pcm_format.SND_PCM_FORMAT_S16_LE));
					Assert(Alsa.snd_pcm_hw_params_set_rate_near(playback_handle, hw_params, &rate, null));
					Assert(Alsa.snd_pcm_hw_params_set_channels(playback_handle, hw_params, 2));
					Assert(Alsa.snd_pcm_hw_params_set_periods(playback_handle, hw_params, periods, 0));
					Assert(Alsa.snd_pcm_hw_params_set_buffer_size(playback_handle, hw_params, (periodsize * periods) >> 2));

					Assert(Alsa.snd_pcm_hw_params(playback_handle, hw_params));
					Assert(Alsa.snd_pcm_hw_params_free(hw_params));
				}
			}

			//available_start = Alsa.snd_pcm_avail_update(playback_handle);
		}

		static private void Assert(int Value)
		{
			//if (Value < 0) throw(new Exception("Alsa error"));
		}

		//public int available_start;

		protected short[] Buffer = new short[512];
		//short[] Buffer = new short[2 * 1024];

		public override void Update(Action<short[]> ReadStream)
		{
			ReadStream(Buffer);
			fixed (short* BufferPtr = &Buffer[0])
			{
				Alsa.snd_pcm_writei(playback_handle, BufferPtr, Buffer.Length / 2);
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

		public override bool IsWorking
		{
			get { return Platform.OperatingSystem == Platform.OS.Posix; }
		}
	}
}
