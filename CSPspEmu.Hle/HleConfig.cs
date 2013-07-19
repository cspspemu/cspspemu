using CSPspEmu.Core;
using CSPspEmu.Core.Types;
using CSPspEmu.Hle.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CSPspEmu.Hle
{
	public class HleConfig
	{
		[Inject]
		PspStoredConfig PspStoredConfig;

		public bool DebugThreadSwitching = false;
		public bool DebugNotImplemented = true;

		//public PspConfirmButton ConfirmButton;
		public bool WlanIsOn = false;
		public bool UseCoRoutines = false;
		public bool DebugSyscalls = false;

		//public PspVersion FirmwareVersion = new PspVersion(6, 3, 0);
		public PspVersion FirmwareVersion = new PspVersion("6.6.0.0");
		public Assembly HleModulesDll;
		public bool TraceLastSyscalls;

		public PspLanguages Language = HleConfigUtils.CultureInfoToPspLanguage(CultureInfo.InstalledUICulture);
		public PSP_SYSTEMPARAM_ADHOC_CHANNEL AdhocChannel = PSP_SYSTEMPARAM_ADHOC_CHANNEL.AUTOMATIC;
		public PSP_SYSTEMPARAM_WLAN_POWERSAVE WlanPowersave = PSP_SYSTEMPARAM_WLAN_POWERSAVE.ON;
		public PSP_SYSTEMPARAM_BUTTON_PREFERENCE ConfirmButton = PSP_SYSTEMPARAM_BUTTON_PREFERENCE.NA;
		public PSP_SYSTEMPARAM_DATE_FORMAT DateFormat = PSP_SYSTEMPARAM_DATE_FORMAT.YYYYMMDD;
		public PSP_SYSTEMPARAM_TIME_FORMAT TimeFormat = PSP_SYSTEMPARAM_TIME_FORMAT._24HR;
		public PSP_SYSTEMPARAM_DAYLIGHTSAVINGS DaylightSavings = PSP_SYSTEMPARAM_DAYLIGHTSAVINGS.STD;
		public string UserName = Environment.GetEnvironmentVariable("USERNAME");
		
		private HleConfig()
		{
		}
	}

	class HleConfigUtils
	{
		static public PspLanguages CultureInfoToPspLanguage(CultureInfo CultureInfo)
		{
			// http://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
			switch (CultureInfo.TwoLetterISOLanguageName)
			{
				case "ja": return PspLanguages.JAPANESE;
				default:
				case "en": return PspLanguages.ENGLISH;
				case "fr": return PspLanguages.FRENCH;
				case "es": return PspLanguages.SPANISH;
				case "de": return PspLanguages.GERMAN;
				case "it": return PspLanguages.ITALIAN;
				case "nl": return PspLanguages.DUTCH;
				case "pt": return PspLanguages.PORTUGUESE;
				case "ru": return PspLanguages.RUSSIAN;
				case "ko": return PspLanguages.KOREAN;
				case "zh": return PspLanguages.SIMPLIFIED_CHINESE; // TODO: TRADITIONAL_CHINESE
			}
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
		//CIRCLE = 0,
		//CROSS = 1,
	}
}
