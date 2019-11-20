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

        public Node RealRootNode
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
            return RemoveNode(NonConcurrentFindNodeFromElement(item)) != null;
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
            if (positionToFind < 0) throw new Exception("Negative locateNodeAt");
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
        public RedBlackTreeWithStats<TElement> Clone() =>
            new RedBlackTreeWithStats<TElement>(Comparer, BaseRootNode.Clone(), _length);

        object ICloneable.Clone() => new RedBlackTreeWithStats<TElement>(Comparer, BaseRootNode.Clone(), _length);

        //private readonly Expression _expression;
        //private readonly RedBlackTreeWithStatsQueryProvider _provider;


        /*
        public System.Type ElementType { get { return All.ElementType; } }
        public Expression Expression { get { return All.Expression; } }
        public IQueryProvider Provider { get { return All.Provider; } }
        */
        
        ///////////////////////////////
        
        static void DebugAssert(bool assertion)
        {
            if (!assertion) throw new InvalidOperationException();
        }

        static void Assert(bool assertion)
        {
            DebugAssert(assertion);
        }

        internal enum Color
        {
            Red,
            Black
        }

        /// <summary>
        /// 
        /// </summary>
        public class Node
        {
            internal Node _LeftNode;
            internal Node _RightNode;
            internal Node _ParentNode;

            internal int ChildCountLeft;
            internal int ChildCountRight;

            public TElement Value;
            internal Color Color;

            internal Node LeftNode
            {
                get => _LeftNode;
                set
                {
                    _LeftNode = value;
                    if (value != null) value._ParentNode = this;
                }
            }

            internal Node RightNode
            {
                get => _RightNode;
                set
                {
                    _RightNode = value;
                    if (value != null) value._ParentNode = this;
                }
            }

            internal Node ParentNode => _ParentNode;

            internal Node RootNode
            {
                get
                {
                    var current = this;
                    while (current.ParentNode != null) current = current.ParentNode;
                    return current;
                }
            }

            internal int DebugValidateStatsNodeSubtree()
            {
                int totalChildCountLeft = 0;
                var totalChildCountRight = 0;
                if (LeftNode != null) totalChildCountLeft = LeftNode.DebugValidateStatsNodeSubtree();
                if (RightNode != null) totalChildCountRight = RightNode.DebugValidateStatsNodeSubtree();
                DebugAssert(ChildCountLeft == totalChildCountLeft);
                DebugAssert(ChildCountRight == totalChildCountRight);
                return 1 + ChildCountLeft + ChildCountRight;
            }

            internal void UpdateCurrentAndAncestors(int countIncrement)
            {
                //return;

                var previousNode = this;
                var currentNode = ParentNode;

                while (currentNode != null)
                {
                    // @TODO: Change
                    // prev.isLeftNode

                    if (previousNode.IsLeftNode)
                    {
                        currentNode.ChildCountLeft += countIncrement;
                    }
                    else if (previousNode.IsRightNode)
                    {
                        currentNode.ChildCountRight += countIncrement;
                    }
                    else
                    {
                        Assert(false);
                    }
                    previousNode = currentNode;
                    currentNode = currentNode.ParentNode;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return
                    $"RedBlackTreeWithStats.Node(Value={Value}, Color={Enum.GetName(typeof(Color), Color)}, ChildCountLeft={ChildCountLeft}, ChildCountRight={ChildCountRight})";
            }

            internal void PrintTree(Node markNode = null, int level = 0, string label = "L")
            {
                var info = "";
                if (this == markNode) info = " (mark)";

                Console.WriteLine(
                    "{0}- {1}:{2}{3}",
                    new string(' ', level * 2),
                    label, this, info
                );

                LeftNode?.PrintTree(markNode, level + 1, "L");
                RightNode?.PrintTree(markNode, level + 1, "R");
            }

            internal bool IsLeftNode
            {
                get
                {
                    Assert(ParentNode != null);
                    return this == ParentNode.LeftNode;
                }
            }

            internal bool IsRightNode
            {
                get
                {
                    Assert(ParentNode != null);
                    return this == ParentNode.RightNode;
                }
            }

            internal Node RotateL() => RotateLeft();

            internal Node RotateR() => RotateRight();

            internal Node RotateRight()
            {
                Assert(LeftNode != null);

                if (IsLeftNode)
                {
                    ParentNode.LeftNode = LeftNode;
                }
                else
                {
                    ParentNode.RightNode = LeftNode;
                }

                ChildCountLeft = LeftNode.ChildCountRight;
                LeftNode.ChildCountRight = ChildCountLeft + ChildCountRight + 1;

                var tempNode = LeftNode.RightNode;
                LeftNode.RightNode = this;
                LeftNode = tempNode;

                return this;
            }

            Node RotateLeft()
            {
                Assert(RightNode != null);

                // sets _right._parent also
                if (IsLeftNode)
                {
                    ParentNode.LeftNode = RightNode;
                }
                else
                {
                    ParentNode.RightNode = RightNode;
                }

                ChildCountRight = RightNode.ChildCountLeft;
                RightNode.ChildCountLeft = ChildCountLeft + ChildCountRight + 1;

                var tempNode = RightNode.LeftNode;
                RightNode.LeftNode = this;
                RightNode = tempNode;

                return this;
            }

            internal void SetColor(Node end)
            {
                //writefln("Updating tree...");
                // test against the marker node
                if (ParentNode == end)
                {
                    //
                    // this is the root node, color it black
                    //
                    Color = Color.Black;
                    return;
                }

                if (ParentNode.Color != Color.Red)
                {
                    return;
                }

                var cur = this;

                while (true)
                {
                    // because root is always black, _parent._parent always exists
                    if (cur.ParentNode.IsLeftNode)
                    {
                        // parent is left node, y is 'uncle', could be null
                        var y = cur._ParentNode._ParentNode._RightNode;
                        if (y != null && y.Color == Color.Red)
                        {
                            cur._ParentNode.Color = Color.Black;
                            y.Color = Color.Black;
                            cur = cur._ParentNode._ParentNode;
                            if (cur._ParentNode == end)
                            {
                                // root node
                                cur.Color = Color.Black;
                                break;
                            }
                            // not root node
                            cur.Color = Color.Red;
                            if (cur._ParentNode.Color == Color.Black)
                            {
                                // satisfied, exit the loop
                                break;
                            }
                        }
                        else
                        {
                            if (!cur.IsLeftNode) cur = cur._ParentNode.RotateLeft();
                            cur._ParentNode.Color = Color.Black;
                            cur = cur._ParentNode._ParentNode.RotateRight();
                            cur.Color = Color.Red;
                            // tree should be satisfied now
                            break;
                        }
                    }
                    else
                    {
                        // parent is right node, y is 'uncle'
                        var y = cur._ParentNode._ParentNode._LeftNode;
                        if (y != null && y.Color == Color.Red)
                        {
                            cur._ParentNode.Color = Color.Black;
                            y.Color = Color.Black;
                            cur = cur._ParentNode._ParentNode;
                            if (cur._ParentNode == end)
                            {
                                // root node
                                cur.Color = Color.Black;
                                break;
                            }
                            // not root node
                            cur.Color = Color.Red;

                            if (cur._ParentNode.Color == Color.Black)
                            {
                                // satisfied, exit the loop
                                break;
                            }
                        }
                        else
                        {
                            if (cur.IsLeftNode)
                            {
                                cur = cur._ParentNode.RotateRight();
                            }
                            cur._ParentNode.Color = Color.Black;
                            cur = cur._ParentNode._ParentNode.RotateLeft();
                            cur.Color = Color.Red;
                            // tree should be satisfied now
                            break;
                        }
                    }
                }
            }

            internal void SetParentThisChild(Node newThis)
            {
                if (IsLeftNode)
                {
                    _ParentNode.LeftNode = newThis;
                }
                else
                {
                    _ParentNode.RightNode = newThis;
                }
            }

            internal Node NonSynchronizedRemove(Node end)
            {
                //
                // remove this node from the tree, fixing the color if necessary.
                //
                Node x;
                Node ret;

                if (_LeftNode == null || _RightNode == null)
                {
                    //static if (hasStats) updateCurrentAndAncestors(-1);
                    ret = NextNode;
                }
                else
                {
                    //
                    // normally, we can just swap this node's and y's value, but
                    // because an iterator could be pointing to y and we don't want to
                    // disturb it, we swap this node and y's structure instead.  This
                    // can also be a benefit if the value of the tree is a large
                    // struct, which takes a long time to copy.
                    //
                    var y = NextNode;
                    var yp = y._ParentNode;
                    var yl = y._LeftNode;
                    var yr = y._RightNode;
                    var yChildCountLeft = y.ChildCountLeft;
                    var yChildCountRight = y.ChildCountRight;
                    var yc = y.Color;
                    var isyleft = y.IsLeftNode;

                    //
                    // replace y's structure with structure of this node.
                    //
                    SetParentThisChild(y);
                    //
                    // need special case so y doesn't point back to itself
                    //
                    y.LeftNode = _LeftNode;
                    y.RightNode = _RightNode == y ? this : _RightNode;
                    y.Color = Color;

                    y.ChildCountLeft = ChildCountLeft;
                    y.ChildCountRight = ChildCountRight;

                    //
                    // replace this node's structure with structure of y.
                    //
                    LeftNode = yl;
                    RightNode = yr;
                    if (_ParentNode != y)
                    {
                        if (isyleft)
                        {
                            yp.LeftNode = this;
                        }
                        else
                        {
                            yp.RightNode = this;
                        }
                    }
                    Color = yc;

                    ChildCountLeft = yChildCountLeft;
                    ChildCountRight = yChildCountRight;

                    //
                    // set return value
                    //
                    ret = y;
                }

                UpdateCurrentAndAncestors(-1);

                // if this has less than 2 children, remove it
                x = _LeftNode ?? _RightNode;

                // remove this from the tree at the end of the procedure
                var removeThis = false;
                if (x == null)
                {
                    // pretend this is a null node, remove this on finishing
                    x = this;
                    removeThis = true;
                }
                else if (IsLeftNode)
                {
                    _ParentNode.LeftNode = x;
                }
                else
                {
                    _ParentNode.RightNode = x;
                }

                // if the color of this is black, then it needs to be fixed
                if (Color == Color.Black)
                {
                    // need to recolor the tree.
                    while (x._ParentNode != end && x.Color == Color.Black)
                    {
                        if (x.IsLeftNode)
                        {
                            // left node
                            var w = x._ParentNode._RightNode;
                            if (w.Color == Color.Red)
                            {
                                w.Color = Color.Black;
                                x._ParentNode.Color = Color.Red;
                                x._ParentNode.RotateLeft();
                                w = x._ParentNode._RightNode;
                            }

                            var wl = w.LeftNode;
                            var wr = w.RightNode;

                            if (
                                (wl == null || wl.Color == Color.Black) &&
                                (wr == null || wr.Color == Color.Black)
                            )
                            {
                                w.Color = Color.Red;
                                x = x._ParentNode;
                            }
                            else
                            {
                                if (wr == null || wr.Color == Color.Black)
                                {
                                    // wl cannot be null here
                                    // ReSharper disable once PossibleNullReferenceException
                                    wl.Color = Color.Black;
                                    w.Color = Color.Red;
                                    w.RotateRight();
                                    w = x._ParentNode._RightNode;
                                }

                                w.Color = x._ParentNode.Color;
                                x._ParentNode.Color = Color.Black;
                                w._RightNode.Color = Color.Black;
                                x._ParentNode.RotateLeft();
                                x = end.LeftNode; // x = root
                            }
                        }
                        else
                        {
                            // right node
                            var w = x._ParentNode._LeftNode;
                            if (w.Color == Color.Red)
                            {
                                w.Color = Color.Black;
                                x._ParentNode.Color = Color.Red;
                                x._ParentNode.RotateR();
                                w = x._ParentNode._LeftNode;
                            }
                            var wl = w.LeftNode;
                            var wr = w.RightNode;
                            if (
                                (wl == null || wl.Color == Color.Black) &&
                                (wr == null || wr.Color == Color.Black)
                            )
                            {
                                w.Color = Color.Red;
                                x = x._ParentNode;
                            }
                            else
                            {
                                if (wl == null || wl.Color == Color.Black)
                                {
                                    // wr cannot be null here
                                    wr.Color = Color.Black;
                                    w.Color = Color.Red;
                                    w.RotateL();
                                    w = x._ParentNode._LeftNode;
                                }

                                w.Color = x._ParentNode.Color;
                                x._ParentNode.Color = Color.Black;
                                w._LeftNode.Color = Color.Black;
                                x._ParentNode.RotateR();
                                x = end.LeftNode; // x = root
                            }
                        }
                    }
                    x.Color = Color.Black;
                }

                if (removeThis)
                {
                    //
                    // clear this node out of the tree
                    //
                    if (IsLeftNode)
                    {
                        _ParentNode.LeftNode = null;
                    }
                    else
                    {
                        _ParentNode.RightNode = null;
                    }
                }

                return ret;
            }

            // Tail recursion.
            public Node LeftMostNode => LeftNode == null ? this : LeftNode.LeftMostNode;

            public Node RightMostNode => RightNode == null ? this : RightNode.RightMostNode;

            internal Node NextNode
            {
                get
                {
                    var node = this;
                    if (node._RightNode == null)
                    {
                        while (!node.IsLeftNode) node = node._ParentNode;
                        return node._ParentNode;
                    }
                    return node._RightNode.LeftMostNode;
                }
            }

            internal Node PreviousNode
            {
                get
                {
                    var n = this;
                    if (n.LeftNode == null)
                    {
                        while (n.IsLeftNode) n = n._ParentNode;
                        return n._ParentNode;
                    }
                    return n.LeftNode.RightMostNode;
                }
            }

            internal Node Clone()
            {
                var that = new Node
                {
                    Value = Value,
                    Color = Color,
                    ChildCountLeft = ChildCountLeft,
                    ChildCountRight = ChildCountRight
                };
                if (_LeftNode != null) that.LeftNode = _LeftNode.Clone();
                if (_RightNode != null) that.RightNode = _RightNode.Clone();
                return that;
            }
        }
        
        ////////////////////////////////
        
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

            bool IsEmpty => RangeStartNode == RangeEndNode;

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