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
		public String Name;
		public uint Low { get; protected set; }
		public uint High { get; protected set; }
		public int Size { get { return (int)(High - Low); } }
		private MemoryPartition _ParentPartition;
		private SortedSet<MemoryPartition> _ChildPartitions;

		public MemoryPartition Root
		{
			get
			{
				return (_ParentPartition != null) ? _ParentPartition.Root : this;
			}
		}

		public IEnumerable<MemoryPartition> ChildPartitions
		{
			get
			{
				return _ChildPartitions;
			}
		}

		public MemoryPartition(uint Low, uint High, bool Allocated = true, string Name = "Unknown", MemoryPartition ParentPartition = null)
		{
			if (Low > High) throw (new InvalidOperationException());
			this._ParentPartition = ParentPartition;
			this.Name = Name;
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
							false,
							ParentPartition: this
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
			return Allocate((int)(High - Low), Anchor.Set, Low);
		}

		public MemoryPartition AllocateLowSize(uint Low, int Size)
		{
			return Allocate(Size, Anchor.Set, Low);
		}

		public MemoryPartition Allocate(int Size, Anchor AllocateAnchor = Anchor.Low, uint Position = 0, int Alignment = 1)
		{
			if (_ChildPartitions.Count == 0)
			{
				_ChildPartitions.Add(new MemoryPartition(Low, High, false, ParentPartition: this));
			}
			MemoryPartition OldFreePartition;
			MemoryPartition NewPartiton;

			try
			{
				var SizeCheck = Size;

				// As much we will need those space.
				SizeCheck += (Alignment - 1);

				var AcceptablePartitions = _ChildPartitions.Where(Partition => !Partition.Allocated && Partition.Size >= SizeCheck);

				switch (AllocateAnchor)
				{
					default:
					case Anchor.Low:
						OldFreePartition = AcceptablePartitions.First();
						break;
					case Anchor.High:
						OldFreePartition = AcceptablePartitions.Last();
						break;
					case Anchor.Set:
						OldFreePartition = AcceptablePartitions.Where(Partition => (Partition.Low <= Position) && (Partition.High >= Position + Size)).Single();
						break;
				}
			}
			catch (InvalidOperationException)
			{
				Console.WriteLine("");
				Console.WriteLine("");
				Root.Dump(this);
				Console.WriteLine("");
				throw (new InvalidOperationException(
					String.Format(
						"Can't allocate Size={0} : AllocateAnchor={1} : Position=0x{2:X}",
						Size, AllocateAnchor, Position
					)
				));
			}

			if (Alignment > 1)
			{
				switch (AllocateAnchor)
				{
					default:
					case Anchor.Low:
						{
							var Low = MathUtils.NextAligned(OldFreePartition.Low, Alignment);
							var High = (uint)(Low + Size);
							return AllocateLowHigh(Low, High);
						}
					case Anchor.High:
						{
							var High = MathUtils.PrevAligned(OldFreePartition.High, Alignment);
							var Low = (uint)(High - Size);
							Console.WriteLine("Low: {0:X}, High: {1:X}", Low, High);
							return AllocateLowHigh(Low, High);
						}
				}
			}

			_ChildPartitions.Remove(OldFreePartition);

			switch (AllocateAnchor)
			{
				default:
				case Anchor.Low:
					_ChildPartitions.Add(NewPartiton = new MemoryPartition(OldFreePartition.Low, (uint)(OldFreePartition.Low + Size), true, ParentPartition: this));
					_ChildPartitions.Add(new MemoryPartition((uint)(OldFreePartition.Low + Size), OldFreePartition.High, false, ParentPartition: this));
					break;
				case Anchor.High:
					_ChildPartitions.Add(NewPartiton = new MemoryPartition(OldFreePartition.Low, (uint)(OldFreePartition.High - Size), true, ParentPartition: this));
					_ChildPartitions.Add(new MemoryPartition((uint)(OldFreePartition.High - Size), OldFreePartition.High, false, ParentPartition: this));
					break;
				case Anchor.Set:
					_ChildPartitions.Add(new MemoryPartition(OldFreePartition.Low, Position, false, ParentPartition: this));
					_ChildPartitions.Add(NewPartiton = new MemoryPartition(Position, (uint)(Position + Size), true, ParentPartition: this));
					_ChildPartitions.Add(new MemoryPartition((uint)(Position + Size), OldFreePartition.High, false, ParentPartition: this));
					break;
			}

			NormalizePartitions();

			return NewPartiton;
		}

		public void Dump(MemoryPartition Mark = null, int Level = 0)
		{
			Console.Write(new String(' ', Level * 2));
			Console.WriteLine(String.Format(
				"MemoryPartition(Low={0:X}, High={1:X}, Allocated={2}, Size={3}){4}",
				Low,
				High,
				Allocated,
				Size,
				(this == Mark) ? " * " : ""
			));
			foreach (var ChildPartition in ChildPartitions)
			{
				ChildPartition.Dump(Level: Level + 1, Mark: Mark);
			}
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
