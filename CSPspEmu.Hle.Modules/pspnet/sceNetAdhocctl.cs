using System;

namespace CSPspEmu.Hle.Modules.pspnet
{
	public unsafe partial class sceNetAdhocctl : HleModuleHost
	{
		/// <summary>
		/// Initialise the Adhoc control library
		/// </summary>
		/// <param name="stacksize">Stack size of the adhocctl thread. Set to 0x2000</param>
		/// <param name="priority">Priority of the adhocctl thread. Set to 0x30</param>
		/// <param name="product">Pass a filled in <see cref="productStruct"/></param>
		/// <returns>0 on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0xE26F226E, FirmwareVersion = 150)]
		public int sceNetAdhocctlInit(int stacksize, int priority, productStruct* product)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Terminate the Adhoc control library
		/// </summary>
		/// <returns>0 on success, &lt; on error.</returns>
		[HlePspFunction(NID = 0x9D689E13, FirmwareVersion = 150)]
		public int sceNetAdhocctlTerm()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Connect to the Adhoc control
		/// </summary>
		/// <param name="name">The name of the connection (maximum 8 alphanumeric characters).</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x0AD043ED, FirmwareVersion = 150)]
		public int sceNetAdhocctlConnect(string name)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Disconnect from the Adhoc control
		/// </summary>
		/// <returns>0 on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0x34401D65, FirmwareVersion = 150)]
		public int sceNetAdhocctlDisconnect()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the state of the Adhoc control
		/// </summary>
		/// <param name="Event">Pointer to an integer to receive the status. Can continue when it becomes 1.</param>
		/// <returns>0 on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0x75ECD386, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetState(int* Event)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Connect to the Adhoc control (as a host)
		/// </summary>
		/// <param name="name">The name of the connection (maximum 8 alphanumeric characters).</param>
		/// <returns> on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0xEC0635C1, FirmwareVersion = 150)]
		public int sceNetAdhocctlCreate(string name)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Connect to the Adhoc control (as a client)
		/// </summary>
		/// <param name="scaninfo">A valid ::SceNetAdhocctlScanInfo struct that has been filled by sceNetAchocctlGetScanInfo</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x5E7F79C9, FirmwareVersion = 150)]
		public int sceNetAdhocctlJoin(SceNetAdhocctlScanInfo* scaninfo)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the adhoc ID
		/// </summary>
		/// <param name="product">A pointer to a  <see cref="productStruct"/></param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x362CBE8F, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlGetAdhocId(productStruct* product)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Connect to the Adhoc control game mode (as a host)
		/// </summary>
		/// <param name="name">The name of the connection (maximum 8 alphanumeric characters).</param>
		/// <param name="unknown">Pass 1</param>
		/// <param name="num">The total number of players (including the host).</param>
		/// <param name="macs"> Pointer to a list of the participating mac addresses, host first, then clients.</param>
		/// <param name="timeout">Timeout in microseconds.</param>
		/// <param name="unknown2">Pass 0</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xA5C055CE, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlCreateEnterGameMode(string name, int unknown, int num, string macs, uint timeout, int unknown2)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Connect to the Adhoc control game mode (as a client)
		/// </summary>
		/// <param name="name">The name of the connection (maximum 8 alphanumeric characters).</param>
		/// <param name="hostmac">The mac address of the host.</param>
		/// <param name="timeout">Timeout in microseconds.</param>
		/// <param name="unknown">Pass 0</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x1FF89745, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlJoinEnterGameMode(string name, string hostmac, uint timeout, int unknown)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get game mode information
		/// </summary>
		/// <param name="gamemodeinfo">Pointer to store the info.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x5A014CE0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlGetGameModeInfo(SceNetAdhocctlGameModeInfo* gamemodeinfo)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Exit game mode.
		/// </summary>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xCF8E084D, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlExitGameMode()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get a list of peers
		/// </summary>
		/// <param name="length">The length of the list.</param>
		/// <param name="buf">An allocated area of size length.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0xE162CB14, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetPeerList(int* length, void* buf)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get peer information
		/// </summary>
		/// <param name="mac">The mac address of the peer.</param>
		/// <param name="size">Size of peerinfo.</param>
		/// <param name="peerinfo">Pointer to store the information.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x8DB83FDC, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetPeerInfo(string mac, int size, SceNetAdhocctlPeerInfo* peerinfo)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Scan the adhoc channels
		/// </summary>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x08FFF7A0, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlScan()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the results of a scan
		/// </summary>
		/// <param name="length">The length of the list.</param>
		/// <param name="buf">An allocated area of size length.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x81AEE1BE, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetScanInfo(int* length, void* buf)
		{
			throw(new NotImplementedException());
		}

		//typedef void (*sceNetAdhocctlHandler)(int flag, int error, void *unknown);

		/// <summary>
		/// Register an adhoc event handler
		/// </summary>
		/// <param name="handler">The event handler.</param>
		/// <param name="unknown">Pass NULL</param>
		/// <returns>Handler ID on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x20B317A0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocctlAddHandler(/*sceNetAdhocctlHandler*/uint handler, void* unknown)
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Delete an adhoc event handler
		/// </summary>
		/// <param name="id">he handler ID as returned by <see cref="sceNetAdhocctlAddHandler"/>.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x6402490B, FirmwareVersion = 150)]
		public int sceNetAdhocctlDelHandler(int id)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get nickname from a mac address
		/// </summary>
		/// <param name="mac">The mac address.</param>
		/// <param name="nickname">Pointer to a char buffer where the nickname will be stored.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x8916C003, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetNameByAddr(string mac, char* nickname)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get mac address from nickname
		/// </summary>
		/// <param name="nickname">The nickname.</param>
		/// <param name="length">The length of the list.</param>
		/// <param name="buf">An allocated area of size length.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x99560ABE, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlGetAddrByName(char* nickname, int* length, void* buf)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get Adhocctl parameter
		/// </summary>
		/// <param name="Params">Pointer to a <see cref="SceNetAdhocctlParams"/></param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xDED9D28E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
		public int sceNetAdhocctlGetParameter(SceNetAdhocctlParams* Params)
		{
			throw(new NotImplementedException());
		}
	}
}
