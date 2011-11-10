using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;

namespace CSPspEmu.Hle
{
	public class MemoryPartition
	{
		public enum Anchor
		{
			Low = 1,
			High = 2,
			Set = 3,
		}

		public bool Allocated;
		public uint Low { get; protected set; }
		public uint High { get; protected set; }
		public int Size { get { return (int)(High - Low); } }
		private SortedSet<MemoryPartition> _ChildPartitions;

		public IEnumerable<MemoryPartition> ChildPartitions
		{
			get
			{
				return _ChildPartitions;
			}
		}

		public MemoryPartition(uint Low, uint High, bool Allocated = true)
		{
			if (Low > High) throw (new InvalidOperationException());
			this.Low = Low;
			this.High = High;
			this.Allocated = Allocated;
			this._ChildPartitions = new SortedSet<MemoryPartition>(new AnonymousComparer<MemoryPartition>((Left, Right) =>
			{
				int Result = Left.Low.CompareTo(Right.Low);
				if (Result == 0)
				{
					Result = Left.High.CompareTo(Right.High);
				}
				return Result;
			}));
		}

		private void NormalizePartitions()
		{
			bool Normalized;
			do
			{
				Normalized = true;
				MemoryPartition Previous = null;
				foreach (var Current in _ChildPartitions)
				{
					if (Current.Size == 0)
					{
						_ChildPartitions.Remove(Current);
						Normalized = false;
						break;
					}

					if ((Previous != null) && (Previous.Allocated == false) && (Current.Allocated == false))
					{
						_ChildPartitions.Remove(Previous);
						_ChildPartitions.Remove(Current);
						_ChildPartitions.Add(new MemoryPartition(
							Math.Min(Previous.Low, Current.Low),
							Math.Max(Previous.High, Current.High),
							false
						));
						Normalized = false;
						break;
					}

					Previous = Current;
				}
			} while (!Normalized);
		}

		public void DeallocateLow(uint Low)
		{
			_ChildPartitions.Single(Partition => Partition.Low == Low).Allocated = false;
			NormalizePartitions();
		}

		public void DeallocateHigh(uint High)
		{
			_ChildPartitions.Single(Partition => Partition.High == High).Allocated = false;
			NormalizePartitions();
		}

		public MemoryPartition AllocateLowHigh(uint Low, uint High)
		{
			return Allocate(High - Low, Anchor.Set, Low);
		}

		public MemoryPartition AllocateLowSize(uint Low, uint Size)
		{
			return Allocate(Size, Anchor.Set, Low);
		}

		public MemoryPartition Allocate(uint Size, Anchor AllocateAnchor = Anchor.Low, uint Position = 0)
		{
			if (_ChildPartitions.Count == 0)
			{
				_ChildPartitions.Add(new MemoryPartition(Low, High, false));
			}
			MemoryPartition OldFreePartition;
			MemoryPartition NewPartiton;

			try
			{
				switch (AllocateAnchor)
				{
					default:
					case Anchor.Low:
						OldFreePartition = _ChildPartitions.First(Partition => !Partition.Allocated && Partition.Size >= Size);
						break;
					case Anchor.High:
						OldFreePartition = _ChildPartitions.Last(Partition => !Partition.Allocated && Partition.Size >= Size);
						break;
					case Anchor.Set:
						OldFreePartition = _ChildPartitions.Single(Partition => !Partition.Allocated && (Partition.Low <= Position) && (Partition.High >= Position + Size));
						break;
				}
			}
			catch (InvalidOperationException)
			{
				throw (new InvalidOperationException(
					String.Format(
						"Can't allocate Size={0} : AllocateAnchor={1} : Position=0x{2:X}",
						Size, AllocateAnchor, Position
					)
				));
			}
			_ChildPartitions.Remove(OldFreePartition);

			switch (AllocateAnchor)
			{
				default:
				case Anchor.Low:
					_ChildPartitions.Add(NewPartiton = new MemoryPartition(OldFreePartition.Low, OldFreePartition.Low + Size, true));
					_ChildPartitions.Add(new MemoryPartition(OldFreePartition.Low + Size, OldFreePartition.High, false));
					break;
				case Anchor.High:
					_ChildPartitions.Add(NewPartiton = new MemoryPartition(OldFreePartition.Low, OldFreePartition.High - Size, true));
					_ChildPartitions.Add(new MemoryPartition(OldFreePartition.High - Size, OldFreePartition.High, false));
					break;
				case Anchor.Set:
					_ChildPartitions.Add(new MemoryPartition(OldFreePartition.Low, Position, false));
					_ChildPartitions.Add(NewPartiton = new MemoryPartition(Position, Position + Size, true));
					_ChildPartitions.Add(new MemoryPartition(Position + Size, OldFreePartition.High, false));
					break;
			}

			NormalizePartitions();

			return NewPartiton;
		}

		public override string ToString()
		{
			if (_ChildPartitions.Count > 0)
			{
				return String.Format(
					"MemoryPartition(Low={0:X}, High={1:X}, Allocated={2}, ChildPartitions=[{3}])",
					Low, High, Allocated,
					String.Join(",", _ChildPartitions)
				);
			}
			else
			{
				return String.Format(
					"MemoryPartition(Low={0:X}, High={1:X}, Allocated={2})",
					Low, High, Allocated
				);
			}
		}
	}
}
