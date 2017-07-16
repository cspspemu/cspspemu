using System;
using System.Runtime.InteropServices;

namespace CSPspEmu.Core.Audio.Impl.Alsa
{
	/// <summary>
	/// ALSA P/Invoke class
	/// </summary>
	/// <see cref="http://www.alsa-project.org/alsa-doc/alsa-lib/pcm.html"/>
	/// <see cref="http://www.alsa-project.org/alsa-doc/alsa-lib/group___p_c_m.html"/>
	public unsafe static class Alsa
	{
		private const string Dll = "asound.so.2";

		/// <summary>
		/// PCM stream
		/// </summary>
		internal enum SndPcmStreamT : int
		{
			SND_PCM_LB_OPEN_PLAYBACK = 0,
			SND_PCM_LB_OPEN_RECORD   = 1,
		}

		/// <summary>
		/// Internal structure for a configuration node object.
		/// <para/>
		/// The ALSA library uses a pointer to this structure as a handle to a configuration node. 
		/// <para/>
		/// Applications don't access its contents directly.
		/// </summary>
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

		/// <summary>
		/// PCM access type
		/// </summary>
		internal enum snd_pcm_access 
		{
			SND_PCM_ACCESS_MMAP_INTERLEAVED = 0,
			SND_PCM_ACCESS_MMAP_NONINTERLEAVED,
			SND_PCM_ACCESS_MMAP_COMPLEX,
			SND_PCM_ACCESS_RW_INTERLEAVED,
			SND_PCM_ACCESS_RW_NONINTERLEAVED,
			SND_PCM_ACCESS_LAST = SND_PCM_ACCESS_RW_NONINTERLEAVED 
		}

		/// <summary>
		/// PCM sample formats
		/// </summary>
		internal enum snd_pcm_format
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

		/// <summary>
		/// PCM State
		/// </summary>
		internal enum _snd_pcm_state
		{
			SND_PCM_STATE_OPEN = 0,
			SND_PCM_STATE_SETUP,
			SND_PCM_STATE_PREPARED,
			SND_PCM_STATE_RUNNING,
			SND_PCM_STATE_XRUN,
			SND_PCM_STATE_DRAINING,
			SND_PCM_STATE_PAUSED,
			SND_PCM_STATE_SUSPENDED,
			SND_PCM_STATE_DISCONNECTED,
			SND_PCM_STATE_LAST = SND_PCM_STATE_DISCONNECTED
		}

		// WaveOut calls
		[DllImport(Dll)]
		//[SuppressUnmanagedCodeSecurity]
		internal static extern int snd_pcm_open(IntPtr* playback_handle, string card, SndPcmStreamT device, int mode);

		[DllImport(Dll)]
		internal static extern int snd_pcm_open_lconf(IntPtr* playback_handle, string name, SndPcmStreamT stream, int mode, out snd_config_t lconf);
		
		[DllImport(Dll)]
		internal static extern int snd_pcm_open_preferred(IntPtr* playback_handle, int* rcard, int* rdevice, int mode);

		[DllImport(Dll)]
		internal static extern int snd_pcm_playback_format(IntPtr playback_handle, ref snd_pcm_format format);

		/// <summary>
		/// Prepare PCM for use.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <returns>0 upon success, otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_prepare(IntPtr playback_handle);

		/// <summary>
		/// Start a PCM.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <returns>0 upon success otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_start(IntPtr playback_handle);

		/// <summary>
		/// Write interleaved frames to a PCM.
		/// </summary>
		/// <param name="playback_handle">PCM frame</param>
		/// <param name="buffer">Frames containing buffer</param>
		/// <param name="size">Frames to be written</param>
		/// <returns>A positive number of frames actually written, otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_writei(IntPtr playback_handle, void* buffer, int size);

		/// <summary>
		/// Close PCM handle.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <returns>0 upon success, otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_close(IntPtr playback_handle);

		/// <summary>
		/// Allocate an invalid snd_pcm_hw_params_t using standard malloc
		/// </summary>
		/// <param name="hw_params">Returned pointer</param>
		/// <returns>0 upon success, otherwise negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_malloc(IntPtr* hw_params);

