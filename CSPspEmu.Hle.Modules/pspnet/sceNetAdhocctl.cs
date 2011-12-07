using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.pspnet
{
	unsafe public partial class sceNetAdhocctl : HleModuleHost
	{
		/**
		 * Initialise the Adhoc control library
		 *
		 * @param stacksize - Stack size of the adhocctl thread. Set to 0x2000
		 * @param priority - Priority of the adhocctl thread. Set to 0x30
		 * @param product - Pass a filled in ::productStruct
		 *
		 * @return 0 on success, < 0 on error
		 */
		[HlePspFunction(NID = 0xE26F226E, FirmwareVersion = 150)]
		public int sceNetAdhocctlInit(int stacksize, int priority, productStruct* product)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Terminate the Adhoc control library
		 *
		 * @return 0 on success, < on error.
		 */
		[HlePspFunction(NID = 0x9D689E13, FirmwareVersion = 150)]
		public int sceNetAdhocctlTerm()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Connect to the Adhoc control
		 *
		 * @param name - The name of the connection (maximum 8 alphanumeric characters).
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0x0AD043ED, FirmwareVersion = 150)]
		public int sceNetAdhocctlConnect(string name)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Disconnect from the Adhoc control
		 *
		 * @return 0 on success, < 0 on error
		 */
		[HlePspFunction(NID = 0x34401D65, FirmwareVersion = 150)]
		public int sceNetAdhocctlDisconnect()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get the state of the Adhoc control
		 *
		 * @param event - Pointer to an integer to receive the status. Can continue when it becomes 1.
		 *
		 * @return 0 on success, < 0 on error
		 */
		[HlePspFunction(NID = 0x75ECD386, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetState(int* Event)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Connect to the Adhoc control (as a host)
		 *
		 * @param name - The name of the connection (maximum 8 alphanumeric characters).
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0xEC0635C1, FirmwareVersion = 150)]
		public int sceNetAdhocctlCreate(string name)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Connect to the Adhoc control (as a client)
		 *
		 * @param scaninfo - A valid ::SceNetAdhocctlScanInfo struct that has been filled by sceNetAchocctlGetScanInfo
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0x5E7F79C9, FirmwareVersion = 150)]
		public int sceNetAdhocctlJoin(SceNetAdhocctlScanInfo* scaninfo)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get the adhoc ID
		 *
		 * @param product - A pointer to a  ::productStruct
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlGetAdhocId(productStruct* product)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Connect to the Adhoc control game mode (as a host)
		 *
		 * @param name - The name of the connection (maximum 8 alphanumeric characters).
		 * @param unknown - Pass 1.
		 * @param num - The total number of players (including the host).
		 * @param macs - A pointer to a list of the participating mac addresses, host first, then clients.
		 * @param timeout - Timeout in microseconds.
		 * @param unknown2 - pass 0.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlCreateEnterGameMode(string name, int unknown, int num, string macs, uint timeout, int unknown2)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Connect to the Adhoc control game mode (as a client)
		 *
		 * @param name - The name of the connection (maximum 8 alphanumeric characters).
		 * @param hostmac - The mac address of the host.
		 * @param timeout - Timeout in microseconds.
		 * @param unknown - pass 0.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlJoinEnterGameMode(string name, string hostmac, uint timeout, int unknown)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get game mode information
		 *
		 * @param gamemodeinfo - Pointer to store the info.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlGetGameModeInfo(SceNetAdhocctlGameModeInfo* gamemodeinfo)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Exit game mode.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlExitGameMode()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get a list of peers
		 *
		 * @param length - The length of the list.
		 * @param buf - An allocated area of size length.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0xE162CB14, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetPeerList(int* length, void* buf)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get peer information
		 *
		 * @param mac - The mac address of the peer.
		 * @param size - Size of peerinfo.
		 * @param peerinfo - Pointer to store the information.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlGetPeerInfo(string mac, int size, SceNetAdhocctlPeerInfo* peerinfo)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Scan the adhoc channels
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0x08FFF7A0, FirmwareVersion = 150)]
		public int sceNetAdhocctlScan()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get the results of a scan
		 *
		 * @param length - The length of the list.
		 * @param buf - An allocated area of size length.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0x81AEE1BE, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetScanInfo(int* length, void* buf)
		{
			throw(new NotImplementedException());
		}

		//typedef void (*sceNetAdhocctlHandler)(int flag, int error, void *unknown);

		/**
		 * Register an adhoc event handler
		 *
		 * @param handler - The event handler.
		 * @param unknown - Pass NULL.
		 *
		 * @return Handler id on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0x20B317A0, FirmwareVersion = 150)]
		public int sceNetAdhocctlAddHandler(/*sceNetAdhocctlHandler*/uint handler, void* unknown)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Delete an adhoc event handler
		 *
		 * @param id - The handler id as returned by sceNetAdhocctlAddHandler.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0x6402490B, FirmwareVersion = 150)]
		public int sceNetAdhocctlDelHandler(int id)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get nickname from a mac address
		 *
		 * @param mac - The mac address.
		 * @param nickname - Pointer to a char buffer where the nickname will be stored.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		[HlePspFunction(NID = 0x8916C003, FirmwareVersion = 150)]
		public int sceNetAdhocctlGetNameByAddr(string mac, char* nickname)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get mac address from nickname
		 *
		 * @param nickname - The nickname.
		 * @param length - The length of the list.
		 * @param buf - An allocated area of size length.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlGetAddrByName(char* nickname, int* length, void* buf)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get Adhocctl parameter
		 *
		 * @param params - Pointer to a ::SceNetAdhocctlParams
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocctlGetParameter(SceNetAdhocctlParams* Params)
		{
			throw(new NotImplementedException());
		}
	}
}
