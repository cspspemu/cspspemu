using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules._unknownPrx
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceUsb : HleModuleHost
	{
		[Flags]
		public enum UsbStateEnum : uint
		{
			/// <summary>
			/// 
			/// </summary>
			PSP_USB_CONNECTION_ESTABLISHED = 0x002,

			/// <summary>
			/// 
			/// </summary>
			PSP_USB_CABLE_CONNECTED = 0x020,

			/// <summary>
			/// 
			/// </summary>
			PSP_USB_ACTIVATED = 0x200,
		}

		public bool UsbActivated;
		public bool UsbStarted;

		public UsbStateEnum UsbState
		{
			get
			{
				// Simulate that a USB cacle is always connected
				UsbStateEnum State = UsbStateEnum.PSP_USB_CABLE_CONNECTED;

				// USB has been activated?
				if (UsbActivated) State |= UsbStateEnum.PSP_USB_ACTIVATED;
				if (UsbStarted) State |= UsbStateEnum.PSP_USB_CONNECTION_ESTABLISHED;

				return State;
			}
		}

		/// <summary>
		/// Start a USB driver.
		/// </summary>
		/// <param name="DriverName">name of the USB driver to start</param>
		/// <param name="Size">Size of arguments to pass to USB driver start</param>
		/// <param name="Args">Arguments to pass to USB driver start</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xAE5DE6AF, FirmwareVersion = 150)]
		public int sceUsbStart(string DriverName, int Size, void* Args)
		{
			UsbStarted = true;

			return 0;
		}

		/// <summary>
		/// Stop a USB driver.
		/// </summary>
		/// <param name="DriverName">name of the USB driver to stop</param>
		/// <param name="Size">Size of arguments to pass to USB driver start</param>
		/// <param name="Args">Arguments to pass to USB driver start</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xC2464FA0, FirmwareVersion = 150)]
		public int sceUsbStop(string DriverName, int Size, void* Args)
		{
			UsbStarted = false;

			return 0;
		}

		/// <summary>
		/// Get USB state
		/// </summary>
		/// <returns>OR'd PSP_USB_* constants</returns>
		[HlePspFunction(NID = 0xC21645A4, FirmwareVersion = 150)]
		public UsbStateEnum sceUsbGetState()
		{
			return UsbState;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x4E537366, FirmwareVersion = 150)]
		public int sceUsbGetDrvList()
		{
			return 0;
		}

		/// <summary>
		/// Get state of a specific USB driver
		/// </summary>
		/// <param name="DriverName">name of USB driver to get status from</param>
		/// <returns>1 if the driver has been started, 2 if it is stopped</returns>
		[HlePspFunction(NID = 0x112CC951, FirmwareVersion = 150)]
		public int sceUsbGetDrvState(string DriverName)
		{
			return 0;
		}

		/// <summary>
		/// Activate a USB driver.
		/// </summary>
		/// <param name="pid">Product ID for the default USB Driver</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0x586DB82C, FirmwareVersion = 150)]
		public int sceUsbActivate(int pid)
		{
			UsbActivated = true;
			return 0;
		}

		/// <summary>
		/// Deactivate USB driver.
		/// </summary>
		/// <param name="pid">Product ID for the default USB driver</param>
		/// <returns>0 on success</returns>
		[HlePspFunction(NID = 0xC572A9C8, FirmwareVersion = 150)]
		public int sceUsbDeactivate(int pid)
		{
			UsbActivated = false;
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x5BE0E002, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUsbWaitState()
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		[HlePspFunction(NID = 0x1C360735, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceUsbWaitCancel()
		{
			return 0;
		}

	}
}
