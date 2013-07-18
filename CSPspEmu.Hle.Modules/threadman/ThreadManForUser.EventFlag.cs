using System;
using CSPspEmu.Hle.Managers;
using CSPspEmu.Hle.Threading.EventFlags;
using CSharpUtils;
using CSPspEmu.Core;

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
		/// <param name="Attributes">Attributes from PspEventFlagAttributes</param>
		/// <param name="BitPattern">Initial bit pattern.</param>
		/// <param name="OptionsPtr">Options, set to NULL</param>
		/// <returns>less than 0 on error. greater or equal to 0 event flag id.</returns>
		[HlePspFunction(NID = 0x55C20A00, FirmwareVersion = 150)]
		public HleEventFlag sceKernelCreateEventFlag(string Name, HleEventFlag.AttributesSet Attributes, uint BitPattern, SceKernelEventFlagOptParam* OptionsPtr)
		{
			if (OptionsPtr != null) throw (new NotImplementedException("(OptionsPtr != null)"));

			return new HleEventFlag()
			{
				Name = Name,
				Info = new EventFlagInfo(0)
				{
					Name = Name,
					Attributes = Attributes,
					InitialPattern = BitPattern,
					CurrentPattern = BitPattern,
				},
			};
		}

		/// <summary>
		/// Delete an event flag
		/// </summary>
		/// <param name="EventId">The event ID returned by <see cref="sceKernelCreateEventFlag"/>.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0xEF9E4C70, FirmwareVersion = 150)]
		public int sceKernelDeleteEventFlag(HleEventFlag EventFlag)
		{
			EventFlag.RemoveUid(InjectContext);
			return 0;
		}

		/// <summary>
		/// Clear a event flag bit pattern
		/// </summary>
		/// <param name="EventId">The event ID returned by <see cref="sceKernelCreateEventFlag"/></param>
		/// <param name="BitsToClear">The bits to clean</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x812346E4, FirmwareVersion = 150)]
		public int sceKernelClearEventFlag(HleEventFlag EventFlag, uint BitsToClear)
		{
			EventFlag.ClearBits(BitsToClear);
			return 0;
		}

		/// <summary>
		/// Wait for an event flag for a given bit pattern with callback.
		/// </summary>
		/// <param name="EventId">The event ID returned by <see cref="sceKernelCreateEventFlag"/>.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="Wait">Wait type, one or more of PspEventFlagWaitTypes or'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <param name="Timeout">Timeout in microseconds</param>
		/// <param name="HandleCallbacks"></param>
		/// <returns>
		///		ERROR_KERNEL_NOT_FOUND_EVENT_FLAG - If can't find the eventFlag
		///		ERROR_KERNEL_WAIT_TIMEOUT         - If there was a timeout
		///		0                                 - On success
		/// </returns>
		public int _sceKernelWaitEventFlagCB(HleEventFlag EventFlag, uint Bits, EventFlagWaitTypeSet Wait, uint* OutBits, uint* Timeout, bool HandleCallbacks)
		{
			if ((Wait & ~(EventFlagWaitTypeSet.MaskValidBits)) != 0) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_ILLEGAL_MODE));
			if (Bits == 0) throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN));
			bool TimedOut = false;

			var PreviousPattern = EventFlag.Info.CurrentPattern;

			ThreadManager.Current.SetWaitAndPrepareWakeUp(
				HleThread.WaitType.Semaphore,
				String.Format("_sceKernelWaitEventFlagCB(EventId={0}, Bits={1:X}, Wait={2})", EventFlag.GetUidIndex(InjectContext), Bits, Wait),
				EventFlag,
				WakeUpCallback =>
			{
				if (Timeout != null)
				{
					PspRtc.RegisterTimerInOnce(TimeSpanUtils.FromMicroseconds(*Timeout), () =>
					{
						TimedOut = true;
						*Timeout = 0;
						WakeUpCallback();
					});
				}

				EventFlag.AddWaitingThread(new HleEventFlag.WaitThread()
				{
					HleThread = ThreadManager.Current,
					BitsToMatch = Bits,
					WaitType = Wait,
					WakeUpCallback = () => { WakeUpCallback(); },
					OutBits = OutBits,
				});
			}, HandleCallbacks: HandleCallbacks);

			if (OutBits != null)
			{
				*OutBits = PreviousPattern;
			}

			if (TimedOut)
			{
				throw(new SceKernelException(SceKernelErrors.ERROR_KERNEL_WAIT_TIMEOUT));
			}

			//throw(new NotImplementedException());
			return 0;
		}

		/// <summary>
		/// Wait for an event flag for a given bit pattern.
		/// </summary>
		/// <param name="EventId">The event ID returned by <see cref="sceKernelCreateEventFlag"/>.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="WaitType">Wait type, one or more of PspEventFlagWaitTypes OR'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <param name="Timeout">Timeout in microseconds</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x402FCF22, FirmwareVersion = 150)]
		public int sceKernelWaitEventFlag(HleEventFlag EventFlag, uint Bits, EventFlagWaitTypeSet WaitType, uint* OutBits, uint* Timeout)
		{
			return _sceKernelWaitEventFlagCB(EventFlag, Bits, WaitType, OutBits, Timeout, false);
		}

		/// <summary>
		/// Wait for an event flag for a given bit pattern with callback.
		/// </summary>
		/// <param name="EventId">The event ID returned by <see cref="sceKernelCreateEventFlag"/>.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="WaitType">Wait type, one or more of PspEventFlagWaitTypes OR'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <param name="Timeout">Timeout in microseconds</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x328C546A, FirmwareVersion = 150)]
		public int sceKernelWaitEventFlagCB(HleEventFlag EventFlag, uint Bits, EventFlagWaitTypeSet WaitType, uint* OutBits, uint* Timeout)
		{
			return _sceKernelWaitEventFlagCB(EventFlag, Bits, WaitType, OutBits, Timeout, true);
		}

		/// <summary>
		/// Set an event flag bit pattern.
		/// </summary>
		/// <param name="EventId">The event ID returned by <see cref="sceKernelCreateEventFlag"/>.</param>
		/// <param name="BitPattern">The bit pattern to set.</param>
		/// <returns>Less than 0 on error</returns>
		[HlePspFunction(NID = 0x1FB15A32, FirmwareVersion = 150)]
		public int sceKernelSetEventFlag(HleEventFlag EventFlag, uint BitPattern)
		{
			//Console.WriteLine("FLAG:{0} : {1:X}", EventId, BitPattern);
			EventFlag.Set(BitPattern);
			return 0;
		}

		/// <summary>
		/// Poll an event flag for a given bit pattern.
		/// </summary>
		/// <param name="EventId">The event ID returned by <see cref="sceKernelCreateEventFlag"/>.</param>
		/// <param name="Bits">The bit pattern to poll for.</param>
		/// <param name="WaitType">Wait type, one or more of ::PspEventFlagWaitTypes or'ed together</param>
		/// <param name="OutBits">The bit pattern that was matched.</param>
		/// <returns>
		///		0 on Success
		///		ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN
		///		ERROR_KERNEL_EVENT_FLAG_POLL_FAILED
		///	</returns>
		[HlePspFunction(NID = 0x30FD48F0, FirmwareVersion = 150)]
		public int sceKernelPollEventFlag(HleEventFlag EventFlag, uint Bits, EventFlagWaitTypeSet WaitType, uint* OutBits)
		{
			if (Bits == 0)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_EVENT_FLAG_ILLEGAL_WAIT_PATTERN));
			}

			bool Matched = EventFlag.Poll(Bits, WaitType, OutBits);

			if (!Matched)
			{
				throw (new SceKernelException(SceKernelErrors.ERROR_KERNEL_EVENT_FLAG_POLL_FAILED));
			}

			return 0;
		}

		/// <summary>
		/// Get the status of an event flag.
		/// </summary>
		/// <param name="EventId">The UID of the event.</param>
		/// <param name="Info">A pointer to a SceKernelEventFlagInfo structure.</param>
		/// <returns>less than 0 on error</returns>
		[HlePspFunction(NID = 0xA66B0120, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelReferEventFlagStatus(HleEventFlag EventFlag, out EventFlagInfo Info)
		{
			Info = EventFlag.Info;
			//Console.WriteLine(Info);
			return 0;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="EventId"></param>
		/// <param name="NewPattern"></param>
		/// <param name="NumWaitThread"></param>
		/// <returns></returns>
		[HlePspFunction(NID = 0xCD203292, FirmwareVersion = 150)]
		[HlePspNotImplemented]
		public int sceKernelCancelEventFlag(HleEventFlag EventFlag, int NewPattern, int* NumWaitThread)
		{
			foreach (var WaitingThread in EventFlag.WaitingThreads)
			{
				WaitingThread.WakeUpCallback();
			}
			
			//throw(new NotImplementedException());
			return 0;
		}
	}
}
