using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;

namespace CSPspEmu.Hle.Modules.wlan
{
	/// <summary>
	/// Functions related to Wlan.
	/// </summary>
	unsafe public class sceWlanDrv : HleModuleHost
	{
		/// <summary>
		/// Determine the state of the Wlan power switch
		/// </summary>
		/// <returns>0 if off, 1 if on</returns>
		[HlePspFunction(NID = 0xD7763699, FirmwareVersion = 150)]
		public int sceWlanGetSwitchState()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the Ethernet Address of the wlan controller
		/// </summary>
		/// <param name="EthernetAddress">
		///		Pointer to a buffer of u8 (NOTE: it only writes to 6 bytes, but 
		///		requests 8 so pass it 8 bytes just in case)
		/// </param>
		/// <returns>0 on success, < 0 on error</returns>
		[HlePspFunction(NID = 0x0C622081, FirmwareVersion = 150)]
		public int sceWlanGetEtherAddr(byte* EthernetAddress) {
			foreach (var ThisNetworkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (ThisNetworkInterface.OperationalStatus == OperationalStatus.Up)
				{
					var Bytes = ThisNetworkInterface.GetPhysicalAddress().GetAddressBytes();
					for (int n = 0; n < 6; n++)
					{
						EthernetAddress[n] = Bytes[n];
					}

					return 0;
				}
			}

			return -1;
		}

		/// <summary>
		/// Determine if the wlan device is currently powered on
		/// </summary>
		/// <returns>0 if off, 1 if on</returns>
		[HlePspFunction(NID = 0x93440B11, FirmwareVersion = 150)]
		public int sceWlanDevIsPowerOn()
		{
			throw (new NotImplementedException());
		}
	}
}
