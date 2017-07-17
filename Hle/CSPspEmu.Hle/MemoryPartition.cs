using System;
using System.Collections.Generic;
using System.Linq;
using CSharpUtils;
using CSPspEmu.Core.Memory;
using System.Runtime.InteropServices;
using CSharpUtils.Extensions;

namespace CSPspEmu.Hle
{
    public class MemoryPartitionNoMemoryException : Exception
    {
        public MemoryPartitionNoMemoryException(string Text)
            : base(Text)
        {
        }
    }

    [HleUidPoolClass(NotFoundError = SceKernelErrors.ERROR_KERNEL_ILLEGAL_PARTITION_ID, FirstItem = 1)]
    public unsafe class MemoryPartition : IHleUidPoolClass, IDisposable
    {
        public enum Anchor : int
        {
            Low = 1,
            High = 2,
            Set = 3,
        }

        [Inject] InjectContext InjectContext;

        [Inject] PspMemory PspMemory;

        public bool Allocated;
        public String Name;

        public void* GetLowPointerSafe<TType>()
        {
            return PspMemory.PspAddressToPointerSafe(Low, Marshal.SizeOf(typeof(TType)));
        }

        public uint Low { get; protected set; }
        public uint High { get; protected set; }

        public int Size
        {
            get { return (int) (High - Low); }
        }

        public MemoryPartition ParentPartition { get; private set; }
        private SortedSet<MemoryPartition> _ChildPartitions;

        public void* LowPointer
        {
            get { return PspMemory.PspPointerToPointerSafe(Low, Size); }
        }

        public uint GetAnchoredAddress(Anchor Anchor)
        {
            switch (Anchor)
            {
                case MemoryPartition.Anchor.High: return High;
                case MemoryPartition.Anchor.Low: return Low;
            }
            throw (new InvalidOperationException("Invalid Anchor Value : " + Anchor));
        }

        public MemoryPartition Root
        {
            get { return (ParentPartition != null) ? ParentPartition.Root : this; }
        }

        public int MaxFreeSize
        {
            get
            {
                return ChildPartitions
                        .Where(Partition => !Partition.Allocated)
                        .OrderByDescending(Partition => Partition.Size)
                        .FirstOrDefault(new MemoryPartition(InjectContext, 0, 0))
                        .Size
                    ;
            }
        }

        public int TotalFreeSize
        {
            get
            {
                return ChildPartitions
                        .Where(Partition => !Partition.Allocated)
                        .Aggregate(0, (Accumulated, Partition) => Accumulated + Partition.Size)
                    ;
            }
        }

        public IEnumerable<MemoryPartition> ChildPartitions
        {
            get { return _ChildPartitions; }
        }

