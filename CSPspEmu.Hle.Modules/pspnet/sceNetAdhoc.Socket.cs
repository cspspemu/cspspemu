using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.pspnet
{
	unsafe public partial class sceNetAdhoc
	{
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
		public int sceNetAdhocPtpOpen(byte* srcmac, ushort srcport, byte* destmac, ushort destport, uint bufsize, uint delay, int count, int unk1)
		{
			throw (new NotImplementedException());
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
		public int sceNetAdhocPtpListen(byte* srcmac, ushort srcport, uint bufsize, uint delay, int count, int queue, int unk1)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Wait for connection created by sceNetAdhocPtpOpen()
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
			throw(new NotImplementedException());
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
			throw (new NotImplementedException());
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
		///		0 on success
		///		less than 0 on error
		/// </returns>
		[HlePspFunction(NID = 0x4DA4C788, FirmwareVersion = 150)]
		public int sceNetAdhocPtpSend(int id, void* data, int* datasize, uint timeout, int nonblock)
		{
			throw (new NotImplementedException());
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
		///		0 on success
		///		less than 0 on error.
		///	</returns>
		[HlePspFunction(NID = 0x8BEA2B3E, FirmwareVersion = 150)]
		public int sceNetAdhocPtpRecv(int id, void* data, int* datasize, uint timeout, int nonblock)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Wait for data in the buffer to be sent
		/// </summary>
		/// <param name="id">A socket ID.</param>
		/// <param name="timeout">Timeout in microseconds.</param>
		/// <param name="nonblock">Set to 0 to block, 1 for non-blocking.</param>
		/// <returns>
		///		A socket ID on success
		///		less than 0 on error.
		/// </returns>
		[HlePspFunction(NID = 0x9AC2EEAC, FirmwareVersion = 150)]
		public int sceNetAdhocPtpFlush(int id, uint timeout, int nonblock)
		{
			throw (new NotImplementedException());
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
			throw (new NotImplementedException());
		}
	}
}
