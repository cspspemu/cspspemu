using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using System.Runtime.InteropServices;

namespace CSPspEmu.Hle.Threading.EventFlags
{
	unsafe public class HleEventFlag
	{
		public AttributesSet Attributes
		{
			get { return Info.Attributes; }
			set { Info.Attributes = value; }
		}
		public EventFlagInfo Info = new EventFlagInfo();
		public uint BitPattern
		{
			get { return Info.CurrentPattern; }
			set { Info.CurrentPattern = value; }
		}
		protected List<WaitThread> WaitingThreads = new List<WaitThread>();

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
			WaitingThreads.Add(WaitThread);
			UpdateWaitingThreads();
		}

		protected void UpdateWaitingThreads()
		{
			foreach (var WaitingThread in WaitingThreads.ToArray())
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
						BitPattern &= ~WaitingThread.BitsToMatch;
						//Matching
						//throw(new NotImplementedException());
					}
					else if (WaitingThread.WaitType.HasFlag(EventFlagWaitTypeSet.ClearAll))
					{
						BitPattern = 0;
						//throw (new NotImplementedException());
					}
					WaitingThreads.Remove(WaitingThread);
					WaitingThread.WakeUpCallback();
					//Console.Error.WriteLine("WAKE UP!!");
				}
			}

			Info.NumberOfWaitingThreads = WaitingThreads.Count;
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
			BitPattern &= BitsToClear;
			UpdateWaitingThreads();
		}

		public unsafe bool Poll(uint BitsToMatch, EventFlagWaitTypeSet WaitType, uint* CheckedBits)
		{
			if (CheckedBits != null)
			{
				*CheckedBits = BitPattern;
			}
			if (WaitType.HasFlag(EventFlagWaitTypeSet.Or))
			{
				return (BitPattern & BitsToMatch) != 0;
			}
			else
			{
				return (BitPattern & BitsToMatch) == BitsToMatch;
			}
		}

		public void Set(uint Bits)
		{
			BitPattern |= Bits;
			UpdateWaitingThreads();
			//BitPattern = Bits;
		}
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
	unsafe public struct EventFlagInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public int Size;

		/// <summary>
		/// 
		/// </summary>
		public fixed byte Name[32];

		/// <summary>
		/// 
		/// </summary>
		public HleEventFlag.AttributesSet Attributes;

		/// <summary>
		/// 
		/// </summary>
		public uint InitialPattern;

		/// <summary>
		/// 
		/// </summary>
		public uint CurrentPattern;

		/// <summary>
		/// 
		/// </summary>
		public int NumberOfWaitingThreads;
	}

	public struct SceKernelEventFlagOptParam
	{
		public int size;
	}
}
