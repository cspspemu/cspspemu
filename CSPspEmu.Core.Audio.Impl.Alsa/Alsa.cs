using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Audio.Impl.Alsa
{
	/// <summary>
	/// 
	/// </summary>
	/// <see cref="http://www.alsa-project.org/alsa-doc/alsa-lib/pcm.html"/>
	/// <see cref="http://www.alsa-project.org/alsa-doc/alsa-lib/group___p_c_m.html"/>
	public unsafe static class Alsa
	{
		private const string DLL = "asound.so.2";

		public enum snd_pcm_stream_t : int
		{
			SND_PCM_LB_OPEN_PLAYBACK = 0,
			SND_PCM_LB_OPEN_RECORD   = 1,
		}

		public struct snd_config_t
		{
			private fixed byte data[4 * 1024];
		}

		/*
		public struct snd_pcm_format
		{
			/// <summary>
			/// Bool
			/// </summary>
			public int interleave;
			public int format;
			public int rate;
			public int voices;
			public int special;

			/// <summary>
			/// must be filled with zeroes
			/// </summary>
			public fixed byte reserved[124];
		}
		*/

		public enum snd_pcm_access 
		{
			SND_PCM_ACCESS_MMAP_INTERLEAVED = 0, SND_PCM_ACCESS_MMAP_NONINTERLEAVED, SND_PCM_ACCESS_MMAP_COMPLEX, SND_PCM_ACCESS_RW_INTERLEAVED, 
			SND_PCM_ACCESS_RW_NONINTERLEAVED, SND_PCM_ACCESS_LAST = SND_PCM_ACCESS_RW_NONINTERLEAVED 
		}

		public enum snd_pcm_format
		{
			SND_PCM_FORMAT_UNKNOWN = -1,
			SND_PCM_FORMAT_S8 = 0,
			SND_PCM_FORMAT_U8,
			SND_PCM_FORMAT_S16_LE,
			SND_PCM_FORMAT_S16_BE,
			SND_PCM_FORMAT_U16_LE,
			SND_PCM_FORMAT_U16_BE,
			SND_PCM_FORMAT_S24_LE,
			SND_PCM_FORMAT_S24_BE,
			SND_PCM_FORMAT_U24_LE,
			SND_PCM_FORMAT_U24_BE,
			SND_PCM_FORMAT_S32_LE,
			SND_PCM_FORMAT_S32_BE,
			SND_PCM_FORMAT_U32_LE,
			SND_PCM_FORMAT_U32_BE,
			SND_PCM_FORMAT_FLOAT_LE,
			SND_PCM_FORMAT_FLOAT_BE,
			SND_PCM_FORMAT_FLOAT64_LE,
			SND_PCM_FORMAT_FLOAT64_BE,
			SND_PCM_FORMAT_IEC958_SUBFRAME_LE,
			SND_PCM_FORMAT_IEC958_SUBFRAME_BE,
			SND_PCM_FORMAT_MU_LAW,
			SND_PCM_FORMAT_A_LAW,
			SND_PCM_FORMAT_IMA_ADPCM,
			SND_PCM_FORMAT_MPEG,
			SND_PCM_FORMAT_GSM,
			SND_PCM_FORMAT_SPECIAL = 31,
			SND_PCM_FORMAT_S24_3LE = 32,
			SND_PCM_FORMAT_S24_3BE,
			SND_PCM_FORMAT_U24_3LE,
			SND_PCM_FORMAT_U24_3BE,
			SND_PCM_FORMAT_S20_3LE,
			SND_PCM_FORMAT_S20_3BE,
			SND_PCM_FORMAT_U20_3LE,
			SND_PCM_FORMAT_U20_3BE,
			SND_PCM_FORMAT_S18_3LE,
			SND_PCM_FORMAT_S18_3BE,
			SND_PCM_FORMAT_U18_3LE,
			SND_PCM_FORMAT_U18_3BE,
			SND_PCM_FORMAT_LAST = SND_PCM_FORMAT_U18_3BE,
		}

		public enum _snd_pcm_state
		{
			SND_PCM_STATE_OPEN = 0, SND_PCM_STATE_SETUP, SND_PCM_STATE_PREPARED, SND_PCM_STATE_RUNNING,
			SND_PCM_STATE_XRUN, SND_PCM_STATE_DRAINING, SND_PCM_STATE_PAUSED, SND_PCM_STATE_SUSPENDED,
			SND_PCM_STATE_DISCONNECTED, SND_PCM_STATE_LAST = SND_PCM_STATE_DISCONNECTED
		}

		// WaveOut calls
		[DllImport(DLL)]
		//[SuppressUnmanagedCodeSecurity]
		public static extern int snd_pcm_open(IntPtr* playback_handle, string card, snd_pcm_stream_t device, int mode);

		[DllImport(DLL)]
		public static extern int snd_pcm_open_lconf(IntPtr* playback_handle, string name, snd_pcm_stream_t stream, int mode, out snd_config_t lconf);
		
		[DllImport(DLL)]
		public static extern int snd_pcm_open_preferred(IntPtr* playback_handle, int* rcard, int* rdevice, int mode);

		[DllImport(DLL)]
		public static extern int snd_pcm_playback_format(IntPtr playback_handle, ref snd_pcm_format format);

		[DllImport(DLL)]
		public static extern int snd_pcm_prepare(IntPtr playback_handle);

		/// <summary>
		/// Start a PCM.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <returns>0 on success otherwise a negative error code</returns>
		[DllImport(DLL)]
		public static extern int snd_pcm_start(IntPtr playback_handle);

		[DllImport(DLL)]
		public static extern int snd_pcm_writei(IntPtr playback_handle, void* buffer, int size);

		[DllImport(DLL)]
		public static extern int snd_pcm_close(IntPtr playback_handle);

		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_malloc(IntPtr* hw_params);

		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_any(IntPtr playback_handle, IntPtr hw_params);
	
		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_set_access(IntPtr playback_handle,IntPtr hw_params,snd_pcm_access snd_pcm_access);
		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_set_format(IntPtr playback_handle,IntPtr hw_params,snd_pcm_format snd_pcm_format);
		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_set_channels(IntPtr playback_handle,IntPtr hw_params,int p);
		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params(IntPtr playback_handle,IntPtr hw_params);
		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_free(IntPtr hw_params);
		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_set_rate_near(IntPtr playback_handle,IntPtr hw_params,int* p, void* p_2);

		[DllImport(DLL)]
		public static extern int snd_pcm_avail(IntPtr playback_handle);
		[DllImport(DLL)]
		public static extern int snd_pcm_avail_update(IntPtr playback_handle);

		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_set_periods(IntPtr playback_handle, IntPtr hw_params, int periods, int p);
		[DllImport(DLL)]
		public static extern int snd_pcm_hw_params_set_buffer_size(IntPtr playback_handle, IntPtr hw_params, int p);

		[DllImport(DLL)]
		public static extern _snd_pcm_state snd_pcm_state(IntPtr playback_handle);
	}
}
