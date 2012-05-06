using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Threading.EventFlags
{
	unsafe public class HleEventFlag : IDisposable
	{
		public EventFlagInfo Info = new EventFlagInfo(0);
		protected List<WaitThread> _WaitingThreads = new List<WaitThread>();

		public IEnumerable<WaitThread> WaitingThreads
		{
			get
			{
				return _WaitingThreads;
			}
		}

		public class WaitThread
		{
			public HleThread HleThread;
			public Action WakeUpCallback;
			public uint BitsToMatch;
			public EventFlagWaitTypeSet WaitType;

			public override string ToString()
			{
				return String.Format(
					"HleEventFlag.WaitThread({0}, {1}, {2})",
					HleThread, BitsToMatch, WaitType
				);
			}

			public unsafe uint* OutBits;
		}

		public string Name
		{
			get
			{
				fixed (byte* NamePointer = Info.Name)
				{
					return PointerUtils.PtrToString(NamePointer, Encoding.ASCII);
				}
			}
			set
			{
				fixed (byte* NamePointer = Info.Name)
				{
					PointerUtils.StoreStringOnPtr(value, Encoding.ASCII, NamePointer, 32);
				}
			}
		}

		public void AddWaitingThread(WaitThread WaitThread)
		{
			_WaitingThreads.Add(WaitThread);
			UpdateWaitingThreads();
		}

		protected void UpdateWaitingThreads()
		{
			foreach (var WaitingThread in _WaitingThreads.ToArray())
			{
				uint Matching = 0;
				//Console.Error.WriteLine("");
				//Console.Error.WriteLine("|| " + WaitingThread + " || ");
				//Console.Error.WriteLine("");
				if (Poll(WaitingThread.BitsToMatch, WaitingThread.WaitType, &Matching))
				{
					if (WaitingThread.OutBits != null)
					{
						*WaitingThread.OutBits = Matching;
					}
					if (WaitingThread.WaitType.HasFlag(EventFlagWaitTypeSet.Clear))
					{
						Info.CurrentPattern &= ~WaitingThread.BitsToMatch;
						//Matching
						//throw(new NotImplementedException());
					}
					else if (WaitingThread.WaitType.HasFlag(EventFlagWaitTypeSet.ClearAll))
					{
						Info.CurrentPattern = 0;
						//throw (new NotImplementedException());
					}
					_WaitingThreads.Remove(WaitingThread);
					WaitingThread.WakeUpCallback();
					//Console.Error.WriteLine("WAKE UP!!");
				}
			}

			Info.NumberOfWaitingThreads = _WaitingThreads.Count;
		}

		/// <summary>
		/// 
		/// </summary>
		public enum AttributesSet : uint
		{
			/// <summary>
			/// Allow the event flag to be waited upon by multiple threads
			/// </summary>
			PSP_EVENT_WAITMULTIPLE = 0x200,
		}

		/// <summary>
		/// Remove bits from BitPattern
		/// </summary>
		/// <param name="BitsToClear"></param>
		public void ClearBits(uint BitsToClear)
		{
			Info.CurrentPattern &= BitsToClear;
			UpdateWaitingThreads();
		}

		public unsafe bool Poll(uint BitsToMatch, EventFlagWaitTypeSet WaitType, uint* CheckedBits)
		{
			if (CheckedBits != null)
			{
				*CheckedBits = Info.CurrentPattern;
			}

			if (WaitType.HasFlag(EventFlagWaitTypeSet.Or))
			{
				return (Info.CurrentPattern & BitsToMatch) != 0;
			}
			else
			{
				return (Info.CurrentPattern & BitsToMatch) == BitsToMatch;
			}
		}

		public void Set(uint Bits)
		{
			Info.CurrentPattern |= Bits;
			UpdateWaitingThreads();
			//BitPattern = Bits;
		}

		void IDisposable.Dispose()
		{
			// TODO
		}
	}

	/// <summary>
	/// Event flag wait types
	/// </summary>
	[Flags]
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
	unsafe public struct EventFlagInfo
	{
		/// <summary>
		/// 0x0000 - 
		/// </summary>
		public int Size;

		/// <summary>
		/// 0x0004 - 
		/// </summary>
		public fixed byte Name[32];

		/// <summary>
		/// 0x0024 - 
		/// </summary>
		public HleEventFlag.AttributesSet Attributes;

		/// <summary>
		/// 0x0028 - 
		/// </summary>
		public uint InitialPattern;

		/// <summary>
		/// 0x002C - 
		/// </summary>
		public uint CurrentPattern;

		/// <summary>
		/// 0x0030 -
		/// </summary>
		public int NumberOfWaitingThreads;

		/// <summary>
		/// 
		/// </summary>
		public EventFlagInfo(int Dummy)
		{
			Size = sizeof(EventFlagInfo);
			//Name[0] = 0;
			Attributes = (HleEventFlag.AttributesSet)0;
			InitialPattern = 0;
			CurrentPattern = 0;
			NumberOfWaitingThreads = 0;
		}

		public override string ToString()
		{
			return this.ToStringDefault();
		}
	}

	public struct SceKernelEventFlagOptParam
	{
		public int size;
	}
}
