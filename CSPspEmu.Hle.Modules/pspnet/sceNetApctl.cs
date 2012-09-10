using System;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public unsafe class sceNetApctl : HleModuleHost
	{
		public struct SceNetApctlInfo
		{
		}

		public enum sceNetApctlHandler : uint
		{
		}

		/// <summary>
		/// Init the apctl.
		/// </summary>
		/// <param name="stackSize">The stack size of the internal thread.</param>
		/// <param name="initPriority">The priority of the internal thread.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xE2F91F9B, FirmwareVersion = 150)]
		public int sceNetApctlInit(int stackSize, int initPriority)
		{
			return 0;
		}

		/// <summary>
		/// Terminate the apctl.
		/// </summary>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB3EDD0EC, FirmwareVersion = 150)]
		public int sceNetApctlTerm()
		{
			return 0;
		}

		/// <summary>
		/// Get the apctl information.
		/// </summary>
		/// <param name="code">One of the PSP_NET_APCTL_INFO_* defines.</param>
		/// <param name="pInfo">Pointer to a ::SceNetApctlInfo.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x2BEFDF23, FirmwareVersion = 150)]
		public int sceNetApctlGetInfo(int code, SceNetApctlInfo *pInfo)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Add an apctl event handler.
		/// </summary>
		/// <param name="handler">Pointer to the event handler function.</param>
		/// <param name="pArg">Value to be passed to the pArg parameter of the handler function.</param>
		/// <returns>A handler id or less than 0 on error.</returns>
		[HlePspFunction(NID = 0x8ABADD51, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetApctlAddHandler(sceNetApctlHandler handler, uint pArg)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Delete an apctl event handler.
		/// </summary>
		/// <param name="handlerId">A handler as created returned from <see cref="sceNetApctlAddHandler"/>.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x5963991B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetApctlDelHandler(int handlerId)
		{
			return 0;
		}

		/// <summary>
		/// Connect to an access point.
		/// </summary>
		/// <param name="ConnectionIndex">The index of the connection.</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0xCFB957C6, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetApctlConnect(int ConnectionIndex)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Disconnect from an access point.
		/// </summary>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x24FE91A1, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetApctlDisconnect()
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Get the state of the access point connection.
		/// </summary>
		/// <param name="pState">Pointer to receive the current state (one of the PSP_NET_APCTL_STATE_* defines).</param>
		/// <returns>Less than 0 on error.</returns>
		[HlePspFunction(NID = 0x5DEAC81B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetApctlGetState(PspNetApctlState* pState)
		{
			*pState = PspNetApctlState.GotIp;
			return 0;
		}

		public enum PspNetApctlState : int
		{
			/// <summary>
			/// PSP_NET_APCTL_STATE_DISCONNECTED
			/// </summary>
			Disconnected = 0,

			/// <summary>
			/// PSP_NET_APCTL_STATE_SCANNING
			/// </summary>
			Scanning = 1,

			/// <summary>
			/// PSP_NET_APCTL_STATE_JOINING
			/// </summary>
			Joining = 2,

			/// <summary>
			/// PSP_NET_APCTL_STATE_GETTING_IP
			/// </summary>
			GettingIp = 3,

			/// <summary>
			/// PSP_NET_APCTL_STATE_GOT_IP
			/// </summary>
			GotIp = 4,

			/// <summary>
			/// PSP_NET_APCTL_STATE_EAP_AUTH
			/// </summary>
			EapAuth = 5,

			/// <summary>
			/// PSP_NET_APCTL_STATE_KEY_EXCHANGE
			/// </summary>
			KeyExchange = 6,
		}
	}
}
