using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSPspEmu.Hle.Attributes;

namespace CSPspEmu.Hle.Modules.pspnet
{
	[HlePspModule(ModuleFlags = ModuleFlags.KernelMode | (ModuleFlags)0x00010011)]
	unsafe public class sceNetAdhocMatching : HleModuleHost
	{
		/// <summary>
		/// Initialise the Adhoc matching library
		/// </summary>
		/// <param name="memsize">Internal memory pool size. Lumines uses 0x20000</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x2A2A1E07, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingInit(int memsize)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Terminate the Adhoc matching library
		/// </summary>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x7945ECDA, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingTerm()
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Create an Adhoc matching object
		/// </summary>
		/// <param name="mode">One of ::pspAdhocMatchingModes</param>
		/// <param name="maxPeers">Maximum number of peers to match (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST)</param>
		/// <param name="port">Port. Lumines uses 0x22B</param>
		/// <param name="bufSize">Receiving buffer size</param>
		/// <param name="helloDelay">Hello message send delay in microseconds (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST or PSP_ADHOC_MATCHING_MODE_PTP)</param>
		/// <param name="pingDelay">Ping send delay in microseconds. Lumines uses 0x5B8D80 (only used when mode is PSP_ADHOC_MATCHING_MODE_HOST or PSP_ADHOC_MATCHING_MODE_PTP)</param>
		/// <param name="initCount">Initial count of the of the resend counter. Lumines uses 3</param>
		/// <param name="msgDelay">Message send delay in microseconds</param>
		/// <param name="callback">Callback to be called for matching</param>
		/// <returns>ID of object on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xCA5EDA6F, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingCreate(int mode, int maxPeers, int port, int bufSize, int helloDelay, int pingDelay, int initCount, int msgDelay, void* callback)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Start a matching object
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="evthPri">Priority of the event handler thread. Lumines uses 0x10</param>
		/// <param name="evthStack">Stack size of the event handler thread. Lumines uses 0x2000</param>
		/// <param name="inthPri">Priority of the input handler thread. Lumines uses 0x10</param>
		/// <param name="inthStack">Stack size of the input handler thread. Lumines uses 0x2000</param>
		/// <param name="optLen">Size of hellodata</param>
		/// <param name="optData">Pointer to block of data passed to callback</param>
		/// <returns>0 on success, less than 0 on error</returns>
		[HlePspFunction(NID = 0x93EF3843, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingStart(int matchingId, int evthPri, int evthStack, int inthPri, int inthStack, int optLen, void* optData)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Stop a matching object
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x32B156B3, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingStop(int matchingId)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Delete an Adhoc matching object
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xF16EAF4F, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingDelete(int matchingId)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Send data to a matching target
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="macAddr">The MAC address to send the data to</param>
		/// <param name="dataLen">Length of the data</param>
		/// <param name="data">Pointer to the data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xF79472D7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingSendData(int matchingId, byte* macAddr, int dataLen, byte* data)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Abort a data send to a matching target
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="macAddr">The MAC address to send the data to</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xEC19337D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingAbortSendData(int matchingId, byte* macAddr)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Select a matching target
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="macAddr">MAC address to select</param>
		/// <param name="optLen">Optional data length</param>
		/// <param name="optData">Pointer to the optional data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x5E3D4B79, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingSelectTarget(int matchingId, void* macAddr, int optLen, void* optData)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Cancel a matching target
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="macAddr">The MAC address to cancel</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xEA3C6108, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingCancelTarget(int matchingId, void* macAddr)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Cancel a matching target (with optional data)
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="macAddr">The MAC address to cancel</param>
		/// <param name="optLen">Optional data length</param>
		/// <param name="optData">Pointer to the optional data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x8F58BEDF, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingCancelTargetWithOpt(int matchingId, void* macAddr, int optLen, void* optData)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get the optional hello message
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="optLenAddr">Length of the hello data (input/output)</param>
		/// <param name="optData">Pointer to the hello data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB5D96C2A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetHelloOpt(int matchingId, uint* optLenAddr, void* optData)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Set the optional hello message
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="optLen">Length of the hello data</param>
		/// <param name="optData">Pointer to the hello data</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xB58E61B7, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingSetHelloOpt(int matchingId, int optLen, void* optData)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get a list of matching members
		/// </summary>
		/// <param name="matchingId">The ID returned from ::sceNetAdhocMatchingCreate</param>
		/// <param name="sizeAddr">The length of the list.</param>
		/// <param name="buf">An allocated area of size length.</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0xC58BCD9E, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetMembers(int matchingId, uint* sizeAddr, void* buf)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get the status of the memory pool used by the matching library
		/// </summary>
		/// <param name="poolstat">A ::pspAdhocPoolStat.</param>
		/// <returns>0 on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x9C5CFB7D, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetPoolStat(int poolstat)
		{
			throw (new NotImplementedException());
		}

		/// <summary>
		/// Get the maximum memory usage by the matching library
		/// </summary>
		/// <returns>The memory usage on success, less than 0 on error.</returns>
		[HlePspFunction(NID = 0x40F8F435, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceNetAdhocMatchingGetPoolMaxAlloc()
		{
			throw (new NotImplementedException());
		}
	}
}
