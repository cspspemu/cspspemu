using System;
using System.Collections.Generic;

namespace CSharpUtils.Ext.Compression.Lz
{
    /// <summary>
    /// 
    /// </summary>
    public unsafe class LzMatcher
    {
        /// <summary>
        /// 
        /// </summary>
        public int MaxOffset { get; }

        /// <summary>
        /// 
        /// </summary>
        public int MinSize { get; }

        /// <summary>
        /// 
        /// </summary>
        public int MaxSize { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowOverlapping { get; }

        byte[] Data;
        readonly Dictionary<int, LinkedList<int>> _waypoints = new Dictionary<int, LinkedList<int>>();
        readonly LinkedList<int> _hashes = new LinkedList<int>();
        private int _offset;

        /// <summary>
        /// 
        /// </summary>
        public int Offset
        {
            get => _offset;
            set
            {
                _waypoints.Clear();
                _hashes.Clear();
                _offset = Math.Max(value - MaxOffset, 0);
                Skip(Math.Min(MaxOffset, value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="maxOffset"></param>
        /// <param name="minSize"></param>
        /// <param name="maxSize"></param>
        /// <param name="allowOverlapping"></param>
        public LzMatcher(byte[] data, int offset = 0, int maxOffset = 0x1000, int minSize = 3, int maxSize = 0x12,
            bool allowOverlapping = true)
        {
            Data = data;
            MaxOffset = maxOffset;
            MinSize = minSize;
            MaxSize = maxSize;
            //if (!AllowOverlapping) throw (new NotImplementedException("!AllowOverlapping"));
            AllowOverlapping = allowOverlapping;
            Offset = offset;
        }

        /// <summary>
        /// 
        /// </summary>
        public int Length => Data.Length;

        int GetOffsetHash(int offset)
        {
            fixed (byte* ptr = &Data[offset])
            {
                return PointerUtils.FastHash(ptr, MinSize);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skipCount"></param>
        public void Skip(int skipCount = 1)
        {
            if (skipCount != 1)
            {
                while (skipCount-- > 0) Skip();
                return;
            }

            var hash = GetOffsetHash(_offset);
            _hashes.AddLast(hash);
            if (!_waypoints.ContainsKey(hash)) _waypoints[hash] = new LinkedList<int>();
            _waypoints[hash].AddLast(_offset);

            if (_hashes.Count > MaxOffset)
            {
                var firstHash = _hashes.First.Value;
                _hashes.RemoveFirst();
                _waypoints[firstHash].RemoveFirst();
            }

            _offset++;
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

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"LzMatcher.FindSequenceResult(Offset={Offset}, Size={Size})";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public FindSequenceResult FindMaxSequence()
        {
            var findSequenceResult = new FindSequenceResult
            {
                Offset = 0,
                Size = 0
            };

            if ((Data.Length - _offset) >= MinSize)
            {
                fixed (byte* dataPtr = Data)
                {
                
                    var hash = GetOffsetHash(_offset);
                    if (!_waypoints.ContainsKey(hash)) return findSequenceResult;
                    //var Node = Waypoints[Hash].Last;
                    foreach (var compareOffset in _waypoints[hash])
                    {
                        var localMaxSize = Math.Min((Data.Length - _offset), MaxSize);

                        //if (!AllowOverlapping && (CompareOffset + LocalMaxSize > _Offset)) continue;

                        var matchedLength = PointerUtils.FindLargestMatch(
                            &dataPtr[compareOffset],
                            &dataPtr[_offset],
                            localMaxSize
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

                        if (!AllowOverlapping && (compareOffset - _offset + matchedLength) > 0)
                        {
                            matchedLength = _offset - compareOffset;
                            //continue;
                        }

                        if (matchedLength < MinSize) continue;
                        
                        if (findSequenceResult.Size < matchedLength)
                        {
                            findSequenceResult.Size = matchedLength;
                            findSequenceResult.Offset = compareOffset;
                        }

                        if (matchedLength == MaxSize) break;
                    }
                }
            }

            return findSequenceResult;
        }
    }
}