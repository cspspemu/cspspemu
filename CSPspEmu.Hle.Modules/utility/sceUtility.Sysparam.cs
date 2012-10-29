using System;
using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.utility
{
	public unsafe partial class sceUtility
	{
		private static int _sceUtilityGetSystemParamInt(PSP_SYSTEMPARAM_ID id)
		{
			switch (id)
			{
				case PSP_SYSTEMPARAM_ID.INT_ADHOC_CHANNEL: return (int)PSP_SYSTEMPARAM_ADHOC_CHANNEL.AUTOMATIC;
				case PSP_SYSTEMPARAM_ID.INT_WLAN_POWERSAVE: return (int)PSP_SYSTEMPARAM_WLAN_POWERSAVE.ON;
				case PSP_SYSTEMPARAM_ID.INT_DATE_FORMAT: return (int)PSP_SYSTEMPARAM_DATE_FORMAT.YYYYMMDD; // English
				case PSP_SYSTEMPARAM_ID.INT_TIME_FORMAT: return (int)PSP_SYSTEMPARAM_TIME_FORMAT._24HR;
				case PSP_SYSTEMPARAM_ID.INT_TIMEZONE: return -5 * 60;
				case PSP_SYSTEMPARAM_ID.INT_DAYLIGHTSAVINGS: return (int)PSP_SYSTEMPARAM_DAYLIGHTSAVINGS.STD;
				case PSP_SYSTEMPARAM_ID.INT_LANGUAGE: return (int)Language.English;
				case PSP_SYSTEMPARAM_ID.INT_BUTTON_PREFERENCE: return (int)PSP_SYSTEMPARAM_BUTTON_PREFERENCE.NA;
				default: throw (new SceKernelException(unchecked((SceKernelErrors)PSP_SYSTEMPARAM_RETVAL.FAIL)));
			}
		}

		private static string _sceUtilityGetSystemParamString(PSP_SYSTEMPARAM_ID id)
		{
			switch (id)
			{
				case PSP_SYSTEMPARAM_ID.STRING_NICKNAME: return Environment.GetEnvironmentVariable("USERNAME");
				default: throw (new SceKernelException(unchecked((SceKernelErrors)PSP_SYSTEMPARAM_RETVAL.FAIL)));
			}
		}

		/// <summary>
		/// Get Integer System Parameter
		/// </summary>
		/// <param name="id">Which parameter to get</param>
		/// <param name="value">Pointer to integer value to place result in</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		[HlePspFunction(NID = 0xA5DA2406, FirmwareVersion = 150)]
		public PSP_SYSTEMPARAM_RETVAL sceUtilityGetSystemParamInt(PSP_SYSTEMPARAM_ID id, int* value)
		{
			*value = _sceUtilityGetSystemParamInt(id);
			return PSP_SYSTEMPARAM_RETVAL.OK;
		}

		/// <summary>
		/// Get String System Parameter
		/// </summary>
		/// <param name="id">Which parameter to get</param>
		/// <param name="str">Char * buffer to place result in</param>
		/// <param name="len">Length of str buffer</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		[HlePspFunction(NID = 0x34B78343, FirmwareVersion = 150)]
		public PSP_SYSTEMPARAM_RETVAL sceUtilityGetSystemParamString(PSP_SYSTEMPARAM_ID id, byte* str, int len)
		{
			var Value = _sceUtilityGetSystemParamString(id);
			PointerUtils.StoreStringOnPtr(Value, Encoding.ASCII, str, len);
			return PSP_SYSTEMPARAM_RETVAL.OK;
		}

		/// <summary>
		/// Set Integer System Parameter
		/// </summary>
		/// <param name="id">Which parameter to set</param>
		/// <param name="value">Integer value to set</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		public PSP_SYSTEMPARAM_RETVAL sceUtilitySetSystemParamInt(PSP_SYSTEMPARAM_ID id, int value)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Set String System Parameter
		/// </summary>
		/// <param name="id">Which parameter to set</param>
		/// <param name="str">char* value to set</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		public PSP_SYSTEMPARAM_RETVAL sceUtilitySetSystemParamString(PSP_SYSTEMPARAM_ID id, string str)
		{
			throw(new NotImplementedException());
		}

	}

	/// <summary>
	/// IDs for use inSystemParam functions
	/// PSP_SYSTEMPARAM_ID_INT    are for use with SystemParamInt    funcs
	/// PSP_SYSTEMPARAM_ID_STRING are for use with SystemParamString funcs
	/// </summary>
	public enum PSP_SYSTEMPARAM_ID
	{
		STRING_NICKNAME = 1,
		INT_ADHOC_CHANNEL = 2,
		INT_WLAN_POWERSAVE = 3,
		INT_DATE_FORMAT = 4,
		INT_TIME_FORMAT = 5,
		INT_TIMEZONE = 6, // Timezone offset from UTC in minutes, (EST = -300 = -5 * 60)
		INT_DAYLIGHTSAVINGS = 7,
		INT_LANGUAGE = 8,
		INT_BUTTON_PREFERENCE = 9,
	}

	/// <summary>
	/// Return values for the SystemParam functions
	/// </summary>
	public enum PSP_SYSTEMPARAM_RETVAL : uint
	{
		OK = 0,
		FAIL = 0x80110103,
	}

	/// <summary>
	/// Valid values for PSP_SYSTEMPARAM_ID_INT_ADHOC_CHANNEL
	/// </summary>
	public enum PSP_SYSTEMPARAM_ADHOC_CHANNEL
	{
		AUTOMATIC = 0,
		C1 = 1,
		C6 = 6,
		C11 = 11,
	}

	/// <summary>
	/// Valid values for PSP_SYSTEMPARAM_ID_INT_WLAN_POWERSAVE
	/// </summary>
	public enum PSP_SYSTEMPARAM_WLAN_POWERSAVE
	{
		OFF = 0,
		ON = 1,
	}

	/// <summary>
	/// Valid values for PSP_SYSTEMPARAM_ID_INT_DATE_FORMAT
	/// </summary>
	public enum PSP_SYSTEMPARAM_DATE_FORMAT
	{
		YYYYMMDD = 0,
		MMDDYYYY = 1,
		DDMMYYYY = 2,
	}

	/// <summary>
	/// Valid values for PSP_SYSTEMPARAM_ID_INT_TIME_FORMAT
	/// </summary>
	public enum PSP_SYSTEMPARAM_TIME_FORMAT
	{
		_24HR = 0,
		_12HR = 1,
	}

	/// <summary>
	/// Valid values for PSP_SYSTEMPARAM_ID_INT_DAYLIGHTSAVINGS
	/// </summary>
	public enum PSP_SYSTEMPARAM_DAYLIGHTSAVINGS
	{
		STD = 0,
		SAVING = 1,
	}

	/// <summary>
	/// Valid values for PSP_SYSTEMPARAM_ID_INT_LANGUAGE
	/// </summary>
	public enum PSP_SYSTEMPARAM_LANGUAGE
	{
		JAPANESE = 0,
		ENGLISH = 1,
		FRENCH = 2,
		SPANISH = 3,
		GERMAN = 4,
		ITALIAN = 5,
		DUTCH = 6,
		PORTUGUESE = 7,
		RUSSIAN = 8,
		KOREAN = 9,
		CHINESE_TRADITIONAL = 10,
		CHINESE_SIMPLIFIED = 11,
	}

	/// <summary>
	/// #9 seems to be Region or maybe X/O button swap.
	/// It doesn't exist on JAP v1.0
	/// is 1 on NA v1.5s
	/// is 0 on JAP v1.5s
	/// is read-only
	/// </summary>
	public enum PSP_SYSTEMPARAM_BUTTON_PREFERENCE
	{
		JAP = 0,
		NA = 1,
	}
}
