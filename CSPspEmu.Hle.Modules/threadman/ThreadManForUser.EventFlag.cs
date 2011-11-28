using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Modules.threadman
{
	public unsafe partial class ThreadManForUser
	{
		/// <summary>
		/// Create an event flag.
		/// </summary>
		/// <example>
		///		int evid;
		///		evid = sceKernelCreateEventFlag("wait_event", 0, 0, 0);
		/// </example>
		/// <param name="Name">The name of the event flag.</param>
		/// <param name="Attributes">Attributes from ::PspEventFlagAttributes</param>
		/// <param name="BitPattern">Initial bit pattern.</param>
		/// <param name="OptionsPtr">Options, set to NULL</param>
		/// <returns>less than 0 on error. greater or equal to 0 event flag id.</returns>
		[HlePspFunction(NID = 0x55C20A00, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelCreateEventFlag(string Name, PspEventFlagAttributes Attributes, uint BitPattern, SceKernelEventFlagOptParam* OptionsPtr)
		{
			return 0;
		}

		/// <summary>
		/// Delete an event flag
		/// </summary>
		/// <param name="EventId">The event id returned by sceKernelCreateEventFlag.</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0xEF9E4C70, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelDeleteEventFlag(int EventId)
		{
			return 0;
		}

		/// <summary>
		/// Clear a event flag bit pattern
		/// </summary>
		/// <param name="EventId">The event id returned by ::sceKernelCreateEventFlag</param>
		/// <param name="BitsToClear">The bits to clean</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x812346E4, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelClearEventFlag(int EventId, uint BitsToClear)
		{
			return 0;
		}

		/// <summary>
		/// Wait for an event flag for a given bit pattern with callback.
		/// </summary>
		/// <param name="EventId">The event id returned by sceKernelCreateEventFlag.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="Wait">Wait type, one or more of ::PspEventFlagWaitTypes or'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <param name="Timeout">Timeout in microseconds</param>
		/// <param name="Callback"></param>
		/// <returns>
		///		ERROR_KERNEL_NOT_FOUND_EVENT_FLAG - If can't find the eventFlag
		///		ERROR_KERNEL_WAIT_TIMEOUT         - If there was a timeout
		///		0                                 - On success
		/// </returns>
		public int _sceKernelWaitEventFlagCB(int EventId, uint Bits, EventFlagWaitTypeSet Wait, uint* OutBits, uint* Timeout, bool Callback)
		{
			throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Wait for an event flag for a given bit pattern.
		/// </summary>
		/// <param name="EventId">The event id returned by sceKernelCreateEventFlag.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="WaitType">Wait type, one or more of ::PspEventFlagWaitTypes or'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <param name="Timeout">Timeout in microseconds</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x402FCF22, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelWaitEventFlag(int EventId, uint Bits, EventFlagWaitTypeSet WaitType, uint* OutBits, uint* Timeout)
		{
			return _sceKernelWaitEventFlagCB(EventId, Bits, WaitType, OutBits, Timeout, false);
		}

		/// <summary>
		/// Wait for an event flag for a given bit pattern with callback.
		/// </summary>
		/// <param name="EventId">The event id returned by sceKernelCreateEventFlag.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="WaitType">Wait type, one or more of ::PspEventFlagWaitTypes or'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <param name="Timeout">Timeout in microseconds</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x328C546A, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelWaitEventFlagCB(int EventId, uint Bits, EventFlagWaitTypeSet WaitType, uint* OutBits, uint* Timeout)
		{
			return _sceKernelWaitEventFlagCB(EventId, Bits, WaitType, OutBits, Timeout, true);
		}

		/// <summary>
		/// Set an event flag bit pattern.
		/// </summary>
		/// <param name="EventId">The event id returned by sceKernelCreateEventFlag.</param>
		/// <param name="Bits">The bit pattern to set.</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x1FB15A32, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelSetEventFlag(int EventId, uint Bits)
		{
			return 0;
		}

		/// <summary>
		/// Poll an event flag for a given bit pattern.
		/// </summary>
		/// <param name="EventId">The event id returned by sceKernelCreateEventFlag.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="WaitType">Wait type, one or more of ::PspEventFlagWaitTypes or'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0x30FD48F0, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelPollEventFlag(int EventId, uint Bits, EventFlagWaitTypeSet WaitType, uint* OutBits)
		{
			return 0;
		}

		/// <summary>
		/// Get the status of an event flag.
		/// </summary>
		/// <param name="EventId">The UID of the event.</param>
		/// <param name="status">A pointer to a ::SceKernelEventFlagInfo structure.</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0xA66B0120, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelReferEventFlagStatus(int EventId, EventFlagInfo* status)
		{
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		public enum PspEventFlagAttributes
		{
			/// <summary>
			/// Allow the event flag to be waited upon by multiple threads
			/// </summary>
			PSP_EVENT_WAITMULTIPLE = 0x200,
		}

		/// <summary>
		/// Event flag wait types
		/// </summary>
		public enum EventFlagWaitTypeSet : uint
		{
			/// <summary>
			/// Wait for all bits in the pattern to be set 
			/// </summary>
			And = 0x00,
			
			/// <summary>
			/// Wait for one or more bits in the pattern to be set
			/// </summary>
			Or = 0x01,

			/// <summary>
			/// Clear all the wait pattern when it matches
			/// </summary>
			ClearAll = 0x10,
			
			/// <summary>
			/// Clear the wait pattern when it matches
			/// </summary>
			Clear = 0x20,
		};

		/// <summary>
		/// Structure to hold the event flag information
		/// </summary>
		public struct EventFlagInfo
		{
			/// <summary>
			/// 
			/// </summary>
			public int        Size;

			/// <summary>
			/// 
			/// </summary>
			public fixed byte Name[32];

			/// <summary>
			/// 
			/// </summary>
			public uint       Attribute;

			/// <summary>
			/// 
			/// </summary>
			public uint       InitialPattern;

			/// <summary>
			/// 
			/// </summary>
			public uint       CurrentPattern;

			/// <summary>
			/// 
			/// </summary>
			public int        NumberOfWaitingThreads;
		}

		public struct SceKernelEventFlagOptParam
		{
			public int size;
		}
	}
}
