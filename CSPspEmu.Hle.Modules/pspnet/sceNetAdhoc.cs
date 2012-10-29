using System;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | ModuleFlags.Flags0x00010011)]
	public unsafe partial class sceNetAdhoc : HleModuleHost
	{
		/// <summary>
		/// Initialise the adhoc library.
		/// </summary>
		/// <returns>
		///		0 on success.
		///		Less than 0 on error.
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
		///		0 on success.
		///		Less than 0 on error.
		/// </returns>
		[HlePspFunction(NID = 0xA62C6F57, FirmwareVersion = 150)]
		[HlePspNotImplemented]
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

		/// <summary>
		/// Create a PDP object.
		/// </summary>
		/// <param name="mac">Your MAC address (from sceWlanGetEtherAddr)</param>
		/// <param name="port">Port to use, lumines uses 0x309</param>
		/// <param name="bufsize">Socket buffer size, lumines sets to 0x400</param>
		/// <param name="unk1">Unknown, lumines sets to 0</param>
		/// <returns>
		///		The ID of the PDP object.
		///		Less than 0 on error.
		///	</returns>
		/// 
		[HlePspFunction(NID = 0x6F92741B, FirmwareVersion = 150)]
		[HlePspNotImplemented]
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
		///		0 on success.
		///		Less than 0 on error.
		/// </returns>
		[HlePspFunction(NID = 0x7F27BB5E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocPdpDelete(int id, int unk1)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Send a PDP packet to a destination.
		/// </summary>
		/// <param name="id">The ID as returned by <see cref="sceNetAdhocPdpCreate"/>.</param>
		/// <param name="destMacAddr">The destination MAC address, can be set to all 0xFF for broadcast.</param>
		/// <param name="port">The port to send to.</param>
		/// <param name="data">The data to send.</param>
		/// <param name="len">The length of the data.</param>
		/// <param name="timeout">Timeout in microseconds</param>
		/// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
		/// <returns>Bytes sent, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0xABED3790, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocPdpSend(int id, byte* destMacAddr, ushort port, void* data, uint len, uint timeout, int nonblock)
		{
			throw (new NotImplementedException());
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
		[HlePspFunction(NID = 0xDFE53E03, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocPdpRecv(int id, byte *srcMacAddr, ushort *port, void *data, void *dataLength, uint timeout, int nonblock)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Get the status of all PDP objects
		/// </summary>
		/// <param name="size">Pointer to the size of the stat array (e.g 20 for one structure)</param>
		/// <param name="stat">Pointer to a list of ::pspStatStruct structures.</param>
		/// <returns> on success, &lt; 0 on error</returns>
		[HlePspFunction(NID = 0xC7C1FC57, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocGetPdpStat(int *size, pdpStatStruct *stat)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Create own game object type data.
		/// </summary>
		/// <param name="data">A pointer to the game object data.</param>
		/// <param name="size">Size of the game data.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x7F75C338, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocGameModeCreateMaster(void *data, int size)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Create peer game object type data.
		/// </summary>
		/// <param name="mac">he mac address of the peer.</param>
		/// <param name="data">A pointer to the game object data.</param>
		/// <param name="size">Size of the game data.</param>
		/// <returns>The id of the replica on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x3278AB0C, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocGameModeCreateReplica(byte *mac, void *data, int size)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Update own game object type data.
		/// </summary>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x98C204C8, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocGameModeUpdateMaster()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Update peer game object type data.
		/// </summary>
		/// <param name="id">The ID of the replica returned by <see cref="sceNetAdhocGameModeCreateReplica"/>.</param>
		/// <param name="unk1">Pass 0.</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0xFA324B4E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocGameModeUpdateReplica(int id, int unk1)
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Delete own game object type data.
		/// </summary>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0xA0229362, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocGameModeDeleteMaster()
		{
			throw(new NotImplementedException());
		}

		/// <summary>
		/// Delete peer game object type data.
		/// </summary>
		/// <param name="id">The ID of the replica</param>
		/// <returns>0 on success, &lt; 0 on error.</returns>
		[HlePspFunction(NID = 0x0B2228E9, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocGameModeDeleteReplica(int id)
		{
			throw(new NotImplementedException());
		}
	}
}
