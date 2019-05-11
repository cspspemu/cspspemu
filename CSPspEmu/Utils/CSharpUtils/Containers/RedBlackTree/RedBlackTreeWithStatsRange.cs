using System;
using System.Collections;
using System.Collections.Generic;
using CountType = System.Int32;

namespace CSharpUtils.Containers.RedBlackTree
{
    public partial class RedBlackTreeWithStats<TElement>
    {
        /// <summary>
        /// 
        /// </summary>
        public class Range : IEnumerable<TElement>, ICloneable /*, IOrderedQueryable<TElement>*/
        {
            internal RedBlackTreeWithStats<TElement> ParentTree;
            internal Node RangeStartNode;
            internal Node RangeEndNode;
            internal int RangeStartPosition;
            internal int RangeEndPosition;

            internal Node RangeLastNode => RangeEndNode.PreviousNode;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="localIndex"></param>
            /// <returns></returns>
            public int GetItemPosition(int localIndex)
            {
                if (RangeStartPosition == -1)
                {
                    return ParentTree.GetNodePosition(RangeStartNode) + localIndex;
                }
                return RangeStartPosition + localIndex;
            }

            internal Range(RedBlackTreeWithStats<TElement> parentTree, Node rangeStartNode, Node rangeEndNode,
                int rangeStartPosition = -1, int rangeEndPosition = -1)
            {
                ParentTree = parentTree;
                if (rangeStartNode == null) rangeStartNode = parentTree.LocateNodeAtPosition(rangeStartPosition);
                if (rangeEndNode == null) rangeEndNode = parentTree.LocateNodeAtPosition(rangeEndPosition);
                if (rangeStartNode == null || rangeEndNode == null)
                {
                    rangeStartNode = rangeEndNode = parentTree.BaseRootNode;
                    rangeStartPosition = -1;
                    rangeEndPosition = -1;
                }
                RangeStartNode = rangeStartNode;
                RangeEndNode = rangeEndNode;
                RangeStartPosition = rangeStartPosition;
                RangeEndPosition = rangeEndPosition;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public Range Clone()
            {
                return new Range(ParentTree, RangeStartNode, RangeEndNode, RangeStartPosition, RangeEndPosition);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="limitCount"></param>
            /// <returns></returns>
            public Range Limit(int limitCount)
            {
                Assert(limitCount >= 0);

                if (RangeStartPosition != -1 && RangeEndPosition != -1)
                {
                    if (RangeStartPosition + limitCount > RangeEndPosition)
                    {
                        limitCount = RangeEndPosition - RangeStartPosition;
                    }
                }
                return LimitUnchecked(limitCount);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="limitCount"></param>
            /// <returns></returns>
            public Range LimitUnchecked(int limitCount)
            {
                Assert(limitCount >= 0);

                return new Range(
                    ParentTree,
                    RangeStartNode, null,
                    RangeStartPosition, GetItemPosition(limitCount)
                );
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="skipCount"></param>
            /// <param name="takeCount"></param>
            /// <returns></returns>
            public Range SkipTake(int skipCount, int takeCount)
            {
                return Skip(skipCount).Take(takeCount);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="skipCount"></param>
            /// <returns></returns>
            public Range Skip(int skipCount)
            {
                return SkipUnchecked(skipCount);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="skipCount"></param>
            /// <returns></returns>
            public Range Take(int skipCount)
            {
                return Limit(skipCount);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="skipCount"></param>
            /// <returns></returns>
            public Range SkipUnchecked(int skipCount)
            {
                return new Range(
                    ParentTree,
                    null, RangeEndNode,
                    GetItemPosition(skipCount), RangeEndPosition
                );
            }

            bool IsEmpty => (RangeStartNode == RangeEndNode);

            /// <summary>
            /// 
            /// </summary>
            public int Count
            {
                get
                {
                    //writefln("Begin: %d:%s", countLesser(_begin), *_begin);
                    //writefln("End: %d:%s", countLesser(_end), *_end);
                    //return _begin

                    if (RangeStartPosition != -1 && RangeEndPosition != -1)
                    {
                        return RangeEndPosition - RangeStartPosition;
                    }

                    return ParentTree.CountLesserThanNode(RangeEndNode) -
                           ParentTree.CountLesserThanNode(RangeStartNode);
                }
            }

            /// <summary>
            /// Slice is immutable, so don't need to clone.
            /// </summary>
            /// <returns></returns>
            public Range Slice()
            {
                //return this.Clone();
                return this;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="startIndex"></param>
            /// <param name="endIndex"></param>
            /// <returns></returns>
            public Range Slice(int startIndex, int endIndex)
            {
                return new Range(
                    ParentTree,
                    null,
                    null,
                    GetItemPosition(startIndex),
                    GetItemPosition(endIndex)
                );
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="start"></param>
            /// <returns></returns>
            public Range Slice(int start)
            {
                return new Range(
                    ParentTree,
                    null,
                    null,
                    GetItemPosition(start),
                    Count
                );
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="index"></param>
            public Node this[int index] => ParentTree.LocateNodeAtPosition(GetItemPosition(index));

            TElement FrontElement => RangeStartNode.Value;

            TElement BackElement => RangeLastNode.Value;

            IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
            {
                for (var currentNode = RangeStartNode; currentNode != RangeEndNode; currentNode = currentNode.NextNode)
                {
                    yield return currentNode.Value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                for (var currentNode = RangeStartNode; currentNode != RangeEndNode; currentNode = currentNode.NextNode)
                {
                    yield return currentNode.Value;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Contains(TElement item)
            {
                if (IsEmpty) return false;
                if (ParentTree.Comparer.Compare(item, RangeStartNode.Value) < 0) return false;
                if (ParentTree.Comparer.Compare(item, RangeLastNode.Value) > 0) return false;
                return ParentTree.Contains(item);
            }

            object ICloneable.Clone()
            {
                return new Range(ParentTree, RangeStartNode, RangeEndNode, RangeStartPosition, RangeEndPosition);
            }

            /*
            public System.Type ElementType { get { return typeof(TElement); } }
            public Expression Expression { get { return Expression.Constant(this); } }
            public IQueryProvider Provider { get { return new RedBlackTreeWithStatsQueryProvider<TElement>(this); } }
            */
        }
    }
}