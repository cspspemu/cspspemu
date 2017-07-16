using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using System.IO;

namespace CSharpUtils.Compression.Lz
{
    unsafe public class LzMatcher
    {
        public int MaxOffset { get; }
        public int MinSize { get; }
        public int MaxSize { get; }
        public bool AllowOverlapping { get; }
        byte[] Data;
        Dictionary<int, LinkedList<int>> Waypoints = new Dictionary<int, LinkedList<int>>();
        LinkedList<int> Hashes = new LinkedList<int>();
        private int _Offset;

        public int Offset
        {
            get { return _Offset; }
            set
            {
                Waypoints.Clear();
                Hashes.Clear();
                _Offset = Math.Max(value - MaxOffset, 0);
                Skip(Math.Min(MaxOffset, value));
            }
        }

        enum Overlapping
        {
            NO = 0,
            YES = 1,
        }

        public LzMatcher(byte[] Data, int Offset = 0, int MaxOffset = 0x1000, int MinSize = 3, int MaxSize = 0x12,
            bool AllowOverlapping = true)
        {
            this.Data = Data;
            this.MaxOffset = MaxOffset;
            this.MinSize = MinSize;
            this.MaxSize = MaxSize;
            //if (!AllowOverlapping) throw (new NotImplementedException("!AllowOverlapping"));
            this.AllowOverlapping = AllowOverlapping;
            this.Offset = Offset;
        }

        public int Length
        {
            get { return Data.Length; }
        }

        int GetOffsetHash(int Offset)
        {
            fixed (byte* Ptr = &this.Data[Offset])
            {
                return PointerUtils.FastHash(Ptr, MinSize);
            }
        }

        public void Skip(int SkipCount = 1)
        {
            if (SkipCount != 1)
            {
                while (SkipCount-- > 0) Skip(1);
                return;
            }

            var Hash = GetOffsetHash(_Offset);
            Hashes.AddLast(Hash);
            if (!Waypoints.ContainsKey(Hash)) Waypoints[Hash] = new LinkedList<int>();
            Waypoints[Hash].AddLast(_Offset);

            if (Hashes.Count > MaxOffset)
            {
                var FirstHash = Hashes.First.Value;
                Hashes.RemoveFirst();
                Waypoints[FirstHash].RemoveFirst();
            }

            _Offset++;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct FindSequenceResult
        {
            /// <summary>
            /// 
            /// </summary>
            public bool Found
            {
                get { return Size > 0; }
            }

            /// <summary>
            /// 
            /// </summary>
            public int Offset;

            /// <summary>
            /// 
            /// </summary>
            public int Size;

            public override string ToString()
            {
                return String.Format("LzMatcher.FindSequenceResult(Offset={0}, Size={1})", Offset, Size);
            }
        }

        public FindSequenceResult FindMaxSequence()
        {
            var FindSequenceResult = new FindSequenceResult()
            {
                Offset = 0,
                Size = 0,
            };

            if ((Data.Length - _Offset) >= MinSize)
            {
                fixed (byte* DataPtr = this.Data)
                {
                    var Hash = GetOffsetHash(_Offset);
                    if (Waypoints.ContainsKey(Hash))
                    {
                        //var Node = Waypoints[Hash].Last;
                        foreach (var CompareOffset in Waypoints[Hash])
                        {
                            int LocalMaxSize = Math.Min((Data.Length - _Offset), MaxSize);

                            //if (!AllowOverlapping && (CompareOffset + LocalMaxSize > _Offset)) continue;

                            int MatchedLength = PointerUtils.FindLargestMatch(
                                &DataPtr[CompareOffset],
                                &DataPtr[_Offset],
                                LocalMaxSize
                            );

                            /*
                            var Hash1 = PointerUtils.FastHash(&DataPtr[CompareOffset], MinSize);
                            var Hash2 = PointerUtils.FastHash(&DataPtr[_Offset], MinSize);
                            for (int n = 0; n < MinSize; n++)
                            {
                                Console.WriteLine("{0:X2}, {1:X2}", DataPtr[CompareOffset + n], DataPtr[_Offset + n]);
                            }
                            Console.WriteLine();
                            Console.WriteLine("{0}, {1}, {2}, {3}: {4:X8}, {5:X8}, {6:X8}", MatchedLength, MinSize, MaxSize, LocalMaxSize, Hash, Hash1, Hash2);
                            */

                            //Console.WriteLine("{0}, {1}, {2}", CompareOffset - _Offset, MatchedLength, CompareOffset - _Offset + MatchedLength);

                            if (!AllowOverlapping && (CompareOffset - _Offset + MatchedLength) > 0)
                            {
                                MatchedLength = _Offset - CompareOffset;
                                //continue;
                            }

                            if (MatchedLength >= MinSize)
                            {
                                if (FindSequenceResult.Size < MatchedLength)
                                {
                                    FindSequenceResult.Size = MatchedLength;
                                    FindSequenceResult.Offset = CompareOffset;
                                }

                                if (MatchedLength == MaxSize) break;
                            }
                        }
                    }
                }
            }

            return FindSequenceResult;
        }
    }
}