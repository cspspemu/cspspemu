using System;
using System.Text;
using CSharpUtils;
using CSPspEmu.Hle.Managers;

namespace CSPspEmu.Hle.Modules.utility
{
	public unsafe partial class sceUtility
	{
		[Inject]
		HleConfig HleConfig;

		private int _sceUtilityGetSystemParamInt(PSP_SYSTEMPARAM_ID id)
		{
			switch (id)
			{
				case PSP_SYSTEMPARAM_ID.INT_ADHOC_CHANNEL: return (int)HleConfig.AdhocChannel;
				case PSP_SYSTEMPARAM_ID.INT_WLAN_POWERSAVE: return (int)HleConfig.WlanPowersave;
				case PSP_SYSTEMPARAM_ID.INT_DATE_FORMAT: return (int)HleConfig.DateFormat;
				case PSP_SYSTEMPARAM_ID.INT_TIME_FORMAT: return (int)HleConfig.TimeFormat;
				case PSP_SYSTEMPARAM_ID.INT_TIMEZONE: return -5 * 60;
				case PSP_SYSTEMPARAM_ID.INT_DAYLIGHTSAVINGS: return (int)HleConfig.DaylightSavings;
				case PSP_SYSTEMPARAM_ID.INT_LANGUAGE: return (int)HleConfig.Language;
				case PSP_SYSTEMPARAM_ID.INT_BUTTON_PREFERENCE: return (int)HleConfig.ConfirmButton;
				default: throw (new SceKernelException(SceKernelErrors.PSP_SYSTEMPARAM_RETVAL));
			}
		}

		private string _sceUtilityGetSystemParamString(PSP_SYSTEMPARAM_ID id)
		{
			switch (id)
			{
				case PSP_SYSTEMPARAM_ID.STRING_NICKNAME: return HleConfig.UserName;
				default: throw (new SceKernelException(SceKernelErrors.PSP_SYSTEMPARAM_RETVAL));
			}
		}

		/// <summary>
		/// Get Integer System Parameter
		/// </summary>
		/// <param name="id">Which parameter to get</param>
		/// <param name="value">Pointer to integer value to place result in</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		[HlePspFunction(NID = 0xA5DA2406, FirmwareVersion = 150)]
		public int sceUtilityGetSystemParamInt(PSP_SYSTEMPARAM_ID id, int* value)
		{
			*value = _sceUtilityGetSystemParamInt(id);
			return 0;
		}

		/// <summary>
		/// Get String System Parameter
		/// </summary>
		/// <param name="id">Which parameter to get</param>
		/// <param name="str">Char * buffer to place result in</param>
		/// <param name="len">Length of str buffer</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		[HlePspFunction(NID = 0x34B78343, FirmwareVersion = 150)]
		public int sceUtilityGetSystemParamString(PSP_SYSTEMPARAM_ID id, byte* str, int len)
		{
			var Value = _sceUtilityGetSystemParamString(id);
			PointerUtils.StoreStringOnPtr(Value, Encoding.ASCII, str, len);
			return 0;
		}

		/// <summary>
		/// Set Integer System Parameter
		/// </summary>
		/// <param name="id">Which parameter to set</param>
		/// <param name="value">Integer value to set</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		public int sceUtilitySetSystemParamInt(PSP_SYSTEMPARAM_ID id, int value)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Set String System Parameter
		/// </summary>
		/// <param name="id">Which parameter to set</param>
		/// <param name="str">char* value to set</param>
		/// <returns>PSP_SYSTEMPARAM_RETVAL.OK on success, PSP_SYSTEMPARAM_RETVAL.FAIL on failure</returns>
		public int sceUtilitySetSystemParamString(PSP_SYSTEMPARAM_ID id, string str)
		{
			throw(new NotImplementedException());
		}
	}
}
