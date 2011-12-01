using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.impose
{
	unsafe public class sceImpose : HleModuleHost
	{
		uint umdPopupStatus;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="umdPopupStatus"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x72189C48, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public uint sceImposeSetUMDPopupFunction(uint umdPopupStatus)
		{
			this.umdPopupStatus = umdPopupStatus;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0xE0887BC8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public uint sceImposeGetUMDPopupFunction()
		{
			return this.umdPopupStatus;
		}

		/// <summary>
		/// Set the language and button assignment parameters
		/// </summary>
		/// <param name="language">Language</param>
		/// <param name="confirmButton">Button assignment (Cross or circle)</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x36AA6E91, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceImposeSetLanguageMode(PspLanguages language, PspConfirmButton confirmButton)
		{
			throw (new NotImplementedException());
			/*
			logError("sceImposeSetLanguageMode(%s, %s)", to!string(language), to!string(confirmButton));
			hleEmulatorState.osConfig.language      = language;
			hleEmulatorState.osConfig.confirmButton = confirmButton;
			*/
			return 0;
		}

		/// <summary>
		/// Get the language and button assignment parameters
		/// </summary>
		/// <param name="language">Pointer to store the language</param>
		/// <param name="confirmButton">Pointer to store the button assignment (Cross or circle)</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x24FD7BCF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceImposeGetLanguageMode(PspLanguages* language, PspConfirmButton* confirmButton)
		{
			throw (new NotImplementedException());
			/*
			*language = hleEmulatorState.osConfig.language;
			*confirmButton = hleEmulatorState.osConfig.confirmButton;

			return 0;
			*/
		}

		/// <summary>
		/// IsChargingPointer:
		///		0 - if not charging
		///		1 - if charging
		///	IconStatusPointer:
		///		0 - Battery is very low
		///		1 - Battery is low
		///		2 - Battery is partial filled
		///		3 - Battery is fully filled
		/// </summary>
		/// <param name="IsChargingPointer"></param>
		/// <param name="IconStatusPointer"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0x8C943191, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public uint sceImposeGetBatteryIconStatus(uint* IsChargingPointer, uint* IconStatusPointer)
		{
			*IsChargingPointer = 0;
			*IconStatusPointer = 0;
			return 0;
		}

	}

	public enum PspLanguages : int
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
		TRADITIONAL_CHINESE = 10,
		SIMPLIFIED_CHINESE = 11,
	}

	public enum PspConfirmButton : int
	{
		CIRCLE = 0,
		CROSS = 1,
	}
}
