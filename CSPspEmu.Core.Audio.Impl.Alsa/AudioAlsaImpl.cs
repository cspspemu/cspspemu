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

		public override void InitializeComponent()
		{
			if (playback_handle == IntPtr.Zero)
			{
				fixed (IntPtr* playback_handle_ptr = &playback_handle)
				fixed (IntPtr* hw_params_ptr = &hw_params)
				{
					int rate = 44100;
					Console.WriteLine(Alsa.snd_pcm_open(playback_handle_ptr, "default", Alsa.snd_pcm_stream_t.SND_PCM_LB_OPEN_PLAYBACK, 0));
					Console.WriteLine(Alsa.snd_pcm_hw_params_malloc(hw_params_ptr));
					Console.WriteLine(Alsa.snd_pcm_hw_params_any(playback_handle, hw_params));

					Console.WriteLine(Alsa.snd_pcm_hw_params_set_access(playback_handle, hw_params, Alsa.snd_pcm_access.SND_PCM_ACCESS_RW_INTERLEAVED));
					Console.WriteLine(Alsa.snd_pcm_hw_params_set_format(playback_handle, hw_params, Alsa.snd_pcm_format.SND_PCM_FORMAT_S16_LE));
					Console.WriteLine(Alsa.snd_pcm_hw_params_set_rate_near(playback_handle, hw_params, &rate, null));
					Console.WriteLine(Alsa.snd_pcm_hw_params_set_channels(playback_handle, hw_params, 2));
					Console.WriteLine(Alsa.snd_pcm_hw_params(playback_handle, hw_params));
					Console.WriteLine(Alsa.snd_pcm_hw_params_free(hw_params));

					/*
					var Data = new byte[16 * 1024];
					for (int n = 0; n < Data.Length; n++)
					{
						Data[n] = (byte)n;
					}
					fixed (byte* DataPtr = Data)
					{
						Alsa.snd_pcm_writei(playback_handle, DataPtr, Data.Length / 4);
					}
					*/
				}
			}

			available_start = Alsa.snd_pcm_avail_update(playback_handle);
		}

		public int available_start;

		//protected short[] Buffer = new short[512];
		short[] Buffer = new short[512];

		public override void Update(Action<short[]> ReadStream)
		{
			/*
			var Data = new byte[16 * 1024];
			for (int n = 0; n < Data.Length; n++)
			{
				Data[n] = (byte)n;
			}
			fixed (byte* DataPtr = Data)
			{
				Alsa.snd_pcm_writei(playback_handle, DataPtr, Data.Length / 4);
			}
			*/

			Console.WriteLine("{0} - {1}", available_start, Alsa.snd_pcm_avail_update(playback_handle));

			//if (available_start - Alsa.snd_pcm_avail_update(playback_handle) < Buffer.Length * 4)
			{
				//Console.WriteLine("aaaaaaaaa");
				ReadStream(Buffer);
				//for (int n = 0; n < Buffer.Length; n++) Console.Write(Buffer[n]);
				fixed (short* BufferPtr = &Buffer[0])
				{
					Alsa.snd_pcm_writei(playback_handle, BufferPtr, Buffer.Length);
					Thread.Sleep(128);
				}
				//throw new NotImplementedException();
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