		/// <summary>
		/// Fill params with a full configuration space for a PCM.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space</param>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_any(IntPtr playback_handle, IntPtr hw_params);
	

		/// <summary>
		/// Restrict a configuration space to contain only one access type.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space</param>
		/// <param name="snd_pcm_access">Access type</param>
		/// <returns>0, otherwise a negative error code if configuration space would become empty</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_access(IntPtr playback_handle, IntPtr hw_params, snd_pcm_access snd_pcm_access);
		
		/// <summary>
		/// Restrict a configuration space to contain only one format.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space</param>
		/// <param name="snd_pcm_format">Format</param>
		/// <returns>0, otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_format(IntPtr playback_handle, IntPtr hw_params, snd_pcm_format snd_pcm_format);
		
		/// <summary>
		/// Restrict a configuration space to contain only one channels count.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space</param>
		/// <param name="p">Channel count</param>
		/// <returns>0, otherwise a negative error code if configuration space would become empty</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_channels(IntPtr playback_handle, IntPtr hw_params, int p);
		
		/// <summary>
		/// Install one PCM hardware configuration chosen from a configuration space and snd_pcm_prepare it.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space definition container</param>
		/// <returns>0 upon success, otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params(IntPtr playback_handle, IntPtr hw_params);
		
		/// <summary>
		/// Frees a previously allocated snd_pcm_hw_params_t
		/// </summary>
		/// <param name="hw_params">Pointer to object to free</param>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_free(IntPtr hw_params);
		
		/// <summary>
		/// Restrict a configuration space to have rate nearest to a target.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space</param>
		/// <param name="val">Approximate target rate / returned approximate set rate</param>
		/// <param name="dir">Sub unit direction</param>
		/// <returns>0, otherwise a negative error code if configuration space is empty</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_rate_near(IntPtr playback_handle, IntPtr hw_params, int* val, void* dir);


		/// <summary>
		/// Return number of frames ready to be read (capture) / written (playback)
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <returns>A positive number of frames ready, otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_avail(IntPtr playback_handle);

		/// <summary>
		/// Return number of frames ready to be read (capture) / written (playback)
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <returns>A positive number of frames ready, otherwise a negative error code</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_avail_update(IntPtr playback_handle);

		/// <summary>
		/// Restrict a configuration space to contain only one periods count.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space</param>
		/// <param name="periods">Approximate periods per buffer</param>
		/// <param name="dir">Sub unit direction</param>
		/// <returns>0, otherwise a negative error code if configuration space would become empty</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_periods(IntPtr playback_handle, IntPtr hw_params, int periods, int dir);
		
		/// <summary>
		/// Restrict a configuration space to contain only one buffer size.
		/// </summary>
		/// <param name="playback_handle">PCM handle</param>
		/// <param name="hw_params">Configuration space</param>
		/// <param name="val">Buffer size in frames</param>
		/// <returns>0, otherwise a negative error code if configuration space would become empty</returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_buffer_size(IntPtr playback_handle, IntPtr hw_params, int val);

		/// <summary>
		/// Return PCM state.
		/// </summary>
		/// <param name="playback_handle">PCM Handle</param>
		/// <returns>PCM state snd_pcm_state_t of given PCM handle</returns>
		[DllImport(Dll)]
		internal static extern _snd_pcm_state snd_pcm_state(IntPtr playback_handle);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="playback_handle"></param>
		/// <param name="hw_params"></param>
		/// <param name="val"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_buffer_time_near(IntPtr playback_handle, IntPtr hw_params, int* val, int* dir);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="playback_handle"></param>
		/// <param name="hw_params"></param>
		/// <param name="val"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_period_time_near(IntPtr playback_handle, IntPtr hw_params, int* val, int* dir);


		/// <summary>
		/// 
		/// </summary>
		/// <param name="playback_handle"></param>
		/// <param name="hw_params"></param>
		/// <param name="val"></param>
		/// <param name="dir"></param>
		/// <returns></returns>
		[DllImport(Dll)]
		internal static extern int snd_pcm_hw_params_set_period_size(IntPtr playback_handle, IntPtr hw_params, int val, int* dir);
	}
}
