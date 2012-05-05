using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	unsafe public partial class sceNetAdhoc : HleModuleHost
	{
		/// <summary>
		/// Initialise the adhoc library.
		/// </summary>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xE1D621D7, FirmwareVersion = 150)]
		public int sceNetAdhocInit()
		{
			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Terminate the adhoc library
		/// </summary>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0xA62C6F57, FirmwareVersion = 150)]
		public int sceNetAdhocTerm()
		{
			throw(new NotImplementedException());
		}

        [HlePspFunction(NID = 0x7A662D6B, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocPollSocket()
        {
            throw (new NotImplementedException());
        }

        [HlePspFunction(NID = 0x6F92741B, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocPdpCreate()
        {
            throw (new NotImplementedException());
        }

        [HlePspFunction(NID = 0xABED3790, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocPdpSend()
        {
            throw (new NotImplementedException());
        }

        [HlePspFunction(NID = 0xDFE53E03, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocPdpRecv()
        {
            throw (new NotImplementedException());
        }

        [HlePspFunction(NID = 0x7F27BB5E, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocPdpDelete()
        {
            throw (new NotImplementedException());
        }

		/// <summary>
		/// Create a PDP object.
		/// </summary>
		/// <param name="mac">Your MAC address (from sceWlanGetEtherAddr)</param>
		/// <param name="port">Port to use, lumines uses 0x309</param>
		/// <param name="bufsize">Socket buffer size, lumines sets to 0x400</param>
		/// <param name="unk1">Unknown, lumines sets to 0</param>
		/// <returns>
		///		The ID of the PDP object
		///		less than 0 on error
		///	</returns>
		public int sceNetAdhocPdpCreate(byte *mac, ushort port, uint bufsize, int unk1)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Delete a PDP object.
		/// </summary>
		/// <param name="id">The ID returned from ::sceNetAdhocPdpCreate</param>
		/// <param name="unk1">Unknown, set to 0</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		public int sceNetAdhocPdpDelete(int id, int unk1)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Set a PDP packet to a destination
		/// </summary>
		/// <param name="id">The ID as returned by ::sceNetAdhocPdpCreate</param>
		/// <param name="destMacAddr">The destination MAC address, can be set to all 0xFF for broadcast</param>
		/// <param name="port">The port to send to</param>
		/// <param name="data">The data to send</param>
		/// <param name="len">The length of the data.</param>
		/// <param name="timeout">Timeout in microseconds.</param>
		/// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
		/// <returns>
		///		Bytes sent
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x4DA4C788, FirmwareVersion = 150)]
		public int sceNetAdhocPdpSend(int id, byte *destMacAddr, ushort port, void *data, uint len, uint timeout, int nonblock)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Receive a PDP packet
		/// </summary>
		/// <param name="id">The ID of the PDP object, as returned by ::sceNetAdhocPdpCreate</param>
		/// <param name="srcMacAddr">Buffer to hold the source mac address of the sender</param>
		/// <param name="port">Buffer to hold the port number of he received data</param>
		/// <param name="data">Data buffer</param>
		/// <param name="dataLength">The length of the data buffer</param>
		/// <param name="timeout">Timeout in microseconds.</param>
		/// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
		/// <returns>
		///		Number of bytes received
		///		less than 0 on error.
		/// </returns>
		public int sceNetAdhocPdpRecv(int id, byte *srcMacAddr, ushort *port, void *data, void *dataLength, uint timeout, int nonblock)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Get the status of all PDP objects
		 *
		 * @param size - Pointer to the size of the stat array (e.g 20 for one structure)
		 * @param stat - Pointer to a list of ::pspStatStruct structures.
		 *
		 * @return 0 on success, < 0 on error
		 */
		public int sceNetAdhocGetPdpStat(int *size, pdpStatStruct *stat)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Create own game object type data.
		 *
		 * @param data - A pointer to the game object data.
		 * @param size - Size of the game data.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocGameModeCreateMaster(void *data, int size)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Create peer game object type data.
		 *
		 * @param mac - The mac address of the peer.
		 * @param data - A pointer to the game object data.
		 * @param size - Size of the game data.
		 *
		 * @return The id of the replica on success, < 0 on error.
		 */
		public int sceNetAdhocGameModeCreateReplica(byte *mac, void *data, int size)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Update own game object type data.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocGameModeUpdateMaster()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Update peer game object type data.
		 *
		 * @param id - The id of the replica returned by sceNetAdhocGameModeCreateReplica.
		 * @param unk1 - Pass 0.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocGameModeUpdateReplica(int id, int unk1)
		{
			throw(new NotImplementedException());
		}

		/**
		 * Delete own game object type data.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocGameModeDeleteMaster()
		{
			throw(new NotImplementedException());
		}

		/**
		 * Delete peer game object type data.
		 *
		 * @param id - The id of the replica.
		 *
		 * @return 0 on success, < 0 on error.
		 */
		public int sceNetAdhocGameModeDeleteReplica(int id)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the status of all PTP objects
		/// </summary>
		/// <param name="size">Pointer to the size of the stat array (e.g 20 for one structure)</param>
		/// <param name="stat">Pointer to a list of ::ptpStatStruct structures.</param>
		/// <returns>
		///		0 on success
		///		less than 0 on error
		/// </returns>
		public int sceNetAdhocGetPtpStat(int *size, ptpStatStruct *stat)
		{
			throw(new NotImplementedException());
		}
	}
}