        public MemoryPartition(InjectContext InjectContext, uint Low, uint High, bool Allocated = true,
            string Name = "<Unknown>", MemoryPartition ParentPartition = null)
        {
            if (Low > High) throw (new InvalidOperationException());
            InjectContext.InjectDependencesTo(this);
            this.ParentPartition = ParentPartition;
            this.Name = Name;
            this.Low = Low;
            this.High = High;
            this.Allocated = Allocated;
            this._ChildPartitions = new SortedSet<MemoryPartition>(new AnonymousComparer<MemoryPartition>(
                (Left, Right) =>
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
                            InjectContext,
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

        public void DeallocateAnchoredAddress(uint Address, Anchor Anchor)
        {
            if (Anchor == MemoryPartition.Anchor.Low)
            {
                DeallocateLow(Address);
                return;
            }
            if (Anchor == MemoryPartition.Anchor.High)
            {
                DeallocateHigh(Address);
                return;
            }
            throw(new InvalidOperationException());
        }

        public MemoryPartition AllocateLowHigh(uint Low, uint High, string Name = "<Unknown>")
        {
            return Allocate((int) (High - Low), Anchor.Set, Low, Name: Name);
        }

        public MemoryPartition AllocateLowSize(uint Low, int Size, string Name = "<Unknown>")
        {
            return Allocate(Size, Anchor.Set, Low, Name: Name);
        }

        public MemoryPartition AllocateItem<T>(T Value) where T : struct
        {
            var Partition = Allocate(Marshal.SizeOf(typeof(T)));
            var PartitionStream = new PspMemoryStream(PspMemory).SliceWithBounds(Partition.Low, Partition.High);
            PartitionStream.WriteStruct(Value);
            return Partition;
        }

        public MemoryPartition AllocateItem(string Value)
        {
            var Partition = Allocate(Value.Length + 1);
            var PartitionStream = new PspMemoryStream(PspMemory).SliceWithBounds(Partition.Low, Partition.High);
            PartitionStream.WriteStringz(Value);
            return Partition;
        }

        public MemoryPartition Allocate(int Size, Anchor AllocateAnchor = Anchor.Low, uint Position = 0,
            int Alignment = 1, string Name = "<Unknown>")
        {
            try
            {
                if (_ChildPartitions.Count == 0)
                {
                    _ChildPartitions.Add(new MemoryPartition(InjectContext, Low, High, false, ParentPartition: this,
                        Name: Name));
                }
                MemoryPartition OldFreePartition;
                MemoryPartition NewPartiton;

                var SizeCheck = Size;

                // As much we will need those space.
                SizeCheck += (Alignment - 1);

                var AcceptablePartitions =
                    _ChildPartitions.Where(Partition => !Partition.Allocated && Partition.Size >= SizeCheck);

                //Console.Error.WriteLine("{0}, {1}", AcceptablePartitions.Count(), AcceptablePartitions.First().Size);

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
                        OldFreePartition = AcceptablePartitions.Single(Partition =>
                            (Partition.Low <= Position) && (Partition.High >= Position + Size));
                        break;
                }

                if (Alignment > 1)
                {
                    switch (AllocateAnchor)
                    {
                        default:
                        case Anchor.Low:
                        {
                            var Low = MathUtils.NextAligned(OldFreePartition.Low, Alignment);
                            var High = (uint) (Low + Size);
                            return AllocateLowHigh(Low, High);
                        }
                        case Anchor.High:
                        {
                            var High = MathUtils.PrevAligned(OldFreePartition.High, Alignment);
                            var Low = (uint) (High - Size);
                            //Console.WriteLine("Low: {0:X}, High: {1:X}", Low, High);
                            return AllocateLowHigh(Low, High);
                        }
                    }
                }

                _ChildPartitions.Remove(OldFreePartition);

                switch (AllocateAnchor)
                {
                    default:
                    case Anchor.Low:
                        _ChildPartitions.Add(NewPartiton = new MemoryPartition(InjectContext, OldFreePartition.Low,
                            (uint) (OldFreePartition.Low + Size), true, ParentPartition: this, Name: Name));
                        _ChildPartitions.Add(new MemoryPartition(InjectContext, (uint) (OldFreePartition.Low + Size),
                            OldFreePartition.High, false, ParentPartition: this, Name: "<Free>"));
                        break;
                    case Anchor.High:
                        _ChildPartitions.Add(NewPartiton = new MemoryPartition(InjectContext,
                            (uint) (OldFreePartition.High - Size), OldFreePartition.High, true, ParentPartition: this,
                            Name: Name));
                        _ChildPartitions.Add(new MemoryPartition(InjectContext, OldFreePartition.Low,
                            (uint) (OldFreePartition.High - Size), false, ParentPartition: this, Name: "<Free>"));
                        break;
                    case Anchor.Set:
                        _ChildPartitions.Add(new MemoryPartition(InjectContext, OldFreePartition.Low, Position, false,
                            ParentPartition: this, Name: "<Free>"));
                        _ChildPartitions.Add(NewPartiton = new MemoryPartition(InjectContext, Position,
                            (uint) (Position + Size), true, ParentPartition: this, Name: Name));
                        _ChildPartitions.Add(new MemoryPartition(InjectContext, (uint) (Position + Size),
                            OldFreePartition.High, false, ParentPartition: this, Name: "<Free>"));
                        break;
                }

                NormalizePartitions();

                return NewPartiton;
            }
            catch (InvalidOperationException)
            {
#if false
					Console.WriteLine("");
					Console.WriteLine("");
					Root.Dump(this);
					Console.WriteLine("");
#endif
                throw (new MemoryPartitionNoMemoryException(
                    String.Format(
                        "Can't allocate Size={0} : AllocateAnchor={1} : Position=0x{2:X}",
                        Size, AllocateAnchor, Position
                    )
                ));
            }
        }

        public void Dump(MemoryPartition Mark = null, int Level = 0)
        {
            Console.Write(new String(' ', Level * 2));
            Console.WriteLine(String.Format(
                "MemoryPartition(Low={0:X}, High={1:X}, Allocated={2}, Size={3}, Name='{4}'){5}",
                Low,
                High,
                Allocated,
                Size,
                Name,
                (this == Mark) ? " * " : ""
            ));
            foreach (var ChildPartition in ChildPartitions)
            {
                ChildPartition.Dump(Level: Level + 1, Mark: Mark);
            }
        }

        /// <summary>
        /// Deallocates this partition from the parent partition.
        /// </summary>
        public void DeallocateFromParent()
        {
            ParentPartition.DeallocateLow(Low);
        }

        public override string ToString()
        {
            if ((_ChildPartitions != null) && _ChildPartitions.Count > 0)
            {
                return String.Format(
                    "MemoryPartition(Low={0:X}, High={1:X}, Allocated={2}, Name='{3}', Size={4}, ChildPartitions=[{5}])",
                    Low, High, Allocated,
                    Name,
                    Size,
                    String.Join(",", _ChildPartitions)
                );
            }
            else
            {
                return String.Format(
                    "MemoryPartition(Low={0:X}, High={1:X}, Allocated={2}, Name='{3}', Size={4})",
                    Low, High, Allocated,
                    Name,
                    Size
                );
            }
        }

        public void Dispose()
        {
        }
    }
}