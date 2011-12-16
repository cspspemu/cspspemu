using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.pspnet
{
	unsafe public partial class sceNetAdhoc
	{
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
