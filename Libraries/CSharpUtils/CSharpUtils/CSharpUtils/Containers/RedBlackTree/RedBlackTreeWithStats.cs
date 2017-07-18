using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CountType = System.Int32;

namespace CSharpUtils.Containers.RedBlackTree
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public partial class
        RedBlackTreeWithStats<TElement> : ICollection<TElement>, ICloneable //, IOrderedQueryable<TElement>
    {
        internal Node BaseRootNode;
        int _length;
        IComparer<TElement> Comparer;
        bool AllowDuplicates = false;
        bool _concurrent;

        /// <summary>
        /// Max number of elements that the collection will have.
        /// If inserted more, it will remove the tail of the collection.
        /// If this value is -1, that means that the collection is not capped.
        /// </summary>
        public int CappedToNumberOfElements = -1;

        bool Concurrent
        {
            set
            {
                if (value) throw new NotImplementedException();
                _concurrent = value;
            }
        }

        ReaderWriterLock ReaderWriterLock = new ReaderWriterLock();
        //const bool AllowDuplicates = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="concurrent"></param>
        public RedBlackTreeWithStats(IComparer<TElement> comparer, bool concurrent = false)
        {
            Comparer = comparer;
            Concurrent = concurrent;
            //this.allowDuplicates = false;
            _setup();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="concurrent"></param>
        public RedBlackTreeWithStats(bool concurrent = false)
        {
            Comparer = Comparer<TElement>.Default;
            Concurrent = concurrent;
            //this.allowDuplicates = false;
            _setup();
        }

        internal RedBlackTreeWithStats(IComparer<TElement> comparer, Node end, int length, bool concurrent = false)
        {
            Comparer = comparer;
            BaseRootNode = end;
            _length = length;
            Concurrent = concurrent;
        }

        private void _setup()
        {
            Assert(BaseRootNode == null); //Make sure that _setup isn't run more than once.
            BaseRootNode = Allocate();
        }

        private static Node Allocate()
        {
            return new Node();
        }

        private static Node Allocate(TElement n)
        {
            Node node = new Node();
            node.Value = n;
            return node;
        }

        private bool _less(TElement a, TElement b) => Comparer.Compare(a, b) < 0;

        private void ConcurrentAcquireWriterLock()
        {
            if (_concurrent) ReaderWriterLock.AcquireWriterLock(int.MaxValue);
        }

        private void ConcurrentReleaseWriterLock()
        {
            if (_concurrent) ReaderWriterLock.ReleaseWriterLock();
        }

        private void ConcurrentAcquireReaderLock()
        {
            if (_concurrent) ReaderWriterLock.AcquireReaderLock(int.MaxValue);
        }

        private void ConcurrentReleaseReaderLock()
        {
            if (_concurrent) ReaderWriterLock.ReleaseReaderLock();
        }

        private Node NonConcurrentAdd(TElement n)
        {
            bool added;
            return NonConcurrentAdd(n, out added);
        }

        private Node NonConcurrentAdd(TElement elementToAdd, out bool added)
        {
            //bool added = false;
            //var Node = ReaderWriterLock.WriterLock<Node>(() =>
            Node result;
            added = true;

            if (RealRootNode == null)
            {
                RealRootNode = result = Allocate(elementToAdd);
            }
            else
            {
                var newParent = RealRootNode;
                Node nxt;

                while (true)
                {
                    if (_less(elementToAdd, newParent.Value))
                    {
                        nxt = newParent.LeftNode;
                        if (nxt == null)
                        {
                            //
                            // add to right of new parent
                            //
                            newParent.LeftNode = result = Allocate(elementToAdd);
                            break;
                        }
                    }
                    else
                    {
                        if (!AllowDuplicates)
                        {
                            if (!_less(newParent.Value, elementToAdd))
                            {
                                result = newParent;
                                added = false;
                                break;
                            }
                        }

                        nxt = newParent.RightNode;
                        if (nxt == null)
                        {
                            //
                            // add to right of new parent
                            //
                            newParent.RightNode = result = Allocate(elementToAdd);
                            break;
                        }
                    }

                    newParent = nxt;
                }
            }

            try
            {
                if (AllowDuplicates)
                {
                    result.UpdateCurrentAndAncestors(+1);
                    result.SetColor(BaseRootNode);
                    _length++;
                    return result;
                }
                else
                {
                    if (added)
                    {
                        result.UpdateCurrentAndAncestors(+1);
                        result.SetColor(BaseRootNode);
                    }
                    _length++;
                    return result;
                }
            }
            finally
            {
                if (added && CappedToNumberOfElements >= 0)
                {
                    if (Count > CappedToNumberOfElements)
                    {
                        RemoveBack();
                    }
                }
            }
        }


        // find a node based on an element value
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Node NonConcurrentFindNodeFromElement(TElement e)
        {
            if (AllowDuplicates)
            {
                Node cur = RealRootNode;
                Node result = null;
                while (cur != null)
                {
                    if (_less(cur.Value, e))
                    {
                        cur = cur.RightNode;
                    }
                    else if (_less(e, cur.Value))
                    {
                        cur = cur.LeftNode;
                    }
                    else
                    {
                        // want to find the left-most element
                        result = cur;
                        cur = cur.LeftNode;
                    }
                }
                return result;
            }
            else
            {
                Node cur = RealRootNode;
                //writefln("------------- (search:%s)", e);
                while (cur != null)
                {
                    //writefln("%s", *cur);
                    if (_less(cur.Value, e))
                    {
                        cur = cur.RightNode;
                    }
                    else if (_less(e, cur.Value))
                    {
                        cur = cur.LeftNode;
                    }
                    else
                    {
                        //writefln("found!");
                        return cur;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsEmpty => RealRootNode == null;

        internal Node RealRootNode
        {
            get => BaseRootNode.LeftNode;
            set => BaseRootNode.LeftNode = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public TElement FrontElement => BaseRootNode.LeftMostNode.Value;

        /// <summary>
        /// 
        /// </summary>
        public TElement BackElement => BaseRootNode.PreviousNode.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool Contains(TElement v) => NonConcurrentFindNodeFromElement(v) != null;

        #region Methods to insert elements

        private void Insert(TElement stuff)
        {
            ConcurrentAcquireWriterLock();
            try
            {
                bool added;
                NonConcurrentAdd(stuff, out added);
            }
            finally
            {
                ConcurrentReleaseWriterLock();
            }
        }

        #endregion

        #region Methods to remove elements

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            ConcurrentAcquireWriterLock();
            try
            {
                BaseRootNode.LeftNode = null;
                _length = 0;
            }
            finally
            {
                ConcurrentReleaseWriterLock();
            }
        }

        internal Node RemoveNode(Node nodeToRemove)
        {
            if (nodeToRemove == null) return null;
            ConcurrentAcquireWriterLock();
            Node removedNode;
            try
            {
                removedNode = nodeToRemove.NonSynchronizedRemove(BaseRootNode);
                _length--;
            }
            finally
            {
                ConcurrentReleaseWriterLock();
            }
            return removedNode;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveFront()
        {
            RemoveNode(BaseRootNode.LeftMostNode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TElement RemoveBack()
        {
            var lastNode = BaseRootNode.PreviousNode;
            RemoveNode(lastNode);
            return lastNode.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elements"></param>
        public void Remove(IEnumerable<TElement> elements)
        {
            foreach (var element in elements)
            {
                Remove(element);
            }
        }

        /// <summary>
        /// Removes an element from the tree.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Returns if an element was removed or not</returns>
        public bool Remove(TElement item)
        {
            return (RemoveNode(NonConcurrentFindNodeFromElement(item)) != null);
        }

        #endregion

        // find the first node where the value is > e
        private Node FindFirstGreaterNode(TElement e)
        {
            // can't use _find, because we cannot return null
            var cur = RealRootNode;
            var result = BaseRootNode;
            while (cur != null)
            {
                if (_less(e, cur.Value))
                {
                    result = cur;
                    cur = cur.LeftNode;
                }
                else
                {
                    cur = cur.RightNode;
                }
            }
            return result;
        }

        // find the first node where the value is >= e
        private Node FindFirstGreaterEqualNode(TElement e)
        {
            // can't use _find, because we cannot return null.
            var cur = RealRootNode;
            var result = BaseRootNode;
            while (cur != null)
            {
                if (_less(cur.Value, e))
                {
                    cur = cur.RightNode;
                }
                else
                {
                    result = cur;
                    cur = cur.LeftNode;
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Range UpperBound(TElement e) => new Range(this, FindFirstGreaterNode(e), BaseRootNode);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public Range LowerBound(TElement e) => new Range(this, BaseRootNode.LeftMostNode, FindFirstGreaterEqualNode(e));

        Range EqualRange(TElement e)
        {
            var beg = FindFirstGreaterEqualNode(e);
            if (beg == BaseRootNode || _less(e, beg.Value))
            {
                // no values are equal
                return new Range(this, beg, beg);
            }

            if (AllowDuplicates)
            {
                return new Range(this, beg, FindFirstGreaterNode(e));
            }
            // no sense in doing a full search, no duplicates are allowed,
            // so we just get the next node.
            return new Range(this, beg, beg.NextNode);
        }

        //auto _equals(Node a, Node b) { return !_less(a, b) && !_less(b, a); }
        //auto _lessOrEquals(Node a, Node b) { return _less(a, b) || _equals(a, b); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int CountLesserThanNode(Node node)
        {
            if (node == null) return -1;
            if (node.ParentNode == null) return node.ChildCountLeft;

            //auto prev = node;
            var it = node;
            int count = 0;
            while (true)
            {
                if (it.ParentNode == null) break;
                //writefln("+%d+1", it.childCountLeft);
                //if (it.value <= node.value) {
                if (!_less(node.Value, it.Value))
                {
                    count += it.ChildCountLeft + 1;
                }
                it = it.ParentNode;
                if (it == null)
                {
                    //writefln("it is null");
                    break;
                }
                //_less
                //if (it._right != prev) break;
                //prev = it;
            }
            return count - 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetNodePosition(Node node) => CountLesserThanNode(node);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public int GetItemPosition(TElement element) => CountLesserThanNode(NonConcurrentFindNodeFromElement(element));

        internal Node LocateNodeAtPosition(int positionToFind)
        {
            if (positionToFind < 0) throw(new Exception("Negative locateNodeAt"));
            Node current = BaseRootNode;
            int currentPosition = BaseRootNode.ChildCountLeft;

            //writefln("[AA---(%d)]", positionToFind);

            while (true)
            {
                if (current == null) return null;
                //writefln("%s : %d", current, currentPosition);

                //CountType currentPositionExpected = getNodePosition(current);
                if (currentPosition == positionToFind) return current;

                if (positionToFind < currentPosition)
                {
                    //currentPosition += current.childCountLeft;
                    current = current.LeftNode;
                    if (current == null) return null;
                    //writefln("Left(%s/%s) ::: %d-%d", current.childCountLeft, current.childCountRight, currentPosition, current.childCountRight);
                    currentPosition -= current.ChildCountRight + 1;
                }
                else
                {
                    current = current.RightNode;
                    if (current == null) return null;
                    //writefln("Right(%s/%s) ::: %d+%d", current.childCountLeft, current.childCountRight, currentPosition, current.childCountLeft);
                    currentPosition += current.ChildCountLeft + 1;
                }
                //writefln("currentPosition: %d/%d/%d", currentPosition, currentPositionExpected, positionToFind);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Range All
        {
            get
            {
                ConcurrentAcquireReaderLock();
                try
                {
                    return new Range(
                        this,
                        RealRootNode?.LeftMostNode,
                        BaseRootNode,
                        0,
                        _length
                    );
                }
                finally
                {
                    ConcurrentReleaseReaderLock();
                }
            }
        }

        int DebugValidateStatsNodeSubtree()
        {
            return RealRootNode.DebugValidateStatsNodeSubtree();
        }

        /// <summary>
        /// 
        /// </summary>
        public void PrintTree()
        {
            RealRootNode.PrintTree();
        }

        /// <summary>
        /// 
        /// </summary>
        public void DebugValidateTree()
        {
            var internalLength = _length;
            var calculatedLength = DebugValidateStatsNodeSubtree();
            Assert(calculatedLength == internalLength);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(TElement item)
        {
            Insert(item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void CopyTo(TElement[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count => _length;

        /// <summary>
        /// 
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TElement> GetEnumerator() => (All as IEnumerable<TElement>).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => (All as IEnumerable<TElement>).GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public RedBlackTreeWithStats<TElement> Clone() => new RedBlackTreeWithStats<TElement>(Comparer, BaseRootNode.Clone(), _length);

        object ICloneable.Clone() => new RedBlackTreeWithStats<TElement>(Comparer, BaseRootNode.Clone(), _length);

        //private readonly Expression _expression;
        //private readonly RedBlackTreeWithStatsQueryProvider _provider;


        /*
        public System.Type ElementType { get { return All.ElementType; } }
        public Expression Expression { get { return All.Expression; } }
        public IQueryProvider Provider { get { return All.Provider; } }
        */
    }
}