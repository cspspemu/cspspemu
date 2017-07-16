using System;
using System.Collections.Generic;
using CountType = System.Int32;
using System.Threading;

namespace CSharpUtils.Containers.RedBlackTree
{
    public partial class
        RedBlackTreeWithStats<TElement> : ICollection<TElement>, ICloneable //, IOrderedQueryable<TElement>
    {
        internal Node BaseRootNode = null;
        CountType _Length = 0;
        IComparer<TElement> Comparer = null;
        bool AllowDuplicates = false;
        bool _Concurrent;

        /// <summary>
        /// Max number of elements that the collection will have.
        /// If inserted more, it will remove the tail of the collection.
        /// If this value is -1, that means that the collection is not capped.
        /// </summary>
        public int CappedToNumberOfElements = -1;

        bool Concurrent
        {
            get { return _Concurrent; }
            set
            {
                if (value == true) throw(new NotImplementedException());
                _Concurrent = value;
            }
        }

        ReaderWriterLock ReaderWriterLock = new ReaderWriterLock();
        //const bool AllowDuplicates = false;

        public RedBlackTreeWithStats(IComparer<TElement> Comparer, bool Concurrent = false)
        {
            this.Comparer = Comparer;
            this.Concurrent = Concurrent;
            //this.allowDuplicates = false;
            _setup();
        }

        public RedBlackTreeWithStats(bool Concurrent = false)
        {
            this.Comparer = Comparer<TElement>.Default;
            this.Concurrent = Concurrent;
            //this.allowDuplicates = false;
            _setup();
        }

        internal RedBlackTreeWithStats(IComparer<TElement> Comparer, Node _end, int _length, bool Concurrent = false)
        {
            this.Comparer = Comparer;
            this.BaseRootNode = _end;
            this._Length = _length;
            this.Concurrent = Concurrent;
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

        bool _less(TElement A, TElement B)
        {
            return Comparer.Compare(A, B) < 0;
        }

        private void ConcurrentAcquireWriterLock()
        {
            if (this._Concurrent) ReaderWriterLock.AcquireWriterLock(Int32.MaxValue);
        }

        private void ConcurrentReleaseWriterLock()
        {
            if (this._Concurrent) ReaderWriterLock.ReleaseWriterLock();
        }

        private void ConcurrentAcquireReaderLock()
        {
            if (this._Concurrent) ReaderWriterLock.AcquireReaderLock(Int32.MaxValue);
        }

        private void ConcurrentReleaseReaderLock()
        {
            if (this._Concurrent) ReaderWriterLock.ReleaseReaderLock();
        }

        private Node NonConcurrentAdd(TElement n)
        {
            bool added;
            return NonConcurrentAdd(n, out added);
        }

        private Node NonConcurrentAdd(TElement ElementToAdd, out bool Added)
        {
            //bool added = false;
            //var Node = ReaderWriterLock.WriterLock<Node>(() =>
            Node result = null;
            Added = true;

            if (RealRootNode == null)
            {
                RealRootNode = result = Allocate(ElementToAdd);
            }
            else
            {
                Node newParent = RealRootNode;
                Node nxt;

                while (true)
                {
                    if (_less(ElementToAdd, newParent.Value))
                    {
                        nxt = newParent.LeftNode;
                        if (nxt == null)
                        {
                            //
                            // add to right of new parent
                            //
                            newParent.LeftNode = result = Allocate(ElementToAdd);
                            break;
                        }
                    }
                    else
                    {
                        if (!AllowDuplicates)
                        {
                            if (!_less(newParent.Value, ElementToAdd))
                            {
                                result = newParent;
                                Added = false;
                                break;
                            }
                        }

                        nxt = newParent.RightNode;
                        if (nxt == null)
                        {
                            //
                            // add to right of new parent
                            //
                            newParent.RightNode = result = Allocate(ElementToAdd);
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
                    _Length++;
                    return result;
                }
                else
                {
                    if (Added)
                    {
                        result.UpdateCurrentAndAncestors(+1);
                        result.SetColor(BaseRootNode);
                    }
                    _Length++;
                    return result;
                }
            }
            finally
            {
                if (Added && this.CappedToNumberOfElements >= 0)
                {
                    if (this.Count > this.CappedToNumberOfElements)
                    {
                        RemoveBack();
                    }
                }
            }
        }


        // find a node based on an element value
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

        public bool IsEmpty
        {
            get { return RealRootNode == null; }
        }

        internal Node RealRootNode
        {
            get { return BaseRootNode.LeftNode; }
            set { BaseRootNode.LeftNode = value; }
        }

        public TElement FrontElement
        {
            get { return BaseRootNode.LeftMostNode.Value; }
        }

        public TElement BackElement
        {
            get { return BaseRootNode.PreviousNode.Value; }
        }

        public bool Contains(TElement V)
        {
            return NonConcurrentFindNodeFromElement(V) != null;
        }

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

        public void Clear()
        {
            ConcurrentAcquireWriterLock();
            try
            {
                BaseRootNode.LeftNode = null;
                _Length = 0;
            }
            finally
            {
                ConcurrentReleaseWriterLock();
            }
        }

        internal Node RemoveNode(Node NodeToRemove)
        {
            if (NodeToRemove == null) return null;
            ConcurrentAcquireWriterLock();
            Node RemovedNode = null;
            try
            {
                RemovedNode = NodeToRemove.NonSynchronizedRemove(BaseRootNode);
                _Length--;
            }
            finally
            {
                ConcurrentReleaseWriterLock();
            }
            return RemovedNode;
        }

        public void RemoveFront()
        {
            RemoveNode(BaseRootNode.LeftMostNode);
        }

        public TElement RemoveBack()
        {
            var LastNode = BaseRootNode.PreviousNode;
            RemoveNode(LastNode);
            return LastNode.Value;
        }

        public void Remove(IEnumerable<TElement> Elements)
        {
            foreach (var Element in Elements)
            {
                Remove(Element);
            }
        }

        /// <summary>
        /// Removes an element from the tree.
        /// </summary>
        /// <param name="Item"></param>
        /// <returns>Returns if an element was removed or not</returns>
        public bool Remove(TElement Item)
        {
            return (RemoveNode(NonConcurrentFindNodeFromElement(Item)) != null);
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

        public Range UpperBound(TElement e)
        {
            return new Range(this, FindFirstGreaterNode(e), BaseRootNode);
        }

        public Range LowerBound(TElement e)
        {
            return new Range(this, BaseRootNode.LeftMostNode, FindFirstGreaterEqualNode(e));
        }

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
            else
            {
                // no sense in doing a full search, no duplicates are allowed,
                // so we just get the next node.
                return new Range(this, beg, beg.NextNode);
            }
        }

        //auto _equals(Node a, Node b) { return !_less(a, b) && !_less(b, a); }
        //auto _lessOrEquals(Node a, Node b) { return _less(a, b) || _equals(a, b); }

        public CountType CountLesserThanNode(Node Node)
        {
            if (Node == null) return -1;
            if (Node.ParentNode == null) return Node.ChildCountLeft;

            //auto prev = node;
            var it = Node;
            CountType count = 0;
            while (true)
            {
                if (it.ParentNode == null) break;
                //writefln("+%d+1", it.childCountLeft);
                //if (it.value <= node.value) {
                if (!_less(Node.Value, it.Value))
                {
                    count += it.ChildCountLeft + 1;
                }
                it = it.ParentNode;
                if (it == null)
                {
                    //writefln("it is null");
                    break;
                }
                else
                {
                    //writefln("less(%s, %s) : %d", it.value, node.value, it.value < node.value);

                    //if (_less(it, node)) break;
                    //if (it.value >= node.value) break;
                }
                //_less
                //if (it._right != prev) break;
                //prev = it;
            }
            return count - 1;
        }

        public CountType GetNodePosition(Node Node)
        {
            return CountLesserThanNode(Node);
        }

        public CountType GetItemPosition(TElement Element)
        {
            return CountLesserThanNode(NonConcurrentFindNodeFromElement(Element));
        }

        internal Node LocateNodeAtPosition(CountType PositionToFind)
        {
            if (PositionToFind < 0) throw(new Exception("Negative locateNodeAt"));
            Node current = BaseRootNode;
            CountType currentPosition = BaseRootNode.ChildCountLeft;

            //writefln("[AA---(%d)]", positionToFind);

            while (true)
            {
                if (current == null) return null;
                //writefln("%s : %d", current, currentPosition);

                //CountType currentPositionExpected = getNodePosition(current);
                if (currentPosition == PositionToFind) return current;

                if (PositionToFind < currentPosition)
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

        public Range All
        {
            get
            {
                ConcurrentAcquireReaderLock();
                try
                {
                    return new Range(
                        this,
                        (RealRootNode != null) ? RealRootNode.LeftMostNode : null,
                        BaseRootNode,
                        0,
                        _Length
                    );
                }
                finally
                {
                    ConcurrentReleaseReaderLock();
                }
            }
        }

        CountType DebugValidateStatsNodeSubtree()
        {
            return RealRootNode.DebugValidateStatsNodeSubtree();
        }

        public void PrintTree()
        {
            RealRootNode.PrintTree();
        }

        public void DebugValidateTree()
        {
            int InternalLength = _Length;
            int CalculatedLength = DebugValidateStatsNodeSubtree();
            Assert(CalculatedLength == InternalLength);
        }

        public void Add(TElement item)
        {
            Insert(item);
        }

        public void CopyTo(TElement[] array, CountType arrayIndex)
        {
            throw new NotImplementedException();
        }

        public CountType Count
        {
            get { return _Length; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return (All as IEnumerable<TElement>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (All as IEnumerable<TElement>).GetEnumerator();
        }

        public RedBlackTreeWithStats<TElement> Clone()
        {
            return new RedBlackTreeWithStats<TElement>(Comparer, BaseRootNode.Clone(), _Length);
        }

        object ICloneable.Clone()
        {
            return new RedBlackTreeWithStats<TElement>(Comparer, BaseRootNode.Clone(), _Length);
        }

        //private readonly Expression _expression;
        //private readonly RedBlackTreeWithStatsQueryProvider _provider;


        /*
        public System.Type ElementType { get { return All.ElementType; } }
        public Expression Expression { get { return All.Expression; } }
        public IQueryProvider Provider { get { return All.Provider; } }
        */
    }
}