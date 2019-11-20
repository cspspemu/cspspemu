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
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HlePspFunction(NID = 0x7A662D6B, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocPollSocket(int socketAddress, int count, int timeout, int nonblock)
        {
            return -1;
            //throw (new NotImplementedException());
        }

        public class PDP : IHleUidPoolClass
        {
            public void Dispose()
            {
            }
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
        public PDP sceNetAdhocPdpCreate(byte* mac, ushort port, uint bufsize, int unk1)
        {
            //throw(new NotImplementedException());
            return new PDP();
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
        public int sceNetAdhocPdpDelete(PDP PDP, int unk1)
        {
            PDP.RemoveUid(InjectContext);
            return 0;
        }

        /// <summary>
        /// Send a PDP packet to a destination.
        /// </summary>
        /// <param name="PDP">The ID as returned by <see cref="sceNetAdhocPdpCreate"/>.</param>
        /// <param name="destMacAddr">The destination MAC address, can be set to all 0xFF for broadcast.</param>
        /// <param name="port">The port to send to.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="len">The length of the data.</param>
        /// <param name="timeout">Timeout in microseconds</param>
        /// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
        /// <returns>Bytes sent, &lt; 0 on error</returns>
        [HlePspFunction(NID = 0xABED3790, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocPdpSend(PDP PDP, byte* destMacAddr, ushort port, void* data, uint len, uint timeout,
            int nonblock)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receive a PDP packet
        /// </summary>
        /// <param name="PDP">The ID of the PDP object, as returned by ::sceNetAdhocPdpCreate</param>
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
        public int sceNetAdhocPdpRecv(PDP PDP, byte*srcMacAddr, ushort*port, void*data, void*dataLength, uint timeout,
            int nonblock)
        {
            //throw(new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Get the status of all PDP objects
        /// </summary>
        /// <param name="size">Pointer to the size of the stat array (e.g 20 for one structure)</param>
        /// <param name="stat">Pointer to a list of ::pspStatStruct structures.</param>
        /// <returns> on success, &lt; 0 on error</returns>
        [HlePspFunction(NID = 0xC7C1FC57, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocGetPdpStat(int*size, pdpStatStruct*stat)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create own game object type data.
        /// </summary>
        /// <param name="data">A pointer to the game object data.</param>
        /// <param name="size">Size of the game data.</param>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x7F75C338, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocGameModeCreateMaster(void*data, int size)
        {
            throw new NotImplementedException();
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
        public int sceNetAdhocGameModeCreateReplica(byte*mac, void*data, int size)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update own game object type data.
        /// </summary>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0x98C204C8, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocGameModeUpdateMaster()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete own game object type data.
        /// </summary>
        /// <returns>0 on success, &lt; 0 on error.</returns>
        [HlePspFunction(NID = 0xA0229362, FirmwareVersion = 150)]
        [HlePspNotImplemented]
        public int sceNetAdhocGameModeDeleteMaster()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Open a PTP (Peer To Peer) connection
        /// </summary>
        /// <param name="srcmac">Local mac address.</param>
        /// <param name="srcport">Local port.</param>
        /// <param name="destmac">Destination mac.</param>
        /// <param name="destport">Destination port</param>
        /// <param name="bufsize">Socket buffer size</param>
        /// <param name="delay">Interval between retrying (microseconds).</param>
        /// <param name="count">Number of retries.</param>
        /// <param name="unk1">Pass 0.</param>
        /// <returns>
        ///		A socket ID on success
        ///		less than 0 on error.
        /// </returns>
        [HlePspFunction(NID = 0x877F6D66, FirmwareVersion = 150)]
        public int sceNetAdhocPtpOpen(byte* srcmac, ushort srcport, byte* destmac, ushort destport, uint bufsize,
            uint delay, int count, int unk1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Wait for an incoming PTP connection
        /// </summary>
        /// <param name="srcmac">Local mac address.</param>
        /// <param name="srcport">Local port.</param>
        /// <param name="bufsize">Socket buffer size</param>
        /// <param name="delay">Interval between retrying (microseconds).</param>
        /// <param name="count">Number of retries.</param>
        /// <param name="queue">Connection queue length.</param>
        /// <param name="unk1">Pass 0.</param>
        /// <returns>
        ///		A socket ID on success
        ///		less than 0 on error
        /// </returns>
        [HlePspFunction(NID = 0xE08BDAC1, FirmwareVersion = 150)]
        public int sceNetAdhocPtpListen(byte* srcmac, ushort srcport, uint bufsize, uint delay, int count, int queue,
            int unk1)
        {
            //throw (new NotImplementedException());
            return -1;
        }

        /// <summary>
        /// Wait for connection created by <see cref="sceNetAdhocPtpOpen"/>
        /// </summary>
        /// <param name="id">A socket ID.</param>
        /// <param name="timeout">Timeout in microseconds.</param>
        /// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
        /// <returns>
        ///		0 on success
        ///		less than 0 on error.
        /// </returns>
        [HlePspFunction(NID = 0xFC6FC07B, FirmwareVersion = 150)]
        public int sceNetAdhocPtpConnect(int id, uint timeout, int nonblock)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Accept an incoming PTP connection
        /// </summary>
        /// <param name="id">A socket ID.</param>
        /// <param name="mac">Connecting peers mac.</param>
        /// <param name="port">Connecting peers port.</param>
        /// <param name="timeout">Timeout in microseconds.</param>
        /// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
        /// <returns>
        ///		0 on success
        ///		less than 0 on error
        /// </returns>
        [HlePspFunction(NID = 0x9DF81198, FirmwareVersion = 150)]
        public int sceNetAdhocPtpAccept(int id, byte* mac, ushort* port, uint timeout, int nonblock)
        {
            //throw (new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Send data
        /// </summary>
        /// <param name="id">A socket ID.</param>
        /// <param name="data">Data to send.</param>
        /// <param name="datasize">Size of the data.</param>
        /// <param name="timeout">Timeout in microseconds.</param>
        /// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
        /// <returns>
        ///		0 on success.
        ///		Less than 0 on error.
        /// </returns>
        [HlePspFunction(NID = 0x4DA4C788, FirmwareVersion = 150)]
        public int sceNetAdhocPtpSend(int id, void* data, int* datasize, uint timeout, int nonblock)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receive data
        /// </summary>
        /// <param name="id">A socket ID.</param>
        /// <param name="data">Buffer for the received data.</param>
        /// <param name="datasize">Size of the data received.</param>
        /// <param name="timeout">Timeout in microseconds.</param>
        /// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
        /// <returns>
        ///		0 on success.
        ///		Less than 0 on error.
        ///	</returns>
        [HlePspFunction(NID = 0x8BEA2B3E, FirmwareVersion = 150)]
        public int sceNetAdhocPtpRecv(int id, void* data, int* datasize, uint timeout, int nonblock)
        {
            //throw (new NotImplementedException());
            return 0;
        }

        /// <summary>
        /// Wait for data in the buffer to be sent
        /// </summary>
        /// <param name="id">A socket ID.</param>
        /// <param name="timeout">Timeout in microseconds.</param>
        /// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
        /// <returns>
        ///		A socket ID on success.
        ///		Less than 0 on error.
        /// </returns>
        [HlePspFunction(NID = 0x9AC2EEAC, FirmwareVersion = 150)]
        public int sceNetAdhocPtpFlush(int id, uint timeout, int nonblock)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Close a socket
        /// </summary>
        /// <param name="id">A socket ID.</param>
        /// <param name="unk1">Pass 0.</param>
        /// <returns>
        ///		A socket ID on success
        ///		less than 0 on error
        /// </returns>
        [HlePspFunction(NID = 0x157E6225, FirmwareVersion = 150)]
        public int sceNetAdhocPtpClose(int id, int unk1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the status of all PTP objects
        /// </summary>
        /// <param name="size">Pointer to the size of the stat array (e.g 20 for one structure)</param>
        /// <param name="stat">Pointer to a list of <see cref="ptpStatStruct"/> structures.</param>
        /// <returns>
        ///		0 on success
        ///		less than 0 on error
        /// </returns>
        /// 
        [HlePspFunction(NID = 0xB9685118, FirmwareVersion = 150)]
        public int sceNetAdhocGetPtpStat(int* size, ptpStatStruct* stat)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// PDP status structure
        /// </summary>
        public struct pdpStatStruct
        {
            /// <summary>
            /// Pointer to next PDP structure in list
            /// </summary>
            //pdpStatStruct *next;
            public uint NextPointer;

            /// <summary>
            /// pdp ID
            /// </summary>
            public int pdpId;

            /// <summary>
            /// MAC address
            /// </summary>
            public fixed byte mac[6];

            /// <summary>
            /// Port
            /// </summary>
            public ushort port;

            /// <summary>
            /// Bytes received
            /// </summary>
            public uint rcvdData;
        }

        /// <summary>
        /// PTP status structure
        /// </summary>
        public struct ptpStatStruct
        {
            /// <summary>
            /// Pointer to next PTP structure in list
            /// </summary>
            //ptpStatStruct *next;
            public uint NextAddress;

            /// <summary>
            /// ptp ID
            /// </summary>
            public int ptpId;

            /// <summary>
            /// MAC address
            /// </summary>
            public fixed byte mac[6];

            /// <summary>
            /// Peer MAC address
            /// </summary>
            public fixed byte peermac[6];

            /// <summary>
            /// Port
            /// </summary>
            public ushort port;

            /// <summary>
            /// Peer Port
            /// </summary>
            public ushort peerport;

            /// <summary>
            /// Bytes sent
            /// </summary>
            public uint sentData;

            /// <summary>
            /// Bytes received
            /// </summary>
            public uint rcvdData;

            /// <summary>
            /// Unknown
            /// </summary>
            public int unk1;
        }
    }
}